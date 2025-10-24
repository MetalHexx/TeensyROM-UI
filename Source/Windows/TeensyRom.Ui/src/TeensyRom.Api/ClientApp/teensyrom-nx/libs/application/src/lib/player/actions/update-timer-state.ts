import { updateState } from '@angular-architects/ngrx-toolkit';
import { createAction, logInfo, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { TimerState } from '../timer-state.interface';
import { WritableStore } from '../player-helpers';

/**
 * Update timer state for a device.
 *
 * This action updates the timerState in the PlayerStore for a specific device.
 * Used by PlayerContextService to synchronize timer state from PlayerTimerManager.
 *
 * Phase 5 Scope:
 * - Music files only (FileItemType.Song)
 * - Updates complete TimerState object from PlayerTimerManager
 * - Null when no timer active
 *
 * @param store - Player store instance
 * @returns Action method object
 */
export function updateTimerState(store: WritableStore<PlayerState>) {
  return {
    updateTimerState: ({
      deviceId,
      timerState,
    }: {
      deviceId: string;
      timerState: TimerState | null;
    }): void => {
      const actionMessage = createAction('update-timer-state');


      updateState(store, actionMessage, (state) => {
        const deviceState = state.players[deviceId];
        if (!deviceState) {
          logInfo(LogType.Warning, `Cannot update timer state - device ${deviceId} not found`);
          return state;
        }

        return {
          players: {
            ...state.players,
            [deviceId]: {
              ...deviceState,
              timerState,
              lastUpdated: Date.now(),
            },
          },
        };
      });
    },
  };
}
