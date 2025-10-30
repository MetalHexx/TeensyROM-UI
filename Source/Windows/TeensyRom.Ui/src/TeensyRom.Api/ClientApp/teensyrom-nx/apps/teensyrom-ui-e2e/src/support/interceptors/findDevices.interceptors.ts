/// <reference types="cypress" />

import type {
  FindDevicesResponse,
} from '@teensyrom-nx/data-access/api-client';
import { singleDevice } from '../test-data/fixtures';
import type { MockDeviceFixture } from '../test-data/fixtures/fixture.types';

/**
 * Cypress interceptor reply options interface
 */
interface CypressReplyOptions {
  statusCode?: number;
  headers?: Record<string, string>;
  body?: unknown;
  delay?: number;
}

/**
 * findDevices endpoint interceptor for device discovery
 * This file consolidates all findDevices-related testing functionality
 */

// ============================================================================
// Section 1: Endpoint Definition
// ============================================================================

/**
 * findDevices endpoint configuration
 */
export const FIND_DEVICES_ENDPOINT = {
  method: 'GET',
  path: '/devices',
  full: 'http://localhost:5168/devices',
  pattern: 'http://localhost:5168/devices*',
  alias: 'findDevices'
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

/**
 * Options for interceptFindDevices interceptor
 */
export interface InterceptFindDevicesOptions {
  /** Override default singleDevice fixture with custom fixture */
  fixture?: MockDeviceFixture;
  /** When true, return HTTP error to simulate API failure */
  errorMode?: boolean;
  /** Simulate network delay in milliseconds */
  responseDelayMs?: number;
  /** Custom HTTP status code for error responses (default: 500) */
  statusCode?: number;
  /** Custom error message for error responses */
  errorMessage?: string;
}

// ============================================================================
// Section 3: Interceptor Function
// ============================================================================

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const FIND_DEVICES_ALIAS = FIND_DEVICES_ENDPOINT.alias;
export const INTERCEPT_FIND_DEVICES = 'findDevices';
export const FIND_DEVICES_METHOD = FIND_DEVICES_ENDPOINT.method;
export const FIND_DEVICES_PATH = FIND_DEVICES_ENDPOINT.path;

/**
 * Intercepts GET /devices - Device discovery endpoint
 * Returns a list of discovered TenesyROM devices using the provided or default fixture.
 *
 * Note: Intercepts requests to http://localhost:5168/devices which may have query params
 */
export function interceptFindDevices(options: InterceptFindDevicesOptions = {}): void {
  const fixture = options.fixture ?? singleDevice;

  cy.intercept(
    FIND_DEVICES_ENDPOINT.method,
    FIND_DEVICES_ENDPOINT.pattern,
    (req) => {
      if (options.errorMode) {
        const statusCode = options.statusCode || 500;
        const errorMessage = options.errorMessage || 'Internal Server Error';

        req.reply({
          statusCode,
          headers: { 'content-type': 'application/problem+json' },
          body: {
            type: `https://tools.ietf.org/html/rfc9110#section-${getRfcSection(statusCode)}`,
            title: errorMessage,
            status: statusCode,
            detail: getErrorTitle(statusCode),
          },
        });
        return;
      }

      // Send response with optional delay
      const response: FindDevicesResponse = {
        devices: [...fixture.devices],
        message: `Found ${fixture.devices.length} device(s)`,
      };

      const replyOptions: CypressReplyOptions = {
        statusCode: 200,
        headers: { 'content-type': 'application/json' },
        body: response,
      };

      // Add delay if specified
      if (options?.responseDelayMs && options.responseDelayMs > 0) {
        replyOptions.delay = options.responseDelayMs;
      }

      req.reply(replyOptions);
    }
  ).as(FIND_DEVICES_ENDPOINT.alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for findDevices endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForFindDevices(): void {
  cy.wait(`@${FIND_DEVICES_ENDPOINT.alias}`);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

/**
 * Verifies findDevices completed successfully and was called
 */
export function verifyFindDevicesCompleted(): void {
  cy.get('@findDevices').should('exist');
}

/**
 * Sets up findDevices with default successful response
 * Convenience function for common setup scenarios
 */
export function setupFindDevices(): void {
  interceptFindDevices();
}

/**
 * Sets up findDevices with error response
 *
 * @param statusCode HTTP status code for error (default: 500)
 * @param errorMessage Custom error message
 */
export function setupFindDevicesError(statusCode = 500, errorMessage = 'Internal Server Error'): void {
  interceptFindDevices({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Sets up findDevices with delayed response
 *
 * @param delayMs Delay in milliseconds
 * @param fixture Optional fixture data
 */
export function setupFindDevicesWithDelay(delayMs: number, fixture?: MockDeviceFixture): void {
  interceptFindDevices({
    responseDelayMs: delayMs,
    fixture: fixture || undefined,
  });
}

/**
 * Intercepts findDevices with 404 NotFound response
 * Convenience function for common "no devices found" scenario
 *
 * @param title Error title (default: 'No devices found')
 * @param detail Optional detailed error message
 */
export function interceptFindDevicesNotFound(title = 'No devices found'): void {
  interceptFindDevices({
    errorMode: true,
    statusCode: 404,
    errorMessage: title,
  });
}

/**
 * Intercepts findDevices with 500 InternalServerError response
 * Convenience function for common server error scenarios
 *
 * @param title Error title (default: 'Internal Server Error')
 * @param detail Optional detailed error message
 */
export function interceptFindDevicesInternalServerError(title = 'Internal Server Error'): void {
  interceptFindDevices({
    errorMode: true,
    statusCode: 500,
    errorMessage: title,
  });
}

/**
 * Intercepts findDevices with custom error response
 * Convenience function for custom error scenarios
 *
 * @param statusCode HTTP status code
 * @param title Error title/message
 * @param detail Optional detailed error message
 */
export function interceptFindDevicesError(statusCode: number, title: string): void {
  interceptFindDevices({
    errorMode: true,
    statusCode,
    errorMessage: title,
  });
}

/**
 * Intercepts findDevices with delayed response using custom fixture
 * Enhanced version that properly handles fixture parameter
 *
 * @param delayMs Delay in milliseconds
 * @param fixture Optional fixture data
 */
export function interceptFindDevicesWithDelay(delayMs: number, fixture?: MockDeviceFixture): void {
  interceptFindDevices({
    responseDelayMs: delayMs,
    fixture,
  });
}

/**
 * Intercepts findDevices with network error simulation
 * Convenience function for network failure scenarios
 */
export function interceptFindDevicesWithNetworkError(): void {
  cy.intercept(
    FIND_DEVICES_ENDPOINT.method,
    FIND_DEVICES_ENDPOINT.pattern,
    { forceNetworkError: true }
  ).as(FIND_DEVICES_ALIAS);
}

/**
 * Gets the last request made to the findDevices endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastFindDevicesRequest(): Cypress.Chainable<any> {
  return cy.get('@findDevices');
}

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