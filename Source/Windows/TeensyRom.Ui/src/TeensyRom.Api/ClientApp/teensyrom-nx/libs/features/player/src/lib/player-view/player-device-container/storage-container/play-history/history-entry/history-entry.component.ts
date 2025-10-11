import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StorageItemComponent, StorageItemActionsComponent } from '@teensyrom-nx/ui/components';
import { FileItemType } from '@teensyrom-nx/domain';
import { HistoryEntry } from '@teensyrom-nx/application';

@Component({
  selector: 'lib-history-entry',
  imports: [CommonModule, StorageItemComponent, StorageItemActionsComponent],
  templateUrl: './history-entry.component.html',
  styleUrls: ['./history-entry.component.scss'],
})
export class HistoryEntryComponent {
  // Inputs
  entry = input.required<HistoryEntry>();
  selected = input<boolean>(false);
  isCurrentlyPlaying = input<boolean>(false);

  // Outputs
  entrySelected = output<HistoryEntry>();
  entryDoubleClick = output<HistoryEntry>();

  // Computed signals
  readonly fileIcon = computed(() => {
    switch (this.entry().file.type) {
      case FileItemType.Song:
        return 'music_note';
      case FileItemType.Game:
        return 'sports_esports';
      case FileItemType.Image:
        return 'image';
      case FileItemType.Hex:
        return 'code';
      case FileItemType.Unknown:
      default:
        return 'insert_drive_file';
    }
  });

  readonly formattedTimestamp = computed(() => {
    const timestamp = this.entry().timestamp;
    const date = new Date(timestamp);
    return date.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
    });
  });

  readonly formattedSize = computed(() => {
    return this.formatFileSize(this.entry().file.size);
  });

  // Event handlers
  onEntryClick(): void {
    this.entrySelected.emit(this.entry());
  }

  onEntryDoubleClick(): void {
    this.entryDoubleClick.emit(this.entry());
  }

  // Private helpers
  private formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`;
  }
}
