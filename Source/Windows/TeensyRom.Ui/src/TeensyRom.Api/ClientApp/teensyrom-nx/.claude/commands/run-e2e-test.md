---
description: Run Cypress E2E tests with detailed failure reporting
argument-hint: [test-file-path]
allowed-tools: Bash(pnpm:*), Bash(nx:*)
---

# Run E2E Test Command

Execute Cypress E2E tests for the TeensyROM UI application with comprehensive failure analysis and debugging information.

## Context

- Test file (optional): $ARGUMENTS
- Project: teensyrom-ui-e2e
- Test framework: Cypress with fixture-driven interceptor approach
- Available test specs: !`ls apps/teensyrom-ui-e2e/src/e2e/**/*.cy.ts | head -20`
- **Architecture Reference**: @apps/teensyrom-ui-e2e/E2E_TESTS.md - Comprehensive guide to the E2E testing architecture, fixtures, interceptors, and best practices. Consult this document for deeper understanding of test patterns and troubleshooting.

## Your Task

Run E2E tests and provide detailed reporting of results, failures, and debugging information.

### Step 1: Determine Test Scope

**If test file provided** (`$ARGUMENTS` is not empty):

- Validate the file path exists
- Extract relative path from workspace root
- Run single test file

**If no test file provided** (`$ARGUMENTS` is empty):

- Run entire E2E test suite
- Report on all test categories (devices, player, etc.)

### Step 2: Execute Tests

Run the appropriate Cypress command:

**Single test file**:

```bash
pnpm nx e2e teensyrom-ui-e2e --spec="$ARGUMENTS"
```

**Full suite**:

```bash
pnpm nx e2e teensyrom-ui-e2e
```

**Additional flags to consider**:

- Add `--headed` for debugging (shows browser)
- Add `--browser=chrome` for specific browser
- Add `--record` if configured for Cypress Dashboard

### Step 3: Capture Test Results

Monitor the test execution output for:

- Total tests run
- Passing tests count
- Failing tests count
- Skipped/pending tests
- Execution duration
- Screenshot paths (on failure)
- Video paths (if enabled)

### Step 4: Analyze Failures

For each failing test, extract and report:

**Test Identification**:

- Test file path
- Test suite name (`describe` block)
- Test case name (`it` block)
- Line number where failure occurred

**Failure Details**:

- Error message
- Stack trace
- Expected vs actual values (for assertions)
- DOM state at failure (from screenshot)
- Network requests (from video/logs)

**Context Information**:

- Which interceptor was active
- What fixture data was used
- Any console errors
- Timing information (did it timeout?)

### Step 5: Locate Debug Artifacts

After test run, find and report paths to:

**Screenshots** (auto-captured on failure):

```
dist/cypress/apps/teensyrom-ui-e2e/screenshots/
```

**Videos** (if enabled):

```
dist/cypress/apps/teensyrom-ui-e2e/videos/
```

**Spec-specific artifacts**:

- Each test file gets its own subfolder
- Screenshots include timestamp and test name
- Videos show full test execution

### Step 6: Provide Debugging Recommendations

Based on failure patterns, suggest:

**Common Issues**:

- **Selector not found**: Check if `data-testid` attribute exists in component
- **Timeout errors**: Interceptor may not be registered before navigation
- **Assertion failures**: Fixture data may not match expected values
- **Network errors**: Check API endpoint patterns match OpenAPI spec

**Investigation Steps**:

1. Review the screenshot to see UI state at failure
2. Check if interceptor pattern matches actual API call
3. Verify fixture data structure matches DTO types
4. Run test in headed mode for interactive debugging
5. Check console logs for JavaScript errors

**Advanced Debugging**:

- Use `pnpm nx e2e teensyrom-ui-e2e:open-cypress` for interactive mode
- Connect Chrome DevTools MCP for live inspection (see E2E_TESTS.md)
- Review OpenAPI spec at `../../TeensyRom.Api/api-spec/TeensyRom.Api.json`

### Step 7: Generate Report

Provide a structured summary:

```
## E2E Test Results

**Scope**: [Full Suite | Single File: <path>]
**Duration**: <duration>
**Status**: [✅ All Passing | ⚠️ Some Failures | ❌ All Failed]

### Summary
- ✅ Passing: X tests
- ❌ Failing: Y tests
- ⏭️ Skipped: Z tests
- 📊 Total: X+Y+Z tests

### Failures

#### 1. Test Name
- **File**: apps/teensyrom-ui-e2e/src/e2e/devices/device-discovery.cy.ts:42
- **Error**: CypressError: Timed out retrying after 4000ms: Expected to find element: [data-testid="device-card"], but never found it.
- **Screenshot**: dist/cypress/.../device-discovery.cy.ts/test-name.png
- **Likely Cause**: Interceptor not registered before navigation
- **Fix**: Move `interceptFindDevices()` before `navigateToDeviceView()`

#### 2. Another Test Name
...

### Debug Artifacts
- Screenshots: dist/cypress/apps/teensyrom-ui-e2e/screenshots/
- Videos: dist/cypress/apps/teensyrom-ui-e2e/videos/

### Recommendations
[Specific advice based on failure patterns]
```

## Important Guidelines

- **Parse output carefully**: Cypress output is verbose - extract key information
- **Check artifacts**: Always look for screenshots/videos on failure
- **Understand architecture**: Tests use fixture-driven interceptors (see E2E_TESTS.md)
- **Respect constants**: Tests use centralized constants (selectors, endpoints, etc.)
- **Consider timing**: Most failures are timing-related (interceptors, navigation, etc.)
- **Reference docs**: Point to E2E_TESTS.md sections for deeper investigation

Remember: Tests use fixture data, not real backend - failures typically indicate UI or interceptor issues, not API problems.
