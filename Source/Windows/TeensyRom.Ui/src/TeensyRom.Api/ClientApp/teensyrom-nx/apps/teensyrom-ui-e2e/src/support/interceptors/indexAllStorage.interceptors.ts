/// <reference types="cypress" />

import type { Method } from 'cypress/types/net-stubbing';
import {
  interceptSuccess,
  interceptError,
  interceptSequence,
  type EndpointDefinition,
} from './primitives/interceptor-primitives';

/**
 * indexAllStorage endpoint interceptor for batch storage indexing
 * Uses primitive-based architecture (interceptSuccess, interceptError, interceptSequence)
 */

export const INDEX_ALL_STORAGE_ENDPOINT: EndpointDefinition = {
  method: 'POST' as Method,
  pattern: 'http://localhost:5168/files/index/all',
  alias: 'indexAllStorage',
} as const;

/**
 * Options for interceptIndexAllStorage interceptor
 */
export interface InterceptIndexAllStorageOptions {
  /** When true, return HTTP error to simulate API failure */
  errorMode?: boolean;
  /** Simulate network delay in milliseconds */
  responseDelayMs?: number;
  /** Custom HTTP status code for error responses (default: 404) */
  statusCode?: number;
  /** Custom error message for error responses */
  errorMessage?: string;
  /** Simulate partial failure mode where some devices fail */
  partialFailureMode?: boolean;
  /** Number of devices to simulate in batch operation */
  deviceCount?: number;
  /** Number of successful device indexing operations */
  successCount?: number;
  /** Number of failed device indexing operations */
  failureCount?: number;
  /** Simulate progress tracking for long batch operations */
  simulateProgress?: boolean;
  /** Total progress steps for simulated progress (default: 10) */
  progressSteps?: number;
  /** Custom devices to include in batch response */
  devices?: Array<{
    deviceId: string;
    storageType: 'USB' | 'SD';
    success: boolean;
    error?: string;
  }>;
}

/**
 * Intercepts POST /files/index/all - Batch storage indexing endpoint
 * Route matches exact URL: POST http://localhost:5168/files/index/all
 * Supports batch processing patterns and complex failure scenarios
 * Uses primitive functions for simplified implementation and RFC 9110 compliance
 *
 * @param options Configuration options for the interceptor
 */
export function interceptIndexAllStorage(options: InterceptIndexAllStorageOptions = {}): void {
  const {
    errorMode = false,
    responseDelayMs = 100,
    statusCode,
    errorMessage,
    partialFailureMode = false,
    deviceCount = 3,
    successCount,
    failureCount,
    simulateProgress = false,
    progressSteps = 10,
    devices: customDevices,
  } = options;

  if (errorMode) {
    interceptError(
      INDEX_ALL_STORAGE_ENDPOINT,
      statusCode || 404,
      errorMessage || 'Failed to index all storage',
      responseDelayMs
    );
    return;
  }

  let response: {
    message: string;
    totalDevices: number;
    successCount: number;
    failureCount: number;
    devices: Array<{
      deviceId: string;
      storageType: 'USB' | 'SD';
      success: boolean;
      error?: string;
    }>;
    timestamp: string;
  };

  if (partialFailureMode) {
    const actualSuccessCount = successCount ?? Math.floor(deviceCount * 0.7);
    const actualFailureCount = failureCount ?? deviceCount - actualSuccessCount;

    if (customDevices && customDevices.length > 0) {
      response = {
        message: `Batch indexing completed with ${actualSuccessCount} successes and ${actualFailureCount} failures`,
        totalDevices: customDevices.length,
        successCount: actualSuccessCount,
        failureCount: actualFailureCount,
        devices: customDevices,
        timestamp: new Date().toISOString(),
      };
    } else {
      response = {
        message: `Batch indexing completed with ${actualSuccessCount} successes and ${actualFailureCount} failures`,
        totalDevices: deviceCount,
        successCount: actualSuccessCount,
        failureCount: actualFailureCount,
        devices: generateBatchDevices(deviceCount, actualSuccessCount),
        timestamp: new Date().toISOString(),
      };
    }
  } else {
    if (customDevices && customDevices.length > 0) {
      response = {
        message: `Successfully indexed all storage for ${customDevices.length} devices`,
        totalDevices: customDevices.length,
        successCount: customDevices.length,
        failureCount: 0,
        devices: customDevices,
        timestamp: new Date().toISOString(),
      };
    } else {
      response = {
        message: `Successfully indexed all storage for ${deviceCount} devices`,
        totalDevices: deviceCount,
        successCount: deviceCount,
        failureCount: 0,
        devices: generateBatchDevices(deviceCount, deviceCount),
        timestamp: new Date().toISOString(),
      };
    }
  }

  if (simulateProgress) {
    simulateBatchIndexingProgress(response.totalDevices, progressSteps, responseDelayMs);
  }

  interceptSuccess(INDEX_ALL_STORAGE_ENDPOINT, response, responseDelayMs);
}

/**
 * Waits for indexAllStorage endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForIndexAllStorage(): void {
  cy.wait(`@${INDEX_ALL_STORAGE_ENDPOINT.alias}`);
}

/**
 * Sets up indexAllStorage interceptor with error response
 * Useful for testing batch indexing error scenarios
 *
 * @param statusCode HTTP status code for the error (default: 404)
 * @param errorMessage Custom error message
 */
export function setupErrorIndexAllStorage(statusCode = 404, errorMessage?: string): void {
  interceptIndexAllStorage({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Sets up indexAllStorage interceptor with delay for testing loading states
 * Useful for testing batch indexing loading scenarios and timeouts
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedIndexAllStorage(
  delayMs: number,
  options: InterceptIndexAllStorageOptions = {}
): void {
  interceptIndexAllStorage({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up indexAllStorage interceptor with partial failures
 * Useful for testing batch operations with some device failures
 *
 * @param deviceCount Total number of devices in batch
 * @param successCount Number of successful operations
 * @param options Additional interceptor options
 */
export function setupPartialFailureIndexAllStorage(
  deviceCount: number,
  successCount: number,
  options: Omit<
    InterceptIndexAllStorageOptions,
    'partialFailureMode' | 'deviceCount' | 'successCount'
  > = {}
): void {
  interceptIndexAllStorage({
    ...options,
    partialFailureMode: true,
    deviceCount,
    successCount,
  });
}

/**
 * Sets up indexAllStorage interceptor with progress simulation
 * Useful for testing long batch operations with progress tracking
 *
 * @param deviceCount Number of devices to simulate
 * @param progressSteps Number of progress steps to simulate
 * @param delayMs Total delay for the batch operation
 */
export function setupIndexAllStorageWithProgress(
  deviceCount = 5,
  progressSteps = 10,
  delayMs = 8000
): void {
  interceptIndexAllStorage({
    deviceCount,
    simulateProgress: true,
    progressSteps,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up indexAllStorage interceptor with custom device data
 * Useful for testing specific batch scenarios
 *
 * @param devices Array of device data to include in response
 * @param options Additional interceptor options
 */
export function setupCustomIndexAllStorage(
  devices: Array<{
    deviceId: string;
    storageType: 'USB' | 'SD';
    success: boolean;
    error?: string;
  }>,
  options: Omit<InterceptIndexAllStorageOptions, 'devices'> = {}
): void {
  interceptIndexAllStorage({
    ...options,
    devices,
  });
}

/**
 * Sets up indexAllStorage interceptor for large batch operations
 * Useful for testing performance with many devices
 *
 * @param deviceCount Number of devices in large batch
 * @param failureRate Failure rate as percentage (0-100)
 * @param delayMs Total delay for the operation
 */
export function setupLargeBatchIndexAllStorage(
  deviceCount = 20,
  failureRate = 10,
  delayMs = 15000
): void {
  const successCount = Math.floor(deviceCount * (1 - failureRate / 100));

  interceptIndexAllStorage({
    deviceCount,
    successCount,
    partialFailureMode: failureRate > 0,
    simulateProgress: true,
    progressSteps: Math.min(20, deviceCount),
    responseDelayMs: delayMs,
  });
}

/**
 * Verifies that an indexAllStorage request was made
 * Useful for validation in tests
 */
export function verifyIndexAllStorageRequested(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${INDEX_ALL_STORAGE_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the indexAllStorage endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastIndexAllStorageRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${INDEX_ALL_STORAGE_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of batch indexing responses to test repeated operations
 * Useful for testing multi-step batch workflows
 * Uses interceptSequence primitive for simplified sequential response handling
 *
 * @param batches Array of batch configurations in sequence
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupBatchIndexingSequence(
  batches: Array<{
    deviceCount?: number;
    successCount?: number;
    failureCount?: number;
  }>,
  delayBetweenMs = 2000
): void {
  const sequenceResponses = batches.map((batch, index) => {
    const deviceCount = batch.deviceCount || 3;
    const successCount = batch.successCount ?? deviceCount;
    const failureCount = batch.failureCount ?? deviceCount - successCount;

    return {
      message: `Batch ${
        index + 1
      }: Completed with ${successCount} successes and ${failureCount} failures`,
      batchIndex: index + 1,
      totalDevices: deviceCount,
      successCount,
      failureCount,
      devices: generateBatchDevices(deviceCount, successCount),
      timestamp: new Date().toISOString(),
    };
  });

  interceptSequence(INDEX_ALL_STORAGE_ENDPOINT, sequenceResponses, delayBetweenMs);
}

function generateBatchDevices(totalDevices: number, successCount: number) {
  const devices = [];

  for (let i = 0; i < totalDevices; i++) {
    const deviceId = `device-${String(i + 1).padStart(3, '0')}`;
    const storageType: 'SD' | 'USB' = i % 2 === 0 ? 'USB' : 'SD';
    const success = i < successCount;

    devices.push({
      deviceId,
      storageType,
      success,
      error: success ? undefined : `Indexing failed for ${storageType} storage`,
    });
  }

  return devices;
}

function simulateBatchIndexingProgress(
  totalDevices: number,
  steps: number,
  totalDelay: number
): void {
  const stepDelay = totalDelay / steps;

  for (let i = 1; i <= steps; i++) {
    const progress = Math.round((i / steps) * 100);
    const completedDevices = Math.round((i / steps) * totalDevices);

    cy.log(
      `Batch indexing progress: ${progress}% (${completedDevices}/${totalDevices} devices completed)`
    );

    if (i < steps) {
      cy.wait(stepDelay, { log: false });
    }
  }
}

export const INDEX_ALL_STORAGE_ALIAS = INDEX_ALL_STORAGE_ENDPOINT.alias;
export const INTERCEPT_INDEX_ALL_STORAGE = 'indexAllStorage';
export const INDEX_ALL_STORAGE_METHOD = INDEX_ALL_STORAGE_ENDPOINT.method;
