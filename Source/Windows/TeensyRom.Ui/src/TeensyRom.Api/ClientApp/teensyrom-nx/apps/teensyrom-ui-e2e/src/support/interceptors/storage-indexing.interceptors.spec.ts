/// <reference types="cypress" />
import { describe, it, expect, beforeEach, vi } from 'vitest';
import {
  interceptIndexStorage,
  interceptIndexAllStorage,
  interceptIndexStorageBatch,
  INDEXING_INTERCEPT_ALIASES,
} from './storage-indexing.interceptors';
import { INDEXING_ENDPOINTS } from '../constants';

describe('Storage Indexing Interceptors', () => {
  let mockIntercept: ReturnType<typeof vi.fn>;
  let mockAs: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    mockAs = vi.fn().mockReturnValue(undefined);
    mockIntercept = vi.fn().mockReturnValue({
      as: mockAs,
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (global as any).cy = {
      intercept: mockIntercept,
    };
  });

  describe('interceptIndexStorage', () => {
    it('should register intercept', () => {
      interceptIndexStorage('device-1', 'USB');
      expect(mockIntercept).toHaveBeenCalled();
    });

    it('should use correct alias USB', () => {
      interceptIndexStorage('device-1', 'USB');
      expect(mockAs).toHaveBeenCalledWith('indexStorage_device-1_USB');
    });

    it('should use correct alias SD', () => {
      interceptIndexStorage('device-2', 'SD');
      expect(mockAs).toHaveBeenCalledWith('indexStorage_device-2_SD');
    });

    it('should support all options', () => {
      interceptIndexStorage('d1', 'USB', { delay: 500, statusCode: 201, errorMode: true });
      expect(mockIntercept).toHaveBeenCalled();
    });
  });

  describe('interceptIndexAllStorage', () => {
    it('should register with correct alias', () => {
      interceptIndexAllStorage();
      expect(mockAs).toHaveBeenCalledWith('indexAllStorage');
    });

    it('should support all options', () => {
      interceptIndexAllStorage({ delay: 2000, statusCode: 500, errorMode: true });
      expect(mockIntercept).toHaveBeenCalled();
    });
  });

  describe('interceptIndexStorageBatch', () => {
    it('should setup combos', () => {
      const combos = [{ deviceId: 'd1', storageType: 'USB' as const }];
      interceptIndexStorageBatch(combos);
      expect(mockIntercept).toHaveBeenCalled();
    });

    it('should handle empty batch', () => {
      interceptIndexStorageBatch([]);
      expect(mockIntercept).not.toHaveBeenCalled();
    });
  });

  describe('Constants validation', () => {
    it('should have correct aliases', () => {
      expect(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE).toBe('indexAllStorage');
      expect(INDEXING_ENDPOINTS.INDEX_STORAGE.method).toBe('POST');
      expect(INDEXING_ENDPOINTS.INDEX_ALL_STORAGE.method).toBe('POST');
    });
  });
});
