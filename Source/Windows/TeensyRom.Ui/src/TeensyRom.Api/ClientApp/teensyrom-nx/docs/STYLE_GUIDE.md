# TeensyROM UI Style Guide

## Overview

This document catalogs all global styles, utility classes, and Material Design customizations available in the TeensyROM application. All styles are defined in `libs/ui/styles/src/lib/theme/styles.scss`.

---

## Color Schemes & Themes

### Light Mode

- **Material Palette**: `mat.$azure-palette`
- **Typography**: Roboto
- **Surface Container**: `#cdcdcd`

### Dark Mode

- **Material Palette**: `mat.$magenta-palette`
- **Typography**: Roboto
- **Surface Container**: `#231e22`

### Custom Color Variables

Available color variables for semantic styling:

- `--color-primary`: Purple brand color for primary actions and branding
- `--color-success`: Green variants for success states
- `--color-error`: Red variants for error states
- `--color-highlight`: Cyan accent color
- `--color-dimmed`: Gray for secondary content

**Usage Example:**

```scss
.my-icon {
  color: var(--color-success);
}

.primary-button {
  background-color: var(--color-primary);
}
```

**Best Practice:** Use these semantic color classes instead of hardcoded colors for consistency and proper theme support.

---

## Utility Classes

### `.dimmed`

**Purpose**: Reduces opacity to 50% for disabled or secondary content

**Usage Example:**

```html
<mat-icon class="dimmed">settings</mat-icon>
<p class="dimmed">Secondary text</p>
```

**Used In:**

- [`device-logs.component.html`](../libs/features/devices/src/lib/device-view/device-logs/device-logs.component.html) - For "No logs to display" message
- [`storage-item.component.html`](../libs/features/devices/src/lib/device-view/storage-item/storage-item.component.html) - For disabled storage items
- [`device-item.component.html`](../libs/features/devices/src/lib/device-view/device-item/device-item.component.html) - For disconnected devices

### `.no-text-selection`

**Purpose**: Prevents text selection on interactive elements to avoid unwanted text highlighting during double-clicks and rapid user interactions

**Usage Example:**

```html
<!-- For buttons and interactive list items -->
<div class="file-item no-text-selection" (dblclick)="openFile()">
  <span>filename.txt</span>
</div>

<!-- For tree nodes and clickable elements -->
<div class="directory-tree-node no-text-selection" (click)="selectNode()">
  <span>Folder Name</span>
</div>
```

**Used In:**

- [`file-item.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/file-item/file-item.component.html) - Prevents text selection on file double-click
- [`directory-item.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-item/directory-item.component.html) - Prevents text selection on directory double-click  
- [`directory-tree-node.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree-node/directory-tree-node.component.html) - Prevents text selection on tree node interactions

**Best Practice**: Apply to any interactive element that users might double-click or rapidly interact with where text selection would be distracting or interfere with the intended user action. Essential for components using the `selectable-item` mixin that need clean double-click behavior.

### `.list-item-highlight`

**Purpose**: Provides pulsing highlight effect for active items in lists with automatic error state handling.

**Usage Example:**

```html
<div class="file-list-item list-item-highlight"
     [attr.data-is-playing]="isActive(item)"
     [attr.data-has-error]="hasError() && isActive(item)">
  <!-- item content -->
</div>
```

**Data Attribute Contract:**
- `data-is-playing="true"` - Triggers cyan pulsing highlight border for active item
- `data-is-playing="true"` + `data-has-error="true"` - Triggers red error pulsing highlight border for active item with error

**Visual Effect:**
- Cyan pulsing border (left side) for active items
- Red pulsing border (left side) for active items with errors
- 10px border radius for modern rounded appearance
- 15% opacity pulsing animation

**Used In:**
- [`directory-files.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.html) - Active file in directory
- [`search-results.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-results/search-results.component.html) - Active search result
- [`play-history.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/play-history/play-history.component.html) - Active history entry

**Best Practice**: Use this utility class for any list that displays items with an active state and potential error states. Apply it alongside component-specific classes (e.g., `class="file-list-item list-item-highlight"`). The data attributes provide semantic meaning and enable consistent visual feedback across all lists in the application.

**Implementation Note**: This class works in conjunction with the `pulsing-highlight` mixin and requires theme color variables (`--color-highlight`, `--color-error`) to be defined.

### Glassy Effect Variations

**Purpose**: Creates glassmorphism effects with varying opacity levels for layered UI elements. All variations are theme-aware and automatically adjust between light and dark modes.

#### `.glassy-subtle`

**Opacity**: 5% (ultra-light overlay)
**Blur**: 10px backdrop blur
**Theme Support**: White in light mode, black in dark mode

**Usage Example:**

```html
<div class="glassy-subtle">Minimal overlay content</div>
```

**Best Practice**: Use for barely-visible background separation or subtle depth effects where minimal visual weight is desired. Ideal for secondary backgrounds that shouldn't draw attention.

#### `.glassy-light`

**Opacity**: 7.5% (light overlay)
**Blur**: 10px backdrop blur
**Theme Support**: White in light mode, black in dark mode

**Usage Example:**

```html
<div class="glassy-light">Light overlay content</div>
```

**Best Practice**: Use for subtle glassmorphism on secondary UI elements that need slight visual separation without being prominent.

#### `.glassy-default` / `.glassy` (Default)

**Opacity**: 10% (standard overlay)
**Blur**: 10px backdrop blur
**Theme Support**: White in light mode (hardcoded for backward compatibility)

**Recommended**: Use `.glassy-default` for new code. The `.glassy` class is maintained for backward compatibility with existing components.

**Usage Example:**

```html
<!-- Preferred for new code -->
<div class="glassy-default">Overlay content</div>

<!-- Legacy - still works but prefer .glassy-default -->
<div class="glassy">Overlay content</div>
<mat-dialog class="glassy-dialog">...</mat-dialog>
```

**Used In:**

- [`layout.component.html`](../libs/app/shell/src/lib/layout/layout.component.html) - Navigation sidenav
- [`layout.component.ts`](../libs/app/shell/src/lib/layout/layout.component.ts) - Modal dialogs (via `panelClass: 'glassy-dialog'`)

**Best Practice**: Default glassmorphism effect for standard overlays, navigation elements, and modal dialogs. Use `.glassy-default` for new implementations to align with the naming convention of other variations.

**Migration Note**: `.glassy` is the legacy name maintained for backward compatibility. Both classes are functionally identical (they share the same CSS rules). New code should use `.glassy-default` to follow the established naming pattern. The legacy `.glassy` class will be deprecated in a future release.

#### `.glassy-medium`

**Opacity**: 15% (pronounced overlay)
**Blur**: 10px backdrop blur
**Theme Support**: White in light mode, black in dark mode

**Usage Example:**

```html
<div class="glassy-medium">Medium overlay content</div>
```

**Best Practice**: Use for more prominent glassmorphism effects where the overlay needs to be clearly visible. Good for important overlays and highlighted UI sections.

#### `.glassy-strong`

**Opacity**: 20% (bold overlay)
**Blur**: 10px backdrop blur
**Theme Support**: White in light mode, black in dark mode

**Usage Example:**

```html
<div class="glassy-strong">Strong overlay content</div>
```

**Best Practice**: Use for high-emphasis glassmorphism where the overlay needs maximum visibility while maintaining the blur effect. Ideal for critical UI elements or prominent feature sections.

---

**Implementation Details:**

- All new variations (subtle, light, medium, strong) use CSS custom property `--glassy-color` for theme awareness
- Light mode: `--glassy-color: 255, 255, 255` (white)
- Dark mode: `--glassy-color: 0, 0, 0` (black)
- Original `.glassy` class remains unchanged for backward compatibility
- All variations use the same 10px backdrop blur for consistent visual effect

**Selection Guide:**

- **Subtle (5%)**: Barely visible, minimal visual weight
- **Light (7.5%)**: Subtle effect, low emphasis
- **Default (10%)**: Standard glassmorphism, balanced visibility (use `.glassy-default` for new code)
- **Medium (15%)**: Pronounced effect, higher emphasis
- **Strong (20%)**: Bold effect, maximum visibility

**Migration Path:**

1. **Phase 1** (Current): Both `.glassy` and `.glassy-default` work identically
2. **Phase 2**: Gradually migrate existing components to use `.glassy-default`
3. **Phase 3**: Deprecate `.glassy` with console warnings (future release)
4. **Phase 4**: Remove `.glassy` in breaking change release (future major version)

---

## Mixins

### `@mixin selectable-item`

**Purpose**: Provides consistent hover and selection styling for interactive list items, tree nodes, and selectable UI elements

**Usage Example:**

```scss
@use 'path/to/styles.scss' as styles;

.my-selectable-item {
  @include styles.selectable-item;
  display: flex;
  align-items: center;
  // Add component-specific styles
}
```

**Used In:**

- [`directory-tree-node.component.scss`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree-node/directory-tree-node.component.scss) - Tree node selection and hover
- [`directory-item.component.scss`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-item/directory-item.component.scss) - Directory list item selection
- [`file-item.component.scss`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/file-item/file-item.component.scss) - File list item selection

**Best Practice:** Use this mixin for any interactive list item that needs hover feedback and selection state. The mixin provides:

- Consistent 8px padding
- 10px border radius for modern rounded appearance
- Smooth 0.2s transition for hover/selection changes
- Unified `--color-dimmed` background for selected state and a translucent variant for hover

Apply the `.selected` class to the element when it's the currently selected item to activate the selection styling.

**Recommended Pairing:** Combine with `.no-text-selection` utility class for elements that support double-click interactions to prevent unwanted text highlighting:

```html
<div class="my-item no-text-selection" [class.selected]="isSelected" (dblclick)="onAction()">
  <!-- content -->
</div>
```

### `@mixin pulsing-highlight`

**Purpose**: Creates a reusable pulsing animation effect with colored borders for highlighting active or important elements

**Usage Example:**

```scss
@use 'path/to/styles.scss' as styles;

.my-active-item {
  @include styles.pulsing-highlight(); // Default: cyan highlight, left border
}

.my-error-item {
  @include styles.pulsing-highlight(
    $color: var(--color-error),
    $opacity: 20%,
    $border-side: all,
    $duration: 1.5s
  );
}

.my-success-notification {
  @include styles.pulsing-highlight(
    $color: var(--color-success),
    $opacity: 10%,
    $border-side: top,
    $border-width: 2px
  );
}
```

**Parameters:**

- `$color`: Border and pulse color (default: `var(--color-highlight)`)
- `$opacity`: Opacity percentage for pulse effect (default: `15%`)
- `$duration`: Animation duration (default: `2s`)
- `$timing`: Animation timing function (default: `ease-in-out`)
- `$border-width`: Border thickness (default: `3px`)
- `$border-side`: Border placement - `left`, `right`, `top`, `bottom`, or `all` (default: `left`)

**Used In:**

- [`directory-files.component.scss`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.scss) - Currently playing file highlighting

**Best Practice**: Use for drawing attention to active states, currently playing items, notifications, or temporary highlights. The mixin adapts to any component's existing shape and styling - just apply it and it will pulse within the component's borders and dimensions. Combine with semantic color variables for consistent theming.

**Technical Note**: Uses CSS custom properties internally (`--pulsing-color`, `--pulsing-opacity`) to enable dynamic color/opacity combinations while maintaining a single set of keyframes for performance.

---

### `.corner-chips`

**Purpose**: Positions chip sets in the upper right corner of relatively positioned containers

**Usage Example:**

```html
<mat-card style="position: relative;">
  <mat-chip-set class="corner-chips">
    <mat-chip>Tag 1</mat-chip>
    <mat-chip>Tag 2</mat-chip>
  </mat-chip-set>
  <!-- card content -->
</mat-card>
```

**Used In:**

- [`file-other.component.html`](../libs/features/player/src/lib/player-view/player-device-container/file-other/file-other.component.html) - Metadata chips in file info cards

**Best Practice:** Ensure the parent container has `position: relative` for proper absolute positioning. Use for non-intrusive metadata or tag display that shouldn't interfere with main content layout.

### `.metadata-source`

**Purpose**: Styles metadata source text with right-aligned, subtle appearance for card footers

**Usage Example:**

```html
<mat-card-footer>
  <p class="metadata-source">Source: Database Name</p>
</mat-card-footer>
```

**Used In:**

- [`file-other.component.html`](../libs/features/player/src/lib/player-view/player-device-container/file-other/file-other.component.html) - Metadata source attribution in card footer

**Best Practice:** Use for displaying data source attribution or metadata origin information. The light grey, italic styling ensures it remains subtle and doesn't compete with main content.

---

## Material Component Customizations

### Cards

#### `.compact-card`

**Purpose**: Creates cards with proper spacing for Material form fields

**Usage Example:**

```html
<mat-card class="compact-card">
  <mat-form-field>
    <input matInput placeholder="Search" />
  </mat-form-field>
</mat-card>
```

**Used In:**

- [`search-toolbar.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.html) - Search input field container

#### `.stretch-card`

**Purpose**: Creates full-height cards with flex layout for components that need to fill available container space with scrollable content

**Usage Example:**

```html
<mat-card class="stretch-card">
  <mat-card-header>
    <mat-card-title>Directory Tree</mat-card-title>
  </mat-card-header>
  <mat-card-content>
    <!-- Scrollable content that fills remaining height -->
    <mat-tree>...</mat-tree>
  </mat-card-content>
</mat-card>
```

**Used In:**

- [`directory-tree.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.html) - Directory tree component
- [`directory-files.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.html) - Directory files component
- [`file-other.component.html`](../libs/features/player/src/lib/player-view/player-device-container/file-other/file-other.component.html) - File info component

**Best Practice:** Use `.stretch-card` for cards that need to fill container height with scrollable content. This ensures consistent behavior and proper flex layout handling.

#### `mat-card-title`

**Purpose**: Adds consistent bottom padding to card titles

**Used In:**

- [`file-other.component.html`](../libs/features/player/src/lib/player-view/player-device-container/file-other/file-other.component.html) - File info card title
- [`file-image.component.html`](../libs/features/player/src/lib/player-view/player-device-container/file-image/file-image.component.html) - Image viewer card title
- [`directory-tree.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.html) - Directory tree card title
- [`directory-files.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.html) - Directory files card title
- [`player-toolbar.component.html`](../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.html) - Player toolbar card title
- [`device-logs.component.html`](../libs/features/devices/src/lib/device-view/device-logs/device-logs.component.html) - Device logs card title
- [`device-item.component.html`](../libs/features/devices/src/lib/device-view/device-item/device-item.component.html) - Device item card title

### Toolbars

#### `.mat-toolbar`

**Purpose**: Customizes Material toolbar height and styling

**Used In:**

- [`header.component.html`](../libs/app/shell/src/lib/components/header/header.component.html) - Main application header

#### `.mat-toolbar.mat-primary`

**Purpose**: Custom background color for primary toolbars

**Used In:**

- [`header.component.html`](../libs/app/shell/src/lib/components/header/header.component.html) - Primary toolbar styling

### Buttons & Icons

#### `.icon-button-small`

**Purpose**: Standardized small-sized icon buttons for compact layouts

**Usage Example:**

```html
<button mat-icon-button class="icon-button-small">
  <mat-icon>edit</mat-icon>
</button>
```

**Used In:**

- Compact toolbars and inline actions
- Secondary actions in dense layouts

#### `.icon-button-medium`

**Purpose**: Standardized medium-sized icon buttons (default size)

**Usage Example:**

```html
<button mat-icon-button class="icon-button-medium">
  <mat-icon>settings</mat-icon>
</button>
```

**Used In:**

- [`device-logs.component.html`](../libs/features/devices/src/lib/device-view/device-logs/device-logs.component.html) - Log control buttons (clear, start/stop logging, download)
- [`storage-item.component.html`](../libs/features/devices/src/lib/device-view/storage-item/storage-item.component.html) - Storage action buttons
- [`device-item.component.html`](../libs/features/devices/src/lib/device-view/device-item/device-item.component.html) - Device power button

#### `.icon-button-large`

**Purpose**: Standardized large-sized icon buttons for prominent actions

**Usage Example:**

```html
<button mat-icon-button class="icon-button-large">
  <mat-icon>play_arrow</mat-icon>
</button>
```

**Used In:**

- Primary action buttons
- Media controls
- Main navigation actions

#### `.icon-button-rounded-primary`

**Purpose**: Rounded icon buttons with primary background color for main actions

**Usage Example:**

```html
<button mat-icon-button class="icon-button-rounded-primary">
  <mat-icon>play_arrow</mat-icon>
</button>
```

**Used In:**

- [`player-toolbar.component.html`](../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.html) - Primary play/pause button

**Best Practice:** Use for primary actions in media controls or other prominent interactive elements that need to stand out with brand color.

#### `.icon-button-rounded-transparent`

**Purpose**: Rounded icon buttons with transparent background for secondary actions

**Usage Example:**

```html
<button mat-icon-button class="icon-button-rounded-transparent">
  <mat-icon>skip_next</mat-icon>
</button>
```

**Used In:**

- [`player-toolbar.component.html`](../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.html) - Secondary media control buttons (previous, fast forward, next)

**Best Practice:** Use for secondary actions that should maintain visual consistency with primary rounded buttons but remain subtle.

#### Icon Color Classes

**Purpose**: Semantic icon colors using custom variables

**Usage Example:**

```html
<mat-icon class="success">check_circle</mat-icon>
<mat-icon class="error">error</mat-icon>
<mat-icon class="highlight">star</mat-icon>
<mat-icon class="dimmed">info</mat-icon>
```

**Used In:**

- [`device-logs.component.html`](../libs/features/devices/src/lib/device-view/device-logs/device-logs.component.html) - Success (play) and error (stop) icons for log controls

**Best Practice:** Use these semantic icon classes instead of hardcoded colors to ensure proper theme support and consistent visual language.

#### Styled Icon Classes

**Purpose**: Size and color styling for the [StyledIconComponent](COMPONENT_LIBRARY.md#stylediconcomponent)

**Size Classes**:

```scss
.styled-icon-small {
  font-size: 16px;
  width: 14px;
  height: 14px;
}

.styled-icon-medium {
  font-size: 24px;
  width: 20px;
  height: 20px;
}

.styled-icon-large {
  font-size: 32px;
  width: 28px;
  height: 28px;
}
```

**Color Classes**:

```scss
.styled-icon-primary {
  color: var(--color-primary-bright);
}

.styled-icon-highlight {
  color: var(--color-highlight);
}

.styled-icon-success {
  color: var(--color-success);
}

.styled-icon-error {
  color: var(--color-error);
}

.styled-icon-dimmed {
  color: var(--color-dimmed);
}

.styled-icon-directory {
  color: var(--color-directory);
}
```

**Usage Example**:

```html
<!-- Via StyledIconComponent (preferred) -->
<lib-styled-icon icon="folder" color="directory" size="medium"> </lib-styled-icon>

<!-- Direct class usage (advanced) -->
<mat-icon class="styled-icon-medium styled-icon-directory">folder</mat-icon>
```

**Used In**:

- [StyledIconComponent](COMPONENT_LIBRARY.md#stylediconcomponent) - Automatically applied based on `size` and `color` props
- Directory tree components for folder/device/storage icons
- File listing components for file type icons

**Best Practice:** Use [StyledIconComponent](COMPONENT_LIBRARY.md#stylediconcomponent) instead of applying these classes directly. The component provides type safety, proper defaults, and automatic class application based on semantic props.

#### Action Button Color Classes

**Purpose**: Bridge design tokens to Material button styling for ActionButtonComponent

**Usage Example:**

```html
<lib-action-button icon="save" label="Save" color="success" (buttonClick)="save()">
</lib-action-button>

<lib-action-button icon="delete" label="Delete" color="error" (buttonClick)="delete()">
</lib-action-button>
```

**Available Classes:**

- **`.action-button-success`**: Maps `--color-success` to Material button text color (green success color)

  - Only changes text color via `--mdc-outlined-button-label-text-color`
  - Preserves Material Design borders and styling
  - Includes hover and focus state overrides

- **`.action-button-error`**: Maps `--color-error` to Material button text color (red error color)

  - Only changes text color via `--mdc-outlined-button-label-text-color`
  - Preserves Material Design borders and styling
  - Includes hover and focus state overrides

- **`.action-button-highlight`**: Maps `--color-highlight` to Material button text color (cyan accent color)
  - Only changes text color via `--mdc-outlined-button-label-text-color`
  - Preserves Material Design borders and styling
  - Includes hover and focus state overrides

**Note**: `primary` and `normal` colors use Material Design's natural styling without any custom CSS classes applied.

**Styling Philosophy**: These classes preserve Material Design's natural button appearance while providing semantic color feedback through text color changes only. This approach maintains proper button borders, spacing, and Material styling consistency.

**Used In:**

- [`device-toolbar.component.html`](../libs/features/devices/src/lib/device-view/device-toolbar/device-toolbar.component.html) - Device management action buttons with semantic colors

**Best Practice:** These classes are automatically applied by [ActionButtonComponent](COMPONENT_LIBRARY.md#actionbuttoncomponent) when using the `color` property. Use semantic colors that match the action's intent (error for destructive actions, success for positive actions, primary for main actions, etc.). The component only applies custom classes for non-primary/normal colors to maintain Material Design consistency.

### Dialogs

#### `.glassy-dialog`

**Purpose**: Creates glass-morphism effect for modal dialogs

**Usage Example:**

```html
<mat-dialog class="glassy-dialog">
  <mat-dialog-content>Dialog content</mat-dialog-content>
</mat-dialog>
```

**Used In:**

- [`layout.component.ts`](../libs/app/shell/src/lib/layout/layout.component.ts) - Modal dialogs via `panelClass: 'glassy-dialog'` configuration

### Overlays

#### `.cdk-overlay-backdrop`

**Purpose**: Custom backdrop color for overlays

**Used In:**

- Dialog backdrops created by Angular Material
- Sidenav overlays in [`layout.component.html`](../libs/app/shell/src/lib/layout/layout.component.html)

---

## Layout & Typography

### `.section`

**Purpose**: Standardized section container with title styling

**Usage Example:**

```html
<div class="section">
  <h3 class="section-title">Settings</h3>
  <!-- section content -->
</div>
```

**Used In:** Currently not used in any component templates (available for future use)

---

## Theme Examples

### `.example-bright-container`

**Purpose**: Demonstration of custom theme application

**Usage Example:**

```html
<div class="example-bright-container">
  <!-- Uses cyan palette theme -->
</div>
```

**Used In:** Currently not used in any component templates (available for theme testing and demonstrations)

---

## Usage Guidelines

### Documentation Maintenance

**IMPORTANT**: This document must be updated whenever new global styles are added to the application.

**When to Update:**

- Adding new utility classes to `styles.scss`
- Creating new Material component customizations
- Introducing new color variables or theme tokens
- Adding new layout patterns or reusable styles

**Update Requirements:**

1. **Document the new style** in the appropriate section with:

   - Purpose and description
   - Complete SCSS code example
   - List of components/features that use it
   - Usage examples where applicable

2. **Update "Used In" sections** for existing styles when they are applied to new components

3. **Add cross-references** between related styles and components

4. **Test examples** to ensure they work correctly in both light and dark themes

**Review Process:**

- All style additions should include documentation updates in the same PR/commit
- Code reviews should verify documentation completeness
- Periodic audits should ensure this guide reflects the current codebase

### Angular Material Best Practices

1. **NO `::ng-deep`** - This is deprecated and breaks component encapsulation

   - Use global styles in `styles.scss` for cross-component styling
   - Create utility classes like `.compact-card` for reusable patterns
   - Override Material components through theme configuration when possible

2. **Follow Material Design Guidelines**

   - Use Material's built-in spacing system (`mat-spacing`)
   - Stick to Material's color palette and semantic naming
   - Respect Material's component hierarchy and structure
   - Use Material's typography scale for consistent text sizing

3. **Component Encapsulation**

   - Keep component-specific styles in component SCSS files
   - Use global styles only for truly shared patterns
   - Prefer composition over style overrides
   - Use Material's appearance variants before custom styling

4. **Theme Integration**
   - Always use CSS custom properties for colors that need theme support
   - Test styling in both light and dark modes
   - Use Material's built-in theming mixins when extending components
   - Leverage Material's density and typography configuration

### Styling Hierarchy

1. **Material Design tokens** (highest priority)
2. **Global utility classes** (`.glassy`, `.compact-card`, etc.)
3. **Component-specific styles** (component SCSS files)
4. **Inline styles** (avoid except for dynamic values)

### Component Integration

- Import global styles are automatically available to all components
- Use theme variables for consistent color schemes
- Apply utility classes for common styling patterns
- Extend base styles rather than overriding when possible

### Maintenance Notes

- All custom color variables support both light and dark modes
- Material component overrides use `!important` sparingly and only when necessary
- Glass effects require backdrop-filter support (modern browsers)
- Icon sizing follows Material Design specifications

---

## Related Files

- **Main Styles**: `libs/ui/styles/src/lib/theme/styles.scss`
- **Theme Configuration**: Angular Material theme setup
- **Component Styles**: Individual component SCSS files extend these base styles
