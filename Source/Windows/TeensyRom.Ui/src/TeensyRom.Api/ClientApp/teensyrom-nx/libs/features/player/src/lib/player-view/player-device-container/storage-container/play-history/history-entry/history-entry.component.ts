import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StorageItemComponent, StorageItemActionsComponent } from '@teensyrom-nx/ui/components';
import { getFileIcon, formatFileSize } from '@teensyrom-nx/utils';
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
  readonly fileIcon = computed(() => getFileIcon(this.entry().file.type));

  readonly formattedTimestamp = computed(() => {
    const timestamp = this.entry().timestamp;
    const date = new Date(timestamp);
    return date.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
    });
  });

  readonly formattedSize = computed(() => formatFileSize(this.entry().file.size));

  // Event handlers
  onEntryClick(): void {
    this.entrySelected.emit(this.entry());
  }

  onEntryDoubleClick(): void {
    this.entryDoubleClick.emit(this.entry());
  }
}
