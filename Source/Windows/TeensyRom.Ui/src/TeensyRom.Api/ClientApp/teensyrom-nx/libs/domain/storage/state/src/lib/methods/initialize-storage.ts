import { patchState, WritableStateSource } from '@ngrx/signals';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { StorageKeyUtil } from '../storage-key.util';
import { StorageState, StorageDirectoryState } from '../storage-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function initializeStorage(
  store: SignalStore<StorageState> & WritableStateSource<StorageState>
) {
  return {
    initializeStorage: ({
      deviceId,
      storageType,
    }: {
      deviceId: string;
      storageType: StorageType;
    }) => {
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Only initialize if not already present
      if (!store.storageEntries()[key]) {
        const initialEntry: StorageDirectoryState = {
          deviceId,
          storageType,
          currentPath: '/',
          directory: null,
          isLoaded: false,
          isLoading: false,
          error: null,
          lastLoadTime: null,
        };

        patchState(store, (state) => ({
          storageEntries: {
            ...state.storageEntries,
            [key]: initialEntry,
          },
        }));
      }
    },
  };
}
