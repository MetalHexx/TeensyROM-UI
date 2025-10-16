import { ViewableItemImage } from './viewable-item-image.model';
import { FileItemType } from './file-item-type.enum';
import { FileLink } from './file-link.model';
import { FileTag } from './file-tag.model';
import { YouTubeVideo } from './youtube-video.model';
import { Competition } from './competition.model';

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
  links: FileLink[];
  tags: FileTag[];
  youTubeVideos: YouTubeVideo[];
  competitions: Competition[];
  avgRating?: number;
  ratingCount: number;
}