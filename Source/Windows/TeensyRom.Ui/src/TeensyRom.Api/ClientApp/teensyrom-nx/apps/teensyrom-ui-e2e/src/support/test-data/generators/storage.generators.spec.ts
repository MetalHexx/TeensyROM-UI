import { describe, expect, it } from 'vitest';
import {
  FileItemType,
  instanceOfFileItemDto,
} from '@teensyrom-nx/data-access/api-client';
import { faker } from '../faker-config';
import {
  createMockFilesystem,
  generateFileItem,
  generateGamesDirectory,
  generateImagesDirectory,
  generateMusicianDirectory,
  getFavoritesPathByType,
} from './storage.generators';

describe('storage.generators', () => {
  describe('generateFileItem', () => {
    it('provides all required FileItemDto fields with sensible defaults', () => {
      faker.seed(2024);
      const file = generateFileItem({
        name: 'Deterministic.sid',
        parentPath: '/music',
        path: '/music/Deterministic.sid',
        type: FileItemType.Song,
      });

      expect(instanceOfFileItemDto(file)).toBe(true);
      expect(file.name).toBe('Deterministic.sid');
      expect(file.parentPath).toBe('/music');
      expect(file.path).toBe('/music/Deterministic.sid');
      expect(file.type).toBe(FileItemType.Song);
      expect(file.links).toEqual([]);
      expect(file.tags).toEqual([]);
      expect(file.youTubeVideos).toEqual([]);
      expect(file.competitions).toEqual([]);
      expect(file.images).toEqual([]);
      expect(file.isFavorite).toBe(false);
    });

    it('is deterministic when seed is reset before invocation', () => {
      faker.seed(77);
      const first = generateFileItem({
        parentPath: '/games',
        path: '/games/SeedTest.crt',
        name: 'SeedTest.crt',
      });

      faker.seed(77);
      const second = generateFileItem({
        parentPath: '/games',
        path: '/games/SeedTest.crt',
        name: 'SeedTest.crt',
      });

      expect(second).toEqual(first);
    });
  });

  describe('generateGamesDirectory', () => {
    it('creates flat directory with expected game names and types', () => {
      const games = generateGamesDirectory(100);

      expect(games).toHaveLength(5);
      expect(games.map((file) => file.name)).toEqual([
        '10th Frame.crt',
        '1942 (Music v1).crt',
        'Donkey Kong (Ocean).crt',
        'Pac-Man (J1).crt',
        'Mario Bros (Ocean) (J1).crt',
      ]);
      games.forEach((file) => {
        expect(file.path.startsWith('/games/')).toBe(true);
        expect(file.parentPath).toBe('/games');
        expect(file.type).toBe(FileItemType.Game);
        expect([64200, 80200, 88200]).toContain(file.size);
      });
    });

    it('is deterministic for identical seeds', () => {
      const first = generateGamesDirectory(222);
      const second = generateGamesDirectory(222);

      expect(second).toEqual(first);
    });
  });

  describe('generateMusicianDirectory', () => {
    it('creates SID files under the correct artist path', () => {
      const files = generateMusicianDirectory('L', 'LukHash', 123);
      expect(files).toHaveLength(4);
      files.forEach((file) => {
        expect(file.path.startsWith('/music/MUSICIANS/L/LukHash/')).toBe(true);
        expect(file.parentPath).toBe('/music/MUSICIANS/L/LukHash');
        expect(file.creator).toBe('LukHash');
        expect(file.type).toBe(FileItemType.Song);
        expect(file.size).toBeGreaterThanOrEqual(6700);
        expect(file.size).toBeLessThanOrEqual(22900);
      });
    });

    it('varies by letter to avoid collisions across directories', () => {
      const artistA = generateMusicianDirectory('A', 'ArtistA', 55);
      const artistB = generateMusicianDirectory('B', 'ArtistB', 55);

      expect(artistA).not.toEqual(artistB);
      expect(artistA[0].parentPath).toBe('/music/MUSICIANS/A/ArtistA');
      expect(artistB[0].parentPath).toBe('/music/MUSICIANS/B/ArtistB');
    });
  });

  describe('generateImagesDirectory', () => {
    it('creates deterministic artwork entries with fixed size', () => {
      const images = generateImagesDirectory(42);
      expect(images).toHaveLength(3);
      expect(images.map((file) => file.name)).toEqual([
        'ChrisCornell1.kla',
        'Dio2.kla',
        'SonicTheHedgehog.kla',
      ]);
      images.forEach((file) => {
        expect(file.type).toBe(FileItemType.Image);
        expect(file.size).toBe(9800);
        expect(file.parentPath).toBe('/images');
      });
    });
  });

  describe('createMockFilesystem', () => {
    it('populates games, music, images, and favorites directories', () => {
      const filesystem = createMockFilesystem(12345);

      const games = filesystem.getDirectory('/games');
      const lukHash = filesystem.getDirectory('/music/MUSICIANS/L/LukHash');
      const images = filesystem.getDirectory('/images');
      const favoritesGames = filesystem.getDirectory('/favorites/games');

      expect(games.storageItem.files).toHaveLength(5);
      expect(lukHash.storageItem.files).toHaveLength(4);
      expect(images.storageItem.files).toHaveLength(3);
      expect(favoritesGames.storageItem.files).toHaveLength(0);
      expect(games.storageItem.directories.map((dir) => dir.name)).toEqual([
        'Extras',
        'Large',
        'MultiLoad64',
      ]);
    });

    it('is resettable via MockFilesystem.reset()', () => {
      const filesystem = createMockFilesystem(500);
      filesystem.saveFavorite('/games/Pac-Man (J1).crt');
      filesystem.reset();

      const favorites = filesystem.getDirectory('/favorites/games');
      const games = filesystem.getDirectory('/games');

      expect(favorites.storageItem.files).toHaveLength(0);
      expect(games.storageItem.files.some((file) => file.isFavorite)).toBe(false);
    });
  });

  describe('getFavoritesPathByType', () => {
    it('maps FileItemType values to favorite paths', () => {
      expect(getFavoritesPathByType(FileItemType.Game)).toBe('/favorites/games');
      expect(getFavoritesPathByType(FileItemType.Song)).toBe('/favorites/music');
      expect(getFavoritesPathByType(FileItemType.Image)).toBe('/favorites/images');
      expect(getFavoritesPathByType(FileItemType.Hex)).toBe('/favorites/games');
      expect(getFavoritesPathByType(FileItemType.Unknown)).toBe('/favorites/games');
    });
  });
});
