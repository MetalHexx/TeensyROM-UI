/**
 * Device Fixture Types
 *
 * Type definitions for device mock fixtures used throughout E2E tests.
 * Fixtures provide pre-built, deterministic device scenarios for interceptors.
 *
 * @example Using a fixture in tests
 * ```typescript
 * import { singleDevice } from './fixtures';
 *
 * cy.intercept('GET', '/api/devices', {
 *   statusCode: 200,
 *   body: singleDevice
 * });
 * ```
 */
import type { CartDto } from '@teensyrom-nx/data-access/api-client';

/**
 * Represents a mock device fixture for E2E testing.
 *
 * Fixtures provide deterministic, pre-built device scenarios that align
 * with the API response structure for `/api/devices`. All fixtures use
 * Phase 1 generators to ensure type safety and consistency.
 *
 * @property devices - Readonly array of CartDto objects representing TeensyROM devices
 *
 * @example Single device fixture
 * ```typescript
 * const fixture: MockDeviceFixture = {
 *   devices: [generateDevice()]
 * };
 * ```
 *
 * @example Multiple devices fixture
 * ```typescript
 * const fixture: MockDeviceFixture = {
 *   devices: [
 *     generateDevice({ name: 'Device 1' }),
 *     generateDevice({ name: 'Device 2' })
 *   ]
 * };
 * ```
 *
 * @example Empty state fixture
 * ```typescript
 * const fixture: MockDeviceFixture = {
 *   devices: []
 * };
 * ```
 */
export interface MockDeviceFixture {
  readonly devices: readonly CartDto[];
}
