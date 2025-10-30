/// <reference types="cypress" />


/**
 * indexAllStorage endpoint interceptor for batch storage indexing
 * This file consolidates all indexAllStorage-related testing functionality
 */

// ============================================================================
// Section 1: Endpoint Definition
// ============================================================================

/**
 * indexAllStorage endpoint configuration
 */
export const INDEX_ALL_STORAGE_ENDPOINT = {
  method: 'POST',
  path: '/files/index/all',
  full: 'http://localhost:5168/files/index/all',
  pattern: 'http://localhost:5168/files/index/all',
  alias: 'indexAllStorage'
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

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

// ============================================================================
// Section 3: Interceptor Function
// ============================================================================

/**
 * Intercepts POST /files/index/all - Batch storage indexing endpoint
 * Route matches exact URL: POST http://localhost:5168/files/index/all
 * Supports batch processing patterns and complex failure scenarios
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
    devices: customDevices
  } = options;

  cy.intercept(
    INDEX_ALL_STORAGE_ENDPOINT.method,
    INDEX_ALL_STORAGE_ENDPOINT.pattern,
    (req) => {
      // Apply response delay if specified
      if (responseDelayMs && responseDelayMs > 0) {
        // Note: Cypress doesn't support req.delay() like req.reply({ delay }),
        // so we handle this by using setTimeout in the reply
      }

      if (errorMode) {
        const responseStatusCode = statusCode || 404;
        const responseErrorMessage = errorMessage || 'Failed to index all storage';

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

      // Success response
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
        // Partial failure mode - some devices succeed, some fail
        const actualSuccessCount = successCount ?? Math.floor(deviceCount * 0.7); // 70% success by default
        const actualFailureCount = failureCount ?? deviceCount - actualSuccessCount;

        if (customDevices && customDevices.length > 0) {
          // Use custom device data
          response = {
            message: `Batch indexing completed with ${actualSuccessCount} successes and ${actualFailureCount} failures`,
            totalDevices: customDevices.length,
            successCount: actualSuccessCount,
            failureCount: actualFailureCount,
            devices: customDevices,
            timestamp: new Date().toISOString(),
          };
        } else {
          // Generate default device data
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
        // Full success mode
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
        // Simulate progress updates for long batch operations
        simulateBatchIndexingProgress(response.totalDevices, progressSteps, responseDelayMs);
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
  ).as(INDEX_ALL_STORAGE_ENDPOINT.alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for indexAllStorage endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForIndexAllStorage(): void {
  cy.wait(`@${INDEX_ALL_STORAGE_ENDPOINT.alias}`);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

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
export function setupDelayedIndexAllStorage(delayMs: number, options: InterceptIndexAllStorageOptions = {}): void {
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
  options: Omit<InterceptIndexAllStorageOptions, 'partialFailureMode' | 'deviceCount' | 'successCount'> = {}
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
export function verifyIndexAllStorageRequested(): Cypress.Chainable<any> {
  return cy.get(`@${INDEX_ALL_STORAGE_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the indexAllStorage endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastIndexAllStorageRequest(): Cypress.Chainable<any> {
  return cy.get(`@${INDEX_ALL_STORAGE_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of batch indexing responses to test repeated operations
 * Useful for testing multi-step batch workflows
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
  let currentIndex = 0;

  cy.intercept(
    INDEX_ALL_STORAGE_ENDPOINT.method,
    INDEX_ALL_STORAGE_ENDPOINT.pattern,
    (req) => {
      const currentBatch = batches[currentIndex % batches.length];
      currentIndex++;

      const deviceCount = currentBatch.deviceCount || 3;
      const successCount = currentBatch.successCount ?? deviceCount;
      const failureCount = currentBatch.failureCount ?? (deviceCount - successCount);

      const response = {
        message: `Batch ${currentIndex}: Completed with ${successCount} successes and ${failureCount} failures`,
        batchIndex: currentIndex,
        totalDevices: deviceCount,
        successCount,
        failureCount,
        devices: generateBatchDevices(deviceCount, successCount),
        timestamp: new Date().toISOString(),
      };

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
        delay: delayBetweenMs,
      });
    }
  ).as(`${INDEX_ALL_STORAGE_ENDPOINT.alias}_sequence`);
}

/**
 * Generates mock device data for batch operations
 * Internal helper function
 *
 * @param totalDevices Total number of devices
 * @param successCount Number of successful devices
 */
function generateBatchDevices(totalDevices: number, successCount: number) {
  const devices = [];

  for (let i = 0; i < totalDevices; i++) {
    const deviceId = `device-${String(i + 1).padStart(3, '0')}`;
    const storageType: "SD" | "USB" = i % 2 === 0 ? 'USB' : 'SD';
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

/**
 * Simulates batch indexing progress updates for long operations
 * Internal helper function for progress simulation
 *
 * @param totalDevices Total number of devices
 * @param steps Number of progress steps
 * @param totalDelay Total delay time
 */
function simulateBatchIndexingProgress(totalDevices: number, steps: number, totalDelay: number): void {
  const stepDelay = totalDelay / steps;

  for (let i = 1; i <= steps; i++) {
    const progress = Math.round((i / steps) * 100);
    const completedDevices = Math.round((i / steps) * totalDevices);

    cy.log(`Batch indexing progress: ${progress}% (${completedDevices}/${totalDevices} devices completed)`);

    if (i < steps) {
      cy.wait(stepDelay, { log: false });
    }
  }
}

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const INDEX_ALL_STORAGE_ALIAS = INDEX_ALL_STORAGE_ENDPOINT.alias;
export const INTERCEPT_INDEX_ALL_STORAGE = 'indexAllStorage';
export const INDEX_ALL_STORAGE_METHOD = INDEX_ALL_STORAGE_ENDPOINT.method;

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