import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { IStorageService, DEVICE_STORAGE_SERVICE } from '@teensyrom-nx/domain';
import { firstValueFrom } from 'rxjs';
import { DeviceState } from '../device-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function indexAllStorage(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  storageService: IStorageService = inject(DEVICE_STORAGE_SERVICE)
) {
  return {
    indexStorageAllStorage: async () => {
      patchState(store, { isIndexing: true, error: null });
      try {
        await firstValueFrom(storageService.indexAll());
        patchState(store, { isIndexing: false });
      } catch (error) {
        patchState(store, { isIndexing: false, error: String(error) });
      }
    },
  };
}
