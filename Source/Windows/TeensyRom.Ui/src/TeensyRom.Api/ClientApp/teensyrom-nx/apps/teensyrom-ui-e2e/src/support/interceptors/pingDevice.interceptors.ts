/// <reference types="cypress" />

import type { PingDeviceResponse } from '@teensyrom-nx/data-access/api-client';
import {
  interceptSuccess,
  interceptError,
  interceptSequence,
  type EndpointDefinition,
} from './primitives/interceptor-primitives';

/**
 * pingDevice endpoint interceptor for device health checks
 * Migrated to primitive-based architecture for simplified maintenance
 */

// ============================================================================
// SECTION 1: ENDPOINT DEFINITION
// ============================================================================

/**
 * pingDevice endpoint configuration
 */
export const PING_DEVICE_ENDPOINT: EndpointDefinition = {
  method: 'GET',
  pattern: 'http://localhost:5168/devices/*/ping',
  alias: 'pingDevice',
} as const;

// ============================================================================
// SECTION 2: INTERFACE DEFINITIONS
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
// SECTION 3: INTERCEPTOR FUNCTION
// ============================================================================

/**
 * Intercepts GET /devices/{deviceId}/ping - Device health check endpoint
 * Route matches any deviceId via wildcard: GET http://localhost:5168/devices/<wildcard>/ping
 *
 * @param options Configuration options for the interceptor
 */
export function interceptPingDevice(options: InterceptPingDeviceOptions = {}): void {
  const isAlive = options.isAlive ?? true;

  if (options.errorMode) {
    interceptError(
      PING_DEVICE_ENDPOINT,
      options.statusCode || 500,
      options.errorMessage || 'Internal Server Error',
      options.responseDelayMs
    );
    return;
  }

  const response: PingDeviceResponse = {
    message: isAlive ? 'Device is responding' : 'Device is not responding',
  };

  interceptSuccess(PING_DEVICE_ENDPOINT, response, options.responseDelayMs);
}

// ============================================================================
// SECTION 4: WAIT FUNCTION
// ============================================================================

/**
 * Waits for pingDevice endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForPingDevice(): void {
  cy.wait(`@${PING_DEVICE_ENDPOINT.alias}`);
}

// ============================================================================
// SECTION 5: HELPER FUNCTIONS
// ============================================================================

/**
 * Sets up pingDevice interceptor with device responding as alive
 *
 * @param options Additional interceptor options
 */
export function setupAlivePingDevice(
  options: Omit<InterceptPingDeviceOptions, 'isAlive'> = {}
): void {
  interceptPingDevice({
    ...options,
    isAlive: true,
  });
}

/**
 * Sets up pingDevice interceptor with device not responding
 *
 * @param options Additional interceptor options
 */
export function setupDeadPingDevice(
  options: Omit<InterceptPingDeviceOptions, 'isAlive'> = {}
): void {
  interceptPingDevice({
    ...options,
    isAlive: false,
  });
}

/**
 * Sets up pingDevice interceptor with a delay
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedPingDevice(
  delayMs: number,
  options: InterceptPingDeviceOptions = {}
): void {
  interceptPingDevice({
    ...options,
    responseDelayMs: delayMs,
  });
}

/**
 * Sets up pingDevice interceptor that always returns an error
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
 */
export function verifyPingDeviceRequested(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${PING_DEVICE_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the pingDevice endpoint
 */
export function getLastPingDeviceRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${PING_DEVICE_ENDPOINT.alias}`);
}

/**
 * Creates a sequence of ping responses to test connection state changes
 *
 * @param responses Array of boolean values where true = alive, false = dead
 * @param delayBetweenMs Delay between each response in milliseconds
 */
export function setupPingResponseSequence(responses: boolean[], delayBetweenMs = 1000): void {
  const sequenceResponses = responses.map((isAlive) => ({
    message: isAlive ? 'Device is responding' : 'Device is not responding',
  }));

  interceptSequence(PING_DEVICE_ENDPOINT, sequenceResponses, delayBetweenMs);
}

// ============================================================================
// SECTION 6: EXPORT CONSTANTS (BACKWARD COMPATIBILITY)
// ============================================================================

export const PING_DEVICE_ALIAS = PING_DEVICE_ENDPOINT.alias;
export const INTERCEPT_PING_DEVICE = 'pingDevice';
export const PING_DEVICE_METHOD = PING_DEVICE_ENDPOINT.method;
