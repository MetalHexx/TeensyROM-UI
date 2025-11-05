import { Component, input } from '@angular/core';

/**
 * Directive component for projecting action buttons/content into the storage item.
 * Similar to mat-card-actions pattern from Angular Material.
 * Can display optional text label along with action buttons.
 *
 * @example
 * <lib-storage-item icon="music_note" label="Song.sid">
 *   <lib-storage-item-actions label="1.5 KB">
 *     <button>Play</button>
 *     <button>Download</button>
 *   </lib-storage-item-actions>
 * </lib-storage-item>
 */
@Component({
  selector: 'lib-storage-item-actions',
  template: `
    @if (label()) {
    <span class="actions-label">{{ label() }}</span>
    }
    <ng-content></ng-content>
  `,
  styles: [
    `
      :host {
        display: flex;
        align-items: center;
        gap: 0.5rem;
      }

      .actions-label {
        color: var(--color-dimmed);
        font-size: 0.875rem;
        white-space: nowrap;
      }
    `,
  ],
})
export class StorageItemActionsComponent {
  /** Optional text label to display before actions (e.g., file size, item count) */
  label = input<string>();
}
