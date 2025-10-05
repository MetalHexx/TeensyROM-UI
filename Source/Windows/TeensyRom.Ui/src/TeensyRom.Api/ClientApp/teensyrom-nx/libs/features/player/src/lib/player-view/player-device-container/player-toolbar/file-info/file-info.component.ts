import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { FileItem } from '@teensyrom-nx/domain';
import { CycleImageComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-file-info',
  imports: [CommonModule, CycleImageComponent],
  templateUrl: './file-info.component.html',
  styleUrl: './file-info.component.scss'
})
export class FileInfoComponent {
  fileItem = input<FileItem | null>();

  // Get all image URLs for cycling
  imageUrls = computed(() => {
    const item = this.fileItem();
    if (!item?.images || item.images.length === 0) {
      return [];
    }
    return item.images.map(img => img.url);
  });
}
