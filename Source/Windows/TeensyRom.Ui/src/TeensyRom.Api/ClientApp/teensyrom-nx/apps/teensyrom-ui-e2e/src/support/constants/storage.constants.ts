/**
 * Storage Domain Constants
 *
 * Constants related to storage operations, file paths, and test data
 * for TeensyROM storage (SD card and USB).
 */

// Re-export TeensyStorageType from API client for convenience
export { TeensyStorageType } from '@teensyrom-nx/data-access/api-client';

/**
 * Common directory paths used in tests
 */
export const TEST_PATHS = {
  /** Root directory */
  ROOT: '/',

  /** Path separator character */
  SEPARATOR: '/',

  /** Games directory */
  GAMES: '/games',

  /** Music root directory */
  MUSIC: '/music',

  /** MUSICIANS subdirectory */
  MUSIC_MUSICIANS: '/music/MUSICIANS',

  /** LukHash artist directory */
  MUSIC_LUKHASH: '/music/MUSICIANS/L/LukHash',

  /** Images directory */
  IMAGES: '/images',

  /** Favorites root directory */
  FAVORITES: '/favorites',

  /** Favorites games directory */
  FAVORITES_GAMES: '/favorites/games',

  /** Favorites music directory */
  FAVORITES_MUSIC: '/favorites/music',

  /** Favorites images directory */
  FAVORITES_IMAGES: '/favorites/images',
} as const;

/**
 * Test file metadata for consistent file references across tests
 */
export const TEST_FILES = {
  /** Default fallback filename for unknown files */
  DEFAULT_UNKNOWN_FILE: 'unknown.crt',

  /**
   * Game files from the /games directory
   */
  GAMES: {
    PAC_MAN: {
      fileName: 'Pac-Man (J1).crt',
      filePath: '/games/Pac-Man (J1).crt',
      title: 'Pac-Man',
    },
    DONKEY_KONG: {
      fileName: 'Donkey Kong (Ocean).crt',
      filePath: '/games/Donkey Kong (Ocean).crt',
      title: 'Donkey Kong',
    },
    MARIO_BROS: {
      fileName: 'Mario Bros (Ocean) (J1).crt',
      filePath: '/games/Mario Bros (Ocean) (J1).crt',
      title: 'Mario Bros',
    },
    FRAME_10TH: {
      fileName: '10th Frame.crt',
      filePath: '/games/10th Frame.crt',
      title: '10th Frame',
    },
    GAME_1942: {
      fileName: '1942 (Music v1).crt',
      filePath: '/games/1942 (Music v1).crt',
      title: '1942',
    },
  },

  /**
   * Music files from LukHash directory
   */
  MUSIC: {
    ALPHA: {
      fileName: 'Alpha.sid',
      filePath: '/music/MUSICIANS/L/LukHash/Alpha.sid',
      title: 'Alpha',
    },
    DREAMS: {
      fileName: 'Dreams.sid',
      filePath: '/music/MUSICIANS/L/LukHash/Dreams.sid',
      title: 'Dreams',
    },
    NEON_THRILLS: {
      fileName: 'Neon_Thrills.sid',
      filePath: '/music/MUSICIANS/L/LukHash/Neon_Thrills.sid',
      title: 'Neon Thrills',
    },
    PROXIMA: {
      fileName: 'Proxima.sid',
      filePath: '/music/MUSICIANS/L/LukHash/Proxima.sid',
      title: 'Proxima',
    },
  },

  /**
   * Image files from /images directory
   */
  IMAGES: {
    CHRIS_CORNELL: {
      fileName: 'ChrisCornell1.kla',
      filePath: '/images/ChrisCornell1.kla',
      title: 'Chris Cornell 1',
    },
    DIO: {
      fileName: 'Dio2.kla',
      filePath: '/images/Dio2.kla',
      title: 'Dio 2',
    },
    SONIC: {
      fileName: 'SonicTheHedgehog.kla',
      filePath: '/images/SonicTheHedgehog.kla',
      title: 'Sonic The Hedgehog',
    },
  },
} as const;
