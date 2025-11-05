/// <reference types="cypress" />

import { interceptSuccess, interceptError } from '../primitives/interceptor-primitives';
import type { EndpointDefinition } from '../primitives/interceptor-primitives';

/**
 * Example endpoint interceptor demonstrating how to build on primitives
 * This file serves as a template for creating per-endpoint interceptor files
 */

// Section 1: Endpoint Definition

/**
 * Sample endpoint configuration
 */
export const SAMPLE_ENDPOINT: EndpointDefinition = {
  method: 'GET',
  pattern: 'http://localhost:5168/api/sample*',
  alias: 'sampleEndpoint',
};

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
 * Options for interceptSampleEndpoint
 */
export interface InterceptSampleEndpointOptions {
  /** Override default response with custom fixture */
  fixture?: MockResponseData | MockResponseData[];
  /** When true, return HTTP 500 error to simulate API failure */
  errorMode?: boolean;
  /** Simulate network delay in milliseconds */
  responseDelayMs?: number;
  /** Custom error message for error mode */
  errorMessage?: string;
}

// Section 3: Interceptor Function

/**
 * Intercepts sample endpoint calls with configurable responses
 *
 * Built on primitive functions for consistency and maintainability.
 *
 * @param options Configuration options for the interceptor
 *
 * @example
 * // Basic usage with default response
 * interceptSampleEndpoint();
 *
 * @example
 * // Custom fixture
 * interceptSampleEndpoint({ fixture: customData });
 *
 * @example
 * // Error mode
 * interceptSampleEndpoint({ errorMode: true });
 *
 * @example
 * // Delayed response
 * interceptSampleEndpoint({ responseDelayMs: 2000 });
 */
export function interceptSampleEndpoint(options?: InterceptSampleEndpointOptions): void {
  if (options?.errorMode) {
    interceptError(
      SAMPLE_ENDPOINT,
      500,
      options.errorMessage || 'An error occurred processing this request',
      options.responseDelayMs
    );
  } else {
    interceptSuccess(
      SAMPLE_ENDPOINT,
      options?.fixture || getDefaultResponse(),
      options?.responseDelayMs
    );
  }
}

// Section 4: Wait Function

/**
 * Waits for sample endpoint call to complete
 */
export function waitForSampleEndpoint(): void {
  cy.wait(`@${SAMPLE_ENDPOINT.alias}`);
}

// Section 5: Helper Functions

/**
 * Verifies sample endpoint was called and completed
 */
export function verifySampleEndpointCompleted(): void {
  cy.get('@sampleEndpoint').should('exist');
}

/**
 * Gets the last request made to the sample endpoint
 * Useful for verifying request parameters and structure
 */
export function getLastSampleEndpointRequest(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get('@sampleEndpoint');
}

// Internal Helper

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
