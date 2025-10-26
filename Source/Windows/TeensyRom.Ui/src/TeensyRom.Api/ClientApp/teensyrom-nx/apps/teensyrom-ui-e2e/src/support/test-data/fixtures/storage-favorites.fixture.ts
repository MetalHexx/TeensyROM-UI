/**
 * Storage Favorites Fixtures
 *
 * Provides pre-built filesystem scenarios for Cypress favorite workflow tests.
 * Fixtures wrap deterministic MockFilesystem instances seeded via Task 2 generators.
 *
 * Each fixture overrides reset() where needed to ensure scenario state persists
 * between tests, keeping fixtures safe for shared usage.
 */
import { createMockFilesystem } from '../generators/storage.generators';
import MockFilesystem from '../mock-filesystem/mock-filesystem';

const DEFAULT_SEED = 12345;

type ScenarioInitializer = (filesystem: MockFilesystem) => void;

/**
 * Enhances a filesystem with scenario-specific state while preserving reset().
 */
const applyScenario = (
  filesystem: MockFilesystem,
  initialize?: ScenarioInitializer,
): MockFilesystem => {
  if (!initialize) {
    return filesystem;
  }

  const baseReset = filesystem.reset.bind(filesystem);

  const configure = () => {
    initialize(filesystem);
  };

  configure();

  filesystem.reset = () => {
    baseReset();
    configure();
  };

  return filesystem;
};

/**
 * Fresh filesystem with no favorites applied.
 *
 * Use for tests covering initial favorite creation from original directories.
 */
export const emptyFilesystem = (() => {
  return createMockFilesystem(DEFAULT_SEED);
})();

/**
 * Filesystem prepared for favorite interactions (identical to empty but provided
 * for semantic clarity in tests that transition immediately into favorites flow).
 */
export const favoriteReadyDirectory = (() => {
  return createMockFilesystem(DEFAULT_SEED);
})();

/**
 * Filesystem with Pac-Man already favorited. Reset maintains this scenario.
 */
export const alreadyFavoritedDirectory = (() => {
  const filesystem = createMockFilesystem(DEFAULT_SEED);
  return applyScenario(filesystem, (fs) => {
    fs.saveFavorite('/games/Pac-Man (J1).crt');
  });
})();

/**
 * Filesystem with favorites across game, music, and image directories.
 * Reset preserves all favorites.
 */
export const mixedFavoritesDirectory = (() => {
  const filesystem = createMockFilesystem(DEFAULT_SEED);
  return applyScenario(filesystem, (fs) => {
    fs.saveFavorite('/games/Donkey Kong (Ocean).crt');
    fs.saveFavorite('/music/MUSICIANS/L/LukHash/Alpha.sid');
    fs.saveFavorite('/images/SonicTheHedgehog.kla');
  });
})();
