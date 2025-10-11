import { FileItemType } from '@teensyrom-nx/domain';
import { getFileIcon, formatFileSize } from './file-utils';

describe('file-utils', () => {
  describe('getFileIcon', () => {
    it('should return music_note for Song type', () => {
      expect(getFileIcon(FileItemType.Song)).toBe('music_note');
    });

    it('should return sports_esports for Game type', () => {
      expect(getFileIcon(FileItemType.Game)).toBe('sports_esports');
    });

    it('should return image for Image type', () => {
      expect(getFileIcon(FileItemType.Image)).toBe('image');
    });

    it('should return code for Hex type', () => {
      expect(getFileIcon(FileItemType.Hex)).toBe('code');
    });

    it('should return insert_drive_file for Unknown type', () => {
      expect(getFileIcon(FileItemType.Unknown)).toBe('insert_drive_file');
    });
  });

  describe('formatFileSize', () => {
    it('should format 0 bytes correctly', () => {
      expect(formatFileSize(0)).toBe('0 B');
    });

    it('should format bytes correctly', () => {
      expect(formatFileSize(512)).toBe('512.0 B');
    });

    it('should format kilobytes correctly', () => {
      expect(formatFileSize(1536)).toBe('1.5 KB');
    });

    it('should format megabytes correctly', () => {
      expect(formatFileSize(2359296)).toBe('2.3 MB');
    });

    it('should format gigabytes correctly', () => {
      expect(formatFileSize(3221225472)).toBe('3.0 GB');
    });
  });
});
