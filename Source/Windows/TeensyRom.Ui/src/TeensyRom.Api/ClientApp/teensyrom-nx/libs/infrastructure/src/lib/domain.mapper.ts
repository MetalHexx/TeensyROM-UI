import {
  CartDto,
  CartStorageDto,
  StorageCacheDto,
  DirectoryItemDto,
  FileItemDto,
  ViewableItemImageDto,
  FileItemType as ApiFileItemType,
  TeensyStorageType as ApiStorageType,
  LaunchRandomScopeEnum,
  NullableOfTeensyFilterType,
  YouTubeVideoDto,
  CompetitionDto,
  FileLinkDto,
  FileTagDto,
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
  FileLink,
  FileTag,
  YouTubeVideo,
  Competition,
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

  static toStorageDirectory(
    storageCacheDto: StorageCacheDto,
    baseApiUrl: string
  ): StorageDirectory {
    if (!storageCacheDto) {
      throw new Error('StorageCacheDto is required for transformation');
    }

    return {
      directories: storageCacheDto.directories?.map(DomainMapper.toDirectoryItem) ?? [],
      files: storageCacheDto.files?.map((file) => DomainMapper.toFileItem(file, baseApiUrl)) ?? [],
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
      images:
        fileItemDto.images?.map((img) => DomainMapper.toViewableItemImage(img, baseApiUrl)) ?? [],
      type: DomainMapper.toFileItemType(fileItemDto.type),
      links: fileItemDto.links?.map(DomainMapper.toFileLink) ?? [],
      tags: fileItemDto.tags?.map(DomainMapper.toFileTag) ?? [],
      youTubeVideos: fileItemDto.youTubeVideos?.map(DomainMapper.toYouTubeVideo) ?? [],
      competitions: fileItemDto.competitions?.map(DomainMapper.toCompetition) ?? [],
      avgRating: fileItemDto.avgRating ?? undefined,
      ratingCount: fileItemDto.ratingCount ?? 0,
    };
  }

  static toViewableItemImage(
    viewableItemImageDto: ViewableItemImageDto,
    baseApiUrl: string
  ): ViewableItemImage {
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
   * Convert domain PlayerFilterType to API NullableOfTeensyFilterType
   * Used for both player random launch and search operations
   */
  static toApiPlayerFilter(filter: PlayerFilterType): NullableOfTeensyFilterType {
    switch (filter) {
      case PlayerFilterType.All:
        return NullableOfTeensyFilterType.All;
      case PlayerFilterType.Games:
        return NullableOfTeensyFilterType.Games;
      case PlayerFilterType.Music:
        return NullableOfTeensyFilterType.Music;
      case PlayerFilterType.Images:
        return NullableOfTeensyFilterType.Images;
      case PlayerFilterType.Hex:
        return NullableOfTeensyFilterType.Hex;
      default:
        return NullableOfTeensyFilterType.All;
    }
  }

  /**
   * Convert domain PlayerFilterType to API NullableOfTeensyFilterType for search operations
   * Alias for toApiPlayerFilter to maintain semantic clarity in search context
   */
  static toApiSearchFilter(filter: PlayerFilterType): NullableOfTeensyFilterType {
    return DomainMapper.toApiPlayerFilter(filter);
  }

  // ===== NEW MAPPING METHODS =====

  /**
   * Convert API FileLink DTO to domain FileLink
   */
  static toFileLink(dto: FileLinkDto): FileLink {
    return {
      name: dto.name ?? '',
      url: dto.url ?? '',
    };
  }

  /**
   * Convert API FileTag DTO to domain FileTag
   */
  static toFileTag(dto: FileTagDto): FileTag {
    return {
      name: dto.name ?? '',
      type: dto.type ?? '',
    };
  }

  /**
   * Convert API YouTubeVideo DTO to domain YouTubeVideo
   */
  static toYouTubeVideo(dto: YouTubeVideoDto): YouTubeVideo {
    return {
      videoId: dto.videoId ?? '',
      url: dto.url ?? '',
      channel: dto.channel ?? '',
      subtune: dto.subtune ?? 0,
    };
  }

  /**
   * Convert API Competition DTO to domain Competition
   */
  static toCompetition(dto: CompetitionDto): Competition {
    return {
      name: dto.name ?? '',
      place: dto.place ?? undefined,
    };
  }
}
