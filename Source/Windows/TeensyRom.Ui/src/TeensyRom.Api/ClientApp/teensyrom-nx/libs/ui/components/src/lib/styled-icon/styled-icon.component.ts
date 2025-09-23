import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

export type StyledIconSize = 'small' | 'medium' | 'large';
export type StyledIconColor =
  | 'normal'
  | 'primary'
  | 'highlight'
  | 'success'
  | 'error'
  | 'dimmed'
  | 'directory';

@Component({
  selector: 'lib-styled-icon',
  imports: [CommonModule, MatIconModule],
  templateUrl: './styled-icon.component.html',
  styleUrl: './styled-icon.component.scss',
})
export class StyledIconComponent {
  icon = input.required<string>();
  color = input<StyledIconColor>('normal');
  size = input<StyledIconSize>('medium');

  iconClasses = computed(() => {
    const classes: string[] = [];

    switch (this.size()) {
      case 'small':
        classes.push('styled-icon-small');
        break;
      case 'medium':
        classes.push('styled-icon-medium');
        break;
      case 'large':
        classes.push('styled-icon-large');
        break;
    }

    switch (this.color()) {
      case 'primary':
        classes.push('styled-icon-primary');
        break;
      case 'highlight':
        classes.push('styled-icon-highlight');
        break;
      case 'success':
        classes.push('styled-icon-success');
        break;
      case 'error':
        classes.push('styled-icon-error');
        break;
      case 'dimmed':
        classes.push('styled-icon-dimmed');
        break;
      case 'directory':
        classes.push('styled-icon-directory');
        break;
      case 'normal':
        break;
    }

    return classes.join(' ');
  });
}
