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
  IPlayerService,
  IDeviceService,
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
  isCompatible: true,
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

type NavigateToDirectoryFn = (params: { deviceId: string; storageType: StorageType; path: string }) => Promise<void>;
type GetSelectedDirectoryStateFn = (deviceId: string) => () => Partial<StorageDirectoryState> | null;

describe('PlayerContextService', () => {
  let service: PlayerContextService;
  let mockStorageStore: {
    navigateToDirectory: MockedFunction<NavigateToDirectoryFn>;
    getSelectedDirectoryState: MockedFunction<GetSelectedDirectoryStateFn>;
  };
  let mockPlayerService: IPlayerService;
  let mockDeviceService: IDeviceService;

  // Helper to wait for async operations
  const nextTick = () => new Promise<void>((r) => setTimeout(r, 0));
  
  // Helper to wait for timer state to be available
  const waitForTimerState = async (deviceId: string, maxAttempts = 50): Promise<TimerState | null> => {
    for (let i = 0; i < maxAttempts; i++) {
      await nextTick();
      const state = service.getTimerState(deviceId)();
      if (state !== null) {
        return state;
      }
    }
    return null;
  };

  beforeEach(() => {
    // Create mocks implementing the actual service contracts
    mockPlayerService = {
      launchFile: vi.fn(),
      launchRandom: vi.fn(),
      toggleMusic: vi.fn(),
    };

    mockDeviceService = {
      findDevices: vi.fn(),
      getConnectedDevices: vi.fn(),
      connectDevice: vi.fn(),
      disconnectDevice: vi.fn(),
      resetDevice: vi.fn(),
      pingDevice: vi.fn(),
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
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: testFile,
        directoryPath: '/music',
        files: testFiles,
      });

      await nextTick();

      // Verify error state
      expect(service.getError(deviceId)()).toBeTruthy();
      expect(service.isLoading(deviceId)()).toBe(false);
      
      // Task 10: currentFile should be set even on error so UI can show which file failed
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile).not.toBeNull();
      expect(currentFile?.file.name).toBe('test-file.sid');
      expect(currentFile?.file.path).toBe('/music/test-file.sid');
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

      it('should load directory context when navigating next in shuffle mode', async () => {
        const randomFile = createTestFileItem({ name: 'random.sid', path: '/music/random.sid' });
        const directoryFiles = [
          createTestFileItem({ name: 'song1.sid', path: '/music/song1.sid' }),
          createTestFileItem({ name: 'random.sid', path: '/music/random.sid' }),
          createTestFileItem({ name: 'song2.sid', path: '/music/song2.sid' }),
        ];

        mockPlayerService.launchRandom.mockReturnValue(of(randomFile));
        mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
        mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => ({
          directory: { files: directoryFiles }
        }));

        await service.next(deviceId);
        await nextTick();

        // Should launch random file
        expect(mockPlayerService.launchRandom).toHaveBeenCalled();

        // Should load directory context
        expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
          deviceId,
          storageType: StorageType.Sd,
          path: '/music'
        });

        // Should have loaded file context with the directory files
        const fileContext = service.getFileContext(deviceId)();
        expect(fileContext?.files).toEqual(directoryFiles);
        expect(fileContext?.currentIndex).toBe(1); // random.sid is at index 1
      });

      it('should load directory context when navigating previous in shuffle mode', async () => {
        const randomFile = createTestFileItem({ name: 'track3.sid', path: '/sounds/track3.sid' });
        const directoryFiles = [
          createTestFileItem({ name: 'track1.sid', path: '/sounds/track1.sid' }),
          createTestFileItem({ name: 'track3.sid', path: '/sounds/track3.sid' }),
        ];

        mockPlayerService.launchRandom.mockReturnValue(of(randomFile));
        mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
        mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => ({
          directory: { files: directoryFiles }
        }));

        await service.previous(deviceId);
        await nextTick();

        // Should launch random file
        expect(mockPlayerService.launchRandom).toHaveBeenCalled();

        // Should load directory context
        expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
          deviceId,
          storageType: StorageType.Sd,
          path: '/sounds'
        });

        // Should have loaded file context with the directory files
        const fileContext = service.getFileContext(deviceId)();
        expect(fileContext?.files).toEqual(directoryFiles);
        expect(fileContext?.currentIndex).toBe(1); // track3.sid is at index 1
      });

      it('should handle directory loading failure gracefully in shuffle navigation', async () => {
        const randomFile = createTestFileItem({ name: 'random.sid', path: '/music/random.sid' });

        mockPlayerService.launchRandom.mockReturnValue(of(randomFile));
        mockStorageStore.navigateToDirectory.mockRejectedValue(new Error('Directory load failed'));

        await service.next(deviceId);
        await nextTick();

        // Should still launch random file
        expect(mockPlayerService.launchRandom).toHaveBeenCalled();
        
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file).toEqual(randomFile);

        // Should not have any error in player state (directory loading failure is silently ignored)
        expect(service.getError(deviceId)()).toBeNull();
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

  describe('Phase 4: Filter System Integration', () => {
    const deviceId = 'device-filter-test';
    const randomFile = createTestFileItem({ name: 'random.sid', path: '/music/random.sid' });

    beforeEach(() => {
      service.initializePlayer(deviceId);
    });

    describe('Filter Pass-Through to API', () => {
      it('should pass current filter to API when launching random file', async () => {
        // Set filter to Games
        service.setFilterMode(deviceId, PlayerFilterType.Games);

        mockPlayerService.launchRandom.mockReturnValue(of(randomFile));
        await service.launchRandomFile(deviceId);

        // Verify API was called with Games filter
        expect(mockPlayerService.launchRandom).toHaveBeenCalledWith(
          deviceId,
          PlayerScope.Storage,
          PlayerFilterType.Games,
          undefined
        );
      });

      it('should pass current filter when navigating next in shuffle mode', async () => {
        // Setup shuffle mode and set filter
        service.toggleShuffleMode(deviceId);
        service.setFilterMode(deviceId, PlayerFilterType.Music);

        mockPlayerService.launchRandom.mockReturnValue(of(randomFile));
        await service.next(deviceId);

        // Verify API was called with Music filter
        expect(mockPlayerService.launchRandom).toHaveBeenCalledWith(
          deviceId,
          PlayerScope.Storage,
          PlayerFilterType.Music,
          undefined
        );
      });

      it('should pass current filter when navigating previous in shuffle mode', async () => {
        // Setup shuffle mode and set filter
        service.toggleShuffleMode(deviceId);
        service.setFilterMode(deviceId, PlayerFilterType.Images);

        mockPlayerService.launchRandom.mockReturnValue(of(randomFile));
        await service.previous(deviceId);

        // Verify API was called with Images filter
        expect(mockPlayerService.launchRandom).toHaveBeenCalledWith(
          deviceId,
          PlayerScope.Storage,
          PlayerFilterType.Images,
          undefined
        );
      });

      it('should update filter and affect subsequent random launches', async () => {
        mockPlayerService.launchRandom.mockReturnValue(of(randomFile));

        // First launch with All filter (default)
        await service.launchRandomFile(deviceId);
        expect(mockPlayerService.launchRandom).toHaveBeenCalledWith(
          deviceId,
          PlayerScope.Storage,
          PlayerFilterType.All,
          undefined
        );

        // Change filter to Games
        service.setFilterMode(deviceId, PlayerFilterType.Games);

        // Next launch should use Games filter
        await service.launchRandomFile(deviceId);
        expect(mockPlayerService.launchRandom).toHaveBeenCalledWith(
          deviceId,
          PlayerScope.Storage,
          PlayerFilterType.Games,
          undefined
        );
      });
    });

    describe('Filter State Persistence', () => {
      it('should persist filter when switching from Shuffle to Directory mode', () => {
        // Set filter in Directory mode
        service.setFilterMode(deviceId, PlayerFilterType.Music);

        // Switch to Shuffle mode
        service.toggleShuffleMode(deviceId);

        // Filter should persist
        const settings = service.getShuffleSettings(deviceId)();
        expect(settings?.filter).toBe(PlayerFilterType.Music);
      });

      it('should persist filter when switching from Directory to Shuffle and back', () => {
        // Set filter
        service.setFilterMode(deviceId, PlayerFilterType.Games);
        const originalFilter = service.getShuffleSettings(deviceId)()?.filter;

        // Switch to Shuffle mode
        service.toggleShuffleMode(deviceId);
        expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Shuffle);

        // Switch back to Directory mode
        service.toggleShuffleMode(deviceId);
        expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Directory);

        // Filter should still be the same
        const finalFilter = service.getShuffleSettings(deviceId)()?.filter;
        expect(finalFilter).toBe(originalFilter);
        expect(finalFilter).toBe(PlayerFilterType.Games);
      });

      it('should maintain independent filter settings per device', () => {
        const device2 = 'device-filter-2';
        service.initializePlayer(device2);

        // Set different filters for each device
        service.setFilterMode(deviceId, PlayerFilterType.Music);
        service.setFilterMode(device2, PlayerFilterType.Images);

        // Each device should maintain its own filter
        const settings1 = service.getShuffleSettings(deviceId)();
        const settings2 = service.getShuffleSettings(device2)();

        expect(settings1?.filter).toBe(PlayerFilterType.Music);
        expect(settings2?.filter).toBe(PlayerFilterType.Images);
      });
    });
  });

  describe('Phase 5: Timer System Integration', () => {
    const deviceId = 'device-timer-test';
    const waitForTime = (ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms));

    beforeEach(() => {
      service.initializePlayer(deviceId);
    });

    describe('Timer Creation & Lifecycle', () => {
      it('should create timer when music file launches with valid playLength', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:45',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200); // Wait for timer setup and store update

        const timerState = service.getTimerState(deviceId)();
        expect(timerState).not.toBeNull();
        expect(timerState?.totalTime).toBe(225000);
      });

      it('should NOT create timer when non-music file launches', async () => {
        const imageFile = createTestFileItem({
          type: FileItemType.Photo,
          playLength: undefined,
        });

        mockPlayerService.launchFile.mockReturnValue(of(imageFile));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: imageFile,
          files: [imageFile],
        });

        await nextTick();
        await waitForTime(200);

        const timerState = service.getTimerState(deviceId)();
        expect(timerState).toBeNull();
      });

      it('should create default 3-minute timer when music file has invalid playLength', async () => {
        const warnSpy = vi.spyOn(console, 'warn');
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: 'invalid',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Should create timer with default 3-minute duration
        const timerState = service.getTimerState(deviceId)();
        expect(timerState).not.toBeNull();
        expect(timerState?.totalTime).toBe(180000); // 3 minutes default
        
        // Should log warning about invalid playLength
        expect(warnSpy).toHaveBeenCalledWith(
          expect.stringContaining('invalid playLength format')
        );
        
        warnSpy.mockRestore();
      });

      it('should create default 3-minute timer when music file has empty playLength', async () => {
        const warnSpy = vi.spyOn(console, 'warn');
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Should create timer with default 3-minute duration
        const timerState = service.getTimerState(deviceId)();
        expect(timerState).not.toBeNull();
        expect(timerState?.totalTime).toBe(180000); // 3 minutes default
        
        // Should log warning about empty playLength
        expect(warnSpy).toHaveBeenCalledWith(
          expect.stringContaining('empty playLength')
        );
        
        warnSpy.mockRestore();
      });

      it('should increment timer currentTime over time', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:45',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        const initialState = service.getTimerState(deviceId)();
        expect(initialState?.currentTime).toBeGreaterThanOrEqual(0);
        expect(initialState?.currentTime).toBeLessThanOrEqual(300); // Allow up to 300ms

        // Wait for timer to increment (1 second)
        await waitForTime(1000);

        const updatedState = service.getTimerState(deviceId)();
        expect(updatedState?.currentTime).toBeGreaterThan(initialState?.currentTime || 0);
        expect(updatedState?.currentTime).toBeLessThanOrEqual(1500);
      });

      it('should parse H:MM:SS playLength format correctly', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '1:23:45', // 1 hour, 23 minutes, 45 seconds
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        const timerState = service.getTimerState(deviceId)();
        expect(timerState).not.toBeNull();
        expect(timerState?.totalTime).toBe(5025000); // 1h 23m 45s in ms
      });
    });

    describe('Playback Control Integration', () => {
      it('should pause timer when pause() called on music file', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:45',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Pause playback
        await service.pause(deviceId);
        await nextTick();
        await waitForTime(100); // Give timer time to process pause

        const timerState = service.getTimerState(deviceId)();
        expect(timerState?.isRunning).toBe(false);
      });

      it('should resume timer when play() called after pause', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:45',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Pause then resume
        await service.pause(deviceId);
        await nextTick();
        await service.play(deviceId);
        await nextTick();

        const timerState = service.getTimerState(deviceId)();
        expect(timerState?.isRunning).toBe(true);
      });

      it('should stop timer and reset to 0 when stop() called', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:45',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        mockDeviceService.resetDevice.mockReturnValue(of(undefined));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Wait a bit for timer to increment
        await waitForTime(500);

        // Stop playback
        await service.stop(deviceId);
        await nextTick();
        await waitForTime(200);

        const timerState = service.getTimerState(deviceId)();
        expect(timerState?.currentTime).toBe(0);
        expect(timerState?.isRunning).toBe(false);
      });

      it('should NOT increment timer currentTime when paused', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:45',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Pause playback
        await service.pause(deviceId);
        await nextTick();

        const pausedState = service.getTimerState(deviceId)();
        const pausedTime = pausedState?.currentTime ?? 0;

        // Wait a bit
        await waitForTime(1000);

        const laterState = service.getTimerState(deviceId)();
        expect(laterState?.currentTime).toBe(pausedTime);
      });

      it('should NOT affect timer when pause/play/stop called on non-music file', async () => {
        const imageFile = createTestFileItem({
          type: FileItemType.Photo,
          playLength: undefined,
        });

        mockPlayerService.launchFile.mockReturnValue(of(imageFile));
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));
        mockDeviceService.resetDevice.mockReturnValue(of(undefined));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: imageFile,
          files: [imageFile],
        });

        await nextTick();
        await waitForTime(200);

        // Try all playback controls
        await service.pause(deviceId);
        await service.play(deviceId);
        await service.stop(deviceId);
        await nextTick();

        const timerState = service.getTimerState(deviceId)();
        expect(timerState).toBeNull();
      });
    });

    describe('Navigation Timer Tests', () => {
      it('should create new timer when navigating to next music file', async () => {
        const musicFile1 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
          path: 'music1.sid',
        });
        const musicFile2 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '4:30',
          path: 'music2.sid',
        });

        mockPlayerService.launchFile
          .mockReturnValueOnce(of(musicFile1))
          .mockReturnValueOnce(of(musicFile2));
        // Note: next() internally uses launchFile, no need to mock next itself

        // Launch first file
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile1,
          files: [musicFile1, musicFile2],
        });

        await nextTick();
        await waitForTime(200);

        const firstTimer = service.getTimerState(deviceId)();
        expect(firstTimer?.totalTime).toBe(180000); // 3:00

        // Navigate to next
        await service.next(deviceId);
        await nextTick();
        await waitForTime(200);

        const secondTimer = service.getTimerState(deviceId)();
        expect(secondTimer?.totalTime).toBe(270000); // 4:30
        // Timer resets but may have already incremented slightly
        expect(secondTimer?.currentTime).toBeLessThan(300);
      });

      it('should clear timer when navigating from music to non-music file', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });
        const imageFile = createTestFileItem({
          type: FileItemType.Photo,
          playLength: undefined,
        });

        mockPlayerService.launchFile
          .mockReturnValueOnce(of(musicFile))
          .mockReturnValueOnce(of(imageFile));

        // Launch music file
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile, imageFile],
        });

        await nextTick();
        await waitForTime(200);

        expect(service.getTimerState(deviceId)()).not.toBeNull();

        // Navigate to image - timer should be destroyed when switching to non-music file
        await service.next(deviceId);
        await nextTick();
        await waitForTime(200);

        // Timer state should be null (destroyed when navigating to non-music file)
        const timerState = service.getTimerState(deviceId)();
        expect(timerState).toBeNull();
      });

      it('should create timer when navigating from non-music to music file', async () => {
        const imageFile = createTestFileItem({
          type: FileItemType.Photo,
          playLength: undefined,
        });
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });

        mockPlayerService.launchFile
          .mockReturnValueOnce(of(imageFile))
          .mockReturnValueOnce(of(musicFile));
        // Note: next() internally uses launchFile, no need to mock next itself

        // Launch image file
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: imageFile,
          files: [imageFile, musicFile],
        });

        await nextTick();
        await waitForTime(200);

        expect(service.getTimerState(deviceId)()).toBeNull();

        // Navigate to music
        await service.next(deviceId);
        await nextTick();
        await waitForTime(200);

        const timerState = service.getTimerState(deviceId)();
        expect(timerState).not.toBeNull();
        expect(timerState?.totalTime).toBe(180000);
      });
    });

    describe('Auto-Progression Tests', () => {
      it('should auto-progress to next file when timer completes in Directory mode', async () => {
        const musicFile1 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '0:01', // 1 second for fast test
          path: 'music1.sid',
          name: 'music1.sid',
        });
        const musicFile2 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '0:01',
          path: 'music2.sid',
          name: 'music2.sid',
        });

        mockPlayerService.launchFile
          .mockReturnValueOnce(of(musicFile1))  // Initial launch
          .mockReturnValueOnce(of(musicFile2)); // Auto-progression

        mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => ({
          files: [musicFile1, musicFile2],
          currentPath: '/music',
        }));

        // Launch first file
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile1,
          files: [musicFile1, musicFile2],
          launchMode: LaunchMode.Directory,
        });

        await nextTick();

        // Verify first file is playing
        expect(service.getCurrentFile(deviceId)()?.file.name).toBe('music1.sid');

        // Wait for timer to complete (1 second + buffer)
        await waitForTime(1200);

        // Verify auto-progression called next()
        expect(mockPlayerService.launchFile).toHaveBeenCalledTimes(2);
        expect(service.getCurrentFile(deviceId)()?.file.name).toBe('music2.sid');
      });

      it('should auto-progress to random file when timer completes in Shuffle mode', async () => {
        const musicFile1 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '0:01',
          path: 'music1.sid',
          name: 'music1.sid',
        });
        const randomMusicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '0:01',
          path: 'random.sid',
          name: 'random.sid',
        });

        mockPlayerService.launchFile.mockReturnValueOnce(of(musicFile1));
        mockPlayerService.launchRandom.mockReturnValueOnce(of(randomMusicFile));

        // Launch first file in shuffle mode
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile1,
          files: [musicFile1],
          launchMode: LaunchMode.Shuffle,
        });

        await nextTick();

        // Verify first file is playing
        expect(service.getCurrentFile(deviceId)()?.file.name).toBe('music1.sid');

        // Wait for timer to complete
        await waitForTime(1200);

        // Verify auto-progression called launchRandom
        expect(mockPlayerService.launchRandom).toHaveBeenCalledTimes(1);
        expect(service.getCurrentFile(deviceId)()?.file.name).toBe('random.sid');
      });

      it('should create new timer after auto-progression', async () => {
        const musicFile1 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '0:01',
          path: 'music1.sid',
        });
        const musicFile2 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '0:02', // Different duration
          path: 'music2.sid',
        });

        mockPlayerService.launchFile
          .mockReturnValueOnce(of(musicFile1))
          .mockReturnValueOnce(of(musicFile2));

        mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => ({
          files: [musicFile1, musicFile2],
          currentPath: '/music',
        }));

        // Launch first file
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile1,
          files: [musicFile1, musicFile2],
        });

        await nextTick();
        await waitForTime(200);

        const firstTimer = service.getTimerState(deviceId)();
        expect(firstTimer?.totalTime).toBe(1000); // 1 second

        // Wait for completion and auto-progression
        await waitForTime(1200);

        const secondTimer = service.getTimerState(deviceId)();
        expect(secondTimer?.totalTime).toBe(2000); // 2 seconds
        expect(secondTimer?.currentTime).toBeLessThan(500); // Reset and started fresh
      });

      it('should handle multiple auto-progression cycles', async () => {
        const musicFile1 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '0:01',
          path: 'music1.sid',
          name: 'music1.sid',
        });
        const musicFile2 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '0:01',
          path: 'music2.sid',
          name: 'music2.sid',
        });
        const musicFile3 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '0:01',
          path: 'music3.sid',
          name: 'music3.sid',
        });

        mockPlayerService.launchFile
          .mockReturnValueOnce(of(musicFile1))
          .mockReturnValueOnce(of(musicFile2))
          .mockReturnValueOnce(of(musicFile3));

        mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => ({
          files: [musicFile1, musicFile2, musicFile3],
          currentPath: '/music',
        }));

        // Launch first file
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile1,
          files: [musicFile1, musicFile2, musicFile3],
        });

        await nextTick();
        expect(service.getCurrentFile(deviceId)()?.file.name).toBe('music1.sid');

        // Wait for first completion
        await waitForTime(1200);
        expect(service.getCurrentFile(deviceId)()?.file.name).toBe('music2.sid');

        // Wait for second completion
        await waitForTime(1200);
        expect(service.getCurrentFile(deviceId)()?.file.name).toBe('music3.sid');

        // Verify 3 launches occurred
        expect(mockPlayerService.launchFile).toHaveBeenCalledTimes(3);
      });

      it('should not auto-progress when paused', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '0:01',
          path: 'music1.sid',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Pause immediately
        await service.pause(deviceId);
        await nextTick();

        // Wait past what would be completion time
        await waitForTime(1500);

        // Should only have launched once (no auto-progression while paused)
        expect(mockPlayerService.launchFile).toHaveBeenCalledTimes(1);
      });
    });

    describe('Timer Error Handling Tests', () => {
      it('should NOT create timer when launchFileWithContext fails', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });

        // Mock service to throw error
        mockPlayerService.launchFile.mockReturnValue(
          throwError(() => new Error('Launch failed'))
        );

        // Attempt to launch file (will set error state, not throw)
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();

        // Timer state should be null (no timer created)
        const timerState = service.getTimerState(deviceId)();
        expect(timerState).toBeNull();
      });

      it('should NOT create timer when launchRandomFile fails', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });

        mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => ({
          path: '/music',
          directory: {
            path: '/music',
            name: 'music',
            files: [musicFile],
            directories: [],
          },
          isLoading: false,
          error: null,
          lastUpdated: Date.now(),
        }));

        // Mock service to throw error
        mockPlayerService.launchRandom.mockReturnValue(
          throwError(() => new Error('Random launch failed'))
        );

        // Attempt to launch random file (will fail but won't throw - handles internally)
        await service.launchRandomFile(deviceId);
        
        await nextTick();

        // Timer state should be null (no timer created)
        const timerState = service.getTimerState(deviceId)();
        expect(timerState).toBeNull();
        
        // Player should be in error state
        const error = service.getError(deviceId)();
        expect(error).toBeTruthy();
      });

      it('should NOT create timer when next() fails', async () => {
        const musicFile1 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
          path: 'music1.sid',
        });
        const musicFile2 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '2:00',
          path: 'music2.sid',
        });

        // First launch succeeds
        mockPlayerService.launchFile
          .mockReturnValueOnce(of(musicFile1))
          .mockReturnValueOnce(
            throwError(() => new Error('Next launch failed'))
          );

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile1,
          files: [musicFile1, musicFile2],
        });

        // Wait for timer to be created (observable emissions are async)
        const firstTimerState = await waitForTimerState(deviceId);
        expect(firstTimerState).not.toBeNull();
        const firstTimerTotalTime = firstTimerState?.totalTime;

        // Attempt next (will fail but won't throw - handles internally)
        await service.next(deviceId);
        
        await nextTick();

        // Timer state should still be from first file (not updated for failed file)
        const timerState = service.getTimerState(deviceId)();
        if (timerState) {
          expect(timerState.totalTime).toBe(firstTimerTotalTime);
        }
        
        // Player should be in error state
        const error = service.getError(deviceId)();
        expect(error).toBeTruthy();
      });

      it('should NOT create timer when previous() fails', async () => {
        const musicFile1 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
          path: 'music1.sid',
        });
        const musicFile2 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '2:00',
          path: 'music2.sid',
        });

        // First launch succeeds
        mockPlayerService.launchFile
          .mockReturnValueOnce(of(musicFile1))
          .mockReturnValueOnce(
            throwError(() => new Error('Previous launch failed'))
          );

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile1,
          files: [musicFile2, musicFile1],
        });

        // Wait for timer to be created (observable emissions are async)
        const firstTimerState = await waitForTimerState(deviceId);
        expect(firstTimerState).not.toBeNull();
        const firstTimerTotalTime = firstTimerState?.totalTime;

        // Attempt previous (will fail but won't throw - handles internally)
        await service.previous(deviceId);
        
        await nextTick();

        // Timer state should still be from first file (not updated for failed file)
        const timerState = service.getTimerState(deviceId)();
        if (timerState) {
          expect(timerState.totalTime).toBe(firstTimerTotalTime);
        }
        
        // Player should be in error state
        const error = service.getError(deviceId)();
        expect(error).toBeTruthy();
      });

      // Task 10: Failed Launch Error Handling & Recovery Tests
      it('should set currentFile when launch fails', async () => {
        const musicFile = createTestFileItem({
          name: 'incompatible.sid',
          path: '/music/incompatible.sid',
          type: FileItemType.Song,
          playLength: '3:00',
        });
        const directoryFiles = [musicFile];

        // Mock service to throw error
        mockPlayerService.launchFile.mockReturnValue(
          throwError(() => new Error('Incompatible SID file'))
        );

        // Attempt to launch file (will set error state, not throw)
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: musicFile,
          directoryPath: '/music',
          files: directoryFiles,
        });

        await nextTick();

        // currentFile should be set even though launch failed
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile).not.toBeNull();
        expect(currentFile?.file.name).toBe('incompatible.sid');
        expect(currentFile?.file.path).toBe('/music/incompatible.sid');

        // Player should be in error state
        const error = service.getError(deviceId)();
        expect(error).toContain('Incompatible SID file');
      });

      it('should set fileContext with directory on failed launch', async () => {
        const musicFile = createTestFileItem({
          name: 'bad.sid',
          path: '/music/bad.sid',
          type: FileItemType.Song,
        });
        const directoryFiles = [
          createTestFileItem({ name: 'good.sid', path: '/music/good.sid' }),
          musicFile,
          createTestFileItem({ name: 'another.sid', path: '/music/another.sid' }),
        ];

        // Mock service to throw error
        mockPlayerService.launchFile.mockReturnValue(
          throwError(() => new Error('Failed to launch'))
        );

        // Attempt to launch file (will set error state, not throw)
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          directoryPath: '/music',
          files: directoryFiles,
        });

        await nextTick();

        // fileContext should be set with full directory
        const fileContext = service.getFileContext(deviceId)();
        expect(fileContext).not.toBeNull();
        expect(fileContext?.files).toHaveLength(3);
        expect(fileContext?.currentIndex).toBe(1);
        expect(fileContext?.directoryPath).toBe('/music');
      });

      it('should cleanup timer when launch fails', async () => {
        const musicFile1 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });
        const musicFile2 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '2:00',
        });

        // First launch succeeds
        mockPlayerService.launchFile
          .mockReturnValueOnce(of(musicFile1))
          .mockReturnValueOnce(
            throwError(() => new Error('Second launch failed'))
          );

        // First successful launch
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: musicFile1,
          files: [musicFile1],
        });

        // Wait for timer to be created
        const firstTimerState = await waitForTimerState(deviceId);
        expect(firstTimerState).not.toBeNull();

        // Second launch fails (will set error state, not throw)
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: musicFile2,
          files: [musicFile2],
        });

        // Timer should be cleaned up (set to null)
        await nextTick();
        const timerState = service.getTimerState(deviceId)();
        expect(timerState).toBeNull();
      });

      it('should allow recovery from failed launch', async () => {
        const badFile = createTestFileItem({
          name: 'bad.sid',
          type: FileItemType.Song,
          playLength: '3:00',
        });
        const goodFile = createTestFileItem({
          name: 'good.sid',
          type: FileItemType.Song,
          playLength: '2:30',
        });

        // First launch fails
        mockPlayerService.launchFile
          .mockReturnValueOnce(throwError(() => new Error('Bad file')))
          .mockReturnValueOnce(of(goodFile));

        // First launch fails (will set error state, not throw)
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: badFile,
          files: [badFile],
        });

        await nextTick();

        // Should be in error state
        expect(service.getError(deviceId)()).toBeTruthy();

        // Second launch succeeds
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: goodFile,
          files: [goodFile],
        });

        // Should recover to success state
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file.name).toBe('good.sid');
        expect(service.getError(deviceId)()).toBeNull();

        // Timer should be created for good file
        const timerState = await waitForTimerState(deviceId);
        expect(timerState).not.toBeNull();
        expect(timerState?.totalTime).toBe(150000); // 2:30 = 150 seconds = 150000 milliseconds
      });

      it('should preserve directory context for shuffle after failed launch', async () => {
        const badFile = createTestFileItem({
          name: 'incompatible.sid',
          path: '/music/incompatible.sid',
          type: FileItemType.Song,
        });
        const goodFile = createTestFileItem({
          name: 'good.sid',
          path: '/music/good.sid',
          type: FileItemType.Song,
        });
        const directoryFiles = [badFile, goodFile];

        // Mock first launch to fail
        mockPlayerService.launchFile
          .mockReturnValueOnce(throwError(() => new Error('Incompatible format')))
          .mockReturnValueOnce(of(goodFile));

        // First launch fails (will set error state, not throw)
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: badFile,
          directoryPath: '/music',
          files: directoryFiles,
          launchMode: LaunchMode.Shuffle,
        });

        await nextTick();

        // Directory context should still be set for shuffle mode
        const failedFileContext = service.getFileContext(deviceId)();
        expect(failedFileContext).not.toBeNull();
        expect(failedFileContext?.files).toHaveLength(2);
        expect(failedFileContext?.directoryPath).toBe('/music');

        // Now user can shuffle to next file
        mockPlayerService.launchRandom.mockReturnValue(of(goodFile));
        await service.launchRandomFile(deviceId);
        await nextTick();

        // Should successfully launch next file
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file.name).toBe('good.sid');
        expect(service.getError(deviceId)()).toBeNull();
      });
    });

    describe('Multi-Device Timer Tests', () => {
      it('should maintain independent timer state per device', async () => {
        const deviceId2 = 'device-2';
        const musicFile1 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });
        const musicFile2 = createTestFileItem({
          type: FileItemType.Song,
          playLength: '5:00',
        });

        mockPlayerService.launchFile
          .mockReturnValueOnce(of(musicFile1))
          .mockReturnValueOnce(of(musicFile2));

        // Launch file on device 1
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile1,
          files: [musicFile1],
        });

        // Launch different file on device 2
        await service.launchFileWithContext({
          deviceId: deviceId2,
          storageType: StorageType.Usb,
          file: musicFile2,
          files: [musicFile2],
        });

        await nextTick();
        await waitForTime(200);

        const timer1 = service.getTimerState(deviceId)();
        const timer2 = service.getTimerState(deviceId2)();

        expect(timer1?.totalTime).toBe(180000); // 3:00
        expect(timer2?.totalTime).toBe(300000); // 5:00
      });

      it('should cleanup timer on device removal without affecting other devices', async () => {
        const deviceId2 = 'device-2';
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));

        // Launch on both devices
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await service.launchFileWithContext({
          deviceId: deviceId2,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        expect(service.getTimerState(deviceId)()).not.toBeNull();
        expect(service.getTimerState(deviceId2)()).not.toBeNull();

        // Remove device 1
        service.removePlayer(deviceId);
        await nextTick();

        expect(service.getTimerState(deviceId)()).toBeNull();
        expect(service.getTimerState(deviceId2)()).not.toBeNull();
      });

      it('should support independent pause/resume per device', async () => {
        const deviceId2 = 'device-2';
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));

        // Launch on both devices
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await service.launchFileWithContext({
          deviceId: deviceId2,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Pause only device 1
        await service.pause(deviceId);
        await nextTick();
        await waitForTime(100); // Give timer time to process pause

        const timer1 = service.getTimerState(deviceId)();
        const timer2 = service.getTimerState(deviceId2)();

        expect(timer1?.isRunning).toBe(false);
        expect(timer2?.isRunning).toBe(true);
      });
    });

    describe('Edge Cases & Error Handling', () => {
      it('should handle rapid play/pause/stop cycles gracefully', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));
        mockDeviceService.resetDevice.mockReturnValue(of(undefined));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Rapid cycling
        await service.pause(deviceId);
        await service.play(deviceId);
        await service.pause(deviceId);
        await service.play(deviceId);
        await service.stop(deviceId);
        await nextTick();
        await waitForTime(200);

        const timerState = service.getTimerState(deviceId)();
        expect(timerState?.currentTime).toBe(0);
        expect(timerState?.isRunning).toBe(false);
      });

      it('should return null timer state for non-existent device', async () => {
        const nonExistentDevice = 'non-existent-device';
        const timerState = service.getTimerState(nonExistentDevice)();
        expect(timerState).toBeNull();
      });

      it('should return null timer state before any file launched', async () => {
        const timerState = service.getTimerState(deviceId)();
        expect(timerState).toBeNull();
      });

      it('should handle playback control operations on non-music files without errors', async () => {
        const imageFile = createTestFileItem({
          type: FileItemType.Photo,
          playLength: undefined,
        });

        mockPlayerService.launchFile.mockReturnValue(of(imageFile));
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));
        mockDeviceService.resetDevice.mockReturnValue(of(undefined));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: imageFile,
          files: [imageFile],
        });

        await nextTick();
        await waitForTime(200);

        // These should not throw errors
        await expect(service.pause(deviceId)).resolves.not.toThrow();
        await expect(service.play(deviceId)).resolves.not.toThrow();
        await expect(service.stop(deviceId)).resolves.not.toThrow();
      });
    });

    describe('Store Integration', () => {
      it('should update store timerState reactively when timer changes', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Timer state accessed via service is actually from the store
        const timerState = service.getTimerState(deviceId)();
        expect(timerState).not.toBeNull();
        expect(timerState?.totalTime).toBe(180000);
      });

      it('should maintain store state consistency across timer lifecycle', async () => {
        const musicFile = createTestFileItem({
          type: FileItemType.Song,
          playLength: '3:00',
        });

        mockPlayerService.launchFile.mockReturnValue(of(musicFile));
        mockPlayerService.toggleMusic.mockReturnValue(of(undefined));
        mockDeviceService.resetDevice.mockReturnValue(of(undefined));

        // Launch
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Usb,
          file: musicFile,
          files: [musicFile],
        });

        await nextTick();
        await waitForTime(200);

        // Timer state is from store - should be running
        let timerState = service.getTimerState(deviceId)();
        expect(timerState?.isRunning).toBe(true);

        // Pause
        await service.pause(deviceId);
        await nextTick();
        await waitForTime(100); // Give timer time to process pause

        timerState = service.getTimerState(deviceId)();
        expect(timerState?.isRunning).toBe(false);

        // Stop
        await service.stop(deviceId);
        await nextTick();
        await waitForTime(200);

        timerState = service.getTimerState(deviceId)();
        expect(timerState?.currentTime).toBe(0);
        expect(timerState?.isRunning).toBe(false);
      });
    });

    describe('Incompatible File Handling', () => {
      it('should handle isCompatible=false from backend on launchFile', async () => {
        const incompatibleFile = createTestFileItem({
          name: 'incompatible.sid',
          path: '/music/incompatible.sid',
          type: FileItemType.Song,
          isCompatible: false, // Backend returns false
        });

        // Mock backend returning incompatible file
        mockPlayerService.launchFile.mockReturnValue(of(incompatibleFile));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: incompatibleFile,
          files: [incompatibleFile],
        });

        await nextTick();

        // Should set currentFile with isCompatible=false
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile).not.toBeNull();
        expect(currentFile?.file.isCompatible).toBe(false);
        expect(currentFile?.isCompatible).toBe(false);

        // Should set error state
        const error = service.getError(deviceId)();
        expect(error).toContain('not compatible');

        // Should NOT create timer
        const timerState = service.getTimerState(deviceId)();
        expect(timerState).toBeNull();
      });

      it('should handle isCompatible=false from backend on launchRandom', async () => {
        const incompatibleFile = createTestFileItem({
          name: 'bad.sid',
          path: '/music/bad.sid',
          type: FileItemType.Song,
          isCompatible: false,
        });

        mockPlayerService.launchRandom.mockReturnValue(of(incompatibleFile));
        mockStorageStore.navigateToDirectory.mockResolvedValue();
        mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => ({
          path: '/music',
          directory: {
            path: '/music',
            name: 'music',
            files: [incompatibleFile],
            directories: [],
          },
          isLoading: false,
          error: null,
          lastUpdated: Date.now(),
        }));

        await service.launchRandomFile(deviceId);
        await nextTick();

        // Should load directory context even when incompatible
        expect(mockStorageStore.navigateToDirectory).toHaveBeenCalled();

        // Should set currentFile with isCompatible=false
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.isCompatible).toBe(false);

        // Should set error state
        const error = service.getError(deviceId)();
        expect(error).toContain('not compatible');

        // Should NOT create timer
        const timerState = service.getTimerState(deviceId)();
        expect(timerState).toBeNull();
      });

      it('should handle isCompatible=false on next() in directory mode', async () => {
        const compatibleFile = createTestFileItem({
          name: 'good.sid',
          path: '/music/good.sid',
          type: FileItemType.Song,
          isCompatible: true,
        });
        const incompatibleFile = createTestFileItem({
          name: 'bad.sid',
          path: '/music/bad.sid',
          type: FileItemType.Song,
          isCompatible: false,
        });

        // First file is compatible, second is not
        mockPlayerService.launchFile
          .mockReturnValueOnce(of(compatibleFile))
          .mockReturnValueOnce(of(incompatibleFile));

        // Launch first file
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: compatibleFile,
          files: [compatibleFile, incompatibleFile],
        });

        await nextTick();
        await waitForTime(200);

        // First file should have timer
        expect(service.getTimerState(deviceId)()).not.toBeNull();

        // Navigate to next (incompatible) file
        await service.next(deviceId);
        await nextTick();

        // Should set currentFile to incompatible file
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file.name).toBe('bad.sid');
        expect(currentFile?.isCompatible).toBe(false);

        // Should have error
        expect(service.getError(deviceId)()).toContain('not compatible');

        // Timer should be cleaned up
        expect(service.getTimerState(deviceId)()).toBeNull();
      });

      it('should handle isCompatible=false on previous() in directory mode', async () => {
        const compatibleFile = createTestFileItem({
          name: 'good.sid',
          path: '/music/good.sid',
          type: FileItemType.Song,
          isCompatible: true,
        });
        const incompatibleFile = createTestFileItem({
          name: 'bad.sid',
          path: '/music/bad.sid',
          type: FileItemType.Song,
          isCompatible: false,
        });

        // First file is incompatible, second is compatible
        mockPlayerService.launchFile
          .mockReturnValueOnce(of(compatibleFile))
          .mockReturnValueOnce(of(incompatibleFile));

        // Launch compatible file (index 1)
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: compatibleFile,
          files: [incompatibleFile, compatibleFile],
        });

        await nextTick();
        await waitForTime(200);

        // Navigate to previous (incompatible) file
        await service.previous(deviceId);
        await nextTick();

        // Should set currentFile to incompatible file
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file.name).toBe('bad.sid');
        expect(currentFile?.isCompatible).toBe(false);

        // Should have error
        expect(service.getError(deviceId)()).toContain('not compatible');

        // Timer should be cleaned up
        expect(service.getTimerState(deviceId)()).toBeNull();
      });

      it('should allow recovery from incompatible file by navigating to next', async () => {
        const incompatibleFile = createTestFileItem({
          name: 'bad.sid',
          path: '/music/bad.sid',
          type: FileItemType.Song,
          isCompatible: false,
        });
        const compatibleFile = createTestFileItem({
          name: 'good.sid',
          path: '/music/good.sid',
          type: FileItemType.Song,
          isCompatible: true,
        });

        mockPlayerService.launchFile
          .mockReturnValueOnce(of(incompatibleFile))
          .mockReturnValueOnce(of(compatibleFile));

        // Launch incompatible file
        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file: incompatibleFile,
          files: [incompatibleFile, compatibleFile],
        });

        await nextTick();

        // Should be in error state
        expect(service.getError(deviceId)()).toBeTruthy();

        // Navigate to next (compatible) file
        await service.next(deviceId);
        await nextTick();
        await waitForTime(200);

        // Should recover
        expect(service.getError(deviceId)()).toBeNull();
        expect(service.getCurrentFile(deviceId)()?.file.name).toBe('good.sid');

        // Timer should be created
        expect(service.getTimerState(deviceId)()).not.toBeNull();
      });
    });
  });
});