# Animation Chain Implementation Plan

## Overview

Implement a self-managing animation system that allows components to automatically chain animations together with minimal consumer code, while maintaining proper DOM entry timing for Angular's `:enter` animations.

## Problem Statement

Currently, animation coordination requires manual signal management and `@if` conditions in consumer templates to ensure proper DOM entry timing. This leads to boilerplate code in every component that uses animations.

**Current Usage (Verbose):**

```html
<lib-sliding-container
  [animationTrigger]="startContainerAnimation"
  (animationComplete)="onComplete()"
>
  @if (showPlayer()) {
  <lib-compact-card-layout animationEntry="from-top">
    <!-- content -->
  </lib-compact-card-layout>
  }
</lib-sliding-container>
```

**Target Usage (Clean):**

```html
<lib-sliding-container [animationTrigger]="isPlayerLoaded()">
  <lib-compact-card-layout animationEntry="from-top">
    <!-- content - automatically waits for parent animation -->
  </lib-compact-card-layout>
</lib-sliding-container>
```

## Core Requirements

### 1. DOM Entry Timing

- Components must control their own DOM entry to trigger Angular's `:enter` animations
- Animation components should exist in DOM immediately but control their content wrapper's entry timing
- Content wrapper entry should be triggered by signals, not just visibility changes

### 2. Auto-Chaining Behavior

- Child animation components should automatically detect parent completion signals
- Use Angular's Dependency Injection to provide/inject parent completion signals
- Maintain explicit override capability when needed

### 3. Priority System

- **Explicit trigger** (provided via `animationTrigger` input): Highest priority
- **Auto-chaining** (parent completion signal): Medium priority
- **Immediate render** (no coordination): Lowest priority (default)

## Implementation Plan

### Phase 1: Enhanced SlidingContainerComponent

#### 1.1 Add Parent Signal Provision

```typescript
// In sliding-container.component.ts
export const PARENT_ANIMATION_COMPLETE = new InjectionToken<Signal<boolean>>(
  'PARENT_ANIMATION_COMPLETE'
);

@Component({
  // ... existing config
  providers: [
    {
      provide: PARENT_ANIMATION_COMPLETE,
      useFactory: () => this.animationCompleteSignal.asReadonly(),
    },
  ],
})
export class SlidingContainerComponent {
  // Add internal completion signal
  private animationCompleteSignal = signal(false);

  // Update animation completion handler
  onContainerAnimationDone(): void {
    this.animationCompleteSignal.set(true);
    this.animationComplete.emit();
  }
}
```

#### 1.2 Update Template Structure

```html
<!-- In sliding-container.component.html -->
<div
  class="sliding-container-shell"
  [style.height]="containerHeight()"
  [style.width]="containerWidth()"
>
  @if (showContainer()) {
  <div
    [@containerAnimation]="animationParams"
    (@containerAnimation.done)="onContainerAnimationDone()"
    class="sliding-container"
  >
    <ng-content></ng-content>
  </div>
  }
</div>
```

### Phase 2: Self-Managing CompactCardLayoutComponent

#### 2.1 Add Auto-Chaining Logic

```typescript
// In compact-card-layout.component.ts
@Component({
  // ... existing config
})
export class CompactCardLayoutComponent {
  // Optional explicit trigger
  animationTrigger = input<Signal<boolean> | null>(null);

  // Inject parent completion signal (optional)
  private parentAnimationComplete = inject(PARENT_ANIMATION_COMPLETE, { optional: true });

  // Animation completion output
  animationComplete = output<void>();

  // Priority-based rendering logic
  private shouldRender = computed(() => {
    const explicit = this.animationTrigger();

    // Priority 1: Explicit trigger (if provided)
    if (explicit !== null && explicit !== undefined) {
      return explicit();
    }

    // Priority 2: Auto-chain with parent (if available)
    if (this.parentAnimationComplete) {
      return this.parentAnimationComplete();
    }

    // Priority 3: Render immediately (default)
    return true;
  });
}
```

#### 2.2 Update Template for Self-Management

```html
<!-- In compact-card-layout.component.html -->
<div class="compact-card-shell">
  @if (shouldRender()) {
  <mat-card
    [@slideIn]="animationParams"
    (@slideIn.done)="onAnimationComplete()"
    class="compact-card"
  >
    <mat-card-content>
      <ng-content></ng-content>
    </mat-card-content>
  </mat-card>
  }
</div>
```

### Phase 3: CardLayoutComponent Enhancement

#### 3.1 Apply Same Pattern

- Add identical auto-chaining logic to `CardLayoutComponent`
- Maintain existing header/title/corner content functionality
- Add self-managing DOM entry behavior

#### 3.2 Provider Chain Support

```typescript
// CardLayoutComponent should also provide completion signal for nested components
providers: [
  {
    provide: PARENT_ANIMATION_COMPLETE,
    useFactory: () => this.animationCompleteSignal.asReadonly(),
  },
];
```

### Phase 4: Update Player-Toolbar (Proof of Concept)

#### 4.1 Simplify Template

```html
<!-- Remove manual animation coordination -->
<lib-sliding-container
  containerHeight="80px"
  animationDirection="from-top"
  [animationTrigger]="isPlayerLoaded()"
>
  <lib-compact-card-layout animationEntry="from-top">
    <div class="player-controls">
      <!-- existing button controls -->
    </div>
  </lib-compact-card-layout>
</lib-sliding-container>
```

#### 4.2 Simplify Component

```typescript
// Remove animation coordination code
export class PlayerToolbarComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);
  deviceId = input.required<string>();

  // Remove these:
  // - startContainerAnimation signal
  // - showPlayer signal
  // - onContainerAnimationComplete method
  // - animation effect in constructor

  // Keep only business logic methods
  toggleShuffleMode() {
    /* existing */
  }
  isPlayerLoaded() {
    /* existing */
  }
  // ... other business methods
}
```

## Testing Requirements

### Unit Tests

- Test priority system: explicit > auto-chain > immediate
- Test DOM entry timing with fresh fixtures
- Test animation completion signal propagation
- Test backward compatibility (components work without triggers)

### Integration Tests

- Test multi-level chaining (container → card → compact-card)
- Test explicit override scenarios
- Test mixed explicit/auto-chain scenarios

### Test Cases to Cover

```typescript
describe('Animation Chain System', () => {
  it('should render immediately when no trigger provided (default)');
  it('should use explicit trigger when provided');
  it('should auto-chain with parent when no explicit trigger');
  it('should prioritize explicit over auto-chain');
  it('should emit completion signals for chaining');
  it('should reset animation state properly');
  it('should support multi-level chaining');
});
```

## Migration Strategy

### Backward Compatibility

- All existing usage should continue to work unchanged
- New auto-chaining is opt-in via absence of explicit triggers
- Components without animation triggers render immediately (no breaking changes)

### Rollout Plan

1. **Phase 1**: Enhance SlidingContainerComponent with provider support
2. **Phase 2**: Update CompactCardLayoutComponent with self-management
3. **Phase 3**: Update CardLayoutComponent with same pattern
4. **Phase 4**: Migrate player-toolbar as proof of concept
5. **Phase 5**: Update component library documentation
6. **Phase 6**: Migrate other animation consumers incrementally

## Expected Benefits

### Developer Experience

- ✅ **Zero boilerplate** for simple animation chains
- ✅ **Explicit control** when needed via `animationTrigger` input
- ✅ **Composable** - any depth of animation chaining supported
- ✅ **Predictable** - clear priority system for trigger resolution

### Code Quality

- ✅ **Separation of concerns** - business logic separate from animation timing
- ✅ **Reusable patterns** - consistent across all animation components
- ✅ **Type safety** - Signal-based with full TypeScript support
- ✅ **Testable** - animation logic isolated and unit testable

### Performance

- ✅ **Proper `:enter` triggers** - real DOM entry, not visibility changes
- ✅ **Signal reactivity** - efficient change detection via computed signals
- ✅ **Minimal overhead** - injection only when parent signals exist

## Files to Modify

### Core Components

- `libs/ui/components/src/lib/sliding-container/sliding-container.component.ts`
- `libs/ui/components/src/lib/sliding-container/sliding-container.component.html`
- `libs/ui/components/src/lib/compact-card-layout/compact-card-layout.component.ts`
- `libs/ui/components/src/lib/compact-card-layout/compact-card-layout.component.html`
- `libs/ui/components/src/lib/card-layout/card-layout.component.ts`
- `libs/ui/components/src/lib/card-layout/card-layout.component.html`

### Shared Tokens

- `libs/ui/components/src/lib/shared/animation-tokens.ts` (new file)

### Test Files

- All corresponding `.spec.ts` files for modified components

### Documentation

- `docs/COMPONENT_LIBRARY.md` - Update animation component docs
- `libs/ui/components/README.md` - Add animation chaining examples

### Proof of Concept

- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.ts`
- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.html`

## Success Criteria

### Functional

- [ ] Player-toolbar achieves clean template with zero animation coordination code
- [ ] Multi-level animation chains work automatically
- [ ] Explicit triggers override auto-chaining properly
- [ ] All existing usage continues to work unchanged

### Technical

- [ ] All tests pass including new animation chain tests
- [ ] No compilation errors or type issues
- [ ] Performance is equivalent or better than current implementation
- [ ] Component library documentation is updated

### Developer Experience

- [ ] New usage patterns are intuitive and well-documented
- [ ] Migration path is clear and non-breaking
- [ ] Error messages are helpful when misconfigured
