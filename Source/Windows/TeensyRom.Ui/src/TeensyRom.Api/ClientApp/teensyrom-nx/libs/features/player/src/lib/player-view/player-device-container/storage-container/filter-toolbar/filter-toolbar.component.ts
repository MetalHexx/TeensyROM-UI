import { Component, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { 
  CompactCardLayoutComponent, 
  IconButtonComponent, 
  JoystickIconComponent,
  ImageIconComponent
} from '@teensyrom-nx/ui/components';
import { PLAYER_CONTEXT } from '@teensyrom-nx/application';
import { RandomRollButtonComponent } from './random-roll-button';

@Component({
  selector: 'lib-filter-toolbar',
  imports: [CommonModule, CompactCardLayoutComponent, IconButtonComponent, JoystickIconComponent, ImageIconComponent, RandomRollButtonComponent],
  templateUrl: './filter-toolbar.component.html',
  styleUrl: './filter-toolbar.component.scss',
})
export class FilterToolbarComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);
  
  deviceId = input.required<string>();

  onAllClick(): void {
    console.log('All filter clicked');
    // TODO: Implement all files filter logic
  }

  onGamesClick(): void {
    console.log('Games filter clicked');
    // TODO: Implement games filter logic
  }

  onMusicClick(): void {
    console.log('Music filter clicked');
    // TODO: Implement music filter logic
  }

  onImagesClick(): void {
    console.log('Images filter clicked');
    // TODO: Implement images filter logic
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