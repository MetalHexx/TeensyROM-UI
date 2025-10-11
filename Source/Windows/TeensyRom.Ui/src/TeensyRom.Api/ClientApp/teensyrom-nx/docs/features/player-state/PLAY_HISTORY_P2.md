# Phase 2: History Navigation in Shuffle Mode

## 🎯 Objective

Implement backward and forward navigation through play history when in shuffle mode, replacing the current "launch another random file" behavior for previous navigation. This enables users to navigate their playback timeline using familiar browser-style controls, making shuffle mode more predictable and user-friendly.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [Play History Planning Document](./PLAY_HISTORY_PLANNING.md) - High-level feature plan and Phase 2 requirements
- [ ] [Player Domain Design Document](./PLAYER_DOMAIN_DESIGN.md) - Technical design and architecture
- [ ] [Phase 1 Implementation](./PLAY_HISTORY_P1.md) - Foundation that Phase 2 builds upon

**Standards & Guidelines:**
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Behavioral testing approaches
- [ ] [State Standards](../../STATE_STANDARDS.md) - State mutation patterns with updateState()
- [ ] [Store Testing Guide](../../STORE_TESTING.md) - Testing through public APIs

---

## 📂 File Structure Overview

```
libs/application/src/lib/player/
├── player-store.ts                          📝 Modified - Add history navigation actions
├── player-context.service.ts                📝 Modified - Orchestrate history navigation
├── player-context.service.spec.ts           📝 Modified - Add Phase 2 behavioral tests
├── actions/
│   ├── index.ts                             📝 Modified - Export new actions
│   ├── navigate-backward-in-history.ts      ✨ New - Navigate backward through history
│   ├── navigate-forward-in-history.ts       ✨ New - Navigate forward through history
│   ├── navigate-next.ts                     📝 Modified - Check history before random launch
│   └── navigate-previous.ts                 📝 Modified - Use history navigation in shuffle
```

---

## 📋 Implementation Guidelines

> **IMPORTANT - Testing Policy:**
> - **Favor behavioral testing** - test observable behaviors, not implementation details
> - Include tests **within each task** as work progresses, not at the end
> - Test through `PlayerContextService` public API, not store actions directly
> - See [Store Testing Guide](../../STORE_TESTING.md) for testing patterns

> **IMPORTANT - Progress Tracking:**
> - **Mark checkboxes ✅ as you complete each subtask**
> - Update progress throughout implementation, not just at the end

---

<details open>
<summary><h3>Task 1: Create Navigate Backward in History Action</h3></summary>

**Purpose**: Implement the action that moves backward through play history by decrementing the position and re-launching the file at that position. This action only operates when history exists and position allows backward movement.

**Related Documentation:**
- [Play History Planning - Phase 2](./PLAY_HISTORY_PLANNING.md#phase-2-history-navigation-in-shuffle-mode) - Phase 2 requirements
- [State Standards](../../STATE_STANDARDS.md) - Use `updateState()` with `actionMessage` for all mutations

**Implementation Subtasks:**
- [ ] **Create Action File**: Create `navigate-backward-in-history.ts` in `actions/` folder
- [ ] **Define Action Interface**: Create `NavigateBackwardInHistoryParams` with `deviceId` property
- [ ] **Implement Action Logic**:
  - Validate player state exists
  - Check history exists and has entries
  - Calculate new position (current position - 1)
  - Validate new position is >= 0
  - Get history entry at new position
  - Launch file from history entry using `IPlayerService.launchFile()`
  - Update state with new position and launched file on success
  - Handle incompatible files with error state
  - Handle launch failures with error state
- [ ] **Export Action**: Add to `actions/index.ts` exports
- [ ] **Integrate with Store**: Add `navigateBackwardInHistory` method to `PlayerStore`

**Testing Subtask:**
- [ ] **Write Tests**: Test behaviors for this task (see Testing Focus below)

**Key Implementation Notes:**
- Action should return early if no history or already at position 0
- Use `StorageKeyUtil.parse()` to extract `storageType` from history entry's `storageKey`
- Re-use existing helper patterns like `setDirectoryNavigationSuccess()` if applicable
- Position should be updated **after** successful file launch, not before
- Do NOT call `recordHistory()` - context service will handle that
- Launch file from `entry.file.path` using the storage type from entry's `storageKey`

**Testing Focus for Task 1:**

> Focus on **behavioral testing** through `PlayerContextService`, not direct store testing

**Behaviors to Test:**
- [ ] **Navigate backward decrements position**: After navigating backward, position is 1 less than before
- [ ] **Navigate backward launches correct file**: File from history[position - 1] is launched
- [ ] **Cannot navigate backward at position 0**: Attempting to navigate backward at start is a no-op
- [ ] **Navigate backward with incompatible file**: Error state is set, position does not change
- [ ] **Navigate backward updates currentFile**: Current file is updated to the history entry's file

**Testing Reference:**
- See [Store Testing Guide](../../STORE_TESTING.md) for testing through `PlayerContextService`
- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns

</details>

---

<details open>
<summary><h3>Task 2: Create Navigate Forward in History Action</h3></summary>

**Purpose**: Implement the action that moves forward through play history by incrementing the position and re-launching the file at that position. This enables users to navigate forward after going backward in history.

**Related Documentation:**
- [Play History Planning - Scenario 5](./PLAY_HISTORY_PLANNING.md#scenario-5-navigate-forward-after-going-backward) - Forward navigation behavior
- [State Standards](../../STATE_STANDARDS.md) - State mutation patterns

**Implementation Subtasks:**
- [ ] **Create Action File**: Create `navigate-forward-in-history.ts` in `actions/` folder
- [ ] **Define Action Interface**: Create `NavigateForwardInHistoryParams` with `deviceId` property
- [ ] **Implement Action Logic**:
  - Validate player state exists
  - Check history exists and has entries
  - Check position is not -1 (not at end)
  - Calculate new position (current position + 1)
  - Validate new position is <= entries.length - 1
  - Get history entry at new position
  - Launch file from history entry using `IPlayerService.launchFile()`
  - Update state with new position and launched file on success
  - Handle incompatible files with error state
  - Handle launch failures with error state
- [ ] **Export Action**: Add to `actions/index.ts` exports
- [ ] **Integrate with Store**: Add `navigateForwardInHistory` method to `PlayerStore`

**Testing Subtask:**
- [ ] **Write Tests**: Test behaviors for this task (see Testing Focus below)

**Key Implementation Notes:**
- Action should return early if position is -1 (at end) or position >= entries.length - 1
- Similar structure to backward navigation but increments position
- Position should be updated **after** successful file launch
- Do NOT call `recordHistory()` - context service will handle that
- If navigating to last entry, position should be set to that index, NOT -1

**Testing Focus for Task 2:**

**Behaviors to Test:**
- [ ] **Navigate forward increments position**: After navigating forward, position is 1 more than before
- [ ] **Navigate forward launches correct file**: File from history[position + 1] is launched
- [ ] **Cannot navigate forward at end**: When position is -1, forward navigation is a no-op
- [ ] **Cannot navigate forward at last entry**: When at entries.length - 1, forward navigation is a no-op
- [ ] **Navigate forward with incompatible file**: Error state is set, position does not change
- [ ] **Navigate forward updates currentFile**: Current file is updated to the history entry's file

**Testing Reference:**
- See [Store Testing Guide](../../STORE_TESTING.md) for testing patterns
- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing

</details>

---

<details open>
<summary><h3>Task 3: Modify Navigate Next Action for History Support</h3></summary>

**Purpose**: Update the existing `navigateNext` action to check for forward history availability in shuffle mode. If forward history exists, navigate forward through history; otherwise, launch a new random file (existing behavior).

**Related Documentation:**
- [Play History Planning - Scenario 5](./PLAY_HISTORY_PLANNING.md#scenario-5-navigate-forward-after-going-backward) - Next button behavior with history
- [Navigate Next Action](../../../libs/application/src/lib/player/actions/navigate-next.ts) - Current implementation

**Implementation Subtasks:**
- [ ] **Add History Check**: In shuffle mode branch, check `canNavigateForwardInHistory` before launching random
- [ ] **Conditional Logic**:
  - If can navigate forward: Call `navigateForwardInHistory` action (delegate to new action)
  - If cannot navigate forward: Keep existing random file launch behavior
- [ ] **Maintain Directory/Search Behavior**: Ensure Directory and Search modes continue to use file context navigation (no changes needed)

**Testing Subtask:**
- [ ] **Write Tests**: Test behaviors for this task (see Testing Focus below)

**Key Implementation Notes:**
- This modifies existing `navigate-next.ts`, does not create a new file
- The shuffle mode branch (lines 34-52) needs conditional logic added
- Check position and history before deciding which path to take
- Random launch should still happen when position is -1 (at end of history)
- Do NOT duplicate code - call the new `navigateForwardInHistory` action when appropriate

**Testing Focus for Task 3:**

**Behaviors to Test:**
- [ ] **Next in shuffle with forward history**: When position < entries.length - 1, next navigates forward in history
- [ ] **Next in shuffle at end of history**: When position is -1, next launches new random file
- [ ] **Next in shuffle after reaching last history entry**: When at entries.length - 1, next launches new random file
- [ ] **Next in directory mode unchanged**: Directory mode still navigates through file context
- [ ] **Next in search mode unchanged**: Search mode still navigates through file context

**Testing Reference:**
- See [Store Testing Guide](../../STORE_TESTING.md) for testing approach

</details>

---

<details open>
<summary><h3>Task 4: Modify Navigate Previous Action for History Support</h3></summary>

**Purpose**: Update the existing `navigatePrevious` action to use history navigation in shuffle mode instead of launching a random file. This replaces the "random file on previous" behavior with proper backward history navigation.

**Related Documentation:**
- [Play History Planning - Scenario 4](./PLAY_HISTORY_PLANNING.md#scenario-4-navigate-backward-in-shuffle-mode) - Previous button behavior
- [Navigate Previous Action](../../../libs/application/src/lib/player/actions/navigate-previous.ts) - Current implementation

**Implementation Subtasks:**
- [ ] **Replace Random Launch Logic**: In shuffle mode branch (lines 34-52), replace random launch with history navigation
- [ ] **Add History Check**: Check `canNavigateBackwardInHistory` before attempting navigation
- [ ] **Conditional Logic**:
  - If position > 0: Decrement position normally
  - If position == 0: Wrap to end (set position to entries.length - 1)
  - If position == -1 with entries: Set position to entries.length - 1
  - Call `navigateBackwardInHistory` action for all cases where history exists
- [ ] **Maintain Directory/Search Behavior**: Ensure Directory and Search modes continue to use file context navigation (no changes)

**Testing Subtask:**
- [ ] **Write Tests**: Test behaviors for this task (see Testing Focus below)

**Key Implementation Notes:**
- This completely changes shuffle mode behavior for previous navigation
- The comment on line 35 "launches another random file" is no longer accurate - update it
- **Wraparound behavior**: When at position 0, wrap to entries.length - 1 (circular navigation)
- Do NOT duplicate code - call the new `navigateBackwardInHistory` action with calculated position

**Testing Focus for Task 4:**

**Behaviors to Test:**
- [ ] **Previous in shuffle with history**: When position > 0 or position is -1 with entries, previous navigates backward
- [ ] **Previous wraps at position 0**: When position is 0, previous wraps to end (entries.length - 1)
- [ ] **Previous wraps at position -1 with entries**: When at end, previous goes to most recent entry
- [ ] **Previous in directory mode unchanged**: Directory mode still navigates through file context
- [ ] **Previous in search mode unchanged**: Search mode still navigates through file context
- [ ] **Previous with no history**: When no history exists, previous is a no-op

**Testing Reference:**
- See [Store Testing Guide](../../STORE_TESTING.md) for testing patterns

</details>

---

<details open>
<summary><h3>Task 5: Update PlayerContextService Orchestration</h3></summary>

**Purpose**: Modify the `next()` and `previous()` methods in `PlayerContextService` to properly orchestrate history navigation, including directory context loading and history recording after navigation.

**Related Documentation:**
- [Player Context Service](../../../libs/application/src/lib/player/player-context.service.ts) - Current orchestration logic
- [State Standards](../../STATE_STANDARDS.md) - Orchestration vs action responsibilities

**Implementation Subtasks:**
- [ ] **Update `next()` Method**:
  - Check launch mode and history state
  - Determine whether to use history navigation or default next behavior
  - Call appropriate store action
  - Load directory context if needed (for history entries)
  - Record history if navigation was successful
  - Setup timer if navigation was successful
- [ ] **Update `previous()` Method**:
  - Check launch mode and history state
  - Determine whether to use history navigation or default previous behavior
  - Call appropriate store action
  - Load directory context if needed (for history entries)
  - Record history if navigation was successful
  - Setup timer if navigation was successful
- [ ] **Handle History Recording**: Decide when to record history vs when not to
  - Navigating through existing history should NOT create new entries
  - Need to track "navigating in history" vs "launching new file"

**Testing Subtask:**
- [ ] **Write Tests**: Test behaviors for this task (see Testing Focus below)

**Key Implementation Notes:**
- History navigation (backward/forward through existing entries) should NOT record new history entries
- Only launching NEW files should record history
- May need a flag or check to differentiate "navigating history" from "launching new file"
- Directory context should be loaded for history entries to show in file list
- Consider: should `recordHistoryIfSuccessful()` be called after history navigation? (Probably NOT)
- Timer setup should still happen after history navigation (music files need timers)

**Critical Decision Point:**

When user navigates backward in history then presses next:
- **Scenario A**: Next navigates forward in history → Do NOT record new history entry
- **Scenario B**: Next launches new file (when at end) → Record new history entry

Need to ensure `recordHistoryIfSuccessful()` is only called when launching NEW files, not when navigating existing history.

**Testing Focus for Task 5:**

**Behaviors to Test:**
- [ ] **History navigation does not record new entries**: Navigating backward then forward does not create duplicate entries
- [ ] **New file launch records history**: Launching new file after navigating backward records new entry
- [ ] **Directory context loads for history entries**: After history navigation, directory files are available
- [ ] **Timer setup after history navigation**: Music files have timers after navigating in history
- [ ] **Error handling in history navigation**: Failed history navigation does not corrupt state

**Testing Reference:**
- See [Store Testing Guide](../../STORE_TESTING.md) for orchestration testing
- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral patterns

</details>

---

<details open>
<summary><h3>Task 6: Comprehensive Phase 2 Integration Testing</h3></summary>

**Purpose**: Add comprehensive behavioral tests to `player-context.service.spec.ts` that validate all Phase 2 history navigation scenarios end-to-end, including boundary conditions and mode-specific behaviors.

**Related Documentation:**
- [Play History Planning - User Scenarios](./PLAY_HISTORY_PLANNING.md#history-navigation-scenarios) - Scenarios 4-8
- [Store Testing Guide](../../STORE_TESTING.md) - Behavioral testing methodology

**Implementation Subtasks:**
- [ ] **Create Phase 2 Test Suite**: Add `describe('Phase 2: History Navigation in Shuffle Mode', () => {})` block
- [ ] **Test Backward Navigation**:
  - Navigate backward moves to previous entry
  - Backward at position 0 is handled correctly
  - Backward with multiple entries
  - Backward launches correct file
- [ ] **Test Forward Navigation**:
  - Navigate forward moves to next entry
  - Forward at end (position -1) is handled correctly
  - Forward at last entry is handled correctly
  - Forward launches correct file
- [ ] **Test Next Button Integration**:
  - Next with forward history available navigates forward
  - Next at end of history launches random file
  - Next in directory mode still uses file context
- [ ] **Test Previous Button Integration**:
  - Previous with history navigates backward
  - Previous at position 0 follows design decision
  - Previous in directory mode still uses file context
- [ ] **Test History Recording**:
  - History navigation does not record new entries
  - New file launch after history navigation records entry
  - Forward history clearing when launching new file
- [ ] **Test Browser-Style Behavior**:
  - Go backward, launch new file, forward history is cleared
  - Position tracking through navigation sequence
  - Current file updates correctly throughout navigation
- [ ] **Test Mode Restrictions**:
  - History navigation only works in shuffle mode
  - Directory mode unaffected by history navigation
  - Search mode unaffected by history navigation
- [ ] **Test Edge Cases**:
  - Empty history
  - Single entry in history
  - Incompatible file in history
  - Failed file launch during history navigation

**Testing Subtask:**
- [ ] **All Tests Passing**: Ensure all new tests pass consistently

**Key Implementation Notes:**
- Tests should use the existing test setup and mocking patterns
- Mock `IPlayerService.launchFile` to return history entry files
- Use `nextTick()` helper for async operations
- Test complete workflows, not individual actions
- Verify observable outcomes: currentFile, position, history entries, error states

**Testing Focus for Task 6:**

**Behaviors to Test:**
- [ ] **Complete backward navigation workflow**: Launch 3 files, navigate backward twice, verify position and currentFile
- [ ] **Complete forward navigation workflow**: Navigate backward, then forward, verify restoration
- [ ] **Browser-style history clearing**: Navigate backward, launch new file, verify forward history cleared
- [ ] **Boundary conditions**: Test all edge cases (start, end, single entry, empty)
- [ ] **Mode isolation**: Verify shuffle-only behavior, directory/search unaffected
- [ ] **Error resilience**: Failed launches during history navigation handled gracefully
- [ ] **Position tracking accuracy**: Position updates correctly through all navigation paths
- [ ] **History integrity**: History recording only on new launches, not history navigation

**Testing Reference:**
- See [Store Testing Guide](../../STORE_TESTING.md) for complete testing patterns
- See existing Phase 1 tests in spec file for patterns to follow

</details>

---

## 🗂️ Files Modified or Created

**New Files:**
- `libs/application/src/lib/player/actions/navigate-backward-in-history.ts`
- `libs/application/src/lib/player/actions/navigate-forward-in-history.ts`

**Modified Files:**
- `libs/application/src/lib/player/player-store.ts` - Add `navigateBackwardInHistory` and `navigateForwardInHistory` methods
- `libs/application/src/lib/player/actions/index.ts` - Export new history navigation actions
- `libs/application/src/lib/player/actions/navigate-next.ts` - Add history check before random launch in shuffle mode
- `libs/application/src/lib/player/actions/navigate-previous.ts` - Replace random launch with history navigation in shuffle mode
- `libs/application/src/lib/player/player-context.service.ts` - Update `next()` and `previous()` orchestration for history
- `libs/application/src/lib/player/player-context.service.spec.ts` - Add Phase 2 behavioral tests (~15-20 tests)

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
> - **Favor behavioral testing** - test what users/consumers observe, not how it's implemented
> - **Test as you go** - tests are integrated into each task's subtasks, not deferred to the end
> - **Test through public APIs** - test through `PlayerContextService`, not store actions directly
> - **Mock at boundaries** - mock `IPlayerService` only, not internal player logic

> **Reference Documentation:**
> - **All tasks**: [Testing Standards](../../TESTING_STANDARDS.md) - Core behavioral testing approach
> - **All tasks**: [Store Testing](../../STORE_TESTING.md) - Testing through service layer

### Where Tests Are Written

**Tests are embedded in each task above** with:
- **Testing Subtask**: Checkbox in the task's subtask list
- **Testing Focus**: "Behaviors to Test" section listing observable outcomes
- **Testing Reference**: Links to relevant testing documentation

**Complete each task's testing subtask before moving to the next task.**

### Test Execution Commands

**Running Tests:**
```bash
# Run application library tests (includes PlayerContextService tests)
npx nx test application --run

# Run tests in watch mode during development
npx nx test application --watch

# Run with coverage
npx nx test application --run --coverage
```

**Expected Test Count:**
- Phase 2 will add approximately **15-20 new behavioral tests**
- Total test count after Phase 2: ~336-341 tests (currently 321)

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All implementation tasks (1-6) completed and checked off
- [ ] All subtasks within each task completed
- [ ] `navigateBackwardInHistory` action created and integrated
- [ ] `navigateForwardInHistory` action created and integrated
- [ ] `navigateNext` action modified for history support
- [ ] `navigatePrevious` action modified for history navigation
- [ ] `PlayerContextService` orchestration updated
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)
- [ ] State management follows [State Standards](../../STATE_STANDARDS.md)

**Testing Requirements:**
- [ ] All testing subtasks completed within each task
- [ ] All behavioral test checkboxes verified (see each task's Testing Focus)
- [ ] Tests written alongside implementation (not deferred)
- [ ] Phase 2 test suite added to `player-context.service.spec.ts`
- [ ] All tests passing with no failures (~336-341 total tests)
- [ ] Backward navigation tests passing (Task 1)
- [ ] Forward navigation tests passing (Task 2)
- [ ] Next button integration tests passing (Task 3)
- [ ] Previous button integration tests passing (Task 4)
- [ ] Orchestration tests passing (Task 5)
- [ ] Integration tests passing (Task 6)

**Quality Checks:**
- [ ] No TypeScript errors or warnings (`npx nx run teensyrom-ui:typecheck`)
- [ ] Linting passes with no errors (`npx nx run teensyrom-ui:lint`)
- [ ] Code formatting is consistent
- [ ] No console errors in browser/terminal when running application

**Documentation:**
- [ ] Inline code comments added for history navigation logic
- [ ] Action files have JSDoc comments explaining behavior
- [ ] Complex logic in orchestration is documented

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] History navigation works correctly in shuffle mode
- [ ] Directory and search modes unaffected by changes
- [ ] Ready to proceed to Phase 3 (Play History UI Components)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **History Navigation Shuffle-Only**: History navigation is restricted to shuffle mode only. Directory and Search modes continue using file context navigation. This prevents confusion where previous/next buttons have different behaviors in different contexts.

- **Position -1 Sentinel Value**: Position -1 represents "at the end of history" where no forward navigation is possible. This differs from position `entries.length - 1` which is the last actual entry that CAN navigate forward to launch a new file.

- **No History Recording During Navigation**: Navigating through existing history (backward/forward) does not create new history entries. Only launching NEW files records history. This prevents history pollution from users exploring their playback timeline.

### Implementation Constraints

- **Must Not Break Existing Modes**: Directory and Search mode navigation must remain unchanged. Only shuffle mode behavior is modified.

- **Backward Compatibility**: Existing tests for directory and search navigation must continue passing without modification.

- **Error Handling**: File launch failures during history navigation must be handled gracefully without corrupting history position or state.

### Open Questions

**Decision 1: Wraparound Behavior at Position 0** ✅ DECIDED

When at the start of history (position 0) and user clicks previous:
- **Option A**: Do nothing / stay at position 0 (disable button in UI)
- **Option B**: Wrap to end (launch most recent entry) - **✅ SELECTED**
- **Option C**: Launch new random file

**Decision**: Option B - Wrap to end. When at position 0, pressing previous wraps to the most recent entry (position = entries.length - 1). This creates a circular navigation experience in shuffle mode.

**Question 2: History Recording After Navigation**

Should navigating in history update timestamps or any metadata?
- **Recommended**: No. History entries are immutable. Navigation doesn't modify existing entries.

**Question 3: Directory Context Loading**

After navigating to a history entry, should we load the directory context for that file?
- **Recommended**: Yes, for consistency. The file list should show the file's directory context.

### Future Enhancements (Phase 3+)

- **History UI Component**: Visual display of play history timeline
- **History Toggle Button**: Button in directory trail to show/hide history view
- **Timestamp Display**: Show relative or absolute times for history entries
- **Clear History Action**: User control to clear all or partial history

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery 1**: [Add findings during implementation]
- **Discovery 2**: [Add unexpected behaviors or edge cases found]

</details>

---

## 💡 Agent Implementation Guide

> **Instructions for AI agents implementing this phase**

### Before Starting Implementation

**Review Required Documentation:**
1. Read all documents in "Required Reading" section above
2. Understand Phase 1 implementation (foundation for Phase 2)
3. Review user scenarios 4-8 in planning document
4. Understand browser-style history behavior pattern

**Clarify Open Questions:**
1. **Wraparound Behavior**: Get user decision on what happens at position 0
2. **History Recording**: Confirm history navigation should NOT record new entries
3. **Directory Context**: Confirm directory should load for history entries

### During Implementation

**Task Execution Order:**
1. **Tasks 1-2 First**: Create backward/forward navigation actions (foundation)
2. **Tasks 3-4 Next**: Modify existing navigation actions to use history
3. **Task 5**: Update orchestration (brings everything together)
4. **Task 6 Last**: Comprehensive integration testing (validates everything)

**Testing Integration:**
- Write tests for each task as you complete it
- Test through `PlayerContextService` public API only
- Use existing test patterns from Phase 1 as reference
- Verify all Phase 1 tests still pass after changes

**Progress Tracking:**
1. ✅ Mark each subtask checkbox as completed
2. 📝 Update "Discoveries During Implementation" section with findings
3. ✅ Mark testing checkboxes as behaviors are verified
4. 📊 Update success criteria as work progresses

### Key Implementation Patterns

**Action Structure** (follow existing patterns):
```typescript
export function actionName(store: WritableStore<PlayerState>, playerService: IPlayerService) {
  return {
    actionName: async ({ deviceId }: Params): Promise<void> => {
      const actionMessage = createAction('action-name');
      // Validate state
      // Perform operation
      // Update state using updateState(store, actionMessage, ...)
    },
  };
}
```

**Testing Pattern** (follow Phase 1 tests):
```typescript
describe('Phase 2: History Navigation in Shuffle Mode', () => {
  beforeEach(() => {
    service.initializePlayer(deviceId);
  });

  it('should navigate backward in history', async () => {
    // Setup: Launch files to create history
    // Action: Navigate backward
    // Verify: Position and currentFile updated correctly
  });
});
```

### Remember

- **Test as you go** - don't defer testing to the end
- **Behavioral focus** - test what users observe, not how code works internally
- **Follow existing patterns** - Phase 1 established the patterns, Phase 2 extends them
- **Mark progress incrementally** - check off items as you complete them
- **Ask questions early** - clarify open questions before implementing

---

## 🎓 Quick Reference

### Current Behavior (Phase 1)
- Shuffle mode: Previous = random file, Next = random file
- History tracked but not used for navigation
- Position always -1 (at end)

### New Behavior (Phase 2)
- Shuffle mode: Previous = navigate backward (if possible), Next = navigate forward OR random
- History used for navigation in shuffle mode
- Position tracks location in history (0 to entries.length-1, or -1 at end)

### Key Methods
- `navigateBackwardInHistory()` - Move position backward, launch history[position - 1]
- `navigateForwardInHistory()` - Move position forward, launch history[position + 1]
- `canNavigateBackwardInHistory()` - Returns true if position > 0 or position == -1 with entries
- `canNavigateForwardInHistory()` - Returns true if position != -1 and position < entries.length - 1

### Testing Through
- `PlayerContextService.next()` - Public method for next navigation
- `PlayerContextService.previous()` - Public method for previous navigation
- `PlayerContextService.getPlayHistory()` - Read history state
- `PlayerContextService.getCurrentHistoryPosition()` - Read position
