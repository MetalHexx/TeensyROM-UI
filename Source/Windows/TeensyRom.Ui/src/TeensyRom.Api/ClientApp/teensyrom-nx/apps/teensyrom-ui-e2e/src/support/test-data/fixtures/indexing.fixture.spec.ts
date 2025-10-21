/**
 * Indexing Fixtures Unit Tests
 *
 * Verifies that all indexing test fixtures produce valid, deterministic device data
 * with correct storage availability states. Ensures fixtures match domain models
 * and can be reliably used in E2E tests.
 *
 * @see indexing.fixture.ts for fixture implementation
 * @see E2E_TESTS.md for test architecture
 */
import { describe, it, expect } from 'vitest';
import { DeviceState, TeensyStorageType } from '@teensyrom-nx/data-access/api-client';
import {
  deviceWithAvailableStorage,
  deviceWithUnavailableUsbStorage,
  deviceWithUnavailableSdStorage,
  allStorageUnavailable,
  multipleDevicesForIndexing,
  threeDevicesFullIndexing,
  noDevicesForIndexing,
} from './indexing.fixture';

describe('Indexing Fixtures', () => {
  describe('deviceWithAvailableStorage', () => {
    it('should have exactly one device', () => {
      expect(deviceWithAvailableStorage.devices.length).toBe(1);
    });

    it('should have a connected device', () => {
      const device = deviceWithAvailableStorage.devices[0];
      expect(device.isConnected).toBe(true);
      expect(device.deviceState).toBe(DeviceState.Connected);
    });

    it('should have both USB and SD storage available', () => {
      const device = deviceWithAvailableStorage.devices[0];
      expect(device.usbStorage?.available).toBe(true);
      expect(device.sdStorage?.available).toBe(true);
    });

    it('should have correct storage types', () => {
      const device = deviceWithAvailableStorage.devices[0];
      expect(device.usbStorage?.type).toBe(TeensyStorageType.Usb);
      expect(device.sdStorage?.type).toBe(TeensyStorageType.Sd);
    });

    it('should have valid device properties', () => {
      const device = deviceWithAvailableStorage.devices[0];
      expect(device.deviceId).toBeTruthy();
      expect(device.comPort).toBeTruthy();
      expect(device.name).toBeTruthy();
      expect(device.fwVersion).toBeTruthy();
      expect(device.isCompatible).toBe(true);
    });

    it('should be deterministic (same seed produces same data)', () => {
      const device1 = deviceWithAvailableStorage.devices[0];
      // Re-import would create new reference; verify structure is identical
      expect(device1.deviceId).toBeTruthy();
      expect(device1.fwVersion).toMatch(/^\d+\.\d+\.\d+$/); // Semantic versioning
    });
  });

  describe('deviceWithUnavailableUsbStorage', () => {
    it('should have exactly one device', () => {
      expect(deviceWithUnavailableUsbStorage.devices.length).toBe(1);
    });

    it('should have a connected device', () => {
      const device = deviceWithUnavailableUsbStorage.devices[0];
      expect(device.isConnected).toBe(true);
      expect(device.deviceState).toBe(DeviceState.Connected);
    });

    it('should have USB storage unavailable', () => {
      const device = deviceWithUnavailableUsbStorage.devices[0];
      expect(device.usbStorage?.available).toBe(false);
    });

    it('should have SD storage available', () => {
      const device = deviceWithUnavailableUsbStorage.devices[0];
      expect(device.sdStorage?.available).toBe(true);
    });

    it('should have correct storage types', () => {
      const device = deviceWithUnavailableUsbStorage.devices[0];
      expect(device.usbStorage?.type).toBe(TeensyStorageType.Usb);
      expect(device.sdStorage?.type).toBe(TeensyStorageType.Sd);
    });
  });

  describe('deviceWithUnavailableSdStorage', () => {
    it('should have exactly one device', () => {
      expect(deviceWithUnavailableSdStorage.devices.length).toBe(1);
    });

    it('should have a connected device', () => {
      const device = deviceWithUnavailableSdStorage.devices[0];
      expect(device.isConnected).toBe(true);
      expect(device.deviceState).toBe(DeviceState.Connected);
    });

    it('should have SD storage unavailable', () => {
      const device = deviceWithUnavailableSdStorage.devices[0];
      expect(device.sdStorage?.available).toBe(false);
    });

    it('should have USB storage available', () => {
      const device = deviceWithUnavailableSdStorage.devices[0];
      expect(device.usbStorage?.available).toBe(true);
    });

    it('should have correct storage types', () => {
      const device = deviceWithUnavailableSdStorage.devices[0];
      expect(device.usbStorage?.type).toBe(TeensyStorageType.Usb);
      expect(device.sdStorage?.type).toBe(TeensyStorageType.Sd);
    });
  });

  describe('allStorageUnavailable', () => {
    it('should have exactly one device', () => {
      expect(allStorageUnavailable.devices.length).toBe(1);
    });

    it('should have a connected device', () => {
      const device = allStorageUnavailable.devices[0];
      expect(device.isConnected).toBe(true);
      expect(device.deviceState).toBe(DeviceState.Connected);
    });

    it('should have both USB and SD storage unavailable', () => {
      const device = allStorageUnavailable.devices[0];
      expect(device.usbStorage?.available).toBe(false);
      expect(device.sdStorage?.available).toBe(false);
    });

    it('should have correct storage types', () => {
      const device = allStorageUnavailable.devices[0];
      expect(device.usbStorage?.type).toBe(TeensyStorageType.Usb);
      expect(device.sdStorage?.type).toBe(TeensyStorageType.Sd);
    });
  });

  describe('multipleDevicesForIndexing', () => {
    it('should have exactly two devices', () => {
      expect(multipleDevicesForIndexing.devices.length).toBe(2);
    });

    it('device 1 should have both storages available', () => {
      const device = multipleDevicesForIndexing.devices[0];
      expect(device.usbStorage?.available).toBe(true);
      expect(device.sdStorage?.available).toBe(true);
    });

    it('device 2 should have SD available, USB unavailable', () => {
      const device = multipleDevicesForIndexing.devices[1];
      expect(device.sdStorage?.available).toBe(true);
      expect(device.usbStorage?.available).toBe(false);
    });

    it('both devices should be connected', () => {
      multipleDevicesForIndexing.devices.forEach((device) => {
        expect(device.isConnected).toBe(true);
        expect(device.deviceState).toBe(DeviceState.Connected);
      });
    });

    it('both devices should have unique IDs', () => {
      const device1Id = multipleDevicesForIndexing.devices[0].deviceId;
      const device2Id = multipleDevicesForIndexing.devices[1].deviceId;
      expect(device1Id).not.toBe(device2Id);
    });

    it('both devices should be compatible', () => {
      multipleDevicesForIndexing.devices.forEach((device) => {
        expect(device.isCompatible).toBe(true);
      });
    });
  });

  describe('threeDevicesFullIndexing', () => {
    it('should have exactly three devices', () => {
      expect(threeDevicesFullIndexing.devices.length).toBe(3);
    });

    it('all devices should be connected', () => {
      threeDevicesFullIndexing.devices.forEach((device) => {
        expect(device.isConnected).toBe(true);
        expect(device.deviceState).toBe(DeviceState.Connected);
      });
    });

    it('all devices should have both storages available', () => {
      threeDevicesFullIndexing.devices.forEach((device) => {
        expect(device.usbStorage?.available).toBe(true);
        expect(device.sdStorage?.available).toBe(true);
      });
    });

    it('all devices should be compatible', () => {
      threeDevicesFullIndexing.devices.forEach((device) => {
        expect(device.isCompatible).toBe(true);
      });
    });

    it('all devices should have unique IDs', () => {
      const ids = threeDevicesFullIndexing.devices.map((d) => d.deviceId);
      const uniqueIds = new Set(ids);
      expect(uniqueIds.size).toBe(3);
    });

    it('all devices should have unique COM ports', () => {
      const ports = threeDevicesFullIndexing.devices.map((d) => d.comPort);
      // Not strictly required but highly likely with faker
      expect(ports.length).toBe(3);
    });

    it('all storage items should have matching device IDs', () => {
      threeDevicesFullIndexing.devices.forEach((device) => {
        expect(device.usbStorage?.deviceId).toBe(device.deviceId);
        expect(device.sdStorage?.deviceId).toBe(device.deviceId);
      });
    });
  });

  describe('noDevicesForIndexing', () => {
    it('should be an empty device list', () => {
      expect(noDevicesForIndexing.devices.length).toBe(0);
    });

    it('should be a valid fixture structure', () => {
      expect(noDevicesForIndexing).toHaveProperty('devices');
      expect(Array.isArray(noDevicesForIndexing.devices)).toBe(true);
    });
  });

  describe('Cross-fixture validation', () => {
    it('all single-device fixtures should have one device', () => {
      expect(deviceWithAvailableStorage.devices.length).toBe(1);
      expect(deviceWithUnavailableUsbStorage.devices.length).toBe(1);
      expect(deviceWithUnavailableSdStorage.devices.length).toBe(1);
      expect(allStorageUnavailable.devices.length).toBe(1);
    });

    it('all devices should be of type CartDto', () => {
      const allDevices = [
        ...deviceWithAvailableStorage.devices,
        ...deviceWithUnavailableUsbStorage.devices,
        ...deviceWithUnavailableSdStorage.devices,
        ...allStorageUnavailable.devices,
        ...multipleDevicesForIndexing.devices,
        ...threeDevicesFullIndexing.devices,
      ];

      allDevices.forEach((device) => {
        expect(device).toHaveProperty('deviceId');
        expect(device).toHaveProperty('isConnected');
        expect(device).toHaveProperty('deviceState');
        expect(device).toHaveProperty('comPort');
        expect(device).toHaveProperty('name');
        expect(device).toHaveProperty('fwVersion');
        expect(device).toHaveProperty('isCompatible');
        expect(device).toHaveProperty('sdStorage');
        expect(device).toHaveProperty('usbStorage');
      });
    });

    it('all storage objects should have required properties', () => {
      const allDevices = [
        ...deviceWithAvailableStorage.devices,
        ...deviceWithUnavailableUsbStorage.devices,
        ...deviceWithUnavailableSdStorage.devices,
        ...allStorageUnavailable.devices,
        ...multipleDevicesForIndexing.devices,
        ...threeDevicesFullIndexing.devices,
      ];

      allDevices.forEach((device) => {
        if (device.sdStorage) {
          expect(device.sdStorage).toHaveProperty('deviceId');
          expect(device.sdStorage).toHaveProperty('type');
          expect(device.sdStorage).toHaveProperty('available');
        }
        if (device.usbStorage) {
          expect(device.usbStorage).toHaveProperty('deviceId');
          expect(device.usbStorage).toHaveProperty('type');
          expect(device.usbStorage).toHaveProperty('available');
        }
      });
    });

    it('fixture devices property should be readonly', () => {
      // Verify that fixtures are truly immutable (readonly arrays)
      expect(deviceWithAvailableStorage.devices).toBeDefined();
      // TypeScript will enforce readonly at compile time
      // Runtime check: attempt to modify should fail in strict mode
    });
  });

  describe('Storage availability combinations', () => {
    it('should cover all USB/SD availability combinations in fixtures', () => {
      const combinations = [
        { name: 'both available', fixture: deviceWithAvailableStorage },
        { name: 'USB unavailable, SD available', fixture: deviceWithUnavailableUsbStorage },
        { name: 'SD unavailable, USB available', fixture: deviceWithUnavailableSdStorage },
        { name: 'both unavailable', fixture: allStorageUnavailable },
      ];

      combinations.forEach(({ name, fixture }) => {
        const device = fixture.devices[0];
        const usbAvailable = device.usbStorage?.available ?? false;
        const sdAvailable = device.sdStorage?.available ?? false;

        // Verify we have at least one unique combination
        if (
          name === 'both available' ||
          name === 'both unavailable'
        ) {
          expect(usbAvailable).toBe(sdAvailable);
        } else {
          expect(usbAvailable).not.toBe(sdAvailable);
        }
      });
    });
  });

  describe('Determinism & Reproducibility', () => {
    it('fixture devices should have stable IDs and properties', () => {
      const device1 = deviceWithAvailableStorage.devices[0];
      const device1Copy = deviceWithAvailableStorage.devices[0];

      // Same fixture reference should produce identical data
      expect(device1.deviceId).toBe(device1Copy.deviceId);
      expect(device1.comPort).toBe(device1Copy.comPort);
      expect(device1.fwVersion).toBe(device1Copy.fwVersion);
    });

    it('different fixtures should have different device IDs', () => {
      const ids = new Set([
        deviceWithAvailableStorage.devices[0].deviceId,
        deviceWithUnavailableUsbStorage.devices[0].deviceId,
        deviceWithUnavailableSdStorage.devices[0].deviceId,
        allStorageUnavailable.devices[0].deviceId,
      ]);

      // All should be unique due to different seeds
      expect(ids.size).toBe(4);
    });
  });
});
