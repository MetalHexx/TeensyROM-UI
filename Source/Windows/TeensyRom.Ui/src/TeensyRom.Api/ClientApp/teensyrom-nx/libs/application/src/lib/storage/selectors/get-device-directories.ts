import { computed } from '@angular/core';
import { StorageState, StorageDirectoryState } from '../storage-store';
import { getAllDeviceStorage, WritableStore } from '../storage-helpers';

export function getDeviceDirectories(store: WritableStore<StorageState>) {
  return {
    getDeviceDirectories: (deviceId: string) =>
      computed(() => {
        const entries = store.storageEntries();
        const deviceEntries = getAllDeviceStorage(entries, deviceId);
        const directories: StorageDirectoryState[] = [];

        for (const [, value] of Object.entries(deviceEntries)) {
          const v = value as StorageState['storageEntries'][string];
          if (v.directory) {
            directories.push(v);
          }
        }
        return directories;
      }),
  };
}
