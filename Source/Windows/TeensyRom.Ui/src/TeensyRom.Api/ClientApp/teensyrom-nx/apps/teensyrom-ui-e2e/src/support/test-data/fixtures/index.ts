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
