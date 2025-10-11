import { computed } from '@angular/core';
import { WritableStore } from '../player-helpers';
import { PlayerState, PlayHistory } from '../player-store';

export function getPlayHistory(store: WritableStore<PlayerState>) {
  return {
    getPlayHistory: (deviceId: string) =>
      computed(() => {
        const playerState = store.players()[deviceId];
        return playerState?.playHistory ?? null;
      }),
  };
}
