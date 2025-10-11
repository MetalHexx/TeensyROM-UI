import { ChangeDetectionStrategy, Component, input, inject, computed, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StorageStore, PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';
import { FileItem, LaunchMode, PlayerFilterType } from '@teensyrom-nx/domain';
import { SearchItemComponent } from './search-item/search-item.component';
import { ScalingCardComponent, EmptyStateMessageComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-search-results',
  imports: [CommonModule, SearchItemComponent, ScalingCardComponent, EmptyStateMessageComponent],
  templateUrl: './search-results.component.html',
  styleUrls: ['./search-results.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchResultsComponent {
  deviceId = input.required<string>();
  animationTrigger = input<boolean>(true);

  private readonly storageStore = inject(StorageStore);
  private readonly playerContext: IPlayerContext = inject(PLAYER_CONTEXT);

  // Computed signal for selected directory state to get storage type
  readonly selectedDirectoryState = computed(() => 
    this.storageStore.getSelectedDirectoryState(this.deviceId())()
  );

  // Computed signal for current storage type
  readonly currentStorageType = computed(() => 
    this.selectedDirectoryState()?.storageType ?? null
  );

  // Computed signal for search state
  readonly searchState = computed(() => {
    const storageType = this.currentStorageType();
    if (!storageType) {
      return null;
    }
    return this.storageStore.getSearchState(this.deviceId(), storageType)();
  });

  // Computed signal for search results
  readonly searchResults = computed(() => 
    this.searchState()?.results ?? []
  );

  // Computed signal for loading state (used to prevent "No files found" flash)
  readonly isSearching = computed(() => 
    this.searchState()?.isSearching ?? false
  );

  // Computed signal for search status
  readonly hasSearched = computed(() => 
    this.searchState()?.hasSearched ?? false
  );

  // Computed signal for error state
  readonly searchError = computed(() => 
    this.searchState()?.error ?? null
  );

  // Computed signal for current playing file
  readonly currentPlayingFile = computed(() => 
    this.playerContext.getCurrentFile(this.deviceId())()
  );

  // Computed signal for launch mode
  readonly currentLaunchMode = computed(() => 
    this.playerContext.getLaunchMode(this.deviceId())()
  );

  // Computed signal for player error state
  readonly hasPlayerError = computed(() => 
    this.playerContext.getError(this.deviceId())() !== null
  );

  // Computed signal for current filter
  readonly currentFilter = computed(() => 
    this.playerContext.getShuffleSettings(this.deviceId())()?.filter ?? PlayerFilterType.All
  );

  // Computed signal to check if a specific filter is applied (not "All")
  readonly hasActiveFilter = computed(() => 
    this.currentFilter() !== PlayerFilterType.All
  );

  // Computed signal for filter display name
  readonly filterDisplayName = computed(() => {
    const filter = this.currentFilter();
    switch (filter) {
      case PlayerFilterType.Games:
        return 'Games';
      case PlayerFilterType.Music:
        return 'Music';
      case PlayerFilterType.Images:
        return 'Images';
      case PlayerFilterType.Hex:
        return 'Hex';
      default:
        return 'All';
    }
  });

  // Computed signal for smart empty state message
  readonly emptyStateSecondaryMessage = computed(() => {
    if (this.hasActiveFilter()) {
      return `You have a <strong>${this.filterDisplayName()}</strong> filter applied. Try changing the filter to see more results.`;
    }
    return undefined; // No secondary message when filter is "All"
  });

  // Local signal for selected item
  readonly selectedItem = signal<FileItem | null>(null);

  // Effect to automatically select and scroll to currently playing file
  constructor() {
    effect(() => {
      const playingFile = this.currentPlayingFile();
      const results = this.searchResults();
      const launchMode = this.currentLaunchMode();

      // Only proceed if we have a playing file, search results, and we're in Search mode
      if (!playingFile || results.length === 0 || launchMode !== LaunchMode.Search) {
        return;
      }

      // Find the playing file in the search results
      const playingFileItem = results.find(item => item.path === playingFile.file.path);

      if (playingFileItem) {
        // Select the playing file
        this.selectedItem.set(playingFileItem);

        // Scroll to the selected file
        this.scrollToSelectedFile(playingFile.file.path);
      }
    });
  }

  onFileSelected(file: FileItem): void {
    this.selectedItem.set(file);
  }

  async onFileDoubleClick(file: FileItem): Promise<void> {
    const searchState = this.searchState();
    const deviceId = this.deviceId();
    const storageType = this.currentStorageType();

    // Validate search state exists (should always be true if displaying results)
    if (!searchState || !storageType) {
      return;
    }

    // Launch file with Search mode
    void this.playerContext.launchFileWithContext({
      deviceId: deviceId,
      storageType: storageType,
      file: file,
      directoryPath: file.parentPath,
      files: searchState.results,
      launchMode: LaunchMode.Search,
    });
  }

  isSelected(file: FileItem): boolean {
    return this.selectedItem()?.path === file.path;
  }

  isCurrentlyPlaying(file: FileItem): boolean {
    const playingFile = this.currentPlayingFile();
    const launchMode = this.currentLaunchMode();
    
    // Only highlight if file is playing AND we're in Search mode
    return playingFile?.file.path === file.path && launchMode === LaunchMode.Search;
  }

  private scrollToSelectedFile(filePath: string): void {
    // Use setTimeout to ensure the DOM is updated after the selection change
    setTimeout(() => {
      // Find the DOM element for the selected file using the data attribute
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
