import { computed } from '@angular/core';
import { WritableStore } from '../player-helpers';
import { PlayerState } from '../player-store';

export function canNavigateBackwardInHistory(store: WritableStore<PlayerState>) {
  return {
    canNavigateBackwardInHistory: (deviceId: string) =>
      computed(() => {
        const playerState = store.players()[deviceId];
        const history = playerState?.playHistory;

        if (!history || history.entries.length === 0) {
          return false;
        }

        // Can always navigate backward if history exists (includes wraparound from position 0)
        return true;
      }),
  };
}
