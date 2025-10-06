# Phase 2: Storage Store State & Actions

## 🎯 Objective

Extend the StorageStore with search state management, implementing actions for executing searches and clearing results while maintaining per-device state isolation and following established state standards. This phase builds on Phase 1's infrastructure to provide reactive state management for search operations.

## 📚 Required Reading

- [ ] [Search Plan](./SEARCH_PLAN.md) - Overall feature architecture and implementation strategy
- [ ] [Phase 1 Document](./SEARCH_P1.md) - Infrastructure layer that this phase depends on
- [ ] [State Standards](../../STATE_STANDARDS.md) - NgRx Signal Store patterns, updateState with actionMessage
- [ ] [Store Testing](../../STORE_TESTING.md) - Store testing methodology and behaviors checklist
- [ ] [Domain Standards](../../DOMAIN_STANDARDS.md) - Domain service patterns and dependency injection
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - TypeScript and Angular conventions
- [ ] [Logging Standards](../../LOGGING_STANDARDS.md) - Emoji-enhanced logging for operations

## File Tree

```
libs/application/src/lib/storage/
├── storage-store.ts                    # UPDATED: Add SearchState to StorageState
├── storage-store.spec.ts               # UPDATED: Add search tests
├── storage-helpers.ts                  # UPDATED: Add search helper functions
├── actions/
│   ├── index.ts                        # UPDATED: Export search actions
│   ├── search-files.ts                 # NEW: Search execution action
│   ├── clear-search.ts                 # NEW: Clear search action
│   ├── navigate-to-directory.ts        # UPDATED: Auto-clear search on navigation
│   ├── navigate-directory-backward.ts  # UPDATED: Auto-clear search on navigation
│   ├── navigate-directory-forward.ts   # UPDATED: Auto-clear search on navigation
│   └── navigate-up-one-directory.ts    # UPDATED: Auto-clear search on navigation
└── selectors/
    ├── index.ts                        # UPDATED: Export search selector
    └── get-search-state.ts             # NEW: Search state selector
```

## 📋 Implementation Tasks

### Task 1: Add Search State Models

**Purpose**: Define search state structure that integrates with existing StorageState, maintaining per-device isolation.

**File**: [`libs/application/src/lib/storage/storage-store.ts`](../../../libs/application/src/lib/storage/storage-store.ts)

- [ ] Add `SearchState` interface definition
  - **Properties**:
    - `searchText: string` - Current search text
    - `filterType: PlayerFilterType | null` - Current filter (null if not specified)
    - `results: FileItem[]` - Search results (files only, no directories)
    - `isSearching: boolean` - Loading state during API call
    - `hasSearched: boolean` - Distinguishes "not searched" from "no results"
    - `error: string | null` - Error message from failed search
  - Place before `StorageState` interface
- [ ] Add `searchState` property to `StorageState` interface
  - **Type**: `Record<string, SearchState>` (keyed by deviceId)
  - Place after `navigationHistory` property
  - Per-device isolation - each device has independent search state
- [ ] Update `initialState` constant
  - Add `searchState: {}` property
  - Empty object matches pattern of other per-device records

**Design Notes**:
- `hasSearched` flag prevents confusion between "no search performed" and "search returned no results"
- `filterType` stored to maintain UI state (what filter was used)
- `searchText` stored to repopulate input field if needed
- Per-device isolation prevents search conflicts when multiple devices connected

---

### Task 2: Create search-files Action

**Purpose**: Execute search operation with proper state management, error handling, and Redux DevTools tracking.

**File**: [`libs/application/src/lib/storage/actions/search-files.ts`](../../../libs/application/src/lib/storage/actions/search-files.ts) (NEW)

- [ ] Import required dependencies
  - `updateState` from `@angular-architects/ngrx-toolkit` (REQUIRED)
  - `firstValueFrom` from `rxjs` for observable conversion
  - `createAction, LogType, logInfo, logError` from `@teensyrom-nx/utils`
  - `WritableStore` from `../storage-helpers`
  - `StorageState` from `../storage-store`
  - `IStorageService` from `@teensyrom-nx/domain`
  - `StorageType, PlayerFilterType, FileItem` from `@teensyrom-nx/domain`
- [ ] Define `searchFiles` function signature
  - Parameters: `store: WritableStore<StorageState>`, `storageService: IStorageService`
  - Returns object with single `searchFiles` async method
- [ ] Define method parameters interface
  - `deviceId: string` - Device identifier
  - `storageType: StorageType` - Storage type to search
  - `searchText: string` - Text to search for
  - `filterType?: PlayerFilterType` - Optional filter type
- [ ] Create action message for Redux DevTools tracking
  - Use `createAction('search-files')` at method start
  - Pass to ALL `updateState()` calls for correlation
- [ ] Initialize search state if needed
  - Check if `searchState[deviceId]` exists
  - If not, create initial SearchState with empty results
  - Use `updateState()` with actionMessage
- [ ] Set loading state before API call
  - Update `searchState[deviceId]`: `isSearching: true, error: null`
  - Use `updateState()` with actionMessage
- [ ] Log operation start
  - Use `LogType.Start` with descriptive message including deviceId
- [ ] Call infrastructure service
  - `await firstValueFrom(storageService.search(deviceId, storageType, searchText, filterType))`
  - Service returns `Observable<FileItem[]>`
- [ ] Update state on success
  - Log with `LogType.Success` including result count
  - Update `searchState[deviceId]`:
    - `results`: API result array
    - `searchText`: Parameter value
    - `filterType`: Parameter value (or null if undefined)
    - `isSearching: false`
    - `hasSearched: true`
    - `error: null`
  - Use `updateState()` with same actionMessage
- [ ] Handle errors with proper state updates
  - Log with `logError()`
  - Update `searchState[deviceId]`:
    - `isSearching: false`
    - `error`: Error message
    - Preserve `hasSearched: true` to show "search failed" not "no search"
  - Use `updateState()` with same actionMessage
- [ ] Return Promise<void> for async/await pattern

**Implementation Pattern** (from STATE_STANDARDS.md):
```typescript
export function searchFiles(store: WritableStore<StorageState>, storageService: IStorageService) {
  return {
    searchFiles: async ({ deviceId, storageType, searchText, filterType }: {
      deviceId: string;
      storageType: StorageType;
      searchText: string;
      filterType?: PlayerFilterType;
    }): Promise<void> => {
      const actionMessage = createAction('search-files');
      
      logInfo(LogType.Start, `Starting search for device ${deviceId}`);
      
      // Initialize if needed, set loading, call service, update on success/error
      // ALL updateState() calls use actionMessage parameter
    },
  };
}
```

---

### Task 3: Create clear-search Action

**Purpose**: Clear search state for a device, returning to non-searched state.

**File**: [`libs/application/src/lib/storage/actions/clear-search.ts`](../../../libs/application/src/lib/storage/actions/clear-search.ts) (NEW)

- [ ] Import required dependencies
  - `updateState` from `@angular-architects/ngrx-toolkit` (REQUIRED)
  - `createAction, LogType, logInfo` from `@teensyrom-nx/utils`
  - `WritableStore` from `../storage-helpers`
  - `StorageState` from `../storage-store`
- [ ] Define `clearSearch` function signature
  - Parameters: `store: WritableStore<StorageState>`
  - Returns object with single `clearSearch` method (synchronous)
  - No service needed - just state mutation
- [ ] Define method parameters
  - `deviceId: string` - Device identifier
- [ ] Create action message for Redux DevTools tracking
  - Use `createAction('clear-search')`
- [ ] Reset search state to initial values
  - Update `searchState[deviceId]`:
    - `searchText: ''`
    - `filterType: null`
    - `results: []`
    - `isSearching: false`
    - `hasSearched: false`
    - `error: null`
  - Use `updateState()` with actionMessage
- [ ] Log operation
  - Use `LogType.Info` with message about clearing search for device
- [ ] Return void (synchronous operation)

**Synchronous Pattern**: This is a simple state reset with no async operations or service calls.

---

### Task 4: Auto-clear Search on Directory Navigation

**Purpose**: Automatically clear search when user navigates to a directory, ensuring clean state transitions.

**Files to Update**:
- [`libs/application/src/lib/storage/actions/navigate-to-directory.ts`](../../../libs/application/src/lib/storage/actions/navigate-to-directory.ts)
- [`libs/application/src/lib/storage/actions/navigate-directory-backward.ts`](../../../libs/application/src/lib/storage/actions/navigate-directory-backward.ts)
- [`libs/application/src/lib/storage/actions/navigate-directory-forward.ts`](../../../libs/application/src/lib/storage/actions/navigate-directory-forward.ts)
- [`libs/application/src/lib/storage/actions/navigate-up-one-directory.ts`](../../../libs/application/src/lib/storage/actions/navigate-up-one-directory.ts)

- [ ] Add search state clearing to each navigation action
  - Check if `searchState[deviceId]` exists and has `hasSearched: true`
  - If yes, clear search state before or after navigation logic
  - Use helper function `clearSearchState()` with actionMessage parameter
  - Pass same actionMessage from navigation action for correlation
- [ ] Ensure clearing happens at appropriate point in navigation flow
  - After setting loading state
  - Before or after directory load (doesn't matter which)
  - Important: Use same actionMessage for correlation in Redux DevTools

**Pattern for Integration**:
```typescript
// In each navigation action method:
const actionMessage = createAction('navigate-to-directory'); // or appropriate action name

// Clear search if active
if (hasActiveSearch(store, deviceId)) {
  clearSearchState(store, deviceId, actionMessage);
}

// Continue with existing navigation logic...
```

---

### Task 5: Create Search State Selector

**Purpose**: Expose search state as computed signal for component consumption.

**File**: [`libs/application/src/lib/storage/selectors/get-search-state.ts`](../../../libs/application/src/lib/storage/selectors/get-search-state.ts) (NEW)

- [ ] Import required dependencies
  - `computed` from `@angular/core`
  - `WritableStore` from `../storage-helpers`
  - `StorageState, SearchState` from `../storage-store`
- [ ] Define `getSearchState` function signature
  - Parameter: `store: WritableStore<StorageState>`
  - Returns object with selector method
- [ ] Create parameterized selector factory
  - Method name: `getSearchState`
  - Parameter: `deviceId: string`
  - Returns: `Signal<SearchState | null>`
- [ ] Implement computed signal
  - Access `store.searchState()` signal
  - Return `searchState[deviceId] ?? null`
  - Null indicates no search state for device (never searched)
- [ ] Follow existing selector patterns
  - See `get-selected-directory-state.ts` for reference
  - Same structure: parameterized factory returning computed signal

**Selector Pattern**:
```typescript
export function getSearchState(store: WritableStore<StorageState>) {
  return {
    getSearchState: (deviceId: string) =>
      computed(() => {
        const searchState = store.searchState();
        return searchState[deviceId] ?? null;
      }),
  };
}
```

---

### Task 6: Add Search Helper Functions

**Purpose**: Reusable helper functions for search state mutations and queries.

**File**: [`libs/application/src/lib/storage/storage-helpers.ts`](../../../libs/application/src/lib/storage/storage-helpers.ts)

**State Mutation Helpers** (require actionMessage parameter):

- [ ] `setSearchLoading(store, deviceId, actionMessage): void`
  - Set `isSearching: true, error: null` for device
  - Use `updateState()` with actionMessage
- [ ] `setSearchResults(store, deviceId, results, searchText, filterType, actionMessage): void`
  - Update search state with results and parameters
  - Set `hasSearched: true, isSearching: false, error: null`
  - Use `updateState()` with actionMessage
- [ ] `setSearchError(store, deviceId, errorMessage, actionMessage): void`
  - Set error message, clear loading
  - Preserve `hasSearched: true`
  - Use `updateState()` with actionMessage
- [ ] `clearSearchState(store, deviceId, actionMessage): void`
  - Reset search state to initial values
  - Use `updateState()` with actionMessage

**State Query Helpers** (no actionMessage needed):

- [ ] `getSearchState(store, deviceId): SearchState | undefined`
  - Return search state for device or undefined
  - Read-only operation
- [ ] `hasActiveSearch(store, deviceId): boolean`
  - Return true if `hasSearched` is true for device
  - Convenient check for navigation actions

**Helper Function Rules** (from STATE_STANDARDS.md):
- Mutation helpers MUST accept `actionMessage: string` as final parameter
- Query helpers do NOT need actionMessage (read-only)
- All mutations use `updateState()` not `patchState()`

---

### Task 7: Update Action and Selector Exports

**Purpose**: Expose new actions and selectors through index files for consumption.

**Actions Index**: [`libs/application/src/lib/storage/actions/index.ts`](../../../libs/application/src/lib/storage/actions/index.ts)

- [ ] Import new action functions
  - `import { searchFiles } from './search-files';`
  - `import { clearSearch } from './clear-search';`
- [ ] Add to returned object in `withStorageActions()`
  - Spread operators: `...searchFiles(writableStore, storageService)`
  - `...clearSearch(writableStore)`
- [ ] Maintain alphabetical order for readability

**Selectors Index**: [`libs/application/src/lib/storage/selectors/index.ts`](../../../libs/application/src/lib/storage/selectors/index.ts)

- [ ] Import new selector function
  - `import { getSearchState } from './get-search-state';`
- [ ] Add to returned object in `withStorageSelectors()`
  - Spread operator: `...getSearchState(writableStore)`
- [ ] Maintain alphabetical order

---

### Task 8: Storage Store Testing

**Purpose**: Comprehensive unit tests for search functionality following STORE_TESTING.md methodology.

**File**: [`libs/application/src/lib/storage/storage-store.spec.ts`](../../../libs/application/src/lib/storage/storage-store.spec.ts)

**Test Setup**:
- [ ] Create typed mock for `IStorageService` with `search()` method
  - `type SearchFn = (deviceId: string, storageType: StorageType, searchText: string, filterType?: PlayerFilterType) => Observable<FileItem[]>`
  - `let searchMock: MockedFunction<SearchFn>`
- [ ] Add mock to TestBed providers
  - `{ provide: STORAGE_SERVICE, useValue: { search: (searchMock = vi.fn<SearchFn>()), /* other methods */ } }`

**Test Suites**:

**search-files Action Tests**:
- [ ] Describe block: `describe('search-files', () => {})`
- [ ] Test: Successfully returns search results
  - Mock `searchMock.mockReturnValue(of([mockFile1, mockFile2]))`
  - Call `store.searchFiles({ deviceId, storageType, searchText, filterType })`
  - Assert search state: results array, searchText, filterType, hasSearched=true, isSearching=false, error=null
  - Verify service called with correct parameters
- [ ] Test: Empty search results
  - Mock returns empty array
  - Assert state: empty results array, hasSearched=true, no error
- [ ] Test: Search without filter parameter
  - Call without filterType (undefined)
  - Verify service called with undefined filterType
  - Assert state properly set
- [ ] Test: Search with different filter types
  - Test each PlayerFilterType enum value
  - Verify correct mapping to API filter
- [ ] Test: Network error handling
  - Mock `searchMock.mockReturnValue(throwError(() => new Error('Network error')))`
  - Assert state: error message set, isSearching=false, hasSearched=true
- [ ] Test: Preserves search state across multiple searches
  - Execute search, verify state
  - Execute another search, verify state updated
- [ ] Test: Initializes search state if not exists
  - First search on device should create searchState[deviceId]

**clear-search Action Tests**:
- [ ] Describe block: `describe('clear-search', () => {})`
- [ ] Test: Clears search state for device
  - Setup: Execute search first to populate state
  - Call `store.clearSearch({ deviceId })`
  - Assert state reset: empty results, empty searchText, null filterType, hasSearched=false
- [ ] Test: Safe to call when no search active
  - Call clear on device with no search
  - Should not throw error

**Auto-clear on Navigation Tests**:
- [ ] Test: navigateToDirectory clears active search
  - Setup: Execute search first
  - Call `store.navigateToDirectory({ deviceId, storageType, path })`
  - Assert search state cleared
- [ ] Test: navigateDirectoryBackward clears active search
  - Same pattern
- [ ] Test: navigateDirectoryForward clears active search
  - Same pattern  
- [ ] Test: navigateUpOneDirectory clears active search
  - Same pattern

**Search State Selector Tests**:
- [ ] Test: Returns search state for device
  - Setup search state
  - Call `store.getSearchState(deviceId)()`
  - Assert correct SearchState returned
- [ ] Test: Returns null when no search state exists
  - Call selector for device with no search
  - Assert null returned

**Per-Device Isolation Tests**:
- [ ] Test: Independent search state per device
  - Search on device1
  - Search on device2 with different text
  - Assert device1 and device2 have separate state
- [ ] Test: Clearing search on one device doesn't affect others
  - Setup searches on device1 and device2
  - Clear search on device1
  - Assert device2 search state unchanged

**Follow STORE_TESTING.md Checklist**:
- [ ] Test setup via Angular TestBed
- [ ] Typed service mocks provided via tokens
- [ ] Initialization behavior verified
- [ ] Caching semantics not applicable (search is always fresh)
- [ ] Loading + error handling tested
- [ ] Multi-context isolation verified
- [ ] Edge cases covered (empty results, errors, missing state)

**Helper for Async Tests**:
```typescript
const nextTick = () => new Promise<void>((r) => setTimeout(r, 0));
// After calling async store method:
await nextTick();
```

---

## 🗂️ File Changes

### New Files
- [`libs/application/src/lib/storage/actions/search-files.ts`](../../../libs/application/src/lib/storage/actions/search-files.ts) - Search execution action
- [`libs/application/src/lib/storage/actions/clear-search.ts`](../../../libs/application/src/lib/storage/actions/clear-search.ts) - Clear search action
- [`libs/application/src/lib/storage/selectors/get-search-state.ts`](../../../libs/application/src/lib/storage/selectors/get-search-state.ts) - Search state selector

### Modified Files
- [`libs/application/src/lib/storage/storage-store.ts`](../../../libs/application/src/lib/storage/storage-store.ts) - Add SearchState to StorageState
- [`libs/application/src/lib/storage/storage-store.spec.ts`](../../../libs/application/src/lib/storage/storage-store.spec.ts) - Add search tests
- [`libs/application/src/lib/storage/storage-helpers.ts`](../../../libs/application/src/lib/storage/storage-helpers.ts) - Add search helpers
- [`libs/application/src/lib/storage/actions/index.ts`](../../../libs/application/src/lib/storage/actions/index.ts) - Export search actions
- [`libs/application/src/lib/storage/selectors/index.ts`](../../../libs/application/src/lib/storage/selectors/index.ts) - Export search selector
- [`libs/application/src/lib/storage/actions/navigate-to-directory.ts`](../../../libs/application/src/lib/storage/actions/navigate-to-directory.ts) - Auto-clear search
- [`libs/application/src/lib/storage/actions/navigate-directory-backward.ts`](../../../libs/application/src/lib/storage/actions/navigate-directory-backward.ts) - Auto-clear search
- [`libs/application/src/lib/storage/actions/navigate-directory-forward.ts`](../../../libs/application/src/lib/storage/actions/navigate-directory-forward.ts) - Auto-clear search
- [`libs/application/src/lib/storage/actions/navigate-up-one-directory.ts`](../../../libs/application/src/lib/storage/actions/navigate-up-one-directory.ts) - Auto-clear search

## 🧪 Testing Requirements

### Unit Tests

**search-files Action**:
- [ ] Successful search with multiple results
- [ ] Successful search with empty results
- [ ] Search without filter parameter
- [ ] Search with each PlayerFilterType value
- [ ] Network error handling
- [ ] HTTP error handling
- [ ] State initialization on first search
- [ ] State updates on subsequent searches

**clear-search Action**:
- [ ] Clears search state completely
- [ ] Safe to call when no search active
- [ ] Resets all SearchState properties

**Auto-clear Behavior**:
- [ ] All navigation actions clear active search
- [ ] Navigation with no search doesn't error
- [ ] Clear uses same actionMessage for correlation

**Search State Selector**:
- [ ] Returns correct search state for device
- [ ] Returns null when no search state exists
- [ ] Reactive to state changes

**Per-Device Isolation**:
- [ ] Independent search state per device
- [ ] Search on device1 doesn't affect device2
- [ ] Clear on device1 doesn't affect device2

**Test Coverage Requirements**:
- Minimum 80% line coverage for new code
- 100% coverage for error handling paths
- All state transitions tested
- Edge cases covered (null, undefined, empty)

## ✅ Success Criteria

- [ ] SearchState interface defined with all required properties
- [ ] SearchState integrated into StorageState as per-device record
- [ ] search-files action implemented with proper async/await pattern
- [ ] clear-search action implemented for state reset
- [ ] Auto-clear search on directory navigation actions
- [ ] Search state selector exposes state as computed signal
- [ ] Search helper functions implemented (mutation and query)
- [ ] Action and selector exports updated in index files
- [ ] All store unit tests passing
- [ ] Test coverage meets requirements (80% minimum, 100% error paths)
- [ ] Code follows STATE_STANDARDS.md patterns (updateState with actionMessage)
- [ ] Redux DevTools properly tracks all search operations with actionMessage
- [ ] Logging follows LOGGING_STANDARDS.md patterns (emoji-enhanced)
- [ ] Per-device state isolation maintained
- [ ] No breaking changes to existing storage functionality

## 📝 Notes

### Key State Management Patterns

- **updateState with actionMessage**: ALL state mutations MUST use `updateState()` from `@angular-architects/ngrx-toolkit` with `actionMessage` parameter for Redux DevTools correlation
- **Per-Device Isolation**: Search state keyed by deviceId prevents conflicts in multi-device scenarios
- **Action Message Correlation**: All state mutations from a single action operation use the same actionMessage identifier
- **Helper Function Parameters**: Mutation helpers require actionMessage, query helpers don't

### Design Decisions

- **hasSearched Flag**: Distinguishes "not searched yet" from "searched and got no results" - important for UI messaging
- **filterType Storage**: Storing filterType allows UI to show what filter was applied, useful for user feedback
- **Auto-clear on Navigation**: Prevents stale search results when user navigates directories - clean UX transition
- **Synchronous clear-search**: No service calls needed, just state reset - simple and fast
- **Observable Return from Service**: Infrastructure returns Observable, action converts to Promise with firstValueFrom()

### Critical Requirements from STATE_STANDARDS.md

- **NEVER use patchState()**: Use `updateState()` with actionMessage for ALL mutations
- **Action Message Creation**: Use `createAction('action-name')` at start of each action method
- **Helper Function Rules**: Mutation helpers MUST accept actionMessage parameter, query helpers don't need it
- **Async/Await Pattern**: Use async/await with firstValueFrom() for all service calls
- **Error Preservation**: Use `(error as any)?.message || 'fallback'` to preserve error messages

### Integration with Other Domains

- **Player Domain**: Search results will be used by PlayerContext for file launching (Phase 6)
- **UI Components**: Phase 3 will consume search state through selector signals
- **Storage Domain**: Auto-clear ensures search doesn't interfere with directory navigation

### Testing Strategy

- **TestBed Setup**: Use Angular TestBed to instantiate store as it runs in production
- **Typed Mocks**: Strongly typed service mocks provided via InjectionToken
- **Async Test Helper**: Use nextTick() helper for microtask queue flushing
- **Behavior Testing**: Test observable behavior through public API, not internal implementation

### Future Enhancements (Out of Scope)

- Search result caching/persistence
- Search history tracking
- Debounced search input (handled in UI layer - Phase 4)
- Search suggestions/autocomplete
- Advanced search operators
