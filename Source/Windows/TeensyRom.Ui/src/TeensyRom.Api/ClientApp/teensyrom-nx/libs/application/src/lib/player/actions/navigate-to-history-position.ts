import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { IPlayerService, PlayerStatus } from '@teensyrom-nx/domain';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore, getPlayerState, setPlayerLoading, setPlayerError } from '../player-helpers';
import { StorageKeyUtil } from '../../storage/storage-key.util';

export interface NavigateToHistoryPositionParams {
  deviceId: string;
  position: number;
}

export function navigateToHistoryPosition(store: WritableStore<PlayerState>, playerService: IPlayerService) {
  return {
    navigateToHistoryPosition: async ({ deviceId, position }: NavigateToHistoryPositionParams): Promise<void> => {
      const actionMessage = createAction('navigate-to-history-position');

      logInfo(LogType.Start, `NavigateToHistoryPosition: Navigating to position ${position} for device ${deviceId}`, { deviceId, position, actionMessage });

      const playerState = getPlayerState(store, deviceId);

      if (!playerState) {
        logError(`NavigateToHistoryPosition: No player state found for device ${deviceId}`);
        return;
      }

      const history = playerState.playHistory;

      if (!history || history.entries.length === 0) {
        logInfo(LogType.Info, `NavigateToHistoryPosition: No history entries available for device ${deviceId}`);
        return;
      }

      // Validate position is within bounds
      if (position < 0 || position >= history.entries.length) {
        logError(`NavigateToHistoryPosition: Position ${position} is out of bounds (0 to ${history.entries.length - 1}) for device ${deviceId}`);
        return;
      }

      const entry = history.entries[position];
      const storageType = StorageKeyUtil.parse(entry.storageKey).storageType;

      logInfo(LogType.Info, `NavigateToHistoryPosition: Loading history entry at position ${position}: ${entry.file.name}`);

      // Set loading state
      setPlayerLoading(store, deviceId, actionMessage);

      try {
        // Launch the file from history
        const launchedFile = await firstValueFrom(playerService.launchFile(deviceId, storageType, entry.file.path));

        logInfo(LogType.Success, `NavigateToHistoryPosition: Successfully launched file from history: ${entry.file.name}`);

        // Update state with the launched file and new history position
        updateState(store, actionMessage, (state) => {
          const currentPlayer = state.players[deviceId];
          if (!currentPlayer) return state;

          return {
            players: {
              ...state.players,
              [deviceId]: {
                ...currentPlayer,
                currentFile: {
                  storageKey: entry.storageKey,
                  file: launchedFile,
                  parentPath: entry.parentPath,
                  launchedAt: entry.timestamp, // Use original timestamp for history matching
                  launchMode: currentPlayer.launchMode,
                  isCompatible: launchedFile.isCompatible,
                },
                playHistory: currentPlayer.playHistory
                  ? {
                      ...currentPlayer.playHistory,
                      currentPosition: position,
                    }
                  : null,
                status: PlayerStatus.Playing,
                isLoading: false,
                error: null,
              },
            },
          };
        });

        logInfo(LogType.Finish, `NavigateToHistoryPosition: Completed navigation to position ${position} for device ${deviceId}`);
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : 'Failed to launch file from history';
        logError(`NavigateToHistoryPosition: Failed to launch file from history position ${position}`, error);
        setPlayerError(store, deviceId, errorMessage, actionMessage);
      }
    },
  };
}
