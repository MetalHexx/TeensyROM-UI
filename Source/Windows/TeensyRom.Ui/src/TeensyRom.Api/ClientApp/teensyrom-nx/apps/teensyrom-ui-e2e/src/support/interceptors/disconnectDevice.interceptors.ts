/// <reference types="cypress" />

import type { DisconnectDeviceResponse } from '@teensyrom-nx/data-access/api-client';
import {
  interceptSuccess,
  interceptError,
  type EndpointDefinition,
  type CypressRequest,
  type CypressMethod,
} from './primitives/interceptor-primitives';

interface WindowWithDisconnectCallCount {
  __disconnectDeviceCallCount?: number;
}

export const DISCONNECT_DEVICE_ENDPOINT: EndpointDefinition = {
  method: 'DELETE',
  pattern: 'http://localhost:5168/devices/*',
  alias: 'disconnectDevice',
} as const;

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

export function waitForDisconnectDevice(): void {
  cy.wait(`@${DISCONNECT_DEVICE_ENDPOINT.alias}`);
}

/**
 * Wait for disconnectDevice API call to start
 * Used to create timing windows for race condition testing
 * @param timeout - Optional timeout in milliseconds (default: 2000ms for race testing)
 */
export function waitForDisconnectDeviceToStart(timeout = 2000): void {
  cy.wait(`@${DISCONNECT_DEVICE_ENDPOINT.alias}`, { timeout });
}

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
 * Sets up disconnectDevice with delayed response for testing loading states
 *
 * @param delayMs Delay in milliseconds
 */
export function setupDisconnectDeviceWithDelay(delayMs: number): void {
  interceptDisconnectDevice({
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

export function verifyDisconnectDeviceRequested(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(`@${DISCONNECT_DEVICE_ENDPOINT.alias}`);
}

function createDisconnectDeviceHeaders(contentType: string): Record<string, string> {
  return {
    'content-type': contentType,
    'cache-control': 'no-cache',
  };
}

function handleDeviceIdValidation(
  req: CypressRequest,
  expectedDeviceId: string,
  errorStatusCode: number,
  errorMessage: string
): void {
  if (!req.url.includes(expectedDeviceId)) {
    const problemDetails = {
      type: `https://tools.ietf.org/html/rfc9110#section-11.4.1`,
      title: errorStatusCode === 404 ? 'Not Found' : `Error (${errorStatusCode})`,
      status: errorStatusCode,
      detail: errorMessage,
    };

    req.reply({
      statusCode: errorStatusCode,
      headers: createDisconnectDeviceHeaders('application/problem+json'),
      body: problemDetails,
    });
    return;
  }

  const response: DisconnectDeviceResponse = {
    message: 'Device disconnected successfully',
  };

  req.reply({
    statusCode: 200,
    headers: createDisconnectDeviceHeaders('application/json'),
    body: response,
  });
}

/**
 * Sets up disconnectDevice with device ID validation
 *
 * Validates that the request URL contains the expected device ID before responding.
 * If the device ID matches, returns a successful DisconnectDeviceResponse.
 * If the device ID doesn't match, fails the request with a 404 Not Found error.
 *
 * **Implementation Note**: Uses direct cy.intercept() for custom request inspection logic
 * rather than primitives, as it requires URL validation before generating responses.
 *
 * @param expectedDeviceId The device ID that the request must contain
 *
 * @example
 * // Set up validation for disconnecting device "test-device-123"
 * setupDisconnectDeviceWithValidation('test-device-123');
 */
export function setupDisconnectDeviceWithValidation(expectedDeviceId: string): void {
  cy.intercept(
    DISCONNECT_DEVICE_ENDPOINT.method as CypressMethod,
    DISCONNECT_DEVICE_ENDPOINT.pattern,
    (req: CypressRequest) => {
      handleDeviceIdValidation(
        req,
        expectedDeviceId,
        404,
        `Device '${expectedDeviceId}' not found`
      );
    }
  ).as(DISCONNECT_DEVICE_ENDPOINT.alias);
}

/**
 * Sets up disconnectDevice with device ID validation and custom error handling
 *
 * Validates that the request URL contains the expected device ID.
 * If validation fails, responds with a customizable error response.
 *
 * **Implementation Note**: Uses direct cy.intercept() for custom request inspection logic
 * rather than primitives, as it requires URL validation before generating responses.
 *
 * @param expectedDeviceId The device ID that the request must contain
 * @param errorStatusCode HTTP status code to return on validation failure (default: 404)
 * @param errorMessage Custom error message for validation failure
 *
 * @example
 * // Set up validation with custom error response
 * setupDisconnectDeviceWithValidationAndError(
 *   'test-device-123',
 *   400,
 *   'Invalid device ID format'
 * );
 */
export function setupDisconnectDeviceWithValidationAndError(
  expectedDeviceId: string,
  errorStatusCode = 404,
  errorMessage = 'Device not found'
): void {
  cy.intercept(
    DISCONNECT_DEVICE_ENDPOINT.method as CypressMethod,
    DISCONNECT_DEVICE_ENDPOINT.pattern,
    (req: CypressRequest) => {
      handleDeviceIdValidation(req, expectedDeviceId, errorStatusCode, errorMessage);
    }
  ).as(DISCONNECT_DEVICE_ENDPOINT.alias);
}

/**
 * Sets up disconnectDevice with API call counting for tracking request frequency
 *
 * Intercepts the DELETE /devices/{deviceId} endpoint and tracks the number of API calls.
 * Maintains a call counter that can be queried and reset for testing refresh scenarios and
 * ensuring unnecessary API calls are not made.
 *
 * **Call Counter State Management**:
 * - Stores call count in Cypress state via cy.window()
 * - Accessible via getDisconnectDeviceCallCount()
 * - Resettable via resetDisconnectDeviceCallCount()
 * - Isolated between tests when reset is called
 *
 * **Implementation Note**: Uses direct cy.intercept() with custom state tracking logic
 * rather than primitives, as call count state tracking requires custom interceptor logic.
 *
 * @example
 * // Set up disconnection counting and verify API is called only once
 * setupDisconnectDeviceWithCounting();
 * // ... perform disconnection operation ...
 * getDisconnectDeviceCallCount().then((count) => {
 *   expect(count).to.equal(1);
 * });
 *
 * @example
 * // Reset counter for next test scenario
 * resetDisconnectDeviceCallCount();
 * setupDisconnectDeviceWithCounting();
 * // ... perform operation ...
 */
export function setupDisconnectDeviceWithCounting(): void {
  cy.window().then((win) => {
    (win as unknown as WindowWithDisconnectCallCount).__disconnectDeviceCallCount = 0;
  });

  cy.intercept(
    DISCONNECT_DEVICE_ENDPOINT.method as CypressMethod,
    DISCONNECT_DEVICE_ENDPOINT.pattern,
    (req: CypressRequest) => {
      cy.window().then((win) => {
        const count =
          ((win as unknown as WindowWithDisconnectCallCount).__disconnectDeviceCallCount ?? 0) + 1;
        (win as unknown as WindowWithDisconnectCallCount).__disconnectDeviceCallCount = count;
      });

      const response: DisconnectDeviceResponse = {
        message: 'Device disconnected successfully',
      };

      req.reply({
        statusCode: 200,
        headers: {
          'content-type': 'application/json',
          'cache-control': 'no-cache',
        },
        body: response,
      });
    }
  ).as(DISCONNECT_DEVICE_ENDPOINT.alias);
}

/**
 * Gets the current call count for disconnectDevice API
 *
 * Retrieves the number of times the disconnectDevice API has been called since
 * the last setup or reset. Returns 0 if the counter has not been initialized.
 *
 * **Usage Pattern**:
 * - Call after setupDisconnectDeviceWithCounting() to get the current count
 * - Useful for verifying API call behavior in refresh scenarios
 * - Returns a Cypress Chainable for integration with test assertions
 *
 * @returns Cypress Chainable that resolves to the current call count (default: 0)
 *
 * @example
 * setupDisconnectDeviceWithCounting();
 * // ... perform disconnection operation ...
 * getDisconnectDeviceCallCount().then((count) => {
 *   expect(count).to.equal(1);
 * });
 */
export function getDisconnectDeviceCallCount(): Cypress.Chainable<number> {
  return cy.window().then((win) => {
    return ((win as unknown as WindowWithDisconnectCallCount).__disconnectDeviceCallCount ??
      0) as number;
  });
}

/**
 * Resets the disconnectDevice API call counter to 0
 *
 * Clears the call counter maintained by setupDisconnectDeviceWithCounting().
 * Should be called in beforeEach() or at the start of each test scenario
 * to ensure test isolation and independent call counting.
 *
 * **Test Isolation Pattern**:
 * - Call in beforeEach() to reset state between tests
 * - Call before each setupDisconnectDeviceWithCounting() in sequential scenarios
 * - Ensures accurate call counting in each test independently
 *
 * @example
 * beforeEach(() => {
 *   resetDisconnectDeviceCallCount();
 * });
 *
 * it('should count API calls', () => {
 *   setupDisconnectDeviceWithCounting();
 *   // ... perform operation ...
 *   getDisconnectDeviceCallCount().then((count) => {
 *     expect(count).to.equal(1);
 *   });
 * });
 */
export function resetDisconnectDeviceCallCount(): void {
  cy.window().then((win) => {
    (win as unknown as WindowWithDisconnectCallCount).__disconnectDeviceCallCount = 0;
  });
}

export const DISCONNECT_DEVICE_ALIAS = DISCONNECT_DEVICE_ENDPOINT.alias;
