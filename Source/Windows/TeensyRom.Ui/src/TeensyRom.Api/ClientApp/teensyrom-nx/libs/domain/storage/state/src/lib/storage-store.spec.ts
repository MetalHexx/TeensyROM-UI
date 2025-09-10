vi.mock('@teensyrom-nx/domain/storage/services', () => {
  // Only export the runtime values your tests use
  return {
    StorageType: { Sd: 'SD', Usb: 'USB' },
    FileItemType: { Game: 'Game' },
  };
});

import { describe, it, expect } from 'vitest';
import { StorageState, StorageDirectoryState } from './storage-store';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { FileItemType, StorageDirectory } from '@teensyrom-nx/domain/storage/services';

describe('Storage Store State Interfaces', () => {
  describe('StorageDirectoryState', () => {
    it('should define complete storage directory state structure', () => {
      const state: StorageDirectoryState = {
        deviceId: 'device123',
        storageType: StorageType.Sd,
        currentPath: '/games/arcade',
        directory: null,
        isLoaded: false,
        isLoading: false,
        error: null,
        lastLoadTime: null,
      };

      expect(state.deviceId).toBe('device123');
      expect(state.storageType).toBe(StorageType.Sd);
      expect(state.currentPath).toBe('/games/arcade');
      expect(state.directory).toBeNull();
      expect(state.isLoaded).toBe(false);
      expect(state.isLoading).toBe(false);
      expect(state.error).toBeNull();
      expect(state.lastLoadTime).toBeNull();
    });

    it('should allow loaded state with directory data', () => {
      const mockDirectory: StorageDirectory = {
        path: '/games',
        directories: [{ name: 'arcade', path: '/games/arcade' }],
        files: [
          {
            name: 'game.prg',
            path: '/games/game.prg',
            size: 1024,
            isFavorite: false,
            title: 'Test Game',
            creator: 'Test Creator',
            releaseInfo: '2024',
            description: 'Test Description',
            shareUrl: '',
            metadataSource: '',
            meta1: '',
            meta2: '',
            metadataSourcePath: '',
            parentPath: '/games',
            playLength: '',
            subtuneLengths: [],
            startSubtuneNum: 0,
            images: [],
            type: FileItemType.Game,
          },
        ],
      };

      const state: StorageDirectoryState = {
        deviceId: 'device123',
        storageType: StorageType.Usb,
        currentPath: '/games',
        directory: mockDirectory,
        isLoaded: true,
        isLoading: false,
        error: null,
        lastLoadTime: Date.now(),
      };

      expect(state.directory).toBe(mockDirectory);
      expect(state.isLoaded).toBe(true);
      expect(state.lastLoadTime).toBeTypeOf('number');
    });

    it('should allow error state', () => {
      const state: StorageDirectoryState = {
        deviceId: 'device123',
        storageType: StorageType.Sd,
        currentPath: '/system',
        directory: null,
        isLoaded: false,
        isLoading: false,
        error: 'Storage not available',
        lastLoadTime: null,
      };

      expect(state.error).toBe('Storage not available');
    });

    it('should allow loading state', () => {
      const state: StorageDirectoryState = {
        deviceId: 'device123',
        storageType: StorageType.Sd,
        currentPath: '/',
        directory: null,
        isLoaded: false,
        isLoading: true,
        error: null,
        lastLoadTime: null,
      };

      expect(state.isLoading).toBe(true);
    });
  });

  describe('StorageState', () => {
    it('should define flat storage state structure', () => {
      const state: StorageState = { storageEntries: {} };
      expect(state.storageEntries).toEqual({});
      expect(typeof state.storageEntries).toBe('object');
    });

    it('should allow storage entries with string keys', () => {
      const directoryState: StorageDirectoryState = {
        deviceId: 'device123',
        storageType: StorageType.Sd,
        currentPath: '/',
        directory: null,
        isLoaded: false,
        isLoading: false,
        error: null,
        lastLoadTime: null,
      };

      const state: StorageState = {
        storageEntries: {
          'device123-SD': directoryState,
          'device123-USB': { ...directoryState, storageType: StorageType.Usb },
        },
      };

      expect(Object.keys(state.storageEntries)).toHaveLength(2);
      expect(state.storageEntries['device123-SD']).toBe(directoryState);
      expect(state.storageEntries['device123-USB'].storageType).toBe(StorageType.Usb);
    });

    it('should support multiple devices with multiple storage types', () => {
      const state: StorageState = {
        storageEntries: {
          'device1-SD': {
            deviceId: 'device1',
            storageType: StorageType.Sd,
            currentPath: '/games',
            directory: null,
            isLoaded: true,
            isLoading: false,
            error: null,
            lastLoadTime: Date.now(),
          },
          'device1-USB': {
            deviceId: 'device1',
            storageType: StorageType.Usb,
            currentPath: '/music',
            directory: null,
            isLoaded: false,
            isLoading: true,
            error: null,
            lastLoadTime: null,
          },
          // Add the third entry you assert against:
          'device2-USB': {
            deviceId: 'device2',
            storageType: StorageType.Usb,
            currentPath: '/',
            directory: null,
            isLoaded: false,
            isLoading: false,
            error: 'Access denied',
            lastLoadTime: null,
          },
        },
      };

      expect(Object.keys(state.storageEntries)).toHaveLength(3);
      expect(state.storageEntries['device1-SD'].currentPath).toBe('/games');
      expect(state.storageEntries['device1-USB'].isLoading).toBe(true);
      expect(state.storageEntries['device2-USB'].error).toBe('Access denied');
    });
  });
});
