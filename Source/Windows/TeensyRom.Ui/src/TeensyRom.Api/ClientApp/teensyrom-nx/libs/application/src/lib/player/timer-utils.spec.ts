import { describe, it, expect } from 'vitest';
import { parsePlayLength } from './timer-utils';

describe('parsePlayLength', () => {
  describe('MM:SS format', () => {
    it('should parse valid MM:SS format correctly', () => {
      expect(parsePlayLength('3:45')).toBe(225000); // 3 minutes 45 seconds = 225000ms
      expect(parsePlayLength('0:30')).toBe(30000); // 30 seconds = 30000ms
      expect(parsePlayLength('10:00')).toBe(600000); // 10 minutes = 600000ms
      expect(parsePlayLength('59:59')).toBe(3599000); // 59 minutes 59 seconds = 3599000ms
    });

    it('should handle single-digit values', () => {
      expect(parsePlayLength('1:5')).toBe(65000); // 1 minute 5 seconds = 65000ms
      expect(parsePlayLength('0:0')).toBe(0); // 0 minutes 0 seconds = 0ms
    });

    it('should handle leading zeros', () => {
      expect(parsePlayLength('03:05')).toBe(185000); // 3 minutes 5 seconds = 185000ms
      expect(parsePlayLength('00:30')).toBe(30000); // 30 seconds = 30000ms
    });
  });

  describe('H:MM:SS format', () => {
    it('should parse valid H:MM:SS format correctly', () => {
      expect(parsePlayLength('1:02:30')).toBe(3750000); // 1 hour 2 minutes 30 seconds = 3750000ms
      expect(parsePlayLength('2:00:00')).toBe(7200000); // 2 hours = 7200000ms
      expect(parsePlayLength('0:15:45')).toBe(945000); // 15 minutes 45 seconds = 945000ms
    });

    it('should handle single-digit values', () => {
      expect(parsePlayLength('1:5:3')).toBe(3903000); // 1 hour 5 minutes 3 seconds = 3903000ms
    });

    it('should handle leading zeros', () => {
      expect(parsePlayLength('01:02:03')).toBe(3723000); // 1 hour 2 minutes 3 seconds = 3723000ms
    });
  });

  describe('invalid formats', () => {
    it('should return 0 for empty string', () => {
      expect(parsePlayLength('')).toBe(0);
    });

    it('should return 0 for whitespace-only string', () => {
      expect(parsePlayLength('   ')).toBe(0);
    });

    it('should return 0 for invalid format (single value)', () => {
      expect(parsePlayLength('123')).toBe(0);
    });

    it('should return 0 for invalid format (too many parts)', () => {
      expect(parsePlayLength('1:2:3:4')).toBe(0);
    });

    it('should return 0 for non-numeric values', () => {
      expect(parsePlayLength('ab:cd')).toBe(0);
      expect(parsePlayLength('1:2a')).toBe(0);
      expect(parsePlayLength('a:2:3')).toBe(0);
    });

    it('should return 0 for negative numbers', () => {
      expect(parsePlayLength('-1:30')).toBe(0);
      expect(parsePlayLength('1:-30')).toBe(0);
    });

    it('should return 0 for null or undefined', () => {
      expect(parsePlayLength(null as unknown as string)).toBe(0);
      expect(parsePlayLength(undefined as unknown as string)).toBe(0);
    });

    it('should return 0 for non-string values', () => {
      expect(parsePlayLength(123 as unknown as string)).toBe(0);
      expect(parsePlayLength({} as unknown as string)).toBe(0);
      expect(parsePlayLength([] as unknown as string)).toBe(0);
    });
  });

  describe('edge cases', () => {
    it('should handle zero duration', () => {
      expect(parsePlayLength('0:00')).toBe(0);
      expect(parsePlayLength('0:0:0')).toBe(0);
    });

    it('should trim whitespace', () => {
      expect(parsePlayLength('  3:45  ')).toBe(225000);
      expect(parsePlayLength(' 1:02:30 ')).toBe(3750000);
    });

    it('should handle large values', () => {
      expect(parsePlayLength('999:59')).toBe(59999000); // 999 minutes 59 seconds
      expect(parsePlayLength('10:30:45')).toBe(37845000); // 10 hours 30 minutes 45 seconds
    });
  });
});
