import { inject, computed } from '@angular/core';
import { signalStore, withMethods, withState, withComputed, patchState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import {
  StorageDirectory,
  IStorageService,
  STORAGE_SERVICE,
} from '@teensyrom-nx/domain/storage/services';
import { StorageKeyUtil, type StorageKey } from './storage-key.util';
import { firstValueFrom } from 'rxjs';

// State interfaces
export interface StorageDirectoryState {
  deviceId: string;
  storageType: StorageType;
  currentPath: string;
  directory: StorageDirectory | null;
  isLoaded: boolean;
  isLoading: boolean;
  error: string | null;
  lastLoadTime: number | null;
}

export interface SelectedDirectory {
  deviceId: string;
  storageType: StorageType;
  path: string;
}

export interface StorageState {
  storageEntries: Record<string, StorageDirectoryState>; // key: "${deviceId}-${storageType}"
  selectedDirectories: Record<string, SelectedDirectory>; // key: deviceId - Per-device selection state
}

// Initial state
const initialState: StorageState = {
  storageEntries: {},
  selectedDirectories: {},
};

// Store definition
export const StorageStore = signalStore(
  { providedIn: 'root' },
  withDevtools('storage'),
  withState(initialState),
  withComputed(() => ({})),
  withMethods((store, storageService: IStorageService = inject(STORAGE_SERVICE)) => ({
    // Per-device selection methods
    getSelectedDirectoryForDevice: (deviceId: string): SelectedDirectory | null => {
      return store.selectedDirectories()[deviceId] ?? null;
    },

    getSelectedDirectoryState: (deviceId: string) =>
      computed<StorageDirectoryState | null>(() => {
        const selected = store.selectedDirectories()[deviceId];
        if (!selected) return null;
        const key = StorageKeyUtil.create(selected.deviceId, selected.storageType);
        return store.storageEntries()[key] ?? null;
      }),

    // Factories: return computed signals
    getDeviceStorageEntries: (deviceId: string) =>
      computed<Record<string, StorageDirectoryState>>(() => {
        const entries = store.storageEntries();
        const result: Record<string, StorageDirectoryState> = {};
        for (const [key, value] of Object.entries(entries)) {
          if (key.startsWith(`${deviceId}-`)) {
            result[key] = value as StorageDirectoryState;
          }
        }
        return result;
      }),

    getDeviceDirectories: (deviceId: string) =>
      computed(() => {
        const entries = store.storageEntries();
        const directories: Array<{
          key: string;
          deviceId: string;
          storageType: StorageType;
          currentPath: string;
          directories: StorageDirectory['directories'];
        }> = [];

        for (const [key, value] of Object.entries(entries)) {
          const v = value as StorageDirectoryState;
          if (key.startsWith(`${deviceId}-`) && v.directory) {
            directories.push({
              key,
              deviceId: v.deviceId,
              storageType: v.storageType,
              currentPath: v.currentPath,
              directories: v.directory.directories,
            });
          }
        }
        return directories;
      }),

    // Initialize storage method
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

    // Navigate to directory method
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

    // Refresh directory method
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

    // Cleanup storage method
    cleanupStorage: ({ deviceId }: { deviceId: string }) => {
      patchState(store, (state) => {
        const updatedSelectedDirectories = { ...state.selectedDirectories };
        const updatedEntries = { ...state.storageEntries };

        // Remove all entries for the specified device and device selection
        Object.keys(updatedEntries).forEach((key) => {
          if (StorageKeyUtil.forDevice(deviceId)(key as StorageKey)) {
            console.log(` Starting async initialization for ${key}`);
            delete updatedEntries[key];
          }
        });
        delete updatedSelectedDirectories[deviceId];

        return {
          storageEntries: updatedEntries,
          selectedDirectories: updatedSelectedDirectories,
        };
      });
    },

    // Cleanup storage type method
    cleanupStorageType: ({
      deviceId,
      storageType,
    }: {
      deviceId: string;
      storageType: StorageType;
    }) => {
      const key = StorageKeyUtil.create(deviceId, storageType);

      patchState(store, (state) => {
        const updatedEntries = { ...state.storageEntries };
        delete updatedEntries[key];
        return { storageEntries: updatedEntries };
      });
    },
  }))
);
