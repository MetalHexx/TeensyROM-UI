/// <reference types="cypress" />

import type { CartDto, ConnectDeviceResponse } from '@teensyrom-nx/data-access/api-client';
import { singleDevice } from '../test-data/fixtures';
import {
  interceptSuccess,
  interceptError,
  type EndpointDefinition,
  type CypressRequest,
  type CypressMethod,
} from './primitives/interceptor-primitives';


/**
 * Window state type for tracking connectDevice API call count
 */
interface WindowWithCallCount {
  __connectDeviceCallCount?: number;
}


/**
 * connectDevice endpoint configuration
 */
export const CONNECT_DEVICE_ENDPOINT: EndpointDefinition = {
  method: 'POST',
  pattern: 'http://localhost:5168/devices/*/connect',
  alias: 'connectDevice',
} as const;


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


/**
 * Waits for connectDevice endpoint call to complete
 */
export function waitForConnectDevice(): void {
  cy.wait(`@${CONNECT_DEVICE_ENDPOINT.alias}`);
}

/**
 * Wait for connectDevice API call to start
 * Used to create timing windows for race condition testing
 * @param timeout - Optional timeout in milliseconds (default: 2000ms for race testing)
 */
export function waitForConnectDeviceToStart(timeout = 2000): void {
  cy.wait(`@${CONNECT_DEVICE_ENDPOINT.alias}`, { timeout });
}


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

export function getLastConnectDeviceRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get('@connectDevice');
}

/**
 * Sets up connectDevice with device ID validation
 *
 * Validates that the request URL contains the expected device ID before responding.
 * If the device ID matches, returns a successful ConnectDeviceResponse.
 * If the device ID doesn't match, fails the request with a 404 Not Found error.
 *
 * **Implementation Note**: Uses direct cy.intercept() for custom request inspection logic
 * rather than primitives, as it requires URL validation before generating responses.
 *
 * @param expectedDeviceId The device ID that the request must contain
 * @param device Optional device to use for successful connection response
 *
 * @example
 * // Set up validation for connecting to device "test-device-123"
 * setupConnectDeviceWithValidation('test-device-123');
 *
 * // Set up validation with a custom device in response
 * setupConnectDeviceWithValidation('test-device-123', customDevice);
 */
export function setupConnectDeviceWithValidation(expectedDeviceId: string, device?: CartDto): void {
  const responseDevice = device ?? singleDevice.devices[0];

  cy.intercept(
    CONNECT_DEVICE_ENDPOINT.method as CypressMethod,
    CONNECT_DEVICE_ENDPOINT.pattern,
    (req: CypressRequest) => {
      if (!req.url.includes(expectedDeviceId)) {
        const problemDetails = {
          type: 'https://tools.ietf.org/html/rfc9110#section-11.4.1',
          title: 'Not Found',
          status: 404,
          detail: `Device '${expectedDeviceId}' not found`,
        };

        req.reply({
          statusCode: 404,
          headers: {
            'content-type': 'application/problem+json',
            'cache-control': 'no-cache',
          },
          body: problemDetails,
        });
        return;
      }

      const response: ConnectDeviceResponse = {
        connectedCart: responseDevice,
        message: `Connected to device ${responseDevice.deviceId}`,
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
  ).as(CONNECT_DEVICE_ENDPOINT.alias);
}

/**
 * Sets up connectDevice with device ID validation and custom error handling
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
 * @param device Optional device to use for successful connection response
 *
 * @example
 * // Set up validation with custom error response
 * setupConnectDeviceWithValidationAndError(
 *   'test-device-123',
 *   400,
 *   'Invalid device ID format'
 * );
 */
export function setupConnectDeviceWithValidationAndError(
  expectedDeviceId: string,
  errorStatusCode = 404,
  errorMessage = 'Device not found',
  device?: CartDto
): void {
  const responseDevice = device ?? singleDevice.devices[0];

  cy.intercept(
    CONNECT_DEVICE_ENDPOINT.method as CypressMethod,
    CONNECT_DEVICE_ENDPOINT.pattern,
    (req: CypressRequest) => {
      if (!req.url.includes(expectedDeviceId)) {
        const problemDetails = {
          type: `https://tools.ietf.org/html/rfc9110#section-11.4.1`,
          title: `Error (${errorStatusCode})`,
          status: errorStatusCode,
          detail: errorMessage,
        };

        req.reply({
          statusCode: errorStatusCode,
          headers: {
            'content-type': 'application/problem+json',
            'cache-control': 'no-cache',
          },
          body: problemDetails,
        });
        return;
      }

      const response: ConnectDeviceResponse = {
        connectedCart: responseDevice,
        message: `Connected to device ${responseDevice.deviceId}`,
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
  ).as(CONNECT_DEVICE_ENDPOINT.alias);
}

/**
 * Sets up connectDevice with API call counting for tracking request frequency
 *
 * Intercepts the POST /devices/{deviceId}/connect endpoint and tracks the number of API calls.
 * Maintains a call counter that can be queried and reset for testing refresh scenarios and
 * ensuring unnecessary API calls are not made.
 *
 * **Call Counter State Management**:
 * - Stores call count in Cypress state via cy.window()
 * - Accessible via getConnectDeviceCallCount()
 * - Resettable via resetConnectDeviceCallCount()
 * - Isolated between tests when reset is called
 *
 * **Implementation Note**: Uses direct cy.intercept() with custom state tracking logic
 * rather than primitives, as call count state tracking requires custom interceptor logic.
 *
 * @param device Optional device to use for successful connection response (defaults to singleDevice[0])
 *
 * @example
 * // Set up connection counting and verify API is called only once
 * setupConnectDeviceWithCounting();
 * // ... perform connection operation ...
 * getConnectDeviceCallCount().then((count) => {
 *   expect(count).to.equal(1);
 * });
 *
 * @example
 * // Reset counter for next test scenario
 * resetConnectDeviceCallCount();
 * setupConnectDeviceWithCounting();
 * // ... perform operation ...
 */
export function setupConnectDeviceWithCounting(device?: CartDto): void {
  const responseDevice = device ?? singleDevice.devices[0];

  cy.window().then((win) => {
    (win as unknown as WindowWithCallCount).__connectDeviceCallCount = 0;
  });

  cy.intercept(
    CONNECT_DEVICE_ENDPOINT.method as CypressMethod,
    CONNECT_DEVICE_ENDPOINT.pattern,
    (req: CypressRequest) => {
      cy.window().then((win) => {
        const count = ((win as unknown as WindowWithCallCount).__connectDeviceCallCount ?? 0) + 1;
        (win as unknown as WindowWithCallCount).__connectDeviceCallCount = count;
      });

      const response: ConnectDeviceResponse = {
        connectedCart: responseDevice,
        message: `Connected to device ${responseDevice.deviceId}`,
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
  ).as(CONNECT_DEVICE_ENDPOINT.alias);
}

/**
 * Gets the current call count for connectDevice API
 *
 * Retrieves the number of times the connectDevice API has been called since
 * the last setup or reset. Returns 0 if the counter has not been initialized.
 *
 * **Usage Pattern**:
 * - Call after setupConnectDeviceWithCounting() to get the current count
 * - Useful for verifying API call behavior in refresh scenarios
 * - Returns a Cypress Chainable for integration with test assertions
 *
 * @returns Cypress Chainable that resolves to the current call count (default: 0)
 *
 * @example
 * setupConnectDeviceWithCounting();
 * // ... perform connection operation ...
 * getConnectDeviceCallCount().then((count) => {
 *   expect(count).to.equal(1);
 * });
 */
export function getConnectDeviceCallCount(): Cypress.Chainable<number> {
  return cy.window().then((win) => {
    return ((win as unknown as WindowWithCallCount).__connectDeviceCallCount ?? 0) as number;
  });
}

/**
 * Resets the connectDevice API call counter to 0
 *
 * Clears the call counter maintained by setupConnectDeviceWithCounting().
 * Should be called in beforeEach() or at the start of each test scenario
 * to ensure test isolation and independent call counting.
 *
 * **Test Isolation Pattern**:
 * - Call in beforeEach() to reset state between tests
 * - Call before each setupConnectDeviceWithCounting() in sequential scenarios
 * - Ensures accurate call counting in each test independently
 *
 * @example
 * beforeEach(() => {
 *   resetConnectDeviceCallCount();
 * });
 *
 * it('should count API calls', () => {
 *   setupConnectDeviceWithCounting();
 *   // ... perform operation ...
 *   getConnectDeviceCallCount().then((count) => {
 *     expect(count).to.equal(1);
 *   });
 * });
 */
export function resetConnectDeviceCallCount(): void {
  cy.window().then((win) => {
    (win as unknown as WindowWithCallCount).__connectDeviceCallCount = 0;
  });
}


export const CONNECT_DEVICE_ALIAS = CONNECT_DEVICE_ENDPOINT.alias;
export const INTERCEPT_CONNECT_DEVICE = 'connectDevice';
export const CONNECT_DEVICE_METHOD = CONNECT_DEVICE_ENDPOINT.method;
export const CONNECT_DEVICE_PATH = CONNECT_DEVICE_ENDPOINT.pattern;
