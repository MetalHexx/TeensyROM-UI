/// <reference types="cypress" />

import {
  INDEXING_ENDPOINTS,
  createProblemDetailsResponse,
} from '../constants';

// ============================================================================
// Interceptor Aliases for cy.wait()
// ============================================================================

/**
 * Aliases for interceptor registration with cy.intercept().as()
 * Used with cy.wait() to verify API calls and control timing in tests
 */
export const INDEXING_INTERCEPT_ALIASES = {
  INDEX_STORAGE_USB: 'indexStorageUSB',
  INDEX_STORAGE_SD: 'indexStorageSD',
  INDEX_ALL_STORAGE: 'indexAllStorage',
  byDeviceAndType: (deviceId: string, storageType: 'USB' | 'SD') =>
    `indexStorage_${deviceId}_${storageType}`,
} as const;

/**
 * Options for interceptIndexStorage interceptor
 */
export interface InterceptIndexStorageOptions {
  /** Simulated network delay in milliseconds before response (default: 100) */
  delay?: number;
  /** HTTP status code to return (default: 200) */
  statusCode?: number;
  /** When true, return error response instead of success */
  errorMode?: boolean;
  /** Custom error message for error responses */
  errorMessage?: string;
}

/**
 * Options for interceptIndexAllStorage interceptor
 */
export interface InterceptIndexAllStorageOptions {
  /** Simulated network delay in milliseconds before response (default: 100) */
  delay?: number;
  /** HTTP status code to return (default: 200) */
  statusCode?: number;
  /** When true, return error response instead of success */
  errorMode?: boolean;
  /** Custom error message for error responses */
  errorMessage?: string;
}

/**
 * Options for interceptIndexStorageBatch helper
 */
export interface InterceptIndexStorageBatchOptions {
  /** Simulated delay for all interceptors */
  delay?: number;
  /** Replicate errors for specific device/storage combos */
  failingCombos?: Array<{ deviceId: string; storageType: 'USB' | 'SD' }>;
}

/**
 * Intercepts POST /devices/{deviceId}/storage/{storageType}/index
 * Mocks single device storage indexing endpoint.
 *
 * **Endpoint Pattern**: POST http://localhost:5168/devices/{deviceId}/storage/{storageType}/index
 * **Storage Type Values**: 'USB' or 'SD' (uppercase)
 * **Success Response**: 200 OK with empty body
 * **Error Response**: 400 Bad Request or 500 Internal Server Error with ProblemDetails
 *
 * @param deviceId - Device identifier to match in request path
 * @param storageType - Storage type ('USB' or 'SD')
 * @param options - Control delay, status code, and error mode
 *
 * @example Index USB storage with 2-second delay
 * ```typescript
 * interceptIndexStorage('device-123', 'USB', { delay: 2000 });
 * ```
 *
 * @example Simulate indexing error
 * ```typescript
 * interceptIndexStorage('device-123', 'SD', { errorMode: true, errorMessage: 'Storage unavailable' });
 * ```
 *
 * @example Register with alias for cy.wait()
 * ```typescript
 * interceptIndexStorage('device-123', 'USB', { delay: 1500 });
 * cy.wait('@indexStorage_device-123_USB', { timeout: 10000 });
 * ```
 */
export function interceptIndexStorage(
  deviceId: string,
  storageType: 'USB' | 'SD',
  options: InterceptIndexStorageOptions = {}
): void {
  const {
    delay = 100,
    statusCode = 200,
    errorMode = false,
    errorMessage = 'Failed to index storage',
  } = options;

  const alias = INDEXING_INTERCEPT_ALIASES.byDeviceAndType(deviceId, storageType);

  cy.intercept(
    INDEXING_ENDPOINTS.INDEX_STORAGE.method,
    INDEXING_ENDPOINTS.INDEX_STORAGE.pattern(storageType),
    (req) => {
      if (errorMode) {
        const errorStatus = statusCode || 400;
        req.reply(
          createProblemDetailsResponse(
            errorStatus,
            errorMessage,
            `Failed to index ${storageType} storage for device ${deviceId}`
          )
        );
      } else {
        // Success response - empty body with 200 OK
        req.reply({
          statusCode: statusCode || 200,
          body: {},
          delay,
        });
      }
    }
  ).as(alias);
}

/**
 * Intercepts POST /files/index/all
 * Mocks batch indexing endpoint for "Index All" operation.
 *
 * **Endpoint**: POST http://localhost:5168/files/index/all
 * **Success Response**: 200 OK with empty body
 * **Error Response**: 404 Not Found or 500 Internal Server Error with ProblemDetails
 *
 * Use this for testing the "Index All" toolbar button behavior across multiple devices.
 *
 * @param options - Control delay, status code, and error mode
 *
 * @example "Index All" with 3-second delay
 * ```typescript
 * interceptIndexAllStorage({ delay: 3000 });
 * ```
 *
 * @example Simulate API error on Index All
 * ```typescript
 * interceptIndexAllStorage({
 *   errorMode: true,
 *   errorMessage: 'Server temporarily unavailable'
 * });
 * ```
 *
 * @example Register with alias for cy.wait()
 * ```typescript
 * interceptIndexAllStorage({ delay: 2000 });
 * cy.get(DEVICE_TOOLBAR_INDEXING_SELECTORS.indexAllButton).click();
 * cy.wait('@indexAllStorage', { timeout: 15000 });
 * ```
 */
export function interceptIndexAllStorage(
  options: InterceptIndexAllStorageOptions = {}
): void {
  const {
    delay = 100,
    statusCode = 200,
    errorMode = false,
    errorMessage = 'Failed to index all storage',
  } = options;

  cy.intercept(
    INDEXING_ENDPOINTS.INDEX_ALL_STORAGE.method,
    INDEXING_ENDPOINTS.INDEX_ALL_STORAGE.pattern,
    (req) => {
      if (errorMode) {
        const errorStatus = statusCode || 404;
        req.reply(
          createProblemDetailsResponse(
            errorStatus,
            errorMessage,
            'The batch indexing operation failed'
          )
        );
      } else {
        // Success response - empty body with 200 OK
        req.reply({
          statusCode: statusCode || 200,
          body: {},
          delay,
        });
      }
    }
  ).as(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
}

/**
 * Batch setup helper for intercepting multiple single-device indexing operations.
 *
 * **IMPORTANT**: This is for testing SEQUENCES of single-device indexing operations,
 * NOT for the "Index All" endpoint. Use this when you want to:
 * - Test indexing Device 1 USB, then Device 1 SD separately
 * - Verify each individual device/storage indexing API call
 * - Simulate partial failures across multiple manual operations
 *
 * **For "Index All"**: Use `interceptIndexAllStorage()` instead - it's a single API call.
 *
 * @param deviceStorageCombos - Array of device/storage combinations to setup interceptors for
 * @param options - Applied to all interceptors (delay, failing combos)
 *
 * @example Setup for manual sequential indexing (3 separate API calls)
 * ```typescript
 * interceptIndexStorageBatch([
 *   { deviceId: 'dev-1', storageType: 'USB' },
 *   { deviceId: 'dev-1', storageType: 'SD' },
 *   { deviceId: 'dev-2', storageType: 'USB' },
 * ], { delay: 2000 });
 *
 * // User clicks USB index on Device 1
 * cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).first().click();
 * cy.wait('@indexStorage_dev-1_USB');
 *
 * // User clicks SD index on Device 1
 * cy.get(STORAGE_INDEX_BUTTON_SELECTORS.sd).first().click();
 * cy.wait('@indexStorage_dev-1_SD');
 *
 * // User clicks USB index on Device 2
 * cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).eq(1).click();
 * cy.wait('@indexStorage_dev-2_USB');
 * ```
 *
 * @example Setup with one failure
 * ```typescript
 * interceptIndexStorageBatch([
 *   { deviceId: 'dev-1', storageType: 'USB' },
 *   { deviceId: 'dev-1', storageType: 'SD' },
 * ], {
 *   delay: 1000,
 *   failingCombos: [{ deviceId: 'dev-1', storageType: 'SD' }]
 * });
 * ```
 */
export function interceptIndexStorageBatch(
  deviceStorageCombos: Array<{ deviceId: string; storageType: 'USB' | 'SD' }>,
  options: InterceptIndexStorageBatchOptions = {}
): void {
  const { delay = 100, failingCombos = [] } = options;

  // Create a Set for O(1) lookup of failing combos
  const failingSet = new Set(
    failingCombos.map((combo) => `${combo.deviceId}|${combo.storageType}`)
  );

  deviceStorageCombos.forEach(({ deviceId, storageType }) => {
    const isFailingCombo = failingSet.has(`${deviceId}|${storageType}`);

    interceptIndexStorage(deviceId, storageType, {
      delay,
      errorMode: isFailingCombo,
      errorMessage: isFailingCombo
        ? `Failed to index ${storageType} storage for device ${deviceId}`
        : undefined,
    });
  });
}
