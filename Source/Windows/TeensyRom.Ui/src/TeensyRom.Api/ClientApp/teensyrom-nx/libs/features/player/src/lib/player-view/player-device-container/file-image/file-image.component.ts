import { Component, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CycleImageComponent, ScalingCardComponent } from '@teensyrom-nx/ui/components';
import type { LaunchedFile } from '@teensyrom-nx/application';

@Component({
  selector: 'lib-file-image',
  imports: [CommonModule, ScalingCardComponent, CycleImageComponent],
  templateUrl: './file-image.component.html',
  styleUrl: './file-image.component.scss',
})
export class FileImageComponent {
  // Inputs
  currentFile = input<LaunchedFile | null>();

  // Computed signals derived from input
  creatorName = computed(() => this.currentFile()?.file.creator ?? '');
  metadataSource = computed(() => this.currentFile()?.file.metadataSource ?? '');
  imageUrls = computed(() =>
    this.currentFile()?.file.images.map((img) => img.url).filter((url: string) => url && url.length > 0) ?? []
  );
  hasImages = computed(() => this.imageUrls().length > 0);
}
