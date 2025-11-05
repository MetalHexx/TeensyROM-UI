import {
  Component,
  input,
  inject,
  computed,
  signal,
  effect,
  untracked,
  viewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ScalingCompactCardComponent,
  InputFieldComponent,
  IconButtonComponent,
} from '@teensyrom-nx/ui/components';
import { StorageStore, PLAYER_CONTEXT } from '@teensyrom-nx/application';
import { PlayerFilterType } from '@teensyrom-nx/domain';

@Component({
  selector: 'lib-search-toolbar',
  imports: [CommonModule, ScalingCompactCardComponent, InputFieldComponent, IconButtonComponent],
  templateUrl: './search-toolbar.component.html',
  styleUrl: './search-toolbar.component.scss',
})
export class SearchToolbarComponent {
  // Required input
  deviceId = input.required<string>();

  // ViewChild reference to search input
  searchInput = viewChild<InputFieldComponent>('searchInput');

  // Injected dependencies
  private readonly storageStore = inject(StorageStore);
  private readonly playerContext = inject(PLAYER_CONTEXT);

  // Computed signals for state
  readonly selectedDirectoryState = computed(() =>
    this.storageStore.getSelectedDirectoryState(this.deviceId())()
  );

  readonly currentStorageType = computed(() => this.selectedDirectoryState()?.storageType ?? null);

  readonly searchState = computed(() => {
    const storageType = this.currentStorageType();
    if (!storageType) {
      return null;
    }
    return this.storageStore.getSearchState(this.deviceId(), storageType)();
  });

  readonly hasActiveSearch = computed(() => this.searchState()?.hasSearched ?? false);

  readonly isSearching = computed(() => this.searchState()?.isSearching ?? false);

  readonly currentFilter = computed(
    () => this.playerContext.getShuffleSettings(this.deviceId())()?.filter ?? PlayerFilterType.All
  );

  // Local signal for search input text
  readonly searchText = signal<string>('');

  // Computed signal for search button enabled state
  readonly canSearch = computed(() => this.searchText().trim().length > 0 && !this.isSearching());

  // Computed signal for clear button visibility
  readonly showClearButton = computed(() => {
    const state = this.searchState();
    return this.hasActiveSearch() && state?.results !== undefined && state.results.length > 0;
  });

  // Debounce timeout for auto-search
  private searchTimeout?: ReturnType<typeof setTimeout>;

  constructor() {
    // Reactive effect: Clear input field when search state is cleared from store
    effect(() => {
      const searchState = this.searchState();

      untracked(() => {
        // If search state is null or undefined, it means search was cleared
        // This can happen from:
        // - clearSearch() action
        // - Navigation actions (navigateToDirectory, navigateUp, etc.)
        if (!searchState) {
          // Clear local signal
          this.searchText.set('');

          // Clear the input field component
          const inputField = this.searchInput();
          if (inputField) {
            inputField.writeValue('');
          }
        }
      });
    });

    // Optional: Debounced auto-search effect
    effect(() => {
      const text = this.searchText();

      // Clear previous timeout
      if (this.searchTimeout) {
        clearTimeout(this.searchTimeout);
      }

      // Don't auto-search on empty text
      if (text.trim().length === 0) {
        return;
      }

      // Use untracked to prevent infinite loops when checking other signals
      untracked(() => {
        if (this.canSearch()) {
          this.searchTimeout = setTimeout(() => {
            this.executeSearch();
          }, 1000); // 1 second debounce
        }
      });
    });
  }

  /**
   * Update search text signal when input value changes
   */
  onSearchInputChange(value: string): void {
    this.searchText.set(value);
  }

  /**
   * Execute search with current text and filter
   */
  executeSearch(): void {
    const trimmedText = this.searchText().trim();
    const storageType = this.currentStorageType();

    // Validate search text is not empty
    if (trimmedText.length === 0) {
      return;
    }

    // Validate storage type is available
    if (!storageType) {
      return;
    }

    // Call storage store search action
    void this.storageStore.searchFiles({
      deviceId: this.deviceId(),
      storageType: storageType,
      searchText: trimmedText,
      filterType: this.currentFilter(),
    });
  }

  /**
   * Clear search results and return to directory view
   */
  clearSearch(): void {
    const storageType = this.currentStorageType();

    // Validate storage type is available
    if (!storageType) {
      return;
    }

    // Clear search state in store
    // Note: The reactive effect will handle clearing the input field
    this.storageStore.clearSearch({
      deviceId: this.deviceId(),
      storageType: storageType,
    });
  }
}
