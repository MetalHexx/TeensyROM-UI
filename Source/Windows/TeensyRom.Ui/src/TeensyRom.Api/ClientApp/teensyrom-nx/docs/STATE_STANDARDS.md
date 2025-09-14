# State Management Standards

## Overview

This document establishes standards for state management using NgRx Signal Store. These standards ensure consistency, maintainability, and scalability across all state management implementations.

---

## Signal Store Architecture

### Store Structure

**Standard**: Use NgRx Signal Store with modular function organization

**Format**:

```typescript
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withDevtools('storeName'),
  withState(initialState),
  withMethods((store, ...services) => ({
    ...methodFunction1(store, service1),
    ...methodFunction2(store, service2),
    // Additional functions
  }))
);
```

**Implementation Example**: See [DeviceStore](../libs/domain/device/state/src/lib/device-store.ts) for a complete implementation following this pattern.

**Requirements**:

- Use `providedIn: 'root'` for application-level stores
- Always include `withDevtools()` with descriptive store name
- Define explicit TypeScript interfaces for state
- Inject services with default parameters using `inject()`
- Group related functions using spread operator

**Reference Implementation**: [`device-store.ts`](../libs/domain/device/state/src/lib/device-store.ts)

---

## Function Organization

### One Function Per File Pattern

**Standard**: Extract each individual function in its own file within a `methods/` directory

**Structure**:

```
state/
├── example-store.ts
└── methods/
    ├── index.ts
    ├── load-data.ts         # loadData function
    ├── create-item.ts       # createItem function
    ├── update-item.ts       # updateItem function
    └── delete-item.ts       # deleteItem function
```

**File Naming Convention**:

- Use kebab-case for function files
- Name files based on the individual function name
- Each file exports one function that returns an object with one property (a function)
- Use descriptive names that indicate the specific operation

**Implementation Examples**: See the following files for concrete implementations:

- [`connect-device.ts`](../libs/domain/device/state/src/lib/methods/connect-device.ts) - Single function example
- [`find-devices.ts`](../libs/domain/device/state/src/lib/methods/find-devices.ts) - Async operation with state updates
- [`disconnect-device.ts`](../libs/domain/device/state/src/lib/methods/disconnect-device.ts) - Error handling patterns

**Key Principles**:

- Each function returns an object with exactly one property (a function)
- Functions are pure and focused on single responsibility
- All external dependencies are injected as parameters
- State updates use `patchState` for immutable updates

### Function File Structure

**Standard**: Follow consistent structure for function files

**Requirements**:

1. Import necessary dependencies at the top
2. Define SignalStore type helper (if needed)
3. Export single function that returns object with one property (a function)
4. Use descriptive parameter names
5. Include proper error handling
6. Clear state updates with `patchState`

**Template Pattern**:

```typescript
import { inject } from '@angular/core';
import { patchState, WritableStateSource } from '@ngrx/signals';
import { ExampleService } from '@teensyrom-nx/domain/example/services';
import { firstValueFrom } from 'rxjs';
import { ExampleState } from '../example-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function methodName(
  store: SignalStore<ExampleState> & WritableStateSource<ExampleState>,
  service: ExampleService = inject(ExampleService)
) {
  return {
    methodName: async (param: ParamType) => {
      patchState(store, { error: null });

      try {
        const result = await firstValueFrom(service.operation(param));
        patchState(store, {
          /* state updates */
        });
      } catch (error) {
        patchState(store, { error: String(error) });
      }
    },
  };
}
```

### Index File Management

**Standard**: Use index files to organize and export individual functions

**Format**:

```typescript
// src/lib/methods/index.ts
export * from './load-data';
export * from './create-item';
export * from './update-item';
export * from './delete-item';
```

**Requirements**:

- Export each function individually
- Use barrel exports for cleaner imports in store files
- Update index when adding new function files
- Group related function exports together for readability

**Reference Implementation**: [`methods/index.ts`](../libs/domain/device/state/src/lib/methods/index.ts)

---

## State Type Definitions

### State Structure Organization

**Standard**: Define state types, initial state, and store in a single store file

**File Structure**: Everything in `[domain]-store.ts`:

- State type definition
- Initial state constant
- Store configuration with methods
- Helper utilities (if needed)

**Format**:

```typescript
// State type definition
export type ExampleState = {
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

// Initial state
const initialState: ExampleState = {
  items: [],
  isLoading: false,
  hasInitialised: false,
  error: null,
  featureSpecificProperty: false,
};

// Store definition
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withDevtools('exampleStoreName'),
  withState(initialState),
  withMethods((store, service = inject(ExampleService)) => ({
    ...methodFunction1(store, service),
    ...methodFunction2(store, service),
  }))
);
```

**Reference Implementation**: See [`DeviceStore`](../libs/domain/device/state/src/lib/device-store.ts) for complete implementation.

**Requirements**:

- **Single File Organization**: State type, initial state, and store in one file
- **Descriptive Property Names**: Use clear, meaningful property names
- **Logical Grouping**: Group related properties together in state type
- **Error Handling**: Always include error handling properties
- **Loading States**: Include loading states for async operations
- **Sensible Defaults**: Provide appropriate defaults for all properties
- **Helper Utilities**: Include any helper utilities (like key generators) in same file if needed

---

## Error Handling

### Error State Management

**Standard**: Implement consistent error handling across all functions

**Pattern**:

```typescript
export function exampleMethod(store, service) {
  return {
    methodName: async (params) => {
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
export function exampleMethod(
  store: SignalStore<ExampleState> & WritableStateSource<ExampleState>,
  service: ExampleService = inject(ExampleService)
) {
  return {
    methodName: async (params) => {
      // function implementation
    },
  };
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
- State update operations in stores
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
public getDataStream(): Observable<ExampleData[]> {
  return this.http.get<ExampleData[]>('/api/data').pipe(
    retry(3),
    catchError(this.handleError)
  );
}

// In components - subscribe to store signals
ngOnInit() {
  // Signal store state is automatically reactive
  this.items = this.exampleStore.items;
}
```

**Requirements**:

- Use `firstValueFrom()` when converting observables to promises in store functions
- Use `async/await` for store function implementations
- Return observables from services for reactive data
- Handle RxJS observables appropriately in each context
- Implement proper error propagation for both patterns

**Best Practice**: Use async/await in store functions for simplicity and RxJS patterns when you need reactive capabilities or complex async coordination.

---

## NgRx Signal Store Method Patterns

### Method Signature Standards

**Standard**: Use consistent type signatures for all store methods to avoid TypeScript errors

**Critical Pattern**: Never use `any` types - always use the proper SignalStore intersection type

**Required Type Pattern**:

```typescript
type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function methodName(
  store: SignalStore<StateType> & WritableStateSource<StateType>,
  service: ServiceType = inject(ServiceType)
) {
  return {
    methodName: // implementation
  };
}
```

### Method Implementation Patterns

**Reactive Methods (rxMethod)**: Use for async operations with observables

```typescript
import { patchState, WritableStateSource } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe } from 'rxjs';
import { switchMap, tap, catchError, of } from 'rxjs';

export function loadData(
  store: SignalStore<ExampleState> & WritableStateSource<ExampleState>,
  service: ExampleService = inject(ExampleService)
) {
  return {
    loadData: rxMethod<{ id: string }>(
      pipe(
        tap(({ id }) => {
          // Set loading state
          patchState(store, { isLoading: true, error: null });
        }),
        switchMap(({ id }) => {
          return service.getData(id).pipe(
            tap((data) => {
              // Update with success data
              patchState(store, {
                data,
                isLoading: false,
                error: null,
                lastLoadTime: Date.now(),
              });
            }),
            catchError((error) => {
              // Handle errors
              patchState(store, {
                isLoading: false,
                error: error.message || 'Failed to load data',
              });
              return of(null);
            })
          );
        })
      )
    ),
  };
}
```

**Synchronous Methods**: Use for simple state updates

```typescript
export function updateItem(store: SignalStore<ExampleState> & WritableStateSource<ExampleState>) {
  return {
    updateItem: ({ id, updates }: { id: string; updates: Partial<Item> }) => {
      patchState(store, (state) => ({
        items: state.items.map((item) => (item.id === id ? { ...item, ...updates } : item)),
      }));
    },
  };
}
```

### Store Configuration Pattern

**Standard**: Remove unused service parameters and only pass services that are actually used

**Correct Store Setup**:

```typescript
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withDevtools('example'),
  withState(initialState),
  withMethods(
    (store, serviceA: ServiceA = inject(ServiceA), serviceB: ServiceB = inject(ServiceB)) => ({
      ...methodUsingServiceA(store, serviceA),
      ...methodUsingServiceB(store, serviceB),
      ...methodNotUsingServices(store), // No service parameter
    })
  )
);
```

### Common Anti-Patterns to Avoid

**❌ WRONG - Using `any` types**:

```typescript
export function badMethod(store: any, service: any) {
  // This violates coding standards and breaks type safety
}
```

**❌ WRONG - Calling other store methods directly**:

```typescript
export function refreshData(
  store: SignalStore<State> & WritableStateSource<State> & { loadData: Function }
) {
  return {
    refreshData: () => {
      store.loadData(); // This breaks the store method pattern
    },
  };
}
```

**❌ WRONG - Complex type intersections for method calls**:

```typescript
// Don't try to reference other store methods in type signatures
store: SignalStore<State> & WritableStateSource<State> & { otherMethod: Function };
```

**✅ CORRECT - Duplicate logic or use reactive patterns**:

```typescript
export function refreshData(
  store: SignalStore<State> & WritableStateSource<State>,
  service: Service = inject(Service)
) {
  return {
    refreshData: rxMethod<{ id: string }>(
      pipe(
        switchMap(({ id }) => {
          // Directly call service, don't depend on other store methods
          return service.getData(id).pipe(/* ... */);
        })
      )
    ),
  };
}
```

### TypeScript Signature Errors

**Problem**: Complex TypeScript errors when store methods try to call other store methods or use incorrect type signatures.

**Error Examples**:

```typescript
// Signature mismatch errors from complex intersections
store: SignalStore<State> & WritableStateSource<State> & { loadDirectory: Function };
```

**Root Cause**: NgRx Signal Store methods should not directly call other store methods. The correct pattern is to duplicate logic or use reactive patterns.

**✅ CORRECT Solution - Duplicate logic or use reactive patterns**:

```typescript
export function refreshData(
  store: SignalStore<State> & WritableStateSource<State>,
  service: Service = inject(Service)
) {
  return {
    refreshData: rxMethod<{ id: string }>(
      pipe(
        switchMap(({ id }) => {
          // Directly call service, don't depend on other store methods
          return service.getData(id).pipe(
            tap((data) => {
              patchState(store, { data, isLoading: false });
            }),
            catchError((error) => {
              patchState(store, { error: error.message, isLoading: false });
              return of(null);
            })
          );
        })
      )
    ),
  };
}
```

### Store Configuration Pitfalls

**❌ WRONG - Passing unused services**:

```typescript
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withMethods((store, serviceA = inject(ServiceA), serviceB = inject(ServiceB)) => ({
    ...methodOnlyUsingServiceA(store, serviceA),
    ...methodNotUsingAnyService(store, serviceB), // Wrong - unused service
  }))
);
```

**✅ CORRECT - Only pass services that are actually used**:

```typescript
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withMethods((store, serviceA = inject(ServiceA)) => ({
    ...methodUsingServiceA(store, serviceA),
    ...methodNotUsingServices(store), // No unused service parameter
  }))
);
```

### Method Implementation Pitfalls

**❌ WRONG - Synchronous method trying to be reactive**:

```typescript
export function updateData(store: SignalStore<State>, service: Service) {
  return {
    updateData: async ({ id, data }) => {
      // Don't mix async/await with rxMethod patterns
      const result = await service.update(id, data);
      patchState(store, { result });
    },
  };
}
```

**✅ CORRECT - Choose the right pattern for the operation**:

```typescript
// For simple async operations - use async/await
export function updateData(store: SignalStore<State>, service: Service) {
  return {
    updateData: async ({ id, data }) => {
      try {
        const result = await firstValueFrom(service.update(id, data));
        patchState(store, { result, error: null });
      } catch (error) {
        patchState(store, { error: String(error) });
      }
    },
  };
}

// For reactive streams - use rxMethod
export function loadDataStream(store: SignalStore<State>, service: Service) {
  return {
    loadDataStream: rxMethod<{ id: string }>(
      pipe(
        switchMap(({ id }) => service.getDataStream(id)),
        tap((data) => patchState(store, { data }))
      )
    ),
  };
}
```

---

## Usage Guidelines

### Store Organization

**Best Practices**:

- One store per domain/feature area
- Keep stores focused on specific responsibilities
- Use composition for complex state requirements
- Maintain clear boundaries between different data domains

### Function Usage

**Guidelines**:

- Each function should handle one specific operation
- Separate functions that use different services into different files
- Keep individual files focused on single responsibility
- Use descriptive names that clearly indicate the operation

### Testing Considerations

**Standards**:

- One-function-per-file pattern enables easier unit testing
- Mock services through parameter injection for isolated testing
- Test error conditions and success paths for each function
- Verify state updates are immutable and consistent

## Store Testing Requirements

See STORE_TESTING.md for the complete, up‑to‑date methodology, patterns, and checklists for testing NgRx Signal Stores (setup, mocking, behaviors to cover, and example snippets).

---

## Related Documentation

- **Testing Standards**: [`TESTING_STANDARDS.md`](./TESTING_STANDARDS.md) - Unit, integration, and E2E testing patterns.
- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md) - General coding patterns and conventions.
- **Nx Library Standards**: [`NX_LIBRARY_STANDARDS.md`](./NX_LIBRARY_STANDARDS.md) - Library creation and integration patterns.
- **Style Guide**: [`STYLE_GUIDE.md`](./STYLE_GUIDE.md) - UI styling standards.

## Reference Implementation

- **DeviceStore**: [`device-store.ts`](../libs/domain/device/state/src/lib/device-store.ts) - Complete store implementation example
- **Function Examples**: [`methods/`](../libs/domain/device/state/src/lib/methods/) - Individual function implementations
- **StorageStore**: `libs/domain/storage/state/src/lib/storage-store.ts` - Example of a properly tested store (see its spec at `libs/domain/storage/state/src/lib/storage-store.spec.ts`)
