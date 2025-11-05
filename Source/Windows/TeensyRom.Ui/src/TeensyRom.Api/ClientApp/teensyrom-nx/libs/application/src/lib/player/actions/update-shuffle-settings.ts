import { updateState } from '@angular-architects/ngrx-toolkit';
import { createAction, LogType, logInfo } from '@teensyrom-nx/utils';
import { WritableStore, ensurePlayerState } from '../player-helpers';
import { PlayerState, ShuffleSettings } from '../player-store';

export function updateShuffleSettings(store: WritableStore<PlayerState>) {
  return {
    updateShuffleSettings: ({
      deviceId,
      shuffleSettings,
    }: {
      deviceId: string;
      shuffleSettings: Partial<ShuffleSettings>;
    }): void => {
      const actionMessage = createAction('update-shuffle-settings');

      logInfo(
        LogType.Start,
        `PlayerAction: Updating shuffle settings for device ${deviceId}`,
        shuffleSettings
      );

      // Ensure player state exists
      ensurePlayerState(store, deviceId, actionMessage);

      // Update shuffle settings
      updateState(store, actionMessage, (state) => {
        const currentPlayer = state.players[deviceId];
        if (!currentPlayer) {
          return state;
        }

        return {
          players: {
            ...state.players,
            [deviceId]: {
              ...currentPlayer,
              shuffleSettings: {
                ...currentPlayer.shuffleSettings,
                ...shuffleSettings,
              },
              lastUpdated: Date.now(),
            },
          },
        };
      });

      logInfo(LogType.Success, `PlayerAction: Shuffle settings updated for device ${deviceId}`);
    },
  };
}
