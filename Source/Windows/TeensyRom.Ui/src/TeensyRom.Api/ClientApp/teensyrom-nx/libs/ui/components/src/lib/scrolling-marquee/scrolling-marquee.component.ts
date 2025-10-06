import {
  Component,
  input,
  signal,
  computed,
  effect,
  ElementRef,
  viewChild,
  afterNextRender,
  Injector,
} from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Available marquee effect types
 */
export type MarqueeEffect =
  | 'none'           // Basic scroll only
  | 'wave'           // Sine wave vertical motion
  | 'rainbow'        // Color spectrum cycling
  | 'glitch'         // Random displacement
  | 'bounce'         // Elastic squash/stretch
  | 'copper'         // Copper bar sweep
  | 'spiral'         // 3D rotation twist
  | 'random';        // Randomly select an effect (excluding 'none' and 'random')

@Component({
  selector: 'lib-scrolling-marquee',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './scrolling-marquee.component.html',
  styleUrl: './scrolling-marquee.component.scss',
})
export class ScrollingMarqueeComponent {
  // Component inputs
  text = input<string>('');
  speed = input<number>(50); // Pixels per second
  direction = input<'left' | 'right'>('left');
  pauseOnHover = input<boolean>(true);
  effect = input<MarqueeEffect>('none');

  // View children references
  containerRef = viewChild.required<ElementRef<HTMLDivElement>>('container');
  contentRef = viewChild.required<ElementRef<HTMLDivElement>>('content');

  // Internal state
  shouldScroll = signal<boolean>(false);
  scrollDuration = signal<number>(10); // Default duration in seconds

  /**
   * Array of individual characters for effect rendering
   * Adds extra spacing between sentences for readability
   */
  protected characters = computed(() => {
    const currentEffect = this.effectClass();
    // Only split into characters if an effect is active
    if (currentEffect === 'none-effect') {
      return [];
    }
    // Add A LOT of extra spaces after periods for better sentence separation
    const textWithExtraSpacing = this.text().replace(/\./g, '.          ');
    return textWithExtraSpacing.split('');
  });

  /**
   * Actual effect to use (resolves 'random' to a specific effect)
   */
  private selectedEffect = signal<Exclude<MarqueeEffect, 'random'>>('none');

  constructor(private injector: Injector) {
    // Select random effect if 'random' is chosen, re-select when text changes
    effect(() => {
      const effectInput = this.effect();
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      const currentText = this.text(); // Track text changes to trigger new random selection

      if (effectInput === 'random') {
        const effects: Exclude<MarqueeEffect, 'none' | 'random'>[] = [
          'wave',
          'rainbow',
          'glitch',
          'bounce',
          'copper',
          'spiral'
        ];
        const randomEffect = effects[Math.floor(Math.random() * effects.length)];
        this.selectedEffect.set(randomEffect);
      } else {
        this.selectedEffect.set(effectInput);
      }
    }, { injector: this.injector });
    // Check overflow after render
    afterNextRender(
      () => {
        this.checkOverflow();
        this.calculateDuration();
      },
      { injector: this.injector }
    );

    // Re-check when text changes
    effect(
      () => {
        this.text(); // Track text changes
        // Use setTimeout to ensure DOM has updated
        setTimeout(() => {
          this.checkOverflow();
          this.calculateDuration();
        }, 0);
      },
      { injector: this.injector }
    );
  }

  /**
   * Check if content overflows container and scrolling is needed
   */
  private checkOverflow(): void {
    const container = this.containerRef()?.nativeElement;
    const content = this.contentRef()?.nativeElement;

    if (!container || !content) {
      return;
    }

    // Always scroll if we have text (classic marquee behavior)
    const hasText = this.text().length > 0;
    this.shouldScroll.set(hasText);
  }

  /**
   * Calculate animation duration based on content width and speed
   */
  private calculateDuration(): void {
    const content = this.contentRef()?.nativeElement;

    if (!content) {
      return;
    }

    const widthPx = content.scrollWidth;
    const speedPxPerSec = this.speed();

    // Calculate duration: distance / speed
    const durationSec = widthPx / speedPxPerSec;
    this.scrollDuration.set(durationSec);
  }

  /**
   * Get the CSS class for scroll direction
   */
  protected directionClass = computed(() => {
    return this.direction() === 'left' ? 'scroll-left' : 'scroll-right';
  });

  /**
   * Get the CSS class for pause on hover
   */
  protected pauseClass = computed(() => {
    return this.pauseOnHover() ? 'pause-on-hover' : '';
  });

  /**
   * Get the CSS class for the effect
   */
  protected effectClass = computed(() => {
    const effect = this.selectedEffect();
    switch (effect) {
      case 'wave':
        return 'wave-effect';
      case 'rainbow':
        return 'rainbow-effect';
      case 'glitch':
        return 'glitch-effect';
      case 'bounce':
        return 'bounce-effect';
      case 'copper':
        return 'copper-effect';
      case 'spiral':
        return 'spiral-effect';
      default:
        return 'none-effect';
    }
  });
}
