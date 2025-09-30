import { describe, it, expect, beforeEach, vi, MockedFunction } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { Observable, of, throwError } from 'rxjs';
import {
  FileItem,
  FileItemType,
  StorageType,
  LaunchMode,
  PlayerStatus,
  PlayerScope,
  PlayerFilterType,
  PLAYER_SERVICE,
  DEVICE_SERVICE,
} from '@teensyrom-nx/domain';
import { PlayerContextService } from './player-context.service';
import { PlayerStore } from './player-store';
import { StorageStore, StorageDirectoryState } from '../storage/storage-store';

// Test data factory functions
const createTestFileItem = (
  overrides: Partial<FileItem> = {}
): FileItem => ({
  name: 'test-file.sid',
  path: '/music/test-file.sid',
  size: 4096,
  type: FileItemType.Song,
  isFavorite: false,
  title: 'Test Song',
  creator: 'Test Artist',
  releaseInfo: '2025',
  description: 'Test song description',
  shareUrl: '',
  metadataSource: '',
  meta1: '',
  meta2: '',
  metadataSourcePath: '',
  parentPath: '/music',
  playLength: '3:45',
  subtuneLengths: [],
  startSubtuneNum: 0,
  images: [],
  ...overrides,
});

const createTestDirectoryFiles = (): FileItem[] => [
  createTestFileItem({ name: 'song1.sid', path: '/music/song1.sid' }),
  createTestFileItem({ name: 'song2.sid', path: '/music/song2.sid' }),
  createTestFileItem({ name: 'game.prg', path: '/music/game.prg', type: FileItemType.Game }),
];

// Mock types
type LaunchFileFn = (deviceId: string, storageType: StorageType, filePath: string) => Observable<FileItem>;
type LaunchRandomFn = (deviceId: string, scope: PlayerScope, filter: PlayerFilterType, startingDirectory?: string) => Observable<FileItem>;
type ToggleMusicFn = (deviceId: string) => Observable<void>;
type ResetDeviceFn = (deviceId: string) => Observable<void>;
type PingDeviceFn = (deviceId: string) => Observable<void>;

type NavigateToDirectoryFn = (params: { deviceId: string; storageType: StorageType; path: string }) => Promise<void>;
type GetSelectedDirectoryStateFn = (deviceId: string) => () => Partial<StorageDirectoryState> | null;

describe('PlayerContextService', () => {
  let service: PlayerContextService;
  let mockStorageStore: {
    navigateToDirectory: MockedFunction<NavigateToDirectoryFn>;
    getSelectedDirectoryState: MockedFunction<GetSelectedDirectoryStateFn>;
  };
  let mockPlayerService: {
    launchFile: MockedFunction<LaunchFileFn>;
    launchRandom: MockedFunction<LaunchRandomFn>;
    toggleMusic: MockedFunction<ToggleMusicFn>;
    resetDevice: MockedFunction<ResetDeviceFn>;
  };
  let mockDeviceService: {
    resetDevice: MockedFunction<ResetDeviceFn>;
    pingDevice: MockedFunction<PingDeviceFn>;
  };

  // Helper to wait for async operations
  const nextTick = () => new Promise<void>((r) => setTimeout(r, 0));

  beforeEach(() => {
    // Create mocks
    mockPlayerService = {
      launchFile: vi.fn<LaunchFileFn>(),
      launchRandom: vi.fn<LaunchRandomFn>(),
      toggleMusic: vi.fn<ToggleMusicFn>(),
      resetDevice: vi.fn<ResetDeviceFn>(),
    };

    mockDeviceService = {
      resetDevice: vi.fn<ResetDeviceFn>(),
      pingDevice: vi.fn<PingDeviceFn>(),
    };

    mockStorageStore = {
      navigateToDirectory: vi.fn(),
      getSelectedDirectoryState: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        PlayerContextService,
        PlayerStore, // Real store - we want to test integration
        { provide: PLAYER_SERVICE, useValue: mockPlayerService },
        { provide: DEVICE_SERVICE, useValue: mockDeviceService },
        { provide: StorageStore, useValue: mockStorageStore },
      ],
    });

    service = TestBed.inject(PlayerContextService);
  });

  describe('Player Initialization & Cleanup', () => {
    it('should initialize player state for device', () => {
      const deviceId = 'device-123';

      service.initializePlayer(deviceId);

      // Verify player was initialized via signal getter
      const currentFile = service.getCurrentFile(deviceId);
      expect(currentFile()).toBeNull();
      
      const status = service.getStatus(deviceId);
      expect(status()).toBe(PlayerStatus.Stopped);
    });

    it('should remove player state for device', () => {
      const deviceId = 'device-123';

      // Initialize then remove
      service.initializePlayer(deviceId);
      service.removePlayer(deviceId);

      // Verify player was removed by checking signals return defaults
      const currentFile = service.getCurrentFile(deviceId);
      expect(currentFile()).toBeNull();
    });

    it('should handle multiple devices independently', () => {
      const device1 = 'device-1';
      const device2 = 'device-2';

      service.initializePlayer(device1);
      service.initializePlayer(device2);

      // Both should be independent
      expect(service.getCurrentFile(device1)()).toBeNull();
      expect(service.getCurrentFile(device2)()).toBeNull();
      expect(service.getStatus(device1)()).toBe(PlayerStatus.Stopped);
      expect(service.getStatus(device2)()).toBe(PlayerStatus.Stopped);

      // Remove one shouldn't affect the other
      service.removePlayer(device1);
      expect(service.getCurrentFile(device2)()).toBeNull(); // Still exists
      expect(service.getStatus(device2)()).toBe(PlayerStatus.Stopped); // Still exists
    });
  });

  describe('Phase 1: File Launching with Context', () => {
    const deviceId = 'device-123';
    const testFile = createTestFileItem();
    const testFiles = createTestDirectoryFiles();

    it('should launch file with directory context successfully', async () => {
      mockPlayerService.launchFile.mockReturnValue(of(testFile));

      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: testFile,
        directoryPath: '/music',
        files: testFiles,
        launchMode: LaunchMode.Directory,
      });

      await nextTick();

      // Verify infrastructure call
      expect(mockPlayerService.launchFile).toHaveBeenCalledWith(
        deviceId,
        StorageType.Sd,
        testFile.path
      );

      // Verify state updates through signals
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile).toBeTruthy();
      expect(currentFile?.file).toEqual(testFile);
      expect(currentFile?.launchMode).toBe(LaunchMode.Directory);

      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.files).toEqual(testFiles);
      expect(fileContext?.directoryPath).toBe('/music');
      expect(fileContext?.currentIndex).toBe(0); // testFile is first in testFiles

      expect(service.isLoading(deviceId)()).toBe(false);
      expect(service.getError(deviceId)()).toBeNull();
    });

    it('should handle launch file API error gracefully', async () => {
      const error = new Error('Launch failed');
      mockPlayerService.launchFile.mockReturnValue(throwError(() => error));

      // The service should handle errors gracefully without throwing
      try {
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: testFile,
          directoryPath: '/music',
          files: testFiles,
        });
      } catch {
        // Expected to complete without throwing
      }

      await nextTick();

      // Verify error state
      expect(service.getError(deviceId)()).toBeTruthy();
      expect(service.isLoading(deviceId)()).toBe(false);
      expect(service.getCurrentFile(deviceId)()).toBeNull();
    });

    it('should default to Directory launch mode when not specified', async () => {
      mockPlayerService.launchFile.mockReturnValue(of(testFile));

      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: testFile,
        directoryPath: '/music',
        files: testFiles,
        // launchMode not specified
      });

      await nextTick();

      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.launchMode).toBe(LaunchMode.Directory);
    });

    it('should set loading state during launch operation', async () => {
      // Create a delayed observable to simulate async loading
      let triggerResolve: () => void;
      const loadingPromise = new Promise<void>((resolve) => {
        triggerResolve = resolve;
      });
      
      const delayedObservable = new Observable<FileItem>((subscriber) => {
        loadingPromise.then(() => {
          subscriber.next(testFile);
          subscriber.complete();
        });
      });
      
      mockPlayerService.launchFile.mockReturnValue(delayedObservable);

      // Start launch (don't await yet)
      const launchPromise = service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: testFile,
        directoryPath: '/music',
        files: testFiles,
      });

      await nextTick();

      // Should be loading
      expect(service.isLoading(deviceId)()).toBe(true);

      // Resolve the promise
      triggerResolve();
      await launchPromise;
      await nextTick();

      // Should no longer be loading
      expect(service.isLoading(deviceId)()).toBe(false);
    });
  });

  describe('Phase 2: Random File Launching & Shuffle Mode', () => {
    const deviceId = 'device-456';
    const randomFile = createTestFileItem({ name: 'random-song.sid', path: '/games/random-song.sid' });

    beforeEach(() => {
      service.initializePlayer(deviceId);
    });

    it('should launch random file successfully', async () => {
      mockPlayerService.launchRandom.mockReturnValue(of(randomFile));

      await service.launchRandomFile(deviceId);
      await nextTick();

      // Verify infrastructure call with default shuffle settings
      expect(mockPlayerService.launchRandom).toHaveBeenCalledWith(
        deviceId,
        PlayerScope.Storage, // Default scope
        PlayerFilterType.All, // Default filter
        undefined // No starting directory by default
      );

      // Verify state updates
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file).toEqual(randomFile);
      expect(currentFile?.launchMode).toBe(LaunchMode.Shuffle);

      expect(service.isLoading(deviceId)()).toBe(false);
      expect(service.getError(deviceId)()).toBeNull();
    });

    it('should attempt to load directory context after random launch', async () => {
      const mockDirectoryState: Partial<StorageDirectoryState> = {
        directory: {
          files: createTestDirectoryFiles(),
          directories: [],
          currentPath: '/games',
        },
      };

      mockPlayerService.launchRandom.mockReturnValue(of(randomFile));
      mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
      mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => mockDirectoryState);

      await service.launchRandomFile(deviceId);
      await nextTick();

      // Verify storage store was called to load directory context
      expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
        deviceId,
        storageType: StorageType.Sd,
        path: randomFile.parentPath,
      });
    });

    it('should handle directory context loading failure silently', async () => {
      mockPlayerService.launchRandom.mockReturnValue(of(randomFile));
      mockStorageStore.navigateToDirectory.mockRejectedValue(new Error('Directory load failed'));

      // Should not throw
      await expect(service.launchRandomFile(deviceId)).resolves.not.toThrow();
      await nextTick();

      // Random file should still be launched
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file).toEqual(randomFile);
    });

    it('should handle random launch API error', async () => {
      const error = new Error('Random launch failed');
      mockPlayerService.launchRandom.mockReturnValue(throwError(() => error));

      await service.launchRandomFile(deviceId);
      await nextTick();

      expect(service.getError(deviceId)()).toBeTruthy();
      expect(service.getCurrentFile(deviceId)()).toBeNull();
      expect(service.isLoading(deviceId)()).toBe(false);
    });

    describe('Shuffle Mode Toggle', () => {
      it('should toggle from Directory to Shuffle mode', () => {
        // Start in Directory mode (default)
        expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Directory);

        service.toggleShuffleMode(deviceId);

        expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Shuffle);
      });

      it('should toggle from Shuffle to Directory mode', () => {
        // Set to Shuffle first
        service.toggleShuffleMode(deviceId);
        expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Shuffle);

        // Toggle back
        service.toggleShuffleMode(deviceId);

        expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Directory);
      });
    });

    describe('Shuffle Settings Management', () => {
      it('should update shuffle scope', () => {
        service.setShuffleScope(deviceId, PlayerScope.DirectoryDeep);

        const settings = service.getShuffleSettings(deviceId)();
        expect(settings?.scope).toBe(PlayerScope.DirectoryDeep);
      });

      it('should update filter mode', () => {
        service.setFilterMode(deviceId, PlayerFilterType.Music);

        const settings = service.getShuffleSettings(deviceId)();
        expect(settings?.filter).toBe(PlayerFilterType.Music);
      });

      it('should maintain independent shuffle settings per device', () => {
        const device2 = 'device-789';
        service.initializePlayer(device2);

        service.setShuffleScope(deviceId, PlayerScope.DirectoryDeep);
        service.setFilterMode(deviceId, PlayerFilterType.Music);

        service.setShuffleScope(device2, PlayerScope.Storage);
        service.setFilterMode(device2, PlayerFilterType.Games);

        const settings1 = service.getShuffleSettings(deviceId)();
        const settings2 = service.getShuffleSettings(device2)();

        expect(settings1?.scope).toBe(PlayerScope.DirectoryDeep);
        expect(settings1?.filter).toBe(PlayerFilterType.Music);

        expect(settings2?.scope).toBe(PlayerScope.Storage);
        expect(settings2?.filter).toBe(PlayerFilterType.Games);
      });
    });
  });

  describe('Phase 3: Playback Controls', () => {
    const deviceId = 'device-789';
    const musicFile = createTestFileItem({ type: FileItemType.Song });
    // const gameFile = createTestFileItem({ type: FileItemType.Game, name: 'game.prg' });

    beforeEach(() => {
      service.initializePlayer(deviceId);
    });

    describe('Play Control', () => {
      it('should start playback when player is stopped', async () => {
        // Setup: Player should start in stopped state
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Stopped);
        
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));

        await service.play(deviceId);
        await nextTick();

        expect(mockPlayerService.toggleMusic).toHaveBeenCalledWith(deviceId);
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Playing);
        expect(service.getError(deviceId)()).toBeNull();
      });

      it('should start playback when player is paused', async () => {
        // Setup: Launch a file and pause it
        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: musicFile,
          directoryPath: '/music',
          files: [musicFile],
        });
        
        // Pause it first
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));
        await service.pause(deviceId);
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Paused);
        
        // Now test play
        await service.play(deviceId);
        await nextTick();

        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Playing);
        expect(service.getError(deviceId)()).toBeNull();
      });

      it('should NOOP when player is already playing', async () => {
        // Setup: Launch a file (which starts playing)
        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: musicFile,
          directoryPath: '/music',
          files: [musicFile],
        });
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Playing);

        // Clear previous calls
        mockPlayerService.toggleMusic.mockClear();

        // Call play when already playing
        await service.play(deviceId);
        await nextTick();

        // Should not call the API
        expect(mockPlayerService.toggleMusic).not.toHaveBeenCalled();
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Playing);
        expect(service.getError(deviceId)()).toBeNull();
      });

      it('should handle play API error', async () => {
        const error = new Error('Play failed');
        mockPlayerService.toggleMusic.mockReturnValue(throwError(() => error));

        await service.play(deviceId);
        await nextTick();

        expect(service.getError(deviceId)()).toBeTruthy();
      });
    });

    describe('Pause Control', () => {
      beforeEach(async () => {
        // Setup: Launch a music file to have something playing
        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: musicFile,
          directoryPath: '/music',
          files: [musicFile],
        });
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Playing);
      });

      it('should pause playback when player is playing', async () => {
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));

        await service.pause(deviceId);
        await nextTick();

        expect(mockPlayerService.toggleMusic).toHaveBeenCalledWith(deviceId);
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Paused);
        expect(service.getError(deviceId)()).toBeNull();
      });

      it('should NOOP when player is already paused', async () => {
        // First pause the player
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));
        await service.pause(deviceId);
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Paused);

        // Clear previous calls
        mockPlayerService.toggleMusic.mockClear();

        // Call pause when already paused
        await service.pause(deviceId);
        await nextTick();

        // Should not call the API
        expect(mockPlayerService.toggleMusic).not.toHaveBeenCalled();
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Paused);
        expect(service.getError(deviceId)()).toBeNull();
      });

      it('should NOOP when player is stopped', async () => {
        // Stop the player first
        mockDeviceService.resetDevice.mockReturnValue(of(undefined));
        await service.stop(deviceId);
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Stopped);

        // Clear previous calls
        mockPlayerService.toggleMusic.mockClear();

        // Call pause when stopped
        await service.pause(deviceId);
        await nextTick();

        // Should not call the API
        expect(mockPlayerService.toggleMusic).not.toHaveBeenCalled();
        expect(service.getPlayerStatus(deviceId)()).toBe(PlayerStatus.Stopped);
        expect(service.getError(deviceId)()).toBeNull();
      });

      it('should handle pause API error', async () => {
        const error = new Error('Pause failed');
        mockPlayerService.toggleMusic.mockReturnValue(throwError(() => error));

        await service.pause(deviceId);
        await nextTick();

        expect(service.getError(deviceId)()).toBeTruthy();
      });
    });



    describe('Stop Control', () => {
    it('should reset device for stop operation', async () => {
      mockDeviceService.resetDevice.mockReturnValue(of(undefined));

      await service.stop(deviceId);
      await nextTick();

      expect(mockDeviceService.resetDevice).toHaveBeenCalledWith(deviceId);
      expect(service.getError(deviceId)()).toBeNull();
    });      it('should handle reset device API error', async () => {
        const error = new Error('Device reset failed');
        mockDeviceService.resetDevice.mockReturnValue(throwError(() => error));

        await service.stop(deviceId);
        await nextTick();

        expect(service.getError(deviceId)()).toBeTruthy();
      });
    });
  });

  describe('Phase 3: File Navigation', () => {
    const deviceId = 'device-nav';
    const testFiles = createTestDirectoryFiles();
    const [file1, file2, file3] = testFiles;

    beforeEach(async () => {
      service.initializePlayer(deviceId);
      
      // Setup initial context with file1 as current
      mockPlayerService.launchFile.mockReturnValue(of(file1));
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: file1,
        directoryPath: '/music',
        files: testFiles,
        launchMode: LaunchMode.Directory,
      });
      await nextTick();
    });

    describe('Directory Mode Navigation', () => {
      it('should navigate to next file in directory', async () => {
        mockPlayerService.launchFile.mockReturnValue(of(file2));

        await service.next(deviceId);
        await nextTick();

        expect(mockPlayerService.launchFile).toHaveBeenCalledWith(
          deviceId,
          StorageType.Sd,
          file2.path
        );

        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file).toEqual(file2);
      });

      it('should navigate to previous file in directory', async () => {
        // First move to second file
        mockPlayerService.launchFile.mockReturnValue(of(file2));
        await service.next(deviceId);
        await nextTick();

        // Then go back
        mockPlayerService.launchFile.mockReturnValue(of(file1));
        await service.previous(deviceId);
        await nextTick();

        expect(mockPlayerService.launchFile).toHaveBeenLastCalledWith(
          deviceId,
          StorageType.Sd,
          file1.path
        );

        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file).toEqual(file1);
      });

      it('should wrap to end when navigating previous from first file', async () => {
        // Currently at file1 (first), go previous should wrap to last
        mockPlayerService.launchFile.mockReturnValue(of(file3));

        await service.previous(deviceId);
        await nextTick();

        expect(mockPlayerService.launchFile).toHaveBeenCalledWith(
          deviceId,
          StorageType.Sd,
          file3.path
        );

        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file).toEqual(file3);
      });

      it('should wrap to beginning when navigating next from last file', async () => {
        // Move to last file first
        mockPlayerService.launchFile.mockReturnValue(of(file3));
        await service.next(deviceId);
        await service.next(deviceId); // Now at file3 (last)
        await nextTick();

        // Navigate next should wrap to first
        mockPlayerService.launchFile.mockReturnValue(of(file1));
        await service.next(deviceId);
        await nextTick();

        expect(mockPlayerService.launchFile).toHaveBeenLastCalledWith(
          deviceId,
          StorageType.Sd,
          file1.path
        );
      });
    });

    describe('Shuffle Mode Navigation', () => {
      beforeEach(() => {
        // Switch to shuffle mode
        service.toggleShuffleMode(deviceId);
      });

      it('should launch random file for next in shuffle mode', async () => {
        const randomFile = createTestFileItem({ name: 'random.sid', path: '/random/random.sid' });
        mockPlayerService.launchRandom.mockReturnValue(of(randomFile));

        await service.next(deviceId);
        await nextTick();

        expect(mockPlayerService.launchRandom).toHaveBeenCalled();
        
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file).toEqual(randomFile);
        expect(currentFile?.launchMode).toBe(LaunchMode.Shuffle);
      });

      it('should launch random file for previous in shuffle mode', async () => {
        const randomFile = createTestFileItem({ name: 'random2.sid', path: '/random/random2.sid' });
        mockPlayerService.launchRandom.mockReturnValue(of(randomFile));

        await service.previous(deviceId);
        await nextTick();

        expect(mockPlayerService.launchRandom).toHaveBeenCalled();
        
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file).toEqual(randomFile);
        expect(currentFile?.launchMode).toBe(LaunchMode.Shuffle);
      });
    });

    describe('Navigation Error Handling', () => {
      it('should handle next navigation API error', async () => {
        const error = new Error('Next navigation failed');
        mockPlayerService.launchFile.mockReturnValue(throwError(() => error));

        await service.next(deviceId);
        await nextTick();

        expect(service.getError(deviceId)()).toBeTruthy();
      });

      it('should handle previous navigation API error', async () => {
        const error = new Error('Previous navigation failed');
        mockPlayerService.launchFile.mockReturnValue(throwError(() => error));

        await service.previous(deviceId);
        await nextTick();

        expect(service.getError(deviceId)()).toBeTruthy();
      });

      it('should handle navigation when no file context exists', async () => {
        // Remove the file context
        service.removePlayer(deviceId);
        service.initializePlayer(deviceId);

        // Should not throw and should handle gracefully
        await expect(service.next(deviceId)).resolves.not.toThrow();
        await expect(service.previous(deviceId)).resolves.not.toThrow();
      });
    });
  });

  describe('Signal API & State Queries', () => {
    const deviceId = 'device-signals';

    beforeEach(() => {
      service.initializePlayer(deviceId);
    });

    it('should provide reactive signals for all state properties', () => {
      // All signal getters should be functions that return current state
      expect(typeof service.getCurrentFile(deviceId)).toBe('function');
      expect(typeof service.getFileContext(deviceId)).toBe('function');
      expect(typeof service.isLoading(deviceId)).toBe('function');
      expect(typeof service.getError(deviceId)).toBe('function');
      expect(typeof service.getStatus(deviceId)).toBe('function');
      expect(typeof service.getShuffleSettings(deviceId)).toBe('function');
      expect(typeof service.getLaunchMode(deviceId)).toBe('function');
      expect(typeof service.getPlayerStatus(deviceId)).toBe('function');
    });

    it('should return consistent values from getStatus and getPlayerStatus', () => {
      const status1 = service.getStatus(deviceId)();
      const status2 = service.getPlayerStatus(deviceId)();
      
      expect(status1).toBe(status2);
      expect(status1).toBe(PlayerStatus.Stopped); // Initial state
    });

    it('should return null values for uninitialized device', () => {
      const uninitializedDevice = 'device-uninitialized';

      expect(service.getCurrentFile(uninitializedDevice)()).toBeNull();
      expect(service.getFileContext(uninitializedDevice)()).toBeNull();
      expect(service.getError(uninitializedDevice)()).toBeNull();
      expect(service.getShuffleSettings(uninitializedDevice)()).toBeNull();
    });

    it('should return default values for uninitialized device state flags', () => {
      const uninitializedDevice = 'device-uninitialized';

      expect(service.isLoading(uninitializedDevice)()).toBe(false);
      expect(service.getStatus(uninitializedDevice)()).toBe(PlayerStatus.Stopped);
      expect(service.getLaunchMode(uninitializedDevice)()).toBe(LaunchMode.Directory);
    });
  });

  describe('Multi-Device Isolation', () => {
    const device1 = 'device-1';
    const device2 = 'device-2';
    const file1 = createTestFileItem({ name: 'song1.sid' });
    const file2 = createTestFileItem({ name: 'song2.sid' });

    beforeEach(() => {
      service.initializePlayer(device1);
      service.initializePlayer(device2);
    });

    it('should maintain independent file states per device', async () => {
      mockPlayerService.launchFile.mockReturnValue(of(file1));
      await service.launchFileWithContext({
        deviceId: device1,
        storageType: StorageType.Sd,
        file: file1,
        directoryPath: '/music',
        files: [file1],
      });

      mockPlayerService.launchFile.mockReturnValue(of(file2));
      await service.launchFileWithContext({
        deviceId: device2,
        storageType: StorageType.Usb,
        file: file2,
        directoryPath: '/games',
        files: [file2],
      });

      await nextTick();

      // Each device should have its own current file
      expect(service.getCurrentFile(device1)()?.file).toEqual(file1);
      expect(service.getCurrentFile(device2)()?.file).toEqual(file2);
    });

    it('should maintain independent shuffle settings per device', () => {
      service.setShuffleScope(device1, PlayerScope.DirectoryDeep);
      service.setFilterMode(device1, PlayerFilterType.Music);

      service.setShuffleScope(device2, PlayerScope.Storage);
      service.setFilterMode(device2, PlayerFilterType.Games);

      const settings1 = service.getShuffleSettings(device1)();
      const settings2 = service.getShuffleSettings(device2)();

      expect(settings1?.scope).toBe(PlayerScope.DirectoryDeep);
      expect(settings1?.filter).toBe(PlayerFilterType.Music);

      expect(settings2?.scope).toBe(PlayerScope.Storage);
      expect(settings2?.filter).toBe(PlayerFilterType.Games);
    });

    it('should maintain independent launch modes per device', () => {
      service.toggleShuffleMode(device1); // Switch to Shuffle
      // device2 stays in Directory mode

      expect(service.getLaunchMode(device1)()).toBe(LaunchMode.Shuffle);
      expect(service.getLaunchMode(device2)()).toBe(LaunchMode.Directory);
    });

    it('should maintain independent error states per device', async () => {
      // Cause error on device1
      mockPlayerService.launchFile.mockReturnValue(throwError(() => new Error('Device1 error')));
      try {
        await service.launchFileWithContext({
          deviceId: device1,
          storageType: StorageType.Sd,
          file: file1,
          directoryPath: '/music',
          files: [file1],
        });
      } catch {
        // Expected to handle error
      }

      // Successful operation on device2
      mockPlayerService.launchFile.mockReturnValue(of(file2));
      await service.launchFileWithContext({
        deviceId: device2,
        storageType: StorageType.Sd,
        file: file2,
        directoryPath: '/music',
        files: [file2],
      });

      await nextTick();

      expect(service.getError(device1)()).toBeTruthy();
      expect(service.getError(device2)()).toBeNull();
    });
  });

  describe('Error Recovery & State Consistency', () => {
    const deviceId = 'device-error-recovery';
    const testFile = createTestFileItem();

    beforeEach(() => {
      service.initializePlayer(deviceId);
    });

    it('should clear previous errors on successful operations', async () => {
      // First operation fails
      mockPlayerService.launchFile.mockReturnValue(throwError(() => new Error('First error')));
      try {
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: testFile,
          directoryPath: '/music',
          files: [testFile],
        });
      } catch {
        // Expected to handle error
      }
      await nextTick();

      expect(service.getError(deviceId)()).toBeTruthy();

      // Second operation succeeds
      mockPlayerService.launchFile.mockReturnValue(of(testFile));
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: testFile,
        directoryPath: '/music',
        files: [testFile],
      });
      await nextTick();

      expect(service.getError(deviceId)()).toBeNull();
      expect(service.getCurrentFile(deviceId)()).toBeTruthy();
    });

    it('should maintain loading state consistency during operations', async () => {
      // Create a delayed observable to simulate async loading
      let triggerResolve: () => void;
      const loadingPromise = new Promise<void>((resolve) => {
        triggerResolve = resolve;
      });
      
      const delayedObservable = new Observable<FileItem>((subscriber) => {
        loadingPromise.then(() => {
          subscriber.next(testFile);
          subscriber.complete();
        });
      });

      mockPlayerService.launchFile.mockReturnValue(delayedObservable);

      // Start operation
      const operationPromise = service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: testFile,
        directoryPath: '/music',
        files: [testFile],
      });

      await nextTick();
      expect(service.isLoading(deviceId)()).toBe(true);

      // Complete operation
      triggerResolve();
      await operationPromise;
      await nextTick();

      expect(service.isLoading(deviceId)()).toBe(false);
    });

    it('should handle rapid successive operations correctly', async () => {
      const file1 = createTestFileItem({ name: 'first.sid' });
      const file2 = createTestFileItem({ name: 'second.sid' });

      // First operation
      mockPlayerService.launchFile.mockReturnValue(of(file1));
      const promise1 = service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: file1,
        directoryPath: '/music',
        files: [file1],
      });

      // Second operation immediately after
      mockPlayerService.launchFile.mockReturnValue(of(file2));
      const promise2 = service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: file2,
        directoryPath: '/music',
        files: [file2],
      });

      await Promise.all([promise1, promise2]);
      await nextTick();

      // Latest operation should win
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file).toEqual(file2);
      expect(service.isLoading(deviceId)()).toBe(false);
      expect(service.getError(deviceId)()).toBeNull();
    });
  });

  describe('State Transitions & Media Player Behaviors', () => {
    const testDeviceId = 'device-state-transitions';

    beforeEach(() => {
      service.initializePlayer(testDeviceId);
      
      // Ensure all mocks are properly set up for state transition tests
      mockPlayerService.launchFile = vi.fn().mockReturnValue(of(createTestFileItem()));
      mockPlayerService.toggleMusic = vi.fn().mockReturnValue(of(undefined));
      mockDeviceService.resetDevice = vi.fn().mockReturnValue(of(undefined));
    });

    describe('Initial State Transitions', () => {
      it('should transition from Stopped to Playing when launching a music file', async () => {
        // Initial state should be Stopped
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Stopped);

        // Launch a music file
        await service.launchFileWithContext({
          deviceId: testDeviceId,
          storageType: StorageType.Sd,
          file: createTestFileItem({ type: FileItemType.Song }),
          directoryPath: '/music',
          files: [createTestFileItem()],
        });

        // Should transition to Playing
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);
      });

      it('should transition to Playing state when launching any file type', async () => {
        // Launch a game file
        await service.launchFileWithContext({
          deviceId: testDeviceId,
          storageType: StorageType.Sd,
          file: createTestFileItem({ type: FileItemType.Game }),
          directoryPath: '/games',
          files: [createTestFileItem({ type: FileItemType.Game })],
        });

        // All file launches result in Playing state (device state, not content-specific)
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);
      });
    });

    describe('Play/Pause State Transitions', () => {
      beforeEach(async () => {
        // Start with a playing music file
        await service.launchFileWithContext({
          deviceId: testDeviceId,
          storageType: StorageType.Sd,
          file: createTestFileItem({ type: FileItemType.Song }),
          directoryPath: '/music',
          files: [createTestFileItem()],
        });
      });

      it('should transition Playing → Paused → Playing when toggling playback', async () => {
        // Start in Playing state
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);

        // First transition: Playing → Paused
        await service.pause(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Paused);

        // Second transition: Paused → Playing
        await service.play(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);
      });

      it('should transition Stopped → Playing when play called on stopped state', async () => {
        // Stop the player first
        await service.stop(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Stopped);

        // Play should resume playback
        await service.play(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);
      });
    });

    describe('Stop State Transitions', () => {
      beforeEach(async () => {
        // Start with a playing music file
        await service.launchFileWithContext({
          deviceId: testDeviceId,
          storageType: StorageType.Sd,
          file: createTestFileItem({ type: FileItemType.Song }),
          directoryPath: '/music',
          files: [createTestFileItem()],
        });
      });

      it('should transition Playing → Stopped when stop is called', async () => {
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);

        await service.stop(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Stopped);
      });

      it('should remain Stopped when stop is called on already stopped player', async () => {
        await service.stop(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Stopped);

        // Calling stop again should keep it stopped
        await service.stop(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Stopped);
      });
    });

    describe('Navigation State Transitions', () => {
      beforeEach(async () => {
        // Set up directory with multiple files
        const files = createTestDirectoryFiles();
        await service.launchFileWithContext({
          deviceId: testDeviceId,
          storageType: StorageType.Sd,
          file: files[0], // Start with first song
          directoryPath: '/music',
          files,
        });
      });

      it('should maintain Playing state when navigating next from Playing', async () => {
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);

        await service.next(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);
      });

      it('should maintain Playing state when navigating previous from Playing', async () => {
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);

        await service.previous(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);
      });

      it('should transition Stopped → Playing when navigating from stopped state', async () => {
        // Stop first
        await service.stop(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Stopped);

        // Navigate next should resume playback
        await service.next(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);
      });
    });

    describe('Complex State Transition Scenarios', () => {
      it('should handle state transitions across multiple operations', async () => {
        const musicFile = createTestFileItem({ type: FileItemType.Song });
        const files = [musicFile, createTestFileItem({ name: 'song2.sid' })];

        // 1. Launch → Playing
        await service.launchFileWithContext({
          deviceId: testDeviceId,
          storageType: StorageType.Sd,
          file: musicFile,
          directoryPath: '/music',
          files,
        });
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);

        // 2. Pause → Paused
        await service.pause(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Paused);

        // 3. Resume → Playing
        await service.play(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);

        // 4. Stop → Stopped
        await service.stop(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Stopped);

        // 5. Navigate → Playing (navigation from stopped resumes)
        await service.previous(testDeviceId);
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);
      });

      it('should maintain Playing state when switching between any file types', async () => {
        const musicFile = createTestFileItem({ type: FileItemType.Song });
        const gameFile = createTestFileItem({ type: FileItemType.Game, name: 'game.prg' });

        // Start with music file (Playing)
        await service.launchFileWithContext({
          deviceId: testDeviceId,
          storageType: StorageType.Sd,
          file: musicFile,
          directoryPath: '/music',
          files: [musicFile],
        });
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);

        // Launch game file (all file launches result in Playing state)
        await service.launchFileWithContext({
          deviceId: testDeviceId,
          storageType: StorageType.Sd,
          file: gameFile,
          directoryPath: '/games',
          files: [gameFile],
        });
        // Player state represents device state, not content-specific playback
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);
      });
    });

    describe('Error State Transitions', () => {
      it('should handle state gracefully when playback operations fail', async () => {
        // Start with playing music
        await service.launchFileWithContext({
          deviceId: testDeviceId,
          storageType: StorageType.Sd,
          file: createTestFileItem({ type: FileItemType.Song }),
          directoryPath: '/music',
          files: [createTestFileItem()],
        });
        expect(service.getPlayerStatus(testDeviceId)()).toBe(PlayerStatus.Playing);

        // Simulate API failure for pause
        mockPlayerService.toggleMusic = vi.fn().mockReturnValue(throwError(() => new Error('API Error')));

        await service.pause(testDeviceId);
        
        // Should handle error gracefully and maintain consistent state
        const status = service.getPlayerStatus(testDeviceId)();
        expect([PlayerStatus.Playing, PlayerStatus.Paused, PlayerStatus.Stopped]).toContain(status);
        expect(service.getError(testDeviceId)()).toBeTruthy();
      });
    });
  });
});