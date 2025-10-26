# Browser Back/Forward Navigation Support for Deep Linking

## 🎯 Objective

Enable browser back/forward button navigation to trigger file relaunches based on URL query parameters. When users navigate through browser history (back/forward buttons), the application should detect URL changes and automatically relaunch the file specified in the query parameters.

**User Value**: Users can navigate through their file playback history using familiar browser controls (back/forward buttons), making the player feel like a native web application with proper URL state synchronization.

**Technical Challenge**: The application currently updates the browser URL via `location.go()` when files are launched, but Angular doesn't re-run the route resolver on query parameter changes. We need to listen for browser `popstate` events and manually trigger file relaunches.

---

## 📚 Required Reading

> Review these documents before starting implementation.

**Feature Documentation:**
- [x] [DEEP_LINKING_PLAN.md](./DEEP_LINKING_PLAN.md) - Deep linking architecture and Phase 1-3 implementation
- [x] [player-route.resolver.ts](../../../libs/app/navigation/src/lib/player-route.resolver.ts) - Current resolver implementation
- [x] [player-context.service.ts](../../../libs/application/src/lib/player/player-context.service.ts) - Service that handles file launching

**Standards & Guidelines:**
- [x] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [x] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices
- [x] [Service Standards](../../SERVICE_STANDARDS.md) - Service layer patterns and DI
- [x] [E2E Testing Guide](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - E2E testing with Cypress

---

## 📂 File Structure Overview

```
libs/application/src/lib/player/
├── player-context.service.ts              📝 Modified - Add popstate listener logic
└── player-context.interface.ts            📝 Modified - Add listener lifecycle methods

libs/app/navigation/src/lib/
└── player-route.resolver.ts               📝 Modified - Start popstate listener

apps/teensyrom-ui-e2e/src/e2e/player/
└── deep-linking.cy.ts                     📝 Modified - Add browser nav test
```

---

## 🔍 Problem Analysis

### Current Behavior

1. **File Launch → URL Update**: When a file is launched via `PlayerContextService.launchFileWithContext()`, the method calls `updateUrlForLaunchedFile()` which uses `location.go()` to update the browser URL with query parameters (`device`, `storage`, `path`, `file`)

2. **Browser Back/Forward Clicked**: When user clicks browser back/forward, the URL changes but **nothing happens** in the application

3. **Why Nothing Happens**:
   - `location.go()` updates the URL **without triggering Angular routing**
   - Route resolvers only run on **initial navigation to `/player`**, not on query parameter changes
   - No code listens for browser `popstate` events
   - No mechanism exists to detect URL changes and relaunch files

### Root Cause

**Missing Listener**: The application needs to listen for browser `popstate` events (fired when back/forward buttons are clicked) and parse the URL query parameters to determine which file to relaunch.

---

## 📋 Implementation Tasks

<details open>
<summary><h3>Task 1: Add PopState Listener to PlayerContextService</h3></summary>

**Purpose**: Add browser `popstate` event listener to detect back/forward navigation and relaunch files based on URL query parameters.

**Related Documentation:**
- [Location API - MDN](https://developer.mozilla.org/en-US/docs/Web/API/Location)
- [PopState Event - MDN](https://developer.mozilla.org/en-US/docs/Web/API/Window/popstate_event)
- [Service Standards](../../SERVICE_STANDARDS.md) - Dependency injection patterns

**Implementation Subtasks:**
- [ ] **Add private field**: Add `popstateListener` property to track the event listener for cleanup
- [ ] **Create startListeningToPopState method**: Public method that registers `popstate` event listener
- [ ] **Create stopListeningToPopState method**: Public method that removes `popstate` event listener
- [ ] **Create handlePopState method**: Private handler that parses URL and relaunches files
- [ ] **Add to IPlayerContext interface**: Add lifecycle methods to contract interface
- [ ] **Update PLAYER_CONTEXT injection token**: Ensure contract includes new methods

**Testing Subtask:**
- [ ] **Write Tests**: Test popstate listener behaviors (see Testing section below)

**Key Implementation Notes:**

**PopState Listener Pattern**:
- Use native browser `window.addEventListener('popstate', ...)` not Angular Router events
- Listener fires when user clicks back/forward buttons
- Does NOT fire when `location.go()` is called (only actual browser navigation)
- Must cleanup listener to avoid memory leaks

**URL Parsing Strategy**:
- Use Angular `ActivatedRoute` or parse `window.location.search` directly
- Extract query parameters: `device`, `storage`, `path`, `file`
- If all 4 parameters present → relaunch file
- If only 3 parameters (no `file`) → navigate to directory only
- If fewer parameters → do nothing (let normal navigation work)

**Integration with Existing Code**:
- Reuse existing `launchFileWithContext()` method for file launches
- Reuse `StorageStore.navigateToDirectory()` for directory-only navigation
- Handle missing files gracefully (show alert via `ALERT_SERVICE`)
- Prevent duplicate URL updates (set flag during popstate handling)

**Lifecycle Management**:
- Start listening: Called from route resolver after initialization
- Stop listening: Called when player is destroyed (optional, but clean)
- Guard against multiple registrations

**Testing Focus for Task 1:**

> Focus on **behavioral testing** - what observable outcomes occur?

**Behaviors to Test:**
- [ ] **Behavior A**: `startListeningToPopState()` registers event listener without errors
- [ ] **Behavior B**: `stopListeningToPopState()` removes event listener and allows re-registration
- [ ] **Behavior C**: PopState with valid 4 parameters relaunches file
- [ ] **Behavior D**: PopState with 3 parameters navigates to directory without launching
- [ ] **Behavior E**: PopState with missing parameters does nothing
- [ ] **Behavior F**: PopState with invalid file shows warning alert
- [ ] **Behavior G**: URL update suppressed during popstate handling (no duplicate history entries)

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns
- See [Service Standards](../../SERVICE_STANDARDS.md) for service testing examples

</details>

---

<details open>
<summary><h3>Task 2: Update PlayerContextService Implementation</h3></summary>

**Purpose**: Implement the popstate listener logic in `PlayerContextService` to handle browser navigation events.

**Related Documentation:**
- [player-context.service.ts](../../../libs/application/src/lib/player/player-context.service.ts) - Current implementation
- [Service Standards](../../SERVICE_STANDARDS.md#dependency-injection) - DI patterns

**Implementation Subtasks:**
- [ ] **Inject dependencies**: Add `ActivatedRoute` and `ALERT_SERVICE` to constructor if not already present
- [ ] **Add private fields**: Add `popstateListener` and `isHandlingPopState` flag
- [ ] **Implement startListeningToPopState**: Register event listener with `handlePopState` handler
- [ ] **Implement stopListeningToPopState**: Remove event listener and cleanup
- [ ] **Implement handlePopState**: Parse URL, validate parameters, launch file or navigate directory
- [ ] **Suppress URL updates during popstate**: Check `isHandlingPopState` flag in `updateUrlForLaunchedFile`
- [ ] **Add alert integration**: Show warning alerts for missing files via `ALERT_SERVICE`

**Testing Subtask:**
- [ ] **Write Tests**: Verify service methods work correctly (see Testing section below)

**Key Implementation Notes:**

**PopState Handler Logic Flow**:
```
1. Parse URL query parameters (device, storage, path, file)
2. Validate all required parameters present
3. If missing required params → log info and return
4. Set isHandlingPopState flag = true
5. If file parameter present:
   - Load directory via StorageStore
   - Find file in directory.files array
   - Launch via launchFileWithContext() if file found
   - Show alert if file not found
6. If no file parameter:
   - Navigate to directory via StorageStore
7. Set isHandlingPopState flag = false
```

**URL Update Suppression**:
- Check `isHandlingPopState` flag before calling `location.go()`
- Prevents duplicate history entries during back/forward navigation
- Flag automatically resets after handler completes

**Error Handling**:
- Missing required parameters: Log warning, no alert (expected for non-deep-link URLs)
- File not found: Show warning alert via `ALERT_SERVICE`
- Directory load failure: Show warning alert via `ALERT_SERVICE`
- API errors: Handled by existing error handling in stores

**Listener Cleanup Pattern**:
```typescript
// Store bound function reference for removeEventListener
private popstateListener: ((event: PopStateEvent) => void) | null = null;

startListeningToPopState(): void {
  if (this.popstateListener) return; // Already listening
  this.popstateListener = (event) => this.handlePopState(event);
  window.addEventListener('popstate', this.popstateListener);
}

stopListeningToPopState(): void {
  if (this.popstateListener) {
    window.removeEventListener('popstate', this.popstateListener);
    this.popstateListener = null;
  }
}
```

**Testing Focus for Task 2:**

**Behaviors to Test:**
- [ ] **Service initialization**: Listener not registered by default
- [ ] **Start listener**: Listener registered successfully
- [ ] **Stop listener**: Listener removed successfully
- [ ] **Popstate with file**: File relaunches correctly
- [ ] **Popstate without file**: Directory navigates correctly
- [ ] **URL suppression**: No duplicate history entries during popstate
- [ ] **Alert integration**: Warning alerts shown for errors

**Testing Reference:**
- Unit test with mocked `StorageStore`, `ALERT_SERVICE`
- Spy on `window.addEventListener` and `window.removeEventListener`
- Simulate popstate events via `window.dispatchEvent(new PopStateEvent('popstate'))`

</details>

---

<details open>
<summary><h3>Task 3: Update IPlayerContext Interface</h3></summary>

**Purpose**: Add lifecycle methods to the `IPlayerContext` domain contract to expose popstate listener management.

**Related Documentation:**
- [player-context.interface.ts](../../../libs/application/src/lib/player/player-context.interface.ts) - Contract definition

**Implementation Subtasks:**
- [ ] **Add startListeningToPopState**: Add method signature to interface
- [ ] **Add stopListeningToPopState**: Add method signature to interface
- [ ] **Add JSDoc comments**: Document purpose and usage of new methods

**Testing Subtask:**
- [ ] **No tests needed**: Interfaces are not tested, used as contracts

**Key Implementation Notes:**

**Interface Contract**:
```typescript
export interface IPlayerContext {
  // ... existing methods ...
  
  /**
   * Start listening to browser popstate events for back/forward navigation.
   * Automatically relaunches files based on URL query parameters.
   * Should be called once during player initialization (e.g., from route resolver).
   */
  startListeningToPopState(): void;
  
  /**
   * Stop listening to browser popstate events.
   * Cleans up event listener to prevent memory leaks.
   * Optional - typically called when player is destroyed.
   */
  stopListeningToPopState(): void;
}
```

**Design Decision**: Methods are optional lifecycle hooks. Application continues to work without calling them, but browser navigation won't trigger file relaunches.

</details>

---

<details open>
<summary><h3>Task 4: Update Route Resolver to Start Listener</h3></summary>

**Purpose**: Call `startListeningToPopState()` from the route resolver to activate browser navigation support when player route is accessed.

**Related Documentation:**
- [player-route.resolver.ts](../../../libs/app/navigation/src/lib/player-route.resolver.ts) - Resolver implementation

**Implementation Subtasks:**
- [ ] **Inject PlayerContextService**: Add to `initPlayer()` function parameters
- [ ] **Call startListeningToPopState**: Add call after storage initialization completes
- [ ] **Add logging**: Log when listener starts for debugging visibility

**Testing Subtask:**
- [ ] **Write Tests**: Verify resolver calls listener method (see Testing section below)

**Key Implementation Notes:**

**Resolver Integration Point**:
- Call `playerContextService.startListeningToPopState()` in `initPlayer()` function
- Place after `initializeAllDeviceStorage()` completes (ensures stores are ready)
- Place before `initDeeplinking()` (deep linking may trigger file launches)

**Orchestration Order**:
```
1. waitForDeviceStoreInitialization()
2. initializeAllDeviceStorage()
3. playerContextService.startListeningToPopState()  ← Add here
4. initDeeplinking()
```

**Error Handling**:
- No try-catch needed - `startListeningToPopState()` is synchronous and won't throw
- Service handles multiple calls gracefully (guards against double-registration)

**Testing Focus for Task 4:**

**Behaviors to Test:**
- [ ] **Resolver calls listener**: `startListeningToPopState()` called during initialization
- [ ] **Call happens after storage init**: Listener starts after devices/storage ready
- [ ] **Call happens before deep linking**: Listener active before initial file launch

**Testing Reference:**
- Integration test with real resolver and mocked services
- Spy on `PlayerContextService.startListeningToPopState()`
- Verify call sequence matches orchestration order

</details>

---

<details open>
<summary><h3>Task 5: Add E2E Test for Browser Back/Forward Navigation</h3></summary>

**Purpose**: Add Cypress E2E test validating that browser back/forward buttons trigger file relaunches based on URL query parameters.

**Related Documentation:**
- [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - E2E testing guide
- [deep-linking.cy.ts](../../../apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts) - Existing deep linking tests

**Implementation Subtasks:**
- [ ] **Add test suite**: Create `Browser Back/Forward Navigation` describe block
- [ ] **Add test**: "relaunches file when browser back button clicked"
- [ ] **Add test**: "relaunches file when browser forward button clicked"
- [ ] **Use existing helpers**: Leverage `goBack()`, `goForward()`, `expectFileIsLaunched()`, etc.

**Testing Subtask:**
- [ ] **Run tests**: Verify tests pass with implementation (`pnpm nx e2e teensyrom-ui-e2e`)

**Key Implementation Notes:**

**Test Scenario Pattern**:
```gherkin
Given a user has launched File A
And navigates to File B (URL updated in history)
When the user clicks browser back button
Then File A should relaunch
And the URL should show File A parameters
And browser forward should still work to get back to File B
```

**Test Structure**:
```typescript
describe('Browser Back/Forward Navigation', () => {
  it('relaunches file when browser back button clicked', () => {
    // 1. Navigate to directory with file A
    navigateToPlayerWithParams({ device, storage, path, file: fileA });
    waitForFileToLoad();
    expectFileIsLaunched(fileA.title);
    
    // 2. Launch file B (via click or next button)
    clickNextButton();
    waitForFileInfoToAppear();
    expectFileIsLaunched(fileB.title);
    
    // 3. Click browser back
    goBack();
    
    // 4. Verify file A relaunched
    waitForFileInfoToAppear();
    expectFileIsLaunched(fileA.title);
    expectUrlContainsParams({ device, storage, path, file: fileA.fileName });
  });
  
  it('relaunches file when browser forward button clicked', () => {
    // Similar pattern but test forward after back
  });
});
```

**Interceptor Requirements**:
- Reuse `interceptLaunchFile({ filesystem })` for file launches
- Reuse `interceptGetDirectory({ filesystem })` for directory loads
- No new interceptors needed

**Testing Focus for Task 5:**

**Behaviors to Test:**
- [ ] **Browser back relaunches previous file**: File A → File B → Back → File A launches
- [ ] **Browser forward relaunches next file**: File A → File B → Back → Forward → File B launches
- [ ] **URL updates correctly**: URL reflects currently playing file after navigation
- [ ] **Multiple back/forward cycles work**: Can navigate back and forward multiple times

**Testing Reference:**
- See [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) for Cypress patterns
- Use existing helpers from `test-helpers.ts`
- Follow existing test structure in `deep-linking.cy.ts`

</details>

---

## 🗂️ Files Modified or Created

**Modified Files:**
- `libs/application/src/lib/player/player-context.service.ts` - Add popstate listener logic
- `libs/application/src/lib/player/player-context.interface.ts` - Add lifecycle methods to contract
- `libs/app/navigation/src/lib/player-route.resolver.ts` - Call listener on initialization
- `apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts` - Add browser navigation tests

**No New Files Created**

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
> - **Favor behavioral testing** - test what users/consumers observe, not how it's implemented
> - **Test as you go** - tests are integrated into each task's subtasks, not deferred to the end
> - **Test through public APIs** - services should be tested through their public interfaces
> - **Mock at boundaries** - mock infrastructure services, not internal logic

> **Reference Documentation:**
> - **All tasks**: [Testing Standards](../../TESTING_STANDARDS.md) - Core behavioral testing approach
> - **Service testing**: [Service Standards](../../SERVICE_STANDARDS.md) - Service layer testing patterns

### Where Tests Are Written

**Tests are embedded in each task above** with:
- **Testing Subtask**: Checkbox in the task's subtask list (e.g., "Write Tests: Test behaviors for this task")
- **Testing Focus**: "Behaviors to Test" section listing observable outcomes
- **Testing Reference**: Links to relevant testing documentation

**Complete each task's testing subtask before moving to the next task.**

### Test Coverage

**Unit Tests (PlayerContextService)**:
- PopState listener registration/cleanup
- URL parameter parsing
- File relaunch on valid parameters
- Directory navigation on missing file parameter
- Alert display on errors
- URL update suppression during popstate handling

**Integration Tests (Route Resolver)**:
- Resolver calls `startListeningToPopState()` during initialization
- Call sequencing relative to storage initialization

**E2E Tests (Cypress)**:
- Browser back button relaunches previous file
- Browser forward button relaunches next file
- URL updates correctly after browser navigation
- Multiple back/forward cycles work

### Test Execution Commands

**Running Unit Tests:**
```bash
# Run tests for player application layer
npx nx test application

# Run tests in watch mode
npx nx test application --watch
```

**Running E2E Tests:**
```bash
# Run all E2E tests
pnpm nx e2e teensyrom-ui-e2e

# Run specific test file
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/player/deep-linking.cy.ts"

# Open Cypress Test Runner
pnpm nx e2e teensyrom-ui-e2e:open-cypress
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] `startListeningToPopState()` method implemented in `PlayerContextService`
- [ ] `stopListeningToPopState()` method implemented in `PlayerContextService`
- [ ] `handlePopState()` private method parses URL and relaunches files
- [ ] Methods added to `IPlayerContext` interface
- [ ] Route resolver calls `startListeningToPopState()` during initialization
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)

**Testing Requirements:**
- [ ] All testing subtasks completed within each task
- [ ] All behavioral test checkboxes verified
- [ ] Unit tests for `PlayerContextService` popstate logic passing
- [ ] Integration tests for resolver initialization passing
- [ ] E2E test for browser back navigation passing
- [ ] E2E test for browser forward navigation passing
- [ ] All tests passing with no failures

**Quality Checks:**
- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors (`pnpm nx lint`)
- [ ] Code formatting is consistent
- [ ] No console errors in browser/terminal when running application
- [ ] No memory leaks from event listeners (cleanup verified)

**Documentation:**
- [ ] Inline code comments added for popstate handler logic
- [ ] JSDoc comments added to interface methods
- [ ] DEEP_LINKING_PLAN.md updated with completion status

**User Experience:**
- [ ] Browser back button relaunches previous file
- [ ] Browser forward button relaunches next file
- [ ] URL accurately reflects current file after browser navigation
- [ ] Warning alerts shown for missing files/directories
- [ ] No duplicate history entries created during navigation
- [ ] Multiple back/forward cycles work smoothly

**Ready for Production:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Manually tested with real devices (if available)
- [ ] Ready for deployment

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

**Decision 1: Service-Level Popstate Handling**
- **Rationale**: `PlayerContextService` already manages file launching and URL updates—it's the natural place for browser navigation handling. Keeps all navigation logic centralized.
- **Alternative Considered**: Component-level listener was considered but would couple navigation logic to UI layer and complicate lifecycle management.
- **Trade-off**: Service must have slightly more responsibility but maintains better separation of concerns.

**Decision 2: Listener Lifecycle via Route Resolver**
- **Rationale**: Resolver already orchestrates player initialization—adding listener startup here keeps initialization logic together and ensures listener is active when player route is accessed.
- **Alternative Considered**: APP_INITIALIZER or component ngOnInit were considered but resolver provides best integration point with existing architecture.
- **Trade-off**: Resolver has additional responsibility but avoids component coupling.

**Decision 3: URL Update Suppression Flag**
- **Rationale**: During popstate handling, we launch files which would normally update the URL again via `location.go()`. This would create duplicate history entries. A flag suppresses URL updates during popstate.
- **Alternative Considered**: Could refactor URL update logic to accept a flag parameter, but internal flag is simpler and less invasive.
- **Trade-off**: Slightly more state to manage but prevents duplicate history entries cleanly.

**Decision 4: Alert Service Integration**
- **Rationale**: Users should see warning alerts when browser navigation fails (file not found, directory load failed). Provides feedback that navigation was attempted but couldn't complete.
- **Alternative Considered**: Silent failures, but this provides poor UX—users wouldn't know why navigation didn't work.
- **Trade-off**: Additional dependency on `ALERT_SERVICE` but significantly improves user experience.

### Implementation Constraints

**Constraint 1: PopState Event Timing**
- PopState events fire **after** the URL has already changed
- Handler must parse the **current** URL (not event state) to get parameters
- Angular `ActivatedRoute` may not update synchronously—use `window.location.search` for parsing

**Constraint 2: Storage Initialization Requirement**
- Popstate handler requires `StorageStore` to be initialized to load directories
- Listener must start after storage initialization in resolver
- Early navigation before storage ready should be handled gracefully (no crashes)

**Constraint 3: Cross-Device Navigation**
- URL may reference a different device than currently selected
- Handler should validate device exists in `DeviceStore` before attempting navigation
- Graceful fallback for missing devices (show alert, don't crash)

### Future Enhancements

**Enhancement 1: Preserve Player State**
- Currently only relaunches files—playback position, timer state, etc. are reset
- Future: Could preserve and restore player state (play/pause status, timer position)
- Would require serializing state to URL or session storage

**Enhancement 2: Deep Link Validation**
- Currently assumes URL parameters are valid if present
- Future: Could add validation service to check device connectivity, storage availability before attempting navigation
- Would provide better error messages for invalid deep links

**Enhancement 3: Loading States**
- Currently no visual indication when browser navigation triggers file launch
- Future: Could show loading spinner or toast during navigation
- Would improve perceived performance for slower operations

### Potential Edge Cases

**Edge Case 1: Rapid Back/Forward Clicks**
- User rapidly clicking back/forward could trigger multiple overlapping file launches
- **Mitigation**: Debounce handler or check if launch is already in progress
- **Priority**: Low - unlikely to occur in normal usage

**Edge Case 2: Navigation During Active Playback**
- Browser back during music playback could interrupt timer
- **Mitigation**: Current timer cleanup logic should handle this correctly
- **Priority**: Low - existing code handles state transitions

**Edge Case 3: Device Disconnect During Navigation**
- User navigates back to file on device that is now disconnected
- **Mitigation**: Existing error handling in stores should show appropriate error
- **Priority**: Medium - should show meaningful error to user

### External References

- [MDN: PopState Event](https://developer.mozilla.org/en-US/docs/Web/API/Window/popstate_event) - Browser API reference
- [MDN: Location API](https://developer.mozilla.org/en-US/docs/Web/API/Location) - URL manipulation
- [Angular Location Service](https://angular.io/api/common/Location) - Angular URL abstraction
- [Deep Linking Planning Doc](./DEEP_LINKING_PLAN.md) - Overall feature architecture

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery**: (To be filled in during implementation)
- **Discovery**: (To be filled in during implementation)

</details>

---

## 💡 Implementation Guide

### Quick Start Checklist

Before starting implementation:

1. ✅ Read [DEEP_LINKING_PLAN.md](./DEEP_LINKING_PLAN.md) - Understand Phase 1-3 context
2. ✅ Review existing popstate event examples in codebase (if any)
3. ✅ Understand current URL update flow in `PlayerContextService`
4. ✅ Review alert service integration pattern
5. ✅ Familiarize with E2E test helpers in `test-helpers.ts`

### Implementation Order

**Recommended sequence** (complete each before moving to next):

1. **Task 3 first** - Update interface (establishes contract)
2. **Task 1 & 2** - Implement service logic (fulfills contract)
3. **Task 4** - Update resolver (activates feature)
4. **Task 5** - Add E2E tests (validates end-to-end behavior)

### Testing as You Go

- After Task 1: Write unit tests for listener registration/cleanup
- After Task 2: Write unit tests for popstate handler logic
- After Task 4: Write integration tests for resolver
- After Task 5: Run E2E tests and verify browser navigation works

### Debugging Tips

**If browser back/forward doesn't work:**
1. Check browser console for errors
2. Verify listener was registered (add log statement)
3. Verify popstate event fires (add log in handler)
4. Check URL parameters are present after navigation
5. Verify storage and device stores are initialized

**If file doesn't relaunch:**
1. Verify URL parameters are parsed correctly
2. Check if file exists in directory (log directory.files)
3. Verify `launchFileWithContext()` is called
4. Check for errors in player store

**If URL updates during popstate:**
1. Verify `isHandlingPopState` flag is set before launch
2. Check flag is checked in `updateUrlForLaunchedFile()`
3. Verify flag resets after handler completes

### Common Pitfalls to Avoid

❌ **Forgetting to cleanup listener** - Memory leak
✅ Store listener reference and call `removeEventListener` in stop method

❌ **Updating URL during popstate handling** - Duplicate history entries
✅ Use flag to suppress URL updates during popstate

❌ **Not handling missing parameters** - Crashes on non-deep-link URLs
✅ Validate required parameters before attempting navigation

❌ **Parsing ActivatedRoute in popstate** - May not be synchronous
✅ Use `window.location.search` and `URLSearchParams` for parsing

❌ **Not showing alerts on errors** - Poor user experience
✅ Integrate `ALERT_SERVICE` for file-not-found and directory-load-failed cases

---

## 🎓 Key Concepts

### PopState Event vs Angular Router

**PopState Event** (Browser Native):
- Fires when browser back/forward buttons clicked
- Fires when `history.back()` / `history.forward()` called
- Does NOT fire when `location.go()` called
- Does NOT fire when `router.navigate()` called

**Angular Router**:
- Fires NavigationEnd event on route changes
- Does NOT fire on query parameter changes when using `location.go()`
- Route resolvers only run on initial navigation to route

**Why We Need PopState**:
- We use `location.go()` to avoid re-running resolver on every file change
- This means Angular Router doesn't detect URL changes
- PopState listener is the only way to detect browser back/forward clicks

### URL Update Suppression Pattern

**Problem**: During popstate handling, we call `launchFileWithContext()` which normally updates the URL via `location.go()`. This would add a duplicate entry to browser history.

**Solution**: Set flag before launching, check flag before updating URL.

```typescript
// In handlePopState
this.isHandlingPopState = true;
await this.launchFileWithContext(...); // This calls updateUrlForLaunchedFile
this.isHandlingPopState = false;

// In updateUrlForLaunchedFile
if (this.isHandlingPopState) {
  return; // Skip URL update - we're replaying history
}
this.location.go(`/player?${queryString}`);
```

### Service Lifecycle Pattern

**Start Listener**: Called once during player initialization (from resolver)
**Stop Listener**: Optional cleanup (typically on app shutdown)

```typescript
// Store bound function for cleanup
private popstateListener: ((event: PopStateEvent) => void) | null = null;

startListeningToPopState(): void {
  if (this.popstateListener) return; // Prevent double-registration
  this.popstateListener = () => this.handlePopState();
  window.addEventListener('popstate', this.popstateListener);
}

stopListeningToPopState(): void {
  if (this.popstateListener) {
    window.removeEventListener('popstate', this.popstateListener);
    this.popstateListener = null;
  }
}
```

---

**Ready to implement? Start with Task 3 (interface), then Task 1 & 2 (service logic), then Task 4 (resolver), then Task 5 (E2E tests). Good luck! 🚀**
