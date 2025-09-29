import { inject } from '@angular/core';
import { withMethods } from '@ngrx/signals';
import { PLAYER_SERVICE, IPlayerService } from '@teensyrom-nx/domain';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';
import { initializePlayer } from './initialize-player';
import { launchFileWithContext } from './launch-file-with-context';
import { removePlayer } from './remove-player';

export function withPlayerActions() {
  return withMethods((store, playerService: IPlayerService = inject(PLAYER_SERVICE)) => {
    const writableStore = store as WritableStore<PlayerState>;
    return {
      ...initializePlayer(writableStore),
      ...launchFileWithContext(writableStore, playerService),
      ...removePlayer(writableStore),
    };
  });
}
