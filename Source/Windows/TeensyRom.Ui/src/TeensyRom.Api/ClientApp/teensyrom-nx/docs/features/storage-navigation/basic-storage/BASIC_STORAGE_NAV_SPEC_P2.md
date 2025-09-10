# Phase 2: State Management Specification

**Related Documentation**: [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

**Standards Documentation**:

- **State Standards**: [`STATE_STANDARD.md`](../../../STATE_STANDARD.md) - NgRx Signal Store patterns.
- **Coding Standards**: [`CODING_STANDARDS.md`](../../../CODING_STANDARDS.md) - Component and TypeScript standards.
- **Testing Standards**: [`TESTING_STANDARDS.md`](../../../TESTING_STANDARDS.md) - Unit, integration, and E2E testing patterns.

## Objective

Implement NgRx Signal Store for multi-device storage navigation state using flat `${deviceId}-${storageType}` key structure following STATE_STANDARD one-function-per-file pattern.

## Prerequisites

- Phase 1 completed: Storage domain services available
- Existing device store functional and accessible
- Understanding of NgRx Signal Store patterns from STATE_STANDARD.md

## Implementation Steps

### Step 1: Create Storage State Library

**Purpose**: Generate Nx domain state library structure

**TDD Approach**: Write tests first that verify library can be imported and store injection works before creating any implementation.

**Standards Reference**: Follow [Nx Library Standards](../../../NX_LIBRARY_STANDARDS.md#domain-state-library) for domain state library creation.

**Quick Commands**:

```bash
cd ClientApp/teensyrom-nx
npx nx generate @nrwl/angular:library --name=state --directory=libs/domain/storage --buildable=false --publishable=false --importPath=@teensyrom-nx/domain/storage/state
```

**Verification**: See [Integration Verification](../../../NX_LIBRARY_STANDARDS.md#integration-verification) section for complete verification steps.

### Step 2: State Structure Design

**Purpose**: Define flat state structure with device-storage keys

**TDD Approach**: Write tests first describing expected state shape, key generation, and state operations before implementing interfaces.

**State Structure**:

```typescript
// Core state interfaces
interface StorageState {
  storageEntries: Record<string, StorageDirectoryState>; // key: "${deviceId}-${storageType}"
  isLoading: boolean;
  error: string | null;
}

interface StorageDirectoryState {
  deviceId: string;
  storageType: TeensyStorageType;
  currentPath: string;
  directory: StorageDirectory | null;
  isLoaded: boolean;
  lastLoadTime: number | null;
}

// Helper types
type StorageKey = `${string}-${TeensyStorageType}`;

const initialState: StorageState = {
  storageEntries: {},
  isLoading: false,
  error: null,
};
```

**File**: `libs/domain/storage/state/src/lib/storage-state.models.ts`

**Key Requirements**:

- Use flat key structure for O(1) device-storage lookups
- Include metadata for cache management and loading states
- Provide type-safe key generation with template literal types
- **Create StorageKeyUtil helper functions** for consistent key creation, parsing, and filtering operations

### Step 3: Store Configuration

**Purpose**: Create NgRx Signal Store following STATE_STANDARD pattern

**TDD Approach**: Write tests first for store initialization, dependency injection, and basic store functionality before implementing store structure.

**Pattern**: Follow existing STATE_STANDARD structure with withMethods spreading individual functions

**File**: `libs/domain/storage/state/src/lib/storage-store.ts`

**Key Requirements**:

- Use `providedIn: 'root'` for dependency injection
- Follow STATE_STANDARD method organization pattern
- Import and spread individual functions
- Include proper devtools integration

### Step 4: Individual functions

**Purpose**: Implement core storage navigation methods as individual functions

**TDD Approach**: Write comprehensive tests first for each method's behavior, including success cases, error scenarios, and edge cases before implementing method logic.

**Pattern**: Each method in its own file, following STATE_STANDARD pattern

**Core Methods Needed**:

- **navigateToDirectory**: Navigate to directory, update current path, and load directory contents
- **refreshDirectory**: Refresh/reload directory contents with cache invalidation
- **initializeStorage**: Initialize storage entries for newly connected device
- **cleanupStorage**: Remove all storage entries when device disconnects

**File Structure**:

```
src/lib/methods/
├── navigate-to-directory.ts  # navigateToDirectory function (combined navigate + load)
├── refresh-directory.ts      # refreshDirectory function
├── initialize-storage.ts     # initializeStorage function
└── cleanup-storage.ts        # cleanupStorage function
```

### Step 5: Method Organization and Barrel Exports

**Purpose**: Follow STATE_STANDARD method organization pattern

**TDD Approach**: Write tests first that verify all methods are correctly exported and accessible through the public API before finalizing export structure.

**File**: `libs/domain/storage/state/src/index.ts`

**Requirements**:

- Export all public interfaces and store
- Follow existing domain library patterns
- Enable clean imports: `import { StorageStore } from '@teensyrom-nx/domain/storage/state'`

### Step 6: Application Integration

**Purpose**: Wire up storage state library following integration standards

**TDD Approach**: Write integration tests first that verify store works correctly in application context before finalizing integration.

**Standards Reference**: Follow [Integration Verification](../../../NX_LIBRARY_STANDARDS.md#integration-verification) checklist for complete integration requirements.

**Key Integration Tasks**:

1. **Store Registration**: Verify `StorageStore` uses `providedIn: 'root'` for automatic dependency injection
2. **Import Path Testing**: Test imports in components: `import { StorageStore } from '@teensyrom-nx/domain/storage/state'`
3. **Build Verification**: Ensure app builds successfully with library integration

**Note**: Cross-store coordination (device-storage integration) will be handled at the component level in Phase 3, as documented in the PlayerViewComponent integration approach.

## Testing Requirements

### Unit Tests

**Files to Create**:

- Individual test files for each function
- `storage-store.spec.ts` - Store initialization and configuration tests
- `storage-state.models.spec.ts` - State interface and type tests

**Storage-Specific Testing Focus**:

- **State Key Logic**: Test `${deviceId}-${storageType}` key generation and flat structure operations
- **Individual Method Behavior**: Each method tested in isolation with proper mocking
- **State Consistency**: Verify methods maintain consistent flat state structure
- **Cross-Method Integration**: Test realistic workflows involving multiple methods
- **Error Handling**: Invalid device IDs, unavailable storage types, network failures

## Success Criteria

### Functional Requirements

- [ ] `StorageStore` manages flat state with `${deviceId}-${storageType}` keys
- [ ] Individual functions follow STATE_STANDARD one-function-per-file pattern
- [ ] All TypeScript compilation passes without errors

### Code Quality Requirements

- [ ] Follows established STATE_STANDARD patterns and file organization
- [ ] Includes comprehensive unit tests written using TDD approach
- [ ] Proper error handling for API failures and edge cases
- [ ] Clean separation between state management and business logic

### Integration Requirements

- [ ] Store can be imported and injected in other libraries
- [ ] Compatible with existing STATE_STANDARD patterns
- [ ] Ready for Phase 3 component integration where device-storage coordination will occur
- [ ] Performance optimized for flat state operations

## File Structure

```
libs/domain/storage/state/src/lib/
├── storage-store.ts              # Main store (like device-store.ts)
├── storage-key.util.ts           # Storage key utility functions
├── methods/
│   ├── navigate-to-directory.ts  # navigateToDirectory function (combined navigate + load)
│   ├── refresh-directory.ts      # refreshDirectory function
│   ├── initialize-storage.ts     # initializeStorage function
│   └── cleanup-storage.ts        # cleanupStorage function
└── index.ts                      # Public API exports
```

## Dependencies

- `@teensyrom-nx/domain/storage/services` - Phase 1 storage services
- `@teensyrom-nx/domain/device/state` - Existing device store
- `@ngrx/signals` - Signal store functionality
- `@angular-architects/ngrx-toolkit` - DevTools integration

## Notes

- This phase creates the state management foundation for Phase 3 component integration
- Flat state structure optimized for performance and future virtual scrolling requirements
- functions follow established STATE_STANDARD patterns for consistency
- Cross-store integration ensures proper device availability validation
