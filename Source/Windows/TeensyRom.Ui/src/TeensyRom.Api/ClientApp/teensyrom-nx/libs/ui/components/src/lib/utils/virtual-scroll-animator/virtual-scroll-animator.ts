import { Signal, WritableSignal } from '@angular/core';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';

/**
 * A reusable utility for animating scroll-to-item in CDK Virtual Scroll viewports.
 *
 * **Features**:
 * - Smooth scrolling to specific items with centering
 * - Dynamic height measurement for theme/font changes
 * - Fallback handling for various edge cases
 * - Loading state management during scroll animation
 *
 * **Use Cases**:
 * - Auto-scroll to currently playing item in media lists
 * - Scroll to selected item in product catalogs
 * - Jump to search result in large lists
 * - Navigate to specific message in chat history
 *
 * @example
 * ```typescript
 * const animator = new VirtualScrollAnimator<FileItem>();
 *
 * animator.scrollToItem({
 *   viewport: this.viewport,
 *   items: this.files(),
 *   findIndex: (items) => items.findIndex(f => f.id === targetId),
 *   itemHeight: 52,
 *   isScrollingSignal: this._isScrolling,
 *   onComplete: () => console.log('Scroll complete')
 * });
 * ```
 */
export class VirtualScrollAnimator<T> {
  private scrollTimeoutId: number | null = null;

  /**
   * Scrolls the virtual viewport to center a specific item.
   *
   * @param config - Configuration object
   */
  scrollToItem(config: ScrollToItemConfig<T>): void {
    const {
      viewport,
      items,
      findIndex,
      itemHeight,
      isScrollingSignal,
      scrollDuration = 600,
      renderDelay = 100,
      onComplete,
    } = config;

    // Clear any existing scroll timeout
    this.cancelPendingScroll();

    // Mark that scrolling has started
    isScrollingSignal.set(true);

    // Use requestAnimationFrame to ensure DOM is fully rendered
    requestAnimationFrame(() => {
      this.scrollTimeoutId = window.setTimeout(() => {
        const viewportInstance = viewport();

        if (!viewportInstance) {
          // Viewport not ready
          this.completeScroll(isScrollingSignal, onComplete);
          return;
        }

        // Find target item index
        const targetIndex = findIndex(items);

        if (targetIndex === -1) {
          // Item not found
          this.completeScroll(isScrollingSignal, onComplete);
          return;
        }

        // Measure actual item height (handles theme changes, font scaling)
        const actualItemHeight = this.measureItemHeight(itemHeight);

        // Force viewport to check its size
        viewportInstance.checkViewportSize();
        const viewportHeight = viewportInstance.getViewportSize();

        // Fallback to simple scroll if viewport not fully rendered
        if (viewportHeight < 100) {
          viewportInstance.scrollToIndex(targetIndex, 'smooth');
          this.scheduleScrollComplete(isScrollingSignal, scrollDuration, onComplete);
          return;
        }

        // Calculate offset to center the item in viewport
        const offsetToCenter = Math.max(0, viewportHeight / 2 - actualItemHeight / 2);
        const targetOffset = Math.max(0, targetIndex * actualItemHeight - offsetToCenter);

        // Scroll to the calculated offset to center the item
        viewportInstance.scrollToOffset(targetOffset, 'smooth');

        // Mark scroll as complete after animation duration
        this.scheduleScrollComplete(isScrollingSignal, scrollDuration, onComplete);
      }, renderDelay);
    });
  }

  /**
   * Cancels any pending scroll animation and cleans up.
   */
  cancelPendingScroll(): void {
    if (this.scrollTimeoutId !== null) {
      clearTimeout(this.scrollTimeoutId);
      this.scrollTimeoutId = null;
    }
  }

  /**
   * Cleans up resources. Call this in ngOnDestroy.
   */
  destroy(): void {
    this.cancelPendingScroll();
  }

  /**
   * Measures actual item height from DOM, with fallback to provided default.
   */
  private measureItemHeight(defaultHeight: number, selector = '.file-list-item'): number {
    const firstRenderedElement = document.querySelector(selector);

    if (firstRenderedElement) {
      return firstRenderedElement.getBoundingClientRect().height;
    }

    return defaultHeight;
  }

  /**
   * Schedules scroll completion after animation duration.
   */
  private scheduleScrollComplete(
    isScrollingSignal: WritableSignal<boolean>,
    duration: number,
    onComplete?: () => void
  ): void {
    this.scrollTimeoutId = window.setTimeout(() => {
      this.completeScroll(isScrollingSignal, onComplete);
    }, duration);
  }

  /**
   * Marks scroll as complete and triggers callback.
   */
  private completeScroll(
    isScrollingSignal: WritableSignal<boolean>,
    onComplete?: () => void
  ): void {
    isScrollingSignal.set(false);
    this.scrollTimeoutId = null;
    onComplete?.();
  }
}

/**
 * Configuration for scrollToItem method.
 */
export interface ScrollToItemConfig<T> {
  /** Signal containing the viewport reference */
  viewport: Signal<CdkVirtualScrollViewport | undefined>;

  /** Array of items to search through */
  items: T[];

  /** Function to find the target item's index */
  findIndex: (items: T[]) => number;

  /** Configured item height in pixels */
  itemHeight: number;

  /** Signal to track scrolling state */
  isScrollingSignal: WritableSignal<boolean>;

  /** Duration of scroll animation in ms (default: 600) */
  scrollDuration?: number;

  /** Delay before starting scroll to ensure DOM is ready (default: 100) */
  renderDelay?: number;

  /** Optional callback when scroll completes */
  onComplete?: () => void;
}
