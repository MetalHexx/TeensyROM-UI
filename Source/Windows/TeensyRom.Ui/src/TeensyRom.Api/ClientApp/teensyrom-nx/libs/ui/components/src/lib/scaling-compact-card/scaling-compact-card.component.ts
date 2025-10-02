import { Component, input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { AnimationDirection } from '../shared/animation.types';
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

    // Priority 2: Parent completion (if available)
    if (this.parentComplete) {
      return this.parentComplete();
    }

    // Priority 3: Default to true (no parent, no explicit trigger)
    return true;
  });
}
