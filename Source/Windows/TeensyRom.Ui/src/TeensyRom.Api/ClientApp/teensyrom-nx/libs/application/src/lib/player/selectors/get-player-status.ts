import { computed } from '@angular/core';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';
import { PlayerStatus } from '@teensyrom-nx/domain';

export function getPlayerStatus(store: WritableStore<PlayerState>) {
  return {
    getPlayerStatus: (deviceId: string) =>
      computed<PlayerStatus>(() => {
        return store.players()[deviceId]?.status ?? PlayerStatus.Stopped;
      }),
  };
}
