import { patchState, StateSignals, WritableStateSource } from '@ngrx/signals';
import { StorageState, StorageDirectoryState } from './storage-store';
import { StorageKey, StorageKeyUtil } from './storage-key.util';
import { StorageType } from '@teensyrom-nx/domain/storage/services';

export type WritableStore<T extends object> = StateSignals<T> & WritableStateSource<T>;

/**
 * Helper to set loading state for a storage entry
 */
export function setLoadingStorage(store: WritableStore<StorageState>, key: StorageKey): void {
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
}

/**
 * Helper to update a storage entry with arbitrary changes
 */
export function updateStorage(
  store: WritableStore<StorageState>,
  key: StorageKey,
  updates: Partial<StorageDirectoryState>
): void {
  patchState(store, (state) => ({
    storageEntries: {
      ...state.storageEntries,
      [key]: {
        ...state.storageEntries[key],
        ...updates,
      },
    },
  }));
}

/**
 * Helper to set loaded state with common success fields
 */
export function setStorageLoaded(
  store: WritableStore<StorageState>,
  key: StorageKey,
  additionalUpdates: Partial<StorageDirectoryState> = {}
): void {
  updateStorage(store, key, {
    isLoaded: true,
    isLoading: false,
    error: null,
    lastLoadTime: Date.now(),
    ...additionalUpdates,
  });
}

/**
 * Helper to get existing storage entry by key
 */
export function getStorage(
  store: WritableStore<StorageState>,
  key: StorageKey
): StorageDirectoryState | undefined {
  const currentState = store.storageEntries();
  return currentState[key];
}

/**
 * Helper to check if a directory is already loaded at a specific path
 */
export function isDirectoryLoadedAtPath(
  entry: StorageDirectoryState | undefined,
  path: string
): boolean {
  return !!(
    entry &&
    entry.currentPath === path &&
    entry.isLoaded &&
    entry.directory &&
    !entry.error
  );
}

/**
 * Helper to check if the given values match the currently selected directory
 */
export function isSelectedDirectory(
  store: WritableStore<StorageState>,
  deviceId: string,
  storageType: StorageType,
  path: string
): boolean {
  const currentSelection = store.selectedDirectories()[deviceId];
  return !!(
    currentSelection &&
    currentSelection.storageType === storageType &&
    currentSelection.path === path
  );
}

/**
 * Helper to set error state for a storage entry
 */
export function setStorageError(
  store: WritableStore<StorageState>,
  key: StorageKey,
  errorMessage: string
): void {
  patchState(store, (state) => {
    const currentEntry = state.storageEntries[key];
    if (!currentEntry) {
      console.error(`? No entry found for ${key} during error update`);
      return state;
    }

    return {
      storageEntries: {
        ...state.storageEntries,
        [key]: {
          ...currentEntry,
          isLoading: false,
          error: errorMessage,
        },
      },
    };
  });
}

/**
 * Helper to insert a storage entry
 */
export function insertStorage(
  store: WritableStore<StorageState>,
  key: StorageKey,
  entry: StorageDirectoryState
): void {
  patchState(store, (state) => ({
    storageEntries: {
      ...state.storageEntries,
      [key]: entry,
    },
  }));
}

/**
 * Helper to create initial storage directory state
 */
export function createStorage(
  store: WritableStore<StorageState>,
  deviceId: string,
  storageType: StorageType,
  path = '/'
): StorageDirectoryState {
  const newEntry: StorageDirectoryState = {
    deviceId,
    storageType,
    currentPath: path,
    directory: null,
    isLoaded: false,
    isLoading: true,
    error: null,
    lastLoadTime: null,
  };
  insertStorage(store, StorageKeyUtil.create(deviceId, storageType), newEntry);
  return newEntry;
}

/**
 * Helper to remove storage entry by key
 */
export function removeStorage(store: WritableStore<StorageState>, key: StorageKey): void {
  patchState(store, (state) => {
    const updatedEntries = { ...state.storageEntries };
    delete updatedEntries[key];
    return { storageEntries: updatedEntries };
  });
}

/**
 * Helper to filter entries by device ID
 */
export function getAllDeviceStorage(
  entries: Record<string, StorageDirectoryState>,
  deviceId: string
): Record<string, StorageDirectoryState> {
  const result: Record<string, StorageDirectoryState> = {};
  for (const [key, value] of Object.entries(entries)) {
    if (key.startsWith(`${deviceId}-`)) {
      result[key] = value as StorageDirectoryState;
    }
  }
  return result;
}

/**
 * Helper to log info messages with appropriate emotes
 */
export enum LogType {
  Start = '🚀',
  Finish = '🏁',
  Success = '✅',
  NetworkRequest = '🌐',
  Navigate = '🧭',
  Refresh = '🔄',
  Cleanup = '🧹',
  Error = '❌',
  Warning = '⚠️',
  Unknown = '❓',
  Select = '🖱️',
  Info = 'ℹ️',
  Critical = '🛑',
  Debug = '🐞',
  Midi = '🎹',
}

export function logInfo(operation: LogType, message: string, data?: unknown): void {
  if (data !== undefined) {
    console.log(`${operation} ${message}`, data);
  } else {
    console.log(`${operation} ${message}`);
  }
}
/**
 * Helper to log error messages with error emote
 */
export function logError(message: string, error?: unknown): void {
  if (error !== undefined) {
    console.error(`${LogType.Error} ${message}`, error);
  } else {
    console.error(`${LogType.Error} ${message}`);
  }
}

/**
 * Helper to log warning messages with warning emote
 */
export function logWarn(message: string): void {
  console.warn(`?? ${message}`);
}

/**
 * Helper to update selected directory for a device
 */
export function setDeviceSelectedDirectory(
  store: WritableStore<StorageState>,
  deviceId: string,
  storageType: StorageType,
  path: string
): void {
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
