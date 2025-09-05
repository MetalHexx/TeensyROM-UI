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

### `.glassy`

**Purpose**: Creates a glass-morphism effect with backdrop blur

**Usage Example:**

```html
<div class="glassy">Overlay content</div>
<mat-dialog class="glassy-dialog">...</mat-dialog>
```

**Used In:**

- [`layout.component.html`](../libs/app/shell/src/lib/layout/layout.component.html) - Navigation sidenav
- [`layout.component.ts`](../libs/app/shell/src/lib/layout/layout.component.ts) - Modal dialogs (via `panelClass: 'glassy-dialog'`)

**Best Practice:** Use `.glassy` for overlay effects and modern UI elements to maintain consistent visual hierarchy.

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

**Used In:**

- [`directory-tree.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.html) - Directory tree component
- [`directory-files.component.html`](../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.html) - Directory files component
- [`file-other.component.html`](../libs/features/player/src/lib/player-view/player-device-container/file-other/file-other.component.html) - File info component

**Best Practice:** Use `.stretch-card` for cards that need to fill container height with scrollable content. This ensures consistent behavior and proper flex layout handling.

**Used In:**

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

#### `.icon-button-medium`

**Purpose**: Standardized medium-sized icon buttons

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
