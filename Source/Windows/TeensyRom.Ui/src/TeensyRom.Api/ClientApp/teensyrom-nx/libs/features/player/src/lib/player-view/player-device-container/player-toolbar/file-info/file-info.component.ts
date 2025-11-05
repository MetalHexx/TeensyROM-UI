import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { FileItem } from '@teensyrom-nx/domain';
import { CycleImageComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-file-info',
  imports: [CommonModule, CycleImageComponent],
  templateUrl: './file-info.component.html',
  styleUrl: './file-info.component.scss',
})
export class FileInfoComponent {
  fileItem = input<FileItem | null>();

  // Get all image URLs for cycling
  imageUrls = computed(() => {
    const item = this.fileItem();
    if (!item?.images || item.images.length === 0) {
      return [];
    }
    return item.images.map((img) => img.url);
  });

  // Get friendly file type name from meta1 field
  fileTypeName = computed(() => {
    const meta1 = this.fileItem()?.meta1?.toLowerCase();
    if (!meta1) return '';

    switch (meta1) {
      case 'sid':
        return 'Music';
      case 'crt':
        return 'Cartridge';
      case 'prg':
        return 'Program';
      case 'p00':
        return 'Program';
      case 'hex':
        return 'Machine Code';
      case 'kla':
        return 'Koala Image';
      case 'koa':
        return 'Koala Image';
      case 'art':
        return 'Art Studio Image';
      case 'aas':
        return 'Art Studio Image';
      case 'hpi':
        return 'HiRes Image';
      case 'd64':
        return 'Disk Image';
      case 'seq':
        return 'Sequential File';
      case 'txt':
        return 'Text File';
      case 'zip':
        return 'Archive';
      case 'nfo':
        return 'Info File';
      case 'unknown':
        return 'Unknown';
      default:
        return meta1.toUpperCase();
    }
  });
}
