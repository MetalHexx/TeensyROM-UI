/// <reference types="cypress" />

import type {
  FileItemDto,
  GetDirectoryResponse,
} from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages } from 'cypress/types/net-stubbing';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';

/**
 * getDirectory endpoint interceptor for directory browsing
 * This file consolidates all getDirectory-related testing functionality
 */

// ============================================================================
// Section 1: Endpoint Definition
// ============================================================================

/**
 * getDirectory endpoint configuration
 */
export const GET_DIRECTORY_ENDPOINT = {
  method: 'GET',
  path: (deviceId: string, storageType: string) => `/devices/${deviceId}/storage/${storageType}/directories`,
  full: (deviceId: string, storageType: string) => `http://localhost:5168/devices/${deviceId}/storage/${storageType}/directories`,
  pattern: 'http://localhost:5168/devices/*/storage/*/directories*',
  alias: 'getDirectory'
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

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

// ============================================================================
// Section 3: Interceptor Function
// ============================================================================

/**
 * Intercepts GET /devices/{deviceId}/storage/{storageType}/directories - Directory browsing endpoint
 * Route matches any deviceId and storageType via wildcard: GET http://localhost:5168/devices/<wildcard>/storage/<wildcard>/directories
 * Supports complex path resolution and mock filesystem integration
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
    customFiles
  } = options;

  cy.intercept(
    GET_DIRECTORY_ENDPOINT.method,
    GET_DIRECTORY_ENDPOINT.pattern,
    (req) => {
      // Apply response delay if specified
      if (responseDelayMs && responseDelayMs > 0) {
        // Note: Cypress doesn't support req.delay() like req.reply({ delay }),
        // so we handle this by using setTimeout in the reply
      }

      if (errorMode) {
        const responseStatusCode = statusCode || 400;
        const responseErrorMessage = errorMessage || 'Failed to load directory. Please try again.';

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

      // Extract directory path from query parameters
      const directoryPath = resolvePath(getQueryParam(req, 'Path'), fallbackPath);
      let response: GetDirectoryResponse;

      // Use provided filesystem or create fallback response
      if (filesystem) {
        response = filesystem.getDirectory(directoryPath);

        // Add custom files if specified
        if (customFiles && customFiles.length > 0) {
          response.storageItem.files = [...response.storageItem.files, ...customFiles];
        }
      } else {
        // Fallback response for tests without filesystem
        response = {
          storageItem: {
            path: directoryPath,
            directories: [],
            files: customFiles || [],
          },
          message: 'Success',
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
  ).as(GET_DIRECTORY_ENDPOINT.alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for getDirectory endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForGetDirectory(): void {
  cy.wait(`@${GET_DIRECTORY_ENDPOINT.alias}`);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

/**
 * Sets up getDirectory interceptor with filesystem for directory browsing tests
 * Useful for testing filesystem navigation scenarios
 *
 * @param filesystem Mock filesystem instance
 * @param options Additional interceptor options
 */
export function setupGetDirectory(filesystem: MockFilesystem, options: Omit<InterceptGetDirectoryOptions, 'filesystem'> = {}): void {
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
export function setupGetDirectoryPath(path: string, options: InterceptGetDirectoryOptions = {}): void {
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
 * Useful for testing directory loading scenarios and timeouts
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedGetDirectory(delayMs: number, options: InterceptGetDirectoryOptions = {}): void {
  interceptGetDirectory({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up getDirectory interceptor with custom files in directory
 * Useful for testing directory content scenarios
 *
 * @param files Array of files to include in directory response
 * @param options Additional interceptor options
 */
export function setupGetDirectoryWithFiles(files: FileItemDto[], options: InterceptGetDirectoryOptions = {}): void {
  interceptGetDirectory({
    ...options,
    customFiles: files,
  });
}

/**
 * Verifies that a getDirectory request was made
 * Useful for validation in tests
 */
export function verifyGetDirectoryRequested(): Cypress.Chainable<any> {
  return cy.get(`@${GET_DIRECTORY_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the getDirectory endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastGetDirectoryRequest(): Cypress.Chainable<any> {
  return cy.get(`@${GET_DIRECTORY_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of directory responses to test navigation scenarios
 * Useful for testing multi-step directory browsing workflows
 *
 * @param paths Array of directory paths to return in sequence
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupDirectoryResponseSequence(paths: string[], delayBetweenMs = 1000): void {
  let currentIndex = 0;

  cy.intercept(
    GET_DIRECTORY_ENDPOINT.method,
    GET_DIRECTORY_ENDPOINT.pattern,
    (req) => {
      const currentPath = paths[currentIndex % paths.length];
      currentIndex++;

      const response: GetDirectoryResponse = {
        storageItem: {
          path: currentPath,
          directories: [],
          files: [],
        },
        message: 'Success',
      };

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
        delay: delayBetweenMs,
      });
    }
  ).as(`${GET_DIRECTORY_ENDPOINT.alias}_sequence`);
}

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const GET_DIRECTORY_ALIAS = GET_DIRECTORY_ENDPOINT.alias;
export const INTERCEPT_GET_DIRECTORY = 'getDirectory';
export const GET_DIRECTORY_METHOD = GET_DIRECTORY_ENDPOINT.method;

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