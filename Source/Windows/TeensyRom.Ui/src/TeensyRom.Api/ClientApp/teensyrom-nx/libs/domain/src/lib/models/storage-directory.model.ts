import { DirectoryItem } from './directory-item.model';
import { FileItem } from './file-item.model';

export interface StorageDirectory {
  directories: DirectoryItem[];
  files: FileItem[];
  path: string;
}
