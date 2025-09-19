import { computed } from '@angular/core';
import { StorageState, StorageDirectoryState } from '../storage-store';
import { getAllDeviceStorage, WritableStore } from '../storage-helpers';

export function getDeviceStorageEntries(store: WritableStore<StorageState>) {
  return {
    getDeviceStorageEntries: (deviceId: string) =>
      computed<Record<string, StorageDirectoryState>>(() => {
        const entries = store.storageEntries();
        return getAllDeviceStorage(entries, deviceId);
      }),
  };
}
