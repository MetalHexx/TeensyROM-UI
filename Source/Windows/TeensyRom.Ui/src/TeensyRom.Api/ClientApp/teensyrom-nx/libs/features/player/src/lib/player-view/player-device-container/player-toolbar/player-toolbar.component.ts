import { Component, inject, input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CompactCardLayoutComponent, IconButtonComponent, IconButtonColor, SlidingContainerComponent } from '@teensyrom-nx/ui/components';
import { PLAYER_CONTEXT } from '@teensyrom-nx/application';
import { LaunchMode, PlayerStatus, FileItemType } from '@teensyrom-nx/domain';

@Component({
  selector: 'lib-player-toolbar',
  imports: [CommonModule, CompactCardLayoutComponent, IconButtonComponent, SlidingContainerComponent],
  templateUrl: './player-toolbar.component.html',
  styleUrl: './player-toolbar.component.scss'
})
export class PlayerToolbarComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);
  
  deviceId = input.required<string>();

  showPlayer = signal(false);

  onContainerAnimationComplete(): void {
    this.showPlayer.set(true);
  }

  toggleShuffleMode(): void {
    const deviceId = this.deviceId();
    if (deviceId) {
      this.playerContext.toggleShuffleMode(deviceId);
    }
  }

  isShuffleMode(): boolean {
    const deviceId = this.deviceId();
    if (!deviceId) return false;
    
    return this.playerContext.getLaunchMode(deviceId)() === LaunchMode.Shuffle;
  }

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
    return (fileContext !== null && fileContext.files.length > 1) || launchMode === LaunchMode.Shuffle;
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

  hasError = computed(() =>
    this.playerContext.getError(this.deviceId())() !== null
  );

  getButtonColor(): IconButtonColor {
    return this.hasError() ? 'error' : 'normal';
  }

}
