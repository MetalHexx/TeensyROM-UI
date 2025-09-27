import { IDeviceService, DEVICE_SERVICE } from '@teensyrom-nx/domain';
import { DeviceState } from '../device-store';
import { firstValueFrom } from 'rxjs';
import { inject } from '@angular/core';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function resetAllDevices(
  store: SignalStore<DeviceState>,
  deviceService: IDeviceService = inject(DEVICE_SERVICE)
) {
  return {
    resetAllDevices: async () => {
      const devices = store.devices();

      await Promise.all(
        devices.map((device) => firstValueFrom(deviceService.resetDevice(device.deviceId)))
      );
    },
  };
}
