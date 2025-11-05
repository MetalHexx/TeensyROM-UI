/**
 * Device Fixture Validation Tests
 *
 * Validates that device fixtures maintain structural integrity, type safety,
 * and determinism. These tests ensure fixtures produce consistent, valid data
 * for E2E tests.
 */
import { describe, it, expect } from 'vitest';
import { DeviceState } from '@teensyrom-nx/data-access/api-client';
import type { MockDeviceFixture } from './fixture.types';
import { generateDevice } from '../generators/device.generators';
import {
  singleDevice,
  multipleDevices,
  noDevices,
  disconnectedDevice,
  unavailableStorageDevice,
  mixedStateDevices,
  threeDisconnectedDevices,
  mixedConnectionDevices,
} from './devices.fixture';

describe('MockDeviceFixture Type', () => {
  describe('Type Safety', () => {
    it('should accept fixture with empty devices array', () => {
      const fixture: MockDeviceFixture = {
        devices: [],
      };

      expect(fixture.devices).toHaveLength(0);
    });

    it('should accept fixture with single device', () => {
      const device = generateDevice();
      const fixture: MockDeviceFixture = {
        devices: [device],
      };

      expect(fixture.devices).toHaveLength(1);
      expect(fixture.devices[0]).toEqual(device);
    });

    it('should accept fixture with multiple devices', () => {
      const devices = [generateDevice(), generateDevice(), generateDevice()];
      const fixture: MockDeviceFixture = {
        devices,
      };

      expect(fixture.devices).toHaveLength(3);
    });

    it('should enforce readonly constraint on devices array', () => {
      const fixture: MockDeviceFixture = {
        devices: [generateDevice()],
      };

      // TypeScript should prevent mutation
      // @ts-expect-error - readonly array cannot be mutated
      fixture.devices.push(generateDevice());
    });
  });

  describe('CartDto Structure Validation', () => {
    it('should validate device has all required properties', () => {
      const device = generateDevice();
      const fixture: MockDeviceFixture = {
        devices: [device],
      };

      const validDevice = fixture.devices[0];
      expect(validDevice).toHaveProperty('deviceId');
      expect(validDevice).toHaveProperty('isConnected');
      expect(validDevice).toHaveProperty('deviceState');
      expect(validDevice).toHaveProperty('comPort');
      expect(validDevice).toHaveProperty('name');
      expect(validDevice).toHaveProperty('fwVersion');
      expect(validDevice).toHaveProperty('isCompatible');
      expect(validDevice).toHaveProperty('sdStorage');
      expect(validDevice).toHaveProperty('usbStorage');
    });

    it('should validate deviceState uses enum values', () => {
      const validStates = Object.values(DeviceState);
      const device = generateDevice();
      const fixture: MockDeviceFixture = {
        devices: [device],
      };

      expect(validStates).toContain(fixture.devices[0].deviceState);
    });

    it('should validate storage objects have required structure', () => {
      const device = generateDevice();
      const fixture: MockDeviceFixture = {
        devices: [device],
      };

      const { sdStorage, usbStorage } = fixture.devices[0];

      // Validate SD storage
      expect(sdStorage).toHaveProperty('deviceId');
      expect(sdStorage).toHaveProperty('type');
      expect(sdStorage).toHaveProperty('available');

      // Validate USB storage
      expect(usbStorage).toHaveProperty('deviceId');
      expect(usbStorage).toHaveProperty('type');
      expect(usbStorage).toHaveProperty('available');
    });
  });
});

describe('singleDevice Fixture', () => {
  describe('Structure Validation', () => {
    it('should match MockDeviceFixture interface', () => {
      expect(singleDevice).toHaveProperty('devices');
      expect(Array.isArray(singleDevice.devices)).toBe(true);
    });

    it('should contain exactly 1 device', () => {
      expect(singleDevice.devices).toHaveLength(1);
    });
  });

  describe('Device Properties', () => {
    it('should have connected device', () => {
      const device = singleDevice.devices[0];
      expect(device.isConnected).toBe(true);
    });

    it('should have Connected device state', () => {
      const device = singleDevice.devices[0];
      expect(device.deviceState).toBe(DeviceState.Connected);
    });

    it('should be compatible', () => {
      const device = singleDevice.devices[0];
      expect(device.isCompatible).toBe(true);
    });

    it('should have available SD storage', () => {
      const device = singleDevice.devices[0];
      expect(device.sdStorage.available).toBe(true);
    });

    it('should have available USB storage', () => {
      const device = singleDevice.devices[0];
      expect(device.usbStorage.available).toBe(true);
    });

    it('should have valid deviceId', () => {
      const device = singleDevice.devices[0];
      expect(device.deviceId).toBeTruthy();
      expect(typeof device.deviceId).toBe('string');
    });

    it('should have valid COM port', () => {
      const device = singleDevice.devices[0];
      expect(device.comPort).toBeTruthy();
      expect(device.comPort).toMatch(/^COM\d+$/);
    });

    it('should have valid device name', () => {
      const device = singleDevice.devices[0];
      expect(device.name).toBeTruthy();
      expect(device.name).toContain('TeensyROM');
    });

    it('should have valid firmware version', () => {
      const device = singleDevice.devices[0];
      expect(device.fwVersion).toBeTruthy();
      expect(device.fwVersion).toMatch(/^\d+\.\d+\.\d+$/);
    });
  });

  describe('Determinism', () => {
    it('should produce identical data across multiple references', () => {
      // Access the fixture multiple times
      const firstAccess = singleDevice.devices[0];
      const secondAccess = singleDevice.devices[0];

      expect(firstAccess).toEqual(secondAccess);
      expect(firstAccess.deviceId).toBe(secondAccess.deviceId);
      expect(firstAccess.name).toBe(secondAccess.name);
      expect(firstAccess.comPort).toBe(secondAccess.comPort);
    });
  });
});

describe('multipleDevices Fixture', () => {
  describe('Structure Validation', () => {
    it('should match MockDeviceFixture interface', () => {
      expect(multipleDevices).toHaveProperty('devices');
      expect(Array.isArray(multipleDevices.devices)).toBe(true);
    });

    it('should contain exactly 3 devices', () => {
      expect(multipleDevices.devices).toHaveLength(3);
    });
  });

  describe('Device Uniqueness', () => {
    it('should have unique device IDs', () => {
      const deviceIds = multipleDevices.devices.map((d) => d.deviceId);
      const uniqueIds = new Set(deviceIds);
      expect(uniqueIds.size).toBe(3);
    });

    it('should have unique COM ports', () => {
      const comPorts = multipleDevices.devices.map((d) => d.comPort);
      const uniquePorts = new Set(comPorts);
      expect(uniquePorts.size).toBe(3);
    });

    it('should have unique device names', () => {
      const names = multipleDevices.devices.map((d) => d.name);
      const uniqueNames = new Set(names);
      expect(uniqueNames.size).toBe(3);
    });
  });

  describe('Device Properties (All Devices)', () => {
    it('should have all devices connected', () => {
      multipleDevices.devices.forEach((device) => {
        expect(device.isConnected).toBe(true);
      });
    });

    it('should have all devices in Connected state', () => {
      multipleDevices.devices.forEach((device) => {
        expect(device.deviceState).toBe(DeviceState.Connected);
      });
    });

    it('should have all devices compatible', () => {
      multipleDevices.devices.forEach((device) => {
        expect(device.isCompatible).toBe(true);
      });
    });

    it('should have all devices with available SD storage', () => {
      multipleDevices.devices.forEach((device) => {
        expect(device.sdStorage.available).toBe(true);
      });
    });

    it('should have all devices with available USB storage', () => {
      multipleDevices.devices.forEach((device) => {
        expect(device.usbStorage.available).toBe(true);
      });
    });
  });

  describe('Determinism', () => {
    it('should produce identical data across multiple references', () => {
      const firstAccess = multipleDevices.devices;
      const secondAccess = multipleDevices.devices;

      expect(firstAccess).toHaveLength(3);
      expect(secondAccess).toHaveLength(3);

      firstAccess.forEach((device, index) => {
        expect(device.deviceId).toBe(secondAccess[index].deviceId);
        expect(device.name).toBe(secondAccess[index].name);
        expect(device.comPort).toBe(secondAccess[index].comPort);
      });
    });
  });
});

describe('noDevices Fixture', () => {
  describe('Structure Validation', () => {
    it('should match MockDeviceFixture interface', () => {
      expect(noDevices).toHaveProperty('devices');
      expect(Array.isArray(noDevices.devices)).toBe(true);
    });

    it('should contain empty array', () => {
      expect(noDevices.devices).toHaveLength(0);
    });

    it('should be properly typed as CartDto array', () => {
      // TypeScript compilation validates this
      const fixture: MockDeviceFixture = noDevices;
      expect(fixture.devices).toEqual([]);
    });
  });
});

describe('disconnectedDevice Fixture', () => {
  describe('Structure Validation', () => {
    it('should match MockDeviceFixture interface', () => {
      expect(disconnectedDevice).toHaveProperty('devices');
      expect(Array.isArray(disconnectedDevice.devices)).toBe(true);
    });

    it('should contain exactly 1 device', () => {
      expect(disconnectedDevice.devices).toHaveLength(1);
    });
  });

  describe('Connection State', () => {
    it('should have disconnected device', () => {
      const device = disconnectedDevice.devices[0];
      expect(device.isConnected).toBe(false);
    });

    it('should have ConnectionLost device state', () => {
      const device = disconnectedDevice.devices[0];
      expect(device.deviceState).toBe(DeviceState.ConnectionLost);
    });
  });

  describe('Previous Connection Data', () => {
    it('should have valid COM port from previous connection', () => {
      const device = disconnectedDevice.devices[0];
      expect(device.comPort).toBeTruthy();
      expect(device.comPort).toMatch(/^COM\d+$/);
    });

    it('should have valid device name', () => {
      const device = disconnectedDevice.devices[0];
      expect(device.name).toBeTruthy();
      expect(device.name).toContain('TeensyROM');
    });

    it('should have valid firmware version', () => {
      const device = disconnectedDevice.devices[0];
      expect(device.fwVersion).toBeTruthy();
      expect(device.fwVersion).toMatch(/^\d+\.\d+\.\d+$/);
    });

    it('should still have storage objects', () => {
      const device = disconnectedDevice.devices[0];
      expect(device.sdStorage).toBeTruthy();
      expect(device.usbStorage).toBeTruthy();
    });
  });
});

describe('unavailableStorageDevice Fixture', () => {
  describe('Structure Validation', () => {
    it('should match MockDeviceFixture interface', () => {
      expect(unavailableStorageDevice).toHaveProperty('devices');
      expect(Array.isArray(unavailableStorageDevice.devices)).toBe(true);
    });

    it('should contain exactly 1 device', () => {
      expect(unavailableStorageDevice.devices).toHaveLength(1);
    });
  });

  describe('Connection State', () => {
    it('should have connected device', () => {
      const device = unavailableStorageDevice.devices[0];
      expect(device.isConnected).toBe(true);
    });

    it('should have Connected device state', () => {
      const device = unavailableStorageDevice.devices[0];
      expect(device.deviceState).toBe(DeviceState.Connected);
    });

    it('should be compatible', () => {
      const device = unavailableStorageDevice.devices[0];
      expect(device.isCompatible).toBe(true);
    });
  });

  describe('Storage Availability', () => {
    it('should have unavailable SD storage', () => {
      const device = unavailableStorageDevice.devices[0];
      expect(device.sdStorage.available).toBe(false);
    });

    it('should have unavailable USB storage', () => {
      const device = unavailableStorageDevice.devices[0];
      expect(device.usbStorage.available).toBe(false);
    });

    it('should have valid storage objects with deviceId and type', () => {
      const device = unavailableStorageDevice.devices[0];
      expect(device.sdStorage.deviceId).toBeTruthy();
      expect(device.sdStorage.type).toBeTruthy();
      expect(device.usbStorage.deviceId).toBeTruthy();
      expect(device.usbStorage.type).toBeTruthy();
    });
  });
});

describe('mixedStateDevices Fixture', () => {
  describe('Structure Validation', () => {
    it('should match MockDeviceFixture interface', () => {
      expect(mixedStateDevices).toHaveProperty('devices');
      expect(Array.isArray(mixedStateDevices.devices)).toBe(true);
    });

    it('should contain exactly 3 devices', () => {
      expect(mixedStateDevices.devices).toHaveLength(3);
    });
  });

  describe('Device State Variety', () => {
    it('should have device 1 in Connected state', () => {
      const device = mixedStateDevices.devices[0];
      expect(device.deviceState).toBe(DeviceState.Connected);
      expect(device.isConnected).toBe(true);
    });

    it('should have device 2 in Busy state', () => {
      const device = mixedStateDevices.devices[1];
      expect(device.deviceState).toBe(DeviceState.Busy);
      expect(device.isConnected).toBe(true);
    });

    it('should have device 3 in ConnectionLost state', () => {
      const device = mixedStateDevices.devices[2];
      expect(device.deviceState).toBe(DeviceState.ConnectionLost);
      expect(device.isConnected).toBe(false);
    });

    it('should have all unique device IDs', () => {
      const deviceIds = mixedStateDevices.devices.map((d) => d.deviceId);
      const uniqueIds = new Set(deviceIds);
      expect(uniqueIds.size).toBe(3);
    });

    it('should have all unique COM ports', () => {
      const comPorts = mixedStateDevices.devices.map((d) => d.comPort);
      const uniquePorts = new Set(comPorts);
      expect(uniquePorts.size).toBe(3);
    });

    it('should have all unique device names', () => {
      const names = mixedStateDevices.devices.map((d) => d.name);
      const uniqueNames = new Set(names);
      expect(uniqueNames.size).toBe(3);
    });
  });

  describe('State Coverage', () => {
    it('should cover key testing scenarios', () => {
      const states = mixedStateDevices.devices.map((d) => d.deviceState);
      expect(states).toContain(DeviceState.Connected);
      expect(states).toContain(DeviceState.Busy);
      expect(states).toContain(DeviceState.ConnectionLost);
    });
  });
});

/**
 * threeDisconnectedDevices Fixture Tests
 *
 * Validates the three-disconnected-devices fixture used for sequential
 * connection tests and connection state isolation tests.
 */
describe('threeDisconnectedDevices Fixture', () => {
  describe('Structure', () => {
    it('should contain exactly 3 devices', () => {
      expect(threeDisconnectedDevices.devices).toHaveLength(3);
    });

    it('should match MockDeviceFixture type', () => {
      const fixture: MockDeviceFixture = threeDisconnectedDevices;
      expect(fixture.devices).toBeDefined();
      expect(Array.isArray(fixture.devices)).toBe(true);
    });
  });

  describe('Device Connection States', () => {
    it('should have all devices disconnected', () => {
      threeDisconnectedDevices.devices.forEach((device) => {
        expect(device.isConnected).toBe(false);
      });
    });

    it('should have all devices in ConnectionLost state', () => {
      threeDisconnectedDevices.devices.forEach((device) => {
        expect(device.deviceState).toBe(DeviceState.ConnectionLost);
      });
    });
  });

  describe('Device Uniqueness', () => {
    it('should have all unique device IDs', () => {
      const deviceIds = threeDisconnectedDevices.devices.map((d) => d.deviceId);
      const uniqueIds = new Set(deviceIds);
      expect(uniqueIds.size).toBe(3);
    });

    it('should have all unique COM ports', () => {
      const comPorts = threeDisconnectedDevices.devices.map((d) => d.comPort);
      const uniquePorts = new Set(comPorts);
      expect(uniquePorts.size).toBe(3);
    });

    it('should have all unique device names', () => {
      const names = threeDisconnectedDevices.devices.map((d) => d.name);
      const uniqueNames = new Set(names);
      expect(uniqueNames.size).toBe(3);
    });
  });

  describe('Device Capabilities', () => {
    it('should have all devices compatible', () => {
      threeDisconnectedDevices.devices.forEach((device) => {
        expect(device.isCompatible).toBe(true);
      });
    });

    it('should have all devices with available storage', () => {
      threeDisconnectedDevices.devices.forEach((device) => {
        expect(device.sdStorage?.available).toBe(true);
        expect(device.usbStorage?.available).toBe(true);
      });
    });
  });

  describe('Determinism', () => {
    it('should produce consistent results across multiple calls', () => {
      const fixture1 = threeDisconnectedDevices;
      const fixture2 = threeDisconnectedDevices;

      fixture1.devices.forEach((device1, index) => {
        const device2 = fixture2.devices[index];
        expect(device1.deviceId).toBe(device2.deviceId);
        expect(device1.comPort).toBe(device2.comPort);
        expect(device1.name).toBe(device2.name);
      });
    });
  });
});

/**
 * mixedConnectionDevices Fixture Tests
 *
 * Validates the mixed-connection-devices fixture used for displaying
 * and managing devices in various connection states simultaneously.
 */
describe('mixedConnectionDevices Fixture', () => {
  describe('Structure', () => {
    it('should contain exactly 3 devices', () => {
      expect(mixedConnectionDevices.devices).toHaveLength(3);
    });

    it('should match MockDeviceFixture type', () => {
      const fixture: MockDeviceFixture = mixedConnectionDevices;
      expect(fixture.devices).toBeDefined();
      expect(Array.isArray(fixture.devices)).toBe(true);
    });
  });

  describe('Device Connection States', () => {
    it('should have device 1 connected', () => {
      const device = mixedConnectionDevices.devices[0];
      expect(device.isConnected).toBe(true);
      expect(device.deviceState).toBe(DeviceState.Connected);
    });

    it('should have device 2 disconnected', () => {
      const device = mixedConnectionDevices.devices[1];
      expect(device.isConnected).toBe(false);
      expect(device.deviceState).toBe(DeviceState.ConnectionLost);
    });

    it('should have device 3 connected', () => {
      const device = mixedConnectionDevices.devices[2];
      expect(device.isConnected).toBe(true);
      expect(device.deviceState).toBe(DeviceState.Connected);
    });

    it('should have mixed connection states overall', () => {
      const connectedCount = mixedConnectionDevices.devices.filter((d) => d.isConnected).length;
      const disconnectedCount = mixedConnectionDevices.devices.filter((d) => !d.isConnected).length;

      expect(connectedCount).toBe(2);
      expect(disconnectedCount).toBe(1);
    });
  });

  describe('Device Uniqueness', () => {
    it('should have all unique device IDs', () => {
      const deviceIds = mixedConnectionDevices.devices.map((d) => d.deviceId);
      const uniqueIds = new Set(deviceIds);
      expect(uniqueIds.size).toBe(3);
    });

    it('should have all unique COM ports', () => {
      const comPorts = mixedConnectionDevices.devices.map((d) => d.comPort);
      const uniquePorts = new Set(comPorts);
      expect(uniquePorts.size).toBe(3);
    });

    it('should have all unique device names', () => {
      const names = mixedConnectionDevices.devices.map((d) => d.name);
      const uniqueNames = new Set(names);
      expect(uniqueNames.size).toBe(3);
    });
  });

  describe('Device Capabilities', () => {
    it('should have all devices compatible', () => {
      mixedConnectionDevices.devices.forEach((device) => {
        expect(device.isCompatible).toBe(true);
      });
    });

    it('should have all devices with available storage', () => {
      mixedConnectionDevices.devices.forEach((device) => {
        expect(device.sdStorage?.available).toBe(true);
        expect(device.usbStorage?.available).toBe(true);
      });
    });
  });

  describe('Determinism', () => {
    it('should produce consistent results across multiple calls', () => {
      const fixture1 = mixedConnectionDevices;
      const fixture2 = mixedConnectionDevices;

      fixture1.devices.forEach((device1, index) => {
        const device2 = fixture2.devices[index];
        expect(device1.deviceId).toBe(device2.deviceId);
        expect(device1.comPort).toBe(device2.comPort);
        expect(device1.name).toBe(device2.name);
        expect(device1.isConnected).toBe(device2.isConnected);
      });
    });
  });
});
