import { computed } from '@angular/core';
import { StorageState, SearchState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import { WritableStore } from '../storage-helpers';
import { StorageType } from '@teensyrom-nx/domain';

export function getSearchState(store: WritableStore<StorageState>) {
  return {
    getSearchState: (deviceId: string, storageType: StorageType) =>
      computed<SearchState | null>(() => {
        const key = StorageKeyUtil.create(deviceId, storageType);
        return store.searchState()[key] ?? null;
      }),
  };
}
