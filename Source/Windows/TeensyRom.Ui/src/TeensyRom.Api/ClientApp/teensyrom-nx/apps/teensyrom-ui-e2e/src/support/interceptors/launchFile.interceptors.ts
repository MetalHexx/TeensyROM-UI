/// <reference types="cypress" />

import type {
  LaunchFileResponse,
  FileItemDto,
} from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages } from 'cypress/types/net-stubbing';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';
import { generateFileItem } from '../test-data/generators/storage.generators';
import { TEST_FILES, TEST_PATHS } from '../constants/storage.constants';

/**
 * launchFile endpoint interceptor for file launch operations
 * This file consolidates all launchFile-related testing functionality
 */

// ============================================================================
// Section 1: Endpoint Definition
// ============================================================================

/**
 * launchFile endpoint configuration
 */
export const LAUNCH_FILE_ENDPOINT = {
  method: 'POST',
  path: (deviceId: string, storageType: string) => `/devices/${deviceId}/storage/${storageType}/launch`,
  full: (deviceId: string, storageType: string) => `http://localhost:5168/devices/${deviceId}/storage/${storageType}/launch`,
  pattern: 'http://localhost:5168/devices/*/storage/*/launch*',
  alias: 'launchFile'
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

/**
 * Options for interceptLaunchFile interceptor
 */
export interface InterceptLaunchFileOptions {
  /** Mock filesystem instance for realistic file lookups and launches */
  filesystem?: MockFilesystem;
  /** When true, return HTTP error to simulate launch failure */
  errorMode?: boolean;
  /** Simulate network delay in milliseconds */
  responseDelayMs?: number;
  /** Custom HTTP status code for error responses (default: 500) */
  statusCode?: number;
  /** Custom error message for error responses */
  errorMessage?: string;
  /** Override file path for testing specific scenarios */
  filePath?: string;
  /** Custom file to launch instead of filesystem lookup */
  overrideFile?: FileItemDto;
}

// ============================================================================
// Section 3: Interceptor Function
// ============================================================================

/**
 * Intercepts POST /devices/{deviceId}/storage/{storageType}/launch - File launch endpoint
 * Route matches any deviceId and storageType via wildcard: POST http://localhost:5168/devices/<wildcard>/storage/<wildcard>/launch
 * Supports filesystem file lookups and custom launch scenarios
 *
 * @param options Configuration options for the interceptor
 */
export function interceptLaunchFile(options: InterceptLaunchFileOptions = {}): void {
  const {
    filesystem,
    errorMode = false,
    responseDelayMs = 0,
    statusCode,
    errorMessage,
    filePath: fallbackFilePath,
    overrideFile
  } = options;

  cy.intercept(
    LAUNCH_FILE_ENDPOINT.method,
    LAUNCH_FILE_ENDPOINT.pattern,
    (req) => {
      // Apply response delay if specified
      if (responseDelayMs && responseDelayMs > 0) {
        // Note: Cypress doesn't support req.delay() like req.reply({ delay }),
        // so we handle this by using setTimeout in the reply
      }

      if (errorMode) {
        const responseStatusCode = statusCode || 500;
        const responseErrorMessage = errorMessage || 'Failed to launch file';

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

      // Extract file path from query parameters
      const filePath = resolvePath(getQueryParam(req, 'FilePath'), fallbackFilePath);
      let launchedFile;

      // Use provided override file first (for deterministic testing)
      if (overrideFile) {
        launchedFile = overrideFile;
      } else if (filesystem) {
        // Look up file from the filesystem by getting the directory and searching for the file
        const dirPath = filePath.substring(0, filePath.lastIndexOf(TEST_PATHS.SEPARATOR)) || TEST_PATHS.ROOT;
        const fileName = filePath.substring(filePath.lastIndexOf(TEST_PATHS.SEPARATOR) + 1);
        const directoryResponse = filesystem.getDirectory(dirPath);

        const file = directoryResponse.storageItem.files?.find((f) => f.name === fileName);
        if (file) {
          launchedFile = file;
        } else {
          // Fallback if file not found in filesystem
          launchedFile = generateFileItem({
            path: filePath,
            name: fileName || TEST_FILES.DEFAULT_UNKNOWN_FILE,
          });
        }
      } else {
        // Generate a default file item if no filesystem provided
        launchedFile = generateFileItem({
          path: filePath,
          name: filePath.split(TEST_PATHS.SEPARATOR).pop() || TEST_FILES.DEFAULT_UNKNOWN_FILE,
        });
      }

      const response: LaunchFileResponse = {
        message: `Successfully launched ${launchedFile.name}`,
        launchedFile,
        isCompatible: launchedFile.isCompatible ?? true,
      };

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
  ).as(LAUNCH_FILE_ENDPOINT.alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for launchFile endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForLaunchFile(): void {
  cy.wait(`@${LAUNCH_FILE_ENDPOINT.alias}`);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

/**
 * Sets up launchFile interceptor with filesystem for file launch tests
 * Useful for testing realistic file launch scenarios
 *
 * @param filesystem Mock filesystem instance
 * @param options Additional interceptor options
 */
export function setupLaunchFile(filesystem: MockFilesystem, options: Omit<InterceptLaunchFileOptions, 'filesystem'> = {}): void {
  interceptLaunchFile({
    ...options,
    filesystem,
  });
}

/**
 * Sets up launchFile interceptor with custom file to launch
 * Useful for testing specific launch scenarios
 *
 * @param file File to launch
 * @param options Additional interceptor options
 */
export function setupLaunchFileWithFile(file: FileItemDto, options: InterceptLaunchFileOptions = {}): void {
  interceptLaunchFile({
    ...options,
    overrideFile: file,
  });
}

/**
 * Sets up launchFile interceptor with specific file path
 * Useful for testing specific file path scenarios
 *
 * @param filePath Path of file to launch
 * @param options Additional interceptor options
 */
export function setupLaunchFileWithPath(filePath: string, options: InterceptLaunchFileOptions = {}): void {
  interceptLaunchFile({
    ...options,
    filePath,
  });
}

/**
 * Sets up launchFile interceptor with error response
 * Useful for testing launch error scenarios
 *
 * @param statusCode HTTP status code for the error (default: 500)
 * @param errorMessage Custom error message
 */
export function setupErrorLaunchFile(statusCode = 500, errorMessage?: string): void {
  interceptLaunchFile({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Sets up launchFile interceptor with delay for testing loading states
 * Useful for testing launch loading scenarios and timeouts
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedLaunchFile(delayMs: number, options: InterceptLaunchFileOptions = {}): void {
  interceptLaunchFile({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Verifies that a launchFile request was made
 * Useful for validation in tests
 */
export function verifyLaunchFileRequested(): Cypress.Chainable<any> {
  return cy.get(`@${LAUNCH_FILE_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the launchFile endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastLaunchFileRequest(): Cypress.Chainable<any> {
  return cy.get(`@${LAUNCH_FILE_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of file launch responses to test multiple launches
 * Useful for testing multi-step file launch workflows
 *
 * @param files Array of files to launch in sequence
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupLaunchFileSequence(files: FileItemDto[], delayBetweenMs = 1000): void {
  let currentIndex = 0;

  cy.intercept(
    LAUNCH_FILE_ENDPOINT.method,
    LAUNCH_FILE_ENDPOINT.pattern,
    (req) => {
      const currentFile = files[currentIndex % files.length];
      currentIndex++;

      const response: LaunchFileResponse = {
        message: `Successfully launched ${currentFile.name}`,
        launchedFile: currentFile,
        isCompatible: currentFile.isCompatible ?? true,
      };

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
        delay: delayBetweenMs,
      });
    }
  ).as(`${LAUNCH_FILE_ENDPOINT.alias}_sequence`);
}

/**
 * Sets up launchFile interceptor for testing incompatible file scenarios
 * Useful for testing how the system handles incompatible files
 *
 * @param file File to launch as incompatible
 * @param options Additional interceptor options
 */
export function setupIncompatibleLaunchFile(file: FileItemDto, options: InterceptLaunchFileOptions = {}): void {
  const incompatibleFile = { ...file, isCompatible: false };

  interceptLaunchFile({
    ...options,
    overrideFile: incompatibleFile,
  });
}

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const LAUNCH_FILE_ALIAS = LAUNCH_FILE_ENDPOINT.alias;
export const INTERCEPT_LAUNCH_FILE = 'launchFile';
export const LAUNCH_FILE_METHOD = LAUNCH_FILE_ENDPOINT.method;

// Helper functions for path resolution (moved from original implementation)
function resolvePath(queryParam: unknown, fallback = '/'): string {
  if (typeof queryParam === 'string' && queryParam.length > 0) {
    return decodeValue(queryParam);
  }

  if (Array.isArray(queryParam) && queryParam[0]) {
    return decodeValue(queryParam[0]);
  }

  return fallback;
}

function decodeValue(value: string): string {
  try {
    return decodeURIComponent(value);
  } catch {
    return value;
  }
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