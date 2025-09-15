import { patchState, WritableStateSource } from '@ngrx/signals';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { IStorageService } from '@teensyrom-nx/domain/storage/services';
import { StorageKeyUtil } from '../storage-key.util';
import { StorageState } from '../storage-store';
import { firstValueFrom } from 'rxjs';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function refreshDirectory(
  store: SignalStore<StorageState> & WritableStateSource<StorageState>,
  storageService: IStorageService
) {
  return {
    refreshDirectory: async ({
      deviceId,
      storageType,
    }: {
      deviceId: string;
      storageType: StorageType;
    }): Promise<void> => {
      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = store.storageEntries()[key];

      if (!entry) {
        console.warn(`🔄 Cannot refresh - no entry found for ${key}`);
        return;
      }

      console.log(`🔄 Refreshing directory for ${key} at path: ${entry.currentPath}`);

      // Set loading state
      patchState(store, (state) => ({
        storageEntries: {
          ...state.storageEntries,
          [key]: {
            ...state.storageEntries[key],
            isLoading: true,
            error: null,
          },
        },
      }));

      try {
        // Load the directory using the current path
        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, entry.currentPath)
        );
        console.log(`✅ Directory refresh successful for ${key}:`, directory);

        patchState(store, (state) => ({
          storageEntries: {
            ...state.storageEntries,
            [key]: {
              ...state.storageEntries[key],
              directory,
              isLoaded: true,
              isLoading: false,
              error: null,
              lastLoadTime: Date.now(),
            },
          },
        }));
      } catch (error) {
        console.error(`❌ Directory refresh failed for ${key}:`, error);
        patchState(store, (state) => ({
          storageEntries: {
            ...state.storageEntries,
            [key]: {
              ...state.storageEntries[key],
              isLoading: false,
              error: 'Failed to refresh directory',
            },
          },
        }));
      }
    },
  };
}
