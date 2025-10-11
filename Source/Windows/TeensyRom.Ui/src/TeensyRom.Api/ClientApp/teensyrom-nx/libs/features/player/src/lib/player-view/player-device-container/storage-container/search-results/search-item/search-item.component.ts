import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StorageItemComponent, StorageItemActionsComponent } from '@teensyrom-nx/ui/components';
import { FileItem, FileItemType } from '@teensyrom-nx/domain';

@Component({
  selector: 'lib-search-item',
  imports: [CommonModule, StorageItemComponent, StorageItemActionsComponent],
  templateUrl: './search-item.component.html',
  styleUrls: ['./search-item.component.scss'],
})
export class SearchItemComponent {
  fileItem = input.required<FileItem>();
  selected = input<boolean>(false);
  isPlaying = input<boolean>(false);

  itemSelected = output<FileItem>();
  itemDoubleClick = output<FileItem>();

  readonly fileIcon = computed(() => {
    switch (this.fileItem().type) {
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
  });

  readonly formattedSize = computed(() => {
    return this.formatFileSize(this.fileItem().size);
  });

  onItemClick(): void {
    this.itemSelected.emit(this.fileItem());
  }

  onItemDoubleClick(): void {
    this.itemDoubleClick.emit(this.fileItem());
  }

  private formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`;
  }
}
