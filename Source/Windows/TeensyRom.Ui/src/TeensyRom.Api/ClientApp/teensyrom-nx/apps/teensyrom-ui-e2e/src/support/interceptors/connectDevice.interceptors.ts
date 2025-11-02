/// <reference types="cypress" />

import type { CartDto, ConnectDeviceResponse } from '@teensyrom-nx/data-access/api-client';
import { singleDevice } from '../test-data/fixtures';
import {
  interceptSuccess,
  interceptError,
  type EndpointDefinition,
} from './primitives/interceptor-primitives';

/**
 * connectDevice endpoint interceptor for device connection
 */

// ============================================================================
// ENDPOINT DEFINITION
// ============================================================================

/**
 * connectDevice endpoint configuration
 */
export const CONNECT_DEVICE_ENDPOINT: EndpointDefinition = {
  method: 'POST',
  pattern: 'http://localhost:5168/devices/*/connect',
  alias: 'connectDevice',
} as const;

// ============================================================================
// INTERFACES
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
// INTERCEPTOR FUNCTION
// ============================================================================

/**
 * Intercepts POST /devices/{deviceId}/connect - Device connection endpoint
 * Route matches any deviceId via wildcard: POST http://localhost:5168/devices/<wildcard>/connect
 */
export function interceptConnectDevice(options: InterceptConnectDeviceOptions = {}): void {
  const device = options.device ?? singleDevice.devices[0];

  if (options.errorMode) {
    interceptError(
      CONNECT_DEVICE_ENDPOINT,
      options.statusCode || 500,
      options.errorMessage || 'Internal Server Error',
      options.responseDelayMs
    );
    return;
  }

  const response: ConnectDeviceResponse = {
    connectedCart: device,
    message: `Connected to device ${device.deviceId}`,
  };

  interceptSuccess(CONNECT_DEVICE_ENDPOINT, response, options.responseDelayMs);
}

// ============================================================================
// WAIT FUNCTION
// ============================================================================

/**
 * Waits for connectDevice endpoint call to complete
 */
export function waitForConnectDevice(): void {
  cy.wait(`@${CONNECT_DEVICE_ENDPOINT.alias}`);
}

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

/**
 * Verifies connectDevice completed successfully and was called
 */
export function verifyConnectDeviceCompleted(): void {
  cy.get('@connectDevice').should('exist');
}

/**
 * Sets up connectDevice with default successful response
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
export function setupConnectDeviceError(
  statusCode = 500,
  errorMessage = 'Internal Server Error'
): void {
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
 */
export function getLastConnectDeviceRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get('@connectDevice');
}

// ============================================================================
// EXPORT CONSTANTS (BACKWARD COMPATIBILITY)
// ============================================================================

// Backward compatibility exports for existing import patterns
export const CONNECT_DEVICE_ALIAS = CONNECT_DEVICE_ENDPOINT.alias;
export const INTERCEPT_CONNECT_DEVICE = 'connectDevice';
export const CONNECT_DEVICE_METHOD = CONNECT_DEVICE_ENDPOINT.method;
export const CONNECT_DEVICE_PATH = CONNECT_DEVICE_ENDPOINT.pattern;
