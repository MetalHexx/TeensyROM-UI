import { TeensyStorageType } from '@teensyrom-nx/data-access/api-client';

export interface DeviceStorage {
  deviceId: string;
  type: TeensyStorageType;
  available: boolean;
}