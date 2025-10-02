import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'lib-card-layout',
  imports: [CommonModule, MatCardModule],
  templateUrl: './card-layout.component.html',
  styleUrl: './card-layout.component.scss',
  host: {
    '[class.no-overflow]': 'enableOverflow() === false',
  }
})
export class CardLayoutComponent {
  title = input<string>();
  subtitle = input<string>();
  metadataSource = input<string>();
  enableOverflow = input<boolean>(true);
}
