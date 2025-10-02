# TeensyROM UI Component Library

## Overview

This document catalogs all shared UI components available in the TeensyROM application. All components are located in `libs/ui/components/src/lib/` and follow Angular 19 standalone component architecture with modern signal-based inputs.

---

## Layout Components

### `CardLayoutComponent`

**Purpose**: Pure layout component for cards with headers, titles, and metadata. No animations - use [ScalingCardComponent](#scalingcardcomponent) for animated cards.

**Selector**: `lib-card-layout`

**Properties**:
- `title`: Card header title (optional)
- `subtitle`: Text below title (optional)
- `metadataSource`: Footer attribution text (optional)
- `enableOverflow`: Allow scrollbars when content overflows (default: `true`)

**Usage**:

```html
<lib-card-layout
  title="File Information"
  subtitle="Release v1.2.3"
  metadataSource="Gamebase Database">

  <mat-chip-set slot="corner">
    <mat-chip>C64</mat-chip>
  </mat-chip-set>

  <p>File details...</p>
</lib-card-layout>
```

**Content Slots**:
- Default: Main card content
- `[slot=corner]`: Upper-right corner content (buttons, chips, etc.)

**See Also**: [ScalingCardComponent](#scalingcardcomponent)

### `CompactCardLayoutComponent`

**Purpose**: Lightweight card layout for forms and toolbars. No headers or animations - use [ScalingCompactCardComponent](#scalingcompactcardcomponent) for animated version.

**Selector**: `lib-compact-card-layout`

**Properties**:
- `enableOverflow`: Allow scrollbars when content overflows (default: `true`)

**Usage**:

```html
<lib-compact-card-layout>
  <mat-form-field appearance="outline">
    <mat-label>Search</mat-label>
    <input matInput />
  </mat-form-field>
</lib-compact-card-layout>
```

**See Also**: [ScalingCompactCardComponent](#scalingcompactcardcomponent)

### `ScalingCardComponent`

**Purpose**: Animated card with scale+fade+slide effects. Combines [CardLayoutComponent](#cardlayoutcomponent) + [ScalingContainerComponent](#scalingcontainercomponent). Use this for most animated card needs.

**Selector**: `lib-scaling-card`

**Properties**:
- `title`: Card header title (optional)
- `subtitle`: Text below title (optional)
- `metadataSource`: Footer attribution text (optional)
- `enableOverflow`: Allow scrollbars (default: `true`)
- `animationEntry`: Entry direction - `'random'`, `'from-left'`, `'from-right'`, `'from-top'`, `'from-bottom'`, etc. (default: `'random'`)
- `animationExit`: Exit direction (default: `'random'`)
- `animationTrigger`: Manual control signal (optional - auto-chains if undefined)

**Usage**:

```html
<!-- Basic animated card -->
<lib-scaling-card
  title="Device Info"
  subtitle="TeensyROM"
  animationEntry="from-left">
  <p>Content...</p>
</lib-scaling-card>

<!-- With corner slot -->
<lib-scaling-card title="Settings">
  <button mat-icon-button slot="corner">
    <mat-icon>more_vert</mat-icon>
  </button>
  <p>Settings...</p>
</lib-scaling-card>

<!-- Nested with auto-chaining (child waits for parent) -->
<lib-scaling-card title="Parent">
  <lib-scaling-card title="Child" animationEntry="from-bottom">
    <p>Animates after parent</p>
  </lib-scaling-card>
</lib-scaling-card>

<!-- Manual control -->
<lib-scaling-card [animationTrigger]="showCard()">
  <p>Shows when signal is true</p>
</lib-scaling-card>
```

**Animation**: 0.8→1.0 scale + fade + directional slide. Auto-chains when nested (child waits for parent). Override with `animationTrigger` for manual control.

**See Also**: [CardLayoutComponent](#cardlayoutcomponent), [ScalingCompactCardComponent](#scalingcompactcardcomponent)

### `ScalingCompactCardComponent`

**Purpose**: Animated compact card for forms, toolbars, and controls. Combines [CompactCardLayoutComponent](#compactcardlayoutcomponent) + [ScalingContainerComponent](#scalingcontainercomponent). No headers - minimal structure.

**Selector**: `lib-scaling-compact-card`

**Properties**:
- `enableOverflow`: Allow scrollbars (default: `true`)
- `animationEntry`: Entry direction (default: `'random'`)
- `animationExit`: Exit direction (default: `'random'`)
- `animationTrigger`: Manual control signal (optional - auto-chains if undefined)

**Usage**:

```html
<!-- Animated search form -->
<lib-scaling-compact-card animationEntry="from-top">
  <mat-form-field appearance="outline">
    <mat-label>Search</mat-label>
    <input matInput />
  </mat-form-field>
</lib-scaling-compact-card>

<!-- Animated toolbar -->
<lib-scaling-compact-card animationEntry="from-bottom">
  <div class="button-group">
    <button mat-icon-button><mat-icon>play_arrow</mat-icon></button>
    <button mat-icon-button><mat-icon>pause</mat-icon></button>
  </div>
</lib-scaling-compact-card>

<!-- Manual control -->
<lib-scaling-compact-card [animationTrigger]="showToolbar()">
  <div class="toolbar">...</div>
</lib-scaling-compact-card>
```

**Animation**: Same as ScalingCardComponent - 0.8→1.0 scale + fade + slide. Auto-chains when nested.

**See Also**: [CompactCardLayoutComponent](#compactcardlayoutcomponent), [ScalingCardComponent](#scalingcardcomponent)

### `SlidingContainerComponent`

**Purpose**: A reusable sliding animation wrapper component that provides smooth container slide animations with configurable directions, timing, and event-driven coordination. Designed for clean separation of concerns where the container handles sliding animation and parent components control content timing via events.

**Selector**: `lib-sliding-container`

**Properties**:

- `containerHeight` (optional): `string` - Height of the animated container - defaults to 'auto'
- `containerWidth` (optional): `string` - Width of the animated container - defaults to 'auto'
- `animationDuration` (optional): `number` - Duration of container animation in milliseconds - defaults to 400
- `animationDirection` (optional): `ContainerAnimationDirection` - Animation direction - defaults to 'from-top'
- `animationTrigger` (optional): `boolean` - Explicit control for when the container should render and animate. When provided, overrides auto-chaining behavior. See [Animation Chaining](#animation-chaining) for details.

**Events**:

- `animationComplete`: Emitted when container animation finishes - can be used for manual coordination if needed (auto-chaining handles this automatically)

**Animation Directions**:

- `AnimationDirection` types: `'from-top'`, `'from-bottom'`, `'from-left'`, `'from-right'`, `'from-top-left'`, `'from-top-right'`, `'from-bottom-left'`, `'from-bottom-right'`, `'none'`, `'random'`
- Container-specific: `'slide-down'`, `'slide-up'`, `'fade'`

**Usage Examples**:

```html
<!-- Basic usage with signal-driven animation -->
<lib-sliding-container
  containerHeight="80px"
  animationDirection="from-top"
  [animationTrigger]="startAnimation"
  (animationComplete)="onAnimationComplete()">
  @if (showContent()) {
    <lib-compact-card-layout animationEntry="from-top">
      <!-- Your content here -->
    </lib-compact-card-layout>
  }
</lib-sliding-container>

<!-- Default behavior (immediate animation) -->
<lib-sliding-container
  containerHeight="80px"
  animationDirection="from-top"
  (animationComplete)="onAnimationComplete()">
  <!-- Content shows and animates immediately -->
</lib-sliding-container>

<!-- Fade animation with custom timing -->
<lib-sliding-container
  animationDirection="fade"
  [animationDuration]="600"
  (containerAnimationComplete)="onFadeComplete()">
  <!-- Content projection -->
</lib-sliding-container>

<!-- Slide down with auto height -->
<lib-sliding-container
  animationDirection="slide-down"
  containerHeight="auto">
  <div class="dynamic-content">
    <!-- Content that determines height -->
  </div>
</lib-sliding-container>
```

**Signal-Driven Animation Pattern**:

```typescript
export class MyComponent {
  // Animation control signals
  startAnimation = signal(false);
  showContent = signal(false);

  constructor() {
    // Trigger animation based on some condition
    effect(() => {
      if (this.shouldAnimate()) {
        this.startAnimation.set(true);
      }
    });
  }

  onAnimationComplete(): void {
    // Container finished animating, now show content
    this.showContent.set(true);
  }

  shouldAnimate(): boolean {
    // Your animation trigger logic
    return this.dataLoaded();
  }
}
```

**Content Projection**: Uses `<ng-content></ng-content>` to project any content into the animated container. Content timing is controlled by parent components via the `containerAnimationComplete` event.

**Animation Features**:

- **Container Animation**: Handles height expansion/collapse with opacity and transform effects
- **Event-Driven**: Emits completion events for parent components to coordinate content timing
- **Configurable Timing**: Customizable duration and easing functions
- **Multiple Directions**: Supports all standard animation directions plus container-specific variants
- **Clean Architecture**: Focuses solely on container animation, delegates content timing to parents

**Best Practice**: Use for any content that needs coordinated animations where container and content animations should be sequential. The event-driven approach ensures clean separation of concerns and reusable animation logic.

**Used In**:

- [`player-toolbar.component.html`](../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.html) - Player controls with coordinated card animation

### `ScalingContainerComponent`

**Purpose**: A reusable animation wrapper that provides scale+fade+slide effects with directional control. Uses transform-based animations that don't affect document flow, creating a smooth "pop-in" effect. Designed to be the animation engine behind card components.

**Selector**: `lib-scaling-container`

**Properties**:

- `animationEntry` (optional): `AnimationDirection` - Controls the entry animation direction. Options: `'none'`, `'random'` (default), `'from-left'`, `'from-right'`, `'from-top'`, `'from-bottom'`, `'from-top-left'`, `'from-top-right'`, `'from-bottom-left'`, `'from-bottom-right'`.
- `animationExit` (optional): `AnimationDirection` - Controls the exit animation direction. Same options as `animationEntry`. Default: `'random'`.
- `animationTrigger` (optional): `boolean` - Explicit control for when the container should render and animate. When provided, overrides auto-chaining behavior. See [Animation Chaining](#animation-chaining) for details.

**Events**:

- `animationComplete`: Emitted when animation finishes - can be used for manual coordination if needed (auto-chaining handles this automatically)

**Animation Characteristics**:

- **Scale Effect**: Starts at 0.8 scale, animates to 1.0 for a "pop" or "zoom-in" effect
- **Opacity Fade**: Smooth fade from 0 to 1 opacity
- **Directional Slide**: Translates from specified direction (-40px offset)
- **Transform Origin**: Makes scale happen from a specific corner/edge based on direction
- **Dual-Speed Animation**: Transform is faster (2000ms), opacity is slower (3000ms) for smooth reveal
- **No Document Flow Impact**: Uses transforms which don't affect layout (unlike height-based sliding)

**Usage Examples**:

```html
<!-- Basic usage with random direction -->
<lib-scaling-container>
  <div class="my-content">Scales and fades in from random direction</div>
</lib-scaling-container>

<!-- Explicit entry direction -->
<lib-scaling-container animationEntry="from-left">
  <div class="panel">Scales in from the left</div>
</lib-scaling-container>

<!-- Controlled by trigger signal -->
<lib-scaling-container [animationTrigger]="isDataLoaded()">
  <div class="data-panel">Only appears when data is loaded</div>
</lib-scaling-container>

<!-- Different entry and exit directions -->
<lib-scaling-container
  animationEntry="from-top-left"
  animationExit="from-bottom-right">
  <div class="notification">
    Enters from top-left corner, exits to bottom-right
  </div>
</lib-scaling-container>

<!-- No animation -->
<lib-scaling-container animationEntry="none" animationExit="none">
  <div>Instant appearance/disappearance</div>
</lib-scaling-container>
```

**With Auto-Chaining**:

```html
<!-- Child waits for parent animation -->
<lib-sliding-container [animationTrigger]="isPanelReady()">
  <lib-scaling-container animationEntry="from-top">
    <div class="nested-content">
      Automatically waits for sliding-container to complete
    </div>
  </lib-scaling-container>
</lib-sliding-container>
```

**Content Projection**: Uses `<ng-content></ng-content>` to project any content into the animated container

**Animation Features**:

- **Supports Auto-Chaining**: Automatically waits for parent animation components to complete
- **Provider/Injector Pattern**: Provides completion signal for child components
- **3-Tier Priority**: Explicit trigger > auto-chain > immediate render
- **Visual Polish**: Dual-speed animation creates professional, smooth transitions
- **Reusable**: Extracted from CardLayoutComponent for use anywhere

**Key Differences from SlidingContainerComponent**:

- **SlidingContainer**: Height/width expansion that pushes/pulls content (affects document flow)
- **ScalingContainer**: Scale+fade+slide transforms (visual only, no layout impact)
- Use SlidingContainer when you want content to push other elements
- Use ScalingContainer for overlay-style or visual "pop" effects

**Best Practice**: Use directly for custom animated panels, or let CardLayoutComponent and CompactCardLayoutComponent handle it internally for Material Design cards.

**Used Internally By**:

- [`CardLayoutComponent`](#cardlayoutcomponent) - Uses ScalingContainer for its animations
- [`CompactCardLayoutComponent`](#compactcardlayoutcomponent) - Uses ScalingContainer for its animations

### Animation Chaining

**Purpose**: A self-managing animation system that automatically coordinates animations between parent and child components, eliminating the need for manual signal management and template conditions.

**Key Concept**: Animation components automatically detect when they're nested inside other animation components and wait for the parent animation to complete before starting their own animation. This ensures proper sequencing without any boilerplate code.

**How It Works**:

Animation components ([`SlidingContainerComponent`](#slidingcontainercomponent), [`ScalingContainerComponent`](#scalingcontainercomponent)) use Angular's Dependency Injection to:

1. **Provide** their own completion signal to child components
2. **Inject** parent completion signals (if available)
3. **Auto-chain** by waiting for parent animations before rendering

**Priority System**:

Each animation component follows a 3-tier priority system for determining when to render:

1. **Explicit Chaining** (Highest): If `animationTrigger` is provided, use its value
2. **Auto-Chaining** (Medium): If nested in parent animation component, wait for parent completion
3. **Immediate Render** (Lowest): Default behavior, render and animate immediately

**Basic Usage (Auto-Chaining)**:

```html
<!-- Child automatically waits for parent animation -->
<lib-sliding-container
  containerHeight="80px"
  animationDirection="from-top"
  [animationTrigger]="isDataLoaded()">

  <lib-compact-card-layout animationEntry="from-top">
    <!-- Content here - no @if needed! -->
    <div class="my-content">...</div>
  </lib-compact-card-layout>
</lib-sliding-container>
```

**Before Animation Chaining** (Manual Coordination):

```typescript
// Component - lots of boilerplate
export class MyComponent {
  startContainerAnimation = signal(false);
  showContent = signal(false);

  constructor() {
    effect(() => {
      if (this.isDataLoaded()) {
        this.startContainerAnimation.set(true);
      } else {
        this.startContainerAnimation.set(false);
        this.showContent.set(false);
      }
    });
  }

  onContainerAnimationComplete(): void {
    this.showContent.set(true);
  }
}
```

```html
<!-- Template - manual @if condition -->
<lib-sliding-container
  [animationTrigger]="startContainerAnimation"
  (animationComplete)="onContainerAnimationComplete()">

  @if (showContent()) {
    <lib-compact-card-layout>
      <div class="my-content">...</div>
    </lib-compact-card-layout>
  }
</lib-sliding-container>
```

**After Animation Chaining** (Automatic):

```typescript
// Component - clean and simple
export class MyComponent {
  // No animation coordination signals needed!
  // No effects needed!
  // No event handlers needed!
}
```

```html
<!-- Template - clean and declarative -->
<lib-sliding-container [animationTrigger]="isDataLoaded()">
  <lib-compact-card-layout>
    <div class="my-content">...</div>
  </lib-compact-card-layout>
</lib-sliding-container>
```

**Advanced Usage - Multi-Level Chaining**:

```html
<!-- Three levels of automatic animation chaining -->
<lib-card-layout
  title="Main Panel"
  animationEntry="from-left"
  [animationTrigger]="isPanelVisible()">

  <!-- Level 2: Waits for card-layout -->
  <lib-sliding-container
    containerHeight="80px"
    animationDirection="from-top">

    <!-- Level 3: Waits for sliding-container -->
    <lib-compact-card-layout animationEntry="from-top">
      <div class="nested-content">
        All animations chain automatically!
      </div>
    </lib-compact-card-layout>
  </lib-sliding-container>
</lib-card-layout>
```

**Explicit Override**:

You can override auto-chaining for specific components when needed:

```html
<lib-sliding-container [animationTrigger]="isContainerReady()">

  <!-- This card auto-chains with parent (default) -->
  <lib-compact-card-layout animationEntry="from-left">
    <p>Waits for parent</p>
  </lib-compact-card-layout>

  <!-- This card has explicit control (overrides auto-chain) -->
  <lib-compact-card-layout
    animationEntry="from-right"
    [animationTrigger]="isSpecialContentReady()">
    <p>Independent trigger</p>
  </lib-compact-card-layout>
</lib-sliding-container>
```

**Real-World Example - Player Toolbar**:

```html
<!-- Clean template with auto-chaining -->
<lib-sliding-container
  containerHeight="80px"
  animationDirection="from-top"
  [animationTrigger]="isPlayerLoaded()">

  <lib-compact-card-layout animationEntry="from-top">
    <div class="player-controls">
      <lib-icon-button icon="skip_previous" ...></lib-icon-button>
      <lib-icon-button icon="play_arrow" ...></lib-icon-button>
      <lib-icon-button icon="skip_next" ...></lib-icon-button>
      <lib-icon-button icon="shuffle" ...></lib-icon-button>
    </div>
  </lib-compact-card-layout>
</lib-sliding-container>
```

**Benefits**:

- ✅ **Zero Boilerplate**: No manual animation coordination signals or effects
- ✅ **Explicit Control**: Use `animationTrigger` when you need manual control
- ✅ **Composable**: Works at any nesting depth
- ✅ **Predictable**: Clear priority system for trigger resolution
- ✅ **Type Safe**: Full TypeScript support with signals
- ✅ **Backward Compatible**: Existing code continues to work

**Technical Implementation**:

Animation chaining uses Angular's Dependency Injection with the `PARENT_ANIMATION_COMPLETE` token:

```typescript
import { PARENT_ANIMATION_COMPLETE } from '@teensyrom-nx/ui/components';

// Each animation component:
// 1. Provides its completion signal for children
// 2. Injects parent completion signal (if available)
// 3. Computes when to render based on priority system
```

**Best Practices**:

1. **Default to Auto-Chaining**: Let components chain automatically unless you have a specific reason to override
2. **Use Explicit Triggers**: Provide `animationTrigger` only at the top level to control the entire chain
3. **Keep Templates Clean**: Avoid manual `@if` conditions for animation timing
4. **Separate Concerns**: Business logic in component, animation timing handled by framework

**Migration Guide**:

To migrate existing code to use auto-chaining:

1. **Remove** manual animation coordination signals (`startAnimation`, `showContent`, etc.)
2. **Remove** animation coordination effects and event handlers
3. **Remove** `@if` conditions that were controlling child component rendering
4. **Keep** the top-level `animationTrigger` that controls when the chain starts
5. **Simplify** component logic to focus on business logic, not animation timing

**See Also**:

- [SlidingContainerComponent](#slidingcontainercomponent) - Supports auto-chaining
- [ScalingContainerComponent](#scalingcontainercomponent) - Supports auto-chaining
- [CardLayoutComponent](#cardlayoutcomponent) - Uses ScalingContainer internally (supports auto-chaining)
- [CompactCardLayoutComponent](#compactcardlayoutcomponent) - Uses ScalingContainer internally (supports auto-chaining)
- [Animation Chain Implementation Plan](../docs/features/animation/ANIMATION_CHAIN.md) - Detailed technical specification

---

## Form Components

### `InputFieldComponent`

**Purpose**: A reusable input field component that provides consistent Material Design form field styling with configurable icons and accessibility features.

**Selector**: `lib-input-field`

**Properties**:

- `label` (required): `string` - The label text displayed above the input field for accessibility and visual clarity
- `placeholder` (required): `string` - Placeholder text displayed inside the input field as a watermark
- `prefixIcon` (optional): `string` - Material Design icon name to display at the beginning of the input field
- `suffixIcon` (optional): `string` - Material Design icon name to display at the end of the input field
- `inputType` (optional): `string` - HTML input type (text, search, email, number, password, etc.) - defaults to 'text'
- `disabled` (optional): `boolean` - Whether the input field is disabled - defaults to false

**Events**:

- `valueChange`: Emitted on every keystroke with the current input value - ideal for real-time search functionality
- `inputFocus`: Emitted when the input field receives focus
- `inputBlur`: Emitted when the input field loses focus

**Usage Examples**:

```html
<!-- Search input with suffix icon -->
<lib-input-field label="Search" placeholder="Search files and folders..." suffixIcon="search">
</lib-input-field>

<!-- Email input with prefix icon -->
<lib-input-field
  label="Email Address"
  placeholder="Enter your email"
  prefixIcon="email"
  inputType="email"
>
</lib-input-field>

<!-- Simple text input without icons -->
<lib-input-field label="Username" placeholder="Enter your username"> </lib-input-field>

<!-- Password input -->
<lib-input-field
  label="Password"
  placeholder="Enter your password"
  inputType="password"
  suffixIcon="visibility"
>
</lib-input-field>

<!-- Disabled input -->
<lib-input-field label="Read Only Field" placeholder="This field is disabled" [disabled]="true">
</lib-input-field>

<!-- Real-time search with event handling -->
<lib-input-field
  label="Search"
  placeholder="Type to search..."
  suffixIcon="search"
  (valueChange)="onSearchChange($event)"
  (inputFocus)="onSearchFocus()"
  (inputBlur)="onSearchBlur()"
>
</lib-input-field>
```

**Advanced Usage Patterns**:

```typescript
// Component class for different integration approaches

export class ExampleComponent {
  // 1. Event-driven approach (real-time search)
  onSearchChange(searchTerm: string): void {
    console.log('Search term:', searchTerm);
    this.performSearch(searchTerm);
  }

  // 2. Two-way binding approach
  searchValue = '';

  ngOnInit() {
    // Watch for changes using signals or effects
    effect(() => {
      console.log('Search value changed:', this.searchValue);
      this.performSearch(this.searchValue);
    });
  }

  // 3. Reactive forms approach
  searchControl = new FormControl('');

  ngOnInit() {
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((value) => {
        this.performSearch(value);
      });
  }

  private performSearch(term: string): void {
    // Your search logic here
  }
}
```

```html
<!-- Different binding approaches -->

<!-- Event-driven (immediate response) -->
<lib-input-field
  label="Live Search"
  placeholder="Results update on every keystroke"
  (valueChange)="onSearchChange($event)"
>
</lib-input-field>

<!-- Two-way binding -->
<lib-input-field
  label="Bound Search"
  placeholder="Bound to component property"
  [(ngModel)]="searchValue"
>
</lib-input-field>

<!-- Reactive forms -->
<lib-input-field
  label="Form Search"
  placeholder="Integrated with reactive forms"
  [formControl]="searchControl"
>
</lib-input-field>
```

**Form Integration & Data Binding**:

The component provides **three flexible approaches** for handling user input:

1. **Event-Driven Approach** (Real-time):

   - Use `(valueChange)` event for immediate response to every keystroke
   - Perfect for live search, real-time validation, or instant feedback
   - No debouncing built-in - handle in parent component if needed

2. **Two-Way Binding** (ngModel):

   - Use `[(ngModel)]="property"` for automatic synchronization
   - Ideal for simple form scenarios and data binding
   - Updates component property on every keystroke

3. **Reactive Forms** (FormControl):
   - Use `[formControl]="control"` for full reactive forms integration
   - Implements `ControlValueAccessor` for seamless integration
   - Supports validation, async validators, and form state management
   - Can be combined with RxJS operators for debouncing and filtering

**Integration Benefits**:

- Full TypeScript type safety with all approaches
- Consistent behavior across all binding methods
- Proper form validation state handling
- Accessibility features maintained regardless of binding approach

**Accessibility Features**:

- Always includes `mat-label` for screen reader compatibility
- Proper ARIA attributes automatically applied by Material Design
- Keyboard navigation support
- High contrast mode compatibility
- Focus management for optimal user experience

**Styling**: Automatically applies Material Design outline appearance with full-width behavior for consistent form layouts.

**Best Practice**: Use for all input fields throughout the application to maintain consistent styling, accessibility, and behavior patterns.

**Used In**:

- [`search-toolbar.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.html) - File and folder search functionality

### `IconButtonComponent`

**Purpose**: A reusable icon button component that provides consistent Material Design button styling with configurable appearance, state management, and accessibility features. Supports two different ways to display icons: Material Design icons or custom icon components via content projection.

**Selector**: `lib-icon-button`

**Properties**:

- `icon` (optional): `string` - Material Design icon name to display in the button. Use this OR ng-content for custom icons.
- `ariaLabel` (required): `string` - Accessibility label for screen readers (required for proper accessibility)
- `color` (optional): `'normal' | 'highlight' | 'success' | 'error' | 'dimmed'` - Semantic color variant that maps to [STYLE_GUIDE.md](STYLE_GUIDE.md) color system - defaults to 'normal'
- `size` (optional): `'small' | 'medium' | 'large'` - Size variant that maps to existing style classes - defaults to 'medium'
- `variant` (optional): `'standard' | 'rounded-primary' | 'rounded-transparent'` - Style variant from [STYLE_GUIDE.md](STYLE_GUIDE.md) - defaults to 'standard'
- `disabled` (optional): `boolean` - Whether the button is disabled - defaults to false

**Events**:

- `buttonClick`: Emitted when the button is clicked (only when not disabled)

**Usage Examples**:

```html
<!-- 1. Material Design Icons (traditional approach) -->
<lib-icon-button
  icon="power_settings_new"
  ariaLabel="Power"
  [color]="connectionStatus() ? 'highlight' : 'normal'"
  size="medium"
  (buttonClick)="connectionStatus() ? onDisconnect() : onConnect()"
>
</lib-icon-button>

<!-- 2. Custom Icon Components (recommended for reusable custom icons) -->
<lib-icon-button
  ariaLabel="Games Filter"
  size="large"
  (buttonClick)="onGamesClick()"
>
  <lib-joystick-icon></lib-joystick-icon>
</lib-icon-button>

<lib-icon-button
  ariaLabel="Images Filter"
  size="large"
  (buttonClick)="onImagesClick()"
>
  <lib-image-icon></lib-image-icon>
</lib-icon-button>

<!-- Log control buttons (from device-logs component) -->
<lib-icon-button
  icon="play_arrow"
  ariaLabel="Start Logs"
  color="success"
  (buttonClick)="startLogs()"
>
</lib-icon-button>

<lib-icon-button icon="stop" ariaLabel="Stop Logs" color="error" (buttonClick)="stopLogs()">
</lib-icon-button>

<!-- Settings button with rounded primary style -->
<lib-icon-button
  icon="settings"
  ariaLabel="Open Settings"
  variant="rounded-primary"
  color="highlight"
  (buttonClick)="openSettings()"
>
</lib-icon-button>

<!-- Delete button with error color -->
<lib-icon-button
  icon="delete"
  ariaLabel="Delete Item"
  color="error"
  [disabled]="!canDelete"
  (buttonClick)="deleteItem()"
>
</lib-icon-button>

<!-- Small dimmed action button -->
<lib-icon-button
  icon="edit"
  ariaLabel="Edit"
  size="small"
  color="dimmed"
  (buttonClick)="editItem()"
>
</lib-icon-button>

<!-- Dynamic color based on state -->
<lib-icon-button
  icon="favorite"
  ariaLabel="Add to Favorites"
  [color]="isFavorite ? 'success' : 'normal'"
  (buttonClick)="toggleFavorite()"
>
</lib-icon-button>
```

**Icon Display Options**:

The component supports two mutually exclusive ways to display icons:

1. **Material Icons** (`icon` property): Use for standard Material Design icons from Google Fonts
2. **Custom Icon Components** (`ng-content`): **Recommended** for reusable custom icons - creates dedicated components for better maintainability, type safety, and code organization

**Style Integration**:

The component automatically maps to existing classes from [STYLE_GUIDE.md](STYLE_GUIDE.md):

- **Size Mapping**:

  - `small` → `.icon-button-small`
  - `medium` → `.icon-button-medium`
  - `large` → `.icon-button-large`

- **Variant Mapping**:

  - `rounded-primary` → `.icon-button-rounded-primary`
  - `rounded-transparent` → `.icon-button-rounded-transparent`
  - `standard` → Default Material Design styling

- **Color Mapping** (references [STYLE_GUIDE.md](STYLE_GUIDE.md) semantic colors):
  - `normal` → `.normal` (default inherit color)
  - `highlight` → `.highlight` (cyan accent color from `--color-highlight`)
  - `success` → `.success` (green color from `--color-success`)
  - `error` → `.error` (red color from `--color-error`)
  - `dimmed` → `.dimmed` (gray color from `--color-dimmed`)

**Accessibility Features**:

- **Required ARIA Label**: Always requires `ariaLabel` for screen reader compatibility
- **Proper Button Semantics**: Uses semantic `<button>` element with `mat-icon-button`
- **Disabled State Handling**: Properly handles disabled state with both visual and functional changes
- **Focus Management**: Maintains proper focus behavior and keyboard navigation
- **High Contrast Support**: Compatible with high contrast modes through Material Design

**Type Safety**:

```typescript
import { IconButtonSize, IconButtonVariant, IconButtonColor } from '@teensyrom-nx/ui/components';

// Component usage with full type safety
export class MyComponent {
  buttonSize: IconButtonSize = 'medium';
  buttonVariant: IconButtonVariant = 'rounded-primary';
  buttonColor: IconButtonColor = 'success';

  onButtonClick(): void {
    console.log('Button clicked!');
  }
}
```

**Best Practice**: Use for all icon-based actions throughout the application to maintain consistent styling, behavior, and accessibility patterns. Always provide meaningful `ariaLabel` values.

**Used In**:

- [`device-item.component.html`](../libs/features/devices/src/lib/device-view/device-item/device-item.component.html) - Power button for device connection control

### `ActionButtonComponent`

**Purpose**: A reusable action button component that combines Material Design buttons with icon and text in a standardized, accessible pattern. Integrates with design tokens for consistent semantic coloring.

**Selector**: `lib-action-button`

**Properties**:

- `icon` (required): `string` - Material Design icon name to display in the button
- `label` (required): `string` - Button text content that appears next to the icon
- `variant` (optional): `'stroked' | 'flat' | 'raised' | 'fab'` - Material button style variant - defaults to 'stroked'
- `color` (optional): `'primary' | 'success' | 'error' | 'highlight' | 'normal'` - Semantic color variant using [STYLE_GUIDE.md](STYLE_GUIDE.md) design tokens - defaults to 'primary'
- `disabled` (optional): `boolean` - Whether the button is disabled - defaults to false
- `ariaLabel` (optional): `string` - Accessibility label for screen readers (defaults to label value)

**Events**:

- `buttonClick`: Emitted when the button is clicked (only when not disabled)

**Usage Examples**:

```html
<!-- Basic usage -->
<lib-action-button icon="refresh" label="Refresh Data" (buttonClick)="refresh()">
</lib-action-button>

<!-- Success action with semantic color -->
<lib-action-button icon="download" label="Index All" color="success" (buttonClick)="indexAll()">
</lib-action-button>

<!-- Destructive action -->
<lib-action-button
  icon="reset_tv"
  label="Reset Devices"
  color="error"
  variant="raised"
  (buttonClick)="resetDevices()"
>
</lib-action-button>

<!-- Disabled state -->
<lib-action-button
  icon="upload"
  label="Upload Files"
  [disabled]="!hasFiles"
  ariaLabel="Upload selected files to server"
  (buttonClick)="upload()"
>
</lib-action-button>
```

**Style Integration**:

The component automatically maps to design token colors from [STYLE_GUIDE.md](STYLE_GUIDE.md#action-button-color-classes):

- **Color Mapping**:

  - `primary` → Uses Material Design's natural primary color (no custom classes applied)
  - `success` → [`.action-button-success`](STYLE_GUIDE.md#action-button-color-classes) (green text color from `--color-success`)
  - `error` → [`.action-button-error`](STYLE_GUIDE.md#action-button-color-classes) (red text color from `--color-error`)
  - `highlight` → [`.action-button-highlight`](STYLE_GUIDE.md#action-button-color-classes) (cyan text color from `--color-highlight`)
  - `normal` → Uses Material Design's default styling (no custom classes applied)

- **Variant Mapping**:
  - `stroked` → `mat-stroked-button` (outlined button - default)
  - `flat` → `mat-flat-button` (text button)
  - `raised` → `mat-raised-button` (elevated button)
  - `fab` → `mat-fab` (floating action button)

**Styling Philosophy**: The component preserves Material Design's natural button appearance and only overrides text color for semantic colors (`success`, `error`, `highlight`). This ensures buttons maintain proper borders, spacing, and Material styling while providing semantic color feedback through text color changes.

**Accessibility Features**:

- **Automatic ARIA handling**: Uses label as aria-label unless explicit ariaLabel provided
- **Proper button semantics**: Uses semantic `<button>` element with Material directives
- **Disabled state support**: Prevents interaction and provides visual feedback when disabled
- **Keyboard navigation**: Full keyboard accessibility support
- **Screen reader compatibility**: Meaningful labels and proper role semantics

**Type Safety**:

```typescript
import { ActionButtonVariant, ActionButtonColor } from '@teensyrom-nx/ui/components';

export class MyComponent {
  buttonVariant: ActionButtonVariant = 'raised';
  buttonColor: ActionButtonColor = 'success';

  onActionButtonClick(): void {
    console.log('Action button clicked!');
  }
}
```

**Best Practice**: Use for all action-oriented buttons with icons and text throughout the application. Replaces manual `mat-stroked-button` + `lib-icon-label` patterns. Choose semantic colors that match the action's intent (error for destructive actions, success for positive actions, etc.).

**Used In**:

- [`device-toolbar.component.html`](../libs/features/devices/src/lib/device-view/device-toolbar/device-toolbar.component.html) - Device management action buttons

---

## Icon Components

### `JoystickIconComponent`

**Purpose**: A custom SVG icon component that displays a joystick/controller icon for gaming-related UI elements. Designed for use with the `IconButtonComponent` content projection pattern.

**Selector**: `lib-joystick-icon`

**Properties**: None - Pure presentational component

**Usage Examples**:

```html
<!-- In icon buttons (recommended pattern) -->
<lib-icon-button
  ariaLabel="Games Filter"
  size="large"
  (buttonClick)="onGamesClick()"
>
  <lib-joystick-icon></lib-joystick-icon>
</lib-icon-button>

<!-- Standalone usage -->
<lib-joystick-icon></lib-joystick-icon>

<!-- In other components -->
<div class="game-section">
  <lib-joystick-icon></lib-joystick-icon>
  <span>Games</span>
</div>
```

**Features**:

- **Responsive Sizing**: Automatically inherits size from parent button (18px for small, 24px for medium, 36px for large)
- **Theme Integration**: Uses `fill: currentColor` to inherit text color from parent for consistent theming
- **High Quality SVG**: Vector-based icon that scales cleanly at any size
- **Accessibility Ready**: Works seamlessly with `IconButtonComponent` accessibility features

**Used In**:

- [`filter-toolbar.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.html) - Games filter button

### `ImageIconComponent`

**Purpose**: A custom SVG icon component that displays an image/photo icon for image-related UI elements. Designed for use with the `IconButtonComponent` content projection pattern.

**Selector**: `lib-image-icon`

**Properties**: None - Pure presentational component

**Usage Examples**:

```html
<!-- In icon buttons (recommended pattern) -->
<lib-icon-button
  ariaLabel="Images Filter"
  size="large"
  (buttonClick)="onImagesClick()"
>
  <lib-image-icon></lib-image-icon>
</lib-icon-button>

<!-- Standalone usage -->
<lib-image-icon></lib-image-icon>

<!-- In gallery components -->
<div class="image-section">
  <lib-image-icon></lib-image-icon>
  <span>Photos</span>
</div>
```

**Features**:

- **Responsive Sizing**: Automatically inherits size from parent button (18px for small, 24px for medium, 36px for large)
- **Theme Integration**: Uses `fill: currentColor` to inherit text color from parent for consistent theming
- **High Quality SVG**: Vector-based icon with detailed image/photo representation
- **Accessibility Ready**: Works seamlessly with `IconButtonComponent` accessibility features

**Used In**:

- [`filter-toolbar.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.html) - Images filter button

**Custom Icon Best Practices**:

1. **Component Structure**: Follow the established pattern with minimal SCSS (just `:host` flex properties and `svg` size/color)
2. **Naming Convention**: Use `lib-[name]-icon` selector pattern for consistency
3. **Size Inheritance**: Always use `fill: currentColor` and size inheritance from parent components
4. **Content Projection**: Design icons specifically for use with `IconButtonComponent` ng-content pattern
5. **Export Path**: Add new icon components to `libs/ui/components/src/index.ts` for easy importing
6. **Documentation**: Document each icon's purpose and usage patterns in this component library

---

## Display Components

### `StyledIconComponent`

**Purpose**: A reusable styled icon component that displays Material Design icons with consistent sizing and semantic coloring from the design system.

**Selector**: `lib-styled-icon`

**Properties**:

- `icon` (required): `string` - Material Design icon name to display
- `color` (optional): `'normal' | 'primary' | 'highlight' | 'success' | 'error' | 'dimmed' | 'directory'` - Semantic color variant from design system - defaults to 'normal'
- `size` (optional): `'small' | 'medium' | 'large'` - Size variant - defaults to 'medium'

**Usage Examples**:

```html
<!-- Directory icon with directory color (yellow/gold) -->
<lib-styled-icon icon="folder" color="directory" size="medium"> </lib-styled-icon>

<!-- Device icon with primary color -->
<lib-styled-icon icon="devices" color="primary" size="large"> </lib-styled-icon>

<!-- Storage type icon with highlight color -->
<lib-styled-icon icon="sd_card" color="highlight"> </lib-styled-icon>

<!-- Success icon with success color -->
<lib-styled-icon icon="check_circle" color="success" size="small"> </lib-styled-icon>

<!-- Error icon with error color -->
<lib-styled-icon icon="error" color="error"> </lib-styled-icon>

<!-- Simple icon without color -->
<lib-styled-icon icon="info"> </lib-styled-icon>
```

**Advanced Usage Patterns**:

```html
<!-- Directory listing with styled icons -->
@for (item of directoryItems; track item.id) {
<div class="directory-item">
  @if (item.isDirectory) {
  <lib-styled-icon icon="folder" color="directory" size="medium"></lib-styled-icon>
  } @else {
  <lib-styled-icon icon="insert_drive_file" color="normal" size="medium"></lib-styled-icon>
  }
  <span>{{ item.name }}</span>
</div>
}

<!-- Navigation tree with different icon types -->
<lib-styled-icon [icon]="nodeIcon()" [color]="nodeColor()" [size]="nodeSize()"></lib-styled-icon>
```

**Style Integration**:

The component automatically maps to design system colors and global style classes from [STYLE_GUIDE.md](STYLE_GUIDE.md#styled-icon-classes):

- **Size Mapping**:

  - `small` → [`.styled-icon-small`](STYLE_GUIDE.md#styled-icon-classes) (16px font, 14px dimensions)
  - `medium` → [`.styled-icon-medium`](STYLE_GUIDE.md#styled-icon-classes) (24px font, 20px dimensions - matches directory tree icons)
  - `large` → [`.styled-icon-large`](STYLE_GUIDE.md#styled-icon-classes) (32px font, 28px dimensions)

- **Color Mapping** (references design system CSS variables):
  - `normal` → Default Material icon color
  - `primary` → [`.styled-icon-primary`](STYLE_GUIDE.md#styled-icon-classes) (uses `--color-primary-bright`)
  - `highlight` → [`.styled-icon-highlight`](STYLE_GUIDE.md#styled-icon-classes) (uses `--color-highlight`)
  - `success` → [`.styled-icon-success`](STYLE_GUIDE.md#styled-icon-classes) (uses `--color-success`)
  - `error` → [`.styled-icon-error`](STYLE_GUIDE.md#styled-icon-classes) (uses `--color-error`)
  - `dimmed` → [`.styled-icon-dimmed`](STYLE_GUIDE.md#styled-icon-classes) (uses `--color-dimmed`)
  - `directory` → [`.styled-icon-directory`](STYLE_GUIDE.md#styled-icon-classes) (uses `--color-directory`)

**Type Safety**:

```typescript
import { StyledIconSize, StyledIconColor } from '@teensyrom-nx/ui/components';

export class MyComponent {
  iconSize: StyledIconSize = 'medium';
  iconColor: StyledIconColor = 'directory';
}
```

**Best Practice**: Use for all standalone icon displays throughout the application to maintain consistent sizing and semantic coloring. Ideal for directory listings, file browsers, navigation trees, and any context where icons need standardized appearance.

**Used In**:

- Directory tree components for folder/device/storage icons
- File listing components for file type icons
- Navigation components for visual indicators

### `IconLabelComponent`

**Purpose**: A versatile component that displays a styled icon alongside text with optional color, size, and text truncation controls. Built on [StyledIconComponent](COMPONENT_LIBRARY.md#stylediconcomponent) for consistent icon styling.

**Selector**: `lib-icon-label`

**Properties**:

- `icon` (optional): `string` - Material Design icon name to display - defaults to empty string
- `label` (optional): `string` - Text label to display next to the icon - defaults to empty string
- `color` (optional): `'normal' | 'primary' | 'highlight' | 'success' | 'error' | 'dimmed' | 'directory'` - Icon color from design system - defaults to 'normal'
- `size` (optional): `'small' | 'medium' | 'large'` - Icon size - defaults to 'medium'
- `truncate` (optional): `boolean` - Enable text truncation with ellipsis (20ch max width) - defaults to true

**Usage Examples**:

```html
<!-- Basic usage (backward compatible) -->
<lib-icon-label icon="folder" label="Documents"> </lib-icon-label>

<!-- With custom color and size -->
<lib-icon-label icon="folder" label="My Folder" color="directory" size="large"> </lib-icon-label>

<!-- Directory tree node pattern -->
<lib-icon-label icon="folder" label="Games" color="directory" size="medium" [truncate]="false">
</lib-icon-label>

<!-- File listing in table (no truncation) -->
<lib-icon-label
  [icon]="file.isDirectory ? 'folder' : 'insert_drive_file'"
  [label]="file.name"
  [color]="file.isDirectory ? 'directory' : 'normal'"
  size="medium"
  [truncate]="false"
>
</lib-icon-label>

<!-- Device info with truncation -->
<lib-icon-label icon="tag" [label]="'Device ID: ' + deviceId"> </lib-icon-label>
```

**Advanced Usage Patterns**:

```typescript
// Dynamic icon and color based on file type
export class FileListComponent {
  getFileIcon(file: FileInfo): string {
    if (file.isDirectory) return 'folder';
    const ext = file.name.split('.').pop()?.toLowerCase();
    switch (ext) {
      case 'jpg':
      case 'png':
        return 'image';
      case 'mp3':
        return 'audio_file';
      default:
        return 'insert_drive_file';
    }
  }

  getFileColor(file: FileInfo): StyledIconColor {
    return file.isDirectory ? 'directory' : 'normal';
  }
}
```

```html
<!-- In template -->
<lib-icon-label
  [icon]="getFileIcon(file)"
  [label]="file.name"
  [color]="getFileColor(file)"
  size="medium"
  [truncate]="false"
>
</lib-icon-label>
```

**Features**:

- **Styled Icons**: Uses [StyledIconComponent](COMPONENT_LIBRARY.md#stylediconcomponent) internally for consistent icon appearance
- **Flexible Layout**: Icon and text horizontally aligned with 0.5rem gap
- **Text Truncation**: Optional ellipsis truncation for long labels (enabled by default)
- **Accessibility**: Text includes `title` attribute for tooltips on hover
- **Backward Compatible**: Works with minimal props (just icon/label) using sensible defaults

**Style Integration**:

The component leverages:

- [StyledIconComponent](COMPONENT_LIBRARY.md#stylediconcomponent) for icon styling
- [`.icon-label-container`](STYLE_GUIDE.md) for flex layout
- Conditional `.truncate` class for text overflow handling

**Best Practice**: Use for any icon-text combination throughout the application. The enhanced props (color, size, truncate) allow it to work in diverse contexts:

- **Device info displays** - default truncation for long IDs
- **Directory trees** - colored icons with no truncation
- **File tables** - dynamic icons based on file type with full text display
- **Navigation menus** - consistent icon-label pattern

**Used In**:

- [`device-item.component.html`](../libs/features/devices/src/lib/device-view/device-item/device-item.component.html) - Device information labels
- [`storage-item.component.html`](../libs/features/devices/src/lib/device-view/storage-item/storage-item.component.html) - Storage information labels
- [`action-button.component.html`](../libs/ui/components/src/lib/action-button/action-button.component.html) - Button labels with icons
- [`directory-tree-node.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree-node/directory-tree-node.component.html) - Tree node labels with semantic colors

### `StatusIconLabelComponent`

**Purpose**: An enhanced icon-label component that adds status indicators with success/error icons.

**Selector**: `lib-status-icon-label`

**Properties**:

- `icon`: `string` - Primary Material Design icon name (defaults to empty string)
- `label`: `string` - Text label to display (defaults to empty string)
- `status`: `boolean | undefined` - Status indicator:
  - `true`: Shows green check circle icon
  - `false`: Shows red cancel icon
  - `undefined`: No status icon displayed

**Usage Example**:

```html
<!-- Success status -->
<lib-status-icon-label icon="wifi" label="Network Connection" [status]="true">
</lib-status-icon-label>

<!-- Error status -->
<lib-status-icon-label icon="storage" label="USB Storage" [status]="false"> </lib-status-icon-label>

<!-- No status -->
<lib-status-icon-label icon="settings" label="Configuration"> </lib-status-icon-label>
```

**Features**:

- Extends `IconLabelComponent` functionality
- Conditional status icons based on boolean state
- Uses semantic color classes (`.success`, `.error`) from [global styles](STYLE_GUIDE.md#icon-color-classes)

**Best Practice**: Use for device status, connection states, or any scenario where you need to show success/failure alongside descriptive text.

---

## Navigation Components

### `MenuItemComponent`

**Purpose**: A standardized menu item component for consistent navigation and menu styling across the application.

**Selector**: `lib-menu-item`

**Properties**:

- `menuItem`: `MenuItem` - Configuration object that defines the menu item's appearance and behavior

**MenuItem Interface**:

```typescript
export interface MenuItem {
  icon?: string; // Material Design icon name
  label: string; // Display text for the menu item
  route?: string; // Angular route for navigation
  action?: () => void; // Custom action function
  disabled?: boolean; // Whether the item is disabled
  children?: MenuItem[]; // Nested menu items for submenus
}
```

**Usage Example**:

```typescript
// Component
export class NavigationComponent {
  menuItems: MenuItem[] = [
    {
      icon: 'dashboard',
      label: 'Dashboard',
      route: '/dashboard',
    },
    {
      icon: 'devices',
      label: 'Devices',
      children: [
        { label: 'Connected Devices', route: '/devices/connected' },
        { label: 'Device Logs', route: '/devices/logs' },
      ],
    },
    {
      icon: 'settings',
      label: 'Settings',
      action: () => this.openSettings(),
      disabled: !this.hasPermission,
    },
  ];
}
```

```html
<!-- Template -->
@for (item of menuItems; track item.label) {
<lib-menu-item [menuItem]="item"></lib-menu-item>
}
```

**Features**:

- Supports both routing and custom actions
- Handles nested menu structures for submenus
- Automatic disabled state styling
- Consistent icon and text alignment
- Keyboard navigation support

**Best Practice**: Use for all navigation menus, context menus, and action lists to maintain consistent interaction patterns.

---

## Component Architecture

### Design Principles

All shared components follow these architectural patterns:

1. **Standalone Components**: No NgModule dependencies, import directly into consuming components
2. **Signal-Based Inputs**: Use `input()` function instead of `@Input()` decorators for better type safety
3. **Modern Control Flow**: Leverage `@if`, `@for`, `@switch` syntax for template logic
4. **Content Projection**: Support flexible content via `<ng-content>` where appropriate
5. **Material Design Integration**: Built on Angular Material foundation with custom styling

### Import Pattern

```typescript
// In consuming components
import { CardLayoutComponent } from '@teensyrom-nx/ui/components';
import { CompactCardLayoutComponent } from '@teensyrom-nx/ui/components';
import { InputFieldComponent } from '@teensyrom-nx/ui/components';
import { IconButtonComponent } from '@teensyrom-nx/ui/components';
import { StyledIconComponent } from '@teensyrom-nx/ui/components';
import { IconLabelComponent } from '@teensyrom-nx/ui/components';
import { StatusIconLabelComponent } from '@teensyrom-nx/ui/components';
import { MenuItemComponent } from '@teensyrom-nx/ui/components';

@Component({
  // ...
  imports: [
    CardLayoutComponent,
    CompactCardLayoutComponent,
    InputFieldComponent,
    IconButtonComponent,
    StyledIconComponent,
    IconLabelComponent,
    StatusIconLabelComponent,
    MenuItemComponent,
    // other imports...
  ],
})
```

### Testing Support

All components include comprehensive unit tests using Vitest:

- **Basic rendering tests**: Verify component creation and initial state
- **Input property tests**: Validate signal-based input handling
- **Content projection tests**: Ensure ng-content works correctly
- **Conditional rendering tests**: Test dynamic show/hide behavior
- **Accessibility tests**: Verify proper ARIA attributes and keyboard support

### Global Styling Integration

Components are designed to work seamlessly with the global style system:

- **Design Tokens**: Use CSS custom properties for consistent theming
- **Utility Classes**: Apply global classes like [`.stretch-card`](STYLE_GUIDE.md#stretch-card) automatically
- **Theme Support**: Full light/dark mode compatibility
- **Material Customizations**: Leverage custom Material component styles

---

## Usage Guidelines

### When to Use Shared Components

- **Consistency**: Use shared components for any UI pattern that appears in multiple places
- **Maintenance**: Prefer shared components over duplicating similar code across features
- **Theming**: Shared components ensure consistent application of global themes and styles

### When to Create New Components

Consider adding to the shared library when you find yourself:

- Creating similar components in multiple feature libraries
- Needing to style Material components consistently across the app
- Building reusable layout patterns that could benefit other developers

### Documentation Maintenance

**IMPORTANT**: This document must be updated whenever new shared components are added.

**Update Requirements**:

1. **Document new components** with complete property/event listings
2. **Provide usage examples** showing real-world implementation
3. **Include TypeScript interfaces** for complex input types
4. **Cross-reference** with [STYLE_GUIDE.md](STYLE_GUIDE.md) for related styling
5. **Update import patterns** and architecture notes as needed

---

## Related Files

- **Component Library**: `libs/ui/components/src/lib/`
- **Global Styles**: [STYLE_GUIDE.md](STYLE_GUIDE.md) - Utility classes and Material customizations
- **Barrel Exports**: `libs/ui/components/src/index.ts` - Public API for importing components
- **Component Tests**: Individual `.spec.ts` files with comprehensive test coverage
