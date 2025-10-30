/// <reference types="cypress" />

import type {
  PingDeviceResponse,
} from '@teensyrom-nx/data-access/api-client';

/**
 * pingDevice endpoint interceptor for device health checks
 * This file consolidates all pingDevice-related testing functionality
 */

// ============================================================================
// Section 1: Endpoint Definition
// ============================================================================

/**
 * pingDevice endpoint configuration
 */
export const PING_DEVICE_ENDPOINT = {
  method: 'GET',
  path: (deviceId: string) => `/devices/${deviceId}/ping`,
  full: (deviceId: string) => `http://localhost:5168/devices/${deviceId}/ping`,
  pattern: 'http://localhost:5168/devices/*/ping',
  alias: 'pingDevice'
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

/**
 * Options for interceptPingDevice interceptor
 */
export interface InterceptPingDeviceOptions {
  /** When true (default) device responds as alive. When false, not responding */
  isAlive?: boolean;
  /** When true, return HTTP 500 error to simulate server error */
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
 * Intercepts GET /devices/{deviceId}/ping - Device health check endpoint
 * Route matches any deviceId via wildcard: GET http://localhost:5168/devices/<wildcard>/ping
 *
 * @param options Configuration options for the interceptor
 */
export function interceptPingDevice(options: InterceptPingDeviceOptions = {}): void {
  const isAlive = options.isAlive ?? true;

  cy.intercept(
    PING_DEVICE_ENDPOINT.method,
    PING_DEVICE_ENDPOINT.pattern,
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
      const response: PingDeviceResponse = {
        message: isAlive ? 'Device is responding' : 'Device is not responding',
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
  ).as(PING_DEVICE_ENDPOINT.alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for pingDevice endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForPingDevice(): void {
  cy.wait(`@${PING_DEVICE_ENDPOINT.alias}`);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

/**
 * Sets up pingDevice interceptor with device responding as alive
 * Useful for testing healthy device scenarios
 *
 * @param options Additional interceptor options
 */
export function setupAlivePingDevice(options: Omit<InterceptPingDeviceOptions, 'isAlive'> = {}): void {
  interceptPingDevice({
    ...options,
    isAlive: true,
  });
}

/**
 * Sets up pingDevice interceptor with device not responding
 * Useful for testing dead/unresponsive device scenarios
 *
 * @param options Additional interceptor options
 */
export function setupDeadPingDevice(options: Omit<InterceptPingDeviceOptions, 'isAlive'> = {}): void {
  interceptPingDevice({
    ...options,
    isAlive: false,
  });
}

/**
 * Sets up pingDevice interceptor with a delay
 * Useful for testing timeout scenarios and loading states
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedPingDevice(delayMs: number, options: InterceptPingDeviceOptions = {}): void {
  interceptPingDevice({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up pingDevice interceptor that always returns an error
 * Useful for testing health check failure scenarios
 *
 * @param statusCode HTTP status code for the error (default: 500)
 * @param errorMessage Custom error message
 */
export function setupErrorPingDevice(statusCode = 500, errorMessage?: string): void {
  interceptPingDevice({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Sets up pingDevice interceptor for timeout testing
 * Uses a very long delay to simulate timeout scenarios
 *
 * @param timeoutMs Timeout duration in milliseconds (default: 30000)
 */
export function setupTimeoutPingDevice(timeoutMs = 30000): void {
  interceptPingDevice({
    responseDelayMs: timeoutMs,
  });
}

/**
 * Verifies that a pingDevice request was made
 * Useful for validation in tests
 */
export function verifyPingDeviceRequested(): Cypress.Chainable<any> {
  return cy.get(`@${PING_DEVICE_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the pingDevice endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastPingDeviceRequest(): Cypress.Chainable<any> {
  return cy.get(`@${PING_DEVICE_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of ping responses to test connection state changes
 * Useful for testing device state transitions
 *
 * @param responses Array of boolean values where true = alive, false = dead
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupPingResponseSequence(responses: boolean[], delayBetweenMs = 1000): void {
  let currentIndex = 0;

  cy.intercept(
    PING_DEVICE_ENDPOINT.method,
    PING_DEVICE_ENDPOINT.pattern,
    (req) => {
      const isAlive = responses[currentIndex % responses.length];
      currentIndex++;

      const response: PingDeviceResponse = {
        message: isAlive ? 'Device is responding' : 'Device is not responding',
      };

      req.reply({
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
        delay: delayBetweenMs,
      });
    }
  ).as(`${PING_DEVICE_ENDPOINT.alias}_sequence`);
}

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const PING_DEVICE_ALIAS = PING_DEVICE_ENDPOINT.alias;
export const INTERCEPT_PING_DEVICE = 'pingDevice';
export const PING_DEVICE_METHOD = PING_DEVICE_ENDPOINT.method;

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