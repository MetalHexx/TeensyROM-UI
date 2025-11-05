/// <reference types="cypress" />

import type { FileItemDto, SaveFavoriteResponse } from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages, Method } from 'cypress/types/net-stubbing';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';
import { generateFileItem } from '../test-data/generators/storage.generators';
import {
  interceptSuccess,
  interceptError,
  interceptSequence,
  type EndpointDefinition,
} from './primitives/interceptor-primitives';

export const SAVE_FAVORITE_ENDPOINT: EndpointDefinition = {
  method: 'POST' as Method,
  pattern: 'http://localhost:5168/devices/*/storage/*/favorite*',
  alias: 'saveFavorite',
} as const;

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

/**
 * Intercepts POST /devices/{deviceId}/storage/{storageType}/favorite requests
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
    favoritesPath: customFavoritesPath,
  } = options;

  if (errorMode) {
    interceptError(
      SAVE_FAVORITE_ENDPOINT,
      statusCode || 400,
      errorMessage || 'Failed to save favorite. Please try again.',
      responseDelayMs
    );
    return;
  }

  let response: SaveFavoriteResponse;

  if (filesystem) {
    cy.intercept(SAVE_FAVORITE_ENDPOINT.method as Method, SAVE_FAVORITE_ENDPOINT.pattern, (req) => {
      const filePath = resolvePath(getQueryParam(req, 'FilePath'), fallbackFilePath);
      response = filesystem.saveFavorite(filePath);

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
        delay: responseDelayMs || 0,
      });
    }).as(SAVE_FAVORITE_ENDPOINT.alias);
  } else {
    const filePath = fallbackFilePath || '/';
    const fallbackFavorite = favoriteFile ?? generateFileItem({ path: filePath });
    const favoritesPath = customFavoritesPath || '/favorites/games';

    response = {
      message: `Favorite tagged and saved to ${favoritesPath}`,
      favoriteFile: fallbackFavorite,
      favoritePath: favoritesPath,
    };

    interceptSuccess(SAVE_FAVORITE_ENDPOINT, response, responseDelayMs);
  }
}

export function waitForSaveFavorite(timeout = 10000): void {
  cy.log(`⏳ Waiting for save favorite API call to complete (timeout: ${timeout}ms)`);
  const startTime = Date.now();

  cy.wait(`@${SAVE_FAVORITE_ENDPOINT.alias}`, { timeout }).then((xhr) => {
    const elapsedTime = Date.now() - startTime;
    cy.log(`✅ Save favorite API call completed in ${formatDuration(elapsedTime)}`);

    // Additional context about the request
    if (xhr?.request) {
      const url = xhr.request.url;
      const method = xhr.request.method;
      cy.log(`📡 Request: ${method} ${url}`);

      // Extract file path from request for better debugging
      if (url.includes('FilePath=')) {
        const filePath = new URL(url).searchParams.get('FilePath');
        if (filePath) {
          cy.log(`📁 File path: ${decodeURIComponent(filePath)}`);
        }
      }
    }

    // Check for error responses
    if (xhr?.response?.statusCode && xhr.response.statusCode >= 400) {
      const errorMsg = [
        `❌ Save favorite API call failed after ${formatDuration(elapsedTime)}`,
        `Status: ${xhr.response.statusCode}`,
        `Response: ${JSON.stringify(xhr.response.body)}`,
        '',
        '💡 This might indicate:',
        '  • File already exists in favorites',
        '  • Storage device is not accessible',
        '  • Filesystem permissions issues',
        '  • Network connectivity problems',
        '',
        '🔧 Debugging suggestions:',
        '  • Verify the file exists and is not already favorited',
        '  • Check device connection and storage status',
        '  • Ensure sufficient storage space is available',
      ].join('\n');

      cy.log(`⚠️ ${errorMsg}`);
    }
  });
}

/**
 * Simple duration formatter for logging
 */
function formatDuration(ms: number): string {
  if (ms < 1000) {
    return `${ms}ms`;
  } else if (ms < 60000) {
    return `${(ms / 1000).toFixed(1)}s`;
  } else {
    const minutes = Math.floor(ms / 60000);
    const seconds = Math.floor((ms % 60000) / 1000);
    return `${minutes}m ${seconds}s`;
  }
}

/**
 * Sets up saveFavorite interceptor with filesystem integration
 *
 * @param filesystem Mock filesystem instance
 * @param options Additional interceptor options
 */
export function setupSaveFavorite(
  filesystem: MockFilesystem,
  options: Omit<InterceptSaveFavoriteOptions, 'filesystem'> = {}
): void {
  interceptSaveFavorite({
    ...options,
    filesystem,
  });
}

/**
 * Sets up saveFavorite interceptor with specific file
 *
 * @param favoriteFile File to save as favorite
 * @param options Additional interceptor options
 */
export function setupSaveFavoriteFile(
  favoriteFile: FileItemDto,
  options: InterceptSaveFavoriteOptions = {}
): void {
  interceptSaveFavorite({
    ...options,
    favoriteFile,
  });
}

/**
 * Sets up saveFavorite interceptor with error response
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
 * Sets up saveFavorite interceptor with response delay
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedSaveFavorite(
  delayMs: number,
  options: InterceptSaveFavoriteOptions = {}
): void {
  interceptSaveFavorite({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up saveFavorite interceptor with custom path
 *
 * @param favoritesPath Custom path where favorites should be saved
 * @param options Additional interceptor options
 */
export function setupSaveFavoriteWithPath(
  favoritesPath: string,
  options: InterceptSaveFavoriteOptions = {}
): void {
  interceptSaveFavorite({
    ...options,
    favoritesPath,
  });
}

export function verifySaveFavoriteRequested(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${SAVE_FAVORITE_ENDPOINT.alias}`);
}

export function getLastSaveFavoriteRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${SAVE_FAVORITE_ENDPOINT.alias}`);
}

/**
 * Sets up sequential favorite save responses
 *
 * @param files Array of files to save as favorites in sequence
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupFavoriteSaveSequence(files: FileItemDto[], delayBetweenMs = 1000): void {
  const favoritesPath = '/favorites/games';

  const sequenceResponses = files.map((file) => ({
    message: `Favorite tagged and saved to ${favoritesPath}`,
    favoriteFile: file,
    favoritePath: favoritesPath,
  }));

  interceptSequence(SAVE_FAVORITE_ENDPOINT, sequenceResponses, delayBetweenMs);
}

/**
 * Sets up saveFavorite interceptor for duplicate scenario testing
 *
 * @param file File to test duplicate saving for
 */
export function setupDuplicateFavoriteTest(file: FileItemDto): void {
  const sequenceResponses = [
    {
      message: `Favorite tagged and saved to /favorites/games`,
      favoriteFile: file,
      favoritePath: '/favorites/games',
    },
    {
      message: `Favorite already exists in /favorites/games`,
      favoriteFile: file,
      favoritePath: '/favorites/games',
    },
  ];

  interceptSequence(SAVE_FAVORITE_ENDPOINT, sequenceResponses, 0);
}

// Backward compatibility exports
export const SAVE_FAVORITE_ALIAS = SAVE_FAVORITE_ENDPOINT.alias;
export const INTERCEPT_SAVE_FAVORITE = 'saveFavorite';
export const SAVE_FAVORITE_METHOD = SAVE_FAVORITE_ENDPOINT.method;

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
