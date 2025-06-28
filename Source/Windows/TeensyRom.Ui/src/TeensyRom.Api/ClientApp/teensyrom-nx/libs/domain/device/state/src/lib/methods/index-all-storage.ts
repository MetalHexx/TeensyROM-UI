import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { StorageService } from '@teensyrom-nx/domain/device/services';
import { firstValueFrom } from 'rxjs';
import { DeviceState } from '../device-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function indexAllStorage(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  storageService: StorageService = inject(StorageService)
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
