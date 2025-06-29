import { inject } from '@angular/core';
import { DeviceService } from '@teensyrom-nx/domain/device/services';
import { DeviceState } from '../device-store';
import { firstValueFrom } from 'rxjs';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function pingAllDevices(
  store: SignalStore<DeviceState>,
  deviceService: DeviceService = inject(DeviceService)
) {
  return {
    pingAllDevices: async () => {
      const devices = store.devices();
      await Promise.all(
        devices.map((device) => firstValueFrom(deviceService.pingDevice(device.deviceId)))
      );
    },
  };
}
