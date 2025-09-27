import { inject } from '@angular/core';
import { signalStore, withMethods, withState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import {
  Device,
  DEVICE_SERVICE,
  IDeviceService,
  DEVICE_STORAGE_SERVICE,
  IStorageService,
} from '@teensyrom-nx/domain';
import { findDevices, connectDevice, disconnectDevice } from './methods/index';
import { indexStorage } from './methods/index-storage';
import { indexAllStorage } from './methods/index-all-storage';
import { resetAllDevices } from './methods/reset-all-devices';
import { pingAllDevices } from './methods/ping-devices';

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
      deviceService: IDeviceService = inject(DEVICE_SERVICE),
      storageService: IStorageService = inject(DEVICE_STORAGE_SERVICE)
    ) => ({
      ...findDevices(store, deviceService),
      ...connectDevice(store, deviceService),
      ...disconnectDevice(store, deviceService),
      ...indexStorage(store, storageService),
      ...indexAllStorage(store, storageService),
      ...resetAllDevices(store, deviceService),
      ...pingAllDevices(store, deviceService),
    })
  )
);
