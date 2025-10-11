import { FileItemType } from '@teensyrom-nx/domain';

/**
 * Utility functions for working with files and file metadata
 */

/**
 * Maps FileItemType enum to Material icon name
 *
 * @param type - The file item type to map
 * @returns Material icon name for display
 *
 * @example
 * getFileIcon(FileItemType.Song)  // Returns 'music_note'
 * getFileIcon(FileItemType.Game)  // Returns 'sports_esports'
 */
export function getFileIcon(type: FileItemType): string {
  switch (type) {
    case FileItemType.Song:
      return 'music_note';
    case FileItemType.Game:
      return 'sports_esports';
    case FileItemType.Image:
      return 'image';
    case FileItemType.Hex:
      return 'code';
    case FileItemType.Unknown:
    default:
      return 'insert_drive_file';
  }
}

/**
 * Formats bytes to human-readable file size with appropriate unit
 *
 * @param bytes - File size in bytes
 * @returns Formatted string with one decimal place and unit (e.g., "1.5 MB")
 *
 * @example
 * formatFileSize(0)           // Returns '0 B'
 * formatFileSize(1536)        // Returns '1.5 KB'
 * formatFileSize(2359296)     // Returns '2.3 MB'
 * formatFileSize(3221225472)  // Returns '3.0 GB'
 */
export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`;
}
