import { StorageType } from './storage-type.enum';

export interface DeviceStorage {
  deviceId: string;
  type: StorageType;
  available: boolean;
}