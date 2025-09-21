# TeensyROM UI Component Library

## Overview

This document catalogs all shared UI components available in the TeensyROM application. All components are located in `libs/ui/components/src/lib/` and follow Angular 19 standalone component architecture with modern signal-based inputs.

---

## Layout Components

### `CardLayoutComponent`

**Purpose**: A reusable card wrapper component that provides consistent card styling with optional header and title support.

**Selector**: `lib-card-layout`

**Properties**:

- `title` (optional): `string` - The title to display in the card header. If not provided, no header will be rendered.
- `subtitle` (optional): `string` - The subtitle to display below the title in the card header. Only rendered when both title and subtitle are provided.

**Usage Example**:

```html
<!-- Card with title, subtitle, and corner content -->
<lib-card-layout title="File Information" subtitle="Release v1.2.3">
  <!-- Corner content using slot -->
  <mat-chip-set slot="corner">
    <mat-chip>C64</mat-chip>
    <mat-chip>PRG</mat-chip>
  </mat-chip-set>

  <!-- Main content -->
  <p>File details and description...</p>
</lib-card-layout>

<!-- Card with title only -->
<lib-card-layout title="Device Information">
  <p>Card content goes here</p>
</lib-card-layout>

<!-- Card without title -->
<lib-card-layout>
  <mat-form-field>
    <input matInput placeholder="Search" />
  </mat-form-field>
</lib-card-layout>

<!-- Card with corner buttons -->
<lib-card-layout title="Storage Details">
  <!-- Any content can go in the corner -->
  <div slot="corner">
    <button mat-icon-button>
      <mat-icon>more_vert</mat-icon>
    </button>
  </div>

  <p>Storage information...</p>
</lib-card-layout>
```

**Content Projection**:

- Main content: Uses `<ng-content></ng-content>` to project any content into the card body
- Corner content: Uses `<ng-content select="[slot=corner]"></ng-content>` to project content into the upper-right corner

**Corner Content Features**:

- Positioned absolutely in the upper-right corner (16px from top and right edges)
- Supports any HTML content: chips, buttons, icons, custom components
- Automatically positioned above other card content with `z-index: 1`
- Use `slot="corner"` attribute to designate content for corner placement

**Styling**: Automatically applies the global [`.stretch-card`](STYLE_GUIDE.md#stretch-card) class for full-height cards with proper flex layout and scrollable content.

**Used In**:

- Directory tree components for consistent card wrapping
- Search toolbars for form field containers
- Any component requiring standardized card layout

---

## Display Components

### `IconLabelComponent`

**Purpose**: A simple component that displays an icon alongside text in a consistent layout pattern.

**Selector**: `lib-icon-label`

**Properties**:

- `icon`: `string` - Material Design icon name to display (defaults to empty string)
- `label`: `string` - Text label to display next to the icon (defaults to empty string)

**Usage Example**:

```html
<lib-icon-label icon="folder" label="Documents"> </lib-icon-label>

<lib-icon-label icon="device_hub" label="Connected Device"> </lib-icon-label>
```

**Features**:

- Icon and text are horizontally aligned
- Text includes a `title` attribute for accessibility/tooltips
- Consistent spacing between icon and label

**Best Practice**: Use for any icon-text combination that needs consistent styling and alignment across the application.

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
import { IconLabelComponent } from '@teensyrom-nx/ui/components';
import { StatusIconLabelComponent } from '@teensyrom-nx/ui/components';
import { MenuItemComponent } from '@teensyrom-nx/ui/components';

@Component({
  // ...
  imports: [
    CardLayoutComponent,
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
