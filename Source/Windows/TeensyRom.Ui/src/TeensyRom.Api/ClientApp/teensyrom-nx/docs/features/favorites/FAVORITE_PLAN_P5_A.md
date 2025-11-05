# Phase 5A: Fix Favorite Button UI State Synchronization

## 🎯 Objective

Fix the issue where the favorite button icon does not update when toggling favorite status. The root cause is that `PlayerStore` maintains snapshots of `FileItem` data in both `currentFile.file` and `fileContext.files[]`, which become stale when `StorageStore` updates favorite status. This phase implements a **pessimistic update strategy** to synchronize player state after successful storage operations.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check boxes as you read.

**Feature Documentation:**

- [x] [Planning Doc](./FAVORITE_PLAN.md) - High planning level doc.
- [x] [Phase 5 Implementation](./FAVORITE_PLAN_P5.md) - Original favorite button implementation
- [x] [Player Store Structure](../../../libs/application/src/lib/player/player-store.ts) - PlayerState interfaces

**Standards & Guidelines:**

- [x] [State Management Standards](../../STATE_STANDARDS.md) - Store action patterns and async/await conventions
- [x] [Store Testing Standards](../../STORE_TESTING.md) - Behavioral testing methodology for application layer
- [x] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns
- [x] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches

---

## 📂 File Structure Overview

```
libs/application/src/lib/player/
├── player-store.ts                                      📝 Modified - Export action
├── player-context.interface.ts                          📝 Modified - Add method signature
├── player-context.service.ts                            📝 Modified - Implement method
├── player-context-favorite.service.spec.ts              ✨ New - Behavioral tests
└── actions/
    ├── index.ts                                         📝 Modified - Export new action
    └── update-favorite-status.ts                        ✨ New - Favorite status update action

libs/features/player/.../player-toolbar-actions/
├── player-toolbar-actions.component.ts                  📝 Modified - Async method
└── player-toolbar-actions.component.spec.ts             📝 Modified - Update tests
```

---

## 🔍 Problem Analysis

### Current Flow (Broken)

1. User clicks favorite button
2. Component calls `StorageStore.saveFavorite()` or `StorageStore.removeFavorite()`
3. `StorageStore` updates directory files with new `isFavorite` value
4. **PlayerStore snapshots remain unchanged**:
   - `currentFile.file.isFavorite` - stale
   - `fileContext.files[].isFavorite` - stale (if file exists in loaded directory)
5. Component reads old value from `PlayerStore`
6. Icon does not update despite operation succeeding

### Root Cause

`PlayerStore` maintains **snapshots** of `FileItem` data in two places:

- `currentFile.file` - The currently playing/launched file
- `fileContext.files[]` - The directory context (loaded when file launches)

When `StorageStore` updates favorite status, these snapshots are not synchronized automatically. There is no propagation mechanism from `StorageStore` to `PlayerStore`.

### Solution Strategy

Use **pessimistic updates**: wait for storage operation success, then update player state. The update action must handle **both** snapshot locations:

- Update `currentFile.file.isFavorite` if path matches
- Update matching file in `fileContext.files[]` if context is loaded

---

<details open>
<summary><h2>📋 Implementation Tasks</h2></summary>

<details open>
<summary><h3>Task 1: Add PlayerStore Action</h3></summary>

**Purpose**: Create store action to update `isFavorite` flag in both `currentFile` and `fileContext.files[]` after successful storage operation.

**Related Documentation:**

- [State Standards - Action Patterns](../../STATE_STANDARDS.md#function-organization) - Store action structure
- [State Standards - updateState Requirement](../../STATE_STANDARDS.md#critical-state-mutation-requirement) - Must use updateState with actionMessage

**Implementation Subtasks:**

- [x] **Create Action File**: Create `libs/application/src/lib/player/actions/update-favorite-status.ts`
- [x] **Define Function Signature**: Accept `deviceId`, `filePath`, `isFavorite` parameters
- [x] **Implement State Update Logic**: Update both `currentFile` and `fileContext.files[]` where path matches
- [x] **Add Redux DevTools Integration**: Use `updateState()` with `actionMessage` from `createAction()`
- [x] **Add Logging**: Log operation with file path and new status for debugging
- [x] **Export Action**: Add to `libs/application/src/lib/player/actions/index.ts` in `withPlayerActions()`

**Testing Subtask:**

- [x] **Write Tests**: Test behaviors for this action (see Testing Focus below)

**Key Implementation Notes:**

- **Must use `updateState()` with `actionMessage`** - NOT `patchState()` (Redux DevTools requirement)
- **Update currentFile**: Only if exists and path matches (guard against stale calls)
- **Update fileContext.files**: Map over array, update matching file immutably
- **Early return**: No-op if player doesn't exist or currentFile is null
- **Immutable updates**: Create new objects at each level of nesting

**Critical Interfaces**:

```typescript
// Action parameters
interface UpdateFavoriteStatusParams {
  deviceId: string;
  filePath: string;
  isFavorite: boolean;
}

// PlayerStore state structure (reference only)
interface DevicePlayerState {
  currentFile: LaunchedFile | null; // Contains file: FileItem
  fileContext: PlayerFileContext | null; // Contains files: FileItem[]
  // ... other properties
}
```

**Testing Focus for Task 1:**

**Behaviors to Test:**

- [x] **Updates currentFile**: `isFavorite` flag updates when path matches current file
- [x] **Updates fileContext files**: Matching file in `files[]` array updates
- [x] **Updates both simultaneously**: If file is both current and in context, both update
- [x] **No-op for null currentFile**: Returns unchanged state when no file launched
- [x] **No-op for path mismatch**: Returns unchanged state when path doesn't match
- [x] **Preserves other file properties**: Only `isFavorite` changes, other fields intact
- [x] **Per-device isolation**: Updates only specified device, others unchanged

**Testing Reference:**

- See [Store Testing](../../STORE_TESTING.md) - Behavioral testing patterns
- See [State Standards](../../STATE_STANDARDS.md) - Action testing examples

</details>

---

<details open>
<summary><h3>Task 2: Expose Action via PlayerContext</h3></summary>

**Purpose**: Add public method to `IPlayerContext` interface and implement in `PlayerContextService` to expose the store action.

**Related Documentation:**

- [Player Context Interface](../../../libs/application/src/lib/player/player-context.interface.ts) - Interface definition
- [Player Context Service](../../../libs/application/src/lib/player/player-context.service.ts) - Service implementation

**Implementation Subtasks:**

- [x] **Add Interface Method**: Add `updateCurrentFileFavoriteStatus(deviceId, filePath, isFavorite)` to `IPlayerContext`
- [x] **Implement Service Method**: Delegate to `this.store.updateCurrentFileFavoriteStatus()`
- [x] **Add JSDoc Comment**: Document purpose, when to call, and pessimistic pattern

**Testing Subtask:**

- [x] **Write Tests**: Test behaviors for this method (see Testing Focus below)

**Key Implementation Notes:**

- **Simple delegation**: Method just calls through to store action
- **Synchronous**: No async needed - store action handles state immediately
- **No additional logic**: Validation happens in store action

**Testing Focus for Task 2:**

**Behaviors to Test:**

- [x] **Calls store action**: Verify store method called with correct parameters
- [x] **Signal updates**: Verify `getCurrentFile()` signal reflects updated state
- [x] **Signal updates fileContext**: Verify `getFileContext()` signal reflects updated files array

**Testing Reference:**

- See [Store Testing](../../STORE_TESTING.md) - Facade testing patterns

</details>

---

<details open>
<summary><h3>Task 3: Update Component to Use Pessimistic Pattern</h3></summary>

**Purpose**: Modify `toggleFavorite()` method to await storage operation before updating player state.

**Related Documentation:**

- [Player Toolbar Actions Component](../../../libs/features/player/.../player-toolbar-actions/player-toolbar-actions.component.ts) - Component file

**Implementation Subtasks:**

- [x] **TypeScript Changes**: Change `toggleFavorite(): void` to `async toggleFavorite(): Promise<void>`
- [x] **Await Storage Operation**: Use `await` on `storageStore.saveFavorite()` and `removeFavorite()` calls
- [x] **Call Player Context Update**: Call `playerContext.updateCurrentFileFavoriteStatus()` after storage succeeds
- [ ] **Error Handling**: Wrap in try-catch (optional) - infrastructure already shows alerts

**Testing Subtask:**

- [x] **Write Tests**: Test behaviors for async method (see Testing Focus below)

**Key Implementation Notes:**

- **Button already disables**: `isFavoriteOperationInProgress()` binding handles disabled state
- **No manual state tracking**: Storage operation manages `isProcessing` automatically
- **Error flow**: If storage throws, update never called - icon stays unchanged
- **Success flow**: Storage completes → update called → icon updates

**Testing Focus for Task 3:**

**Behaviors to Test:**

- [x] **Calls storage first**: Verify `storageStore.saveFavorite()` called before player update
- [x] **Calls player update on success**: Verify player context method called after storage succeeds
- [x] **Correct parameters**: Verify deviceId, filePath, and isFavorite passed correctly
- [x] **No update on error**: If storage throws, player context NOT called
- [x] **Async handling**: Test properly awaits operations

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) - Component testing approaches
- See [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) - Smart component patterns

</details>

---

<details open>
<summary><h3>Task 4: Update Component Unit Tests</h3></summary>

**Purpose**: Update existing component tests to handle async method and test pessimistic update flow.

**Related Documentation:**

- [Player Toolbar Actions Spec](../../../libs/features/player/.../player-toolbar-actions.component.spec.ts) - Test file

**Implementation Subtasks:**

- [x] **Make Tests Async**: Add `async` keyword to test functions calling `toggleFavorite()`
- [x] **Await Method Calls**: Add `await` before `component.toggleFavorite()` calls
- [ ] **Add Async Helper**: Add `nextTick()` helper for microtask flushing
- [x] **Update Existing Assertions**: Verify storage and player context call order
- [x] **Add New Test Scenarios**: Test success path, error path, parameter passing

**Testing Subtask:**

- [ ] **Verify All Tests Pass**: Run tests and confirm no failures

**Key Implementation Notes:**

- **4 Toggle Favorite tests**: Add async/await, verify call order
- **8 Template Integration tests**: Update any that trigger toggleFavorite
- **Mock both services**: Mock `StorageStore` and `IPlayerContext` (PLAYER_CONTEXT token)

**Testing Focus for Task 4:**

**Test Scenarios to Add:**

- [ ] **Success path - save**: Storage succeeds → player context called with `isFavorite: true`
- [ ] **Success path - remove**: Storage succeeds → player context called with `isFavorite: false`
- [ ] **Error path**: Storage throws → player context NOT called
- [ ] **Parameter validation**: Correct deviceId, filePath, isFavorite values passed

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) - Unit testing approaches

</details>

---

<details open>
<summary><h3>Task 5: Add PlayerContext Behavioral Tests</h3></summary>

**Purpose**: Create comprehensive behavioral tests for favorite status synchronization following store testing standards.

**Related Documentation:**

- [Player Context History Tests](../../../libs/application/src/lib/player/player-context-history.service.spec.ts) - Reference pattern
- [Store Testing](../../STORE_TESTING.md) - Behavioral testing methodology

**Implementation Subtasks:**

- [x] **Create Test File**: Create `libs/application/src/lib/player/player-context-favorite.service.spec.ts`
- [x] **Setup Test Infrastructure**: Configure TestBed with real PlayerStore, mocked infrastructure services
- [x] **Create Test Data Factories**: Helper functions for `FileItem`, `LaunchedFile`
- [x] **Write Update Current File Tests**: Test currentFile updates correctly
- [x] **Write Update FileContext Tests**: Test fileContext.files updates correctly
- [x] **Write Integration Tests**: Test with file launch, history navigation
- [x] **Write Edge Case Tests**: Test null file, path mismatch, multi-device
- [x] **Write Workflow Tests**: Test complete favorite/unfavorite workflows

**Testing Subtask:**

- [ ] **Verify All Tests Pass**: Run tests and confirm comprehensive coverage

**Key Implementation Notes:**

- **Real PlayerStore**: Use actual store for integration, not mocked
- **Mock at boundaries**: Mock `IPlayerService`, `IDeviceService`, `StorageStore`
- **Behavioral focus**: Test observable outcomes through context signals
- **Follow existing patterns**: Use same structure as `player-context-history.service.spec.ts`

**Testing Focus for Task 5:**

**Test Categories:**

1. **Update Current File Favorite Status**:

   - [x] Updates currentFile.isFavorite when path matches
   - [x] No-op when currentFile is null
   - [x] No-op when file path does not match
   - [x] Signal reactivity: getCurrentFile() reflects change
   - [x] Multi-device independence

2. **Update FileContext Files**:

   - [x] Updates matching file in fileContext.files[] array
   - [x] Preserves other files in array unchanged
   - [x] Updates multiple matches if file appears multiple times
   - [ ] No-op when fileContext is null
   - [x] Signal reactivity: getFileContext() reflects change

3. **Integration with File Launch**:

   - [x] Launch file with isFavorite flag set
   - [x] Update favorite status after launch
   - [x] Preserve other file properties when updating
   - [x] Work with different storage types (SD, USB)

4. **Integration with History Navigation**:

   - [ ] Update favorite status for file in play history
   - [ ] Preserve history entries when updating status
   - [ ] Update file in fileContext if it exists in directory

5. **Edge Cases**:

   - [x] Handle update for uninitialized device gracefully
   - [ ] Handle concurrent favorite updates correctly
   - [ ] Handle rapid favorite toggling without race conditions
   - [x] Not corrupt other device states when updating one device

6. **Complete Workflow Scenarios**:
   - [x] Full favorite/unfavorite workflow
   - [ ] Maintain favorite status through navigation
   - [ ] Handle favorite toggle during shuffle mode
   - [ ] Preserve favorite status in play history

**Testing Reference:**

- See [Store Testing](../../STORE_TESTING.md) - Behavioral testing patterns for facades
- See [Testing Standards](../../TESTING_STANDARDS.md) - Test structure and organization

</details>

</details>

---

## 🗂️ Files Modified or Created

**New Files:**

- `libs/application/src/lib/player/actions/update-favorite-status.ts`
- `libs/application/src/lib/player/player-context-favorite.service.spec.ts`

**Modified Files:**

- `libs/application/src/lib/player/actions/index.ts`
- `libs/application/src/lib/player/player-context.interface.ts`
- `libs/application/src/lib/player/player-context.service.ts`
- `libs/features/player/.../player-toolbar-actions/player-toolbar-actions.component.ts`
- `libs/features/player/.../player-toolbar-actions/player-toolbar-actions.component.spec.ts`

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is a summary only.

### Testing Philosophy

- **Behavioral testing**: Test observable outcomes, not implementation details
- **Test as you go**: Each task includes testing subtask
- **Mock at boundaries**: Mock infrastructure services, use real stores
- **Test through facades**: Use PlayerContextService, not store directly

### Test Execution Commands

```bash
# Run player application tests
npx nx test application

# Run player feature tests
npx nx test player

# Run all tests
npx nx run-many --target=test --all

# Watch mode during development
npx nx test application --watch
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

**Functional Requirements:**

- [ ] All implementation tasks completed (checkboxes marked)
- [ ] Favorite button icon updates after successful storage operation
- [ ] Button shows disabled state during operation
- [ ] Failed operations do not corrupt player state
- [ ] Favorite status updates in both currentFile and fileContext
- [ ] Works correctly with SD and USB storage types
- [ ] Independent state management per device

**Testing Requirements:**

- [ ] All testing subtasks completed within each task
- [ ] Task 1: Store action behavioral tests passing
- [ ] Task 2: PlayerContext method tests passing
- [ ] Task 3: Component integration tests passing
- [ ] Task 4: Updated component unit tests passing
- [ ] Task 5: Comprehensive behavioral tests passing
- [ ] All tests passing with no failures

**Quality Checks:**

- [ ] New action follows `STATE_STANDARDS.md` patterns
- [ ] Uses `updateState()` with `actionMessage` (Redux DevTools tracking)
- [ ] TypeScript compilation succeeds with no errors
- [ ] ESLint passes with no new warnings
- [ ] Code formatting is consistent

**Documentation:**

- [ ] JSDoc comments added to new methods
- [ ] Code follows existing patterns
- [ ] Changes logged in Redux DevTools

**Ready for Next Phase:**

- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Code reviewed and approved (if applicable)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Pessimistic over Optimistic**: Chose pessimistic update pattern for simplicity (~15 lines vs 60+ lines), linear flow, and better error UX
- **Update Both Snapshots**: Action updates both `currentFile` and `fileContext.files[]` to ensure complete synchronization
- **Path-Based Matching**: Use file path as unique identifier for matching files across snapshots

### Implementation Constraints

- **Redux DevTools Requirement**: Must use `updateState()` with `actionMessage` for all state mutations (not `patchState()`)
- **Immutable Updates**: All state updates must be immutable at every nesting level
- **Button State Management**: Button disable/enable handled automatically by existing `isFavoriteOperationInProgress()` binding

### Why Pessimistic Updates?

**Complexity Comparison**:

- **Pessimistic**: ~15 lines, straightforward async/await, no state tracking
- **Optimistic**: ~60+ lines, state tracking, effects, rollback, race conditions

**UX Comparison**:

- **Pessimistic**: Brief loading state (~100-300ms), no icon flicker on error
- **Optimistic**: Instant feedback, but icon flips twice on error (poor UX)

**Maintenance**:

- **Pessimistic**: Linear flow, easy to debug, predictable
- **Optimistic**: Complex state machine, effect lifecycle, timing issues

### External References

- [Phase 5 Original Implementation](./FAVORITE_PLAN_P5.md) - Original favorite button work
- [Player Context History Tests Reference](../../../libs/application/src/lib/player/player-context-history.service.spec.ts) - Testing pattern

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

</details>
