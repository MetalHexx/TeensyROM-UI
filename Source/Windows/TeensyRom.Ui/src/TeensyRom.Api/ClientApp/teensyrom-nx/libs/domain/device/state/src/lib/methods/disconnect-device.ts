import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { DeviceService, Device } from '@teensyrom-nx/domain/device/services';
import { firstValueFrom } from 'rxjs';
import { DeviceState } from '../device-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function disconnectDevice(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  deviceService: DeviceService = inject(DeviceService)
) {
  return {
    disconnectDevice: async (deviceId: string) => {
      try {
        await firstValueFrom(deviceService.disconnectDevice(deviceId));

        patchState(store, {
          devices: store
            .devices()
            .map((d) => (d.deviceId === deviceId ? { ...d, isConnected: false } : d)),
        });
      } catch (error) {
        patchState(store, { error: String(error) });
      }
    },
  };
}
