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
  creatorName = computed(() => {
    const creator = this.currentFile()?.file.creator;
    return creator && creator.trim().length > 0 ? creator : 'Welcome to TeensyROM!';
  });
  metadataSource = computed(() => {
    const images = this.currentFile()?.file.images;
    if (images && images.length > 0) {
      const source = images[0].source;
      return source && source.trim().length > 0 ? source : 'hExx';
    }
    return 'hExx';
  });
  imageUrls = computed(
    () =>
      this.currentFile()
        ?.file.images.map((img) => img.url)
        .filter((url: string) => url && url.length > 0) ?? []
  );
  hasImages = computed(() => this.imageUrls().length > 0);
}
