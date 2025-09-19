# State Management Standards

## Overview

This document establishes standards for state management using NgRx Signal Store with async/await patterns. These standards ensure consistency, maintainability, and scalability across all state management implementations.

**Primary Pattern**: Use async/await for store methods to provide deterministic, sequential Promise resolution that avoids concurrency issues common with RxJS observables in complex state scenarios.

---

## Signal Store Architecture

### Store Structure

**Standard**: Use NgRx Signal Store with custom features for selectors and actions

**Format**:

```typescript
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withDevtools('storeName'),
  withState(initialState),
  withExampleSelectors(),
  withExampleActions()
);
```

**Custom Features Pattern**:

```typescript
// /selectors/index.ts - Read-only computed signals
export function withExampleSelectors() {
  return withMethods((store) => {
    const writableStore = store as WritableStore<ExampleState>;
    return {
      ...getSelectedItem(writableStore),
      ...getFilteredItems(writableStore),
    };
  });
}

// /actions/index.ts - State-changing methods
export function withExampleActions() {
  return withMethods((store, service: ServiceType = inject(SERVICE_TOKEN)) => {
    const writableStore = store as WritableStore<ExampleState>;
    return {
      ...loadData(writableStore, service),
      ...updateItem(writableStore, service),
    };
  });
}
```

**Implementation Example**: See [StorageStore](../libs/domain/storage/state/src/lib/storage-store.ts) for a complete implementation following this pattern with async/await methods.

**Requirements**:

- Use `providedIn: 'root'` for application-level stores
- Always include `withDevtools()` with descriptive store name
- Define explicit TypeScript interfaces for state
- Use `inject()` within `withMethods()` for service dependency injection
- Separate read-only operations (selectors) from state-changing operations (actions)
- Use custom features pattern with `withDomainSelectors()` and `withDomainActions()`
- Prefer async/await methods over RxJS patterns for state operations

**Reference Implementation**: [`storage-store.ts`](../libs/domain/storage/state/src/lib/storage-store.ts)

---

## Function Organization

### Selectors and Actions Pattern

**Standard**: Separate read-only operations (selectors) from state-changing operations (actions) into distinct folders

**Structure**:

```
state/
├── example-store.ts
├── actions/
│   ├── index.ts             # withExampleActions() custom feature
│   ├── load-data.ts         # async state-changing operations
│   ├── create-item.ts       # async operations with services
│   ├── update-item.ts       # state mutations
│   └── delete-item.ts       # cleanup operations
└── selectors/
    ├── index.ts             # withExampleSelectors() custom feature
    ├── get-selected-item.ts # computed signals for derived state
    ├── get-filtered-items.ts# parameterized computed signals
    └── get-item-summary.ts  # complex derived computations
```

**File Naming Convention**:

- Use kebab-case for function files
- **Actions**: Name with action verbs (load-, create-, update-, delete-)
- **Selectors**: Name with get- prefix for computed signals
- Each file exports one function that returns an object with one property
- Use descriptive names that indicate the specific operation

**Implementation Examples**: See the following files for concrete implementations:

- [`actions/initialize-storage.ts`](../libs/domain/storage/state/src/lib/actions/initialize-storage.ts) - Async/await action example
- [`actions/navigate-to-directory.ts`](../libs/domain/storage/state/src/lib/actions/navigate-to-directory.ts) - Async operation with state updates
- [`selectors/get-selected-directory-state.ts`](../libs/domain/storage/state/src/lib/selectors/get-selected-directory-state.ts) - Computed signal example

**Key Principles**:

- **Actions**: Each function returns an object with exactly one async method for state changes
- **Selectors**: Each function returns an object with computed signals for derived state
- Functions are pure and focused on single responsibility
- All external dependencies are injected as parameters
- State updates use `patchState` for immutable updates
- Selectors use `computed()` for reactive derived state

### Selectors vs Actions Distinction

**Standard**: Clear separation between read-only operations (selectors) and state-changing operations (actions)

**Selectors (/selectors folder)**:

- **Purpose**: Compute derived state from existing store state
- **Pattern**: Use `withMethods()` that return `computed()` signals
- **Characteristics**: Read-only, no state mutations, reactive
- **Return Type**: Computed signals or factory functions that return computed signals

**Selector Example**:

```typescript
// selectors/get-selected-item.ts
export function getSelectedItem(store: WritableStore<ExampleState>) {
  return {
    getSelectedItem: (id: string) =>
      computed(() => {
        const items = store.items();
        return items.find((item) => item.id === id) ?? null;
      }),
  };
}
```

**Actions (/actions folder)**:

- **Purpose**: Perform state mutations and async operations
- **Pattern**: Use `withMethods()` with async functions and `patchState()`
- **Characteristics**: State-changing, async operations, side effects
- **Return Type**: Promise-based async functions

**Action Example**:

```typescript
// actions/load-data.ts
export function loadData(store: WritableStore<ExampleState>, service: ExampleService) {
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

**Key Differences**:

- **Selectors**: Return `computed()` signals, no `patchState()`, no async operations
- **Actions**: Use `patchState()` for mutations, async/await patterns, error handling

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

**Standard**: Use index files to create custom features that combine individual functions

**Actions Index Format**:

```typescript
// src/lib/actions/index.ts
import { inject } from '@angular/core';
import { withMethods } from '@ngrx/signals';
import { ServiceType, SERVICE_TOKEN } from '@domain/services';
import { WritableStore } from '../helpers';
import { ExampleState } from '../example-store';
import { loadData } from './load-data';
import { createItem } from './create-item';
import { updateItem } from './update-item';
import { deleteItem } from './delete-item';

export function withExampleActions() {
  return withMethods((store, service: ServiceType = inject(SERVICE_TOKEN)) => {
    const writableStore = store as WritableStore<ExampleState>;
    return {
      ...loadData(writableStore, service),
      ...createItem(writableStore, service),
      ...updateItem(writableStore, service),
      ...deleteItem(writableStore, service),
    };
  });
}
```

**Selectors Index Format**:

```typescript
// src/lib/selectors/index.ts
import { withMethods } from '@ngrx/signals';
import { WritableStore } from '../helpers';
import { ExampleState } from '../example-store';
import { getSelectedItem } from './get-selected-item';
import { getFilteredItems } from './get-filtered-items';
import { getItemSummary } from './get-item-summary';

export function withExampleSelectors() {
  return withMethods((store) => {
    const writableStore = store as WritableStore<ExampleState>;
    return {
      ...getSelectedItem(writableStore),
      ...getFilteredItems(writableStore),
      ...getItemSummary(writableStore),
    };
  });
}
```

**Requirements**:

- Create custom feature functions (`withDomainActions()`, `withDomainSelectors()`)
- Import individual function files
- Use proper TypeScript typing with `WritableStore<T>`
- Handle service injection in actions index only
- Group related function imports together for readability

**Reference Implementation**:

- [`actions/index.ts`](../libs/domain/storage/state/src/lib/actions/index.ts) - Actions custom feature
- [`selectors/index.ts`](../libs/domain/storage/state/src/lib/selectors/index.ts) - Selectors custom feature

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
  withMainSelectors(),
  withMainActions()
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

### Derived State with Selectors

**Standard**: Expose derived state from stores using selector functions that return `computed()` signals. Consumers should read values as signals (not plain values) to keep data flow fully reactive and composable.

**Patterns**:

```typescript
// selectors/get-selected-item.ts
export function getSelectedItem(store: WritableStore<ExampleState>) {
  return {
    // Non‑parameterized derivation
    selectedItem: computed(() => {
      const id = store.selectedId();
      return id ? store.items().find((i) => i.id === id) ?? null : null;
    }),
  };
}

// selectors/get-items-by-group.ts
export function getItemsByGroup(store: WritableStore<ExampleState>) {
  return {
    // Parameterized derivation via factory
    getItemsByGroup: (groupId: string) =>
      computed(() => store.items().filter((i) => i.groupId === groupId)),
  };
}

// selectors/index.ts
export function withExampleSelectors() {
  return withMethods((store) => {
    const writableStore = store as WritableStore<ExampleState>;
    return {
      ...getSelectedItem(writableStore),
      ...getItemsByGroup(writableStore),
    };
  });
}
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

## Helper Utilities

### State Mutation Helpers

**Standard**: Create reusable helper functions to reduce code duplication and improve maintainability in actions

**Pattern**: Create a `[domain]-helpers.ts` file with common state operations

**Helper Categories**:

1. **State Mutation Helpers**:

```typescript
// Common state updates
export function setLoadingStorage(store: WritableStore<StorageState>, key: StorageKey): void {
  patchState(store, (state) => ({
    storageEntries: {
      ...state.storageEntries,
      [key]: {
        ...state.storageEntries[key],
        isLoading: true,
        error: null,
      },
    },
  }));
}

export function setStorageLoaded(
  store: WritableStore<StorageState>,
  key: StorageKey,
  additionalUpdates: Partial<StorageDirectoryState> = {}
): void {
  updateStorage(store, key, {
    isLoaded: true,
    isLoading: false,
    error: null,
    lastLoadTime: Date.now(),
    ...additionalUpdates,
  });
}
```

2. **State Query Helpers**:

```typescript
// Read operations for complex state logic
export function getStorage(
  store: WritableStore<StorageState>,
  key: StorageKey
): StorageDirectoryState | undefined {
  const currentState = store.storageEntries();
  return currentState[key];
}

export function isDirectoryLoadedAtPath(
  entry: StorageDirectoryState | undefined,
  path: string
): boolean {
  return !!(
    entry &&
    entry.currentPath === path &&
    entry.isLoaded &&
    entry.directory &&
    !entry.error
  );
}
```

3. **Type Helpers**:

```typescript
// Shared types for consistent store patterns
export type WritableStore<T extends object> = StateSignals<T> & WritableStateSource<T>;
```

4. **Logging Helpers**:

```typescript
export enum LogType {
  Start = '🚀',
  Success = '✅',
  NetworkRequest = '📡',
  Finish = '🏁',
  Error = '❌',
}

export function logInfo(operation: LogType, message: string, data?: unknown): void {
  if (data !== undefined) {
    console.log(`${operation} ${message}`, data);
  } else {
    console.log(`${operation} ${message}`);
  }
}
```

**Benefits**:

- **Consistency**: Standardized state mutations across all actions
- **Maintainability**: Single place to update common operations
- **Readability**: Actions focus on business logic, not implementation details
- **Testing**: Helper functions can be unit tested independently

**Usage in Actions**:

```typescript
// actions/load-data.ts
import {
  setLoadingStorage,
  setStorageLoaded,
  setStorageError,
  logInfo,
  LogType,
} from '../domain-helpers';

export function loadData(store: WritableStore<DomainState>, service: DomainService) {
  return {
    loadData: async ({ id }: { id: string }): Promise<void> => {
      const key = createKey(id);

      logInfo(LogType.Start, `Loading data for ${key}`);
      setLoadingStorage(store, key);

      try {
        const data = await firstValueFrom(service.getData(id));
        logInfo(LogType.Success, `Data loaded for ${key}`);
        setStorageLoaded(store, key, { data });
      } catch (error) {
        setStorageError(store, key, error.message || 'Failed to load data');
      }
    },
  };
}
```

**Reference Implementation**: [`storage-helpers.ts`](../libs/domain/storage/state/src/lib/storage-helpers.ts)

---

## Service Integration

### Service Injection

**Standard**: Use Angular's `inject()` function in actions custom features

**Actions Pattern**:

```typescript
// actions/index.ts
export function withExampleActions() {
  return withMethods((store, service: ExampleService = inject(EXAMPLE_SERVICE)) => {
    const writableStore = store as WritableStore<ExampleState>;
    return {
      ...loadData(writableStore, service),
      ...updateItem(writableStore, service),
    };
  });
}

// actions/load-data.ts
export function loadData(store: WritableStore<ExampleState>, service: ExampleService) {
  return {
    loadData: async (params) => {
      // function implementation
    },
  };
}
```

**Requirements**:

- Use `inject()` with default parameters in actions custom feature
- Services are passed to individual action functions as parameters
- Use explicit typing for service parameters
- Import services from appropriate domain paths
- Enable dependency injection testing through parameter overrides
- Selectors do not require service injection (read-only operations)

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

## Logging Standards

### Operation Visibility

**Standard**: Implement consistent emoji-enhanced logging across all store operations to provide clear operational visibility and debugging support.

**Core Principle**: Use a centralized `LogType` enum system with direct emoji values to create visually distinctive, semantically meaningful logs that clearly show operation lifecycle and state changes.

**Implementation**: See [`LOGGING_STANDARDS.md`](./LOGGING_STANDARDS.md) for complete logging standards including:

- LogType enum system with emoji values (🚀 Start, 📡 NetworkRequest, ✅ Success, 🏁 Finish, etc.)
- Operation lifecycle logging patterns
- Cache hit detection with informational logging
- Error handling and warning systems
- Integration patterns for NgRx Signal Stores
- Testing considerations for logged operations

**Quick Reference**:

```typescript
import { LogType, logInfo, logError } from './helpers';

// Operation lifecycle pattern
logInfo(LogType.Start, `Starting operation for ${key}`);
logInfo(LogType.NetworkRequest, `Making API call for ${key}`);
logInfo(LogType.Success, `API call successful for ${key}:`, data);
logInfo(LogType.Finish, `Operation completed for ${key}`);

// Cache hits and informational
logInfo(LogType.Info, `${key} already loaded, skipping operation`);

// Error handling
logError(`Operation failed for ${key}:`, error);
```

**Requirements**:

- Use LogType enum for all store logging operations
- Follow operation lifecycle patterns (Start → NetworkRequest → Success → Finish)
- Log cache hits with LogType.Info for optimization visibility
- Include relevant context (keys, identifiers, paths) in all log messages
- Maintain reasonable logging density without overwhelming console output

**Reference Implementation**: [`storage-store.ts`](../libs/domain/storage/state/src/lib/storage-store.ts) demonstrates complete logging integration within NgRx Signal Store operations.

---

## NgRx Signal Store Method Patterns

### Method Signature Standards

**Standard**: Use consistent type signatures for actions and selectors to avoid TypeScript errors

**Critical Pattern**: Never use `any` types - always use the proper `WritableStore<T>` type

**Required Type Pattern**:

```typescript
// Define helper type for store methods
type WritableStore<T extends object> = {
  [K in keyof T]: () => T[K];
} & WritableStateSource<T>;

// Action function signature
export function actionName(store: WritableStore<StateType>, service: ServiceType) {
  return {
    actionName: async (params) => {
      // implementation
    },
  };
}

// Selector function signature
export function selectorName(store: WritableStore<StateType>) {
  return {
    selectorName: computed(() => {
      // implementation
    }),
  };
}
```

**Custom Features Pattern**:

```typescript
// actions/index.ts
export function withDomainActions() {
  return withMethods((store, service: ServiceType = inject(SERVICE_TOKEN)) => {
    const writableStore = store as WritableStore<StateType>;
    return {
      ...actionFunction1(writableStore, service),
      ...actionFunction2(writableStore, service),
    };
  });
}

// selectors/index.ts
export function withDomainSelectors() {
  return withMethods((store) => {
    const writableStore = store as WritableStore<StateType>;
    return {
      ...selectorFunction1(writableStore),
      ...selectorFunction2(writableStore),
    };
  });
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

**Standard**: Use custom features for clean separation of selectors and actions

**Correct Store Setup**:

```typescript
export const MainStore = signalStore(
  { providedIn: 'root' },
  withDevtools('mainStore'),
  withState(initialState),
  withMainSelectors(), // Read-only computed signals
  withMainActions() // State-changing async methods
);
```

**Custom Features Implementation**:

```typescript
// selectors/index.ts
export function withMainSelectors() {
  return withMethods((store) => {
    const writableStore = store as WritableStore<MainState>;
    return {
      ...getSelectedItem(writableStore),
      ...getFilteredItems(writableStore),
    };
  });
}

// actions/index.ts
export function withMainActions() {
  return withMethods((store, service = inject(SERVICE_TOKEN)) => {
    const writableStore = store as WritableStore<MainState>;
    return {
      ...loadData(writableStore, service),
      ...updateItem(writableStore, service),
    };
  });
}
```

### Common Anti-Patterns to Avoid

**❌ WRONG - Using `any` types**:

```typescript
export function badMethod(store: any, service: any) {
  // This violates coding standards and breaks type safety
}
```

**❌ WRONG - Using old methods pattern instead of custom features**:

```typescript
// Old pattern - don't use
export const Store = signalStore(
  withState(initialState),
  withMethods((store, service = inject(SERVICE)) => ({
    loadData: async ({ id }) => {
      /* action logic */
    },
    selectedItem: computed(() => {
      /* selector logic */
    }),
  }))
);
```

**❌ WRONG - Mixing selectors and actions in same custom feature**:

```typescript
export function withMixedFeatures() {
  return withMethods((store, service = inject(SERVICE)) => ({
    loadData: async ({ id }) => {
      /* action - state changing */
    },
    selectedItem: computed(() => {
      /* selector - read only */
    }),
  }));
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

**✅ CORRECT - Use custom features with separated selectors and actions**:

```typescript
// Store definition
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withDevtools('example'),
  withState(initialState),
  withExampleSelectors(), // Read-only computed signals
  withExampleActions() // State-changing async methods
);

// actions/load-data.ts
export function loadData(store: WritableStore<State>, service: Service) {
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

// selectors/get-selected-item.ts
export function getSelectedItem(store: WritableStore<State>) {
  return {
    selectedItem: computed(() => {
      const id = store.selectedId();
      return id ? store.items().find((item) => item.id === id) ?? null : null;
    }),
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

- **Logging Standards**: [`LOGGING_STANDARDS.md`](./LOGGING_STANDARDS.md) - Emoji-enhanced logging patterns for operational visibility.
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
