---
description: Run code audit tool to detect and fix errors in changed files
---

Run the code audit tool to check for ESLint and TypeScript errors in modified files, then iteratively fix them until all issues are resolved.

## Process

1. Run the audit tool to detect issues in changed files:
   ```bash
   pnpm run audit:code
   ```

2. Read the generated `code-audit.md` report to see what errors/warnings were found.

3. For each file with issues:
   - Analyze the errors and warnings reported
   - Fix the issues by editing the files
   - Focus on one file at a time to ensure quality fixes

4. After fixing issues, re-run the audit tool to verify the fixes worked:
   ```bash
   pnpm run audit:code
   ```

5. Repeat steps 2-4 until the audit report shows **"All files passed audit!"**

6. If you encounter any issues you cannot fix automatically (complex logic changes, architectural decisions, or ambiguous requirements), report back to the user with:
   - The specific file and line number
   - The error/warning message
   - Why you need guidance (e.g., "This requires a design decision about..." or "This change might affect business logic...")
   - Suggested approaches if applicable

## Important Notes

- The tool checks **only changed files** (staged, unstaged, and untracked)
- It reports both ESLint errors/warnings AND TypeScript compilation errors
- Continue the cycle until zero issues remain
- Do not skip warnings - treat them as errors that need fixing
- After all issues are resolved, confirm success by showing the final clean report
- Delete the @code-audit.md once completed.