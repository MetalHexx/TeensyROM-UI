import { describe, it, expect, beforeAll, afterEach, afterAll } from 'vitest';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import { HttpClient, HttpXhrBackend } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import {
  FilesApiService,
  Configuration,
  TeensyStorageType,
  GetDirectoryResponse,
  StorageCacheDto,
  FileItemType as ApiFileItemType,
} from '@teensyrom-nx/data-access/api-client';
import { StorageService } from './storage.service';
import { FileItemType } from './storage.models';

describe('StorageService Integration Tests', () => {
  let storageService: StorageService;

  // MSW server setup
  const server = setupServer();

  beforeAll(() => {
    server.listen({ onUnhandledRequest: 'error' });
  });

  afterEach(() => {
    server.resetHandlers();
  });

  afterAll(() => {
    server.close();
  });

  beforeAll(() => {
    // Create real HttpClient with XMLHttpRequest backend for MSW interception
    const httpHandler = new HttpXhrBackend({ build: () => new XMLHttpRequest() });
    const httpClient = new HttpClient(httpHandler);
    const config = new Configuration({ basePath: 'http://localhost:5168' });

    const filesApiService = new FilesApiService(httpClient, config.basePath || '', config);
    storageService = new StorageService(filesApiService);
  });

  describe('getDirectory', () => {
    it('should successfully fetch and transform directory data', async () => {
      // Arrange
      const deviceId = 'test-device-123';
      const storageType = TeensyStorageType.Sd;
      const path = '/games';

      const mockStorageCache: StorageCacheDto = {
        directories: [
          { name: 'arcade', path: '/games/arcade' },
          { name: 'puzzle', path: '/games/puzzle' },
        ],
        files: [
          {
            name: 'pacman.prg',
            path: '/games/pacman.prg',
            size: 2048,
            isFavorite: true,
            title: 'Pac-Man',
            creator: 'Namco',
            releaseInfo: '1980',
            description: 'Classic arcade game',
            shareUrl: 'https://example.com/pacman',
            metadataSource: 'IGDB',
            meta1: 'Arcade',
            meta2: 'Action',
            metadataSourcePath: '/metadata/pacman',
            parentPath: '/games',
            playLength: '0:00',
            subtuneLengths: [],
            startSubtuneNum: 0,
            images: [
              {
                fileName: 'pacman.png',
                path: '/images/pacman.png',
                source: 'local',
              },
            ],
            type: ApiFileItemType.Game,
          },
        ],
        path: '/games',
      };

      const mockResponse: GetDirectoryResponse = {
        storageItem: mockStorageCache,
        message: 'Directory retrieved successfully',
      };

      server.use(
        http.get(
          `http://localhost:5168/devices/${deviceId}/storage/${storageType}/directories`,
          ({ request }) => {
            const url = new URL(request.url);
            const pathParam = url.searchParams.get('Path');
            expect(pathParam).toBe(path);
            return HttpResponse.json(mockResponse);
          }
        )
      );

      // Act
      const result = await firstValueFrom(storageService.getDirectory(deviceId, storageType, path));

      // Assert
      expect(result).toBeDefined();
      expect(result.path).toBe('/games');

      // Test directory transformation
      expect(result.directories).toHaveLength(2);
      expect(result.directories[0].name).toBe('arcade');
      expect(result.directories[0].path).toBe('/games/arcade');
      expect(result.directories[1].name).toBe('puzzle');
      expect(result.directories[1].path).toBe('/games/puzzle');

      // Test file transformation
      expect(result.files).toHaveLength(1);
      const file = result.files[0];
      expect(file.name).toBe('pacman.prg');
      expect(file.path).toBe('/games/pacman.prg');
      expect(file.size).toBe(2048);
      expect(file.isFavorite).toBe(true);
      expect(file.title).toBe('Pac-Man');
      expect(file.creator).toBe('Namco');
      expect(file.type).toBe(FileItemType.Game);
      expect(file.images).toHaveLength(1);
      expect(file.images[0].fileName).toBe('pacman.png');
    });

    it('should handle requests without path parameter', async () => {
      // Arrange
      const deviceId = 'test-device-456';
      const storageType = TeensyStorageType.Usb;

      const mockResponse: GetDirectoryResponse = {
        storageItem: {
          directories: [{ name: 'root-dir', path: '/root-dir' }],
          files: [],
          path: '/',
        },
        message: 'Root directory retrieved',
      };

      server.use(
        http.get(
          `http://localhost:5168/devices/${deviceId}/storage/${storageType}/directories`,
          ({ request }) => {
            const url = new URL(request.url);
            const pathParam = url.searchParams.get('Path');
            expect(pathParam).toBeNull();
            return HttpResponse.json(mockResponse);
          }
        )
      );

      // Act
      const result = await firstValueFrom(storageService.getDirectory(deviceId, storageType));

      // Assert
      expect(result.path).toBe('/');
      expect(result.directories).toHaveLength(1);
      expect(result.directories[0].name).toBe('root-dir');
      expect(result.files).toHaveLength(0);
    });

    it('should handle API errors gracefully', async () => {
      // Arrange
      const deviceId = 'invalid-device';
      const storageType = TeensyStorageType.Sd;

      server.use(
        http.get(
          `http://localhost:5168/devices/${deviceId}/storage/${storageType}/directories`,
          () => {
            return HttpResponse.json(
              {
                title: 'Device not found',
                detail: 'The specified device was not found',
                status: 404,
              },
              { status: 404 }
            );
          }
        )
      );

      // Act & Assert
      await expect(
        firstValueFrom(storageService.getDirectory(deviceId, storageType))
      ).rejects.toThrow();
    });

    it('should handle empty directories and files arrays', async () => {
      // Arrange
      const deviceId = 'empty-device';
      const storageType = TeensyStorageType.Sd;

      const mockResponse: GetDirectoryResponse = {
        storageItem: {
          directories: [],
          files: [],
          path: '/empty',
        },
        message: 'Empty directory',
      };

      server.use(
        http.get(
          `http://localhost:5168/devices/${deviceId}/storage/${storageType}/directories`,
          () => HttpResponse.json(mockResponse)
        )
      );

      // Act
      const result = await firstValueFrom(storageService.getDirectory(deviceId, storageType));

      // Assert
      expect(result.directories).toEqual([]);
      expect(result.files).toEqual([]);
      expect(result.path).toBe('/empty');
    });

    it('should handle malformed API response', async () => {
      // Arrange
      const deviceId = 'malformed-device';
      const storageType = TeensyStorageType.Usb;

      const mockResponse: GetDirectoryResponse = {
        storageItem: null as any,
        message: 'Malformed response',
      };

      server.use(
        http.get(
          `http://localhost:5168/devices/${deviceId}/storage/${storageType}/directories`,
          () => HttpResponse.json(mockResponse)
        )
      );

      // Act & Assert
      await expect(
        firstValueFrom(storageService.getDirectory(deviceId, storageType))
      ).rejects.toThrow('Invalid response: storageItem is missing');
    });

    it('should work with different storage types', async () => {
      // Arrange
      const deviceId = 'multi-storage-device';

      const createMockResponse = (storageType: string): GetDirectoryResponse => ({
        storageItem: {
          directories: [{ name: `${storageType}-dir`, path: `/${storageType}-dir` }],
          files: [],
          path: '/',
        },
        message: `${storageType} directory retrieved`,
      });

      // Setup handlers for both storage types
      server.use(
        http.get(`http://localhost:5168/devices/${deviceId}/storage/SD/directories`, () =>
          HttpResponse.json(createMockResponse('sd'))
        ),
        http.get(`http://localhost:5168/devices/${deviceId}/storage/USB/directories`, () =>
          HttpResponse.json(createMockResponse('usb'))
        )
      );

      // Act - Test SD storage
      const sdResult = await firstValueFrom(
        storageService.getDirectory(deviceId, TeensyStorageType.Sd)
      );

      // Act - Test USB storage
      const usbResult = await firstValueFrom(
        storageService.getDirectory(deviceId, TeensyStorageType.Usb)
      );

      // Assert
      expect(sdResult.directories[0].name).toBe('sd-dir');
      expect(usbResult.directories[0].name).toBe('usb-dir');
    });
  });
});
