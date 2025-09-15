# State Management Standards

## Overview

This document establishes standards for state management using NgRx Signal Store with async/await patterns. These standards ensure consistency, maintainability, and scalability across all state management implementations.

**Primary Pattern**: Use async/await for store methods to provide deterministic, sequential Promise resolution that avoids concurrency issues common with RxJS observables in complex state scenarios.

---

## Signal Store Architecture

### Store Structure

**Standard**: Use NgRx Signal Store with modular async function organization

**Format**:

```typescript
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withDevtools('storeName'),
  withState(initialState),
  withMethods((store, service = inject(SERVICE_TOKEN)) => ({
    ...asyncMethod1(store, service),
    ...asyncMethod2(store, service),
    // Additional async functions
  }))
);
```

**Implementation Example**: See [StorageStore](../libs/domain/storage/state/src/lib/storage-store.ts) for a complete implementation following this pattern with async/await methods.

**Requirements**:

- Use `providedIn: 'root'` for application-level stores
- Always include `withDevtools()` with descriptive store name
- Define explicit TypeScript interfaces for state
- Inject services with default parameters using `inject()`
- Group related functions using spread operator
- Prefer async/await methods over RxJS patterns for state operations

**Reference Implementation**: [`storage-store.ts`](../libs/domain/storage/state/src/lib/storage-store.ts)

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

- [`initialize-storage.ts`](../libs/domain/storage/state/src/lib/methods/initialize-storage.ts) - Async/await function example
- [`navigate-to-directory.ts`](../libs/domain/storage/state/src/lib/methods/navigate-to-directory.ts) - Async operation with state updates
- [`refresh-directory.ts`](../libs/domain/storage/state/src/lib/methods/refresh-directory.ts) - Error handling patterns

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
import { patchState, WritableStateSource } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function methodName(
  store: SignalStore<StateType> & WritableStateSource<StateType>,
  service: ServiceType
) {
  return {
    methodName: async ({ param }: { param: ParamType }): Promise<void> => {
      // Clear any previous errors
      patchState(store, { isLoading: true, error: null });

      try {
        const result = await firstValueFrom(service.operation(param));

        patchState(store, {
          data: result,
          isLoading: false,
          isLoaded: true,
          error: null,
          lastUpdateTime: Date.now(),
        });
      } catch (error) {
        patchState(store, {
          isLoading: false,
          error: (error as any)?.message || 'Operation failed',
        });
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

**Reference Implementation**: [`methods/`](../libs/domain/storage/state/src/lib/methods/) directory structure

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
export interface MainState {
  // Core data collections
  entries: Record<string, EntryType>;
  selectedItems: Record<string, SelectionType>;

  // UI state
  isLoading: boolean;
  isLoaded: boolean;

  // Error handling
  error: string | null;
  lastLoadTime: number | null;
}

// Initial state
const initialState: MainState = {
  entries: {},
  selectedItems: {},
  isLoading: false,
  isLoaded: false,
  error: null,
  lastLoadTime: null,
};

// Store definition
export const MainStore = signalStore(
  { providedIn: 'root' },
  withDevtools('mainStore'),
  withState(initialState),
  withMethods((store, service = inject(SERVICE_TOKEN)) => ({
    ...asyncMethod1(store, service),
    ...asyncMethod2(store, service),
  }))
);
```

**Reference Implementation**: See [`StorageStore`](../libs/domain/storage/state/src/lib/storage-store.ts) for complete implementation.

**Requirements**:

- **Single File Organization**: State type, initial state, and store in one file
- **Descriptive Property Names**: Use clear, meaningful property names
- **Logical Grouping**: Group related properties together in state type
- **Error Handling**: Always include error handling properties
- **Loading States**: Include loading states for async operations
- **Sensible Defaults**: Provide appropriate defaults for all properties
- **Helper Utilities**: Include any helper utilities (like key generators) in same file if needed

---

## Computed Signals

### Derived State with `withComputed`

**Standard**: Expose derived state from stores using `withComputed` and Angular Signals. Consumers should read values as signals (not plain values) to keep data flow fully reactive and composable.

**Patterns**:

```typescript
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    // Non‑parameterized derivation
    selectedItem: computed(() => {
      const id = store.selectedId();
      return id ? store.items().find((i) => i.id === id) ?? null : null;
    }),

    // Parameterized derivation via factory
    getItemsByGroup: (groupId: string) =>
      computed(() => store.items().filter((i) => i.groupId === groupId)),
  }))
);
```

**Guidelines**:

- Return Signals: All derived values should be `computed()` signals exposed from the store API.
- Prefer Factories: For parameterized selection, expose factories that return `computed()` signals (e.g., `getById(id)`), not plain arrays/objects.
- Purity: Keep `computed()` functions pure (no side effects or state mutations).
- Composition: Build complex derivations from simpler computed signals.
- Performance: Avoid recreating expensive factories in tight loops; cache at call sites if needed.

**Component Usage**:

- Read signals directly in templates and components.
- Pass parameters via signal inputs; construct factory‑based `computed()` selections in the component or expose factories from the store.
- Prefer signals over observables in new code; interop only when necessary.

---

## Error Handling

### Error State Management

**Standard**: Implement consistent error handling across all async functions

**Pattern**:

```typescript
export function asyncMethod(store, service) {
  return {
    methodName: async ({ param }: { param: ParamType }): Promise<void> => {
      // 1. Set loading state and clear previous errors
      patchState(store, { isLoading: true, error: null });

      try {
        // 2. Perform async operation with firstValueFrom
        const result = await firstValueFrom(service.operation(param));

        // 3. Update state on success
        patchState(store, {
          data: result,
          isLoading: false,
          isLoaded: true,
          error: null,
          lastUpdateTime: Date.now(),
        });
      } catch (error) {
        // 4. Handle errors with original message preservation
        patchState(store, {
          isLoading: false,
          error: (error as any)?.message || 'Operation failed',
        });
      }
    },
  };
}
```

**Requirements**:

- Always set loading state and clear errors at operation start
- Use `(error as any)?.message || 'fallback'` for error message preservation
- Reset loading states in error handling
- Include timestamp tracking for successful operations
- Use `firstValueFrom()` for converting observables to promises

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

**Standard**: Prefer async/await pattern for NgRx Signal Store methods to ensure deterministic execution and avoid concurrency issues

**Primary Pattern - Async/Await**: Use for all store operations unless specifically requiring reactive streams

**When to Use Async/Await**:

- All HTTP requests (GET, POST, PUT, DELETE)
- State update operations in stores
- Sequential operations requiring deterministic execution
- Operations that need to avoid race conditions
- One-time operations that return a single result

**Pattern**:

```typescript
export function storeMethod(store, service) {
  return {
    methodName: async ({ param }: { param: Type }): Promise<void> => {
      patchState(store, { isLoading: true, error: null });

      try {
        const result = await firstValueFrom(service.getData(param));

        patchState(store, {
          data: result,
          isLoading: false,
          isLoaded: true,
          lastUpdateTime: Date.now(),
        });
      } catch (error) {
        patchState(store, {
          isLoading: false,
          error: (error as any)?.message || 'Operation failed',
        });
      }
    },
  };
}
```

**Limited RxJS Usage**: Only use for reactive streams requiring continuous data

**When to Use RxJS**:

- Real-time data streams (WebSocket, SSE)
- Service-level observable streams
- Component subscriptions to store signals

**Requirements**:

- **Primary**: Use `async/await` with `firstValueFrom()` for all store methods
- **Services**: Return observables for reactive data streams
- **Components**: Subscribe to store signals directly (they are automatically reactive)
- **Error Handling**: Preserve original error messages with proper type casting
- **State Management**: Include loading, loaded, and error states consistently

**Best Practice**: Default to async/await for store methods. This provides deterministic Promise resolution and avoids the concurrency issues that can occur with RxJS observables in complex state scenarios.

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

**Primary Pattern - Async Methods**: Use for all operations requiring external data or services

```typescript
import { patchState, WritableStateSource } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';

export function loadData(
  store: SignalStore<StateType> & WritableStateSource<StateType>,
  service: ServiceType
) {
  return {
    loadData: async ({ id }: { id: string }): Promise<void> => {
      // Set loading state and clear errors
      patchState(store, { isLoading: true, error: null });

      try {
        const data = await firstValueFrom(service.getData(id));

        // Update with success data
        patchState(store, {
          data,
          isLoading: false,
          isLoaded: true,
          error: null,
          lastLoadTime: Date.now(),
        });
      } catch (error) {
        // Handle errors with message preservation
        patchState(store, {
          isLoading: false,
          error: (error as any)?.message || 'Failed to load data',
        });
      }
    },
  };
}
```

**Synchronous Methods**: Use only for simple state updates that don't require external services

```typescript
export function updateSelection(store: SignalStore<StateType> & WritableStateSource<StateType>) {
  return {
    updateSelection: ({ id, selection }: { id: string; selection: SelectionType }) => {
      patchState(store, (state) => ({
        selectedItems: {
          ...state.selectedItems,
          [id]: selection,
        },
      }));
    },
  };
}
```

### Store Configuration Pattern

**Standard**: Remove unused service parameters and only pass services that are actually used

**Correct Store Setup**:

```typescript
export const MainStore = signalStore(
  { providedIn: 'root' },
  withDevtools('mainStore'),
  withState(initialState),
  withMethods((store, service = inject(SERVICE_TOKEN)) => ({
    ...asyncMethod1(store, service),
    ...asyncMethod2(store, service),
    ...syncMethodNotUsingServices(store), // No service parameter needed
  }))
);
```

### Common Anti-Patterns to Avoid

**❌ WRONG - Using `any` types**:

```typescript
export function badMethod(store: any, service: any) {
  // This violates coding standards and breaks type safety
}
```

**❌ WRONG - Using RxJS rxMethod for simple operations**:

```typescript
export function loadData(store, service) {
  return {
    loadData: rxMethod<{ id: string }>(
      pipe(
        switchMap(({ id }) => service.getData(id)),
        tap((data) => patchState(store, { data }))
      )
    ),
  };
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

**✅ CORRECT - Use async/await pattern**:

```typescript
export function loadData(store: SignalStore<State> & WritableStateSource<State>, service: Service) {
  return {
    loadData: async ({ id }: { id: string }): Promise<void> => {
      patchState(store, { isLoading: true, error: null });

      try {
        const data = await firstValueFrom(service.getData(id));
        patchState(store, {
          data,
          isLoading: false,
          isLoaded: true,
          lastUpdateTime: Date.now(),
        });
      } catch (error) {
        patchState(store, {
          isLoading: false,
          error: (error as any)?.message || 'Failed to load data',
        });
      }
    },
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

**Root Cause**: NgRx Signal Store methods should not directly call other store methods. The correct pattern is to use async/await with direct service calls.

**✅ CORRECT Solution - Use async/await with direct service calls**:

```typescript
export function refreshData(
  store: SignalStore<State> & WritableStateSource<State>,
  service: Service
) {
  return {
    refreshData: async ({ id }: { id: string }): Promise<void> => {
      patchState(store, { isLoading: true, error: null });

      try {
        // Directly call service, don't depend on other store methods
        const data = await firstValueFrom(service.getData(id));

        patchState(store, {
          data,
          isLoading: false,
          isLoaded: true,
          lastUpdateTime: Date.now(),
        });
      } catch (error) {
        patchState(store, {
          isLoading: false,
          error: (error as any)?.message || 'Failed to refresh data',
        });
      }
    },
  };
}
```

### Store Configuration Pitfalls

**❌ WRONG - Passing unused services**:

```typescript
export const MainStore = signalStore(
  { providedIn: 'root' },
  withMethods((store, serviceA = inject(ServiceA), serviceB = inject(ServiceB)) => ({
    ...asyncMethodUsingServiceA(store, serviceA),
    ...syncMethodNotUsingServices(store, serviceB), // Wrong - unused service
  }))
);
```

**✅ CORRECT - Only pass services that are actually used**:

```typescript
export const MainStore = signalStore(
  { providedIn: 'root' },
  withMethods((store, service = inject(SERVICE_TOKEN)) => ({
    ...asyncMethodUsingService(store, service),
    ...syncMethodNotUsingServices(store), // No unused service parameter
  }))
);
```

### Method Implementation Pitfalls

**❌ WRONG - Missing proper error handling and state management**:

```typescript
export function updateData(store: SignalStore<State>, service: Service) {
  return {
    updateData: async ({ id, data }) => {
      // Missing loading state, error handling, and proper typing
      const result = await service.update(id, data);
      patchState(store, { result });
    },
  };
}
```

**✅ CORRECT - Proper async/await implementation with complete state management**:

```typescript
export function updateData(
  store: SignalStore<State> & WritableStateSource<State>,
  service: Service
) {
  return {
    updateData: async ({ id, data }: { id: string; data: DataType }): Promise<void> => {
      patchState(store, { isLoading: true, error: null });

      try {
        const result = await firstValueFrom(service.update(id, data));

        patchState(store, {
          result,
          isLoading: false,
          isLoaded: true,
          error: null,
          lastUpdateTime: Date.now(),
        });
      } catch (error) {
        patchState(store, {
          isLoading: false,
          error: (error as any)?.message || 'Update failed',
        });
      }
    },
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

- Each function should handle one specific async operation
- Use async/await pattern for all service-dependent operations
- Keep individual files focused on single responsibility
- Use descriptive names that clearly indicate the operation
- Include proper TypeScript typing with Promise return types

### Testing Considerations

**Standards**:

- One-function-per-file pattern enables easier unit testing
- Mock services through parameter injection for isolated testing
- Test async/await functions with proper Promise handling
- Test error conditions and success paths for each async function
- Verify state updates are immutable and consistent
- Use `await` for all async method calls in tests

## Store Testing Requirements

See STORE_TESTING.md for the complete, up‑to‑date methodology, patterns, and checklists for testing NgRx Signal Stores (setup, mocking, behaviors to cover, and example snippets).

---

## Related Documentation

- **Testing Standards**: [`TESTING_STANDARDS.md`](./TESTING_STANDARDS.md) - Unit, integration, and E2E testing patterns.
- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md) - General coding patterns and conventions.
- **Nx Library Standards**: [`NX_LIBRARY_STANDARDS.md`](./NX_LIBRARY_STANDARDS.md) - Library creation and integration patterns.
- **Style Guide**: [`STYLE_GUIDE.md`](./STYLE_GUIDE.md) - UI styling standards.

## Reference Implementation

- **StorageStore**: [`storage-store.ts`](../libs/domain/storage/state/src/lib/storage-store.ts) - Primary example of async/await store implementation
- **Storage Methods**: [`methods/`](../libs/domain/storage/state/src/lib/methods/) - Individual async function implementations
- **Storage Tests**: [`storage-store.spec.ts`](../libs/domain/storage/state/src/lib/storage-store.spec.ts) - Complete async/await testing patterns

**Key Features of Reference Implementation**:

- Async/await methods with `firstValueFrom()` for Promise conversion
- Comprehensive error handling with message preservation
- Loading state management with proper cleanup
- Sequential operation support to avoid concurrency issues
- Complete test coverage with async test patterns
