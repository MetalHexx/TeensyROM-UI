/// <reference types="cypress" />

import type {
  DisconnectDeviceResponse,
} from '@teensyrom-nx/data-access/api-client';

/**
 * disconnectDevice endpoint interceptor for device disconnection
 * This file consolidates all disconnectDevice-related testing functionality
 */

// ============================================================================
// Section 1: Endpoint Definition
// ============================================================================

/**
 * disconnectDevice endpoint configuration
 */
export const DISCONNECT_DEVICE_ENDPOINT = {
  method: 'DELETE',
  path: (deviceId: string) => `/devices/${deviceId}`,
  full: (deviceId: string) => `http://localhost:5168/devices/${deviceId}`,
  pattern: 'http://localhost:5168/devices/*',
  alias: 'disconnectDevice'
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

/**
 * Options for interceptDisconnectDevice interceptor
 */
export interface InterceptDisconnectDeviceOptions {
  /** When true, return HTTP 500 error to simulate disconnection failure */
  errorMode?: boolean;
  /** Custom HTTP status code for error responses (default: 500) */
  statusCode?: number;
  /** Custom error message for error responses */
  errorMessage?: string;
  /** Custom response delay in milliseconds (default: 0) */
  responseDelayMs?: number;
}

// ============================================================================
// Section 3: Interceptor Function
// ============================================================================

/**
 * Intercepts DELETE /devices/{deviceId} - Device disconnection endpoint
 * Route matches any deviceId via wildcard: DELETE http://localhost:5168/devices/<wildcard>
 *
 * @param options Configuration options for the interceptor
 */
export function interceptDisconnectDevice(options: InterceptDisconnectDeviceOptions = {}): void {
  cy.intercept(
    DISCONNECT_DEVICE_ENDPOINT.method,
    DISCONNECT_DEVICE_ENDPOINT.pattern,
    (req) => {
      // Apply response delay if specified
      if (options.responseDelayMs && options.responseDelayMs > 0) {
        // Note: Cypress doesn't support req.delay() like req.reply({ delay }),
        // so we handle this by using setTimeout in the reply
      }

      if (options.errorMode) {
        const statusCode = options.statusCode || 500;
        const errorMessage = options.errorMessage || 'Internal Server Error';

        req.reply({
          statusCode,
          headers: {
            'content-type': 'application/problem+json',
          },
          body: {
            type: `https://tools.ietf.org/html/rfc9110#section-${getRfcSection(statusCode)}`,
            title: getErrorTitle(statusCode),
            status: statusCode,
            detail: errorMessage,
          },
        });
        return;
      }

      // Success response
      const response: DisconnectDeviceResponse = {
        message: 'Device disconnected successfully',
      };

      if (options.responseDelayMs && options.responseDelayMs > 0) {
        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: response,
          delay: options.responseDelayMs,
        });
      } else {
        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: response,
        });
      }
    }
  ).as(DISCONNECT_DEVICE_ENDPOINT.alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for disconnectDevice endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForDisconnectDevice(): void {
  cy.wait(`@${DISCONNECT_DEVICE_ENDPOINT.alias}`);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

/**
 * Sets up disconnectDevice interceptor with a delay
 * Useful for testing loading states during disconnection
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedDisconnectDevice(delayMs: number, options: InterceptDisconnectDeviceOptions = {}): void {
  interceptDisconnectDevice({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up disconnectDevice interceptor that always returns an error
 * Useful for testing error handling during disconnection
 *
 * @param statusCode HTTP status code for the error (default: 500)
 * @param errorMessage Custom error message
 */
export function setupErrorDisconnectDevice(statusCode = 500, errorMessage?: string): void {
  interceptDisconnectDevice({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Verifies that a disconnectDevice request was made
 * Useful for validation in tests
 */
export function verifyDisconnectDeviceRequested(): Cypress.Chainable<any> {
  return cy.get(`@${DISCONNECT_DEVICE_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the disconnectDevice endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastDisconnectDeviceRequest(): Cypress.Chainable<any> {
  return cy.get(`@${DISCONNECT_DEVICE_ENDPOINT.alias}`);
}

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const DISCONNECT_DEVICE_ALIAS = DISCONNECT_DEVICE_ENDPOINT.alias;
export const INTERCEPT_DISCONNECT_DEVICE = 'disconnectDevice';
export const DISCONNECT_DEVICE_METHOD = DISCONNECT_DEVICE_ENDPOINT.method;

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