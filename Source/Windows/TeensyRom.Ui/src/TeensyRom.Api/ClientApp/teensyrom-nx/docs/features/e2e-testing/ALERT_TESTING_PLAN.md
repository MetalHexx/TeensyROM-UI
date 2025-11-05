# Phase: E2E Alert Testing Infrastructure

## 🎯 Objective

Establish a centralized, reusable E2E alert assertion system that standardizes alert verification across all Cypress test suites, eliminating duplicated assertion code and improving test readability and maintainability.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**

- [ ] [E2E Tests Overview](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - E2E testing architecture and three-layer pattern
- [ ] [Alert Contract](../../../libs/domain/src/lib/contracts/alert.contract.ts) - Alert service interface (for understanding alert behavior)

**Standards & Guidelines:**

- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions

**Reference Tests:**

- [ ] [device-refresh-error.cy.ts](../../../apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-error.cy.ts) - Contains 15+ alert assertions to refactor

---

## 📂 File Structure Overview

> Alert helpers introduce a fourth layer to the E2E architecture: **Assertion Helpers**

```
apps/teensyrom-ui-e2e/src/
├── support/
│   ├── helpers/
│   │   ✨ alert.helpers.ts                   # New - Reusable alert assertion functions
│   │   └── indexing.helpers.ts               # Existing helper example
│   └── constants/
│       ├── alert.constants.ts                ✨ New - Alert severity/icon constants
│       ├── selector.constants.ts             📝 Referenced - Contains ALERT_SELECTORS
│       └── index.ts                          📝 Modified - Export alert constants
├── e2e/
│   ├── devices/
│   │   ├── device-refresh-error.cy.ts        📝 Modified - Replace inline assertions
│   │   └── test-helpers.ts                   📝 Modified - Remove ALERT_SELECTORS import
│   └── player/
│       └── test-helpers.ts                   📝 Modified - Import/re-export alert helpers

docs/
└── features/
    └── e2e-testing/
        └── ALERT_TESTING_PLAN.md             📝 This document
```

---

<details open>
<summary><h3>Task 1: Create Reusable Alert Assertion Helpers</h3></summary>

**Purpose**: Build a centralized library of reusable alert verification functions that abstract away direct selector usage and provide a clean, declarative API for testing alert behavior across all E2E specs.

**Related Documentation:**

- [E2E_TESTS.md - Three-Layer Architecture](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md#architecture) - This adds a fourth "helpers" layer
- [selector.constants.ts](../../../apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts) - Source of `ALERT_SELECTORS`

**Implementation Subtasks:**

- [ ] **Create constants file**: Create `apps/teensyrom-ui-e2e/src/support/constants/alert.constants.ts` with `ALERT_SEVERITY` and `ALERT_ICON` constants
- [ ] **Export constants**: Add `export * from './alert.constants'` to `constants/index.ts`
- [ ] **Create helpers file**: Create `apps/teensyrom-ui-e2e/src/support/helpers/alert.helpers.ts`
- [ ] **Import selectors and constants**: Import `ALERT_SELECTORS` from `../constants/selector.constants` and `ALERT_SEVERITY`, `ALERT_ICON` from `../constants/alert.constants`
- [ ] **Implement `verifyAlertVisible()`**: Assert alert container is visible in DOM
- [ ] **Implement `verifyAlertNotVisible()`**: Assert alert container does not exist
- [ ] **Implement `verifyAlertMessage(message: string)`**: Assert alert message contains expected text (partial match)
- [ ] **Implement `verifyAlertMessageExact(message: string)`**: Assert alert message matches exactly after trimming whitespace
- [ ] **Implement `verifyAlertIcon(iconText)`**: Assert alert icon displays expected Material icon text using `ALERT_ICON` constant
- [ ] **Implement `verifyAlertSeverity(severity)`**: Convenience function combining icon verification with `ALERT_SEVERITY` constant parameter
- [ ] **Implement `verifyAlertMessageDoesNotContain(text)`**: Assert alert message does NOT contain specific text (for ProblemDetails testing)
- [ ] **Implement `dismissAlert()`**: Click alert dismiss button
- [ ] **Implement `waitForAlertAutoDismiss(timeoutMs?)`**: Wait for alert to auto-dismiss (default 3500ms to cover 3000ms timeout)
- [ ] **Implement `verifyAlertDismissed()`**: Assert alert container no longer exists
- [ ] **Add JSDoc comments**: Document each function with parameter descriptions and usage examples
- [ ] **Export all functions**: Ensure all helpers are exported for use in test specs

**Testing Subtask:**

- [ ] **Validate helpers**: Use helpers in `device-refresh-error.cy.ts` refactoring (Task 2) to verify functionality

**Key Implementation Notes:**

- Follow Cypress chainable patterns (`cy.get(...).should(...)`) for consistency
- Use `ALERT_SELECTORS` constants exclusively (never hardcode selectors)
- Use `ALERT_SEVERITY` and `ALERT_ICON` constants for type-safe severity/icon parameters
- Support optional parameters for flexibility (e.g., custom timeouts)
- Keep helpers focused on single verification concern (don't combine unrelated checks)
- Match patterns from existing `test-helpers.ts` in device/player feature folders

**Critical Constants Structure** (`alert.constants.ts`):

```typescript
export const ALERT_SEVERITY = {
  SUCCESS: 'success',
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info',
} as const;

export const ALERT_ICON = {
  SUCCESS: 'check_circle',
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info',
} as const;

export type AlertSeverityType = (typeof ALERT_SEVERITY)[keyof typeof ALERT_SEVERITY];
export type AlertIconType = (typeof ALERT_ICON)[keyof typeof ALERT_ICON];
```

**Critical Function Signatures**:

```typescript
import { AlertSeverityType, AlertIconType } from '../constants/alert.constants';

// Visibility checks
export function verifyAlertVisible(): void;
export function verifyAlertNotVisible(): void;
export function verifyAlertDismissed(): void;

// Content verification
export function verifyAlertMessage(message: string): void;
export function verifyAlertMessageExact(message: string): void;
export function verifyAlertMessageDoesNotContain(text: string): void;

// Severity/icon checks
export function verifyAlertIcon(iconText: AlertIconType): void;
export function verifyAlertSeverity(severity: AlertSeverityType): void;

// Interactions
export function dismissAlert(): void;
export function waitForAlertAutoDismiss(timeoutMs?: number): void;
```

**Testing Focus for Task 1:**

> Validation happens through Task 2 refactoring - if all `device-refresh-error.cy.ts` tests pass, helpers work correctly

**Behaviors to Validate:**

- [ ] **Alert visibility helpers work**: Tests using `verifyAlertVisible()` pass
- [ ] **Message verification works**: Tests using `verifyAlertMessage()` correctly match text
- [ ] **Icon verification works**: Tests using `verifyAlertIcon()` correctly match severity icons
- [ ] **Dismissal helpers work**: Tests using `dismissAlert()` and `waitForAlertAutoDismiss()` pass
- [ ] **Negative assertions work**: `verifyAlertMessageDoesNotContain()` correctly fails when text is found

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for E2E testing approach
- Validation is implicit through Task 2 - all 15+ tests must pass

</details>

---

<details open>
<summary><h3>Task 2: Refactor device-refresh-error.cy.ts to Use Alert Helpers</h3></summary>

**Purpose**: Replace all inline alert verification code in `device-refresh-error.cy.ts` with centralized alert helpers, validating the helper API and establishing the pattern for future tests.

**Related Documentation:**

- [device-refresh-error.cy.ts](../../../apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-error.cy.ts) - Test file with 15+ alert assertions
- Alert helpers from Task 1

**Implementation Subtasks:**

- [ ] **Add helper imports**: Import all alert helper functions from `../../support/helpers/alert.helpers` in `device-refresh-error.cy.ts`
- [ ] **Add constant imports**: Import `ALERT_SEVERITY`, `ALERT_ICON` from `../../support/constants` for use in helper function calls
- [ ] **Refactor test 1** ("should display ProblemDetails error message"): Replace inline `cy.get(ALERT_SELECTORS.container).should('be.visible').within(() => {...})` with `verifyAlertVisible()`, `verifyAlertMessage()`, and `verifyAlertIcon()`
- [ ] **Refactor test 2** ("should extract error message from ProblemDetails.title"): Replace `cy.get(ALERT_SELECTORS.messageInContainer).should('contain.text', errorMessage).and('not.contain.text', 'Technical details')` with `verifyAlertMessage(errorMessage)` and `verifyAlertMessageDoesNotContain('Technical details')`
- [ ] **Refactor test 3** ("should handle 404 with missing ProblemDetails.title"): Replace inline message check with `verifyAlertMessage(detailMessage)`
- [ ] **Refactor test 4** ("should allow dismissing the error alert"): Replace `cy.get(ALERT_SELECTORS.container).should('be.visible')` with `verifyAlertVisible()` and `cy.get(ALERT_SELECTORS.dismissButton).click()` with `dismissAlert()`
- [ ] **Refactor test 5** ("should auto-dismiss error alert after timeout"): Replace `cy.wait(3500); cy.get(ALERT_SELECTORS.container).should('not.exist')` with `waitForAlertAutoDismiss()`
- [ ] **Refactor test 6** ("should handle 500 Internal Server Error"): Replace inline alert checks with helpers
- [ ] **Refactor test 7** ("should handle network errors gracefully"): Replace `cy.get(ALERT_SELECTORS.container).should('be.visible')` with `verifyAlertVisible()`
- [ ] **Refactor bootstrap tests** (3 tests): Replace all inline alert assertions with helpers
- [ ] **Remove ALERT_SELECTORS import**: Remove import of `ALERT_SELECTORS` from test-helpers (no longer needed)
- [ ] **Remove unused selectors**: Clean up any now-unused selector imports

**Testing Subtask:**

- [ ] **Run full test suite**: Execute `pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/device-refresh-error.cy.ts"` to verify all 15+ tests pass

**Key Implementation Notes:**

- This is a **pure refactoring** - no behavior changes, only improved readability
- Some tests may combine multiple helpers (e.g., `verifyAlertMessage()` + `verifyAlertMessageDoesNotContain()`)
- Test execution time should remain the same (no performance impact)
- More descriptive helper names improve test readability significantly

**Example Refactoring Pattern**:

```typescript
// Before (inline assertions with magic strings)
cy.get(ALERT_SELECTORS.container)
  .should('be.visible')
  .within(() => {
    cy.get(ALERT_SELECTORS.message).should('contain.text', errorMessage);
    cy.get(ALERT_SELECTORS.icon).should('contain.text', 'error');
  });

// After (using helpers with constants)
import { ALERT_ICON } from '../../support/constants';

verifyAlertVisible();
verifyAlertMessage(errorMessage);
verifyAlertIcon(ALERT_ICON.ERROR);
```

**Testing Focus for Task 2:**

> All existing tests must continue to pass with identical behavior

**Behaviors to Verify:**

- [ ] **All 15+ tests pass**: Full test suite passes without failures
- [ ] **Test output is clearer**: Helper function names make test intent more obvious
- [ ] **No flakiness introduced**: Tests remain deterministic and stable
- [ ] **Execution time unchanged**: Refactoring doesn't slow down test execution

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for E2E testing patterns

**Test Execution Commands:**

```bash
# Run device-refresh-error tests specifically
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/device-refresh-error.cy.ts"

# Run all device tests to ensure no regressions
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/**/*.cy.ts"
```

</details>

---

<details open>
<summary><h3>Task 3: Update E2E Documentation</h3></summary>

**Purpose**: Document the new alert helpers layer in the E2E testing architecture guide so developers know about and can use these reusable assertions.

**Related Documentation:**

- [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Main E2E documentation to update

**Implementation Subtasks:**

- [ ] **Update architecture section**: Modify "Three-Layer Testing Pattern" to "Four-Layer Testing Pattern" and add "Assertion Helpers Layer"
- [ ] **Add helpers layer description**: Document purpose and scope of helpers layer (alert assertions, future: busy dialog, forms, etc.)
- [ ] **Add alert helpers section**: Create new section documenting available alert helper functions with constant usage examples
- [ ] **Add alert constants section**: Document `ALERT_SEVERITY` and `ALERT_ICON` constants for test consumption
- [ ] **Update best practices**: Add guidance on when to use helpers vs inline assertions, and always use constants instead of magic strings
- [ ] **Add code examples**: Include before/after examples showing helper usage
- [ ] **Update file structure diagram**: Add `support/helpers/` folder to architecture diagram

**Testing Subtask:**

- [ ] **Review documentation**: Verify docs are clear and complete with no broken links

**Key Implementation Notes:**

- Keep documentation concise and focused on practical usage
- Include JSDoc-style function signatures for quick reference
- Link to actual helper file for full implementation details
- Emphasize that helpers are preferred over inline selector usage

**Documentation Outline**:

```markdown
## Four-Layer Testing Pattern (updated from three-layer)

### 1. Test Data Layer (Fixtures & Generators)

### 2. API Mocking Layer (Interceptors)

### 3. Assertion Helpers Layer (NEW)

- Alert assertion helpers
- Future: Busy dialog, form validation, navigation helpers

### 4. E2E Tests (Cypress Specs)

## Alert Constants

Available in `support/constants/alert.constants.ts`:

- ALERT_SEVERITY: { SUCCESS, ERROR, WARNING, INFO }
- ALERT_ICON: { SUCCESS, ERROR, WARNING, INFO }
- AlertSeverityType and AlertIconType types

## Alert Assertion Helpers

Available functions:

- verifyAlertVisible()
- verifyAlertMessage(message)
- verifyAlertIcon(ALERT_ICON.ERROR)
- verifyAlertSeverity(ALERT_SEVERITY.WARNING)
- dismissAlert()
- waitForAlertAutoDismiss()
  [...and others]

Example usage:
import { ALERT_ICON } from '../../support/constants';
verifyAlertIcon(ALERT_ICON.ERROR); // ✅ Use constants
verifyAlertIcon('error'); // ❌ Avoid magic strings
```

**Testing Focus for Task 3:**

> Documentation review and completeness check

**Review Checklist:**

- [ ] **Architecture diagram updated**: Shows four layers instead of three
- [ ] **Helper functions documented**: All alert helpers listed with descriptions
- [ ] **Constants documented**: ALERT_SEVERITY and ALERT_ICON constants explained with examples
- [ ] **Examples are clear**: Code examples demonstrate helper usage with constants (no magic strings)
- [ ] **Links work**: All internal documentation links are valid
- [ ] **Terminology consistent**: "Assertion Helpers Layer" used consistently

</details>

---

## 🗂️ Files Modified or Created

> Complete list of files affected by this phase

**New Files:**

- `apps/teensyrom-ui-e2e/src/support/constants/alert.constants.ts` - Alert severity and icon constants
- `apps/teensyrom-ui-e2e/src/support/helpers/alert.helpers.ts` - Reusable alert assertion functions

**Modified Files:**

- `apps/teensyrom-ui-e2e/src/support/constants/index.ts` - Export alert constants
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-error.cy.ts` - Refactored to use alert helpers with constants
- `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts` - Remove ALERT_SELECTORS import
- `apps/teensyrom-ui-e2e/E2E_TESTS.md` - Document new helpers layer

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary.

### Testing Philosophy

**Validation Through Refactoring**:

- Alert helpers are validated by refactoring existing tests to use them
- If all 15+ tests in `device-refresh-error.cy.ts` pass after refactoring, helpers work correctly
- No new test files needed - existing tests serve as validation

**Behavioral Focus**:

- Tests verify observable UI behavior (alert appears, shows correct message, dismisses)
- Helpers abstract implementation details (selectors) from tests
- Test intent becomes more declarative and readable

### Test Execution

**During Development:**

```bash
# Run device-refresh-error tests after each refactoring step
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/device-refresh-error.cy.ts"

# Run in headed mode to see tests execute
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/device-refresh-error.cy.ts" --headed
```

**Final Validation:**

```bash
# Run full E2E suite to ensure no regressions
pnpm nx e2e teensyrom-ui-e2e

# Run all device tests
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/**/*.cy.ts"
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**

- [ ] Alert constants file created with `ALERT_SEVERITY` and `ALERT_ICON` exported
- [ ] Constants exported from `constants/index.ts` for easy import
- [ ] Alert helpers file created with 12+ exported functions
- [ ] All helpers properly documented with JSDoc comments
- [ ] All helper functions use constant types (no magic strings)
- [ ] `device-refresh-error.cy.ts` fully refactored to use helpers
- [ ] No inline `ALERT_SELECTORS` usage remains in refactored tests
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)

**Testing Requirements:**

- [ ] All 15+ tests in `device-refresh-error.cy.ts` pass
- [ ] No test flakiness or timing issues introduced
- [ ] Test execution time unchanged or improved
- [ ] Tests are more readable with helper function names

**Quality Checks:**

- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors (`pnpm nx lint teensyrom-ui-e2e`)
- [ ] No console errors when running tests
- [ ] Helper functions follow consistent naming patterns

**Documentation:**

- [ ] E2E_TESTS.md updated to document helpers layer
- [ ] Alert constants documented with usage examples
- [ ] Helper functions documented with constant usage patterns
- [ ] Architecture diagram updated to show four layers
- [ ] No broken documentation links

**Ready for Next Phase:**

- [ ] All success criteria met
- [ ] Helpers validated through refactored tests
- [ ] Documentation complete and accurate
- [ ] Ready to use helpers in other test suites (e.g., deep linking tests)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

**Why Add a Helpers Layer?**

- **DRY Principle**: Eliminates duplicated alert assertion code across 10+ test files
- **Maintainability**: Alert DOM structure changes only require updating helpers, not all tests
- **Readability**: `verifyAlertMessage('error')` is clearer than 4 lines of Cypress commands
- **Discoverability**: Developers know where to find assertion utilities
- **Consistency**: Standardizes how alerts are verified across all specs

**Why Use Constants Instead of Magic Strings?**

- **Type Safety**: TypeScript enforces valid constant values at compile time
- **Autocomplete**: IDE provides intellisense for available severity/icon values
- **Maintainability**: Change icon name once in constants, affects all tests
- **Consistency**: All tests use same values, no typos or variations
- **Discoverability**: Developers know where to find valid values
- **Follows E2E Patterns**: Matches existing constant usage for selectors, routes, etc.

**Single File vs Split Files**:

- All alert helpers in one file (`alert.helpers.ts`)
- Reason: Cohesive functionality (all verify alerts), small enough (~150 lines), easier to import
- Split files would be premature optimization given current scope

**Constants in Separate File**:

- Alert constants in dedicated `alert.constants.ts` file
- Reason: Shared by helpers AND test consumers, cleaner imports, matches existing E2E constant pattern
- Exported from `constants/index.ts` for centralized access

### Implementation Constraints

**Existing Test Behavior**:

- Refactoring must preserve exact test behavior (no new assertions or removals)
- Test execution time should remain the same (no performance impact)
- All 15+ existing tests must continue to pass

**Cypress Patterns**:

- Helpers must use standard Cypress chainable commands
- Must work with Cypress retry-ability mechanism
- Should support Cypress timeouts and assertion options

### Future Enhancements

**Additional Helper Categories** (out of scope for this phase):

- **Busy Dialog Helpers**: `verifyBusyDialogVisible()`, `verifyBusyDialogMessage()`, `waitForBusyDialogDismissed()`
- **Form Validation Helpers**: `verifyFieldError()`, `verifyFormInvalid()`, `verifyValidationMessage()`
- **Navigation Helpers**: `verifyUrlContains()`, `verifyRouteParams()`, `verifyQueryParams()`
- **Accessibility Helpers**: `verifyAriaLabel()`, `verifyRole()`, `verifyKeyboardNavigable()`

These would follow the same centralized helper pattern established in this phase.

**Helper Composition**:

- Future consideration: Allow composing multiple helpers into higher-level workflows
- Example: `verifyErrorStateWithAlert(message)` could combine multiple lower-level helpers
- Keep initial implementation simple; add composition only if patterns emerge

### External References

- [Cypress Best Practices](https://docs.cypress.io/guides/references/best-practices) - Cypress testing patterns
- [Material Icons Reference](https://fonts.google.com/icons?icon.set=Material+Icons) - Icon names used in severity checks

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery 1**: [Something learned during implementation that affects approach]
- **Discovery 2**: [Unexpected complexity or simplification found]

</details>

---

## 💡 Questions for Clarification

> These questions should be answered before starting implementation

### Question 1: Helper Import Strategy

Should feature-specific test helper files (e.g., `player/test-helpers.ts`) re-export alert helpers for convenience?

**Option A - Direct imports only (recommended)**

```typescript
// In test spec
import { verifyAlertMessage } from '../../support/helpers/alert.helpers';
```

**Option B - Re-export from feature helpers**

```typescript
// In player/test-helpers.ts
export { verifyAlertMessage, verifyAlertIcon } from '../../support/helpers/alert.helpers';

// In test spec
import { verifyAlertMessage } from './test-helpers';
```

**Option C - Both patterns supported**

**📌 Recommendation: Option A**
_Because: Clearer import paths, easier to trace helper origin, no duplication of exports. Feature helpers should focus on feature-specific logic._

---

### Question 2: Alert Icon Values

Should `ALERT_ICON` constants use Material icon names or match alert severity names?

**Option A - Material icon names (recommended)**

```typescript
export const ALERT_ICON = {
  SUCCESS: 'check_circle', // Material icon name
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info',
} as const;
```

**Option B - Match severity names**

```typescript
export const ALERT_ICON = {
  SUCCESS: 'success', // Same as severity
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info',
} as const;
```

**Option C - Both with aliases**

```typescript
export const ALERT_ICON = {
  SUCCESS: 'check_circle',
  SUCCESS_ALT: 'success',
  // ...
} as const;
```

**📌 Recommendation: Option A**
_Because: Matches actual Material icon text content in DOM, aligns with how Cypress verifies icon presence, explicit about what's being tested. Investigate actual rendered icon text first._

---

### Question 3: Helper Timeout Defaults

Should helpers use Cypress default timeouts or custom defaults?

**Option A - Cypress defaults (recommended)**

```typescript
// Uses Cypress defaultCommandTimeout (4000ms)
verifyAlertVisible();
```

**Option B - Custom defaults**

```typescript
// Uses custom timeout (e.g., 2000ms for alerts)
verifyAlertVisible();
```

**Option C - Always explicit timeouts**

```typescript
// Always require timeout parameter
verifyAlertVisible(2000);
```

**📌 Recommendation: Option A**
_Because: Consistent with Cypress ecosystem, adjustable via Cypress config, no magic numbers in helper code. Custom timeouts can be passed when needed._

</details>
