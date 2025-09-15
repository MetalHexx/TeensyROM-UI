import { inject, computed } from '@angular/core';
import { signalStore, withMethods, withState, withComputed } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import {
  StorageDirectory,
  IStorageService,
  STORAGE_SERVICE,
} from '@teensyrom-nx/domain/storage/services';
import { navigateToDirectory } from './methods/navigate-to-directory';
import { refreshDirectory } from './methods/refresh-directory';
import { initializeStorage } from './methods/initialize-storage';
import { cleanupStorage } from './methods/cleanup-storage';
import { StorageKeyUtil } from './storage-key.util';

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

    ...navigateToDirectory(store, storageService),
    ...refreshDirectory(store, storageService),
    ...initializeStorage(store, storageService),
    ...cleanupStorage(store),
  }))
);
