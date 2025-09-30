import { updateState } from '@angular-architects/ngrx-toolkit';
import { PlayerStatus } from '@teensyrom-nx/domain';
import { createAction, logInfo, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';

export function updatePlayerStatus(store: WritableStore<PlayerState>) {
  return {
    updatePlayerStatus: ({ deviceId, status }: {
      deviceId: string;
      status: PlayerStatus;
    }): void => {
      const actionMessage = createAction('update-player-status');

      logInfo(LogType.Info, `Updating player status for ${deviceId} to ${status}`, { deviceId, status, actionMessage });

      updateState(store, actionMessage, (state) => ({
        players: {
          ...state.players,
          [deviceId]: {
            ...state.players[deviceId],
            status,
            lastUpdated: Date.now(),
          },
        },
      }));
    },
  };
}