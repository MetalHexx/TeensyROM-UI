import { inject } from '@angular/core';
import { withMethods } from '@ngrx/signals';
import { PLAYER_SERVICE, IPlayerService } from '@teensyrom-nx/domain';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';
import { initializePlayer } from './initialize-player';
import { launchFileWithContext } from './launch-file-with-context';
import { launchRandomFile } from './launch-random-file';
import { loadFileContext } from './load-file-context';
import { updateShuffleSettings } from './update-shuffle-settings';
import { updateLaunchMode } from './update-launch-mode';
import { removePlayer } from './remove-player';

export function withPlayerActions() {
  return withMethods((store, playerService: IPlayerService = inject(PLAYER_SERVICE)) => {
    const writableStore = store as WritableStore<PlayerState>;
    return {
      ...initializePlayer(writableStore),
      ...launchFileWithContext(writableStore, playerService),
      ...launchRandomFile(writableStore, playerService),
      ...loadFileContext(writableStore),
      ...updateShuffleSettings(writableStore),
      ...updateLaunchMode(writableStore),
      ...removePlayer(writableStore),
    };
  });
}
