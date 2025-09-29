import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import {
  PlayerApiService,
  LaunchFileResponse,
  LaunchRandomResponse,
  TeensyStorageType,
  FileItemDto,
  FileItemType as ApiFileItemType,
} from '@teensyrom-nx/data-access/api-client';
import { PlayerService } from './player.service';
import { FileItem, FileItemType, PlayerFilterType, PlayerScope, StorageType } from '@teensyrom-nx/domain';

const createFileItemDto = (): FileItemDto => ({
  name: 'Test File',
  path: '/test/file.sid',
  size: 4096,
  isFavorite: false,
  title: 'Test File',
  creator: 'Test Creator',
  releaseInfo: '2025',
  description: 'Test description',
  shareUrl: '',
  metadataSource: '',
  meta1: '',
  meta2: '',
  metadataSourcePath: '',
  parentPath: '/test',
  playLength: '',
  subtuneLengths: [],
  startSubtuneNum: 0,
  images: [],
  type: ApiFileItemType.Song,
});

describe('PlayerService', () => {
  let service: PlayerService;
  let mockPlayerApi: {
    launchFile: ReturnType<typeof vi.fn>;
    launchRandom: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    mockPlayerApi = {
      launchFile: vi.fn(),
      launchRandom: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [PlayerService, { provide: PlayerApiService, useValue: mockPlayerApi }],
    });

    service = TestBed.inject(PlayerService);
  });

  describe('launchFile', () => {
    it('should call API and map response to FileItem', async () => {
      const deviceId = 'device-123';
      const storageType = StorageType.Sd;
      const filePath = '/test/file.sid';

      const response: LaunchFileResponse = {
        message: 'Launched',
        launchedFile: createFileItemDto(),
      };

      mockPlayerApi.launchFile.mockResolvedValue(response);

      const result = await new Promise<FileItem>((resolve, reject) => {
        service.launchFile(deviceId, storageType, filePath).subscribe({
          next: resolve,
          error: reject,
        });
      });

      expect(mockPlayerApi.launchFile).toHaveBeenCalledWith({
        deviceId,
        storageType: TeensyStorageType.Sd,
        filePath,
      });
      expect(result.name).toBe('Test File');
      expect(result.type).toBe(FileItemType.Song);
    });

    it('should throw error when launchedFile is missing', async () => {
      mockPlayerApi.launchFile.mockResolvedValue({
        message: 'Invalid',
        launchedFile: null as unknown as FileItemDto,
      });

      await expect(
        new Promise((resolve, reject) => {
          service.launchFile('device-1', StorageType.Sd, '/path').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow('Invalid response: launchedFile is missing');
    });

    it('should propagate API errors with additional logging', async () => {
      const error = new Error('network issue');
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
      mockPlayerApi.launchFile.mockRejectedValue(error);

      await expect(
        new Promise((resolve, reject) => {
          service.launchFile('device-1', StorageType.Sd, '/path').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow('network issue');

      consoleSpy.mockRestore();
    });
  });

  describe('launchRandom', () => {
    it('should call API with scope and filter parameters and map response', async () => {
      const response: LaunchRandomResponse = {
        launchedFile: createFileItemDto(),
        message: 'Random launched',
      };

      mockPlayerApi.launchRandom.mockResolvedValue(response);

      const result = await new Promise<FileItem>((resolve, reject) => {
        service
          .launchRandom('device-9', PlayerScope.DirectoryShallow, PlayerFilterType.Music, '/music')
          .subscribe({
            next: resolve,
            error: reject,
          });
      });

      expect(mockPlayerApi.launchRandom).toHaveBeenCalledWith({
        deviceId: 'device-9',
        scope: 'DIRECTORY_SHALLOW',
        filter: 'MUSIC',
        startingDirectory: '/music',
      });
      expect(result.name).toBe('Test File');
      expect(result.type).toBe(FileItemType.Song);
    });

    it('should handle optional startingDirectory parameter', async () => {
      const response: LaunchRandomResponse = {
        launchedFile: createFileItemDto(),
        message: 'Random launched',
      };

      mockPlayerApi.launchRandom.mockResolvedValue(response);

      await new Promise<FileItem>((resolve, reject) => {
        service
          .launchRandom('device-1', PlayerScope.Storage, PlayerFilterType.All)
          .subscribe({
            next: resolve,
            error: reject,
          });
      });

      expect(mockPlayerApi.launchRandom).toHaveBeenCalledWith({
        deviceId: 'device-1',
        scope: 'STORAGE',
        filter: 'ALL',
        startingDirectory: undefined,
      });
    });

    it('should map scope and filter enums to API parameters correctly', async () => {
      const response: LaunchRandomResponse = {
        launchedFile: createFileItemDto(),
        message: 'Random launched',
      };

      mockPlayerApi.launchRandom.mockResolvedValue(response);

      // Test all enum mappings
      await new Promise<FileItem>((resolve, reject) => {
        service
          .launchRandom('device-1', PlayerScope.DirectoryDeep, PlayerFilterType.Games)
          .subscribe({
            next: resolve,
            error: reject,
          });
      });

      expect(mockPlayerApi.launchRandom).toHaveBeenCalledWith({
        deviceId: 'device-1',
        scope: 'DIRECTORY_DEEP',
        filter: 'GAMES',
        startingDirectory: undefined,
      });
    });

    it('should throw error when launchedFile is missing from random response', async () => {
      mockPlayerApi.launchRandom.mockResolvedValue({
        message: 'Invalid random response',
        launchedFile: null as unknown as FileItemDto,
      });

      await expect(
        new Promise((resolve, reject) => {
          service.launchRandom('device-1', PlayerScope.Storage, PlayerFilterType.All).subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow('Invalid response: launchedFile is missing');
    });

    it('should propagate API errors for random launch with proper logging', async () => {
      const error = new Error('random selection failed');
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
      mockPlayerApi.launchRandom.mockRejectedValue(error);

      await expect(
        new Promise((resolve, reject) => {
          service.launchRandom('device-1', PlayerScope.Storage, PlayerFilterType.All).subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow('random selection failed');

      expect(consoleSpy).toHaveBeenCalledWith('PlayerService launchRandom failed:', error);
      consoleSpy.mockRestore();
    });
  });
});
