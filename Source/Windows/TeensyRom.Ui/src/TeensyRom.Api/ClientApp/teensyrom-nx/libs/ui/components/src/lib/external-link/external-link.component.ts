import { Component, computed, input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconLabelComponent } from '../icon-label/icon-label.component';
import { StyledIconColor } from '../styled-icon/styled-icon.component';

@Component({
  selector: 'lib-external-link',
  standalone: true,
  imports: [CommonModule, IconLabelComponent],
  templateUrl: './external-link.component.html',
  styleUrl: './external-link.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'link',
  },
})
export class ExternalLinkComponent {
  // Required inputs
  href = input.required<string>();
  label = input.required<string>();

  // Optional inputs
  icon = input<string>('link');
  iconColor = input<StyledIconColor>('primary');
  target = input<'_blank' | '_self'>('_blank');
  ariaLabel = input<string>('');

  // Computed signals for link behavior
  isExternal = computed(() => {
    const url = this.href();
    return url.startsWith('http://') || url.startsWith('https://');
  });

  relAttribute = computed(() => {
    if (this.isExternal() && this.target() === '_blank') {
      return 'noopener noreferrer';
    }
    return undefined;
  });

  effectiveTarget = computed(() => this.target());

  // Accessibility: Generate appropriate aria-label for external links
  ariaLabelText = computed(() => {
    const customLabel = this.ariaLabel();
    if (customLabel) return customLabel;
    
    const label = this.label();
    if (this.isExternal() && this.target() === '_blank') {
      return `${label} (opens in new window)`;
    }
    return label;
  });
}
