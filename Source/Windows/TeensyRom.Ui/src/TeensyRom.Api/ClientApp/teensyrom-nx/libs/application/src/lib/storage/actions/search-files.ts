import { firstValueFrom } from 'rxjs';
import { StorageType, IStorageService, PlayerFilterType } from '@teensyrom-nx/domain';
import { StorageState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import { WritableStore } from '../storage-helpers';
import { createAction, LogType, logInfo, logError } from '@teensyrom-nx/utils';
import { updateState } from '@angular-architects/ngrx-toolkit';

export function searchFiles(
  store: WritableStore<StorageState>,
  storageService: IStorageService
) {
  return {
    searchFiles: async ({
      deviceId,
      storageType,
      searchText,
      filterType,
    }: {
      deviceId: string;
      storageType: StorageType;
      searchText: string;
      filterType?: PlayerFilterType;
    }): Promise<void> => {
      const actionMessage = createAction('search-files');
      const key = StorageKeyUtil.create(deviceId, storageType);

      logInfo(
        LogType.Start,
        `Starting search for ${key} with text: "${searchText}"`,
        filterType ? { filterType } : undefined
      );

      // Get current search state to preserve hasSearched flag
      const currentSearchState = store.searchState()[key];
      const wasAlreadySearching = currentSearchState?.hasSearched ?? false;

      // Set searching state
      updateState(store, actionMessage, (state) => ({
        searchState: {
          ...state.searchState,
          [key]: {
            searchText,
            filterType: filterType ?? null,
            results: [],
            isSearching: true,
            hasSearched: wasAlreadySearching, // Preserve existing state - don't reset to false
            error: null,
          },
        },
      }));

      try {
        logInfo(LogType.NetworkRequest, `Making search API call for ${key}`);

        const results = await firstValueFrom(
          storageService.search(deviceId, storageType, searchText, filterType)
        );

        logInfo(LogType.Success, `Search successful for ${key}:`, {
          resultCount: results.length,
        });

        // Update with search results
        updateState(store, actionMessage, (state) => ({
          searchState: {
            ...state.searchState,
            [key]: {
              ...state.searchState[key],
              results,
              isSearching: false,
              hasSearched: true,
              error: null,
            },
          },
        }));

        logInfo(LogType.Finish, `Search completed for ${key}`);
      } catch (error) {
        logError(`Search failed for ${key}:`, error);

        updateState(store, actionMessage, (state) => ({
          searchState: {
            ...state.searchState,
            [key]: {
              ...state.searchState[key],
              results: [],
              isSearching: false,
              hasSearched: true,
              error: error instanceof Error ? error.message : 'Search failed',
            },
          },
        }));
      }
    },
  };
}
