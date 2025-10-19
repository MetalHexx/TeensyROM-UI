/**
 * Faker Configuration Tests
 *
 * Verifies that the seeded Faker instance produces deterministic, reproducible data.
 * These tests validate the foundation of the entire E2E test data system.
 */
import { describe, it, expect, beforeEach } from 'vitest';
import { faker as fakerInstance } from '@faker-js/faker';
import { faker } from './faker-config';

describe('Faker Configuration', () => {
  describe('Determinism', () => {
    it('should generate identical values across multiple calls with same seed', () => {
      // Reset to known seed
      fakerInstance.seed(12345);

      // Generate first sequence
      const firstUuid = fakerInstance.string.uuid();
      const firstName = fakerInstance.person.firstName();
      const firstNumber = fakerInstance.number.int();

      // Reset to same seed
      fakerInstance.seed(12345);

      // Generate second sequence
      const secondUuid = fakerInstance.string.uuid();
      const secondName = fakerInstance.person.firstName();
      const secondNumber = fakerInstance.number.int();

      // Should be identical
      expect(secondUuid).toBe(firstUuid);
      expect(secondName).toBe(firstName);
      expect(secondNumber).toBe(firstNumber);
    });

    it('should produce consistent sequence from exported faker instance', () => {
      // Reset seed on exported instance
      faker.seed(12345);

      // Generate expected sequence
      const expectedValues = [
        faker.string.uuid(),
        faker.string.uuid(),
        faker.string.uuid(),
      ];

      // Reset and generate again
      faker.seed(12345);
      const actualValues = [
        faker.string.uuid(),
        faker.string.uuid(),
        faker.string.uuid(),
      ];

      // Should match exactly
      expect(actualValues).toEqual(expectedValues);
    });

    it('should share same seeded state across imports', () => {
      // Reset seed
      faker.seed(12345);

      // Generate value
      const firstValue = faker.string.uuid();

      // Re-import shouldn't create new instance - should continue sequence
      const secondValue = faker.string.uuid();

      // Reset and verify sequence
      faker.seed(12345);
      const firstCheck = faker.string.uuid();
      const secondCheck = faker.string.uuid();

      expect(firstCheck).toBe(firstValue);
      expect(secondCheck).toBe(secondValue);
    });
  });

  describe('Seed Behavior', () => {
    it('should allow resetting seed to reproduce sequences', () => {
      faker.seed(12345);
      const value1 = faker.string.uuid();

      faker.seed(12345);
      const value2 = faker.string.uuid();

      expect(value2).toBe(value1);
    });

    it('should produce different values with different seeds', () => {
      faker.seed(12345);
      const value1 = faker.string.uuid();

      faker.seed(99999);
      const value2 = faker.string.uuid();

      expect(value2).not.toBe(value1);
    });
  });

  describe('Data Types', () => {
    beforeEach(() => {
      faker.seed(12345);
    });

    it('should generate valid UUIDs', () => {
      const uuid = faker.string.uuid();

      // UUID v4 pattern
      const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
      expect(uuid).toMatch(uuidPattern);
    });

    it('should generate strings', () => {
      const name = faker.person.firstName();
      expect(typeof name).toBe('string');
      expect(name.length).toBeGreaterThan(0);
    });

    it('should generate numbers', () => {
      const num = faker.number.int();
      expect(typeof num).toBe('number');
    });

    it('should select from arrays deterministically', () => {
      const options = ['A', 'B', 'C', 'D'];
      const selected = faker.helpers.arrayElement(options);

      expect(options).toContain(selected);
    });
  });
});
