import { computed } from '@angular/core';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';

export function getDevicePlayer(store: WritableStore<PlayerState>) {
  return {
    getDevicePlayer: (deviceId: string) =>
      computed(() => {
        return store.players()[deviceId] ?? null;
      }),
  };
}
