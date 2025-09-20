import { signalStore, withState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { StorageDirectory } from '@teensyrom-nx/domain/storage/services';
import { withStorageActions } from './actions';
import { withStorageSelectors } from './selectors';

// State interfaces
export interface StorageDirectoryState {
  deviceId: string;
  storageType: StorageType;
  currentPath: string;
  directory: StorageDirectory | null; // could be null if there was an error in the api call.
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
  withStorageSelectors(),
  withStorageActions()
);
