import { StateSignals, WritableStateSource } from '@ngrx/signals';
import { updateState } from '@angular-architects/ngrx-toolkit';
import { PlayerState, DevicePlayerState, LaunchedFile, PlayerFileContext } from './player-store';
import { FileItem, PlayerStatus, LaunchMode, StorageType, PlayerFilterType, PlayerScope } from '@teensyrom-nx/domain';
import { StorageKeyUtil, StorageKey } from '../storage/storage-key.util';
import { logInfo, logError, LogType } from '@teensyrom-nx/utils';

export type WritableStore<T extends object> = StateSignals<T> & WritableStateSource<T>;

export function createDefaultDeviceState(deviceId: string): DevicePlayerState {
  return {
    deviceId,
    currentFile: null,
    fileContext: null,
    status: PlayerStatus.Stopped,
    launchMode: LaunchMode.Directory,
    shuffleSettings: {
      scope: PlayerScope.Storage,
      filter: PlayerFilterType.All,
      startingDirectory: undefined,
    },
    playHistory: null,
    historyViewVisible: false,
    timerState: null,
    isLoading: false,
    error: null,
    lastUpdated: null,
  };
}

export function ensurePlayerState(
  store: WritableStore<PlayerState>,
  deviceId: string,
  actionMessage: string
): DevicePlayerState {
  const existing = store.players()[deviceId];
  if (existing) {
    logInfo(LogType.Info, `PlayerHelper: Using existing player state for device ${deviceId}`);
    return existing;
  }

  logInfo(LogType.Start, `PlayerHelper: Creating new player state for device ${deviceId}`);

  const defaultState = createDefaultDeviceState(deviceId);
  updateState(store, actionMessage, (state) => ({
    players: {
      ...state.players,
      [deviceId]: defaultState,
    },
  }));

  logInfo(LogType.Success, `PlayerHelper: New player state created for device ${deviceId}`);

  return defaultState;
}

function updatePlayerState(
  store: WritableStore<PlayerState>,
  deviceId: string,
  updater: (state: DevicePlayerState) => DevicePlayerState,
  actionMessage: string
): void {
  updateState(store, actionMessage, (state) => {
    const current = state.players[deviceId];
    if (!current) {
      return state;
    }

    return {
      players: {
        ...state.players,
        [deviceId]: updater(current),
      },
    };
  });
}

export function setPlayerLoading(
  store: WritableStore<PlayerState>,
  deviceId: string,
  actionMessage: string
): void {
  logInfo(LogType.Start, `PlayerHelper: Setting player loading state for device ${deviceId}`);

  ensurePlayerState(store, deviceId, actionMessage);
  updatePlayerState(
    store,
    deviceId,
    (state) => {
      logInfo(LogType.Info, `PlayerHelper: Updating isLoading flag for device ${deviceId}`);

      return {
        ...state,
        isLoading: true,
        error: null,
      };
    },
    actionMessage
  );
}

export function setPlayerLaunchSuccess(
  store: WritableStore<PlayerState>,
  deviceId: string,
  launchedFile: LaunchedFile,
  fileContext: PlayerFileContext,
  launchMode: LaunchMode,
  actionMessage: string
): void {
  const timestamp = Date.now();

  logInfo(LogType.Success, `PlayerHelper: Setting player launch success for device ${deviceId} with file ${launchedFile.file.name}`);

  ensurePlayerState(store, deviceId, actionMessage);
  updatePlayerState(
    store,
    deviceId,
    (state) => {
      logInfo(LogType.Info, `PlayerHelper: Updating player state to Playing for device ${deviceId}`);

      return {
        ...state,
        currentFile: launchedFile,
        fileContext,
        isLoading: false,
        status: PlayerStatus.Playing,
        error: null,
        launchMode: launchMode,
        lastUpdated: timestamp,
      };
    },
    actionMessage
  );
}

export function setPlayerError(
  store: WritableStore<PlayerState>,
  deviceId: string,
  errorMessage: string,
  actionMessage: string
): void {
  const timestamp = Date.now();
  
  logError(`PlayerHelper: Setting player error state for device ${deviceId}: ${errorMessage}`);

  ensurePlayerState(store, deviceId, actionMessage);
  updatePlayerState(
    store,
    deviceId,
    (state) => {
      logInfo(LogType.Info, `PlayerHelper: Updating player state to Stopped due to error for device ${deviceId}`);

      return {
        ...state,
        isLoading: false,
        status: PlayerStatus.Stopped,
        error: errorMessage,
        lastUpdated: timestamp,
      };
    },
    actionMessage
  );
}

export function setPlayerLaunchFailure(
  store: WritableStore<PlayerState>,
  deviceId: string,
  launchedFile: LaunchedFile,
  fileContext: PlayerFileContext,
  launchMode: LaunchMode,
  errorMessage: string,
  actionMessage: string
): void {
  const timestamp = Date.now();

  logError(`PlayerHelper: Setting player launch failure for device ${deviceId} with file ${launchedFile.file.name}: ${errorMessage}`);

  ensurePlayerState(store, deviceId, actionMessage);
  updatePlayerState(
    store,
    deviceId,
    (state) => {
      logInfo(LogType.Info, `PlayerHelper: Updating player state with failed file context for device ${deviceId}`);

      return {
        ...state,
        currentFile: launchedFile,
        fileContext,
        isLoading: false,
        status: PlayerStatus.Stopped,
        error: errorMessage,
        launchMode: launchMode,
        lastUpdated: timestamp,
      };
    },
    actionMessage
  );
}

export function removePlayerState(
  store: WritableStore<PlayerState>,
  deviceId: string,
  actionMessage: string
): void {
  updateState(store, actionMessage, (state) => {
    if (!(deviceId in state.players)) {
      return state;
    }

    const updated = { ...state.players };
    delete updated[deviceId];

    logInfo(LogType.Success, `Player state for device ${deviceId} removed.`);

    return {
      players: updated,
    };
  });
}

export function createLaunchedFile(
  deviceId: string,
  storageType: StorageType,
  file: FileItem,
  isCompatible = true
): LaunchedFile {
  const timestamp = Date.now();
  const storageKey = StorageKeyUtil.create(deviceId, storageType);

  logInfo(LogType.Info, `PlayerHelper: Creating launched file object for ${file.name} on device ${deviceId} (compatible: ${isCompatible})`);

  return {
    parentPath: file.parentPath,
    storageKey,
    file,
    launchedAt: timestamp,
    isCompatible,
  };
}

export function createPlayerFileContext(
  deviceId: string,
  storageType: StorageType,
  directoryPath: string,
  files: FileItem[],
  currentIndex: number,
  launchMode: LaunchMode
): PlayerFileContext {
  const storageKey = StorageKeyUtil.create(deviceId, storageType);
  
  logInfo(LogType.Info, `PlayerHelper: Creating player file context with ${files.length} files for device ${deviceId}`);

  return {
    storageKey,
    directoryPath,
    files,
    currentIndex,
    launchMode,
  };
}

export function getPlayerState(store: WritableStore<PlayerState>, deviceId: string): DevicePlayerState | null {
  return store.players()[deviceId] ?? null;
}

/**
 * Helper for navigation actions - handles compatible file launches in shuffle mode
 */
export function setShuffleNavigationSuccess(
  store: WritableStore<PlayerState>,
  deviceId: string,
  launchedFile: FileItem,
  existingStorageKey: StorageKey | undefined,
  actionMessage: string
): void {
  const timestamp = Date.now();
  const storageKey = existingStorageKey || StorageKeyUtil.create(deviceId, StorageType.Sd);
  const parentPath = launchedFile.path.substring(0, launchedFile.path.lastIndexOf('/')) || '/';

  logInfo(LogType.Success, `PlayerHelper: Shuffle navigation success for device ${deviceId} with file ${launchedFile.name}`);

  updateState(store, actionMessage, (state) => ({
    players: {
      ...state.players,
      [deviceId]: {
        ...state.players[deviceId],
        currentFile: {
          storageKey,
          file: launchedFile,
          parentPath,
          launchedAt: timestamp,
          isCompatible: launchedFile.isCompatible,
        },
        status: PlayerStatus.Playing,
        error: null,
        lastUpdated: timestamp,
      },
    },
  }));
}

/**
 * Helper for navigation actions - handles incompatible file launches in shuffle mode
 */
export function setShuffleNavigationFailure(
  store: WritableStore<PlayerState>,
  deviceId: string,
  launchedFile: FileItem,
  existingStorageKey: StorageKey | undefined,
  errorMessage: string,
  actionMessage: string
): void {
  const timestamp = Date.now();
  const storageKey = existingStorageKey || StorageKeyUtil.create(deviceId, StorageType.Sd);
  const parentPath = launchedFile.path.substring(0, launchedFile.path.lastIndexOf('/')) || '/';

  logError(`PlayerHelper: Shuffle navigation failure for device ${deviceId} with file ${launchedFile.name}: ${errorMessage}`);

  updateState(store, actionMessage, (state) => ({
    players: {
      ...state.players,
      [deviceId]: {
        ...state.players[deviceId],
        currentFile: {
          storageKey,
          file: launchedFile,
          parentPath,
          launchedAt: timestamp,
          isCompatible: launchedFile.isCompatible,
        },
        status: PlayerStatus.Stopped,
        error: errorMessage,
        lastUpdated: timestamp,
      },
    },
  }));
}

/**
 * Helper for navigation actions - handles compatible file launches in directory mode
 */
export function setDirectoryNavigationSuccess(
  store: WritableStore<PlayerState>,
  deviceId: string,
  launchedFile: FileItem,
  fileContext: PlayerFileContext,
  newIndex: number,
  actionMessage: string
): void {
  const timestamp = Date.now();

  logInfo(LogType.Success, `PlayerHelper: Directory navigation success for device ${deviceId} with file ${launchedFile.name}`);

  updateState(store, actionMessage, (state) => ({
    players: {
      ...state.players,
      [deviceId]: {
        ...state.players[deviceId],
        currentFile: {
          storageKey: fileContext.storageKey,
          file: launchedFile,
          parentPath: fileContext.directoryPath,
          launchedAt: timestamp,
          isCompatible: launchedFile.isCompatible,
        },
        fileContext: {
          ...fileContext,
          currentIndex: newIndex,
        },
        status: PlayerStatus.Playing,
        error: null,
        lastUpdated: timestamp,
      },
    },
  }));
}

/**
 * Helper for navigation actions - handles incompatible file launches in directory mode
 */
export function setDirectoryNavigationFailure(
  store: WritableStore<PlayerState>,
  deviceId: string,
  launchedFile: FileItem,
  fileContext: PlayerFileContext,
  newIndex: number,
  errorMessage: string,
  actionMessage: string
): void {
  const timestamp = Date.now();

  logError(`PlayerHelper: Directory navigation failure for device ${deviceId} with file ${launchedFile.name}: ${errorMessage}`);

  updateState(store, actionMessage, (state) => ({
    players: {
      ...state.players,
      [deviceId]: {
        ...state.players[deviceId],
        currentFile: {
          storageKey: fileContext.storageKey,
          file: launchedFile,
          parentPath: fileContext.directoryPath,
          launchedAt: timestamp,
          isCompatible: launchedFile.isCompatible,
        },
        fileContext: {
          ...fileContext,
          currentIndex: newIndex,
        },
        status: PlayerStatus.Stopped,
        error: errorMessage,
        lastUpdated: timestamp,
      },
    },
  }));
}



