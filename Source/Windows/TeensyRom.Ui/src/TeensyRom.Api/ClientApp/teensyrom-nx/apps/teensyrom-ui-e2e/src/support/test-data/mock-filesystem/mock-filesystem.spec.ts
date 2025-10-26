import { beforeEach, describe, expect, it } from 'vitest';
import { FileItemType } from '@teensyrom-nx/data-access/api-client';
import MockFilesystem from './mock-filesystem';
import type { MockDirectory, MockFile } from './filesystem.types';

const createMockFile = (overrides: Partial<MockFile> = {}): MockFile => ({
  name: 'Sample.sid',
  path: '/music/Sample.sid',
  size: 1024,
  isFavorite: false,
  isCompatible: true,
  title: 'Sample Title',
  creator: 'Sample Creator',
  releaseInfo: 'Sample Release',
  description: 'Sample Description',
  shareUrl: 'https://example.com/sample',
  metadataSource: 'HVSC',
  meta1: 'meta1',
  meta2: 'meta2',
  links: [],
  tags: [],
  youTubeVideos: [],
  competitions: [],
  avgRating: null,
  ratingCount: 0,
  metadataSourcePath: '/metadata/sample',
  parentPath: '/music',
  playLength: '03:25',
  subtuneLengths: [],
  startSubtuneNum: 1,
  images: [],
  type: FileItemType.Song,
  ...overrides,
});

const buildFilesystem = () => {
  const filesystem = new MockFilesystem(12345);

  const gameFile = createMockFile({
    name: 'Pac-Man (J1).crt',
    path: '/games/Pac-Man (J1).crt',
    parentPath: '/games',
    type: FileItemType.Game,
  });

  const songFile = createMockFile({
    name: 'Alpha.sid',
    path: '/music/MUSICIANS/L/LukHash/Alpha.sid',
    parentPath: '/music/MUSICIANS/L/LukHash',
    type: FileItemType.Song,
  });

  const baseDirectories: MockDirectory[] = [
    {
      path: '/games',
      files: [gameFile],
      subdirectories: [],
    },
    {
      path: '/music/MUSICIANS/L/LukHash',
      files: [songFile],
      subdirectories: [],
    },
    {
      path: '/favorites/games',
      files: [],
      subdirectories: [],
    },
    {
      path: '/favorites/music',
      files: [],
      subdirectories: [],
    },
    {
      path: '/favorites/images',
      files: [],
      subdirectories: [],
    },
  ];

  for (const directory of baseDirectories) {
    filesystem.addDirectory(directory.path, directory);
  }

  return { filesystem, gameFile, songFile };
};

describe('MockFilesystem', () => {
  let filesystem: MockFilesystem;

  beforeEach(() => {
    ({ filesystem } = buildFilesystem());
  });

  describe('getDirectory', () => {
    it('returns storage data for known directory', () => {
      const { storageItem, message } = filesystem.getDirectory('/games');

      expect(message).toBe('Success');
      expect(storageItem.path).toBe('/games');
      expect(storageItem.files).toHaveLength(1);
      expect(storageItem.files[0]).toMatchObject({
        name: 'Pac-Man (J1).crt',
        path: '/games/Pac-Man (J1).crt',
      });
    });

    it('returns empty directory when path missing', () => {
      const { storageItem, message } = filesystem.getDirectory('/unknown');

      expect(message).toBe('Success');
      expect(storageItem.path).toBe('/unknown');
      expect(storageItem.files).toHaveLength(0);
      expect(storageItem.directories).toHaveLength(0);
    });

    it('returns cloned results to avoid accidental mutation', () => {
      const first = filesystem.getDirectory('/games');
      first.storageItem.files[0].isFavorite = true;

      const second = filesystem.getDirectory('/games');
      expect(second.storageItem.files[0].isFavorite).toBe(false);
    });
  });

  describe('saveFavorite', () => {
    it('marks original file and copies into favorites directory', () => {
      const response = filesystem.saveFavorite('/games/Pac-Man (J1).crt');
      expect(response).toMatchObject({
        message: 'Favorite tagged and saved to /favorites/games',
        favoritePath: '/favorites/games',
      });
      expect(response.favoriteFile).toMatchObject({
        path: '/favorites/games/Pac-Man (J1).crt',
        parentPath: '/games',
        isFavorite: true,
      });

      const favoritesDirectory = filesystem.getDirectory('/favorites/games');
      expect(favoritesDirectory.storageItem.files).toHaveLength(1);
      expect(favoritesDirectory.storageItem.files[0]).toMatchObject({
        name: 'Pac-Man (J1).crt',
        isFavorite: true,
        parentPath: '/games',
      });

      const sourceDirectory = filesystem.getDirectory('/games');
      expect(sourceDirectory.storageItem.files[0].isFavorite).toBe(true);
    });

    it('overwrites an existing favorite copy by file name', () => {
      filesystem.saveFavorite('/games/Pac-Man (J1).crt');
      filesystem.saveFavorite('/games/Pac-Man (J1).crt');

      const favoritesDirectory = filesystem.getDirectory('/favorites/games');
      expect(favoritesDirectory.storageItem.files).toHaveLength(1);
    });

    it('throws when file does not exist', () => {
      expect(() => filesystem.saveFavorite('/games/Missing.crt')).toThrowError(
        'File not found: /games/Missing.crt',
      );
    });
  });

  describe('removeFavorite', () => {
    beforeEach(() => {
      filesystem.saveFavorite('/games/Pac-Man (J1).crt');
    });

    it('accepts original path and clears favorite state', () => {
      const response = filesystem.removeFavorite('/games/Pac-Man (J1).crt');
      expect(response.message).toBe('Favorite untagged and removed from /favorites/games');

      const favoritesDirectory = filesystem.getDirectory('/favorites/games');
      expect(favoritesDirectory.storageItem.files).toHaveLength(0);

      const sourceDirectory = filesystem.getDirectory('/games');
      expect(sourceDirectory.storageItem.files[0].isFavorite).toBe(false);
    });

    it('accepts favorites path and clears state', () => {
      const response = filesystem.removeFavorite('/favorites/games/Pac-Man (J1).crt');
      expect(response.message).toBe('Favorite untagged and removed from /favorites/games');

      const favoritesDirectory = filesystem.getDirectory('/favorites/games');
      expect(favoritesDirectory.storageItem.files).toHaveLength(0);

      const sourceDirectory = filesystem.getDirectory('/games');
      expect(sourceDirectory.storageItem.files[0].isFavorite).toBe(false);
    });

    it('throws when file missing', () => {
      filesystem.removeFavorite('/games/Pac-Man (J1).crt');

      expect(() =>
        filesystem.removeFavorite('/favorites/games/Pac-Man (J1).crt'),
      ).toThrowError('File not found: /favorites/games/Pac-Man (J1).crt');
    });
  });

  describe('reset', () => {
    it('restores directories to seeded snapshot', () => {
      filesystem.saveFavorite('/games/Pac-Man (J1).crt');
      filesystem.saveFavorite('/music/MUSICIANS/L/LukHash/Alpha.sid');
      filesystem.removeFavorite('/games/Pac-Man (J1).crt');

      filesystem.reset();

      const gamesDirectory = filesystem.getDirectory('/games');
      expect(gamesDirectory.storageItem.files[0]).toMatchObject({
        name: 'Pac-Man (J1).crt',
        isFavorite: false,
      });

      const favoritesGamesDirectory = filesystem.getDirectory('/favorites/games');
      expect(favoritesGamesDirectory.storageItem.files).toHaveLength(0);

      const favoritesMusicDirectory = filesystem.getDirectory('/favorites/music');
      expect(favoritesMusicDirectory.storageItem.files).toHaveLength(0);
    });
  });
});
