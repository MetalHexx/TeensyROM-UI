import { Component, input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { AnimationDirection, AnimationParentMode } from '../shared/animation.types';
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
  cardClass = input<string>(''); // Optional CSS class(es) to apply to the card

  // Animation inputs
  animationEntry = input<AnimationDirection>('random');
  animationExit = input<AnimationDirection>('random');
  animationTrigger = input<boolean | undefined>(undefined);
  
  /**
   * Animation duration in milliseconds for transform animation (default: 2000ms)
   * Opacity animation runs 1.5x this duration for smooth fade
   */
  animationDuration = input<number>(2000);
  
  /**
   * Controls whether this component waits for parent animations:
   * - undefined (default): No waiting - component animates immediately
   * - 'auto': Opt-in to wait for nearest animation parent in DI tree
   * - AnimationParent: Wait for specific component (sibling, ancestor, or any component)
   * - null: No waiting - same as undefined
   * 
   * Note: Composed components don't register as animation parents themselves,
   * but they pass this setting to their internal animation container.
   */
  animationParent = input<AnimationParentMode>(undefined);

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

    // Priority 2: Check animation parent mode
    const parentMode = this.animationParent();
    
    // If 'auto', wait for parent
    if (parentMode === 'auto' && this.parentComplete) {
      return this.parentComplete();
    }
    
    // If custom parent provided, wait for that parent
    if (parentMode && parentMode !== 'auto' && parentMode !== null) {
      return parentMode.animationCompleteSignal.asReadonly()();
    }

    // Priority 3: Default - render immediately (no chaining)
    return true;
  });
}
