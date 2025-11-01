---
description: Run Cypress E2E tests and generate detailed report.
argument-hint: [test-file-full-path]
allowed-tools: Bash(pnpm:*), Bash(nx:*)
---

## Steps to run
- Take the [test-file-full-path] and just grab the file name.  These files end in .cy.ts
- We'll call this [file-name].
  - For example, [file-name] should be look something like filename.cy.ts
- If [file-name] is not empty call:
  - pnpm nx e2e teensyrom-ui-e2e --spec=src/e2e/[file-name]
- If [file-name] is empty, call: 
  - pnpm nx e2e teensyrom-ui-e2e
- Make sure you wait until ALL the tests are passing before you complete.


## E2E Test Report

### Test Execution Summary
- Take note of all the problems you see.
- Return a detailed report using the format below:
- **Test File**: [file-name or "All tests"]
- **Command Run**: [exact command executed]
- **Start Time**: [timestamp when tests started]
- **End Time**: [timestamp when tests completed]
- **Total Duration**: [total time taken]
- **Overall Status**: [PASSED/FAILED]

### Test Results
- **Tests Passed**: [number]
- **Tests Failed**: [number]
- **Tests Skipped**: [number]
- **Test Suites**: [number of suites run]

### Failed Tests Details
[If any tests failed, include for each failed test:]
- **Test File**: [full path to test file]
- **Test Name**: [full test title/description]
- **Error Message**: [complete error message]
- **Stack Trace**: [relevant stack trace if available]
- **Screenshot**: [mention if screenshots were captured]
- **Video**: [mention if video recordings are available]

### Cypress Output
[Include any relevant Cypress console output, warnings, or errors]

### Browser Information
- **Browser**: [browser used]
- **Version**: [browser version]
- **Headless**: [yes/no]

### Recommendations
[If tests failed, provide specific recommendations for fixing the issues]

