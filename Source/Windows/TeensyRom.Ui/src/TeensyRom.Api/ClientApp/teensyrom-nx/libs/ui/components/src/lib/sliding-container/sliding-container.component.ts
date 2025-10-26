import { Component, input, output, signal, computed, inject, Self, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, style, transition, animate } from '@angular/animations';
import type { AnimationDirection, AnimationParentMode } from '../shared/animation.types';
import { PARENT_ANIMATION_COMPLETE } from '../shared/animation-tokens';

export type ContainerAnimationDirection =
  | AnimationDirection
  | 'slide-down'
  | 'slide-up'
  | 'fade';

@Component({
  selector: 'lib-sliding-container',
  imports: [CommonModule],
  templateUrl: './sliding-container.component.html',
  styleUrl: './sliding-container.component.scss',
  host: {
    '[class.visible]': 'animationParams.value === "visible"',
    '[class.hidden]': 'animationParams.value === "hidden"'
  },
  providers: [
    {
      provide: PARENT_ANIMATION_COMPLETE,
      useFactory: (self: SlidingContainerComponent) => {
        // Always register self as a parent (children can opt-in to wait)
        return self.animationCompleteSignal.asReadonly();
      },
      deps: [[new Self(), SlidingContainerComponent]]
    }
  ],
  animations: [
    trigger('containerAnimation', [
      transition('void => visible', [
        style({
          opacity: 0,
          height: '{{ startHeight }}',
          width: '{{ startWidth }}',
          transform: '{{ startTransform }}'
        }),
        animate('{{ duration }}ms {{ easing }}', style({
          opacity: 1,
          height: '{{ endHeight }}',
          width: '{{ endWidth }}',
          transform: '{{ endTransform }}'
        }))
      ], { params: { 
        startHeight: '0', 
        endHeight: 'auto',
        startWidth: 'auto',
        endWidth: 'auto',
        startTransform: 'translateY(-20px)', 
        endTransform: 'translateY(0)',
        duration: '400',
        easing: 'cubic-bezier(0.4, 0.0, 0.2, 1)'
      }}),
      transition('hidden => visible', [
        style({
          opacity: 0,
          height: '{{ startHeight }}',
          width: '{{ startWidth }}',
          transform: '{{ startTransform }}'
        }),
        animate('{{ duration }}ms {{ easing }}', style({
          opacity: 1,
          height: '{{ endHeight }}',
          width: '{{ endWidth }}',
          transform: '{{ endTransform }}'
        }))
      ], { params: { 
        startHeight: '0', 
        endHeight: 'auto',
        startWidth: 'auto',
        endWidth: 'auto',
        startTransform: 'translateY(-20px)', 
        endTransform: 'translateY(0)',
        duration: '400',
        easing: 'cubic-bezier(0.4, 0.0, 0.2, 1)'
      }}),
      transition('visible => hidden', [
        animate('{{ duration }}ms {{ easing }}', style({
          opacity: 0,
          height: '{{ startHeight }}',
          width: '{{ startWidth }}',
          transform: '{{ startTransform }}'
        }))
      ], { params: { 
        startHeight: '0',
        startWidth: 'auto',
        startTransform: 'translateY(-20px)',
        duration: '400',
        easing: 'cubic-bezier(0.4, 0.0, 0.2, 1)'
      }})
    ])
  ]
})
export class SlidingContainerComponent {
  // Animation configuration inputs
  containerHeight = input<string>('auto'); // Height of the animated container
  containerWidth = input<string>('auto'); // Width of the animated container
  animationDuration = input<number>(400); // Duration of container animation
  animationDirection = input<ContainerAnimationDirection>('from-top'); // Animation direction
  animationTrigger = input<boolean | undefined>(undefined); // Optional trigger for manual control
  
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
  animationComplete = output<void>(); // Emitted when container animation finishes

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

  // Determine when to render the container
  protected showContainer = computed(() => {
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

  // Animation parameter computation
  get animationParams() {
    const shouldShow = this.showContainer();
    const direction = this.animationDirection();
    const duration = this.animationDuration();
    const height = this.containerHeight();
    const width = this.containerWidth();
    const { startHeight, endHeight, startWidth, endWidth, startTransform, endTransform } = this.getAnimationValues(direction, height, width);

    return {
      value: shouldShow ? 'visible' : 'hidden',
      params: {
        startHeight,
        endHeight,
        startWidth,
        endWidth,
        startTransform,
        endTransform,
        duration: duration.toString(),
        easing: 'cubic-bezier(0.4, 0.0, 0.2, 1)'
      }
    };
  }

  onContainerAnimationDone(): void {
    // Set internal signal for child components
    this.animationCompleteSignal.set(true);
    // Emit event for backward compatibility
    this.animationComplete.emit();

    // Remove from DOM after exit animation completes
    const currentState = this.animationParams;
    if (currentState && currentState.value === 'hidden') {
      this.shouldBeInDom.set(false);
    }
  }

  // Effect to manage DOM presence based on showContainer changes
  constructor() {
    effect(() => {
      const shouldShow = this.showContainer();
      
      // If we should show, ensure we're in the DOM before animation starts
      if (shouldShow) {
        this.shouldBeInDom.set(true);
      }
      // If we shouldn't show, the DOM removal happens in onContainerAnimationDone after exit animation
    });
  }

  private getAnimationValues(direction: ContainerAnimationDirection, height: string, width: string): {
    startHeight: string;
    endHeight: string;
    startWidth: string;
    endWidth: string;
    startTransform: string;
    endTransform: string;
  } {
    switch (direction) {
      case 'fade':
        return {
          startHeight: height,
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(0, 0)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'slide-down':
      case 'from-top':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translateY(-20px)',
          endTransform: 'translateY(0)'
        };
      
      case 'slide-up':
      case 'from-bottom':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translateY(20px)',
          endTransform: 'translateY(0)'
        };
      
      case 'from-left':
        return {
          startHeight: height,
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translateX(-20px)',
          endTransform: 'translateX(0)'
        };
      
      case 'from-right':
        return {
          startHeight: height,
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translateX(20px)',
          endTransform: 'translateX(0)'
        };
      
      case 'from-top-left':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(-20px, -20px)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'from-top-right':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(20px, -20px)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'from-bottom-left':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(-20px, 20px)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'from-bottom-right':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(20px, 20px)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'none':
        return {
          startHeight: height,
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(0, 0)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'random': {
        const directions: ContainerAnimationDirection[] = [
          'from-left', 'from-right', 'from-top', 'from-bottom',
          'from-top-left', 'from-top-right', 'from-bottom-left', 'from-bottom-right'
        ];
        const randomDir = directions[Math.floor(Math.random() * directions.length)];
        return this.getAnimationValues(randomDir, height, width);
      }
      
      default:
        return this.getAnimationValues('from-top', height, width);
    }
  }
}