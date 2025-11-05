import { computed } from '@angular/core';
import { WritableStore } from '../player-helpers';
import { PlayerState, ShuffleSettings } from '../player-store';

export function getShuffleSettings(store: WritableStore<PlayerState>) {
  return {
    getShuffleSettings: (deviceId: string) =>
      computed(() => {
        const player = store.players()[deviceId];
        return player?.shuffleSettings ?? null;
      }),
  };
}
