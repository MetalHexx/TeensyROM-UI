import { InjectionToken, Signal } from '@angular/core';

/**
 * Injection token for parent animation completion signals.
 *
 * Animation components provide their completion signal via this token,
 * allowing child animation components to automatically chain and wait
 * for parent animations to complete before starting their own.
 *
 * @example
 * ```typescript
 * // In an animation component
 * @Component({
 *   providers: [{
 *     provide: PARENT_ANIMATION_COMPLETE,
 *     useFactory: () => {
 *       const self = inject(MyAnimationComponent);
 *       return self.animationComplete.asReadonly();
 *     }
 *   }]
 * })
 * export class MyAnimationComponent {
 *   animationComplete = signal(false);
 *
 *   // Inject parent completion (if exists)
 *   private parentComplete = inject(PARENT_ANIMATION_COMPLETE, {
 *     optional: true,
 *     skipSelf: true
 *   });
 * }
 * ```
 */
export const PARENT_ANIMATION_COMPLETE = new InjectionToken<Signal<boolean>>(
  'PARENT_ANIMATION_COMPLETE'
);
