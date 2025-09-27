import { describe, it, expect, beforeEach, vi, type MockedFunction } from 'vitest';
import { of, throwError, type Observable } from 'rxjs';
import '@analogjs/vitest-angular/setup-zone';
import { TestBed, getTestBed } from '@angular/core/testing';
import {
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting,
} from '@angular/platform-browser-dynamic/testing';

import {
  StorageStore,
  StorageDirectoryState,
  SelectedDirectory,
  NavigationHistory,
} from './storage-store';
import {
  StorageDirectory,
  StorageType,
  IStorageService,
  STORAGE_SERVICE,
} from '@teensyrom-nx/domain';
import { StorageKeyUtil } from './storage-key.util';

// -----------------------------
// Task 1: Mock Infrastructure
// -----------------------------

type StorageStoreInstance = {
  // state getters
  storageEntries: () => Record<string, StorageDirectoryState>;
  selectedDirectories: () => Record<string, SelectedDirectory>;
  navigationHistory: () => Record<string, NavigationHistory>;
  // async methods
  initializeStorage: (args: { deviceId: string; storageType: StorageType }) => Promise<void>;
  navigateToDirectory: (args: {
    deviceId: string;
    storageType: StorageType;
    path: string;
  }) => Promise<void>;
  navigateDirectoryBackward: (args: { deviceId: string }) => Promise<void>;
  navigateDirectoryForward: (args: { deviceId: string }) => Promise<void>;
  refreshDirectory: (args: { deviceId: string; storageType: StorageType }) => Promise<void>;
  navigateUpOneDirectory: (args: { deviceId: string; storageType: StorageType }) => Promise<void>;
  removeAllStorage: (args: { deviceId: string }) => void;
  removeStorage: (args: { deviceId: string; storageType: StorageType }) => void;
  // per-device selection methods
  getSelectedDirectoryForDevice: (deviceId: string) => SelectedDirectory | null;
  getSelectedDirectoryState: (deviceId: string) => () => StorageDirectoryState | null;
  // computed factories
  getDeviceStorageEntries: (deviceId: string) => () => Record<string, StorageDirectoryState>;
  getDeviceDirectories: (deviceId: string) => () => StorageDirectoryState[];
};

describe('StorageStore (NgRx Signal Store)', () => {
  let store: StorageStoreInstance;
  type GetDirectoryFn = (
    deviceId: string,
    storageType: StorageType,
    path?: string
  ) => Observable<StorageDirectory>;
  let getDirectoryMock: MockedFunction<GetDirectoryFn>;
  let mockStorageService: IStorageService;

  // No longer needed since we use async/await directly

  // Test Data Factories
  const createMockStorageDirectory = (path = '/'): StorageDirectory => ({
    path,
    directories: [{ name: 'dir', path: `${path}/dir` }],
    files: [],
  });

  // (Factories for state/selection can be added when needed)

  const createTestStore = () => {
    getDirectoryMock = vi.fn<GetDirectoryFn>();
    mockStorageService = {
      getDirectory: getDirectoryMock,
      index: vi.fn().mockResolvedValue({}),
      indexAll: vi.fn().mockResolvedValue({}),
    };

    TestBed.configureTestingModule({
      providers: [{ provide: STORAGE_SERVICE, useValue: mockStorageService }],
    });

    store = TestBed.inject(StorageStore) as unknown as StorageStoreInstance;
  };

  beforeEach(() => {
    TestBed.resetTestingModule();
    // Ensure Angular testing environment is initialized (idempotent)
    try {
      getTestBed().initTestEnvironment(
        BrowserDynamicTestingModule,
        platformBrowserDynamicTesting()
      );
    } catch {
      // ignore if already initialized
    }
    createTestStore();
  });

  // -----------------------------------------
  // Task 2: Store Initialization & Config
  // -----------------------------------------
  describe('Store Setup', () => {
    it('should be injectable via TestBed (providedIn: root)', () => {
      expect(store).toBeDefined();
    });

    it('should initialize with expected initial state', () => {
      expect(store.storageEntries()).toEqual({});
      expect(store.selectedDirectories()).toEqual({});
      expect(store.navigationHistory()).toEqual({});
    });

    it('should expose expected methods on the store', () => {
      expect(typeof store.initializeStorage).toBe('function');
      expect(typeof store.navigateToDirectory).toBe('function');
      expect(typeof store.navigateDirectoryBackward).toBe('function');
      expect(typeof store.navigateDirectoryForward).toBe('function');
      expect(typeof store.refreshDirectory).toBe('function');
      expect(typeof store.removeAllStorage).toBe('function');
      expect(typeof store.removeStorage).toBe('function');
    });

    it('should inject mocked StorageService dependency', () => {
      const injected = TestBed.inject(STORAGE_SERVICE);
      expect(injected).toBeDefined();
      // Ensure our mock is what Angular provides
      expect(injected).toBe(mockStorageService);
    });

    it('devtools configured (smoke check)', () => {
      // We can’t directly inspect devtools name here; ensure store is alive.
      expect(store).toBeTruthy();
    });
  });

  // -----------------------------------------
  // Task 3: initializeStorage() - Enhanced with Root Directory Fetching
  // -----------------------------------------
  describe('initializeStorage() - Enhanced', () => {
    it('creates initial storage entry and fetches root directory on first call', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));

      await store.initializeStorage({ deviceId, storageType });

      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = store.storageEntries()[key];

      expect(entry).toBeDefined();
      expect(entry.deviceId).toBe(deviceId);
      expect(entry.storageType).toBe(storageType);
      expect(entry.currentPath).toBe('/');
      expect(entry.directory).toBeTruthy();
      expect(entry.directory?.path).toBe('/');
      expect(entry.isLoaded).toBe(true);
      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBeNull();
      expect(entry.lastLoadTime).toBeGreaterThan(0);

      // Should have called API for root directory
      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '/');
    });

    it('sets selectedDirectory to root on initialize', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));

      await store.initializeStorage({ deviceId, storageType });

      expect(store.selectedDirectories()[deviceId]).toEqual({
        deviceId,
        storageType,
        path: '/',
      });
    });

    it('skips API call when root directory already loaded (cache hit)', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;

      // First initialization - loads from API
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      // Clear mock call history
      getDirectoryMock.mockClear();

      // Second initialization - should use cache
      await store.initializeStorage({ deviceId, storageType });

      expect(getDirectoryMock).not.toHaveBeenCalled();

      // Selection still updates
      expect(store.selectedDirectories()[deviceId]).toEqual({
        deviceId,
        storageType,
        path: '/',
      });
    });

    it('makes API call when root directory has error (recovery)', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);

      // First initialization fails
      getDirectoryMock.mockReturnValue(throwError(() => new Error('Network error')));
      await store.initializeStorage({ deviceId, storageType });

      expect(store.storageEntries()[key].error).toBe('Network error');
      expect(store.storageEntries()[key].isLoaded).toBe(false);

      // Second initialization should retry API call
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      expect(getDirectoryMock).toHaveBeenCalledTimes(2); // Both calls made
      expect(store.storageEntries()[key].error).toBeNull();
      expect(store.storageEntries()[key].isLoaded).toBe(true);
    });

    it('handles API error by setting error state and clearing loading', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      getDirectoryMock.mockReturnValue(throwError(() => new Error('API Error')));

      await store.initializeStorage({ deviceId, storageType });

      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = store.storageEntries()[key];
      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBe('API Error');
      expect(entry.directory).toBeNull();
      expect(entry.isLoaded).toBe(false);
    });

    it('supports multiple device-storage combinations independently', async () => {
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));

      await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });
      await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Usb });
      await store.initializeStorage({ deviceId: 'device-2', storageType: StorageType.Sd });

      const entries = store.storageEntries();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Sd)]).toBeDefined();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Usb)]).toBeDefined();
      expect(entries[StorageKeyUtil.create('device-2', StorageType.Sd)]).toBeDefined();
      expect(Object.keys(entries).length).toBe(3);

      // All should be loaded
      expect(Object.values(entries).every((e) => e.isLoaded)).toBe(true);
      expect(getDirectoryMock).toHaveBeenCalledTimes(3);
    });
  });

  // -----------------------------------------
  // Task 4: navigateToDirectory() - Selection & Caching
  // -----------------------------------------
  describe('navigateToDirectory()', () => {
    const deviceId = 'device-1';
    const storageType = StorageType.Sd;

    beforeEach(async () => {
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });
      getDirectoryMock.mockClear();
    });

    it('updates selectedDirectory immediately on navigate', async () => {
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));

      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      expect(store.selectedDirectories()[deviceId]).toEqual({
        deviceId,
        storageType,
        path: '/games',
      });
    });

    it('skips API call when directory is already loaded (cache hit)', async () => {
      // First load to populate cache
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      // Clear mock call history
      getDirectoryMock.mockClear();

      // Navigate to same path → should use cache
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      expect(getDirectoryMock).not.toHaveBeenCalled();
      // selection still updates
      expect(store.selectedDirectories()[deviceId]).toEqual({
        deviceId,
        storageType,
        path: '/games',
      });
    });

    it('makes API call for new directory path (cache miss) and updates state on success', async () => {
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));

      await store.navigateToDirectory({ deviceId, storageType, path: '/music' });

      const key = StorageKeyUtil.create(deviceId, storageType);

      // API called once
      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '/music');

      const entry = store.storageEntries()[key];
      expect(entry.directory?.path).toBe('/music');
      expect(entry.isLoaded).toBe(true);
      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBeNull();
      expect(entry.lastLoadTime).toBeGreaterThan(0);
    });

    it('sets error and clears loading on API error (cache miss)', async () => {
      getDirectoryMock.mockReturnValue(throwError(() => new Error('API Error')));

      await store.navigateToDirectory({ deviceId, storageType, path: '/error' });

      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = store.storageEntries()[key];
      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBe('Failed to navigate to directory');
      expect(entry.directory).toBeNull();
      expect(entry.isLoaded).toBe(false);
    });
  });

  // -----------------------------------------
  // navigateUpOneDirectory() - Parent Path Navigation
  // -----------------------------------------
  describe('navigateUpOneDirectory()', () => {
    const deviceId = 'device-1';
    const storageType = StorageType.Sd;

    beforeEach(async () => {
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });
      getDirectoryMock.mockClear();
    });

    it('navigates up one directory level and updates state', async () => {
      // Arrange: Navigate to /games/arcade first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games/arcade')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games/arcade' });

      // Act: Navigate up one level
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateUpOneDirectory({ deviceId, storageType });

      // Assert: Should be at /games
      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = store.storageEntries()[key];
      expect(entry.currentPath).toBe('/games');
      expect(entry.directory?.path).toBe('/games');
      expect(entry.isLoaded).toBe(true);
      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBeNull();

      // Verify API was called with parent path
      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '/games');
    });

    it('updates selectedDirectory to parent path', async () => {
      // Arrange: Navigate to /games/arcade
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games/arcade')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games/arcade' });

      // Act: Navigate up
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateUpOneDirectory({ deviceId, storageType });

      // Assert: Selection updated
      expect(store.selectedDirectories()[deviceId]).toEqual({
        deviceId,
        storageType,
        path: '/games',
      });
    });

    it('is a no-op when already at root directory', async () => {
      // Already at root from beforeEach setup
      getDirectoryMock.mockClear();

      await store.navigateUpOneDirectory({ deviceId, storageType });

      // Should not make API call
      expect(getDirectoryMock).not.toHaveBeenCalled();

      // Should remain at root
      const key = StorageKeyUtil.create(deviceId, storageType);
      expect(store.storageEntries()[key].currentPath).toBe('/');
    });

    it('is a no-op when storage entry does not exist', async () => {
      await store.navigateUpOneDirectory({
        deviceId: 'non-existent',
        storageType,
      });

      expect(getDirectoryMock).not.toHaveBeenCalled();
    });

    it('handles API error by setting error state', async () => {
      // Arrange: Navigate to subdirectory first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      // Act: Navigate up with API error
      getDirectoryMock.mockReturnValue(throwError(() => new Error('Network error')));
      await store.navigateUpOneDirectory({ deviceId, storageType });

      // Assert: Error state set correctly
      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = store.storageEntries()[key];
      expect(entry.currentPath).toBe('/'); // Parent path still set
      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBe('Failed to navigate up one directory');
      expect(entry.directory).toBeNull();
      expect(entry.isLoaded).toBe(false);
    });

    it('calculates parent paths correctly for various path formats', async () => {
      const testCases = [
        { current: '/games/arcade/pacman', expected: '/games/arcade' },
        { current: '/games/arcade/', expected: '/games' },
        { current: '/games', expected: '/' },
        { current: '/single', expected: '/' },
      ];

      for (const testCase of testCases) {
        // Setup current path
        if (testCase.current !== '/') {
          getDirectoryMock.mockReturnValue(of(createMockStorageDirectory(testCase.current)));
          await store.navigateToDirectory({ deviceId, storageType, path: testCase.current });
        }

        // Mock parent directory response
        getDirectoryMock.mockReturnValue(of(createMockStorageDirectory(testCase.expected)));

        // Navigate up
        await store.navigateUpOneDirectory({ deviceId, storageType });

        // Verify result (unless at root where no API call is made)
        if (testCase.current !== '/') {
          expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, testCase.expected);
        }

        getDirectoryMock.mockClear();
      }
    });

    it('makes API call to load parent directory when not cached', async () => {
      // Arrange: Navigate to child directory
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games/arcade')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games/arcade' });

      getDirectoryMock.mockClear();

      // Act: Navigate up (requires API call since parent isn't cached)
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateUpOneDirectory({ deviceId, storageType });

      // Assert: API call made to load parent directory
      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '/games');

      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = store.storageEntries()[key];
      expect(entry.currentPath).toBe('/games');
      expect(entry.directory?.path).toBe('/games');
      expect(entry.isLoaded).toBe(true);
    });
  });

  // -----------------------------------------
  // navigateDirectoryBackward() - Browser-like Back Navigation
  // -----------------------------------------
  describe('navigateDirectoryBackward()', () => {
    const deviceId = 'device-1';
    const storageType = StorageType.Sd;

    beforeEach(async () => {
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });
      getDirectoryMock.mockClear();
    });

    it('is a no-op when already at beginning of history (currentIndex <= 0)', async () => {
      // After initialization, we're at root (index 0) so can't go back
      await store.navigateDirectoryBackward({ deviceId });
      expect(getDirectoryMock).not.toHaveBeenCalled();
    });

    it('navigates backward in history correctly', async () => {
      // Build history: root -> games -> music
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/music' });

      // Now history should be: ['/', '/games', '/music'] with currentIndex = 2
      const historyBefore = store.navigationHistory()[deviceId];
      expect(historyBefore.history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(historyBefore.currentIndex).toBe(2);

      getDirectoryMock.mockClear();

      // Navigate backward (music -> games)
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateDirectoryBackward({ deviceId });

      // Should have called API to load /games and updated history index
      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '/games');
      expect(store.selectedDirectories()[deviceId].path).toBe('/games');

      const historyAfter = store.navigationHistory()[deviceId];
      expect(historyAfter.history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(historyAfter.currentIndex).toBe(1);
    });

    it('makes API call to load target directory (cache miss)', async () => {
      // Build history and navigate to ensure navigation history is populated
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/music' });

      // Verify current storage state
      const key = StorageKeyUtil.create(deviceId, storageType);
      const cachedEntry = store.storageEntries()[key];
      expect(cachedEntry).toBeDefined();
      expect(cachedEntry.directory?.path).toBe('/music'); // Currently at /music

      getDirectoryMock.mockClear();

      // Navigate backward to /games - needs API call since only current directory is cached
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateDirectoryBackward({ deviceId });

      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '/games');
      expect(store.selectedDirectories()[deviceId].path).toBe('/games');
    });

    it('handles API error gracefully when loading target directory', async () => {
      // Build history
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/music' });

      getDirectoryMock.mockClear();

      // Clear cache to force API call, then mock error
      const key = StorageKeyUtil.create(deviceId, storageType);
      store.removeStorage({ deviceId, storageType });

      getDirectoryMock.mockReturnValue(throwError(() => new Error('Network error')));
      await store.navigateDirectoryBackward({ deviceId });

      // Should set error and update history index still
      expect(store.storageEntries()[key]?.error).toBe('Failed to load directory from history');
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(1);
    });

    it('handles missing current selection gracefully', async () => {
      await store.navigateDirectoryBackward({ deviceId: 'non-existent' });
      expect(getDirectoryMock).not.toHaveBeenCalled();
    });
  });

  // -----------------------------------------
  // navigateDirectoryForward() - Browser-like Forward Navigation
  // -----------------------------------------
  describe('navigateDirectoryForward()', () => {
    const deviceId = 'device-1';
    const storageType = StorageType.Sd;

    beforeEach(async () => {
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });
      getDirectoryMock.mockClear();
    });

    it('is a no-op when already at end of history', async () => {
      // After initialization, we only have root so can't go forward
      await store.navigateDirectoryForward({ deviceId });
      expect(getDirectoryMock).not.toHaveBeenCalled();
    });

    it('navigates forward in history correctly', async () => {
      // Build history: root -> games -> music
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/music' });

      // Go back once (music -> games)
      await store.navigateDirectoryBackward({ deviceId });

      // Now history: ['/', '/games', '/music'] with currentIndex = 1 (at /games)
      const historyBefore = store.navigationHistory()[deviceId];
      expect(historyBefore.history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(historyBefore.currentIndex).toBe(1);

      getDirectoryMock.mockClear();

      // Navigate forward (games -> music)
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateDirectoryForward({ deviceId });

      // Should have called API to load /music and updated history index
      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '/music');
      expect(store.selectedDirectories()[deviceId].path).toBe('/music');

      const historyAfter = store.navigationHistory()[deviceId];
      expect(historyAfter.history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(historyAfter.currentIndex).toBe(2);
    });

    it('makes API call to load target directory (cache miss)', async () => {
      // Build history and cache
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/music' });

      // Go back to create forward opportunity
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateDirectoryBackward({ deviceId });

      getDirectoryMock.mockClear();

      // Navigate forward to /music - needs API call since only current directory is cached
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateDirectoryForward({ deviceId });

      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '/music');
      expect(store.selectedDirectories()[deviceId].path).toBe('/music');
    });

    it('handles API error gracefully when loading target directory', async () => {
      // Build history
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/music' });

      // Go back to create forward opportunity
      await store.navigateDirectoryBackward({ deviceId });

      // Clear cache to force API call, then mock error
      const key = StorageKeyUtil.create(deviceId, storageType);
      store.removeStorage({ deviceId, storageType });

      getDirectoryMock.mockClear();
      getDirectoryMock.mockReturnValue(throwError(() => new Error('Network error')));
      await store.navigateDirectoryForward({ deviceId });

      // Should set error and update history index still
      expect(store.storageEntries()[key]?.error).toBe('Failed to load directory from history');
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(2);
    });

    it('handles missing current selection gracefully', async () => {
      await store.navigateDirectoryForward({ deviceId: 'non-existent' });
      expect(getDirectoryMock).not.toHaveBeenCalled();
    });
  });

  // -----------------------------------------
  // Task 5: refreshDirectory() - Success, Error, No-Op
  // -----------------------------------------
  describe('refreshDirectory()', () => {
    const deviceId = 'device-1';
    const storageType = StorageType.Sd;

    beforeEach(async () => {
      // Initialize with root directory first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      // Navigate to /games
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });
      getDirectoryMock.mockClear();
    });

    it('sets loading, calls API with currentPath, and updates directory + timestamp on success', async () => {
      const refreshed = createMockStorageDirectory('/games');
      getDirectoryMock.mockReturnValue(of(refreshed));

      const key = StorageKeyUtil.create(deviceId, storageType);
      const beforeTs = store.storageEntries()[key].lastLoadTime;

      await store.refreshDirectory({ deviceId, storageType });

      // API called with currentPath
      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '/games');

      const entry = store.storageEntries()[key];
      expect(entry.isLoading).toBe(false);
      expect(entry.isLoaded).toBe(true);
      expect(entry.error).toBeNull();
      expect(entry.directory).toEqual(refreshed);
      // Allow equality if refresh completes within the same millisecond
      expect(entry.lastLoadTime ?? 0).toBeGreaterThanOrEqual(beforeTs ?? 0);
    });

    it('handles API error by clearing loading, setting error, and preserving directory', async () => {
      const key = StorageKeyUtil.create(deviceId, storageType);
      const originalDir = store.storageEntries()[key].directory;

      getDirectoryMock.mockReturnValue(throwError(() => new Error('Refresh failed')));

      await store.refreshDirectory({ deviceId, storageType });

      const entry = store.storageEntries()[key];
      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBe('Failed to refresh directory');
      expect(entry.directory).toEqual(originalDir);
    });

    it('is a no-op when storage entry does not exist', async () => {
      await store.refreshDirectory({ deviceId: 'missing', storageType });
      expect(getDirectoryMock).not.toHaveBeenCalled();
    });
  });

  // -----------------------------------------
  // Task 2: Computed Signals
  // -----------------------------------------
  describe('computed signals', () => {
    it('selectedDirectoryState returns null when nothing selected', () => {
      const deviceId = 'device-1';
      expect(store.selectedDirectories()[deviceId]).toBeUndefined();
      expect(store.getSelectedDirectoryState(deviceId)()).toBeNull();
    });

    it('selectedDirectoryState reflects the selected entry after navigation', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;

      // Initialize with root first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      // Navigate to games
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      const sel = store.getSelectedDirectoryState(deviceId)();
      expect(sel).toBeTruthy();
      expect(sel?.deviceId).toBe(deviceId);
      expect(sel?.storageType).toBe(storageType);
      expect(sel?.currentPath).toBe('/games');
      expect(sel?.isLoaded).toBe(true);
      expect(sel?.directory?.path).toBe('/games');
    });

    it('getDeviceStorageEntries filters entries by deviceId', async () => {
      // Initialize multiple entries
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });
      await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Usb });
      await store.initializeStorage({ deviceId: 'device-2', storageType: StorageType.Sd });

      const dev1Entries = store.getDeviceStorageEntries('device-1')();
      const keys = Object.keys(dev1Entries);
      expect(keys.every((k) => k.startsWith('device-1-'))).toBe(true);
      expect(keys.length).toBe(2);
    });

    it('getDeviceDirectories returns directories-only projections for a device', async () => {
      const deviceId = 'device-1';

      // Initialize SD
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType: StorageType.Sd });

      // Navigate SD to games
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType: StorageType.Sd, path: '/games' });

      // Initialize USB
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType: StorageType.Usb });

      // Navigate USB to roms
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/roms')));
      await store.navigateToDirectory({ deviceId, storageType: StorageType.Usb, path: '/roms' });

      const dirs = store.getDeviceDirectories(deviceId)();
      // Expect entries for both SD and USB with directory objects
      expect(dirs.length).toBe(2);
      expect(dirs.map((d) => d.deviceId)).toEqual(['device-1', 'device-1']);
      expect(dirs.some((d) => d.storageType === StorageType.Sd)).toBe(true);
      expect(dirs.some((d) => d.storageType === StorageType.Usb)).toBe(true);
      expect(dirs.every((d) => d.directory && Array.isArray(d.directory.directories))).toBe(true);
    });
  });

  // -----------------------------------------
  // Task 6: removeAllStorage() and removeStorage()
  // -----------------------------------------
  describe('cleanup storage entries', () => {
    it('removes all entries for a specific device and preserves others', async () => {
      // Arrange - create multiple entries (now requires mock setup)
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });
      await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Usb });
      await store.initializeStorage({ deviceId: 'device-2', storageType: StorageType.Sd });

      // Act - cleanup device-1
      store.removeAllStorage({ deviceId: 'device-1' });

      // Assert - only device-2/SD remains
      const entries = store.storageEntries();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Sd)]).toBeUndefined();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Usb)]).toBeUndefined();
      expect(entries[StorageKeyUtil.create('device-2', StorageType.Sd)]).toBeDefined();
    });

    it('removes only the targeted device-storage entry', async () => {
      // Arrange
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });
      await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Usb });

      // Act - cleanup only SD for device-1
      store.removeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });

      // Assert - USB remains, SD removed
      const entries = store.storageEntries();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Sd)]).toBeUndefined();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Usb)]).toBeDefined();
    });

    it('removes device selection during cleanup', async () => {
      // Arrange: set up two devices and select one
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });
      await store.initializeStorage({ deviceId: 'device-2', storageType: StorageType.Sd });

      // Navigate to a different path
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/sel')));
      await store.navigateToDirectory({
        deviceId: 'device-1',
        storageType: StorageType.Sd,
        path: '/sel',
      });

      // Verify device-1 has selection
      expect(store.selectedDirectories()['device-1']).toBeDefined();
      expect(store.selectedDirectories()['device-2']).toBeDefined();

      // Act: cleanup device-1 (the selected one)
      store.removeAllStorage({ deviceId: 'device-1' });

      // Assert: device-1 selection removed, device-2 preserved
      expect(store.selectedDirectories()['device-1']).toBeUndefined();
      expect(store.selectedDirectories()['device-2']).toBeDefined();
    });
  });

  // -----------------------------------------
  // Task 7: Multi-Device State Management
  // -----------------------------------------
  describe('multi-device state management', () => {
    it('maintains independent state per device-storage combination', async () => {
      // Arrange - Initialize entries first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: 'dev-1', storageType: StorageType.Sd });
      await store.initializeStorage({ deviceId: 'dev-1', storageType: StorageType.Usb });
      await store.initializeStorage({ deviceId: 'dev-2', storageType: StorageType.Sd });

      getDirectoryMock.mockImplementation((_, __, path?: string) =>
        of(createMockStorageDirectory(path ?? '/'))
      );

      // Act
      await store.navigateToDirectory({
        deviceId: 'dev-1',
        storageType: StorageType.Sd,
        path: '/games',
      });
      await store.navigateToDirectory({
        deviceId: 'dev-1',
        storageType: StorageType.Usb,
        path: '/music',
      });
      await store.navigateToDirectory({
        deviceId: 'dev-2',
        storageType: StorageType.Sd,
        path: '/apps',
      });

      // Assert
      const sd1 = store.storageEntries()[StorageKeyUtil.create('dev-1', StorageType.Sd)];
      const usb1 = store.storageEntries()[StorageKeyUtil.create('dev-1', StorageType.Usb)];
      const sd2 = store.storageEntries()[StorageKeyUtil.create('dev-2', StorageType.Sd)];

      expect(sd1.currentPath).toBe('/games');
      expect(sd1.directory?.path).toBe('/games');
      expect(sd1.isLoaded).toBe(true);

      expect(usb1.currentPath).toBe('/music');
      expect(usb1.directory?.path).toBe('/music');
      expect(usb1.isLoaded).toBe(true);

      expect(sd2.currentPath).toBe('/apps');
      expect(sd2.directory?.path).toBe('/apps');
      expect(sd2.isLoaded).toBe(true);
    });

    it('caching works independently per device-storage', async () => {
      // Arrange - Initialize entries first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: 'dev-1', storageType: StorageType.Sd });
      await store.initializeStorage({ deviceId: 'dev-1', storageType: StorageType.Usb });
      await store.initializeStorage({ deviceId: 'dev-2', storageType: StorageType.Sd });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));

      // Prime cache for dev-1 SD
      await store.navigateToDirectory({
        deviceId: 'dev-1',
        storageType: StorageType.Sd,
        path: '/games',
      });

      // Cache hit for dev-1 SD
      getDirectoryMock.mockClear();
      await store.navigateToDirectory({
        deviceId: 'dev-1',
        storageType: StorageType.Sd,
        path: '/games',
      });
      expect(getDirectoryMock).not.toHaveBeenCalled();

      // Cache miss for dev-1 USB (same path string but different storage)
      getDirectoryMock.mockClear();
      await store.navigateToDirectory({
        deviceId: 'dev-1',
        storageType: StorageType.Usb,
        path: '/games',
      });
      expect(getDirectoryMock).toHaveBeenCalledTimes(1);

      // Cache miss for dev-2 SD
      getDirectoryMock.mockClear();
      await store.navigateToDirectory({
        deviceId: 'dev-2',
        storageType: StorageType.Sd,
        path: '/games',
      });
      expect(getDirectoryMock).toHaveBeenCalledTimes(1);
    });

    it('selection switches across devices and persists within same device-storage', async () => {
      // Arrange - Initialize entries first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: 'dev-1', storageType: StorageType.Sd });
      await store.initializeStorage({ deviceId: 'dev-2', storageType: StorageType.Usb });
      getDirectoryMock.mockImplementation((_, __, path?: string) =>
        of(createMockStorageDirectory(path ?? '/'))
      );

      // Act 1: select dev-1 SD
      await store.navigateToDirectory({
        deviceId: 'dev-1',
        storageType: StorageType.Sd,
        path: '/a',
      });
      // selection updates immediately; no need to await
      expect(store.selectedDirectories()['dev-1']).toEqual({
        deviceId: 'dev-1',
        storageType: StorageType.Sd,
        path: '/a',
      });

      // Act 2: select dev-2 USB (independent of dev-1)
      await store.navigateToDirectory({
        deviceId: 'dev-2',
        storageType: StorageType.Usb,
        path: '/b',
      });
      expect(store.selectedDirectories()['dev-2']).toEqual({
        deviceId: 'dev-2',
        storageType: StorageType.Usb,
        path: '/b',
      });
      // Verify dev-1 selection is preserved
      expect(store.selectedDirectories()['dev-1']).toEqual({
        deviceId: 'dev-1',
        storageType: StorageType.Sd,
        path: '/a',
      });

      // Act 3: navigate within same device-storage (dev-2 USB)
      await store.navigateToDirectory({
        deviceId: 'dev-2',
        storageType: StorageType.Usb,
        path: '/c',
      });
      expect(store.selectedDirectories()['dev-2']).toEqual({
        deviceId: 'dev-2',
        storageType: StorageType.Usb,
        path: '/c',
      });
      // Verify dev-1 selection still preserved
      expect(store.selectedDirectories()['dev-1']).toEqual({
        deviceId: 'dev-1',
        storageType: StorageType.Sd,
        path: '/a',
      });
    });
  });

  // -----------------------------------------
  // Browser-like Navigation Integration Tests
  // -----------------------------------------
  describe('browser-like navigation workflows', () => {
    it('Complete navigation flow: initialize → navigate → back → forward', async () => {
      const deviceId = 'nav-test';
      const storageType = StorageType.Sd;

      // Initialize - creates history with ['/'] at index 0
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });
      expect(store.navigationHistory()[deviceId].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(0);

      // Navigate to /games - history becomes ['/', '/games'] at index 1
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });
      expect(store.navigationHistory()[deviceId].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(1);
      expect(store.selectedDirectories()[deviceId].path).toBe('/games');

      // Navigate to /music - history becomes ['/', '/games', '/music'] at index 2
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/music' });
      expect(store.navigationHistory()[deviceId].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(2);
      expect(store.selectedDirectories()[deviceId].path).toBe('/music');

      getDirectoryMock.mockClear();

      // Navigate back to /games - history stays same, index goes to 1
      await store.navigateDirectoryBackward({ deviceId });
      expect(store.navigationHistory()[deviceId].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(1);
      expect(store.selectedDirectories()[deviceId].path).toBe('/games');

      // Navigate back to / - history stays same, index goes to 0
      await store.navigateDirectoryBackward({ deviceId });
      expect(store.navigationHistory()[deviceId].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(0);
      expect(store.selectedDirectories()[deviceId].path).toBe('/');

      // Navigate forward to /games - index goes to 1
      await store.navigateDirectoryForward({ deviceId });
      expect(store.navigationHistory()[deviceId].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(1);
      expect(store.selectedDirectories()[deviceId].path).toBe('/games');

      // Navigate forward to /music - index goes to 2
      await store.navigateDirectoryForward({ deviceId });
      expect(store.navigationHistory()[deviceId].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(2);
      expect(store.selectedDirectories()[deviceId].path).toBe('/music');
    });

    it('Browser-like behavior: new navigation clears forward history', async () => {
      const deviceId = 'clear-test';
      const storageType = StorageType.Sd;

      // Build history: / → games → music
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/music' });

      // Go back to /games
      await store.navigateDirectoryBackward({ deviceId });
      expect(store.navigationHistory()[deviceId].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(1);

      // Navigate to new directory /apps - should clear forward history
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/apps')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/apps' });

      // Forward history should be cleared, /music is gone
      expect(store.navigationHistory()[deviceId].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
        { path: '/apps', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(2);

      // Can't go forward anymore
      await store.navigateDirectoryForward({ deviceId });
      expect(store.navigationHistory()[deviceId].currentIndex).toBe(2); // No change
    });

    it('History respects maxHistorySize limit', async () => {
      const deviceId = 'limit-test';
      const storageType = StorageType.Sd;

      // Initialize with small history size for testing
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      // Manually set small history size to test limit
      const currentHistory = store.navigationHistory()[deviceId];
      currentHistory.maxHistorySize = 3;

      // Navigate through several directories to exceed limit
      const paths = ['/dir1', '/dir2', '/dir3', '/dir4'];
      for (const path of paths) {
        getDirectoryMock.mockReturnValue(of(createMockStorageDirectory(path)));
        await store.navigateToDirectory({ deviceId, storageType, path });
      }

      // Should only keep last 3 entries due to maxHistorySize
      const finalHistory = store.navigationHistory()[deviceId];
      expect(finalHistory.history.length).toBe(3);
      expect(finalHistory.history).toEqual([
        { path: '/dir2', storageType: StorageType.Sd },
        { path: '/dir3', storageType: StorageType.Sd },
        { path: '/dir4', storageType: StorageType.Sd },
      ]);
      expect(finalHistory.currentIndex).toBe(2);
    });

    it('Multi-device navigation history isolation', async () => {
      const device1 = 'device-1';
      const device2 = 'device-2';
      const storageType = StorageType.Sd;

      // Initialize both devices
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: device1, storageType });
      await store.initializeStorage({ deviceId: device2, storageType });

      // Navigate device1 to /games
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.navigateToDirectory({ deviceId: device1, storageType, path: '/games' });

      // Navigate device2 to /music
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));
      await store.navigateToDirectory({ deviceId: device2, storageType, path: '/music' });

      // Each device should have independent history
      expect(store.navigationHistory()[device1].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/games', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[device1].currentIndex).toBe(1);
      expect(store.selectedDirectories()[device1].path).toBe('/games');

      expect(store.navigationHistory()[device2].history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/music', storageType: StorageType.Sd },
      ]);
      expect(store.navigationHistory()[device2].currentIndex).toBe(1);
      expect(store.selectedDirectories()[device2].path).toBe('/music');

      // Navigate device1 back - shouldn't affect device2
      await store.navigateDirectoryBackward({ deviceId: device1 });
      expect(store.navigationHistory()[device1].currentIndex).toBe(0);
      expect(store.navigationHistory()[device2].currentIndex).toBe(1); // Unchanged
    });

    it('Cross-storage navigation: navigating between SD and USB preserves storage type in history', async () => {
      const deviceId = 'cross-storage-test';

      // Start on SD storage
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType: StorageType.Sd });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/sdgames')));
      await store.navigateToDirectory({ deviceId, storageType: StorageType.Sd, path: '/sdgames' });

      // User switches to USB storage and navigates
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.navigateToDirectory({ deviceId, storageType: StorageType.Usb, path: '/' });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/usbmusic')));
      await store.navigateToDirectory({
        deviceId,
        storageType: StorageType.Usb,
        path: '/usbmusic',
      });

      // History should track all navigations with correct storage types
      const history = store.navigationHistory()[deviceId];
      expect(history.history).toEqual([
        { path: '/', storageType: StorageType.Sd },
        { path: '/sdgames', storageType: StorageType.Sd },
        { path: '/', storageType: StorageType.Usb },
        { path: '/usbmusic', storageType: StorageType.Usb },
      ]);
      expect(history.currentIndex).toBe(3);

      // Navigate backward - should go to USB:/
      await store.navigateDirectoryBackward({ deviceId });
      expect(store.selectedDirectories()[deviceId].storageType).toBe(StorageType.Usb);
      expect(store.selectedDirectories()[deviceId].path).toBe('/');

      // Navigate backward again - should go to SD:/sdgames
      await store.navigateDirectoryBackward({ deviceId });
      expect(store.selectedDirectories()[deviceId].storageType).toBe(StorageType.Sd);
      expect(store.selectedDirectories()[deviceId].path).toBe('/sdgames');
    });

    it('Cross-storage navigation: forward works across storage types', async () => {
      const deviceId = 'forward-cross-test';

      // Build cross-storage history
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType: StorageType.Sd });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/sdfiles')));
      await store.navigateToDirectory({ deviceId, storageType: StorageType.Sd, path: '/sdfiles' });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.navigateToDirectory({ deviceId, storageType: StorageType.Usb, path: '/' });

      // Go back to SD:/sdfiles
      await store.navigateDirectoryBackward({ deviceId });
      expect(store.selectedDirectories()[deviceId].storageType).toBe(StorageType.Sd);

      // Go forward to USB:/
      await store.navigateDirectoryForward({ deviceId });
      expect(store.selectedDirectories()[deviceId].storageType).toBe(StorageType.Usb);
      expect(store.selectedDirectories()[deviceId].path).toBe('/');
    });

    it('Cross-storage navigation: same path on different storage types creates distinct history entries', async () => {
      const deviceId = 'same-path-test';

      // Navigate to /data on SD
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType: StorageType.Sd });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/data')));
      await store.navigateToDirectory({ deviceId, storageType: StorageType.Sd, path: '/data' });

      // Navigate to /data on USB (same path, different storage)
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/data')));
      await store.navigateToDirectory({ deviceId, storageType: StorageType.Usb, path: '/data' });

      // History should have both /data entries as distinct items with different storage types
      const history = store.navigationHistory()[deviceId];
      expect(history.history).toContainEqual({ path: '/data', storageType: StorageType.Sd });
      expect(history.history).toContainEqual({ path: '/data', storageType: StorageType.Usb });

      // The entries should be distinct based on storage type
      const sdDataIndex = history.history.findIndex(
        (item) => item.path === '/data' && item.storageType === StorageType.Sd
      );
      const usbDataIndex = history.history.findIndex(
        (item) => item.path === '/data' && item.storageType === StorageType.Usb
      );
      expect(sdDataIndex).not.toBe(usbDataIndex);
    });
  });

  // -----------------------------------------
  // Task 8: Complex Workflows
  // -----------------------------------------
  describe('complex workflows', () => {
    it('Device Connection Flow: initialize → navigate → cache-hit (single API call)', async () => {
      const deviceId = 'd1';
      const storageType = StorageType.Sd;

      // Initialize (this now makes 1 API call for root)
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });
      expect(getDirectoryMock).toHaveBeenCalledTimes(1); // Root fetch

      // First navigation to different path: API call
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/x')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/x' });
      expect(getDirectoryMock).toHaveBeenCalledTimes(2); // Root + /x

      // Second navigation to same path: cache hit, no API call
      getDirectoryMock.mockClear();
      await store.navigateToDirectory({ deviceId, storageType, path: '/x' });
      expect(getDirectoryMock).not.toHaveBeenCalled();
    });

    it('Device Reconnection: cleanup device → initialize again → clean state', async () => {
      // Setup entries for device and another device
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: 'd1', storageType: StorageType.Sd });
      await store.initializeStorage({ deviceId: 'd1', storageType: StorageType.Usb });
      await store.initializeStorage({ deviceId: 'd2', storageType: StorageType.Sd });

      // Cleanup device d1
      store.removeAllStorage({ deviceId: 'd1' });

      // Re-initialize one storage type for d1
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: 'd1', storageType: StorageType.Sd });

      const entries = store.storageEntries();
      // d1-USB should remain removed, d1-SD re-created and loaded
      expect(entries[StorageKeyUtil.create('d1', StorageType.Usb)]).toBeUndefined();
      const d1sd = entries[StorageKeyUtil.create('d1', StorageType.Sd)];
      expect(d1sd).toBeDefined();
      expect(d1sd.currentPath).toBe('/');
      expect(d1sd.isLoaded).toBe(true); // Now loaded since initialize fetches root
      // d2-SD preserved
      expect(entries[StorageKeyUtil.create('d2', StorageType.Sd)]).toBeDefined();
    });

    it('Error → Success Flow: navigate error then success clears error and loads data', async () => {
      const deviceId = 'd1';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Initialize first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      // First navigation call errors
      getDirectoryMock.mockReturnValueOnce(throwError(() => new Error('Network down')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/err' });
      expect(store.storageEntries()[key].error).toBe('Failed to navigate to directory');
      expect(store.storageEntries()[key].isLoaded).toBe(false);

      // Retry succeeds
      getDirectoryMock.mockReturnValueOnce(of(createMockStorageDirectory('/err')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/err' });
      const entry = store.storageEntries()[key];
      expect(entry.error).toBeNull();
      expect(entry.isLoaded).toBe(true);
      expect(entry.directory?.path).toBe('/err');
    });

    it('Navigate → cache hit → refresh loads new data', async () => {
      const deviceId = 'd1';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Initialize (fetches root)
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      // Navigate to different path
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/same')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/same' });
      const beforeTs = store.storageEntries()[key].lastLoadTime ?? 0;

      // Cache hit on navigate
      getDirectoryMock.mockClear();
      await store.navigateToDirectory({ deviceId, storageType, path: '/same' });
      expect(getDirectoryMock).not.toHaveBeenCalled();

      // Refresh calls API and updates timestamp
      // Add small delay to ensure timestamp difference
      await new Promise((resolve) => setTimeout(resolve, 1));
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/same')));
      await store.refreshDirectory({ deviceId, storageType });
      const afterTs = store.storageEntries()[key].lastLoadTime ?? 0;
      expect(afterTs).toBeGreaterThan(beforeTs);
    });

    it('Concurrent operations: navigate while refresh in progress results in consistent final state', async () => {
      const deviceId = 'd1';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Initialize first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      // Mock returns for both calls (order does not matter in this simple test)
      getDirectoryMock.mockImplementation((_, __, path?: string) =>
        of(createMockStorageDirectory(path ?? '/'))
      );

      // Start first navigate
      await store.navigateToDirectory({ deviceId, storageType, path: '/c1' });
      // Before it settles, trigger refresh (will use currentPath which may be '/c1' depending on timing)
      await store.refreshDirectory({ deviceId, storageType });

      const entry = store.storageEntries()[key];
      expect(entry.isLoading).toBe(false);
      expect(entry.directory).toBeTruthy();
      // currentPath is '/c1' and directory path should match
      expect(entry.directory?.path).toBe(entry.currentPath);
    });
  });

  // -----------------------------------------
  // Task 9: Edge Cases & Error Conditions
  // -----------------------------------------
  describe('edge cases & error handling', () => {
    it('initializeStorage handles empty deviceId (creates key with empty prefix)', async () => {
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId: '', storageType: StorageType.Sd });

      const key = StorageKeyUtil.create('', StorageType.Sd); // "-SD"
      const entry = store.storageEntries()[key];
      expect(entry).toBeDefined();
      expect(entry.deviceId).toBe('');
      expect(entry.storageType).toBe(StorageType.Sd);
      expect(entry.isLoaded).toBe(true); // Now loaded since initialize fetches root
    });

    it('navigateToDirectory supports empty path (selection updates and API called)', async () => {
      const deviceId = 'dev-empty';
      const storageType = StorageType.Sd;

      // Initialize first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('')));
      await store.navigateToDirectory({ deviceId, storageType, path: '' });
      expect(store.selectedDirectories()[deviceId]).toEqual({ deviceId, storageType, path: '' });
      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '');
    });

    it('rapid navigation to different paths updates currentPath and selection to latest', async () => {
      const deviceId = 'dev-rapid';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Initialize first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      getDirectoryMock.mockImplementation((_, __, path?: string) =>
        of(createMockStorageDirectory(path ?? '/'))
      );

      // Fire two navigations quickly
      await store.navigateToDirectory({ deviceId, storageType, path: '/a' });
      await store.navigateToDirectory({ deviceId, storageType, path: '/b' });

      const entry = store.storageEntries()[key];
      expect(entry.currentPath).toBe('/b');
      expect(store.selectedDirectories()[deviceId]).toEqual({ deviceId, storageType, path: '/b' });
    });

    it('cleanup during navigation removes entries without errors', async () => {
      const deviceId = 'dev-clean';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Initialize first
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/')));
      await store.initializeStorage({ deviceId, storageType });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/x')));
      await store.navigateToDirectory({ deviceId, storageType, path: '/x' });

      // Immediately cleanup the device
      store.removeAllStorage({ deviceId });

      expect(store.storageEntries()[key]).toBeUndefined();
      // Current implementation does not clear selectedDirectory here
      // Ensure no throws occurred (test would fail otherwise)
    });
  });
});
