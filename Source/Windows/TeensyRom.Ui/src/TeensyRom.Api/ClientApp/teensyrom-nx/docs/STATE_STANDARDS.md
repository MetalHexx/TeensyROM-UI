# State Management Standards

## Overview

This document establishes standards for state management using NgRx Signal Store with async/await patterns. These standards ensure consistency, maintainability, and scalability across all state management implementations.

**Primary Pattern**: Use async/await for store methods to provide deterministic, sequential Promise resolution that avoids concurrency issues common with RxJS observables in complex state scenarios.

---

## ⚠️ Critical State Mutation Requirement

### Use updateState with actionMessage for ALL State Mutations

**REQUIRED**: All store actions MUST use `updateState()` from `@angular-architects/ngrx-toolkit` with an `actionMessage` parameter instead of `patchState()` from `@ngrx/signals`.

**Rationale**: The `patchState()` function does not support the `actionMessage` parameter required for Redux DevTools correlation. Without `actionMessage` tracking:

- State mutations cannot be traced in Redux DevTools
- Multiple state updates from a single action cannot be correlated
- Debugging complex state flows becomes extremely difficult
- State updates may fail silently in some cases

**Correct Pattern**:

```typescript
import { updateState } from '@angular-architects/ngrx-toolkit';
import { createAction } from '@teensyrom-nx/utils';

export function someAction(store: WritableStore<DomainState>) {
  return {
    someAction: async ({ id }: { id: string }): Promise<void> => {
      // Create action message for Redux DevTools tracking
      const actionMessage = createAction('some-action'); // kebab-case

      // Use updateState with actionMessage
      updateState(store, actionMessage, (state) => ({
        isLoading: true,
        error: null,
      }));

      try {
        // Perform async operation
        const data = await firstValueFrom(service.getData(id));

        // All state mutations use same actionMessage for correlation
        updateState(store, actionMessage, (state) => ({
          data,
          isLoading: false,
          isLoaded: true,
        }));
      } catch (error) {
        updateState(store, actionMessage, (state) => ({
          isLoading: false,
          error: error.message,
        }));
      }
    },
  };
}
```

**Incorrect Pattern (DO NOT USE)**:

```typescript
// ❌ WRONG - Do not use patchState
import { patchState } from '@ngrx/signals';

export function someAction(store: WritableStore<DomainState>) {
  return {
    someAction: async ({ id }: { id: string }): Promise<void> => {
      // ❌ patchState doesn't accept actionMessage parameter
      patchState(store, { isLoading: true }); // Cannot be tracked in Redux DevTools
    },
  };
}
```

**Helper Functions**: All helper functions that mutate state MUST also accept `actionMessage` as their final parameter:

```typescript
// ✅ CORRECT - Helper accepts actionMessage
export function setLoading(
  store: WritableStore<DomainState>,
  key: string,
  actionMessage: string // Required parameter
): void {
  updateState(store, actionMessage, (state) => ({
    entries: {
      ...state.entries,
      [key]: { ...state.entries[key], isLoading: true },
    },
  }));
}

// Usage in action
const actionMessage = createAction('load-data');
setLoading(store, key, actionMessage); // Pass actionMessage to helper
```

**Benefits of updateState with actionMessage**:

- ✅ Full Redux DevTools integration with action correlation
- ✅ All state mutations from a single operation show the same identifier
- ✅ Easier debugging of complex state flows
- ✅ Consistent tracking across all store actions
- ✅ Better visibility into state changes during development

**Historical Context**: This requirement was established after discovering a critical bug (Phase 3, Bug #4) where player actions using `patchState` were not properly tracked in Redux DevTools, making debugging impossible and causing state updates to fail silently. See `docs/features/player-state/PLAYER_DOMAIN_P3.md` Bug #4 for full details.

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
- **State updates MUST use `updateState` with `actionMessage` parameter for Redux DevTools tracking**
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
- **Pattern**: Use `withMethods()` with async functions and `updateState()` with `actionMessage`
- **Characteristics**: State-changing, async operations, side effects, Redux DevTools tracked
- **Return Type**: Promise-based async functions

**Action Example**:

```typescript
// actions/load-data.ts
import { updateState } from '@angular-architects/ngrx-toolkit';
import { createAction } from '@teensyrom-nx/utils';

export function loadData(store: WritableStore<ExampleState>, service: ExampleService) {
  return {
    loadData: async ({ id }: { id: string }): Promise<void> => {
      const actionMessage = createAction('load-data'); // Required for Redux DevTools

      updateState(store, actionMessage, (state) => ({ isLoading: true, error: null }));

      try {
        const data = await firstValueFrom(service.getData(id));
        updateState(store, actionMessage, (state) => ({
          data,
          isLoading: false,
          isLoaded: true,
          lastUpdateTime: Date.now(),
        }));
      } catch (error) {
        updateState(store, actionMessage, (state) => ({
          isLoading: false,
          error: (error as any)?.message || 'Failed to load data',
        }));
      }
    },
  };
}
```

**Key Differences**:

- **Selectors**: Return `computed()` signals, no `updateState()`, no async operations, no actionMessage
- **Actions**: Use `updateState()` with `actionMessage` for mutations, async/await patterns, error handling, Redux DevTools tracking

### Function File Structure

**Standard**: Follow consistent structure for function files

**Requirements**:

1. Import necessary dependencies at the top (including `updateState` from `@angular-architects/ngrx-toolkit`)
2. Define SignalStore type helper (if needed)
3. Export single function that returns object with one property (a function)
4. Use descriptive parameter names
5. Include proper error handling
6. **CRITICAL**: Use `updateState` with `actionMessage` for all state mutations (not `patchState`)
7. Create `actionMessage` at the start of each action using `createAction()`

**Template Pattern**:

```typescript
import { updateState } from '@angular-architects/ngrx-toolkit'; // REQUIRED
import { WritableStateSource } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { createAction, LogType, logInfo } from '@teensyrom-nx/utils';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function methodName(
  store: SignalStore<StateType> & WritableStateSource<StateType>,
  service: ServiceType
) {
  return {
    methodName: async ({ param }: { param: ParamType }): Promise<void> => {
      // REQUIRED: Create action message for Redux DevTools correlation
      const actionMessage = createAction('method-name'); // kebab-case

      logInfo(LogType.Start, 'Starting operation', { param });

      // Use updateState with actionMessage (NOT patchState)
      updateState(store, actionMessage, (state) => ({ isLoading: true, error: null }));

      try {
        const result = await firstValueFrom(service.operation(param));

        logInfo(LogType.Success, 'Operation completed successfully');

        // All state mutations use same actionMessage for correlation
        updateState(store, actionMessage, (state) => ({
          data: result,
          isLoading: false,
          isLoaded: true,
          error: null,
          lastUpdateTime: Date.now(),
        }));
      } catch (error) {
        logError('Operation failed', error);

        updateState(store, actionMessage, (state) => ({
          isLoading: false,
          error: (error as any)?.message || 'Operation failed',
        }));
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

## Action Message Tracking

### Redux DevTools Correlation

**Standard**: Use action message identifiers to correlate related store operations in Redux DevTools for better debugging and operational visibility.

**Purpose**: When a single store method performs multiple state mutations through helper functions, the action message creates a unique identifier that allows you to see which Redux DevTools actions belong to the same logical operation.

**Implementation Pattern**:

```typescript
import { createAction } from '@teensyrom-nx/utils';

export function storageMethod(store: WritableStore<StateType>, service: ServiceType) {
  return {
    methodName: async ({ param }: { param: ParamType }): Promise<void> => {
      // 1. Create unique action message at start of operation
      const actionMessage = createAction('method-name');

      // 2. Pass action message to all helper functions
      setLoadingState(store, key, actionMessage);

      try {
        const result = await firstValueFrom(service.operation(param));

        // 3. Continue passing action message through operation
        setSuccessState(store, key, result, actionMessage);
        updateRelatedData(store, relatedKey, data, actionMessage);
      } catch (error) {
        setErrorState(store, key, error.message, actionMessage);
      }
    },
  };
}
```

**Real-World Example** from [`navigate-to-directory.ts`](../libs/domain/storage/state/src/lib/actions/navigate-to-directory.ts):

```typescript
export function navigateToDirectory(
  store: WritableStore<StorageState>,
  storageService: IStorageService
) {
  return {
    navigateToDirectory: async ({ deviceId, storageType, path }) => {
      const actionMessage = createAction('navigate-to-directory');
      const key = StorageKeyUtil.create(deviceId, storageType);

      // All helper calls use the same action message
      if (!isSelectedDirectory(store, deviceId, storageType, path)) {
        setDeviceSelectedDirectory(store, deviceId, storageType, path, actionMessage);
      }

      setLoadingStorage(store, key, actionMessage);

      try {
        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, path)
        );
        setStorageLoaded(store, key, { currentPath: path, directory }, actionMessage);
      } catch (error) {
        updateStorage(
          store,
          key,
          {
            /* error state */
          },
          actionMessage
        );
      }
    },
  };
}
```

**Benefits**:

- **Debugging**: In Redux DevTools, all actions show `[method-name] [1234]` making it easy to see which actions are related
- **Operation Tracking**: Random number helps distinguish between multiple calls to the same method
- **State Flow Visibility**: Clear correlation between logical operations and their state mutations
- **Performance Analysis**: Easy to measure time between start and completion of complex operations

**Requirements**:

- Import `createAction` from `@teensyrom-nx/utils`
- Create action message at the start of each store method using descriptive method name
- Pass action message as the final parameter to ALL helper functions that perform state mutations
- Use kebab-case naming that matches the method name (e.g., `'navigate-to-directory'`, `'initialize-storage'`)
- ALL state mutation helpers MUST accept actionMessage parameter for Redux DevTools correlation

**Action Message Utility**: See [`store-helper.ts`](../libs/utils/src/lib/store-helper.ts) for the `createAction()` implementation that generates unique identifiers with random numbers.

---

## Helper Utilities

### State Mutation Helpers

**Standard**: Create reusable helper functions to reduce code duplication and improve maintainability in actions

**Pattern**: Create a `[domain]-helpers.ts` file with common state operations

**Helper Categories**:

1. **State Mutation Helpers**:

```typescript
// Common state updates - ALL helpers MUST accept actionMessage
export function setLoadingStorage(
  store: WritableStore<StorageState>,
  key: StorageKey,
  actionMessage: string
): void {
  updateState(store, actionMessage, (state) => ({
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
  additionalUpdates: Partial<StorageDirectoryState> = {},
  actionMessage: string
): void {
  updateStorage(
    store,
    key,
    {
      isLoaded: true,
      isLoading: false,
      error: null,
      lastLoadTime: Date.now(),
      ...additionalUpdates,
    },
    actionMessage
  );
}
```

2. **State Query Helpers** (read-only, no actionMessage needed):

```typescript
// Read operations for complex state logic - no state mutations, no actionMessage needed
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

4. **Action Message Helpers**:

```typescript
// Action message creation for Redux DevTools correlation
// Use kebab-case method name matching the action method
export function createAction(message: string): string {
  const randomInt = Math.floor(Math.random() * 10000);
  return `${message} [${randomInt}]`;
}

// Usage in actions - message should match method name in kebab-case
export function navigateToDirectory(store: WritableStore<StorageState>, service: IStorageService) {
  return {
    navigateToDirectory: async ({ deviceId, storageType, path }) => {
      const actionMessage = createAction('navigate-to-directory'); // kebab-case

      // Pass actionMessage to ALL helper functions
      setLoadingStorage(store, key, actionMessage);
      setDeviceSelectedDirectory(store, deviceId, storageType, path, actionMessage);
      // ... etc
    },
  };
}
```

5. **Logging Helpers**:

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

**Helper Function Action Message Rules**:

- **State Mutation Helpers**: MUST accept `actionMessage: string` as final parameter
- **State Query Helpers**: Do NOT need actionMessage (read-only operations)
- **Action Message Creation**: Use `createAction('method-name')` with kebab-case matching the action method name
- **Redux DevTools Correlation**: All state mutations from a single action operation show the same message identifier

**Benefits**:

- **Consistency**: Standardized state mutations across all actions
- **Maintainability**: Single place to update common operations
- **Readability**: Actions focus on business logic, not implementation details
- **Testing**: Helper functions can be unit tested independently
- **Debugging**: Easy correlation of related state mutations in Redux DevTools

**Usage in Actions**:

```typescript
// actions/load-data.ts
import { setLoadingStorage, setStorageLoaded, setStorageError } from '../domain-helpers';
import { LogType, logInfo, createAction } from '@teensyrom-nx/utils';

export function loadData(store: WritableStore<DomainState>, service: DomainService) {
  return {
    loadData: async ({ id }: { id: string }): Promise<void> => {
      const actionMessage = createAction('load-data');
      const key = createKey(id);

      logInfo(LogType.Start, `Loading data for ${key}`);
      setLoadingStorage(store, key, actionMessage);

      try {
        const data = await firstValueFrom(service.getData(id));
        logInfo(LogType.Success, `Data loaded for ${key}`);
        setStorageLoaded(store, key, { data }, actionMessage);
      } catch (error) {
        setStorageError(store, key, error.message || 'Failed to load data', actionMessage);
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
import { LogType, logInfo, logError } from '@teensyrom-nx/utils';

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

### Action Composition Rules

**Standard**: Actions should never call other actions directly. Instead, use shared helper functions to reduce logical duplication.

**❌ WRONG - Actions calling other actions**:

```typescript
export function refreshData(store: WritableStore<State>, service: Service) {
  return {
    refreshData: async ({ id }: { id: string }): Promise<void> => {
      // Don't call other store actions directly
      await store.loadData({ id }); // WRONG - action calling action
    },
  };
}
```

**✅ CORRECT - Use shared helper functions**:

```typescript
// helpers/data-helpers.ts
export function loadDataWithService(
  store: WritableStore<State>,
  service: Service,
  id: string,
  actionMessage: string
): Promise<DataType> {
  updateState(store, actionMessage, (state) => ({ isLoading: true, error: null }));

  try {
    const data = await firstValueFrom(service.getData(id));
    updateState(store, actionMessage, (state) => ({
      data,
      isLoading: false,
      isLoaded: true,
      lastUpdateTime: Date.now(),
    }));
    return data;
  } catch (error) {
    updateState(store, actionMessage, (state) => ({
      isLoading: false,
      error: (error as any)?.message || 'Failed to load data',
    }));
    throw error;
  }
}

// actions/load-data.ts
export function loadData(store: WritableStore<State>, service: Service) {
  return {
    loadData: async ({ id }: { id: string }): Promise<void> => {
      const actionMessage = createAction('load-data');
      await loadDataWithService(store, service, id, actionMessage);
    },
  };
}

// actions/refresh-data.ts
export function refreshData(store: WritableStore<State>, service: Service) {
  return {
    refreshData: async ({ id }: { id: string }): Promise<void> => {
      const actionMessage = createAction('refresh-data');
      // Reuse the same helper function, not the action
      await loadDataWithService(store, service, id, actionMessage);
    },
  };
}
```

**Benefits**:

- **Maintainability**: Logic changes only need to be made in helper functions
- **Testability**: Helper functions can be unit tested independently
- **Consistency**: Same logic produces identical behavior across actions
- **Action Message Tracking**: Each action gets proper message correlation
- **Avoid Coupling**: Actions remain independent and focused

**Reference Implementation**: See [`storage-helpers.ts`](../libs/domain/storage/state/src/lib/storage-helpers.ts) for examples of shared helper functions used across multiple actions.

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
