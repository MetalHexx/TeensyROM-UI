/**
 * Indexing Test Data Generators
 *
 * Factory functions for creating dynamic device scenarios with specific storage states.
 * Use these for one-off custom indexing scenarios not covered by pre-built fixtures.
 *
 * @see device.generators.ts for base device generation
 * @see indexing.fixture.ts for pre-built, reusable scenarios
 */
import { CartDto, DeviceState, TeensyStorageType } from '@teensyrom-nx/data-access/api-client';
import { generateCartStorage, generateDevice } from './device.generators';
import type { MockDeviceFixture } from '../fixtures/fixture.types';

/**
 * Generator options for indexing scenarios.
 *
 * @property sdAvailable - Whether SD storage is available for indexing
 * @property usbAvailable - Whether USB storage is available for indexing
 * @property connected - Whether device is connected (default: true)
 * @property compatible - Whether device is compatible (default: true)
 */
export interface GenerateIndexingDeviceOptions {
  sdAvailable?: boolean;
  usbAvailable?: boolean;
  connected?: boolean;
  compatible?: boolean;
}

/**
 * Generator options for batch device scenarios.
 *
 * @property count - Number of devices to generate
 * @property options - Apply same indexing options to all devices
 */
export interface GenerateMultipleIndexingDevicesOptions {
  options?: GenerateIndexingDeviceOptions;
}

/**
 * Generates a single device with flexible storage availability for indexing scenarios.
 *
 * Use this for custom one-off device scenarios not covered by pre-built fixtures.
 * Each call generates a new unique device (not deterministic like fixtures).
 *
 * **Defaults**:
 * - Connected: true
 * - Compatible: true
 * - SD Available: true
 * - USB Available: true
 *
 * @param options - Control storage availability and connection state
 * @returns Unique device with specified storage states
 *
 * @example Single device, USB only
 * ```typescript
 * const device = generateDeviceForIndexing({ sdAvailable: false, usbAvailable: true });
 * ```
 *
 * @example Disconnected device with available storage
 * ```typescript
 * const device = generateDeviceForIndexing({ connected: false });
 * ```
 *
 * @example Device with no available storage
 * ```typescript
 * const device = generateDeviceForIndexing({ sdAvailable: false, usbAvailable: false });
 * ```
 */
export function generateDeviceForIndexing(
  options: GenerateIndexingDeviceOptions = {}
): CartDto {
  const {
    sdAvailable = true,
    usbAvailable = true,
    connected = true,
    compatible = true,
  } = options;

  const baseDevice = generateDevice({
    isConnected: connected,
    deviceState: connected ? DeviceState.Connected : DeviceState.Connectable,
    isCompatible: compatible,
  });

  // Generate storage with matching deviceId
  return {
    ...baseDevice,
    sdStorage: generateCartStorage({
      deviceId: baseDevice.deviceId,
      type: TeensyStorageType.Sd,
      available: sdAvailable,
    }),
    usbStorage: generateCartStorage({
      deviceId: baseDevice.deviceId,
      type: TeensyStorageType.Usb,
      available: usbAvailable,
    }),
  };
}

/**
 * Generates multiple devices with flexible storage availability for batch indexing scenarios.
 *
 * Use this for custom multi-device test setups. Each device generated is unique.
 *
 * @param count - Number of devices to generate
 * @param options - Optional indexing options applied to all generated devices
 * @returns MockDeviceFixture with specified number of devices
 *
 * @example 3 devices with USB only
 * ```typescript
 * const fixture = generateMultipleDevicesForIndexing(3, { options: { sdAvailable: false } });
 * ```
 *
 * @example 5 devices with all storage available
 * ```typescript
 * const fixture = generateMultipleDevicesForIndexing(5);
 * ```
 *
 * @example 2 disconnected devices
 * ```typescript
 * const fixture = generateMultipleDevicesForIndexing(2, { options: { connected: false } });
 * ```
 */
export function generateMultipleDevicesForIndexing(
  count: number,
  options: GenerateMultipleIndexingDevicesOptions = {}
): MockDeviceFixture {
  const devices: CartDto[] = [];

  for (let i = 0; i < count; i++) {
    devices.push(generateDeviceForIndexing(options.options));
  }

  return { devices };
}
