import { StorageType } from '@teensyrom-nx/domain';

// Storage key type for flat state structure
export type StorageKey = `${string}-${StorageType}`;

// Storage key utility functions
export const StorageKeyUtil = {
  create(deviceId: string, storageType: StorageType): StorageKey {
    return `${deviceId}-${storageType}` as StorageKey;
  },

  parse(key: StorageKey): { deviceId: string; storageType: StorageType } {
    const lastDashIndex = key.lastIndexOf('-');
    return {
      deviceId: key.substring(0, lastDashIndex),
      storageType: key.substring(lastDashIndex + 1) as StorageType,
    };
  },

  // Useful for filtering by device
  forDevice(deviceId: string): (key: StorageKey) => boolean {
    return (key) => key.startsWith(`${deviceId}-`);
  },

  // Useful for filtering by storage type
  forStorageType(storageType: StorageType): (key: StorageKey) => boolean {
    return (key) => key.endsWith(`-${storageType}`);
  },
} as const;
