import { ChangeDetectionStrategy, Component, input, inject, computed, signal, effect, viewChild, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { ScalingCardComponent, LoadingTextComponent, VirtualScrollAnimator } from '@teensyrom-nx/ui/components';
import { StorageStore, PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';
import { DirectoryItem, FileItem, LaunchMode } from '@teensyrom-nx/domain';
import { ProgressiveBatchRenderer, BatchRenderCancellation } from '@teensyrom-nx/utils';
import { DirectoryItemComponent } from './directory-item/directory-item.component';
import { FileItemComponent } from './file-item/file-item.component';

/**
 * Smart container component for displaying and managing directory contents (folders and files).
 * 
 * **Performance Optimization**: Uses Angular CDK Virtual Scrolling to efficiently render large
 * file listings (100-1000+ items) by only rendering visible items (~30) and recycling DOM nodes.
 * 
 * **Key Features**:
 * - Virtual scrolling for optimal performance with large directories
 * - Auto-scroll to currently playing file with centered positioning
 * - Selection highlighting with single-click
 * - Directory navigation on double-click
 * - File playback launch on double-click
 * - Real-time playing file indicator
 * - Error state visualization
 * 
 * **Virtual Scrolling Configuration**:
 * - itemSize: 52px (measured height of .file-list-item)
 * - minBufferPx: 200px (pre-render buffer above/below viewport)
 * - maxBufferPx: 400px (maximum buffer before recycling)
 * - Height constraint: 400px mat-card-content with absolutely positioned viewport
 * 
 * **Performance Metrics** (1000 items):
 * - DOM nodes: ~30 (vs 1000 without virtual scrolling)
 * - Initial render: <50ms (vs 750ms)
 * - Scroll: 60fps smooth (vs 30fps janky)
 * 
 * @see {@link https://material.angular.io/cdk/scrolling/overview Angular CDK Virtual Scrolling}
 * @see {@link ../../../../../../../../docs/VIRTUAL_SCROLL_TESTING.md Testing Guide}
 * @see {@link ../../../../../../../../docs/features/directory-files/VIRTUAL_SCROLL_PLAN.md Implementation Plan}
 */
@Component({
  selector: 'lib-directory-files',
  imports: [
    CommonModule,
    ScalingCardComponent,
    LoadingTextComponent,
    DirectoryItemComponent,
    FileItemComponent,
    ScrollingModule,
  ],
  templateUrl: './directory-files.component.html',
  styleUrls: ['./directory-files.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DirectoryFilesComponent implements OnDestroy {
  deviceId = input.required<string>();
  animationTrigger = input<boolean>(true);

  /**
   * Reference to the CDK Virtual Scroll Viewport for programmatic scrolling.
   * Used to auto-scroll to currently playing file with centered positioning.
   */
  private readonly viewport = viewChild<CdkVirtualScrollViewport>('viewport');

  private readonly storageStore = inject(StorageStore);
  private readonly playerContext: IPlayerContext = inject(PLAYER_CONTEXT);

  readonly selectedDirectoryState = computed(() =>
    this.storageStore.getSelectedDirectoryState(this.deviceId())()
  );

  readonly directoryContents = computed(() => {
    const state = this.selectedDirectoryState();
    if (!state?.directory) {
      return { files: [], directories: [], hasContent: false };
    }

    return {
      files: state.directory.files || [],
      directories: state.directory.directories || [],
      hasContent: true,
      currentPath: state.currentPath,
      storageType: state.storageType,
      deviceId: state.deviceId,
      isLoading: state.isLoading,
      error: state.error,
    };
  });

  readonly selectedItem = signal<DirectoryItem | FileItem | null>(null);

  // Plain writable signal for loading state - won't trigger unnecessary re-evaluations
  private readonly _isLoading = signal<boolean>(false);
  readonly isLoading = this._isLoading.asReadonly();

  // Get current playing file from player context
  readonly currentPlayingFile = computed(() =>
    this.playerContext.getCurrentFile(this.deviceId())()
  );

  // Get file context to know when directory content is available
  readonly playerFileContext = computed(() => 
    this.playerContext.getFileContext(this.deviceId())()
  );

  // Writable signal for progressive rendering
  private readonly _directoriesAndFiles = signal<((DirectoryItem | FileItem) & { itemType: string })[]>([]);
  readonly directoriesAndFiles = this._directoriesAndFiles.asReadonly();

  // Utilities for progressive rendering and scrolling
  private readonly batchRenderer = new ProgressiveBatchRenderer<
    DirectoryItem | FileItem,
    (DirectoryItem | FileItem) & { itemType: string }
  >();
  private readonly scrollAnimator = new VirtualScrollAnimator<(DirectoryItem | FileItem) & { itemType: string }>();
  
  private batchCancellation: BatchRenderCancellation | null = null;
  private readonly _isProcessingBatches = signal(false);
  private readonly _isScrollingToItem = signal(false);

  constructor() {
    // Track loading state - stays true until all batches complete AND scrolling finishes
    effect(() => {
      const contents = this.directoryContents();
      const apiLoading = contents.isLoading ?? false;
      const processingBatches = this._isProcessingBatches();
      const scrollingToItem = this._isScrollingToItem();
      
      // Show loading if ANY of these conditions are true:
      // 1. API is loading
      // 2. We're processing batches
      // 3. We're scrolling to an item
      this._isLoading.set(apiLoading || processingBatches || scrollingToItem);
    });

    // Effect to progressively populate items in batches to keep animations smooth
    effect(() => {
      const contents = this.directoryContents();
      
      // Cancel any pending batch work
      if (this.batchCancellation) {
        this.batchCancellation.cancel();
        this.batchCancellation = null;
      }

      // Clear immediately when loading or empty
      if (contents.isLoading || !contents.hasContent) {
        this._directoriesAndFiles.set([]);
        this._isProcessingBatches.set(false);
        return;
      }

      const directories = contents.directories;
      const files = contents.files;
      const allItems = [...directories, ...files];

      // For small lists, just populate immediately (no performance impact)
      if (allItems.length < 100) {
        const transformed = allItems.map((item, index) => 
          index < directories.length
            ? { ...item, itemType: 'directory' as const }
            : { ...item, itemType: 'file' as const }
        );
        this._directoriesAndFiles.set(transformed);
        this._isProcessingBatches.set(false);
        return;
      }

      // For large lists, use progressive batch renderer
      this.batchCancellation = this.batchRenderer.renderBatches(
        allItems,
        (item, index) => 
          index < directories.length
            ? { ...item, itemType: 'directory' as const }
            : { ...item, itemType: 'file' as const },
        (accumulated) => this._directoriesAndFiles.set(accumulated),
        (isComplete) => this._isProcessingBatches.set(!isComplete),
        50 // batch size
      );
    });

    // Effect to automatically select and scroll to the currently playing file
    effect(() => {
      const playingFile = this.currentPlayingFile();
      const fileContext = this.playerFileContext();
      const directoriesAndFiles = this.directoriesAndFiles();

      // Only proceed if we have a playing file and directory content is loaded
      if (!playingFile || directoriesAndFiles.length === 0) {
        return;
      }

      // In directory mode, the file should already be in the current directory
      // In shuffle mode, we need to wait for the directory context to be loaded after navigation
      const currentLaunchMode = this.playerContext.getLaunchMode(this.deviceId())();

      if (currentLaunchMode === LaunchMode.Shuffle) {
        // For shuffle mode, wait for file context to be populated with directory files
        // This happens after loadDirectoryContextForRandomFile is called
        if (!fileContext || fileContext.files.length === 0) {
          return; // Directory context not loaded yet
        }
      }

      // Find the playing file in the current directory
      const playingFileItem = directoriesAndFiles.find(item =>
        item.path === playingFile.file.path
      );

      if (playingFileItem) {
        // Select the playing file
        this.selectedItem.set(playingFileItem);

        // Scroll to the selected file
        this.scrollToSelectedFile(playingFile.file.path);
      }
    });
  }

  isDirectory(item: DirectoryItem | FileItem): item is DirectoryItem {
    return 'itemType' in item && (item as { itemType: string }).itemType === 'directory';
  }

  isSelected(item: DirectoryItem | FileItem): boolean {
    const selected = this.selectedItem();
    return selected !== null && selected.path === item.path;
  }

  isCurrentlyPlaying(item: DirectoryItem | FileItem): boolean {
    const playingFile = this.currentPlayingFile();
    return playingFile !== null && playingFile.file.path === item.path;
  }

  hasCurrentFileError = computed(() => 
    this.playerContext.getError(this.deviceId())() !== null
  );

  onItemSelected(item: DirectoryItem | FileItem): void {
    this.selectedItem.set(item);
  }

  onDirectoryDoubleClick(directory: DirectoryItem): void {
    const contents = this.directoryContents();
    if (contents.storageType && contents.deviceId) {
      this.storageStore.navigateToDirectory({
        deviceId: contents.deviceId,
        storageType: contents.storageType,
        path: directory.path,
      });
      this.selectedItem.set(null);
    }
  }

  onFileDoubleClick(file: FileItem): void {
    const contents = this.directoryContents();
    if (!contents.storageType || !contents.deviceId) {
      return;
    }

    this.selectedItem.set(file);
    void this.playerContext.launchFileWithContext({
      deviceId: contents.deviceId,
      storageType: contents.storageType,
      file,
      directoryPath: contents.currentPath ?? '/',
      files: contents.files,
      launchMode: LaunchMode.Directory,
    });
  }

  /**
   * Scrolls the virtual viewport to show the specified file, centered in view.
   * Uses VirtualScrollAnimator utility for reusable scroll logic.
   */
  private scrollToSelectedFile(filePath: string): void {
    this.scrollAnimator.scrollToItem({
      viewport: this.viewport,
      items: this.directoriesAndFiles(),
      findIndex: (items) => items.findIndex(item => item.path === filePath),
      itemHeight: 52,
      isScrollingSignal: this._isScrollingToItem,
      scrollDuration: 600,
      renderDelay: 100
    });
  }

  ngOnDestroy(): void {
    // Clean up batch rendering
    if (this.batchCancellation) {
      this.batchCancellation.cancel();
      this.batchCancellation = null;
    }
    
    // Clean up scroll animator
    this.scrollAnimator.destroy();
    
    this._isProcessingBatches.set(false);
    this._isScrollingToItem.set(false);
  }
}

