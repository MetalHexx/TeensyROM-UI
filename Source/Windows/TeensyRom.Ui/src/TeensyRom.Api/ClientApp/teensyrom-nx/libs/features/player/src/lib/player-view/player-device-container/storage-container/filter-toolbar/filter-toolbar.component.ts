import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { 
  CompactCardLayoutComponent, 
  IconButtonComponent, 
  JoystickIconComponent,
  ImageIconComponent
} from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-filter-toolbar',
  imports: [CommonModule, CompactCardLayoutComponent, IconButtonComponent, JoystickIconComponent, ImageIconComponent],
  templateUrl: './filter-toolbar.component.html',
  styleUrl: './filter-toolbar.component.scss',
})
export class FilterToolbarComponent {
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
}