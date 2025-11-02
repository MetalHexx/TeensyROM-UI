/// <reference types="cypress" />

import type { DisconnectDeviceResponse } from '@teensyrom-nx/data-access/api-client';
import {
  interceptSuccess,
  interceptError,
  type EndpointDefinition,
} from './primitives/interceptor-primitives';

export const DISCONNECT_DEVICE_ENDPOINT: EndpointDefinition = {
  method: 'DELETE',
  pattern: 'http://localhost:5168/devices/*',
  alias: 'disconnectDevice',
} as const;

// INTERFACE DEFINITIONS

/**
 * Options for disconnect device interceptor
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

/**
 * Intercepts DELETE /devices/{deviceId} endpoint for device disconnection
 * Route matches any deviceId via wildcard
 *
 * @param options Configuration options for the interceptor
 */
export function interceptDisconnectDevice(options: InterceptDisconnectDeviceOptions = {}): void {
  if (options.errorMode) {
    interceptError(
      DISCONNECT_DEVICE_ENDPOINT,
      options.statusCode || 500,
      options.errorMessage || 'Internal Server Error',
      options.responseDelayMs
    );
    return;
  }

  const response: DisconnectDeviceResponse = {
    message: 'Device disconnected successfully',
  };

  interceptSuccess(DISCONNECT_DEVICE_ENDPOINT, response, options.responseDelayMs);
}

// WAIT FUNCTION

/**
 * Waits for disconnectDevice endpoint call to complete
 */
export function waitForDisconnectDevice(): void {
  cy.wait(`@${DISCONNECT_DEVICE_ENDPOINT.alias}`);
}

// HELPER FUNCTIONS

/**
 * Sets up disconnectDevice interceptor with a delay for testing loading states
 *
 * @param delayMs Delay in milliseconds before response
 * @param options Additional interceptor options
 */
export function setupDelayedDisconnectDevice(
  delayMs: number,
  options: InterceptDisconnectDeviceOptions = {}
): void {
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
 */
export function verifyDisconnectDeviceRequested(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${DISCONNECT_DEVICE_ENDPOINT.alias}`);
}

/**
 * Gets the last request made to the disconnectDevice endpoint
 */
export function getLastDisconnectDeviceRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${DISCONNECT_DEVICE_ENDPOINT.alias}`);
}

export const DISCONNECT_DEVICE_ALIAS = DISCONNECT_DEVICE_ENDPOINT.alias;
