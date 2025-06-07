import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { DeviceService } from '@teensyrom-nx/domain/device/services';
import { DeviceState } from '../device-store';
import { firstValueFrom } from 'rxjs';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function findDevices(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  deviceService: DeviceService = inject(DeviceService)
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
          error: String(error),
          isLoading: false,
          hasInitialised: true,
        });
      }
    },
  };
}
