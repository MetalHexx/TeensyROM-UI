import { signalStore, withState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { StorageType, PlayerFilterType, FileItem } from '@teensyrom-nx/domain';
import { StorageDirectory } from '@teensyrom-nx/domain';
import { withStorageActions } from './actions';
import { withStorageSelectors } from './selectors';

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

export interface SearchState {
  searchText: string;
  filterType: PlayerFilterType | null;
  results: FileItem[];
  isSearching: boolean;
  hasSearched: boolean;
  error: string | null;
}

export interface SelectedDirectory {
  deviceId: string;
  storageType: StorageType;
  path: string;
}

export interface NavigationHistoryItem {
  path: string;
  storageType: StorageType;
}

export class NavigationHistory {
  history: NavigationHistoryItem[] = [];
  currentIndex = -1;
  maxHistorySize: number;

  constructor(maxHistorySize = 50) {
    this.maxHistorySize = maxHistorySize;
  }
}

export interface FavoriteOperationsState {
  isProcessing: boolean;
  error: string | null;
}

export interface StorageState {
  storageEntries: Record<string, StorageDirectoryState>; // key: "${deviceId}-${storageType}"
  selectedDirectories: Record<string, SelectedDirectory>; // key: deviceId - Per-device selection state
  navigationHistory: Record<string, NavigationHistory>; // key: deviceId - Navigation history per device
  searchState: Record<string, SearchState>; // key: "${deviceId}-${storageType}" - Per-device/storage search state
  favoriteOperationsState: FavoriteOperationsState;
}

const initialState: StorageState = {
  storageEntries: {},
  selectedDirectories: {},
  navigationHistory: {},
  searchState: {},
  favoriteOperationsState: {
    isProcessing: false,
    error: null,
  },
};

export const StorageStore = signalStore(
  { providedIn: 'root' },
  withDevtools('storage'),
  withState(initialState),
  withStorageSelectors(),
  withStorageActions()
);
