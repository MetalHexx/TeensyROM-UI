import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import {
  FilesApiService,
  GetDirectoryResponse,
  TeensyStorageType,
  StorageCacheDto,
  FileItemType as ApiFileItemType,
} from '@teensyrom-nx/data-access/api-client';
import { StorageService } from './storage.service';
import { FileItemType, StorageDirectory } from './storage.models';

describe('StorageService', () => {
  let service: StorageService;
  let mockFilesApiService: {
    getDirectory: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    mockFilesApiService = {
      getDirectory: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [StorageService, { provide: FilesApiService, useValue: mockFilesApiService }],
    });

    service = TestBed.inject(StorageService);
  });

  describe('getDirectory', () => {
    it('should return transformed StorageDirectory when API call succeeds', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = TeensyStorageType.Sd;
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
            },
          ],
          path: '/games',
        } as StorageCacheDto,
        message: 'Success',
      };

      mockFilesApiService.getDirectory.mockReturnValue(of(mockResponse));

      // Act
      const result = await new Promise<StorageDirectory>((resolve, reject) => {
        service.getDirectory(deviceId, storageType, path).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.getDirectory).toHaveBeenCalledWith(deviceId, storageType, path);
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
      const storageType = TeensyStorageType.Usb;

      const mockResponse: GetDirectoryResponse = {
        storageItem: {
          directories: [],
          files: [],
          path: '/',
        } as StorageCacheDto,
        message: 'Success',
      };

      mockFilesApiService.getDirectory.mockReturnValue(of(mockResponse));

      // Act
      const result = await new Promise<StorageDirectory>((resolve, reject) => {
        service.getDirectory(deviceId, storageType).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.getDirectory).toHaveBeenCalledWith(
        deviceId,
        storageType,
        undefined
      );
      expect(result).toBeDefined();
      expect(result.path).toBe('/');
    });

    it('should throw error when response storageItem is missing', async () => {
      // Arrange
      const deviceId = 'test-device';
      const storageType = TeensyStorageType.Sd;
      const mockResponse: GetDirectoryResponse = {
        storageItem: null as any,
        message: 'Error',
      };

      mockFilesApiService.getDirectory.mockReturnValue(of(mockResponse));

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
      const storageType = TeensyStorageType.Sd;
      const errorMessage = 'Network error';
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {
        // Suppress console.error during test
      });

      mockFilesApiService.getDirectory.mockReturnValue(throwError(() => new Error(errorMessage)));

      // Act & Assert
      await expect(
        new Promise((resolve, reject) => {
          service.getDirectory(deviceId, storageType).subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow(errorMessage);

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Storage directory fetch failed:',
        expect.any(Error)
      );

      // Cleanup
      consoleErrorSpy.mockRestore();
    });

    it('should pass correct parameters to API service', async () => {
      // Arrange
      const deviceId = 'device-123';
      const storageType = TeensyStorageType.Usb;
      const path = '/music/classical';

      const mockResponse: GetDirectoryResponse = {
        storageItem: {
          directories: [],
          files: [],
          path: path,
        } as StorageCacheDto,
        message: 'Success',
      };

      mockFilesApiService.getDirectory.mockReturnValue(of(mockResponse));

      // Act
      await new Promise((resolve, reject) => {
        service.getDirectory(deviceId, storageType, path).subscribe({
          next: resolve,
          error: reject,
        });
      });

      // Assert
      expect(mockFilesApiService.getDirectory).toHaveBeenCalledTimes(1);
      expect(mockFilesApiService.getDirectory).toHaveBeenCalledWith(deviceId, storageType, path);
    });
  });
});
