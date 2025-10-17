import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconLabelComponent } from '../icon-label/icon-label.component';
import { StyledIconColor } from '../styled-icon/styled-icon.component';

@Component({
  selector: 'lib-link',
  standalone: true,
  imports: [CommonModule, IconLabelComponent],
  templateUrl: './link.component.html',
  styleUrl: './link.component.scss',
})
export class LinkComponent {
  // Required inputs
  label = input.required<string>();

  // Optional inputs
  icon = input<string>('link');
  iconColor = input<StyledIconColor>('primary');
}
