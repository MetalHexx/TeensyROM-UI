import { DeviceState } from '@teensyrom-nx/data-access/api-client';
import { DeviceStorage } from './device-storage.model';

export interface Device {
  deviceId: string;
  comPort: string;
  name: string;
  fwVersion: string;
  isCompatible: boolean;
  isConnected: boolean;
  deviceState: DeviceState;
  sdStorage: DeviceStorage;
  usbStorage: DeviceStorage;
}