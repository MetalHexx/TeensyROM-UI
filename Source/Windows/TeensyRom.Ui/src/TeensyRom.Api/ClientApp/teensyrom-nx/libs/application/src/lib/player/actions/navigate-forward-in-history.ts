import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { IPlayerService, PlayerStatus } from '@teensyrom-nx/domain';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore, getPlayerState, setPlayerLoading, setPlayerError } from '../player-helpers';
import { StorageKeyUtil } from '../../storage/storage-key.util';

export interface NavigateForwardInHistoryParams {
  deviceId: string;
}

export function navigateForwardInHistory(
  writableStore: WritableStore<PlayerState>,
  playerService: IPlayerService
) {
  return {
    navigateForwardInHistory: async (params: NavigateForwardInHistoryParams): Promise<void> => {
      const { deviceId } = params;
      const actionMessage = createAction('navigate-forward-in-history');

    logInfo(LogType.Start, `NavigateForwardInHistory: Starting forward navigation for device ${deviceId}`, {
      deviceId,
      actionMessage,
    });

    // Get player state and validate
    const player = getPlayerState(writableStore, deviceId);
    if (!player) {
      logError(`NavigateForwardInHistory: Player not found for device ${deviceId}`);
      return;
    }

    // Get history and validate
    const history = player.playHistory;
    if (!history || history.entries.length === 0) {
      logInfo(LogType.Info, `NavigateForwardInHistory: No history for device ${deviceId}, cannot navigate forward`);
      return;
    }

    const currentPosition = history.currentPosition;

    // Already at end (-1), cannot go forward
    if (currentPosition === -1) {
      logInfo(LogType.Info, `NavigateForwardInHistory: Already at end (-1) for device ${deviceId}, cannot go forward`);
      return;
    }

    // Calculate target position
    const targetPosition = currentPosition + 1;

    // Check if target is beyond history
    if (targetPosition >= history.entries.length) {
      logInfo(LogType.Info, `NavigateForwardInHistory: Target position ${targetPosition} beyond history length ${history.entries.length} for device ${deviceId}`);
      return;
    }

    // Get history entry
    const historyEntry = history.entries[targetPosition];
    if (!historyEntry) {
      logError(`NavigateForwardInHistory: No entry at position ${targetPosition} for device ${deviceId}`);
      return;
    }

    logInfo(LogType.Info, `NavigateForwardInHistory: Moving from position ${currentPosition} to ${targetPosition}`);
    logInfo(LogType.Info, `NavigateForwardInHistory: Navigating to file ${historyEntry.file.name} from history`);

    // Extract storage type from storage key
    const { storageType } = StorageKeyUtil.parse(historyEntry.storageKey);

    // Set loading state
    setPlayerLoading(writableStore, deviceId, actionMessage);

    try {
      // Launch the file from history
      const launchedFile = await firstValueFrom(
        playerService.launchFile(deviceId, storageType, historyEntry.file.path)
      );

      logInfo(LogType.Success, `NavigateForwardInHistory: File ${launchedFile.name} launched successfully`);

      // On success, update state with the launched file and new position
      updateState(writableStore, actionMessage, (state) => {
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
                launchedAt: historyEntry.timestamp, // Use original history entry timestamp
                launchMode: player.launchMode,
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

      logInfo(LogType.Finish, `NavigateForwardInHistory: Forward navigation completed for device ${deviceId}. New position: ${targetPosition}`);

    } catch (error) {
      const errorMessage = (error as Error)?.message || 'Failed to navigate forward in history';
      logError(`NavigateForwardInHistory: Failed for device ${deviceId}:`, error);
      setPlayerError(writableStore, deviceId, errorMessage, actionMessage);
    }
    },
  };
}
