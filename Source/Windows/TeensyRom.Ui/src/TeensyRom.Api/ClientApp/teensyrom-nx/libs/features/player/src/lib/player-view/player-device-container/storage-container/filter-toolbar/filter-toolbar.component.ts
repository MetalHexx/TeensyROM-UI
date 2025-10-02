import { Component, inject, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ScalingCompactCardComponent,
  IconButtonComponent,
  IconButtonColor,
  JoystickIconComponent,
  ImageIconComponent
} from '@teensyrom-nx/ui/components';
import { PLAYER_CONTEXT } from '@teensyrom-nx/application';
import { PlayerFilterType } from '@teensyrom-nx/domain';
import { RandomRollButtonComponent } from './random-roll-button';

@Component({
  selector: 'lib-filter-toolbar',
  imports: [CommonModule, ScalingCompactCardComponent, IconButtonComponent, JoystickIconComponent, ImageIconComponent, RandomRollButtonComponent],
  templateUrl: './filter-toolbar.component.html',
  styleUrl: './filter-toolbar.component.scss',
})
export class FilterToolbarComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);

  deviceId = input.required<string>();

  // Expose PlayerFilterType enum for template access
  readonly PlayerFilterType = PlayerFilterType;

  // Computed signal for active filter
  activeFilter = computed(() =>
    this.playerContext.getShuffleSettings(this.deviceId())()?.filter ?? PlayerFilterType.All
  );

  // Computed signal for error state
  hasError = computed(() =>
    this.playerContext.getError(this.deviceId())() !== null
  );

  // Helper method to determine button color (error takes precedence over active state)
  getButtonColor(filterType: PlayerFilterType): IconButtonColor {
    if (this.hasError()) return 'error';
    return this.activeFilter() === filterType ? 'highlight' : 'normal';
  }

  onAllClick(): void {
    this.playerContext.setFilterMode(this.deviceId(), PlayerFilterType.All);
  }

  onGamesClick(): void {
    this.playerContext.setFilterMode(this.deviceId(), PlayerFilterType.Games);
  }

  onMusicClick(): void {
    this.playerContext.setFilterMode(this.deviceId(), PlayerFilterType.Music);
  }

  onImagesClick(): void {
    this.playerContext.setFilterMode(this.deviceId(), PlayerFilterType.Images);
  }

  async onRandomLaunchClick(): Promise<void> {
    console.log('🚀 LaunchRandomFile method called!');
    const deviceId = this.deviceId();
    console.log('📱 Device ID:', deviceId);
    
    if (deviceId) {
      try {
        await this.playerContext.launchRandomFile(deviceId);
      } catch (error) {
        console.error('Failed to launch random file:', error);
      }
    } else {
      console.log('❌ No device ID provided, skipping launch');
    }
  }
}