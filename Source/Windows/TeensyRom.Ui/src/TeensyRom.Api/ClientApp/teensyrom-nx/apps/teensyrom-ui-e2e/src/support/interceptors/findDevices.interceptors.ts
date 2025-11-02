/// <reference types="cypress" />

import type { FindDevicesResponse } from '@teensyrom-nx/data-access/api-client';
import { singleDevice } from '../test-data/fixtures';
import type { MockDeviceFixture } from '../test-data/fixtures/fixture.types';
import {
  interceptSuccess,
  interceptError,
  interceptNetworkError,
  type EndpointDefinition,
} from './primitives/interceptor-primitives';

/**
 * findDevices endpoint interceptor for device discovery
 * Migrated to primitive-based architecture for simplified maintenance
 */

// ============================================================================
// ENDPOINT DEFINITION
// ============================================================================

/**
 * findDevices endpoint configuration
 */
export const FIND_DEVICES_ENDPOINT: EndpointDefinition = {
  method: 'GET',
  pattern: 'http://localhost:5168/devices*',
  alias: 'findDevices',
} as const;

// ============================================================================
// INTERFACE DEFINITIONS
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
// INTERCEPTOR FUNCTION
// ============================================================================

/**
 * Intercepts GET /devices - Device discovery endpoint
 * Returns a list of discovered TeensyROM devices using the provided or default fixture.
 */
export function interceptFindDevices(options: InterceptFindDevicesOptions = {}): void {
  const fixture = options.fixture ?? singleDevice;

  if (options.errorMode) {
    interceptError(
      FIND_DEVICES_ENDPOINT,
      options.statusCode || 500,
      options.errorMessage || 'Internal Server Error',
      options.responseDelayMs
    );
    return;
  }

  const response: FindDevicesResponse = {
    devices: [...fixture.devices],
    message: `Found ${fixture.devices.length} device(s)`,
  };

  interceptSuccess(FIND_DEVICES_ENDPOINT, response, options.responseDelayMs);
}

// ============================================================================
// WAIT FUNCTION
// ============================================================================

/**
 * Waits for findDevices endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForFindDevices(): void {
  cy.wait(`@${FIND_DEVICES_ENDPOINT.alias}`);
}

// ============================================================================
// HELPER FUNCTIONS
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
export function setupFindDevicesError(
  statusCode = 500,
  errorMessage = 'Internal Server Error'
): void {
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
  interceptNetworkError(FIND_DEVICES_ENDPOINT);
}

/**
 * Gets the last request made to the findDevices endpoint
 * Useful for verifying request parameters in tests
 *
 * @returns Cypress.Chainable containing the intercepted request object with
 * properties like url, method, headers, body, and response information
 */
export function getLastFindDevicesRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get('@findDevices');
}

// ============================================================================
// EXPORT CONSTANTS (BACKWARD COMPATIBILITY)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const FIND_DEVICES_ALIAS = FIND_DEVICES_ENDPOINT.alias;
export const INTERCEPT_FIND_DEVICES = 'findDevices';
export const FIND_DEVICES_METHOD = FIND_DEVICES_ENDPOINT.method;
export const FIND_DEVICES_PATH = FIND_DEVICES_ENDPOINT.pattern;
