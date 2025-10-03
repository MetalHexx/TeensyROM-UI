import { Component, input, output, signal, computed, inject, Self } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, style, transition, animate, group } from '@angular/animations';
import type { AnimationDirection, AnimationParent } from '../shared/animation.types';
import { PARENT_ANIMATION_COMPLETE } from '../shared/animation-tokens';

@Component({
  selector: 'lib-scaling-container',
  imports: [CommonModule],
  templateUrl: './scaling-container.component.html',
  styleUrl: './scaling-container.component.scss',
  host: {
    '[@scaleIn]': 'animationState()',
    '(@scaleIn.done)': 'onAnimationDone()'
  },
  providers: [
    {
      provide: PARENT_ANIMATION_COMPLETE,
      useFactory: (self: ScalingContainerComponent) => {
        // If explicitly set to null, break the chain
        if (self.animationParent() === null) {
          return null;
        }
        // If a custom parent is provided, use it
        const customParent = self.animationParent();
        if (customParent) {
          return customParent.animationCompleteSignal.asReadonly();
        }
        // Default: register self as parent
        return self.animationCompleteSignal.asReadonly();
      },
      deps: [[new Self(), ScalingContainerComponent]]
    }
  ],
  animations: [
    trigger('scaleIn', [
      transition('void => *', [
        style({
          opacity: 0,
          overflow: 'hidden',
          transform: '{{ startTransform }} scale(0.8)',
          transformOrigin: '{{ transformOrigin }}'
        }),
        group([
          animate('2000ms cubic-bezier(0.35, 0, 0.25, 1)', style({
            overflow: 'hidden',
            transform: 'translate(0, 0) scale(1)',
            transformOrigin: '{{ transformOrigin }}'
          })),
          animate('3000ms cubic-bezier(0.35, 0, 0.25, 1)', style({
            opacity: 1,
            overflow: 'hidden'
          }))
        ])
      ], { params: { startTransform: 'translate(-40px, -40px)', transformOrigin: 'top left' } }),
      transition('* => void', [
        style({
          overflow: 'hidden',
          transformOrigin: '{{ transformOrigin }}'
        }),
        group([
          animate('2000ms cubic-bezier(0.35, 0, 0.25, 1)', style({
            transform: '{{ exitTransform }} scale(0.8)',
            transformOrigin: '{{ transformOrigin }}'
          })),
          animate('3000ms cubic-bezier(0.35, 0, 0.25, 1)', style({
            opacity: 0
          }))
        ])
      ], { params: { exitTransform: 'translate(40px, 40px)', transformOrigin: 'top left' } })
    ])
  ]
})
export class ScalingContainerComponent {
  // Animation configuration inputs
  animationEntry = input<AnimationDirection>('random');
  animationExit = input<AnimationDirection>('random');
  animationTrigger = input<boolean | undefined>(undefined);
  
  /**
   * Controls animation chaining behavior:
   * - undefined (default): This component registers as a parent for child animations
   * - null: Breaks the animation chain - this component won't register as a parent
   * - AnimationParent: Uses the provided component as the parent instead of self
   */
  animationParent = input<AnimationParent | null | undefined>(undefined);

  // Output events
  animationComplete = output<void>();

  // Internal animation completion signal for child components (public for provider access)
  animationCompleteSignal = signal(false);

  // Inject parent completion signal (if exists)
  private parentComplete = inject(PARENT_ANIMATION_COMPLETE, {
    optional: true,
    skipSelf: true
  });

  // Determine when to render content (for DOM entry)
  protected shouldRender = computed(() => {
    const trigger = this.animationTrigger();

    // Priority 1: Explicit trigger (if defined)
    if (trigger !== undefined) {
      return trigger;
    }

    // Priority 2: Parent completion (if available)
    if (this.parentComplete) {
      return this.parentComplete();
    }

    // Priority 3: Default to true (no parent, no explicit trigger)
    return true;
  });

  // Determine animation state based on trigger logic
  protected animationState = computed(() => {
    const shouldAnimate = this.shouldRender();

    if (!shouldAnimate) {
      return 'void';
    }

    return {
      value: 'visible',
      params: {
        startTransform: this.getTransformForDirection(this.animationEntry(), false),
        exitTransform: this.getTransformForDirection(this.animationExit(), true),
        transformOrigin: this.getTransformOrigin(this.animationEntry())
      }
    };
  });

  animationParams = computed(() => ({
    value: 'visible',
    params: {
      startTransform: this.getTransformForDirection(this.animationEntry(), false),
      exitTransform: this.getTransformForDirection(this.animationExit(), true),
      transformOrigin: this.getTransformOrigin(this.animationEntry())
    }
  }));

  private getTransformForDirection(direction: AnimationDirection, isExit: boolean): string {
    // No animation - return no transform
    if (direction === 'none') {
      return 'translate(0, 0)';
    }

    // For diagonal corners, we invert the direction on exit for variety
    // For cardinal directions (left/right/top/bottom), we keep the same direction
    const multiplier = isExit ? -1 : 1;

    const directionMap: Record<Exclude<AnimationDirection, 'random' | 'none'>, string> = {
      'from-left': `translate(-40px, 0)`,
      'from-right': `translate(40px, 0)`,
      'from-top': `translate(0, -40px)`,
      'from-bottom': `translate(0, 40px)`,
      'from-top-left': `translate(${-40 * multiplier}px, ${-40 * multiplier}px)`,
      'from-top-right': `translate(${40 * multiplier}px, ${-40 * multiplier}px)`,
      'from-bottom-left': `translate(${-40 * multiplier}px, ${40 * multiplier}px)`,
      'from-bottom-right': `translate(${40 * multiplier}px, ${40 * multiplier}px)`,
    };

    if (direction === 'random') {
      const directions = Object.keys(directionMap) as Array<Exclude<AnimationDirection, 'random' | 'none'>>;
      const randomDir = directions[Math.floor(Math.random() * directions.length)];
      return directionMap[randomDir];
    }

    return directionMap[direction];
  }

  private getTransformOrigin(direction: AnimationDirection): string {
    const originMap: Record<Exclude<AnimationDirection, 'random' | 'none'>, string> = {
      'from-left': 'left center',
      'from-right': 'right center',
      'from-top': 'center top',
      'from-bottom': 'center bottom',
      'from-top-left': 'top left',
      'from-top-right': 'top right',
      'from-bottom-left': 'bottom left',
      'from-bottom-right': 'bottom right',
    };

    if (direction === 'random') {
      const directions = Object.keys(originMap) as Array<Exclude<AnimationDirection, 'random' | 'none'>>;
      const randomDir = directions[Math.floor(Math.random() * directions.length)];
      return originMap[randomDir];
    }

    if (direction === 'none') {
      return 'center center';
    }

    return originMap[direction];
  }

  onAnimationDone(): void {
    this.animationCompleteSignal.set(true);
    this.animationComplete.emit();
  }
}
