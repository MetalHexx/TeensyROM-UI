# TeensyROM UI Component Library

## Overview

This document catalogs all shared UI components available in the TeensyROM application. All components are located in `libs/ui/components/src/lib/` and follow Angular 19 standalone component architecture with modern signal-based inputs.

---

## Layout Components

Pure layout components without animations. Use these when you need static cards.

### `CardLayoutComponent`

**Purpose**: Card layout with headers, titles, metadata, and corner slots. No animations.

**Selector**: `lib-card-layout`

**Properties**:
- `title`: Header title (optional)
- `subtitle`: Text below title (optional)
- `metadataSource`: Footer attribution (optional)
- `enableOverflow`: Allow scrollbars (default: `true`)

**Usage**:

```html
<lib-card-layout title="File Info" subtitle="v1.2.3">
  <mat-chip slot="corner"><mat-chip>C64</mat-chip>
  <p>Content...</p>
</lib-card-layout>
```

**Content Slots**: Default slot for content, `[slot=corner]` for upper-right corner

**See Also**: [ScalingCardComponent](#scalingcardcomponent) for animated version

### `CompactCardLayoutComponent`

**Purpose**: Minimal card layout for forms and toolbars. No headers, no animations.

**Selector**: `lib-compact-card-layout`

**Properties**:
- `enableOverflow`: Allow scrollbars (default: `true`)

**Usage**:

```html
<lib-compact-card-layout>
  <mat-form-field>
    <input matInput />
  </mat-form-field>
</lib-compact-card-layout>
```

**See Also**: [ScalingCompactCardComponent](#scalingcompactcardcomponent) for animated version

---

## Animated Card Components

Animated cards that combine layout + animation. See [Animation System](#animation-system) for chaining details.

### `ScalingCardComponent`

**Purpose**: Animated card with scale+fade+slide effects. Composes [CardLayoutComponent](#cardlayoutcomponent) + [ScalingContainerComponent](#scalingcontainercomponent).

**Selector**: `lib-scaling-card`

**Properties**:
- Layout props: `title`, `subtitle`, `metadataSource`, `enableOverflow`
- Animation props: `animationEntry`, `animationExit`, `animationTrigger`, `animationParent`

**Usage**:

```html
<lib-scaling-card title="Device" animationEntry="from-left">
  <button mat-icon-button slot="corner"><mat-icon>more_vert</mat-icon></button>
  <p>Content...</p>
</lib-scaling-card>
```

**See Also**: [CardLayoutComponent](#cardlayoutcomponent), [ScalingContainerComponent](#scalingcontainercomponent), [Animation System](#animation-system)

### `ScalingCompactCardComponent`

**Purpose**: Animated compact card for forms/toolbars. Composes [CompactCardLayoutComponent](#compactcardlayoutcomponent) + [ScalingContainerComponent](#scalingcontainercomponent).

**Selector**: `lib-scaling-compact-card`

**Properties**:
- Layout props: `enableOverflow`
- Animation props: `animationEntry`, `animationExit`, `animationTrigger`, `animationParent`

**Usage**:

```html
<lib-scaling-compact-card animationEntry="from-top">
  <mat-form-field><input matInput /></mat-form-field>
</lib-scaling-compact-card>
```

**See Also**: [CompactCardLayoutComponent](#compactcardlayoutcomponent), [ScalingContainerComponent](#scalingcontainercomponent), [Animation System](#animation-system)

---

## Animation Container Components

Reusable animation wrappers. Can be used directly or composed into other components.

### `SlidingContainerComponent`

**Purpose**: Container slide animation with height/width expansion. Affects document flow - content pushes/pulls surrounding elements.

**Selector**: `lib-sliding-container`

**Properties**:
- `containerHeight`: Height (default: `'auto'`)
- `containerWidth`: Width (default: `'auto'`)
- `animationDuration`: Duration in ms (default: `400`)
- `animationDirection`: Direction - `'from-top'`, `'slide-down'`, `'slide-up'`, `'fade'`, etc. (default: `'from-top'`)
- `animationTrigger`: Manual control signal (optional)
- `animationParent`: Override animation chaining (optional - see [Animation System](#animation-system))

**Events**:
- `animationComplete`: Emitted when animation finishes

**Usage**:

```html
<lib-sliding-container
  containerHeight="80px"
  animationDirection="from-top"
  [animationTrigger]="isReady()">
  <lib-scaling-compact-card>
    <div class="toolbar">...</div>
  </lib-scaling-compact-card>
</lib-sliding-container>
```

**See Also**: [ScalingContainerComponent](#scalingcontainercomponent), [Animation System](#animation-system)

### `ScalingContainerComponent`

**Purpose**: Scale+fade+slide animation wrapper. Transform-based - doesn't affect document flow. Creates "pop-in" effect.

**Selector**: `lib-scaling-container`

**Properties**:
- `animationEntry`: Entry direction - `'random'`, `'from-left'`, `'from-right'`, `'from-top'`, `'from-bottom'`, etc. (default: `'random'`)
- `animationExit`: Exit direction (default: `'random'`)
- `animationTrigger`: Manual control signal (optional)
- `animationParent`: Override animation chaining (optional - see [Animation System](#animation-system))

**Events**:
- `animationComplete`: Emitted when animation finishes

**Animation**: 0.8→1.0 scale, opacity fade, -40px directional slide. Transform (2000ms) + opacity (3000ms) for smooth reveal.

**Usage**:

```html
<lib-scaling-container animationEntry="from-left">
  <div class="panel">Scales in from left</div>
</lib-scaling-container>

<!-- Nested with auto-chaining -->
<lib-sliding-container [animationTrigger]="isReady()">
  <lib-scaling-container animationEntry="from-top">
    <div>Waits for parent</div>
  </lib-scaling-container>
</lib-sliding-container>
```

**Comparison**:
- **ScalingContainer**: Transform-based, no layout impact, visual "pop" effect
- **SlidingContainer**: Height/width expansion, pushes surrounding content

**Used By**: [ScalingCardComponent](#scalingcardcomponent), [ScalingCompactCardComponent](#scalingcompactcardcomponent)

**See Also**: [SlidingContainerComponent](#slidingcontainercomponent), [Animation System](#animation-system)

---

## Animation System

**Purpose**: Automatic animation coordination between components via Angular's dependency injection. Components can chain animations when nested, or use explicit triggers for independent control.

**How It Works**:

Animation components use Angular DI to provide/inject completion signals via `PARENT_ANIMATION_COMPLETE` token. Components **do not auto-chain by default** - you must explicitly opt-in using `animationParent="auto"` or set a custom animation parent. This prevents unintended chaining when nesting components for layout purposes.

**Priority System**:

1. **Explicit trigger** (highest): Use provided `animationTrigger` signal
2. **Custom parent override** (high): Use provided `animationParent` to opt-in or redirect chains
3. **Immediate** (default): Render immediately if no trigger or parent specified

**Controlling Animation Chaining**:

All animatable components support an `animationParent` input for controlling when they wait for parent animations:

- `undefined` (default): No waiting - component animates immediately, ignoring any parent animations
- `null`: Same as undefined - no waiting, immediate animation
- `'auto'`: Opt-in to wait for the nearest animation parent in the DI tree
- `AnimationParent`: Wait for a specific component reference (can be sibling, ancestor, or any component)

**Important**: Components **always register as animation parents** regardless of their `animationParent` setting. The `animationParent` input only controls whether a component **waits for** a parent, not whether it **is available as** a parent to its children.

> **Note**: "Animation parent" refers to the animation chaining relationship, not the DOM parent. You can set any component as an animation parent - siblings, ancestors, or components anywhere in your tree. This decouples animation choreography from component hierarchy.

**Usage**:

```html
<!-- Explicit auto-chaining: nested components opt-in to wait for parent -->
<lib-sliding-container [animationTrigger]="isReady()">
  <!-- Must explicitly use animationParent="auto" to wait for parent -->
  <lib-scaling-compact-card animationEntry="from-top" animationParent="auto">
    <div>Animates after parent (explicit opt-in)</div>
  </lib-scaling-compact-card>
</lib-sliding-container>

<!-- Independent control: components animate immediately by default -->
<div class="layout">
  <lib-scaling-card>Animates immediately</lib-scaling-card>
  <lib-scaling-card>Animates immediately</lib-scaling-card>
  <lib-scaling-card [animationTrigger]="showCard3()">Explicit timing</lib-scaling-card>
</div>

<!-- Multi-level chaining with explicit opt-in -->
<lib-scaling-card title="Parent" [animationTrigger]="show()">
  <!-- Child must opt-in to wait for parent -->
  <lib-sliding-container animationDirection="slide-down" animationParent="auto">
    <!-- Grandchild must opt-in to wait for sliding-container -->
    <lib-scaling-compact-card animationParent="auto">
      <p>Waits for parent, then sliding-container</p>
    </lib-scaling-compact-card>
  </lib-sliding-container>
</lib-scaling-card>

<!-- Nesting for layout without animation chaining -->
<lib-sliding-container [animationTrigger]="isReady()">
  <!-- These don't wait - they're just using the container for layout -->
  <lib-scaling-card>Animates immediately</lib-scaling-card>
  <lib-scaling-card>Animates immediately</lib-scaling-card>
  
  <!-- This one opts into waiting for parent -->
  <lib-scaling-card animationParent="auto">Waits for parent animation</lib-scaling-card>
</lib-sliding-container>

<!-- Custom parent: redirect animation chain to any component -->
<div class="layout">
  <!-- Master timeline controller (animation parent for coordination) -->
  <lib-sliding-container #masterTimeline [animationTrigger]="show()">
  </lib-sliding-container>
  
  <!-- Elsewhere in the tree - siblings sync to same animation parent -->
  <div class="section-a">
    <lib-scaling-card [animationParent]="masterTimeline">Card A</lib-scaling-card>
  </div>
  
  <div class="section-b">
    <lib-scaling-card [animationParent]="masterTimeline">Card B</lib-scaling-card>
  </div>
</div>
```

**Flexibility**: By default, components animate immediately. Use `animationParent="auto"` for parent/child coordination, explicit `animationTrigger` signals for any timing pattern, or `animationParent` with a component reference to create custom choreography - sequenced animations, parallel animations, conditional animations, independent timelines, etc.

**Applies To**: All animation components - [ScalingCardComponent](#scalingcardcomponent), [ScalingCompactCardComponent](#scalingcompactcardcomponent), [ScalingContainerComponent](#scalingcontainercomponent), [SlidingContainerComponent](#slidingcontainercomponent)

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

### `ThumbnailImageComponent`

**Purpose**: A compact image thumbnail component for displaying cover art, album artwork, or file previews. Designed for use in media players, file listings, and anywhere a small preview image is needed.

**Selector**: `lib-thumbnail-image`

**Properties**:

- `imageUrl` (optional): `string | null` - URL of the image to display. Component renders nothing when null.
- `size` (optional): `'small' | 'medium' | 'large'` - Size variant - defaults to 'medium'

**Usage Examples**:

```html
<!-- Basic usage with medium size (default) -->
<lib-thumbnail-image [imageUrl]="file.coverArtUrl"></lib-thumbnail-image>

<!-- Small thumbnail for compact lists -->
<lib-thumbnail-image [imageUrl]="album.thumbnailUrl" size="small"></lib-thumbnail-image>

<!-- Large thumbnail for detailed views -->
<lib-thumbnail-image [imageUrl]="track.artworkUrl" size="large"></lib-thumbnail-image>

<!-- Conditional rendering (only shows when URL exists) -->
<lib-thumbnail-image [imageUrl]="file.images[0]?.url ?? null" size="medium"></lib-thumbnail-image>
```

**Advanced Usage Patterns**:

```typescript
// Component with computed thumbnail URL
export class FileInfoComponent {
  fileItem = input<FileItem | null>();

  // Get the first available image
  thumbnailUrl = computed(() => {
    const item = this.fileItem();
    if (!item?.images || item.images.length === 0) {
      return null;
    }
    return item.images[0].url;
  });
}
```

```html
<!-- In template -->
<lib-thumbnail-image [imageUrl]="thumbnailUrl()" size="medium"></lib-thumbnail-image>
```

**Features**:

- **Conditional Rendering**: Only renders when imageUrl is provided (not null)
- **Responsive Sizing**: Three size variants for different UI contexts
- **Object Fit**: Uses `object-fit: cover` for proper image scaling without distortion
- **Rounded Corners**: 4px border radius for modern card-style appearance
- **Fallback Background**: Subtle background color when image is loading

**Style Details**:

- **Size Mapping**:
  - `small` → 32px × 32px (compact lists, inline previews)
  - `medium` → 48px × 48px (standard player controls, file listings)
  - `large` → 64px × 64px (detailed views, featured content)

- **Visual Properties**:
  - `object-fit: cover` - Maintains aspect ratio, crops to fit
  - `border-radius: 4px` - Subtle rounded corners
  - `background-color: rgba(255, 255, 255, 0.05)` - Loading state indicator

**Best Practice**: Use for any thumbnail image display throughout the application. The component handles null URLs gracefully, making it safe to use with optional image data. Perfect for media player controls, file browsers, and anywhere visual preview enhances UX.

**Used In**:

- [`file-info.component.html`](../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/file-info/file-info.component.html) - Media player file information display

### `CycleImageComponent`

**Purpose**: An advanced image carousel component that automatically cycles through multiple images with smooth fade transitions and optional blurred background effects. Supports multiple size variants from small thumbnails to large detail views.

**Selector**: `lib-cycle-image`

**Properties**:

- `images` (required): `string[]` - Array of image URLs to cycle through
- `intervalMs` (optional): `number` - Milliseconds between image transitions - defaults to 8000 (8 seconds)
- `placeholderUrl` (optional): `string` - Fallback image when no images provided - defaults to '/placeholder.jpg'
- `size` (optional): `'thumbnail' | 'small' | 'medium' | 'large'` - Size variant - defaults to 'large'

**Usage Examples**:

```html
<!-- Large detail view with blurred background (default) -->
<lib-cycle-image [images]="albumArtUrls"></lib-cycle-image>

<!-- Small thumbnail for player toolbar (simple mode - no blur) -->
<lib-cycle-image
  [images]="fileItem.images.map(img => img.url)"
  size="thumbnail">
</lib-cycle-image>

<!-- Medium size with custom interval -->
<lib-cycle-image
  [images]="galleryImages"
  size="medium"
  [intervalMs]="5000">
</lib-cycle-image>

<!-- With placeholder for empty state -->
<lib-cycle-image
  [images]="screenshots"
  placeholderUrl="/assets/no-image.png"
  size="large">
</lib-cycle-image>
```

**Advanced Usage Patterns**:

```typescript
// Component with computed image URLs from FileItem
export class FileInfoComponent {
  fileItem = input<FileItem | null>();

  // Extract all image URLs for cycling
  imageUrls = computed(() => {
    const item = this.fileItem();
    if (!item?.images || item.images.length === 0) {
      return [];
    }
    return item.images.map(img => img.url);
  });
}
```

```html
<!-- In template - conditional rendering with cycling -->
@if (imageUrls().length > 0) {
  <lib-cycle-image [images]="imageUrls()" size="thumbnail"></lib-cycle-image>
}
```

**Features**:

- **Automatic Cycling**: Smoothly transitions between images at specified interval
- **Fade Animations**: 1-second fade transitions with opacity and blur effects
- **Blurred Background**: Large/medium sizes show artistic blurred background (complex mode)
- **Simple Mode**: Thumbnail/small sizes disable blur effects for clean thumbnails
- **Single Image Support**: Works perfectly with single image (no cycling)
- **Empty State Handling**: Shows placeholder when images array is empty
- **Responsive Sizing**: Four size variants for different UI contexts

**Size Variants**:

- **`thumbnail`** (48px × 48px):
  - Simple mode (no blur effects)
  - `object-fit: cover` for clean thumbnails
  - 4px border radius
  - Perfect for: Player toolbars, compact lists, inline previews

- **`small`** (80px × 80px):
  - Simple mode (no blur effects)
  - `object-fit: cover`
  - 6px border radius
  - Perfect for: Card thumbnails, small galleries

- **`medium`** (160px × 160px):
  - Complex mode with blurred background
  - `object-fit: contain` with clip-path
  - 8px border radius
  - Perfect for: Album art displays, featured content

- **`large`** (full width/height):
  - Complex mode with blurred background
  - `object-fit: contain` with clip-path
  - 16px border radius
  - Perfect for: Full detail views, main content displays

**Animation Details**:

- **Fade Duration**: 1 second for smooth transitions
- **Blur Animation**: 1.5 seconds fade-in-from-blur effect (complex mode only)
- **Background Blur**: 20px gaussian blur at 80% opacity
- **Layer Management**: Z-index controlled layering for smooth overlays

**Complex vs Simple Mode**:

- **Complex Mode** (medium/large):
  - Dual-layer blurred backgrounds for depth
  - Fade-in-from-blur animation
  - Artistic presentation with `object-fit: contain`
  - 16px clip-path rounded corners

- **Simple Mode** (thumbnail/small):
  - Single image layer only
  - Standard fade animation
  - Clean presentation with `object-fit: cover`
  - No blur effects for performance

**Best Practice**:
- Use `size="thumbnail"` in player toolbars and compact UI
- Use `size="medium"` or `size="large"` for detail views and galleries
- Provide multiple images for automatic cycling effect
- Works seamlessly with single image (no cycling needed)
- The component automatically selects simple vs complex mode based on size

**Used In**:

- [`file-info.component.html`](../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/file-info/file-info.component.html) - Player toolbar cycling thumbnails
- File detail views with multiple screenshots/artwork

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
