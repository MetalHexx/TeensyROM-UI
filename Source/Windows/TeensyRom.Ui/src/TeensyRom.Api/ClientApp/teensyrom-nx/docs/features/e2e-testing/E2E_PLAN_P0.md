# Phase 0: Cypress Sanity Check - Implementation Plan

## 🎯 Objective

Validate that Cypress E2E testing infrastructure is correctly configured and can successfully test the Angular application before investing in complex fixture systems. Create the first device view test that verifies navigation to `/devices` route and rendering of the device view component.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**

- [ ] [E2E Testing Plan](./E2E_PLAN.md) - High-level E2E testing strategy and phase breakdown
- [ ] [app.routes.ts](../../apps/teensyrom-ui/src/app/app.routes.ts) - Application routing configuration

**Standards & Guidelines:**

- [ ] [Coding Standards](../../docs/CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../docs/TESTING_STANDARDS.md) - Testing approaches and best practices

**Existing Cypress Files to Review:**

- [ ] `apps/teensyrom-ui-e2e/cypress.config.ts` - Cypress configuration with baseUrl and server settings
- [ ] `apps/teensyrom-ui-e2e/src/e2e/app.cy.ts` - Existing example test showing Cypress patterns
- [ ] `apps/teensyrom-ui-e2e/src/support/app.po.ts` - Page object pattern example

---

## 📂 File Structure Overview

```
apps/teensyrom-ui-e2e/
├── cypress.config.ts                        (existing - no changes)
├── project.json                             (existing - no changes)
└── src/
    ├── e2e/
    │   ├── app.cy.ts                        (existing - no changes)
    │   └── devices/
    │       ├── device-view-navigation.cy.ts  ✨ New - First device view test
    │       └── DEVICE_TESTS.md               ✨ New - Test documentation
    ├── support/
    │   ├── commands.ts                      (existing - no changes)
    │   ├── e2e.ts                           (existing - no changes)
    │   └── app.po.ts                        (existing - no changes)
    └── fixtures/                            (existing - empty for now)
```

---

<details open>
<summary><h3>Task 1: Validate Existing Cypress Configuration</h3></summary>

**Purpose**: Verify that Cypress is properly installed, configured, and integrated with the Nx workspace before creating new tests. Confirm existing setup can run tests against the Angular application.

**Related Documentation:**

- [Cypress Config](../../apps/teensyrom-ui-e2e/cypress.config.ts) - Current Cypress configuration
- [Nx Project Config](../../apps/teensyrom-ui-e2e/project.json) - E2E project configuration
- [Existing Test](../../apps/teensyrom-ui-e2e/src/e2e/app.cy.ts) - Example test pattern

**Implementation Subtasks:**

- [ ] **Inspect cypress.config.ts**: Confirm baseUrl is set to `http://localhost:4200` and webServerCommand references `teensyrom-ui:serve`
- [ ] **Verify Nx integration**: Check that `project.json` has implicit dependency on `teensyrom-ui` application
- [ ] **Review existing test structure**: Examine `app.cy.ts` to understand existing test patterns, `beforeEach` hooks, and assertion syntax
- [ ] **Check support files**: Confirm `commands.ts`, `e2e.ts`, and `app.po.ts` are present and properly configured
- [ ] **Verify Cypress installation**: Confirm Cypress dependencies exist in root `package.json`

**Testing Subtask:**

- [ ] **Run Existing Test**: Execute `pnpm nx e2e teensyrom-ui-e2e` to verify existing `app.cy.ts` test runs (even if it fails due to outdated content)

**Key Implementation Notes:**

- Cypress config uses Nx E2E preset with automatic dev server management
- baseUrl `http://localhost:4200` matches Angular dev server default port
- Existing `app.cy.ts` may have outdated content (references `/Welcome/` greeting) - this is expected
- Support files provide custom commands and setup - no modifications needed for Phase 0
- Screenshot configuration should capture on failure by default

**Testing Focus for Task 1:**

**Behaviors to Verify:**

- [ ] **Cypress runs**: Command `pnpm nx e2e teensyrom-ui-e2e` executes without installation errors
- [ ] **Dev server starts**: Angular application serves on port 4200 during test execution
- [ ] **Cypress UI launches**: Cypress test runner or headless mode executes successfully
- [ ] **Test completes**: Existing test runs to completion (pass or fail doesn't matter yet)
- [ ] **Screenshots configured**: Confirm screenshots directory exists or will be created on failure

**Testing Reference:**

- Cypress documentation for Nx integration verification

</details>

---

<details open>
<summary><h3>Task 2: Create Device View Navigation Test</h3></summary>

**Purpose**: Implement the first device-focused E2E test that validates basic navigation to the device view route. This establishes the testing foundation for all future device management tests.

**Related Documentation:**

- [Device View Component](../../libs/features/devices/src/lib/device-view/device-view.component.ts) - Component structure and selector
- [Application Routes](../../apps/teensyrom-ui/src/app/app.routes.ts) - Routing configuration showing `/devices` path

**Implementation Subtasks:**

- [ ] **Create devices directory**: Add `apps/teensyrom-ui-e2e/src/e2e/devices/` folder for device-related tests
- [ ] **Create test file**: Add `device-view-navigation.cy.ts` in devices directory
- [ ] **Write test suite**: Create `describe` block titled "Device View - Navigation"
- [ ] **Add beforeEach hook**: Navigate to `/devices` route using `cy.visit('/devices')`
- [ ] **Add first test case**: Test "should navigate to device view"
- [ ] **Assert route navigation**: Verify URL contains `/devices` using `cy.url().should('include', '/devices')`
- [ ] **Assert device view renders**: Verify `.device-view` container exists using `cy.get('.device-view').should('exist')`

**Testing Subtask:**

- [ ] **Run new test**: Execute `pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/device-view-navigation.cy.ts"` to run specific test file

**Key Implementation Notes:**

- Device view component uses CSS class `.device-view` on root container - this is the primary assertion target
- Default route (`/`) redirects to `/devices` per `app.routes.ts` configuration
- No API mocking needed - test should handle "no devices found" empty state gracefully
- Keep assertions minimal - just confirm navigation succeeded and view container is present

**Testing Focus for Task 2:**

**Behaviors to Test:**

- [ ] **Navigation succeeds**: Browser navigates to `/devices` route without errors
- [ ] **URL updates correctly**: Address bar shows `/devices` path after navigation
- [ ] **Device view present**: `.device-view` root container exists in DOM

**Testing Reference:**

- See [Testing Standards](../../docs/TESTING_STANDARDS.md) for E2E behavioral testing approach
- Cypress best practices for `cy.visit()`, `cy.get()`, and URL assertions

</details>

---

<details open>
<summary><h3>Task 3: Create Test Documentation</h3></summary>

**Purpose**: Document the device view test structure, conventions, and patterns to guide future test development in subsequent phases. Establish naming conventions and organizational standards.

**Related Documentation:**

- [E2E Plan](./E2E_PLAN.md) - Overall E2E testing strategy

**Implementation Subtasks:**

- [ ] **Create DEVICE_TESTS.md**: Add documentation file in `apps/teensyrom-ui-e2e/src/e2e/devices/` directory
- [ ] **Document test purpose**: Explain that device tests cover device discovery, connection, and management workflows
- [ ] **Document file naming**: Establish `[feature]-[workflow].cy.ts` convention (e.g., `device-view-navigation.cy.ts`)
- [ ] **Document test structure**: Explain `describe` block naming pattern: "[Component/View] - [Workflow]"
- [ ] **Document navigation pattern**: Show `beforeEach` hook usage for common setup like `cy.visit()`
- [ ] **Document assertion approach**: Explain testing rendered elements and URLs
- [ ] **List test files**: Include index of current test files with brief descriptions

**Testing Subtask:**

- [ ] **Review documentation**: Verify DEVICE_TESTS.md is clear and helpful for developers creating new tests

**Key Implementation Notes:**

- Documentation should be concise and actionable - focus on conventions over explanations
- Include examples from `device-view-navigation.cy.ts` to illustrate patterns
- Reference E2E_PLAN.md for context on future phases
- Keep DEVICE_TESTS.md up-to-date as new test files are added in later phases

**Testing Focus for Task 3:**

**Documentation Quality Checks:**

- [ ] **DEVICE_TESTS.md exists**: File created in correct location
- [ ] **Conventions documented**: Naming, structure, and organization patterns explained
- [ ] **Examples included**: Concrete examples from actual test files referenced
- [ ] **Navigation clear**: Easy to find information about test organization
- [ ] **Actionable guidance**: Developers can create new tests following documented patterns

**Testing Reference:**

- Documentation should help developers understand the testing approach without reading source code

</details>

---

<details open>
<summary><h3>Task 4: Validate Test Execution and Screenshot Output</h3></summary>

**Purpose**: Confirm the new device view test runs successfully, produces consistent results, and generates screenshot artifacts on test failure. Validate Cypress integration end-to-end.

**Related Documentation:**

- [Cypress Config](../../apps/teensyrom-ui-e2e/cypress.config.ts) - Screenshot and video configuration

**Implementation Subtasks:**

- [ ] **Run full test suite**: Execute `pnpm nx e2e teensyrom-ui-e2e` to run all E2E tests including new device test
- [ ] **Run specific test**: Execute `pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/device-view-navigation.cy.ts"` to run only device test
- [ ] **Verify test passes**: Confirm all assertions in device view navigation test succeed (green checkmarks)
- [ ] **Check execution time**: Record test execution duration - should be under 10 seconds for navigation test
- [ ] **Verify screenshot directory**: Confirm `apps/teensyrom-ui-e2e/screenshots/` directory exists or will be created on failure
- [ ] **Test failure scenario**: Temporarily break test (change assertion) to confirm screenshot capture works
- [ ] **Restore test**: Fix temporary breakage and verify test passes again
- [ ] **Check for warnings**: Review Cypress output for deprecation warnings or configuration issues

**Testing Subtask:**

- [ ] **Document test results**: Record test execution time, screenshot behavior, and any warnings encountered

**Key Implementation Notes:**

- Cypress should automatically start dev server before running tests via Nx integration
- Screenshots only generated on test failure by default - this is expected behavior
- Test execution time should be fast since no API calls are mocked/delayed yet
- Warnings about missing videos or screenshots when test passes are normal
- Nx caching may speed up subsequent test runs - ignore cache for validation

**Testing Focus for Task 4:**

**Behaviors to Verify:**

- [ ] **Test passes consistently**: Test succeeds on multiple runs with same results
- [ ] **Execution time reasonable**: Test completes in under 10 seconds
- [ ] **Dev server starts automatically**: Angular app serves without manual intervention
- [ ] **Screenshot on failure works**: Breaking test produces screenshot artifact
- [ ] **Screenshot contains view content**: Screenshot shows device view container when captured
- [ ] **No blocking warnings**: No critical errors or warnings in Cypress output
- [ ] **Test is deterministic**: Running test multiple times produces same pass/fail result

**Testing Reference:**

- Cypress documentation on screenshot and artifact configuration
- Nx Cypress integration documentation for dev server behavior

</details>

---

## 🗂️ Files Modified or Created

**New Files:**

- `apps/teensyrom-ui-e2e/src/e2e/devices/device-view-navigation.cy.ts` - First device view navigation test
- `apps/teensyrom-ui-e2e/src/e2e/devices/DEVICE_TESTS.md` - Device test documentation and conventions

**Modified Files:**

- None (all existing files remain unchanged)

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

### Where Tests Are Written

**Tests are embedded in each task above** with:

- **Testing Subtask**: Checkbox in the task's subtask list
- **Testing Focus**: "Behaviors to Verify/Test" section listing observable outcomes
- **Testing Reference**: Links to relevant documentation

**Complete each task's testing subtask before moving to the next task.**

### Test Execution Commands

**Running Cypress Tests:**

```powershell
# Run all E2E tests (existing + new device test)
pnpm nx e2e teensyrom-ui-e2e

# Run specific test file only
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/device-view-navigation.cy.ts"

# Run in headed mode (open Cypress UI)
pnpm nx e2e teensyrom-ui-e2e --watch

# Run with specific browser
pnpm nx e2e teensyrom-ui-e2e --browser=chrome
```

**Expected Output:**

- Angular dev server starts on port 4200
- Cypress executes tests (headless by default)
- Test results display in terminal (pass/fail)
- Screenshots saved to `apps/teensyrom-ui-e2e/screenshots/` on failure

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**

- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Device view navigation test file created
- [ ] Test documentation (DEVICE_TESTS.md) created

**Testing Requirements:**

- [ ] All testing subtasks completed within each task
- [ ] Existing Cypress test runs (pass or fail doesn't matter)
- [ ] New device view navigation test runs successfully
- [ ] Test passes consistently (green) on multiple runs
- [ ] Screenshot capture verified working on failure scenario

**Quality Checks:**

- [ ] No Cypress configuration errors
- [ ] Dev server starts automatically during test execution
- [ ] No blocking warnings in Cypress output
- [ ] Test execution completes in under 10 seconds

**Documentation:**

- [ ] DEVICE_TESTS.md created in devices test directory
- [ ] Test conventions and patterns documented
- [ ] Examples included from actual test file
- [ ] Documentation is clear and actionable

**Ready for Next Phase:**

- [ ] All success criteria met
- [ ] Cypress E2E infrastructure validated and working
- [ ] First device test establishes pattern for future tests
- [ ] Ready to proceed to Phase 1 (Faker Setup and DTO Generators)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **No API Mocking Yet**: Phase 0 tests against real application without fixtures - this validates baseline functionality before adding mock complexity
- **Device View Focus**: Starting with device view navigation establishes foundation for more complex device management tests in later phases
- **Existing Test Unchanged**: Leave `app.cy.ts` as-is even if outdated - demonstrates baseline Cypress functionality
- **Screenshot Only on Failure**: Default Cypress behavior captures screenshots only when tests fail to reduce artifact storage
- **Minimal Assertions**: Keep test focused on navigation success - no need to validate complete page render yet

### Implementation Constraints

- **No Backend Required**: Test runs against frontend only - device view will show "no devices found" empty state without backend
- **Dev Server Dependency**: Cypress tests require Angular dev server running - Nx handles this automatically via config
- **Port Conflict Risk**: If port 4200 is already in use, Cypress will fail - ensure port is available before running tests

### Future Enhancements

- **Phase 1 Preparation**: Once Phase 0 passes, proceed to Faker setup and DTO generator creation
- **API Interceptors**: Future phases will add `cy.intercept()` calls to mock backend responses
- **Page Objects**: Consider adding page object pattern if tests become more complex
- **Custom Commands**: May add device-specific Cypress commands in `support/commands.ts` later

### External References

- [Cypress Documentation](https://docs.cypress.io/) - Official Cypress testing guide
- [Nx Cypress Plugin](https://nx.dev/packages/cypress) - Nx integration documentation
- [E2E Plan](./E2E_PLAN.md) - Complete E2E testing strategy

### Discoveries During Implementation

> **Phase 0 Status**: ✅ COMPLETE (October 19, 2025)

- **Discovery**: Cypress 14.2.1 cannot load TypeScript config files in ESM mode - upgraded to Cypress 15.5.0 which uses `tsx` internally for native TypeScript support
- **Discovery**: Root package.json has `"type": "module"` causing all `.js` files to be treated as ESM - e2e tsconfig needs `"module": "commonjs"` to work with Nx Cypress preset (which is CommonJS)
- **Discovery**: Device view renders correctly with "No devices found" message when no backend is available - perfect for Phase 0 testing
- **Discovery**: Test execution is fast (<1 second for navigation test) and deterministic
- **Discovery**: Screenshot capture works perfectly - only generates on failure as expected

**See**: `docs/features/e2e-testing/E2E_PHASE0_SUMMARY.md` for complete implementation summary.

</details>

---

## 💡 Implementation Tips

### Before Starting

1. **Verify Nx and Cypress versions**: Check `package.json` to confirm compatible versions
2. **Close other dev servers**: Ensure port 4200 is available before running tests
3. **Read existing test**: Review `app.cy.ts` to understand basic Cypress patterns used in this project

### During Implementation

1. **Run existing test first (Task 1)**: Validate Cypress works before creating new tests
2. **Test incrementally (Task 2)**: Add one assertion at a time and verify it passes
3. **Use Cypress UI**: Run with `--watch` flag to see tests execute in browser for debugging
4. **Check application manually**: Navigate to `http://localhost:4200/devices` in browser to see what test will encounter

### Common Issues and Solutions

**Issue**: Port 4200 already in use  
**Solution**: Stop other dev servers or change port in `cypress.config.ts` and Angular config

**Issue**: Test fails with "element not found"  
**Solution**: Add `cy.wait()` or use Cypress retry-ability - component may need time to render

**Issue**: Dev server doesn't start  
**Solution**: Verify Angular app builds successfully with `pnpm nx build teensyrom-ui`

**Issue**: Screenshots not captured  
**Solution**: Screenshots only generate on failure - temporarily break test to verify

---
