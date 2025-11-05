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
 * Waits for getDirectory endpoint call to complete with enhanced error reporting
 * Uses the registered alias from the interceptor
 */
export function waitForGetDirectory(timeout = 10000): void {
  cy.log(`⏳ Waiting for directory load API call to complete (timeout: ${timeout}ms)`);
  const startTime = Date.now();

  cy.wait(`@${GET_DIRECTORY_ENDPOINT.alias}`, { timeout }).then((xhr) => {
    const elapsedTime = Date.now() - startTime;
    cy.log(`✅ Directory load API call completed in ${formatDuration(elapsedTime)}`);

    // Additional context about the request
    if (xhr?.request) {
      const url = xhr.request.url;
      const method = xhr.request.method;
      cy.log(`📡 Request: ${method} ${url}`);

      // Extract directory path from request for better debugging
      if (url.includes('Path=')) {
        const dirPath = new URL(url).searchParams.get('Path');
        if (dirPath) {
          cy.log(`📁 Directory path: ${decodeURIComponent(dirPath)}`);
        }
      }
    }

    // Log directory contents for debugging
    if (xhr?.response?.body?.storageItem) {
      const { directories, files } = xhr.response.body.storageItem;
      const dirCount = directories?.length || 0;
      const fileCount = files?.length || 0;
      cy.log(`📂 Directory contents: ${dirCount} subdirectories, ${fileCount} files`);

      if (fileCount > 0 && fileCount <= 5) {
        // Log file names if there aren't too many
        const fileNames = files.map((f: FileItemDto) => f.name).join(', ');
        cy.log(`📄 Files: ${fileNames}`);
      }
    }

    // Check for error responses
    if (xhr?.response?.statusCode && xhr.response.statusCode >= 400) {
      const errorMsg = [
        `❌ Directory load API call failed after ${formatDuration(elapsedTime)}`,
        `Status: ${xhr.response.statusCode}`,
        `Response: ${JSON.stringify(xhr.response.body)}`,
        '',
        '💡 This might indicate:',
        '  • Directory not found or does not exist',
        '  • Storage device is not accessible',
        '  • Filesystem permissions issues',
        '  • Network connectivity problems',
        '',
        '🔧 Debugging suggestions:',
        '  • Verify the directory path exists in the mock filesystem',
        '  • Check device connection and storage status',
        '  • Ensure proper filesystem permissions',
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
