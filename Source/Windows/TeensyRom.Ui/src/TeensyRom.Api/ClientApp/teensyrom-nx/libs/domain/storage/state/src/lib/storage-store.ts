import { inject } from '@angular/core';
import { signalStore, withMethods, withState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { StorageDirectory, StorageService } from '@teensyrom-nx/domain/storage/services';
import { navigateToDirectory } from './methods/navigate-to-directory';
import { refreshDirectory } from './methods/refresh-directory';
import { initializeStorage } from './methods/initialize-storage';
import { cleanupStorage } from './methods/cleanup-storage';

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
  selectedDirectory: SelectedDirectory | null; // Global selection state
}

// Initial state
const initialState: StorageState = {
  storageEntries: {},
  selectedDirectory: null,
};

// Store definition
export const StorageStore = signalStore(
  { providedIn: 'root' },
  withDevtools('storage'),
  withState(initialState),
  withMethods((store, storageService: StorageService = inject(StorageService)) => ({
    ...navigateToDirectory(store, storageService),
    ...refreshDirectory(store, storageService),
    ...initializeStorage(store),
    ...cleanupStorage(store),
  }))
);
