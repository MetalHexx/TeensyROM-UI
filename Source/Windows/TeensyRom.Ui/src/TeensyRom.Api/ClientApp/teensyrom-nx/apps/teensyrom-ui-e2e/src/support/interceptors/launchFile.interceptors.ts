/// <reference types="cypress" />

import type { LaunchFileResponse, FileItemDto } from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages, Method } from 'cypress/types/net-stubbing';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';
import { generateFileItem } from '../test-data/generators/storage.generators';
import { TEST_FILES, TEST_PATHS } from '../constants/storage.constants';
import {
  interceptError,
  interceptSequence,
  type EndpointDefinition,
} from './primitives/interceptor-primitives';

/**
 * launchFile endpoint interceptor for file launch operations
 * Migrated to primitive-based architecture for simplified maintenance
 * Uses custom cy.intercept() logic for file path extraction from requests
 */
export const LAUNCH_FILE_ENDPOINT: EndpointDefinition = {
  method: 'POST' as Method,
  pattern: 'http://localhost:5168/devices/*/storage/*/launch*',
  alias: 'launchFile',
} as const;

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

/**
 * Intercepts POST /devices/{deviceId}/storage/{storageType}/launch - File launch endpoint
 * Matches any deviceId and storageType via wildcard pattern
 * Supports filesystem file lookups and custom launch scenarios
 * Uses primitive functions for simplified implementation
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
    overrideFile,
  } = options;

  if (errorMode) {
    interceptError(
      LAUNCH_FILE_ENDPOINT,
      statusCode || 500,
      errorMessage || 'Failed to launch file',
      responseDelayMs
    );
    return;
  }

  // Note: Custom cy.intercept() logic needed for file path extraction from requests
  // Primitives don't provide request access for extracting FilePath parameter
  cy.intercept(LAUNCH_FILE_ENDPOINT.method as Method, LAUNCH_FILE_ENDPOINT.pattern, (req) => {
    const filePath = resolvePath(getQueryParam(req, 'FilePath'), fallbackFilePath);
    let launchedFile;

    if (overrideFile) {
      launchedFile = overrideFile;
    } else if (filesystem) {
      const dirPath =
        filePath.substring(0, filePath.lastIndexOf(TEST_PATHS.SEPARATOR)) || TEST_PATHS.ROOT;
      const fileName = filePath.substring(filePath.lastIndexOf(TEST_PATHS.SEPARATOR) + 1);
      const directoryResponse = filesystem.getDirectory(dirPath);

      const file = directoryResponse.storageItem.files?.find((f) => f.name === fileName);
      if (file) {
        launchedFile = file;
      } else {
        launchedFile = generateFileItem({
          path: filePath,
          name: fileName || TEST_FILES.DEFAULT_UNKNOWN_FILE,
        });
      }
    } else {
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

    req.reply({
      statusCode: 200,
      headers: { 'content-type': 'application/json' },
      body: response,
      delay: responseDelayMs || 0,
    });
  }).as(LAUNCH_FILE_ENDPOINT.alias);
}

/**
 * Waits for launchFile endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForLaunchFile(): void {
  cy.wait(`@${LAUNCH_FILE_ENDPOINT.alias}`);
}

/**
 * Sets up launchFile interceptor with filesystem for realistic file launch tests
 *
 * @param filesystem Mock filesystem instance
 * @param options Additional interceptor options
 */
export function setupLaunchFile(
  filesystem: MockFilesystem,
  options: Omit<InterceptLaunchFileOptions, 'filesystem'> = {}
): void {
  interceptLaunchFile({
    ...options,
    filesystem,
  });
}

/**
 * Sets up launchFile interceptor with custom file to launch for specific scenarios
 *
 * @param file File to launch
 * @param options Additional interceptor options
 */
export function setupLaunchFileWithFile(
  file: FileItemDto,
  options: InterceptLaunchFileOptions = {}
): void {
  interceptLaunchFile({
    ...options,
    overrideFile: file,
  });
}

/**
 * Sets up launchFile interceptor with specific file path for path-specific tests
 *
 * @param filePath Path of file to launch
 * @param options Additional interceptor options
 */
export function setupLaunchFileWithPath(
  filePath: string,
  options: InterceptLaunchFileOptions = {}
): void {
  interceptLaunchFile({
    ...options,
    filePath,
  });
}

/**
 * Sets up launchFile interceptor with error response for error scenario tests
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
 * Sets up launchFile interceptor with delay for testing loading states and timeouts
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedLaunchFile(
  delayMs: number,
  options: InterceptLaunchFileOptions = {}
): void {
  interceptLaunchFile({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Verifies that a launchFile request was made for test validation
 */
export function verifyLaunchFileRequested(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${LAUNCH_FILE_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the launchFile endpoint for parameter verification
 */
export function getLastLaunchFileRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${LAUNCH_FILE_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of file launch responses for testing multi-step launch workflows
 * Uses interceptSequence primitive for simplified sequential response handling
 *
 * @param files Array of files to launch in sequence
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupLaunchFileSequence(files: FileItemDto[], delayBetweenMs = 1000): void {
  const sequenceResponses = files.map((file) => ({
    message: `Successfully launched ${file.name}`,
    launchedFile: file,
    isCompatible: file.isCompatible ?? true,
  }));

  interceptSequence(LAUNCH_FILE_ENDPOINT, sequenceResponses, delayBetweenMs);
}

/**
 * Sets up launchFile interceptor for testing incompatible file scenarios
 *
 * @param file File to launch as incompatible
 * @param options Additional interceptor options
 */
export function setupIncompatibleLaunchFile(
  file: FileItemDto,
  options: InterceptLaunchFileOptions = {}
): void {
  const incompatibleFile = { ...file, isCompatible: false };

  interceptLaunchFile({
    ...options,
    overrideFile: incompatibleFile,
  });
}

// Export Constants (Backward Compatibility)

export const LAUNCH_FILE_ALIAS = LAUNCH_FILE_ENDPOINT.alias;
export const INTERCEPT_LAUNCH_FILE = 'launchFile';
export const LAUNCH_FILE_METHOD = LAUNCH_FILE_ENDPOINT.method;

// Helper functions for path resolution
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

// Note: This interceptor requires custom cy.intercept() implementation for file path
// extraction from requests, which cannot be handled by primitives
