vi.mock('@teensyrom-nx/domain/storage/services', () => {
  return {
    StorageType: { Sd: 'SD', Usb: 'USB' },
  };
});

import { describe, it, expect } from 'vitest';
import { StorageKeyUtil, StorageKey } from './storage-key.util';
import { StorageType } from '@teensyrom-nx/domain/storage/services';

describe('StorageKeyUtil', () => {
  describe('create', () => {
    it('should create storage key from deviceId and storageType', () => {
      const deviceId = 'device123';
      const storageType = StorageType.Sd;

      const result = StorageKeyUtil.create(deviceId, storageType);

      expect(result).toBe('device123-SD');
      expect(typeof result).toBe('string');
    });

    it('should create different keys for different storage types', () => {
      const deviceId = 'device123';

      const sdKey = StorageKeyUtil.create(deviceId, StorageType.Sd);
      const usbKey = StorageKeyUtil.create(deviceId, StorageType.Usb);

      expect(sdKey).toBe('device123-SD');
      expect(usbKey).toBe('device123-USB');
      expect(sdKey).not.toBe(usbKey);
    });

    it('should create different keys for different device IDs', () => {
      const storageType = StorageType.Sd;

      const key1 = StorageKeyUtil.create('device1', storageType);
      const key2 = StorageKeyUtil.create('device2', storageType);

      expect(key1).toBe('device1-SD');
      expect(key2).toBe('device2-SD');
      expect(key1).not.toBe(key2);
    });

    it('should handle device IDs with special characters', () => {
      const deviceId = 'device-123_test.unit';
      const storageType = StorageType.Usb;

      const result = StorageKeyUtil.create(deviceId, storageType);

      expect(result).toBe('device-123_test.unit-USB');
    });

    it('should handle device IDs with existing dashes', () => {
      const deviceId = 'multi-dash-device-id';
      const storageType = StorageType.Sd;

      const result = StorageKeyUtil.create(deviceId, storageType);

      expect(result).toBe('multi-dash-device-id-SD');
    });
  });

  describe('parse', () => {
    it('should parse storage key back to deviceId and storageType', () => {
      const key: StorageKey = 'device123-SD';

      const result = StorageKeyUtil.parse(key);

      expect(result.deviceId).toBe('device123');
      expect(result.storageType).toBe(StorageType.Sd);
    });

    it('should parse keys with different storage types', () => {
      const sdKey: StorageKey = 'device123-SD';
      const usbKey: StorageKey = 'device123-USB';

      const sdResult = StorageKeyUtil.parse(sdKey);
      const usbResult = StorageKeyUtil.parse(usbKey);

      expect(sdResult.deviceId).toBe('device123');
      expect(sdResult.storageType).toBe(StorageType.Sd);
      expect(usbResult.deviceId).toBe('device123');
      expect(usbResult.storageType).toBe(StorageType.Usb);
    });

    it('should handle device IDs with dashes correctly', () => {
      const key: StorageKey = 'multi-dash-device-id-USB';

      const result = StorageKeyUtil.parse(key);

      expect(result.deviceId).toBe('multi-dash-device-id');
      expect(result.storageType).toBe(StorageType.Usb);
    });

    it('should handle complex device IDs', () => {
      const key: StorageKey = 'device-123_test.unit-SD';

      const result = StorageKeyUtil.parse(key);

      expect(result.deviceId).toBe('device-123_test.unit');
      expect(result.storageType).toBe(StorageType.Sd);
    });
  });

  describe('create and parse roundtrip', () => {
    it('should maintain data integrity in create -> parse roundtrip', () => {
      const originalDeviceId = 'device123';
      const originalStorageType = StorageType.Sd;

      const key = StorageKeyUtil.create(originalDeviceId, originalStorageType);
      const parsed = StorageKeyUtil.parse(key);

      expect(parsed.deviceId).toBe(originalDeviceId);
      expect(parsed.storageType).toBe(originalStorageType);
    });

    it('should maintain data integrity with complex device IDs', () => {
      const originalDeviceId = 'complex-device-123_test.unit';
      const originalStorageType = StorageType.Usb;

      const key = StorageKeyUtil.create(originalDeviceId, originalStorageType);
      const parsed = StorageKeyUtil.parse(key);

      expect(parsed.deviceId).toBe(originalDeviceId);
      expect(parsed.storageType).toBe(originalStorageType);
    });

    it('should work with all storage types', () => {
      const deviceId = 'testDevice';
      const storageTypes = [StorageType.Sd, StorageType.Usb];

      storageTypes.forEach((storageType) => {
        const key = StorageKeyUtil.create(deviceId, storageType);
        const parsed = StorageKeyUtil.parse(key);

        expect(parsed.deviceId).toBe(deviceId);
        expect(parsed.storageType).toBe(storageType);
      });
    });
  });

  describe('forDevice', () => {
    it('should return filter function for specific device', () => {
      const deviceId = 'device123';
      const filter = StorageKeyUtil.forDevice(deviceId);

      expect(typeof filter).toBe('function');
    });

    it('should filter keys for specific device', () => {
      const targetDeviceId = 'device123';
      const filter = StorageKeyUtil.forDevice(targetDeviceId);

      const keys: StorageKey[] = ['device123-SD', 'device123-USB', 'device456-SD', 'device789-USB'];

      const filtered = keys.filter(filter);

      expect(filtered).toEqual(['device123-SD', 'device123-USB']);
      expect(filtered).toHaveLength(2);
    });

    it('should handle device IDs with dashes in filtering', () => {
      const targetDeviceId = 'multi-dash-device';
      const filter = StorageKeyUtil.forDevice(targetDeviceId);

      const keys: StorageKey[] = [
        'multi-dash-device-SD',
        'multi-dash-device-USB',
        'other-device-SD',
        'multi-dash-SD', // This should not match
      ];

      const filtered = keys.filter(filter);

      expect(filtered).toEqual(['multi-dash-device-SD', 'multi-dash-device-USB']);
      expect(filtered).toHaveLength(2);
    });

    it('should return empty array when no keys match device', () => {
      const targetDeviceId = 'nonexistent';
      const filter = StorageKeyUtil.forDevice(targetDeviceId);

      const keys: StorageKey[] = ['device123-SD', 'device456-USB'];
      const filtered = keys.filter(filter);

      expect(filtered).toEqual([]);
      expect(filtered).toHaveLength(0);
    });
  });

  describe('forStorageType', () => {
    it('should return filter function for specific storage type', () => {
      const storageType = StorageType.Sd;
      const filter = StorageKeyUtil.forStorageType(storageType);

      expect(typeof filter).toBe('function');
    });

    it('should filter keys for specific storage type', () => {
      const targetStorageType = StorageType.Sd;
      const filter = StorageKeyUtil.forStorageType(targetStorageType);

      const keys: StorageKey[] = ['device123-SD', 'device456-SD', 'device123-USB', 'device789-USB'];

      const filtered = keys.filter(filter);

      expect(filtered).toEqual(['device123-SD', 'device456-SD']);
      expect(filtered).toHaveLength(2);
    });

    it('should filter correctly for different storage types', () => {
      const usbFilter = StorageKeyUtil.forStorageType(StorageType.Usb);
      const sdFilter = StorageKeyUtil.forStorageType(StorageType.Sd);

      const keys: StorageKey[] = ['device1-SD', 'device1-USB', 'device2-SD', 'device2-USB'];

      const usbFiltered = keys.filter(usbFilter);
      const sdFiltered = keys.filter(sdFilter);

      expect(usbFiltered).toEqual(['device1-USB', 'device2-USB']);
      expect(sdFiltered).toEqual(['device1-SD', 'device2-SD']);
    });

    it('should return empty array when no keys match storage type', () => {
      const filter = StorageKeyUtil.forStorageType(StorageType.Sd);

      const keys: StorageKey[] = ['device123-USB', 'device456-USB'];
      const filtered = keys.filter(filter);

      expect(filtered).toEqual([]);
      expect(filtered).toHaveLength(0);
    });
  });

  describe('combined filtering', () => {
    it('should work with combined device and storage type filters', () => {
      const keys: StorageKey[] = [
        'device1-SD',
        'device1-USB',
        'device2-SD',
        'device2-USB',
        'device3-USB',
      ];

      const deviceFilter = StorageKeyUtil.forDevice('device1');
      const storageFilter = StorageKeyUtil.forStorageType(StorageType.Usb);

      const combined = keys.filter((key) => deviceFilter(key) && storageFilter(key));

      expect(combined).toEqual(['device1-USB']);
      expect(combined).toHaveLength(1);
    });
  });
});
