import { computed } from '@angular/core';
import { WritableStore } from '../player-helpers';
import { PlayerState } from '../player-store';

/**
 * Returns a signal indicating whether the history view is visible for a device.
 * @param store - The player store
 * @returns Object with isHistoryViewVisible method that accepts deviceId
 */
export function isHistoryViewVisible(store: WritableStore<PlayerState>) {
  return {
    isHistoryViewVisible: (deviceId: string) =>
      computed(() => {
        const playerState = store.players()[deviceId];

        if (!playerState) {
          return false;
        }

        return playerState.historyViewVisible;
      }),
  };
}
