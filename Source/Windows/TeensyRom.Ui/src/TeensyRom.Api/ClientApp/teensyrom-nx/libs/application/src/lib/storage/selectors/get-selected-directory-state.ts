import { computed } from '@angular/core';
import { StorageState, StorageDirectoryState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import { WritableStore } from '../storage-helpers';

export function getSelectedDirectoryState(store: WritableStore<StorageState>) {
  return {
    getSelectedDirectoryState: (deviceId: string) =>
      computed<StorageDirectoryState | null>(() => {
        const selected = store.selectedDirectories()[deviceId];
        if (!selected) return null;
        const key = StorageKeyUtil.create(selected.deviceId, selected.storageType);
        return store.storageEntries()[key] ?? null;
      }),
  };
}
