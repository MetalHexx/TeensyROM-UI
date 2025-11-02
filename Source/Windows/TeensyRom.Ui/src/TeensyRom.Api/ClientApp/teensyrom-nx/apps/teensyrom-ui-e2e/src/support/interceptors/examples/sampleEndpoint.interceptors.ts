/// <reference types="cypress" />

/**
 * Generic sample endpoint interceptor demonstrating the 6-section structure
 * This file serves as a template for creating per-endpoint interceptor files
 */

// Section 1: Endpoint Definition

/**
 * Sample endpoint configuration demonstrating the standard pattern
 */
export const SAMPLE_ENDPOINT = {
  method: 'GET',
  path: '/api/sample',
  full: 'http://localhost:5168/api/sample',
  pattern: 'http://localhost:5168/api/sample*',
  alias: 'sampleEndpoint'
} as const;

// Section 2: Interface Definitions

/**
 * Mock response data structure for sample endpoint
 */
export interface MockResponseData {
  id: string;
  name: string;
  description: string;
  status: 'active' | 'inactive' | 'pending';
  createdAt: string;
  updatedAt: string;
}

/**
 * Options for interceptSampleEndpoint interceptor
 */
export interface InterceptSampleEndpointOptions {
  /** Override default response with custom fixture or array of fixtures */
  fixture?: MockResponseData | MockResponseData[];
  /** When true, return HTTP error to simulate API failure */
  errorMode?: boolean;
  /** Simulate network delay in milliseconds */
  responseDelayMs?: number;
  /** Custom HTTP status code for error responses (default: 500) */
  statusCode?: number;
  /** Custom error message for error responses */
  errorMessage?: string;
}

// Section 3: Interceptor Function

/**
 * Intercepts sample endpoint calls with configurable responses
 *
 * @param options Configuration options for the interceptor
 */
export function interceptSampleEndpoint(options?: InterceptSampleEndpointOptions): void {
  cy.intercept(SAMPLE_ENDPOINT.method, SAMPLE_ENDPOINT.pattern, (req) => {
    if (options?.errorMode) {
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

    if (options?.responseDelayMs && options.responseDelayMs > 0) {
      cy.wait(options.responseDelayMs).then(() => {
        sendSuccessResponse();
      });
    } else {
      sendSuccessResponse();
    }

    function sendSuccessResponse() {
      if (options?.fixture) {
        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: options.fixture,
        });
      } else {
        req.reply({
          statusCode: 200,
          headers: { 'content-type': 'application/json' },
          body: getDefaultResponse(),
        });
      }
    }
  }).as(SAMPLE_ENDPOINT.alias);
}

// Section 4: Wait Function

/**
 * Waits for sample endpoint call to complete
 * Uses the registered alias from the interceptor
 */
export function waitForSampleEndpoint(): void {
  cy.wait(`@${SAMPLE_ENDPOINT.alias}`);
}

// Section 5: Helper Functions

/**
 * Verifies sample endpoint completed successfully and was called
 */
export function verifySampleEndpointCompleted(): void {
  cy.get('@sampleEndpoint').should('exist');
}

/**
 * Sets up sample endpoint with default successful response
 * Convenience function for common setup scenarios
 */
export function setupSampleEndpoint(): void {
  interceptSampleEndpoint();
}

/**
 * Sets up sample endpoint with error response
 *
 * @param statusCode HTTP status code for error (default: 500)
 * @param errorMessage Custom error message
 */
export function setupSampleEndpointError(statusCode = 500, errorMessage = 'Internal Server Error'): void {
  interceptSampleEndpoint({
    errorMode: true,
    statusCode,
    errorMessage,
  });
}

/**
 * Sets up sample endpoint with delayed response
 *
 * @param delayMs Delay in milliseconds
 * @param fixture Optional fixture data
 */
export function setupSampleEndpointWithDelay(delayMs: number, fixture?: MockResponseData): void {
  interceptSampleEndpoint({
    responseDelayMs: delayMs,
    fixture: fixture || undefined,
  });
}

/**
 * Gets the last request made to the sample endpoint
 * Useful for verifying request parameters in tests
 */
export function getLastSampleEndpointRequest(): Cypress.Chainable<any> {
  return cy.get('@sampleEndpoint');
}

// Section 6: Export Constants (Backward Compatibility)

// Backward compatibility exports for existing import patterns
export const SAMPLE_ENDPOINT_ALIAS = SAMPLE_ENDPOINT.alias;
export const INTERCEPT_SAMPLE_ENDPOINT = 'sampleEndpoint';
export const SAMPLE_ENDPOINT_METHOD = SAMPLE_ENDPOINT.method;
export const SAMPLE_ENDPOINT_PATH = SAMPLE_ENDPOINT.path;

// Internal Helper Functions

/**
 * Gets default response data for the sample endpoint
 */
function getDefaultResponse(): MockResponseData {
  return {
    id: 'default-123',
    name: 'Default Sample Item',
    description: 'This is a default response for the sample endpoint',
    status: 'active',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };
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