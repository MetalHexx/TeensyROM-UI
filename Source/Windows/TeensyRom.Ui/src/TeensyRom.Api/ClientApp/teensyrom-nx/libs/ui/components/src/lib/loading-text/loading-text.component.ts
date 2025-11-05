import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeetTextContainerComponent } from '../leet-text-container/leet-text-container.component';

/**
 * Loading text component with fade-in/fade-out animation and leet-speak cycling effect.
 * Wraps the leet-text-container with elegant fade animations suitable for corner slots and loading indicators.
 *
 * Displays "Loading..." by default, but custom text can be provided via ng-content.
 *
 * @example
 * ```html
 * <!-- Default "Loading..." text -->
 * <lib-loading-text [visible]="isLoading()"></lib-loading-text>
 *
 * <!-- In a corner slot with default text -->
 * <lib-scaling-card title="Data">
 *   <lib-loading-text slot="corner" [visible]="isLoading()"></lib-loading-text>
 * </lib-scaling-card>
 *
 * <!-- Custom text -->
 * <lib-loading-text [visible]="isLoading()">Processing...</lib-loading-text>
 *
 * <!-- With custom animation speed -->
 * <lib-loading-text [visible]="isLoading()" [showSpinner]="true" [animationDuration]="500">
 *   Saving...
 * </lib-loading-text>
 * ```
 */
@Component({
  selector: 'lib-loading-text',
  imports: [CommonModule, LeetTextContainerComponent],
  templateUrl: './loading-text.component.html',
  styleUrl: './loading-text.component.scss',
  host: {
    '[class.visible]': 'visible()',
  },
})
export class LoadingTextComponent {
  /**
   * Controls visibility with fade animation
   * - true: Fade in and show
   * - false: Fade out and hide
   */
  visible = input<boolean>(false);

  /**
   * Show animated spinner before the text (default: true)
   */
  showSpinner = input<boolean>(true);

  /**
   * Duration of the leet cycling animation in milliseconds (default: 1000)
   */
  animationDuration = input<number>(1000);
}
