/**
 * Indexing Test Fixtures
 *
 * Pre-built device scenarios specifically designed for storage indexing E2E tests.
 * These fixtures vary storage availability states to test different indexing scenarios.
 *
 * @see E2E_TESTS.md for test architecture and patterns
 * @see E2E_FIXTURES.md for general fixture documentation
 */
import { DeviceState, TeensyStorageType } from '@teensyrom-nx/data-access/api-client';
import { faker } from '../faker-config';
import { generateCartStorage, generateDevice } from '../generators/device.generators';
import type { MockDeviceFixture } from './fixture.types';

/**
 * Single connected device with both USB and SD storage available.
 *
 * **Scenario**: Device ready for indexing all storage types.
 * Perfect for testing single-device indexing workflows.
 *
 * **Device Properties**:
 * - Connected: ✅
 * - USB Storage: Available ✅
 * - SD Storage: Available ✅
 *
 * **Use Cases**:
 * - Single storage indexing (USB or SD)
 * - Button state verification
 * - Busy dialog display
 * - Error handling on single device
 *
 * @example
 * ```typescript
 * cy.intercept('GET', '/devices', singleDeviceForIndexing);
 * ```
 */
export const deviceWithAvailableStorage: MockDeviceFixture = (() => {
  faker.seed(54321);
  const device = generateDevice({
    isConnected: true,
    deviceState: DeviceState.Connected,
  });
  return {
    devices: [
      {
        ...device,
        sdStorage: generateCartStorage({
          deviceId: device.deviceId,
          type: TeensyStorageType.Sd,
          available: true,
        }),
        usbStorage: generateCartStorage({
          deviceId: device.deviceId,
          type: TeensyStorageType.Usb,
          available: true,
        }),
      },
    ],
  };
})();

/**
 * Single connected device with USB storage unavailable, SD available.
 *
 * **Scenario**: Mixed storage availability for testing selective indexing.
 *
 * **Device Properties**:
 * - Connected: ✅
 * - USB Storage: Unavailable ❌
 * - SD Storage: Available ✅
 *
 * **Use Cases**:
 * - Test disabled USB index button
 * - Test enabled SD index button
 * - Test partial storage availability scenarios
 *
 * @example
 * ```typescript
 * cy.intercept('GET', '/devices', deviceWithUnavailableUsbStorage);
 * ```
 */
export const deviceWithUnavailableUsbStorage: MockDeviceFixture = (() => {
  faker.seed(54322);
  const device = generateDevice({
    isConnected: true,
    deviceState: DeviceState.Connected,
  });
  return {
    devices: [
      {
        ...device,
        sdStorage: generateCartStorage({
          deviceId: device.deviceId,
          type: TeensyStorageType.Sd,
          available: true,
        }),
        usbStorage: generateCartStorage({
          deviceId: device.deviceId,
          type: TeensyStorageType.Usb,
          available: false,
        }),
      },
    ],
  };
})();

/**
 * Single connected device with SD storage unavailable, USB available.
 *
 * **Scenario**: Mixed storage availability - opposite of previous fixture.
 *
 * **Device Properties**:
 * - Connected: ✅
 * - USB Storage: Available ✅
 * - SD Storage: Unavailable ❌
 *
 * **Use Cases**:
 * - Test disabled SD index button
 * - Test enabled USB index button
 * - Verify independent storage state handling
 *
 * @example
 * ```typescript
 * cy.intercept('GET', '/devices', deviceWithUnavailableSdStorage);
 * ```
 */
export const deviceWithUnavailableSdStorage: MockDeviceFixture = (() => {
  faker.seed(54323);
  const device = generateDevice({
    isConnected: true,
    deviceState: DeviceState.Connected,
  });
  return {
    devices: [
      {
        ...device,
        sdStorage: generateCartStorage({
          deviceId: device.deviceId,
          type: TeensyStorageType.Sd,
          available: false,
        }),
        usbStorage: generateCartStorage({
          deviceId: device.deviceId,
          type: TeensyStorageType.Usb,
          available: true,
        }),
      },
    ],
  };
})();

/**
 * Single connected device with both USB and SD storage unavailable.
 *
 * **Scenario**: No indexable storage available on device.
 *
 * **Device Properties**:
 * - Connected: ✅
 * - USB Storage: Unavailable ❌
 * - SD Storage: Unavailable ❌
 *
 * **Use Cases**:
 * - Test both index buttons disabled
 * - Test "no storage available" scenarios
 * - Verify UI doesn't allow indexing when storage unavailable
 *
 * @example
 * ```typescript
 * cy.intercept('GET', '/devices', allStorageUnavailable);
 * ```
 */
export const allStorageUnavailable: MockDeviceFixture = (() => {
  faker.seed(54324);
  const device = generateDevice({
    isConnected: true,
    deviceState: DeviceState.Connected,
  });
  return {
    devices: [
      {
        ...device,
        sdStorage: generateCartStorage({
          deviceId: device.deviceId,
          type: TeensyStorageType.Sd,
          available: false,
        }),
        usbStorage: generateCartStorage({
          deviceId: device.deviceId,
          type: TeensyStorageType.Usb,
          available: false,
        }),
      },
    ],
  };
})();

/**
 * Two connected devices with different storage availability patterns.
 *
 * **Scenario**: Multi-device setup with mixed storage states for "Index All" testing.
 *
 * **Device 1 Properties**:
 * - Connected: ✅
 * - USB Storage: Available ✅
 * - SD Storage: Available ✅
 *
 * **Device 2 Properties**:
 * - Connected: ✅
 * - USB Storage: Unavailable ❌
 * - SD Storage: Available ✅
 *
 * **Use Cases**:
 * - Test "Index All" with partial storage availability
 * - Verify only available storage is indexed
 * - Test batch indexing with mixed states
 * - Multi-device UI state management
 *
 * @example
 * ```typescript
 * cy.intercept('GET', '/devices', multipleDevicesForIndexing);
 * ```
 */
export const multipleDevicesForIndexing: MockDeviceFixture = (() => {
  faker.seed(54325);
  
  const device1 = generateDevice({
    isConnected: true,
    deviceState: DeviceState.Connected,
  });

  const device2 = generateDevice({
    isConnected: true,
    deviceState: DeviceState.Connected,
  });

  return {
    devices: [
      {
        ...device1,
        sdStorage: generateCartStorage({
          deviceId: device1.deviceId,
          type: TeensyStorageType.Sd,
          available: true,
        }),
        usbStorage: generateCartStorage({
          deviceId: device1.deviceId,
          type: TeensyStorageType.Usb,
          available: true,
        }),
      },
      {
        ...device2,
        sdStorage: generateCartStorage({
          deviceId: device2.deviceId,
          type: TeensyStorageType.Sd,
          available: true,
        }),
        usbStorage: generateCartStorage({
          deviceId: device2.deviceId,
          type: TeensyStorageType.Usb,
          available: false,
        }),
      },
    ],
  };
})();

/**
 * Three connected devices all with available storage.
 *
 * **Scenario**: Full indexing capability across multiple devices for "Index All".
 *
 * **Device Properties** (all devices):
 * - Connected: ✅
 * - USB Storage: Available ✅
 * - SD Storage: Available ✅
 *
 * **Use Cases**:
 * - Test "Index All" with full storage availability
 * - Test batch indexing across 3+ devices
 * - Verify all API calls are made for Index All
 * - Test dialog persistence across batch operation
 * - Verify button state management with multiple devices
 *
 * @example
 * ```typescript
 * cy.intercept('GET', '/devices', threeDevicesFullIndexing);
 * ```
 */
export const threeDevicesFullIndexing: MockDeviceFixture = (() => {
  faker.seed(54326);
  
  const devices = [
    generateDevice({
      isConnected: true,
      deviceState: DeviceState.Connected,
    }),
    generateDevice({
      isConnected: true,
      deviceState: DeviceState.Connected,
    }),
    generateDevice({
      isConnected: true,
      deviceState: DeviceState.Connected,
    }),
  ];

  // Ensure storage deviceIds match parent device IDs
  return {
    devices: devices.map((device) => ({
      ...device,
      sdStorage: generateCartStorage({
        deviceId: device.deviceId,
        type: TeensyStorageType.Sd,
        available: true,
      }),
      usbStorage: generateCartStorage({
        deviceId: device.deviceId,
        type: TeensyStorageType.Usb,
        available: true,
      }),
    })),
  };
})();

/**
 * No devices connected.
 *
 * **Scenario**: Empty device list for testing "Index All" button disabled state.
 *
 * **Use Cases**:
 * - Test "Index All" button disabled when no devices
 * - Test empty state UI
 * - Verify button remains disabled until devices connect
 *
 * @example
 * ```typescript
 * cy.intercept('GET', '/devices', noDevicesForIndexing);
 * ```
 */
export const noDevicesForIndexing: MockDeviceFixture = {
  devices: [],
};
