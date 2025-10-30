#!/usr/bin/env node
import { execSync } from 'child_process';
import { existsSync, writeFileSync } from 'fs';

interface FileAudit {
  file: string;
  eslintErrors: string[];
  typescriptErrors: string[];
  hasErrors: boolean;
}

interface AuditResult {
  totalFiles: number;
  filesWithErrors: number;
  audits: FileAudit[];
}

/**
 * Execute a command and return output, handling errors gracefully
 */
function executeCommand(command: string): { stdout: string; stderr: string; exitCode: number } {
  try {
    const stdout = execSync(command, { encoding: 'utf-8', stdio: 'pipe' });
    return { stdout, stderr: '', exitCode: 0 };
  } catch (error: unknown) {
    const execError = error as { stdout?: Buffer; stderr?: Buffer; status?: number };
    return {
      stdout: execError.stdout?.toString() || '',
      stderr: execError.stderr?.toString() || '',
      exitCode: execError.status || 1,
    };
  }
}

/**
 * Get list of changed files from git diff
 */
function getChangedFiles(): string[] {
  console.log('🔍 Finding changed files...\n');

  // Get changed files (staged and unstaged)
  // Note: git commands return paths relative to cwd when run from subdirectory
  const { stdout: stagedFiles } = executeCommand('git diff --name-only --cached');
  const { stdout: unstagedFiles } = executeCommand('git diff --name-only');
  const { stdout: untrackedFiles } = executeCommand('git ls-files --others --exclude-standard');

  const allFiles = [
    ...stagedFiles.split('\n'),
    ...unstagedFiles.split('\n'),
    ...untrackedFiles.split('\n'),
  ]
    .map((f) => f.trim())
    .filter((f) => f.length > 0)
    .filter((f) => /\.(ts|tsx)$/.test(f) && !f.includes('.spec.') && !f.includes('node_modules'));

  // Remove duplicates and verify files exist
  const uniqueFilesSet = new Set(allFiles);
  const uniqueFiles: string[] = [];
  
  uniqueFilesSet.forEach((file) => {
    // Git returns paths that should work from current directory
    const normalizedPath = file.replace(/\//g, '\\');
    
    // Check if file exists relative to cwd
    if (existsSync(normalizedPath)) {
      uniqueFiles.push(normalizedPath);
    }
  });

  console.log(`Found ${uniqueFiles.length} TypeScript file(s) with changes:\n`);
  uniqueFiles.forEach((file) => console.log(`  - ${file}`));
  console.log('');

  return uniqueFiles;
}

/**
 * Run ESLint on a specific file
 */
function auditWithESLint(file: string): string[] {
  const errors: string[] = [];

  if (!existsSync(file)) {
    errors.push(`⚠️  File not found: ${file}`);
    return errors;
  }

  console.log(`  📋 Running ESLint...`);

  const { stdout, stderr, exitCode } = executeCommand(
    `npx eslint "${file}" --format json --no-error-on-unmatched-pattern`
  );

  if (exitCode !== 0 && stdout) {
    try {
      const results = JSON.parse(stdout);
      if (results && results.length > 0) {
        const fileResult = results[0];
        if (fileResult.messages && fileResult.messages.length > 0) {
          fileResult.messages.forEach((msg: { severity: number; line: number; column: number; message: string; ruleId?: string }) => {
            const severity = msg.severity === 2 ? 'ERROR' : 'WARNING';
            errors.push(
              `    [${severity}] Line ${msg.line}:${msg.column} - ${msg.message} (${msg.ruleId || 'eslint'})`
            );
          });
        }
      }
    } catch {
      errors.push(`    ⚠️  Failed to parse ESLint output: ${stderr || stdout}`);
    }
  } else if (stdout) {
    // Check for warnings even when exitCode is 0
    try {
      const results = JSON.parse(stdout);
      if (results && results.length > 0) {
        const fileResult = results[0];
        if (fileResult.messages && fileResult.messages.length > 0) {
          fileResult.messages.forEach((msg: { severity: number; line: number; column: number; message: string; ruleId?: string }) => {
            const severity = msg.severity === 2 ? 'ERROR' : 'WARNING';
            errors.push(
              `    [${severity}] Line ${msg.line}:${msg.column} - ${msg.message} (${msg.ruleId || 'eslint'})`
            );
          });
        }
      }
    } catch {
      // Ignore parse errors when exitCode is 0
    }
  }

  return errors;
}

/**
 * Run TypeScript compiler check on a specific file
 */
function auditWithTypeScript(file: string): string[] {
  const errors: string[] = [];

  if (!existsSync(file)) {
    return errors; // Already reported in ESLint
  }

  console.log(`  🔷 Running TypeScript check...`);

  const { stdout, stderr, exitCode } = executeCommand(
    `npx tsc --noEmit --pretty false "${file}"`
  );

  if (exitCode !== 0) {
    const output = stdout + stderr;
    const lines = output.split('\n').filter((line) => line.trim().length > 0);

    lines.forEach((line) => {
      // Parse TypeScript error format: file(line,col): error TS####: message
      if (line.includes(file) && line.includes('error TS')) {
        const match = line.match(/\((\d+),(\d+)\):\s*error\s*(TS\d+):\s*(.+)/);
        if (match) {
          const [, lineNum, col, errorCode, message] = match;
          errors.push(`    [ERROR] Line ${lineNum}:${col} - ${message} (${errorCode})`);
        } else {
          // Fallback for different format
          errors.push(`    [ERROR] ${line.trim()}`);
        }
      }
    });
  }

  return errors;
}

/**
 * Audit a single file
 */
function auditFile(file: string): FileAudit {
  console.log(`\n🔎 Auditing: ${file}`);

  const eslintErrors = auditWithESLint(file);
  const typescriptErrors = auditWithTypeScript(file);

  const hasErrors = eslintErrors.length > 0 || typescriptErrors.length > 0;

  if (!hasErrors) {
    console.log(`  ✅ No errors found`);
  } else {
    console.log(
      `  ❌ Found ${eslintErrors.length} ESLint error(s) and ${typescriptErrors.length} TypeScript error(s)`
    );
  }

  return {
    file,
    eslintErrors,
    typescriptErrors,
    hasErrors,
  };
}

/**
 * Generate markdown report
 */
function generateReport(result: AuditResult): string {
  const timestamp = new Date().toISOString();
  let report = `# Code Audit Report\n\n`;
  report += `**Generated:** ${timestamp}\n\n`;
  report += `**Branch:** ${executeCommand('git rev-parse --abbrev-ref HEAD').stdout.trim()}\n\n`;
  report += `---\n\n`;

  report += `## Summary\n\n`;
  report += `- **Total Files Audited:** ${result.totalFiles}\n`;
  report += `- **Files with Errors:** ${result.filesWithErrors}\n`;
  report += `- **Files Clean:** ${result.totalFiles - result.filesWithErrors}\n\n`;

  if (result.filesWithErrors === 0) {
    report += `### ✅ All files passed audit!\n\n`;
    report += `No ESLint or TypeScript errors found in the changed files.\n\n`;
  } else {
    report += `### ❌ Issues Found\n\n`;
    report += `The following files have errors that need to be corrected:\n\n`;
  }

  report += `---\n\n`;

  // Group files by status
  const filesWithErrors = result.audits.filter((a) => a.hasErrors);
  const cleanFiles = result.audits.filter((a) => !a.hasErrors);

  if (filesWithErrors.length > 0) {
    report += `## Files with Errors (${filesWithErrors.length})\n\n`;

    filesWithErrors.forEach((audit) => {
      report += `### 📄 \`${audit.file}\`\n\n`;

      const totalErrors = audit.eslintErrors.length + audit.typescriptErrors.length;
      report += `**Total Issues:** ${totalErrors}\n\n`;

      if (audit.eslintErrors.length > 0) {
        report += `#### ESLint Errors (${audit.eslintErrors.length})\n\n`;
        report += '```\n';
        audit.eslintErrors.forEach((error) => {
          report += error + '\n';
        });
        report += '```\n\n';
      }

      if (audit.typescriptErrors.length > 0) {
        report += `#### TypeScript Errors (${audit.typescriptErrors.length})\n\n`;
        report += '```\n';
        audit.typescriptErrors.forEach((error) => {
          report += error + '\n';
        });
        report += '```\n\n';
      }

      report += `---\n\n`;
    });
  }

  if (cleanFiles.length > 0) {
    report += `## Clean Files (${cleanFiles.length})\n\n`;
    report += `The following files passed all checks:\n\n`;
    cleanFiles.forEach((audit) => {
      report += `- ✅ \`${audit.file}\`\n`;
    });
    report += `\n`;
  }

  report += `---\n\n`;
  report += `## Recommendations\n\n`;
  if (filesWithErrors.length > 0) {
    report += `1. Review and fix all ESLint errors following the project's coding standards\n`;
    report += `2. Address TypeScript type violations to ensure type safety\n`;
    report += `3. Run \`pnpm nx lint\` to verify fixes\n`;
    report += `4. Consider running \`pnpm format\` to auto-fix formatting issues\n`;
  } else {
    report += `All files are clean! You're ready to commit.\n`;
  }

  return report;
}

/**
 * Main execution
 */
function main() {
  console.log('🚀 Starting Code Audit Tool\n');
  console.log('='.repeat(60));

  const changedFiles = getChangedFiles();

  if (changedFiles.length === 0) {
    console.log('\n✅ No TypeScript files have changed. Nothing to audit.\n');
    const report = generateReport({ totalFiles: 0, filesWithErrors: 0, audits: [] });
    writeFileSync('code-audit.md', report);
    console.log('📝 Report saved to: code-audit.md\n');
    return;
  }

  console.log('='.repeat(60));

  const audits: FileAudit[] = [];

  for (const file of changedFiles) {
    const audit = auditFile(file);
    audits.push(audit);
  }

  const result: AuditResult = {
    totalFiles: audits.length,
    filesWithErrors: audits.filter((a) => a.hasErrors).length,
    audits,
  };

  console.log('\n' + '='.repeat(60));
  console.log('\n📊 Audit Complete!\n');
  console.log(`Total Files: ${result.totalFiles}`);
  console.log(`Files with Errors: ${result.filesWithErrors}`);
  console.log(`Clean Files: ${result.totalFiles - result.filesWithErrors}`);

  const report = generateReport(result);
  writeFileSync('code-audit.md', report);

  console.log('\n📝 Report saved to: code-audit.md\n');

  if (result.filesWithErrors > 0) {
    console.log('⚠️  Some files have errors. Please review the report.\n');
    process.exit(1);
  } else {
    console.log('✅ All files passed audit!\n');
  }
}

main();
