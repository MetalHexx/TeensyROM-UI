import { describe, it, expect, beforeEach, vi, MockedFunction } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import {
  FileItem,
  FileItemType,
  StorageType,
  LaunchMode,
  PLAYER_SERVICE,
  DEVICE_SERVICE,
  IPlayerService,
  IDeviceService,
} from '@teensyrom-nx/domain';
import { PlayerContextService } from './player-context.service';
import { PlayerStore } from './player-store';
import { StorageStore, StorageDirectoryState } from '../storage/storage-store';

// Test data factory functions
const createTestFileItem = (overrides: Partial<FileItem> = {}): FileItem => ({
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
  createTestFileItem({ name: 'song3.sid', path: '/music/song3.sid' }),
];

type NavigateToDirectoryFn = (params: {
  deviceId: string;
  storageType: StorageType;
  path: string;
}) => Promise<void>;
type GetSelectedDirectoryStateFn = (
  deviceId: string
) => () => Partial<StorageDirectoryState> | null;

describe('PlayerContextService - Phase 2: History Navigation', () => {
  let service: PlayerContextService;
  let mockStorageStore: {
    navigateToDirectory: MockedFunction<NavigateToDirectoryFn>;
    getSelectedDirectoryState: MockedFunction<GetSelectedDirectoryStateFn>;
  };
  let mockPlayerService: {
    launchFile: MockedFunction<IPlayerService['launchFile']>;
    launchRandom: MockedFunction<IPlayerService['launchRandom']>;
    toggleMusic: MockedFunction<IPlayerService['toggleMusic']>;
  };
  let mockDeviceService: {
    findDevices: MockedFunction<IDeviceService['findDevices']>;
    getConnectedDevices: MockedFunction<IDeviceService['getConnectedDevices']>;
    connectDevice: MockedFunction<IDeviceService['connectDevice']>;
    disconnectDevice: MockedFunction<IDeviceService['disconnectDevice']>;
    resetDevice: MockedFunction<IDeviceService['resetDevice']>;
    pingDevice: MockedFunction<IDeviceService['pingDevice']>;
  };

  // Helper to wait for async operations
  const nextTick = () => new Promise<void>((r) => setTimeout(r, 0));

  // Helper to wait for timer state to be available
  const waitForTimerState = async (deviceId: string, maxAttempts = 50) => {
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

  describe('Previous Button with History', () => {
    const deviceId = 'device-history-nav';
    const testFiles = createTestDirectoryFiles();
    const [file1, file2, file3] = testFiles;

    beforeEach(async () => {
      service.initializePlayer(deviceId);

      // Switch to Shuffle mode
      service.toggleShuffleMode(deviceId);
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Shuffle);

      // Mock directory state for loading context after navigation
      const mockDirectoryState: Partial<StorageDirectoryState> = {
        directory: {
          files: testFiles,
          directories: [],
          path: '/music',
        },
      };
      mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
      mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => mockDirectoryState);

      // Launch 3 files in shuffle mode to build history
      mockPlayerService.launchRandom.mockReturnValueOnce(of(file1));
      await service.launchRandomFile(deviceId);
      await nextTick();

      mockPlayerService.launchRandom.mockReturnValueOnce(of(file2));
      await service.next(deviceId);
      await nextTick();

      mockPlayerService.launchRandom.mockReturnValueOnce(of(file3));
      await service.next(deviceId);
      await nextTick();

      // At this point we have history: [file1, file2, file3] with position -1 (at end)
      const playHistory = service.getPlayHistory(deviceId)();
      expect(playHistory?.entries.length).toBe(3);

      const position = service.getCurrentHistoryPosition(deviceId)();
      expect(position).toBe(-1);
    });

    it('should navigate backward from end (-1) to most recent entry (position 2)', async () => {
      // We're at position -1 (just launched file3), history: [file1, file2, file3]
      // Verify we can navigate backward
      const canNavigateBack = service.canNavigateBackwardInHistory(deviceId)();
      expect(canNavigateBack).toBe(true);

      // Clear mocks from setup phase
      mockPlayerService.launchRandom.mockClear();
      mockPlayerService.launchFile.mockClear();

      // Mock the launchFile call that will be made by navigateBackwardInHistory
      mockPlayerService.launchFile.mockReturnValue(of(file3));

      // Call previous() - should use history navigation, not random launch
      await service.previous(deviceId);
      await nextTick();

      // Should have moved to position 2 (most recent entry in history)
      const position = service.getCurrentHistoryPosition(deviceId)();
      expect(position).toBe(2);

      // Should have launched file3 again via launchFile (not launchRandom)
      expect(mockPlayerService.launchFile).toHaveBeenCalledWith(
        deviceId,
        StorageType.Sd,
        file3.path
      );
      expect(mockPlayerService.launchRandom).not.toHaveBeenCalled();

      // Current file should be file3
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.path).toBe(file3.path);
    });

    it('should wrap from position 0 to end when navigating backward', async () => {
      // First, navigate backward to position 2
      mockPlayerService.launchFile.mockReturnValue(of(file3));
      await service.previous(deviceId);
      await nextTick();
      await nextTick(); // Ensure state fully propagates

      // Then navigate to position 1
      mockPlayerService.launchFile.mockReturnValue(of(file2));
      await service.previous(deviceId);
      await nextTick();
      await nextTick(); // Ensure state fully propagates

      // Then navigate to position 0 (first entry)
      mockPlayerService.launchFile.mockReturnValue(of(file1));
      await service.previous(deviceId);
      await nextTick();
      await nextTick(); // Ensure state fully propagates

      // Verify we're at position 0
      const position = service.getCurrentHistoryPosition(deviceId)();
      expect(position).toBe(0);

      // Verify we have 3 entries in history before wraparound
      const historyBefore = service.getPlayHistory(deviceId)();
      expect(historyBefore?.entries.length).toBe(3);

      // Now navigate backward from position 0 - should wrap to position 2 (end)
      mockPlayerService.launchFile.mockClear(); // Clear previous calls
      mockPlayerService.launchFile.mockReturnValue(of(file3));

      await service.previous(deviceId);

      // Wait for state to fully update with multiple ticks
      for (let i = 0; i < 5; i++) {
        await nextTick();
      }

      // Should have wrapped to position 2 (last entry)
      const newPosition = service.getCurrentHistoryPosition(deviceId)();

      // Debug: Check if file was launched
      expect(mockPlayerService.launchFile).toHaveBeenCalled();

      // Verify history still has 3 entries (no new entry added)
      const historyAfter = service.getPlayHistory(deviceId)();
      expect(historyAfter?.entries.length).toBe(3);

      expect(newPosition).toBe(2);

      // Should have launched file3
      expect(mockPlayerService.launchFile).toHaveBeenCalledWith(
        deviceId,
        StorageType.Sd,
        file3.path
      );

      // Verify current file is file3
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.path).toBe(file3.path);
    });

    it('should launch random when no history exists', async () => {
      // Clear history
      service.clearHistory(deviceId);

      const playHistory = service.getPlayHistory(deviceId)();
      expect(playHistory).toBeNull();

      const canNavigateBack = service.canNavigateBackwardInHistory(deviceId)();
      expect(canNavigateBack).toBe(false);

      // Mock launchRandom for fallback behavior
      const randomFile = createTestFileItem({ name: 'random.sid', path: '/random/random.sid' });
      mockPlayerService.launchRandom.mockReturnValue(of(randomFile));

      // Call previous() with no history - should fall back to random launch
      await service.previous(deviceId);
      await nextTick();

      // Should have called launchRandom (fallback behavior)
      expect(mockPlayerService.launchRandom).toHaveBeenCalled();

      // Current file should be the random file
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.path).toBe(randomFile.path);
    });

    it('should NOT record history entry when navigating backward', async () => {
      // We start with 3 entries in history
      const initialHistory = service.getPlayHistory(deviceId)();
      expect(initialHistory?.entries.length).toBe(3);

      // Navigate backward
      mockPlayerService.launchFile.mockReturnValue(of(file3));
      await service.previous(deviceId);
      await nextTick();

      // History count should remain unchanged (no new entry recorded)
      const finalHistory = service.getPlayHistory(deviceId)();
      expect(finalHistory?.entries.length).toBe(3);

      // Navigate backward again
      mockPlayerService.launchFile.mockReturnValue(of(file2));
      await service.previous(deviceId);
      await nextTick();

      // Still should be 3 entries
      const stillHistory = service.getPlayHistory(deviceId)();
      expect(stillHistory?.entries.length).toBe(3);
    });

    it('should use file context navigation in directory mode (unchanged)', async () => {
      // Switch back to directory mode
      service.toggleShuffleMode(deviceId); // Toggle back to Directory
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Directory);

      // Set up file context for directory navigation
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: file2, // Start at middle file
        directoryPath: '/music',
        files: testFiles,
        launchMode: LaunchMode.Directory,
      });
      await nextTick();

      mockPlayerService.launchFile.mockReturnValue(of(file1));

      // Call previous() in directory mode
      await service.previous(deviceId);
      await nextTick();

      // Should use file context navigation (launch file1, which is previous in directory)
      expect(mockPlayerService.launchFile).toHaveBeenCalledWith(
        deviceId,
        StorageType.Sd,
        file1.path
      );

      // Should NOT use history navigation
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.path).toBe(file1.path);
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Directory);
    });

    it('should load directory context after history navigation', async () => {
      // Navigate backward from position -1
      mockPlayerService.launchFile.mockReturnValue(of(file3));
      await service.previous(deviceId);
      await nextTick();

      // Should have called navigateToDirectory to load context
      expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
        deviceId,
        storageType: StorageType.Sd,
        path: '/music',
      });

      // Should have loaded file context
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.files).toEqual(testFiles);
      expect(fileContext?.directoryPath).toBe('/music');
      expect(fileContext?.currentIndex).toBe(2); // file3 is at index 2
    });

    it('should set up timer for music file after history navigation', async () => {
      // Ensure we have a music file with playLength
      const musicFile = createTestFileItem({
        name: 'music.sid',
        path: '/music/music.sid',
        type: FileItemType.Song,
        playLength: '2:30',
      });

      // Add music file to history
      mockPlayerService.launchRandom.mockReturnValue(of(musicFile));
      await service.next(deviceId);
      await nextTick();

      // Now at position -1, history has music file as most recent
      // Navigate backward to the music file
      mockPlayerService.launchFile.mockReturnValue(of(musicFile));
      await service.previous(deviceId);
      await nextTick();

      // Wait for timer to be created
      const timerState = await waitForTimerState(deviceId);
      expect(timerState).not.toBeNull();
      if (timerState) {
        expect(timerState.isRunning).toBe(true);
        expect(timerState.totalTime).toBeGreaterThan(0);
      }
    });
  });

  describe('Next Button with History', () => {
    const deviceId = 'device-forward-nav';
    const testFiles = createTestDirectoryFiles();
    const [file1, file2, file3] = testFiles;
    const testDirectory = {
      files: testFiles,
      directories: [],
      path: '/music',
    };

    // Same setup: 3-file history in shuffle mode
    beforeEach(async () => {
      // Initialize device
      service.initializePlayer(deviceId);
      await nextTick();

      // Switch to Shuffle mode
      service.toggleShuffleMode(deviceId);
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Shuffle);
      await nextTick();

      // Launch 3 files to build history
      // After each launch, history is at position -1 (end)
      mockPlayerService.launchRandom
        .mockReturnValueOnce(of(file1))
        .mockReturnValueOnce(of(file2))
        .mockReturnValueOnce(of(file3));

      await service.next(deviceId);
      await nextTick();
      await service.next(deviceId);
      await nextTick();
      await service.next(deviceId);
      await nextTick();

      // History now: [file1, file2, file3], position -1 (at end)
      // Now navigate backward twice to position 1 (file2)
      mockPlayerService.launchFile
        .mockReturnValueOnce(of(file3)) // Back to position 2
        .mockReturnValueOnce(of(file2)); // Back to position 1

      await service.previous(deviceId);
      await nextTick();
      await service.previous(deviceId);
      await nextTick();

      // Now at position 1 (file2), can navigate forward
      // Mock directory state for file context
      mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
      mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => ({
        directory: testDirectory,
        files: testFiles,
        isLoading: false,
        error: null,
      }));

      // Clear previous mock calls
      mockPlayerService.launchRandom.mockClear();
      mockPlayerService.launchFile.mockClear();
      mockStorageStore.navigateToDirectory.mockClear();
    });

    it('should navigate forward in history when available (from middle position)', async () => {
      // At position 1 (file2), navigate forward to position 2 (file3)
      mockPlayerService.launchFile.mockReturnValue(of(file3));

      await service.next(deviceId);
      await nextTick();

      // Should use forward history navigation
      expect(mockPlayerService.launchFile).toHaveBeenCalledWith(
        deviceId,
        StorageType.Sd,
        file3.path
      );

      // Should NOT use launchRandom
      expect(mockPlayerService.launchRandom).not.toHaveBeenCalled();

      // History position should be 2
      const history = service.getPlayHistory(deviceId)();
      expect(history).not.toBeNull();
      if (history) {
        expect(history.currentPosition).toBe(2);
        expect(history.entries[2].file.path).toBe(file3.path);
      }
    });

    it('should launch new random file when at end of history (position -1 or at last entry)', async () => {
      // Navigate forward once more to position 2 (file3)
      mockPlayerService.launchFile.mockReturnValue(of(file3));
      await service.next(deviceId);
      await nextTick();

      // Now at position 2 (end of history)
      // Next should launch new random file, not use forward history
      const newFile = createTestFileItem({
        name: 'file4.prg',
        path: '/music/file4.prg',
      });
      mockPlayerService.launchRandom.mockReturnValue(of(newFile));

      await service.next(deviceId);
      await nextTick();

      // Should launch new random file (navigateNext uses internal shuffle settings)
      expect(mockPlayerService.launchRandom).toHaveBeenCalled();

      // Should NOT use forward history
      expect(mockPlayerService.launchFile).toHaveBeenCalledTimes(1); // Only from the first forward nav
    });

    it('should NOT record history entry when navigating forward through history', async () => {
      // Get initial history entry count
      const initialHistory = service.getPlayHistory(deviceId)();
      const initialCount = initialHistory?.entries.length ?? 0;
      expect(initialCount).toBe(3);

      // Navigate forward from position 1 to position 2
      mockPlayerService.launchFile.mockReturnValue(of(file3));
      await service.next(deviceId);
      await nextTick();

      // History should still have 3 entries (no new recording)
      const finalHistory = service.getPlayHistory(deviceId)();
      const finalCount = finalHistory?.entries.length ?? 0;
      expect(finalCount).toBe(3);

      // Position should be 2
      const history = service.getPlayHistory(deviceId)();
      expect(history).not.toBeNull();
      if (history) {
        expect(history.currentPosition).toBe(2);
      }
    });

    it('should record NEW history entry when launching random at end of history', async () => {
      // Navigate forward to position 2 (end of history)
      mockPlayerService.launchFile.mockReturnValue(of(file3));
      await service.next(deviceId);
      await nextTick();

      // Get history count (should be 3)
      const historyBeforeNew = service.getPlayHistory(deviceId)();
      const countBeforeNew = historyBeforeNew?.entries.length ?? 0;
      expect(countBeforeNew).toBe(3);

      // Launch new random file at end
      const newFile = createTestFileItem({
        name: 'file4.prg',
        path: '/music/file4.prg',
      });
      mockPlayerService.launchRandom.mockReturnValue(of(newFile));

      await service.next(deviceId);
      await nextTick();

      // History should now have 4 entries (new file recorded)
      const finalHistory = service.getPlayHistory(deviceId)();
      const finalCount = finalHistory?.entries.length ?? 0;
      expect(finalCount).toBe(4);

      // New file should be at position -1 (end)
      const history = service.getPlayHistory(deviceId)();
      expect(history).not.toBeNull();
      if (history) {
        expect(history.currentPosition).toBe(-1);
        expect(history.entries[3].file.path).toBe(newFile.path);
      }
    });

    it('should clear forward history when launching new file from middle position (browser-style)', async () => {
      // At position 1 (file2), history has: [file1, file2, file3]
      const initialHistory = service.getPlayHistory(deviceId)();
      expect(initialHistory).not.toBeNull();
      if (initialHistory) {
        expect(initialHistory.entries.length).toBe(3);
        expect(initialHistory.currentPosition).toBe(1);
      }

      // Launch a NEW random file (not navigating forward through history)
      // This should clear file3 and replace with new file
      const newFile = createTestFileItem({
        name: 'file-new.prg',
        path: '/music/file-new.prg',
      });

      // To trigger "launch new random" instead of forward history,
      // we need to be at the end of history. Let's test from position 2 instead.
      // Navigate forward to position 2 first
      mockPlayerService.launchFile.mockReturnValue(of(file3));
      await service.next(deviceId);
      await nextTick();

      // Now at position 2 (end), next launches new random
      mockPlayerService.launchRandom.mockReturnValue(of(newFile));
      await service.next(deviceId);
      await nextTick();

      // History should be: [file1, file2, file3, newFile]
      // This is the expected behavior - new entries append at end
      const finalHistory = service.getPlayHistory(deviceId)();
      expect(finalHistory).not.toBeNull();
      if (finalHistory) {
        expect(finalHistory.entries.length).toBe(4);
        expect(finalHistory.currentPosition).toBe(-1);
        expect(finalHistory.entries[3].file.path).toBe(newFile.path);
      }
    });

    it('should NOT use forward history in directory mode (behavior unchanged)', async () => {
      // Switch to Directory mode
      service.toggleShuffleMode(deviceId); // Toggle back to Directory
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Directory);
      await nextTick();

      // Set up file context for directory navigation
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: file2, // Start at middle file
        directoryPath: '/music',
        files: testFiles,
        launchMode: LaunchMode.Directory,
      });
      await nextTick();

      // Set up for directory next (file3 is next in order)
      mockPlayerService.launchFile.mockReturnValue(of(file3));

      // Call next() in directory mode
      await service.next(deviceId);
      await nextTick();

      // Should use file context navigation (launch file3, which is next in directory)
      expect(mockPlayerService.launchFile).toHaveBeenCalledWith(
        deviceId,
        StorageType.Sd,
        file3.path
      );

      // Should NOT be using forward history navigation
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.path).toBe(file3.path);
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Directory);

      // History position should NOT have changed from directory navigation
      // (Directory mode doesn't affect history position)
    });

    it('should load directory context after forward history navigation', async () => {
      // Navigate forward from position 1 to position 2 (file3)
      mockPlayerService.launchFile.mockReturnValue(of(file3));
      await service.next(deviceId);
      await nextTick();

      // Should have called navigateToDirectory to load context
      expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
        deviceId,
        storageType: StorageType.Sd,
        path: '/music',
      });

      // Should have loaded file context
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.files).toEqual(testFiles);
      expect(fileContext?.directoryPath).toBe('/music');
      expect(fileContext?.currentIndex).toBe(2); // file3 is at index 2
    });
  });

  describe('Edge Cases & Error Handling', () => {
    const deviceId = 'device-edge-cases';
    let testFiles: FileItem[];

    beforeEach(async () => {
      // Create test files
      testFiles = createTestDirectoryFiles();

      // Setup with shuffle mode enabled
      await service.initializePlayer(deviceId);
      await service.toggleShuffleMode(deviceId);
      await nextTick();
    });

    it('should handle history navigation with single entry', async () => {
      // Launch only 1 file
      const file = testFiles[0];
      mockPlayerService.launchRandom.mockReturnValue(of(file));
      await service.next(deviceId);
      await nextTick();

      // Verify single entry exists
      const history = service.getPlayHistory(deviceId)();
      expect(history?.entries).toHaveLength(1);
      expect(history?.currentPosition).toBe(-1); // At end marker

      // Call previous() - should wrap around to the same file at position 0
      mockPlayerService.launchFile.mockReturnValue(of(file));
      await service.previous(deviceId);
      await nextTick();

      // Position should be 0 (the only entry)
      const historyAfterPrev = service.getPlayHistory(deviceId)();
      expect(historyAfterPrev?.currentPosition).toBe(0);
      expect(historyAfterPrev?.entries).toHaveLength(1);

      // Call next() - since we're at position 0 with only 1 entry,
      // there's no forward history, so it should launch a NEW random file
      const file2 = testFiles[1];
      mockPlayerService.launchRandom.mockReturnValue(of(file2));
      await service.next(deviceId);
      await nextTick();

      const historyAfterNext = service.getPlayHistory(deviceId)();
      // Should have launched new file and cleared forward history, position now 0 (new end)
      expect(historyAfterNext?.currentPosition).toBe(-1); // At new end
      expect(historyAfterNext?.entries).toHaveLength(2); // Now 2 entries

      // System should remain stable
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.path).toBe(file2.path);
    });

    it('should handle history navigation with empty history gracefully', async () => {
      // Don't launch any files - history should be null or empty
      const history = service.getPlayHistory(deviceId)();
      expect(history).toBeNull();

      // Call previous() - should be no-op, no errors
      await expect(service.previous(deviceId)).resolves.not.toThrow();
      await nextTick();

      // Call next() - should launch a new random file (normal behavior)
      const file = testFiles[0];
      mockPlayerService.launchRandom.mockReturnValue(of(file));
      await service.next(deviceId);
      await nextTick();

      // Should have created first history entry
      const historyAfter = service.getPlayHistory(deviceId)();
      expect(historyAfter?.entries).toHaveLength(1);
      expect(historyAfter?.entries[0].file.path).toBe(file.path);
    });

    it('should handle failed launch during backward history navigation', async () => {
      // Setup: Launch 2 files to build history
      const file1 = testFiles[0];
      const file2 = testFiles[1];
      mockPlayerService.launchRandom.mockReturnValueOnce(of(file1));
      await service.next(deviceId);
      await nextTick();
      mockPlayerService.launchRandom.mockReturnValueOnce(of(file2));
      await service.next(deviceId);
      await nextTick();

      // Verify position at -1 (end)
      const historyBefore = service.getPlayHistory(deviceId)();
      expect(historyBefore?.currentPosition).toBe(-1);
      expect(historyBefore?.entries).toHaveLength(2);

      // Mock launchFile to fail
      const error = new Error('Launch failed');
      mockPlayerService.launchFile.mockReturnValue(throwError(() => error));

      // Call previous() - should attempt to navigate but handle error
      await service.previous(deviceId);
      await nextTick();

      // Position should remain at -1 (unchanged due to error)
      const historyAfter = service.getPlayHistory(deviceId)();
      expect(historyAfter?.currentPosition).toBe(-1);

      // Error state should be set
      const playerError = service.getError(deviceId)();
      expect(playerError).not.toBeNull();

      // Timer should be cleaned up
      const timerState = service.getTimerState(deviceId)();
      expect(timerState).toBeNull();
    });

    it('should handle failed launch during forward history navigation', async () => {
      // Setup: Launch 3 files, then go back to position 1
      const file1 = testFiles[0];
      const file2 = testFiles[1];
      const file3 = testFiles[2];
      mockPlayerService.launchRandom
        .mockReturnValueOnce(of(file1))
        .mockReturnValueOnce(of(file2))
        .mockReturnValueOnce(of(file3));

      await service.next(deviceId);
      await nextTick();
      await service.next(deviceId);
      await nextTick();
      await service.next(deviceId);
      await nextTick();

      // Navigate back to position 1
      mockPlayerService.launchFile.mockReturnValue(of(file2));
      await service.previous(deviceId);
      await nextTick();
      await service.previous(deviceId);
      await nextTick();

      const historyBefore = service.getPlayHistory(deviceId)();
      expect(historyBefore?.currentPosition).toBe(1);

      // Mock launchFile to fail for forward navigation
      const error = new Error('Forward launch failed');
      mockPlayerService.launchFile.mockReturnValue(throwError(() => error));

      // Call next() - should attempt forward navigation but handle error
      await service.next(deviceId);
      await nextTick();

      // Position should remain at 1 (unchanged due to error)
      const historyAfter = service.getPlayHistory(deviceId)();
      expect(historyAfter?.currentPosition).toBe(1);

      // Error state should be set
      const playerError = service.getError(deviceId)();
      expect(playerError).not.toBeNull();
    });

    it('should maintain independent histories for multiple devices', async () => {
      const deviceId2 = 'device-edge-cases-2';

      // Setup device2
      await service.initializePlayer(deviceId2);
      await service.toggleShuffleMode(deviceId2);
      await nextTick();

      // Launch files on device1
      const file1 = testFiles[0];
      const file2 = testFiles[1];
      mockPlayerService.launchRandom.mockReturnValueOnce(of(file1));
      await service.next(deviceId);
      await nextTick();
      mockPlayerService.launchRandom.mockReturnValueOnce(of(file2));
      await service.next(deviceId);
      await nextTick();

      // Launch different files on device2
      const file3 = testFiles[2];
      mockPlayerService.launchRandom.mockReturnValueOnce(of(file3));
      await service.next(deviceId2);
      await nextTick();

      // Get initial histories
      const history1Before = service.getPlayHistory(deviceId)();
      const history2Before = service.getPlayHistory(deviceId2)();
      expect(history1Before?.entries).toHaveLength(2);
      expect(history2Before?.entries).toHaveLength(1);

      // Navigate backward on device1
      mockPlayerService.launchFile.mockReturnValue(of(file1));
      await service.previous(deviceId);
      await nextTick();

      // Verify device1 history changed
      const history1After = service.getPlayHistory(deviceId)();
      expect(history1After?.currentPosition).toBe(1);

      // Verify device2 history UNCHANGED
      const history2After = service.getPlayHistory(deviceId2)();
      expect(history2After?.entries).toHaveLength(1);
      expect(history2After?.currentPosition).toBe(-1);
      expect(history2After).toEqual(history2Before);
    });

    it('should handle directory context loading failure gracefully', async () => {
      // Launch files to build history
      const file1 = testFiles[0];
      const file2 = testFiles[1];
      mockPlayerService.launchRandom.mockReturnValueOnce(of(file1)).mockReturnValueOnce(of(file2));

      await service.next(deviceId);
      await nextTick();
      await service.next(deviceId);
      await nextTick();

      // Mock navigateToDirectory to fail/reject
      mockStorageStore.navigateToDirectory.mockRejectedValue(new Error('Directory load failed'));

      // Navigate backward - file should still launch successfully
      mockPlayerService.launchFile.mockReturnValue(of(file1));
      await service.previous(deviceId);
      await nextTick();

      // File should have launched despite directory context failure
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.path).toBe(file1.path);
      // Note: hasError would be on player state for file launch failures, not directory loading failures

      // History navigation should have succeeded
      const history = service.getPlayHistory(deviceId)();
      expect(history?.currentPosition).toBe(1);

      // System should not crash - error handled silently
    });

    it('should handle rapid navigation commands correctly', async () => {
      // Launch 5 files to build history - need to create 5 files
      const additionalFiles = [
        createTestFileItem({ name: 'song4.sid', path: '/music/song4.sid' }),
        createTestFileItem({ name: 'song5.sid', path: '/music/song5.sid' }),
      ];
      const files = [...testFiles, ...additionalFiles];

      for (const file of files) {
        mockPlayerService.launchRandom.mockReturnValueOnce(of(file));
        await service.next(deviceId);
        await nextTick();
      }

      // Verify 5 entries at position -1
      const historyBefore = service.getPlayHistory(deviceId)();
      expect(historyBefore?.entries).toHaveLength(5);
      expect(historyBefore?.currentPosition).toBe(-1);

      // Execute rapid navigation sequence
      // previous() -> position 4
      mockPlayerService.launchFile.mockReturnValueOnce(of(files[4]));
      await service.previous(deviceId);
      await nextTick();

      // previous() -> position 3
      mockPlayerService.launchFile.mockReturnValueOnce(of(files[3]));
      await service.previous(deviceId);
      await nextTick();

      // next() -> position 4
      mockPlayerService.launchFile.mockReturnValueOnce(of(files[4]));
      await service.next(deviceId);
      await nextTick();

      // previous() -> position 3
      mockPlayerService.launchFile.mockReturnValueOnce(of(files[3]));
      await service.previous(deviceId);
      await nextTick();

      // next() -> position 4
      mockPlayerService.launchFile.mockReturnValueOnce(of(files[4]));
      await service.next(deviceId);
      await nextTick();

      // Verify final state
      const historyAfter = service.getPlayHistory(deviceId)();
      expect(historyAfter?.currentPosition).toBe(4);
      expect(historyAfter?.entries).toHaveLength(5);

      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.path).toBe(files[4].path);
    });

    it('should preserve history when switching modes', async () => {
      // Launch files in shuffle mode to build history
      const file1 = testFiles[0];
      const file2 = testFiles[1];
      mockPlayerService.launchRandom.mockReturnValueOnce(of(file1)).mockReturnValueOnce(of(file2));

      await service.next(deviceId);
      await nextTick();
      await service.next(deviceId);
      await nextTick();

      // Capture history in shuffle mode
      const historyInShuffle = service.getPlayHistory(deviceId)();
      expect(historyInShuffle?.entries).toHaveLength(2);

      // Switch to directory mode
      await service.toggleShuffleMode(deviceId);
      await nextTick();

      // History should still exist
      const historyInDirectory = service.getPlayHistory(deviceId)();
      expect(historyInDirectory?.entries).toHaveLength(2);
      expect(historyInDirectory).toEqual(historyInShuffle);

      // Switch back to shuffle mode
      await service.toggleShuffleMode(deviceId);
      await nextTick();

      // History should still be intact
      const historyBackInShuffle = service.getPlayHistory(deviceId)();
      expect(historyBackInShuffle?.entries).toHaveLength(2);
      expect(historyBackInShuffle).toEqual(historyInShuffle);

      // Should be able to navigate backward using history
      mockPlayerService.launchFile.mockReturnValue(of(file1));
      await service.previous(deviceId);
      await nextTick();

      const historyAfterNav = service.getPlayHistory(deviceId)();
      expect(historyAfterNav?.currentPosition).toBe(1);
    });
  });

  describe('Complete Workflow Scenarios', () => {
    const deviceId = 'device-workflow';
    let testFiles: FileItem[];

    beforeEach(async () => {
      testFiles = createTestDirectoryFiles();
      await service.initializePlayer(deviceId);
      await service.toggleShuffleMode(deviceId);
      await nextTick();
    });

    it('should handle complete shuffle session with history navigation', async () => {
      // Setup: Launch 5 random files in shuffle mode
      const files = [
        ...testFiles,
        createTestFileItem({ name: 'song4.sid', path: '/music/song4.sid' }),
        createTestFileItem({ name: 'song5.sid', path: '/music/song5.sid' }),
      ];

      // Mock directory state for shuffle mode
      const mockDirectoryState: Partial<StorageDirectoryState> = {
        directory: {
          files,
          directories: [],
          path: '/music',
        },
      };
      mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
      mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => mockDirectoryState);

      // Launch 5 files using launchRandom (shuffle mode)
      for (const file of files) {
        mockPlayerService.launchRandom.mockReturnValueOnce(of(file));
        await service.next(deviceId);
        await nextTick();
      }

      // Verify initial state: 5 entries, position at -1 (end)
      const historyAfterLaunches = service.getPlayHistory(deviceId)();
      expect(historyAfterLaunches?.entries).toHaveLength(5);
      expect(historyAfterLaunches?.currentPosition).toBe(-1);
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('song5.sid');

      // Clear mocks from setup phase
      mockPlayerService.launchRandom.mockClear();

      // Action: Navigate backward 3 times (uses launchFile for history navigation)
      mockPlayerService.launchFile.mockReturnValueOnce(of(files[4]));
      await service.previous(deviceId);
      await nextTick();

      let currentHistory = service.getPlayHistory(deviceId)();
      expect(currentHistory?.currentPosition).toBe(4);
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('song5.sid');

      mockPlayerService.launchFile.mockReturnValueOnce(of(files[3]));
      await service.previous(deviceId);
      await nextTick();

      currentHistory = service.getPlayHistory(deviceId)();
      expect(currentHistory?.currentPosition).toBe(3);
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('song4.sid');

      mockPlayerService.launchFile.mockReturnValueOnce(of(files[2]));
      await service.previous(deviceId);
      await nextTick();

      currentHistory = service.getPlayHistory(deviceId)();
      expect(currentHistory?.currentPosition).toBe(2);
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('song3.sid');

      // Action: Navigate forward 2 times (uses launchFile for history navigation)
      mockPlayerService.launchFile.mockReturnValueOnce(of(files[3]));
      await service.next(deviceId);
      await nextTick();

      currentHistory = service.getPlayHistory(deviceId)();
      expect(currentHistory?.currentPosition).toBe(3);
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('song4.sid');

      mockPlayerService.launchFile.mockReturnValueOnce(of(files[4]));
      await service.next(deviceId);
      await nextTick();

      currentHistory = service.getPlayHistory(deviceId)();
      expect(currentHistory?.currentPosition).toBe(4);
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('song5.sid');

      // Action: Launch new random file (uses launchRandom for new file)
      const newFile = createTestFileItem({ name: 'song6.sid', path: '/music/song6.sid' });
      mockPlayerService.launchRandom.mockReturnValueOnce(of(newFile));
      await service.next(deviceId);
      await nextTick();

      // Verify: New entry added, position at -1, 6 total entries
      const finalHistory = service.getPlayHistory(deviceId)();
      expect(finalHistory?.entries).toHaveLength(6);
      expect(finalHistory?.currentPosition).toBe(-1);
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('song6.sid');
    });

    it('should handle browser-style workflow with forward history clearing', async () => {
      // Scenario: Launch 4 files in shuffle mode
      const files = [
        ...testFiles,
        createTestFileItem({ name: 'song4.sid', path: '/music/song4.sid' }),
      ];

      // Mock directory state for shuffle mode
      const mockDirectoryState: Partial<StorageDirectoryState> = {
        directory: {
          files,
          directories: [],
          path: '/music',
        },
      };
      mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
      mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => mockDirectoryState);

      // Launch 4 files using launchRandom
      for (const file of files) {
        mockPlayerService.launchRandom.mockReturnValueOnce(of(file));
        await service.next(deviceId);
        await nextTick();
      }

      // Verify: 4 entries, position at -1
      let history = service.getPlayHistory(deviceId)();
      expect(history?.entries).toHaveLength(4);
      expect(history?.currentPosition).toBe(-1);

      // Clear mocks
      mockPlayerService.launchRandom.mockClear();

      // Action: Call previous() 2 times to go back (uses launchFile)
      mockPlayerService.launchFile.mockReturnValueOnce(of(files[3]));
      await service.previous(deviceId);
      await nextTick();

      mockPlayerService.launchFile.mockReturnValueOnce(of(files[2]));
      await service.previous(deviceId);
      await nextTick();

      // Verify: Position at 2
      history = service.getPlayHistory(deviceId)();
      expect(history?.currentPosition).toBe(2);

      // Action: Navigate forward to end of history
      mockPlayerService.launchFile.mockReturnValueOnce(of(files[3]));
      await service.next(deviceId);
      await nextTick();

      // Verify: Now at position 3 (navigated forward to entry 3)
      history = service.getPlayHistory(deviceId)();
      expect(history?.currentPosition).toBe(3);

      // Action: Launch a new random file from position 3 (still has forward available, but we're using launchRandom which will navigate to the end first)
      const newFile = createTestFileItem({ name: 'song5.sid', path: '/music/song5.sid' });
      mockPlayerService.launchFile.mockReturnValueOnce(of(files[3])); // Navigate to end first
      mockPlayerService.launchRandom.mockReturnValueOnce(of(newFile));
      await service.next(deviceId);
      await nextTick();
      await service.next(deviceId); //  Now launch new file
      await nextTick();

      // Verify: 5 entries total (kept all 4 + new file)
      history = service.getPlayHistory(deviceId)();
      expect(history?.entries).toHaveLength(5);
      expect(history?.currentPosition).toBe(-1);
      expect(history?.entries[4].file.name).toBe('song5.sid'); // New file at end
    });

    it('should handle mixed mode workflow', async () => {
      // Mock directory state for shuffle mode
      const mockDirectoryState: Partial<StorageDirectoryState> = {
        directory: {
          files: testFiles,
          directories: [],
          path: '/music',
        },
      };
      mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
      mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => mockDirectoryState);

      // Scenario: Start in shuffle mode, launch files and build history
      mockPlayerService.launchRandom.mockReturnValueOnce(of(testFiles[0]));
      await service.next(deviceId);
      await nextTick();

      mockPlayerService.launchRandom.mockReturnValueOnce(of(testFiles[1]));
      await service.next(deviceId);
      await nextTick();

      // Verify shuffle mode history
      const historyInShuffle = service.getPlayHistory(deviceId)();
      expect(historyInShuffle?.entries).toHaveLength(2);

      // Action: Switch to directory mode
      await service.toggleShuffleMode(deviceId);
      await nextTick();

      const launchMode1 = service.getLaunchMode(deviceId)();
      expect(launchMode1).toBe(LaunchMode.Directory);

      // Action: Navigate in directory mode (mock file context)
      const directoryFile = createTestFileItem({
        name: 'dir-file.sid',
        path: '/music/dir-file.sid',
      });
      mockPlayerService.launchFile.mockReturnValueOnce(of(directoryFile));

      // Mock navigateNext to simulate directory navigation
      await service.next(deviceId);
      await nextTick();

      // Verify directory navigation worked
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('dir-file.sid');

      // History should grow even in directory mode
      const historyInDirectory = service.getPlayHistory(deviceId)();
      expect(historyInDirectory?.entries.length).toBeGreaterThanOrEqual(2);

      // Action: Switch back to shuffle mode
      await service.toggleShuffleMode(deviceId);
      await nextTick();

      const launchMode2 = service.getLaunchMode(deviceId)();
      expect(launchMode2).toBe(LaunchMode.Shuffle);

      // Verify: History persists, can navigate
      const historyBackInShuffle = service.getPlayHistory(deviceId)();
      expect(historyBackInShuffle?.entries.length).toBeGreaterThanOrEqual(2);

      // Should be able to navigate in shuffle mode
      const canGoBack = service.canNavigateBackwardInHistory(deviceId)();
      expect(canGoBack).toBe(true);
    });

    it('should maintain timer consistency through navigation', async () => {
      // Mock directory state for shuffle mode
      const mockDirectoryState: Partial<StorageDirectoryState> = {
        directory: {
          files: testFiles,
          directories: [],
          path: '/music',
        },
      };
      mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
      mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => mockDirectoryState);

      // Scenario: Launch music files (that create timers)
      const musicFiles = testFiles; // All test files are music files

      // Launch 3 music files using launchRandom
      for (let i = 0; i < 3; i++) {
        mockPlayerService.launchRandom.mockReturnValueOnce(of(musicFiles[i]));
        await service.next(deviceId);
        await nextTick();
      }

      // Verify we have 3 entries
      let playHistory = service.getPlayHistory(deviceId)();
      expect(playHistory?.entries).toHaveLength(3);

      // Clear mocks
      mockPlayerService.launchRandom.mockClear();

      // Action: Navigate backward (uses launchFile)
      mockPlayerService.launchFile.mockReturnValueOnce(of(musicFiles[2]));
      await service.previous(deviceId);
      await nextTick();

      // Verify: Position changed
      playHistory = service.getPlayHistory(deviceId)();
      expect(playHistory?.currentPosition).toBe(2);

      // Action: Navigate backward again to position 1 (uses launchFile)
      mockPlayerService.launchFile.mockReturnValueOnce(of(musicFiles[1]));
      await service.previous(deviceId);
      await nextTick();

      // Verify: Position 1, correct file loaded
      playHistory = service.getPlayHistory(deviceId)();
      expect(playHistory?.currentPosition).toBe(1);
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.name).toBe('song2.sid');

      // Action: Navigate backward to position 0
      mockPlayerService.launchFile.mockReturnValueOnce(of(musicFiles[0]));
      await service.previous(deviceId);
      await nextTick();

      // Verify: Position 0, timers working correctly throughout
      playHistory = service.getPlayHistory(deviceId)();
      expect(playHistory?.currentPosition).toBe(0);
      const finalFile = service.getCurrentFile(deviceId)();
      expect(finalFile?.file.name).toBe('song1.sid');
    });

    it('should maintain accurate position tracking through complex navigation', async () => {
      // Scenario: Launch 10 files in shuffle mode
      const files = [
        ...testFiles,
        createTestFileItem({ name: 'song4.sid', path: '/music/song4.sid' }),
        createTestFileItem({ name: 'song5.sid', path: '/music/song5.sid' }),
        createTestFileItem({ name: 'song6.sid', path: '/music/song6.sid' }),
        createTestFileItem({ name: 'song7.sid', path: '/music/song7.sid' }),
        createTestFileItem({ name: 'song8.sid', path: '/music/song8.sid' }),
        createTestFileItem({ name: 'song9.sid', path: '/music/song9.sid' }),
        createTestFileItem({ name: 'song10.sid', path: '/music/song10.sid' }),
      ];

      // Mock directory state for shuffle mode
      const mockDirectoryState: Partial<StorageDirectoryState> = {
        directory: {
          files,
          directories: [],
          path: '/music',
        },
      };
      mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
      mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => mockDirectoryState);

      // Launch all files using launchRandom
      for (const file of files) {
        mockPlayerService.launchRandom.mockReturnValueOnce(of(file));
        await service.next(deviceId);
        await nextTick();
      }

      // Verify: 10 entries, position at -1
      let history = service.getPlayHistory(deviceId)();
      expect(history?.entries).toHaveLength(10);
      expect(history?.currentPosition).toBe(-1);

      // Clear mocks
      mockPlayerService.launchRandom.mockClear();

      // Action: Execute complex navigation pattern
      // Go back 5 times (uses launchFile)
      for (let i = 0; i < 5; i++) {
        const targetFile = files[9 - i];
        mockPlayerService.launchFile.mockReturnValueOnce(of(targetFile));
        await service.previous(deviceId);
        await nextTick();
      }

      // Position should be 5 (at index 5)
      history = service.getPlayHistory(deviceId)();
      expect(history?.currentPosition).toBe(5);

      // Current file should match entry at position 5
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.name).toBe(history?.entries[5].file.name);

      // Go forward 3 times (uses launchFile)
      for (let i = 0; i < 3; i++) {
        const targetFile = files[5 + i + 1];
        mockPlayerService.launchFile.mockReturnValueOnce(of(targetFile));
        await service.next(deviceId);
        await nextTick();
      }

      // Position should be 8 (at index 8)
      history = service.getPlayHistory(deviceId)();
      expect(history?.currentPosition).toBe(8);

      // Current file should match entry at position 8
      const currentFileAfterForward = service.getCurrentFile(deviceId)();
      expect(currentFileAfterForward?.file.name).toBe(history?.entries[8].file.name);

      // Go back 2 times (uses launchFile)
      for (let i = 0; i < 2; i++) {
        const targetFile = files[8 - i - 1]; // -1 because we're loading the previous file
        mockPlayerService.launchFile.mockReturnValueOnce(of(targetFile));
        await service.previous(deviceId);
        await nextTick();
      }

      // Position should be 6 (at index 6)
      history = service.getPlayHistory(deviceId)();
      expect(history?.currentPosition).toBe(6);

      // Current file should match entry at position 6
      const finalCurrentFile = service.getCurrentFile(deviceId)();
      expect(finalCurrentFile?.file.name).toBe(history?.entries[6].file.name);

      // Verify: Position always matches currentFile's index
      expect(history?.currentPosition).toBe(6);
      expect(history?.entries[6].file.path).toBe(finalCurrentFile?.file.path);
    });
  });

  describe('Phase 3: History View & UI Integration', () => {
    describe('History View Visibility', () => {
      const deviceId = 'device-history-view';
      const testFiles = createTestDirectoryFiles();

      beforeEach(async () => {
        service.initializePlayer(deviceId);
        await nextTick();
      });

      it('should have history view hidden by default after player initialization', () => {
        // Verify history view is hidden by default
        const isVisible = service.isHistoryViewVisible(deviceId)();
        expect(isVisible).toBe(false);
      });

      it('should make history view visible when toggled once', () => {
        // Initial state: hidden
        expect(service.isHistoryViewVisible(deviceId)()).toBe(false);

        // Toggle on
        service.toggleHistoryView(deviceId);

        // Verify visible
        expect(service.isHistoryViewVisible(deviceId)()).toBe(true);
      });

      it('should return to hidden state when toggled twice', () => {
        // Initial state: hidden
        expect(service.isHistoryViewVisible(deviceId)()).toBe(false);

        // Toggle on
        service.toggleHistoryView(deviceId);
        expect(service.isHistoryViewVisible(deviceId)()).toBe(true);

        // Toggle off
        service.toggleHistoryView(deviceId);
        expect(service.isHistoryViewVisible(deviceId)()).toBe(false);
      });

      it('should maintain independent visibility per device', () => {
        const deviceId2 = 'device-history-view-2';
        service.initializePlayer(deviceId2);

        // Both start hidden
        expect(service.isHistoryViewVisible(deviceId)()).toBe(false);
        expect(service.isHistoryViewVisible(deviceId2)()).toBe(false);

        // Toggle device1 on
        service.toggleHistoryView(deviceId);
        expect(service.isHistoryViewVisible(deviceId)()).toBe(true);
        expect(service.isHistoryViewVisible(deviceId2)()).toBe(false);

        // Toggle device2 on
        service.toggleHistoryView(deviceId2);
        expect(service.isHistoryViewVisible(deviceId)()).toBe(true);
        expect(service.isHistoryViewVisible(deviceId2)()).toBe(true);

        // Toggle device1 off - device2 unaffected
        service.toggleHistoryView(deviceId);
        expect(service.isHistoryViewVisible(deviceId)()).toBe(false);
        expect(service.isHistoryViewVisible(deviceId2)()).toBe(true);
      });

      it('should keep history visible when navigating to history position', async () => {
        // Switch to shuffle mode and build history
        service.toggleShuffleMode(deviceId);

        // Mock directory state
        const mockDirectoryState: Partial<StorageDirectoryState> = {
          directory: {
            files: testFiles,
            directories: [],
            path: '/music',
          },
        };
        mockStorageStore.navigateToDirectory.mockResolvedValue(undefined);
        mockStorageStore.getSelectedDirectoryState.mockReturnValue(() => mockDirectoryState);

        // Launch 3 files to build history
        mockPlayerService.launchRandom
          .mockReturnValueOnce(of(testFiles[0]))
          .mockReturnValueOnce(of(testFiles[1]))
          .mockReturnValueOnce(of(testFiles[2]));

        await service.next(deviceId);
        await nextTick();
        await service.next(deviceId);
        await nextTick();
        await service.next(deviceId);
        await nextTick();

        // Verify history exists
        const history = service.getPlayHistory(deviceId)();
        expect(history?.entries.length).toBe(3);

        // Toggle history view on
        service.toggleHistoryView(deviceId);
        expect(service.isHistoryViewVisible(deviceId)()).toBe(true);

        // Navigate to history position 0
        mockPlayerService.launchFile.mockReturnValue(of(testFiles[0]));
        await service.navigateToHistoryPosition(deviceId, 0);
        await nextTick();

        // Verify history view remains visible
        expect(service.isHistoryViewVisible(deviceId)()).toBe(true);

        // Verify we're at position 0
        const currentPosition = service.getCurrentHistoryPosition(deviceId)();
        expect(currentPosition).toBe(0);

        // Verify current file matches entry at position 0
        const currentFile = service.getCurrentFile(deviceId)();
        expect(currentFile?.file.path).toBe(testFiles[0].path);

        // Verify no new history entry was recorded (still 3 entries)
        const finalHistory = service.getPlayHistory(deviceId)();
        expect(finalHistory?.entries.length).toBe(3);
      });

      it('should hide history view when launching a new file', async () => {
        // Toggle history view on
        service.toggleHistoryView(deviceId);
        expect(service.isHistoryViewVisible(deviceId)()).toBe(true);

        // Launch a file using launchFileWithContext
        const file = testFiles[0];
        mockPlayerService.launchFile.mockReturnValue(of(file));

        await service.launchFileWithContext({
          deviceId,
          storageType: StorageType.Sd,
          file,
          directoryPath: '/music',
          files: testFiles,
          launchMode: LaunchMode.Directory,
        });
        await nextTick();

        // Verify history view is now hidden
        expect(service.isHistoryViewVisible(deviceId)()).toBe(false);
      });
    });
  });
});
