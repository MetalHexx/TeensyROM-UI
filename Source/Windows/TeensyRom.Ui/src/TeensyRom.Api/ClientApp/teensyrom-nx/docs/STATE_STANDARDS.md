# TeensyROM UI State Management Standards

## Overview

This document establishes standards for state management using NgRx Signal Store in the TeensyROM application. These standards ensure consistency, maintainability, and scalability across all state management implementations.

---

## Signal Store Architecture

### Store Structure

**Standard**: Use NgRx Signal Store with modular method organization

**Format**:

```typescript
export const StoreService = signalStore(
  { providedIn: 'root' },
  withDevtools('storeName'),
  withState(initialState),
  withMethods((store, ...services) => ({
    ...methodGroup1(store, service1),
    ...methodGroup2(store, service2),
    // Additional method groups
  }))
);
```

**Usage Example**:

```typescript
import { inject } from '@angular/core';
import { signalStore, withMethods, withState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { Device, DeviceService, StorageService } from '@teensyrom-nx/domain/device/services';
import { findDevices, connectDevice, disconnectDevice } from './methods/index';
import { indexStorage } from './methods/index-storage';

export type DeviceState = {
  devices: Device[];
  hasInitialised: boolean;
  isLoading: boolean;
  error: string | null;
};

const initialState: DeviceState = {
  devices: [],
  hasInitialised: false,
  isLoading: true,
  error: null,
};

export const DeviceStore = signalStore(
  { providedIn: 'root' },
  withDevtools('devices'),
  withState(initialState),
  withMethods(
    (
      store,
      deviceService: DeviceService = inject(DeviceService),
      storageService: StorageService = inject(StorageService)
    ) => ({
      ...findDevices(store, deviceService),
      ...connectDevice(store, deviceService),
      ...disconnectDevice(store, deviceService),
      ...indexStorage(store, storageService),
    })
  )
);
```

**Requirements**:

- Use `providedIn: 'root'` for application-level stores
- Always include `withDevtools()` with descriptive store name
- Define explicit TypeScript interfaces for state
- Inject services with default parameters using `inject()`
- Group related methods using spread operator

**Used In**:

- [`device-store.ts`](../libs/domain/device/state/src/lib/device-store.ts) - Main device state management

---

## Method Organization

### Separate Method Files

**Standard**: Extract each logical group of actions into separate files within a `methods/` directory

**Structure**:

```
state/
├── store-name.ts
└── methods/
    ├── index.ts
    ├── action-group-1.ts
    ├── action-group-2.ts
    └── specific-action.ts
```

**File Naming Convention**:

- Use kebab-case for method files
- Name files based on the primary action or action group
- Use descriptive names that indicate the operation

**Usage Example**:

```typescript
// methods/connect-device.ts
import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { DeviceService } from '@teensyrom-nx/domain/device/services';
import { firstValueFrom } from 'rxjs';
import { DeviceState } from '../device-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function connectDevice(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  deviceService: DeviceService = inject(DeviceService)
) {
  return {
    connectDevice: async (deviceId: string) => {
      patchState(store, { error: null });

      try {
        await firstValueFrom(deviceService.connectDevice(deviceId));

        patchState(store, {
          isLoading: false,
          devices: store
            .devices()
            .map((d) => (d.deviceId === deviceId ? { ...d, isConnected: true } : d)),
        });
      } catch (error) {
        patchState(store, { error: String(error) });
      }
    },
  };
}
```

**Used In**:

- [`connect-device.ts`](../libs/domain/device/state/src/lib/methods/connect-device.ts) - Device connection logic
- [`disconnect-device.ts`](../libs/domain/device/state/src/lib/methods/disconnect-device.ts) - Device disconnection logic
- [`find-devices.ts`](../libs/domain/device/state/src/lib/methods/find-devices.ts) - Device discovery logic
- [`index-storage.ts`](../libs/domain/device/state/src/lib/methods/index-storage.ts) - Storage indexing operations

### Method File Structure

**Standard**: Follow consistent structure for method files

**Requirements**:

1. Import necessary dependencies at the top
2. Define SignalStore type helper (if needed)
3. Export single function that returns method object
4. Use descriptive parameter names
5. Include proper error handling
6. Clear state updates with `patchState`

**Template**:

```typescript
import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { ServiceType } from '@teensyrom-nx/domain/service-path';
import { firstValueFrom } from 'rxjs';
import { StateType } from '../store-name';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function methodGroupName(
  store: SignalStore<StateType> & WritableStateSource<StateType>,
  service: ServiceType = inject(ServiceType)
) {
  return {
    methodName: async (param: ParamType) => {
      // Clear any previous errors
      patchState(store, { error: null });

      try {
        // Perform async operation
        const result = await firstValueFrom(service.operation(param));

        // Update state on success
        patchState(store, {
          // state updates
        });
      } catch (error) {
        // Handle errors
        patchState(store, { error: String(error) });
      }
    },
  };
}
```

### Index File Management

**Standard**: Use index files to organize and export method groups

**Format**:

```typescript
// methods/index.ts
export * from './method-group-1';
export * from './method-group-2';
export * from './specific-action';
```

**Usage Example**:

```typescript
// methods/index.ts
export * from './find-devices';
export * from './connect-device';
export * from './disconnect-device';
```

**Requirements**:

- Group related exports together
- Use barrel exports for cleaner imports
- Keep individual action files separate when they have different service dependencies
- Update index when adding new method files

**Used In**:

- [`methods/index.ts`](../libs/domain/device/state/src/lib/methods/index.ts) - Core device operations export

---

## State Type Definitions

### State Interface

**Standard**: Define explicit TypeScript interfaces for all state structures

**Format**:

```typescript
export type StoreName = {
  // Core data
  items: ItemType[];

  // UI state
  isLoading: boolean;
  hasInitialised: boolean;

  // Error handling
  error: string | null;

  // Feature-specific state
  featureSpecificProperty: boolean;
};
```

**Usage Example**:

```typescript
export type DeviceState = {
  devices: Device[];
  hasInitialised: boolean;
  isLoading: boolean;
  isIndexing: boolean;
  error: string | null;
};
```

**Requirements**:

- Use descriptive property names
- Group related properties logically
- Always include error handling properties
- Include loading states for async operations
- Use union types for specific value sets

### Initial State

**Standard**: Define complete initial state with all properties

**Format**:

```typescript
const initialState: StateType = {
  // Set appropriate defaults for each property
  items: [],
  isLoading: false,
  hasInitialised: false,
  error: null,
  featureSpecificProperty: false,
};
```

**Requirements**:

- Provide sensible defaults for all properties
- Use empty arrays for collections
- Set loading states appropriately
- Initialize error as `null`
- Document any non-obvious initial values

---

## Error Handling

### Error State Management

**Standard**: Implement consistent error handling across all methods

**Pattern**:

```typescript
export function actionMethod(store, service) {
  return {
    actionName: async (params) => {
      // 1. Clear previous errors
      patchState(store, { error: null });

      try {
        // 2. Perform operation
        const result = await firstValueFrom(service.operation(params));

        // 3. Update state on success
        patchState(store, {
          // success state updates
          error: null,
        });
      } catch (error) {
        // 4. Handle errors consistently
        patchState(store, {
          error: String(error),
          // Reset any loading states
          isLoading: false,
        });
      }
    },
  };
}
```

**Requirements**:

- Always clear errors at the start of operations
- Use `String(error)` for consistent error formatting
- Reset loading states in error handling
- Maintain existing state when possible during errors

---

## Service Integration

### Service Injection

**Standard**: Use Angular's `inject()` function with default parameters

**Format**:

```typescript
export function methodGroup(
  store: SignalStore<StateType> & WritableStateSource<StateType>,
  service: ServiceType = inject(ServiceType)
) {
  // method implementations
}
```

**Requirements**:

- Always provide default injection using `inject()`
- Use explicit typing for service parameters
- Import services from appropriate domain paths
- Enable dependency injection testing through parameter overrides

### Async Operations

**Standard**: Choose between async/await and RxJS patterns based on the operation type

**Async/Await Pattern**: Use for single-value operations and state updates

**When to Use**:

- Single HTTP requests (GET, POST, PUT, DELETE)
- One-time operations that return a single result
- State update methods in stores
- Simple asynchronous operations

**Pattern**:

```typescript
const result = await firstValueFrom(service.operation(params));
```

**RxJS/Observable Pattern**: Use for reactive data streams and complex async flows

**When to Use**:

- Real-time data streams (WebSocket, SSE)
- Multiple dependent HTTP requests
- Operations requiring cancellation
- Complex async flows with operators (debounce, retry, etc.)
- Component subscriptions to store state

**Pattern**:

```typescript
// In services - return observables
public getDataStream(): Observable<Data[]> {
  return this.http.get<Data[]>('/api/data').pipe(
    retry(3),
    catchError(this.handleError)
  );
}

// In components - subscribe to store signals
ngOnInit() {
  // Signal store state is automatically reactive
  this.devices = this.deviceStore.devices;
}
```

**Requirements**:

- Use `firstValueFrom()` when converting observables to promises in store methods
- Use `async/await` for store method implementations
- Return observables from services for reactive data
- Handle RxJS observables appropriately in each context
- Implement proper error propagation for both patterns

**Best Practice**: Use async/await in store methods for simplicity and RxJS patterns when you need reactive capabilities or complex async coordination.

---

## Usage Guidelines

### Store Organization

**Best Practices**:

- One store per domain/feature area
- Keep stores focused on specific responsibilities
- Use composition for complex state requirements
- Maintain clear boundaries between different data domains

### Method Grouping

**Guidelines**:

- Group methods by functionality (CRUD operations, UI actions, etc.)
- Separate methods that use different services
- Keep individual files focused on single responsibility
- Use descriptive names that indicate the operation scope

### Testing Considerations

**Standards**:

- Method separation enables easier unit testing
- Mock services through parameter injection
- Test error conditions and success paths
- Verify state updates are immutable

---

## Related Files

- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md) - General coding patterns and conventions
- **Style Guide**: [`STYLE_GUIDE.md`](./STYLE_GUIDE.md) - UI styling standards
- **Store Implementation**: [`device-store.ts`](../libs/domain/device/state/src/lib/device-store.ts) - Reference implementation
