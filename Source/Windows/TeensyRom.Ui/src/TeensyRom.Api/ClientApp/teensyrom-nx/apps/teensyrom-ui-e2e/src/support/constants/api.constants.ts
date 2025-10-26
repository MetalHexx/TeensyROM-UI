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

// ============================================================================
// Timeout Constants
// ============================================================================

export const TIMEOUTS = {
  API_RESPONSE: 10000,
  BUTTON_STATE_CHANGE: 500,
  DIALOG_APPEARANCE: 1000,
  INDEXING_COMPLETION: 15000, // Allow for slow indexing operations
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

// Storage endpoints
export const STORAGE_ENDPOINTS = {
  GET_DIRECTORY: {
    method: 'GET',
    path: (deviceId: string, storageType: string) =>
      `/devices/${deviceId}/storage/${storageType}/directories`,
    full: (deviceId: string, storageType: string) =>
      `${API_CONFIG.BASE_URL}/devices/${deviceId}/storage/${storageType}/directories`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/storage/*/directories*`,
  },
  SAVE_FAVORITE: {
    method: 'POST',
    path: (deviceId: string, storageType: string) =>
      `/devices/${deviceId}/storage/${storageType}/favorite`,
    full: (deviceId: string, storageType: string) =>
      `${API_CONFIG.BASE_URL}/devices/${deviceId}/storage/${storageType}/favorite`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/storage/*/favorite*`,
  },
  REMOVE_FAVORITE: {
    method: 'DELETE',
    path: (deviceId: string, storageType: string) =>
      `/devices/${deviceId}/storage/${storageType}/favorite`,
    full: (deviceId: string, storageType: string) =>
      `${API_CONFIG.BASE_URL}/devices/${deviceId}/storage/${storageType}/favorite`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/storage/*/favorite*`,
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

// Player endpoints
export const PLAYER_ENDPOINTS = {
  LAUNCH_FILE: {
    method: 'POST',
    path: (deviceId: string, storageType: string) =>
      `/devices/${deviceId}/storage/${storageType}/launch`,
    full: (deviceId: string, storageType: string) =>
      `${API_CONFIG.BASE_URL}/devices/${deviceId}/storage/${storageType}/launch`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/storage/*/launch*`,
  },
  LAUNCH_RANDOM: {
    method: 'POST',
    path: (deviceId: string, storageType: string) =>
      `/devices/${deviceId}/storage/${storageType}/random-launch`,
    full: (deviceId: string, storageType: string) =>
      `${API_CONFIG.BASE_URL}/devices/${deviceId}/storage/${storageType}/random-launch`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/storage/*/random-launch*`,
  },
} as const;

// Indexing endpoints
export const INDEXING_ENDPOINTS = {
  INDEX_STORAGE: {
    method: 'POST',
    path: (deviceId: string, storageType: 'USB' | 'SD') =>
      `/devices/${deviceId}/storage/${storageType}/index`,
    full: (deviceId: string, storageType: 'USB' | 'SD') =>
      `${API_CONFIG.BASE_URL}/devices/${deviceId}/storage/${storageType}/index`,
    pattern: (storageType: 'USB' | 'SD') =>
      `${API_CONFIG.BASE_URL}/devices/*/storage/${storageType}/index`,
  },
  INDEX_ALL_STORAGE: {
    method: 'POST',
    path: '/files/index/all',
    full: `${API_CONFIG.BASE_URL}/files/index/all`,
    pattern: `${API_CONFIG.BASE_URL}/files/index/all`,
  },
} as const;

// Note: INDEXING_ENDPOINTS moved to indexing.constants.ts to avoid duplication

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
  GET_DIRECTORY: 'getDirectory',
  SAVE_FAVORITE: 'saveFavorite',
  REMOVE_FAVORITE: 'removeFavorite',
  LAUNCH_FILE: 'launchFile',
  LAUNCH_RANDOM: 'launchRandom',
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
  // Indexing-specific errors
  STORAGE_NOT_AVAILABLE: 'Storage device is not available for indexing',
  INDEX_FAILED: 'Failed to index storage. Please try again.',
  INVALID_DEVICE: 'Device not found or invalid',
  API_ERROR: 'An error occurred while indexing storage',
  INDEXING_TIMEOUT: 'Indexing operation timed out',
  CONCURRENT_INDEXING: 'Another indexing operation is already in progress',
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
