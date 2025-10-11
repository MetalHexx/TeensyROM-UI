import { computed } from '@angular/core';
import { WritableStore } from '../player-helpers';
import { PlayerState } from '../player-store';

export function getCurrentHistoryPosition(store: WritableStore<PlayerState>) {
  return {
    getCurrentHistoryPosition: (deviceId: string) =>
      computed(() => {
        const playerState = store.players()[deviceId];
        return playerState?.playHistory?.currentPosition ?? -1;
      }),
  };
}
