import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export type IconButtonSize = 'small' | 'medium' | 'large';
export type IconButtonVariant = 'standard' | 'rounded-primary' | 'rounded-transparent';
export type IconButtonColor = 'normal' | 'highlight' | 'success' | 'error' | 'dimmed';

@Component({
  selector: 'lib-icon-button',
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './icon-button.component.html',
  styleUrl: './icon-button.component.scss',
})
export class IconButtonComponent {
  // Input properties
  icon = input<string>(); // Optional for cases using ng-content
  ariaLabel = input.required<string>();
  color = input<IconButtonColor>('normal');
  size = input<IconButtonSize>('medium');
  variant = input<IconButtonVariant>('standard');
  disabled = input<boolean>(false);

  // Events
  buttonClick = output<void>();

  // Computed properties
  hasIcon = computed(() => !!this.icon());

  // Computed classes
  buttonClasses = computed(() => {
    const classes: string[] = [];

    // Size classes
    switch (this.size()) {
      case 'small':
        classes.push('icon-button-small');
        break;
      case 'medium':
        classes.push('icon-button-medium');
        break;
      case 'large':
        classes.push('icon-button-large');
        break;
    }

    // Variant classes
    switch (this.variant()) {
      case 'rounded-primary':
        classes.push('icon-button-rounded-primary');
        break;
      case 'rounded-transparent':
        classes.push('icon-button-rounded-transparent');
        break;
      case 'standard':
        // Default Material Design styling
        break;
    }

    return classes.join(' ');
  });

  iconClasses = computed(() => {
    return this.color();
  });

  onButtonClick(): void {
    if (!this.disabled()) {
      this.buttonClick.emit();
    }
  }
}
