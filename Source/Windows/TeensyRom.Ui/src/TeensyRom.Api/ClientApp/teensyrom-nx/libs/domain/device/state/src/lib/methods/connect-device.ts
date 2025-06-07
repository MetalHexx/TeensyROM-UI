import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { DeviceService } from '@teensyrom-nx/domain/device/services';
import { firstValueFrom } from 'rxjs';
import { DeviceState } from '../device-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function connectDevice(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  deviceService: DeviceService = inject(DeviceService)
) {
  return {
    connectDevice: async (deviceId: string) => {
      patchState(store, { error: null });

      try {
        await firstValueFrom(deviceService.connectDevice(deviceId));

        patchState(store, {
          isLoading: false,
          devices: store
            .devices()
            .map((d) => (d.deviceId === deviceId ? { ...d, isConnected: true } : d)),
        });
      } catch (error) {
        patchState(store, { error: String(error) });
      }
    },
  };
}
