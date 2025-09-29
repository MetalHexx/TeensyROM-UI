import { patchState } from '@ngrx/signals';
import { createAction, LogType, logInfo } from '@teensyrom-nx/utils';
import { WritableStore, ensurePlayerState } from '../player-helpers';
import { PlayerState } from '../player-store';
import { FileItem, LaunchMode, StorageType } from '@teensyrom-nx/domain';
import { createPlayerFileContext } from '../player-helpers';

export function loadFileContext(store: WritableStore<PlayerState>) {
  return {
    loadFileContext: ({
      deviceId,
      storageType,
      directoryPath,
      files,
      currentFileIndex,
      launchMode,
    }: {
      deviceId: string;
      storageType: StorageType;
      directoryPath: string;
      files: FileItem[];
      currentFileIndex: number;
      launchMode: LaunchMode;
    }): void => {
      const actionMessage = createAction('load-file-context');

      logInfo(LogType.Start, `PlayerAction: Loading file context for device ${deviceId} with ${files.length} files`);

      // Ensure player state exists
      ensurePlayerState(store, deviceId, actionMessage);

      // Create file context
      const fileContext = createPlayerFileContext(
        deviceId,
        storageType,
        directoryPath,
        files,
        currentFileIndex,
        launchMode
      );

      // Update file context in state
      patchState(store, (state) => {
        const currentPlayer = state.players[deviceId];
        if (!currentPlayer) {
          return state;
        }

        return {
          players: {
            ...state.players,
            [deviceId]: {
              ...currentPlayer,
              fileContext,
              launchMode,
              lastUpdated: Date.now(),
            },
          },
        };
      });

      logInfo(LogType.Success, `PlayerAction: File context loaded for device ${deviceId} with ${files.length} files`);
    },
  };
}