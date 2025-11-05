import { TEST_FILES } from '../../constants/storage.constants';

/**
 * Favorite-specific test fixtures
 */
export const favoriteTestFiles = {
  /** PAC_MAN game file for favorite testing */
  pacMan: {
    ...TEST_FILES.GAMES.PAC_MAN,
    isFavorite: false, // Initial state for testing
  },
} as const;

/**
 * Test device configuration for favorite tests
 */
export const favoriteTestDevice = {
  id: 'test-device',
  name: 'Test Device',
} as const;

/**
 * Default navigation parameters for PAC_MAN favorite testing
 */
export const pacManNavigationParams = {
  device: favoriteTestDevice.id,
  storage: 'Sd',
  path: TEST_FILES.GAMES.PAC_MAN.filePath,
  file: TEST_FILES.GAMES.PAC_MAN.fileName,
} as const;
