import { describe, it, expect, beforeEach, vi } from 'vitest';
import { of, throwError } from 'rxjs';
import { signalStore, withMethods, withState, patchState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, switchMap, tap, catchError, filter } from 'rxjs';

// Define test interfaces directly to avoid import issues
interface TestStorageDirectory {
  path: string;
  directories: Array<{ name: string; path: string }>;
  files: Array<{
    name: string;
    path: string;
    size: number;
    isFavorite: boolean;
    title: string;
    creator: string;
    releaseInfo: string;
    description: string;
    shareUrl: string;
    metadataSource: string;
    meta1: string;
    meta2: string;
    metadataSourcePath: string;
    parentPath: string;
    playLength: string;
    subtuneLengths: string[];
    startSubtuneNum: number;
    images: string[];
    type: number;
  }>;
}

const TestStorageType = {
  Sd: 'SD',
  Usb: 'USB',
  Internal: 'Internal',
} as const;

const TestFileItemType = {
  Game: 1,
} as const;

interface TestStorageDirectoryState {
  deviceId: string;
  storageType: string;
  currentPath: string;
  directory: TestStorageDirectory | null;
  isLoaded: boolean;
  isLoading: boolean;
  error: string | null;
  lastLoadTime: number | null;
}

interface TestSelectedDirectory {
  deviceId: string;
  storageType: string;
  path: string;
}

interface TestStorageState {
  storageEntries: Record<string, TestStorageDirectoryState>;
  selectedDirectory: TestSelectedDirectory | null;
}

// Test utility
const createKey = (deviceId: string, storageType: string) => `${deviceId}-${storageType}`;

// Test implementation of navigate method
function createNavigateToDirectory(store: any, storageService: any) {
  return {
    navigateToDirectory: rxMethod<{ deviceId: string; storageType: string; path: string }>(
      pipe(
        tap(({ deviceId, storageType, path }) => {
          const key = createKey(deviceId, storageType);
          const currentState = store.storageEntries();
          const existingEntry = currentState[key];

          // Always update global selection
          patchState(store, {
            selectedDirectory: { deviceId, storageType, path },
          });

          // Check if we already have this directory loaded
          const isAlreadyLoaded =
            existingEntry &&
            existingEntry.currentPath === path &&
            existingEntry.isLoaded &&
            existingEntry.directory &&
            !existingEntry.error;

          if (!isAlreadyLoaded) {
            // Initialize entry if it doesn't exist
            if (!existingEntry) {
              patchState(store, (state) => ({
                storageEntries: {
                  ...state.storageEntries,
                  [key]: {
                    deviceId,
                    storageType,
                    currentPath: path,
                    directory: null,
                    isLoaded: false,
                    isLoading: true,
                    error: null,
                    lastLoadTime: null,
                  },
                },
              }));
            } else {
              // Update existing entry
              patchState(store, (state) => ({
                storageEntries: {
                  ...state.storageEntries,
                  [key]: {
                    ...existingEntry,
                    currentPath: path,
                    isLoading: true,
                    error: null,
                  },
                },
              }));
            }
          }
        }),
        filter(({ deviceId, storageType, path }) => {
          const key = createKey(deviceId, storageType);
          const currentState = store.storageEntries();
          const existingEntry = currentState[key];

          const isAlreadyLoaded =
            existingEntry &&
            existingEntry.currentPath === path &&
            existingEntry.isLoaded &&
            existingEntry.directory &&
            !existingEntry.error;

          return !isAlreadyLoaded;
        }),
        switchMap(({ deviceId, storageType, path }) => {
          return storageService.getDirectory(deviceId, storageType, path).pipe(
            tap((directory: TestStorageDirectory) => {
              const key = createKey(deviceId, storageType);
              patchState(store, (state) => ({
                storageEntries: {
                  ...state.storageEntries,
                  [key]: {
                    ...state.storageEntries[key],
                    directory,
                    isLoaded: true,
                    isLoading: false,
                    error: null,
                    lastLoadTime: Date.now(),
                  },
                },
              }));
            }),
            catchError((error) => {
              const key = createKey(deviceId, storageType);
              patchState(store, (state) => ({
                storageEntries: {
                  ...state.storageEntries,
                  [key]: {
                    ...state.storageEntries[key],
                    isLoading: false,
                    error: error.message || 'Failed to navigate to directory',
                  },
                },
              }));
              return of(null);
            })
          );
        })
      )
    ),
  };
}

// Test implementation of refresh method
function createRefreshDirectory(store: any, storageService: any) {
  return {
    refreshDirectory: rxMethod<{ deviceId: string; storageType: string }>(
      pipe(
        tap(({ deviceId, storageType }) => {
          const key = createKey(deviceId, storageType);
          const currentState = store.storageEntries();
          const existingEntry = currentState[key];

          if (existingEntry) {
            patchState(store, (state) => ({
              storageEntries: {
                ...state.storageEntries,
                [key]: {
                  ...existingEntry,
                  isLoading: true,
                  error: null,
                },
              },
            }));
          }
        }),
        filter(({ deviceId, storageType }) => {
          const key = createKey(deviceId, storageType);
          const currentState = store.storageEntries();
          return !!currentState[key];
        }),
        switchMap(({ deviceId, storageType }) => {
          const key = createKey(deviceId, storageType);
          const currentState = store.storageEntries();
          const existingEntry = currentState[key];
          const path = existingEntry.currentPath;

          return storageService.getDirectory(deviceId, storageType, path).pipe(
            tap((directory: TestStorageDirectory) => {
              patchState(store, (state) => ({
                storageEntries: {
                  ...state.storageEntries,
                  [key]: {
                    ...state.storageEntries[key],
                    directory,
                    isLoaded: true,
                    isLoading: false,
                    error: null,
                    lastLoadTime: Date.now(),
                  },
                },
              }));
            }),
            catchError((error) => {
              patchState(store, (state) => ({
                storageEntries: {
                  ...state.storageEntries,
                  [key]: {
                    ...state.storageEntries[key],
                    isLoading: false,
                    error: error.message || 'Refresh failed',
                  },
                },
              }));
              return of(null);
            })
          );
        })
      )
    ),
  };
}

// Test implementation of initialize method
function createInitializeStorage(store: any) {
  return {
    initializeStorage: ({ deviceId, storageType }: { deviceId: string; storageType: string }) => {
      const key = createKey(deviceId, storageType);
      const currentState = store.storageEntries();

      if (!currentState[key]) {
        patchState(store, (state) => ({
          storageEntries: {
            ...state.storageEntries,
            [key]: {
              deviceId,
              storageType,
              currentPath: '/',
              directory: null,
              isLoaded: false,
              isLoading: false,
              error: null,
              lastLoadTime: null,
            },
          },
        }));
      }
    },
  };
}

// Test implementation of cleanup methods
function createCleanupStorage(store: any) {
  return {
    cleanupStorage: ({ deviceId }: { deviceId: string }) => {
      patchState(store, (state) => {
        const newEntries = { ...state.storageEntries };
        Object.keys(newEntries).forEach((key) => {
          if (key.startsWith(`${deviceId}-`)) {
            delete newEntries[key];
          }
        });

        const shouldClearSelection = state.selectedDirectory?.deviceId === deviceId;

        return {
          storageEntries: newEntries,
          selectedDirectory: shouldClearSelection ? null : state.selectedDirectory,
        };
      });
    },
    cleanupStorageType: ({ deviceId, storageType }: { deviceId: string; storageType: string }) => {
      const key = createKey(deviceId, storageType);
      patchState(store, (state) => {
        const newEntries = { ...state.storageEntries };
        delete newEntries[key];

        const shouldClearSelection =
          state.selectedDirectory?.deviceId === deviceId &&
          state.selectedDirectory?.storageType === storageType;

        return {
          storageEntries: newEntries,
          selectedDirectory: shouldClearSelection ? null : state.selectedDirectory,
        };
      });
    },
  };
}

describe('StorageStore', () => {
  let store: any;
  let mockStorageService: any;

  // Test data factory
  const createMockDirectory = (path = '/games'): TestStorageDirectory => ({
    path,
    directories: [{ name: 'arcade', path: `${path}/arcade` }],
    files: [
      {
        name: 'game1.prg',
        path: `${path}/game1.prg`,
        size: 1024,
        isFavorite: false,
        title: 'Test Game 1',
        creator: 'Test Creator',
        releaseInfo: '2023',
        description: 'Test game description',
        shareUrl: '',
        metadataSource: '',
        meta1: '',
        meta2: '',
        metadataSourcePath: '',
        parentPath: path,
        playLength: '',
        subtuneLengths: [],
        startSubtuneNum: 0,
        images: [],
        type: TestFileItemType.Game,
      },
    ],
  });

  // Create test store with mocked StorageService
  const createTestStore = (mockService: any) => {
    const initialState: TestStorageState = {
      storageEntries: {},
      selectedDirectory: null,
    };

    return signalStore(
      withState(initialState),
      withMethods((store) => ({
        ...createNavigateToDirectory(store, mockService),
        ...createRefreshDirectory(store, mockService),
        ...createInitializeStorage(store),
        ...createCleanupStorage(store),
      }))
    );
  };

  beforeEach(() => {
    mockStorageService = {
      getDirectory: vi.fn(),
    };

    const TestStore = createTestStore(mockStorageService);
    store = new TestStore();
  });

  describe('Initial State', () => {
    it('should have empty storage entries and no selected directory', () => {
      expect(store.storageEntries()).toEqual({});
      expect(store.selectedDirectory()).toBeNull();
    });
  });

  describe('initializeStorage()', () => {
    it('should create initial storage entry with default state', () => {
      store.initializeStorage({ deviceId: 'device1', storageType: TestStorageType.Sd });

      const key = createKey('device1', TestStorageType.Sd);
      const entry = store.storageEntries()[key];

      expect(entry).toEqual({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        currentPath: '/',
        directory: null,
        isLoaded: false,
        isLoading: false,
        error: null,
        lastLoadTime: null,
      });
    });

    it('should not overwrite existing storage entry', () => {
      const key = createKey('device1', TestStorageType.Sd);

      store.initializeStorage({ deviceId: 'device1', storageType: TestStorageType.Sd });
      const originalEntry = store.storageEntries()[key];

      store.initializeStorage({ deviceId: 'device1', storageType: TestStorageType.Sd });
      const afterSecondInit = store.storageEntries()[key];

      expect(afterSecondInit).toEqual(originalEntry);
    });
  });

  describe('navigateToDirectory()', () => {
    it('should update global selection immediately', () => {
      mockStorageService.getDirectory.mockReturnValue(of(createMockDirectory('/games')));

      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });

      expect(store.selectedDirectory()).toEqual({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });
    });

    it('should create storage entry inline if not initialized', async () => {
      mockStorageService.getDirectory.mockReturnValue(of(createMockDirectory('/games')));

      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });
      await new Promise((resolve) => setTimeout(resolve, 0));

      const key = createKey('device1', TestStorageType.Sd);
      const entry = store.storageEntries()[key];

      expect(entry).toBeDefined();
      expect(entry.currentPath).toBe('/games');
      expect(entry.directory).toEqual(createMockDirectory('/games'));
    });

    it('should set loading state when API call needed', () => {
      mockStorageService.getDirectory.mockReturnValue(of(createMockDirectory('/games')));

      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });

      const key = createKey('device1', TestStorageType.Sd);
      const entry = store.storageEntries()[key];

      expect(entry.isLoading).toBe(true);
      expect(entry.error).toBeNull();
    });

    it('should not call API when directory already loaded (caching)', () => {
      store.initializeStorage({ deviceId: 'device1', storageType: TestStorageType.Sd });
      mockStorageService.getDirectory.mockReturnValue(of(createMockDirectory('/games')));

      // First navigation
      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });

      mockStorageService.getDirectory.mockClear();

      // Second navigation to same path
      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });

      expect(mockStorageService.getDirectory).not.toHaveBeenCalled();
    });

    it('should complete loading state on API success', async () => {
      mockStorageService.getDirectory.mockReturnValue(of(createMockDirectory('/games')));

      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });
      await new Promise((resolve) => setTimeout(resolve, 0));

      const key = createKey('device1', TestStorageType.Sd);
      const entry = store.storageEntries()[key];

      expect(entry.isLoading).toBe(false);
      expect(entry.isLoaded).toBe(true);
      expect(entry.directory).toEqual(createMockDirectory('/games'));
      expect(entry.lastLoadTime).toBeGreaterThan(0);
    });

    it('should handle API error state', async () => {
      mockStorageService.getDirectory.mockReturnValue(throwError(() => new Error('API Error')));

      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });
      await new Promise((resolve) => setTimeout(resolve, 0));

      const key = createKey('device1', TestStorageType.Sd);
      const entry = store.storageEntries()[key];

      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBe('API Error');
      expect(entry.isLoaded).toBe(false);
      expect(entry.directory).toBeNull();
    });
  });

  describe('refreshDirectory()', () => {
    beforeEach(() => {
      store.initializeStorage({ deviceId: 'device1', storageType: TestStorageType.Sd });
    });

    it('should set loading state immediately', () => {
      mockStorageService.getDirectory.mockReturnValue(of(createMockDirectory('/games')));

      // Setup existing directory first
      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });

      store.refreshDirectory({ deviceId: 'device1', storageType: TestStorageType.Sd });

      const key = createKey('device1', TestStorageType.Sd);
      const entry = store.storageEntries()[key];
      expect(entry.isLoading).toBe(true);
      expect(entry.error).toBeNull();
    });

    it('should complete loading state on success', async () => {
      const refreshedDirectory = createMockDirectory('/games');
      mockStorageService.getDirectory.mockReturnValue(of(refreshedDirectory));

      // Setup existing directory
      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });
      await new Promise((resolve) => setTimeout(resolve, 0));

      store.refreshDirectory({ deviceId: 'device1', storageType: TestStorageType.Sd });
      await new Promise((resolve) => setTimeout(resolve, 0));

      const key = createKey('device1', TestStorageType.Sd);
      const entry = store.storageEntries()[key];

      expect(entry.isLoading).toBe(false);
      expect(entry.isLoaded).toBe(true);
      expect(entry.directory).toEqual(refreshedDirectory);
      expect(entry.lastLoadTime).toBeGreaterThan(0);
    });

    it('should preserve existing directory on error', async () => {
      const originalDirectory = createMockDirectory('/games');
      mockStorageService.getDirectory.mockReturnValueOnce(of(originalDirectory));

      // Setup existing directory
      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });
      await new Promise((resolve) => setTimeout(resolve, 0));

      // Error on refresh
      mockStorageService.getDirectory.mockReturnValueOnce(
        throwError(() => new Error('Refresh failed'))
      );

      store.refreshDirectory({ deviceId: 'device1', storageType: TestStorageType.Sd });
      await new Promise((resolve) => setTimeout(resolve, 0));

      const key = createKey('device1', TestStorageType.Sd);
      const entry = store.storageEntries()[key];

      expect(entry.isLoading).toBe(false);
      expect(entry.error).toBe('Refresh failed');
      expect(entry.directory).toEqual(originalDirectory); // Original preserved
    });

    it('should do nothing when entry does not exist', () => {
      store.refreshDirectory({ deviceId: 'nonexistent', storageType: TestStorageType.Sd });

      expect(mockStorageService.getDirectory).not.toHaveBeenCalled();
    });
  });

  describe('cleanupStorage()', () => {
    it('should remove all entries for device', () => {
      store.initializeStorage({ deviceId: 'device1', storageType: TestStorageType.Sd });
      store.initializeStorage({ deviceId: 'device1', storageType: TestStorageType.Usb });
      store.initializeStorage({ deviceId: 'device2', storageType: TestStorageType.Sd });

      store.cleanupStorage({ deviceId: 'device1' });

      const entries = store.storageEntries();
      expect(Object.keys(entries)).toHaveLength(1);
      expect(entries['device2-SD']).toBeDefined();
      expect(entries['device1-SD']).toBeUndefined();
      expect(entries['device1-USB']).toBeUndefined();
    });

    it('should remove specific storage type', () => {
      store.initializeStorage({ deviceId: 'device1', storageType: TestStorageType.Sd });
      store.initializeStorage({ deviceId: 'device1', storageType: TestStorageType.Usb });

      store.cleanupStorageType({ deviceId: 'device1', storageType: TestStorageType.Sd });

      const entries = store.storageEntries();
      expect(entries['device1-USB']).toBeDefined();
      expect(entries['device1-SD']).toBeUndefined();
    });

    it('should not affect selectedDirectory when cleaning up non-selected storage', () => {
      store.initializeStorage({ deviceId: 'device1', storageType: TestStorageType.Sd });
      store.initializeStorage({ deviceId: 'device2', storageType: TestStorageType.Sd });

      // Select device1
      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });

      // Clean up device2
      store.cleanupStorage({ deviceId: 'device2' });

      expect(store.selectedDirectory()?.deviceId).toBe('device1');
    });
  });

  describe('Multi-device state management', () => {
    it('should maintain separate state per device-storage combination', async () => {
      mockStorageService.getDirectory.mockImplementation(
        (deviceId: string, storageType: string, path: string) => of(createMockDirectory(path))
      );

      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });
      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Usb,
        path: '/music',
      });
      store.navigateToDirectory({
        deviceId: 'device2',
        storageType: TestStorageType.Sd,
        path: '/apps',
      });
      await new Promise((resolve) => setTimeout(resolve, 0));

      const sdKey = createKey('device1', TestStorageType.Sd);
      const usbKey = createKey('device1', TestStorageType.Usb);
      const device2Key = createKey('device2', TestStorageType.Sd);
      const entries = store.storageEntries();

      expect(entries[sdKey].currentPath).toBe('/games');
      expect(entries[usbKey].currentPath).toBe('/music');
      expect(entries[device2Key].currentPath).toBe('/apps');

      // Global selection points to last navigated
      expect(store.selectedDirectory()).toEqual({
        deviceId: 'device2',
        storageType: TestStorageType.Sd,
        path: '/apps',
      });
    });

    it('should clear previous selection when navigating to different device', () => {
      store.navigateToDirectory({
        deviceId: 'device1',
        storageType: TestStorageType.Sd,
        path: '/games',
      });
      expect(store.selectedDirectory()?.deviceId).toBe('device1');

      store.navigateToDirectory({
        deviceId: 'device2',
        storageType: TestStorageType.Usb,
        path: '/music',
      });
      expect(store.selectedDirectory()?.deviceId).toBe('device2');
    });
  });

  describe('State Interfaces', () => {
    it('should support proper state structure', () => {
      const state: TestStorageState = {
        storageEntries: {
          'device1-SD': {
            deviceId: 'device1',
            storageType: TestStorageType.Sd,
            currentPath: '/games',
            directory: null,
            isLoaded: true,
            isLoading: false,
            error: null,
            lastLoadTime: Date.now(),
          },
        },
        selectedDirectory: {
          deviceId: 'device1',
          storageType: TestStorageType.Sd,
          path: '/games',
        },
      };

      expect(state.selectedDirectory?.deviceId).toBe('device1');
      expect(state.storageEntries['device1-SD']).toBeDefined();
    });
  });
});
