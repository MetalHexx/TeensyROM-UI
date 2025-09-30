import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { PlayerStatus } from '@teensyrom-nx/domain';
import { IPlayerService } from '@teensyrom-nx/domain';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';

export function playPauseMusic(store: WritableStore<PlayerState>, playerService: IPlayerService) {
  return {
    playPauseMusic: async ({ deviceId }: { deviceId: string }): Promise<void> => {
      const actionMessage = createAction('play-pause-music');

      logInfo(LogType.Start, `Toggling music playback for ${deviceId}`, { deviceId, actionMessage });

      const currentState = store.players();
      const playerState = currentState[deviceId];

      if (!playerState) {
        logError(`No player state found for device ${deviceId}`);
        return;
      }

      const currentStatus = playerState.status;

      // Determine what the new status should be based on current status
      const targetStatus = currentStatus === PlayerStatus.Playing ? PlayerStatus.Paused : PlayerStatus.Playing;

      try {
        logInfo(LogType.NetworkRequest, `Calling toggleMusic API for ${deviceId}`);
        await firstValueFrom(playerService.toggleMusic(deviceId));

        logInfo(LogType.Success, `Music toggle successful for ${deviceId}, new status: ${targetStatus}`);

        updateState(store, actionMessage, (state) => ({
          players: {
            ...state.players,
            [deviceId]: {
              ...state.players[deviceId],
              status: targetStatus,
              error: null,
              lastUpdated: Date.now(),
            },
          },
        }));

      } catch (error) {
        const errorMessage = (error as any)?.message || 'Failed to toggle music playback';
        logError(`Music toggle failed for ${deviceId}:`, error);

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