/// <reference types="cypress" />

import type MockFilesystem from '../test-data/mock-filesystem/mock-filesystem';


// Re-export indexAllStorage from dedicated file for backward compatibility
export {
  interceptIndexAllStorage,
  waitForIndexAllStorage,
  INDEX_ALL_STORAGE_ENDPOINT,
  type InterceptIndexAllStorageOptions,
  INDEX_ALL_STORAGE_ALIAS,
  INTERCEPT_INDEX_ALL_STORAGE,
  INDEX_ALL_STORAGE_METHOD,
} from './indexAllStorage.interceptors';

/**
 * indexStorage endpoint interceptor for single device storage indexing
 * This file consolidates all indexStorage-related testing functionality
 */

// ============================================================================
// Section 1: Endpoint Definition
// ============================================================================

/**
 * indexStorage endpoint configuration
 */
export const INDEX_STORAGE_ENDPOINT = {
  method: 'POST',
  path: (deviceId: string, storageType: string) => `/devices/${deviceId}/storage/${storageType}/index`,
  full: (deviceId: string, storageType: string) => `http://localhost:5168/devices/${deviceId}/storage/${storageType}/index`,
  pattern: 'http://localhost:5168/devices/*/storage/*/index*',
  alias: 'indexStorage'
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

/**
 * Options for interceptIndexStorage interceptor
 */
export interface InterceptIndexStorageOptions {
  /** Mock filesystem instance for realistic indexing scenarios */
  filesystem?: MockFilesystem;
  /** When true, return HTTP error to simulate indexing failure */
  errorMode?: boolean;
  /** Simulate network delay in milliseconds */
  responseDelayMs?: number;
  /** Custom HTTP status code for error responses (default: 400) */
  statusCode?: number;
  /** Custom error message for error responses */
  errorMessage?: string;
  /** Device identifier to intercept (for pattern matching) */
  deviceId?: string;
  /** Storage type to intercept ('USB' or 'SD') */
  storageType?: 'USB' | 'SD';
  /** Override alias for the interceptor */
  customAlias?: string;
}

/**
 * Device and storage type combination for indexing
 */
export interface DeviceStorageCombo {
  /** Device identifier */
  deviceId: string;
  /** Storage type (USB or SD) */
  storageType: 'USB' | 'SD';
}

/**
 * Options for batch indexing operations
 */
export interface IndexStorageBatchOptions {
  /** Simulated network delay in milliseconds */
  delay?: number;
  /** Array of device/storage combinations that should fail */
  failingCombos?: Array<{ deviceId: string; storageType: 'USB' | 'SD' }>;
}

// ============================================================================
// Section 3: Interceptor Function
// ============================================================================

/**
 * Intercepts POST /devices/{deviceId}/storage/{storageType}/index - Single device storage indexing endpoint
 * Route matches any deviceId and storageType via wildcard: POST http://localhost:5168/devices/<wildcard>/storage/<wildcard>/index
 * Supports device-specific indexing and complex failure scenarios
 *
 * @param options Configuration options for the interceptor
 */
export function interceptIndexStorage(options: InterceptIndexStorageOptions = {}): void {
  const {
    errorMode = false,
    responseDelayMs = 100,
    statusCode,
    errorMessage,
    deviceId: targetDeviceId,
    storageType: targetStorageType,
    customAlias
  } = options;

  const alias = customAlias || INDEX_STORAGE_ENDPOINT.alias;

  cy.intercept(
    INDEX_STORAGE_ENDPOINT.method,
    INDEX_STORAGE_ENDPOINT.pattern,
    (req) => {
      // Extract device ID and storage type from request URL
      const url = req.url;
      const deviceIdMatch = url.match(/\/devices\/([^/]+)\/storage\//);
      const storageTypeMatch = url.match(/\/storage\/([^/]+)\/index/);
      const requestDeviceId = deviceIdMatch ? deviceIdMatch[1] : 'unknown';
      const requestStorageType = storageTypeMatch ? storageTypeMatch[1] : 'unknown';

      // If specific device/storage type targeting is requested, only intercept matching requests
      if (targetDeviceId && requestDeviceId !== targetDeviceId) {
        return; // Don't intercept, let it proceed normally
      }
      if (targetStorageType && requestStorageType !== targetStorageType) {
        return; // Don't intercept, let it proceed normally
      }

      // Apply response delay if specified
      if (responseDelayMs && responseDelayMs > 0) {
        // Note: Cypress doesn't support req.delay() like req.reply({ delay }),
        // so we handle this by using setTimeout in the reply
      }

      if (errorMode) {
        const responseStatusCode = statusCode || 400;
        const responseErrorMessage = errorMessage || `Failed to index ${requestStorageType} storage for device ${requestDeviceId}`;

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

      // Success response - empty body with 200 OK
      if (responseDelayMs && responseDelayMs > 0) {
        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: {},
          delay: responseDelayMs,
        });
      } else {
        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: {},
        });
      }
    }
  ).as(alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for indexStorage endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForIndexStorage(alias?: string): void {
  const waitAlias = alias || `@${INDEX_STORAGE_ENDPOINT.alias}`;
  cy.wait(waitAlias);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

/**
 * Sets up indexStorage interceptor for specific device
 * Useful for testing individual device indexing scenarios
 *
 * @param deviceId Device identifier to intercept
 * @param storageTypes Storage types to intercept
 * @param options Additional interceptor options
 */
export function setupIndexStorageForDevice(
  deviceId: string,
  storageTypes: ('USB' | 'SD')[] = ['USB', 'SD'],
  options: Omit<InterceptIndexStorageOptions, 'deviceId' | 'storageType'> = {}
): void {
  storageTypes.forEach(storageType => {
    interceptIndexStorage({
      ...options,
      deviceId,
      storageType,
      customAlias: `indexStorage_${deviceId}_${storageType}`
    });
  });
}

/**
 * Sets up indexStorage interceptor with error response
 * Useful for testing indexing error scenarios
 *
 * @param deviceId Device identifier to intercept
 * @param storageTypes Storage types to intercept
 * @param statusCode HTTP status code for the error (default: 400)
 * @param errorMessage Custom error message
 */
export function setupErrorIndexStorage(
  deviceId: string,
  storageTypes: ('USB' | 'SD')[] = ['USB', 'SD'],
  statusCode = 400,
  errorMessage?: string
): void {
  storageTypes.forEach(storageType => {
    interceptIndexStorage({
      errorMode: true,
      statusCode,
      errorMessage: errorMessage || `Failed to index ${storageType} storage for device ${deviceId}`,
      deviceId,
      storageType,
      customAlias: `indexStorage_${deviceId}_${storageType}`
    });
  });
}

/**
 * Sets up indexStorage interceptor with delay for testing loading states
 * Useful for testing indexing loading scenarios and timeouts
 *
 * @param deviceId Device identifier to intercept
 * @param delayMs Delay in milliseconds before response
 * @param storageTypes Storage types to intercept
 * @param options Additional interceptor options
 */
export function setupDelayedIndexStorage(
  deviceId: string,
  delayMs: number,
  storageTypes: ('USB' | 'SD')[] = ['USB', 'SD'],
  options: Omit<InterceptIndexStorageOptions, 'responseDelayMs' | 'deviceId' | 'storageType'> = {}
): void {
  storageTypes.forEach(storageType => {
    interceptIndexStorage({
      ...options,
      responseDelayMs: delayMs,
      deviceId,
      storageType,
      customAlias: `indexStorage_${deviceId}_${storageType}`
    });
  });
}

/**
 * Sets up indexStorage interceptor for batch indexing operations
 * Useful for testing multi-device indexing scenarios
 *
 * @param deviceIds Array of device identifiers to intercept
 * @param storageTypes Storage types to intercept
 * @param options Additional interceptor options
 */
export function setupBatchIndexStorage(
  deviceIds: string[],
  storageTypes: ('USB' | 'SD')[] = ['USB', 'SD'],
  options: Omit<InterceptIndexStorageOptions, 'deviceId' | 'storageType'> = {}
): void {
  deviceIds.forEach(deviceId => {
    storageTypes.forEach(storageType => {
      interceptIndexStorage({
        ...options,
        deviceId,
        storageType,
        customAlias: `indexStorage_${deviceId}_${storageType}`
      });
    });
  });
}

/**
 * Batch setup helper for intercepting multiple single-device indexing operations
 * Useful for testing SEQUENCES of single-device indexing operations
 * IMPORTANT: This is for testing individual device indexing operations, NOT for the "Index All" endpoint
 *
 * @param deviceStorageCombos Array of device/storage combinations to setup interceptors for
 * @param options Applied to all interceptors (delay, failing combos)
 */
export function interceptIndexStorageBatch(
  deviceStorageCombos: DeviceStorageCombo[],
  options: IndexStorageBatchOptions = {}
): void {
  const { delay = 100, failingCombos = [] } = options;

  // Create a Set for O(1) lookup of failing combos
  const failingSet = new Set(
    failingCombos.map((combo) => `${combo.deviceId}|${combo.storageType}`)
  );

  deviceStorageCombos.forEach(({ deviceId, storageType }) => {
    const isFailingCombo = failingSet.has(`${deviceId}|${storageType}`);

    interceptIndexStorage({
      responseDelayMs: delay,
      errorMode: isFailingCombo,
      errorMessage: isFailingCombo
        ? `Failed to index ${storageType} storage for device ${deviceId}`
        : undefined,
      deviceId,
      storageType,
      customAlias: `indexStorage_${deviceId}_${storageType}`
    });
  });
}

/**
 * Verifies that an indexStorage request was made
 * Useful for validation in tests
 */
export function verifyIndexStorageRequested(alias?: string): Cypress.Chainable<any> {
  const waitAlias = alias || INDEX_STORAGE_ENDPOINT.alias;
  return cy.get(`@${waitAlias}`);
}

/**
 * Gets the last request made to the indexStorage endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastIndexStorageRequest(alias?: string): Cypress.Chainable<any> {
  const waitAlias = alias || INDEX_STORAGE_ENDPOINT.alias;
  return cy.get(`@${waitAlias}`);
}

/**
 * Creates a sequence of indexing responses to test multiple indexing operations
 * Useful for testing multi-step indexing workflows
 *
 * @param deviceStorageCombos Array of device/storage combinations to index in sequence
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupIndexStorageSequence(deviceStorageCombos: DeviceStorageCombo[], delayBetweenMs = 1000): void {
  let currentIndex = 0;

  cy.intercept(
    INDEX_STORAGE_ENDPOINT.method,
    INDEX_STORAGE_ENDPOINT.pattern,
    (req) => {
      // Extract device ID and storage type from request URL
      const url = req.url;
      const deviceIdMatch = url.match(/\/devices\/([^/]+)\/storage\//);
      const storageTypeMatch = url.match(/\/storage\/([^/]+)\/index/);
      const requestDeviceId = deviceIdMatch ? deviceIdMatch[1] : 'unknown';
      const requestStorageType = storageTypeMatch ? storageTypeMatch[1] : 'unknown';

      // Find matching combination in sequence
      const currentCombo = deviceStorageCombos[currentIndex % deviceStorageCombos.length];

      if (currentCombo.deviceId === requestDeviceId && currentCombo.storageType === requestStorageType) {
        currentIndex++;

        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: {},
          delay: delayBetweenMs,
        });
      } else {
        // Let it proceed normally if not in sequence
        return;
      }
    }
  ).as(`${INDEX_STORAGE_ENDPOINT.alias}_sequence`);
}

/**
 * Sets up indexStorage interceptor for testing partial failure scenarios
 * Useful for testing how the system handles some indexing operations failing
 *
 * @param deviceStorageCombos All device/storage combinations
 * @param successCombos Combinations that should succeed
 * @param options Additional interceptor options
 */
export function setupPartialFailureIndexStorage(
  deviceStorageCombos: DeviceStorageCombo[],
  successCombos: DeviceStorageCombo[],
  options: Omit<InterceptIndexStorageOptions, 'errorMode' | 'deviceId' | 'storageType'> = {}
): void {
  const successSet = new Set(
    successCombos.map((combo) => `${combo.deviceId}|${combo.storageType}`)
  );

  deviceStorageCombos.forEach(({ deviceId, storageType }) => {
    const shouldSucceed = successSet.has(`${deviceId}|${storageType}`);

    interceptIndexStorage({
      ...options,
      errorMode: !shouldSucceed,
      errorMessage: !shouldSucceed
        ? `Failed to index ${storageType} storage for device ${deviceId}`
        : undefined,
      deviceId,
      storageType,
      customAlias: `indexStorage_${deviceId}_${storageType}`
    });
  });
}

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const INDEX_STORAGE_ALIAS = INDEX_STORAGE_ENDPOINT.alias;
export const INTERCEPT_INDEX_STORAGE = 'indexStorage';
export const INDEX_STORAGE_METHOD = INDEX_STORAGE_ENDPOINT.method;

// Legacy exports for backward compatibility
export const INDEXING_INTERCEPT_ALIASES = {
  INDEX_STORAGE_USB: 'indexStorageUSB',
  INDEX_STORAGE_SD: 'indexStorageSD',
  INDEX_ALL_STORAGE: 'indexAllStorage',
  byDeviceAndType: (deviceId: string, storageType: 'USB' | 'SD') =>
    `indexStorage_${deviceId}_${storageType}`,
} as const;

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