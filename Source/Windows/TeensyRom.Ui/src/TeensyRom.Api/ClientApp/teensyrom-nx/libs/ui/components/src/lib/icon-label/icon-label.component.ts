import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  StyledIconComponent,
  StyledIconColor,
  StyledIconSize,
} from '../styled-icon/styled-icon.component';

@Component({
  selector: 'lib-icon-label',
  standalone: true,
  imports: [CommonModule, StyledIconComponent],
  templateUrl: './icon-label.component.html',
  styleUrl: './icon-label.component.scss',
})
export class IconLabelComponent {
  icon = input<string>('');
  label = input<string>('');
  color = input<StyledIconColor>('normal');
  size = input<StyledIconSize>('medium');
  truncate = input<boolean>(true);
}
