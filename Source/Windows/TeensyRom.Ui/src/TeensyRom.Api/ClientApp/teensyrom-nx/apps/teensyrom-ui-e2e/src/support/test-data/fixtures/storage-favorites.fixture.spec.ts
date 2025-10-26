import { beforeEach, describe, expect, it } from 'vitest';
import {
  alreadyFavoritedDirectory,
  emptyFilesystem,
  favoriteReadyDirectory,
  mixedFavoritesDirectory,
} from './storage-favorites.fixture';

const resetFixtures = () => {
  emptyFilesystem.reset();
  favoriteReadyDirectory.reset();
  alreadyFavoritedDirectory.reset();
  mixedFavoritesDirectory.reset();
};

describe('storage-favorites.fixture', () => {
  beforeEach(() => {
    resetFixtures();
  });

  describe('emptyFilesystem', () => {
    it('contains seeded files with no favorites', () => {
      const games = emptyFilesystem.getDirectory('/games');
      const music = emptyFilesystem.getDirectory('/music/MUSICIANS/L/LukHash');
      const favorites = emptyFilesystem.getDirectory('/favorites/games');

      expect(games.storageItem.files.every((file) => file.isFavorite === false)).toBe(true);
      expect(music.storageItem.files.every((file) => file.isFavorite === false)).toBe(true);
      expect(favorites.storageItem.files).toHaveLength(0);
    });
  });

  describe('favoriteReadyDirectory', () => {
    it('matches empty filesystem state for baseline workflows', () => {
      const games = favoriteReadyDirectory.getDirectory('/games');
      const favorites = favoriteReadyDirectory.getDirectory('/favorites/games');

      expect(games.storageItem.files).toHaveLength(5);
      expect(games.storageItem.files.some((file) => file.isFavorite)).toBe(false);
      expect(favorites.storageItem.files).toHaveLength(0);
    });
  });

  describe('alreadyFavoritedDirectory', () => {
    it('includes Pac-Man in favorites and marks original as favorite', () => {
      const games = alreadyFavoritedDirectory.getDirectory('/games');
      const favorites = alreadyFavoritedDirectory.getDirectory('/favorites/games');

      const pacMan = games.storageItem.files.find((file) => file.name === 'Pac-Man (J1).crt');
      expect(pacMan?.isFavorite).toBe(true);
      expect(favorites.storageItem.files).toHaveLength(1);
      expect(favorites.storageItem.files[0].name).toBe('Pac-Man (J1).crt');
    });

    it('reapplies favorite after reset to maintain scenario', () => {
      alreadyFavoritedDirectory.removeFavorite('/games/Pac-Man (J1).crt');

      alreadyFavoritedDirectory.reset();

      const favorites = alreadyFavoritedDirectory.getDirectory('/favorites/games');
      expect(favorites.storageItem.files).toHaveLength(1);
      expect(favorites.storageItem.files[0].name).toBe('Pac-Man (J1).crt');
    });
  });

  describe('mixedFavoritesDirectory', () => {
    it('contains favorites across game, music, and image directories', () => {
      const favoriteGames = mixedFavoritesDirectory.getDirectory('/favorites/games');
      const favoriteMusic = mixedFavoritesDirectory.getDirectory('/favorites/music');
      const favoriteImages = mixedFavoritesDirectory.getDirectory('/favorites/images');

      expect(favoriteGames.storageItem.files.map((file) => file.name)).toContain(
        'Donkey Kong (Ocean).crt',
      );
      expect(favoriteMusic.storageItem.files.map((file) => file.name)).toContain('Alpha.sid');
      expect(favoriteImages.storageItem.files.map((file) => file.name)).toContain(
        'SonicTheHedgehog.kla',
      );
    });

    it('restores favorites after reset', () => {
      mixedFavoritesDirectory.removeFavorite('/favorites/games/Donkey Kong (Ocean).crt');
      mixedFavoritesDirectory.removeFavorite('/favorites/music/Alpha.sid');
      mixedFavoritesDirectory.removeFavorite('/favorites/images/SonicTheHedgehog.kla');

      mixedFavoritesDirectory.reset();

      const favoriteGames = mixedFavoritesDirectory.getDirectory('/favorites/games');
      const favoriteMusic = mixedFavoritesDirectory.getDirectory('/favorites/music');
      const favoriteImages = mixedFavoritesDirectory.getDirectory('/favorites/images');

      expect(favoriteGames.storageItem.files.map((file) => file.name)).toContain(
        'Donkey Kong (Ocean).crt',
      );
      expect(favoriteMusic.storageItem.files.map((file) => file.name)).toContain('Alpha.sid');
      expect(favoriteImages.storageItem.files.map((file) => file.name)).toContain(
        'SonicTheHedgehog.kla',
      );
    });
  });
});
