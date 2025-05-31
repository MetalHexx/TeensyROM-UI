import { inject } from '@angular/core';
import { signalStore, withMethods, withState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { Device, DeviceService } from '@teensyrom-nx/domain/device/services';
import { findDevices, connectDevice, disconnectDevice } from './methods/index';

export type DeviceState = {
  availableDevices: Device[];
  connectedDevices: Device[];
  hasInitialised: boolean;
  isLoading: boolean;
  error: string | null;
};

const initialState: DeviceState = {
  availableDevices: [],
  connectedDevices: [],
  hasInitialised: false,
  isLoading: false,
  error: null,
};

export const DeviceStore = signalStore(
  { providedIn: 'root' },
  withDevtools('devices'),
  withState(initialState),
  withMethods((store, deviceService: DeviceService = inject(DeviceService)) => ({
    ...findDevices(store, deviceService),
    ...connectDevice(store, deviceService),
    ...disconnectDevice(store, deviceService),
  }))
);
