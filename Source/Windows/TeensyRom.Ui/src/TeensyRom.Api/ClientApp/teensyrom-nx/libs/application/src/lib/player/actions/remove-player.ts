import { createAction, logInfo, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore, removePlayerState } from '../player-helpers';

export interface RemovePlayerParams {
  deviceId: string;
}

export function removePlayer(store: WritableStore<PlayerState>) {
  return {
    removePlayer: ({ deviceId }: RemovePlayerParams): void => {
      logInfo(LogType.Cleanup, `PlayerAction: Removing player state for device ${deviceId}`);

      const actionMessage = createAction('remove-player');
      removePlayerState(store, deviceId, actionMessage);
    },
  };
}
