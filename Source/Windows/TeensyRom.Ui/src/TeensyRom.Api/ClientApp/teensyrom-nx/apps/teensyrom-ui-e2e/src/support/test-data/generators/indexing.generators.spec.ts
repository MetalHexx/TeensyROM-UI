/**
 * Indexing Generators Unit Tests
 *
 * Verifies that generator functions create valid device data with the specified
 * storage availability options. Tests both single and batch generation modes.
 *
 * @see indexing.generators.ts for generator implementation
 */
import { describe, it, expect } from 'vitest';
import { DeviceState, TeensyStorageType } from '@teensyrom-nx/data-access/api-client';
import {
  generateDeviceForIndexing,
  generateMultipleDevicesForIndexing,
} from './indexing.generators';

describe('Indexing Generators', () => {
  describe('generateDeviceForIndexing', () => {
    it('should generate a valid CartDto with defaults', () => {
      const device = generateDeviceForIndexing();

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

    it('should default to connected device', () => {
      const device = generateDeviceForIndexing();

      expect(device.isConnected).toBe(true);
      expect(device.deviceState).toBe(DeviceState.Connected);
    });

    it('should default to compatible device', () => {
      const device = generateDeviceForIndexing();

      expect(device.isCompatible).toBe(true);
    });

    it('should default to all storage available', () => {
      const device = generateDeviceForIndexing();

      expect(device.sdStorage?.available).toBe(true);
      expect(device.usbStorage?.available).toBe(true);
    });

    it('should respect sdAvailable option', () => {
      const deviceWithSD = generateDeviceForIndexing({ sdAvailable: true });
      const deviceWithoutSD = generateDeviceForIndexing({ sdAvailable: false });

      expect(deviceWithSD.sdStorage?.available).toBe(true);
      expect(deviceWithoutSD.sdStorage?.available).toBe(false);
    });

    it('should respect usbAvailable option', () => {
      const deviceWithUSB = generateDeviceForIndexing({ usbAvailable: true });
      const deviceWithoutUSB = generateDeviceForIndexing({ usbAvailable: false });

      expect(deviceWithUSB.usbStorage?.available).toBe(true);
      expect(deviceWithoutUSB.usbStorage?.available).toBe(false);
    });

    it('should create USB unavailable, SD available device', () => {
      const device = generateDeviceForIndexing({
        sdAvailable: true,
        usbAvailable: false,
      });

      expect(device.sdStorage?.available).toBe(true);
      expect(device.usbStorage?.available).toBe(false);
    });

    it('should create SD unavailable, USB available device', () => {
      const device = generateDeviceForIndexing({
        sdAvailable: false,
        usbAvailable: true,
      });

      expect(device.sdStorage?.available).toBe(false);
      expect(device.usbStorage?.available).toBe(true);
    });

    it('should create device with both storage unavailable', () => {
      const device = generateDeviceForIndexing({
        sdAvailable: false,
        usbAvailable: false,
      });

      expect(device.sdStorage?.available).toBe(false);
      expect(device.usbStorage?.available).toBe(false);
    });

    it('should respect connected option', () => {
      const connectedDevice = generateDeviceForIndexing({ connected: true });
      const disconnectedDevice = generateDeviceForIndexing({ connected: false });

      expect(connectedDevice.isConnected).toBe(true);
      expect(connectedDevice.deviceState).toBe(DeviceState.Connected);

      expect(disconnectedDevice.isConnected).toBe(false);
      expect(disconnectedDevice.deviceState).toBe(DeviceState.Connectable);
    });

    it('should respect compatible option', () => {
      const compatibleDevice = generateDeviceForIndexing({ compatible: true });
      const incompatibleDevice = generateDeviceForIndexing({ compatible: false });

      expect(compatibleDevice.isCompatible).toBe(true);
      expect(incompatibleDevice.isCompatible).toBe(false);
    });

    it('should have valid storage types', () => {
      const device = generateDeviceForIndexing();

      expect(device.sdStorage?.type).toBe(TeensyStorageType.Sd);
      expect(device.usbStorage?.type).toBe(TeensyStorageType.Usb);
    });

    it('should generate unique device IDs across calls', () => {
      const device1 = generateDeviceForIndexing();
      const device2 = generateDeviceForIndexing();

      expect(device1.deviceId).not.toBe(device2.deviceId);
    });

    it('should match storage device ID with parent device ID', () => {
      const device = generateDeviceForIndexing();

      expect(device.sdStorage?.deviceId).toBe(device.deviceId);
      expect(device.usbStorage?.deviceId).toBe(device.deviceId);
    });

    it('should generate valid firmware version format', () => {
      const device = generateDeviceForIndexing();

      // Should match semantic versioning pattern: X.Y.Z
      expect(device.fwVersion).toMatch(/^\d+\.\d+\.\d+$/);
    });

    it('should support combining multiple options', () => {
      const device = generateDeviceForIndexing({
        sdAvailable: false,
        usbAvailable: true,
        connected: false,
        compatible: false,
      });

      expect(device.sdStorage?.available).toBe(false);
      expect(device.usbStorage?.available).toBe(true);
      expect(device.isConnected).toBe(false);
      expect(device.isCompatible).toBe(false);
    });
  });

  describe('generateMultipleDevicesForIndexing', () => {
    it('should generate correct number of devices', () => {
      const devices1 = generateMultipleDevicesForIndexing(1);
      const devices3 = generateMultipleDevicesForIndexing(3);
      const devices5 = generateMultipleDevicesForIndexing(5);

      expect(devices1.devices.length).toBe(1);
      expect(devices3.devices.length).toBe(3);
      expect(devices5.devices.length).toBe(5);
    });

    it('should return valid MockDeviceFixture structure', () => {
      const fixture = generateMultipleDevicesForIndexing(2);

      expect(fixture).toHaveProperty('devices');
      expect(Array.isArray(fixture.devices)).toBe(true);
    });

    it('should generate 0 devices when count is 0', () => {
      const fixture = generateMultipleDevicesForIndexing(0);

      expect(fixture.devices.length).toBe(0);
    });

    it('should apply options to all generated devices', () => {
      const fixture = generateMultipleDevicesForIndexing(3, {
        options: {
          sdAvailable: false,
          usbAvailable: true,
          connected: false,
        },
      });

      fixture.devices.forEach((device) => {
        expect(device.sdStorage?.available).toBe(false);
        expect(device.usbStorage?.available).toBe(true);
        expect(device.isConnected).toBe(false);
      });
    });

    it('should generate unique devices', () => {
      const fixture = generateMultipleDevicesForIndexing(3);
      const ids = fixture.devices.map((d) => d.deviceId);
      const uniqueIds = new Set(ids);

      expect(uniqueIds.size).toBe(3);
    });

    it('should generate all devices as valid CartDto', () => {
      const fixture = generateMultipleDevicesForIndexing(2);

      fixture.devices.forEach((device) => {
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

    it('should match storage device IDs with parent IDs in all devices', () => {
      const fixture = generateMultipleDevicesForIndexing(3);

      fixture.devices.forEach((device) => {
        expect(device.sdStorage?.deviceId).toBe(device.deviceId);
        expect(device.usbStorage?.deviceId).toBe(device.deviceId);
      });
    });

    it('should handle large counts efficiently', () => {
      const startTime = performance.now();
      const fixture = generateMultipleDevicesForIndexing(50);
      const endTime = performance.now();

      expect(fixture.devices.length).toBe(50);
      expect(endTime - startTime).toBeLessThan(1000); // Should complete in < 1 second
    });

    it('should generate devices without options applied uniformly', () => {
      const fixture = generateMultipleDevicesForIndexing(3);

      // Without options, all should default to connected and compatible
      fixture.devices.forEach((device) => {
        expect(device.isConnected).toBe(true);
        expect(device.isCompatible).toBe(true);
        expect(device.sdStorage?.available).toBe(true);
        expect(device.usbStorage?.available).toBe(true);
      });
    });

    it('should generate devices with partial storage availability when configured', () => {
      const fixture = generateMultipleDevicesForIndexing(2, {
        options: { sdAvailable: true, usbAvailable: false },
      });

      fixture.devices.forEach((device) => {
        expect(device.sdStorage?.available).toBe(true);
        expect(device.usbStorage?.available).toBe(false);
      });
    });

    it('should support both connected and disconnected device generation', () => {
      const connectedFixture = generateMultipleDevicesForIndexing(2, {
        options: { connected: true },
      });
      const disconnectedFixture = generateMultipleDevicesForIndexing(2, {
        options: { connected: false },
      });

      connectedFixture.devices.forEach((device) => {
        expect(device.isConnected).toBe(true);
      });

      disconnectedFixture.devices.forEach((device) => {
        expect(device.isConnected).toBe(false);
      });
    });

    it('should generate compatible and incompatible devices', () => {
      const compatibleFixture = generateMultipleDevicesForIndexing(2, {
        options: { compatible: true },
      });
      const incompatibleFixture = generateMultipleDevicesForIndexing(2, {
        options: { compatible: false },
      });

      compatibleFixture.devices.forEach((device) => {
        expect(device.isCompatible).toBe(true);
      });

      incompatibleFixture.devices.forEach((device) => {
        expect(device.isCompatible).toBe(false);
      });
    });
  });

  describe('Cross-generator consistency', () => {
    it('single and batch generators should produce compatible device structures', () => {
      const singleDevice = generateDeviceForIndexing();
      const batchFixture = generateMultipleDevicesForIndexing(1);
      const batchDevice = batchFixture.devices[0];

      // Both should have same properties
      expect(singleDevice).toHaveProperty('deviceId');
      expect(batchDevice).toHaveProperty('deviceId');

      expect(singleDevice.sdStorage).toBeDefined();
      expect(batchDevice.sdStorage).toBeDefined();

      expect(singleDevice.usbStorage).toBeDefined();
      expect(batchDevice.usbStorage).toBeDefined();
    });

    it('batch generator with single device should behave like single generator', () => {
      const singleResult = generateDeviceForIndexing({
        sdAvailable: true,
        usbAvailable: false,
      });
      const batchResult = generateMultipleDevicesForIndexing(1, {
        options: { sdAvailable: true, usbAvailable: false },
      });

      const batchDevice = batchResult.devices[0];

      // Both should have matching storage availability
      expect(singleResult.sdStorage?.available).toBe(batchDevice.sdStorage?.available);
      expect(singleResult.usbStorage?.available).toBe(batchDevice.usbStorage?.available);
    });
  });

  describe('Generator flexibility for E2E test scenarios', () => {
    it('should support creating all available storage combinations', () => {
      const bothAvailable = generateDeviceForIndexing({
        sdAvailable: true,
        usbAvailable: true,
      });
      const usbOnly = generateDeviceForIndexing({
        sdAvailable: false,
        usbAvailable: true,
      });
      const sdOnly = generateDeviceForIndexing({
        sdAvailable: true,
        usbAvailable: false,
      });
      const noneAvailable = generateDeviceForIndexing({
        sdAvailable: false,
        usbAvailable: false,
      });

      expect(bothAvailable.sdStorage?.available).toBe(true);
      expect(bothAvailable.usbStorage?.available).toBe(true);

      expect(usbOnly.sdStorage?.available).toBe(false);
      expect(usbOnly.usbStorage?.available).toBe(true);

      expect(sdOnly.sdStorage?.available).toBe(true);
      expect(sdOnly.usbStorage?.available).toBe(false);

      expect(noneAvailable.sdStorage?.available).toBe(false);
      expect(noneAvailable.usbStorage?.available).toBe(false);
    });

    it('should support creating connected/disconnected device combinations', () => {
      const connected = generateDeviceForIndexing({ connected: true });
      const disconnected = generateDeviceForIndexing({ connected: false });

      expect(connected.isConnected).toBe(true);
      expect(disconnected.isConnected).toBe(false);
    });

    it('should support batch generation with mixed scenarios', () => {
      const fixture = generateMultipleDevicesForIndexing(5);

      // Should have 5 unique, independent devices
      const ids = new Set(fixture.devices.map((d) => d.deviceId));
      expect(ids.size).toBe(5);

      // Each should be independently valid
      fixture.devices.forEach((device) => {
        expect(device.deviceId).toBeTruthy();
        expect(device.sdStorage?.deviceId).toBe(device.deviceId);
        expect(device.usbStorage?.deviceId).toBe(device.deviceId);
      });
    });
  });
});
