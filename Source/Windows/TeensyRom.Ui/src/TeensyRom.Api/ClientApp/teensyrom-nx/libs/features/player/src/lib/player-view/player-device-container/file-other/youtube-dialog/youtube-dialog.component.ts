import { Component, Inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ScalingCardComponent, IconButtonComponent } from '@teensyrom-nx/ui/components';
import { YouTubeVideo } from '@teensyrom-nx/domain';

export interface YouTubeDialogData {
  video: YouTubeVideo;
}

@Component({
  selector: 'lib-youtube-dialog',
  standalone: true,
  imports: [CommonModule, ScalingCardComponent, IconButtonComponent],
  templateUrl: './youtube-dialog.component.html',
  styleUrl: './youtube-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class YouTubeDialogComponent {
  youtubeEmbedUrl: SafeResourceUrl;

  constructor(
    public dialogRef: MatDialogRef<YouTubeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: YouTubeDialogData,
    private sanitizer: DomSanitizer
  ) {
    const embedUrl = `https://www.youtube.com/embed/${this.data.video.videoId}`;
    this.youtubeEmbedUrl = this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
