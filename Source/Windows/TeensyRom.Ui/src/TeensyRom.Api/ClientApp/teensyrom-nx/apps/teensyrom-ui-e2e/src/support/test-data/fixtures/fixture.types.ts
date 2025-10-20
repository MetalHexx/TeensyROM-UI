/**
 * Device Fixture Types
 *
 * Type definitions for device mock fixtures used in E2E tests.
 * See E2E_FIXTURES.md for comprehensive documentation and usage patterns.
 */
import type { CartDto } from '@teensyrom-nx/data-access/api-client';

/**
 * Mock device fixture for E2E testing.
 * Pre-built, deterministic device scenarios aligned with API response structure.
 *
 * @property devices - Readonly array of CartDto objects
 * @see E2E_FIXTURES.md for available fixtures and usage examples
 */
export interface MockDeviceFixture {
  readonly devices: readonly CartDto[];
}
