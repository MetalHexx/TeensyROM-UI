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
- `metadataSource` (optional): `string` - Source attribution text displayed in the card footer. Uses [`.metadata-source`](STYLE_GUIDE.md#metadata-source) styling for subtle, right-aligned appearance. Only rendered when provided.

**Usage Example**:

```html
<!-- Card with title, subtitle, corner content, and metadata -->
<lib-card-layout
  title="File Information"
  subtitle="Release v1.2.3"
  metadataSource="Gamebase Database"
>
  <!-- Corner content using slot -->
  <mat-chip-set slot="corner">
    <mat-chip>C64</mat-chip>
    <mat-chip>PRG</mat-chip>
  </mat-chip-set>

  <!-- Main content -->
  <p>File details and description...</p>
</lib-card-layout>

<!-- Card with title and metadata only -->
<lib-card-layout title="Artist Image" metadataSource="MusicBrainz">
  <img src="/placeholder.jpg" alt="Artist Image" />
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

**Footer Features**:

- Metadata attribution: Uses `metadataSource` property to display source attribution in card footer
- Automatic styling: Applies [`.metadata-source`](STYLE_GUIDE.md#metadata-source) class for consistent right-aligned, subtle appearance
- Conditional rendering: Footer only appears when `metadataSource` is provided

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

### `CompactCardLayoutComponent`

**Purpose**: A lightweight card wrapper component designed specifically for form fields and compact content areas, using the `.compact-card` styling.

**Selector**: `lib-compact-card-layout`

**Properties**: None - Pure content projection component

**Usage Example**:

```html
<!-- Compact card for form fields -->
<lib-compact-card-layout>
  <mat-form-field appearance="outline">
    <mat-label>Search</mat-label>
    <input matInput placeholder="Search files and folders..." />
    <mat-icon matSuffix>search</mat-icon>
  </mat-form-field>
</lib-compact-card-layout>

<!-- Compact card for button groups -->
<lib-compact-card-layout>
  <div class="button-group">
    <button mat-icon-button><mat-icon>play_arrow</mat-icon></button>
    <button mat-icon-button><mat-icon>pause</mat-icon></button>
    <button mat-icon-button><mat-icon>stop</mat-icon></button>
  </div>
</lib-compact-card-layout>

<!-- Compact card for simple content -->
<lib-compact-card-layout>
  <p>Simple content without header</p>
</lib-compact-card-layout>
```

**Content Projection**: Uses `<ng-content></ng-content>` to project any content directly into the card without headers or structure

**Styling**: Automatically applies the global [`.compact-card`](STYLE_GUIDE.md#compact-card) class optimized for Material form fields and compact layouts

**Key Differences from CardLayoutComponent**:

- **No headers/titles**: Compact design without mat-card-header
- **No corner content**: Simplified structure for forms and controls
- **Form-optimized spacing**: Uses `.compact-card` instead of `.stretch-card`
- **Minimal overhead**: Pure content projection without additional features

**Best Practice**: Use for form controls, toolbars, search bars, and other compact UI elements where you need card styling without the overhead of headers or complex layouts.

**Used In**:

- Search toolbars and form containers
- Control panels and button groups
- Compact content areas that need card styling

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

**Purpose**: A reusable icon button component that provides consistent Material Design button styling with configurable appearance, state management, and accessibility features.

**Selector**: `lib-icon-button`

**Properties**:

- `icon` (required): `string` - Material Design icon name to display in the button
- `ariaLabel` (required): `string` - Accessibility label for screen readers (required for proper accessibility)
- `color` (optional): `'normal' | 'highlight' | 'success' | 'error' | 'dimmed'` - Semantic color variant that maps to [STYLE_GUIDE.md](STYLE_GUIDE.md) color system - defaults to 'normal'
- `size` (optional): `'small' | 'medium' | 'large'` - Size variant that maps to existing style classes - defaults to 'medium'
- `variant` (optional): `'standard' | 'rounded-primary' | 'rounded-transparent'` - Style variant from [STYLE_GUIDE.md](STYLE_GUIDE.md) - defaults to 'standard'
- `disabled` (optional): `boolean` - Whether the button is disabled - defaults to false

**Events**:

- `buttonClick`: Emitted when the button is clicked (only when not disabled)

**Usage Examples**:

```html
<!-- Power button (from device-item component) -->
<lib-icon-button
  icon="power_settings_new"
  ariaLabel="Power"
  [color]="connectionStatus() ? 'highlight' : 'normal'"
  size="medium"
  (buttonClick)="connectionStatus() ? onDisconnect() : onConnect()"
>
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

The component automatically maps to design token colors from [STYLE_GUIDE.md](STYLE_GUIDE.md):

- **Color Mapping**:

  - `primary` → `.action-button-primary` (purple brand color from `--color-primary`)
  - `success` → `.action-button-success` (green color from `--color-success`)
  - `error` → `.action-button-error` (red color from `--color-error`)
  - `highlight` → `.action-button-highlight` (cyan color from `--color-highlight`)
  - `normal` → `.action-button-normal` (default Material styling)

- **Variant Mapping**:
  - `stroked` → `mat-stroked-button` (outlined button)
  - `flat` → `mat-flat-button` (text button)
  - `raised` → `mat-raised-button` (elevated button)
  - `fab` → `mat-fab` (floating action button)

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
import { CompactCardLayoutComponent } from '@teensyrom-nx/ui/components';
import { InputFieldComponent } from '@teensyrom-nx/ui/components';
import { IconButtonComponent } from '@teensyrom-nx/ui/components';
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
