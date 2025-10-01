import { Component, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { 
  CompactCardLayoutComponent, 
  IconButtonComponent, 
  JoystickIconComponent,
  ImageIconComponent
} from '@teensyrom-nx/ui/components';
import { PLAYER_CONTEXT } from '@teensyrom-nx/application';

@Component({
  selector: 'lib-filter-toolbar',
  imports: [CommonModule, CompactCardLayoutComponent, IconButtonComponent, JoystickIconComponent, ImageIconComponent],
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

  async launchRandomFile(): Promise<void> {
    const deviceId = this.deviceId();
    if (deviceId) {
      try {
        await this.playerContext.launchRandomFile(deviceId);
      } catch (error) {
        console.error('Failed to launch random file:', error);
      }
    }
  }
}