import { Component, computed, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CycleImageComponent, ScalingCardComponent } from '@teensyrom-nx/ui/components';
import { PLAYER_CONTEXT } from '@teensyrom-nx/application';

@Component({
  selector: 'lib-file-image',
  imports: [CommonModule, ScalingCardComponent, CycleImageComponent],
  templateUrl: './file-image.component.html',
  styleUrl: './file-image.component.scss',
})
export class FileImageComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);

  // Inputs
  deviceId = input.required<string>();
  creatorName = input<string>();
  metadataSource = input<string>('Metadata Source');

  // Computed signals derived from player context
  currentFile = computed(() => this.playerContext.getCurrentFile(this.deviceId())());
  imageUrls = computed(() =>
    this.currentFile()?.file.images.map(img => img.url).filter((url: string) => url && url.length > 0) ?? []
  );
  hasImages = computed(() => this.imageUrls().length > 0);
}
