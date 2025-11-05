# Phase 3C: Migrate Indexing Helpers to Interceptors

## 🎯 Objective

Migrate API-related helper functions from `indexing.helpers.ts` to their appropriate interceptor files, ensuring consistent patterns across the E2E testing infrastructure. This phase completes the migration of orchestration and wait functions to interceptors while maintaining clear separation between API mocking and DOM/UI helpers. By moving `setupIndexingScenario` (renamed to `setupIndexingInterceptors`) and `waitForIndexingComplete` to interceptor files, we establish a unified pattern where all API-related test utilities live alongside their corresponding interceptor definitions.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [Interceptor Primitives Plan](./INTERCEPTOR_PRIMITIVES_PLAN.md) - Overall architecture plan
- [ ] [Phase 3B Plan](./INTERCEPTOR_PRIMITIVES_P3-B.md) - Similar migration pattern reference
- [ ] [E2E Interceptor Documentation](../../../apps/teensyrom-ui-e2e/src/support/interceptors/E2E_INTERCEPTORS.md) - Interceptor patterns

**Standards & Guidelines:**
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices
- [ ] [E2E Testing Guide](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - E2E testing overview

---

## 📂 File Structure Overview

```
apps/teensyrom-ui-e2e/src/
├── support/
│   ├── interceptors/
│   │   ├── indexStorage.interceptors.ts         📝 Modified - Add setupIndexingInterceptors & waitForIndexingComplete
│   │   └── E2E_INTERCEPTORS.md                   📝 Modified - Document new functions
│   └── helpers/
│       └── indexing.helpers.ts                   📝 Modified - Remove migrated and unused functions
└── e2e/devices/
    └── device-indexing.cy.ts                     📝 Modified - Update imports and function calls (38 updates)
```

---

## 📋 Implementation Tasks

<details open>
<summary><h3>Task 1: Add setupIndexingInterceptors to indexStorage.interceptors.ts</h3></summary>

**Purpose**: Migrate the `setupIndexingScenario` function from helpers to interceptors with improved naming (`setupIndexingInterceptors`) to better reflect its purpose of setting up API mocking rather than complete scenario orchestration.

**Related Documentation:**
- [indexing.helpers.ts:40-102](../../../apps/teensyrom-ui-e2e/src/support/helpers/indexing.helpers.ts#L40-L102) - Current implementation
- [indexStorage.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/indexStorage.interceptors.ts) - Target file

**Implementation Subtasks:**
- [ ] **Add `setupIndexingInterceptors` function** to `indexStorage.interceptors.ts` after existing setup functions
- [ ] **Preserve function signature** from original: `(fixture: { devices: readonly any[] }, interceptorOptions?: { delay?, statusCode?, errorMode?, errorMessage? })`
- [ ] **Copy implementation logic** that iterates devices and calls `interceptIndexStorage` for each available storage
- [ ] **Ensure interceptIndexAllStorage call** is included as in original implementation
- [ ] **Add JSDoc comments** following interceptor file conventions (describe purpose, parameters, examples)
- [ ] **Export the function** from the interceptor file

**Key Implementation Notes:**
- Function orchestrates interceptor setup by analyzing fixture device data
- Collects device IDs and storage types (USB/SD) from fixture
- Creates custom aliases using pattern `indexStorage_${deviceId}_${storageType}`
- Must maintain backward compatibility with existing interceptor option parameters
- No DOM/UI interactions - purely API mocking setup

**Example Function Signature:**
```typescript
/**
 * Setup indexing interceptors for multiple devices with storage
 * @param fixture Device fixture data with shape { devices: [...] }
 * @param interceptorOptions Optional configuration for all interceptors
 */
export function setupIndexingInterceptors(
  fixture: { devices: readonly any[] },
  interceptorOptions?: { delay?: number; statusCode?: number; errorMode?: boolean; errorMessage?: string }
): void
```

**Testing Subtask:**
- [ ] **Write Tests**: Verify function sets up interceptors correctly (see Testing section below)

**Testing Focus for Task 1:**

> Focus on **behavioral testing** - does the function correctly set up interceptors?

**Behaviors to Test:**
- [ ] **Interceptor setup**: Function calls `interceptIndexStorage` for each available storage type
- [ ] **Alias generation**: Custom aliases follow `indexStorage_${deviceId}_${storageType}` pattern
- [ ] **Index All setup**: Function calls `interceptIndexAllStorage` with correct options
- [ ] **Option forwarding**: Interceptor options are correctly passed to underlying functions

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns

</details>

---

<details open>
<summary><h3>Task 2: Add waitForIndexingComplete to indexStorage.interceptors.ts</h3></summary>

**Purpose**: Migrate the convenience wrapper `waitForIndexingComplete` from helpers to interceptors, providing intelligent routing between single-device and Index All wait functions based on parameters.

**Related Documentation:**
- [indexing.helpers.ts:225-242](../../../apps/teensyrom-ui-e2e/src/support/helpers/indexing.helpers.ts#L225-L242) - Current implementation
- [indexStorage.interceptors.ts:141-144](../../../apps/teensyrom-ui-e2e/src/support/interceptors/indexStorage.interceptors.ts#L141-L144) - Existing waitForIndexStorage

**Implementation Subtasks:**
- [ ] **Add `waitForIndexingComplete` function** to `indexStorage.interceptors.ts` near other wait functions
- [ ] **Implement alias resolution logic**: Use `storageType` param to determine if single-device or Index All
- [ ] **Single-device path**: When `storageType` provided, use `INDEXING_INTERCEPT_ALIASES.byDeviceAndType(deviceIdOrAlias, storageType)`
- [ ] **Index All path**: When no `storageType`, treat `deviceIdOrAlias` as alias directly
- [ ] **Call cy.wait()** with resolved alias and optional timeout (default 10000ms)
- [ ] **Add JSDoc comments** with parameter descriptions and usage examples
- [ ] **Export the function** from the interceptor file

**Key Implementation Notes:**
- Provides unified interface for waiting on either single-device or batch operations
- Timeout parameter allows customization for slow operations
- Return type should be `Cypress.Chainable<any>` for proper chaining
- Must work with both custom aliases and standard INDEXING_INTERCEPT_ALIASES constants

**Function Signature:**
```typescript
/**
 * Wait for indexing API call to complete
 * @param deviceIdOrAlias Device ID for single-device indexing, or alias constant for Index All
 * @param storageType Optional storage type for single-device operations ('USB' or 'SD')
 * @param timeout Optional timeout for API response (default: 10000ms)
 * @returns Cypress chainable with intercept response
 */
export function waitForIndexingComplete(
  deviceIdOrAlias: string,
  storageType?: 'USB' | 'SD',
  timeout = 10000
): Cypress.Chainable<any>
```

**Testing Subtask:**
- [ ] **Write Tests**: Verify function waits correctly for different scenarios (see Testing section below)

**Testing Focus for Task 2:**

**Behaviors to Test:**
- [ ] **Single-device wait**: Correctly waits when deviceId and storageType provided
- [ ] **Index All wait**: Correctly waits when only alias provided (no storageType)
- [ ] **Timeout handling**: Custom timeout parameter is respected
- [ ] **Alias resolution**: Proper alias generated via `byDeviceAndType` for single-device operations

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns

</details>

---

<details open>
<summary><h3>Task 3: Establish E2E Test Baseline</h3></summary>

**Purpose**: Run the existing `device-indexing.cy.ts` test suite before making any changes to establish a baseline for comparison and ensure current tests are passing.

**Related Documentation:**
- [device-indexing.cy.ts](../../../apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts) - Test file to baseline

**Implementation Subtasks:**
- [ ] **Run @e2e-runner agent** on `device-indexing.cy.ts` test file
- [ ] **Capture baseline metrics**: Test count (20 tests), pass/fail status, execution time
- [ ] **Document current state**: Note any existing failures or warnings
- [ ] **Review agent report**: Verify all tests are passing before proceeding

**Agent Usage for Task 3:**
- Use **@e2e-runner** agent to establish baseline
  - Test file: `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts`
  - Agent must report: Total test count, pass/fail status for each test, execution time, any failures with details
- Review agent report and address any pre-existing issues before proceeding

**Key Implementation Notes:**
- This baseline allows us to compare before/after migration
- Any failures at this stage are pre-existing and should be noted separately
- Baseline execution time helps identify performance regressions later

**Testing Subtask:**
- [ ] **Establish Baseline**: Run E2E tests and document results

**Testing Focus for Task 3:**

**Behaviors to Verify:**
- [ ] **All tests passing**: Current test suite should have 0 failures
- [ ] **Test count**: Should be 20 tests across 5 describe blocks
- [ ] **Execution time**: Document baseline execution time for comparison

**Agent Report Requirements:**
- **@e2e-runner**: Must report test count, pass/fail status, execution time, detailed failure information if any

</details>

---

<details open>
<summary><h3>Task 4: Update device-indexing.cy.ts Test File</h3></summary>

**Purpose**: Update the single test file that uses the migrated functions, changing imports and function calls to use the new interceptor functions with updated names.

**Related Documentation:**
- [device-indexing.cy.ts:26-33](../../../apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts#L26-L33) - Current import statement
- Phase 3B Task examples - Similar migration pattern

**Implementation Subtasks:**
- [ ] **Update import statement** (lines 26-33): Remove `setupIndexingScenario` and `waitForIndexingComplete`, add import from `indexStorage.interceptors`
- [ ] **Add interceptor import**: `import { setupIndexingInterceptors, waitForIndexingComplete } from '../../support/interceptors/indexStorage.interceptors';`
- [ ] **Replace function calls**: Change all 22 occurrences of `setupIndexingScenario` to `setupIndexingInterceptors`
- [ ] **Verify waitForIndexingComplete calls**: Ensure all 16 calls remain compatible with new implementation
- [ ] **Preserve helper imports**: Keep imports for DOM/UI helpers (`verifyBusyDialogDisplayed`, `verifyBusyDialogHidden`, `clickStorageIndexButton`, `clickIndexAllButton`)

**Agent Usage for Task 4:**
- Use **@clean-coder** agent to perform the migration
  - File path: `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts`
  - Changes: Update import statement, rename 22 function calls, verify 16 wait function calls
  - Agent must report: Specific import changes, number of function call replacements, any issues encountered

**Key Implementation Notes:**
- Import statement will split between interceptors and helpers
- Function call replacements are straightforward renames (no parameter changes)
- `waitForIndexingComplete` has identical signature, so calls don't need modification
- Total of 38 function call updates (22 setup + 16 wait, though wait calls don't change)

**Before (lines 26-33):**
```typescript
import {
  setupIndexingScenario,      // → Move to interceptor import
  verifyBusyDialogDisplayed,  // ✓ Keep in helper import
  verifyBusyDialogHidden,     // ✓ Keep in helper import
  clickStorageIndexButton,    // ✓ Keep in helper import
  clickIndexAllButton,        // ✓ Keep in helper import
  waitForIndexingComplete,    // → Move to interceptor import
} from '../../support/helpers/indexing.helpers';
```

**After:**
```typescript
import {
  setupIndexingInterceptors,
  waitForIndexingComplete,
} from '../../support/interceptors/indexStorage.interceptors';
import {
  verifyBusyDialogDisplayed,
  verifyBusyDialogHidden,
  clickStorageIndexButton,
  clickIndexAllButton,
} from '../../support/helpers/indexing.helpers';
```

**Testing Subtask:**
- [ ] **Write Tests**: Verify updated test file works correctly (see Testing section below)

**Testing Focus for Task 4:**

**Behaviors to Test:**
- [ ] **Import resolution**: New imports resolve correctly to interceptor functions
- [ ] **Function calls work**: All 22 renamed calls execute successfully
- [ ] **Wait functions work**: All 16 wait function calls complete successfully
- [ ] **Test suite passes**: All 20 tests pass with new function names

**Agent Report Requirements:**
- **@clean-coder**: Must report import changes made, number of function call replacements (22 for setup, 16 for wait), any issues encountered

</details>

---

<details open>
<summary><h3>Task 5: Validate Test File Changes with E2E Runner</h3></summary>

**Purpose**: Run the updated test file to verify that all changes work correctly and no regressions were introduced during the migration.

**Related Documentation:**
- [device-indexing.cy.ts](../../../apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts) - Updated test file

**Implementation Subtasks:**
- [ ] **Run @e2e-runner agent** on updated `device-indexing.cy.ts`
- [ ] **Compare with baseline**: Verify test count (20), pass/fail status, execution time
- [ ] **Verify zero failures**: All tests must pass
- [ ] **Check for performance regression**: Execution time should be similar to baseline
- [ ] **Review agent report**: Check for any warnings or issues

**Agent Usage for Task 5:**
- Use **@e2e-runner** agent to validate changes
  - Test file: `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts`
  - Agent must report: Test pass/fail status, number of tests run (expect 20), any failures with details, execution time comparison with baseline
- If failures occur, use **@clean-coder** to fix issues before proceeding

**Key Implementation Notes:**
- All 20 tests should pass with zero failures
- Execution time should match baseline (±10% tolerance)
- Any failures indicate issues with function migration or import updates
- Must fix all issues before proceeding to cleanup tasks

**Testing Subtask:**
- [ ] **Run E2E Tests**: Validate all tests pass with migrated functions

**Testing Focus for Task 5:**

**Behaviors to Verify:**
- [ ] **All tests passing**: 20/20 tests pass
- [ ] **No regressions**: Same tests pass as in baseline
- [ ] **Performance maintained**: Execution time within 10% of baseline
- [ ] **No console errors**: Clean execution with no warnings

**Agent Report Requirements:**
- **@e2e-runner**: Must report test count (20), pass/fail status for each test, execution time, comparison with baseline, detailed failure information if any

</details>

---

<details open>
<summary><h3>Task 6: Clean Up indexing.helpers.ts</h3></summary>

**Purpose**: Remove migrated functions and unused functions from the helpers file, leaving only DOM/UI-related helpers and updating documentation to reflect the changes.

**Related Documentation:**
- [indexing.helpers.ts](../../../apps/teensyrom-ui-e2e/src/support/helpers/indexing.helpers.ts) - File to clean up

**Implementation Subtasks:**
- [ ] **Remove `setupIndexingScenario` function** (lines 40-102, 62 lines) - migrated to interceptors
- [ ] **Remove `waitForIndexingComplete` function** (lines 225-242, 17 lines) - migrated to interceptors
- [ ] **Remove `verifyIndexingCallMade` function** (lines 259-262, 3 lines) - 0 usages, unused
- [ ] **Remove `verifyIndexAllCallMade` function** (lines 276-278, 2 lines) - 0 usages, unused
- [ ] **Remove `verifyStorageIndexButtonState` function** if truly unused (verify with Grep first)
- [ ] **Update file header comments** (lines 1-13): Remove references to migrated functions, update description
- [ ] **Update import statements**: Remove unused imports (`interceptIndexStorage`, `interceptIndexAllStorage`, `INDEXING_INTERCEPT_ALIASES`)
- [ ] **Verify remaining exports**: Only DOM/UI helpers should remain (verifyBusyDialog*, clickStorageIndexButton, clickIndexAllButton)

**Agent Usage for Task 6:**
- Use **@clean-coder** agent to clean up the helpers file
  - File path: `apps/teensyrom-ui-e2e/src/support/helpers/indexing.helpers.ts`
  - Changes: Remove 4-5 functions, update header comments, remove unused imports
  - Agent must report: Functions removed, lines deleted, import changes, remaining exports list

**Key Implementation Notes:**
- Total removal: approximately 84+ lines of code
- File will be significantly smaller after cleanup
- Remaining functions are all DOM/UI-related
- Must verify `verifyStorageIndexButtonState` has zero usages before removal

**Remaining Functions After Cleanup (5 functions):**
1. `verifyBusyDialogDisplayed(timeout?)` - UI verification
2. `verifyBusyDialogHidden(timeout?)` - UI verification
3. `clickStorageIndexButton(storageType)` - UI interaction
4. `clickIndexAllButton()` - UI interaction
5. `verifyStorageIndexButtonState(storageType, shouldBeDisabled)` - UI verification (only if used)

**Updated File Header:**
```typescript
/**
 * Indexing E2E Test Helpers - UI/DOM Interactions
 *
 * Reusable Cypress commands for UI interactions in indexing tests.
 * These helpers compose Cypress commands for DOM queries and user interactions.
 *
 * For API-related helpers (interceptors, wait functions), see:
 * - Interceptors: `indexStorage.interceptors.ts`
 * - Wait functions: `indexStorage.interceptors.ts` (waitForIndexingComplete)
 */
```

**Testing Subtask:**
- [ ] **Verify Cleanup**: Ensure no broken imports and file structure is correct

**Testing Focus for Task 6:**

**Behaviors to Verify:**
- [ ] **Functions removed**: All migrated and unused functions are gone
- [ ] **Imports cleaned**: No unused imports remain
- [ ] **Exports correct**: Only DOM/UI helpers are exported
- [ ] **Documentation updated**: Header reflects new scope

**Agent Report Requirements:**
- **@clean-coder**: Must report functions removed (list names), total lines deleted, import changes made, final export list

</details>

---

<details open>
<summary><h3>Task 7: Update E2E_INTERCEPTORS.md Documentation</h3></summary>

**Purpose**: Document the newly added `setupIndexingInterceptors` and `waitForIndexingComplete` functions in the interceptor documentation, providing clear usage examples and integration guidance.

**Related Documentation:**
- [E2E_INTERCEPTORS.md](../../../apps/teensyrom-ui-e2e/src/support/interceptors/E2E_INTERCEPTORS.md) - Documentation file
- Phase 3B Task 10 - Similar documentation update pattern

**Implementation Subtasks:**
- [ ] **Locate indexing section** in E2E_INTERCEPTORS.md (or create if missing)
- [ ] **Add `setupIndexingInterceptors` documentation**: Describe purpose, parameters, usage examples
- [ ] **Add `waitForIndexingComplete` documentation**: Describe convenience wrapper behavior, parameter routing
- [ ] **Add code examples**: Show typical usage patterns for both functions
- [ ] **Cross-reference**: Link to related interceptor functions (`interceptIndexStorage`, `interceptIndexAllStorage`)
- [ ] **Update table of contents** if the documentation has one

**Agent Usage for Task 7:**
- Use **@clean-coder** agent to update documentation
  - File path: `apps/teensyrom-ui-e2e/src/support/interceptors/E2E_INTERCEPTORS.md`
  - Changes: Add two function documentation sections with examples
  - Agent must report: Sections added, examples provided, any structural changes made

**Key Implementation Notes:**
- Documentation should follow existing patterns in E2E_INTERCEPTORS.md
- Include parameter descriptions and return types
- Provide realistic usage examples from actual test scenarios
- Explain the relationship between setup function and individual interceptors

**Example Documentation Structure:**
```markdown
### setupIndexingInterceptors(fixture, interceptorOptions?)

Orchestrates indexing interceptor setup for multiple devices with storage.

**Parameters:**
- `fixture`: Device fixture data with shape `{ devices: [...] }`
- `interceptorOptions?`: Optional configuration for all interceptors
  - `delay?`: Response delay in milliseconds
  - `statusCode?`: HTTP status code for error scenarios
  - `errorMode?`: Enable error response mode
  - `errorMessage?`: Custom error message

**Usage:**
```typescript
import { setupIndexingInterceptors } from '../../support/interceptors/indexStorage.interceptors';

// Setup interceptors for devices with storage
setupIndexingInterceptors(deviceFixture, { delay: 500 });
```

### waitForIndexingComplete(deviceIdOrAlias, storageType?, timeout?)

Convenience wrapper for waiting on indexing API calls with intelligent routing.

**Parameters:**
- `deviceIdOrAlias`: Device ID for single-device, or alias constant for Index All
- `storageType?`: Optional storage type ('USB' | 'SD') for single-device operations
- `timeout?`: Optional timeout in milliseconds (default: 10000)

**Usage:**
```typescript
// Wait for single-device indexing
waitForIndexingComplete('device-123', 'USB');

// Wait for Index All operation
waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
```
```

**Testing Subtask:**
- [ ] **Verify Documentation**: Ensure examples are accurate and formatting is correct

**Testing Focus for Task 7:**

**Behaviors to Verify:**
- [ ] **Documentation complete**: Both functions documented with parameters and examples
- [ ] **Examples accurate**: Code examples match actual function signatures
- [ ] **Formatting correct**: Markdown renders properly
- [ ] **Cross-references valid**: Links to related functions work

**Agent Report Requirements:**
- **@clean-coder**: Must report sections added, number of examples provided, any structural changes to documentation

</details>

---

<details open>
<summary><h3>Task 8: TypeScript and ESLint Cleanup</h3></summary>

**Purpose**: Run TypeScript compiler and ESLint to identify and fix any type errors, unused imports, or linting issues introduced during the migration.

**Related Documentation:**
- [Coding Standards](../../CODING_STANDARDS.md) - Linting and type safety standards

**Implementation Subtasks:**
- [ ] **Run @code-cleaner agent** on all modified files
- [ ] **Fix TypeScript errors**: Resolve any type mismatches or errors
- [ ] **Fix ESLint warnings**: Address linting issues in modified files
- [ ] **Verify no unused imports**: Ensure all imports are used
- [ ] **Check for formatting issues**: Run formatter if needed
- [ ] **Review agent report**: Confirm all issues resolved

**Agent Usage for Task 8:**
- Use **@code-cleaner** agent for workspace-wide cleanup
  - Scope: All modified files from this phase
  - Agent must report: TypeScript errors found/fixed, ESLint warnings found/fixed, remaining issues if any, affected files list

**Key Implementation Notes:**
- Focus on files modified during this phase:
  - `indexStorage.interceptors.ts`
  - `device-indexing.cy.ts`
  - `indexing.helpers.ts`
- Common issues to expect:
  - Unused imports after function removal
  - Type inference issues with new functions
  - ESLint warnings about function complexity
- All errors must be resolved before proceeding

**Testing Subtask:**
- [ ] **Run Type Checking**: Verify no TypeScript or ESLint errors remain

**Testing Focus for Task 8:**

**Behaviors to Verify:**
- [ ] **TypeScript clean**: `npx tsc` runs with zero errors
- [ ] **ESLint clean**: `npx eslint` runs with zero warnings on modified files
- [ ] **No unused imports**: All imports are utilized
- [ ] **Formatting consistent**: Code follows project style guide

**Agent Report Requirements:**
- **@code-cleaner**: Must report total errors found, errors fixed, remaining issues (should be 0), list of affected files

</details>

---

<details open>
<summary><h3>Task 9: Comment Cleanup</h3></summary>

**Purpose**: Review and clean up comments in all modified files, removing outdated comments while preserving valuable documentation and maintaining code clarity.

**Related Documentation:**
- [Coding Standards](../../CODING_STANDARDS.md) - Comment guidelines

**Implementation Subtasks:**
- [ ] **Run @comment-cleaner agents in parallel** on all modified files
- [ ] **Remove outdated comments**: Delete comments referencing old function names
- [ ] **Preserve JSDoc comments**: Keep function documentation
- [ ] **Preserve TODO comments**: Keep actionable TODO items
- [ ] **Remove redundant comments**: Clean up obvious/unnecessary comments
- [ ] **Review agent reports**: Verify appropriate comment preservation

**Agent Usage for Task 9:**
- Launch **3 @comment-cleaner agents IN PARALLEL** (one per file):
  1. `apps/teensyrom-ui-e2e/src/support/interceptors/indexStorage.interceptors.ts`
  2. `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts`
  3. `apps/teensyrom-ui-e2e/src/support/helpers/indexing.helpers.ts`
- Each agent must report: File processed, comments removed, comments preserved (with reason), any issues

**Key Implementation Notes:**
- This is the ONLY task that runs agents in parallel
- Each agent operates on a different file (no conflicts)
- Preserve valuable documentation (JSDoc, complex logic explanations, TODOs)
- Remove obvious comments that duplicate code intent
- Update comments that reference old function names

**Testing Subtask:**
- [ ] **Review Comments**: Verify comment quality and relevance

**Testing Focus for Task 9:**

**Behaviors to Verify:**
- [ ] **Outdated comments removed**: No references to old function names
- [ ] **JSDoc preserved**: Function documentation intact
- [ ] **Code clarity maintained**: Important explanatory comments kept
- [ ] **No redundant comments**: Obvious comments removed

**Agent Report Requirements:**
- **@comment-cleaner (each instance)**: Must report file path, comments removed (with examples), comments preserved (with reasons), any issues encountered

</details>

---

## 🗂️ Files Modified or Created

**Modified Files:**
- `apps/teensyrom-ui-e2e/src/support/interceptors/indexStorage.interceptors.ts` - Add 2 new functions
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts` - Update imports and 22 function calls
- `apps/teensyrom-ui-e2e/src/support/helpers/indexing.helpers.ts` - Remove 4-5 functions (~84 lines)
- `apps/teensyrom-ui-e2e/src/support/interceptors/E2E_INTERCEPTORS.md` - Add documentation for new functions

**No New Files Created**

---

## 📝 Testing Summary

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
> - **Favor behavioral testing** - test what users/consumers observe, not how it's implemented
> - **Test as you go** - tests are integrated into each task's subtasks, not deferred to the end
> - **Agent-driven validation** - use @e2e-runner to validate changes after each migration step
> - **Fix issues immediately** - address any test failures before proceeding to next task

> **Reference Documentation:**
> - **All tasks**: [Testing Standards](../../TESTING_STANDARDS.md) - Core behavioral testing approach

### Agent Execution Workflow

**Sequential Tasks (Tasks 1-8):**
1. **@clean-coder** performs implementation or migration for specific file(s)
2. **@e2e-runner** validates changes (for test file modifications)
3. Review both agent reports:
   - @clean-coder report: What changed, any issues encountered
   - @e2e-runner report: Test results, pass/fail status, error details
4. Fix any reported issues before proceeding to next task
5. Move to next sequential task

**Parallel Task (Task 9 only):**
- Launch **3 @comment-cleaner agents simultaneously** on different files
- Each agent reports independently
- Review all reports and address any issues found

**Agent Report Requirements:**
- **@clean-coder**: Changes made, files modified, function additions/removals, import updates, issues
- **@e2e-runner**: Test results, pass/fail counts, execution time, detailed failure information, baseline comparison
- **@code-cleaner**: Errors found/fixed, remaining issues, affected files
- **@comment-cleaner**: Files processed, comments removed/preserved, issues

### Where Tests Are Written

**Tests are embedded in each task above** with:
- **Testing Subtask**: Checkbox in the task's subtask list
- **Testing Focus**: "Behaviors to Test/Verify" section listing observable outcomes
- **Agent Usage**: Agent invocation with reporting requirements

**Complete each task's testing subtask before moving to the next task.**

---

## ✅ Success Criteria

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] `setupIndexingInterceptors` function added to interceptor file with correct implementation
- [ ] `waitForIndexingComplete` function added to interceptor file with correct implementation
- [ ] Test file updated with new imports and function names (38 updates)
- [ ] Helpers file cleaned up (4-5 functions removed, ~84 lines deleted)
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)

**Testing Requirements:**
- [ ] Baseline established (Task 3) with all tests passing
- [ ] Post-migration validation (Task 5) shows all 20 tests passing
- [ ] No test failures introduced by migration
- [ ] Execution time maintained within 10% of baseline
- [ ] All behavioral tests verified

**Quality Checks:**
- [ ] No TypeScript errors or warnings (Task 8 @code-cleaner)
- [ ] Linting passes with no errors on modified files
- [ ] Code formatting is consistent
- [ ] No console errors when running tests
- [ ] Comments are clean and relevant (Task 9 @comment-cleaner)

**Documentation:**
- [ ] E2E_INTERCEPTORS.md updated with new functions (Task 7)
- [ ] Function documentation includes parameters, examples, usage patterns
- [ ] File header comments updated in indexing.helpers.ts
- [ ] Cross-references to related functions added

**Agent Execution:**
- [ ] All agent reports reviewed and issues addressed
- [ ] @e2e-runner baseline captured (Task 3)
- [ ] @clean-coder migrations completed successfully (Tasks 1, 2, 4, 6, 7)
- [ ] @e2e-runner validation passed (Task 5)
- [ ] @code-cleaner cleanup completed (Task 8)
- [ ] @comment-cleaner parallel execution completed (Task 9)

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] All tests passing (20/20)
- [ ] Documentation complete and accurate

---

## 📝 Notes & Considerations

### Design Decisions

- **Rename to setupIndexingInterceptors**: Better reflects purpose of interceptor setup rather than full scenario orchestration
- **Migrate waitForIndexingComplete**: Despite interceptors having specific wait functions, the convenience wrapper provides value by intelligently routing between single-device and Index All operations
- **Remove unused functions**: Clean up `verifyIndexingCallMade`, `verifyIndexAllCallMade`, and `verifyStorageIndexButtonState` (all have 0 usages)
- **Keep DOM helpers in helpers file**: Clear separation of concerns - interceptors handle API mocking, helpers handle UI interactions

### Implementation Constraints

- **Single test file affected**: Only `device-indexing.cy.ts` uses these functions, making migration lower risk than Phase 3B
- **Function signature compatibility**: All migrated functions maintain identical signatures for seamless migration
- **Import path changes**: Test file will have split imports (interceptors for API, helpers for UI)

### Migration Scope

- **Functions migrated**: 2 (setupIndexingScenario → setupIndexingInterceptors, waitForIndexingComplete)
- **Functions removed**: 3-4 (unused functions cleaned up)
- **Function calls updated**: 22 (setupIndexingScenario renames) + 16 (waitForIndexingComplete, no changes needed)
- **Test files affected**: 1 (device-indexing.cy.ts with 20 test cases)
- **Total lines removed from helpers**: ~84 lines

### Future Enhancements

- **Consider deprecating waitForIndexingComplete**: If direct usage of `waitForIndexStorage` and `waitForIndexAllStorage` becomes preferred pattern
- **Expand documentation**: Add more complex usage examples to E2E_INTERCEPTORS.md
- **Test coverage**: Add unit tests for interceptor functions if beneficial

### Relationship to Other Phases

- **Follows Phase 3B pattern**: Similar migration approach with agent orchestration and reporting
- **Part of overall primitives plan**: Continues the work of consolidating API-related utilities in interceptor files
- **Smaller scope**: Only 1 test file vs 7 test files in Phase 3B, making this a lower-risk migration

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

---

## 💡 Agent Implementation Guide

> **Instructions for AI agents creating and using this document**

### Execution Strategy

**Sequential Execution (Tasks 1-8):**
1. Complete each task fully before moving to next
2. Review agent reports after each task
3. Fix any issues immediately
4. Verify success criteria for task before proceeding

**Parallel Execution (Task 9 only):**
1. Launch 3 @comment-cleaner agents simultaneously
2. Each operates on different file (no conflicts)
3. Review all reports when complete
4. Address any issues found

### Agent Coordination

**Task Dependencies:**
- Task 1 & 2 must complete before Task 3 (need new functions for baseline)
- Task 3 baseline must complete before Task 4 (need comparison point)
- Task 4 must complete before Task 5 (need updated file to test)
- Task 5 must pass before Task 6 (verify migration works before cleanup)
- Task 6 must complete before Task 7 (know final state for documentation)
- Task 7 must complete before Task 8 (have all code changes for type checking)
- Task 8 must complete before Task 9 (clean code before comment cleanup)

**Critical Path:**
Tasks 1-2 (Add functions) → Task 3 (Baseline) → Task 4 (Migrate test) → Task 5 (Validate) → Task 6 (Cleanup) → Task 7 (Document) → Task 8 (Type check) → Task 9 (Comments)

### Reporting Requirements

**Every agent must report:**
- What was completed
- What changed (specific files, lines, functions)
- Any issues encountered
- Success/failure status
- Relevant metrics (test counts, errors fixed, etc.)

**Orchestrator must:**
- Review each report before proceeding
- Address any failures immediately
- Compare results with success criteria
- Document any deviations or discoveries

### Remember

- **One task at a time** (except Task 9 parallel execution)
- **Agent reports are critical** for orchestrator decision-making
- **Fix issues immediately** - don't defer problems
- **Verify success criteria** after each task
- **Document discoveries** in Notes section

---

## 🎯 Quick Reference

**Total Scope:**
- Functions migrated: 2
- Functions removed: 3-4
- Test files affected: 1
- Function call updates: 38 (22 renames + 16 unchanged)
- Lines removed from helpers: ~84

**Execution Pattern:**
- Sequential: Tasks 1-8
- Parallel: Task 9 (3 agents on different files)

**Key Agents:**
- @clean-coder: Tasks 1, 2, 4, 6, 7 (implementation and migration)
- @e2e-runner: Tasks 3, 5 (baseline and validation)
- @code-cleaner: Task 8 (TypeScript/ESLint)
- @comment-cleaner: Task 9 (parallel comment cleanup)

**Success Metric:**
All 20 tests in device-indexing.cy.ts pass with updated function names and imports.
