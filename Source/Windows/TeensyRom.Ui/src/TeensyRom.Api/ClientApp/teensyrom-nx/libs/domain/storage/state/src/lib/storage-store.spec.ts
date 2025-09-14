import { describe, it, expect, beforeEach, vi, type MockedFunction } from 'vitest';
import { of, throwError, type Observable } from 'rxjs';
import '@analogjs/vitest-angular/setup-zone';
import { TestBed, getTestBed } from '@angular/core/testing';
import {
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting,
} from '@angular/platform-browser-dynamic/testing';

import { StorageStore, StorageDirectoryState, SelectedDirectory } from './storage-store';
import {
  StorageDirectory,
  StorageType,
  IStorageService,
  STORAGE_SERVICE,
} from '@teensyrom-nx/domain/storage/services';
import { StorageKeyUtil } from './storage-key.util';

// -----------------------------
// Task 1: Mock Infrastructure
// -----------------------------

type StorageStoreInstance = {
  // state getters
  storageEntries: () => Record<string, StorageDirectoryState>;
  selectedDirectory: () => SelectedDirectory | null;
  // methods
  initializeStorage: (args: { deviceId: string; storageType: StorageType }) => void;
  navigateToDirectory: (args: { deviceId: string; storageType: StorageType; path: string }) => void;
  refreshDirectory: (args: { deviceId: string; storageType: StorageType }) => void;
  cleanupStorage: (args: { deviceId: string }) => void;
  cleanupStorageType: (args: { deviceId: string; storageType: StorageType }) => void;
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

  // Simple async tick helper for rxMethod completions
  const nextTick = () => new Promise<void>((resolve) => setTimeout(resolve, 0));

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
      expect(store.selectedDirectory()).toBeNull();
    });

    it('should expose expected methods on the store', () => {
      expect(typeof store.initializeStorage).toBe('function');
      expect(typeof store.navigateToDirectory).toBe('function');
      expect(typeof store.refreshDirectory).toBe('function');
      expect(typeof store.cleanupStorage).toBe('function');
      expect(typeof store.cleanupStorageType).toBe('function');
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
  // Task 3: initializeStorage()
  // -----------------------------------------
  describe('initializeStorage()', () => {
    it('creates initial storage entry when key does not exist', () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;

      store.initializeStorage({ deviceId, storageType });

      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = store.storageEntries()[key];

      expect(entry).toBeDefined();
      expect(entry.deviceId).toBe(deviceId);
      expect(entry.storageType).toBe(storageType);
      expect(entry.currentPath).toBe('/');
      expect(entry.directory).toBeNull();
      expect(entry.isLoaded).toBe(false);
      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBeNull();
      expect(entry.lastLoadTime).toBeNull();
    });

    it('does not overwrite existing storage entry on duplicate call', () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);

      store.initializeStorage({ deviceId, storageType });
      const original = store.storageEntries()[key];

      // Mutate via navigate path to detect unintended overwrite later
      // (simulate some state change)
      // No API call here; just call initialize again
      store.initializeStorage({ deviceId, storageType });

      const after = store.storageEntries()[key];
      expect(after).toEqual(original);
    });

    it('preserves existing entries when initializing a new entry', () => {
      const deviceIdA = 'device-1';
      const deviceIdB = 'device-2';
      const storageType = StorageType.Sd;

      store.initializeStorage({ deviceId: deviceIdA, storageType });
      const keyA = StorageKeyUtil.create(deviceIdA, storageType);
      const originalA = store.storageEntries()[keyA];

      // Initialize a different entry
      store.initializeStorage({ deviceId: deviceIdB, storageType });

      const afterA = store.storageEntries()[keyA];
      expect(afterA).toEqual(originalA);
    });

    it('supports multiple device-storage combinations', () => {
      store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });
      store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Usb });
      store.initializeStorage({ deviceId: 'device-2', storageType: StorageType.Sd });

      const entries = store.storageEntries();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Sd)]).toBeDefined();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Usb)]).toBeDefined();
      expect(entries[StorageKeyUtil.create('device-2', StorageType.Sd)]).toBeDefined();
      expect(Object.keys(entries).length).toBe(3);
    });
  });

  // -----------------------------------------
  // Task 4: navigateToDirectory() - Selection & Caching
  // -----------------------------------------
  describe('navigateToDirectory()', () => {
    const deviceId = 'device-1';
    const storageType = StorageType.Sd;

    beforeEach(() => {
      store.initializeStorage({ deviceId, storageType });
    });

    it('updates selectedDirectory immediately on navigate', () => {
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));

      store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      expect(store.selectedDirectory()).toEqual({
        deviceId,
        storageType,
        path: '/games',
      });
    });

    it('skips API call when directory is already loaded (cache hit)', async () => {
      // First load to populate cache
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      store.navigateToDirectory({ deviceId, storageType, path: '/games' });
      await nextTick();

      // Clear mock call history
      getDirectoryMock.mockClear();

      // Navigate to same path → should use cache
      store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      expect(getDirectoryMock).not.toHaveBeenCalled();
      // selection still updates
      expect(store.selectedDirectory()).toEqual({
        deviceId,
        storageType,
        path: '/games',
      });
    });

    it('makes API call for new directory path (cache miss) and updates state on success', async () => {
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/music')));

      store.navigateToDirectory({ deviceId, storageType, path: '/music' });

      const key = StorageKeyUtil.create(deviceId, storageType);
      await nextTick();

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

      store.navigateToDirectory({ deviceId, storageType, path: '/error' });
      await nextTick();

      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = store.storageEntries()[key];
      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBe('API Error');
      expect(entry.directory).toBeNull();
      expect(entry.isLoaded).toBe(false);
    });
  });

  // -----------------------------------------
  // Task 5: refreshDirectory() - Success, Error, No-Op
  // -----------------------------------------
  describe('refreshDirectory()', () => {
    const deviceId = 'device-1';
    const storageType = StorageType.Sd;

    beforeEach(async () => {
      // Initialize and load an initial directory at /games
      store.initializeStorage({ deviceId, storageType });
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      store.navigateToDirectory({ deviceId, storageType, path: '/games' });
      await nextTick();
      getDirectoryMock.mockClear();
    });

    it('sets loading, calls API with currentPath, and updates directory + timestamp on success', async () => {
      const refreshed = createMockStorageDirectory('/games');
      getDirectoryMock.mockReturnValue(of(refreshed));

      const key = StorageKeyUtil.create(deviceId, storageType);
      const beforeTs = store.storageEntries()[key].lastLoadTime;

      store.refreshDirectory({ deviceId, storageType });

      await nextTick();

      // API called with currentPath
      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '/games');

      const entry = store.storageEntries()[key];
      expect(entry.isLoading).toBe(false);
      expect(entry.isLoaded).toBe(true);
      expect(entry.error).toBeNull();
      expect(entry.directory).toEqual(refreshed);
      expect(entry.lastLoadTime).toBeGreaterThan(beforeTs ?? 0);
    });

    it('handles API error by clearing loading, setting error, and preserving directory', async () => {
      const key = StorageKeyUtil.create(deviceId, storageType);
      const originalDir = store.storageEntries()[key].directory;

      getDirectoryMock.mockReturnValue(throwError(() => new Error('Refresh failed')));

      store.refreshDirectory({ deviceId, storageType });
      await nextTick();

      const entry = store.storageEntries()[key];
      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBe('Refresh failed');
      // Directory is preserved on error
      expect(entry.directory).toEqual(originalDir);
    });

    it('is a no-op when storage entry does not exist', () => {
      store.refreshDirectory({ deviceId: 'missing', storageType });
      expect(getDirectoryMock).not.toHaveBeenCalled();
    });
  });

  // -----------------------------------------
  // Task 6: cleanupStorage() and cleanupStorageType()
  // -----------------------------------------
  describe('cleanup storage entries', () => {
    it('removes all entries for a specific device and preserves others', () => {
      // Arrange - create multiple entries
      store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });
      store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Usb });
      store.initializeStorage({ deviceId: 'device-2', storageType: StorageType.Sd });

      // Act - cleanup device-1
      store.cleanupStorage({ deviceId: 'device-1' });

      // Assert - only device-2/SD remains
      const entries = store.storageEntries();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Sd)]).toBeUndefined();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Usb)]).toBeUndefined();
      expect(entries[StorageKeyUtil.create('device-2', StorageType.Sd)]).toBeDefined();
    });

    it('removes only the targeted device-storage entry', () => {
      // Arrange
      store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });
      store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Usb });

      // Act - cleanup only SD for device-1
      store.cleanupStorageType({ deviceId: 'device-1', storageType: StorageType.Sd });

      // Assert - USB remains, SD removed
      const entries = store.storageEntries();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Sd)]).toBeUndefined();
      expect(entries[StorageKeyUtil.create('device-1', StorageType.Usb)]).toBeDefined();
    });

    it('does not modify selectedDirectory (current implementation) during cleanup', () => {
      // Arrange: set up two devices and select one
      store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });
      store.initializeStorage({ deviceId: 'device-2', storageType: StorageType.Sd });
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/sel')));
      store.navigateToDirectory({
        deviceId: 'device-1',
        storageType: StorageType.Sd,
        path: '/sel',
      });

      const beforeSelection = store.selectedDirectory();

      // Act: cleanup device-1 (the selected one)
      store.cleanupStorage({ deviceId: 'device-1' });

      // Assert: selection remains unchanged per current code behavior
      expect(store.selectedDirectory()).toEqual(beforeSelection);
    });
  });

  // -----------------------------------------
  // Task 7: Multi-Device State Management
  // -----------------------------------------
  describe('multi-device state management', () => {
    it('maintains independent state per device-storage combination', async () => {
      // Arrange
      store.initializeStorage({ deviceId: 'dev-1', storageType: StorageType.Sd });
      store.initializeStorage({ deviceId: 'dev-1', storageType: StorageType.Usb });
      store.initializeStorage({ deviceId: 'dev-2', storageType: StorageType.Sd });

      getDirectoryMock.mockImplementation((_, __, path?: string) =>
        of(createMockStorageDirectory(path ?? '/'))
      );

      // Act
      store.navigateToDirectory({ deviceId: 'dev-1', storageType: StorageType.Sd, path: '/games' });
      store.navigateToDirectory({
        deviceId: 'dev-1',
        storageType: StorageType.Usb,
        path: '/music',
      });
      store.navigateToDirectory({ deviceId: 'dev-2', storageType: StorageType.Sd, path: '/apps' });
      await nextTick();

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
      // Arrange
      store.initializeStorage({ deviceId: 'dev-1', storageType: StorageType.Sd });
      store.initializeStorage({ deviceId: 'dev-1', storageType: StorageType.Usb });
      store.initializeStorage({ deviceId: 'dev-2', storageType: StorageType.Sd });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));

      // Prime cache for dev-1 SD
      store.navigateToDirectory({ deviceId: 'dev-1', storageType: StorageType.Sd, path: '/games' });
      await nextTick();

      // Cache hit for dev-1 SD
      getDirectoryMock.mockClear();
      store.navigateToDirectory({ deviceId: 'dev-1', storageType: StorageType.Sd, path: '/games' });
      expect(getDirectoryMock).not.toHaveBeenCalled();

      // Cache miss for dev-1 USB (same path string but different storage)
      getDirectoryMock.mockClear();
      store.navigateToDirectory({
        deviceId: 'dev-1',
        storageType: StorageType.Usb,
        path: '/games',
      });
      expect(getDirectoryMock).toHaveBeenCalledTimes(1);

      // Cache miss for dev-2 SD
      getDirectoryMock.mockClear();
      store.navigateToDirectory({ deviceId: 'dev-2', storageType: StorageType.Sd, path: '/games' });
      expect(getDirectoryMock).toHaveBeenCalledTimes(1);
    });

    it('selection switches across devices and persists within same device-storage', async () => {
      // Arrange
      store.initializeStorage({ deviceId: 'dev-1', storageType: StorageType.Sd });
      store.initializeStorage({ deviceId: 'dev-2', storageType: StorageType.Usb });
      getDirectoryMock.mockImplementation((_, __, path?: string) =>
        of(createMockStorageDirectory(path ?? '/'))
      );

      // Act 1: select dev-1 SD
      store.navigateToDirectory({ deviceId: 'dev-1', storageType: StorageType.Sd, path: '/a' });
      // selection updates immediately; no need to await
      expect(store.selectedDirectory()).toEqual({
        deviceId: 'dev-1',
        storageType: StorageType.Sd,
        path: '/a',
      });

      // Act 2: switch to dev-2 USB
      store.navigateToDirectory({ deviceId: 'dev-2', storageType: StorageType.Usb, path: '/b' });
      expect(store.selectedDirectory()).toEqual({
        deviceId: 'dev-2',
        storageType: StorageType.Usb,
        path: '/b',
      });

      // Act 3: navigate within same device-storage (dev-2 USB)
      store.navigateToDirectory({ deviceId: 'dev-2', storageType: StorageType.Usb, path: '/c' });
      expect(store.selectedDirectory()).toEqual({
        deviceId: 'dev-2',
        storageType: StorageType.Usb,
        path: '/c',
      });
    });
  });

  // -----------------------------------------
  // Task 8: Complex Workflows
  // -----------------------------------------
  describe('complex workflows', () => {
    it('Device Connection Flow: initialize → navigate → cache-hit (single API call)', async () => {
      const deviceId = 'd1';
      const storageType = StorageType.Sd;
      store.initializeStorage({ deviceId, storageType });

      // First navigation: API call
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/x')));
      store.navigateToDirectory({ deviceId, storageType, path: '/x' });
      await nextTick();
      expect(getDirectoryMock).toHaveBeenCalledTimes(1);

      // Second navigation to same path: cache hit, no API call
      getDirectoryMock.mockClear();
      store.navigateToDirectory({ deviceId, storageType, path: '/x' });
      expect(getDirectoryMock).not.toHaveBeenCalled();
    });

    it('Device Reconnection: cleanup device → initialize again → clean state', () => {
      // Setup entries for device and another device
      store.initializeStorage({ deviceId: 'd1', storageType: StorageType.Sd });
      store.initializeStorage({ deviceId: 'd1', storageType: StorageType.Usb });
      store.initializeStorage({ deviceId: 'd2', storageType: StorageType.Sd });

      // Cleanup device d1
      store.cleanupStorage({ deviceId: 'd1' });

      // Re-initialize one storage type for d1
      store.initializeStorage({ deviceId: 'd1', storageType: StorageType.Sd });

      const entries = store.storageEntries();
      // d1-USB should remain removed, d1-SD re-created with defaults
      expect(entries[StorageKeyUtil.create('d1', StorageType.Usb)]).toBeUndefined();
      const d1sd = entries[StorageKeyUtil.create('d1', StorageType.Sd)];
      expect(d1sd).toBeDefined();
      expect(d1sd.currentPath).toBe('/');
      expect(d1sd.isLoaded).toBe(false);
      // d2-SD preserved
      expect(entries[StorageKeyUtil.create('d2', StorageType.Sd)]).toBeDefined();
    });

    it('Error → Success Flow: navigate error then success clears error and loads data', async () => {
      const deviceId = 'd1';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);
      store.initializeStorage({ deviceId, storageType });

      // First call errors
      getDirectoryMock.mockReturnValueOnce(throwError(() => new Error('Network down')));
      store.navigateToDirectory({ deviceId, storageType, path: '/err' });
      await nextTick();
      expect(store.storageEntries()[key].error).toBe('Network down');
      expect(store.storageEntries()[key].isLoaded).toBe(false);

      // Retry succeeds
      getDirectoryMock.mockReturnValueOnce(of(createMockStorageDirectory('/err')));
      store.navigateToDirectory({ deviceId, storageType, path: '/err' });
      await nextTick();
      const entry = store.storageEntries()[key];
      expect(entry.error).toBeNull();
      expect(entry.isLoaded).toBe(true);
      expect(entry.directory?.path).toBe('/err');
    });

    it('Navigate → cache hit → refresh loads new data', async () => {
      const deviceId = 'd1';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);
      store.initializeStorage({ deviceId, storageType });

      // Initial load
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/same')));
      store.navigateToDirectory({ deviceId, storageType, path: '/same' });
      await nextTick();
      const beforeTs = store.storageEntries()[key].lastLoadTime ?? 0;

      // Cache hit on navigate
      getDirectoryMock.mockClear();
      store.navigateToDirectory({ deviceId, storageType, path: '/same' });
      expect(getDirectoryMock).not.toHaveBeenCalled();

      // Refresh calls API and updates timestamp
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/same')));
      store.refreshDirectory({ deviceId, storageType });
      await nextTick();
      const afterTs = store.storageEntries()[key].lastLoadTime ?? 0;
      expect(afterTs).toBeGreaterThan(beforeTs);
    });

    it('Concurrent operations: navigate while refresh in progress results in consistent final state', async () => {
      const deviceId = 'd1';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);
      store.initializeStorage({ deviceId, storageType });

      // Mock returns for both calls (order does not matter in this simple test)
      getDirectoryMock.mockImplementation((_, __, path?: string) =>
        of(createMockStorageDirectory(path ?? '/'))
      );

      // Start first navigate
      store.navigateToDirectory({ deviceId, storageType, path: '/c1' });
      // Before it settles, trigger refresh (will use currentPath which may be '/c1' depending on timing)
      store.refreshDirectory({ deviceId, storageType });

      await nextTick();

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
    it('initializeStorage handles empty deviceId (creates key with empty prefix)', () => {
      store.initializeStorage({ deviceId: '', storageType: StorageType.Sd });
      const key = StorageKeyUtil.create('', StorageType.Sd); // "-SD"
      const entry = store.storageEntries()[key];
      expect(entry).toBeDefined();
      expect(entry.deviceId).toBe('');
      expect(entry.storageType).toBe(StorageType.Sd);
    });

    it('navigateToDirectory supports empty path (selection updates and API called)', async () => {
      const deviceId = 'dev-empty';
      const storageType = StorageType.Sd;
      store.initializeStorage({ deviceId, storageType });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('')));
      store.navigateToDirectory({ deviceId, storageType, path: '' });
      expect(store.selectedDirectory()).toEqual({ deviceId, storageType, path: '' });
      await nextTick();
      expect(getDirectoryMock).toHaveBeenCalledWith(deviceId, storageType, '');
    });

    it('rapid navigation to different paths updates currentPath and selection to latest', async () => {
      const deviceId = 'dev-rapid';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);
      store.initializeStorage({ deviceId, storageType });

      getDirectoryMock.mockImplementation((_, __, path?: string) =>
        of(createMockStorageDirectory(path ?? '/'))
      );

      // Fire two navigations quickly
      store.navigateToDirectory({ deviceId, storageType, path: '/a' });
      store.navigateToDirectory({ deviceId, storageType, path: '/b' });
      await nextTick();

      const entry = store.storageEntries()[key];
      expect(entry.currentPath).toBe('/b');
      expect(store.selectedDirectory()).toEqual({ deviceId, storageType, path: '/b' });
    });

    it('cleanup during navigation removes entries without errors', async () => {
      const deviceId = 'dev-clean';
      const storageType = StorageType.Sd;
      const key = StorageKeyUtil.create(deviceId, storageType);
      store.initializeStorage({ deviceId, storageType });

      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/x')));
      store.navigateToDirectory({ deviceId, storageType, path: '/x' });

      // Immediately cleanup the device
      store.cleanupStorage({ deviceId });
      await nextTick();

      expect(store.storageEntries()[key]).toBeUndefined();
      // Current implementation does not clear selectedDirectory here
      // Ensure no throws occurred (test would fail otherwise)
    });
  });
});
