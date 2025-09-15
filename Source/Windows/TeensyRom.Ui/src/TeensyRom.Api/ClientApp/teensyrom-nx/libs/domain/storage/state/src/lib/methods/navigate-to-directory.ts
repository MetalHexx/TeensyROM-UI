import { patchState, WritableStateSource } from '@ngrx/signals';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { IStorageService } from '@teensyrom-nx/domain/storage/services';
import { StorageKeyUtil } from '../storage-key.util';
import { StorageState } from '../storage-store';
import { firstValueFrom } from 'rxjs';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function navigateToDirectory(
  store: SignalStore<StorageState> & WritableStateSource<StorageState>,
  storageService: IStorageService
) {
  return {
    navigateToDirectory: async ({
      deviceId,
      storageType,
      path,
    }: {
      deviceId: string;
      storageType: StorageType;
      path: string;
    }): Promise<void> => {
      const key = StorageKeyUtil.create(deviceId, storageType);
      console.log(`🧭 Navigating to ${key} at path: ${path}`);

      // Only update device selection if it's different
      const currentSelection = store.selectedDirectories()[deviceId];
      const isSelectionDifferent =
        !currentSelection ||
        currentSelection.storageType !== storageType ||
        currentSelection.path !== path;

      if (isSelectionDifferent) {
        patchState(store, (state) => ({
          selectedDirectories: {
            ...state.selectedDirectories,
            [deviceId]: {
              deviceId,
              storageType,
              path,
            },
          },
        }));
      }

      // Check if we already have this directory loaded
      const currentState = store.storageEntries();
      const existingEntry = currentState[key];
      const isAlreadyLoaded =
        existingEntry &&
        existingEntry.currentPath === path &&
        existingEntry.isLoaded &&
        existingEntry.directory &&
        !existingEntry.error;

      if (isAlreadyLoaded) {
        console.log(`✅ Directory already loaded for ${key} at path: ${path}`);
        return;
      }

      // Update loading state
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
        console.log(`📡 Loading directory for ${key} at path: ${path}`);
        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, path)
        );
        console.log(`✅ Directory navigation successful for ${key}:`, directory);

        patchState(store, (state) => ({
          storageEntries: {
            ...state.storageEntries,
            [key]: {
              ...state.storageEntries[key],
              currentPath: path,
              directory,
              isLoaded: true,
              isLoading: false,
              error: null,
              lastLoadTime: Date.now(),
            },
          },
        }));
      } catch (error) {
        console.error(`❌ Directory navigation failed for ${key} at path ${path}:`, error);

        patchState(store, (state) => ({
          storageEntries: {
            ...state.storageEntries,
            [key]: {
              ...state.storageEntries[key],
              currentPath: path,
              directory: null,
              isLoaded: false,
              isLoading: false,
              error: 'Failed to navigate to directory',
            },
          },
        }));
      }
    },
  };
}
