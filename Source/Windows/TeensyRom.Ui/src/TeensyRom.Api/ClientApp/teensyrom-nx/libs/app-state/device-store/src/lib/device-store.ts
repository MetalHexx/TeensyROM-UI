import { signalStore, withState } from '@ngrx/signals';
import { Device } from '@teensyrom-nx/device';

type DeviceState = {
  devices: Device[];
  isLoading: boolean;
  error: string | null;
};

const initialState: DeviceState = {
  devices: [
    {
      deviceId: '123',
      name: 'Test Device',
      fwVersion: '1.0.0',
      isCompatible: true,
      sdStorage: {
        deviceId: '123',
        type: 1,
        available: true,
      },
      usbStorage: {
        deviceId: '123',
        type: 0,
        available: true,
      },
      comPort: 'COM1',
    },
  ],
  isLoading: false,
  error: null,
};

export const deviceStore = signalStore(withState(initialState));
