/**
 * A pure TypeScript utility for progressively rendering large arrays in batches
 * to avoid blocking the main thread.
 *
 * **Use Cases**:
 * - Rendering large lists (1000+ items)
 * - Transforming arrays with expensive operations
 * - Loading search results progressively
 * - Populating data grids or tables
 *
 * **How it works**:
 * - Splits array into configurable batch sizes (default: 50)
 * - Uses `requestAnimationFrame` to process batches across multiple frames
 * - Accumulates results and calls callback after each batch
 * - Returns cancellation token for cleanup
 *
 * @example
 * ```typescript
 * const renderer = new ProgressiveBatchRenderer<Product, DisplayProduct>();
 *
 * const cancellation = renderer.renderBatches(
 *   products,
 *   (product) => ({ ...product, displayPrice: formatPrice(product.price) }),
 *   (batch) => this.displayedProducts.set(batch),
 *   (completed) => this.isLoading.set(!completed),
 *   50 // batch size
 * );
 *
 * // Later, if needed:
 * cancellation.cancel();
 * ```
 */
export class ProgressiveBatchRenderer<TInput, TOutput> {
  /**
   * Renders an array progressively in batches using requestAnimationFrame.
   *
   * @param items - Source array to process
   * @param transform - Function to transform each item
   * @param onBatchUpdate - Called after each batch with accumulated results
   * @param onProgressUpdate - Called with completion status (true when done)
   * @param batchSize - Number of items to process per frame (default: 50)
   * @returns Cancellation token with cancel() method
   */
  renderBatches(
    items: TInput[],
    transform: (item: TInput, index: number) => TOutput,
    onBatchUpdate: (accumulatedItems: TOutput[]) => void,
    onProgressUpdate: (isComplete: boolean) => void,
    batchSize = 50
  ): BatchRenderCancellation {
    let rafId: number | null = null;
    let currentIndex = 0;
    const accumulated: TOutput[] = [];
    const totalItems = items.length;
    let isCancelled = false;

    // Signal that rendering has started
    onProgressUpdate(false);

    const processBatch = () => {
      if (isCancelled) {
        return;
      }

      const endIndex = Math.min(currentIndex + batchSize, totalItems);

      // Process batch
      for (let i = currentIndex; i < endIndex; i++) {
        accumulated.push(transform(items[i], i));
      }

      // Update with accumulated results
      onBatchUpdate([...accumulated]);

      currentIndex = endIndex;

      // Schedule next batch or complete
      if (currentIndex < totalItems) {
        rafId = requestAnimationFrame(processBatch);
      } else {
        rafId = null;
        onProgressUpdate(true);
      }
    };

    // Start processing
    rafId = requestAnimationFrame(processBatch);

    // Return cancellation token
    return {
      cancel: () => {
        isCancelled = true;
        if (rafId !== null) {
          cancelAnimationFrame(rafId);
          rafId = null;
        }
        onProgressUpdate(true); // Mark as complete on cancel
      },
    };
  }
}

/**
 * Token returned from renderBatches() that allows cancellation.
 */
export interface BatchRenderCancellation {
  /**
   * Cancels the progressive rendering and cleans up any pending RAF callbacks.
   */
  cancel(): void;
}
