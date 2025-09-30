import { ChangeDetectionStrategy, Component, input, inject, computed, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardLayoutComponent } from '@teensyrom-nx/ui/components';
import { StorageStore, PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';
import { DirectoryItem, FileItem, LaunchMode } from '@teensyrom-nx/domain';
import { DirectoryItemComponent } from './directory-item/directory-item.component';
import { FileItemComponent } from './file-item/file-item.component';

@Component({
  selector: 'lib-directory-files',
  imports: [CommonModule, CardLayoutComponent, DirectoryItemComponent, FileItemComponent],
  templateUrl: './directory-files.component.html',
  styleUrls: ['./directory-files.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DirectoryFilesComponent {
  deviceId = input.required<string>();

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

  // Get current playing file from player context
  readonly currentPlayingFile = computed(() => 
    this.playerContext.getCurrentFile(this.deviceId())()
  );

  // Get file context to know when directory content is available
  readonly playerFileContext = computed(() => 
    this.playerContext.getFileContext(this.deviceId())()
  );

  readonly combinedItems = computed(() => {
    const contents = this.directoryContents();
    const directories = contents.directories.map((dir) => ({
      ...dir,
      itemType: 'directory' as const,
    }));
    const files = contents.files.map((file) => ({
      ...file,
      itemType: 'file' as const,
    }));
    return [...directories, ...files];
  });

  // Effect to automatically select and scroll to the currently playing file
  constructor() {
    effect(() => {
      const playingFile = this.currentPlayingFile();
      const fileContext = this.playerFileContext();
      const combinedItems = this.combinedItems();

      // Only proceed if we have a playing file and directory content is loaded
      if (!playingFile || combinedItems.length === 0) {
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
      const playingFileItem = combinedItems.find(item => 
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

  private scrollToSelectedFile(filePath: string): void {
    // Use setTimeout to ensure the DOM is updated after the selection change
    setTimeout(() => {
      // Find the DOM element for the selected file using the data attribute on the container
      const targetElement = document.querySelector(`.file-list-item[data-item-path="${CSS.escape(filePath)}"]`);
      
      if (targetElement) {
        targetElement.scrollIntoView({ 
          behavior: 'smooth', 
          block: 'center' 
        });
      }
    }, 0);
  }
}

