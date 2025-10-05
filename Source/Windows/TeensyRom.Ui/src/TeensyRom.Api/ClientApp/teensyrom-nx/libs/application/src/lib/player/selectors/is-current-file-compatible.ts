import { computed } from '@angular/core';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';

export function isCurrentFileCompatible(store: WritableStore<PlayerState>) {
  return {
    isCurrentFileCompatible: (deviceId: string) =>
      computed<boolean>(() => {
        const currentFile = store.players()[deviceId]?.currentFile;
        if (!currentFile?.file) {
          return false;
        }
        return currentFile.file.isCompatible;
      }),
  };
}
