/// <reference types="cypress" />

import type { LaunchRandomResponse, FileItemDto } from '@teensyrom-nx/data-access/api-client';
import type { CyHttpMessages, Method } from 'cypress/types/net-stubbing';
import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';
import { generateFileItem } from '../test-data/generators/storage.generators';
import { interceptError, type EndpointDefinition } from './primitives/interceptor-primitives';

/**
 * launchRandom endpoint interceptor for random file launch operations
 * Migrated to primitive-based architecture for simplified maintenance
 */

export const LAUNCH_RANDOM_ENDPOINT: EndpointDefinition = {
  method: 'POST' as Method,
  pattern: 'http://localhost:5168/devices/*/storage/*/random-launch*',
  alias: 'launchRandom',
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

/**
 * Options for interceptLaunchRandom interceptor
 */
export interface InterceptLaunchRandomOptions {
  /** Mock filesystem instance for realistic random file selection */
  filesystem?: MockFilesystem;
  /** Specific file to return instead of random selection (for deterministic testing) */
  selectedFile?: FileItemDto;
  /** When true, return HTTP error to simulate launch failure */
  errorMode?: boolean;
  /** Simulate network delay in milliseconds */
  responseDelayMs?: number;
  /** Custom HTTP status code for error responses (default: 500) */
  statusCode?: number;
  /** Custom error message for error responses */
  errorMessage?: string;
  /** Override starting directory for random selection */
  startingDirectory?: string;
  /** Array of files to randomly select from (overrides filesystem) */
  filePool?: FileItemDto[];
}

/**
 * Intercepts POST /devices/{deviceId}/storage/{storageType}/random-launch - Random file launch endpoint
 * Route matches any deviceId and storageType via wildcard: POST http://localhost:5168/devices/<wildcard>/storage/<wildcard>/random-launch
 * Supports filesystem random selection and deterministic testing scenarios
 * Uses primitive functions for simplified implementation and RFC 9110 compliance
 *
 * @param options Configuration options for the interceptor
 */
export function interceptLaunchRandom(options: InterceptLaunchRandomOptions = {}): void {
  const {
    filesystem,
    selectedFile,
    errorMode = false,
    responseDelayMs = 0,
    statusCode,
    errorMessage,
    startingDirectory: fallbackStartingDir,
    filePool,
  } = options;

  if (errorMode) {
    interceptError(
      LAUNCH_RANDOM_ENDPOINT,
      statusCode || 500,
      errorMessage || 'Failed to launch random file',
      responseDelayMs
    );
    return;
  }

  // This interceptor requires custom cy.intercept() logic due to starting directory extraction
  // Primitives don't provide request access for extracting StartingDirectory parameter
  cy.intercept(LAUNCH_RANDOM_ENDPOINT.method as 'POST', LAUNCH_RANDOM_ENDPOINT.pattern, (req) => {
    let launchedFile;

    // Priority 1: Use selectedFile for deterministic testing
    if (selectedFile) {
      launchedFile = selectedFile;
    }
    // Priority 2: Use provided filePool for controlled randomness
    else if (filePool && filePool.length > 0) {
      launchedFile = filePool[Math.floor(Math.random() * filePool.length)];
    }
    // Priority 3: Use filesystem for realistic random selection
    else if (filesystem) {
      const startingDir = resolvePath(getQueryParam(req, 'StartingDirectory'), fallbackStartingDir);
      const directoryResponse = filesystem.getDirectory(startingDir);
      const availableFiles = directoryResponse.storageItem.files || [];

      if (availableFiles.length > 0) {
        launchedFile = availableFiles[Math.floor(Math.random() * availableFiles.length)];
      } else {
        launchedFile = generateFileItem();
      }
    }
    else {
      launchedFile = generateFileItem();
    }

    const response: LaunchRandomResponse = {
      message: `Successfully launched random file ${launchedFile.name}`,
      launchedFile,
      isCompatible: launchedFile.isCompatible ?? true,
    };

    req.reply({
      statusCode: 200,
      headers: { 'content-type': 'application/json' },
      body: response,
      delay: responseDelayMs || 0,
    });
  }).as(LAUNCH_RANDOM_ENDPOINT.alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for launchRandom endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForLaunchRandom(): void {
  cy.wait(`@${LAUNCH_RANDOM_ENDPOINT.alias}`);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

/**
 * Sets up launchRandom interceptor with filesystem for random file launch tests
 * Useful for testing realistic random file launch scenarios
 *
 * @param filesystem Mock filesystem instance
 * @param options Additional interceptor options
 */
export function setupLaunchRandom(
  filesystem: MockFilesystem,
  options: Omit<InterceptLaunchRandomOptions, 'filesystem'> = {}
): void {
  interceptLaunchRandom({
    ...options,
    filesystem,
  });
}

/**
 * Sets up launchRandom interceptor with specific file to launch (deterministic)
 * Useful for testing specific random launch scenarios without actual randomness
 *
 * @param file File to launch
 * @param options Additional interceptor options
 */
export function setupLaunchRandomWithFile(
  file: FileItemDto,
  options: InterceptLaunchRandomOptions = {}
): void {
  interceptLaunchRandom({
    ...options,
    selectedFile: file,
  });
}

/**
 * Sets up launchRandom interceptor with specific file pool for controlled randomness
 * Useful for testing random selection from a predefined set of files
 *
 * @param files Array of files to randomly select from
 * @param options Additional interceptor options
 */
export function setupLaunchRandomWithPool(
  files: FileItemDto[],
  options: InterceptLaunchRandomOptions = {}
): void {
  interceptLaunchRandom({
    ...options,
    filePool: files,
  });
}

/**
 * Sets up launchRandom interceptor with specific starting directory
 * Useful for testing random file selection from specific directories
 *
 * @param startingDirectory Directory to select random files from
 * @param options Additional interceptor options
 */
export function setupLaunchRandomFromDirectory(
  startingDirectory: string,
  options: InterceptLaunchRandomOptions = {}
): void {
  interceptLaunchRandom({
    ...options,
    startingDirectory,
  });
}

/**
 * Sets up launchRandom interceptor with error response
 * Useful for testing random launch error scenarios
 *
 * @param statusCode HTTP status code for the error (default: 500)
 * @param errorMessage Custom error message
 */
export function setupErrorLaunchRandom(statusCode = 500, errorMessage?: string): void {
  interceptLaunchRandom({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Sets up launchRandom interceptor with delay for testing loading states
 * Useful for testing random launch loading scenarios and timeouts
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedLaunchRandom(
  delayMs: number,
  options: InterceptLaunchRandomOptions = {}
): void {
  interceptLaunchRandom({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Verifies that a launchRandom request was made
 * Useful for validation in tests
 */
export function verifyLaunchRandomRequested(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${LAUNCH_RANDOM_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the launchRandom endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastLaunchRandomRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${LAUNCH_RANDOM_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of random file launch responses to test multiple launches
 * Useful for testing multi-step random launch workflows
 *
 * @param files Array of files to launch in sequence (or random pool)
 * @param delayBetweenMs Delay between each response in milliseconds
 * @param useRandomSelection Whether to randomly select from files array
 */
export function setupLaunchRandomSequence(
  files: FileItemDto[],
  delayBetweenMs = 1000,
  useRandomSelection = true
): void {
  let currentIndex = 0;

  cy.intercept(LAUNCH_RANDOM_ENDPOINT.method as 'POST', LAUNCH_RANDOM_ENDPOINT.pattern, (req) => {
    let currentFile;

    if (useRandomSelection) {
      currentFile = files[Math.floor(Math.random() * files.length)];
    } else {
      currentFile = files[currentIndex % files.length];
      currentIndex++;
    }

    const response: LaunchRandomResponse = {
      message: `Successfully launched random file ${currentFile.name}`,
      launchedFile: currentFile,
      isCompatible: currentFile.isCompatible ?? true,
    };

    req.reply({
      statusCode: 200,
      headers: { 'content-type': 'application/json' },
      body: response,
      delay: delayBetweenMs,
    });
  }).as(`${LAUNCH_RANDOM_ENDPOINT.alias}_sequence`);
}

/**
 * Sets up launchRandom interceptor for testing incompatible file scenarios
 * Useful for testing how the system handles incompatible random files
 *
 * @param file File to launch as incompatible
 * @param options Additional interceptor options
 */
export function setupIncompatibleLaunchRandom(
  file: FileItemDto,
  options: InterceptLaunchRandomOptions = {}
): void {
  const incompatibleFile = { ...file, isCompatible: false };

  interceptLaunchRandom({
    ...options,
    selectedFile: incompatibleFile,
  });
}

/**
 * Sets up launchRandom interceptor that guarantees different files on each call
 * Useful for testing true randomness without repetition
 *
 * @param filesystem Mock filesystem instance
 * @param options Additional interceptor options
 */
export function setupUniqueLaunchRandom(
  filesystem: MockFilesystem,
  options: Omit<InterceptLaunchRandomOptions, 'filesystem'> = {}
): void {
  const launchedFiles = new Set<string>();

  cy.intercept(LAUNCH_RANDOM_ENDPOINT.method as 'POST', LAUNCH_RANDOM_ENDPOINT.pattern, (req) => {
    const startingDir = resolvePath(
      getQueryParam(req, 'StartingDirectory'),
      options.startingDirectory
    );
    const directoryResponse = filesystem.getDirectory(startingDir);
    const availableFiles = directoryResponse.storageItem.files || [];

    const remainingFiles = availableFiles.filter((file) => !launchedFiles.has(file.path));

    const filesToSelect = remainingFiles.length > 0 ? remainingFiles : availableFiles;

    if (filesToSelect.length > 0) {
      const selectedFile = filesToSelect[Math.floor(Math.random() * filesToSelect.length)];
      launchedFiles.add(selectedFile.path);

      const response: LaunchRandomResponse = {
        message: `Successfully launched random file ${selectedFile.name}`,
        launchedFile: selectedFile,
        isCompatible: selectedFile.isCompatible ?? true,
      };

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
        delay: options.responseDelayMs || 0,
      });
    } else {
      req.reply({
        statusCode: 404,
        headers: { 'content-type': 'application/problem+json' },
        body: {
          type: 'https://tools.ietf.org/html/rfc9110#section-15.5.5',
          title: 'Not Found',
          status: 404,
          detail: 'No files available for random launch',
        },
      });
    }
  }).as(`${LAUNCH_RANDOM_ENDPOINT.alias}_unique`);
}

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

export const LAUNCH_RANDOM_ALIAS = LAUNCH_RANDOM_ENDPOINT.alias;
export const INTERCEPT_LAUNCH_RANDOM = 'launchRandom';
export const LAUNCH_RANDOM_METHOD = LAUNCH_RANDOM_ENDPOINT.method;
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
