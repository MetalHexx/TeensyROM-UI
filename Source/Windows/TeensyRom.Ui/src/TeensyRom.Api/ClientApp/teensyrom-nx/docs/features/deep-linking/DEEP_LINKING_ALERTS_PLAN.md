# Phase: Deep Linking Error Alerts

## 🎯 Objective

Enhance deep linking user experience by displaying user-friendly warning alerts when files or directories fail to load during route resolution, replacing silent failures with visible error feedback and adding comprehensive E2E test coverage.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [Player Route Resolver](../../../libs/app/navigation/src/lib/player-route.resolver.ts) - Current implementation with silent failures
- [ ] [Deep Linking E2E Tests](../../../apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts) - Existing deep linking test suite
- [ ] [Alert Testing Plan](../e2e-testing/ALERT_TESTING_PLAN.md) - **PREREQUISITE**: Alert helpers must be implemented first

**Domain Contracts:**
- [ ] [Alert Contract](../../../libs/domain/src/lib/contracts/alert.contract.ts) - IAlertService interface and injection token
- [ ] [Alert Severity](../../../libs/domain/src/lib/models/alert-severity.enum.ts) - AlertSeverity enum values
- [ ] [Alert Position](../../../libs/domain/src/lib/models/alert-position.enum.ts) - AlertPosition enum values

**Standards & Guidelines:**
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns
- [ ] [Logging Standards](../../LOGGING_STANDARDS.md) - Logging patterns (keep logs alongside alerts)
- [ ] [E2E Tests Overview](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - E2E testing architecture
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches

---

## 📂 File Structure Overview

```
libs/app/navigation/src/lib/
└── player-route.resolver.ts                  📝 Modified - Inject ALERT_SERVICE, show warnings

apps/teensyrom-ui-e2e/src/
├── support/
│   └── helpers/
│       └── alert.helpers.ts                  📝 Referenced - Use for alert assertions
├── e2e/
│   └── player/
│       ├── deep-linking.cy.ts                📝 Modified - Add error scenario tests
│       └── test-helpers.ts                   📝 Modified - Import alert helpers

docs/features/deep-linking/
└── DEEP_LINKING_ALERTS_PLAN.md               📝 This document
```

---

<details open>
<summary><h3>Task 1: Add Alert Service to Player Route Resolver</h3></summary>

**Purpose**: Inject `ALERT_SERVICE` into the player route resolver and display user-friendly warning alerts when deep linking fails due to missing files or directories, replacing silent `logWarn()` calls with visible error feedback.

**Related Documentation:**
- [player-route.resolver.ts](../../../libs/app/navigation/src/lib/player-route.resolver.ts) - Current implementation
- [Alert Contract](../../../libs/domain/src/lib/contracts/alert.contract.ts) - Service interface
- [Logging Standards](../../LOGGING_STANDARDS.md) - Keep logs alongside alerts

**Implementation Subtasks:**
- [ ] **Import alert types**: Add imports for `ALERT_SERVICE`, `AlertPosition` from `@teensyrom-nx/domain`
- [ ] **Inject ALERT_SERVICE**: Use `inject(ALERT_SERVICE)` in `initDeeplinking()` function (pass as parameter from `initPlayer()`)
- [ ] **Add file not found alert**: In `initDeeplinking()`, locate the `if (targetFile) { } else { }` block and add `alertService.warning()` call with message `File "${file}" not found in directory "${path}"`
- [ ] **Add directory not loaded alert**: In `initDeeplinking()`, locate the `if (directoryState?.directory?.files) { } else { }` block and add `alertService.warning()` call with message `Directory "${path}" could not be loaded or has no files`
- [ ] **Set alert position**: Use `AlertPosition.TopCenter` for both alerts
- [ ] **Set auto-dismiss timeout**: Use 5000ms (5 seconds) for both alerts
- [ ] **Keep existing logs**: Preserve `logWarn()` calls alongside new alerts (alerts supplement logs, don't replace them)

**Testing Subtask:**
- [ ] **Manual testing**: Test deep linking with invalid file/directory parameters and verify alerts appear (see Testing Focus below)

**Key Implementation Notes:**
- Alerts should be **warnings** (not errors) - user can recover by fixing URL or navigating manually
- Position alerts at `TopCenter` for visibility during page load (no content visible yet)
- Keep existing `logWarn()` calls for developer debugging in browser console
- Auto-dismiss after 5 seconds - long enough to read but not annoying
- Message text should be user-friendly and explain what went wrong

**Alert Call Pattern**:
```typescript
alertService.warning(
  `File "${file}" not found in directory "${path}"`,
  AlertPosition.TopCenter,
  5000 // auto-dismiss after 5 seconds
);
```

**Testing Focus for Task 1:**

> Manual testing validates alert behavior before E2E tests are written

**Manual Test Scenarios:**
- [ ] **Invalid file parameter**: Navigate to `http://localhost:4200/player?device=<valid-id>&storage=SD&path=/games&file=NonExistent.crt` - verify warning alert appears with "File 'NonExistent.crt' not found" message
- [ ] **Invalid directory path**: Navigate to `http://localhost:4200/player?device=<valid-id>&storage=SD&path=/invalid&file=test.crt` - verify warning alert appears with "Directory '/invalid' could not be loaded" message
- [ ] **Alert displays at top-center**: Verify alert appears at top of screen, centered horizontally
- [ ] **Alert has warning icon**: Verify Material 'warning' icon displays in alert
- [ ] **Alert auto-dismisses**: Wait 5 seconds and verify alert disappears automatically
- [ ] **Alert is dismissible**: Click dismiss button and verify alert closes immediately
- [ ] **Console logs preserved**: Verify `logWarn()` messages still appear in browser console

**Testing Reference:**
- See [Logging Standards](../../LOGGING_STANDARDS.md) for logging patterns

</details>

---

<details open>
<summary><h3>Task 2: Add E2E Tests for Deep Linking Error Scenarios</h3></summary>

**Purpose**: Add comprehensive E2E test coverage for deep linking error scenarios using reusable alert helpers to verify that alerts display correctly when files or directories fail to load.

**Related Documentation:**
- [deep-linking.cy.ts](../../../apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts) - Existing tests
- [Alert Helpers](../../../apps/teensyrom-ui-e2e/src/support/helpers/alert.helpers.ts) - **PREREQUISITE**: Must be implemented first
- [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Testing patterns

**Implementation Subtasks:**
- [ ] **Import alert helpers**: Add imports for `verifyAlertVisible`, `verifyAlertMessage`, `verifyAlertIcon`, `dismissAlert`, `waitForAlertAutoDismiss` from `../../support/helpers/alert.helpers`
- [ ] **Add error test describe block**: Create new `describe('Deep Linking Error Handling', () => { ... })` block
- [ ] **Add file not found test**: Create test "displays warning alert when file parameter does not match any file in directory"
- [ ] **Add directory not loaded test**: Create test "displays warning alert when directory path is invalid"
- [ ] **Add alert dismissal test**: Create test "allows dismissing deep linking error alerts"
- [ ] **Add auto-dismiss test**: Create test "auto-dismisses deep linking error alerts after timeout"
- [ ] **Use existing fixtures**: Use `singleDevice` fixture and mock filesystem from generators
- [ ] **Use existing interceptors**: Use `interceptFindDevices`, `interceptGetDirectory` interceptors

**Testing Subtask:**
- [ ] **Run new tests**: Execute `pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/player/deep-linking.cy.ts" --grep="Error Handling"` to verify all 4 tests pass

**Key Implementation Notes:**
- Use existing test infrastructure (fixtures, interceptors, helpers) - no new setup needed
- Follow existing test patterns in `deep-linking.cy.ts` for consistency
- Use `TIMEOUTS.DEFAULT` from test constants for wait operations
- Set up valid device/storage/path in `beforeEach`, test invalid file/path in individual tests
- Verify both alert appearance AND no file launch (use `expectNoFileLaunched()`)

**Test Implementation Pattern**:
```typescript
describe('Deep Linking Error Handling', () => {
  beforeEach(() => {
    // Setup: Valid device/storage/path
    interceptFindDevices({ fixture: singleDevice });
    interceptGetDirectory({ filesystem });
  });

  it('displays warning alert when file parameter does not match any file', () => {
    // Navigate with invalid file
    navigateToPlayerWithParams({
      device: testDeviceId,
      storage: TeensyStorageType.Sd,
      path: TEST_PATHS.GAMES,
      file: 'NonExistent.crt'
    });

    // Wait for directory load
    waitForDirectoryLoad();

    // Verify alert
    verifyAlertVisible();
    verifyAlertMessage('not found');
    verifyAlertIcon('warning');

    // Verify no file launched
    expectNoFileLaunched();
  });
});
```

**Testing Focus for Task 2:**

> E2E tests validate alert behavior programmatically using reusable helpers

**Behaviors to Test:**
- [ ] **File not found alert displays**: Warning alert appears when file parameter doesn't match any file in loaded directory
- [ ] **File not found message correct**: Alert message contains file name and "not found" text
- [ ] **Directory not loaded alert displays**: Warning alert appears when directory path is invalid or fails to load
- [ ] **Directory not loaded message correct**: Alert message contains directory path and loading failure text
- [ ] **Alert has warning icon**: Alert displays Material 'warning' icon in both scenarios
- [ ] **Alert is dismissible**: User can manually dismiss alert using dismiss button
- [ ] **Alert auto-dismisses**: Alert automatically disappears after 5-second timeout
- [ ] **No file launches on error**: `expectNoFileLaunched()` confirms file didn't load despite error

**Testing Reference:**
- See [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) for testing patterns
- See [Alert Testing Plan](../e2e-testing/ALERT_TESTING_PLAN.md) for alert helper usage

**Test Execution Commands:**
```bash
# Run only deep linking error tests
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/player/deep-linking.cy.ts" --grep="Error Handling"

# Run all deep linking tests
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/player/deep-linking.cy.ts"

# Run in headed mode to watch tests
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/player/deep-linking.cy.ts" --headed --grep="Error Handling"
```

</details>

---

<details open>
<summary><h3>Task 3: Update Player Test Helpers</h3></summary>

**Purpose**: Import and optionally re-export alert helpers in player test helpers for convenient access in player feature tests.

**Related Documentation:**
- [player/test-helpers.ts](../../../apps/teensyrom-ui-e2e/src/e2e/player/test-helpers.ts) - Player-specific helpers
- [Alert Helpers](../../../apps/teensyrom-ui-e2e/src/support/helpers/alert.helpers.ts) - Alert assertions

**Implementation Subtasks:**
- [ ] **Add alert helper imports**: Import commonly-used alert helpers from `../../support/helpers/alert.helpers`
- [ ] **Re-export for convenience** (optional): Re-export alert helpers so player tests can import from `./test-helpers` instead of reaching to support folder
- [ ] **Update JSDoc comments**: Document that alert helpers are available from this file

**Testing Subtask:**
- [ ] **Verify imports work**: Ensure deep linking tests can import alert helpers from player test-helpers file (if re-exported)

**Key Implementation Notes:**
- This is an **optional convenience** - tests can always import directly from `support/helpers/`
- Re-exporting reduces import path complexity for player feature tests
- Only re-export commonly-used alert helpers (visibility, message, icon, dismiss)
- Keep alert helpers as primary source of truth (don't duplicate implementation)

**Import/Re-export Pattern**:
```typescript
// Option 1: Import only (tests import from support/helpers)
import { verifyAlertMessage } from '../../support/helpers/alert.helpers';

// Option 2: Re-export (tests import from test-helpers)
export {
  verifyAlertVisible,
  verifyAlertMessage,
  verifyAlertIcon,
  dismissAlert,
  waitForAlertAutoDismiss
} from '../../support/helpers/alert.helpers';
```

**Testing Focus for Task 3:**

> Verify imports work correctly in deep linking tests

**Verification Steps:**
- [ ] **Imports resolve**: TypeScript compiler accepts imports (no type errors)
- [ ] **Tests run**: Deep linking tests execute without import errors
- [ ] **Helpers work**: Alert verification functions execute correctly in tests

</details>

---

## 🗂️ Files Modified or Created

> Complete list of files affected by this phase

**Modified Files:**
- `libs/app/navigation/src/lib/player-route.resolver.ts` - Add ALERT_SERVICE injection and warning calls
- `apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts` - Add 4 error scenario tests
- `apps/teensyrom-ui-e2e/src/e2e/player/test-helpers.ts` - Import/re-export alert helpers

**Referenced Files (no changes):**
- `apps/teensyrom-ui-e2e/src/support/helpers/alert.helpers.ts` - Reusable alert assertions (from prerequisite phase)

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary.

### Testing Approach

**Two-Phase Testing**:
1. **Task 1 - Manual Testing**: Validate alert behavior manually before writing E2E tests
2. **Task 2 - E2E Testing**: Automate alert verification with reusable helpers

**Why Manual First?**:
- Faster feedback during development (no test setup needed)
- Visual confirmation of alert appearance, position, and timing
- Validates resolver logic works before investing in E2E test infrastructure

### Test Coverage

**Manual Testing (Task 1):**
- Invalid file parameter → file not found alert
- Invalid directory path → directory not loaded alert
- Alert positioning (top-center)
- Alert severity (warning icon)
- Alert auto-dismiss (5 seconds)
- Manual dismiss functionality
- Console logs preserved

**E2E Testing (Task 2):**
- Programmatic validation of all manual test scenarios
- Alert message content verification
- Alert icon/severity verification
- Dismissal mechanisms (manual + auto)
- No file launch on error

### Test Execution

**Manual Testing:**
```bash
# Start dev server
pnpm start

# Navigate in browser to:
# http://localhost:4200/player?device=<valid-id>&storage=SD&path=/games&file=NonExistent.crt
# http://localhost:4200/player?device=<valid-id>&storage=SD&path=/invalid&file=test.crt
```

**E2E Testing:**
```bash
# Run new error tests only
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/player/deep-linking.cy.ts" --grep="Error Handling"

# Run full deep linking suite
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/player/deep-linking.cy.ts"
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] `ALERT_SERVICE` injected into player route resolver
- [ ] File not found warning alert implemented
- [ ] Directory not loaded warning alert implemented
- [ ] Alerts positioned at `TopCenter`
- [ ] Alerts auto-dismiss after 5 seconds
- [ ] Existing `logWarn()` calls preserved
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)

**Testing Requirements:**
- [ ] All manual test scenarios pass
- [ ] 4 new E2E tests added to `deep-linking.cy.ts`
- [ ] All new E2E tests pass consistently
- [ ] Existing deep linking tests still pass (no regressions)
- [ ] Alert helpers used correctly in tests
- [ ] Tests follow patterns from [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)

**Quality Checks:**
- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors (`pnpm nx lint`)
- [ ] No console errors when running application
- [ ] Alerts display correctly in browser
- [ ] Alert positioning and timing correct

**Documentation:**
- [ ] Inline code comments added for alert calls
- [ ] Test comments explain error scenarios clearly
- [ ] No broken test imports or references

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] Manual and E2E tests passing
- [ ] No known bugs or UX issues
- [ ] Deep linking error handling complete

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

**Why Warning Severity?**
- File/directory not found during deep linking is a **user error**, not an application error
- Users can recover by fixing the URL or navigating manually
- Warnings are dismissible and auto-dismiss (non-blocking UX)
- Matches severity patterns for recoverable errors across the application

**Why TopCenter Position?**
- Deep linking happens on page load (no content visible yet)
- Top-center position is most visible without blocking content
- Consistent with bootstrap error handling patterns
- Alert appears above player content that may load later

**Why 5-Second Auto-Dismiss?**
- Long enough for users to read message (average reading speed)
- Short enough not to be annoying for users who navigate away
- Consistent with other alert timeouts in the application
- Users can still manually dismiss if needed

**Alert Message Wording**
- **Technical but clear**: `File "${file}" not found in directory "${path}"`
- **Provides context**: Includes both file name and directory path
- **Actionable**: Users understand what went wrong
- **Not overly casual**: Maintains professional tone appropriate for development tool

### Implementation Constraints

**Prerequisite Dependency**:
- **CRITICAL**: Alert helpers must be implemented first (see [Alert Testing Plan](../e2e-testing/ALERT_TESTING_PLAN.md))
- E2E tests depend on `verifyAlertVisible()`, `verifyAlertMessage()`, etc.
- Cannot complete Task 2 until alert helpers exist

**Route Resolver Execution Context**:
- Resolver runs during navigation (before component renders)
- Alerts must not block navigation (warnings, not errors)
- Keep resolver logic non-blocking (fire-and-forget alert calls)

**Testing Infrastructure**:
- Must use existing fixtures (`singleDevice`, mock filesystem)
- Must use existing interceptors (`interceptFindDevices`, `interceptGetDirectory`)
- Follow existing test patterns in `deep-linking.cy.ts`

### Future Enhancements

**Additional Error Scenarios** (out of scope):
- **Device not found**: Alert when device ID parameter doesn't match any connected device
- **Storage unavailable**: Alert when storage type parameter references unavailable storage
- **Permission errors**: Alert when directory requires permissions not granted
- **Network errors**: Alert when API calls fail during deep linking resolution

**Enhanced Error Messages**:
- Provide suggestions for fixing errors (e.g., "Check the URL and try again")
- Link to documentation or help resources
- Show list of available files if directory loads but file doesn't match

**User Preferences**:
- Allow users to disable deep linking error alerts
- Persist "don't show again" preference for specific error types
- Customize alert auto-dismiss timeout

### External References

- [Angular Router Resolver Docs](https://angular.dev/guide/routing/common-router-tasks#resolve) - Resolver patterns
- [Material Icons - Warning](https://fonts.google.com/icons?icon.query=warning) - Warning icon reference

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery 1**: [Something learned during implementation that affects approach]
- **Discovery 2**: [Unexpected complexity or simplification found]

</details>

---

## 💡 Questions for Clarification

> These questions should be answered before starting implementation

### Question 1: Alert Message Wording

What message format should resolver alerts use for user-facing error text?

**Option A - Technical but clear (recommended)**
```typescript
`File "${file}" not found in directory "${path}"`
`Directory "${path}" could not be loaded or has no files`
```

**Option B - User-friendly conversational**
```typescript
`We couldn't find "${file}" in ${path}. Please check the URL and try again.`
`Oops! The directory ${path} isn't available right now.`
```

**Option C - Minimal technical**
```typescript
`File not found: ${file}`
`Directory unavailable: ${path}`
```

**📌 Recommendation: Option A**
*Because: Strikes a balance between technical precision (developers may share URLs) and user-friendliness. Provides enough context to understand what went wrong without being overly casual or terse.*

---

### Question 2: Preserve Logs or Replace with Alerts?

Should existing `logWarn()` calls be preserved alongside alerts or replaced entirely?

**Option A - Preserve logs (recommended)**
```typescript
logWarn(`File not found in directory: ${file}`);
alertService.warning(`File "${file}" not found in directory "${path}"`, ...);
```

**Option B - Replace logs with alerts**
```typescript
// Remove logWarn, only use alertService
alertService.warning(`File "${file}" not found in directory "${path}"`, ...);
```

**Option C - Conditional logging**
```typescript
// Only log if alerts disabled
if (!alertsEnabled) {
  logWarn(`File not found: ${file}`);
}
alertService.warning(...);
```

**📌 Recommendation: Option A**
*Because: Logs serve different audience (developers debugging) than alerts (end users). Browser console logs are valuable for troubleshooting even when alerts visible. No reason to remove existing debugging capability.*

---

### Question 3: E2E Test Organization

Should error scenario tests be in a separate describe block or integrated with existing tests?

**Option A - Separate describe block (recommended)**
```typescript
describe('Deep Linking Error Handling', () => {
  it('file not found', ...);
  it('directory not loaded', ...);
  it('alert dismissal', ...);
  it('alert auto-dismiss', ...);
});
```

**Option B - Integrated with existing blocks**
```typescript
describe('Route Resolution & Navigation', () => {
  it('navigates to directory', ...);
  it('displays error when file not found', ...); // NEW
  it('auto-launches file', ...);
});
```

**Option C - New file for error tests**
```typescript
// deep-linking-errors.cy.ts
describe('Deep Linking Errors', () => { ... });
```

**📌 Recommendation: Option A**
*Because: Groups related error tests together, makes test suite organization clearer, easier to run error tests independently with `--grep="Error Handling"`. Keeps error scenarios separate from happy path tests.*

</details>
