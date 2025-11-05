import { Component, inject, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ScalingCompactCardComponent,
  IconButtonComponent,
  IconButtonColor,
  SlidingContainerComponent,
} from '@teensyrom-nx/ui/components';
import { PLAYER_CONTEXT } from '@teensyrom-nx/application';
import { LaunchMode, PlayerStatus, FileItemType } from '@teensyrom-nx/domain';
import { ProgressBarComponent } from './progress-bar/progress-bar.component';
import { FileInfoComponent } from './file-info/file-info.component';
import { FileTimeComponent } from './file-time/file-time.component';
import { PlayerToolbarActionsComponent } from './player-toolbar-actions/player-toolbar-actions.component';

@Component({
  selector: 'lib-player-toolbar',
  imports: [
    CommonModule,
    ScalingCompactCardComponent,
    IconButtonComponent,
    SlidingContainerComponent,
    ProgressBarComponent,
    FileInfoComponent,
    FileTimeComponent,
    PlayerToolbarActionsComponent,
  ],
  templateUrl: './player-toolbar.component.html',
  styleUrl: './player-toolbar.component.scss',
})
export class PlayerToolbarComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);

  deviceId = input.required<string>();

  isLoading(): boolean {
    const deviceId = this.deviceId();
    if (!deviceId) return false;

    return this.playerContext.isLoading(deviceId)();
  }

  // Phase 3: New playback control methods
  async playPause(): Promise<void> {
    const deviceId = this.deviceId();
    if (deviceId) {
      const status = this.playerContext.getPlayerStatus(deviceId)();

      if (status === PlayerStatus.Playing) {
        await this.playerContext.pause(deviceId);
      } else {
        await this.playerContext.play(deviceId);
      }
    }
  }

  async stop(): Promise<void> {
    const deviceId = this.deviceId();
    if (deviceId) {
      await this.playerContext.stop(deviceId);
    }
  }

  async next(): Promise<void> {
    const deviceId = this.deviceId();
    if (deviceId) {
      await this.playerContext.next(deviceId);
    }
  }

  async previous(): Promise<void> {
    const deviceId = this.deviceId();
    if (deviceId) {
      await this.playerContext.previous(deviceId);
    }
  }

  // UI helper methods for button display logic
  getPlayPauseIcon(): string {
    const deviceId = this.deviceId();
    if (!deviceId) return 'play_arrow';

    const status = this.playerContext.getPlayerStatus(deviceId)();
    return status === PlayerStatus.Playing ? 'pause' : 'play_arrow';
  }

  getPlayPauseLabel(): string {
    const deviceId = this.deviceId();
    if (!deviceId) return 'Play';

    const status = this.playerContext.getPlayerStatus(deviceId)();
    return status === PlayerStatus.Playing ? 'Pause' : 'Play';
  }

  isCurrentFileMusicType(): boolean {
    const deviceId = this.deviceId();
    if (!deviceId) return false;

    const currentFile = this.playerContext.getCurrentFile(deviceId)();
    return currentFile?.file?.type === FileItemType.Song;
  }

  canNavigate(): boolean {
    const deviceId = this.deviceId();
    if (!deviceId) return false;

    const fileContext = this.playerContext.getFileContext(deviceId)();
    const launchMode = this.playerContext.getLaunchMode(deviceId)();

    // Can navigate if we have file context (directory mode) or are in shuffle mode
    return (
      (fileContext !== null && fileContext.files.length > 1) || launchMode === LaunchMode.Shuffle
    );
  }

  canNavigatePrevious(): boolean {
    // Same logic as canNavigate for now - in shuffle mode, previous launches another random file
    return this.canNavigate();
  }

  getPlayerStatus(): PlayerStatus {
    const deviceId = this.deviceId();
    if (!deviceId) return PlayerStatus.Stopped;

    return this.playerContext.getPlayerStatus(deviceId)();
  }

  isPlayerLoaded(): boolean {
    const deviceId = this.deviceId();
    if (!deviceId) return false;

    const currentFile = this.playerContext.getCurrentFile(deviceId)();
    return currentFile !== null;
  }

  hasError = computed(() => this.playerContext.getError(this.deviceId())() !== null);

  isFileCompatible = computed(() => this.playerContext.isCurrentFileCompatible(this.deviceId())());

  getPlayButtonColor(): IconButtonColor {
    // Only show error (red) on play button when file is incompatible
    return !this.isFileCompatible() ? 'error' : 'normal';
  }

  // Phase 5: Timer state for progress bar
  timerState = computed(() => this.playerContext.getTimerState(this.deviceId())());

  showProgressBar = computed(() => {
    const state = this.timerState();
    return state !== null && state.showProgress;
  });

  currentTime = computed(() => this.timerState()?.currentTime ?? 0);
  totalTime = computed(() => this.timerState()?.totalTime ?? 0);

  // Current file for file-info component
  currentFile = computed(() => {
    const deviceId = this.deviceId();
    if (!deviceId) return null;
    return this.playerContext.getCurrentFile(deviceId)();
  });
}
