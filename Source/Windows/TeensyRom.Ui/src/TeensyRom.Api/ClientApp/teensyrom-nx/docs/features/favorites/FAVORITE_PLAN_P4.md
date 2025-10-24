# Phase 4: Application Layer - Favorite Store Actions

## 🎯 Objective

Create application layer store actions for favorite operations in the `StorageStore`, integrating infrastructure services with state management. These actions will enable UI components to save and remove favorites while maintaining consistent state across the application.

**Deliverable**: Two async store actions (`saveFavorite` and `removeFavorite`) that call infrastructure services, manage loading/error states, and update file favorite status in the currently loaded directory.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [Favorite Feature Plan](./FAVORITE_PLAN.md) - High-level feature plan and context
- [ ] [Phase 3 Documentation](./FAVORITE_PLAN_P3.md) - Infrastructure layer implementation

**Standards & Guidelines:**
- [ ] [State Standards](../../STATE_STANDARDS.md) - NgRx Signal Store patterns with async/await
- [ ] [Store Testing Guide](../../STORE_TESTING.md) - Behavioral testing methodology
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - General testing approaches

**Reference Implementations:**
- [ ] [storage-store.ts](../../../libs/application/src/lib/storage/storage-store.ts) - Existing store structure
- [ ] [initialize-storage.ts](../../../libs/application/src/lib/storage/actions/initialize-storage.ts) - Action pattern example
- [ ] [storage-helpers.ts](../../../libs/application/src/lib/storage/storage-helpers.ts) - Helper function patterns
- [ ] [storage-store.spec.ts](../../../libs/application/src/lib/storage/storage-store.spec.ts) - Behavioral testing patterns

---

## 📂 File Structure Overview

> Files to be modified and created during Phase 4 implementation.

```
libs/application/src/lib/storage/
├── storage-store.ts                          📝 Modified - Add favoriteOperationsState to StorageState
├── storage-helpers.ts                        📝 Modified - Add favorite helper functions
├── actions/
│   ├── index.ts                              📝 Modified - Export new favorite actions
│   ├── save-favorite.ts                      ✨ New - Save favorite action
│   └── remove-favorite.ts                    ✨ New - Remove favorite action
└── storage-store.spec.ts                     📝 Modified - Add behavioral tests for favorite actions
```

---

## 📋 Implementation Guidelines

<details open>
<summary><h3>Task 1: Extend Storage State Interface</h3></summary>

**Purpose**: Add state properties to track favorite operation loading states and errors globally across all devices and storage types.

**Related Documentation:**
- [StorageState interface](../../../libs/application/src/lib/storage/storage-store.ts#L49) - Current state structure
- [State Standards - State Structure](../../STATE_STANDARDS.md#state-structure-organization) - State organization patterns

**Implementation Subtasks:**
- [ ] **Add `FavoriteOperationsState` interface** to `storage-store.ts` with properties:
  - `isProcessing: boolean` - Global loading flag for both save and remove operations
  - `error: string | null` - Error message from failed favorite operations
- [ ] **Add `favoriteOperationsState` property** to `StorageState` interface with type `FavoriteOperationsState`
- [ ] **Update `initialState`** to include initial `favoriteOperationsState` object with all flags set to false/null

**Testing Subtask:**
- [ ] **Write Tests**: Test store initialization includes favoriteOperationsState (see Testing section)

**Key Implementation Notes:**
- Use single global loading flag since UI has one toggle button that performs both operations
- Error state should be cleared when starting a new operation
- Follow existing state interface patterns from `StorageState`

**Critical State Structure** (small snippet for clarity):
```typescript
export interface FavoriteOperationsState {
  isProcessing: boolean;
  error: string | null;
}

export interface StorageState {
  // ... existing properties
  favoriteOperationsState: FavoriteOperationsState;
}
```

**Testing Focus for Task 1:**

> Focus on behavioral testing - observable state initialization

**Behaviors to Test:**
- [ ] **Initial state includes favoriteOperationsState**: Store initializes with favoriteOperationsState object containing expected default values
- [ ] **Default values are correct**: isProcessing is false, error is null

**Testing Reference:**
- See [storage-store.spec.ts](../../../libs/application/src/lib/storage/storage-store.spec.ts#L119) - Store Setup tests

</details>

---

<details open>
<summary><h3>Task 2: Create Save Favorite Action</h3></summary>

**Purpose**: Implement `saveFavorite` action that calls infrastructure service to save a favorite and updates the file's `isFavorite` flag in the currently loaded directory.

**Related Documentation:**
- [initialize-storage.ts](../../../libs/application/src/lib/storage/actions/initialize-storage.ts) - Action pattern with async/await
- [IStorageService.saveFavorite](../../../libs/domain/src/lib/contracts/storage.contract.ts) - Infrastructure service method
- [State Standards - Action Pattern](../../STATE_STANDARDS.md#function-file-structure) - Action implementation pattern

**Implementation Subtasks:**
- [ ] **Create `save-favorite.ts`** file in `actions/` folder
- [ ] **Export `saveFavorite` function** that returns object with `saveFavorite` async method
- [ ] **Accept parameters**: `deviceId: string`, `storageType: StorageType`, `filePath: string`
- [ ] **Create action message** using `createAction('save-favorite')` for Redux DevTools tracking
- [ ] **Set loading state** using helper function at operation start (set `isProcessing: true`, clear `error`)
- [ ] **Call infrastructure service** `storageService.saveFavorite()` with parameters using `firstValueFrom()`
- [ ] **Update file in directory** on success - find file in current directory and set `isFavorite: true`
- [ ] **Clear loading state** on success (set `isProcessing: false`)
- [ ] **Handle errors** by setting error state with message, clearing loading flag
- [ ] **Use `updateState` with `actionMessage`** for ALL state mutations (not `patchState`)

**Testing Subtask:**
- [ ] **Write Tests**: Test save favorite behaviors (see Testing section below)

**Key Implementation Notes:**
- Infrastructure service already displays success/error alerts via alert service integration
- Only update the file's `isFavorite` flag in the CURRENT directory (identified by `StorageKey`)
- Use `updateFileInDirectory` helper function (created in Task 4) to update file flag
- Follow async/await pattern with try-catch for error handling
- Use `firstValueFrom()` to convert Observable to Promise
- Pass `actionMessage` to all helper functions that mutate state

**Action Method Signature** (small snippet for clarity):
```typescript
saveFavorite: async ({
  deviceId,
  storageType,
  filePath,
}: {
  deviceId: string;
  storageType: StorageType;
  filePath: string;
}): Promise<void>
```

**Testing Focus for Task 2:**

> Focus on behavioral testing - observable outcomes from save favorite operation

**Behaviors to Test:**
- [ ] **Loading state appears during operation**: `isProcessing` is true during API call, false after completion
- [ ] **Success updates file flag**: File's `isFavorite` flag changes from false to true in current directory
- [ ] **Success clears error state**: Error is null after successful save
- [ ] **Infrastructure service called correctly**: Mock verifies correct parameters passed to `saveFavorite()`
- [ ] **Error sets error state**: Failed API call sets error message and clears loading flag
- [ ] **Error preserves existing state**: Failed operation doesn't corrupt directory or file data
- [ ] **File not in directory is no-op**: Attempting to update file not in current directory doesn't cause errors
- [ ] **Multi-device isolation**: Saving favorite for one device doesn't affect other devices' state

**Testing Reference:**
- See [storage-store.spec.ts - initializeStorage tests](../../../libs/application/src/lib/storage/storage-store.spec.ts#L157) for async action testing patterns
- See [storage-store.spec.ts - error handling](../../../libs/application/src/lib/storage/storage-store.spec.ts#L242) for error scenario patterns

</details>

---

<details open>
<summary><h3>Task 3: Create Remove Favorite Action</h3></summary>

**Purpose**: Implement `removeFavorite` action that calls infrastructure service to remove a favorite, updates the file's `isFavorite` flag, and optionally removes the file from directory listing if viewing favorites directory.

**Related Documentation:**
- [IStorageService.removeFavorite](../../../libs/domain/src/lib/contracts/storage.contract.ts) - Infrastructure service method
- [State Standards - Action Pattern](../../STATE_STANDARDS.md#function-file-structure) - Action implementation pattern

**Implementation Subtasks:**
- [ ] **Create `remove-favorite.ts`** file in `actions/` folder
- [ ] **Export `removeFavorite` function** that returns object with `removeFavorite` async method
- [ ] **Accept parameters**: `deviceId: string`, `storageType: StorageType`, `filePath: string`
- [ ] **Create action message** using `createAction('remove-favorite')` for Redux DevTools tracking
- [ ] **Set loading state** using helper function (set `isProcessing: true`, clear `error`)
- [ ] **Call infrastructure service** `storageService.removeFavorite()` with parameters using `firstValueFrom()`
- [ ] **Update or remove file** on success:
  - If current directory path starts with `/favorites/`, remove file from directory listing
  - Otherwise, find file and set `isFavorite: false`
- [ ] **Clear loading state** on success (set `isProcessing: false`)
- [ ] **Handle errors** by setting error state with message, clearing loading flag
- [ ] **Use `updateState` with `actionMessage`** for ALL state mutations

**Testing Subtask:**
- [ ] **Write Tests**: Test remove favorite behaviors (see Testing section below)

**Key Implementation Notes:**
- Check if current directory path starts with `/favorites/` to determine removal vs flag update
- Use `removeFileFromDirectory` helper (created in Task 4) to remove file from listing
- Use `updateFileInDirectory` helper to update flag when not in favorites directory
- Infrastructure service already displays success/error alerts
- Follow same async/await and error handling patterns as save action

**Action Method Signature** (small snippet for clarity):
```typescript
removeFavorite: async ({
  deviceId,
  storageType,
  filePath,
}: {
  deviceId: string;
  storageType: StorageType;
  filePath: string;
}): Promise<void>
```

**Testing Focus for Task 3:**

> Focus on behavioral testing - observable outcomes from remove favorite operation

**Behaviors to Test:**
- [ ] **Loading state appears during operation**: `isProcessing` is true during API call, false after completion
- [ ] **Success updates file flag in normal directory**: File's `isFavorite` flag changes from true to false
- [ ] **Success removes file from favorites directory**: When viewing `/favorites/games`, file is removed from directory listing
- [ ] **Success clears error state**: Error is null after successful remove
- [ ] **Infrastructure service called correctly**: Mock verifies correct parameters passed to `removeFavorite()`
- [ ] **Error sets error state**: Failed API call sets error message and clears loading flag
- [ ] **Error preserves existing state**: Failed operation doesn't corrupt directory or file data
- [ ] **File not in directory is no-op**: Attempting to update file not in current directory doesn't cause errors
- [ ] **Multi-device isolation**: Removing favorite for one device doesn't affect other devices' state

**Testing Reference:**
- See [storage-store.spec.ts - navigateToDirectory](../../../libs/application/src/lib/storage/storage-store.spec.ts#L279) for directory update patterns
- See [storage-store.spec.ts - error handling](../../../libs/application/src/lib/storage/storage-store.spec.ts#L339) for error scenarios

</details>

---

<details open>
<summary><h3>Task 4: Add Helper Functions</h3></summary>

**Purpose**: Create reusable helper functions to update file favorite flags and remove files from directory listings, following established helper patterns.

**Related Documentation:**
- [storage-helpers.ts](../../../libs/application/src/lib/storage/storage-helpers.ts) - Existing helper functions
- [State Standards - Helper Utilities](../../STATE_STANDARDS.md#helper-utilities) - Helper function patterns

**Implementation Subtasks:**
- [ ] **Add `updateFileInDirectory` helper function** to `storage-helpers.ts`:
  - Accept `store`, `key`, `filePath`, `updates` (partial FileItem), and `actionMessage` parameters
  - Retrieve storage entry by key
  - Find file in directory's files array by path
  - Update file with provided updates (e.g., `{ isFavorite: true }`)
  - Use `updateState` with `actionMessage` to mutate state
- [ ] **Add `removeFileFromDirectory` helper function** to `storage-helpers.ts`:
  - Accept `store`, `key`, `filePath`, and `actionMessage` parameters
  - Retrieve storage entry by key
  - Filter out file from directory's files array
  - Use `updateState` with `actionMessage` to mutate state
- [ ] **Add `setFavoriteOperationLoading` helper function**:
  - Accept `store` and `actionMessage` parameters
  - Set `isProcessing: true`, clear error
- [ ] **Add `clearFavoriteOperationLoading` helper function**:
  - Accept `store` and `actionMessage` parameters
  - Set `isProcessing: false`
- [ ] **Add `setFavoriteOperationError` helper function**:
  - Accept `store`, `errorMessage`, and `actionMessage` parameters
  - Set error, clear `isProcessing` flag

**Testing Subtask:**
- [ ] **Write Tests**: Helper functions are tested through action behaviors (no isolated helper tests needed)

**Key Implementation Notes:**
- All helper functions MUST accept `actionMessage` as final parameter for Redux DevTools tracking
- Use `updateState` (not `patchState`) for all state mutations
- Handle missing entries gracefully (check for undefined before accessing)
- Follow immutable update patterns for nested state updates
- Use existing helpers like `getStorage()` to retrieve entries

**Helper Function Signatures** (small snippet for clarity):
```typescript
export function updateFileInDirectory(
  store: WritableStore<StorageState>,
  key: StorageKey,
  filePath: string,
  updates: Partial<FileItem>,
  actionMessage: string
): void;

export function removeFileFromDirectory(
  store: WritableStore<StorageState>,
  key: StorageKey,
  filePath: string,
  actionMessage: string
): void;
```

**Testing Focus for Task 4:**

> Helper functions are tested through action behaviors - no isolated unit tests needed

**Behaviors Validated Through Actions:**
- [ ] **File flag updates correctly**: Actions successfully update `isFavorite` flag via helper
- [ ] **File removal works correctly**: Actions successfully remove files from directory via helper
- [ ] **Missing entries handled gracefully**: Helpers don't throw when entry or file not found
- [ ] **State immutability preserved**: Helper mutations create new objects, don't mutate existing

**Testing Reference:**
- Helpers are validated through action tests - see Tasks 2 and 3 testing sections

</details>

---

<details open>
<summary><h3>Task 5: Export Actions from Index</h3></summary>

**Purpose**: Export new favorite actions from actions index file to make them available on the store instance.

**Related Documentation:**
- [actions/index.ts](../../../libs/application/src/lib/storage/actions/index.ts) - Actions export pattern

**Implementation Subtasks:**
- [ ] **Import `saveFavorite`** from `./save-favorite`
- [ ] **Import `removeFavorite`** from `./remove-favorite`
- [ ] **Spread both actions** into the return object of `withStorageActions()`
- [ ] **Verify TypeScript compilation** succeeds with new exports

**Testing Subtask:**
- [ ] **Write Tests**: Verify store exposes new action methods (see Testing section)

**Key Implementation Notes:**
- Follow existing pattern from actions index file
- Both actions share the same `storageService` injection from `withStorageActions`
- No additional service parameters needed for favorite actions

**Testing Focus for Task 5:**

> Verify store API surface includes new methods

**Behaviors to Test:**
- [ ] **Store exposes saveFavorite method**: `typeof store.saveFavorite === 'function'`
- [ ] **Store exposes removeFavorite method**: `typeof store.removeFavorite === 'function'`

**Testing Reference:**
- See [storage-store.spec.ts - Store Setup](../../../libs/application/src/lib/storage/storage-store.spec.ts#L131) for method exposure tests

</details>

---

## 🗂️ Files Modified or Created

> Full relative paths from project root for all files changed in this phase.

**New Files:**
- `libs/application/src/lib/storage/actions/save-favorite.ts`
- `libs/application/src/lib/storage/actions/remove-favorite.ts`

**Modified Files:**
- `libs/application/src/lib/storage/storage-store.ts`
- `libs/application/src/lib/storage/storage-helpers.ts`
- `libs/application/src/lib/storage/actions/index.ts`
- `libs/application/src/lib/storage/storage-store.spec.ts`

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
> - **Favor behavioral testing** - test observable outcomes, not implementation
> - **Test as you go** - tests are integrated into each task's subtasks
> - **Test through public APIs** - test store methods, not internal helpers
> - **Mock at boundaries** - mock infrastructure service, use real store and helpers

> **Reference Documentation:**
> - **All tasks**: [Testing Standards](../../TESTING_STANDARDS.md) - Core behavioral testing approach
> - **Store testing**: [Store Testing](../../STORE_TESTING.md) - Store-specific patterns
> - **Existing tests**: [storage-store.spec.ts](../../../libs/application/src/lib/storage/storage-store.spec.ts) - Patterns to follow

### Where Tests Are Written

**Tests are embedded in each task above** with:
- **Testing Subtask**: Checkbox in task's subtask list
- **Testing Focus**: "Behaviors to Test" section listing observable outcomes
- **Testing Reference**: Links to relevant testing documentation

**Complete each task's testing subtask before moving to the next task.**

### Test Setup Requirements

**TestBed Configuration:**
- Provide `StorageStore` as subject under test
- Mock `STORAGE_SERVICE` token with typed mock functions:
  - `saveFavorite: MockedFunction<(deviceId, storageType, filePath) => Observable<FileItem>>`
  - `removeFavorite: MockedFunction<(deviceId, storageType, filePath) => Observable<void>>`
- All other infrastructure methods mocked (getDirectory, search, etc.)

**Mock Control:**
- Use `vi.fn()` for typed mock functions
- Control return values with `mockReturnValue(of(...))` for success
- Control errors with `mockReturnValue(throwError(() => new Error(...)))`
- Verify calls with `expect(mock).toHaveBeenCalledWith(...)`

**Async Handling:**
- Use `await` for all async store method calls
- No additional flush needed - async/await handles promises directly

### Test Organization by Feature

**Describe Blocks:**
- `saveFavorite() - Save Favorite Operation` - All save favorite behaviors
- `removeFavorite() - Remove Favorite Operation` - All remove favorite behaviors
- `Favorite Operations - Loading States` - Loading state coordination tests
- `Favorite Operations - Multi-Device Isolation` - Device isolation tests
- `Favorite Operations - Error Handling` - Error scenario tests

### Test Execution Commands

**Running Tests:**
```bash
# Run storage store tests
npx nx test application

# Run tests in watch mode during development
npx nx test application --watch

# Run all application tests
npx nx run-many --target=test --projects=application
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Code follows [State Standards](../../STATE_STANDARDS.md)
- [ ] All state mutations use `updateState` with `actionMessage` (not `patchState`)
- [ ] Actions follow async/await pattern with `firstValueFrom()`

**State Management Requirements:**
- [ ] `FavoriteOperationsState` interface added to `storage-store.ts`
- [ ] `favoriteOperationsState` property added to `StorageState` interface
- [ ] Initial state includes `favoriteOperationsState` with correct defaults
- [ ] Loading states managed correctly during operations
- [ ] Error states set on failures, cleared on success
- [ ] File `isFavorite` flags update correctly after operations

**Action Implementation Requirements:**
- [ ] `saveFavorite` action created in `save-favorite.ts`
- [ ] `removeFavorite` action created in `remove-favorite.ts`
- [ ] Both actions exported from `actions/index.ts`
- [ ] Both actions use `createAction()` for Redux DevTools tracking
- [ ] Infrastructure service called with correct parameters
- [ ] Actions handle success and error paths correctly
- [ ] File updates work for normal directories
- [ ] File removal works for favorites directories

**Helper Function Requirements:**
- [ ] `updateFileInDirectory` helper added to `storage-helpers.ts`
- [ ] `removeFileFromDirectory` helper added to `storage-helpers.ts`
- [ ] Loading state helpers added for favorite operations
- [ ] All helpers accept `actionMessage` as final parameter
- [ ] Helpers use `updateState` for all mutations
- [ ] Helpers handle missing entries gracefully

**Testing Requirements:**
- [ ] All testing subtasks completed within each task
- [ ] All behavioral test checkboxes verified
- [ ] Tests written alongside implementation (not deferred)
- [ ] All tests passing with no failures
- [ ] Test coverage meets project standards
- [ ] Tests follow patterns from existing `storage-store.spec.ts`
- [ ] Mock infrastructure service at boundary
- [ ] Tests focus on observable behaviors, not implementation

**Quality Checks:**
- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors (`npm run lint`)
- [ ] Code formatting is consistent
- [ ] No console errors when running tests
- [ ] Redux DevTools shows action messages correctly

**Integration Checks:**
- [ ] Store exposes `saveFavorite` method
- [ ] Store exposes `removeFavorite` method
- [ ] Actions integrate correctly with existing store state
- [ ] No regressions in existing store functionality
- [ ] Multi-device state isolation preserved

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Code reviewed and approved (if applicable)
- [ ] Ready to proceed to Phase 5 (UI implementation)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

**Single Global Loading State**: Using a single `isProcessing` flag for both save and remove operations keeps implementation simple. Since the UI uses one toggle button that performs either save or remove based on current favorite status, only one operation can be in progress at a time. This simplifies both state management and UI implementation.

**No Optimistic Updates**: State updates occur only after successful API response. This prevents inconsistent state if operations fail and aligns with the existing storage store pattern where loading states provide user feedback during operations.

**Current Directory Updates Only**: The action updates the file's `isFavorite` flag only in the currently loaded directory (identified by `StorageKey`). This keeps the implementation simple and matches the user's immediate context. When users navigate to other directories containing the same file, the updated flag will be reflected when those directories are loaded from the API.

**Favorites Directory Handling**: The `removeFavorite` action checks if the current directory path starts with `/favorites/` to determine whether to remove the file from the listing or just update its flag. This provides immediate feedback when unfavoriting from within the favorites directory view.

**Infrastructure Service Integration**: The Phase 3 infrastructure service already handles success/error alert notifications via `alert.service.ts`, so store actions don't need to display alerts. Actions only manage state and delegate API calls to the infrastructure layer.

**Error Handling Strategy**: Failed operations set a global error state that UI components can display. Errors don't corrupt existing state - the file's `isFavorite` flag and directory listings remain unchanged on failure.

### Implementation Constraints

**Action Message Requirement**: ALL state mutations must use `updateState` with `actionMessage` parameter for Redux DevTools correlation. This is a critical requirement from [State Standards](../../STATE_STANDARDS.md#critical-state-mutation-requirement) and enables debugging of complex state flows.

**Async/Await Pattern**: Actions must use async/await with `firstValueFrom()` to convert infrastructure service Observables to Promises. This provides deterministic Promise resolution and avoids concurrency issues.

**Helper Function Convention**: Helper functions that mutate state MUST accept `actionMessage` as their final parameter. Read-only helper functions (queries) do NOT need `actionMessage`.

**Type Safety**: Use `WritableStore<StorageState>` type for store parameter in actions and helpers. Never use `any` types.

### Future Enhancements

**Batch Favorite Operations**: Currently actions handle one file at a time. Future enhancement could support batch operations for favoriting multiple files simultaneously.

**Favorite Status Sync**: Currently updates only current directory. Future enhancement could update the `isFavorite` flag across ALL loaded storage entries containing the same file path for more comprehensive state synchronization.

**Optimistic UI Updates**: Future enhancement could implement optimistic updates with rollback on error, though current approach prioritizes simplicity and consistency.

**Per-File Loading States**: Future enhancement could track loading state per file to enable more granular UI feedback (e.g., disable specific file's button during operation).

**Undo/Redo Support**: Future enhancement could maintain favorite operation history for undo/redo functionality.

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

</details>

---

## 💡 Implementation Reference Examples

> **Links to existing implementations demonstrating patterns to follow**

**Action Patterns:**
- [initialize-storage.ts](../../../libs/application/src/lib/storage/actions/initialize-storage.ts) - Complete async action with service call, state updates, error handling
- [navigate-to-directory.ts](../../../libs/application/src/lib/storage/actions/navigate-to-directory.ts) - Action with conditional logic and state updates
- [refresh-directory.ts](../../../libs/application/src/lib/storage/actions/refresh-directory.ts) - Simple refresh pattern

**Helper Patterns:**
- [storage-helpers.ts](../../../libs/application/src/lib/storage/storage-helpers.ts) - All helper functions with state mutations and queries
- [setLoadingStorage](../../../libs/application/src/lib/storage/storage-helpers.ts#L13) - Loading state helper
- [updateStorage](../../../libs/application/src/lib/storage/storage-helpers.ts#L33) - Generic update helper
- [getStorage](../../../libs/application/src/lib/storage/storage-helpers.ts#L76) - Query helper (no actionMessage)

**Testing Patterns:**
- [storage-store.spec.ts](../../../libs/application/src/lib/storage/storage-store.spec.ts) - Complete behavioral test suite
- [Store Setup tests](../../../libs/application/src/lib/storage/storage-store.spec.ts#L119) - Initialization and method exposure
- [initializeStorage tests](../../../libs/application/src/lib/storage/storage-store.spec.ts#L157) - Async action success/error scenarios
- [navigateToDirectory tests](../../../libs/application/src/lib/storage/storage-store.spec.ts#L279) - State update validation
- [Multi-device tests](../../../libs/application/src/lib/storage/storage-store.spec.ts#L914) - Device isolation patterns

**Infrastructure Service:**
- [storage.service.ts](../../../libs/infrastructure/src/lib/storage/storage.service.ts) - Infrastructure implementation with alert service integration
- [IStorageService](../../../libs/domain/src/lib/contracts/storage.contract.ts) - Service contract interface

---

## 🎓 Key Patterns Summary

**What to Do:**
- Describe WHAT to implement (method names, parameters, behavior)
- Reference existing implementations as examples
- Use small snippets (2-5 lines) for critical type definitions
- Focus on observable behaviors in testing
- Follow async/await pattern with firstValueFrom
- Use updateState with actionMessage for all mutations
- Test through public store API, not internal helpers

**What NOT to Do:**
- Don't write full implementation code
- Don't use patchState (use updateState)
- Don't test implementation details
- Don't mock application layer components
- Don't skip actionMessage parameter
- Don't use optimistic updates
- Don't update state across all directories (only current)

---

## 📚 Related Documentation

**Primary References:**
- [Favorite Feature Plan](./FAVORITE_PLAN.md) - Overall feature context
- [Phase 3 Documentation](./FAVORITE_PLAN_P3.md) - Infrastructure layer (prerequisites)
- [State Standards](../../STATE_STANDARDS.md) - NgRx Signal Store patterns
- [Store Testing](../../STORE_TESTING.md) - Behavioral testing methodology

**Supporting Documentation:**
- [Testing Standards](../../TESTING_STANDARDS.md) - General testing approaches
- [Coding Standards](../../CODING_STANDARDS.md) - Code style and conventions
