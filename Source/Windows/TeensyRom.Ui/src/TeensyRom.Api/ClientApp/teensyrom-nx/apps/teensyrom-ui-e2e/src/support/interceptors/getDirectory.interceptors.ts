/// <reference types="cypress" />

import type { FileItemDto, GetDirectoryResponse } from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages, Method } from 'cypress/types/net-stubbing';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';
import {
  interceptSuccess,
  interceptError,
  interceptSequence,
  type EndpointDefinition,
} from './primitives/interceptor-primitives';

/**
 * getDirectory endpoint interceptor for directory browsing
 * Migrated to primitive-based architecture for simplified maintenance
 */

// ============================================================================
// ENDPOINT DEFINITION
// ============================================================================

/**
 * getDirectory endpoint configuration
 */
export const GET_DIRECTORY_ENDPOINT: EndpointDefinition = {
  method: 'GET' as Method,
  pattern: 'http://localhost:5168/devices/*/storage/*/directories*',
  alias: 'getDirectory',
};

/**
 * Options for interceptGetDirectory interceptor
 */
export interface InterceptGetDirectoryOptions {
  /** Mock filesystem instance for realistic directory structure testing */
  filesystem?: MockFilesystem;
  /** When true, return HTTP error to simulate API failure */
  errorMode?: boolean;
  /** Simulate network delay in milliseconds */
  responseDelayMs?: number;
  /** Custom HTTP status code for error responses (default: 400) */
  statusCode?: number;
  /** Custom error message for error responses */
  errorMessage?: string;
  /** Override directory path for testing specific scenarios */
  path?: string;
  /** Custom files to include in directory response */
  customFiles?: FileItemDto[];
}

/**
 * Intercepts GET /devices/{deviceId}/storage/{storageType}/directories
 * Route matches any deviceId and storageType via wildcard pattern
 * Supports path resolution and mock filesystem integration
 * Uses primitive functions for simplified implementation and RFC 9110 compliance
 *
 * @param options Configuration options for the interceptor
 */
export function interceptGetDirectory(options: InterceptGetDirectoryOptions = {}): void {
  const {
    filesystem,
    errorMode = false,
    responseDelayMs = 0,
    statusCode,
    errorMessage,
    path: fallbackPath,
    customFiles,
  } = options;

  if (errorMode) {
    interceptError(
      GET_DIRECTORY_ENDPOINT,
      statusCode || 400,
      errorMessage || 'Failed to load directory. Please try again.',
      responseDelayMs
    );
    return;
  }

  let response: GetDirectoryResponse;

  if (filesystem) {
    cy.intercept(GET_DIRECTORY_ENDPOINT.method as Method, GET_DIRECTORY_ENDPOINT.pattern, (req) => {
      const directoryPath = resolvePath(getQueryParam(req, 'Path'), fallbackPath);
      response = filesystem.getDirectory(directoryPath);

      if (customFiles && customFiles.length > 0) {
        response.storageItem.files = [...response.storageItem.files, ...customFiles];
      }

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
        delay: responseDelayMs || 0,
      });
    }).as(GET_DIRECTORY_ENDPOINT.alias);
  } else {
    const directoryPath = fallbackPath || '/';
    response = {
      storageItem: {
        path: directoryPath,
        directories: [],
        files: customFiles || [],
      },
      message: 'Success',
    };

    interceptSuccess(GET_DIRECTORY_ENDPOINT, response, responseDelayMs);
  }
}

// ============================================================================
// WAIT FUNCTIONS
// ============================================================================

/**
 * Waits for getDirectory endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForGetDirectory(): void {
  cy.wait(`@${GET_DIRECTORY_ENDPOINT.alias}`);
}

/**
 * Sets up getDirectory interceptor with filesystem for directory browsing tests
 *
 * @param filesystem Mock filesystem instance
 * @param options Additional interceptor options
 */
export function setupGetDirectory(
  filesystem: MockFilesystem,
  options: Omit<InterceptGetDirectoryOptions, 'filesystem'> = {}
): void {
  interceptGetDirectory({
    ...options,
    filesystem,
  });
}

/**
 * Sets up getDirectory interceptor with custom directory path
 * Useful for testing specific directory scenarios
 *
 * @param path Directory path to return
 * @param options Additional interceptor options
 */
export function setupGetDirectoryPath(
  path: string,
  options: InterceptGetDirectoryOptions = {}
): void {
  interceptGetDirectory({
    ...options,
    path,
  });
}

/**
 * Sets up getDirectory interceptor with error response
 * Useful for testing directory browsing error scenarios
 *
 * @param statusCode HTTP status code for the error (default: 400)
 * @param errorMessage Custom error message
 */
export function setupErrorGetDirectory(statusCode = 400, errorMessage?: string): void {
  interceptGetDirectory({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Sets up getDirectory interceptor with delay for testing loading states
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedGetDirectory(
  delayMs: number,
  options: InterceptGetDirectoryOptions = {}
): void {
  interceptGetDirectory({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up getDirectory interceptor with custom files in directory
 *
 * @param files Array of files to include in directory response
 * @param options Additional interceptor options
 */
export function setupGetDirectoryWithFiles(
  files: FileItemDto[],
  options: InterceptGetDirectoryOptions = {}
): void {
  interceptGetDirectory({
    ...options,
    customFiles: files,
  });
}

/**
 * Verifies that a getDirectory request was made
 */
export function verifyGetDirectoryRequested(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${GET_DIRECTORY_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the getDirectory endpoint
 */
export function getLastGetDirectoryRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${GET_DIRECTORY_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of directory responses to test navigation scenarios
 *
 * @param paths Array of directory paths to return in sequence
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupDirectoryResponseSequence(paths: string[], delayBetweenMs = 1000): void {
  const sequenceResponses = paths.map((path) => ({
    storageItem: {
      path,
      directories: [],
      files: [],
    },
    message: 'Success',
  }));

  interceptSequence(GET_DIRECTORY_ENDPOINT, sequenceResponses, delayBetweenMs);
}

// Backward compatibility exports
export const GET_DIRECTORY_ALIAS = GET_DIRECTORY_ENDPOINT.alias;
export const INTERCEPT_GET_DIRECTORY = 'getDirectory';
export const GET_DIRECTORY_METHOD = GET_DIRECTORY_ENDPOINT.method;

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
