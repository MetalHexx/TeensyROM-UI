import { updateState } from '@angular-architects/ngrx-toolkit';
import { createAction, logInfo, LogType } from '@teensyrom-nx/utils';
import { PlayerState, HistoryEntry } from '../player-store';
import { WritableStore, getPlayerState } from '../player-helpers';

const MAX_HISTORY_ENTRIES = 1000;

export interface RecordHistoryParams {
  deviceId: string;
  entry: HistoryEntry;
}

export function recordHistory(store: WritableStore<PlayerState>) {
  return {
    recordHistory: ({ deviceId, entry }: RecordHistoryParams): void => {
      const actionMessage = createAction('record-history');

      logInfo(
        LogType.Start,
        `RecordHistory: Recording history entry for device ${deviceId}, file: ${entry.file.name}`
      );

      const playerState = getPlayerState(store, deviceId);

      if (!playerState) {
        logInfo(
          LogType.Info,
          `RecordHistory: No player state for device ${deviceId}, skipping history recording`
        );
        return;
      }

      const currentHistory = playerState.playHistory;

      // Check for consecutive duplicate - skip if same file as last entry
      if (currentHistory && currentHistory.entries.length > 0) {
        const lastEntry = currentHistory.entries[currentHistory.entries.length - 1];
        if (lastEntry.file.path === entry.file.path) {
          logInfo(
            LogType.Info,
            `RecordHistory: Skipping duplicate consecutive entry for file ${entry.file.name}`
          );
          return;
        }
      }

      updateState(store, actionMessage, (state) => {
        const player = state.players[deviceId];
        if (!player) {
          return state;
        }

        let history = player.playHistory;

        // Initialize history if null
        if (!history) {
          logInfo(LogType.Info, `RecordHistory: Initializing history for device ${deviceId}`);
          history = {
            entries: [],
            currentPosition: -1,
          };
        }

        let newEntries = [...history.entries];

        // Clear forward history if user navigated backward
        if (history.currentPosition !== -1) {
          logInfo(
            LogType.Info,
            `RecordHistory: Clearing forward history from position ${history.currentPosition}`
          );
          newEntries = newEntries.slice(0, history.currentPosition + 1);
        }

        // Add new entry
        newEntries.push(entry);

        // Enforce maximum size - remove oldest entries if needed
        if (newEntries.length > MAX_HISTORY_ENTRIES) {
          const entriesToRemove = newEntries.length - MAX_HISTORY_ENTRIES;
          logInfo(
            LogType.Info,
            `RecordHistory: Removing ${entriesToRemove} oldest entries to maintain max size of ${MAX_HISTORY_ENTRIES}`
          );
          newEntries = newEntries.slice(entriesToRemove);
        }

        logInfo(
          LogType.Success,
          `RecordHistory: Entry recorded successfully. Total entries: ${newEntries.length}`
        );

        return {
          players: {
            ...state.players,
            [deviceId]: {
              ...player,
              playHistory: {
                entries: newEntries,
                currentPosition: -1, // Always reset to end after recording
              },
            },
          },
        };
      });
    },
  };
}
