import {
  StorageCacheDto,
  DirectoryItemDto,
  FileItemDto,
  ViewableItemImageDto,
  FileItemType as ApiFileItemType,
} from '@teensyrom-nx/data-access/api-client';
import {
  StorageDirectory,
  DirectoryItem,
  FileItem,
  ViewableItemImage,
  FileItemType,
} from './storage.models';

export class StorageMapper {
  static toStorageDirectory(storageCacheDto: StorageCacheDto): StorageDirectory {
    if (!storageCacheDto) {
      throw new Error('StorageCacheDto is required for transformation');
    }

    return {
      directories: storageCacheDto.directories?.map(StorageMapper.toDirectoryItem) ?? [],
      files: storageCacheDto.files?.map(StorageMapper.toFileItem) ?? [],
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

  static toFileItem(fileItemDto: FileItemDto): FileItem {
    if (!fileItemDto) {
      throw new Error('FileItemDto is required for transformation');
    }

    return {
      name: fileItemDto.name ?? '',
      path: fileItemDto.path ?? '',
      size: fileItemDto.size ?? 0,
      isFavorite: fileItemDto.isFavorite ?? false,
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
      images: fileItemDto.images?.map(StorageMapper.toViewableItemImage) ?? [],
      type: StorageMapper.toFileItemType(fileItemDto.type),
    };
  }

  static toViewableItemImage(viewableItemImageDto: ViewableItemImageDto): ViewableItemImage {
    if (!viewableItemImageDto) {
      throw new Error('ViewableItemImageDto is required for transformation');
    }

    return {
      fileName: viewableItemImageDto.fileName ?? '',
      path: viewableItemImageDto.path ?? '',
      source: viewableItemImageDto.source ?? '',
    };
  }

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
}
