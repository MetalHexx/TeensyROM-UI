---
description: Run Cypress E2E tests and generate detailed JSON report.
argument-hint: [test-file-full-path]
allowed-tools: Bash(pnpm:*), Bash(nx:*), Read(apps/teensyrom-ui-e2e/dist/**)
---

## Steps to run

1. Take the [test-file-full-path] and extract just the filename (these files end in .cy.ts)
   - For example: `app.cy.ts`, `devices/device-indexing.cy.ts`
2. Run the appropriate command with a 10-minute timeout (E2E tests take several minutes):
   - If [file-name] is specified: `pnpm nx e2e:report teensyrom-ui-e2e --spec=src/e2e/[file-name]`
   - If [file-name] is empty: `pnpm nx e2e:report teensyrom-ui-e2e`
3. **Let the Bash tool wait for command completion naturally** (no polling needed)
4. The command will exit with status code 0 on success, or non-zero on error
5. After completion, read the JSON report and generate the test summary

## Read JSON Report

After tests complete, read the JSON report:

- **Report Location**: `apps/teensyrom-ui-e2e/dist/cypress/apps/teensyrom-ui-e2e/reports/index.json`
- Use the Read tool to load this file
- Parse the JSON to extract test results and failures

## E2E Test Report

Return a detailed report using the format below, extracting data from the JSON report:

### JSON Report Location

`apps/teensyrom-ui-e2e/dist/cypress/apps/teensyrom-ui-e2e/reports/index.json`

### Test Execution Summary

- **Test File**: [file-name or "All tests"]
- **Command Run**: [exact command executed]
- **Start Time**: [from JSON: stats.start]
- **End Time**: [from JSON: stats.end]
- **Total Duration**: [from JSON: stats.duration]ms
- **Overall Status**: [PASSED if stats.failures === 0, else FAILED]

### Test Results (from JSON stats object)

- **Total Tests**: [stats.tests]
- **Tests Passed**: [stats.passes]
- **Tests Failed**: [stats.failures]
- **Tests Pending**: [stats.pending]
- **Tests Skipped**: [stats.skipped]
- **Pass Percentage**: [stats.passPercent]%
- **Test Suites**: [stats.suites]

### Failed Tests Details

[Parse from JSON: results[].suites[].tests[] where test.fail === true]
[For each failed test, extract:]

#### [test.fullTitle]

- **File**: [test.fullFile] (navigate to this file to fix)
- **Error**: [test.err.message]
- **Stack Trace**:
  ```
  [first 10 lines of test.err.estack]
  ```
- **Test Code**:
  ```typescript
  [test.code];
  ```
- **Duration**: [test.duration]ms

### Browser Information (from console output)

- **Browser**: [browser used]
- **Version**: [browser version]
- **Headless**: [yes/no]

### Screenshots

- **Location**: `apps/teensyrom-ui-e2e/dist/cypress/apps/teensyrom-ui-e2e/screenshots/`
- [List any screenshots captured for failures]

### Recommendations

[If tests failed, analyze the error messages and stack traces from JSON to provide specific recommendations for fixing the issues]

## Implementation Notes

- **No polling required**: The Bash tool automatically waits for command completion
- **Single source of truth**: All test results come from the JSON report, not console output
- **Efficient**: Only 2 tool calls needed (Bash to run tests, Read to get results)
- **Deterministic**: JSON schema is stable; no fragile text pattern matching
- **Exit codes**: Indicate if command ran (0 = success, non-zero = error), not if tests passed (JSON shows pass/fail)
