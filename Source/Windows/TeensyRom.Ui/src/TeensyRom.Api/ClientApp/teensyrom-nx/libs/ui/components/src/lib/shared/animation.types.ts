import { Signal } from '@angular/core';

export type AnimationDirection =
  | 'none'
  | 'random'
  | 'from-left'
  | 'from-right'
  | 'from-top'
  | 'from-bottom'
  | 'from-top-left'
  | 'from-top-right'
  | 'from-bottom-left'
  | 'from-bottom-right';

/**
 * Represents a component that can act as a parent for animation chaining.
 * Components implementing this interface can be used with the `animationParent` input
 * to create custom animation chains that don't follow the default DOM hierarchy.
 */
export interface AnimationParent {
  animationCompleteSignal: { asReadonly: () => Signal<boolean> };
}

/**
 * Animation parent mode for controlling animation chaining behavior.
 *
 * - `undefined` (default): No chaining - component animates immediately
 * - `'auto'`: Explicitly opt into auto-chaining with nearest animation parent
 * - `AnimationParent`: Chain to a specific component (sibling, ancestor, or any component)
 * - `null`: No chaining - same as undefined (kept for API consistency)
 */
export type AnimationParentMode = 'auto' | AnimationParent | null | undefined;
