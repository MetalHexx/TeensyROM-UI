import { computed } from '@angular/core';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';

export function getPlayerError(store: WritableStore<PlayerState>) {
  return {
    getPlayerError: (deviceId: string) =>
      computed(() => {
        return store.players()[deviceId]?.error ?? null;
      }),
  };
}
