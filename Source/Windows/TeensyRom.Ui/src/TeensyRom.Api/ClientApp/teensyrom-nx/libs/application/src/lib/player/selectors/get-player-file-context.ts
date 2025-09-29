import { computed } from '@angular/core';
import { PlayerState, PlayerFileContext } from '../player-store';
import { WritableStore } from '../player-helpers';

export function getPlayerFileContext(store: WritableStore<PlayerState>) {
  return {
    getPlayerFileContext: (deviceId: string) =>
      computed<PlayerFileContext | null>(() => {
        return store.players()[deviceId]?.fileContext ?? null;
      }),
  };
}
