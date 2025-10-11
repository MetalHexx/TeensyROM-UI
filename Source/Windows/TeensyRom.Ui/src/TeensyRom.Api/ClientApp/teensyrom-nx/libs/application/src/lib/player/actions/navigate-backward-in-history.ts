import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { IPlayerService, PlayerStatus } from '@teensyrom-nx/domain';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore, getPlayerState, setPlayerLoading, setPlayerError } from '../player-helpers';
import { StorageKeyUtil } from '../../storage/storage-key.util';

export interface NavigateBackwardInHistoryParams {
  deviceId: string;
}

export function navigateBackwardInHistory(store: WritableStore<PlayerState>, playerService: IPlayerService) {
  return {
    navigateBackwardInHistory: async ({ deviceId }: NavigateBackwardInHistoryParams): Promise<void> => {
      const actionMessage = createAction('navigate-backward-in-history');

      logInfo(LogType.Start, `NavigateBackwardInHistory: Starting backward navigation for device ${deviceId}`, { deviceId, actionMessage });

      const playerState = getPlayerState(store, deviceId);

      if (!playerState) {
        logError(`NavigateBackwardInHistory: No player state found for device ${deviceId}`);
        return;
      }

      const history = playerState.playHistory;

      if (!history || history.entries.length === 0) {
        logInfo(LogType.Info, `NavigateBackwardInHistory: No history entries available for device ${deviceId}`);
        return;
      }

      const currentPosition = history.currentPosition;

      // Calculate target position with wraparound logic
      let targetPosition: number;
      if (currentPosition === -1) {
        // At end, move to most recent entry
        targetPosition = history.entries.length - 1;
        logInfo(LogType.Info, `NavigateBackwardInHistory: At end (-1), moving to most recent entry at position ${targetPosition}`);
      } else if (currentPosition === 0) {
        // At start, wrap to end
        targetPosition = history.entries.length - 1;
        logInfo(LogType.Info, `NavigateBackwardInHistory: At start (0), wrapping to end at position ${targetPosition}`);
      } else {
        // Normal backward movement
        targetPosition = currentPosition - 1;
        logInfo(LogType.Info, `NavigateBackwardInHistory: Moving from position ${currentPosition} to ${targetPosition}`);
      }

      const historyEntry = history.entries[targetPosition];

      if (!historyEntry) {
        logError(`NavigateBackwardInHistory: No history entry found at position ${targetPosition}`);
        return;
      }

      logInfo(LogType.Info, `NavigateBackwardInHistory: Navigating to file ${historyEntry.file.name} from history`);

      // Extract storage type from storage key
      const { storageType } = StorageKeyUtil.parse(historyEntry.storageKey);

      // Set loading state
      setPlayerLoading(store, deviceId, actionMessage);

      try {
        // Launch the file from history
        const launchedFile = await firstValueFrom(
          playerService.launchFile(deviceId, storageType, historyEntry.file.path)
        );

        logInfo(LogType.Success, `NavigateBackwardInHistory: File ${launchedFile.name} launched successfully`);

        // On success, update state with the launched file and new position
        updateState(store, actionMessage, (state) => {
          const player = state.players[deviceId];
          if (!player || !player.playHistory) {
            return state;
          }

          const timestamp = Date.now();

          return {
            players: {
              ...state.players,
              [deviceId]: {
                ...player,
                currentFile: {
                  storageKey: historyEntry.storageKey,
                  file: launchedFile,
                  parentPath: historyEntry.parentPath,
                  launchedAt: timestamp,
                  launchMode: historyEntry.launchMode,
                  isCompatible: launchedFile.isCompatible,
                },
                status: PlayerStatus.Playing,
                playHistory: {
                  ...player.playHistory,
                  currentPosition: targetPosition,
                },
                error: null,
                isLoading: false,
                lastUpdated: timestamp,
              },
            },
          };
        });

        logInfo(LogType.Finish, `NavigateBackwardInHistory: Backward navigation completed for device ${deviceId}. New position: ${targetPosition}`);

      } catch (error) {
        const errorMessage = (error as Error)?.message || 'Failed to navigate backward in history';
        logError(`NavigateBackwardInHistory: Failed for device ${deviceId}:`, error);

        setPlayerError(store, deviceId, errorMessage, actionMessage);
      }
    },
  };
}
