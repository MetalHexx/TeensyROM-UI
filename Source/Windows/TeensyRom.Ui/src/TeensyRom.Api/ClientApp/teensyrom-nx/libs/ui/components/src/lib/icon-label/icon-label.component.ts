import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'lib-icon-label',
  standalone: true,
  imports: [MatIconModule, CommonModule],
  templateUrl: './icon-label.component.html',
  styleUrl: './icon-label.component.scss',
})
export class IconLabelComponent {
  icon = input<string>('');
  label = input<string>('');
}
