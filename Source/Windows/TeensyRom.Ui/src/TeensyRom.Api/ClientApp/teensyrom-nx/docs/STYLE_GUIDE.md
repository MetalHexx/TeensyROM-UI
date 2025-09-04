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

- `--color-success`: Green variants for success states
- `--color-error`: Red variants for error states
- `--color-highlight`: Cyan accent color
- `--color-dimmed`: Gray for secondary content

**Usage Example:**

```scss
.my-icon {
  color: var(--color-success);
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

**Used In:** Disabled device states, secondary UI elements

### `.glassy`

**Purpose**: Creates a glass-morphism effect with backdrop blur

**Usage Example:**

```html
<div class="glassy">Overlay content</div>
<mat-dialog class="glassy-dialog">...</mat-dialog>
```

**Used In:** Navigation sidenav, modal dialogs

**Best Practice:** Use `.glassy` for overlay effects and modern UI elements to maintain consistent visual hierarchy.

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

**Used In:** Search toolbar components, form-based card layouts

**Best Practice:** Apply `.compact-card` for any card containing Material form fields to ensure proper spacing and avoid layout issues.

#### `.stretch-card`

**Purpose**: Creates full-height cards with scrollable content areas

**Usage Example:**

```html
<mat-card class="stretch-card">
  <mat-card-header>
    <mat-card-title>Your Title</mat-card-title>
  </mat-card-header>
  <mat-card-content>
    <!-- Your scrollable content here -->
  </mat-card-content>
</mat-card>
```

**Features:**

- Header stays fixed at top
- Content area fills remaining space
- Automatic scrollbars when content overflows
- Properly handles flex layout constraints

**Used In:** Directory tree component, directory files component

**Best Practice:** Use `.stretch-card` for cards that need to fill container height with scrollable content. This ensures consistent behavior and proper flex layout handling.

**Used In:**

- `SearchToolbarComponent` - for search input field
- Any card containing Material form fields

#### `mat-card-title`

**Purpose**: Adds consistent bottom padding to card titles

**Used In:** All cards with titles throughout the application

### Toolbars

#### `.mat-toolbar`

**Purpose**: Customizes Material toolbar height and styling

**Used In:** Main application header, any component using `mat-toolbar`

#### `.mat-toolbar.mat-primary`

**Purpose**: Custom background color for primary toolbars

### Buttons & Icons

#### `.icon-button-medium`

**Purpose**: Standardized medium-sized icon buttons

**Usage Example:**

```html
<button mat-icon-button class="icon-button-medium">
  <mat-icon>settings</mat-icon>
</button>
```

**Used In:** Navigation controls, action buttons in toolbars

#### Icon Color Classes

**Purpose**: Semantic icon colors using custom variables

**Usage Example:**

```html
<mat-icon class="success">check_circle</mat-icon>
<mat-icon class="error">error</mat-icon>
<mat-icon class="highlight">star</mat-icon>
<mat-icon class="dimmed">info</mat-icon>
```

**Used In:** Device status indicators, connection state icons, action feedback icons

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

**Used In:** Modal dialogs, settings panels, confirmation dialogs

### Overlays

#### `.cdk-overlay-backdrop`

**Purpose**: Custom backdrop color for overlays

**Used In:** Dialog backdrops, sidenav overlays

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

**Used In:** Content sections in feature components, grouped UI elements

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

**Used In:** Theme testing and demonstrations, special highlighted containers

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
