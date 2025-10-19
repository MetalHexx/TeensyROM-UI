import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import {
  FilesApiService,
  GetDirectoryResponse,
  SearchResponse,
  TeensyStorageType,
  StorageCacheDto,
  FileItemDto,
  FileItemType as ApiFileItemType,
  NullableOfTeensyFilterType,
} from '@teensyrom-nx/data-access/api-client';
import { StorageService } from './storage.service';
import { FileItemType, StorageDirectory, StorageType, PlayerFilterType, FileItem, ALERT_SERVICE, IAlertService } from '@teensyrom-nx/domain';

describe('StorageService', () => {
  let service: StorageService;
  let mockFilesApiService: {
    getDirectory: ReturnType<typeof vi.fn>;
    search: ReturnType<typeof vi.fn>;
    index: ReturnType<typeof vi.fn>;
    indexAll: ReturnType<typeof vi.fn>;
  };
  let mockAlertService: Partial<IAlertService>;

  beforeEach(() => {
    mockFilesApiService = {
      getDirectory: vi.fn(),
      search: vi.fn(),
      index: vi.fn(),
      indexAll: vi.fn(),
    };

    mockAlertService = {
      error: vi.fn(),
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        StorageService,
        { provide: FilesApiService, useValue: mockFilesApiService },
        { provide: ALERT_SERVICE, useValue: mockAlertService },
      ],
    });

    service = TestBed.inject(StorageService);
  });

  describe('getDirectory', () => {
    it('should return transformed StorageDirectory when API call succeeds', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const path = '/games';

      const mockResponse: GetDirectoryResponse = {
        storageItem: {
          directories: [
            { name: 'arcade', path: '/games/arcade' },
            { name: 'puzzle', path: '/games/puzzle' },
          ],
          files: [
            {
              name: 'test.prg',
              path: '/games/test.prg',
              size: 1024,
              isFavorite: false,
              isCompatible: true,
              title: 'Test Game',
              creator: 'Test Dev',
              releaseInfo: '2023',
              description: 'A test game',
              shareUrl: '',
              metadataSource: '',
              meta1: '',
              meta2: '',
              metadataSourcePath: '',
              parentPath: '/games',
              playLength: '',
              subtuneLengths: [],
              startSubtuneNum: 0,
              images: [],
              type: ApiFileItemType.Game,
              links: [],
              tags: [],
              youTubeVideos: [],
              competitions: [],
              ratingCount: 0,
            },
          ],
          path: '/games',
        } as StorageCacheDto,
        message: 'Success',
      };

      mockFilesApiService.getDirectory.mockResolvedValue(mockResponse);

      // Act
      const result = await new Promise<StorageDirectory>((resolve, reject) => {
        service.getDirectory(deviceId, storageType, path).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.getDirectory).toHaveBeenCalledWith({
        deviceId,
        storageType: TeensyStorageType.Sd, // API service expects API type
        path,
      });
      expect(result).toBeDefined();
      expect(result.path).toBe('/games');
      expect(result.directories).toHaveLength(2);
      expect(result.directories[0].name).toBe('arcade');
      expect(result.files).toHaveLength(1);
      expect(result.files[0].name).toBe('test.prg');
      expect(result.files[0].type).toBe(FileItemType.Game);
    });

    it('should handle API call without path parameter', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Usb;

      const mockResponse: GetDirectoryResponse = {
        storageItem: {
          directories: [],
          files: [],
          path: '/',
        } as StorageCacheDto,
        message: 'Success',
      };

      mockFilesApiService.getDirectory.mockResolvedValue(mockResponse);

      // Act
      const result = await new Promise<StorageDirectory>((resolve, reject) => {
        service.getDirectory(deviceId, storageType).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.getDirectory).toHaveBeenCalledWith({
        deviceId,
        storageType: TeensyStorageType.Usb, // API service expects API type
        path: undefined,
      });
      expect(result).toBeDefined();
      expect(result.path).toBe('/');
    });

    it('should throw error when response storageItem is missing', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const mockResponse: GetDirectoryResponse = {
        storageItem: null as unknown as StorageCacheDto,
        message: 'Error',
      };

      mockFilesApiService.getDirectory.mockResolvedValue(mockResponse);

      // Act & Assert
      await expect(
        new Promise((resolve, reject) => {
          service.getDirectory(deviceId, storageType).subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow('Invalid response: storageItem is missing');
    });

    it('should handle API errors gracefully', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const errorMessage = 'Network error';
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {
        // Suppress console.error during test
      });

      mockFilesApiService.getDirectory.mockRejectedValue(new Error(errorMessage));

      // Act & Assert
      await expect(
        new Promise((resolve, reject) => {
          service.getDirectory(deviceId, storageType).subscribe({
            error: reject,
          });
        })
      ).rejects.toThrow(errorMessage);

      // Verify error was logged with correct format (logError utility adds ❌ prefix)
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        '❌ StorageService.getDirectory error:',
        expect.any(Error)
      );

      // Cleanup
      consoleErrorSpy.mockRestore();
    });

    it('should pass correct parameters to API service', async () => {
      // Arrange
      const deviceId = 'device-123';
      const storageType = StorageType.Usb;
      const path = '/music/classical';

      const mockResponse: GetDirectoryResponse = {
        storageItem: {
          directories: [],
          files: [],
          path: path,
        } as StorageCacheDto,
        message: 'Success',
      };

      mockFilesApiService.getDirectory.mockResolvedValue(mockResponse);

      // Act
      await new Promise((resolve, reject) => {
        service.getDirectory(deviceId, storageType, path).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.getDirectory).toHaveBeenCalledTimes(1);
      expect(mockFilesApiService.getDirectory).toHaveBeenCalledWith({
        deviceId,
        storageType: TeensyStorageType.Usb, // API service expects API type
        path,
      });
    });
  });

  describe('search', () => {
    const createMockFileItemDto = (overrides?: Partial<FileItemDto>): FileItemDto => ({
      name: 'test.prg',
      path: '/games/test.prg',
      size: 1024,
      isFavorite: false,
      isCompatible: true,
      title: 'Test Game',
      creator: 'Test Dev',
      releaseInfo: '2023',
      description: 'A test game',
      shareUrl: '',
      metadataSource: '',
      meta1: '',
      meta2: '',
      metadataSourcePath: '',
      parentPath: '/games',
      playLength: '',
      subtuneLengths: [],
      startSubtuneNum: 0,
      images: [],
      type: ApiFileItemType.Game,
      links: [],
      tags: [],
      youTubeVideos: [],
      competitions: [],
      ratingCount: 0,
      ...overrides,
    });

    const createMockSearchResponse = (files: FileItemDto[]): SearchResponse => ({
      files,
      searchText: 'test',
      totalCount: files.length,
      count: files.length,
      skip: 0,
      take: 1000,
      hasMore: false,
      message: 'Success',
    });

    it('should return mapped FileItem array when search succeeds with multiple results', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const searchText = 'test';
      const filterType = PlayerFilterType.Games;

      const mockFiles: FileItemDto[] = [
        createMockFileItemDto({
          name: 'test-game-1.prg',
          path: '/games/test-game-1.prg',
          title: 'Test Game 1',
          parentPath: '/games',
        }),
        createMockFileItemDto({
          name: 'test-game-2.prg',
          path: '/action/test-game-2.prg',
          title: 'Test Game 2',
          parentPath: '/action',
        }),
      ];

      const mockResponse = createMockSearchResponse(mockFiles);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      const result = await new Promise<FileItem[]>((resolve, reject) => {
        service.search(deviceId, storageType, searchText, filterType).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.search).toHaveBeenCalledWith({
        deviceId,
        storageType: TeensyStorageType.Sd,
        searchText,
        skip: 0,
        take: 1000,
        filterType: NullableOfTeensyFilterType.Games,
      });
      expect(result).toHaveLength(2);
      expect(result[0].name).toBe('test-game-1.prg');
      expect(result[0].parentPath).toBe('/games');
      expect(result[1].name).toBe('test-game-2.prg');
      expect(result[1].parentPath).toBe('/action');
    });

    it('should return empty array when search has no results', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Usb;
      const searchText = 'nonexistent';

      const mockResponse = createMockSearchResponse([]);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      const result = await new Promise<FileItem[]>((resolve, reject) => {
        service.search(deviceId, storageType, searchText).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.search).toHaveBeenCalledWith({
        deviceId,
        storageType: TeensyStorageType.Usb,
        searchText,
        skip: 0,
        take: 1000,
        filterType: undefined,
      });
      expect(result).toEqual([]);
    });

    it('should work correctly without filterType parameter', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const searchText = 'music';

      const mockFiles: FileItemDto[] = [
        createMockFileItemDto({
          name: 'song.sid',
          type: ApiFileItemType.Song,
          title: 'Test Song',
        }),
      ];

      const mockResponse = createMockSearchResponse(mockFiles);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      const result = await new Promise<FileItem[]>((resolve, reject) => {
        service.search(deviceId, storageType, searchText).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.search).toHaveBeenCalledWith({
        deviceId,
        storageType: TeensyStorageType.Sd,
        searchText,
        skip: 0,
        take: 1000,
        filterType: undefined,
      });
      expect(result).toHaveLength(1);
      expect(result[0].type).toBe(FileItemType.Song);
    });

    it('should correctly map PlayerFilterType.All to API enum', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const searchText = 'all';
      const filterType = PlayerFilterType.All;

      const mockResponse = createMockSearchResponse([]);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      await new Promise((resolve, reject) => {
        service.search(deviceId, storageType, searchText, filterType).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.search).toHaveBeenCalledWith({
        deviceId,
        storageType: TeensyStorageType.Sd,
        searchText,
        skip: 0,
        take: 1000,
        filterType: NullableOfTeensyFilterType.All,
      });
    });

    it('should correctly map PlayerFilterType.Games to API enum', async () => {
      // Arrange
      const filterType = PlayerFilterType.Games;
      const mockResponse = createMockSearchResponse([]);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      await new Promise((resolve, reject) => {
        service.search('device', StorageType.Sd, 'test', filterType).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.search).toHaveBeenCalledWith(
        expect.objectContaining({ 
          skip: 0,
          take: 1000,
          filterType: NullableOfTeensyFilterType.Games 
        })
      );
    });

    it('should correctly map PlayerFilterType.Music to API enum', async () => {
      // Arrange
      const filterType = PlayerFilterType.Music;
      const mockResponse = createMockSearchResponse([]);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      await new Promise((resolve, reject) => {
        service.search('device', StorageType.Sd, 'test', filterType).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.search).toHaveBeenCalledWith(
        expect.objectContaining({ 
          skip: 0,
          take: 1000,
          filterType: NullableOfTeensyFilterType.Music 
        })
      );
    });

    it('should correctly map PlayerFilterType.Images to API enum', async () => {
      // Arrange
      const filterType = PlayerFilterType.Images;
      const mockResponse = createMockSearchResponse([]);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      await new Promise((resolve, reject) => {
        service.search('device', StorageType.Sd, 'test', filterType).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.search).toHaveBeenCalledWith(
        expect.objectContaining({ 
          skip: 0,
          take: 1000,
          filterType: NullableOfTeensyFilterType.Images 
        })
      );
    });

    it('should correctly map PlayerFilterType.Hex to API enum', async () => {
      // Arrange
      const filterType = PlayerFilterType.Hex;
      const mockResponse = createMockSearchResponse([]);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      await new Promise((resolve, reject) => {
        service.search('device', StorageType.Sd, 'test', filterType).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.search).toHaveBeenCalledWith(
        expect.objectContaining({ 
          skip: 0,
          take: 1000,
          filterType: NullableOfTeensyFilterType.Hex 
        })
      );
    });

    it('should handle network errors and propagate them', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const searchText = 'test';
      const errorMessage = 'Network error';
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {
        // Suppress console.error during test
      });

      mockFilesApiService.search.mockRejectedValue(new Error(errorMessage));

      // Act & Assert
      await expect(
        new Promise((resolve, reject) => {
          service.search(deviceId, storageType, searchText).subscribe({
            error: reject,
          });
        })
      ).rejects.toThrow(errorMessage);

      // Verify error was logged with correct format (logError utility adds ❌ prefix)
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        '❌ StorageService.search error:',
        expect.any(Error)
      );

      // Cleanup
      consoleErrorSpy.mockRestore();
    });

    it('should handle HTTP errors and propagate them', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const searchText = 'test';
      const httpError = { status: 500, message: 'Internal Server Error' };
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {
        // Suppress console.error during test
      });

      mockFilesApiService.search.mockRejectedValue(httpError);

      // Act & Assert
      await expect(
        new Promise((resolve, reject) => {
          service.search(deviceId, storageType, searchText).subscribe({
            error: reject,
          });
        })
      ).rejects.toEqual(httpError);

      // Verify error was logged with correct format (logError utility adds ❌ prefix)
      expect(consoleErrorSpy).toHaveBeenCalledWith(
        '❌ StorageService.search error:',
        httpError
      );

      // Cleanup
      consoleErrorSpy.mockRestore();
    });

    it('should return empty array when API returns null files array', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const searchText = 'test';

      const mockResponse: SearchResponse = {
        files: undefined as unknown as FileItemDto[],
        searchText: 'test',
        totalCount: 0,
        count: 0,
        skip: 0,
        take: 1000,
        hasMore: false,
        message: 'Success',
      };

      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      const result = await new Promise<FileItem[]>((resolve, reject) => {
        service.search(deviceId, storageType, searchText).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(result).toEqual([]);
    });

    it('should correctly map all FileItem properties', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const searchText = 'detailed';

      const mockFile: FileItemDto = createMockFileItemDto({
        name: 'detailed-game.prg',
        path: '/games/detailed-game.prg',
        size: 2048,
        isFavorite: true,
        isCompatible: true,
        title: 'Detailed Game',
        creator: 'Famous Dev',
        releaseInfo: '1984',
        description: 'A very detailed game',
        shareUrl: 'https://example.com/share',
        metadataSource: 'HVSC',
        parentPath: '/games',
        type: ApiFileItemType.Game,
      });

      const mockResponse = createMockSearchResponse([mockFile]);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      const result = await new Promise<FileItem[]>((resolve, reject) => {
        service.search(deviceId, storageType, searchText).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(result).toHaveLength(1);
      const fileItem = result[0];
      expect(fileItem.name).toBe('detailed-game.prg');
      expect(fileItem.path).toBe('/games/detailed-game.prg');
      expect(fileItem.size).toBe(2048);
      expect(fileItem.isFavorite).toBe(true);
      expect(fileItem.isCompatible).toBe(true);
      expect(fileItem.title).toBe('Detailed Game');
      expect(fileItem.creator).toBe('Famous Dev');
      expect(fileItem.releaseInfo).toBe('1984');
      expect(fileItem.description).toBe('A very detailed game');
      expect(fileItem.shareUrl).toBe('https://example.com/share');
      expect(fileItem.metadataSource).toBe('HVSC');
      expect(fileItem.parentPath).toBe('/games');
      expect(fileItem.type).toBe(FileItemType.Game);
    });

    it('should construct image URLs with base API URL', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const searchText = 'image';

      const mockFile: FileItemDto = createMockFileItemDto({
        name: 'game-with-image.prg',
        images: [
          {
            fileName: 'cover.png',
            path: '/images/cover.png',
            source: 'local',
            baseAssetPath: '/api/assets/images/cover.png',
          },
          {
            fileName: 'screenshot.png',
            path: '/images/screenshot.png',
            source: 'local',
            baseAssetPath: '/api/assets/images/screenshot.png',
          },
        ],
      });

      const mockResponse = createMockSearchResponse([mockFile]);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      const result = await new Promise<FileItem[]>((resolve, reject) => {
        service.search(deviceId, storageType, searchText).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(result).toHaveLength(1);
      expect(result[0].images).toHaveLength(2);
      expect(result[0].images[0].url).toBe('http://localhost:5168/api/assets/images/cover.png');
      expect(result[0].images[0].fileName).toBe('cover.png');
      expect(result[0].images[1].url).toBe('http://localhost:5168/api/assets/images/screenshot.png');
      expect(result[0].images[1].fileName).toBe('screenshot.png');
    });

    it('should preserve parentPath for directory context', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = StorageType.Sd;
      const searchText = 'game';

      const mockFiles: FileItemDto[] = [
        createMockFileItemDto({
          name: 'game1.prg',
          path: '/action/platform/game1.prg',
          parentPath: '/action/platform',
        }),
        createMockFileItemDto({
          name: 'game2.prg',
          path: '/puzzle/game2.prg',
          parentPath: '/puzzle',
        }),
        createMockFileItemDto({
          name: 'game3.prg',
          path: '/game3.prg',
          parentPath: '/',
        }),
      ];

      const mockResponse = createMockSearchResponse(mockFiles);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act
      const result = await new Promise<FileItem[]>((resolve, reject) => {
        service.search(deviceId, storageType, searchText).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(result).toHaveLength(3);
      expect(result[0].parentPath).toBe('/action/platform');
      expect(result[1].parentPath).toBe('/puzzle');
      expect(result[2].parentPath).toBe('/');
    });

    it('should correctly convert StorageType to API type', async () => {
      // Arrange
      const mockResponse = createMockSearchResponse([]);
      mockFilesApiService.search.mockResolvedValue(mockResponse);

      // Act - Test SD storage type
      await new Promise((resolve, reject) => {
        service.search('device', StorageType.Sd, 'test').subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.search).toHaveBeenCalledWith(
        expect.objectContaining({ 
          skip: 0,
          take: 1000,
          storageType: TeensyStorageType.Sd 
        })
      );

      // Act - Test USB storage type
      await new Promise((resolve, reject) => {
        service.search('device', StorageType.Usb, 'test').subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.search).toHaveBeenCalledWith(
        expect.objectContaining({ 
          skip: 0,
          take: 1000,
          storageType: TeensyStorageType.Usb 
        })
      );
    });
  });

  describe('Alert Integration - Error Handling', () => {
    let mockAlertService: { error: ReturnType<typeof vi.fn> };
    let logSpy: ReturnType<typeof vi.spyOn>;

    beforeEach(() => {
      mockAlertService = {
        error: vi.fn(),
      };
      
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        providers: [
          StorageService,
          { provide: FilesApiService, useValue: mockFilesApiService },
          { provide: ALERT_SERVICE, useValue: mockAlertService },
        ],
      });
      
      service = TestBed.inject(StorageService);
      logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);
    });

    afterEach(() => {
      logSpy.mockRestore();
    });

    describe('getDirectory error handling with alerts', () => {
      it('should display error alert when API fails with error message', async () => {
        const error = new Error('Directory not found');
        mockFilesApiService.getDirectory.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service.getDirectory('device-1', StorageType.Sd, '/path').subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Directory not found');
      });

      it('should extract message from error.error.message for getDirectory', async () => {
        // Test that non-Error objects with nested structure use fallback message
        const error = { error: { message: 'Storage device offline' } };
        mockFilesApiService.getDirectory.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service.getDirectory('device-1', StorageType.Sd).subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        // Non-Error objects use fallback message
        expect(mockAlertService.error).toHaveBeenCalledWith('Failed to retrieve directory');
      });

      it('should use fallback message when no error message available for getDirectory', async () => {
        const error = {};
        mockFilesApiService.getDirectory.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service.getDirectory('device-1', StorageType.Sd).subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Failed to retrieve directory');
      });

      it('should rethrow error after displaying alert for getDirectory', async () => {
        const error = new Error('Storage error');
        mockFilesApiService.getDirectory.mockRejectedValue(error);

        let caughtError: Error | null = null;
        await new Promise<void>((resolve) => {
          service.getDirectory('device-1', StorageType.Sd).subscribe({
            error: (err: Error) => {
              caughtError = err;
              resolve();
            },
          });
        });

        expect(caughtError).toBeDefined();
        expect(caughtError).toBe(error);
      });
    });

    describe('search error handling with alerts', () => {
      it('should display error alert when search fails', async () => {
        const error = new Error('Search timeout');
        mockFilesApiService.search.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service
              .search('device-1', StorageType.Sd, 'test', PlayerFilterType.Music)
              .subscribe({
                next: resolve,
                error: reject,
              });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Search timeout');
      });

      it('should use search fallback message when error is empty', async () => {
        const error = { error: {} };
        mockFilesApiService.search.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service.search('device-1', StorageType.Sd, 'term').subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Search operation failed');
      });

      it('should rethrow error after displaying alert for search', async () => {
        const error = new Error('Query error');
        mockFilesApiService.search.mockRejectedValue(error);

        let caughtError: Error | null = null;
        await new Promise<void>((resolve) => {
          service.search('device-1', StorageType.Sd, 'query').subscribe({
            error: (err: Error) => {
              caughtError = err;
              resolve();
            },
          });
        });

        expect(caughtError).toBeDefined();
        expect(caughtError).toBe(error);
      });
    });

    describe('index error handling with alerts', () => {
      it('should display error alert when index fails', async () => {
        const error = new Error('Index operation failed');
        mockFilesApiService.index.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service.index('device-1', StorageType.Sd).subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Index operation failed');
      });

      it('should use index fallback message when error is empty', async () => {
        const error = {};
        mockFilesApiService.index.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service.index('device-1', StorageType.Sd).subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Failed to index storage');
      });
    });

    describe('indexAll error handling with alerts', () => {
      it('should display error alert when indexAll fails', async () => {
        const error = new Error('Global index failed');
        mockFilesApiService.indexAll.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service.indexAll().subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Global index failed');
      });

      it('should use indexAll fallback message when error is empty', async () => {
        const error = { error: { message: null } };
        mockFilesApiService.indexAll.mockRejectedValue(error);

        await expect(
          new Promise((resolve, reject) => {
            service.indexAll().subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledWith('Failed to index all storage');
      });
    });

    describe('Alert service called exactly once', () => {
      it('should call alert service once per getDirectory error', async () => {
        mockFilesApiService.getDirectory.mockRejectedValue(new Error('Test'));

        await expect(
          new Promise((resolve, reject) => {
            service.getDirectory('device-1', StorageType.Sd).subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledTimes(1);
      });

      it('should call alert service once per search error', async () => {
        mockFilesApiService.search.mockRejectedValue(new Error('Test'));

        await expect(
          new Promise((resolve, reject) => {
            service.search('device-1', StorageType.Sd, 'test').subscribe({
              next: resolve,
              error: reject,
            });
          })
        ).rejects.toThrow();

        expect(mockAlertService.error).toHaveBeenCalledTimes(1);
      });
    });
  });
});
