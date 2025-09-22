import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { IconLabelComponent } from '../icon-label/icon-label.component';

export type ActionButtonVariant = 'stroked' | 'flat' | 'raised' | 'fab';
export type ActionButtonColor = 'primary' | 'success' | 'error' | 'highlight' | 'normal';

@Component({
  selector: 'lib-action-button',
  imports: [CommonModule, MatButtonModule, IconLabelComponent],
  templateUrl: './action-button.component.html',
  styleUrl: './action-button.component.scss',
})
export class ActionButtonComponent {
  // Required inputs
  icon = input.required<string>();
  label = input.required<string>();

  // Optional inputs
  variant = input<ActionButtonVariant>('stroked');
  color = input<ActionButtonColor>('primary');
  disabled = input<boolean>(false);
  ariaLabel = input<string>();

  // Events
  buttonClick = output<void>();

  // Computed properties
  buttonClasses = computed(() => {
    const classes: string[] = [];

    // Only add color class for non-primary colors that need custom styling
    const color = this.color();
    if (color !== 'primary' && color !== 'normal') {
      classes.push(`action-button-${color}`);
    }

    return classes.join(' ');
  });

  materialColor = computed(() => {
    // Use Material's natural color system for primary and normal
    // Only override for semantic colors that need custom styling
    switch (this.color()) {
      case 'primary':
        return 'primary'; // Use Material's primary color
      case 'normal':
        return undefined; // Use Material's default styling
      case 'success':
      case 'error':
      case 'highlight':
        return 'primary'; // Use primary as base, let CSS override the colors
      default:
        return 'primary';
    }
  });

  effectiveAriaLabel = computed(() => this.ariaLabel() || this.label());

  onButtonClick(): void {
    if (!this.disabled()) {
      this.buttonClick.emit();
    }
  }
}
