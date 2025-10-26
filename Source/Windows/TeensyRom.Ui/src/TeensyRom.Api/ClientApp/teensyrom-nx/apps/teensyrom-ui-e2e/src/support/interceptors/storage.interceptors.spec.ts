/// <reference types="cypress" />
import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  interceptGetDirectory,
  interceptRemoveFavorite,
  interceptSaveFavorite,
} from './storage.interceptors';
import { INTERCEPT_ALIASES, STORAGE_ENDPOINTS } from '../constants/api.constants';
import { createMockFilesystem } from '../test-data/generators/storage.generators';

describe('Storage Interceptors', () => {
  let mockIntercept: ReturnType<typeof vi.fn>;
  let mockAs: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    mockAs = vi.fn().mockReturnValue(undefined);
    mockIntercept = vi.fn().mockReturnValue({
      as: mockAs,
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (global as any).cy = {
      intercept: mockIntercept,
    };

    mockIntercept.mockClear();
    mockAs.mockClear();
  });

  describe('interceptGetDirectory', () => {
    it('registers GET intercept with wildcard pattern', () => {
      interceptGetDirectory();

      expect(mockIntercept).toHaveBeenCalledWith(
        STORAGE_ENDPOINTS.GET_DIRECTORY.method,
        STORAGE_ENDPOINTS.GET_DIRECTORY.pattern,
        expect.any(Function),
      );
      expect(mockAs).toHaveBeenCalledWith(INTERCEPT_ALIASES.GET_DIRECTORY);
    });

    it('returns filesystem directory data when provided', () => {
      const filesystem = createMockFilesystem(12345);
      interceptGetDirectory({ filesystem });

      const handler = mockIntercept.mock.calls[0][2];
      const reply = vi.fn();
      handler({
        query: { Path: '/games' },
        reply,
      });

      const responsePayload = reply.mock.calls[0][0];
      expect(responsePayload.body.storageItem.path).toBe('/games');
      expect(responsePayload.body.storageItem.files).toHaveLength(5);
    });

    it('returns ProblemDetails when errorMode enabled', () => {
      interceptGetDirectory({ errorMode: true });
      const handler = mockIntercept.mock.calls[0][2];
      const reply = vi.fn();

      handler({
        query: {},
        reply,
      });

      const responsePayload = reply.mock.calls[0][0];
      expect(responsePayload.statusCode).toBe(400);
      expect(responsePayload.body.title).toBe('Failed to load directory. Please try again.');
    });

    it('applies response delay when configured', () => {
      interceptGetDirectory({ responseDelayMs: 150 });
      const handler = mockIntercept.mock.calls[0][2];
      const reply = vi.fn();

      handler({
        query: {},
        reply,
      });

      expect(reply.mock.calls[0][0].delay).toBe(150);
    });
  });

  describe('interceptSaveFavorite', () => {
    it('registers POST intercept with wildcard pattern', () => {
      interceptSaveFavorite();

      expect(mockIntercept).toHaveBeenCalledWith(
        STORAGE_ENDPOINTS.SAVE_FAVORITE.method,
        STORAGE_ENDPOINTS.SAVE_FAVORITE.pattern,
        expect.any(Function),
      );
      expect(mockAs).toHaveBeenCalledWith(INTERCEPT_ALIASES.SAVE_FAVORITE);
    });

    it('persists favorite using filesystem and returns updated response', () => {
      const filesystem = createMockFilesystem(12345);
      interceptSaveFavorite({ filesystem });

      const handler = mockIntercept.mock.calls[0][2];
      const reply = vi.fn();

      handler({
        query: { FilePath: '/games/Pac-Man (J1).crt' },
        reply,
      });

      const responsePayload = reply.mock.calls[0][0];
      expect(responsePayload.body.message).toBe('Favorite tagged and saved to /favorites/games');

      const favoritesDirectory = filesystem.getDirectory('/favorites/games');
      expect(favoritesDirectory.storageItem.files.map((file) => file.name)).toContain(
        'Pac-Man (J1).crt',
      );
    });

    it('returns error response when errorMode enabled', () => {
      interceptSaveFavorite({ errorMode: true });
      const handler = mockIntercept.mock.calls[0][2];
      const reply = vi.fn();

      handler({
        query: { FilePath: '/games/Pac-Man (J1).crt' },
        reply,
      });

      const responsePayload = reply.mock.calls[0][0];
      expect(responsePayload.statusCode).toBe(502);
      expect(responsePayload.body.title).toBe('Failed to save favorite. Please try again.');
    });
  });

  describe('interceptRemoveFavorite', () => {
    it('registers DELETE intercept with wildcard pattern', () => {
      interceptRemoveFavorite();

      expect(mockIntercept).toHaveBeenCalledWith(
        STORAGE_ENDPOINTS.REMOVE_FAVORITE.method,
        STORAGE_ENDPOINTS.REMOVE_FAVORITE.pattern,
        expect.any(Function),
      );
      expect(mockAs).toHaveBeenCalledWith(INTERCEPT_ALIASES.REMOVE_FAVORITE);
    });

    it('removes favorite using filesystem', () => {
      const filesystem = createMockFilesystem(12345);
      filesystem.saveFavorite('/games/Pac-Man (J1).crt');
      interceptRemoveFavorite({ filesystem });

      const handler = mockIntercept.mock.calls[0][2];
      const reply = vi.fn();

      handler({
        query: { FilePath: '/favorites/games/Pac-Man (J1).crt' },
        reply,
      });

      const favoritesDirectory = filesystem.getDirectory('/favorites/games');
      expect(favoritesDirectory.storageItem.files).toHaveLength(0);
      const responsePayload = reply.mock.calls[0][0];
      expect(responsePayload.body.message).toBe(
        'Favorite untagged and removed from /favorites/games',
      );
    });

    it('returns error response when errorMode enabled', () => {
      interceptRemoveFavorite({ errorMode: true });
      const handler = mockIntercept.mock.calls[0][2];
      const reply = vi.fn();

      handler({
        query: { FilePath: '/favorites/games/Pac-Man (J1).crt' },
        reply,
      });

      const responsePayload = reply.mock.calls[0][0];
      expect(responsePayload.statusCode).toBe(502);
      expect(responsePayload.body.title).toBe('Failed to remove favorite. Please try again.');
    });
  });
});
