# Phase 1: Core History Tracking Infrastructure

## 🎯 Objective

Establish the foundational state management for tracking play history, including state structure, history recording logic, and forward/backward history management with browser-style clearing behavior. This phase creates the data structures and store actions needed to track every file launch across all modes (Directory, Shuffle, Search), maintaining a chronological record with browser-style navigation tracking.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**

- [] [Play History Planning](./PLAY_HISTORY_PLANNING.md) - High-level feature plan
- [] [Player Domain Design](./PLAYER_DOMAIN_DESIGN.md) - Technical design and architecture

**Standards & Guidelines:**

- [] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices
- [] [State Standards](../../STATE_STANDARDS.md) - **State management patterns with updateState**
- [] [Style Guide](../../STYLE_GUIDE.md) - UI styling standards

---

## 📂 File Structure Overview

```
libs/application/src/lib/player/
├── player-store.ts                          📝 Modified - Add PlayHistory to state
├── player-helpers.ts                        📝 Modified - Add history helper functions
├── actions/
│   ├── index.ts                             📝 Modified - Export new history actions
│   ├── record-history.ts                    ✨ New - Record file launch in history
│   ├── clear-history.ts                     ✨ New - Clear all history for device
│   ├── launch-file-with-context.ts          📝 Modified - Call recordHistory after launch
│   ├── launch-random-file.ts                📝 Modified - Call recordHistory after launch
│   ├── navigate-next.ts                     📝 Modified - Call recordHistory after navigation
│   └── navigate-previous.ts                 📝 Modified - Call recordHistory after navigation
└── selectors/
    ├── index.ts                             📝 Modified - Export new history selectors
    ├── get-play-history.ts                  ✨ New - Get complete history for device
    ├── get-current-history-position.ts      ✨ New - Get current position in history
    ├── can-navigate-backward-in-history.ts  ✨ New - Check if backward navigation available
    └── can-navigate-forward-in-history.ts   ✨ New - Check if forward navigation available
```

---

<details open>
<summary><h3>Task 1: Define Play History State Structure</h3></summary>

**Purpose**: Create the TypeScript interfaces and types that define how play history is stored in player state, including individual history entries and the overall history tracking structure with browser-style forward/backward navigation support.

**Related Documentation:**

- [PLAY_HISTORY_PLANNING.md - Phase 1](./PLAY_HISTORY_PLANNING.md#phase-1-core-history-tracking-infrastructure) - Phase objectives
- [STATE_STANDARDS.md - State Structure](../../STATE_STANDARDS.md#state-structure-organization) - State organization patterns

**Implementation Subtasks:**

- [ ] **Add `HistoryEntry` interface** to `player-store.ts` with properties:
  - `file: FileItem` - The file that was played
  - `storageKey: StorageKey` - Reference to storage location
  - `parentPath: string` - Directory containing the file
  - `launchMode: LaunchMode` - How the file was launched (Directory, Shuffle, Search)
  - `timestamp: number` - When the file was launched
  - `isCompatible: boolean` - Whether file was compatible with hardware
- [ ] **Add `PlayHistory` interface** to `player-store.ts` with properties:
  - `entries: HistoryEntry[]` - Chronological list of all played files
  - `currentPosition: number` - Current position in history (-1 if at end, navigating creates positions)
- [ ] **Add `playHistory` property** to `DevicePlayerState` interface:
  - Type: `PlayHistory | null`
  - Default value in `createDefaultDeviceState`: `null`
- [ ] **Update `createDefaultDeviceState` helper** in `player-helpers.ts` to initialize `playHistory` as `null`

**Testing Subtask:**

- [ ] **Write Tests**: Verify state structure initialization (see Testing section below)

**Key Implementation Notes:**

- `currentPosition` of `-1` means "at the end" (latest entry) - standard state
- When navigating backward, `currentPosition` becomes explicit index in history
- When at position and launching new file, forward history (everything after position) is cleared
- History tracking starts as `null` and is initialized on first file launch

**Critical Type Definitions**:

```typescript
export interface HistoryEntry {
  file: FileItem;
  storageKey: StorageKey;
  parentPath: string;
  launchMode: LaunchMode;
  timestamp: number;
  isCompatible: boolean;
}

export interface PlayHistory {
  entries: HistoryEntry[];
  currentPosition: number; // -1 = at end, 0+ = explicit position
}
```

**Testing Focus for Task 1:**

**Behaviors to Test:**

- [ ] **State initializes correctly**: `DevicePlayerState` has `playHistory: null` by default
- [ ] **Helper creates default state**: `createDefaultDeviceState()` includes `playHistory: null`

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns
- See [State Standards](../../STATE_STANDARDS.md) for state testing guidance

</details>

---

<details open>
<summary><h3>Task 2: Create Record History Action</h3></summary>

**Purpose**: Implement the core action that captures file launches and adds them to history, handling browser-style forward history clearing when navigating after going backward.

**Related Documentation:**

- [STATE_STANDARDS.md - Action Behaviors](../../STATE_STANDARDS.md#action-behaviors) - Action implementation patterns
- [PLAY_HISTORY_PLANNING.md - Browser-Style History](./PLAY_HISTORY_PLANNING.md#architecture-overview) - Browser navigation pattern

**Implementation Subtasks:**

- [ ] **Create `record-history.ts`** in actions folder
- [ ] **Implement `recordHistory` action** that:
  - Accepts `{ deviceId: string; entry: HistoryEntry }`
  - Creates `actionMessage` using `createAction('record-history')`
  - Initializes `playHistory` if currently `null` with empty `entries` array and `currentPosition: -1`
  - If `currentPosition !== -1` (user navigated backward), clears forward history (all entries after current position)
  - Adds new entry to `entries` array
  - Sets `currentPosition` back to `-1` (at end)
  - Uses `updateState()` with `actionMessage` for all state mutations
- [ ] **Add helper function `recordHistoryEntry`** to `player-helpers.ts`:
  - Accepts `store`, `deviceId`, `entry: HistoryEntry`, `actionMessage`
  - Handles the state update logic
  - Includes appropriate logging with `LogType`
- [ ] **Export `recordHistory`** from `actions/index.ts`

**Testing Subtask:**

- [ ] **Write Tests**: Verify history recording behaviors (see Testing section below)

**Key Implementation Notes:**

- Follow [navigate-previous.ts](../../../libs/application/src/lib/player/actions/navigate-previous.ts) as reference for action structure
- Browser behavior: navigating backward then launching new file clears forward history
- All state mutations MUST use `updateState()` with `actionMessage` (not `patchState()`)
- Initialize history on first record if `playHistory === null`
- Use helper function pattern from existing actions for consistency

**Testing Focus for Task 2:**

**Behaviors to Test:**

- [ ] **First entry initializes history**: Recording when `playHistory === null` creates structure with one entry
- [ ] **Subsequent entries append**: Recording adds entry to end when `currentPosition === -1`
- [ ] **Forward history clears**: Recording when `currentPosition !== -1` removes entries after current position
- [ ] **Position resets**: `currentPosition` is set to `-1` after recording
- [ ] **Logging occurs**: Action logs start, success, and finish messages

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns
- See [State Standards](../../STATE_STANDARDS.md#action-behaviors) for action testing guidance

</details>

---

<details open>
<summary><h3>Task 3: Create Clear History Action</h3></summary>

**Purpose**: Implement action to remove all history for a device, used during device cleanup or when user explicitly clears history.

**Related Documentation:**

- [STATE_STANDARDS.md - Action Behaviors](../../STATE_STANDARDS.md#action-behaviors) - Action patterns
- [remove-player.ts](../../../libs/application/src/lib/player/actions/remove-player.ts) - Similar cleanup pattern

**Implementation Subtasks:**

- [ ] **Create `clear-history.ts`** in actions folder
- [ ] **Implement `clearHistory` action** that:
  - Accepts `{ deviceId: string }`
  - Creates `actionMessage` using `createAction('clear-history')`
  - Sets `playHistory` to `null` for the device
  - Uses `updateState()` with `actionMessage`
- [ ] **Add helper function `clearHistoryForDevice`** to `player-helpers.ts` (optional, for consistency)
- [ ] **Export `clearHistory`** from `actions/index.ts`

**Testing Subtask:**

- [ ] **Write Tests**: Verify history clearing behavior (see Testing section below)

**Key Implementation Notes:**

- Simple action - just sets `playHistory` back to `null`
- Logs operation with appropriate `LogType` values
- Will be called from `remove-player` action in future integration

**Testing Focus for Task 3:**

**Behaviors to Test:**

- [ ] **History is cleared**: `playHistory` is set to `null` after clear action
- [ ] **No-op when already null**: Clearing when history is already `null` completes without error
- [ ] **Logging occurs**: Action logs appropriate messages

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns

</details>

---

<details open>
<summary><h3>Task 4: Create History Selectors</h3></summary>

**Purpose**: Build computed signal selectors that provide reactive access to play history state for UI components and other actions.

**Related Documentation:**

- [STATE_STANDARDS.md - Selectors](../../STATE_STANDARDS.md#selector-behaviors) - Selector patterns
- [selectors/index.ts](../../../libs/application/src/lib/player/selectors/index.ts) - Existing selector structure

**Implementation Subtasks:**

- [ ] **Create `get-play-history.ts`** selector:
  - Returns `computed()` signal with `PlayHistory | null` for a device
- [ ] **Create `get-current-history-position.ts`** selector:
  - Returns `computed()` signal with current position number
  - Returns `-1` if no history or at end
- [ ] **Create `can-navigate-backward-in-history.ts`** selector:
  - Returns `computed()` signal with `boolean`
  - `true` if `entries.length > 0` and `currentPosition !== 0` (not at start)
- [ ] **Create `can-navigate-forward-in-history.ts`** selector:
  - Returns `computed()` signal with `boolean`
  - `true` if `currentPosition !== -1` (not at end) and `currentPosition < entries.length - 1`
- [ ] **Export all selectors** from `selectors/index.ts`
- [ ] **Add selectors to `withPlayerSelectors()`** custom feature

**Testing Subtask:**

- [ ] **Write Tests**: Verify selector computations (see Testing section below)

**Key Implementation Notes:**

- Follow pattern from existing selectors like `get-current-file.ts`
- Use factory functions that return `computed()` signals
- Handle null/undefined states gracefully
- Navigation availability based on position and entries length

**Testing Focus for Task 4:**

**Behaviors to Test:**

- [ ] **getPlayHistory returns null**: When history hasn't been initialized
- [ ] **getPlayHistory returns history**: When history exists with entries
- [ ] **getCurrentHistoryPosition returns -1**: When at end or no history
- [ ] **canNavigateBackward returns false**: When at start (position 0) or no history
- [ ] **canNavigateBackward returns true**: When position > 0 with entries
- [ ] **canNavigateForward returns false**: When at end (position -1) or no forward entries
- [ ] **canNavigateForward returns true**: When position is valid and < last index

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns
- See [State Standards](../../STATE_STANDARDS.md#computed-signals) for selector testing

</details>

---

<details open>
<summary><h3>Task 5: Integrate History Recording into Existing Launch Actions</h3></summary>

**Purpose**: Add `recordHistory` calls to all file launch actions so every file launch is automatically tracked in history.

**Related Documentation:**

- [launch-file-with-context.ts](../../../libs/application/src/lib/player/actions/launch-file-with-context.ts) - Launch action to modify
- [launch-random-file.ts](../../../libs/application/src/lib/player/actions/launch-random-file.ts) - Shuffle launch to modify

**Implementation Subtasks:**

- [ ] **Modify `launch-file-with-context.ts`**:
  - After successful launch (in success helper call), create `HistoryEntry` from `LaunchedFile`
  - Call `recordHistory` action with the entry
- [ ] **Modify `launch-random-file.ts`**:
  - After successful random launch, create `HistoryEntry` from `LaunchedFile`
  - Call `recordHistory` action with the entry
- [ ] **Modify `navigate-next.ts`**:
  - After successful navigation (both directory and shuffle modes), create `HistoryEntry`
  - Call `recordHistory` action with the entry
- [ ] **Modify `navigate-previous.ts`**:
  - After successful navigation (both directory and shuffle modes), create `HistoryEntry`
  - Call `recordHistory` action with the entry
- [ ] **Add helper function `createHistoryEntryFromLaunchedFile`** to `player-helpers.ts`:
  - Converts `LaunchedFile` to `HistoryEntry` format

**Testing Subtask:**

- [ ] **Write Tests**: Verify history recording integration (see Testing section below)

**Key Implementation Notes:**

- Record history ONLY on successful launches (when `isCompatible === true` or after successful launch)
- Do NOT record on failed launches or errors
- Use same `actionMessage` for the entire operation (launch + record)
- Entry should include all required properties from `LaunchedFile`

**Testing Focus for Task 5:**

**Behaviors to Test:**

- [ ] **Launch file with context records history**: After successful file launch, history contains new entry
- [ ] **Launch random records history**: After successful random launch, history contains entry
- [ ] **Navigate next records history**: After successful next navigation, history contains entry
- [ ] **Navigate previous records history**: After successful previous navigation, history contains entry
- [ ] **Failed launches don't record**: Incompatible files or errors don't add to history
- [ ] **History entries have correct data**: Entries include file, storageKey, launchMode, timestamp

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns

</details>

---

<details open>
<summary><h3>Task 6: Update Device Cleanup to Clear History</h3></summary>

**Purpose**: Ensure history is properly removed when a device disconnects, preventing memory leaks and stale state.

**Related Documentation:**

- [remove-player.ts](../../../libs/application/src/lib/player/actions/remove-player.ts) - Device cleanup action

**Implementation Subtasks:**

- [ ] **Modify `remove-player.ts`**:
  - History is automatically cleared when entire player state is removed
  - Verify current implementation removes all player state (including history)
  - No explicit `clearHistory` call needed since entire state is deleted

**Testing Subtask:**

- [ ] **Write Tests**: Verify cleanup behavior (see Testing section below)

**Key Implementation Notes:**

- Current `removePlayerState` helper deletes entire device entry from `players` record
- This automatically removes `playHistory` along with all other state
- No separate cleanup needed since entire state is removed

**Testing Focus for Task 6:**

**Behaviors to Test:**

- [ ] **Remove player clears all state**: When player is removed, history is also gone
- [ ] **No memory leaks**: Removed player state doesn't persist in store

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns

</details>

---

## 🗂️ Files Modified or Created

**New Files:**

- `libs/application/src/lib/player/actions/record-history.ts`
- `libs/application/src/lib/player/actions/clear-history.ts`
- `libs/application/src/lib/player/selectors/get-play-history.ts`
- `libs/application/src/lib/player/selectors/get-current-history-position.ts`
- `libs/application/src/lib/player/selectors/can-navigate-backward-in-history.ts`
- `libs/application/src/lib/player/selectors/can-navigate-forward-in-history.ts`

**Modified Files:**

- `libs/application/src/lib/player/player-store.ts` - Add PlayHistory interfaces and state
- `libs/application/src/lib/player/player-helpers.ts` - Add history helper functions
- `libs/application/src/lib/player/actions/index.ts` - Export history actions
- `libs/application/src/lib/player/actions/launch-file-with-context.ts` - Integrate recordHistory
- `libs/application/src/lib/player/actions/launch-random-file.ts` - Integrate recordHistory
- `libs/application/src/lib/player/actions/navigate-next.ts` - Integrate recordHistory
- `libs/application/src/lib/player/actions/navigate-previous.ts` - Integrate recordHistory
- `libs/application/src/lib/player/selectors/index.ts` - Export history selectors

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
>
> - **Favor behavioral testing** - test what users/consumers observe, not how it's implemented
> - **Test as you go** - tests are integrated into each task's subtasks, not deferred to the end
> - **Test through public APIs** - stores should be tested through their actions and selectors
> - **Mock at boundaries** - mock infrastructure services (IPlayerService), not internal logic

### Test Execution Commands

**Running Tests:**

```bash
# Run player store tests
npx nx test application-player

# Run tests in watch mode during development
npx nx test application-player --watch

# Run all application tests
npx nx run-many --target=test --projects=application-*
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**

- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)
- [ ] State management follows [State Standards](../../STATE_STANDARDS.md)
- [ ] All state mutations use `updateState()` with `actionMessage`

**Testing Requirements:**

- [ ] All testing subtasks completed within each task
- [ ] All behavioral test checkboxes verified
- [ ] Tests written alongside implementation (not deferred)
- [ ] All tests passing with no failures
- [ ] Test coverage meets project standards

**Quality Checks:**

- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors (`npm run lint`)
- [ ] Code formatting is consistent
- [ ] No console errors in browser/terminal when running application

**Documentation:**

- [ ] Inline code comments added for complex logic
- [ ] Helper functions documented with JSDoc comments

**Ready for Next Phase:**

- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] History tracking working for all launch modes
- [ ] Ready to proceed to Phase 2 (History Navigation)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Browser-Style Navigation**: Following web browser history pattern where going backward then launching new content clears forward history. This is familiar to users and prevents confusing state.
- **Position at -1 Means "At End"**: Using `-1` as sentinel value for current position allows simple detection of "user is at latest entry" vs "user has navigated backward to specific position".
- **Null vs Empty History**: Using `null` for no history initialization vs empty array with position -1. Null indicates "history feature not yet initialized", empty array would mean "initialized but no entries yet".
- **Single Entry Object**: `HistoryEntry` duplicates some data from `LaunchedFile` but keeps history self-contained and independent of currentFile state changes.

### Implementation Constraints

- **No Maximum Size Yet**: Phase 1 doesn't implement history size limits. This will be addressed in future phases if needed.
- **Memory Only**: History is not persisted to local storage in Phase 1. Clearing browser or disconnecting device clears history.
- **No Deduplication**: Recording same file multiple times creates multiple history entries. Phase 1 doesn't implement smart deduplication.

### Future Enhancements

- **Maximum History Size**: Implement configurable max entries (e.g., 100 items) to prevent unbounded growth
- **History Persistence**: Save/restore history from local storage for cross-session continuity
- **Smart Deduplication**: Option to collapse consecutive plays of same file into single entry
- **History Metadata**: Add play duration, skip count, or user rating to entries
- **Clear Partial History**: Allow clearing history for specific date range or launch mode

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

</details>

---

## 💡 Open Questions for Phase 1

**These questions should be answered during implementation:**

1. **History Entry Deduplication**: If the same file is played multiple times consecutively, should each play create a new history entry, or should we group consecutive duplicates?

   - **Decision**: Create new entry for each play (simple, accurate timeline)

2. **Maximum History Size**: What is the optimal maximum number of history entries before we start removing oldest entries?

   - **Decision**: No maximum in Phase 1, defer to future phase

3. **Failed Launch Recording**: Should failed/incompatible file launches be recorded in history with error flag, or completely excluded?
   - **Decision**: Exclude failed launches from history (only successful plays tracked)
