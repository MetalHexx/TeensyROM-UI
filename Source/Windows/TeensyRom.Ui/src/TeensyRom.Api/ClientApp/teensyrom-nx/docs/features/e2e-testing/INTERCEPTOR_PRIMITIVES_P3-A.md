# Interceptor Primitives Phase 3-A: Additional Cleanup Tasks

## Overview

Following the successful completion of Phase 3 (Interceptor Primitives P3), this document outlines additional cleanup tasks to improve code quality, consistency, and maintainability across the E2E test interceptor suite.

## Objectives

1. Replace direct `cy.wait()` calls with semantic wait functions
2. Refactor hardcoded timing waits into named helper functions
3. Standardize naming conventions across interceptor functions
4. Add documentation explaining design decisions for validation/counting functions
5. Perform comprehensive comment cleanup across all modified files

## Task 1: Replace Direct cy.wait() in device-connection.cy.ts

### Problem

The test file uses direct Cypress wait calls instead of interceptor wait functions, reducing consistency and making intent less clear.

### Solution

Replace direct `cy.wait()` calls with proper wait function imports that already exist in the interceptor files.

### Files Modified

- `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts`

### Changes

| Line | Current Code                   | Replacement                 | Reason                               |
| ---- | ------------------------------ | --------------------------- | ------------------------------------ |
| 90   | `cy.wait('@connectDevice')`    | `waitForConnectDevice()`    | Use semantic helper from interceptor |
| 131  | `cy.wait('@disconnectDevice')` | `waitForDisconnectDevice()` | Use semantic helper from interceptor |

### Implementation Notes

- Import statements already include these functions
- No new code needed, just replacements
- Improves readability and consistency

---

## Task 2: Replace Hardcoded Timing Waits in device-refresh-connection.cy.ts

### Problem

The test file contains hardcoded `cy.wait(200)` calls for timing windows during race condition tests. The intent is unclear without explanation.

### Solution

Investigate whether existing wait functions can be used, or create a semantic helper function if needed.

### Files Modified

- `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-connection.cy.ts` (primary)
- `apps/teensyrom-ui-e2e/src/support/interceptors/connectDevice.interceptors.ts` OR `disconnectDevice.interceptors.ts` (if new helper needed)

### Affected Lines

| Line | Context                                                                            | Purpose                                                     |
| ---- | ---------------------------------------------------------------------------------- | ----------------------------------------------------------- |
| 407  | After `clickPowerButton(0)` in "refresh during connection" test                    | Create timing window for concurrent connection operation    |
| 426  | After `clickPowerButton(0)` in "refresh during disconnection" test                 | Create timing window for concurrent disconnection operation |
| 443  | After `clickRefreshDevices()` in "allow connection after interrupted refresh" test | Create timing window for delayed discovery operation        |

### Implementation Options

1. **Check existing wait functions** in test-helpers.ts to see if any can handle timing windows
2. **Create new helper** if needed (e.g., `waitForOperationToStart()`, `ensureOperationInProgress()`, or `waitMs()`)
3. **Add inline comments** explaining the timing window purpose if keeping hardcoded waits

### Design Decision Pending

Await implementation guidance on which approach to use.

---

## Task 3: Rename setupFindDevicesWithProblemDetails → interceptFindDevicesWithError

### Problem

Naming inconsistency: uses "setup" prefix instead of "intercept" prefix like other error/special setup functions.

### Solution

Rename function to `interceptFindDevicesWithError()` to align with naming conventions and make the function's purpose clearer.

### Files Modified

1. **Interceptor file**: `apps/teensyrom-ui-e2e/src/support/interceptors/findDevices.interceptors.ts`

   - Current function name: `setupFindDevicesWithProblemDetails()`
   - New function name: `interceptFindDevicesWithError()`
   - Also rename: `setupFindDevicesWithNetworkErrorForce()` → `interceptFindDevicesWithNetworkError()` (for consistency)

2. **Test file**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-error.cy.ts`
   - Update all function calls and import statement

### Call Sites to Update (device-refresh-error.cy.ts)

| Line    | Current Call                                                 | New Call                                                | Notes         |
| ------- | ------------------------------------------------------------ | ------------------------------------------------------- | ------------- |
| 49      | `setupFindDevicesWithNetworkErrorForce,`                     | `interceptFindDevicesWithNetworkError,`                 | Import rename |
| 121-125 | `setupFindDevicesWithProblemDetails(404, errorMessage, ...)` | `interceptFindDevicesWithError(404, errorMessage, ...)` | Test call     |
| 137     | `setupFindDevicesWithProblemDetails(404, detailMessage)`     | `interceptFindDevicesWithError(404, detailMessage)`     | Test call     |
| 168     | `setupFindDevicesWithProblemDetails(404, errorMessage)`      | `interceptFindDevicesWithError(404, errorMessage)`      | Test call     |
| 249     | `setupFindDevicesWithNetworkErrorForce()`                    | `interceptFindDevicesWithNetworkError()`                | Test call     |
| 259     | `setupFindDevicesWithProblemDetails(404, errorMessage)`      | `interceptFindDevicesWithError(404, errorMessage)`      | Test call     |
| 276     | `setupFindDevicesWithProblemDetails(404, errorMessage)`      | `interceptFindDevicesWithError(404, errorMessage)`      | Test call     |

### Note on Status Code Parameter

The explicit `404` status code is semantically important and should remain. It clearly indicates the type of error being tested and affects the response format. No simplification is possible without losing semantic clarity.

---

## Task 4: Add JSDoc to Validation/Counting Functions

### Problem

Functions like `setupConnectDeviceWithValidation()` and `setupConnectDeviceWithCounting()` use direct `cy.intercept()` instead of interceptor primitives, but the reason isn't documented.

### Solution

Add brief JSDoc notes explaining why these functions cannot use the primitive-based architecture.

### Files Modified

1. `apps/teensyrom-ui-e2e/src/support/interceptors/connectDevice.interceptors.ts`
2. `apps/teensyrom-ui-e2e/src/support/interceptors/disconnectDevice.interceptors.ts`

### Functions Requiring Documentation

**connectDevice.interceptors.ts:**

- `setupConnectDeviceWithValidation()` (line 178)
- `setupConnectDeviceWithValidationAndError()` (line 240)
- `setupConnectDeviceWithCounting()` (line 317)

**disconnectDevice.interceptors.ts:**

- `setupDisconnectDeviceWithValidation()` (line ~185)
- `setupDisconnectDeviceWithValidationAndError()` (line ~218)
- `setupDisconnectDeviceWithCounting()` (line ~259)

### Documentation Template

```typescript
/**
 * [Existing JSDoc...]
 *
 * **Implementation Note**: Uses direct cy.intercept() for custom request inspection logic
 * rather than primitives, as it requires URL validation before generating responses.
 */
export function setupConnectDeviceWithValidation(...) { }
```

### Brevity Guidelines

- Keep notes to 1-2 sentences
- Focus on "why not primitives" rather than explaining the implementation
- Avoid redundancy with existing JSDoc

---

## Task 5: Comment Cleanup on All Modified Files

### Problem

Files modified during Phase 3 and this phase may contain redundant comments that add noise without value.

### Solution

Run comment cleanup agent on all modified TypeScript files in git diff to remove unnecessary comments while preserving valuable documentation.

### Files to Clean (Explicit List)

All files are in the path: `apps/teensyrom-ui-e2e/src/`

#### Test Files (3)

1. `e2e/devices/device-connection.cy.ts`
2. `e2e/devices/device-refresh-connection.cy.ts`
3. `e2e/devices/device-refresh-error.cy.ts`

#### Interceptor Files (4)

4. `support/interceptors/connectDevice.interceptors.ts`
5. `support/interceptors/disconnectDevice.interceptors.ts`
6. `support/interceptors/findDevices.interceptors.ts`
7. `support/interceptors/primitives/interceptor-primitives.ts`

### Cleanup Guidelines

- **Preserve**: All JSDoc comments (`/** ... */`)
- **Preserve**: Organizational section headers with visual separators (`// ====...`)
- **Preserve**: TODO, FIXME, NOTE markers
- **Remove**: Inline comments that restate obvious code
- **Remove**: Comments explaining simple assignments or operations
- **Remove**: Redundant comments after functions
- **Improve**: JSDoc comments that are incomplete or inaccurate

### Execution

Run comment-cleaner agent in **parallel** on all 7 files to maximize efficiency.

---

## Implementation Sequence

### Phase 1: Code Modifications (Sequential)

1. **Task 1** - Replace direct cy.wait() calls in device-connection.cy.ts
2. **Task 2** - Investigate and handle hardcoded waits in device-refresh-connection.cy.ts
3. **Task 3** - Rename functions in findDevices.interceptors.ts and device-refresh-error.cy.ts
4. **Task 4** - Add brief JSDoc notes to connectDevice and disconnectDevice interceptors

**Agent to use**: `@agent-clean-coder` for all code modifications

### Phase 2: Testing

- Run `@agent-e2e-runner` on affected test files:
  - `device-connection.cy.ts`
  - `device-refresh-error.cy.ts`
  - `device-refresh-connection.cy.ts` (if modified)

**Agent to use**: `@agent-e2e-runner`

### Phase 3: Code Quality

- Run `@agent-code-cleaner` on all modified interceptor files

**Agent to use**: `@agent-code-cleaner`

### Phase 4: Comment Cleanup (Parallel)

- Run `@agent-comment-cleaner` on all 7 files simultaneously

**Agent to use**: `@agent-comment-cleaner` (7 instances in parallel)

---

## Expected Outcomes

### Code Quality Improvements

- ✅ All direct Cypress calls replaced with semantic helpers
- ✅ Naming conventions consistent across interceptor suite
- ✅ Design decisions documented in code
- ✅ Redundant comments removed
- ✅ All tests passing

### Files Changed

- **Test files**: 3 modified (device-connection.cy.ts, device-refresh-error.cy.ts, possibly device-refresh-connection.cy.ts)
- **Interceptor files**: 2-3 modified (connectDevice.interceptors.ts, disconnectDevice.interceptors.ts, findDevices.interceptors.ts)
- **Documentation file**: This file (new)

### Estimated Changes

- ~15-25 lines of code changes
- ~5 function renames
- ~6 brief JSDoc additions
- Comprehensive comment cleanup across 7 files

---

## Success Criteria

- [ ] All direct `cy.wait()` calls replaced with semantic functions
- [ ] Hardcoded timing waits refactored or documented
- [ ] Function naming consistent (intercept\* prefix)
- [ ] Validation/counting functions documented
- [ ] All tests passing (21 + 16 + 27+ tests)
- [ ] TypeScript compilation clean
- [ ] ESLint passing
- [ ] Comments cleaned on all 7 files

---

## Notes

- This phase builds on the successful Phase 3 completion (12+ direct intercepts migrated)
- All changes are incremental improvements to code quality and consistency
- No breaking changes; all functionality remains identical
- Tests provide safety net for all refactoring

---

**Phase 3-A Status**: Planned (awaiting execution approval)
**Last Updated**: 2025-11-03
