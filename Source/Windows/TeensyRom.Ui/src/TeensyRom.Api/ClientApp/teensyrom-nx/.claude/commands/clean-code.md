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
- Current ESLint errors: !`pnpm nx lint --fix=false 2>&1 | grep -A 5 "$ARGUMENTS" || echo "No errors found"`
- Current TypeScript errors: !`pnpm tsc --noEmit 2>&1 | grep "$ARGUMENTS" || echo "No errors found"`

## Your Task

Follow this systematic approach to clean the file:

### Step 1: Analyze Errors

1. Read the file at `$ARGUMENTS` to understand its current state
2. Use `get_errors` tool to identify all TypeScript and ESLint errors in the file
3. Categorize errors by type (e.g., unused imports, type errors, linting violations)
4. Create a mental checklist of all errors to fix

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

Fix errors in this priority order:

1. **Unused imports/variables** - Safe to remove
2. **Type errors** - Add proper types, fix type mismatches
3. **ESLint violations** - Auto-fix where possible, manually fix complex issues
4. **Architecture violations** - Fix module boundary violations per project's Clean Architecture rules

For each fix:

- Make small, focused changes
- Preserve existing functionality
- Add types rather than using `any` (only use `any` as last resort)
- Follow the project's coding standards (see attached instructions)

### Step 5: Validate After Fixes

After fixing all errors in the file:

1. Re-run the tests: `pnpm nx test <project-name> --testFile="<test-file-path>"`
2. Compare results to baseline
3. If tests fail that previously passed:
   - Document the failure
   - Report that manual intervention is needed
   - Do NOT iterate multiple times - this is a single-file command

### Step 6: Report

Provide a summary including:

- Number and types of errors fixed
- Test results (before/after)
- Any errors that couldn't be fixed (with explanation)
- Any pre-existing test failures
- Recommendations for further improvements

## Important Guidelines

- **Single file focus** - This command cleans ONE file, not the whole workspace
- **Never break functionality** - If fixes cause test failures, document them
- **Respect Clean Architecture** - Don't fix module boundary violations by adding dependencies that violate architectural constraints
- **Preserve intent** - Understand what the code is trying to do before changing it
- **Type safety** - Add proper types; avoid `any` except when truly necessary
- **No complex iteration** - Fix the file once and report; don't retry multiple times
- **Document blockers** - If an error can't be fixed, explain why clearly

## Error Handling

If you encounter:

- **Module boundary violations**: Check `.eslintrc` and Clean Architecture rules; may need to refactor dependencies
- **Missing type definitions**: Check if types should be imported from domain layer contracts
- **Test failures**: Carefully read test expectations; your fix may be changing behavior unexpectedly
- **Circular dependencies**: May indicate architectural issue; document for team discussion

Remember: The goal is clean code that works correctly, not just code that passes linting.
