import { Component, input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { AnimationDirection } from '../shared/animation.types';
import { ScalingContainerComponent } from '../scaling-container/scaling-container.component';
import { CardLayoutComponent } from '../card-layout/card-layout.component';
import { PARENT_ANIMATION_COMPLETE } from '../shared/animation-tokens';

@Component({
  selector: 'lib-scaling-card',
  imports: [CommonModule, ScalingContainerComponent, CardLayoutComponent],
  templateUrl: './scaling-card.component.html',
  styleUrl: './scaling-card.component.scss'
})
export class ScalingCardComponent {
  // Card layout inputs
  title = input<string>();
  subtitle = input<string>();
  metadataSource = input<string>();
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
