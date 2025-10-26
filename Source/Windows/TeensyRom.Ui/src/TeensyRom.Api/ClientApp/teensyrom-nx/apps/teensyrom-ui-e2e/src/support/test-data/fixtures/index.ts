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

// Export indexing-specific fixtures
export {
  deviceWithAvailableStorage,
  deviceWithUnavailableUsbStorage,
  deviceWithUnavailableSdStorage,
  allStorageUnavailable,
  multipleDevicesForIndexing,
  threeDevicesFullIndexing,
  noDevicesForIndexing,
} from './indexing.fixture';

// Export storage favorites fixtures
export {
  alreadyFavoritedDirectory,
  emptyFilesystem,
  favoriteReadyDirectory,
  mixedFavoritesDirectory,
} from './storage-favorites.fixture';
