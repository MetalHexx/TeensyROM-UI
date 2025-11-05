/// <reference types="cypress" />

import type { RemoveFavoriteResponse } from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages, Method } from 'cypress/types/net-stubbing';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';
import {
  interceptSuccess,
  interceptError,
  interceptSequence,
  type EndpointDefinition,
  type CypressRequest,
} from './primitives/interceptor-primitives';

/**
 * removeFavorite endpoint interceptor for favorite management
 * Uses primitive-based architecture for simplified maintenance
 */

// ============================================================================
// Endpoint Definition
// ============================================================================

export const REMOVE_FAVORITE_ENDPOINT: EndpointDefinition = {
  method: 'DELETE' as Method,
  pattern: 'http://localhost:5168/devices/*/storage/*/favorite*',
  alias: 'removeFavorite',
} as const;

// ============================================================================
// Interface Definitions
// ============================================================================

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
// Interceptor Function
// ============================================================================

/**
 * Intercepts DELETE /devices/{deviceId}/storage/{storageType}/favorite - Remove favorite endpoint
 * Route matches any deviceId and storageType via wildcard: DELETE http://localhost:5168/devices/<wildcard>/storage/<wildcard>/favorite
 * Supports filesystem favorite state management and cleanup scenarios
 * Uses primitive functions for simplified implementation and RFC 9110 compliance
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
    favoritesPath: customFavoritesPath,
  } = options;

  if (errorMode) {
    interceptError(
      REMOVE_FAVORITE_ENDPOINT,
      statusCode || 400,
      errorMessage || 'Failed to remove favorite. Please try again.',
      responseDelayMs
    );
    return;
  }

  let response: RemoveFavoriteResponse;

  if (filesystem) {
    cy.intercept(
      REMOVE_FAVORITE_ENDPOINT.method as Method,
      REMOVE_FAVORITE_ENDPOINT.pattern,
      (req) => {
        const filePath = resolvePath(getQueryParam(req, 'FilePath'), fallbackFilePath);
        response = filesystem.removeFavorite(filePath);

        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: response,
          delay: responseDelayMs || 0,
        });
      }
    ).as(REMOVE_FAVORITE_ENDPOINT.alias);
  } else {
    const favoritesPath = customFavoritesPath || '/favorites/games';
    response = {
      message: `Favorite untagged and removed from ${favoritesPath}`,
    };

    interceptSuccess(REMOVE_FAVORITE_ENDPOINT, response, responseDelayMs);
  }
}

// ============================================================================
// Wait Function
// ============================================================================

/**
 * Waits for removeFavorite endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForRemoveFavorite(): void {
  cy.wait(`@${REMOVE_FAVORITE_ENDPOINT.alias}`);
}

// ============================================================================
// Helper Functions
// ============================================================================

/**
 * Sets up removeFavorite interceptor with filesystem for favorite cleanup tests
 *
 * @param filesystem Mock filesystem instance
 * @param options Additional interceptor options
 */
export function setupRemoveFavorite(
  filesystem: MockFilesystem,
  options: Omit<InterceptRemoveFavoriteOptions, 'filesystem'> = {}
): void {
  interceptRemoveFavorite({
    ...options,
    filesystem,
  });
}

/**
 * Sets up removeFavorite interceptor with specific file path to remove
 *
 * @param filePath Path of file to remove from favorites
 * @param options Additional interceptor options
 */
export function setupRemoveFavoritePath(
  filePath: string,
  options: InterceptRemoveFavoriteOptions = {}
): void {
  interceptRemoveFavorite({
    ...options,
    filePath,
  });
}

/**
 * Sets up removeFavorite interceptor with error response
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
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedRemoveFavorite(
  delayMs: number,
  options: InterceptRemoveFavoriteOptions = {}
): void {
  interceptRemoveFavorite({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up removeFavorite interceptor with custom favorites path
 *
 * @param favoritesPath Custom path where favorites should be removed from
 * @param options Additional interceptor options
 */
export function setupRemoveFavoriteFromPath(
  favoritesPath: string,
  options: InterceptRemoveFavoriteOptions = {}
): void {
  interceptRemoveFavorite({
    ...options,
    favoritesPath,
  });
}

/**
 * Verifies that a removeFavorite request was made
 * Useful for validation in tests
 */
export function verifyRemoveFavoriteRequested(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${REMOVE_FAVORITE_ENDPOINT.alias}`);
}

export function getLastRemoveFavoriteRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${REMOVE_FAVORITE_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of favorite removal responses to test multiple removals
 *
 * @param paths Array of file paths to remove from favorites in sequence
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupFavoriteRemovalSequence(paths: string[], delayBetweenMs = 1000): void {
  const sequenceResponses = paths.map(() => ({
    message: `Favorite untagged and removed from /favorites/games`,
  }));

  interceptSequence(REMOVE_FAVORITE_ENDPOINT, sequenceResponses, delayBetweenMs);
}

/**
 * Sets up removeFavorite interceptor for testing non-existent favorite scenarios
 *
 * @param filePath File path that doesn't exist in favorites
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
export function setupNonExistentFavoriteTest(filePath: string): void {
  cy.intercept(
    REMOVE_FAVORITE_ENDPOINT.method as Method,
    REMOVE_FAVORITE_ENDPOINT.pattern,
    (req: CypressRequest) => {
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
 *
 * @param cleanupCount Number of favorites to clean up
 */
export function setupBatchFavoriteCleanup(cleanupCount: number): void {
  let removedCount = 0;

  cy.intercept(
    REMOVE_FAVORITE_ENDPOINT.method as Method,
    REMOVE_FAVORITE_ENDPOINT.pattern,
    (req: CypressRequest) => {
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

export const REMOVE_FAVORITE_ALIAS = REMOVE_FAVORITE_ENDPOINT.alias;
export const INTERCEPT_REMOVE_FAVORITE = 'removeFavorite';
export const REMOVE_FAVORITE_METHOD = REMOVE_FAVORITE_ENDPOINT.method;

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
