import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { PlayerStatus, IDeviceService } from '@teensyrom-nx/domain';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';

export function stopPlayback(store: WritableStore<PlayerState>, deviceService: IDeviceService) {
  return {
    stopPlayback: async ({ deviceId }: { deviceId: string }): Promise<void> => {
      const actionMessage = createAction('stop-playback');

      logInfo(LogType.Start, `Stopping playback for ${deviceId}`, { deviceId, actionMessage });

      try {
        logInfo(LogType.NetworkRequest, `Calling resetDevice API for ${deviceId}`);
        await firstValueFrom(deviceService.resetDevice(deviceId));

        logInfo(LogType.Success, `Device reset successful for ${deviceId}, status: Stopped`);

        updateState(store, actionMessage, (state) => ({
          players: {
            ...state.players,
            [deviceId]: {
              ...state.players[deviceId],
              status: PlayerStatus.Stopped,
              error: null,
              lastUpdated: Date.now(),
            },
          },
        }));
      } catch (error) {
        const errorMessage = (error as any)?.message || 'Failed to stop playback';
        logError(`Stop playback failed for ${deviceId}:`, error);

        updateState(store, actionMessage, (state) => ({
          players: {
            ...state.players,
            [deviceId]: {
              ...state.players[deviceId],
              status: PlayerStatus.Stopped, // Still set to stopped even on error
              error: errorMessage,
              lastUpdated: Date.now(),
            },
          },
        }));
      }
    },
  };
}
