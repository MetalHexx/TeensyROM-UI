# Phase 3B: Wait Helper Migration to Interceptors

## 🎯 Objective

Migrate all API wait helper functions from `test-helpers.ts` to their corresponding interceptor files, establishing a unified naming convention and clear separation of concerns between API-level helpers and UI-level helpers. This phase eliminates duplicate wait functionality and consolidates API testing utilities within the interceptor layer where they logically belong.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [Interceptor Primitives Plan](./INTERCEPTOR_PRIMITIVES_PLAN.md) - Overall architecture and phase context
- [ ] [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - E2E testing patterns and best practices

**Standards & Guidelines:**
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices

---

## 📂 File Structure Overview

> Files to be modified during this phase

```
apps/teensyrom-ui-e2e/
├── src/
│   ├── support/interceptors/
│   │   ├── findDevices.interceptors.ts           📝 Modified - Add waitForFindDevicesToStart()
│   │   ├── connectDevice.interceptors.ts         📝 Modified - Add waitForConnectDeviceToStart()
│   │   └── disconnectDevice.interceptors.ts      📝 Modified - Add waitForDisconnectDeviceToStart()
│   └── e2e/devices/
│       ├── test-helpers.ts                       📝 Modified - Remove migrated wait functions
│       ├── device-refresh-connection.cy.ts       📝 Modified - Update imports and function names
│       ├── device-connection-multi.cy.ts         📝 Modified - Update imports and function names
│       ├── device-connection.cy.ts               📝 Modified - Update imports and function names
│       ├── device-indexing.cy.ts                 📝 Modified - Update imports and function names
│       ├── device-toolbar-disabled.cy.ts         📝 Modified - Update imports and function names
│       ├── device-discovery.cy.ts                📝 Modified - Update imports and function names
│       └── device-refresh-error.cy.ts            📝 Modified - Update imports and function names
```

---

## 🎯 Implementation Context

### Current State Analysis

**Existing Wait Functions in Interceptors:**
- `findDevices.interceptors.ts` has `waitForFindDevices()` (line 67-69)
- `connectDevice.interceptors.ts` has `waitForConnectDevice()` (line 93-95)
- `disconnectDevice.interceptors.ts` has `waitForDisconnectDevice()` (line 68-70)

**Wait Functions in test-helpers.ts to Migrate:**
- `waitForDeviceDiscovery()` (lines 48-50) → Maps to `waitForFindDevices()`
- `waitForConnection()` (lines 200-202) → Maps to `waitForConnectDevice()`
- `waitForDisconnection()` (lines 204-206) → Maps to `waitForDisconnectDevice()`
- `waitForConnectionToStart()` (lines 212-214) → NEW function needed
- `waitForDisconnectionToStart()` (lines 219-221) → NEW function needed
- `waitForFindDevicesToStart()` (lines 227-229) → NEW function needed

### Naming Convention Decision

**Unified Standard:** `waitFor<EndpointName>` pattern
- ✅ `waitForFindDevices()` - Standard wait for findDevices endpoint
- ✅ `waitForConnectDevice()` - Standard wait for connectDevice endpoint
- ✅ `waitForDisconnectDevice()` - Standard wait for disconnectDevice endpoint
- ✅ `waitForFindDevicesToStart()` - Race condition testing variant (2s timeout)
- ✅ `waitForConnectDeviceToStart()` - Race condition testing variant (2s timeout)
- ✅ `waitForDisconnectDeviceToStart()` - Race condition testing variant (2s timeout)

**Rationale:** Technical naming aligns with endpoint/action names, maintains consistency with existing interceptor patterns, and clearly identifies which API endpoint each wait function targets.

### Test File Update Scope

**Total Impact:** 202 function call updates across 7 test files

| Test File | Usage Count | Primary Wait Functions Used |
|-----------|-------------|---------------------------|
| device-refresh-connection.cy.ts | 62 | All wait functions |
| device-connection-multi.cy.ts | 51 | Device discovery, connection, disconnection |
| device-connection.cy.ts | 32 | Device discovery, connection, disconnection |
| device-indexing.cy.ts | 21 | Device discovery only |
| device-toolbar-disabled.cy.ts | 18 | Device discovery only |
| device-discovery.cy.ts | 13 | Device discovery only |
| device-refresh-error.cy.ts | 5 | Device discovery only |

---

## 📋 Implementation Guidelines

> **IMPORTANT - Testing Policy:**
> - After each test file is updated, run E2E tests for ONLY that specific file
> - Do not proceed to the next test file until current file's tests pass
> - This ensures issues are caught and fixed immediately, not batched at the end

> **IMPORTANT - Agent Coordination:**
> - Use **clean-coder** agents sequentially (one test file at a time) to prevent file conflicts
> - Use **e2e-runner** agent after each test file update to validate changes
> - Use **comment-cleaner** agents in parallel only at the very end
> - Use **code-cleaner** agent for final TypeScript/ESLint validation

---

<details open>
<summary><h3>Task 1: Add Race Condition Wait Functions to Interceptors</h3></summary>

**Purpose**: Add missing `*ToStart()` wait function variants to interceptor files for race condition testing scenarios. These functions use shorter timeouts (2000ms) to create precise timing windows for testing concurrent operations.

**Related Documentation:**
- [Interceptor Primitives Plan](./INTERCEPTOR_PRIMITIVES_PLAN.md#primitive-function-categories) - Timing primitives category

**Implementation Subtasks:**

**findDevices.interceptors.ts:**
- [ ] Add `waitForFindDevicesToStart()` function after existing `waitForFindDevices()` function
- [ ] Set timeout parameter to 2000ms for race condition testing
- [ ] Add JSDoc comment explaining race condition testing purpose
- [ ] Export `waitForFindDevicesToStart` in file exports

**connectDevice.interceptors.ts:**
- [ ] Add `waitForConnectDeviceToStart()` function after existing `waitForConnectDevice()` function
- [ ] Set timeout parameter to 2000ms for race condition testing
- [ ] Add JSDoc comment explaining race condition testing purpose
- [ ] Export `waitForConnectDeviceToStart` in file exports

**disconnectDevice.interceptors.ts:**
- [ ] Add `waitForDisconnectDeviceToStart()` function after existing `waitForDisconnectDevice()` function
- [ ] Set timeout parameter to 2000ms for race condition testing
- [ ] Add JSDoc comment explaining race condition testing purpose
- [ ] Export `waitForDisconnectDeviceToStart` in file exports

**Testing Subtask:**
- [ ] **Write Tests**: Verify new functions are exported and callable (see Testing section below)

**Key Implementation Notes:**
- These functions are specifically for race condition testing, not general usage
- The 2000ms timeout is intentionally shorter to create precise timing windows
- Functions should follow the exact same pattern as existing wait functions, only changing timeout
- Add clear JSDoc comments to distinguish these from standard wait functions

**Function Pattern to Follow:**
```typescript
/**
 * Wait for <endpoint> API call to start
 * Used to create timing windows for race condition testing
 * @param timeout - Optional timeout in milliseconds (default: 2000ms for race testing)
 */
export function waitFor<Endpoint>ToStart(timeout = 2000): void {
  cy.wait(`@${<ENDPOINT>_ALIAS}`, { timeout });
}
```

**Testing Focus for Task 1:**

**Behaviors to Test:**
- [ ] `waitForFindDevicesToStart()` is exported from findDevices.interceptors.ts
- [ ] `waitForConnectDeviceToStart()` is exported from connectDevice.interceptors.ts
- [ ] `waitForDisconnectDeviceToStart()` is exported from disconnectDevice.interceptors.ts
- [ ] Functions can be imported and called without errors
- [ ] Functions use correct alias constants

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for general testing approach
- Manual verification: Import functions in a test file and verify no import errors

</details>

---

<details open>
<summary><h3>Task 2: Update device-refresh-connection.cy.ts (62 usages)</h3></summary>

**Purpose**: Update the highest-usage test file first to establish the migration pattern. This file uses all wait function variants and serves as the template for subsequent migrations.

**Related Documentation:**
- [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md#test-file-structure) - Test file organization patterns

**Implementation Subtasks:**
- [ ] Remove wait function imports from `test-helpers.ts` import statement
- [ ] Add direct imports from `findDevices.interceptors.ts` for `waitForFindDevices` and `waitForFindDevicesToStart`
- [ ] Add direct imports from `connectDevice.interceptors.ts` for `waitForConnectDevice` and `waitForConnectDeviceToStart`
- [ ] Add direct imports from `disconnectDevice.interceptors.ts` for `waitForDisconnectDevice` and `waitForDisconnectDeviceToStart`
- [ ] Replace all `waitForDeviceDiscovery()` calls with `waitForFindDevices()`
- [ ] Replace all `waitForConnection()` calls with `waitForConnectDevice()`
- [ ] Replace all `waitForDisconnection()` calls with `waitForDisconnectDevice()`
- [ ] Replace all `waitForConnectionToStart()` calls with `waitForConnectDeviceToStart()`
- [ ] Replace all `waitForDisconnectionToStart()` calls with `waitForDisconnectDeviceToStart()`
- [ ] Replace all `waitForFindDevicesToStart()` calls with `waitForFindDevicesToStart()` (no change in name)

**Testing Subtask:**
- [ ] **Run E2E Tests**: Execute only device-refresh-connection.cy.ts and verify all tests pass

**Key Implementation Notes:**
- This file has the most complex usage patterns - establish clean import organization
- Group imports by interceptor file for clarity
- Verify all timing-sensitive race condition tests still work correctly
- Document any unexpected behaviors or edge cases discovered

**Agent Usage for Task 2:**
- Use **@clean-coder** agent to perform the migration
  - File path: `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-connection.cy.ts`
  - Agent must report: Specific changes made, import statements updated, function name replacements, any issues encountered
- Use **@e2e-runner** agent to validate changes
  - Test file: `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-connection.cy.ts`
  - Agent must report: Test pass/fail status, number of tests run, any failures with details, test execution time
- Review agent reports and fix any failures before proceeding to Task 3

**Testing Focus for Task 2:**

**Behaviors to Test:**
- [ ] All tests in device-refresh-connection.cy.ts pass without errors
- [ ] No import errors or missing function errors
- [ ] Race condition tests still function correctly with new *ToStart() functions
- [ ] Test execution time remains consistent with baseline

**Testing Reference:**
- Use `/run-e2e-test apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-connection.cy.ts`

</details>

---

<details open>
<summary><h3>Task 3: Update device-connection-multi.cy.ts (51 usages)</h3></summary>

**Purpose**: Update the second-highest usage file, applying lessons learned from Task 2.

**Implementation Subtasks:**
- [ ] Remove wait function imports from `test-helpers.ts` import statement
- [ ] Add direct imports from interceptor files (findDevices, connectDevice, disconnectDevice)
- [ ] Replace `waitForDeviceDiscovery()` with `waitForFindDevices()`
- [ ] Replace `waitForConnection()` with `waitForConnectDevice()`
- [ ] Replace `waitForDisconnection()` with `waitForDisconnectDevice()`

**Testing Subtask:**
- [ ] **Run E2E Tests**: Execute only device-connection-multi.cy.ts and verify all tests pass

**Agent Usage for Task 3:**
- Use **@clean-coder** agent to perform the migration
  - File path: `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection-multi.cy.ts`
  - Agent must report: Changes made, import updates, function replacements, any issues
- Use **@e2e-runner** agent to validate changes
  - Test file: `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection-multi.cy.ts`
  - Agent must report: Test results, pass/fail status, any failures with details
- Review reports and fix any failures before proceeding to Task 4

**Testing Focus for Task 3:**

**Behaviors to Test:**
- [ ] All tests in device-connection-multi.cy.ts pass without errors
- [ ] Multi-device connection scenarios work correctly
- [ ] No regression in test reliability

**Testing Reference:**
- Use `/run-e2e-test apps/teensyrom-ui-e2e/src/e2e/devices/device-connection-multi.cy.ts`

</details>

---

<details open>
<summary><h3>Task 4: Update device-connection.cy.ts (32 usages)</h3></summary>

**Purpose**: Update device connection test file with new wait function imports and names.

**Implementation Subtasks:**
- [ ] Remove wait function imports from `test-helpers.ts` import statement
- [ ] Add direct imports from interceptor files (findDevices, connectDevice, disconnectDevice)
- [ ] Replace `waitForDeviceDiscovery()` with `waitForFindDevices()`
- [ ] Replace `waitForConnection()` with `waitForConnectDevice()`
- [ ] Replace `waitForDisconnection()` with `waitForDisconnectDevice()`

**Testing Subtask:**
- [ ] **Run E2E Tests**: Execute only device-connection.cy.ts and verify all tests pass

**Agent Usage for Task 4:**
- Use **@clean-coder** agent to perform the migration
  - File path: `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts`
  - Agent must report: Changes made, import updates, function replacements, any issues
- Use **@e2e-runner** agent to validate changes
  - Test file: `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts`
  - Agent must report: Test results, pass/fail status, any failures with details

**Testing Focus for Task 4:**

**Behaviors to Test:**
- [ ] All tests in device-connection.cy.ts pass without errors
- [ ] Connection state transitions work correctly

**Testing Reference:**
- Use `/run-e2e-test apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts`

</details>

---

<details open>
<summary><h3>Task 5: Update device-indexing.cy.ts (21 usages)</h3></summary>

**Purpose**: Update device indexing test file - only uses `waitForDeviceDiscovery()` function.

**Implementation Subtasks:**
- [ ] Remove `waitForDeviceDiscovery` import from `test-helpers.ts` import statement
- [ ] Add direct import from `findDevices.interceptors.ts` for `waitForFindDevices`
- [ ] Replace all `waitForDeviceDiscovery()` calls with `waitForFindDevices()`

**Testing Subtask:**
- [ ] **Run E2E Tests**: Execute only device-indexing.cy.ts and verify all tests pass

**Agent Usage for Task 5:**
- Use **@clean-coder** agent to perform the migration
  - File path: `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts`
  - Agent must report: Changes made, import updates, function replacements, any issues
- Use **@e2e-runner** agent to validate changes
  - Test file: `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts`
  - Agent must report: Test results, pass/fail status, any failures with details

**Testing Focus for Task 5:**

**Behaviors to Test:**
- [ ] All tests in device-indexing.cy.ts pass without errors
- [ ] Device indexing workflows complete successfully

**Testing Reference:**
- Use `/run-e2e-test apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts`

</details>

---

<details open>
<summary><h3>Task 6: Update device-toolbar-disabled.cy.ts (18 usages)</h3></summary>

**Purpose**: Update device toolbar disabled state test file - only uses `waitForDeviceDiscovery()` function.

**Implementation Subtasks:**
- [ ] Remove `waitForDeviceDiscovery` import from `test-helpers.ts` import statement
- [ ] Add direct import from `findDevices.interceptors.ts` for `waitForFindDevices`
- [ ] Replace all `waitForDeviceDiscovery()` calls with `waitForFindDevices()`

**Testing Subtask:**
- [ ] **Run E2E Tests**: Execute only device-toolbar-disabled.cy.ts and verify all tests pass

**Agent Usage for Task 6:**
- Use **@clean-coder** agent to perform the migration
  - File path: `apps/teensyrom-ui-e2e/src/e2e/devices/device-toolbar-disabled.cy.ts`
  - Agent must report: Changes made, import updates, function replacements, any issues
- Use **@e2e-runner** agent to validate changes
  - Test file: `apps/teensyrom-ui-e2e/src/e2e/devices/device-toolbar-disabled.cy.ts`
  - Agent must report: Test results, pass/fail status, any failures with details

**Testing Focus for Task 6:**

**Behaviors to Test:**
- [ ] All tests in device-toolbar-disabled.cy.ts pass without errors
- [ ] Toolbar disabled state logic works correctly

**Testing Reference:**
- Use `/run-e2e-test apps/teensyrom-ui-e2e/src/e2e/devices/device-toolbar-disabled.cy.ts`

</details>

---

<details open>
<summary><h3>Task 7: Update device-discovery.cy.ts (13 usages)</h3></summary>

**Purpose**: Update device discovery test file - only uses `waitForDeviceDiscovery()` function.

**Implementation Subtasks:**
- [ ] Remove `waitForDeviceDiscovery` import from `test-helpers.ts` import statement
- [ ] Add direct import from `findDevices.interceptors.ts` for `waitForFindDevices`
- [ ] Replace all `waitForDeviceDiscovery()` calls with `waitForFindDevices()`

**Testing Subtask:**
- [ ] **Run E2E Tests**: Execute only device-discovery.cy.ts and verify all tests pass

**Agent Usage for Task 7:**
- Use **@clean-coder** agent to perform the migration
  - File path: `apps/teensyrom-ui-e2e/src/e2e/devices/device-discovery.cy.ts`
  - Agent must report: Changes made, import updates, function replacements, any issues
- Use **@e2e-runner** agent to validate changes
  - Test file: `apps/teensyrom-ui-e2e/src/e2e/devices/device-discovery.cy.ts`
  - Agent must report: Test results, pass/fail status, any failures with details

**Testing Focus for Task 7:**

**Behaviors to Test:**
- [ ] All tests in device-discovery.cy.ts pass without errors
- [ ] Device discovery scenarios work correctly

**Testing Reference:**
- Use `/run-e2e-test apps/teensyrom-ui-e2e/src/e2e/devices/device-discovery.cy.ts`

</details>

---

<details open>
<summary><h3>Task 8: Update device-refresh-error.cy.ts (5 usages)</h3></summary>

**Purpose**: Update device refresh error test file - only uses `waitForDeviceDiscovery()` function. This is the lowest usage file.

**Implementation Subtasks:**
- [ ] Remove `waitForDeviceDiscovery` import from `test-helpers.ts` import statement
- [ ] Add direct import from `findDevices.interceptors.ts` for `waitForFindDevices`
- [ ] Replace all `waitForDeviceDiscovery()` calls with `waitForFindDevices()`

**Testing Subtask:**
- [ ] **Run E2E Tests**: Execute only device-refresh-error.cy.ts and verify all tests pass

**Agent Usage for Task 8:**
- Use **@clean-coder** agent to perform the migration
  - File path: `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-error.cy.ts`
  - Agent must report: Changes made, import updates, function replacements, any issues
- Use **@e2e-runner** agent to validate changes
  - Test file: `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-error.cy.ts`
  - Agent must report: Test results, pass/fail status, any failures with details

**Testing Focus for Task 8:**

**Behaviors to Test:**
- [ ] All tests in device-refresh-error.cy.ts pass without errors
- [ ] Error handling scenarios work correctly

**Testing Reference:**
- Use `/run-e2e-test apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-error.cy.ts`

</details>

---

<details open>
<summary><h3>Task 9: Clean Up test-helpers.ts</h3></summary>

**Purpose**: Remove migrated wait functions from test-helpers.ts, keeping only DOM/UI helper functions that remain relevant.

**Related Documentation:**
- Current test-helpers.ts location: `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts`

**Implementation Subtasks:**
- [ ] Remove `waitForDeviceDiscovery()` function (lines 48-50)
- [ ] Remove `waitForConnection()` function (lines 200-202)
- [ ] Remove `waitForDisconnection()` function (lines 204-206)
- [ ] Remove `waitForConnectionToStart()` function (lines 212-214)
- [ ] Remove `waitForDisconnectionToStart()` function (lines 219-221)
- [ ] Remove `waitForFindDevicesToStart()` function (lines 227-229)
- [ ] Remove now-unused imports: `FIND_DEVICES_ALIAS`, `CONNECT_DEVICE_ALIAS`, `DISCONNECT_DEVICE_ALIAS`
- [ ] Verify remaining functions are all DOM/UI-related helpers
- [ ] Update file header comment to clarify scope (DOM/UI helpers only)

**Testing Subtask:**
- [ ] **Verify Compilation**: Ensure no TypeScript errors after removal

**Key Implementation Notes:**
- Keep ALL other helper functions (navigateToDeviceView, getDeviceCard, verifyDeviceCard, etc.)
- These DOM/UI helpers are still centralized and used across multiple test files
- Ensure no dangling imports or references remain
- The exports list at the top should no longer include the removed wait functions

**Functions to KEEP (DOM/UI helpers):**
- `navigateToDeviceView()`
- `getDeviceCard()`
- `expectDeviceCardVisible()`
- `verifyDeviceCard()`
- `verifyDeviceCount()`
- `verifyEmptyState()`
- `verifyLoadingState()`
- `verifyDeviceState()`
- `verifyStorageStatus()`
- `verifyFullDeviceInfo()`
- `clickConnectDevice()`
- `clickDisconnectDevice()`
- `verifyErrorMessage()`
- `clickRefreshDevices()`
- `clickPowerButton()`
- `verifyConnected()`
- `verifyDisconnected()`

**Testing Focus for Task 9:**

**Behaviors to Test:**
- [ ] File compiles without TypeScript errors
- [ ] All remaining helper functions are exported correctly
- [ ] No unused imports remain in the file

**Testing Reference:**
- Run `npx nx typecheck teensyrom-ui-e2e` to verify no errors

</details>

---

<details open>
<summary><h3>Task 10: Update E2E Testing Documentation</h3></summary>

**Purpose**: Document the migration changes, updated naming conventions, and new patterns in the interceptor documentation.

**Related Documentation:**
- [E2E Interceptors Documentation](../../../apps/teensyrom-ui-e2e/src/support/interceptors/E2E_INTERCEPTORS.md) - Documentation to update

**Implementation Subtasks:**
- [ ] Add new "Wait Helper Functions" section to E2E_INTERCEPTORS.md
- [ ] Document the `waitFor<EndpointName>` standard pattern for each interceptor
- [ ] Add guidance on when to use standard wait vs. `*ToStart()` variants for race condition testing
- [ ] Update import examples to show wait function imports alongside interceptor imports
- [ ] Document that wait helpers are co-located with their corresponding interceptors
- [ ] Add examples showing combined usage of interceptors and wait functions

**Testing Subtask:**
- [ ] **Review Documentation**: Ensure documentation is clear and accurate

**Key Implementation Notes:**
- Emphasize the unified naming convention across all wait functions
- Explain the race condition testing use case for `*ToStart()` variants
- Provide before/after examples of import statements
- Link to interceptor files as the source of truth for API wait functions

**Testing Focus for Task 10:**

**Behaviors to Test:**
- [ ] Documentation accurately reflects the new structure
- [ ] Examples are correct and follow established patterns
- [ ] Links to relevant files and sections are valid

**Testing Reference:**
- Manual review of documentation updates

</details>

---

<details open>
<summary><h3>Task 11: TypeScript and ESLint Cleanup</h3></summary>

**Purpose**: Run comprehensive linting and type checking to ensure all changes meet project quality standards.

**Implementation Subtasks:**
- [ ] Run TypeScript type checking across entire E2E test suite
- [ ] Run ESLint on all modified files
- [ ] Fix any TypeScript errors discovered
- [ ] Fix any ESLint violations discovered
- [ ] Verify no unused imports remain
- [ ] Verify all import paths are correct

**Testing Subtask:**
- [ ] **Run Quality Checks**: Execute linting and type checking commands

**Agent Usage for Task 11:**
- Use **@code-cleaner** agent to fix TypeScript and ESLint issues
  - Scope: `apps/teensyrom-ui-e2e` directory
  - Agent must report: Number of errors found, number fixed automatically, remaining manual fixes needed, list of affected files

**Key Implementation Notes:**
- This is a comprehensive cleanup pass after all migrations
- Focus on automated fixes where possible
- Document any manual fixes required
- Ensure consistent code formatting throughout

**Testing Focus for Task 11:**

**Behaviors to Test:**
- [ ] TypeScript compilation succeeds with zero errors
- [ ] ESLint passes with zero violations
- [ ] All imports resolve correctly
- [ ] No unused variables or imports remain

**Testing Reference:**
- Run `npx nx lint teensyrom-ui-e2e`
- Run `npx nx typecheck teensyrom-ui-e2e`

</details>

---

<details open>
<summary><h3>Task 12: Comment Cleanup (Parallel Execution)</h3></summary>

**Purpose**: Clean up comments across all modified files, removing outdated or redundant comments while preserving valuable documentation.

**Implementation Subtasks:**
- [ ] Clean comments in `findDevices.interceptors.ts`
- [ ] Clean comments in `connectDevice.interceptors.ts`
- [ ] Clean comments in `disconnectDevice.interceptors.ts`
- [ ] Clean comments in `test-helpers.ts`
- [ ] Clean comments in all 7 test files

**Testing Subtask:**
- [ ] **Review Comment Quality**: Verify comments add value and are not redundant

**Agent Usage for Task 12:**
- Use multiple **@comment-cleaner** agents in PARALLEL for all modified files
  - This is the ONLY task where parallel execution is allowed
  - Launch separate agents for different file groups:
    - Agent 1: 3 interceptor files (findDevices, connectDevice, disconnectDevice)
    - Agent 2: test-helpers.ts
    - Agent 3-9: Individual test files (one agent per test file)
  - Each agent must report: Files processed, comments removed, comments preserved, any issues encountered
- Collect all agent reports before proceeding to final validation

**Key Implementation Notes:**
- This is the ONLY task where parallel agent execution is recommended
- Comment cleaning is non-breaking and safe to parallelize
- Focus on removing obvious redundant comments
- Preserve JSDoc comments for public functions
- Keep comments that explain "why" not "what"

**Testing Focus for Task 12:**

**Behaviors to Test:**
- [ ] Comments that remain add genuine value
- [ ] No redundant comments describing obvious code
- [ ] JSDoc comments preserved for public APIs
- [ ] Code still compiles and tests still pass

**Testing Reference:**
- Run full E2E test suite to verify no functionality broken: `npx nx e2e teensyrom-ui-e2e`

</details>

---

## 🗂️ Files Modified or Created

**Modified Files:**

**Interceptor Files:**
- `apps/teensyrom-ui-e2e/src/support/interceptors/findDevices.interceptors.ts`
- `apps/teensyrom-ui-e2e/src/support/interceptors/connectDevice.interceptors.ts`
- `apps/teensyrom-ui-e2e/src/support/interceptors/disconnectDevice.interceptors.ts`

**Test Helper File:**
- `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts`

**Test Files:**
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-connection.cy.ts`
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection-multi.cy.ts`
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts`
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts`
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-toolbar-disabled.cy.ts`
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-discovery.cy.ts`
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-error.cy.ts`

**Documentation:**
- `apps/teensyrom-ui-e2e/E2E_TESTS.md`

**Total Files Modified:** 15 files

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are executed **after each test file is updated**, not batched at the end.

> **Core Testing Philosophy:**
> - **Test as you go** - E2E tests run after each test file migration
> - **Isolate failures** - Fix issues immediately before moving to next file
> - **Validate behavior** - Ensure no regressions in test reliability or timing
> - **Sequential execution** - Prevents cascading failures and identifies root causes quickly

### Testing Strategy by Task

**Task 1 (Interceptor Functions):**
- Manual import verification
- No agent execution needed for this foundational task

**Tasks 2-8 (Test File Updates):**
- **@clean-coder** agent migrates each test file
- **@e2e-runner** agent validates that specific file immediately
- Review agent reports for both execution steps
- Fix any failures before proceeding to next task
- Document any unexpected behaviors in agent reports

**Task 9 (test-helpers cleanup):**
- Manual verification via TypeScript compilation
- No agent execution needed

**Task 10 (Documentation):**
- Manual documentation update
- No agent execution needed

**Task 11 (TypeScript/ESLint):**
- **@code-cleaner** agent handles all linting and type checking
- Review agent report for errors found and fixed

**Task 12 (Comment Cleanup):**
- Multiple **@comment-cleaner** agents run in parallel
- Collect and review all agent reports
- Verify functionality maintained through full test suite if concerns arise

### Agent Execution Workflow

**Sequential Tasks (Tasks 1-11):**
1. **@clean-coder** performs migration for specific test file
2. **@e2e-runner** validates the changes on that specific file
3. Review both agent reports:
   - clean-coder report: What changed, any issues encountered
   - e2e-runner report: Test results, pass/fail status, error details
4. Fix any reported issues before proceeding to next task
5. Move to next sequential task

**Parallel Task (Task 12 only):**
1. Launch multiple **@comment-cleaner** agents simultaneously
2. Collect all agent reports
3. Review aggregated results
4. Validate with full test suite if needed

**Agent Report Requirements:**
- **@clean-coder**: Changes made, files modified, import updates, function replacements, issues
- **@e2e-runner**: Test results, pass/fail counts, execution time, detailed failure information
- **@code-cleaner**: Errors found/fixed, remaining issues, affected files
- **@comment-cleaner**: Files processed, comments removed/preserved, issues

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All implementation tasks (1-12) completed and checked off
- [ ] All subtasks within each task completed
- [ ] All 3 interceptor files have new `*ToStart()` functions added
- [ ] All 7 test files updated with new imports and function names
- [ ] test-helpers.ts cleaned up with only DOM/UI helpers remaining
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)

**Testing Requirements:**
- [ ] Each test file's E2E tests passed after its migration (Tasks 2-8)
- [ ] Full E2E test suite passes after all changes complete
- [ ] No test reliability regressions or timing issues introduced
- [ ] All race condition tests still function correctly with `*ToStart()` variants
- [ ] TypeScript compilation succeeds with zero errors
- [ ] ESLint passes with zero violations

**Quality Checks:**
- [ ] No TypeScript errors or warnings across E2E test suite
- [ ] Linting passes with no errors (`npx nx lint teensyrom-ui-e2e`)
- [ ] All import paths resolve correctly
- [ ] No unused imports or variables remain
- [ ] Code formatting is consistent across all modified files

**Documentation:**
- [ ] E2E_TESTS.md updated with new naming conventions and patterns
- [ ] Import examples reflect new direct interceptor imports
- [ ] Wait function variants documented clearly
- [ ] Separation of concerns (API vs UI helpers) explained

**Migration Completeness:**
- [ ] Zero references to old wait function names remain in test files
- [ ] All wait functions consolidated in appropriate interceptor files
- [ ] test-helpers.ts no longer contains API wait functions
- [ ] Naming convention unified to `waitFor<EndpointName>` pattern
- [ ] Total migration: 202 function call updates completed successfully

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Full E2E test suite green
- [ ] Documentation complete and accurate
- [ ] Code review approved (if applicable)
- [ ] Ready to proceed with Phase 3 remaining tasks

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

**Decision 1: Unified Technical Naming Convention**
- **Rationale**: Using `waitFor<EndpointName>` pattern maintains consistency with interceptor file naming and makes it immediately clear which API endpoint is being waited for
- **Alternative Considered**: User-friendly names like `waitForDeviceDiscovery` are more intuitive but create inconsistency with interceptor patterns
- **Trade-offs**: Slight learning curve for test writers, but clearer technical mapping and better long-term maintainability

**Decision 2: Direct Imports from Interceptors**
- **Rationale**: Explicit imports make dependencies clear and keep API concerns within interceptor layer
- **Alternative Considered**: Barrel export file (`interceptors/index.ts`) would simplify imports but obscures actual dependencies
- **Trade-offs**: More import statements per file, but clearer dependency graph and better tree-shaking

**Decision 3: Sequential Test File Migration**
- **Rationale**: Testing after each file update isolates failures and ensures immediate issue resolution
- **Alternative Considered**: Batch all updates then test once - faster but harder to debug failures
- **Trade-offs**: Longer total migration time, but much safer and more predictable

### Implementation Constraints

**Constraint 1: Race Condition Test Timing**
- The `*ToStart()` variants must maintain the 2000ms timeout to preserve timing windows
- Changes to these timeouts could break carefully-tuned race condition tests
- Document any timing adjustments needed during migration

**Constraint 2: Import Path Complexity**
- Test files are nested in `apps/teensyrom-ui-e2e/src/e2e/devices/`
- Interceptors are in `apps/teensyrom-ui-e2e/src/support/interceptors/`
- Relative paths will be `../../support/interceptors/<file>`

**Constraint 3: Agent Coordination**
- Only Task 12 (comment cleanup) can use parallel agent execution
- All other tasks must be sequential to prevent file modification conflicts
- E2E testing must happen after each file update, not batched

### Future Enhancements

**Enhancement 1: Barrel Export Pattern**
- Consider adding `interceptors/index.ts` in a future phase to simplify imports
- Could re-export all wait functions for cleaner test file import statements
- Low priority - direct imports provide better clarity for now

**Enhancement 2: Wait Function Timeout Configurability**
- Consider making timeout configurable via environment variables for CI/CD environments
- Could help with slower CI environments or debugging scenarios
- Would require updates to interceptor primitive patterns

**Enhancement 3: Automated Migration Script**
- Could create codemod script for similar migrations in the future
- Would handle import updates and function name replacements automatically
- Useful if pattern extends to other test domains beyond devices

### External References

- [Cypress Wait Best Practices](https://docs.cypress.io/api/commands/wait) - Cypress wait command documentation
- [Interceptor Primitives Architecture](./INTERCEPTOR_PRIMITIVES_PLAN.md) - Parent plan for overall architecture
- [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Project E2E testing guidelines

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery 1**: [Document any unexpected test timing issues discovered]
- **Discovery 2**: [Note any interceptor patterns that could be improved]
- **Discovery 3**: [Record any test reliability improvements or regressions]

</details>

---

## 💡 Agent Implementation Guide

> **Instructions for AI agents executing this phase**

### Before Starting Implementation

**Pre-Implementation Checklist:**

1. **Verify Context**: Ensure you have read and understood all Required Reading documents
2. **Check Dependencies**: Confirm all interceptor files exist and have expected structure
3. **Baseline Tests**: Run full E2E suite to establish passing baseline before changes
4. **Environment Setup**: Ensure Cypress and test infrastructure are properly configured

### Task Execution Order

**CRITICAL: Follow this exact sequence:**

1. **Task 1**: Add `*ToStart()` functions to interceptors (foundation work)
2. **Tasks 2-8**: Update test files ONE AT A TIME in specified order
   - For each file:
     - Use **@clean-coder** agent for migration
     - Use **@e2e-runner** agent for validation
     - Fix failures before moving to next file
3. **Task 9**: Clean up test-helpers.ts (after all test files updated)
4. **Task 10**: Update documentation
5. **Task 11**: Run **@code-cleaner** for TypeScript/ESLint cleanup
6. **Task 12**: Run **@comment-cleaner** agents in parallel (only parallelization allowed)

### Agent Usage Patterns

**Sequential Work (Tasks 1-11):**
```
Step 1: @clean-coder updates test file
Step 2: @e2e-runner validates that specific file
Step 3: Fix any failures
Step 4: Move to next test file
```

**Parallel Work (Task 12 Only):**
```
Launch multiple @comment-cleaner agents simultaneously:
- Agent 1: Clean interceptor files
- Agent 2: Clean test-helpers.ts
- Agent 3: Clean device-refresh-connection.cy.ts
- Agent 4: Clean device-connection-multi.cy.ts
- Agent 5: Clean remaining test files
```

### Error Handling Strategy

**If E2E Test Fails After Migration:**

1. Review import statements for correct paths and function names
2. Check for typos in function name replacements
3. Verify timeout values preserved correctly (especially for `*ToStart()` variants)
4. Compare test behavior with baseline before migration
5. Check Cypress console for specific error messages
6. Document the failure and resolution for future reference

**If TypeScript Compilation Fails:**

1. Verify all imports resolve to existing files
2. Check for typo in function names
3. Ensure exports match imports
4. Verify no circular dependencies introduced

### Progress Tracking

**Update Checkboxes Immediately:**
- ✅ Mark subtasks as soon as completed
- ✅ Update task-level testing checkboxes after E2E validation
- ✅ Document any issues in the Discoveries section
- ✅ Keep success criteria current

### Communication Guidelines

**Status Updates Should Include:**
- Current task number and name
- Completion status of subtasks
- E2E test results (pass/fail, any issues)
- Any unexpected behaviors or discoveries
- Estimated time to complete remaining tasks

### Remember

- **No parallelization except Task 12** - Prevents file modification conflicts
- **Test after each file** - Isolates issues immediately
- **Fix before proceeding** - Don't accumulate technical debt
- **Document discoveries** - Help future maintainers
- **Mark progress incrementally** - Keep checkboxes current

---

## 🎓 Migration Statistics

### Total Scope

- **Interceptor Files Modified**: 3
- **Test Files Modified**: 7
- **Function Call Updates**: 202
- **New Functions Added**: 3 (`*ToStart()` variants)
- **Functions Removed**: 6 (from test-helpers.ts)
- **Documentation Files Updated**: 1

### Impact Analysis

**High Impact Test Files** (require careful validation):
- device-refresh-connection.cy.ts - 62 updates, uses all wait variants
- device-connection-multi.cy.ts - 51 updates, complex multi-device scenarios

**Medium Impact Test Files**:
- device-connection.cy.ts - 32 updates
- device-indexing.cy.ts - 21 updates

**Low Impact Test Files** (simpler migrations):
- device-toolbar-disabled.cy.ts - 18 updates
- device-discovery.cy.ts - 13 updates
- device-refresh-error.cy.ts - 5 updates

### Estimated Effort

- **Task 1**: 30-45 minutes (add 3 functions to interceptors)
- **Tasks 2-8**: 3-4 hours total (migration + testing, ~30-40 min per file)
- **Task 9**: 15-20 minutes (cleanup test-helpers.ts)
- **Task 10**: 20-30 minutes (documentation updates)
- **Task 11**: 15-20 minutes (TypeScript/ESLint cleanup)
- **Task 12**: 30-40 minutes (comment cleanup, parallelized)

**Total Estimated Time**: 5-7 hours for complete phase

---

## 📊 Context Budget Tracking

**Current Context Status**: ~158,000 tokens remaining

**Recommended Checkpoints**:
- After Task 4 completes: Check context remaining
- After Task 8 completes: Check context remaining
- Before Task 12: Ensure sufficient context for parallel agents

**If Context Runs Low**:
- Consider using `/compact` command to compress conversation history
- Focus on completing current task before context exhausted
- Document progress clearly so work can resume if needed

---

*This phase plan follows the Interceptor Primitives Architecture established in INTERCEPTOR_PRIMITIVES_PLAN.md Phase 3. For questions or issues, refer to parent plan documentation or project testing standards.*
