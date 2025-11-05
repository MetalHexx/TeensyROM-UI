import { updateState } from '@angular-architects/ngrx-toolkit';
import { createAction, logInfo, LogType } from '@teensyrom-nx/utils';
import { WritableStore } from '../player-helpers';
import { PlayerState } from '../player-store';

export interface UpdateHistoryViewVisibilityParams {
  deviceId: string;
  visible: boolean;
}

export function updateHistoryViewVisibility(
  store: WritableStore<PlayerState>,
  params: UpdateHistoryViewVisibilityParams
): void {
  const { deviceId, visible } = params;
  const actionMessage = createAction('update-history-view-visibility');

  logInfo(
    LogType.Start,
    `${actionMessage}: Setting history view visibility to ${visible} for device ${deviceId}`
  );

  const playerState = store.players()[deviceId];
  if (!playerState) {
    logInfo(LogType.Warning, `${actionMessage}: Player state not found for device ${deviceId}`);
    return;
  }

  updateState(store, actionMessage, (state) => ({
    players: {
      ...state.players,
      [deviceId]: {
        ...playerState,
        historyViewVisible: visible,
      },
    },
  }));

  logInfo(
    LogType.Success,
    `${actionMessage}: History view visibility updated to ${visible} for device ${deviceId}`
  );
}
