import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { PlayerStatus } from '@teensyrom-nx/domain';
import { IPlayerService } from '@teensyrom-nx/domain';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';

export function play(store: WritableStore<PlayerState>, playerService: IPlayerService) {
  return {
    play: async ({ deviceId }: { deviceId: string }): Promise<void> => {
      const actionMessage = createAction('play');

      const currentState = store.players();
      const playerState = currentState[deviceId];

      if (!playerState) {
        logError(`No player state found for device ${deviceId}`);
        return;
      }

      const currentStatus = playerState.status;

      // NOOP if already playing
      if (currentStatus === PlayerStatus.Playing) {
        logInfo(LogType.Info, `Player already playing for ${deviceId}, no action needed`);
        return;
      }

      logInfo(LogType.Start, `Starting playback for ${deviceId}`, { deviceId, actionMessage });

      try {
        logInfo(LogType.NetworkRequest, `Calling toggleMusic API for ${deviceId}`);
        await firstValueFrom(playerService.toggleMusic(deviceId));

        logInfo(LogType.Success, `Play successful for ${deviceId}, new status: Playing`);

        updateState(store, actionMessage, (state) => ({
          players: {
            ...state.players,
            [deviceId]: {
              ...state.players[deviceId],
              status: PlayerStatus.Playing,
              error: null,
              lastUpdated: Date.now(),
            },
          },
        }));
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : 'Failed to play';
        logError(`Play failed for ${deviceId}:`, error);

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
