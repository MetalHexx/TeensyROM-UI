export interface DirectoryItem {
  name: string;
  path: string;
}

export interface FileItem {
  name: string;
  path: string;
  size: number;
  isFavorite: boolean;
  title: string;
  creator: string;
  releaseInfo: string;
  description: string;
  shareUrl: string;
  metadataSource: string;
  meta1: string;
  meta2: string;
  metadataSourcePath: string;
  parentPath: string;
  playLength: string;
  subtuneLengths: string[];
  startSubtuneNum: number;
  images: ViewableItemImage[];
  type: FileItemType;
}

export interface ViewableItemImage {
  fileName: string;
  path: string;
  source: string;
}

export enum FileItemType {
  Unknown = 'Unknown',
  Song = 'Song',
  Game = 'Game',
  Image = 'Image',
  Hex = 'Hex',
}

export enum StorageType {
  Sd = 'SD',
  Usb = 'USB',
}

export interface StorageDirectory {
  directories: DirectoryItem[];
  files: FileItem[];
  path: string;
}
