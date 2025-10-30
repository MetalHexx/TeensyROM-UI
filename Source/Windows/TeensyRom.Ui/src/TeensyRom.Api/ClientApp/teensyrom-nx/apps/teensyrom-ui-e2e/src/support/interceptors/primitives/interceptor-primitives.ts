/**
 * Interceptor Primitives Library
 *
 * Core primitive functions for E2E test API interception.
 * Provides ultra-minimal set of three functions covering 85% of common interceptor scenarios.
 *
 * @author TeensyROM E2E Testing Infrastructure
 * @version 1.0.0
 */

// ============================================================================
// Interfaces and Type Definitions
// ============================================================================

/**
 * Standard endpoint definition interface
 * Matches existing endpoint patterns in the codebase
 */
export interface EndpointDefinition {
  /** HTTP method (GET, POST, PUT, DELETE, etc.) */
  method: string;
  /** URL pattern for matching requests */
  pattern: string;
  /** Cypress alias for the interceptor */
  alias: string;
}

/**
 * HTTP status code type for better type safety
 */
export type StatusCode = 200 | 201 | 400 | 401 | 403 | 404 | 500 | 502 | 503;

/**
 * Standard HTTP headers for responses
 */
export interface ResponseHeaders {
  'content-type'?: string;
  'cache-control'?: string;
  'x-custom-header'?: string;
}

/**
 * RFC 9110 ProblemDetails error response format
 */
export interface ProblemDetails {
  /** URI reference to the error type */
  type: string;
  /** Human-readable summary of the error */
  title: string;
  /** HTTP status code */
  status: number;
  /** Human-readable explanation of the error */
  detail: string;
}

/**
 * Cypress request interface for interceptors
 */
export interface CypressRequest {
  reply: (response: {
    statusCode?: number;
    headers?: Record<string, string>;
    body?: unknown;
    delay?: number;
  }) => void;
}

// ============================================================================
// Helper Functions
// ============================================================================

/**
 * Maps HTTP status codes to RFC 9110 sections
 * @param statusCode - HTTP status code
 * @returns RFC 9110 section reference
 */
function getRfcSection(statusCode: number): string {
  const sections: Record<number, string> = {
    400: '11.5.1',
    401: '11.6.1',
    403: '11.5.3',
    404: '11.4.1',
    500: '12.0.0',
    502: '11.5.6',
    503: '11.5.7'
  };
  return sections[statusCode] || '12.0.0';
}

/**
 * Maps HTTP status codes to human-readable titles
 * @param statusCode - HTTP status code
 * @returns Human-readable error title
 */
function getErrorTitle(statusCode: number): string {
  const titles: Record<number, string> = {
    400: 'Bad Request',
    401: 'Unauthorized',
    403: 'Forbidden',
    404: 'Not Found',
    500: 'Internal Server Error',
    502: 'Bad Gateway',
    503: 'Service Unavailable'
  };
  return titles[statusCode] || 'Error';
}

/**
 * Creates standard HTTP headers for responses
 * @param contentType - Content type header value
 * @param customHeaders - Additional custom headers
 * @returns Complete headers object
 */
function createHeaders(contentType: string, customHeaders?: Record<string, string>): ResponseHeaders {
  return {
    'content-type': contentType,
    'cache-control': 'no-cache',
    ...customHeaders
  };
}

/**
 * Creates a standard ProblemDetails error response
 * @param statusCode - HTTP status code
 * @param message - Custom error message
 * @returns RFC 9110 compliant ProblemDetails object
 */
function createProblemDetails(statusCode: number, message: string): ProblemDetails {
  return {
    type: `https://tools.ietf.org/html/rfc9110#section-${getRfcSection(statusCode)}`,
    title: getErrorTitle(statusCode),
    status: statusCode,
    detail: message
  };
}

// ============================================================================
// Core Primitive Functions
// ============================================================================

/**
 * Intercepts API requests with successful responses
 *
 * @param endpoint - Endpoint definition with method, pattern, and alias
 * @param data - Optional response data payload
 * @param delay - Optional response delay in milliseconds
 * @param headers - Optional custom headers
 *
 * @example
 * ```typescript
 * // Simple success response
 * interceptSuccess(FIND_DEVICES_ENDPOINT)
 *
 * // Success with custom data
 * interceptSuccess(FIND_DEVICES_ENDPOINT, mockDevices)
 *
 * // Success with delay
 * interceptSuccess(FIND_DEVICES_ENDPOINT, mockDevices, 1000)
 * ```
 */
export function interceptSuccess(
  endpoint: EndpointDefinition,
  data?: unknown,
  delay?: number,
  headers?: Record<string, string>
): void {
  const responseHeaders = createHeaders('application/json', headers);

  cy.intercept(endpoint.method, endpoint.pattern, (req: CypressRequest) => {
    req.reply({
      statusCode: 200,
      headers: responseHeaders,
      body: data || {},
      delay: delay || 0
    });
  }).as(endpoint.alias);
}

/**
 * Intercepts API requests with error responses
 *
 * Uses RFC 9110 ProblemDetails format for standardized error responses.
 *
 * @param endpoint - Endpoint definition with method, pattern, and alias
 * @param statusCode - HTTP status code (defaults to 500)
 * @param message - Custom error message (defaults to generic message)
 * @param delay - Optional response delay in milliseconds
 * @param headers - Optional custom headers
 *
 * @example
 * ```typescript
 * // Generic error (defaults to 500)
 * interceptError(CONNECT_DEVICE_ENDPOINT)
 *
 * // Custom status code
 * interceptError(FIND_DEVICES_ENDPOINT, 404)
 *
 * // Custom status and message
 * interceptError(SAVE_FAVORITE_ENDPOINT, 400, 'Invalid file path')
 *
 * // Error with delay
 * interceptError(GET_DIRECTORY_ENDPOINT, 503, 'Service unavailable', 2000)
 * ```
 */
export function interceptError(
  endpoint: EndpointDefinition,
  statusCode = 500,
  message?: string,
  delay?: number,
  headers?: Record<string, string>
): void {
  const responseHeaders = createHeaders('application/problem+json', headers);
  const errorMessage = message || getErrorTitle(statusCode);
  const problemDetails = createProblemDetails(statusCode, errorMessage);

  cy.intercept(endpoint.method, endpoint.pattern, (req: CypressRequest) => {
    req.reply({
      statusCode,
      headers: responseHeaders,
      body: problemDetails,
      delay: delay || 0
    });
  }).as(endpoint.alias);
}

/**
 * Intercepts API requests with sequential responses
 *
 * Perfect for progressive operations like indexing, device state changes,
 * or any scenario that requires multiple different responses over time.
 *
 * @param endpoint - Endpoint definition with method, pattern, and alias
 * @param responses - Array of responses that cycle through successive calls
 * @param delay - Optional delay between responses in milliseconds
 * @param headers - Optional custom headers
 *
 * @example
 * ```typescript
 * // Progressive operation (indexing)
 * interceptSequence(INDEX_FILES_ENDPOINT, [
 *   { status: 202, message: 'Indexing started' },
 *   { status: 202, message: 'Indexing in progress', progress: 50 },
 *   { status: 200, message: 'Indexing complete', totalFiles: 1250 }
 * ])
 *
 * // Device state changes
 * interceptSequence(CONNECT_DEVICE_ENDPOINT, [
 *   { status: 'connecting' },
 *   { status: 'connected' },
 *   { status: 'ready' }
 * ])
 *
 * // Error recovery scenario
 * interceptSequence(REFRESH_DEVICES_ENDPOINT, [
 *   { statusCode: 500, message: 'Service temporarily unavailable' },
 *   { devices: mockDevices, status: 'healthy' }
 * ], 500)
 * ```
 */
export function interceptSequence(
  endpoint: EndpointDefinition,
  responses: unknown[] = [],
  delay?: number,
  headers?: Record<string, string>
): void {
  if (!responses || responses.length === 0) {
    console.warn(`interceptSequence: No responses provided for ${endpoint.alias}. Using empty array.`);
    responses = [{}];
  }

  let currentIndex = 0;
  const responseHeaders = createHeaders('application/json', headers);

  cy.intercept(endpoint.method, endpoint.pattern, (req: CypressRequest) => {
    const currentResponse = responses[currentIndex % responses.length];

    // Auto-detect if response is an error or success
    const isError = currentResponse &&
                   typeof currentResponse === 'object' &&
                   'statusCode' in currentResponse &&
                   (currentResponse as { statusCode?: number }).statusCode !== undefined &&
                   (currentResponse as { statusCode: number }).statusCode >= 400;

    if (isError) {
      const errorResponse = currentResponse as { statusCode: number; message?: string; detail?: string };
      const problemDetails = createProblemDetails(
        errorResponse.statusCode,
        errorResponse.message || errorResponse.detail || 'Error occurred'
      );

      req.reply({
        statusCode: errorResponse.statusCode,
        headers: createHeaders('application/problem+json', headers),
        body: problemDetails,
        delay: delay || 0
      });
    } else {
      req.reply({
        statusCode: 200,
        headers: responseHeaders,
        body: currentResponse,
        delay: delay || 0
      });
    }

    currentIndex++;
  }).as(endpoint.alias);
}

// ============================================================================
// Convenience Functions (Optional Enhancements)
// ============================================================================

/**
 * Convenience function for empty responses
 * @param endpoint - Endpoint definition
 * @param delay - Optional delay in milliseconds
 */
export function interceptEmpty(endpoint: EndpointDefinition, delay?: number): void {
  interceptSuccess(endpoint, {}, delay);
}

/**
 * Convenience function for network errors
 * @param endpoint - Endpoint definition
 */
export function interceptNetworkError(endpoint: EndpointDefinition): void {
  cy.intercept(endpoint.method, endpoint.pattern, { forceNetworkError: true }).as(endpoint.alias);
}

// ============================================================================
// Re-exports for Easy Import
// ============================================================================

export type {
  EndpointDefinition,
  StatusCode,
  ResponseHeaders,
  ProblemDetails,
  CypressRequest
};