/**
 * Device Generator Tests
 *
 * Validates that device generators produce type-safe, realistic, and deterministic test data.
 * Tests cover default values, override behavior, type correctness, and reproducibility.
 */
import { describe, it, expect, beforeEach } from 'vitest';
import {
  CartDto,
  CartStorageDto,
  DeviceState,
  TeensyStorageType,
} from '@teensyrom-nx/data-access/api-client';
import { faker } from '../faker-config';
import { generateCartStorage, generateDevice } from './device.generators';

describe('Device Generators', () => {
  beforeEach(() => {
    // Reset seed before each test for consistency
    faker.seed(12345);
  });

  describe('generateCartStorage', () => {
    describe('Required Properties', () => {
      it('should generate all required CartStorageDto properties', () => {
        const storage = generateCartStorage();

        expect(storage).toHaveProperty('deviceId');
        expect(storage).toHaveProperty('type');
        expect(storage).toHaveProperty('available');
      });

      it('should generate valid UUID for deviceId', () => {
        const storage = generateCartStorage();

        const uuidPattern =
          /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
        expect(storage.deviceId).toMatch(uuidPattern);
      });

      it('should generate type as SD or USB', () => {
        const storage = generateCartStorage();

        expect([TeensyStorageType.Sd, TeensyStorageType.Usb]).toContain(storage.type);
      });
    });

    describe('Default Values', () => {
      it('should default available to true', () => {
        const storage = generateCartStorage();

        expect(storage.available).toBe(true);
      });
    });

    describe('Override Behavior', () => {
      it('should allow overriding deviceId', () => {
        const customId = 'custom-device-id';
        const storage = generateCartStorage({ deviceId: customId });

        expect(storage.deviceId).toBe(customId);
      });

      it('should allow overriding type to SD', () => {
        const storage = generateCartStorage({ type: TeensyStorageType.Sd });

        expect(storage.type).toBe(TeensyStorageType.Sd);
      });

      it('should allow overriding type to USB', () => {
        const storage = generateCartStorage({ type: TeensyStorageType.Usb });

        expect(storage.type).toBe(TeensyStorageType.Usb);
      });

      it('should allow overriding available to false', () => {
        const storage = generateCartStorage({ available: false });

        expect(storage.available).toBe(false);
      });

      it('should allow overriding multiple properties', () => {
        const overrides: Partial<CartStorageDto> = {
          deviceId: 'test-id',
          type: TeensyStorageType.Usb,
          available: false,
        };
        const storage = generateCartStorage(overrides);

        expect(storage.deviceId).toBe('test-id');
        expect(storage.type).toBe(TeensyStorageType.Usb);
        expect(storage.available).toBe(false);
      });
    });

    describe('Determinism', () => {
      it('should produce consistent results with same seed', () => {
        faker.seed(12345);
        const storage1 = generateCartStorage();

        faker.seed(12345);
        const storage2 = generateCartStorage();

        expect(storage2).toEqual(storage1);
      });

      it('should produce varied values in sequence', () => {
        faker.seed(12345);
        const storage1 = generateCartStorage();
        const storage2 = generateCartStorage();
        const storage3 = generateCartStorage();

        // DeviceIds should be different (following faker sequence)
        expect(storage1.deviceId).not.toBe(storage2.deviceId);
        expect(storage2.deviceId).not.toBe(storage3.deviceId);
      });
    });
  });

  describe('generateDevice', () => {
    describe('Required Properties', () => {
      it('should generate all required CartDto properties', () => {
        const device = generateDevice();

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

      it('should generate valid UUID for deviceId', () => {
        const device = generateDevice();

        const uuidPattern =
          /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
        expect(device.deviceId).toMatch(uuidPattern);
      });

      it('should generate valid COM port', () => {
        const device = generateDevice();

        expect(['COM3', 'COM4', 'COM5', 'COM6']).toContain(device.comPort);
      });

      it('should generate semantic version format for fwVersion', () => {
        const device = generateDevice();

        const semverPattern = /^\d+\.\d+\.\d+$/;
        expect(device.fwVersion).toMatch(semverPattern);
      });

      it('should generate name with TeensyROM prefix', () => {
        const device = generateDevice();

        expect(device.name).toMatch(/^TeensyROM /);
        expect(device.name.length).toBeGreaterThan('TeensyROM '.length);
      });
    });

    describe('Default Values', () => {
      it('should default isConnected to true', () => {
        const device = generateDevice();

        expect(device.isConnected).toBe(true);
      });

      it('should default deviceState to Connected', () => {
        const device = generateDevice();

        expect(device.deviceState).toBe(DeviceState.Connected);
      });

      it('should default isCompatible to true', () => {
        const device = generateDevice();

        expect(device.isCompatible).toBe(true);
      });
    });

    describe('Storage Objects', () => {
      it('should generate properly formed sdStorage', () => {
        const device = generateDevice();

        expect(device.sdStorage).toBeDefined();
        expect(device.sdStorage.type).toBe(TeensyStorageType.Sd);
        expect(device.sdStorage.deviceId).toBe(device.deviceId);
        expect(device.sdStorage.available).toBe(true);
      });

      it('should generate properly formed usbStorage', () => {
        const device = generateDevice();

        expect(device.usbStorage).toBeDefined();
        expect(device.usbStorage.type).toBe(TeensyStorageType.Usb);
        expect(device.usbStorage.deviceId).toBe(device.deviceId);
        expect(device.usbStorage.available).toBe(true);
      });

      it('should use same deviceId for storage as parent device', () => {
        const device = generateDevice();

        expect(device.sdStorage.deviceId).toBe(device.deviceId);
        expect(device.usbStorage.deviceId).toBe(device.deviceId);
      });
    });

    describe('Override Behavior', () => {
      it('should allow overriding isConnected', () => {
        const device = generateDevice({ isConnected: false });

        expect(device.isConnected).toBe(false);
      });

      it('should allow overriding deviceState', () => {
        const device = generateDevice({ deviceState: DeviceState.Busy });

        expect(device.deviceState).toBe(DeviceState.Busy);
      });

      it('should allow overriding isCompatible', () => {
        const device = generateDevice({ isCompatible: false });

        expect(device.isCompatible).toBe(false);
      });

      it('should allow overriding comPort', () => {
        const device = generateDevice({ comPort: 'COM99' });

        expect(device.comPort).toBe('COM99');
      });

      it('should allow overriding name', () => {
        const device = generateDevice({ name: 'Custom Device' });

        expect(device.name).toBe('Custom Device');
      });

      it('should allow overriding fwVersion', () => {
        const device = generateDevice({ fwVersion: '9.9.9' });

        expect(device.fwVersion).toBe('9.9.9');
      });

      it('should allow overriding sdStorage', () => {
        const customStorage: CartStorageDto = {
          deviceId: 'custom-id',
          type: TeensyStorageType.Sd,
          available: false,
        };
        const device = generateDevice({ sdStorage: customStorage });

        expect(device.sdStorage).toEqual(customStorage);
      });

      it('should allow overriding usbStorage', () => {
        const customStorage: CartStorageDto = {
          deviceId: 'custom-id',
          type: TeensyStorageType.Usb,
          available: false,
        };
        const device = generateDevice({ usbStorage: customStorage });

        expect(device.usbStorage).toEqual(customStorage);
      });

      it('should allow overriding multiple properties', () => {
        const overrides: Partial<CartDto> = {
          isConnected: false,
          deviceState: DeviceState.Connectable,
          isCompatible: false,
        };
        const device = generateDevice(overrides);

        expect(device.isConnected).toBe(false);
        expect(device.deviceState).toBe(DeviceState.Connectable);
        expect(device.isCompatible).toBe(false);
      });
    });

    describe('Determinism', () => {
      it('should produce consistent results with same seed', () => {
        faker.seed(12345);
        const device1 = generateDevice();

        faker.seed(12345);
        const device2 = generateDevice();

        expect(device2).toEqual(device1);
      });

      it('should produce varied devices in sequence', () => {
        faker.seed(12345);
        const device1 = generateDevice();
        const device2 = generateDevice();
        const device3 = generateDevice();

        // DeviceIds should be different
        expect(device1.deviceId).not.toBe(device2.deviceId);
        expect(device2.deviceId).not.toBe(device3.deviceId);

        // Names should be different (due to faker sequence)
        expect(device1.name).not.toBe(device2.name);
        expect(device2.name).not.toBe(device3.name);
      });
    });

    describe('Scenario Testing', () => {
      it('should generate realistic connected device', () => {
        const device = generateDevice();

        expect(device.isConnected).toBe(true);
        expect(device.deviceState).toBe(DeviceState.Connected);
        expect(device.isCompatible).toBe(true);
        expect(device.sdStorage.available).toBe(true);
        expect(device.usbStorage.available).toBe(true);
      });

      it('should allow creating disconnected device scenario', () => {
        const device = generateDevice({
          isConnected: false,
          deviceState: DeviceState.Connectable,
        });

        expect(device.isConnected).toBe(false);
        expect(device.deviceState).toBe(DeviceState.Connectable);
      });

      it('should allow creating incompatible device scenario', () => {
        const device = generateDevice({
          isCompatible: false,
          fwVersion: '0.1.0',
        });

        expect(device.isCompatible).toBe(false);
        expect(device.fwVersion).toBe('0.1.0');
      });

      it('should allow creating device with unavailable storage', () => {
        const device = generateDevice({
          sdStorage: {
            deviceId: 'test-id',
            type: TeensyStorageType.Sd,
            available: false,
          },
          usbStorage: {
            deviceId: 'test-id',
            type: TeensyStorageType.Usb,
            available: false,
          },
        });

        expect(device.sdStorage.available).toBe(false);
        expect(device.usbStorage.available).toBe(false);
      });
    });
  });
});
