import { Component, input, output, signal, computed, inject, Self, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, style, transition, animate } from '@angular/animations';
import type { AnimationParentMode } from '../shared/animation.types';
import { PARENT_ANIMATION_COMPLETE } from '../shared/animation-tokens';

@Component({
  selector: 'lib-fading-container',
  imports: [CommonModule],
  templateUrl: './fading-container.component.html',
  styleUrl: './fading-container.component.scss',
  host: {
    '[@fadeIn]': 'animationStateWithParams()',
    '(@fadeIn.done)': 'onAnimationDone()',
    '[class.visible]': 'animationState() === "visible"',
    '[class.hidden]': 'animationState() === "hidden"'
  },
  providers: [
    {
      provide: PARENT_ANIMATION_COMPLETE,
      useFactory: (self: FadingContainerComponent) => {
        // Always register self as a parent (children can opt-in to wait)
        return self.animationCompleteSignal.asReadonly();
      },
      deps: [[new Self(), FadingContainerComponent]]
    }
  ],
  animations: [
    trigger('fadeIn', [
      transition('void => visible', [
        style({
          opacity: 0,
          filter: 'blur(10px)'
        }),
        animate('{{ duration }}ms cubic-bezier(0.35, 0, 0.25, 1)', style({
          opacity: 1,
          filter: 'blur(0px)'
        }))
      ], { params: { duration: 800 } }),
      transition('hidden => visible', [
        style({
          opacity: 0,
          filter: 'blur(10px)'
        }),
        animate('{{ duration }}ms cubic-bezier(0.35, 0, 0.25, 1)', style({
          opacity: 1,
          filter: 'blur(0px)'
        }))
      ], { params: { duration: 800 } }),
      transition('visible => hidden', [
        style({
          opacity: 1,
          filter: 'blur(0px)'
        }),
        animate('{{ duration }}ms cubic-bezier(0.35, 0, 0.25, 1)', style({
          opacity: 0,
          filter: 'blur(10px)'
        }))
      ], { params: { duration: 800 } })
    ])
  ]
})
export class FadingContainerComponent {
  // Animation configuration inputs
  animationTrigger = input<boolean | undefined>(undefined);

  /**
   * Animation duration in milliseconds (default: 800ms)
   */
  animationDuration = input<number>(800);

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
    return shouldAnimate ? 'visible' : 'hidden';
  });

  // Animation state with duration params for Angular animations
  protected animationStateWithParams = computed(() => ({
    value: this.animationState(),
    params: { duration: this.animationDuration() }
  }));

  onAnimationDone(): void {
    this.animationCompleteSignal.set(true);
    this.animationComplete.emit();

    // Remove from DOM after exit animation completes
    if (this.animationState() === 'hidden') {
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
