import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import {
  PLAYER_SERVICE,
  IPlayerService,
  StorageType,
  FileItem,
  LaunchMode,
  PlayerStatus,
  PlayerFilterType,
  PlayerScope,
} from '@teensyrom-nx/domain';
import { PlayerContextService } from './player-context.service';
import { PlayerStore } from './player-store';
import { StorageStore } from '../storage/storage-store';
import { StorageKeyUtil } from '../storage/storage-key.util';

const createFile = (overrides: Partial<FileItem> = {}): FileItem => ({
  name: 'test-file.sid',
  path: '/music/test-file.sid',
  size: 1024,
  type: overrides.type ?? ('Song' as FileItem['type']),
  isFavorite: false,
  title: overrides.title ?? 'Test Song',
  creator: overrides.creator ?? 'Test Creator',
  releaseInfo: '',
  description: '',
  shareUrl: '',
  metadataSource: '',
  meta1: '',
  meta2: '',
  metadataSourcePath: '',
  parentPath: '/music',
  playLength: '',
  subtuneLengths: [],
  startSubtuneNum: 0,
  images: [],
  ...overrides,
});

const createFileList = (count: number, baseName = 'file'): FileItem[] => {
  return Array.from({ length: count }, (_, i) => 
    createFile({ 
      name: `${baseName}-${i + 1}.sid`,
      path: `/music/${baseName}-${i + 1}.sid`,
      title: `${baseName} ${i + 1}`
    })
  );
};

describe('PlayerContextService - Behavioral Testing', () => {
  let service: PlayerContextService;
  let playerServiceMock: IPlayerService;
  let storageStoreMock: Partial<StorageStore>;

  beforeEach(() => {
    playerServiceMock = {
      launchFile: vi.fn(),
      launchRandom: vi.fn(),
    };

    storageStoreMock = {
      navigateToDirectory: vi.fn().mockResolvedValue(undefined),
      getSelectedDirectoryState: vi.fn().mockReturnValue(() => null),
    };

    TestBed.configureTestingModule({
      providers: [
        PlayerContextService,
        PlayerStore,
        { provide: PLAYER_SERVICE, useValue: playerServiceMock },
        { provide: StorageStore, useValue: storageStoreMock },
      ],
    });

    service = TestBed.inject(PlayerContextService);
  });

  describe('Player Lifecycle Management', () => {
    it('should initialize player state for new device', () => {
      const deviceId = 'device-new';

      // Act: Initialize player
      service.initializePlayer(deviceId);

      // Assert: Initial state should be created
      expect(service.getCurrentFile(deviceId)()).toBeNull();
      expect(service.getFileContext(deviceId)()).toBeNull();
      expect(service.isLoading(deviceId)()).toBe(false);
      expect(service.getError(deviceId)()).toBeNull();
      expect(service.getStatus(deviceId)()).toBe(PlayerStatus.Stopped);
    });

    it('should remove player state for device', () => {
      const deviceId = 'device-to-remove';
      const file = createFile();

      // Arrange: Set up player with some state
      service.initializePlayer(deviceId);
      playerServiceMock.launchFile = vi.fn().mockReturnValue(of(file));

      // Act: Remove player
      service.removePlayer(deviceId);

      // Assert: State should be cleaned up
      expect(service.getCurrentFile(deviceId)()).toBeNull();
      expect(service.getFileContext(deviceId)()).toBeNull();
      expect(service.isLoading(deviceId)()).toBe(false);
      expect(service.getError(deviceId)()).toBeNull();
      expect(service.getStatus(deviceId)()).toBe(PlayerStatus.Stopped);
    });

    it('should handle removing non-existent player gracefully', () => {
      const deviceId = 'non-existent-device';

      // Act: Remove player that doesn't exist
      expect(() => service.removePlayer(deviceId)).not.toThrow();

      // Assert: Should return default state
      expect(service.getCurrentFile(deviceId)()).toBeNull();
      expect(service.getStatus(deviceId)()).toBe(PlayerStatus.Stopped);
    });
  });

  describe('File Launch Orchestration', () => {
    it('should orchestrate successful file launch with full context', async () => {
      // Arrange: Set up test data
      const deviceId = 'device-launch-success';
      const storageType = StorageType.Sd;
      const targetFile = createFile({ name: 'target.sid', path: '/music/target.sid' });
      const contextFiles = createFileList(5, 'song');
      const launchedFile = createFile({ name: 'target.sid', description: 'Successfully launched' });
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(of(launchedFile));

      // Act: Launch file with context
      await service.launchFileWithContext({
        deviceId,
        storageType,
        file: targetFile,
        directoryPath: '/music',
        files: contextFiles,
        launchMode: LaunchMode.Directory,
      });

      // Assert: Infrastructure service called correctly
      expect(playerServiceMock.launchFile).toHaveBeenCalledWith(
        deviceId, 
        storageType, 
        targetFile.path
      );
      expect(playerServiceMock.launchFile).toHaveBeenCalledTimes(1);

      // Assert: Player state updated correctly
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile).not.toBeNull();
      expect(currentFile?.file.name).toBe('target.sid');
      expect(currentFile?.file.description).toBe('Successfully launched');
      expect(currentFile?.launchMode).toBe(LaunchMode.Directory);
      expect(currentFile?.storageKey).toBe(StorageKeyUtil.create(deviceId, storageType));
      expect(currentFile?.launchedAt).toBeGreaterThan(0);

      // Assert: File context stored correctly
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext).not.toBeNull();
      expect(fileContext?.directoryPath).toBe('/music');
      expect(fileContext?.files).toHaveLength(5);
      expect(fileContext?.currentIndex).toBe(0); // First file in context
      expect(fileContext?.launchMode).toBe(LaunchMode.Directory);
      expect(fileContext?.storageKey).toBe(StorageKeyUtil.create(deviceId, storageType));

      // Assert: Player status updated
      expect(service.isLoading(deviceId)()).toBe(false);
      expect(service.getError(deviceId)()).toBeNull();
      expect(service.getStatus(deviceId)()).toBe(PlayerStatus.Playing);
    });

    it('should use default values for optional launch parameters', async () => {
      // Arrange: Minimal launch request
      const deviceId = 'device-defaults';
      const storageType = StorageType.Usb;
      const file = createFile();
      const contextFiles = [file];
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(of(file));

      // Act: Launch without optional parameters
      await service.launchFileWithContext({
        deviceId,
        storageType,
        file,
        directoryPath: '/music',
        files: contextFiles,
        // launchMode omitted - should default to Directory
      });

      // Assert: Default values applied
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.launchMode).toBe(LaunchMode.Directory);
      
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.launchMode).toBe(LaunchMode.Directory);
    });

    it('should preserve empty directory path as provided', async () => {
      // Arrange: Empty directory path
      const deviceId = 'device-empty-path';
      const storageType = StorageType.Sd;
      const file = createFile({ path: '/test.sid' });
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(of(file));

      // Act: Launch with empty string directoryPath
      await service.launchFileWithContext({
        deviceId,
        storageType,
        file,
        directoryPath: '', // Empty string - should be preserved as-is
        files: [file],
      });

      // Assert: Empty string preserved (nullish coalescing only affects null/undefined)
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.directoryPath).toBe('');
    });

    it('should preserve file array immutability', async () => {
      // Arrange: Original file array
      const deviceId = 'device-immutable';
      const storageType = StorageType.Sd;
      const file = createFile();
      const originalFiles = createFileList(3, 'original');
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(of(file));

      // Act: Launch with files array
      await service.launchFileWithContext({
        deviceId,
        storageType,
        file,
        directoryPath: '/music',
        files: originalFiles,
      });

      // Assert: Original array unchanged
      expect(originalFiles).toHaveLength(3);
      expect(originalFiles[0].name).toBe('original-1.sid');
      
      // Assert: Service has its own copy
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.files).not.toBe(originalFiles);
      expect(fileContext?.files).toHaveLength(3);
    });
  });

  describe('Multi-Device State Isolation', () => {
    it('should maintain completely independent state per device', async () => {
      // Arrange: Multiple devices with different configurations
      const deviceA = 'device-a';
      const deviceB = 'device-b'; 
      const deviceC = 'device-c';
      
      const fileA = createFile({ name: 'song-a.sid', path: '/a/song-a.sid' });
      const fileB = createFile({ name: 'song-b.sid', path: '/b/song-b.sid' });
      const fileC = createFile({ name: 'song-c.sid', path: '/c/song-c.sid' });
      
      const filesA = [fileA, createFile({ name: 'extra-a.sid' })];
      const filesB = [fileB];
      const filesC = createFileList(10, 'album');
      
      playerServiceMock.launchFile = vi.fn()
        .mockReturnValueOnce(of(fileA))
        .mockReturnValueOnce(of(fileB))
        .mockReturnValueOnce(of(fileC));

      // Act: Launch files on different devices
      await service.launchFileWithContext({
        deviceId: deviceA,
        storageType: StorageType.Sd,
        file: fileA,
        directoryPath: '/a',
        files: filesA,
        launchMode: LaunchMode.Directory,
      });

      await service.launchFileWithContext({
        deviceId: deviceB,
        storageType: StorageType.Usb,
        file: fileB,
        directoryPath: '/b',
        files: filesB,
        launchMode: LaunchMode.Shuffle,
      });

      await service.launchFileWithContext({
        deviceId: deviceC,
        storageType: StorageType.Sd,
        file: fileC,
        directoryPath: '/c/albums',
        files: filesC,
        launchMode: LaunchMode.Search,
      });

      // Assert: Device A state
      expect(service.getCurrentFile(deviceA)()?.file.path).toBe('/a/song-a.sid');
      expect(service.getFileContext(deviceA)()?.directoryPath).toBe('/a');
      expect(service.getFileContext(deviceA)()?.files).toHaveLength(2);
      expect(service.getFileContext(deviceA)()?.launchMode).toBe(LaunchMode.Directory);
      expect(service.getStatus(deviceA)()).toBe(PlayerStatus.Playing);

      // Assert: Device B state
      expect(service.getCurrentFile(deviceB)()?.file.path).toBe('/b/song-b.sid');
      expect(service.getFileContext(deviceB)()?.directoryPath).toBe('/b');
      expect(service.getFileContext(deviceB)()?.files).toHaveLength(1);
      expect(service.getFileContext(deviceB)()?.launchMode).toBe(LaunchMode.Shuffle);
      expect(service.getStatus(deviceB)()).toBe(PlayerStatus.Playing);

      // Assert: Device C state
      expect(service.getCurrentFile(deviceC)()?.file.path).toBe('/c/song-c.sid');
      expect(service.getFileContext(deviceC)()?.directoryPath).toBe('/c/albums');
      expect(service.getFileContext(deviceC)()?.files).toHaveLength(10);
      expect(service.getFileContext(deviceC)()?.launchMode).toBe(LaunchMode.Search);
      expect(service.getStatus(deviceC)()).toBe(PlayerStatus.Playing);

      // Assert: States are truly isolated
      expect(service.getCurrentFile(deviceA)()?.storageKey).not.toBe(service.getCurrentFile(deviceB)()?.storageKey);
      expect(service.getCurrentFile(deviceB)()?.storageKey).not.toBe(service.getCurrentFile(deviceC)()?.storageKey);
    });

    it('should handle device removal without affecting other devices', async () => {
      // Arrange: Multiple devices with active states
      const deviceKeep = 'device-keep';
      const deviceRemove = 'device-remove';
      const file = createFile();
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(of(file));

      // Set up both devices
      await service.launchFileWithContext({
        deviceId: deviceKeep,
        storageType: StorageType.Sd,
        file,
        directoryPath: '/keep',
        files: [file],
      });

      await service.launchFileWithContext({
        deviceId: deviceRemove,
        storageType: StorageType.Usb,
        file,
        directoryPath: '/remove',
        files: [file],
      });

      // Act: Remove one device
      service.removePlayer(deviceRemove);

      // Assert: Kept device unaffected
      expect(service.getCurrentFile(deviceKeep)()?.file.name).toBe('test-file.sid');
      expect(service.getFileContext(deviceKeep)()?.directoryPath).toBe('/keep');
      expect(service.getStatus(deviceKeep)()).toBe(PlayerStatus.Playing);

      // Assert: Removed device state cleared
      expect(service.getCurrentFile(deviceRemove)()).toBeNull();
      expect(service.getFileContext(deviceRemove)()).toBeNull();
      expect(service.getStatus(deviceRemove)()).toBe(PlayerStatus.Stopped);
    });
  });

  describe('Error Handling and Edge Cases', () => {
    it('should handle infrastructure launch failures gracefully', async () => {
      // Arrange: Infrastructure service will fail
      const deviceId = 'device-fail';
      const file = createFile();
      const errorMessage = 'TeensyROM device not responding';
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(
        throwError(() => new Error(errorMessage))
      );

      // Act & Assert: Error should be propagated
      await expect(service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file,
        directoryPath: '/music',
        files: [file],
      })).rejects.toThrow(errorMessage);

      // Assert: Error state tracked correctly
      expect(service.getError(deviceId)()).toBe(errorMessage);
      expect(service.getCurrentFile(deviceId)()).toBeNull();
      expect(service.isLoading(deviceId)()).toBe(false);
      expect(service.getStatus(deviceId)()).toBe(PlayerStatus.Stopped);
    });

    it('should handle network timeout errors', async () => {
      // Arrange: Network timeout simulation
      const deviceId = 'device-timeout';
      const file = createFile();
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(
        throwError(() => ({ name: 'TimeoutError', message: 'Request timeout after 30s' }))
      );

      // Act: Attempt launch
      await expect(service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file,
        directoryPath: '/timeout-test',
        files: [file],
      })).rejects.toMatchObject({
        name: 'TimeoutError',
        message: 'Request timeout after 30s'
      });

      // Assert: Service maintains consistent error state
      expect(service.getError(deviceId)()).toBe('Request timeout after 30s');
      expect(service.getStatus(deviceId)()).toBe(PlayerStatus.Stopped);
    });

    it('should handle device not found errors', async () => {
      // Arrange: Device not found scenario
      const deviceId = 'device-not-found';
      const file = createFile();
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(
        throwError(() => ({ status: 404, message: 'Device not found' }))
      );

      // Act: Launch on non-existent device
      await expect(service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file,
        directoryPath: '/music',
        files: [file],
      })).rejects.toMatchObject({
        status: 404,
        message: 'Device not found'
      });

      // Assert: Appropriate error handling
      expect(service.getError(deviceId)()).toBe('Device not found');
    });

    it('should handle invalid file path errors', async () => {
      // Arrange: Invalid file path
      const deviceId = 'device-invalid-path';
      const invalidFile = createFile({ path: '/invalid/path/nonexistent.sid' });
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(
        throwError(() => new Error('File not found: /invalid/path/nonexistent.sid'))
      );

      // Act: Launch invalid file
      await expect(service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: invalidFile,
        directoryPath: '/invalid',
        files: [invalidFile],
      })).rejects.toThrow('File not found: /invalid/path/nonexistent.sid');

      // Assert: Error properly tracked
      expect(service.getError(deviceId)()).toBe('File not found: /invalid/path/nonexistent.sid');
    });

    it('should handle corrupted file launch attempts', async () => {
      // Arrange: Corrupted file scenario
      const deviceId = 'device-corrupted';
      const corruptedFile = createFile({ name: 'corrupted.sid' });
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(
        throwError(() => new Error('File corrupted or invalid format'))
      );

      // Act: Launch corrupted file
      await expect(service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: corruptedFile,
        directoryPath: '/music',
        files: [corruptedFile],
      })).rejects.toThrow('File corrupted or invalid format');

      // Assert: Service handles corruption gracefully
      expect(service.getError(deviceId)()).toBe('File corrupted or invalid format');
      expect(service.getCurrentFile(deviceId)()).toBeNull();
    });

    it('should handle empty file context gracefully', async () => {
      // Arrange: Empty files array
      const deviceId = 'device-empty-context';
      const file = createFile();
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(of(file));

      // Act: Launch with empty context
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file,
        directoryPath: '/empty',
        files: [], // Empty array
      });

      // Assert: Service handles empty context properly
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.files).toHaveLength(0);
      expect(fileContext?.currentIndex).toBe(0);
      expect(fileContext?.directoryPath).toBe('/empty');
    });
  });

  describe('Signal-Based API Behavior', () => {
    it('should return reactive signals that update automatically', async () => {
      // Arrange: Test device
      const deviceId = 'device-reactive';
      const file1 = createFile({ name: 'first.sid' });
      const file2 = createFile({ name: 'second.sid' });
      
      playerServiceMock.launchFile = vi.fn()
        .mockReturnValueOnce(of(file1))
        .mockReturnValueOnce(of(file2));

      // Act: Get signals before any launches
      const currentFileSignal = service.getCurrentFile(deviceId);
      const statusSignal = service.getStatus(deviceId);
      const isLoadingSignal = service.isLoading(deviceId);
      const errorSignal = service.getError(deviceId);

      // Assert: Initial signal values
      expect(currentFileSignal()).toBeNull();
      expect(statusSignal()).toBe(PlayerStatus.Stopped);
      expect(isLoadingSignal()).toBe(false);
      expect(errorSignal()).toBeNull();

      // Act: Launch first file
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: file1,
        directoryPath: '/music',
        files: [file1, file2],
      });

      // Assert: Signals updated automatically
      expect(currentFileSignal()?.file.name).toBe('first.sid');
      expect(statusSignal()).toBe(PlayerStatus.Playing);
      expect(isLoadingSignal()).toBe(false);
      expect(errorSignal()).toBeNull();

      // Act: Launch second file
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: file2,
        directoryPath: '/music',
        files: [file1, file2],
      });

      // Assert: Same signal instances updated with new values
      expect(currentFileSignal()?.file.name).toBe('second.sid');
      expect(statusSignal()).toBe(PlayerStatus.Playing);
    });

    it('should provide independent signals per device', () => {
      // Arrange: Multiple devices
      const deviceA = 'device-a';
      const deviceB = 'device-b';

      // Act: Get signals for different devices
      const fileSignalA = service.getCurrentFile(deviceA);
      const fileSignalB = service.getCurrentFile(deviceB);
      const statusSignalA = service.getStatus(deviceA);
      const statusSignalB = service.getStatus(deviceB);

      // Assert: Signals are independent objects
      expect(fileSignalA).not.toBe(fileSignalB);
      expect(statusSignalA).not.toBe(statusSignalB);

      // Assert: Initial values are the same but independent
      expect(fileSignalA()).toBeNull();
      expect(fileSignalB()).toBeNull();
      expect(statusSignalA()).toBe(PlayerStatus.Stopped);
      expect(statusSignalB()).toBe(PlayerStatus.Stopped);
    });
  });

  describe('Infrastructure Service Integration', () => {
    it('should pass correct parameters to infrastructure service', async () => {
      // Arrange: Specific test parameters
      const deviceId = 'test-device-123';
      const storageType = StorageType.Usb;
      const file = createFile({ path: '/specific/path/to/file.sid' });
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(of(file));

      // Act: Launch with specific parameters
      await service.launchFileWithContext({
        deviceId,
        storageType,
        file,
        directoryPath: '/specific/path',
        files: [file],
        launchMode: LaunchMode.Shuffle,
      });

      // Assert: Infrastructure service called with exact parameters
      expect(playerServiceMock.launchFile).toHaveBeenCalledWith(
        'test-device-123',
        StorageType.Usb,
        '/specific/path/to/file.sid'
      );
      expect(playerServiceMock.launchFile).toHaveBeenCalledTimes(1);
    });

    it('should handle infrastructure service returning modified file data', async () => {
      // Arrange: Service returns enhanced file data
      const deviceId = 'device-enhanced';
      const originalFile = createFile({ 
        name: 'original.sid', 
        description: 'Original description',
        title: 'Original Title'
      });
      
      const enhancedFile = createFile({
        name: 'enhanced.sid',
        description: 'Enhanced by TeensyROM',
        title: 'Enhanced Title with Metadata',
        creator: 'Enhanced Creator Info'
      });
      
      playerServiceMock.launchFile = vi.fn().mockReturnValue(of(enhancedFile));

      // Act: Launch original file
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Sd,
        file: originalFile,
        directoryPath: '/music',
        files: [originalFile],
      });

      // Assert: Service uses enhanced data from infrastructure
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.name).toBe('enhanced.sid');
      expect(currentFile?.file.description).toBe('Enhanced by TeensyROM');
      expect(currentFile?.file.title).toBe('Enhanced Title with Metadata');
      expect(currentFile?.file.creator).toBe('Enhanced Creator Info');
    });
  });

  describe('Shuffle Mode Functionality', () => {
    it('should orchestrate random file launch without directory context', async () => {
      // Arrange: Set up random file launch
      const deviceId = 'device-shuffle';
      const randomFile = createFile({ 
        name: 'random.sid', 
        path: '/music/subdir/random.sid',
        parentPath: '/music/subdir',
        title: 'Random Song' 
      });
      
      playerServiceMock.launchRandom = vi.fn().mockReturnValue(of(randomFile));

      // Act: Launch random file
      await service.launchRandomFile(deviceId);

      // Assert: Random file should be launched
      expect(playerServiceMock.launchRandom).toHaveBeenCalledWith(
        deviceId,
        PlayerScope.Storage,
        PlayerFilterType.All,
        undefined
      );

      // Assert: Current file should be set
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.name).toBe('random.sid');
      expect(currentFile?.launchMode).toBe(LaunchMode.Shuffle);
    });

    it('should coordinate with storage store to load directory context for random file', async () => {
      // Arrange: Set up random file with parent directory
      const deviceId = 'device-shuffle-context';
      const randomFile = createFile({ 
        name: 'random.sid', 
        path: '/music/jazz/random.sid',
        parentPath: '/music/jazz',
      });
      
      const directoryFiles = [
        createFile({ name: 'song1.sid', path: '/music/jazz/song1.sid' }),
        randomFile,
        createFile({ name: 'song3.sid', path: '/music/jazz/song3.sid' }),
      ];

      playerServiceMock.launchRandom = vi.fn().mockReturnValue(of(randomFile));
      
      // Mock storage store to return directory data
      storageStoreMock.getSelectedDirectoryState = vi.fn().mockReturnValue(() => ({
        directory: { files: directoryFiles },
        currentPath: '/music/jazz',
        deviceId,
        storageType: StorageType.Sd,
        isLoaded: true,
        isLoading: false,
        error: null,
        lastLoadTime: Date.now(),
      }));

      // Act: Launch random file (should trigger directory loading)
      await service.launchRandomFile(deviceId);

      // Assert: Storage store should be called to navigate to parent directory
      expect(storageStoreMock.navigateToDirectory).toHaveBeenCalledWith({
        deviceId,
        storageType: StorageType.Sd,
        path: '/music/jazz',
      });

      // Assert: Directory state should be retrieved
      expect(storageStoreMock.getSelectedDirectoryState).toHaveBeenCalledWith(deviceId);

      // Assert: File context should be loaded with directory files
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.files).toHaveLength(3);
      expect(fileContext?.currentIndex).toBe(1); // random.sid is at index 1
      expect(fileContext?.directoryPath).toBe('/music/jazz');
      expect(fileContext?.launchMode).toBe(LaunchMode.Shuffle);
    });

    it('should handle directory loading failure gracefully', async () => {
      // Arrange: Random file launch succeeds but directory loading fails
      const deviceId = 'device-shuffle-fail';
      const randomFile = createFile({ 
        name: 'random.sid', 
        path: '/music/random.sid',
        parentPath: '/music',
      });
      
      playerServiceMock.launchRandom = vi.fn().mockReturnValue(of(randomFile));
      storageStoreMock.navigateToDirectory = vi.fn().mockRejectedValue(new Error('Directory not found'));

      // Act: Launch random file (directory loading will fail)
      await service.launchRandomFile(deviceId);

      // Assert: Random file launch should still succeed
      const currentFile = service.getCurrentFile(deviceId)();
      expect(currentFile?.file.name).toBe('random.sid');
      expect(currentFile?.launchMode).toBe(LaunchMode.Shuffle);

      // Assert: Should not have file context due to directory loading failure
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.files).toEqual([]); // Empty array from action fallback
    });

    it('should toggle shuffle mode correctly', () => {
      // Arrange: Device starts in Directory mode
      const deviceId = 'device-toggle';
      service.initializePlayer(deviceId);

      // Assert: Should start in Directory mode
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Directory);

      // Act: Toggle to Shuffle mode
      service.toggleShuffleMode(deviceId);

      // Assert: Should be in Shuffle mode
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Shuffle);

      // Act: Toggle back to Directory mode
      service.toggleShuffleMode(deviceId);

      // Assert: Should be back in Directory mode
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Directory);
    });

    it('should manage shuffle settings per device', () => {
      // Arrange: Two devices
      const deviceA = 'device-a';
      const deviceB = 'device-b';

      service.initializePlayer(deviceA);
      service.initializePlayer(deviceB);

      // Act: Set different shuffle settings for each device
      service.setShuffleScope(deviceA, PlayerScope.DirectoryDeep);
      service.setFilterMode(deviceA, PlayerFilterType.Music);

      service.setShuffleScope(deviceB, PlayerScope.DirectoryShallow);
      service.setFilterMode(deviceB, PlayerFilterType.Games);

      // Assert: Each device has independent settings
      const settingsA = service.getShuffleSettings(deviceA)();
      const settingsB = service.getShuffleSettings(deviceB)();

      expect(settingsA.scope).toBe(PlayerScope.DirectoryDeep);
      expect(settingsA.filter).toBe(PlayerFilterType.Music);

      expect(settingsB.scope).toBe(PlayerScope.DirectoryShallow);
      expect(settingsB.filter).toBe(PlayerFilterType.Games);
    });

    it('should use shuffle settings when launching random files', async () => {
      // Arrange: Set specific shuffle settings
      const deviceId = 'device-settings';
      const randomFile = createFile({ name: 'random.sid' });
      
      service.initializePlayer(deviceId);
      service.setShuffleScope(deviceId, PlayerScope.DirectoryDeep);
      service.setFilterMode(deviceId, PlayerFilterType.Games);

      playerServiceMock.launchRandom = vi.fn().mockReturnValue(of(randomFile));

      // Act: Launch random file
      await service.launchRandomFile(deviceId);

      // Assert: Should use configured shuffle settings
      expect(playerServiceMock.launchRandom).toHaveBeenCalledWith(
        deviceId,
        PlayerScope.DirectoryDeep,
        PlayerFilterType.Games,
        undefined
      );
    });
  });
});
