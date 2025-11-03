---
description: Run Cypress E2E tests and generate detailed JSON report.
argument-hint: [test-file-full-path]
allowed-tools: Bash(pnpm:*), Bash(nx:*), Read(apps/teensyrom-ui-e2e/dist/**)
---

## Steps to run
- Take the [test-file-full-path] and just grab the file name.  These files end in .cy.ts
- We'll call this [file-name].
  - For example, [file-name] should be look something like filename.cy.ts
- If [file-name] is not empty call:
  - pnpm nx e2e:report teensyrom-ui-e2e --spec=src/e2e/[file-name]
- If [file-name] is empty, call:
  - pnpm nx e2e:report teensyrom-ui-e2e

## WAIT FOR THE TEST TO COMPLETE!!
- Make sure you wait until ALL the tests are passing before you complete.

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
  [test.code]
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

### Agent Instructions
- Use the JSON report at the path above for detailed analysis
- Navigate to files listed in `test.fullFile` to view and fix failing tests
- Error messages and stack traces provide exact failure points

