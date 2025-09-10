import { patchState, WritableStateSource } from '@ngrx/signals';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { StorageKeyUtil, type StorageKey } from '../storage-key.util';
import { StorageState } from '../storage-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function cleanupStorage(
  store: SignalStore<StorageState> & WritableStateSource<StorageState>
) {
  return {
    cleanupStorage: ({ deviceId }: { deviceId: string }) => {
      patchState(store, (state) => {
        const updatedEntries = { ...state.storageEntries };

        // Remove all entries for the specified device
        Object.keys(updatedEntries).forEach((key) => {
          if (StorageKeyUtil.forDevice(deviceId)(key as StorageKey)) {
            delete updatedEntries[key];
          }
        });

        return { storageEntries: updatedEntries };
      });
    },

    cleanupStorageType: ({
      deviceId,
      storageType,
    }: {
      deviceId: string;
      storageType: StorageType;
    }) => {
      const key = StorageKeyUtil.create(deviceId, storageType);

      patchState(store, (state) => {
        const updatedEntries = { ...state.storageEntries };
        delete updatedEntries[key];
        return { storageEntries: updatedEntries };
      });
    },
  };
}
