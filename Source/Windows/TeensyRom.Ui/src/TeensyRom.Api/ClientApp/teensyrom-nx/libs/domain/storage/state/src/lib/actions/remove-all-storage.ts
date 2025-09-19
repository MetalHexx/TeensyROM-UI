import { patchState } from '@ngrx/signals';
import { StorageState } from '../storage-store';
import { StorageKeyUtil, type StorageKey } from '../storage-key.util';
import { WritableStore } from '../storage-helpers';
import { LogType, logInfo } from '@teensyrom-nx/utils';

export function removeAllStorage(store: WritableStore<StorageState>) {
  return {
    removeAllStorage: ({ deviceId }: { deviceId: string }) => {
      logInfo(LogType.Cleanup, `Cleaning up all storage entries for device: ${deviceId}`);

      patchState(store, (state) => {
        const updatedSelectedDirectories = { ...state.selectedDirectories };
        const updatedEntries = { ...state.storageEntries };

        Object.keys(updatedEntries).forEach((key) => {
          if (StorageKeyUtil.forDevice(deviceId)(key as StorageKey)) {
            logInfo(LogType.Start, `Removing storage entry for ${key}`);
            delete updatedEntries[key];
          }
        });
        delete updatedSelectedDirectories[deviceId];

        return {
          storageEntries: updatedEntries,
          selectedDirectories: updatedSelectedDirectories,
        };
      });
    },
  };
}
