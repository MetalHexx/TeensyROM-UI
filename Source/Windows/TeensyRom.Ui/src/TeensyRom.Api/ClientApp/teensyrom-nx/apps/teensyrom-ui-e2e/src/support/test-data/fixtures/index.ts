/**
 * Device Mock Fixtures - Barrel Exports
 *
 * Central export point for all device fixtures and fixture types.
 * Import fixtures from this index for clean, consistent imports in E2E tests.
 *
 * @example Importing fixtures
 * ```typescript
 * import { singleDevice, multipleDevices, noDevices } from '../support/test-data/fixtures';
 * ```
 *
 * @example Importing types
 * ```typescript
 * import type { MockDeviceFixture } from '../support/test-data/fixtures';
 * ```
 */

// Export fixture type interface
export type { MockDeviceFixture } from './fixture.types';

// Export all device fixtures
export {
  disconnectedDevice,
  mixedConnectionDevices,
  mixedStateDevices,
  multipleDevices,
  noDevices,
  singleDevice,
  threeDisconnectedDevices,
  unavailableStorageDevice,
} from './devices.fixture';
