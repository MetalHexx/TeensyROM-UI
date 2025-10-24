import { describe, it, expect, beforeEach, vi, type MockedFunction } from 'vitest';
import { of, throwError, type Observable } from 'rxjs';
import '@analogjs/vitest-angular/setup-zone';
import { TestBed, getTestBed } from '@angular/core/testing';
import {
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting,
} from '@angular/platform-browser-dynamic/testing';

import { StorageStore, StorageDirectoryState } from './storage-store';
import {
  StorageDirectory,
  StorageType,
  IStorageService,
  STORAGE_SERVICE,
  FileItem,
  FileItemType,
} from '@teensyrom-nx/domain';
import { StorageKeyUtil } from './storage-key.util';

// -----------------------------
// Type Definitions
// -----------------------------

type StorageStoreInstance = {
  storageEntries: () => Record<string, StorageDirectoryState>;
  favoriteOperationsState: () => { isProcessing: boolean; error: string | null };
  initializeStorage: (args: { deviceId: string; storageType: StorageType }) => Promise<void>;
  navigateToDirectory: (args: {
    deviceId: string;
    storageType: StorageType;
    path: string;
  }) => Promise<void>;
  saveFavorite: (args: {
    deviceId: string;
    storageType: StorageType;
    filePath: string;
  }) => Promise<void>;
  removeFavorite: (args: {
    deviceId: string;
    storageType: StorageType;
    filePath: string;
  }) => Promise<void>;
};

// -----------------------------
// Test Data Factories
// -----------------------------

const createMockStorageDirectory = (path = '/'): StorageDirectory => ({
  path,
  directories: [{ name: 'dir', path: `${path}/dir` }],
  files: [],
});

const createMockFileItem = (overrides: Partial<FileItem> = {}): FileItem => ({
  name: 'test.sid',
  path: '/test.sid',
  size: 1024,
  isFavorite: false,
  isCompatible: true,
  title: 'Test Title',
  creator: 'Test Creator',
  releaseInfo: '',
  description: '',
  shareUrl: '',
  metadataSource: '',
  meta1: '',
  meta2: '',
  metadataSourcePath: '',
  parentPath: '/',
  playLength: '0:00',
  subtuneLengths: [],
  startSubtuneNum: 0,
  images: [],
  type: FileItemType.Song,
  links: [],
  tags: [],
  youTubeVideos: [],
  competitions: [],
  ratingCount: 0,
  ...overrides,
});

// -----------------------------
// Test Suite Setup
// -----------------------------

describe('StorageStore - Favorite Operations', () => {
  let store: StorageStoreInstance;
  type GetDirectoryFn = (
    deviceId: string,
    storageType: StorageType,
    path?: string
  ) => Observable<StorageDirectory>;
  let getDirectoryMock: MockedFunction<GetDirectoryFn>;
  let saveFavoriteMock: MockedFunction<
    (deviceId: string, storageType: StorageType, filePath: string) => Observable<FileItem>
  >;
  let removeFavoriteMock: MockedFunction<
    (deviceId: string, storageType: StorageType, filePath: string) => Observable<void>
  >;
  let mockStorageService: IStorageService;

  const createTestStore = () => {
    getDirectoryMock = vi.fn<GetDirectoryFn>();
    saveFavoriteMock = vi.fn();
    removeFavoriteMock = vi.fn();

    mockStorageService = {
      getDirectory: getDirectoryMock,
      index: vi.fn().mockResolvedValue({}),
      indexAll: vi.fn().mockResolvedValue({}),
      search: vi.fn(),
      saveFavorite: saveFavoriteMock,
      removeFavorite: removeFavoriteMock,
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
  // saveFavorite() - Save Favorite Functionality
  // -----------------------------------------
  describe('saveFavorite() - Save Favorite Functionality', () => {
    it('should set processing state, call API, and update file isFavorite flag on success', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      const filePath = '/games/test.sid';
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Setup directory with file
      const mockDirectory = createMockStorageDirectory('/games');
      mockDirectory.files = [
        createMockFileItem({ name: 'test.sid', path: filePath, isFavorite: false }),
      ];

      getDirectoryMock.mockReturnValue(of(mockDirectory));
      await store.initializeStorage({ deviceId, storageType });
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      // Mock API response
      const updatedFile = createMockFileItem({
        name: 'test.sid',
        path: filePath,
        isFavorite: true,
      });
      saveFavoriteMock.mockReturnValue(of(updatedFile));

      await store.saveFavorite({ deviceId, storageType, filePath });

      // Verify API was called
      expect(saveFavoriteMock).toHaveBeenCalledWith(deviceId, storageType, filePath);

      // Verify file was updated in directory
      const entry = store.storageEntries()[key];
      expect(entry.directory?.files?.[0].isFavorite).toBe(true);

      // Verify processing state was cleared
      expect(store.favoriteOperationsState().isProcessing).toBe(false);
      expect(store.favoriteOperationsState().error).toBeNull();
    });

    it('should handle API error by setting error state', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      const filePath = '/games/test.sid';
      const errorMessage = 'Failed to save favorite';

      // Setup directory
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.initializeStorage({ deviceId, storageType });

      // Mock API error
      saveFavoriteMock.mockReturnValue(throwError(() => new Error(errorMessage)));

      await store.saveFavorite({ deviceId, storageType, filePath });

      // Verify error state
      expect(store.favoriteOperationsState().isProcessing).toBe(false);
      expect(store.favoriteOperationsState().error).toBe(errorMessage);
    });

    it('should not update directory if file is not found in current directory', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      const filePath = '/other/test.sid'; // Different path
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Setup directory
      const mockDirectory = createMockStorageDirectory('/games');
      mockDirectory.files = [
        createMockFileItem({
          name: 'different.sid',
          path: '/games/different.sid',
          isFavorite: false,
        }),
      ];

      getDirectoryMock.mockReturnValue(of(mockDirectory));
      await store.initializeStorage({ deviceId, storageType });
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      // Mock API response
      const updatedFile = createMockFileItem({
        name: 'test.sid',
        path: filePath,
        isFavorite: true,
      });
      saveFavoriteMock.mockReturnValue(of(updatedFile));

      await store.saveFavorite({ deviceId, storageType, filePath });

      // Verify directory was not modified (file not in current directory)
      const entry = store.storageEntries()[key];
      expect(entry.directory?.files?.[0].isFavorite).toBe(false);
      expect(entry.directory?.files?.[0].path).toBe('/games/different.sid');

      // Operation should still succeed
      expect(store.favoriteOperationsState().isProcessing).toBe(false);
      expect(store.favoriteOperationsState().error).toBeNull();
    });

    it('should handle case where storage entry does not exist', async () => {
      const deviceId = 'nonexistent';
      const storageType = StorageType.Sd;
      const filePath = '/test.sid';

      // Mock API response
      const updatedFile = createMockFileItem({
        name: 'test.sid',
        path: filePath,
        isFavorite: true,
      });
      saveFavoriteMock.mockReturnValue(of(updatedFile));

      await store.saveFavorite({ deviceId, storageType, filePath });

      // Should complete without error even if no directory loaded
      expect(store.favoriteOperationsState().isProcessing).toBe(false);
      expect(store.favoriteOperationsState().error).toBeNull();
    });
  });

  // -----------------------------------------
  // removeFavorite() - Remove Favorite Functionality
  // -----------------------------------------
  describe('removeFavorite() - Remove Favorite Functionality', () => {
    it('should set processing state, call API, and update file isFavorite flag for non-favorites directory', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      const filePath = '/games/test.sid';
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Setup directory with file
      const mockDirectory = createMockStorageDirectory('/games');
      mockDirectory.files = [
        createMockFileItem({ name: 'test.sid', path: filePath, isFavorite: true }),
      ];

      getDirectoryMock.mockReturnValue(of(mockDirectory));
      await store.initializeStorage({ deviceId, storageType });
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      // Mock API response
      removeFavoriteMock.mockReturnValue(of(void 0));

      await store.removeFavorite({ deviceId, storageType, filePath });

      // Verify API was called
      expect(removeFavoriteMock).toHaveBeenCalledWith(deviceId, storageType, filePath);

      // Verify file was updated in directory
      const entry = store.storageEntries()[key];
      expect(entry.directory?.files?.[0].isFavorite).toBe(false);

      // Verify processing state was cleared
      expect(store.favoriteOperationsState().isProcessing).toBe(false);
      expect(store.favoriteOperationsState().error).toBeNull();
    });

    it('should remove file from favorites directory listing', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      const filePath = '/favorites/test.sid';
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Setup favorites directory with file
      const mockDirectory = createMockStorageDirectory('/favorites');
      mockDirectory.files = [
        createMockFileItem({ name: 'test.sid', path: filePath, isFavorite: true }),
        createMockFileItem({
          name: 'other.sid',
          path: '/favorites/other.sid',
          isFavorite: true,
        }),
      ];

      getDirectoryMock.mockReturnValue(of(mockDirectory));
      await store.initializeStorage({ deviceId, storageType });
      await store.navigateToDirectory({ deviceId, storageType, path: '/favorites' });

      // Mock API response
      removeFavoriteMock.mockReturnValue(of(void 0));

      await store.removeFavorite({ deviceId, storageType, filePath });

      // Verify file was removed from directory
      const entry = store.storageEntries()[key];
      expect(entry.directory?.files?.length).toBe(1);
      expect(entry.directory?.files?.[0].path).toBe('/favorites/other.sid');

      // Verify processing state was cleared
      expect(store.favoriteOperationsState().isProcessing).toBe(false);
      expect(store.favoriteOperationsState().error).toBeNull();
    });

    it('should handle API error by setting error state', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      const filePath = '/games/test.sid';
      const errorMessage = 'Failed to remove favorite';

      // Setup directory
      getDirectoryMock.mockReturnValue(of(createMockStorageDirectory('/games')));
      await store.initializeStorage({ deviceId, storageType });

      // Mock API error
      removeFavoriteMock.mockReturnValue(throwError(() => new Error(errorMessage)));

      await store.removeFavorite({ deviceId, storageType, filePath });

      // Verify error state
      expect(store.favoriteOperationsState().isProcessing).toBe(false);
      expect(store.favoriteOperationsState().error).toBe(errorMessage);
    });

    it('should not update directory if file is not found in current directory', async () => {
      const deviceId = 'device-1';
      const storageType = StorageType.Sd;
      const filePath = '/other/test.sid'; // Different path
      const key = StorageKeyUtil.create(deviceId, storageType);

      // Setup directory
      const mockDirectory = createMockStorageDirectory('/games');
      mockDirectory.files = [
        createMockFileItem({
          name: 'different.sid',
          path: '/games/different.sid',
          isFavorite: true,
        }),
      ];

      getDirectoryMock.mockReturnValue(of(mockDirectory));
      await store.initializeStorage({ deviceId, storageType });
      await store.navigateToDirectory({ deviceId, storageType, path: '/games' });

      // Mock API response
      removeFavoriteMock.mockReturnValue(of(void 0));

      await store.removeFavorite({ deviceId, storageType, filePath });

      // Verify directory was not modified (file not in current directory)
      const entry = store.storageEntries()[key];
      expect(entry.directory?.files?.[0].isFavorite).toBe(true);
      expect(entry.directory?.files?.[0].path).toBe('/games/different.sid');

      // Operation should still succeed
      expect(store.favoriteOperationsState().isProcessing).toBe(false);
      expect(store.favoriteOperationsState().error).toBeNull();
    });

    it('should handle case where storage entry does not exist', async () => {
      const deviceId = 'nonexistent';
      const storageType = StorageType.Sd;
      const filePath = '/test.sid';

      // Mock API response
      removeFavoriteMock.mockReturnValue(of(void 0));

      await store.removeFavorite({ deviceId, storageType, filePath });

      // Should complete without error even if no directory loaded
      expect(store.favoriteOperationsState().isProcessing).toBe(false);
      expect(store.favoriteOperationsState().error).toBeNull();
    });
  });
});
