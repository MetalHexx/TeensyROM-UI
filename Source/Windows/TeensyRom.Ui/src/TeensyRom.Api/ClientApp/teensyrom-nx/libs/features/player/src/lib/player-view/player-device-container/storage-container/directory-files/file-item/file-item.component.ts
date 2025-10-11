import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StorageItemComponent, StorageItemActionsComponent } from '@teensyrom-nx/ui/components';
import { FileItem } from '@teensyrom-nx/domain';
import { getFileIcon, formatFileSize } from '@teensyrom-nx/utils';

@Component({
  selector: 'lib-file-item',
  imports: [CommonModule, StorageItemComponent, StorageItemActionsComponent],
  templateUrl: './file-item.component.html',
  styleUrls: ['./file-item.component.scss'],
})
export class FileItemComponent {
  fileItem = input.required<FileItem>();
  selected = input<boolean>(false);

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
