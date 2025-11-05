# Phase 3: Play History UI & View Switching Integration

## 🎯 Objective

Create the visual components that display play history and integrate them into the storage container's view management system, implementing intelligent switching between directory files, search results, and play history based on user context and mode.

**User Value**: Users can toggle on play history view to see their playback timeline with timestamps, launch modes, and file details. The history view intelligently hides/shows based on search activity and directory navigation, providing a seamless experience that complements existing navigation patterns.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**

- [ ] [Play History Planning Document](./PLAY_HISTORY_PLANNING.md) - High-level feature plan and Phase 3 requirements
- [ ] [Player Domain Design Document](./PLAYER_DOMAIN_DESIGN.md) - Technical design and architecture
- [ ] [Phase 1 Implementation](./PLAY_HISTORY_P1.md) - History state structure and tracking
- [ ] [Phase 2 Implementation](./PLAY_HISTORY_P2_REVISED.md) - History navigation in shuffle mode

**Standards & Guidelines:**

- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Behavioral testing approaches
- [ ] [State Standards](../../STATE_STANDARDS.md) - State mutation patterns with updateState()
- [ ] [Style Guide](../../STYLE_GUIDE.md) - UI styling standards and patterns
- [ ] [Component Library](../../COMPONENT_LIBRARY.md) - Reusable UI components
- [ ] [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) - Feature component testing methodology

**Code Review - Understand Existing Patterns:**

- [ ] Review `search-results.component.ts` - Template for history component structure
- [ ] Review `storage-container.component.ts` - View switching with `animationTrigger`
- [ ] Review `directory-trail.component.ts` - Navigation button patterns
- [ ] Review `file-item.component.ts` - File display component patterns
- [ ] Review `ScalingCardComponent` - Animation container for cards

---

## 📂 File Structure Overview

```
libs/application/src/lib/player/
├── player-store.ts                              📝 Modified - Add historyViewVisible to state
├── player-context.service.ts                    📝 Modified - Add toggleHistoryView() + navigateToHistoryPosition()
├── player-context.service.spec.ts               ✅ Unchanged - Phase 1-5 tests remain (3,647 lines)
├── player-context-history.service.spec.ts       📝 Modified - Add Phase 3 tests (dedicated file)
├── player-context.interface.ts                  📝 Modified - Add interface methods
├── actions/
│   ├── index.ts                                 📝 Modified - Export new actions
│   ├── update-history-view-visibility.ts        ✨ New - Set history view visibility
│   └── navigate-to-history-position.ts          ✨ New - Navigate to specific history position
└── selectors/
    ├── index.ts                                 📝 Modified - Export new selectors
    └── is-history-view-visible.ts               ✨ New - Get history view visibility state

libs/features/player/src/lib/player-view/player-device-container/storage-container/
├── storage-container.component.ts               📝 Modified - Add history view logic
├── storage-container.component.html             📝 Modified - Add play-history component
├── storage-container.component.scss             📝 Modified - Update layout if needed
├── play-history/
│   ├── play-history.component.ts                ✨ New - History list component
│   ├── play-history.component.html              ✨ New - History list template
│   ├── play-history.component.scss              ✨ New - History list styles
│   ├── play-history.component.spec.ts           ✨ New - History list tests
│   └── history-entry/
│       ├── history-entry.component.ts           ✨ New - Individual history entry
│       ├── history-entry.component.html         ✨ New - Entry template
│       ├── history-entry.component.scss         ✨ New - Entry styles
│       └── history-entry.component.spec.ts      ✨ New - Entry tests
└── directory-trail/
    ├── directory-trail.component.ts             📝 Modified - Add history toggle handler
    ├── directory-trail.component.html           📝 Modified - Add history toggle button (no changes needed)
    └── directory-navigate/
        ├── directory-navigate.component.ts      📝 Modified - Add history toggle button
        └── directory-navigate.component.html    📝 Modified - Add history button to nav
```

---

## 🎯 Implementation Approach

**Key Principle**: Follow established patterns from `search-results` component for consistency.

Each task creates:

1. **Store Action/Selector** - State management for history view visibility
2. **UI Component** - Visual components following existing patterns
3. **Integration** - Wire components into storage container and directory trail
4. **Behavioral Test** - Test through component public API

**Pattern Reference**: The `search-results` component provides the template:

- Uses `ScalingCardComponent` with `animationTrigger` input
- Displays list of items with empty states
- Highlights currently playing file with `pulsing-highlight` mixin
- Handles selection and double-click for file launching
- Integrates with `StorageContainer` via `animationTrigger` based on computed signal

This approach ensures:

- ✅ Consistent UI patterns across the application
- ✅ Proper animation and view switching behavior
- ✅ Clear separation between state (store) and presentation (components)
- ✅ Testable components with mocked dependencies

---

## 📋 Design Decisions Summary

**From Open Questions Discussion:**

1. **Timestamp Display**: Use absolute times (e.g., "3:45 PM", "10:42 AM")
2. **History View Default**: Hidden by default, requires user to toggle on
3. **Component Pattern**: Follow `search-results.component.ts` structure using `ScalingCardComponent`
4. **Currently Playing Indicator**: Use `pulsing-highlight` mixin (same as directory-files and search-results)
5. **Empty State**: Use `EmptyStateMessageComponent` with history icon and helpful message
6. **View Priority**: Search results > play history > directory files
7. **Multi-Device**: Device-scoped - show only history for current device
8. **Entry Interactions**: Single-click selects, double-click launches file from history

---

## 🎛️ State Management Approach

**Simplified Coordination Pattern:**

**Single Store Action**: `updateHistoryViewVisibility({ deviceId, visible: boolean })`

- Simple setter - no toggle logic in store
- Called by context service for all visibility changes

**Context Service Orchestration**:

- **`toggleHistoryView()`** - Reads current state, calls store action with opposite value
- **`launchFileWithContext()`** - Calls store action to hide history when user launches a file

**Auto-Hide Behavior**:

- History is hidden when user **explicitly launches a NEW file** via:
  - Clicking file in directory view
  - Clicking file in search results
- History **stays visible** when user navigates within history:
  - Clicking history entry (navigates to that position)
  - Using next/prev while in history navigation mode
- Single coordination point: `launchFileWithContext()` hides history for new launches

**Why This Approach**:

- Avoids toggle race conditions
- Single coordination method for file launches
- Context service contains orchestration logic (not store)
- Follows existing pattern (similar to `toggleShuffleMode()`)

---

<details>
<summary><h2>✅ Task 1: Add History View Visibility State</h2></summary>

**Goal**: Add state management for history view visibility with incremental implementation and testing.

**Pattern**: Store Element → Context Method → Test (repeat for each behavior)

**Status**: ✅ **COMPLETE** - All functionality implemented and tested. All 354 application tests passing.

**Behaviors Implemented:**

1. ✅ Toggle history view visibility
2. ✅ Auto-hide history when launching new files
3. ✅ Navigate to specific history position

---

### Step 1.1: Add History View State to DevicePlayerState

**Purpose**: Extend player state to track history view visibility.

**Files**:

- `libs/application/src/lib/player/player-store.ts`
- `libs/application/src/lib/player/player-helpers.ts`

**Instructions:**

- [x] Add `historyViewVisible: boolean` property to `DevicePlayerState` interface
- [x] Update `createDefaultDeviceState()` to initialize `historyViewVisible: false`
- [x] Verify TypeScript compilation succeeds

**Key Notes:**

- Default to `false` - history view is hidden by default
- Per-device state maintains multi-device independence

---

### Step 1.2: Create Update History View Visibility Action

**Purpose**: Build store action to set history view visibility explicitly.

**File**: `libs/application/src/lib/player/actions/update-history-view-visibility.ts`

**Instructions:**

- [x] Create new action file
- [x] Define `UpdateHistoryViewVisibilityParams` interface with:
  - `deviceId: string`
  - `visible: boolean`
- [x] Implement action function that:
  - Creates action message using `createAction('update-history-view-visibility')`
  - Uses `updateState()` (NOT `patchState()`) with action message
  - Updates `players[deviceId].historyViewVisible` to the provided value
  - Includes logging with appropriate `LogType`
- [x] Export from `actions/index.ts`
- [x] Add `updateHistoryViewVisibility(params)` method to `PlayerStore`

**Pattern Reference**: Follow `updateLaunchMode` action structure

---

### Step 1.3: Create History View Visibility Selector

**Purpose**: Build selector to read history view visibility state.

**File**: `libs/application/src/lib/player/selectors/is-history-view-visible.ts`

**Instructions:**

- [x] Create new selector file
- [x] Implement selector function that returns `computed()` signal
- [x] Return `false` if device player state doesn't exist
- [x] Return `state.historyViewVisible` value if exists
- [x] Export from `selectors/index.ts`
- [x] Add `isHistoryViewVisible(deviceId)` to `withPlayerSelectors()` custom feature

**Pattern Reference**: Follow existing selector patterns like `getLaunchMode`

---

### Step 1.4: Implement toggleHistoryView() Context Method

**Purpose**: Add toggle method to context service.

**Files**:

- `libs/application/src/lib/player/player-context.interface.ts`
- `libs/application/src/lib/player/player-context.service.ts`

**Instructions:**

- [x] Add method signatures to `IPlayerContext` interface:
  - `toggleHistoryView(deviceId: string): void`
  - `isHistoryViewVisible(deviceId: string): Signal<boolean>`
- [x] Implement `toggleHistoryView()` in `PlayerContextService`:
  - Get current visibility from store selector
  - Call store action with opposite value
  - Handle toggle logic at context service level (not store)
- [x] Implement `isHistoryViewVisible()` as simple delegation to store selector

**Pattern Reference**: Follow `toggleShuffleMode()` orchestration pattern

---

### Step 1.5: Test Toggle History View Behavior

**Purpose**: Validate toggle behavior through behavioral tests.

**File**: `libs/application/src/lib/player/player-context-history.service.spec.ts` ⚠️ **SAME DEDICATED FILE AS PHASE 2**

**Instructions:**

- [x] Add `describe('Phase 3: History View & UI Integration', () => {})` block in dedicated test file
- [x] Add `describe('History View Visibility')` nested test suite
- [x] Test 1: Verify history view is hidden by default after player initialization
- [x] Test 2: Verify toggling once makes history view visible
- [x] Test 3: Verify toggling twice returns to hidden state
- [x] Test 4: Verify visibility is per-device (toggle A doesn't affect B)
- [x] Run tests: `npx nx test application --run --testNamePattern="History View Visibility"`

**Success Criteria:**

- [x] All 4 tests passing
- [x] No existing tests broken

**Important Note**: Phase 3 tests continue in the same `player-context-history.service.spec.ts` file created in Phase 2. This keeps all history-related tests together and separate from the original 3,647-line test file.

---

### Step 1.6: Create Navigate to History Position Action

**Purpose**: Build store action to navigate to a specific history position.

**File**: `libs/application/src/lib/player/actions/navigate-to-history-position.ts`

**Instructions:**

- [x] Create new action file
- [x] Define `NavigateToHistoryPositionParams` interface with:
  - `deviceId: string`
  - `position: number`
- [x] Implement action function that:
  - Creates action message using `createAction('navigate-to-history-position')`
  - Validates position is within bounds (0 to entries.length - 1)
  - Gets history entry at specified position
  - Uses `updateState()` with action message to:
    - Set `currentHistoryPosition` to position
    - Set `currentFile` from history entry
    - Set `launchMode` from history entry
  - Does NOT modify `playHistory.entries` array
  - Includes logging with appropriate `LogType`
- [x] Export from `actions/index.ts`
- [x] Add `navigateToHistoryPosition(params)` method to `PlayerStore`

**Pattern Reference**: Similar to `navigateBackwardInHistory` and `navigateForwardInHistory`

**Key Notes:**

- This navigates existing history, does not record new entries
- Validates bounds to prevent errors

---

### Step 1.7: Implement navigateToHistoryPosition() Context Method

**Purpose**: Add history navigation method to context service.

**Files**:

- `libs/application/src/lib/player/player-context.interface.ts`
- `libs/application/src/lib/player/player-context.service.ts`

**Instructions:**

- [x] Add method signature to `IPlayerContext`:
  - `navigateToHistoryPosition(deviceId: string, position: number): Promise<void>`
- [x] Implement in `PlayerContextService`:
  - Call store action `navigateToHistoryPosition({ deviceId, position })`
  - Get current file from store
  - Load directory context using `loadDirectoryContextForRandomFile()`
  - Setup timer using `setupTimerForFile()` if successful
  - Do NOT call `recordHistoryIfSuccessful()` - navigating existing timeline
  - Do NOT call `updateHistoryViewVisibility()` - keep history visible

**Pattern Reference**: Follow `next()`/`previous()` history navigation structure

**Key Notes:**

- User is navigating within history, not launching new files
- History view should remain visible

---

### Step 1.8: Test Navigate to History Position Behavior

**Purpose**: Validate history position navigation.

**File**: `libs/application/src/lib/player/player-context-history.service.spec.ts` ⚠️ **SAME DEDICATED FILE**

**Instructions:**

- [x] Add test to existing "Phase 3: History View & UI Integration" → "History View Visibility" suite
- [x] Test: Navigating to history position keeps history visible
  - Setup: Initialize player, launch 3 files to build history, toggle history view on
  - Verify: History has 3 entries and view is visible
  - Action: Call `navigateToHistoryPosition(deviceId, 0)`
  - Verify:
    - History view visibility remains `true`
    - Current file matches entry at position 0
    - Current history position is 0
    - No new history entry was recorded (still 3 entries)
- [x] Run tests

**Success Criteria:**

- [x] Test passing
- [x] Previous tests still passing

---

### Step 1.9: Update launchFileWithContext() to Auto-Hide History

**Purpose**: Add coordination to hide history when launching new files.

**File**: `libs/application/src/lib/player/player-context.service.ts`

**Instructions:**

- [x] Locate `launchFileWithContext()` method
- [x] After `this.store.initializePlayer()` call, add:
  - Call `this.store.updateHistoryViewVisibility()` with `visible: false`
  - Add comment explaining this hides history for new file launches (directory clicks, search clicks)
- [x] Verify placement is BEFORE `this.store.launchFileWithContext()` call

**Key Notes:**

- This captures all explicit new file launches from UI
- Placement ensures visibility is updated before file loads

---

### Step 1.10: Test launchFileWithContext() Auto-Hide Behavior

**Purpose**: Validate that launching new files hides history view.

**File**: `libs/application/src/lib/player/player-context-history.service.spec.ts` ⚠️ **SAME DEDICATED FILE**

**Instructions:**

- [x] Add test to existing "Phase 3" → "History View Visibility" suite
- [x] Test: Launching file hides history view
  - Setup: Initialize player, toggle history view on
  - Verify: History view is visible
  - Action: Launch a file using `launchFileWithContext()`
  - Verify: History view visibility is now `false`
- [x] Run all tests in suite

**Success Criteria:**

- [x] All tests passing (now 6 total in Phase 3 suite)
- [x] No existing tests broken

</details>

---

<details open>
<summary><h2>Task 2: Create History Entry Component</h2></summary>

**Goal**: Build the individual history entry component that displays a single file with timestamp and launch mode.

---

### Step 2.1: Create History Entry Component

**Purpose**: Build presentational component for individual history entry.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/play-history/history-entry/history-entry.component.ts`

**Implementation Checklist:**

- [ ] Create component with proper imports
- [ ] Define inputs:
  - [ ] `entry = input.required<HistoryEntry>()`
  - [ ] `selected = input<boolean>(false)`
  - [ ] `isCurrentlyPlaying = input<boolean>(false)`
- [ ] Define outputs:
  - [ ] `entrySelected = output<HistoryEntry>()`
  - [ ] `entryDoubleClick = output<HistoryEntry>()`
- [ ] Create computed signals:
  - [ ] `fileIcon` - Based on file type (reuse logic from `file-item.component.ts`)
  - [ ] `formattedTimestamp` - Format timestamp as absolute time (e.g., "3:45 PM")
  - [ ] `launchModeLabel` - Display launch mode ("Directory", "Shuffle", "Search")
  - [ ] `formattedSize` - Format file size (reuse logic from `file-item.component.ts`)
- [ ] Implement event handlers:
  - [ ] `onEntryClick()` - Emit `entrySelected`
  - [ ] `onEntryDoubleClick()` - Emit `entryDoubleClick`

**Key Implementation Details:**

- Create computed signal `fileIcon` that returns appropriate Material icon based on file type
- Create computed signal `formattedTimestamp` using `toLocaleTimeString()` with 12-hour format
- Create computed signal `launchModeLabel` that returns readable string ("Directory", "Shuffle", "Search")
- Create computed signal `formattedSize` for file size display
- Format timestamp as absolute time (e.g., "3:45 PM") not relative time

**Pattern Reference**: Follow `file-item.component.ts` structure for icon mapping and formatting

**Key Notes:**

- Follow `file-item.component.ts` pattern closely
- Use `StorageItemComponent` for consistent styling
- Include timestamp and launch mode in display

---

### Step 2.2: Create History Entry Template

**Purpose**: Build template using `StorageItemComponent` wrapper.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/play-history/history-entry/history-entry.component.html`

**Implementation Checklist:**

- [ ] Use `lib-storage-item` component
- [ ] Pass icon, label, selected state
- [ ] Wire up selection and double-click events
- [ ] Add `lib-storage-item-actions` for timestamp and launch mode
- [ ] Include visual indicators (badge or icon) for launch mode

**Template Instructions:**

- Use `lib-storage-item` component as wrapper
- Bind `icon` input to `fileIcon()` computed signal
- Bind `label` input to `entry().file.name`
- Bind `selected` input to `selected()` signal
- Wire `selectedChange` event to `onEntryClick()` handler
- Wire `activated` event to `onEntryDoubleClick()` handler
- Use `lib-storage-item-actions` to display `formattedTimestamp()` in actions area

**Pattern Reference**: Follow `file-item.component.html` template structure

**Key Notes:**

- `StorageItemComponent` handles hover, selection, and keyboard navigation
- Timestamp displayed in actions area (right-aligned)
- Launch mode can be shown as badge or omitted for simplicity

---

### Step 2.3: Add History Entry Styles

**Purpose**: Style history entries, including currently playing highlight.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/play-history/history-entry/history-entry.component.scss`

**Instructions:**

- [ ] Set `:host` display to `flex`
- [ ] Set `:host` width to `100%`
- [ ] Keep styles minimal - most handled by `StorageItemComponent`

**Pattern Reference**: Follow `file-item.component.scss` minimal host styling

**Key Notes:**

- Most styling handled by `StorageItemComponent`
- Currently playing highlight applied by parent (play-history component)
- Keep component styles minimal

---

### Step 2.4: Write History Entry Component Tests

**Purpose**: Unit test history entry component.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/play-history/history-entry/history-entry.component.spec.ts`

**Test Suite:**

- [ ] Add `describe('HistoryEntryComponent', () => {})` block

**Tests to Implement:**

**Test 1: Displays file information**

- Setup: Provide test history entry
- Verify: File name, icon, and timestamp rendered

**Test 2: Emits entrySelected on click**

- Setup: Create component with entry
- Action: Click entry
- Verify: `entrySelected` event emitted with entry

**Test 3: Emits entryDoubleClick on double-click**

- Setup: Create component with entry
- Action: Double-click entry
- Verify: `entryDoubleClick` event emitted with entry

**Test 4: Displays selected state**

- Setup: Set `selected` input to `true`
- Verify: Component has selected styling

**Test 5: Formats timestamp correctly**

- Setup: Entry with specific timestamp
- Verify: Displays formatted time (e.g., "3:45 PM")

**Run Tests:**

```bash
npx nx test player --run --testNamePattern="HistoryEntryComponent"
```

**Success Criteria:**

- [ ] All 5 tests passing
- [ ] Component follows presentational component testing patterns
- [ ] No console errors or warnings

</details>

---

<details open>
<summary><h2>Task 3: Create Play History Component</h2></summary>

**Goal**: Build the main play history component that displays the list of history entries.

---

### Step 3.1: Create Play History Component

**Purpose**: Build smart component for play history list.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/play-history/play-history.component.ts`

**Implementation Checklist:**

- [ ] Create component with proper imports
- [ ] Define inputs:
  - [ ] `deviceId = input.required<string>()`
  - [ ] `animationTrigger = input<boolean>(true)`
- [ ] Inject dependencies:
  - [ ] `private readonly playerContext: IPlayerContext = inject(PLAYER_CONTEXT)`
- [ ] Create computed signals:
  - [ ] `playHistory()` - Get play history from player context
  - [ ] `historyEntries()` - Get entries array from history (or empty array)
  - [ ] `currentFile()` - Get currently playing file
  - [ ] `currentLaunchMode()` - Get current launch mode
  - [ ] `hasPlayerError()` - Get player error state
  - [ ] `selectedEntry()` - Local signal for selected entry
- [ ] Implement methods:
  - [ ] `onEntrySelected(entry: HistoryEntry)` - Update local selection
  - [ ] `onEntryDoubleClick(entry: HistoryEntry, index: number)` - Navigate to position in history
  - [ ] `isSelected(entry: HistoryEntry)` - Check if entry is selected
  - [ ] `isCurrentlyPlaying(entry: HistoryEntry)` - Check if entry is currently playing
- [ ] Add effect for auto-scrolling to currently playing entry (optional, follow search-results pattern)

**Key Implementation Details:**

- Create computed signal `playHistory()` that calls `playerContext.getPlayHistory(deviceId)()`
- Create computed signal `historyEntries()` that returns `playHistory()?.entries ?? []`
- Create computed signal `currentFile()` that calls `playerContext.getCurrentFile(deviceId)()`
- Create computed signal `hasPlayerError()` that checks if error is not null
- Create writable signal `selectedEntry` for tracking local selection state
- Implement `onEntryDoubleClick()` to call `playerContext.navigateToHistoryPosition(deviceId, index)`
- Note: Double-click navigates to position in history, does NOT launch new file

**Pattern Reference**: Follow `search-results.component.ts` structure closely

**Key Notes:**

- Follow search-results component pattern for consistency
- Handle empty history with `EmptyStateMessageComponent`
- Track local selection state with signal
- Double-click navigates to that position in history (NOT launching new file)

---

### Step 3.2: Create Play History Template

**Purpose**: Build template using `ScalingCardComponent`.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/play-history/play-history.component.html`

**Instructions:**

- [ ] Wrap in div with class `play-history`
- [ ] Use `lib-scaling-card` with title "Play History"
- [ ] Bind `animationTrigger` input to component's `animationTrigger()` signal
- [ ] Add `@if` block for empty state when `historyEntries().length === 0`
  - Show `lib-empty-state-message` with icon="history", title="No Play History", message="Files you play will appear here", size="small"
- [ ] Add `@if` block for history list when `historyEntries().length > 0`
  - Create div with class `history-list` for scroll container
  - Use `@for` loop over `historyEntries()` with track by `entry.timestamp`
  - Capture loop index: `let i = $index`
  - Wrap each entry in div with class `history-list-item`
  - Add data attributes: `data-item-path`, `data-is-playing`, `data-has-error`
  - Use `lib-history-entry` component with appropriate bindings
  - Bind outputs: `entrySelected` and `entryDoubleClick` (passing index to double-click)

**Pattern Reference**: Follow `search-results.component.html` template structure

**Key Notes:**

- Empty state shows helpful message with history icon
- Use data attributes for CSS styling hooks (pulsing-highlight mixin)
- Pass index to double-click handler for history navigation
- Reverse chronological order (newest first) by default
[attr.data-is-playing]="isCurrentlyPlaying(entry)"
[attr.data-has-error]="hasPlayerError() && isCurrentlyPlaying(entry)">
<lib-history-entry
[entry]="entry"
[selected]="isSelected(entry)"
[isCurrentlyPlaying]="isCurrentlyPlaying(entry)"
(entrySelected)="onEntrySelected(entry)"
(entryDoubleClick)="onEntryDoubleClick(entry, i)">
</lib-history-entry>
</div>
}
</div>
}
</lib-scaling-card>
</div>

````

**Key Notes:**
- Follow search-results template structure exactly
- Use data attributes for CSS styling hooks
- Empty state shows helpful message with history icon
- Reverse chronological order (newest first) by default

---

### Step 3.3: Add Play History Styles

**Purpose**: Style play history list with currently playing highlight.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/play-history/play-history.component.scss`

**Instructions:**
- [ ] Set `:host` display to flex, flex-direction column, height 100%, width 100%
- [ ] Style `.play-history` container with flex column layout, full height/width
- [ ] Style `.history-list` with flex column layout, full width
- [ ] Style `.history-list-item`:
  - Transparent background with transition
  - Border-radius for rounded corners
  - Use attribute selector `[data-is-playing="true"][data-has-error="true"]` for error state
    - Include `pulsing-highlight` mixin with error color
    - Add negative left margin and padding adjustments
  - Use attribute selector `[data-is-playing="true"]` for normal playing state
    - Include `pulsing-highlight` mixin with highlight color
    - Add negative left margin and padding adjustments

**Pattern Reference**: Copy `search-results.component.scss` styles exactly for consistency

**Key Notes:**
- Import styles from `ui/styles` for mixin access
- `pulsing-highlight` mixin provides animated border (left side)
- Error state selector must come before normal playing state to override
- Margin/padding adjustments compensate for pulsing border

---

### Step 3.4: Write Play History Component Tests

**Purpose**: Test play history component with mocked dependencies.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/play-history/play-history.component.spec.ts`

**Test Setup:**
- [ ] Mock `IPlayerContext` using interface
- [ ] Create test history entries with various timestamps
- [ ] Provide mocked dependencies in TestBed

**Tests to Implement:**

**Test 1: Displays empty state when no history**
- Setup: Mock player context with no history
- Verify: Empty state message displayed

**Test 2: Displays history entries**
- Setup: Mock player context with 3 history entries
- Verify: 3 history entries rendered

**Test 3: Highlights currently playing entry**
- Setup: Mock current file matching one history entry
- Verify: Entry has `data-is-playing="true"` attribute

**Test 4: Navigates to history position on double-click**
- Setup: Mock player context, history with entries
- Action: Double-click history entry at index 1
- Verify: `navigateToHistoryPosition()` called with deviceId and position 1


**Test 5: Selects entry on click**
- Setup: Render history with entries
- Action: Click an entry
- Verify: Entry has selected styling

**Test 6: Shows error highlight for failed launch**
- Setup: Mock current file with player error
- Verify: Entry has error styling (`data-has-error="true"`)

**Run Tests:**
```bash
npx nx test player --run --testNamePattern="PlayHistoryComponent"
````

**Success Criteria:**

- [ ] All 6 tests passing
- [ ] Tests use mocked `IPlayerContext` interface
- [ ] No console errors or warnings

</details>

---

<details open>
<summary><h2>Task 4: Add History Toggle Button</h2></summary>

**Goal**: Add history toggle button to directory navigation component.

---

### Step 4.1: Update Directory Navigate Component

**Purpose**: Add history toggle button to navigation controls.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-trail/directory-navigate/directory-navigate.component.ts`

**Implementation Checklist:**

- [ ] Add input property:
  - [ ] `historyViewVisible = input<boolean>(false)`
        **Implementation Checklist:**
- [ ] Add input signal:
  - [ ] `historyViewVisible = input<boolean>(false)` - Required input
- [ ] Add output event:
  - [ ] `historyToggleClicked = output<void>()`
- [ ] Add event handler:
  - [ ] `onHistoryToggleClick()` - Emit `historyToggleClicked`

**Pattern Reference**: Follow existing button pattern (back, forward, up, refresh)

**Key Notes:**

- Input uses signal-based `input()` function, not decorator
- Output uses `output<void>()` function for Angular 19
- Handler emits event with no payload (void)

---

### Step 4.2: Update Directory Navigate Template

**Purpose**: Add history toggle button to navigation button row.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-trail/directory-navigate/directory-navigate.component.html`

**Implementation Checklist:**

- [ ] Add `lib-icon-button` for history toggle
- [ ] Use `history` icon
- [ ] Set `ariaLabel` to "Toggle History"
- [ ] Size `large` to match other buttons
- [ ] Use `color="highlight"` when history view is visible
- [ ] Wire up `(buttonClick)` to handler

**Instructions:**

- [ ] Add button using `lib-icon-button` component
- [ ] Set icon to `history`
- [ ] Set ariaLabel to "Toggle History"
- [ ] Set size to "large"
- [ ] Bind color input: Use ternary to show 'highlight' when visible, 'normal' otherwise
- [ ] Bind `buttonClick` event to `onHistoryToggleClick()` handler

**Key Notes:**

- Place button logically in button row (suggest: after refresh button)
- Highlight button when history view is visible for visual feedback
- Icon: `history` or `schedule` are good Material icon options

---

### Step 4.3: Update Directory Trail Component

**Purpose**: Wire history toggle button to player context service.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-trail/directory-trail.component.ts`

**Instructions:**

- [ ] Inject `IPlayerContext` as private readonly property
- [ ] Add computed signal `historyViewVisible` that calls `playerContext.isHistoryViewVisible(deviceId)()`
- [ ] Add event handler method `onHistoryToggleClick()` that calls `playerContext.toggleHistoryView(deviceId)`

**Pattern Reference**: Follow existing navigation handler patterns in component

---

### Step 4.4: Update Directory Trail Template

**Purpose**: Pass history visibility and toggle handler to navigate component.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-trail/directory-trail.component.html`

**Instructions:**

- [ ] Add `[historyViewVisible]` input binding to `lib-directory-navigate`
- [ ] Bind it to `historyViewVisible()` computed signal
- [ ] Add `(historyToggleClicked)` output binding
- [ ] Wire it to `onHistoryToggleClick()` handler

**Pattern Reference**: Follow existing input/output binding patterns for navigation buttons

---

### Step 4.5: Write Directory Trail Tests

**Purpose**: Test history toggle functionality.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-trail/directory-trail.component.spec.ts`

**Tests to Add:**

**Test 1: History toggle button visible**

- Setup: Render component
- Verify: History toggle button exists in DOM

**Test 2: History toggle button highlighted when visible**

- Setup: Mock history view visible = true
- Verify: Button has highlight color

**Test 3: Clicking history toggle calls playerContext**

- Setup: Mock player context
- Action: Click history toggle button
- Verify: `toggleHistoryView()` called with correct deviceId

**Run Tests:**

```bash
npx nx test player --run --testNamePattern="DirectoryTrailComponent"
```

**Success Criteria:**

- [x] All 4 new tests passing (added 4 tests instead of 3 for better coverage)
- [x] Existing directory trail tests still passing (35 total tests passing)

</details>

---

<details open>
<summary><h2>Task 5: Integrate Play History into Storage Container</h2></summary>

**Goal**: Add play history component to storage container with intelligent view switching.

---

### Step 5.1: Update Storage Container Component

**Purpose**: Add view management logic for play history.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.ts`

**Implementation Checklist:**

- [ ] Import `PlayHistoryComponent` in component
- [ ] Add `PlayHistoryComponent` to imports array
- [ ] Inject `IPlayerContext` service using `inject(PLAYER_CONTEXT)`
- [ ] Create `historyViewVisible()` computed signal - calls `playerContext.isHistoryViewVisible(deviceId)()`
- [ ] Create `hasPlayHistory()` computed signal - checks if history entries exist for device
- [ ] Create `shouldShowHistory()` computed signal - combines three conditions:
  - History view toggle is on (`historyViewVisible()`)
  - Search is NOT active (`!hasActiveSearch()`)
  - Device has history entries (`hasPlayHistory()`)

**Pattern Reference**: Follow search-results integration in this component (already implemented)

**View Priority Logic:**

1. **Search Active** → Show search results (highest priority)
2. **History Toggle On + Has Entries** → Show play history
3. **Default** → Show directory files

**Key Notes:**

- All three computed signals read from existing player context methods
- View switching is mutually exclusive based on computed signal values
- Search always takes priority over history

---

### Step 5.2: Update Storage Container Template

**Purpose**: Add play history component with animation trigger.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.html`

**Implementation Checklist:**

- [ ] Add `lib-play-history` component in right-container
- [ ] Pass `deviceId` and `animationTrigger` inputs
- [ ] Set `animationTrigger` based on `shouldShowHistory()` signal
- [ ] Update directory-files `animationTrigger` to hide when history or search shown

**Template Instructions:**

- [ ] Add third view component for play-history
- [ ] Use `lib-play-history` component with required inputs
- [ ] Set `animationTrigger` input to `shouldShowHistory()` computed signal
- [ ] Place after search-results component, before directory-files component
- [ ] Update directory-files `animationTrigger` to exclude history: `!hasActiveSearch() && !shouldShowHistory()`

**View Priority Order** (highest to lowest):

1. Search results (when `hasActiveSearch()` is true)
2. Play history (when `shouldShowHistory()` is true)
3. Directory files (default fallback)

**Pattern Reference**: Follow search-results integration pattern

**Key Notes:**

- Animation triggers are mutually exclusive
- Search has highest priority (if active, others are hidden)
- History shows when toggle is on AND not searching
- Directory files show as default (when neither search nor history active)

---

### Step 5.3: Write Storage Container Tests

**Purpose**: Test view switching logic.

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.spec.ts`

**Tests to Add:**

**Test 1: Shows directory files by default**

- Setup: No search, history toggle off
- Verify: Directory files visible, history hidden

**Test 2: Shows history when toggled on**

- Setup: Mock history toggle on, has history entries
- Verify: History visible, directory files hidden

**Test 3: Shows search results over history**

- Setup: History toggle on, search active
- Verify: Search results visible, history hidden

**Test 4: Hides history when no entries**

- Setup: History toggle on, but no history entries exist
- Verify: History hidden (empty), directory files shown

**Test 5: View priority order**

- Verify: Search > History > Directory Files

**Run Tests:**

```bash
npx nx test player --run --testNamePattern="StorageContainerComponent"
```

**Success Criteria:**

- [x] All 5 new tests passing (6 tests total including 'should create')
- [x] Existing storage container tests still passing (all 6 passing)
- [x] View switching logic validated

</details>

---

<details open>
<summary><h2>Task 6: End-to-End Integration Testing</h2></summary>

**Goal**: Validate complete play history feature integration.

---

### Step 6.1: Write Integration Tests

**Purpose**: Test complete workflows through the UI.

**Tests to Implement:**

**Integration Test 1: Toggle history view workflow**

- Action: Launch files in shuffle mode
- Action: Toggle history view on
- Verify: History component displays with entries
- Action: Toggle history view off
- Verify: Directory files component displays

**Integration Test 2: Search hides history**

- Setup: Toggle history view on
- Action: Perform search
- Verify: Search results visible, history hidden
- Action: Clear search
- Verify: History visible again

**Integration Test 3: Currently playing file highlighted**

- Setup: Launch multiple files, toggle history view on
- Verify: Currently playing file has pulsing highlight

**Integration Test 4: Launch file from history**

- Setup: Build history with multiple files
- Action: Double-click old history entry
- Verify: File launches, becomes current file

**Integration Test 5: Multi-device independence**

- Setup: Two devices with different histories
- Action: Toggle history on for device A
- Verify: Only device A history visible, device B unaffected

**Run Tests:**

```bash
npx nx test player --run --testNamePattern="Play History Integration"
```

---

### Step 6.2: Manual Testing Checklist

**Manual Testing Steps:**

- [ ] **Toggle History View**
  - [ ] History button appears in directory trail
  - [ ] Clicking button toggles history view on/off
  - [ ] Button highlights when history is visible
- [ ] **History Display**
  - [ ] Empty state shows when no history
  - [ ] History entries display with file info and timestamps
  - [ ] Timestamps show absolute times (e.g., "3:45 PM")
  - [ ] Currently playing file has pulsing highlight
- [ ] **File Launching**
  - [ ] Single-click selects history entry
  - [ ] Double-click launches file from history
  - [ ] Launched file becomes currently playing
- [ ] **View Switching**
  - [ ] Search hides history, shows search results
  - [ ] Clearing search shows history again (if toggle was on)
  - [ ] Directory navigation works independently
- [ ] **Animations**
  - [ ] History component animates in/out smoothly
  - [ ] No visual glitches during view switching
  - [ ] Pulsing highlight animates properly
- [ ] **Multi-Device**
  - [ ] Each device has independent history view toggle
  - [ ] Toggling one device doesn't affect others

</details>

---

## 🗂️ Files Modified or Created

**New Files:**

```
libs/application/src/lib/player/
├── actions/
│   ├── update-history-view-visibility.ts        ✨ New
│   └── navigate-to-history-position.ts          ✨ New
└── selectors/
    └── is-history-view-visible.ts               ✨ New

libs/features/player/src/lib/player-view/player-device-container/storage-container/
└── play-history/
    ├── play-history.component.ts                ✨ New
    ├── play-history.component.html              ✨ New
    ├── play-history.component.scss              ✨ New
    ├── play-history.component.spec.ts           ✨ New
    └── history-entry/
        ├── history-entry.component.ts           ✨ New
        ├── history-entry.component.html         ✨ New
        ├── history-entry.component.scss         ✨ New
        └── history-entry.component.spec.ts      ✨ New
```

**Modified Files:**

```
libs/application/src/lib/player/
├── player-store.ts                              📝 Modified - Add historyViewVisible
├── player-context.service.ts                    📝 Modified - Add toggle methods
├── player-context.service.spec.ts               ✅ Unchanged - Phase 1-5 tests remain (3,647 lines)
├── player-context-history.service.spec.ts       📝 Modified - Add Phase 3 tests (~6 new tests)
├── player-context.interface.ts                  📝 Modified - Add interface methods
├── player-helpers.ts                            📝 Modified - Update default state
├── actions/index.ts                             📝 Modified - Export actions
└── selectors/index.ts                           📝 Modified - Export selectors

libs/features/player/src/lib/player-view/player-device-container/storage-container/
├── storage-container.component.ts               📝 Modified - Add history logic
├── storage-container.component.html             📝 Modified - Add play-history
└── directory-trail/
    ├── directory-trail.component.ts             📝 Modified - Add toggle handler
    ├── directory-trail.component.html           📝 Modified - Pass toggle props
    └── directory-navigate/
        ├── directory-navigate.component.ts      📝 Modified - Add toggle button
        └── directory-navigate.component.html    📝 Modified - Add button HTML
```

---

<details>
<summary><h2>Task 6: Refactor File Icon Mapping to Shared Utility</h2></summary>

**Goal**: Eliminate code duplication by extracting the file icon mapping logic into a shared utility function.

**Status**: 🔜 **PENDING** - To be implemented after core Phase 3 features complete

---

### Background

The `fileIcon` computed signal logic is currently duplicated across three components:

1. `file-item.component.ts` (directory files)
2. `search-results.component.ts` (uses `FileItemComponent`, which has the logic)
3. `history-entry.component.ts` (play history entries)

All three use identical switch logic mapping `FileItemType` to Material icons:

```typescript
readonly fileIcon = computed(() => {
  switch (this.entry().file.type) {
    case FileItemType.Song: return 'music_note';
    case FileItemType.Game: return 'sports_esports';
    case FileItemType.Image: return 'image';
    case FileItemType.Hex: return 'code';
    case FileItemType.Unknown:
    default: return 'insert_drive_file';
  }
});
```

---

### Open Question: Where Should This Utility Live?

**Options:**

**Option A: Domain Layer** (`libs/domain/src/lib/utils/file-icon.util.ts`)

- **Pros**: Icon mapping is domain knowledge - file types have canonical icons
- **Pros**: Available to all layers (domain, application, features)
- **Cons**: Couples domain to Material Icons (could be abstracted further)

**Option B: UI Layer** (`libs/ui/utils/src/lib/file-icon.util.ts`)

- **Pros**: Icon mapping is presentation concern
- **Pros**: Keeps Material Icons dependency at UI layer
- **Cons**: Less discoverable for feature components
- **Cons**: Creates dependency from features → ui/utils

**Option C: Utils Layer** (`libs/utils/src/lib/file-icon.util.ts`)

- **Pros**: Shared utility layer, clear location
- **Cons**: Not semantic - utils should be low-level helpers
- **Cons**: May not be the right home for domain-specific mapping

**Recommendation**: Discuss during implementation. Leaning toward **Option A (Domain Layer)** since the mapping represents domain knowledge about file types, though it does introduce a Material Icons dependency.

---

### Implementation Approach

**Step 6.1: Create File Icon Utility**

**File**: TBD based on decision above (e.g., `libs/domain/src/lib/utils/file-icon.util.ts`)

**Implementation:**

```typescript
import { FileItemType } from '../models/file-item-type.enum';

/**
 * Maps FileItemType to Material Icon name.
 *
 * @param type - The file item type to map
 * @returns Material icon name for the file type
 */
export function getFileIcon(type: FileItemType): string {
  switch (type) {
    case FileItemType.Song:
      return 'music_note';
    case FileItemType.Game:
      return 'sports_esports';
    case FileItemType.Image:
      return 'image';
    case FileItemType.Hex:
      return 'code';
    case FileItemType.Unknown:
    default:
      return 'insert_drive_file';
  }
}
```

**Step 6.2: Update FileItemComponent**

Replace computed signal with utility call:

```typescript
readonly fileIcon = computed(() => getFileIcon(this.fileItem().type));
```

**Step 6.3: Update HistoryEntryComponent**

Replace computed signal with utility call:

```typescript
readonly fileIcon = computed(() => getFileIcon(this.entry().file.type));
```

**Step 6.4: Add Tests**

Create `file-icon.util.spec.ts` with tests for all file types:

```typescript
describe('getFileIcon', () => {
  it('should return music_note for Song type', () => {
    expect(getFileIcon(FileItemType.Song)).toBe('music_note');
  });

  it('should return sports_esports for Game type', () => {
    expect(getFileIcon(FileItemType.Game)).toBe('sports_esports');
  });

  // ... etc for all types
});
```

**Step 6.5: Update Existing Component Tests**

Verify all component tests still pass after refactoring.

---

### Success Criteria

- [ ] Utility function created in agreed-upon location
- [ ] All three components updated to use utility
- [ ] Utility function has comprehensive test coverage
- [ ] All existing component tests still passing
- [ ] No duplication of icon mapping logic
- [ ] TypeScript compilation successful
- [ ] Linting passes

</details>

---

<details>
<summary><h2>Task 7: Create SearchResultItem Component for Consistency</h2></summary>

**Goal**: Extract search result item rendering into a dedicated child component, following the pattern established by `history-entry.component.ts` and `file-item.component.ts`.

**Status**: 🔜 **PENDING** - To be implemented after Task 6 (refactoring benefits this task)

---

### Background

Currently, `search-results.component.ts` directly uses `FileItemComponent` to render each search result. While this works, it creates inconsistency with the pattern established in this phase where list components use dedicated child components:

- **Directory Files**: `directory-files.component.ts` → `file-item.component.ts`
- **Play History**: `play-history.component.ts` → `history-entry.component.ts`
- **Search Results**: `search-results.component.ts` → **directly uses `FileItemComponent`** ⚠️

Creating `SearchResultItemComponent` will:

1. Maintain pattern consistency across all file list views
2. Allow search-specific presentation customization in the future
3. Provide clear component hierarchy and separation of concerns

---

### Implementation Approach

**Step 7.1: Create SearchResultItem Component**

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/search-results/search-result-item/search-result-item.component.ts`

**Implementation:**

```typescript
import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StorageItemComponent, StorageItemActionsComponent } from '@teensyrom-nx/ui/components';
import { FileItem } from '@teensyrom-nx/domain';
import { getFileIcon } from '@teensyrom-nx/domain'; // From Task 6

@Component({
  selector: 'lib-search-result-item',
  imports: [CommonModule, StorageItemComponent, StorageItemActionsComponent],
  templateUrl: './search-result-item.component.html',
  styleUrls: ['./search-result-item.component.scss'],
})
export class SearchResultItemComponent {
  // Inputs
  fileItem = input.required<FileItem>();
  selected = input<boolean>(false);
  isCurrentlyPlaying = input<boolean>(false);

  // Outputs
  itemSelected = output<FileItem>();
  itemDoubleClick = output<FileItem>();

  // Computed signals
  readonly fileIcon = computed(() => getFileIcon(this.fileItem().type));

  readonly formattedSize = computed(() => {
    return this.formatFileSize(this.fileItem().size);
  });

  // Event handlers
  onItemClick(): void {
    this.itemSelected.emit(this.fileItem());
  }

  onItemDoubleClick(): void {
    this.itemDoubleClick.emit(this.fileItem());
  }

  // Private helpers
  private formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`;
  }
}
```

**Step 7.2: Create SearchResultItem Template**

**File**: `search-result-item.component.html`

```html
<lib-storage-item
  [icon]="fileIcon()"
  iconColor="normal"
  [label]="fileItem().name"
  [selected]="selected()"
  (selectedChange)="onItemClick()"
  (activated)="onItemDoubleClick()"
>
  <lib-storage-item-actions [label]="formattedSize()"> </lib-storage-item-actions>
</lib-storage-item>
```

**Step 7.3: Create SearchResultItem Styles**

**File**: `search-result-item.component.scss`

```scss
:host {
  display: flex;
  width: 100%;
}
```

**Step 7.4: Update SearchResultsComponent**

Replace direct `FileItemComponent` usage with `SearchResultItemComponent`:

```typescript
// In search-results.component.ts
import { SearchResultItemComponent } from './search-result-item/search-result-item.component';

@Component({
  // ...
  imports: [CommonModule, SearchResultItemComponent, ScalingCardComponent, EmptyStateMessageComponent],
  // ...
})
```

```html
<!-- In search-results.component.html -->
@for (file of searchResults(); track file.path) {
<div
  class="file-list-item"
  [attr.data-item-path]="file.path"
  [attr.data-is-playing]="isCurrentlyPlaying(file)"
  [attr.data-has-error]="hasPlayerError() && isCurrentlyPlaying(file)"
>
  <lib-search-result-item
    [fileItem]="file"
    [selected]="isSelected(file)"
    [isCurrentlyPlaying]="isCurrentlyPlaying(file)"
    (itemSelected)="onFileSelected(file)"
    (itemDoubleClick)="onFileDoubleClick(file)"
  />
</div>
}
```

**Step 7.5: Write SearchResultItem Tests**

**File**: `search-result-item.component.spec.ts`

Follow the same test pattern as `history-entry.component.spec.ts` and `file-item.component.spec.ts`:

- Component creation
- File name display
- Icon mapping for all types
- Click event emission
- Double-click event emission
- Selected state styling
- File size formatting

**Step 7.6: Update SearchResults Tests**

Update `search-results.component.spec.ts` to reflect new component usage (if needed).

---

### Success Criteria

- [ ] `SearchResultItemComponent` created following established pattern
- [ ] Component uses `getFileIcon()` utility from Task 6
- [ ] `SearchResultsComponent` updated to use new child component
- [ ] Template and styles consistent with other item components
- [ ] Comprehensive test coverage (same tests as other item components)
- [ ] All existing tests still passing
- [ ] Consistent component hierarchy across all file list views:
  - ✅ Directory Files → FileItemComponent
  - ✅ Play History → HistoryEntryComponent
  - ✅ Search Results → SearchResultItemComponent

---

### Benefits

1. **Consistency**: All three file list views follow the same component pattern
2. **Maintainability**: Changes to search result presentation isolated to dedicated component
3. **Reusability**: Search-specific styling/behavior can be added without affecting other views
4. **Clarity**: Clear component hierarchy and separation of concerns
5. **Testability**: Dedicated tests for search result item presentation

</details>

---

## ✅ Success Criteria

**Functional Requirements:**

- [x] ~~Task 1: History view visibility state management complete~~ ✅ DONE
- [ ] Task 2: History entry component created
- [ ] Task 3: Play history component created
- [ ] Task 4: History toggle button added to directory trail
- [ ] Task 5: Integration into storage container complete
- [ ] Task 6: File icon utility refactored (optional cleanup)
- [ ] Task 7: SearchResultItem component created (optional consistency)
- [ ] History view toggle button appears in directory trail
- [ ] History view displays chronological list of played files
- [ ] Empty state shows when no history exists
- [ ] Currently playing file highlighted with pulsing effect
- [ ] Single-click selects, double-click launches from history
- [ ] Search automatically hides history view
- [ ] View switching animations smooth and consistent
- [ ] Multi-device support with independent history views

**Testing Requirements:**

- [x] ~~Task 1 tests: Phase 3 visibility tests passing~~ ✅ 6/6 tests passing
- [ ] Task 2 tests: History entry component tests passing
- [ ] Task 3 tests: Play history component tests passing
- [ ] Task 4 tests: Directory trail tests updated
- [ ] Task 5 tests: Storage container tests updated
- [ ] All unit tests passing for new components
- [ ] All integration tests passing
- [x] ~~PlayerContextService tests updated and passing~~ ✅ All 354 application tests passing
- [ ] Storage container tests updated and passing
- [x] ~~No existing tests broken~~ ✅ All Phase 1-5 tests still passing
- [ ] Manual testing checklist completed

**Quality Checks:**

- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors
- [ ] Code formatting is consistent
- [ ] No console errors in browser when using feature
- [ ] Follows established patterns from search-results component
- [ ] Component styles match existing design system

**Documentation:**

- [ ] Inline code comments for complex logic
- [ ] Component JSDoc comments added
- [ ] Update COMPONENT_LIBRARY.md if needed

**Ready for Production:**

- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Feature works across all supported devices
- [ ] Performance is acceptable (no lag during view switching)

---

## 📝 Notes & Considerations

### Design Decisions

- **Absolute Timestamps**: Using absolute times (e.g., "3:45 PM") for clarity and consistency
- **Hidden by Default**: History view requires explicit toggle to avoid cluttering UI
- **ScalingCard Pattern**: Following search-results component pattern for consistency
- **View Priority**: Search > History > Directory Files ensures clear hierarchy
- **No Auto-Hide**: Letting `animationTrigger` handle visibility (simpler than explicit state mutation)
- **Pulsing Highlight**: Using same visual indicator as directory-files for currently playing file

### Implementation Constraints

- **Phase 3 Only**: This phase focuses on UI and view switching - no new backend features
- **Browser-Style History**: Building on Phase 2's forward/backward navigation infrastructure
- **Device-Scoped**: Each device maintains independent history view visibility state

### Future Enhancements

- **Relative Timestamps Option**: Add user preference for relative vs absolute times
- **Launch Mode Badges**: Colorful badges showing Directory/Shuffle/Search modes
- **History Filtering**: Filter by file type or launch mode
- **History Search**: Search within history entries
- **Clear History Action**: Button to clear all history for device
- **History Stats**: Show aggregate statistics (most played, etc.)

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

---

## 💡 Open Questions for Phase 3

**Resolved:**

1. ✅ **Timestamp Display**: Absolute times (e.g., "3:45 PM")
2. ✅ **History View Default**: Hidden by default, toggle on to show
3. ✅ **Component Pattern**: Follow search-results.component.ts structure
4. ✅ **Animation Timing**: Use default ScalingCard animations (no custom duration needed)

**New Questions:**

- None at this time - all questions resolved during planning discussion
