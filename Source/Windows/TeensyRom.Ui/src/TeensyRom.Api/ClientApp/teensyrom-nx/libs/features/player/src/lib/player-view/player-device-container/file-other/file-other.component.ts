import { Component, input, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScalingCardComponent, EmptyStateMessageComponent } from '@teensyrom-nx/ui/components';
import { MatChipsModule } from '@angular/material/chips';
import { PLAYER_CONTEXT } from '@teensyrom-nx/application';

@Component({
  selector: 'lib-file-other',
  imports: [CommonModule, ScalingCardComponent, MatChipsModule, EmptyStateMessageComponent],
  templateUrl: './file-other.component.html',
  styleUrl: './file-other.component.scss',
})
export class FileOtherComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);

  // Required input for device identification
  deviceId = input.required<string>();

  // Computed signals derived from current file
  private readonly currentFile = computed(() => this.playerContext.getCurrentFile(this.deviceId())());

  readonly filename = computed(() => this.currentFile()?.file.name ?? '');
  readonly title = computed(() => this.currentFile()?.file.title ?? '');
  readonly releaseInfo = computed(() => this.currentFile()?.file.releaseInfo ?? '');
  readonly description = computed(() => this.currentFile()?.file.description ?? '');
  readonly meta1 = computed(() => this.currentFile()?.file.meta1 ?? '');
  readonly meta2 = computed(() => this.currentFile()?.file.meta2 ?? '');
  readonly metadataSource = computed(() => this.currentFile()?.file.metadataSource ?? '');
  readonly hasContent = computed(() => {
    const file = this.currentFile();
    return !!(file?.file.title || file?.file.description);
  });
  readonly hasFile = computed(() => !!this.currentFile());
}
