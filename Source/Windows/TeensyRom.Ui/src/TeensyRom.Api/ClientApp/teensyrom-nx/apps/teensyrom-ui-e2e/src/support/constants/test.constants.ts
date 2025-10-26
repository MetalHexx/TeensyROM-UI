/**
 * Test Infrastructure Constants
 *
 * General-purpose constants for E2E test configuration.
 * These are not domain-specific and can be used across all test suites.
 */

/**
 * Standard viewport dimensions for E2E tests
 */
export const VIEWPORT = {
  /**
   * Standard desktop viewport matching responsive breakpoints
   * Width: 1400px ensures proper layout (1200px+ breakpoint)
   * Height: 900px provides adequate vertical space
   */
  STANDARD: {
    width: 1400,
    height: 900,
  },
} as const;

/**
 * Common timeout values for Cypress operations
 */
export const TIMEOUTS = {
  /**
   * Short timeout for fast operations (5 seconds)
   */
  SHORT: 5000,

  /**
   * Default timeout for most operations (10 seconds)
   * Used for directory loads, file renders, etc.
   */
  DEFAULT: 10000,

  /**
   * Long timeout for slow operations (30 seconds)
   */
  LONG: 30000,
} as const;

/**
 * Deterministic seeds for mock data generation
 */
export const MOCK_SEEDS = {
  /**
   * Default seed for consistent test data (12345)
   * Used in storage.generators.ts for createMockFilesystem()
   */
  DEFAULT: 12345,

  /**
   * Alternative seed for testing different data sets
   */
  ALTERNATIVE: 54321,
} as const;
