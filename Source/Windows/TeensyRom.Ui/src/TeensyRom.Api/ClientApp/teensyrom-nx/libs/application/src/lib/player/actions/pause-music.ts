import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { PlayerStatus } from '@teensyrom-nx/domain';
import { IPlayerService } from '@teensyrom-nx/domain';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';

export function pauseMusic(store: WritableStore<PlayerState>, playerService: IPlayerService) {
  return {
    pauseMusic: async ({ deviceId }: { deviceId: string }): Promise<void> => {
      const actionMessage = createAction('pause-music');

      const currentState = store.players();
      const playerState = currentState[deviceId];

      if (!playerState) {
        logError(`No player state found for device ${deviceId}`);
        return;
      }

      const currentStatus = playerState.status;

      // NOOP if already paused or stopped
      if (currentStatus === PlayerStatus.Paused || currentStatus === PlayerStatus.Stopped) {
        logInfo(LogType.Info, `Player already paused/stopped for ${deviceId}, no action needed`);
        return;
      }

      logInfo(LogType.Start, `Pausing music playback for ${deviceId}`, { deviceId, actionMessage });

      try {
        logInfo(LogType.NetworkRequest, `Calling toggleMusic API for ${deviceId}`);
        await firstValueFrom(playerService.toggleMusic(deviceId));

        logInfo(LogType.Success, `Music pause successful for ${deviceId}, new status: Paused`);

        updateState(store, actionMessage, (state) => ({
          players: {
            ...state.players,
            [deviceId]: {
              ...state.players[deviceId],
              status: PlayerStatus.Paused,
              error: null,
              lastUpdated: Date.now(),
            },
          },
        }));

      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : 'Failed to pause music';
        logError(`Music pause failed for ${deviceId}:`, error);

        updateState(store, actionMessage, (state) => ({
          players: {
            ...state.players,
            [deviceId]: {
              ...state.players[deviceId],
              status: currentStatus, // Revert to original status on error
              error: errorMessage,
              lastUpdated: Date.now(),
            },
          },
        }));
      }
    },
  };
}