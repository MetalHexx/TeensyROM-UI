import type {
  FileItemDto,
  GetDirectoryResponse,
  RemoveFavoriteResponse,
  SaveFavoriteResponse,
} from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages } from 'cypress/types/net-stubbing';
import {
  INTERCEPT_ALIASES,
  STORAGE_ENDPOINTS,
  createProblemDetailsResponse,
} from '../constants/api.constants';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';
import { generateFileItem } from '../test-data/generators/storage.generators';

export interface InterceptGetDirectoryOptions {
  filesystem?: MockFilesystem;
  errorMode?: boolean;
  responseDelayMs?: number;
}

export interface InterceptSaveFavoriteOptions {
  filesystem?: MockFilesystem;
  favoriteFile?: FileItemDto;
  errorMode?: boolean;
  responseDelayMs?: number;
}

export interface InterceptRemoveFavoriteOptions {
  filesystem?: MockFilesystem;
  errorMode?: boolean;
  responseDelayMs?: number;
}

function resolvePath(queryParam: unknown, fallback = '/'): string {
  if (typeof queryParam === 'string' && queryParam.length > 0) {
    return queryParam;
  }

  if (Array.isArray(queryParam) && queryParam[0]) {
    return queryParam[0];
  }

  return fallback;
}

function getQueryParam(request: CyHttpMessages.IncomingHttpRequest, key: string): unknown {
  const query = (request.query ?? {}) as Record<string, unknown>;
  return query[key];
}

export function interceptGetDirectory(options: InterceptGetDirectoryOptions = {}): void {
  const { filesystem, errorMode = false, responseDelayMs = 0 } = options;

  cy.intercept(
    STORAGE_ENDPOINTS.GET_DIRECTORY.method,
    STORAGE_ENDPOINTS.GET_DIRECTORY.pattern,
    (req) => {
      if (errorMode) {
        req.reply(
          createProblemDetailsResponse(400, 'Failed to load directory. Please try again.'),
        );
        return;
      }

      const directoryPath = resolvePath(getQueryParam(req, 'Path'));
      let response: GetDirectoryResponse;

      if (filesystem) {
        response = filesystem.getDirectory(directoryPath);
      } else {
        response = {
          storageItem: {
            path: directoryPath,
            directories: [],
            files: [],
          },
          message: 'Success',
        };
      }

      req.reply({
        delay: responseDelayMs,
        body: response,
      });
    },
  ).as(INTERCEPT_ALIASES.GET_DIRECTORY);
}

export function interceptSaveFavorite(options: InterceptSaveFavoriteOptions = {}): void {
  const { filesystem, favoriteFile, errorMode = false, responseDelayMs = 0 } = options;

  cy.intercept(
    STORAGE_ENDPOINTS.SAVE_FAVORITE.method,
    STORAGE_ENDPOINTS.SAVE_FAVORITE.pattern,
    (req) => {
      if (errorMode) {
        req.reply(
          createProblemDetailsResponse(502, 'Failed to save favorite. Please try again.'),
        );
        return;
      }

      const filePath = resolvePath(getQueryParam(req, 'FilePath'));
      let response: SaveFavoriteResponse;

      if (filesystem) {
        response = filesystem.saveFavorite(filePath);
      } else {
        const fallbackFavorite = favoriteFile ?? generateFileItem({ path: filePath });
        const favoritesPath = '/favorites/games';
        response = {
          message: `Favorite tagged and saved to ${favoritesPath}`,
          favoriteFile: fallbackFavorite,
          favoritePath: favoritesPath,
        };
      }

      req.reply({
        delay: responseDelayMs,
        body: response,
      });
    },
  ).as(INTERCEPT_ALIASES.SAVE_FAVORITE);
}

export function interceptRemoveFavorite(options: InterceptRemoveFavoriteOptions = {}): void {
  const { filesystem, errorMode = false, responseDelayMs = 0 } = options;

  cy.intercept(
    STORAGE_ENDPOINTS.REMOVE_FAVORITE.method,
    STORAGE_ENDPOINTS.REMOVE_FAVORITE.pattern,
    (req) => {
      if (errorMode) {
        req.reply(
          createProblemDetailsResponse(502, 'Failed to remove favorite. Please try again.'),
        );
        return;
      }

      const filePath = resolvePath(getQueryParam(req, 'FilePath'));
      let response: RemoveFavoriteResponse;

      if (filesystem) {
        response = filesystem.removeFavorite(filePath);
      } else {
        const favoritesPath = '/favorites/games';
        response = {
          message: `Favorite untagged and removed from ${favoritesPath}`,
        };
      }

      req.reply({
        delay: responseDelayMs,
        body: response,
      });
    },
  ).as(INTERCEPT_ALIASES.REMOVE_FAVORITE);
}
