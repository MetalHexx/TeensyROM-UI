# Refactoring Plan: Single Source of Truth for `launchMode`

**Goal**: Keep only `DevicePlayerState.launchMode` as the single source of truth. Remove all redundant copies.

**Key Principle**: Only user-initiated UI actions change mode. History navigation preserves current mode.

---

## **Phase 0: Baseline Testing** ✅
**Establish test baseline - MUST PASS before proceeding**
```bash
npx nx test --all
```

**CRITICAL**: Record baseline results. All phases must maintain or improve test pass rate.

**After each phase**:
1. Run `npx nx test --all`
2. Verify all tests pass
3. Fix any failures before moving to next phase
4. Commit changes with descriptive message

---

## **Phase 1: Fix History Navigation - Don't Restore Mode** 🔧

**Current Problem**: History navigation overwrites `DevicePlayerState.launchMode` from `HistoryEntry.launchMode`

### Files to modify:
1. **`navigate-backward-in-history.ts`** (line 94)
   - Remove `launchMode: historyEntry.launchMode,` from currentFile creation
   - Keep mode unchanged - history navigation is mode-agnostic

2. **`navigate-forward-in-history.ts`** (line 101)
   - Remove `launchMode: historyEntry.launchMode,` from currentFile creation

3. **`navigate-to-history-position.ts`** (line 70)
   - Remove `launchMode: entry.launchMode,` from currentFile creation

**Expected**: History navigation loads files but preserves current `DevicePlayerState.launchMode`

### Testing Phase 1:
```bash
npx nx test --all
```
- ✅ All tests must pass
- ✅ Fix any failures before Phase 2
- ✅ Commit: `"refactor: remove launchMode restore from history navigation"`

---

## **Phase 2: Fix Launch Actions - Use Parameter to Update Device Mode** 🔧

**Current Problem**: Launch helpers overwrite `DevicePlayerState.launchMode` from `LaunchedFile.launchMode` instead of using the action's parameter

### Files to modify:

1. **`launch-file-with-context.ts`** (lines 58, 65, 85, 93)
   - Pass `launchMode` parameter to `setPlayerLaunchSuccess()` and `setPlayerLaunchFailure()`
   - These helpers should use this parameter to update `DevicePlayerState.launchMode`

2. **`player-helpers.ts`** - Update helper signatures:
   - `setPlayerLaunchSuccess(store, deviceId, launchedFile, fileContext, launchMode, actionMessage)`
     - Add `launchMode` parameter
     - Use it to set `DevicePlayerState.launchMode` (line 127)
     - Don't use `launchedFile.launchMode` (which will be removed)

   - `setPlayerLaunchFailure(store, deviceId, launchedFile, fileContext, launchMode, errorMessage, actionMessage)`
     - Add `launchMode` parameter
     - Use it to set `DevicePlayerState.launchMode` (line 190)
     - Don't use `launchedFile.launchMode` (which will be removed)

3. **`launch-random-file.ts`** - Update calls to helpers:
   - Line 92: `setPlayerLaunchSuccess(..., LaunchMode.Shuffle, ...)`
   - Line 109: `setPlayerLaunchFailure(..., LaunchMode.Shuffle, ...)`

4. **Navigation helpers - Remove hardcoded modes**:
   - `setShuffleNavigationSuccess()` (line 292): Remove `launchMode: LaunchMode.Shuffle,`
   - `setShuffleNavigationFailure()` (line 330): Remove `launchMode: LaunchMode.Shuffle,`
   - `setDirectoryNavigationSuccess()` (line 366): Remove `launchMode: LaunchMode.Directory,`
   - `setDirectoryNavigationFailure()` (line 407): Remove `launchMode: LaunchMode.Directory,`

**Expected**: `DevicePlayerState.launchMode` updated from action parameters, not from stored objects

### Testing Phase 2:
```bash
npx nx test --all
```
- ✅ All tests must pass
- ✅ Fix any failures before Phase 3
- ✅ Commit: `"refactor: update DevicePlayerState.launchMode from action parameters"`

---

## **Phase 3: Remove LaunchedFile.launchMode** 🗑️

**Now safe to remove since it's no longer read or written**

### Files to modify:

1. **`player-store.ts`** (line 15)
   - Remove `launchMode: LaunchMode;` from `LaunchedFile` interface

2. **`player-helpers.ts`** (lines 223, 235)
   - `createLaunchedFile()`: Remove `launchMode` parameter from signature
   - Remove from return object

3. **Update all callers**:
   - `launch-file-with-context.ts` (lines 58, 85): Remove `launchMode` argument
   - `launch-random-file.ts` (lines 92, 109): Remove `launchMode` argument

### Testing Phase 3:
```bash
npx nx test --all
```
- ✅ All tests must pass
- ✅ Fix any failures before Phase 4
- ✅ Commit: `"refactor: remove LaunchedFile.launchMode property"`

---

## **Phase 4: Remove PlayerFileContext.launchMode** 🗑️

**Not used by navigation logic**

### Files to modify:

1. **`player-store.ts`** (line 24)
   - Remove `launchMode: LaunchMode;` from `PlayerFileContext` interface

2. **`player-helpers.ts`** (lines 247, 258)
   - `createPlayerFileContext()`: Remove `launchMode` parameter
   - Remove from return object

3. **Update all callers**:
   - `launch-file-with-context.ts` (lines 65, 93): Remove `launchMode` argument
   - `load-file-context.ts` (lines 39, 55): Remove `launchMode` argument

4. **`load-file-context.ts`** (line 23)
   - Remove `launchMode: LaunchMode;` from params interface
   - Remove from function body

5. **`player-context.service.ts`** (line 122)
   - Remove `launchMode: currentFile.launchMode,` from `loadFileContext` call

### Testing Phase 4:
```bash
npx nx test --all
```
- ✅ All tests must pass
- ✅ Fix any failures before Phase 5
- ✅ Commit: `"refactor: remove PlayerFileContext.launchMode property"`

---

## **Phase 5: Remove HistoryEntry.launchMode** 🗑️

**Not needed since history navigation doesn't change mode**

### Files to modify:

1. **`player-store.ts`** (line 37)
   - Remove `launchMode: LaunchMode;` from `HistoryEntry` interface

2. **`player-context.service.ts`** (line 408)
   - Remove `launchMode: currentFile.launchMode,` from history entry creation

### Testing Phase 5:
```bash
npx nx test --all
```
- ✅ All tests must pass
- ✅ Fix any failures before Phase 6
- ✅ Commit: `"refactor: remove HistoryEntry.launchMode property"`

---

## **Phase 6: Ensure Mode Setting in UI Components** ✅

**Verify UI components properly set mode when launching files**

### Files to verify:

1. **`directory-files.component.ts`** (line 156)
   - Already passes `launchMode: LaunchMode.Directory` ✅

2. **`search-results.component.ts`** (line 160)
   - Already passes `launchMode: LaunchMode.Search` ✅

3. **Shuffle toggle** (player-context.service.ts:131-139)
   - Already uses `updateLaunchMode()` ✅

**No changes needed** - just verification

### Testing Phase 6:
```bash
npx nx test --all
```
- ✅ All tests must pass (should be green, no changes made)
- ✅ Verify UI components properly set launchMode

---

## **Phase 7: Update Test Files** 🧹

**Remove `launchMode` from mock objects in tests**

### Files to update:
- `player-context.service.spec.ts` - Remove from LaunchedFile mocks
- `player-context-history.service.spec.ts` - Remove from LaunchedFile/HistoryEntry mocks
- `player-toolbar.component.spec.ts` - Remove from LaunchedFile mocks
- `storage-container.component.spec.ts` - Remove from LaunchedFile mocks
- `search-results.component.spec.ts` - Remove from LaunchFileContextRequest mocks
- `directory-files.component.spec.ts` - Remove from LaunchFileContextRequest mocks
- `play-history.component.spec.ts` - Remove from HistoryEntry mocks
- `history-entry.component.spec.ts` - Remove from HistoryEntry mocks

### Testing Phase 7:
```bash
npx nx test --all
```
- ✅ All tests must pass
- ✅ Fix any test failures from mock updates
- ✅ Commit: `"test: update mocks after launchMode cleanup"`

---

## **Phase 8: Final Verification** ✅

### Automated Testing:
```bash
# Run all tests
npx nx test --all

# Run linting
npx nx lint

# Type checking (if applicable)
npx nx typecheck
```

### Manual Verification Checklist:
- [ ] Launch from directory → Mode = Directory, next/prev navigates directory
- [ ] Launch from search → Mode = Search, next/prev navigates search results
- [ ] Toggle shuffle → Mode = Shuffle, next/prev launches random
- [ ] Navigate through history → **Mode stays unchanged**
- [ ] Launch from history while in shuffle → File loads, still in shuffle mode
- [ ] Toggle shuffle while in search mode → Switches to shuffle, next/prev launches random
- [ ] Toggle shuffle off → Returns to Directory mode

### Final Commit:
```bash
git add .
git commit -m "refactor: complete launchMode cleanup - single source of truth"
```

---

## **Expected Outcome**

✅ **Single source of truth**: Only `DevicePlayerState.launchMode` exists
✅ **Clean state transitions**: Only UI launches change mode
✅ **History is mode-agnostic**: Replays files without changing mode
✅ **No circular overwrites**: Mode comes from action parameters, not stored objects
✅ **All tests passing**: No regressions

---

## **State Transition Rules**

### What Changes LaunchMode:
1. ✅ Launch from directory → `DevicePlayerState.launchMode = Directory`
2. ✅ Launch from search → `DevicePlayerState.launchMode = Search`
3. ✅ Launch random/shuffle → `DevicePlayerState.launchMode = Shuffle`
4. ✅ Toggle shuffle → `DevicePlayerState.launchMode = Shuffle` or back to `Directory`

### What DOES NOT Change LaunchMode:
1. ❌ Navigate next/previous → Uses current mode, doesn't change it
2. ❌ Navigate through history → Preserves current mode (mode-agnostic)
3. ❌ Play/pause/stop → No effect on mode

---

## **Architecture Clarification**

### Before (Problematic):
- `DevicePlayerState.launchMode` - "Current mode"
- `LaunchedFile.launchMode` - Copy of mode when file launched
- `PlayerFileContext.launchMode` - Copy of mode when context created
- `HistoryEntry.launchMode` - Copy of mode when history recorded

**Problem**: Circular overwrites, unclear source of truth, mode gets restored from history

### After (Clean):
- `DevicePlayerState.launchMode` - **ONLY** source of truth
- `LaunchFileContextRequest.launchMode` - Parameter to SET the mode (not stored)
- All stored objects reference the device mode, never store copies

---

## **Systematic Testing Strategy**

### Test-Driven Refactoring Process:

1. **Never skip testing between phases**
   - Each phase is incremental and isolated
   - Failures are easier to debug when caught immediately
   - Tests act as regression safeguards

2. **Test execution after each phase:**
   ```bash
   npx nx test --all
   ```

3. **If tests fail:**
   - ❌ DO NOT proceed to next phase
   - 🔍 Debug and fix the failure
   - ✅ Re-run tests until green
   - 📝 Document what broke and how you fixed it

4. **Commit strategy:**
   - Commit after each successful phase
   - Use descriptive commit messages
   - Makes it easy to rollback if needed

5. **Final validation:**
   - All automated tests pass
   - Manual verification checklist completed
   - No linting errors
   - Type checking passes (if applicable)

### Risk Mitigation:

- **Small, incremental changes** - Each phase is focused and testable
- **Frequent testing** - Catch issues early when they're easy to fix
- **Git commits per phase** - Easy rollback if something goes wrong
- **Clear success criteria** - Know when a phase is complete

### Success Metrics:

✅ All tests pass at baseline (Phase 0)
✅ All tests pass after each phase (Phases 1-7)
✅ All tests pass at final verification (Phase 8)
✅ Manual verification checklist completed
✅ No increase in test failures or linting errors
✅ Code complexity reduced (fewer launchMode copies)
