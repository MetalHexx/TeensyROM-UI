import {
  CartDto,
  CartStorageDto,
  StorageCacheDto,
  DirectoryItemDto,
  FileItemDto,
  ViewableItemImageDto,
  FileItemType as ApiFileItemType,
  TeensyStorageType as ApiStorageType,
  LaunchRandomFilterTypeEnum,
  LaunchRandomScopeEnum,
} from '@teensyrom-nx/data-access/api-client';
import { DeviceState as ApiDeviceState } from '@teensyrom-nx/data-access/api-client';
import {
  Device,
  DeviceStorage,
  DeviceState,
  StorageType,
  StorageDirectory,
  DirectoryItem,
  FileItem,
  ViewableItemImage,
  FileItemType,
  PlayerFilterType,
  PlayerScope,
} from '@teensyrom-nx/domain';

/**
 * Centralized mapper for all domain model transformations.
 * Contains mapping logic for Device, Storage, and shared models.
 */
export class DomainMapper {
  // ===== DEVICE MAPPING =====

  static toDevice(cartDto: CartDto): Device {
    if (!cartDto) {
      return {} as Device;
    }

    return {
      deviceId: cartDto.deviceId,
      comPort: cartDto.comPort,
      name: cartDto.name,
      fwVersion: cartDto.fwVersion,
      isCompatible: cartDto.isCompatible,
      isConnected: cartDto.isConnected,
      deviceState: this.mapDeviceState(cartDto.deviceState),
      sdStorage: this.toDeviceStorage(cartDto.sdStorage),
      usbStorage: this.toDeviceStorage(cartDto.usbStorage),
    };
  }

  static toDeviceStorage(storageDto: CartStorageDto): DeviceStorage {
    return {
      deviceId: storageDto.deviceId,
      type: this.toDomainStorageType(storageDto.type),
      available: storageDto.available,
    };
  }

  static toDeviceList(cartDtos: CartDto[]): Device[] {
    return cartDtos.map((cart) => this.toDevice(cart));
  }

  private static mapDeviceState(apiState: ApiDeviceState): DeviceState {
    // Both enums have the same values, so we can safely cast
    return apiState as unknown as DeviceState;
  }

  // ===== STORAGE MAPPING =====

  static toStorageDirectory(storageCacheDto: StorageCacheDto, baseApiUrl: string): StorageDirectory {
    if (!storageCacheDto) {
      throw new Error('StorageCacheDto is required for transformation');
    }

    return {
      directories: storageCacheDto.directories?.map(DomainMapper.toDirectoryItem) ?? [],
      files: storageCacheDto.files?.map(file => DomainMapper.toFileItem(file, baseApiUrl)) ?? [],
      path: storageCacheDto.path ?? '',
    };
  }

  static toDirectoryItem(directoryItemDto: DirectoryItemDto): DirectoryItem {
    if (!directoryItemDto) {
      throw new Error('DirectoryItemDto is required for transformation');
    }

    return {
      name: directoryItemDto.name ?? '',
      path: directoryItemDto.path ?? '',
    };
  }

  static toFileItem(fileItemDto: FileItemDto, baseApiUrl: string): FileItem {
    if (!fileItemDto) {
      throw new Error('FileItemDto is required for transformation');
    }

    return {
      name: fileItemDto.name ?? '',
      path: fileItemDto.path ?? '',
      size: fileItemDto.size ?? 0,
      isFavorite: fileItemDto.isFavorite ?? false,
      isCompatible: fileItemDto.isCompatible ?? true,
      title: fileItemDto.title ?? '',
      creator: fileItemDto.creator ?? '',
      releaseInfo: fileItemDto.releaseInfo ?? '',
      description: fileItemDto.description ?? '',
      shareUrl: fileItemDto.shareUrl ?? '',
      metadataSource: fileItemDto.metadataSource ?? '',
      meta1: fileItemDto.meta1 ?? '',
      meta2: fileItemDto.meta2 ?? '',
      metadataSourcePath: fileItemDto.metadataSourcePath ?? '',
      parentPath: fileItemDto.parentPath ?? '',
      playLength: fileItemDto.playLength ?? '',
      subtuneLengths: fileItemDto.subtuneLengths ?? [],
      startSubtuneNum: fileItemDto.startSubtuneNum ?? 0,
      images: fileItemDto.images?.map(img => DomainMapper.toViewableItemImage(img, baseApiUrl)) ?? [],
      type: DomainMapper.toFileItemType(fileItemDto.type),
    };
  }

  static toViewableItemImage(viewableItemImageDto: ViewableItemImageDto, baseApiUrl: string): ViewableItemImage {
    if (!viewableItemImageDto) {
      throw new Error('ViewableItemImageDto is required for transformation');
    }

    const baseAssetPath = viewableItemImageDto.baseAssetPath ?? '';
    const url = baseAssetPath ? `${baseApiUrl}${baseAssetPath}` : '';

    return {
      fileName: viewableItemImageDto.fileName ?? '',
      path: viewableItemImageDto.path ?? '',
      source: viewableItemImageDto.source ?? '',
      url,
    };
  }

  // ===== SHARED TYPE MAPPING =====

  static toFileItemType(apiFileItemType: ApiFileItemType): FileItemType {
    switch (apiFileItemType) {
      case ApiFileItemType.Song:
        return FileItemType.Song;
      case ApiFileItemType.Game:
        return FileItemType.Game;
      case ApiFileItemType.Image:
        return FileItemType.Image;
      case ApiFileItemType.Hex:
        return FileItemType.Hex;
      case ApiFileItemType.Unknown:
      default:
        return FileItemType.Unknown;
    }
  }

  /**
   * Convert domain StorageType to API TeensyStorageType
   */
  static toApiStorageType(domainType: StorageType): ApiStorageType {
    switch (domainType) {
      case StorageType.Sd:
        return ApiStorageType.Sd;
      case StorageType.Usb:
        return ApiStorageType.Usb;
      default:
        throw new Error(`Unknown storage type: ${domainType}`);
    }
  }

  /**
   * Convert API TeensyStorageType to domain StorageType
   */
  static toDomainStorageType(apiType: ApiStorageType): StorageType {
    switch (apiType) {
      case ApiStorageType.Sd:
        return StorageType.Sd;
      case ApiStorageType.Usb:
        return StorageType.Usb;
      default:
        throw new Error(`Unknown API storage type: ${apiType}`);
    }
  }

  // ===== PLAYER MAPPING =====

  /**
   * Convert domain PlayerScope to API LaunchRandomScopeEnum
   */
  static toApiPlayerScope(scope: PlayerScope): LaunchRandomScopeEnum {
    switch (scope) {
      case PlayerScope.Storage:
        return LaunchRandomScopeEnum.Storage;
      case PlayerScope.DirectoryDeep:
        return LaunchRandomScopeEnum.DirDeep;
      case PlayerScope.DirectoryShallow:
        return LaunchRandomScopeEnum.DirShallow;
      default:
        return LaunchRandomScopeEnum.Storage;
    }
  }

  /**
   * Convert domain PlayerFilterType to API LaunchRandomFilterTypeEnum
   */
  static toApiPlayerFilter(filter: PlayerFilterType): LaunchRandomFilterTypeEnum {
    switch (filter) {
      case PlayerFilterType.All:
        return LaunchRandomFilterTypeEnum.All;
      case PlayerFilterType.Games:
        return LaunchRandomFilterTypeEnum.Games;
      case PlayerFilterType.Music:
        return LaunchRandomFilterTypeEnum.Music;
      case PlayerFilterType.Images:
        return LaunchRandomFilterTypeEnum.Images;
      case PlayerFilterType.Hex:
        return LaunchRandomFilterTypeEnum.Hex;
      default:
        return LaunchRandomFilterTypeEnum.All;
    }
  }
}