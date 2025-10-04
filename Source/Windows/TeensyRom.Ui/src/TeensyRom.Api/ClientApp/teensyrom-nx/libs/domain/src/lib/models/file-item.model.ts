import { ViewableItemImage } from './viewable-item-image.model';
import { FileItemType } from './file-item-type.enum';

export interface FileItem {
  name: string;
  path: string;
  size: number;
  isFavorite: boolean;
  isCompatible: boolean;
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