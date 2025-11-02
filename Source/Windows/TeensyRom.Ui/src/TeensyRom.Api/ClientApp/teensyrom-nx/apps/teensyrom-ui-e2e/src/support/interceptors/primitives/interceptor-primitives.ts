/// <reference types="cypress" />

/**
 * Interceptor Primitives Library
 *
 * Core primitive functions for E2E test API interception.
 * Provides ultra-minimal set of three functions covering 85% of common interceptor scenarios.
 */

// =========================================================================
// INTERFACES AND TYPE DEFINITIONS
// =========================================================================

export interface EndpointDefinition {
  method: string;
  pattern: string;
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

export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
}

/**
 * Cypress request interface for interceptors
 * Extended to match IncomingHttpRequest interface for better compatibility
 */
export interface CypressRequest {
  reply: (response: {
    statusCode?: number;
    headers?: Record<string, string>;
    body?: unknown;
    delay?: number;
  }) => void;
  url: string;
  method: string;
  headers: Record<string, string>;
  body: unknown;
  query: Record<string, unknown>;
  destroy: () => void;
  continue: () => void;
  redirect: (url: string) => void;
  httpVersion: string;
  resourceType: string;
  on: (event: string, handler: (...args: unknown[]) => void) => void;
}

/**
 * Cypress HTTP method type for interceptors
 */
export type CypressMethod =
  | 'ACL'
  | 'BIND'
  | 'CHECKOUT'
  | 'CONNECT'
  | 'COPY'
  | 'DELETE'
  | 'GET'
  | 'HEAD'
  | 'LINK'
  | 'LOCK'
  | 'M-SEARCH'
  | 'MERGE'
  | 'MKACTIVITY'
  | 'MKCALENDAR'
  | 'MKCOL'
  | 'MOVE'
  | 'NOTIFY'
  | 'OPTIONS'
  | 'PATCH'
  | 'POST'
  | 'PROPFIND'
  | 'PROPPATCH'
  | 'PURGE'
  | 'PUT'
  | 'REBIND'
  | 'REPORT'
  | 'SEARCH'
  | 'SOURCE'
  | 'SUBSCRIBE'
  | 'TRACE'
  | 'UNBIND'
  | 'UNLINK'
  | 'UNLOCK'
  | 'UNSUBSCRIBE';

// Helper Functions

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
    503: '11.5.7',
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
    503: 'Service Unavailable',
  };
  return titles[statusCode] || 'Error';
}

/**
 * Creates standard HTTP headers for responses
 * @param contentType - Content type header value
 * @param customHeaders - Additional custom headers
 * @returns Complete headers object compatible with Cypress
 */
function createHeaders(
  contentType: string,
  customHeaders?: Record<string, string>
): Record<string, string> {
  const baseHeaders = {
    'content-type': contentType,
    'cache-control': 'no-cache',
  };

  return {
    ...baseHeaders,
    ...customHeaders,
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
    detail: message,
  };
}

// =========================================================================
// CORE PRIMITIVE FUNCTIONS
// =========================================================================

/**
 * Intercepts API requests with successful responses
 *
 * @param endpoint - Endpoint definition with method, pattern, and alias
 * @param data - Optional response data payload
 * @param delay - Optional response delay in milliseconds
 * @param headers - Optional custom headers
 */
export function interceptSuccess(
  endpoint: EndpointDefinition,
  data?: unknown,
  delay?: number,
  headers?: Record<string, string>
): void {
  const responseHeaders = createHeaders('application/json', headers);

  cy.intercept(endpoint.method as CypressMethod, endpoint.pattern, (req: CypressRequest) => {
    req.reply({
      statusCode: 200,
      headers: responseHeaders,
      body: data || {},
      delay: delay || 0,
    });
  }).as(endpoint.alias);
}

/**
 * Intercepts API requests with error responses using RFC 9110 ProblemDetails format
 *
 * @param endpoint - Endpoint definition with method, pattern, and alias
 * @param statusCode - HTTP status code (defaults to 500)
 * @param message - Custom error message (defaults to generic message)
 * @param delay - Optional response delay in milliseconds
 * @param headers - Optional custom headers
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

  cy.intercept(endpoint.method as CypressMethod, endpoint.pattern, (req: CypressRequest) => {
    req.reply({
      statusCode,
      headers: responseHeaders,
      body: problemDetails,
      delay: delay || 0,
    });
  }).as(endpoint.alias);
}

/**
 * Intercepts API requests with sequential responses for progressive operations
 *
 * Perfect for indexing, device state changes, or scenarios requiring multiple responses over time.
 *
 * @param endpoint - Endpoint definition with method, pattern, and alias
 * @param responses - Array of responses that cycle through successive calls
 * @param delay - Optional delay between responses in milliseconds
 * @param headers - Optional custom headers
 */
export function interceptSequence(
  endpoint: EndpointDefinition,
  responses: unknown[] = [],
  delay?: number,
  headers?: Record<string, string>
): void {
  if (!responses || responses.length === 0) {
    console.warn(
      `interceptSequence: No responses provided for ${endpoint.alias}. Using empty array.`
    );
    responses = [{}];
  }

  let currentIndex = 0;
  const responseHeaders = createHeaders('application/json', headers);

  cy.intercept(endpoint.method as CypressMethod, endpoint.pattern, (req: CypressRequest) => {
    const currentResponse = responses[currentIndex % responses.length];

    const isError =
      currentResponse &&
      typeof currentResponse === 'object' &&
      'statusCode' in currentResponse &&
      (currentResponse as { statusCode?: number }).statusCode !== undefined &&
      (currentResponse as { statusCode: number }).statusCode >= 400;

    if (isError) {
      const errorResponse = currentResponse as {
        statusCode: number;
        message?: string;
        detail?: string;
      };
      const problemDetails = createProblemDetails(
        errorResponse.statusCode,
        errorResponse.message || errorResponse.detail || 'Error occurred'
      );

      req.reply({
        statusCode: errorResponse.statusCode,
        headers: createHeaders('application/problem+json', headers),
        body: problemDetails,
        delay: delay || 0,
      });
    } else {
      req.reply({
        statusCode: 200,
        headers: responseHeaders,
        body: currentResponse,
        delay: delay || 0,
      });
    }

    currentIndex++;
  }).as(endpoint.alias);
}

// Convenience Functions

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
  cy.intercept(endpoint.method as CypressMethod, endpoint.pattern, { forceNetworkError: true }).as(
    endpoint.alias
  );
}
