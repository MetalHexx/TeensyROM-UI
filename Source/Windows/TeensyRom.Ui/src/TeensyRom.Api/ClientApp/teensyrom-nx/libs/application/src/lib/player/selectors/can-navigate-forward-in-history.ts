import { computed } from '@angular/core';
import { WritableStore } from '../player-helpers';
import { PlayerState } from '../player-store';

export function canNavigateForwardInHistory(store: WritableStore<PlayerState>) {
  return {
    canNavigateForwardInHistory: (deviceId: string) =>
      computed(() => {
        const playerState = store.players()[deviceId];
        const history = playerState?.playHistory;

        if (!history || history.entries.length === 0) {
          return false;
        }

        // Can navigate forward if not at end (position -1) and position is less than last index
        return (
          history.currentPosition !== -1 && history.currentPosition < history.entries.length - 1
        );
      }),
  };
}
