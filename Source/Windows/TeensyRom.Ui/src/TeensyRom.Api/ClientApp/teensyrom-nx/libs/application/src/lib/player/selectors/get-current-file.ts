import { computed } from '@angular/core';
import { PlayerState, LaunchedFile } from '../player-store';
import { WritableStore } from '../player-helpers';

export function getCurrentFile(store: WritableStore<PlayerState>) {
  return {
    getCurrentFile: (deviceId: string) =>
      computed<LaunchedFile | null>(() => {
        return store.players()[deviceId]?.currentFile ?? null;
      }),
  };
}
