import { describe, it, expect } from 'vitest';
import {
  CartDto,
  CartStorageDto,
  StorageCacheDto,
  DirectoryItemDto,
  FileItemDto,
  ViewableItemImageDto,
  FileItemType as ApiFileItemType,
  TeensyStorageType as ApiStorageType,
  DeviceState as ApiDeviceState,
  LaunchRandomFilterTypeEnum,
  LaunchRandomScopeEnum,
} from '@teensyrom-nx/data-access/api-client';
import { DomainMapper } from './domain.mapper';
import { FileItemType, StorageType, DeviceState, PlayerFilterType, PlayerScope } from '@teensyrom-nx/domain';

describe('DomainMapper (Storage)', () => {
  const baseApiUrl = 'http://localhost:5168';

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
            isCompatible: true,
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
                baseAssetPath: '/Assets/Games/Screenshots/image.png',
                source: 'local',
              },
            ],
            type: ApiFileItemType.Game,
          },
        ],
        path: '/root',
      };

      // Act
      const result = DomainMapper.toStorageDirectory(storageCacheDto, baseApiUrl);

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
      const result = DomainMapper.toStorageDirectory(storageCacheDto, baseApiUrl);

      // Assert
      expect(result.directories).toEqual([]);
      expect(result.files).toEqual([]);
      expect(result.path).toBe('/test');
    });

    it('should throw error when StorageCacheDto is null', () => {
      // Arrange
      const storageCacheDto = null as unknown as StorageCacheDto;

      // Act & Assert
      expect(() => DomainMapper.toStorageDirectory(storageCacheDto, baseApiUrl)).toThrow(
        'StorageCacheDto is required for transformation'
      );
    });
  });

  describe('toFileItemType', () => {
    it('should map API file types to domain file types correctly', () => {
      expect(DomainMapper.toFileItemType(ApiFileItemType.Song)).toBe(FileItemType.Song);
      expect(DomainMapper.toFileItemType(ApiFileItemType.Game)).toBe(FileItemType.Game);
      expect(DomainMapper.toFileItemType(ApiFileItemType.Image)).toBe(FileItemType.Image);
      expect(DomainMapper.toFileItemType(ApiFileItemType.Hex)).toBe(FileItemType.Hex);
      expect(DomainMapper.toFileItemType(ApiFileItemType.Unknown)).toBe(FileItemType.Unknown);
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
      const result = DomainMapper.toDirectoryItem(dto);

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
      const result = DomainMapper.toDirectoryItem(dto);

      // Assert
      expect(result.name).toBe('');
      expect(result.path).toBe('');
    });

    it('should throw error when DirectoryItemDto is null', () => {
      // Arrange
      const dto = null as unknown as DirectoryItemDto;

      // Act & Assert
      expect(() => DomainMapper.toDirectoryItem(dto)).toThrow(
        'DirectoryItemDto is required for transformation'
      );
    });
  });

  describe('toFileItem', () => {
    const baseApiUrl = 'http://localhost:5168';

    it('should transform FileItemDto with all properties', () => {
      // Arrange
      const dto: FileItemDto = {
        name: 'test.sid',
        path: '/music/test.sid',
        size: 2048,
        isFavorite: false,
        isCompatible: true,
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
      const result = DomainMapper.toFileItem(dto, baseApiUrl);

      // Assert
      expect(result.name).toBe('test.sid');
      expect(result.path).toBe('/music/test.sid');
      expect(result.size).toBe(2048);
      expect(result.isFavorite).toBe(false);
      expect(result.isCompatible).toBe(true);
      expect(result.title).toBe('Test Song');
      expect(result.creator).toBe('Test Musician');
      expect(result.type).toBe(FileItemType.Song);
      expect(result.subtuneLengths).toEqual(['2:45', '1:30']);
    });

    it('should map isCompatible field correctly', () => {
      // Arrange - Compatible file
      const compatibleDto: FileItemDto = {
        name: 'compatible.sid',
        path: '/music/compatible.sid',
        size: 1024,
        isFavorite: false,
        isCompatible: true,
        title: '',
        creator: '',
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
        type: ApiFileItemType.Song,
      };

      // Act
      const compatibleResult = DomainMapper.toFileItem(compatibleDto, baseApiUrl);

      // Assert
      expect(compatibleResult.isCompatible).toBe(true);

      // Arrange - Incompatible file
      const incompatibleDto: FileItemDto = {
        ...compatibleDto,
        name: 'incompatible.sid',
        isCompatible: false,
      };

      // Act
      const incompatibleResult = DomainMapper.toFileItem(incompatibleDto, baseApiUrl);

      // Assert
      expect(incompatibleResult.isCompatible).toBe(false);
    });

    it('should default isCompatible to true when undefined', () => {
      // Arrange
      const dto: FileItemDto = {
        name: 'test.sid',
        path: '/music/test.sid',
        size: 1024,
        isFavorite: false,
        isCompatible: undefined,
        title: '',
        creator: '',
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
        type: ApiFileItemType.Song,
      };

      // Act
      const result = DomainMapper.toFileItem(dto, baseApiUrl);

      // Assert
      expect(result.isCompatible).toBe(true);
    });

    it('should pass baseApiUrl to toViewableItemImage for all images', () => {
      // Arrange
      const dto: FileItemDto = {
        name: 'test.prg',
        path: '/games/test.prg',
        size: 1024,
        isFavorite: false,
        isCompatible: true,
        title: '',
        creator: '',
        releaseInfo: '',
        description: '',
        shareUrl: '',
        metadataSource: '',
        meta1: '',
        meta2: '',
        metadataSourcePath: '',
        parentPath: '/games',
        playLength: '',
        subtuneLengths: [],
        startSubtuneNum: 0,
        images: [
          {
            fileName: 'screenshot1.png',
            path: '/images/screenshot1.png',
            baseAssetPath: '/Assets/Games/Screenshots/screenshot1.png',
            source: 'local',
          },
          {
            fileName: 'screenshot2.png',
            path: '/images/screenshot2.png',
            baseAssetPath: '/Assets/Games/Screenshots/screenshot2.png',
            source: 'local',
          },
        ],
        type: ApiFileItemType.Game,
      };

      // Act
      const result = DomainMapper.toFileItem(dto, baseApiUrl);

      // Assert
      expect(result.images).toHaveLength(2);
      expect(result.images[0].url).toBe('http://localhost:5168/Assets/Games/Screenshots/screenshot1.png');
      expect(result.images[1].url).toBe('http://localhost:5168/Assets/Games/Screenshots/screenshot2.png');
    });

    it('should throw error when FileItemDto is null', () => {
      // Arrange
      const dto = null as unknown as FileItemDto;

      // Act & Assert
      expect(() => DomainMapper.toFileItem(dto, baseApiUrl)).toThrow(
        'FileItemDto is required for transformation'
      );
    });
  });

  describe('toViewableItemImage', () => {
    const baseApiUrl = 'http://localhost:5168';

    it('should transform ViewableItemImageDto successfully with URL construction', () => {
      // Arrange
      const dto: ViewableItemImageDto = {
        fileName: 'screenshot.png',
        path: '/images/screenshot.png',
        baseAssetPath: '/Assets/Games/Screenshots/screenshot.png',
        source: 'embedded',
      };

      // Act
      const result = DomainMapper.toViewableItemImage(dto, baseApiUrl);

      // Assert
      expect(result.fileName).toBe('screenshot.png');
      expect(result.path).toBe('/images/screenshot.png');
      expect(result.source).toBe('embedded');
      expect(result.url).toBe('http://localhost:5168/Assets/Games/Screenshots/screenshot.png');
    });

    it('should construct URL correctly from baseApiUrl + baseAssetPath', () => {
      // Arrange
      const dto: ViewableItemImageDto = {
        fileName: 'test.png',
        path: '/test.png',
        baseAssetPath: '/Assets/Music/Covers/test.png',
        source: 'local',
      };

      // Act
      const result = DomainMapper.toViewableItemImage(dto, baseApiUrl);

      // Assert
      expect(result.url).toBe('http://localhost:5168/Assets/Music/Covers/test.png');
    });

    it('should return empty string for url when baseAssetPath is empty', () => {
      // Arrange
      const dto: ViewableItemImageDto = {
        fileName: 'test.png',
        path: '/test.png',
        baseAssetPath: '',
        source: 'local',
      };

      // Act
      const result = DomainMapper.toViewableItemImage(dto, baseApiUrl);

      // Assert
      expect(result.url).toBe('');
    });

    it('should return empty string for url when baseAssetPath is undefined', () => {
      // Arrange
      const dto: ViewableItemImageDto = {
        fileName: 'test.png',
        path: '/test.png',
        baseAssetPath: undefined as any,
        source: 'local',
      };

      // Act
      const result = DomainMapper.toViewableItemImage(dto, baseApiUrl);

      // Assert
      expect(result.url).toBe('');
    });

    it('should throw error when ViewableItemImageDto is null', () => {
      // Arrange
      const dto = null as unknown as ViewableItemImageDto;

      // Act & Assert
      expect(() => DomainMapper.toViewableItemImage(dto, baseApiUrl)).toThrow(
        'ViewableItemImageDto is required for transformation'
      );
    });
  });

  describe('Storage Type Mapping', () => {
    describe('toApiStorageType', () => {
      it('should map domain StorageType to API TeensyStorageType correctly', () => {
        expect(DomainMapper.toApiStorageType(StorageType.Sd)).toBe(ApiStorageType.Sd);
        expect(DomainMapper.toApiStorageType(StorageType.Usb)).toBe(ApiStorageType.Usb);
      });

      it('should throw error for unknown storage type', () => {
        const unknownType = 'InvalidType' as unknown as StorageType;
        expect(() => DomainMapper.toApiStorageType(unknownType)).toThrow(
          'Unknown storage type: InvalidType'
        );
      });
    });

    describe('toDomainStorageType', () => {
      it('should map API TeensyStorageType to domain StorageType correctly', () => {
        expect(DomainMapper.toDomainStorageType(ApiStorageType.Sd)).toBe(StorageType.Sd);
        expect(DomainMapper.toDomainStorageType(ApiStorageType.Usb)).toBe(StorageType.Usb);
      });

      it('should throw error for unknown API storage type', () => {
        const unknownType = 'InvalidApiType' as unknown as ApiStorageType;
        expect(() => DomainMapper.toDomainStorageType(unknownType)).toThrow(
          'Unknown API storage type: InvalidApiType'
        );
      });
    });
  });
});

describe('DomainMapper (Device)', () => {
  describe('toDevice', () => {
    it('should transform CartDto to Device successfully', () => {
      // Arrange
      const cartDto: CartDto = {
        deviceId: 'device-123',
        comPort: 'COM3',
        name: 'TeensyROM Cart',
        fwVersion: '1.0.0',
        isCompatible: true,
        isConnected: true,
        deviceState: ApiDeviceState.Connected,
        sdStorage: {
          deviceId: 'device-123',
          type: ApiStorageType.Sd,
          available: true,
        },
        usbStorage: {
          deviceId: 'device-123',
          type: ApiStorageType.Usb,
          available: false,
        },
      };

      // Act
      const result = DomainMapper.toDevice(cartDto);

      // Assert
      expect(result).toBeDefined();
      expect(result.deviceId).toBe('device-123');
      expect(result.comPort).toBe('COM3');
      expect(result.name).toBe('TeensyROM Cart');
      expect(result.fwVersion).toBe('1.0.0');
      expect(result.isCompatible).toBe(true);
      expect(result.isConnected).toBe(true);
      expect(result.deviceState).toBe(DeviceState.Connected);
      expect(result.sdStorage).toBeDefined();
      expect(result.sdStorage.type).toBe(StorageType.Sd);
      expect(result.sdStorage.available).toBe(true);
      expect(result.usbStorage).toBeDefined();
      expect(result.usbStorage.type).toBe(StorageType.Usb);
      expect(result.usbStorage.available).toBe(false);
    });

    it('should handle null CartDto', () => {
      // Arrange
      const cartDto = null as unknown as CartDto;

      // Act
      const result = DomainMapper.toDevice(cartDto);

      // Assert
      expect(result).toEqual({});
    });

    it('should handle undefined CartDto', () => {
      // Arrange
      const cartDto = undefined as unknown as CartDto;

      // Act
      const result = DomainMapper.toDevice(cartDto);

      // Assert
      expect(result).toEqual({});
    });
  });

  describe('toDeviceStorage', () => {
    it('should transform CartStorageDto to DeviceStorage successfully', () => {
      // Arrange
      const storageDto: CartStorageDto = {
        deviceId: 'device-456',
        type: ApiStorageType.Sd,
        available: true,
      };

      // Act
      const result = DomainMapper.toDeviceStorage(storageDto);

      // Assert
      expect(result).toBeDefined();
      expect(result.deviceId).toBe('device-456');
      expect(result.type).toBe(StorageType.Sd);
      expect(result.available).toBe(true);
    });

    it('should handle USB storage type', () => {
      // Arrange
      const storageDto: CartStorageDto = {
        deviceId: 'device-789',
        type: ApiStorageType.Usb,
        available: false,
      };

      // Act
      const result = DomainMapper.toDeviceStorage(storageDto);

      // Assert
      expect(result.deviceId).toBe('device-789');
      expect(result.type).toBe(StorageType.Usb);
      expect(result.available).toBe(false);
    });
  });

  describe('toDeviceList', () => {
    it('should transform array of CartDto to Device array successfully', () => {
      // Arrange
      const cartDtos: CartDto[] = [
        {
          deviceId: 'device-1',
          comPort: 'COM1',
          name: 'Cart 1',
          fwVersion: '1.0.0',
          isCompatible: true,
          isConnected: false,
          deviceState: ApiDeviceState.Disconnected,
          sdStorage: {
            deviceId: 'device-1',
            type: ApiStorageType.Sd,
            available: true,
          },
          usbStorage: {
            deviceId: 'device-1',
            type: ApiStorageType.Usb,
            available: true,
          },
        },
        {
          deviceId: 'device-2',
          comPort: 'COM2',
          name: 'Cart 2',
          fwVersion: '1.1.0',
          isCompatible: false,
          isConnected: true,
          deviceState: ApiDeviceState.Connected,
          sdStorage: {
            deviceId: 'device-2',
            type: ApiStorageType.Sd,
            available: false,
          },
          usbStorage: {
            deviceId: 'device-2',
            type: ApiStorageType.Usb,
            available: true,
          },
        },
      ];

      // Act
      const result = DomainMapper.toDeviceList(cartDtos);

      // Assert
      expect(result).toHaveLength(2);
      expect(result[0].deviceId).toBe('device-1');
      expect(result[0].name).toBe('Cart 1');
      expect(result[0].isConnected).toBe(false);
      expect(result[0].deviceState).toBe(DeviceState.Disconnected);
      expect(result[1].deviceId).toBe('device-2');
      expect(result[1].name).toBe('Cart 2');
      expect(result[1].isCompatible).toBe(false);
      expect(result[1].deviceState).toBe(DeviceState.Connected);
    });

    it('should handle empty array', () => {
      // Arrange
      const cartDtos: CartDto[] = [];

      // Act
      const result = DomainMapper.toDeviceList(cartDtos);

      // Assert
      expect(result).toEqual([]);
    });
  });
});

describe('DomainMapper (Player)', () => {
  describe('toApiPlayerScope', () => {
    it('should map PlayerScope.Storage to LaunchRandomScopeEnum.Storage', () => {
      const result = DomainMapper.toApiPlayerScope(PlayerScope.Storage);
      expect(result).toBe(LaunchRandomScopeEnum.Storage);
    });

    it('should map PlayerScope.DirectoryDeep to LaunchRandomScopeEnum.DirDeep', () => {
      const result = DomainMapper.toApiPlayerScope(PlayerScope.DirectoryDeep);
      expect(result).toBe(LaunchRandomScopeEnum.DirDeep);
    });

    it('should map PlayerScope.DirectoryShallow to LaunchRandomScopeEnum.DirShallow', () => {
      const result = DomainMapper.toApiPlayerScope(PlayerScope.DirectoryShallow);
      expect(result).toBe(LaunchRandomScopeEnum.DirShallow);
    });
  });

  describe('toApiPlayerFilter', () => {
    it('should map PlayerFilterType.All to LaunchRandomFilterTypeEnum.All', () => {
      const result = DomainMapper.toApiPlayerFilter(PlayerFilterType.All);
      expect(result).toBe(LaunchRandomFilterTypeEnum.All);
    });

    it('should map PlayerFilterType.Games to LaunchRandomFilterTypeEnum.Games', () => {
      const result = DomainMapper.toApiPlayerFilter(PlayerFilterType.Games);
      expect(result).toBe(LaunchRandomFilterTypeEnum.Games);
    });

    it('should map PlayerFilterType.Music to LaunchRandomFilterTypeEnum.Music', () => {
      const result = DomainMapper.toApiPlayerFilter(PlayerFilterType.Music);
      expect(result).toBe(LaunchRandomFilterTypeEnum.Music);
    });

    it('should map PlayerFilterType.Images to LaunchRandomFilterTypeEnum.Images', () => {
      const result = DomainMapper.toApiPlayerFilter(PlayerFilterType.Images);
      expect(result).toBe(LaunchRandomFilterTypeEnum.Images);
    });

    it('should map PlayerFilterType.Hex to LaunchRandomFilterTypeEnum.Hex', () => {
      const result = DomainMapper.toApiPlayerFilter(PlayerFilterType.Hex);
      expect(result).toBe(LaunchRandomFilterTypeEnum.Hex);
    });
  });
});
