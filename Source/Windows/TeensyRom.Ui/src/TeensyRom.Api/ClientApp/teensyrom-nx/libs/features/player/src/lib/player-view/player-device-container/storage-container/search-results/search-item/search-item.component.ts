import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StorageItemComponent, StorageItemActionsComponent } from '@teensyrom-nx/ui/components';
import { FileItem, getFileIcon, formatFileSize } from '@teensyrom-nx/domain';

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

  readonly fileIcon = computed(() => getFileIcon(this.fileItem().type));
  readonly formattedSize = computed(() => formatFileSize(this.fileItem().size));

  onItemClick(): void {
    this.itemSelected.emit(this.fileItem());
  }

  onItemDoubleClick(): void {
    this.itemDoubleClick.emit(this.fileItem());
  }
}
