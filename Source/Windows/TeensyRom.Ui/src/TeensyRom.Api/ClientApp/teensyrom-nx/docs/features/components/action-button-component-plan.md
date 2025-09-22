# ActionButtonComponent Implementation Plan

## Overview

This document outlines the plan to create a reusable `ActionButtonComponent` to replace the repetitive button pattern found in the device-toolbar component. This component will provide a standardized way to create action buttons with icons and text throughout the application.

## Current State Analysis

### Existing Pattern

The device-toolbar component currently uses this repetitive pattern:

```html
<button mat-stroked-button color="primary" (click)="onAction()">
  <lib-icon-label icon="icon_name" [label]="'Action Label'"> </lib-icon-label>
</button>
```

**Issues with Current Approach:**

- Repetitive boilerplate code
- Hard-coded Material color instead of design tokens
- No standardization across different contexts
- Manual aria-label management
- No centralized styling control

### Design Token Integration

The application has established design tokens that should be used:

- `--color-primary`: Purple brand color for primary actions
- `--color-success`: Green variants for success states
- `--color-error`: Red variants for error states
- `--color-highlight`: Cyan accent color
- `--color-dimmed`: Gray for secondary content

## Proposed Solution: ActionButtonComponent

### Component Name Rationale

**Chosen Name**: `ActionButtonComponent`

**Why "ActionButton":**

- **Semantic Clarity**: Clearly indicates this is for actionable user interactions
- **Context Agnostic**: Not tied to specific UI areas like "toolbar" or "form"
- **Distinguishes Purpose**: Different from `IconButtonComponent` (icon-only) and `lib-icon-label` (display-only)
- **Follows Conventions**: Matches existing naming patterns in the codebase

### Component Architecture

#### File Structure

```
libs/ui/components/src/lib/action-button/
├── action-button.component.ts
├── action-button.component.html
├── action-button.component.scss
└── action-button.component.spec.ts
```

#### TypeScript Interface

```typescript
export type ActionButtonVariant = 'stroked' | 'flat' | 'raised' | 'fab';
export type ActionButtonColor = 'primary' | 'success' | 'error' | 'highlight' | 'normal';

@Component({
  selector: 'lib-action-button',
  imports: [CommonModule, MatButtonModule, IconLabelComponent],
  // ...
})
export class ActionButtonComponent {
  // Required inputs
  icon = input.required<string>(); // Material Design icon name
  label = input.required<string>(); // Button text content

  // Optional inputs
  variant = input<ActionButtonVariant>('stroked'); // Button style variant
  color = input<ActionButtonColor>('primary'); // Semantic color
  disabled = input<boolean>(false); // Disabled state
  ariaLabel = input<string>(); // Accessibility label (defaults to label)

  // Events
  buttonClick = output<void>(); // Click event emission

  // Computed properties
  materialColor = computed(() => this.getMaterialColor());
  buttonClasses = computed(() => this.getButtonClasses());
  effectiveAriaLabel = computed(() => this.ariaLabel() || this.label());
}
```

#### Template Design

```html
<button
  [attr.mat-stroked-button]="variant() === 'stroked' ? '' : null"
  [attr.mat-flat-button]="variant() === 'flat' ? '' : null"
  [attr.mat-raised-button]="variant() === 'raised' ? '' : null"
  [attr.mat-fab]="variant() === 'fab' ? '' : null"
  [class]="buttonClasses()"
  [disabled]="disabled()"
  [attr.aria-label]="effectiveAriaLabel()"
  (click)="onButtonClick()"
>
  <lib-icon-label [icon]="icon()" [label]="label()"></lib-icon-label>
</button>
```

### Design Token Integration Strategy

#### CSS Bridge Classes

New CSS classes will be added to `libs/ui/styles/src/lib/theme/styles.scss`:

```scss
// Action Button Color Integration
.action-button-primary {
  --mdc-protected-button-container-color: var(--color-primary);
  --mdc-outlined-button-outline-color: var(--color-primary);
  --mdc-outlined-button-label-text-color: var(--color-primary);
}

.action-button-success {
  --mdc-protected-button-container-color: var(--color-success);
  --mdc-outlined-button-outline-color: var(--color-success);
  --mdc-outlined-button-label-text-color: var(--color-success);
}

.action-button-error {
  --mdc-protected-button-container-color: var(--color-error);
  --mdc-outlined-button-outline-color: var(--color-error);
  --mdc-outlined-button-label-text-color: var(--color-error);
}

.action-button-highlight {
  --mdc-protected-button-container-color: var(--color-highlight);
  --mdc-outlined-button-outline-color: var(--color-highlight);
  --mdc-outlined-button-label-text-color: var(--color-highlight);
}

.action-button-normal {
  // Uses default Material Design styling
}
```

#### Color Mapping Logic

```typescript
private getMaterialColor(): string {
  const colorMap = {
    'primary': 'action-button-primary',
    'success': 'action-button-success',
    'error': 'action-button-error',
    'highlight': 'action-button-highlight',
    'normal': 'action-button-normal'
  };
  return colorMap[this.color()];
}
```

### Component Flexibility Features

#### Variant Support

- **stroked** (default): Outlined button style
- **flat**: Text button without outline
- **raised**: Elevated button with shadow
- **fab**: Floating action button (circular)

#### Color Semantics

- **primary**: Main brand actions (purple)
- **success**: Positive actions like save, confirm (green)
- **error**: Destructive actions like delete, reset (red)
- **highlight**: Special emphasis actions (cyan)
- **normal**: Default Material styling

#### Accessibility Features

- Automatic aria-label generation from label
- Override capability with explicit ariaLabel
- Proper disabled state handling
- Keyboard navigation support
- High contrast mode compatibility

### Implementation Steps

#### Phase 1: Component Creation

1. **Generate component**: `nx g @angular/core:component action-button --project=ui-components`
2. **Implement TypeScript**: Add types, inputs, outputs, computed properties
3. **Create template**: Design flexible button template using Material + IconLabel
4. **Add component styles**: Basic component-specific styling

#### Phase 2: Design Token Integration

1. **Add CSS bridge classes**: Extend `styles.scss` with action button color classes
2. **Test color mapping**: Ensure all semantic colors work correctly
3. **Verify Material integration**: Confirm proper Material button behavior

#### Phase 3: Testing & Quality

1. **Unit tests**: Comprehensive test suite covering all variants and colors
2. **Accessibility testing**: Screen reader compatibility, keyboard navigation
3. **Visual testing**: Ensure consistent appearance across themes
4. **Integration testing**: Test within device-toolbar context

#### Phase 4: Migration & Documentation

1. **Update device-toolbar**: Replace existing pattern with ActionButtonComponent
2. **Update barrel exports**: Add to `ui/components` public API
3. **Update COMPONENT_LIBRARY.md**: Complete documentation with examples
4. **Update STYLE_GUIDE.md**: Document new action button classes

### Usage Examples

#### Basic Usage

```html
<lib-action-button icon="refresh" label="Refresh Data" (buttonClick)="refresh()">
</lib-action-button>
```

#### Success Action

```html
<lib-action-button icon="save" label="Save Changes" color="success" (buttonClick)="save()">
</lib-action-button>
```

#### Destructive Action

```html
<lib-action-button
  icon="delete"
  label="Delete All"
  color="error"
  variant="raised"
  (buttonClick)="deleteAll()"
>
</lib-action-button>
```

#### Disabled State

```html
<lib-action-button
  icon="upload"
  label="Upload Files"
  [disabled]="!hasFiles"
  ariaLabel="Upload selected files to server"
  (buttonClick)="upload()"
>
</lib-action-button>
```

### Migration Impact

#### Device Toolbar Transformation

**Before:**

```html
<button mat-stroked-button color="primary" (click)="onIndexAllStorage()">
  <lib-icon-label icon="download" [label]="'Index All'"> </lib-icon-label>
</button>
<button mat-stroked-button color="primary" (click)="onRefreshDevices()">
  <lib-icon-label icon="refresh" [label]="'Refresh Devices'"> </lib-icon-label>
</button>
```

**After:**

```html
<lib-action-button icon="download" label="Index All" (buttonClick)="onIndexAllStorage()">
</lib-action-button>
<lib-action-button icon="refresh" label="Refresh Devices" (buttonClick)="onRefreshDevices()">
</lib-action-button>
```

#### Benefits Achieved

- **50% less boilerplate** code per button
- **Design token compliance** instead of generic Material colors
- **Consistent accessibility** handling
- **Centralized styling** control
- **Type safety** for variants and colors

### Testing Strategy

#### Unit Tests Coverage

```typescript
describe('ActionButtonComponent', () => {
  // Basic functionality
  it('should create and display icon and label');
  it('should emit buttonClick when clicked');
  it('should handle disabled state correctly');

  // Variant testing
  it('should apply correct Material button directive for each variant');

  // Color testing
  it('should apply correct color classes for each semantic color');

  // Accessibility
  it('should use label as aria-label when ariaLabel not provided');
  it('should use explicit ariaLabel when provided');
  it('should have proper keyboard navigation');

  // Integration
  it('should compose with IconLabelComponent correctly');
});
```

#### Integration Tests

- Test within device-toolbar component
- Verify backward compatibility
- Test responsive behavior
- Cross-browser compatibility

### Documentation Requirements

#### COMPONENT_LIBRARY.md Updates

- Complete ActionButtonComponent documentation
- Usage examples for all variants and colors
- Migration guide from old pattern
- Integration examples with forms and dialogs

#### STYLE_GUIDE.md Updates

- Document new action button color classes
- Usage guidelines for semantic colors
- Best practices for action button placement
- Accessibility recommendations

### Future Considerations

#### Extensibility

- Support for additional variants (outlined, text, etc.)
- Custom color support beyond semantic tokens
- Size variants (small, medium, large)
- Loading state indicator

#### Related Components

- Consider similar pattern for other button types
- Potential FormButtonComponent for form contexts
- MenuButtonComponent for dropdown triggers

### Risk Assessment

#### Low Risk

- Uses existing, well-tested components (IconLabel, Material buttons)
- Maintains existing functionality and appearance
- Backward compatible approach

#### Mitigation Strategies

- Comprehensive testing before migration
- Gradual rollout starting with device-toolbar
- Fallback to original pattern if issues arise
- Clear documentation for troubleshooting

## Conclusion

The ActionButtonComponent provides a robust, flexible, and maintainable solution for standardizing action buttons throughout the TeensyROM application. By leveraging established design tokens and existing components, it enhances consistency while reducing boilerplate code and improving accessibility.

The implementation follows Angular best practices, integrates seamlessly with the existing design system, and provides a clear migration path for current usage patterns.
