import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { IDeviceService, DEVICE_SERVICE } from '@teensyrom-nx/domain';
import { DeviceState } from '../device-store';
import { firstValueFrom } from 'rxjs';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function findDevices(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  deviceService: IDeviceService = inject(DEVICE_SERVICE)
) {
  return {
    findDevices: async () => {
      patchState(store, { isLoading: true });

      try {
        const devices = await firstValueFrom(deviceService.findDevices(false));
        patchState(store, {
          devices,
          error: devices.length === 0 ? 'No devices found' : null,
          isLoading: false,
          hasInitialised: true,
        });
      } catch (error) {
        patchState(store, {
          devices: [], // Clear devices on error (e.g., when API returns 404)
          error: String(error),
          isLoading: false,
          hasInitialised: true,
        });
      }
    },
  };
}
