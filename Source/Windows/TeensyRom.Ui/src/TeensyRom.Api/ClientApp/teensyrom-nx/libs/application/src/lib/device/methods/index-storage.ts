import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { IStorageService, DEVICE_STORAGE_SERVICE, StorageType } from '@teensyrom-nx/domain';
import { firstValueFrom } from 'rxjs';
import { DeviceState } from '../device-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function indexStorage(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  storageService: IStorageService = inject(DEVICE_STORAGE_SERVICE)
) {
  return {
    indexStorage: async (deviceId: string, storageType: StorageType, startingPath?: string) => {
      patchState(store, { isIndexing: true, error: null });
      try {
        await firstValueFrom(storageService.index(deviceId, storageType, startingPath));
        patchState(store, { isIndexing: false });
      } catch (error) {
        patchState(store, { isIndexing: false, error: String(error) });
      }
    },
  };
}
