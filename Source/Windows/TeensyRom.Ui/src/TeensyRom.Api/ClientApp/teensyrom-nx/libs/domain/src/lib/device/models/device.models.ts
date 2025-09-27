import { DeviceState, TeensyStorageType } from '@teensyrom-nx/data-access/api-client';

export interface DeviceStorage {
  deviceId: string;
  type: TeensyStorageType;
  available: boolean;
}

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
