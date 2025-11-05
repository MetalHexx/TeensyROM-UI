import { Component, input, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import {
  ScalingCardComponent,
  EmptyStateMessageComponent,
  ExternalLinkComponent,
  ActionLinkComponent,
} from '@teensyrom-nx/ui/components';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { PLAYER_CONTEXT } from '@teensyrom-nx/application';
import { FileItemType, YouTubeVideo } from '@teensyrom-nx/domain';
import { YouTubeDialogComponent } from './youtube-dialog/youtube-dialog.component';

@Component({
  selector: 'lib-file-other',
  imports: [
    CommonModule,
    ScalingCardComponent,
    MatChipsModule,
    MatIconModule,
    EmptyStateMessageComponent,
    ExternalLinkComponent,
    ActionLinkComponent,
  ],
  templateUrl: './file-other.component.html',
  styleUrl: './file-other.component.scss',
})
export class FileOtherComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);
  private readonly dialog = inject(MatDialog);

  // Required input for device identification
  deviceId = input.required<string>();

  // Computed signals derived from current file
  private readonly currentFile = computed(() =>
    this.playerContext.getCurrentFile(this.deviceId())()
  );

  readonly filename = computed(() => this.currentFile()?.file.name ?? '');
  readonly title = computed(() => this.currentFile()?.file.title ?? '');
  readonly releaseInfo = computed(() => this.currentFile()?.file.releaseInfo ?? '');
  readonly description = computed(() => this.currentFile()?.file.description ?? '');
  readonly meta1 = computed(() => this.currentFile()?.file.meta1 ?? '');
  readonly meta2 = computed(() => this.currentFile()?.file.meta2 ?? '');
  readonly metadataSource = computed(() => this.currentFile()?.file.metadataSource ?? '');
  readonly fileType = computed(() => this.currentFile()?.file.type);
  readonly isSong = computed(() => this.fileType() === FileItemType.Song);

  // NEW: DeepSID metadata computed signals
  readonly links = computed(() => this.currentFile()?.file.links ?? []);
  readonly tags = computed(() => this.currentFile()?.file.tags ?? []);
  readonly youTubeVideos = computed(() => this.currentFile()?.file.youTubeVideos ?? []);
  readonly competitions = computed(() => this.currentFile()?.file.competitions ?? []);
  readonly avgRating = computed(() => this.currentFile()?.file.avgRating);
  readonly ratingCount = computed(() => this.currentFile()?.file.ratingCount ?? 0);

  readonly hasContent = computed(() => {
    const file = this.currentFile();
    return !!(
      file?.file.title ||
      file?.file.description ||
      file?.file.links?.length ||
      file?.file.tags?.length ||
      file?.file.youTubeVideos?.length ||
      file?.file.competitions?.length
    );
  });
  readonly hasFile = computed(() => !!this.currentFile());

  openYouTubeDialog(video: YouTubeVideo): void {
    this.dialog.open(YouTubeDialogComponent, {
      data: { video },
      width: '800px',
      maxWidth: '90vw',
      panelClass: 'youtube-dialog',
      backdropClass: 'youtube-dialog-backdrop',
    });
  }
}
