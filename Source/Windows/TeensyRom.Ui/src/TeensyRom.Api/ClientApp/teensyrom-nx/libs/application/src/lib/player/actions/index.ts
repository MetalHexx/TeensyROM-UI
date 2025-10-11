import { inject } from '@angular/core';
import { withMethods } from '@ngrx/signals';
import { PLAYER_SERVICE, IPlayerService, DEVICE_SERVICE, IDeviceService } from '@teensyrom-nx/domain';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';
import { initializePlayer } from './initialize-player';
import { launchFileWithContext } from './launch-file-with-context';
import { launchRandomFile } from './launch-random-file';
import { loadFileContext } from './load-file-context';
import { updateShuffleSettings } from './update-shuffle-settings';
import { updateLaunchMode } from './update-launch-mode';
import { updatePlayerStatus } from './update-player-status';
import { updateTimerState } from './update-timer-state';
import { play } from './play';
import { pauseMusic } from './pause-music';
import { stopPlayback } from './stop-playback';
import { navigateNext } from './navigate-next';
import { navigatePrevious } from './navigate-previous';
import { removePlayer } from './remove-player';
import { recordHistory } from './record-history';
import { clearHistory } from './clear-history';
import { navigateBackwardInHistory } from './navigate-backward-in-history';
import { navigateForwardInHistory } from './navigate-forward-in-history';
import { updateHistoryViewVisibility } from './update-history-view-visibility';
import { navigateToHistoryPosition } from './navigate-to-history-position';

export function withPlayerActions() {
  return withMethods((
    store,
    playerService: IPlayerService = inject(PLAYER_SERVICE),
    deviceService: IDeviceService = inject(DEVICE_SERVICE)
  ) => {
    const writableStore = store as WritableStore<PlayerState>;
    return {
      ...initializePlayer(writableStore),
      ...launchFileWithContext(writableStore, playerService),
      ...launchRandomFile(writableStore, playerService),
      ...loadFileContext(writableStore),
      ...updateShuffleSettings(writableStore),
      ...updateLaunchMode(writableStore),
      ...updatePlayerStatus(writableStore),
      ...updateTimerState(writableStore),
      ...play(writableStore, playerService),
      ...pauseMusic(writableStore, playerService),
      ...stopPlayback(writableStore, deviceService),
      ...navigateNext(writableStore, playerService),
      ...navigatePrevious(writableStore, playerService),
      ...removePlayer(writableStore),
      ...recordHistory(writableStore),
      ...clearHistory(writableStore),
      ...navigateBackwardInHistory(writableStore, playerService),
      ...navigateForwardInHistory(writableStore, playerService),
      updateHistoryViewVisibility: (params: { deviceId: string; visible: boolean }) =>
        updateHistoryViewVisibility(writableStore, params),
      ...navigateToHistoryPosition(writableStore, playerService),
    };
  });
}
