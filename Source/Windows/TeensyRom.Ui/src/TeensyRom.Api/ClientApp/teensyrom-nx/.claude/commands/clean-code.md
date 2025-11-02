---
description: Clean TypeScript and ESLint errors from a single file with test validation
argument-hint: <file-path>
allowed-tools: Bash(pnpm:*), Bash(nx:*), Read, Write, Edit, Grep
---

# Clean Code Command

Clean all TypeScript and ESLint errors from ONE specified file while ensuring no functional regressions through test validation.

**Scope**: This command focuses on a SINGLE file. For workspace-wide cleanup, use the `code-cleaner` subagent which orchestrates this command across multiple files.

## Context

- File to clean: $ARGUMENTS
- **Run comprehensive quality analysis** to identify all issues in the file:
  ```bash
  node .claude/hooks/angular-nx-quality/quality-check.cjs --json "$ARGUMENTS"
  ```
- This provides complete issue analysis: TypeScript compilation errors, ESLint violations (all treated as critical), and Prettier formatting issues

## Your Task

Follow this systematic approach to clean the file:

### Step 1: Analyze Errors

1. Read the file at `$ARGUMENTS` to understand its current state
2. **Run comprehensive quality analysis** to identify all issues:
   ```bash
   node .claude/hooks/angular-nx-quality/quality-check.cjs --json "$ARGUMENTS"
   ```
3. Parse the structured JSON output from stdout to understand:
   - **TypeScript compilation errors** (severity 6-9)
   - **ESLint violations** (all treated as critical, severity 8-9)
     - Special attention to `@typescript-eslint/no-explicit-any` violations (severity 9)
     - Style violations, unused variables, architecture issues
   - **Prettier formatting issues** (severity 3, auto-fixable)
4. Categorize errors by severity and type:
   - **Critical TypeScript errors** - compilation failures, type mismatches
   - **Critical ESLint violations** - `any` types, unused code, style issues
   - **Format issues** - Prettier inconsistencies
5. Create a prioritized checklist: TypeScript compilation errors → ESLint violations → formatting issues

### Step 2: Identify Test Files

1. Look for associated test files using the pattern:
   - Same directory: `<filename>.spec.ts`
   - Nearby test directories: Look for test files that import or reference the target file
2. Use `grep_search` to find files that import the target file - these may have tests
3. Note the test project name (e.g., `@teensyrom-nx/infrastructure`, `@teensyrom-nx/application`)

### Step 3: Run Baseline Tests

1. Run tests for the identified test files: `pnpm nx test <project-name> --testFile="<test-file-path>"`
2. Record baseline test results (pass/fail count)
3. If tests are failing before changes, note them as pre-existing issues

### Step 4: Fix Errors Systematically

Fix errors in this priority order based on severity scores:

1. **TypeScript compilation errors** (severity 8-9) - Must be fixed first
   - Missing imports, type mismatches, interface errors
   - Critical compilation failures that prevent code execution
2. **ESLint type violations** (severity 9) - Critical type safety issues
   - `@typescript-eslint/no-explicit-any` violations
   - Replace `any` types with proper TypeScript types
3. **ESLint architecture violations** (severity 8-9) - Module boundary and unused code issues
   - Unused imports/variables, missing dependencies
   - Architecture violations per Clean Architecture rules
4. **ESLint style violations** (severity 8) - Code style and formatting issues
   - Quotes, semicolons, indentation (all treated as critical)
5. **Prettier formatting issues** (severity 3) - Auto-fixable formatting

For each fix:

- Make small, focused changes
- Preserve existing functionality
- **Replace `any` types**: Use specific types based on context and usage patterns
- Add proper types rather than using `unknown` or `any`
- Follow the project's coding standards and Clean Architecture principles

### Step 5: Validate After Fixes

After fixing all errors in the file:

1. **Re-run comprehensive quality analysis** to verify all issues are resolved:
   ```bash
   node .claude/hooks/angular-nx-quality/quality-check.cjs --json "$ARGUMENTS"
   ```
2. Verify the output shows:
   - **No critical issues** remaining (totalIssues should be 0 or only non-critical formatting issues)
   - **TypeScript compilation** passes
   - **ESLint violations** resolved (especially `no-explicit-any` issues)
3. Re-run the tests: `pnpm nx test <project-name> --testFile="<test-file-path>"`
4. Compare results to baseline
5. If tests fail that previously passed:
   - Document the failure
   - Report that manual intervention is needed
   - Do NOT iterate multiple times - this is a single-file command

### Step 6: Report

Provide a summary including:

- Number and types of errors fixed by category:
  - TypeScript compilation errors resolved
  - ESLint violations fixed (especially `@typescript-eslint/no-explicit-any` issues)
  - Style and formatting issues corrected
- Quality check results before/after (totalIssues, critical issues count)
- Test results (before/after)
- Any errors that couldn't be fixed (with explanation)
- Any pre-existing test failures
- Recommendations for further improvements

## Important Guidelines

- **Single file focus** - This command cleans ONE file, not the whole workspace
- **Never break functionality** - If fixes cause test failures, document them
- **Zero tolerance for ESLint violations** - All ESLint issues are critical and must be fixed
- **Replace `any` and `unknown` types** - Always use specific TypeScript types instead of `any` or `unknown`
- **Respect Clean Architecture** - Don't fix module boundary violations by adding dependencies that violate architectural constraints
- **Preserve intent** - Understand what the code is trying to do before changing it
- **Comprehensive quality** - Use quality-check.cjs to ensure ALL issues are identified and fixed
- **No complex iteration** - Fix the file once and report; don't retry multiple times
- **Document blockers** - If an error can't be fixed, explain why clearly

## Error Handling

If you encounter:

- **Module boundary violations**: Check `.eslintrc` and Clean Architecture rules; may need to refactor dependencies
- **Missing type definitions**: Check if types should be imported from domain layer contracts
- **Test failures**: Carefully read test expectations; your fix may be changing behavior unexpectedly
- **Circular dependencies**: May indicate architectural issue; document for team discussion

Remember: The goal is clean code that works correctly, not just code that passes linting.
