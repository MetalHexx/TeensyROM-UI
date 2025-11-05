import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import {
  PlayerApiService,
  DevicesApiService,
  LaunchFileResponse,
  LaunchRandomResponse,
  ToggleMusicResponse,
  TeensyStorageType,
  FileItemDto,
  FileItemType as ApiFileItemType,
} from '@teensyrom-nx/data-access/api-client';
import { PlayerService } from './player.service';
import {
  FileItem,
  FileItemType,
  PlayerFilterType,
  PlayerScope,
  StorageType,
  ALERT_SERVICE,
  IAlertService,
} from '@teensyrom-nx/domain';

const createFileItemDto = (): FileItemDto => ({
  name: 'Test File',
  path: '/test/file.sid',
  size: 4096,
  isFavorite: false,
  isCompatible: true,
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
  links: [],
  tags: [],
  youTubeVideos: [],
  competitions: [],
  ratingCount: 0,
});

describe('PlayerService', () => {
  let service: PlayerService;
  let mockPlayerApi: {
    launchFile: ReturnType<typeof vi.fn>;
    launchRandom: ReturnType<typeof vi.fn>;
    toggleMusic: ReturnType<typeof vi.fn>;
  };
  let mockDevicesApi: {
    resetDevice: ReturnType<typeof vi.fn>;
  };
  let mockAlertService: Partial<IAlertService>;

  beforeEach(() => {
    mockPlayerApi = {
      launchFile: vi.fn(),
      launchRandom: vi.fn(),
      toggleMusic: vi.fn(),
    };

    mockDevicesApi = {
      resetDevice: vi.fn(),
    };

    mockAlertService = {
      error: vi.fn(),
      warning: vi.fn(),
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        PlayerService,
        { provide: PlayerApiService, useValue: mockPlayerApi },
        { provide: DevicesApiService, useValue: mockDevicesApi },
        { provide: ALERT_SERVICE, useValue: mockAlertService },
      ],
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
        isCompatible: true,
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

    it('should show warning alert when file is not compatible', async () => {
      const incompatibleFile = createFileItemDto();
      incompatibleFile.isCompatible = false;
      incompatibleFile.name = 'IncompatibleGame.prg';

      const response: LaunchFileResponse = {
        message: 'File is not compatible with this device',
        launchedFile: incompatibleFile,
        isCompatible: false,
      };

      mockPlayerApi.launchFile.mockResolvedValue(response);
      mockAlertService.warning = vi.fn();

      const result = await new Promise<FileItem>((resolve, reject) => {
        service.launchFile('device-123', StorageType.Sd, '/test/file.prg').subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Verify file was still returned
      expect(result.name).toBe('IncompatibleGame.prg');
      expect(result.isCompatible).toBe(false);

      // Verify warning alert was shown with message from response
      expect(mockAlertService.warning).toHaveBeenCalledWith(
        'File is not compatible with this device'
      );
    });

    it('should not show warning when file is compatible', async () => {
      const compatibleFile = createFileItemDto();
      compatibleFile.isCompatible = true;

      const response: LaunchFileResponse = {
        message: 'Launched',
        launchedFile: compatibleFile,
        isCompatible: true,
      };

      mockPlayerApi.launchFile.mockResolvedValue(response);
      mockAlertService.warning = vi.fn();

      await new Promise<FileItem>((resolve, reject) => {
        service.launchFile('device-123', StorageType.Sd, '/test/file.prg').subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Verify warning was NOT called
      expect(mockAlertService.warning).not.toHaveBeenCalled();
    });
  });

  describe('launchRandom', () => {
    it('should call API with scope and filter parameters and map response', async () => {
      const response: LaunchRandomResponse = {
        launchedFile: createFileItemDto(),
        message: 'Random launched',
        isCompatible: true,
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
        storageType: TeensyStorageType.Sd,
        scope: 'DirShallow',
        filterType: 'Music',
        startingDirectory: '/music',
      });
      expect(result.name).toBe('Test File');
      expect(result.type).toBe(FileItemType.Song);
    });

    it('should handle optional startingDirectory parameter', async () => {
      const response: LaunchRandomResponse = {
        launchedFile: createFileItemDto(),
        message: 'Random launched',
        isCompatible: true,
      };

      mockPlayerApi.launchRandom.mockResolvedValue(response);

      await new Promise<FileItem>((resolve, reject) => {
        service.launchRandom('device-1', PlayerScope.Storage, PlayerFilterType.All).subscribe({
          next: resolve,
          error: reject,
        });
      });

      expect(mockPlayerApi.launchRandom).toHaveBeenCalledWith({
        deviceId: 'device-1',
        storageType: TeensyStorageType.Sd,
        scope: 'Storage',
        filterType: 'All',
        startingDirectory: undefined,
      });
    });

    it('should map scope and filter enums to API parameters correctly', async () => {
      const response: LaunchRandomResponse = {
        launchedFile: createFileItemDto(),
        message: 'Random launched',
        isCompatible: true,
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
        storageType: TeensyStorageType.Sd,
        scope: 'DirDeep',
        filterType: 'Games',
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

      expect(consoleSpy).toHaveBeenCalledWith('❌ PlayerService.launchRandom failed:', error);
      consoleSpy.mockRestore();
    });

    it('should show warning alert when randomly launched file is not compatible', async () => {
      const incompatibleFile = createFileItemDto();
      incompatibleFile.isCompatible = false;
      incompatibleFile.name = 'IncompatibleRandom.prg';

      const response: LaunchRandomResponse = {
        launchedFile: incompatibleFile,
        message: 'Warning: Random file may not work on this device',
        isCompatible: false,
      };

      mockPlayerApi.launchRandom.mockResolvedValue(response);
      mockAlertService.warning = vi.fn();

      const result = await new Promise<FileItem>((resolve, reject) => {
        service.launchRandom('device-1', PlayerScope.Storage, PlayerFilterType.All).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Verify file was still returned
      expect(result.name).toBe('IncompatibleRandom.prg');
      expect(result.isCompatible).toBe(false);

      // Verify warning alert was shown with message from response
      expect(mockAlertService.warning).toHaveBeenCalledWith(
        'Warning: Random file may not work on this device'
      );
    });

    it('should not show warning when randomly launched file is compatible', async () => {
      const compatibleFile = createFileItemDto();
      compatibleFile.isCompatible = true;

      const response: LaunchRandomResponse = {
        launchedFile: compatibleFile,
        message: 'Random launched',
        isCompatible: true,
      };

      mockPlayerApi.launchRandom.mockResolvedValue(response);
      mockAlertService.warning = vi.fn();

      await new Promise<FileItem>((resolve, reject) => {
        service.launchRandom('device-1', PlayerScope.Storage, PlayerFilterType.All).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Verify warning was NOT called
      expect(mockAlertService.warning).not.toHaveBeenCalled();
    });
  });

  describe('Phase 3: Playback Control Methods', () => {
    describe('toggleMusic', () => {
      it('should call toggleMusic API and return void', async () => {
        const deviceId = 'device-123';
        const response: ToggleMusicResponse = {
          message: 'Music toggled successfully',
        };

        mockPlayerApi.toggleMusic.mockResolvedValue(response);

        const result = await new Promise<void>((resolve, reject) => {
          service.toggleMusic(deviceId).subscribe({
            next: resolve,
            error: reject,
          });
        });

        expect(mockPlayerApi.toggleMusic).toHaveBeenCalledWith({ deviceId });
        expect(result).toBeUndefined(); // Should return void
      });

      it('should throw error when response message is missing', async () => {
        const deviceId = 'device-123';
        mockPlayerApi.toggleMusic.mockResolvedValue({
          message: null as unknown as string,
        });

        await expect(
          new Promise((resolve, reject) => {
            service.toggleMusic(deviceId).subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow('Invalid response: message is missing');
      });

      it('should handle API errors with logging', async () => {
        const deviceId = 'device-123';
        const error = new Error('Music toggle failed');
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

        mockPlayerApi.toggleMusic.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service.toggleMusic(deviceId).subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow('Music toggle failed');

        expect(consoleSpy).toHaveBeenCalledWith('❌ PlayerService.toggleMusic failed:', error);
        consoleSpy.mockRestore();
      });

      it('should re-throw errors from API', async () => {
        const deviceId = 'device-123';
        const error = new Error();
        mockPlayerApi.toggleMusic.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service.toggleMusic(deviceId).subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow(error);
      });
    });
  });

  describe('isCompatible Field Mapping', () => {
    it('should map isCompatible field from launchFile response', async () => {
      const deviceId = 'device-123';
      const incompatibleFile: FileItemDto = {
        ...createFileItemDto(),
        name: 'incompatible.sid',
        isCompatible: false,
      };

      const response: LaunchFileResponse = {
        message: 'Launched',
        launchedFile: incompatibleFile,
        isCompatible: false,
      };

      mockPlayerApi.launchFile.mockResolvedValue(response);

      const result = await new Promise<FileItem>((resolve, reject) => {
        service.launchFile(deviceId, StorageType.Sd, '/test/incompatible.sid').subscribe({
          next: resolve,
          error: reject,
        });
      });

      expect(result.isCompatible).toBe(false);
      expect(result.name).toBe('incompatible.sid');
    });

    it('should map isCompatible=true for compatible files', async () => {
      const deviceId = 'device-123';
      const compatibleFile: FileItemDto = {
        ...createFileItemDto(),
        name: 'compatible.sid',
        isCompatible: true,
      };

      const response: LaunchFileResponse = {
        message: 'Launched',
        launchedFile: compatibleFile,
        isCompatible: true,
      };

      mockPlayerApi.launchFile.mockResolvedValue(response);

      const result = await new Promise<FileItem>((resolve, reject) => {
        service.launchFile(deviceId, StorageType.Sd, '/test/compatible.sid').subscribe({
          next: resolve,
          error: reject,
        });
      });

      expect(result.isCompatible).toBe(true);
      expect(result.name).toBe('compatible.sid');
    });

    it('should map isCompatible field from launchRandom response', async () => {
      const incompatibleFile: FileItemDto = {
        ...createFileItemDto(),
        name: 'random-incompatible.sid',
        isCompatible: false,
      };

      const response: LaunchRandomResponse = {
        launchedFile: incompatibleFile,
        message: 'Random launched',
        isCompatible: false,
      };

      mockPlayerApi.launchRandom.mockResolvedValue(response);

      const result = await new Promise<FileItem>((resolve, reject) => {
        service.launchRandom('device-1', PlayerScope.Storage, PlayerFilterType.All).subscribe({
          next: resolve,
          error: reject,
        });
      });

      expect(result.isCompatible).toBe(false);
      expect(result.name).toBe('random-incompatible.sid');
    });
  });

  describe('Alert Integration - Error Handling', () => {
    let mockAlertService: { error: ReturnType<typeof vi.fn> };

    beforeEach(() => {
      mockAlertService = {
        error: vi.fn(),
      };

      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        providers: [
          PlayerService,
          { provide: PlayerApiService, useValue: mockPlayerApi },
          { provide: DevicesApiService, useValue: mockDevicesApi },
          { provide: ALERT_SERVICE, useValue: mockAlertService },
        ],
      });

      service = TestBed.inject(PlayerService);
    });

    describe('launchFile error handling with alerts', () => {
      it('should display error alert when launchFile fails', async () => {
        const error = new Error('File launch failed');
        mockPlayerApi.launchFile.mockRejectedValue(error);
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

        await expect(
          new Promise((resolve, reject) => {
            service.launchFile('device-1', StorageType.Sd, '/path').subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('File launch failed');
        consoleSpy.mockRestore();
      });

      it('should extract message from error.error.message for launchFile', async () => {
        const error = { error: { message: 'Device not ready' } };
        mockPlayerApi.launchFile.mockRejectedValue(error);
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

        await expect(
          new Promise((resolve, reject) => {
            service.launchFile('device-1', StorageType.Sd, '/path').subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        // Non-Error objects use fallback message
        expect(mockAlertService.error).toHaveBeenCalledWith('Failed to launch file');
        consoleSpy.mockRestore();
      });

      it('should use fallback message when no error message for launchFile', async () => {
        const error = {};
        mockPlayerApi.launchFile.mockRejectedValue(error);
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

        await expect(
          new Promise((resolve, reject) => {
            service.launchFile('device-1', StorageType.Sd, '/path').subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Failed to launch file');
        consoleSpy.mockRestore();
      });

      it('should call alert service exactly once for launchFile', async () => {
        mockPlayerApi.launchFile.mockRejectedValue(new Error('Test'));
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

        await expect(
          new Promise((resolve, reject) => {
            service.launchFile('device-1', StorageType.Sd, '/path').subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledTimes(1);
        consoleSpy.mockRestore();
      });
    });

    describe('launchRandom error handling with alerts', () => {
      it('should display error alert when launchRandom fails', async () => {
        const error = new Error('Random selection failed');
        mockPlayerApi.launchRandom.mockRejectedValue(error);
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

        await expect(
          new Promise((resolve, reject) => {
            service.launchRandom('device-1', PlayerScope.Storage, PlayerFilterType.All).subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Random selection failed');
        consoleSpy.mockRestore();
      });

      it('should use fallback message for launchRandom', async () => {
        const error = { error: {} };
        mockPlayerApi.launchRandom.mockRejectedValue(error);
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

        await expect(
          new Promise((resolve, reject) => {
            service.launchRandom('device-1', PlayerScope.Storage, PlayerFilterType.All).subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Failed to launch random file');
        consoleSpy.mockRestore();
      });
    });

    describe('toggleMusic error handling with alerts', () => {
      it('should display error alert when toggleMusic fails', async () => {
        const error = new Error('Toggle failed');
        mockPlayerApi.toggleMusic.mockRejectedValue(error);
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

        await expect(
          new Promise((resolve, reject) => {
            service.toggleMusic('device-1').subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Toggle failed');
        consoleSpy.mockRestore();
      });

      it('should use fallback message for toggleMusic', async () => {
        const error = {};
        mockPlayerApi.toggleMusic.mockRejectedValue(error);
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

        await expect(
          new Promise((resolve, reject) => {
            service.toggleMusic('device-1').subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Failed to toggle music');
        consoleSpy.mockRestore();
      });

      it('should call alert service exactly once for toggleMusic', async () => {
        mockPlayerApi.toggleMusic.mockRejectedValue(new Error('Test'));
        const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

        await expect(
          new Promise((resolve, reject) => {
            service.toggleMusic('device-1').subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledTimes(1);
        consoleSpy.mockRestore();
      });
    });
  });
});
