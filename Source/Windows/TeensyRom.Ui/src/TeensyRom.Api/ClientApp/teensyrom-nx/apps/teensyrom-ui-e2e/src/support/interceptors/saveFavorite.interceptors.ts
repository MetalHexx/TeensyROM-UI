/// <reference types="cypress" />

import type {
  FileItemDto,
  SaveFavoriteResponse,
} from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages } from 'cypress/types/net-stubbing';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';
import { generateFileItem } from '../test-data/generators/storage.generators';

/**
 * saveFavorite endpoint interceptor for favorite management
 * This file consolidates all saveFavorite-related testing functionality
 */

// ============================================================================
// Section 1: Endpoint Definition
// ============================================================================

/**
 * saveFavorite endpoint configuration
 */
export const SAVE_FAVORITE_ENDPOINT = {
  method: 'POST',
  path: (deviceId: string, storageType: string) => `/devices/${deviceId}/storage/${storageType}/favorite`,
  full: (deviceId: string, storageType: string) => `http://localhost:5168/devices/${deviceId}/storage/${storageType}/favorite`,
  pattern: 'http://localhost:5168/devices/*/storage/*/favorite*',
  alias: 'saveFavorite'
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

/**
 * Options for interceptSaveFavorite interceptor
 */
export interface InterceptSaveFavoriteOptions {
  /** Mock filesystem instance for realistic favorite state management */
  filesystem?: MockFilesystem;
  /** Specific file to save as favorite (for deterministic testing) */
  favoriteFile?: FileItemDto;
  /** When true, return HTTP error to simulate API failure */
  errorMode?: boolean;
  /** Simulate network delay in milliseconds */
  responseDelayMs?: number;
  /** Custom HTTP status code for error responses (default: 502) */
  statusCode?: number;
  /** Custom error message for error responses */
  errorMessage?: string;
  /** Override file path for testing specific scenarios */
  filePath?: string;
  /** Custom favorites path where the favorite should be saved */
  favoritesPath?: string;
}

// ============================================================================
// Section 3: Interceptor Function
// ============================================================================

/**
 * Intercepts POST /devices/{deviceId}/storage/{storageType}/favorite - Save favorite endpoint
 * Route matches any deviceId and storageType via wildcard: POST http://localhost:5168/devices/<wildcard>/storage/<wildcard>/favorite
 * Supports filesystem favorite state management and custom favorite scenarios
 *
 * @param options Configuration options for the interceptor
 */
export function interceptSaveFavorite(options: InterceptSaveFavoriteOptions = {}): void {
  const {
    filesystem,
    favoriteFile,
    errorMode = false,
    responseDelayMs = 0,
    statusCode,
    errorMessage,
    filePath: fallbackFilePath,
    favoritesPath: customFavoritesPath
  } = options;

  cy.intercept(
    SAVE_FAVORITE_ENDPOINT.method,
    SAVE_FAVORITE_ENDPOINT.pattern,
    (req) => {
      // Apply response delay if specified
      if (responseDelayMs && responseDelayMs > 0) {
        // Note: Cypress doesn't support req.delay() like req.reply({ delay }),
        // so we handle this by using setTimeout in the reply
      }

      if (errorMode) {
        const responseStatusCode = statusCode || 400;
        const responseErrorMessage = errorMessage || 'Failed to save favorite. Please try again.';

        req.reply({
          statusCode: responseStatusCode,
          headers: {
            'content-type': 'application/problem+json',
          },
          body: {
            type: `https://tools.ietf.org/html/rfc9110#section-${getRfcSection(responseStatusCode)}`,
            title: getErrorTitle(responseStatusCode),
            status: responseStatusCode,
            detail: responseErrorMessage,
          },
        });
        return;
      }

      // Extract file path from query parameters or request body
      const filePath = resolvePath(getQueryParam(req, 'FilePath'), fallbackFilePath);
      let response: SaveFavoriteResponse;

      // Use provided filesystem or create fallback response
      if (filesystem) {
        response = filesystem.saveFavorite(filePath);
      } else {
        // Fallback response for tests without filesystem
        const fallbackFavorite = favoriteFile ?? generateFileItem({ path: filePath });
        const favoritesPath = customFavoritesPath || '/favorites/games';
        response = {
          message: `Favorite tagged and saved to ${favoritesPath}`,
          favoriteFile: fallbackFavorite,
          favoritePath: favoritesPath,
        };
      }

      if (responseDelayMs && responseDelayMs > 0) {
        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: response,
          delay: responseDelayMs,
        });
      } else {
        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: response,
        });
      }
    }
  ).as(SAVE_FAVORITE_ENDPOINT.alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for saveFavorite endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForSaveFavorite(): void {
  cy.wait(`@${SAVE_FAVORITE_ENDPOINT.alias}`);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

/**
 * Sets up saveFavorite interceptor with filesystem for favorite management tests
 * Useful for testing favorite persistence scenarios
 *
 * @param filesystem Mock filesystem instance
 * @param options Additional interceptor options
 */
export function setupSaveFavorite(filesystem: MockFilesystem, options: Omit<InterceptSaveFavoriteOptions, 'filesystem'> = {}): void {
  interceptSaveFavorite({
    ...options,
    filesystem,
  });
}

/**
 * Sets up saveFavorite interceptor with custom file to favorite
 * Useful for testing specific favorite scenarios
 *
 * @param favoriteFile File to save as favorite
 * @param options Additional interceptor options
 */
export function setupSaveFavoriteFile(favoriteFile: FileItemDto, options: InterceptSaveFavoriteOptions = {}): void {
  interceptSaveFavorite({
    ...options,
    favoriteFile,
  });
}

/**
 * Sets up saveFavorite interceptor with error response
 * Useful for testing favorite save error scenarios
 *
 * @param statusCode HTTP status code for the error (default: 502)
 * @param errorMessage Custom error message
 */
export function setupErrorSaveFavorite(statusCode = 502, errorMessage?: string): void {
  interceptSaveFavorite({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Sets up saveFavorite interceptor with delay for testing loading states
 * Useful for testing favorite save loading scenarios and timeouts
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedSaveFavorite(delayMs: number, options: InterceptSaveFavoriteOptions = {}): void {
  interceptSaveFavorite({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up saveFavorite interceptor with custom favorites path
 * Useful for testing different favorites destination scenarios
 *
 * @param favoritesPath Custom path where favorites should be saved
 * @param options Additional interceptor options
 */
export function setupSaveFavoriteWithPath(favoritesPath: string, options: InterceptSaveFavoriteOptions = {}): void {
  interceptSaveFavorite({
    ...options,
    favoritesPath,
  });
}

/**
 * Verifies that a saveFavorite request was made
 * Useful for validation in tests
 */
export function verifySaveFavoriteRequested(): Cypress.Chainable<any> {
  return cy.get(`@${SAVE_FAVORITE_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the saveFavorite endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastSaveFavoriteRequest(): Cypress.Chainable<any> {
  return cy.get(`@${SAVE_FAVORITE_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of favorite save responses to test multiple saves
 * Useful for testing multi-step favorite management workflows
 *
 * @param files Array of files to save as favorites in sequence
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupFavoriteSaveSequence(files: FileItemDto[], delayBetweenMs = 1000): void {
  let currentIndex = 0;

  cy.intercept(
    SAVE_FAVORITE_ENDPOINT.method,
    SAVE_FAVORITE_ENDPOINT.pattern,
    (req) => {
      const currentFile = files[currentIndex % files.length];
      currentIndex++;

      const favoritesPath = '/favorites/games';
      const response: SaveFavoriteResponse = {
        message: `Favorite tagged and saved to ${favoritesPath}`,
        favoriteFile: currentFile,
        favoritePath: favoritesPath,
      };

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
        delay: delayBetweenMs,
      });
    }
  ).as(`${SAVE_FAVORITE_ENDPOINT.alias}_sequence`);
}

/**
 * Sets up saveFavorite interceptor for testing duplicate favorite scenarios
 * Useful for testing how the system handles saving the same file multiple times
 *
 * @param file File to test duplicate saving for
 * @param options Additional interceptor options
 */
export function setupDuplicateFavoriteTest(file: FileItemDto): void {
  let saveCount = 0;

  cy.intercept(
    SAVE_FAVORITE_ENDPOINT.method,
    SAVE_FAVORITE_ENDPOINT.pattern,
    (req) => {
      saveCount++;

      const response: SaveFavoriteResponse = {
        message: saveCount === 1
          ? `Favorite tagged and saved to /favorites/games`
          : `Favorite already exists in /favorites/games`,
        favoriteFile: file,
        favoritePath: '/favorites/games',
      };

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
      });
    }
  ).as(`${SAVE_FAVORITE_ENDPOINT.alias}_duplicate`);
}

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const SAVE_FAVORITE_ALIAS = SAVE_FAVORITE_ENDPOINT.alias;
export const INTERCEPT_SAVE_FAVORITE = 'saveFavorite';
export const SAVE_FAVORITE_METHOD = SAVE_FAVORITE_ENDPOINT.method;

// Helper functions for path resolution (moved from original implementation)
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

/**
 * Gets the appropriate RFC section for HTTP status codes
 */
function getRfcSection(statusCode: number): string {
  if (statusCode === 400) return '15.5.1';
  if (statusCode === 404) return '15.5.5';
  if (statusCode === 500) return '15.6.1';
  if (statusCode === 502) return '15.6.3';
  return '15.5.5'; // default
}

/**
 * Gets appropriate error title for HTTP status codes
 */
function getErrorTitle(statusCode: number): string {
  switch (statusCode) {
    case 400: return 'Bad Request';
    case 401: return 'Unauthorized';
    case 403: return 'Forbidden';
    case 404: return 'Not Found';
    case 500: return 'Internal Server Error';
    case 502: return 'Bad Gateway';
    case 503: return 'Service Unavailable';
    default: return 'Error';
  }
}