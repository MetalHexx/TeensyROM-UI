# TeensyROM UI Styles Guide

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

```scss
--color-success: #86c691 (light) / #6fdc8c (dark)
--color-error: #cc666c (light) / #ff6f6f (dark)
--color-highlight: #00f7ff
--color-dimmed: #656363
```

**Usage Examples:**

- Icon buttons with semantic colors
- Status indicators throughout the application

---

## Utility Classes

### `.dimmed`

**Purpose**: Reduces opacity to 50% for disabled or secondary content

```scss
.dimmed {
  opacity: 0.5;
}
```

**Used In:**

- Disabled device states
- Secondary UI elements

### `.glassy`

**Purpose**: Creates a glass-morphism effect with backdrop blur

```scss
.glassy {
  background-color: rgba(255, 255, 255, 0.1) !important;
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}
```

**Used In:**

- Navigation sidenav (`LayoutComponent`)
- Modal dialogs (via `.glassy-dialog`)

---

## Material Component Customizations

### Cards

#### `.compact-card`

**Purpose**: Creates cards with proper spacing for Material form fields

```scss
.compact-card {
  padding: 16px !important;
  margin: 0 !important;
  overflow: hidden;

  mat-form-field {
    width: 100%;
    margin-bottom: -20px;
  }
}
```

**Used In:**

- `SearchToolbarComponent` - for search input field
- Any card containing Material form fields

#### `mat-card-title`

**Purpose**: Adds consistent bottom padding to card titles

```scss
mat-card-title {
  padding-bottom: 0.8rem;
}
```

**Used In:**

- All cards with titles throughout the application

### Toolbars

#### `.mat-toolbar`

**Purpose**: Customizes Material toolbar height and styling

```scss
.mat-toolbar {
  height: 50px !important;
  min-height: 50px !important;
  box-shadow: var(--mat-sys-level3);
}
```

**Used In:**

- Main application header
- Any component using `mat-toolbar`

#### `.mat-toolbar.mat-primary`

**Purpose**: Custom background color for primary toolbars

```scss
.mat-toolbar.mat-primary {
  --mat-toolbar-container-background-color: #890089;
}
```

### Buttons & Icons

#### `.icon-button-medium`

**Purpose**: Standardized medium-sized icon buttons

```scss
.icon-button-medium {
  width: 32px !important;
  height: 32px !important;
  border-radius: 50%;
  /* ... additional styling ... */
}
```

**Used In:**

- Navigation controls
- Action buttons in toolbars

#### Icon Color Classes

**Purpose**: Semantic icon colors using custom variables

```scss
.mat-icon {
  &.highlight {
    color: var(--color-highlight);
  }
  &.dimmed {
    color: var(--color-dimmed);
  }
  &.success {
    color: var(--color-success);
  }
  &.error {
    color: var(--color-error);
  }
}
```

**Used In:**

- Device status indicators
- Connection state icons
- Action feedback icons

### Dialogs

#### `.glassy-dialog`

**Purpose**: Creates glass-morphism effect for modal dialogs

```scss
.glassy-dialog {
  .mat-mdc-dialog-container {
    @extend .glassy;
    border-radius: var(--mat-sys-corner-large);
    /* ... additional styling ... */
  }
}
```

**Used In:**

- Modal dialogs throughout the application
- Settings panels
- Confirmation dialogs

### Overlays

#### `.cdk-overlay-backdrop`

**Purpose**: Custom backdrop color for overlays

```scss
.cdk-overlay-backdrop {
  background-color: var(--dialog-backdrop-color) !important;
}
```

**Used In:**

- Dialog backdrops
- Sidenav overlays

---

## Layout & Typography

### `.section`

**Purpose**: Standardized section container with title styling

```scss
.section {
  width: 100%;

  .section-title {
    font-size: 1.2rem;
    font-weight: 400;
    margin-bottom: 0.5rem;
    font-family: Roboto;
  }
}
```

**Used In:**

- Content sections in feature components
- Grouped UI elements

---

## Theme Examples

### `.example-bright-container`

**Purpose**: Demonstration of custom theme application

```scss
.example-bright-container {
  @include mat.theme(
    (
      color: mat.$cyan-palette,
    )
  );
}
```

**Used In:**

- Theme testing and demonstrations
- Special highlighted containers

---

## Usage Guidelines

### Best Practices

1. **Use semantic color classes** (`.success`, `.error`, `.highlight`, `.dimmed`) instead of hardcoded colors
2. **Apply `.compact-card`** for any card containing Material form fields
3. **Use `.glassy`** for overlay effects and modern UI elements
4. **Prefer CSS custom properties** for consistent theming

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
