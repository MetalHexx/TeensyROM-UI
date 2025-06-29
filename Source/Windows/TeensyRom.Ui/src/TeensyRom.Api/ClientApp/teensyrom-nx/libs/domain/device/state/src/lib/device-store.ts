import { inject } from '@angular/core';
import { signalStore, withMethods, withState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { Device, DeviceService, StorageService } from '@teensyrom-nx/domain/device/services';
import { findDevices, connectDevice, disconnectDevice } from './methods/index';
import { indexStorage } from './methods/index-storage';
import { indexAllStorage } from './methods/index-all-storage';
import { firstValueFrom } from 'rxjs';
import { resetAllDevices } from './methods/reset-all-devices';

export type DeviceState = {
  devices: Device[];
  hasInitialised: boolean;
  isLoading: boolean;
  isIndexing: boolean;
  error: string | null;
};

const initialState: DeviceState = {
  devices: [],
  hasInitialised: false,
  isLoading: true,
  isIndexing: false,
  error: null,
};

export const DeviceStore = signalStore(
  { providedIn: 'root' },
  withDevtools('devices'),
  withState(initialState),
  withMethods(
    (
      store,
      deviceService: DeviceService = inject(DeviceService),
      storageService: StorageService = inject(StorageService)
    ) => ({
      ...findDevices(store, deviceService),
      ...connectDevice(store, deviceService),
      ...disconnectDevice(store, deviceService),
      ...indexStorage(store, storageService),
      ...indexAllStorage(store, storageService),
      ...resetAllDevices(store, deviceService),
    })
  )
);
