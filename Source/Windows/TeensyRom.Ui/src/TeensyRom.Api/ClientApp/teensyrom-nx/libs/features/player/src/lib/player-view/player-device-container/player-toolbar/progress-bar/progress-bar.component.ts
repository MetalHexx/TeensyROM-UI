import { Component, input, computed } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';

/**
 * Generic progress bar component for displaying progress percentage.
 * Fully presentational - receives progress data via inputs, no dependencies on player context.
 *
 * Design:
 * - Flush to top positioning (absolute)
 * - 4px height for subtle visual presence
 * - Uses --color-primary-bright theme variable
 * - Border-radius matches card corners (4px)
 *
 * @example
 * ```html
 * <lib-progress-bar
 *   [currentValue]="50"
 *   [totalValue]="100"
 *   [show]="true">
 * </lib-progress-bar>
 * ```
 */
@Component({
  selector: 'lib-progress-bar',
  imports: [MatProgressBarModule],
  templateUrl: './progress-bar.component.html',
  styleUrl: './progress-bar.component.scss',
})
export class ProgressBarComponent {
  /** Current progress value (e.g., elapsed time in seconds) */
  currentValue = input<number>(0);

  /** Total/max progress value (e.g., total duration in seconds) */
  totalValue = input<number>(0);

  /** Whether to show the progress bar */
  show = input<boolean>(false);

  /** Computed progress percentage (0-100) */
  progressPercent = computed(() => {
    const current = this.currentValue();
    const total = this.totalValue();

    if (total === 0) return 0;
    return (current / total) * 100;
  });
}
