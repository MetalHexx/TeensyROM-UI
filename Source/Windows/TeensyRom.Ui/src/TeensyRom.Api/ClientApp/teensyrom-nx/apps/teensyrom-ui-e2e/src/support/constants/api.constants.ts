/**
 * E2E API Constants - Single source of truth for API endpoints
 * Note: TeensyROM API has NO /api prefix - routes go directly to base URL
 */

export const API_CONFIG = {
  BASE_URL: 'http://localhost:5168',
  TIMEOUT: 5000,
  CONTENT_TYPE_JSON: 'application/json',
  CONTENT_TYPE_PROBLEM_JSON: 'application/problem+json',
} as const;

// Device endpoints
export const DEVICE_ENDPOINTS = {
  FIND_DEVICES: {
    method: 'GET',
    path: '/devices',
    full: `${API_CONFIG.BASE_URL}/devices`,
    pattern: `${API_CONFIG.BASE_URL}/devices*`,
  },
  CONNECT_DEVICE: {
    method: 'POST',
    path: (deviceId: string) => `/devices/${deviceId}/connect`,
    full: (deviceId: string) => `${API_CONFIG.BASE_URL}/devices/${deviceId}/connect`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/connect`,
  },
  DISCONNECT_DEVICE: {
    method: 'DELETE',
    path: (deviceId: string) => `/devices/${deviceId}`,
    full: (deviceId: string) => `${API_CONFIG.BASE_URL}/devices/${deviceId}`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*`,
  },
  PING_DEVICE: {
    method: 'GET',
    path: (deviceId: string) => `/devices/${deviceId}/ping`,
    full: (deviceId: string) => `${API_CONFIG.BASE_URL}/devices/${deviceId}/ping`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/ping`,
  },
} as const;

// File endpoints
export const FILE_ENDPOINTS = {
  GET_DIRECTORY: {
    method: 'GET',
    path: '/files/directory',
    full: `${API_CONFIG.BASE_URL}/files/directory`,
    pattern: `${API_CONFIG.BASE_URL}/files/directory*`,
  },
} as const;

// HTTP status codes
export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  NOT_FOUND: 404,
  INTERNAL_SERVER_ERROR: 500,
} as const;

// Cypress interceptor aliases
export const INTERCEPT_ALIASES = {
  FIND_DEVICES: 'findDevices',
  CONNECT_DEVICE: 'connectDevice',
  DISCONNECT_DEVICE: 'disconnectDevice',
  PING_DEVICE: 'pingDevice',
} as const;

// Error messages
export const ERROR_MESSAGES = {
  DEVICE_NOT_FOUND: 'Device not found',
  NO_DEVICES_FOUND: 'No TeensyRom devices found.',
  CONNECTION_FAILED: 'Failed to connect to device',
  DISCONNECTION_FAILED: 'Failed to disconnect from device',
  INTERNAL_SERVER_ERROR: 'Internal Server Error',
  NETWORK_ERROR: 'Network error',
  TIMEOUT: 'Request timeout',
} as const;

// Helpers
export function buildUrl(
  endpoint: { full: string | ((...args: string[]) => string) },
  ...args: string[]
): string {
  if (typeof endpoint.full === 'function') {
    return endpoint.full(...args);
  }
  return endpoint.full;
}

export function getInterceptPattern(endpoint: { pattern: string }): string {
  return endpoint.pattern;
}

export function getHttpMethod(endpoint: { method: string }): string {
  return endpoint.method;
}

/**
 * Build a ProblemDetails error response for API interceptors
 * @param statusCode HTTP status code (e.g., 404, 500)
 * @param title User-friendly error message
 * @param detail Optional technical details
 * @returns Object ready for cy.intercept() req.reply()
 */
export function createProblemDetailsResponse(
  statusCode: number,
  title: string,
  detail?: string
) {
  return {
    statusCode,
    headers: {
      'content-type': 'application/problem+json',
    },
    body: {
      type: `https://tools.ietf.org/html/rfc9110#section-${statusCode === 404 ? '15.5.5' : statusCode === 500 ? '15.6.1' : '15.5.5'}`,
      title,
      status: statusCode,
      ...(detail && { detail }),
    },
  };
}
