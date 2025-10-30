/// <reference types="cypress" />

import type {
  CartDto,
  ConnectDeviceResponse,
} from '@teensyrom-nx/data-access/api-client';
import { singleDevice } from '../test-data/fixtures';

/**
 * connectDevice endpoint interceptor for device connection
 * This file consolidates all connectDevice-related testing functionality
 */

// ============================================================================
// Section 1: Endpoint Definition
// ============================================================================

/**
 * connectDevice endpoint configuration
 */
export const CONNECT_DEVICE_ENDPOINT = {
  method: 'POST',
  path: '/devices/{deviceId}/connect',
  full: 'http://localhost:5168/devices/{deviceId}/connect',
  pattern: 'http://localhost:5168/devices/*/connect',
  alias: 'connectDevice'
} as const;

// ============================================================================
// Section 2: Interface Definitions
// ============================================================================

/**
 * Options for interceptConnectDevice interceptor
 */
export interface InterceptConnectDeviceOptions {
  /** Override default device (first device from singleDevice fixture) */
  device?: CartDto;
  /** When true, return HTTP error to simulate connection failure */
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

/**
 * Intercepts POST /devices/{deviceId}/connect - Device connection endpoint
 * Route matches any deviceId via wildcard: POST http://localhost:5168/devices/<wildcard>/connect
 */
export function interceptConnectDevice(options: InterceptConnectDeviceOptions = {}): void {
  const device = options.device ?? singleDevice.devices[0];

  cy.intercept(
    CONNECT_DEVICE_ENDPOINT.method,
    CONNECT_DEVICE_ENDPOINT.pattern,
    (req) => {
      if (options.errorMode) {
        const statusCode = options.statusCode || 500;
        const errorMessage = options.errorMessage || 'Internal Server Error';

        req.reply({
          statusCode,
          headers: { 'content-type': 'application/problem+json' },
          body: {
            type: `https://tools.ietf.org/html/rfc9110#section-${getRfcSection(statusCode)}`,
            title: getErrorTitle(statusCode),
            status: statusCode,
            detail: errorMessage,
          },
        });
        return;
      }

      // Handle response delay
      if (options?.responseDelayMs && options.responseDelayMs > 0) {
        cy.wait(options.responseDelayMs).then(() => {
          sendSuccessResponse();
        });
      } else {
        sendSuccessResponse();
      }

      function sendSuccessResponse() {
        const response: ConnectDeviceResponse = {
          connectedCart: device,
          message: `Connected to device ${device.deviceId}`,
        };
        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: response,
        });
      }
    }
  ).as(CONNECT_DEVICE_ENDPOINT.alias);
}

// ============================================================================
// Section 4: Wait Function
// ============================================================================

/**
 * Waits for connectDevice endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForConnectDevice(): void {
  cy.wait(`@${CONNECT_DEVICE_ENDPOINT.alias}`);
}

// ============================================================================
// Section 5: Helper Functions
// ============================================================================

/**
 * Verifies connectDevice completed successfully and was called
 */
export function verifyConnectDeviceCompleted(): void {
  cy.get('@connectDevice').should('exist');
}

/**
 * Sets up connectDevice with default successful response
 * Convenience function for common setup scenarios
 */
export function setupConnectDevice(): void {
  interceptConnectDevice();
}

/**
 * Sets up connectDevice with error response
 *
 * @param statusCode HTTP status code for error (default: 500)
 * @param errorMessage Custom error message
 */
export function setupConnectDeviceError(statusCode = 500, errorMessage = 'Internal Server Error'): void {
  interceptConnectDevice({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Sets up connectDevice with delayed response
 *
 * @param delayMs Delay in milliseconds
 * @param device Optional device to connect
 */
export function setupConnectDeviceWithDelay(delayMs: number, device?: CartDto): void {
  interceptConnectDevice({
    responseDelayMs: delayMs,
    device: device || undefined,
  });
}

/**
 * Sets up connectDevice with specific device
 *
 * @param device Device to use for connection response
 */
export function setupConnectDeviceWithDevice(device: CartDto): void {
  interceptConnectDevice({ device });
}

/**
 * Gets the last request made to the connectDevice endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastConnectDeviceRequest(): Cypress.Chainable<any> {
  return cy.get('@connectDevice');
}

// ============================================================================
// Section 6: Export Constants (Backward Compatibility)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const CONNECT_DEVICE_ALIAS = CONNECT_DEVICE_ENDPOINT.alias;
export const INTERCEPT_CONNECT_DEVICE = 'connectDevice';
export const CONNECT_DEVICE_METHOD = CONNECT_DEVICE_ENDPOINT.method;
export const CONNECT_DEVICE_PATH = CONNECT_DEVICE_ENDPOINT.path;

// ============================================================================
// Internal Helper Functions
// ============================================================================

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