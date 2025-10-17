import { Component, input, output, signal, computed, inject, Self, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, style, transition, animate, group } from '@angular/animations';
import type { AnimationDirection, AnimationParentMode } from '../shared/animation.types';
import { PARENT_ANIMATION_COMPLETE } from '../shared/animation-tokens';

@Component({
  selector: 'lib-scaling-container',
  imports: [CommonModule],
  templateUrl: './scaling-container.component.html',
  styleUrl: './scaling-container.component.scss',
  host: {
    '[@scaleIn]': 'animationState()',
    '(@scaleIn.done)': 'onAnimationDone()',
    '[class.visible]': 'animationState().value === "visible"',
    '[class.hidden]': 'animationState().value === "hidden"'
  },
  providers: [
    {
      provide: PARENT_ANIMATION_COMPLETE,
      useFactory: (self: ScalingContainerComponent) => {
        // Always register self as a parent (children can opt-in to wait)
        return self.animationCompleteSignal.asReadonly();
      },
      deps: [[new Self(), ScalingContainerComponent]]
    }
  ],
  animations: [
    trigger('scaleIn', [
      transition('void => visible', [
        style({
          opacity: 0,
          transform: '{{ startTransform }} scale(0.8)',
          transformOrigin: '{{ transformOrigin }}'
        }),
        group([
          animate('{{ transformDuration }}ms cubic-bezier(0.35, 0, 0.25, 1)', style({
            transform: 'translate(0, 0) scale(1)',
            transformOrigin: '{{ transformOrigin }}'
          })),
          animate('{{ opacityDuration }}ms cubic-bezier(0.35, 0, 0.25, 1)', style({
            opacity: 1
          }))
        ])
      ], { params: { 
        startTransform: 'translate(-40px, -40px)', 
        transformOrigin: 'top left',
        transformDuration: '2000',
        opacityDuration: '3000'
      } }),
      transition('hidden => visible', [
        style({
          opacity: 0,
          transform: '{{ startTransform }} scale(0.8)',
          transformOrigin: '{{ transformOrigin }}'
        }),
        group([
          animate('{{ transformDuration }}ms cubic-bezier(0.35, 0, 0.25, 1)', style({
            transform: 'translate(0, 0) scale(1)',
            transformOrigin: '{{ transformOrigin }}'
          })),
          animate('{{ opacityDuration }}ms cubic-bezier(0.35, 0, 0.25, 1)', style({
            opacity: 1
          }))
        ])
      ], { params: { 
        startTransform: 'translate(-40px, -40px)', 
        transformOrigin: 'top left',
        transformDuration: '2000',
        opacityDuration: '3000'
      } }),
      transition('visible => hidden', [
        style({
          transformOrigin: '{{ transformOrigin }}'
        }),
        group([
          animate('{{ transformDuration }}ms cubic-bezier(0.35, 0, 0.25, 1)', style({
            transform: '{{ exitTransform }} scale(0.8)',
            transformOrigin: '{{ transformOrigin }}'
          })),
          animate('{{ opacityDuration }}ms cubic-bezier(0.35, 0, 0.25, 1)', style({
            opacity: 0
          }))
        ])
      ], { params: { 
        exitTransform: 'translate(40px, 40px)', 
        transformOrigin: 'top left',
        transformDuration: '2000',
        opacityDuration: '3000'
      } })
    ])
  ]
})
export class ScalingContainerComponent {
  // Animation configuration inputs
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
   * Note: This component always registers as an animation parent for its children,
   * regardless of this setting. This input only controls waiting behavior.
   */
  animationParent = input<AnimationParentMode>(undefined);

  // Output events
  animationComplete = output<void>();

  // Internal animation completion signal for child components (public for provider access)
  animationCompleteSignal = signal(false);

  // Track whether component should be in DOM (stays true during exit animation)
  private shouldBeInDom = signal(true);

  // Expose for template
  protected shouldRenderInDom = this.shouldBeInDom.asReadonly();

  // Memoize random direction selection per instance
  private selectedEntryDirection: Exclude<AnimationDirection, 'random' | 'none'> | null = null;
  private selectedExitDirection: Exclude<AnimationDirection, 'random' | 'none'> | null = null;

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

  // Determine animation state based on trigger logic
  protected animationState = computed(() => {
    const shouldAnimate = this.shouldRender();
    const transformDuration = this.animationDuration();
    const opacityDuration = Math.round(transformDuration * 1.5); // Opacity animation is 1.5x transform duration

    return {
      value: shouldAnimate ? 'visible' : 'hidden',
      params: {
        startTransform: this.getTransformForDirection(this.animationEntry(), false),
        exitTransform: this.getTransformForDirection(this.animationExit(), true),
        transformOrigin: this.getTransformOrigin(this.animationEntry()),
        transformDuration: transformDuration.toString(),
        opacityDuration: opacityDuration.toString()
      }
    };
  });

  private getTransformForDirection(direction: AnimationDirection, isExit: boolean): string {
    // No animation - return no transform
    if (direction === 'none') {
      return 'translate(0, 0)';
    }

    // Direction naming: "from-X" means element comes FROM direction X
    // Entry: element starts at direction position, moves to center (0,0)
    // Exit: element starts at center (0,0), moves OUT to direction position
    // So entry and exit use the SAME offset, just applied at different times

    const directionMap: Record<Exclude<AnimationDirection, 'random' | 'none'>, string> = {
      'from-left': `translate(-40px, 0)`,      // Coming from left = starts at left (-X)
      'from-right': `translate(40px, 0)`,      // Coming from right = starts at right (+X)
      'from-top': `translate(0, -40px)`,       // Coming from top = starts at top (-Y)
      'from-bottom': `translate(0, 40px)`,     // Coming from bottom = starts at bottom (+Y)
      'from-top-left': `translate(-40px, -40px)`,
      'from-top-right': `translate(40px, -40px)`,
      'from-bottom-left': `translate(-40px, 40px)`,
      'from-bottom-right': `translate(40px, 40px)`,  // Starts at bottom-right (+X, +Y)
    };

    // Handle random direction - memoize selection per instance
    if (direction === 'random') {
      const directions = Object.keys(directionMap) as Array<Exclude<AnimationDirection, 'random' | 'none'>>;

      if (isExit) {
        if (!this.selectedExitDirection) {
          this.selectedExitDirection = directions[Math.floor(Math.random() * directions.length)];
        }
        return directionMap[this.selectedExitDirection];
      } else {
        if (!this.selectedEntryDirection) {
          this.selectedEntryDirection = directions[Math.floor(Math.random() * directions.length)];
        }
        return directionMap[this.selectedEntryDirection];
      }
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
      // Use the same random direction as entry transform
      if (this.selectedEntryDirection) {
        return originMap[this.selectedEntryDirection];
      }

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

    // Remove from DOM after exit animation completes
    const currentState = this.animationState();
    if (currentState && typeof currentState === 'object' && currentState.value === 'hidden') {
      this.shouldBeInDom.set(false);
    }
  }

  // Effect to manage DOM presence based on shouldRender changes
  constructor() {
    effect(() => {
      const shouldShow = this.shouldRender();
      
      // If we should show, ensure we're in the DOM before animation starts
      if (shouldShow) {
        this.shouldBeInDom.set(true);
      }
      // If we shouldn't show, the DOM removal happens in onAnimationDone after exit animation
    });
  }
}
