import { describe, it, expect } from 'vitest';
import {
  StorageCacheDto,
  DirectoryItemDto,
  FileItemDto,
  ViewableItemImageDto,
  FileItemType as ApiFileItemType,
} from '@teensyrom-nx/data-access/api-client';
import { StorageMapper } from './storage.mapper';
import { FileItemType } from './storage.models';

describe('StorageMapper', () => {
  describe('toStorageDirectory', () => {
    it('should transform StorageCacheDto to StorageDirectory successfully', () => {
      // Arrange
      const storageCacheDto: StorageCacheDto = {
        directories: [
          { name: 'Games', path: '/games' },
          { name: 'Music', path: '/music' },
        ],
        files: [
          {
            name: 'test.prg',
            path: '/test.prg',
            size: 1024,
            isFavorite: true,
            title: 'Test Game',
            creator: 'Test Creator',
            releaseInfo: '2023',
            description: 'A test game',
            shareUrl: 'http://example.com',
            metadataSource: 'HVSC',
            meta1: 'meta1',
            meta2: 'meta2',
            metadataSourcePath: '/metadata',
            parentPath: '/',
            playLength: '3:30',
            subtuneLengths: ['3:30'],
            startSubtuneNum: 1,
            images: [
              {
                fileName: 'image.png',
                path: '/images/image.png',
                source: 'local',
              },
            ],
            type: ApiFileItemType.Game,
          },
        ],
        path: '/root',
      };

      // Act
      const result = StorageMapper.toStorageDirectory(storageCacheDto);

      // Assert
      expect(result).toBeDefined();
      expect(result.path).toBe('/root');
      expect(result.directories).toHaveLength(2);
      expect(result.directories[0].name).toBe('Games');
      expect(result.directories[0].path).toBe('/games');
      expect(result.files).toHaveLength(1);
      expect(result.files[0].name).toBe('test.prg');
      expect(result.files[0].type).toBe(FileItemType.Game);
    });

    it('should handle null/undefined directories and files arrays', () => {
      // Arrange
      const storageCacheDto: StorageCacheDto = {
        directories: [],
        files: [],
        path: '/test',
      };

      // Act
      const result = StorageMapper.toStorageDirectory(storageCacheDto);

      // Assert
      expect(result.directories).toEqual([]);
      expect(result.files).toEqual([]);
      expect(result.path).toBe('/test');
    });

    it('should throw error when StorageCacheDto is null', () => {
      // Arrange
      const storageCacheDto = null as unknown as StorageCacheDto;

      // Act & Assert
      expect(() => StorageMapper.toStorageDirectory(storageCacheDto)).toThrow(
        'StorageCacheDto is required for transformation'
      );
    });
  });

  describe('toFileItemType', () => {
    it('should map API file types to domain file types correctly', () => {
      expect(StorageMapper.toFileItemType(ApiFileItemType.Song)).toBe(FileItemType.Song);
      expect(StorageMapper.toFileItemType(ApiFileItemType.Game)).toBe(FileItemType.Game);
      expect(StorageMapper.toFileItemType(ApiFileItemType.Image)).toBe(FileItemType.Image);
      expect(StorageMapper.toFileItemType(ApiFileItemType.Hex)).toBe(FileItemType.Hex);
      expect(StorageMapper.toFileItemType(ApiFileItemType.Unknown)).toBe(FileItemType.Unknown);
    });
  });

  describe('toDirectoryItem', () => {
    it('should transform DirectoryItemDto successfully', () => {
      // Arrange
      const dto: DirectoryItemDto = {
        name: 'Test Directory',
        path: '/test',
      };

      // Act
      const result = StorageMapper.toDirectoryItem(dto);

      // Assert
      expect(result.name).toBe('Test Directory');
      expect(result.path).toBe('/test');
    });

    it('should handle null/undefined properties with defaults', () => {
      // Arrange
      const dto: DirectoryItemDto = {
        name: '',
        path: '',
      };

      // Act
      const result = StorageMapper.toDirectoryItem(dto);

      // Assert
      expect(result.name).toBe('');
      expect(result.path).toBe('');
    });

    it('should throw error when DirectoryItemDto is null', () => {
      // Arrange
      const dto = null as unknown as DirectoryItemDto;

      // Act & Assert
      expect(() => StorageMapper.toDirectoryItem(dto)).toThrow(
        'DirectoryItemDto is required for transformation'
      );
    });
  });

  describe('toFileItem', () => {
    it('should transform FileItemDto with all properties', () => {
      // Arrange
      const dto: FileItemDto = {
        name: 'test.sid',
        path: '/music/test.sid',
        size: 2048,
        isFavorite: false,
        title: 'Test Song',
        creator: 'Test Musician',
        releaseInfo: '1985',
        description: 'Classic SID tune',
        shareUrl: 'http://example.com/test.sid',
        metadataSource: 'HVSC',
        meta1: 'additional info',
        meta2: 'more info',
        metadataSourcePath: '/hvsc/test.sid',
        parentPath: '/music',
        playLength: '2:45',
        subtuneLengths: ['2:45', '1:30'],
        startSubtuneNum: 0,
        images: [],
        type: ApiFileItemType.Song,
      };

      // Act
      const result = StorageMapper.toFileItem(dto);

      // Assert
      expect(result.name).toBe('test.sid');
      expect(result.path).toBe('/music/test.sid');
      expect(result.size).toBe(2048);
      expect(result.isFavorite).toBe(false);
      expect(result.title).toBe('Test Song');
      expect(result.creator).toBe('Test Musician');
      expect(result.type).toBe(FileItemType.Song);
      expect(result.subtuneLengths).toEqual(['2:45', '1:30']);
    });

    it('should throw error when FileItemDto is null', () => {
      // Arrange
      const dto = null as unknown as FileItemDto;

      // Act & Assert
      expect(() => StorageMapper.toFileItem(dto)).toThrow(
        'FileItemDto is required for transformation'
      );
    });
  });

  describe('toViewableItemImage', () => {
    it('should transform ViewableItemImageDto successfully', () => {
      // Arrange
      const dto: ViewableItemImageDto = {
        fileName: 'screenshot.png',
        path: '/images/screenshot.png',
        source: 'embedded',
      };

      // Act
      const result = StorageMapper.toViewableItemImage(dto);

      // Assert
      expect(result.fileName).toBe('screenshot.png');
      expect(result.path).toBe('/images/screenshot.png');
      expect(result.source).toBe('embedded');
    });

    it('should throw error when ViewableItemImageDto is null', () => {
      // Arrange
      const dto = null as unknown as ViewableItemImageDto;

      // Act & Assert
      expect(() => StorageMapper.toViewableItemImage(dto)).toThrow(
        'ViewableItemImageDto is required for transformation'
      );
    });
  });
});
