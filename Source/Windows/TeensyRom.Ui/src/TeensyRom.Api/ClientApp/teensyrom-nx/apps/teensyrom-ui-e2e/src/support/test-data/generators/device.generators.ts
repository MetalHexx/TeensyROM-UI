/**
 * Device DTO Generators
 *
 * Type-safe generator functions for creating realistic TeensyROM device test data.
 * All generators use the seeded Faker instance for deterministic, reproducible data.
 *
 * These generators produce fully satisfied DTO objects with sensible "happy path" defaults.
 * Tests can override specific properties using the Partial<T> parameter to create failure scenarios.
 *
 * @example Default device (connected, compatible)
 * ```typescript
 * const device = generateDevice();
 * // All properties populated with realistic values
 * ```
 *
 * @example Disconnected device
 * ```typescript
 * const device = generateDevice({ isConnected: false, deviceState: DeviceState.Connectable });
 * ```
 *
 * @see FAKE_TEST_DATA.md for usage patterns and examples
 */
import {
  CartDto,
  CartStorageDto,
  DeviceState,
  TeensyStorageType,
} from '@teensyrom-nx/data-access/api-client';
import { faker } from '../faker-config';

/**
 * Generates a realistic CartStorageDto object representing TeensyROM storage.
 *
 * Defaults to available storage with randomly selected type (SD or USB).
 * Storage deviceId matches the parent device by default.
 *
 * @param overrides - Optional partial object to override generated values
 * @returns Fully populated CartStorageDto
 *
 * @example Generate default available storage
 * ```typescript
 * const storage = generateCartStorage();
 * ```
 *
 * @example Generate unavailable USB storage
 * ```typescript
 * const usbStorage = generateCartStorage({
 *   type: TeensyStorageType.Usb,
 *   available: false
 * });
 * ```
 */
export function generateCartStorage(
  overrides?: Partial<CartStorageDto>
): CartStorageDto {
  const generated: CartStorageDto = {
    deviceId: faker.string.uuid(),
    type: faker.helpers.arrayElement([TeensyStorageType.Sd, TeensyStorageType.Usb]),
    available: true,
  };

  return { ...generated, ...overrides };
}

/**
 * Generates a realistic CartDto object representing a complete TeensyROM device.
 *
 * Defaults to a connected, compatible device with available SD and USB storage.
 * All properties are populated with realistic values (COM ports, firmware versions, names).
 *
 * @param overrides - Optional partial object to override generated values
 * @returns Fully populated CartDto
 *
 * @example Generate default connected device
 * ```typescript
 * const device = generateDevice();
 * // Connected, compatible, both storages available
 * ```
 *
 * @example Generate disconnected device
 * ```typescript
 * const device = generateDevice({
 *   isConnected: false,
 *   deviceState: DeviceState.Connectable
 * });
 * ```
 *
 * @example Generate incompatible device with unavailable storage
 * ```typescript
 * const device = generateDevice({
 *   isCompatible: false,
 *   sdStorage: { deviceId: '', type: TeensyStorageType.Sd, available: false },
 *   usbStorage: { deviceId: '', type: TeensyStorageType.Usb, available: false }
 * });
 * ```
 */
export function generateDevice(overrides?: Partial<CartDto>): CartDto {
  const deviceId = faker.string.uuid();
  const fwVersion = `${faker.number.int({ min: 1, max: 2 })}.${faker.number.int({ min: 0, max: 9 })}.${faker.number.int({ min: 0, max: 20 })}`;

  const generated: CartDto = {
    deviceId,
    isConnected: true,
    deviceState: DeviceState.Connected,
    comPort: faker.helpers.arrayElement(['COM3', 'COM4', 'COM5', 'COM6']),
    name: `TeensyROM ${faker.company.name()}`,
    fwVersion,
    isCompatible: true,
    sdStorage: generateCartStorage({ deviceId, type: TeensyStorageType.Sd }),
    usbStorage: generateCartStorage({ deviceId, type: TeensyStorageType.Usb }),
  };

  return { ...generated, ...overrides };
}
