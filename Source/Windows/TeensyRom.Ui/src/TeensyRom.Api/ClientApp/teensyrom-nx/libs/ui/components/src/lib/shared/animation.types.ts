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
