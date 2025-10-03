import { Component, input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { AnimationDirection, AnimationParent } from '../shared/animation.types';
import { ScalingContainerComponent } from '../scaling-container/scaling-container.component';
import { CompactCardLayoutComponent } from '../compact-card-layout/compact-card-layout.component';
import { PARENT_ANIMATION_COMPLETE } from '../shared/animation-tokens';

@Component({
  selector: 'lib-scaling-compact-card',
  imports: [CommonModule, ScalingContainerComponent, CompactCardLayoutComponent],
  templateUrl: './scaling-compact-card.component.html',
  styleUrl: './scaling-compact-card.component.scss'
})
export class ScalingCompactCardComponent {
  // Compact card layout inputs
  enableOverflow = input<boolean>(true);

  // Animation inputs
  animationEntry = input<AnimationDirection>('random');
  animationExit = input<AnimationDirection>('random');
  animationTrigger = input<boolean | undefined>(undefined);
  
  /**
   * Controls animation chaining behavior:
   * - undefined (default): This component chains to parent animations if available
   * - null: Breaks the animation chain - ignores parent animations
   * - AnimationParent: Uses the provided component as the parent instead of DOM parent
   */
  animationParent = input<AnimationParent | null | undefined>(undefined);

  // Inject parent completion signal (if exists)
  private parentComplete = inject(PARENT_ANIMATION_COMPLETE, {
    optional: true,
    skipSelf: true
  });

  // Determine when to render the scaling container
  protected shouldRender = computed(() => {
    const trigger = this.animationTrigger();

    // Priority 1: Explicit trigger (if defined)
    if (trigger !== undefined) {
      return trigger;
    }

    // Priority 2: Custom parent override
    const customParent = this.animationParent();
    if (customParent === null) {
      // Explicitly break chain - render immediately
      return true;
    }
    if (customParent) {
      // Use custom parent's completion signal
      return customParent.animationCompleteSignal.asReadonly()();
    }

    // Priority 3: Parent completion (if available)
    if (this.parentComplete) {
      return this.parentComplete();
    }

    // Priority 4: Default to true (no parent, no explicit trigger)
    return true;
  });
}
