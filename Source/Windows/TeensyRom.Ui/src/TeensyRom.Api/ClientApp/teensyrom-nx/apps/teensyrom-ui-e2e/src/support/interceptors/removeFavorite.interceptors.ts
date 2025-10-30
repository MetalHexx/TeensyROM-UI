/// <reference types="cypress" />

import type {
  RemoveFavoriteResponse,
} from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages } from 'cypress/types/net-stubbing';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';

/**
 * removeFavorite endpoint interceptor for favorite management cleanup
 * This file consolidates all removeFavorite-related testing functionality
 */

// ============================================================================
// Section 1: Endpoint Definition
// ============================================================================

/**
 * removeFavorite endpoint configuration
 */
export const REMOVE_FAVORITE_ENDPOINT = {
  method: 'DELETE',
  path: (deviceId: string, storageType: string) => `/devices/${deviceId}/storage/${storageType}/favorite`,
  full: (deviceId: string, storageType: string) => `http://localhost:5168/devices/${deviceId}/storage/${storageType}/favorite`,
  pattern: 'http://localhost:5168/devices/*/storage/*/favorite*',
  alias: 'removeFavorite'
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

/**
 * Options for interceptRemoveFavorite interceptor
 */
export interface InterceptRemoveFavoriteOptions {
  /** Mock filesystem instance for realistic favorite state management */
  filesystem?: MockFilesystem;
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
  /** Custom favorites path where the favorite should be removed from */
  favoritesPath?: string;
}

// ============================================================================
// Section 3: Interceptor Function
// ============================================================================

/**
 * Intercepts DELETE /devices/{deviceId}/storage/{storageType}/favorite - Remove favorite endpoint
 * Route matches any deviceId and storageType via wildcard: DELETE http://localhost:5168/devices/<wildcard>/storage/<wildcard>/favorite
 * Supports filesystem favorite state management and cleanup scenarios
 *
 * @param options Configuration options for the interceptor
 */
export function interceptRemoveFavorite(options: InterceptRemoveFavoriteOptions = {}): void {
  const {
    filesystem,
    errorMode = false,
    responseDelayMs = 0,
    statusCode,
    errorMessage,
    filePath: fallbackFilePath,
    favoritesPath: customFavoritesPath
  } = options;

  cy.intercept(
    REMOVE_FAVORITE_ENDPOINT.method,
    REMOVE_FAVORITE_ENDPOINT.pattern,
    (req) => {
      // Apply response delay if specified
      if (responseDelayMs && responseDelayMs > 0) {
        // Note: Cypress doesn't support req.delay() like req.reply({ delay }),
        // so we handle this by using setTimeout in the reply
      }

      if (errorMode) {
        const responseStatusCode = statusCode || 400;
        const responseErrorMessage = errorMessage || 'Failed to remove favorite. Please try again.';

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
      let response: RemoveFavoriteResponse;

      // Use provided filesystem or create fallback response
      if (filesystem) {
        response = filesystem.removeFavorite(filePath);
      } else {
        // Fallback response for tests without filesystem
        const favoritesPath = customFavoritesPath || '/favorites/games';
        response = {
          message: `Favorite untagged and removed from ${favoritesPath}`,
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
  ).as(REMOVE_FAVORITE_ENDPOINT.alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for removeFavorite endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForRemoveFavorite(): void {
  cy.wait(`@${REMOVE_FAVORITE_ENDPOINT.alias}`);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

/**
 * Sets up removeFavorite interceptor with filesystem for favorite cleanup tests
 * Useful for testing favorite removal scenarios
 *
 * @param filesystem Mock filesystem instance
 * @param options Additional interceptor options
 */
export function setupRemoveFavorite(filesystem: MockFilesystem, options: Omit<InterceptRemoveFavoriteOptions, 'filesystem'> = {}): void {
  interceptRemoveFavorite({
    ...options,
    filesystem,
  });
}

/**
 * Sets up removeFavorite interceptor with specific file path to remove
 * Useful for testing specific favorite removal scenarios
 *
 * @param filePath Path of file to remove from favorites
 * @param options Additional interceptor options
 */
export function setupRemoveFavoritePath(filePath: string, options: InterceptRemoveFavoriteOptions = {}): void {
  interceptRemoveFavorite({
    ...options,
    filePath,
  });
}

/**
 * Sets up removeFavorite interceptor with error response
 * Useful for testing favorite removal error scenarios
 *
 * @param statusCode HTTP status code for the error (default: 502)
 * @param errorMessage Custom error message
 */
export function setupErrorRemoveFavorite(statusCode = 502, errorMessage?: string): void {
  interceptRemoveFavorite({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Sets up removeFavorite interceptor with delay for testing loading states
 * Useful for testing favorite removal loading scenarios and timeouts
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedRemoveFavorite(delayMs: number, options: InterceptRemoveFavoriteOptions = {}): void {
  interceptRemoveFavorite({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up removeFavorite interceptor with custom favorites path
 * Useful for testing different favorites source scenarios
 *
 * @param favoritesPath Custom path where favorites should be removed from
 * @param options Additional interceptor options
 */
export function setupRemoveFavoriteFromPath(favoritesPath: string, options: InterceptRemoveFavoriteOptions = {}): void {
  interceptRemoveFavorite({
    ...options,
    favoritesPath,
  });
}

/**
 * Verifies that a removeFavorite request was made
 * Useful for validation in tests
 */
export function verifyRemoveFavoriteRequested(): Cypress.Chainable<any> {
  return cy.get(`@${REMOVE_FAVORITE_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the removeFavorite endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastRemoveFavoriteRequest(): Cypress.Chainable<any> {
  return cy.get(`@${REMOVE_FAVORITE_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of favorite removal responses to test multiple removals
 * Useful for testing multi-step favorite cleanup workflows
 *
 * @param paths Array of file paths to remove from favorites in sequence
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupFavoriteRemovalSequence(paths: string[], delayBetweenMs = 1000): void {
  cy.intercept(
    REMOVE_FAVORITE_ENDPOINT.method,
    REMOVE_FAVORITE_ENDPOINT.pattern,
    (req) => {
      const response: RemoveFavoriteResponse = {
        message: `Favorite untagged and removed from /favorites/games`,
      };

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
        delay: delayBetweenMs,
      });
    }
  ).as(`${REMOVE_FAVORITE_ENDPOINT.alias}_sequence`);
}

/**
 * Sets up removeFavorite interceptor for testing non-existent favorite scenarios
 * Useful for testing how the system handles removing favorites that don't exist
 *
 * @param filePath File path that doesn't exist in favorites
 * @param options Additional interceptor options
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
export function setupNonExistentFavoriteTest(filePath: string): void {
  cy.intercept(
    REMOVE_FAVORITE_ENDPOINT.method,
    REMOVE_FAVORITE_ENDPOINT.pattern,
    (req) => {
      const response: RemoveFavoriteResponse = {
        message: `Favorite not found in /favorites/games - nothing to remove`,
      };

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
      });
    }
  ).as(`${REMOVE_FAVORITE_ENDPOINT.alias}_nonexistent`);
}

/**
 * Sets up removeFavorite interceptor for testing cleanup after batch operations
 * Useful for testing scenarios where multiple favorites need to be cleaned up
 *
 * @param cleanupCount Number of favorites to clean up
 * @param options Additional interceptor options
 */
export function setupBatchFavoriteCleanup(cleanupCount: number): void {
  let removedCount = 0;

  cy.intercept(
    REMOVE_FAVORITE_ENDPOINT.method,
    REMOVE_FAVORITE_ENDPOINT.pattern,
    (req) => {
      removedCount++;

      const response: RemoveFavoriteResponse = {
        message: `Favorite untagged and removed from /favorites/games (${removedCount}/${cleanupCount})`,
      };

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
      });
    }
  ).as(`${REMOVE_FAVORITE_ENDPOINT.alias}_batch`);
}

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const REMOVE_FAVORITE_ALIAS = REMOVE_FAVORITE_ENDPOINT.alias;
export const INTERCEPT_REMOVE_FAVORITE = 'removeFavorite';
export const REMOVE_FAVORITE_METHOD = REMOVE_FAVORITE_ENDPOINT.method;

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