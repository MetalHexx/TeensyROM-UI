# TeensyROM UI Coding Standards

## Overview

This document establishes coding standards and best practices for the TeensyROM Angular application. These standards ensure consistency, maintainability, and readability across the codebase.

---

## Components

### Input Properties

**Standard**: Use Angular's modern `input()` function with explicit typing and default values

**Format**:

```typescript
propertyName = input<Type>(defaultValue);
```

**Usage Example**:

```typescript
import { Component, input } from '@angular/core';

@Component({
  selector: 'lib-example',
  templateUrl: './example.component.html',
})
export class ExampleComponent {
  icon = input<string>('');
  label = input<string>('');
  status = input<boolean | undefined>(undefined);
  count = input<number>(0);
  items = input<string[]>([]);
}
```

**Requirements**:

- Always provide explicit TypeScript typing
- Always provide a default value appropriate for the type
- Use descriptive property names in camelCase
- Place input properties at the top of the component class

**Used In**:

- [`storage-item.component.ts`](../libs/features/devices/src/lib/device-view/storage-item/storage-item.component.ts) - Icon, label, and status inputs
- All modern component implementations

**Best Practice**: Use `input()` instead of the legacy `@Input()` decorator for better type safety and performance.

### Output Properties

**Standard**: Use Angular's modern `output()` function with descriptive event names

**Format**:

```typescript
eventName = output<Type>();
```

**Usage Example**:

```typescript
import { Component, output } from '@angular/core';

@Component({
  selector: 'lib-example',
  templateUrl: './example.component.html',
})
export class ExampleComponent {
  itemClick = output<string>();
  statusChange = output<boolean>();
  indexSelected = output<void>();

  onItemClick(item: string) {
    this.itemClick.emit(item);
  }
}
```

**Requirements**:

- Use descriptive names that clearly indicate the event purpose
- Provide explicit typing for the emitted value
- Use `void` type for events that don't emit data
- Place output properties after input properties in the component class

**Best Practice**: Use `output()` instead of the legacy `@Output()` decorator for consistency with modern Angular patterns.

### Component Structure

**Standard**: Follow a consistent ordering of component class members

**Order**:

1. Input properties (`input()`)
2. Output properties (`output()`)
3. Public properties
4. Private properties
5. Constructor
6. Lifecycle hooks (in Angular's execution order)
7. Public methods
8. Private methods

**Usage Example**:

```typescript
@Component({
  selector: 'lib-example',
  templateUrl: './example.component.html',
  styleUrl: './example.component.scss',
})
export class ExampleComponent implements OnInit, OnDestroy {
  // 1. Input properties
  title = input<string>('');
  data = input<any[]>([]);

  // 2. Output properties
  itemSelected = output<string>();

  // 3. Public properties
  isLoading = false;
  items: string[] = [];

  // 4. Private properties
  private subscription = new Subscription();

  // 5. Constructor
  constructor(private service: DataService) {}

  // 6. Lifecycle hooks
  ngOnInit() {
    this.loadData();
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  // 7. Public methods
  onItemClick(item: string) {
    this.itemSelected.emit(item);
  }

  // 8. Private methods
  private loadData() {
    // Implementation
  }
}
```

---

## TypeScript Standards

### Type Definitions

**Standard**: Use explicit typing for all properties, parameters, and return values

**Best Practices**:

- Avoid `any` type unless absolutely necessary
- Use union types for specific value sets: `type Status = 'active' | 'inactive' | 'pending'`
- Define interfaces for complex objects
- Use generic types for reusable components

### Imports

**Standard**: Organize imports in the following order with blank lines between groups

**Order**:

1. Angular core imports
2. Angular feature module imports
3. Third-party library imports
4. Application imports (absolute paths)
5. Relative imports

**Usage Example**:

```typescript
import { Component, input, output } from '@angular/core';

import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

import { Observable } from 'rxjs';

import { IconLabelComponent } from '@teensyrom-nx/ui/components';

import { DataService } from './data.service';
```

---

## Naming Conventions

### Components

**Standard**: Use PascalCase for component class names with descriptive suffixes

**Format**: `[Feature][Purpose]Component`

**Examples**:

- `StorageItemComponent`
- `DeviceLogsComponent`
- `PlayerToolbarComponent`
- `SearchToolbarComponent`

### Files

**Standard**: Use kebab-case for all file names

**Format**: `[feature-name].[file-type].[extension]`

**Examples**:

- `storage-item.component.ts`
- `device-logs.component.html`
- `player-toolbar.component.scss`

### Properties and Methods

**Standard**: Use camelCase for all properties and methods

**Examples**:

- `isConnected`
- `deviceStatus`
- `onItemClick()`
- `handleStatusChange()`

---

## Documentation Standards

### Component Documentation

**Standard**: Document all public components with JSDoc comments

**Usage Example**:

````typescript
/**
 * Displays storage device information with status indicator and action button
 *
 * @example
 * ```html
 * <lib-storage-status
 *   icon="storage"
 *   label="SD Card"
 *   [status]="true"
 *   (index)="onStorageIndex()">
 * </lib-storage-status>
 * ```
 */
@Component({
  selector: 'lib-storage-status',
  templateUrl: './storage-item.component.html',
})
export class StorageStatusComponent {
  /** Icon name to display for the storage device */
  icon = input<string>('');

  /** Display label for the storage device */
  label = input<string>('');

  /** Current connection status of the storage device */
  status = input<boolean | undefined>(undefined);

  /** Emitted when the index action is triggered */
  index = output<void>();
}
````

### Code Maintenance

**Standard**: Update this document when introducing new patterns or standards

**Update Requirements**:

1. Document new coding patterns as they are established
2. Provide examples for all standards
3. Include "Used In" references to actual implementations
4. Update when Angular or framework patterns change
5. Review and validate examples during code reviews

---

## Usage Guidelines

### Code Reviews

**Requirements**:

- Verify adherence to input/output property standards
- Check component structure ordering
- Validate TypeScript typing completeness
- Ensure proper import organization
- Confirm documentation completeness for public APIs

### New Component Checklist

When creating new components, ensure:

- [ ] Component follows naming conventions
- [ ] Input properties use `input()` with typing and defaults
- [ ] Output properties use `output()` with descriptive names
- [ ] Class members are ordered according to standards
- [ ] Imports are organized correctly
- [ ] Public APIs are documented with JSDoc
- [ ] Component is added to appropriate "Used In" sections

---

## Related Files

- **Style Guide**: [`STYLE_GUIDE.md`](./STYLE_GUIDE.md) - UI styling standards and patterns
- **Component Examples**: Individual component files demonstrate these standards in practice
- **Angular Documentation**: [Angular Coding Style Guide](https://angular.io/guide/styleguide)
