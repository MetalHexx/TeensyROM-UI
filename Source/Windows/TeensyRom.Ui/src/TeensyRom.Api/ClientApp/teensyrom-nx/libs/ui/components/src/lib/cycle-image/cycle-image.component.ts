import { Component, computed, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'lib-cycle-image',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cycle-image.component.html',
  styleUrl: './cycle-image.component.scss',
  animations: [
    trigger('fadeIn', [
      transition(':increment, :decrement', [
        style({ opacity: 0 }),
        animate('1s ease-out', style({ opacity: 1 })),
      ]),
    ]),
  ],
})
export class CycleImageComponent {
  // Inputs
  images = input.required<string[]>();
  intervalMs = input<number>(8000);
  placeholderUrl = input<string>('/placeholder.jpg');
  size = input<'thumbnail' | 'small' | 'medium' | 'large'>('large');

  // Signals for state management
  currentIndex = signal(0);
  previousIndex = signal<number | null>(null);
  showPrevious = signal(false);
  animationKey = signal(0);

  // Computed signals - use placeholder if no images
  private effectiveImages = computed(() => {
    const imgs = this.images();
    return imgs.length > 0 ? imgs : [this.placeholderUrl()];
  });

  currentImage = computed(() => this.effectiveImages()[this.currentIndex()] || null);
  previousImage = computed(() => {
    const idx = this.previousIndex();
    return idx !== null ? this.effectiveImages()[idx] || null : null;
  });
  hasMultipleImages = computed(() => this.effectiveImages().length > 1);

  // Simple mode disables blur/background effects for small sizes
  isSimpleMode = computed(() => {
    const sz = this.size();
    return sz === 'thumbnail' || sz === 'small';
  });

  // Dependency injection
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    // Reset when images change and trigger animation
    effect(() => {
      const imgs = this.images();
      this.currentIndex.set(0);
      this.previousIndex.set(null);
      this.showPrevious.set(false);
      this.animationKey.update((v) => v + 1);
    });

    // Start carousel when there are multiple images
    effect((onCleanup) => {
      if (!this.hasMultipleImages()) {
        return;
      }

      const subscription = interval(this.intervalMs())
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => {
          this.cycleToNext();
        });

      onCleanup(() => {
        subscription.unsubscribe();
      });
    });
  }

  private cycleToNext(): void {
    // Move current to previous
    this.previousIndex.set(this.currentIndex());
    this.showPrevious.set(true);

    // Update to next image
    const nextIndex = (this.currentIndex() + 1) % this.effectiveImages().length;
    this.currentIndex.set(nextIndex);
    this.animationKey.update((v) => v + 1);

    // After animation duration, hide previous
    setTimeout(() => {
      this.showPrevious.set(false);
    }, 1000); // CSS animation duration
  }
}
