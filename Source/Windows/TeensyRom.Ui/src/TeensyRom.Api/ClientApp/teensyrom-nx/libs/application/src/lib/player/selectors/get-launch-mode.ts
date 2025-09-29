import { computed } from '@angular/core';
import { WritableStore } from '../player-helpers';
import { PlayerState } from '../player-store';
import { LaunchMode } from '@teensyrom-nx/domain';

export function getLaunchMode(store: WritableStore<PlayerState>) {
  return {
    getLaunchMode: (deviceId: string) =>
      computed(() => {
        const player = store.players()[deviceId];
        return player?.launchMode ?? LaunchMode.Directory;
      }),
  };
}