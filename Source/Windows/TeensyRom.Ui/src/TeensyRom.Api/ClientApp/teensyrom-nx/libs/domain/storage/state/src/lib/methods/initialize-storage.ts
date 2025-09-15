import { patchState, WritableStateSource } from '@ngrx/signals';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { IStorageService } from '@teensyrom-nx/domain/storage/services';
import { StorageKeyUtil } from '../storage-key.util';
import { StorageState, StorageDirectoryState } from '../storage-store';
import { firstValueFrom } from 'rxjs';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function initializeStorage(
  store: SignalStore<StorageState> & WritableStateSource<StorageState>,
  storageService: IStorageService
) {
  return {
    initializeStorage: async ({
      deviceId,
      storageType,
    }: {
      deviceId: string;
      storageType: StorageType;
    }): Promise<void> => {
      const key = StorageKeyUtil.create(deviceId, storageType);

      console.log(`🚀 Starting async initialization for ${key}`);

      // Set global device selected directory to root
      patchState(store, (state) => ({
        selectedDirectories: {
          ...state.selectedDirectories,
          [deviceId]: {
            deviceId,
            storageType,
            path: '/',
          },
        },
      }));

      // Check if we need to proceed with initialization
      const currentState = store.storageEntries();
      const existingEntry = currentState[key];

      const isRootAlreadyLoaded =
        existingEntry &&
        existingEntry.currentPath === '/' &&
        existingEntry.isLoaded &&
        existingEntry.directory &&
        !existingEntry.error;

      if (isRootAlreadyLoaded) {
        console.log(`✅ ${key} already loaded, skipping initialization`);
        return;
      }

      // Entry doesn't exist, set the currentPath to root and isLoading to true.
      if (!existingEntry) {
        const initialEntry: StorageDirectoryState = {
          deviceId,
          storageType,
          currentPath: '/',
          directory: null,
          isLoaded: false,
          isLoading: true,
          error: null,
          lastLoadTime: null,
        };

        patchState(store, (state) => ({
          storageEntries: {
            ...state.storageEntries,
            [key]: initialEntry,
          },
        }));
      }
      // Entry already exists, set the currentPath to root and isloading to true.
      else {
        patchState(store, (state) => ({
          storageEntries: {
            ...state.storageEntries,
            [key]: {
              ...state.storageEntries[key],
              currentPath: '/',
              isLoading: true,
              error: null,
            },
          },
        }));
      }

      // Make the API call to load root directory and patch the state
      try {
        console.log(`📡 Making API call for ${key}`);

        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, '/')
        );

        console.log(`✅ API call successful for ${key}:`, directory);

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
        console.log(`✅ State updated successfully for ${key}`);
      } catch (error) {
        //If there was an error during the API call, patch the state with the error
        console.error(`❌ API error for ${key}:`, error);

        patchState(store, (state) => {
          const currentEntry = state.storageEntries[key];
          if (!currentEntry) {
            console.error(`❌ No entry found for ${key} during error update`);
            return state;
          }

          return {
            storageEntries: {
              ...state.storageEntries,
              [key]: {
                ...currentEntry,
                isLoading: false,
                error: (error as any)?.message || 'Failed to initialize storage',
              },
            },
          };
        });
      }
    },
  };
}
