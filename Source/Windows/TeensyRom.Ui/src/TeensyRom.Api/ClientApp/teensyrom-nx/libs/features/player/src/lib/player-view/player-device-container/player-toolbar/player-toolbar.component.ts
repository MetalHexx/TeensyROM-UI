import { Component, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CompactCardLayoutComponent, IconButtonComponent } from '@teensyrom-nx/ui/components';
import { IPlayerContext, PLAYER_CONTEXT } from '@teensyrom-nx/application';
import { LaunchMode } from '@teensyrom-nx/domain';

@Component({
  selector: 'lib-player-toolbar',
  imports: [CommonModule, CompactCardLayoutComponent, IconButtonComponent],
  templateUrl: './player-toolbar.component.html',
  styleUrl: './player-toolbar.component.scss',
})
export class PlayerToolbarComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);
  
  deviceId = input.required<string>();

  async launchRandomFile(): Promise<void> {
    const deviceId = this.deviceId();
    if (deviceId) {
      await this.playerContext.launchRandomFile(deviceId);
    }
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
}
