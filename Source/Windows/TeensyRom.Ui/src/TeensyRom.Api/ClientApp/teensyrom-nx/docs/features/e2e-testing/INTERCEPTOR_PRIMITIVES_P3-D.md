# Phase 3D: Player Test Helpers Migration to Interceptors

## 🎯 Objective

Migrate API-related functions from `player/test-helpers.ts` to their appropriate interceptor files, eliminating duplicate wait/setup functions and ensuring tests use interceptor primitives directly. This phase splits composite functions into separate DOM and API concerns, updates 2 test files (26 tests total), and validates each file individually before proceeding.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**

- [ ] [Interceptor Primitives Plan](./INTERCEPTOR_PRIMITIVES_PLAN.md) - Overall architecture plan
- [ ] [Phase 3C - Indexing Helpers Migration](./INTERCEPTOR_PRIMITIVES_P3-C.md) - Similar migration pattern

**Standards & Guidelines:**

- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices
- [ ] [E2E Testing Guide](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - E2E testing patterns

---

## 📂 File Structure Overview

```
apps/teensyrom-ui-e2e/src/
├── e2e/player/
│   ├── test-helpers.ts                      📝 Modified - Remove 11 API functions (~90 lines)
│   ├── favorite-functionality.cy.ts         📝 Modified - Update ~15 function calls (12 tests)
│   └── deep-linking.cy.ts                   📝 Modified - Update ~13 function calls (14 tests)
├── support/interceptors/
│   ├── getDirectory.interceptors.ts         ✅ Already has waitForGetDirectory()
│   ├── launchFile.interceptors.ts           ✅ Already has waitForLaunchFile()
│   ├── launchRandom.interceptors.ts         ✅ Already has waitForLaunchRandom()
│   ├── saveFavorite.interceptors.ts         ✅ Already has waitForSaveFavorite(), setupErrorSaveFavorite()
│   └── removeFavorite.interceptors.ts       ✅ Already has waitForRemoveFavorite(), setupErrorRemoveFavorite()
```

---

## 📋 Migration Details

### Functions to Remove from test-helpers.ts

**Wait Functions (8 functions - already exist in interceptors):**

1. `waitForDirectoryLoad()` (line 153-155) → Use `waitForGetDirectory()` from getDirectory.interceptors
2. `waitForFileToLoad()` (line 170-172) → Use `waitForGetDirectory()` from getDirectory.interceptors
3. `waitForFileLaunch()` (line 177-179) → Use `waitForLaunchFile()` from launchFile.interceptors
4. `waitForRandomLaunch()` (line 184-186) → Use `waitForLaunchRandom()` from launchRandom.interceptors
5. `waitForSaveFavorite()` (line 216-218) → Use from saveFavorite.interceptors
6. `waitForRemoveFavorite()` (line 223-225) → Use from removeFavorite.interceptors
7. `setupSaveFavoriteErrorScenario()` (line 382-388) → Use `setupErrorSaveFavorite()` from saveFavorite.interceptors
8. `setupRemoveFavoriteErrorScenario()` (line 393-399) → Use `setupErrorRemoveFavorite()` from removeFavorite.interceptors

**Composite Functions to Split (2 functions):**

1. `clickFavoriteButtonAndWait()` (line 271-274)

   - Keep `clickFavoriteButton()` in test-helpers (DOM action)
   - Tests call both `clickFavoriteButton()` and `waitForSaveFavorite()` separately
   - Used in 3 places in favorite-functionality.cy.ts

2. `clickFavoriteButtonAndWaitForRemove()` (line 279-282)
   - Keep `clickFavoriteButton()` in test-helpers (DOM action)
   - Tests call both `clickFavoriteButton()` and `waitForRemoveFavorite()` separately
   - Used in 4 places in favorite-functionality.cy.ts

**Orchestration Function to Inline (1 function):**

1. `launchFileFromFavorites()` (line 347-366)
   - Only 1 usage in favorite-functionality.cy.ts
   - Inline the logic directly in the test

### Test File Changes Summary

**favorite-functionality.cy.ts (12 tests):**

- Update imports (remove 8 functions from test-helpers, add 4 from interceptors)
- Replace 7 composite function calls with split DOM + API calls
- Replace 1 `launchFileFromFavorites()` call with inlined logic
- Replace 2 setup error scenario calls with interceptor equivalents
- ~15 total function call updates

**deep-linking.cy.ts (14 tests):**

- Update imports (remove 4 functions from test-helpers, add 4 from interceptors)
- Replace 4 `waitForFileToLoad()` with `waitForGetDirectory()`
- Replace 4 `waitForFileLaunch()` with `waitForLaunchFile()`
- Replace 2 `waitForRandomLaunch()` calls
- ~13 total function call updates

---

<details open>
<summary><h3>Task 1: Establish E2E Test Baseline</h3></summary>

**Purpose**: Run all affected E2E tests before making any changes to establish a passing baseline and verify current test suite health.

**Related Documentation:**

- [E2E Testing Guide](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - E2E test execution patterns

**Implementation Subtasks:**

- [ ] **Run @e2e-runner agent**: Execute E2E tests for both test files to establish baseline
  - Test file 1: `apps/teensyrom-ui-e2e/src/e2e/player/favorite-functionality.cy.ts` (12 tests)
  - Test file 2: `apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts` (14 tests)
- [ ] **Verify baseline**: Confirm all 26 tests pass before proceeding
- [ ] **Document baseline**: Save test results for comparison after migration

**Agent Execution:**

```
@e2e-runner agent:
- Run: apps/teensyrom-ui-e2e/src/e2e/player/favorite-functionality.cy.ts
- Run: apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts
- Generate JSON report showing all tests passing
- Report any pre-existing failures (should be zero)
```

**Key Implementation Notes:**

- All 26 tests must pass before proceeding to migration
- If any tests fail at baseline, fix them first before starting migration
- Save test execution time for performance comparison

**Success Criteria:**

- [ ] All 12 tests in favorite-functionality.cy.ts passing
- [ ] All 14 tests in deep-linking.cy.ts passing
- [ ] Baseline results documented
- [ ] No pre-existing test failures

</details>

---

<details open>
<summary><h3>Task 2: Migrate favorite-functionality.cy.ts</h3></summary>

**Purpose**: Update the favorite-functionality test file to use interceptor functions directly, split composite functions, and inline the orchestration function.

**Related Documentation:**

- [Phase 3C - Indexing Helpers Migration](./INTERCEPTOR_PRIMITIVES_P3-C.md) - Similar test file migration pattern

**Implementation Subtasks:**

- [ ] **Update imports**: Remove test-helper imports, add interceptor imports

  - Remove from test-helpers: `waitForDirectoryLoad`, `waitForSaveFavorite`, `waitForRemoveFavorite`, `clickFavoriteButtonAndWait`, `clickFavoriteButtonAndWaitForRemove`, `launchFileFromFavorites`, `setupSaveFavoriteErrorScenario`, `setupRemoveFavoriteErrorScenario`
  - Add from interceptors: `waitForGetDirectory`, `waitForSaveFavorite`, `waitForRemoveFavorite`, `setupErrorSaveFavorite`, `setupErrorRemoveFavorite`
  - Keep from test-helpers: All DOM/UI functions like `clickFavoriteButton`, `verifyFavoriteIconIsEmpty`, etc.

- [ ] **Split composite function calls** (7 locations):

  - Replace 3 `clickFavoriteButtonAndWait()` calls with:
    ```typescript
    clickFavoriteButton();
    waitForSaveFavorite();
    ```
  - Replace 4 `clickFavoriteButtonAndWaitForRemove()` calls with:
    ```typescript
    clickFavoriteButton();
    waitForRemoveFavorite();
    ```

- [ ] **Inline orchestration function** (1 location):

  - Replace `launchFileFromFavorites({ device, storage, fileName })` with inlined logic:
    ```typescript
    navigateToDirectory({
      device: params.device,
      storage: TeensyStorageType.Sd,
      path: TEST_PATHS.FAVORITES_GAMES,
      file: params.fileName,
    });
    waitForGetDirectory();
    waitForLaunchFile();
    waitForDirectoryFilesToBeVisible(TEST_PATHS.FAVORITES_GAMES);
    const favoritesFilePath = `${TEST_PATHS.FAVORITES_GAMES}/${params.fileName}`;
    cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(favoritesFilePath))
      .should('be.visible')
      .click({ force: true });
    ```

- [ ] **Replace error setup functions** (4 locations):

  - Replace 2 `setupSaveFavoriteErrorScenario(filesystem)` with `setupErrorSaveFavorite(filesystem)`
  - Replace 2 `setupRemoveFavoriteErrorScenario(filesystem)` with `setupErrorRemoveFavorite(filesystem)`

- [ ] **Replace wait function** (1 location):
  - Replace 1 `waitForDirectoryLoad()` with `waitForGetDirectory()`

**Agent Execution:**

```
@clean-coder agent:
- Update file: apps/teensyrom-ui-e2e/src/e2e/player/favorite-functionality.cy.ts
- Follow the subtasks above
- Ensure all 12 tests maintain their original behavior
- Preserve test structure and assertions
```

**Key Implementation Notes:**

- Split composite functions make tests more explicit about what's happening
- Inlined orchestration logic is only used once, so duplication isn't a concern
- Use interceptor function names (`waitForGetDirectory` not `waitForDirectoryLoad`)
- Import `TEST_PATHS` and `DIRECTORY_FILES_SELECTORS` if not already imported for the inlined logic

**Testing Focus:**

- All 12 tests should continue passing after migration
- No changes to test behavior, only to how interceptors are called
- Test execution time should be similar to baseline

</details>

---

<details open>
<summary><h3>Task 3: Validate favorite-functionality.cy.ts</h3></summary>

**Purpose**: Run E2E tests for the updated favorite-functionality.cy.ts file to ensure all tests pass after migration.

**Implementation Subtasks:**

- [ ] **Run @e2e-runner agent**: Execute E2E tests for favorite-functionality.cy.ts
- [ ] **Verify all tests pass**: Confirm all 12 tests still passing
- [ ] **Compare with baseline**: Check that test execution time is similar
- [ ] **Fix any failures**: If tests fail, use @clean-coder to repair

**Agent Execution:**

```
@e2e-runner agent:
- Run: apps/teensyrom-ui-e2e/src/e2e/player/favorite-functionality.cy.ts
- Generate JSON report showing test results
- Report any failures with detailed error messages
- If failures occur, investigate and report root cause
```

**Key Implementation Notes:**

- All 12 tests must pass before proceeding to next file
- If failures occur, they're likely due to incorrect function name replacements or missing imports
- Common issues: Wrong interceptor alias, missing import statements, incorrect function signatures

**Success Criteria:**

- [ ] All 12 tests passing
- [ ] No new test failures introduced
- [ ] Test execution time within 10% of baseline
- [ ] Ready to proceed to next test file

</details>

---

<details open>
<summary><h3>Task 4: TypeScript/ESLint Cleanup for favorite-functionality.cy.ts</h3></summary>

**Purpose**: Clean up any TypeScript or ESLint errors in the updated test file.

**Implementation Subtasks:**

- [ ] **Run @code-cleaner agent**: Fix TypeScript and ESLint errors in favorite-functionality.cy.ts
- [ ] **Verify no errors**: Confirm file has no type or lint errors
- [ ] **Run tests again**: Quick validation that cleanup didn't break tests

**Agent Execution:**

```
@code-cleaner agent:
- Clean file: apps/teensyrom-ui-e2e/src/e2e/player/favorite-functionality.cy.ts
- Fix any TypeScript errors
- Fix any ESLint errors
- Optimize imports (remove unused, sort alphabetically)
- Run tests after cleanup to ensure no breakage
```

**Key Implementation Notes:**

- Focus on type safety and import optimization
- Don't change test logic, only fix errors
- Common cleanup: Unused imports, type assertions, spacing

**Success Criteria:**

- [ ] No TypeScript errors
- [ ] No ESLint errors
- [ ] All tests still passing
- [ ] Imports optimized

</details>

---

<details open>
<summary><h3>Task 5: Migrate deep-linking.cy.ts</h3></summary>

**Purpose**: Update the deep-linking test file to use interceptor functions directly with correct naming conventions.

**Related Documentation:**

- [Phase 3C - Indexing Helpers Migration](./INTERCEPTOR_PRIMITIVES_P3-C.md) - Similar test file migration pattern

**Implementation Subtasks:**

- [ ] **Update imports**: Remove test-helper imports, add interceptor imports

  - Remove from test-helpers: `waitForFileToLoad`, `waitForFileLaunch`, `waitForRandomLaunch`
  - Add from interceptors: `waitForGetDirectory`, `waitForLaunchFile`, `waitForLaunchRandom`
  - Keep from test-helpers: All DOM/UI functions like `navigateToPlayer`, `clickNextButton`, etc.

- [ ] **Replace wait functions** (10 locations):
  - Replace 4 `waitForFileToLoad()` with `waitForGetDirectory()`
  - Replace 4 `waitForFileLaunch()` with `waitForLaunchFile()`
  - Replace 2 `waitForRandomLaunch()` with `waitForLaunchRandom()`

**Agent Execution:**

```
@clean-coder agent:
- Update file: apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts
- Follow the subtasks above
- Ensure all 14 tests maintain their original behavior
- Preserve test structure and assertions
```

**Key Implementation Notes:**

- This file has simpler changes than favorite-functionality (only wait function replacements)
- No composite functions to split
- No orchestration functions to inline
- Use interceptor function names consistently

**Testing Focus:**

- All 14 tests should continue passing after migration
- No changes to test behavior, only to how interceptors are called
- Test execution time should be similar to baseline

</details>

---

<details open>
<summary><h3>Task 6: Validate deep-linking.cy.ts</h3></summary>

**Purpose**: Run E2E tests for the updated deep-linking.cy.ts file to ensure all tests pass after migration.

**Implementation Subtasks:**

- [ ] **Run @e2e-runner agent**: Execute E2E tests for deep-linking.cy.ts
- [ ] **Verify all tests pass**: Confirm all 14 tests still passing
- [ ] **Compare with baseline**: Check that test execution time is similar
- [ ] **Fix any failures**: If tests fail, use @clean-coder to repair

**Agent Execution:**

```
@e2e-runner agent:
- Run: apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts
- Generate JSON report showing test results
- Report any failures with detailed error messages
- If failures occur, investigate and report root cause
```

**Key Implementation Notes:**

- All 14 tests must pass before proceeding to cleanup phase
- If failures occur, they're likely due to incorrect function name replacements
- Common issues: Wrong function name (`waitForFileToLoad` vs `waitForGetDirectory`)

**Success Criteria:**

- [ ] All 14 tests passing
- [ ] No new test failures introduced
- [ ] Test execution time within 10% of baseline
- [ ] Ready to proceed to cleanup phase

</details>

---

<details open>
<summary><h3>Task 7: TypeScript/ESLint Cleanup for deep-linking.cy.ts</h3></summary>

**Purpose**: Clean up any TypeScript or ESLint errors in the updated test file.

**Implementation Subtasks:**

- [ ] **Run @code-cleaner agent**: Fix TypeScript and ESLint errors in deep-linking.cy.ts
- [ ] **Verify no errors**: Confirm file has no type or lint errors
- [ ] **Run tests again**: Quick validation that cleanup didn't break tests

**Agent Execution:**

```
@code-cleaner agent:
- Clean file: apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts
- Fix any TypeScript errors
- Fix any ESLint errors
- Optimize imports (remove unused, sort alphabetically)
- Run tests after cleanup to ensure no breakage
```

**Key Implementation Notes:**

- Focus on type safety and import optimization
- Don't change test logic, only fix errors
- Common cleanup: Unused imports, type assertions, spacing

**Success Criteria:**

- [ ] No TypeScript errors
- [ ] No ESLint errors
- [ ] All tests still passing
- [ ] Imports optimized

</details>

---

<details open>
<summary><h3>Task 8: Remove Functions from test-helpers.ts</h3></summary>

**Purpose**: Clean up test-helpers.ts by removing all migrated API-related functions, leaving only DOM/UI helper functions.

**Related Documentation:**

- [Coding Standards](../../CODING_STANDARDS.md) - File organization patterns

**Implementation Subtasks:**

- [ ] **Remove wait functions** (6 functions):

  - Remove `waitForDirectoryLoad()` (line 153-155)
  - Remove `waitForFileToLoad()` (line 170-172)
  - Remove `waitForFileLaunch()` (line 177-179)
  - Remove `waitForRandomLaunch()` (line 184-186)
  - Remove `waitForSaveFavorite()` (line 216-218)
  - Remove `waitForRemoveFavorite()` (line 223-225)

- [ ] **Remove error setup functions** (2 functions):

  - Remove `setupSaveFavoriteErrorScenario()` (line 382-388)
  - Remove `setupRemoveFavoriteErrorScenario()` (line 393-399)

- [ ] **Remove composite functions** (2 functions):

  - Remove `clickFavoriteButtonAndWait()` (line 271-274)
  - Remove `clickFavoriteButtonAndWaitForRemove()` (line 279-282)

- [ ] **Remove orchestration function** (1 function):

  - Remove `launchFileFromFavorites()` (line 347-366)

- [ ] **Update header documentation**:

  - Update file comment to reflect that only DOM/UI helpers remain
  - Remove references to API wait functions
  - Update import examples in comments

- [ ] **Clean up imports**:
  - Remove `interceptSaveFavorite` import (no longer used)
  - Remove `interceptRemoveFavorite` import (no longer used)
  - Remove `MockFilesystem` import if no longer used

**Agent Execution:**

```
@clean-coder agent:
- Update file: apps/teensyrom-ui-e2e/src/e2e/player/test-helpers.ts
- Remove 11 functions (approximately 90 lines)
- Update header documentation
- Clean up unused imports
- Preserve all DOM/UI helper functions
```

**Key Implementation Notes:**

- File should only contain DOM/UI helper functions after cleanup
- Keep all navigation, click, verify, and DOM wait functions
- Header comment should clearly state this file is for DOM/UI helpers only
- No test files should break since we already migrated them

**Testing Focus:**

- No testing needed for this task since all test files already migrated
- Next task will run final validation to ensure nothing broke

**Success Criteria:**

- [ ] All 11 API-related functions removed
- [ ] Header documentation updated
- [ ] Unused imports cleaned up
- [ ] All DOM/UI functions preserved
- [ ] File compiles without errors

</details>

---

<details open>
<summary><h3>Task 9: Final E2E Validation</h3></summary>

**Purpose**: Run complete E2E test suite one final time to ensure all changes work correctly together.

**Implementation Subtasks:**

- [ ] **Run @e2e-runner agent**: Execute both test files to validate complete migration
- [ ] **Verify all tests pass**: Confirm all 26 tests still passing
- [ ] **Compare with baseline**: Verify test execution times are similar
- [ ] **Document results**: Save final test results for comparison

**Agent Execution:**

```
@e2e-runner agent:
- Run: apps/teensyrom-ui-e2e/src/e2e/player/favorite-functionality.cy.ts
- Run: apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts
- Generate JSON report showing all tests passing
- Compare execution times with baseline
- Report final migration success
```

**Key Implementation Notes:**

- This is the final validation before comment cleanup
- All 26 tests must pass
- Test execution times should be within 10% of baseline
- If any failures occur, investigate and fix before proceeding

**Success Criteria:**

- [ ] All 12 tests in favorite-functionality.cy.ts passing
- [ ] All 14 tests in deep-linking.cy.ts passing
- [ ] Test execution times similar to baseline
- [ ] No errors or warnings in test output
- [ ] Ready for final cleanup phase

</details>

---

<details open>
<summary><h3>Task 10: Parallel Comment Cleanup</h3></summary>

**Purpose**: Run comment cleanup on all modified files in parallel to remove outdated comments and ensure documentation is current.

**Implementation Subtasks:**

- [ ] **Run @comment-cleaner agents in parallel** (3 agents):
  - Agent 1: Clean `apps/teensyrom-ui-e2e/src/e2e/player/test-helpers.ts`
  - Agent 2: Clean `apps/teensyrom-ui-e2e/src/e2e/player/favorite-functionality.cy.ts`
  - Agent 3: Clean `apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts`

**Agent Execution:**

```
Run 3 @comment-cleaner agents in parallel:

Agent 1:
- File: apps/teensyrom-ui-e2e/src/e2e/player/test-helpers.ts
- Remove outdated comments referencing removed functions
- Update header documentation to reflect DOM/UI focus
- Preserve valuable comments for remaining functions

Agent 2:
- File: apps/teensyrom-ui-e2e/src/e2e/player/favorite-functionality.cy.ts
- Remove outdated import comments
- Update test comments to reflect new function usage
- Preserve test documentation and describe blocks

Agent 3:
- File: apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts
- Remove outdated import comments
- Update test comments to reflect new function usage
- Preserve test documentation and describe blocks
```

**Key Implementation Notes:**

- All three agents can run in parallel since they operate on different files
- Focus on removing outdated comments, not all comments
- Preserve valuable documentation like JSDoc, test descriptions, and complex logic explanations
- Don't remove TODO comments or structural organization comments

**Success Criteria:**

- [ ] All three files cleaned simultaneously
- [ ] Outdated comments removed
- [ ] Valuable documentation preserved
- [ ] Files still compile and tests still pass

</details>

---

## 🗂️ Files Modified or Created

**Modified Files:**

- `apps/teensyrom-ui-e2e/src/e2e/player/test-helpers.ts` - Remove 11 functions, update documentation
- `apps/teensyrom-ui-e2e/src/e2e/player/favorite-functionality.cy.ts` - Update imports and ~15 function calls
- `apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts` - Update imports and ~13 function calls

**No New Files Created**

---

<details open>
<summary><h2>📝 Agent Workflow Summary</h2></summary>

This migration uses a systematic agent-based workflow with validation at each step:

### Phase 1: Baseline

1. **@e2e-runner** - Establish baseline for both test files (26 tests)

### Phase 2: First Test File (favorite-functionality.cy.ts)

2. **@clean-coder** - Update favorite-functionality.cy.ts with new imports and split functions
3. **@e2e-runner** - Validate 12 tests in favorite-functionality.cy.ts
4. **@code-cleaner** - Clean TypeScript/ESLint errors in favorite-functionality.cy.ts

### Phase 3: Second Test File (deep-linking.cy.ts)

5. **@clean-coder** - Update deep-linking.cy.ts with new imports
6. **@e2e-runner** - Validate 14 tests in deep-linking.cy.ts
7. **@code-cleaner** - Clean TypeScript/ESLint errors in deep-linking.cy.ts

### Phase 4: Cleanup

8. **@clean-coder** - Remove functions from test-helpers.ts
9. **@e2e-runner** - Final validation of all 26 tests
10. **@comment-cleaner (3x parallel)** - Clean comments in all 3 modified files

### Key Workflow Principles

- **Validate after each test file** - Catch issues early before moving to next file
- **Clean code immediately** - Fix TypeScript/ESLint errors before moving forward
- **One file at a time** - Isolate changes to reduce debugging complexity
- **Parallel cleanup** - Use 3 comment-cleaner agents simultaneously for efficiency
- **Comprehensive baseline** - Always know the starting state before making changes

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

**Functional Requirements:**

- [ ] All 11 API-related functions removed from test-helpers.ts
- [ ] All 2 composite functions split in test files
- [ ] All 1 orchestration function inlined in test
- [ ] All test files using interceptor function names (`waitForGetDirectory` not `waitForDirectoryLoad`)
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)

**Testing Requirements:**

- [ ] Baseline established with all 26 tests passing
- [ ] favorite-functionality.cy.ts validated individually (12 tests passing)
- [ ] deep-linking.cy.ts validated individually (14 tests passing)
- [ ] Final validation with all 26 tests passing
- [ ] Test execution times within 10% of baseline

**Quality Checks:**

- [ ] No TypeScript errors or warnings
- [ ] No ESLint errors
- [ ] All imports optimized and sorted
- [ ] No console errors when running tests
- [ ] Comments cleaned and documentation current

**Agent Execution:**

- [ ] All 10 tasks completed in sequence
- [ ] Each @e2e-runner execution successful
- [ ] Each @clean-coder execution successful
- [ ] Each @code-cleaner execution successful
- [ ] All 3 @comment-cleaner executions successful (parallel)

**Documentation:**

- [ ] test-helpers.ts header updated to reflect DOM/UI focus only
- [ ] No references to removed functions in comments
- [ ] All test file comments current and accurate

**Ready for Next Phase:**

- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] All tests passing
- [ ] Ready to proceed to next interceptor migration

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Split Composite Functions**: Splitting `clickFavoriteButtonAndWait()` into separate DOM and API calls makes tests more explicit and follows the separation of concerns principle
- **Inline Orchestration**: `launchFileFromFavorites()` is only used once, so inlining eliminates unnecessary abstraction
- **Use Interceptor Names**: Using function names from interceptor files (`waitForGetDirectory`) instead of helper names (`waitForDirectoryLoad`) maintains consistency across the codebase
- **One File at a Time**: Updating and validating each test file individually reduces risk and makes debugging easier

### Implementation Constraints

- **Function Name Changes**: Tests must use exact interceptor function names, not helper aliases
- **Import Organization**: Must import from correct files (interceptors vs helpers)
- **Test Behavior**: All test behavior must remain identical after migration
- **Error Scenarios**: Error setup functions have slightly different names in interceptors (`setupErrorSaveFavorite` vs `setupSaveFavoriteErrorScenario`)

### Migration Statistics

- **Functions Removed**: 11 total (6 wait, 2 error setup, 2 composite, 1 orchestration)
- **Lines Removed**: ~90 lines from test-helpers.ts
- **Test Files Updated**: 2 files
- **Total Tests**: 26 tests (12 + 14)
- **Function Call Updates**: ~28 updates across both test files
- **Import Updates**: ~12 import statement changes

### Future Enhancements

- **Standardize Wait Functions**: Consider standardizing all wait function names across interceptors (e.g., always `waitFor[EndpointName]`)
- **Error Setup Patterns**: Consider standardizing error setup function names (e.g., always `setup[EndpointName]Error`)
- **Test Orchestration Helpers**: Consider creating a dedicated `test-orchestration.ts` file for high-level test workflows if more complex scenarios emerge

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

</details>
