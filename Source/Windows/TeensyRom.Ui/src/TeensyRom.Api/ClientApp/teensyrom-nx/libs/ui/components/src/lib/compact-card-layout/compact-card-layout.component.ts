import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'lib-compact-card-layout',
  imports: [CommonModule, MatCardModule],
  templateUrl: './compact-card-layout.component.html',
  styleUrl: './compact-card-layout.component.scss',
  host: {
    '[class.no-overflow]': 'enableOverflow() === false',
  }
})
export class CompactCardLayoutComponent {
  enableOverflow = input<boolean>(true);
}
