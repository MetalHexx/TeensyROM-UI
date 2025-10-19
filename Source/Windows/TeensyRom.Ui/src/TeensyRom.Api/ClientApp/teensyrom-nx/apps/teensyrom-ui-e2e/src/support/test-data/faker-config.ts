/**
 * Faker Configuration with Fixed Seed
 *
 * This module provides a deterministically seeded Faker instance for E2E test data generation.
 * The fixed seed (12345) ensures 100% reproducible test data across all test runs and developers.
 *
 * **CRITICAL**: All test data generators MUST import faker from this file, NOT directly from @faker-js/faker.
 * Direct imports will use different random seeds and break test reproducibility.
 *
 * @example
 * ```typescript
 * // ✅ Correct - uses seeded faker
 * import { faker } from '../faker-config';
 * const id = faker.string.uuid();
 *
 * // ❌ Wrong - breaks determinism
 * import { faker } from '@faker-js/faker';
 * ```
 *
 * @see E2E_PLAN.md for overall testing strategy
 */
import { faker as fakerInstance } from '@faker-js/faker';

// Configure with fixed seed for reproducibility
fakerInstance.seed(12345);

/**
 * Seeded Faker instance for generating deterministic test data.
 *
 * Always import this instance instead of creating new Faker instances to maintain
 * test reproducibility. Same seed = same data sequence on every test run.
 */
export const faker = fakerInstance;
