import { computed } from '@angular/core';
import { StorageDirectory, StorageType } from '@teensyrom-nx/domain/storage/services';
import { StorageState } from '../storage-store';
import { getAllDeviceStorage, WritableStore } from '../storage-helpers';

export function getDeviceDirectories(store: WritableStore<StorageState>) {
  return {
    getDeviceDirectories: (deviceId: string) =>
      computed(() => {
        const entries = store.storageEntries();
        const deviceEntries = getAllDeviceStorage(entries, deviceId);
        const directories: Array<{
          key: string;
          deviceId: string;
          storageType: StorageType;
          currentPath: string;
          directories: StorageDirectory['directories'];
        }> = [];

        for (const [key, value] of Object.entries(deviceEntries)) {
          const v = value as StorageState['storageEntries'][string];
          if (v.directory) {
            directories.push({
              key,
              deviceId: v.deviceId,
              storageType: v.storageType,
              currentPath: v.currentPath,
              directories: v.directory.directories,
            });
          }
        }
        return directories;
      }),
  };
}
