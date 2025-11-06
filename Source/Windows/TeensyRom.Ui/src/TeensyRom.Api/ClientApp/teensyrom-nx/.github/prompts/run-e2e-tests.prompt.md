---
description: Execute Cypress E2E tests with detailed JSON reporting and analysis
mode: agent
tools: ['runCommands', 'runTests']
---

# Run E2E Tests

Execute Cypress E2E tests and generate a detailed JSON report with comprehensive test analysis.

## Input

Provide the path to a specific test file (e.g., `apps/teensyrom-ui-e2e/src/e2e/app.cy.ts`) or leave empty to run all tests.

## Process

### Step 1: Parse Test File Input

Extract the test filename from the provided path:
- Expected format: Files ending in `.cy.ts`
- Examples: `app.cy.ts`, `devices/device-indexing.cy.ts`
- Remove directory structure to get filename only

### Step 2: Run E2E Tests

Execute tests with a 10-minute timeout (E2E tests typically run 2-5 minutes):

**For specific test file:**
```bash
pnpm nx e2e:report teensyrom-ui-e2e --spec=src/e2e/[file-name]
```

**For all tests:**
```bash
pnpm nx e2e:report teensyrom-ui-e2e
```

**Important**: Let the command complete naturally without polling. It will exit with:
- Status code `0` on successful execution
- Non-zero status code on execution errors

### Step 3: Parse JSON Report

After tests complete, read the JSON report from:
```
apps/teensyrom-ui-e2e/dist/cypress/apps/teensyrom-ui-e2e/reports/index.json
```

## Output Format

Generate a comprehensive report with the following sections:

### Test Execution Summary

- **Test File**: [specific file or "All tests"]
- **Command Run**: [exact command executed]
- **Start Time**: [from JSON: stats.start]
- **End Time**: [from JSON: stats.end]
- **Total Duration**: [from JSON: stats.duration]ms
- **Overall Status**: `PASSED` (if stats.failures === 0) or `FAILED`

### Test Results Statistics

- **Total Tests**: [stats.tests]
- **Tests Passed**: [stats.passes]
- **Tests Failed**: [stats.failures]
- **Tests Pending**: [stats.pending]
- **Tests Skipped**: [stats.skipped]
- **Pass Percentage**: [stats.passPercent]%
- **Test Suites**: [stats.suites]

### Failed Tests Details

For each failed test (from `results[].suites[].tests[]` where `test.fail === true`):

#### [test.fullTitle]

- **File**: [test.fullFile] (navigate to this file to fix)
- **Error Message**: [test.err.message]
- **Stack Trace** (first 10 lines):
  ```
  [test.err.estack]
  ```
- **Test Code**:
  ```typescript
  [test.code]
  ```
- **Duration**: [test.duration]ms

### Browser Information

- **Browser**: [browser type from execution]
- **Version**: [browser version]
- **Headless Mode**: [yes/no]

### Screenshots and Artifacts

- **Screenshot Location**: `apps/teensyrom-ui-e2e/dist/cypress/apps/teensyrom-ui-e2e/screenshots/`
- **List**: Any screenshots captured for failed tests

### Analysis and Recommendations

Provide specific recommendations for fixing failed tests based on:
- Error messages and stack traces from the JSON report
- Test code analysis
- Common E2E test failure patterns

## Implementation Notes

- **No polling required**: Commands automatically wait for completion
- **Single source of truth**: All test results come from the JSON report, not console output
- **Efficient**: Typically 2 tool calls needed (run tests, read report)
- **Deterministic**: JSON schema is stable; no fragile text pattern matching
- **Exit codes**: Indicate if command ran (0 = success, non-zero = error), not if tests passed (JSON shows pass/fail)

## Reference Documentation

- [E2E Test Architecture](../../apps/teensyrom-ui-e2e/E2E_TESTS.md)
- [Testing Standards](../../docs/TESTING_STANDARDS.md)
- [Test Fixtures and Helpers](../../apps/teensyrom-ui-e2e/src/support/)
