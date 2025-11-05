import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

/**
 * Size variant for the empty state message component
 */
export type EmptyStateMessageSize = 'small' | 'medium' | 'large';

/**
 * Empty State Message Component
 *
 * A reusable component for displaying centered empty state messages with an icon,
 * title, and optional descriptive text. Commonly used for "no data" scenarios like
 * empty lists, no search results, or missing connections.
 *
 * @example
 * ```html
 * <!-- Basic usage with required fields -->
 * <lib-empty-state-message
 *   icon="devices"
 *   title="No Connected Devices">
 * </lib-empty-state-message>
 *
 * <!-- Full usage with all optional fields -->
 * <lib-empty-state-message
 *   icon="search_off"
 *   title="No Results Found"
 *   message="Try adjusting your search terms."
 *   secondaryMessage="Visit the Device View to manage your devices."
 *   size="large">
 * </lib-empty-state-message>
 * ```
 */
@Component({
  selector: 'lib-empty-state-message',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './empty-state-message.component.html',
  styleUrl: './empty-state-message.component.scss',
})
export class EmptyStateMessageComponent {
  /**
   * Material Design icon name to display
   */
  icon = input.required<string>();

  /**
   * Primary title text (required)
   */
  title = input.required<string>();

  /**
   * Optional message text displayed below the title
   */
  message = input<string>();

  /**
   * Optional secondary message text (typically dimmed/smaller)
   */
  secondaryMessage = input<string>();

  /**
   * Size variant for the component
   * @default 'medium'
   */
  size = input<EmptyStateMessageSize>('medium');
}
