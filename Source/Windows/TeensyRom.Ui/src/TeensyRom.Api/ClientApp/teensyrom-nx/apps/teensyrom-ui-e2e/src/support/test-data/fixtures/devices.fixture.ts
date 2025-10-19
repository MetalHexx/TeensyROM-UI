/**
 * Device Mock Fixtures
 *
 * Pre-built device scenarios for E2E testing. All fixtures use Phase 1 generators
 * with deterministic seeding to ensure consistent, reproducible test data.
 *
 * Each fixture represents a realistic device scenario that can be used directly
 * in API interceptors without manual device construction.
 *
 * @see fixtures/README.md for detailed usage documentation
 * @see FAKE_TEST_DATA.md for generator documentation
 */
import { DeviceState } from '@teensyrom-nx/data-access/api-client';
import { faker } from '../faker-config';
import { generateCartStorage, generateDevice } from '../generators/device.generators';
import type { MockDeviceFixture } from './fixture.types';

/**
 * Single connected device with available storage.
 *
 * **Scenario**: Most common "happy path" - one TeensyROM device is connected
 * with both SD and USB storage available and accessible.
 *
 * **Use Cases**:
 * - Default device discovery tests
 * - Single device connection workflows
 * - Storage browsing with available storage
 * - Typical user scenarios
 *
 * **Device Properties**:
 * - Connected: ✅
 * - Device State: Connected
 * - Compatible: ✅
 * - SD Storage: Available ✅
 * - USB Storage: Available ✅
 *
 * @example Using in interceptor
 * ```typescript
 * cy.intercept('GET', '/api/devices', {
 *   statusCode: 200,
 *   body: singleDevice
 * });
 * ```
 */
export const singleDevice: MockDeviceFixture = (() => {
  faker.seed(12345);
  return {
    devices: [generateDevice()],
  };
})();

/**
 * Three connected devices with unique ports and names.
 *
 * **Scenario**: Multiple TeensyROM devices connected simultaneously.
 * Each device has unique identification (ID, port, name) due to faker's
 * sequential generation after seed reset.
 *
 * **Use Cases**:
 * - Multi-device discovery tests
 * - Device selection workflows
 * - Testing UI with multiple devices
 * - Device list rendering
 * - Device filtering and sorting
 *
 * **Device Properties** (all devices):
 * - Connected: ✅
 * - Device State: Connected
 * - Compatible: ✅
 * - SD Storage: Available ✅
 * - USB Storage: Available ✅
 * - Unique IDs, ports, and names
 *
 * @example Using in interceptor
 * ```typescript
 * cy.intercept('GET', '/api/devices', {
 *   statusCode: 200,
 *   body: multipleDevices
 * });
 * ```
 */
export const multipleDevices: MockDeviceFixture = (() => {
  faker.seed(12345);
  return {
    devices: [generateDevice(), generateDevice(), generateDevice()],
  };
})();

/**
 * No devices found - empty state.
 *
 * **Scenario**: No TeensyROM devices are connected or discovered.
 * Used to test empty state UI and "no devices found" messaging.
 *
 * **Use Cases**:
 * - Empty state display
 * - "No devices found" messaging
 * - First-time user experience
 * - All devices disconnected
 *
 * **Device Properties**:
 * - Device count: 0 (empty array)
 *
 * @example Using in interceptor
 * ```typescript
 * cy.intercept('GET', '/api/devices', {
 *   statusCode: 200,
 *   body: noDevices
 * });
 * ```
 */
export const noDevices: MockDeviceFixture = {
  devices: [],
};

/**
 * Device that lost connection.
 *
 * **Scenario**: A TeensyROM device was previously connected but has lost
 * its connection. Device properties (name, port, etc.) are still available
 * from the previous connection, enabling reconnection workflows.
 *
 * **Use Cases**:
 * - Connection loss handling
 * - Reconnection workflows
 * - Device state change testing
 * - Error recovery scenarios
 *
 * **Device Properties**:
 * - Connected: ❌
 * - Device State: ConnectionLost
 * - Valid previous connection data (name, port, version)
 * - Storage objects still exist
 *
 * @example Using in interceptor
 * ```typescript
 * cy.intercept('GET', '/api/devices', {
 *   statusCode: 200,
 *   body: disconnectedDevice
 * });
 * ```
 */
export const disconnectedDevice: MockDeviceFixture = (() => {
  faker.seed(12345);
  return {
    devices: [
      generateDevice({
        isConnected: false,
        deviceState: DeviceState.ConnectionLost,
      }),
    ],
  };
})();

/**
 * Connected device with unavailable storage.
 *
 * **Scenario**: Device is connected but both SD and USB storage are
 * unavailable (unmounted, hardware issue, corrupted filesystem, etc.).
 * Tests storage error handling without connection issues.
 *
 * **Use Cases**:
 * - Storage error handling
 * - Storage warning displays
 * - Preventing file operations on unavailable storage
 * - Hardware issue scenarios
 *
 * **Device Properties**:
 * - Connected: ✅
 * - Device State: Connected
 * - Compatible: ✅
 * - SD Storage: Unavailable ❌
 * - USB Storage: Unavailable ❌
 *
 * @example Using in interceptor
 * ```typescript
 * cy.intercept('GET', '/api/devices', {
 *   statusCode: 200,
 *   body: unavailableStorageDevice
 * });
 * ```
 */
export const unavailableStorageDevice: MockDeviceFixture = (() => {
  faker.seed(12345);
  const deviceId = faker.string.uuid();
  return {
    devices: [
      generateDevice({
        deviceId,
        sdStorage: generateCartStorage({ deviceId, available: false }),
        usbStorage: generateCartStorage({ deviceId, available: false }),
      }),
    ],
  };
})();

/**
 * Multiple devices in various states.
 *
 * **Scenario**: Three devices demonstrating different device states simultaneously.
 * Tests complex multi-device scenarios with state variety.
 *
 * **Device Properties**:
 * - Device 1: Connected and available
 * - Device 2: Busy (processing command)
 * - Device 3: ConnectionLost (disconnected)
 *
 * **Use Cases**:
 * - State filtering and display
 * - Multi-device state management
 * - Device list with varied states
 * - Complex device scenarios
 *
 * @example Using in interceptor
 * ```typescript
 * cy.intercept('GET', '/api/devices', {
 *   statusCode: 200,
 *   body: mixedStateDevices
 * });
 * ```
 */
export const mixedStateDevices: MockDeviceFixture = (() => {
  faker.seed(12345);
  return {
    devices: [
      generateDevice(), // Connected (default)
      generateDevice({ deviceState: DeviceState.Busy }), // Busy
      generateDevice({
        isConnected: false,
        deviceState: DeviceState.ConnectionLost,
      }), // Disconnected
    ],
  };
})();

/**
 * Three disconnected devices.
 *
 * **Scenario**: Three TeensyROM devices all disconnected with ConnectionLost state.
 * Provides clean slate for sequential connection tests.
 *
 * **Use Cases**:
 * - Sequential connection workflow tests (connect one by one)
 * - Connection state isolation tests
 * - Testing initial disconnected state for multiple devices
 * - Multi-device connection recovery scenarios
 *
 * **Device Properties** (all devices):
 * - Connected: ❌
 * - Device State: ConnectionLost
 * - Compatible: ✅
 * - SD Storage: Available ✅
 * - USB Storage: Available ✅
 * - Unique IDs, ports, and names
 *
 * @example Using in interceptor
 * ```typescript
 * cy.intercept('GET', '/api/devices', {
 *   statusCode: 200,
 *   body: threeDisconnectedDevices
 * });
 * ```
 */
export const threeDisconnectedDevices: MockDeviceFixture = (() => {
  faker.seed(12345);
  return {
    devices: [
      generateDevice({
        isConnected: false,
        deviceState: DeviceState.ConnectionLost,
      }),
      generateDevice({
        isConnected: false,
        deviceState: DeviceState.ConnectionLost,
      }),
      generateDevice({
        isConnected: false,
        deviceState: DeviceState.ConnectionLost,
      }),
    ],
  };
})();

/**
 * Three devices with mixed connection states.
 *
 * **Scenario**: Three devices demonstrating different connection states simultaneously.
 * Device 1 connected, Device 2 disconnected, Device 3 connected.
 * Tests realistic multi-device scenarios with varied connection states.
 *
 * **Use Cases**:
 * - Mixed device state display tests
 * - Device state isolation validation
 * - Testing UI with varied connection states
 * - Partial connection/disconnection scenarios
 *
 * **Device Properties**:
 * - Device 1: Connected (isConnected: true, state: Connected)
 * - Device 2: Disconnected (isConnected: false, state: ConnectionLost)
 * - Device 3: Connected (isConnected: true, state: Connected)
 * - All have compatible hardware and available storage
 *
 * @example Using in interceptor
 * ```typescript
 * cy.intercept('GET', '/api/devices', {
 *   statusCode: 200,
 *   body: mixedConnectionDevices
 * });
 * ```
 */
export const mixedConnectionDevices: MockDeviceFixture = (() => {
  faker.seed(12345);
  return {
    devices: [
      generateDevice(), // Device 1: Connected (default)
      generateDevice({
        isConnected: false,
        deviceState: DeviceState.ConnectionLost,
      }), // Device 2: Disconnected
      generateDevice(), // Device 3: Connected (default)
    ],
  };
})();
