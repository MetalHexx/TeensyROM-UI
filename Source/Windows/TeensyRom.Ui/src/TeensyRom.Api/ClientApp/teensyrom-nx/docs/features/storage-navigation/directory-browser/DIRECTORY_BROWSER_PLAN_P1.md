# Phase 1: Storage State Enhancement with NavigationHistory Class

**High Level Plan Documentation**: [Directory Browser Plan](./DIRECTORY_BROWSER_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../../CODING_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../../STORE_TESTING.md)
- **State Standards**: [STATE_STANDARDS.md](../../../STATE_STANDARDS.md)

## 🎯 Objective

Create a dedicated NavigationHistory class and enhance the StorageState interface to support navigation history tracking per device. This phase establishes the foundational state structure needed for browser-like navigation while keeping the StorageState interface clean and focused.

## 📚 Required Reading

- [ ] [STATE_STANDARDS.md](../../../STATE_STANDARDS.md) - State interface design patterns
- [ ] [STORE_TESTING.md](../../../STORE_TESTING.md) - Testing methodology for stores
- [ ] [Existing storage-store.ts](../../../../libs/domain/storage/state/src/lib/storage-store.ts) - Current state structure

## 📋 Implementation Tasks

### Task 1: NavigationHistory Class Definition

**Purpose**: Create a simple data container class for navigation history state following established patterns.

- [ ] Create NavigationHistory class in storage-store.ts with the other interfaces
- [ ] Add history: string[] property for path history
- [ ] Add currentIndex: number property for current position (-1 for empty)
- [ ] Add maxHistorySize: number property (default: 50 entries)
- [ ] Add constructor to initialize properties with defaults
- [ ] Include comprehensive JSDoc comments explaining the class purpose
- [ ] Keep class as simple data container only (no methods)

### Task 2: StorageState Interface Enhancement

**Purpose**: Add navigation history field to the existing StorageState interface using the new NavigationHistory class.

- [ ] Add navigationHistory field as Record<string, NavigationHistory> to StorageState
- [ ] Replace existing storageHistory: Record<string, string[]> with NavigationHistory instances
- [ ] Remove any separate currentHistoryIndex field (now encapsulated in NavigationHistory)
- [ ] Document field purpose with clear JSDoc comments
- [ ] Maintain compatibility with existing StorageState usage

### Task 3: Initial State Configuration

**Purpose**: Configure default values for the new navigation history field.

- [ ] Update initialState object to use navigationHistory: {} instead of storageHistory: {}
- [ ] Verify initial state maintains existing behavior
- [ ] Document the default state behavior
- [ ] Ensure proper initialization when NavigationHistory instances are created

### Task 4: Type Safety and Export Configuration

**Purpose**: Ensure all new types integrate properly with existing TypeScript compilation.

- [ ] Verify TypeScript compilation with enhanced StorageState
- [ ] Check that storage store instantiation works with new NavigationHistory field
- [ ] Export NavigationHistory class from storage state index.ts
- [ ] Ensure no breaking changes to existing storage store consumers
- [ ] Add type exports to public API for NavigationHistory class

## 🗂️ File Changes

- [storage-store.ts](../../../../libs/domain/storage/state/src/lib/storage-store.ts) - Enhanced state interface with NavigationHistory class
- [index.ts](../../../../libs/domain/storage/state/src/index.ts) - Export new NavigationHistory class

## 🧪 Testing Requirements

Following the established behavioral testing patterns from [storage-store.spec.ts](../../../../libs/domain/storage/state/src/lib/storage-store.spec.ts):

### Unit Tests

- [ ] **Store Setup**: NavigationHistory class can be instantiated with correct default values
- [ ] **Property Access**: NavigationHistory properties (history, currentIndex, maxHistorySize) are accessible and typed correctly
- [ ] **State Interface**: StorageState interface accepts new navigationHistory field without breaking existing functionality
- [ ] **Initial State**: Store initializes with navigationHistory: {} and maintains existing behavior
- [ ] **TypeScript Compilation**: Enhanced interface compiles successfully with no type errors
- [ ] **Regression Testing**: All existing storage store functionality remains unaffected

### Integration Tests

- [ ] **Store Instantiation**: Storage store creates successfully with enhanced state structure
- [ ] **State Signals**: State signals provide access to NavigationHistory instances when created
- [ ] **Multi-Device Support**: NavigationHistory instances can be independently managed per deviceId
- [ ] **No Breaking Changes**: Existing components and selectors continue to work unchanged

### Behavioral Test Patterns (for future phases)

Following the established testing approach, future navigation action tests should verify:

- **State Changes**: NavigationHistory state updates correctly after actions complete
- **API Interactions**: Service calls are made appropriately for navigation operations
- **Cache Behavior**: Navigation uses cached data when available, makes API calls when needed
- **Error Handling**: Both success and failure scenarios are handled correctly
- **Multi-Device Independence**: Each device maintains separate navigation history
- **Browser-like Behavior**: Forward history clearing, boundary conditions, etc.

## ✅ Success Criteria

- [ ] NavigationHistory class properly implemented as simple data container
- [ ] StorageState enhanced with navigationHistory field using NavigationHistory class
- [ ] Initial state configured with appropriate defaults for NavigationHistory
- [ ] TypeScript compilation succeeds with no errors
- [ ] No breaking changes to existing storage store functionality
- [ ] NavigationHistory class exported and available for use in storage actions
- [ ] Ready to proceed to Phase 2 (navigation action implementations)

## 📝 Notes

- Keep changes minimal and focused on NavigationHistory class and state structure only
- Do not modify any store actions or logic in this phase
- Ensure NavigationHistory class follows established coding standards
- Navigation history uses device ID as the key for per-device tracking
- NavigationHistory class is simple data container - all logic will be in action files
- Memory management will be handled by action methods following STATE_STANDARDS.md patterns

## 📋 Interface Specifications

### NavigationHistory Class

```typescript
/**
 * Simple data container for navigation history state.
 * All navigation logic will be implemented in action files following STATE_STANDARDS.md patterns.
 */
export class NavigationHistory {
  /**
   * Array of path strings representing the browsing history
   */
  history: string[] = [];

  /**
   * Current position in the history array (-1 indicates empty history)
   */
  currentIndex: number = -1;

  /**
   * Maximum number of history entries to maintain
   */
  maxHistorySize: number;

  constructor(maxHistorySize: number = 50) {
    this.maxHistorySize = maxHistorySize;
  }
}
```

### Enhanced StorageState

```typescript
interface StorageState {
  storageEntries: Record<string, StorageDirectoryState>;
  selectedDirectories: Record<string, SelectedDirectory>;

  // Navigation history using NavigationHistory class instances
  navigationHistory: Record<string, NavigationHistory>; // key: deviceId
}
```

### Example State Structure

```typescript
// Example of how the enhanced state would look
{
  storageEntries: { ... },
  selectedDirectories: { ... },
  navigationHistory: {
    "device-1": new NavigationHistory(), // Contains history for device-1
    "device-2": new NavigationHistory()  // Contains history for device-2
  }
}

// Example NavigationHistory state:
// device-1: { history: ["/", "/games", "/music"], currentIndex: 2, maxHistorySize: 50 }
// device-2: { history: ["/"], currentIndex: 0, maxHistorySize: 50 }
```
