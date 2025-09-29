import { patchState } from '@ngrx/signals';
import { createAction, LogType, logInfo } from '@teensyrom-nx/utils';
import { WritableStore, ensurePlayerState } from '../player-helpers';
import { PlayerState } from '../player-store';
import { LaunchMode } from '@teensyrom-nx/domain';

export function updateLaunchMode(store: WritableStore<PlayerState>) {
  return {
    updateLaunchMode: ({
      deviceId,
      launchMode,
    }: {
      deviceId: string;
      launchMode: LaunchMode;
    }): void => {
      const actionMessage = createAction('update-launch-mode');

      logInfo(LogType.Start, `PlayerAction: Updating launch mode for device ${deviceId} to ${launchMode}`);

      // Ensure player state exists
      ensurePlayerState(store, deviceId, actionMessage);

      // Update launch mode
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
              launchMode,
              lastUpdated: Date.now(),
            },
          },
        };
      });

      logInfo(LogType.Success, `PlayerAction: Launch mode updated to ${launchMode} for device ${deviceId}`);
    },
  };
}