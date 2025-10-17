import { Component, computed, input, ChangeDetectionStrategy, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LinkComponent } from '../link/link.component';
import { StyledIconColor } from '../styled-icon/styled-icon.component';

@Component({
  selector: 'lib-action-link',
  standalone: true,
  imports: [CommonModule, LinkComponent],
  templateUrl: './action-link.component.html',
  styleUrl: './action-link.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActionLinkComponent {
  // Required inputs
  label = input.required<string>();

  // Optional inputs
  icon = input<string>('link');
  iconColor = input<StyledIconColor>('primary');
  ariaLabel = input<string>('');
  disabled = input<boolean>(false);

  // Events
  linkClick = output<void>();

  // Computed accessibility
  ariaLabelText = computed(() => {
    const customLabel = this.ariaLabel();
    return customLabel || this.label();
  });

  onButtonClick(): void {
    if (!this.disabled()) {
      this.linkClick.emit();
    }
  }
}
