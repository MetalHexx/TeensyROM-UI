import { computed } from '@angular/core';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';

export function isPlayerLoading(store: WritableStore<PlayerState>) {
  return {
    isPlayerLoading: (deviceId: string) =>
      computed(() => {
        return store.players()[deviceId]?.isLoading ?? false;
      }),
  };
}
