import { ChangeDetectionStrategy, Component, input, inject, computed, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PLAYER_CONTEXT, IPlayerContext, HistoryEntry } from '@teensyrom-nx/application';
import { ScalingCardComponent, EmptyStateMessageComponent } from '@teensyrom-nx/ui/components';
import { HistoryEntryComponent } from './history-entry/history-entry.component';

@Component({
  selector: 'lib-play-history',
  imports: [CommonModule, HistoryEntryComponent, ScalingCardComponent, EmptyStateMessageComponent],
  templateUrl: './play-history.component.html',
  styleUrls: ['./play-history.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayHistoryComponent {
  deviceId = input.required<string>();
  animationTrigger = input<boolean>(true);

  private readonly playerContext: IPlayerContext = inject(PLAYER_CONTEXT);

  // Computed signal for play history
  readonly playHistory = computed(() => 
    this.playerContext.getPlayHistory(this.deviceId())()
  );

  // Computed signal for history entries (reverse chronological - newest first)
  readonly historyEntries = computed(() => 
    this.playHistory()?.entries.slice().reverse() ?? []
  );

  // Computed signal for current playing file
  readonly currentPlayingFile = computed(() => 
    this.playerContext.getCurrentFile(this.deviceId())()
  );

  // Computed signal for current history position
  readonly currentHistoryPosition = computed(() => 
    this.playHistory()?.currentPosition ?? -1
  );

  // Computed signal for player error state
  readonly hasPlayerError = computed(() => 
    this.playerContext.getError(this.deviceId())() !== null
  );

  // Local signal for selected entry
  readonly selectedEntry = signal<HistoryEntry | null>(null);

  // Effect to automatically select and scroll to currently playing file
  constructor() {
    effect(() => {
      const entries = this.historyEntries();
      const currentPosition = this.currentHistoryPosition();

      // Only proceed if we have history entries and a valid position
      if (entries.length === 0 || currentPosition < 0) {
        return;
      }

      // Calculate the display index (reversed) from the actual position
      const displayIndex = entries.length - 1 - currentPosition;
      
      // Get the entry at the current position
      const playingEntry = entries[displayIndex];

      if (playingEntry) {
        // Select the playing entry
        this.selectedEntry.set(playingEntry);

        // Scroll to the selected entry using a unique identifier
        this.scrollToSelectedEntry(displayIndex);
      }
    });
  }

  onEntrySelected(entry: HistoryEntry): void {
    this.selectedEntry.set(entry);
  }

  async onEntryDoubleClick(entry: HistoryEntry, index: number): Promise<void> {
    const deviceId = this.deviceId();
    const entries = this.playHistory()?.entries ?? [];

    // Validate history exists
    if (entries.length === 0) {
      return;
    }

    // Calculate the actual index in the original array (reverse the index since display is reversed)
    const actualIndex = entries.length - 1 - index;

    // Navigate to this position in history
    await this.playerContext.navigateToHistoryPosition(deviceId, actualIndex);
  }

  isSelected(entry: HistoryEntry): boolean {
    return this.selectedEntry()?.file.path === entry.file.path && 
           this.selectedEntry()?.timestamp === entry.timestamp;
  }

  isCurrentlyPlaying(entry: HistoryEntry): boolean {
    const playingFile = this.currentPlayingFile();
    
    // Check if this entry matches the currently playing file by path and timestamp
    return playingFile?.file.path === entry.file.path && 
           playingFile?.launchedAt === entry.timestamp;
  }

  private scrollToSelectedEntry(index: number): void {
    // Use setTimeout to ensure the DOM is updated after the selection change
    setTimeout(() => {
      // Find the DOM element for the selected entry using the data-index attribute
      const targetElement = document.querySelector(`.history-list-item[data-index="${index}"]`);

      if (targetElement) {
        targetElement.scrollIntoView({
          behavior: 'smooth',
          block: 'center'
        });
      }
    }, 0);
  }
}
