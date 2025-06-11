import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { StorageService } from '@teensyrom-nx/domain/device/services';
import { firstValueFrom } from 'rxjs';
import { DeviceState } from '../device-store';
import { TeensyStorageType } from '@teensyrom-nx/data-access/api-client';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function indexStorage(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  storageService: StorageService = inject(StorageService)
) {
  return {
    indexStorage: async (
      deviceId: string,
      storageType: TeensyStorageType,
      startingPath?: string
    ) => {
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
