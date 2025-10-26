import { StorageType } from './storage-type.enum';

/**
 * Utility functions for working with StorageType enum values
 */
export const StorageTypeUtil = {
  /**
   * Convert StorageType enum value to its string representation
   * StorageType.Sd → 'SD'
   * StorageType.Usb → 'USB'
   */
  toString(storageType: StorageType): string {
    return storageType;
  },
} as const;
