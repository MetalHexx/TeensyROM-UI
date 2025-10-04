import { computed } from '@angular/core';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';
import { TimerState } from '../timer-state.interface';

/**
 * Get timer state for a device.
 *
 * Returns the current timer state from the PlayerStore for a specific device.
 * Used by components to display timer progress and UI.
 *
 * Phase 5 Scope:
 * - Music files only (FileItemType.Song)
 * - Returns null when no timer active or device not found
 * - Computed signal for reactive updates
 *
 * @param store - Player store instance
 * @returns Selector method object with computed signal
 */
export function getTimerState(store: WritableStore<PlayerState>) {
  return {
    getTimerState: (deviceId: string) =>
      computed<TimerState | null>(() => {
        return store.players()[deviceId]?.timerState ?? null;
      }),
  };
}
