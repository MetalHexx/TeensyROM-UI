import { createAction, logInfo, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore, ensurePlayerState } from '../player-helpers';

export interface InitializePlayerParams {
  deviceId: string;
}

export function initializePlayer(store: WritableStore<PlayerState>) {
  return {
    initializePlayer: ({ deviceId }: InitializePlayerParams): void => {
      logInfo(LogType.Start, `PlayerAction: Initializing player for device ${deviceId}`);

      const actionMessage = createAction('initialize-player');
      ensurePlayerState(store, deviceId, actionMessage);

      logInfo(
        LogType.Success,
        `PlayerAction: Player initialized successfully for device ${deviceId}`
      );
    },
  };
}
