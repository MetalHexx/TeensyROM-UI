#!/usr/bin/env node

/**
 * TeensyROM Angular/Nx Quality Check Hook - AI Enhanced
 *
 * Enhanced quality gate with structured output for Claude Code agents.
 * Provides AI-consumable format with actionable recommendations and
 * Nx project awareness for TeensyROM's Clean Architecture.
 *
 * Features:
 * - Structured JSON output for AI consumption
 * - Nx project and architecture layer awareness
 * - Actionable fix recommendations with tool suggestions
 * - Severity scoring and priority ordering
 * - Integration-ready format for Claude Code workflows
 */

const { execSync } = require('child_process');
const { readFileSync, writeFileSync, existsSync } = require('fs');
const { createHash } = require('crypto');
const { join, dirname } = require('path');

// Colors for output
const colors = {
  reset: '\x1b[0m',
  red: '\x1b[31m',
  green: '\x1b[32m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
  magenta: '\x1b[35m',
  cyan: '\x1b[36m'
};

// TeensyROM specific configuration
const TEENSYROM_CONFIG = {
  // Critical TypeScript patterns that should always block
  criticalTypeErrors: [
    'error TS',  // All TypeScript compilation errors
  ],

  // Auto-fixable ESLint rules (common ones)
  autoFixableRules: [
    'quotes', 'semi', 'comma-dangle', 'indent', 'object-curly-spacing',
    'array-bracket-spacing', 'space-before-function-paren', 'keyword-spacing'
  ],

  // File patterns to validate
  includePatterns: [
    '**/*.ts',
    '**/*.tsx',
    '**/*.js',
    '**/*.jsx'
  ],

  // Exclude patterns
  excludePatterns: [
    '**/node_modules/**',
    '**/dist/**',
    '**/build/**',
    '**/.nx/**',
    '**/coverage/**'
  ]
};

/**
 * Represents a single quality issue with structured metadata
 */
class QualityIssue {
  constructor(severity, tool, category, file, line = null, column = null, message = '', rule = '', fixable = false) {
    this.id = this.generateId();
    this.severity = severity; // 1-10 numeric score
    this.tool = tool; // 'TypeScript', 'ESLint', 'Prettier'
    this.category = category; // 'syntax', 'type', 'style', 'architecture', 'format'
    this.file = file;
    this.line = line;
    this.column = column;
    this.message = message;
    this.rule = rule; // Specific rule name
    this.fixable = fixable; // boolean
    this.fixCommand = null; // Generated fix command
    this.project = null; // Nx project context
    this.architectureLayer = null; // domain, application, infrastructure
    this.recommendation = ''; // Actionable recommendation
    this.suggestedTool = null; // Claude Code tool suggestion
  }

  generateId() {
    return `issue-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  setProjectContext(project, architectureLayer) {
    this.project = project;
    this.architectureLayer = architectureLayer;
  }

  setFixDetails(command, recommendation, suggestedTool = null) {
    this.fixCommand = command;
    this.recommendation = recommendation;
    this.suggestedTool = suggestedTool;
  }

  getSeverityLevel() {
    if (this.severity >= 8) return 'critical';
    if (this.severity >= 6) return 'error';
    if (this.severity >= 4) return 'warning';
    return 'info';
  }
}

/**
 * Handles output formatting for different modes (JSON, Markdown, Console)
 */
class OutputFormatter {
  /**
   * Formats output as structured JSON for AI consumption
   */
  static formatJson(issues, metadata = {}) {
    const summary = this.generateSummary(issues);

    return JSON.stringify({
      timestamp: new Date().toISOString(),
      file: metadata.targetFile || null,
      project: metadata.project || null,
      projectType: metadata.projectType || null,
      summary,
      issues: issues.map(issue => ({
        id: issue.id,
        severity: issue.severity,
        severityLevel: issue.getSeverityLevel(),
        tool: issue.tool,
        category: issue.category,
        file: issue.file,
        line: issue.line,
        column: issue.column,
        message: issue.message,
        rule: issue.rule,
        fixable: issue.fixable,
        fixCommand: issue.fixCommand,
        recommendation: issue.recommendation,
        suggestedTool: issue.suggestedTool,
        project: issue.project,
        architectureLayer: issue.architectureLayer
      })),
      metadata: {
        fileType: metadata.fileType,
        architectureLayer: metadata.architectureLayer,
        dependencies: metadata.dependencies || [],
        changeImpact: metadata.changeImpact || 'unknown'
      },
      recommendations: this.generateRecommendations(issues)
    }, null, 2);
  }

  /**
   * Formats output as structured markdown for human readability
   */
  static formatMarkdown(issues, metadata = {}) {
    const summary = this.generateSummary(issues);
    const output = [];

    // Header
    output.push(`# Quality Check Results`);
    output.push(`**File:** \`${metadata.targetFile || 'Multiple files'}\``);
    output.push(`**Project:** ${metadata.project || 'Unknown'}`);
    output.push(`**Timestamp:** ${new Date().toISOString()}`);
    output.push('');

    // Summary
    output.push(`## Summary`);
    output.push(`- **Total Issues:** ${summary.totalIssues}`);
    output.push(`- **Critical:** ${summary.critical}`);
    output.push(`- **Errors:** ${summary.errors}`);
    output.push(`- **Warnings:** ${summary.warnings}`);
    output.push(`- **Auto-Fixable:** ${summary.autoFixable}`);
    output.push(`- **Severity Score:** ${summary.severityScore}/10`);
    output.push('');

    // Issues table
    if (issues.length > 0) {
      output.push(`## Issues`);
      output.push(`| Severity | Tool | Category | Line | Message | Fixable |`);
      output.push(`|----------|------|----------|------|---------|----------|`);

      issues.sort((a, b) => b.severity - a.severity).forEach(issue => {
        const severity = issue.getSeverityLevel().toUpperCase();
        const fixable = issue.fixable ? '✅' : '❌';
        const line = issue.line || 'N/A';
        const message = issue.message.length > 50 ? issue.message.substring(0, 47) + '...' : issue.message;

        output.push(`| ${severity} | ${issue.tool} | ${issue.category} | ${line} | ${message} | ${fixable} |`);
      });
      output.push('');
    }

    // Recommendations
    const recommendations = this.generateRecommendations(issues);
    if (recommendations.length > 0) {
      output.push(`## Recommendations`);
      recommendations.forEach((rec, index) => {
        output.push(`${index + 1}. **${rec.action}** (Priority: ${rec.priority})`);
        output.push(`   - Command: \`${rec.command}\``);
        if (rec.suggestedTool) {
          output.push(`   - Suggested Tool: \`${rec.suggestedTool}\``);
        }
        output.push('');
      });
    }

    return output.join('\n');
  }

  /**
   * Enhanced console output with better structure
   */
  static formatConsole(issues, metadata = {}, useColors = true) {
    const summary = this.generateSummary(issues);
    const output = [];

    // Header
    if (useColors) {
      output.push(`${colors.magenta}🚀 TeensyROM Quality Hook - Enhanced${colors.reset}`);
      output.push(`${colors.cyan}File: ${metadata.targetFile || 'Multiple files'}${colors.reset}`);
      output.push(`${colors.cyan}Project: ${metadata.project || 'Unknown'}${colors.reset}`);
    } else {
      output.push('TeensyROM Quality Hook - Enhanced');
      output.push(`File: ${metadata.targetFile || 'Multiple files'}`);
      output.push(`Project: ${metadata.project || 'Unknown'}`);
    }
    output.push('');

    // Summary
    if (useColors) {
      output.push(`${colors.cyan}📊 Quality Check Summary${colors.reset}`);
      output.push('='.repeat(50));

      if (summary.critical > 0) {
        output.push(`${colors.red}❌ ${summary.critical} critical issues${colors.reset}`);
      }
      if (summary.errors > 0) {
        output.push(`${colors.red}❌ ${summary.errors} errors${colors.reset}`);
      }
      if (summary.warnings > 0) {
        output.push(`${colors.yellow}⚠️  ${summary.warnings} warnings${colors.reset}`);
      }
      if (summary.autoFixable > 0) {
        output.push(`${colors.green}🔧 ${summary.autoFixable} auto-fixable${colors.reset}`);
      }

      if (summary.totalIssues === 0) {
        output.push(`${colors.green}✅ All quality checks passed!${colors.reset}`);
      }
    } else {
      output.push('Quality Check Summary');
      output.push('='.repeat(50));
      output.push(`Critical: ${summary.critical}`);
      output.push(`Errors: ${summary.errors}`);
      output.push(`Warnings: ${summary.warnings}`);
      output.push(`Auto-fixable: ${summary.autoFixable}`);
      output.push(`Total: ${summary.totalIssues}`);
    }

    // Critical issues details
    const criticalIssues = issues.filter(i => i.severity >= 8);
    if (criticalIssues.length > 0) {
      output.push('');
      output.push(useColors ? `${colors.red}🚨 Critical Issues:${colors.reset}` : 'Critical Issues:');
      criticalIssues.forEach(issue => {
        const location = issue.line ? `${issue.file}:${issue.line}` : issue.file;
        output.push(`  • ${location} - ${issue.message}`);
        if (issue.fixCommand) {
          output.push(`    Fix: ${issue.fixCommand}`);
        }
      });
    }

    return output.join('\n');
  }

  static generateSummary(issues) {
    const critical = issues.filter(i => i.severity >= 8).length;
    const errors = issues.filter(i => i.severity >= 6 && i.severity < 8).length;
    const warnings = issues.filter(i => i.severity >= 4 && i.severity < 6).length;
    const autoFixable = issues.filter(i => i.fixable).length;
    const severityScore = issues.length > 0
      ? Math.max(...issues.map(i => i.severity))
      : 0;

    return {
      totalIssues: issues.length,
      critical,
      errors,
      warnings,
      autoFixable,
      severityScore
    };
  }

  static generateRecommendations(issues) {
    const recommendations = [];

    // Group fixable issues by tool
    const fixableByTool = issues
      .filter(i => i.fixable)
      .reduce((acc, issue) => {
        if (!acc[issue.tool]) acc[issue.tool] = [];
        acc[issue.tool].push(issue);
        return acc;
      }, {});

    // Auto-fix recommendations
    Object.entries(fixableByTool).forEach(([tool, toolIssues]) => {
      const command = this.generateFixCommand(tool, toolIssues);
      recommendations.push({
        priority: 1,
        action: `Auto-fix ${tool} issues`,
        command,
        expectedResults: `${toolIssues.length} issues resolved automatically`,
        suggestedTool: tool === 'Prettier' ? 'format' : 'lint'
      });
    });

    // Manual fix recommendations
    const manualIssues = issues.filter(i => !i.fixable && i.severity >= 6);
    if (manualIssues.length > 0) {
      recommendations.push({
        priority: 2,
        action: `Fix ${manualIssues.length} manual issues`,
        command: `/clean-code ${manualIssues.map(i => i.file).join(' ')}`,
        reason: 'Requires manual intervention and understanding of code context',
        suggestedTool: 'clean-code'
      });
    }

    return recommendations.sort((a, b) => a.priority - b.priority);
  }

  static generateFixCommand(tool, issues) {
    const files = [...new Set(issues.map(i => i.file))];

    switch (tool) {
      case 'ESLint':
        return files.length === 1
          ? `npx eslint "${files[0]}" --fix`
          : `npx eslint ${files.map(f => `"${f}"`).join(' ')} --fix`;
      case 'Prettier':
        return files.length === 1
          ? `npx prettier --write "${files[0]}"`
          : `npx prettier --write ${files.map(f => `"${f}"`).join(' ')}`;
      case 'TypeScript':
        return 'TypeScript errors require manual code fixes';
      default:
        return `Manual fixes required for ${tool} issues`;
    }
  }
}

/**
 * Integrates with Claude Code workflows by suggesting tools and commands
 */
class ClaudeCodeIntegration {
  /**
   * Suggests which Claude Code tool to use for each issue
   */
  static suggestTool(issue) {
    switch (issue.category) {
      case 'type':
      case 'syntax':
        return 'clean-code';
      case 'style':
      case 'format':
        return issue.fixable ? 'format' : 'clean-code';
      case 'architecture':
        return 'clean-code';
      default:
        return 'edit';
    }
  }

  /**
   * Generates specific Claude Code commands for issues
   */
  static generateCommand(issue) {
    const tool = this.suggestTool(issue);

    switch (tool) {
      case 'clean-code':
        return `/clean-code "${issue.file}"`;
      case 'format':
        return `/format "${issue.file}"`;
      case 'edit':
        return `Edit "${issue.file}" to fix ${issue.rule || issue.message}`;
      default:
        return `Review "${issue.file}" for ${issue.category} issues`;
    }
  }

  /**
   * Calculates priority score for issue ordering
   */
  static calculatePriority(issue) {
    let priority = issue.severity;

    // Boost priority for critical files
    if (issue.architectureLayer === 'domain') priority += 2;
    if (issue.file.includes('endpoint') || issue.file.includes('service')) priority += 1;

    // Reduce priority for auto-fixable issues
    if (issue.fixable) priority -= 2;

    return Math.max(1, 11 - priority); // Invert so lower numbers = higher priority
  }

  /**
   * Enriches issues with Claude Code integration data
   */
  static enrichIssues(issues) {
    return issues.map(issue => {
      issue.suggestedTool = this.suggestTool(issue);
      if (!issue.fixCommand) {
        issue.fixCommand = this.generateCommand(issue);
      }
      issue.priority = this.calculatePriority(issue);
      return issue;
    });
  }
}

class TypeScriptConfigCache {
  constructor(workspaceRoot) {
    this.workspaceRoot = workspaceRoot;
    this.cacheFile = join(__dirname, 'quality-check-cache.json');
    this.cache = this.loadCache();

    // Initialize cache structure if empty
    if (!this.cache.hashes) {
      this.cache.hashes = {};
    }
    if (!this.cache.mappings) {
      this.cache.mappings = {};
    }

    // Build cache if invalid
    if (!this.isValid()) {
      this.buildCache();
    }
  }

  loadCache() {
    try {
      if (existsSync(this.cacheFile)) {
        return JSON.parse(readFileSync(this.cacheFile, 'utf8'));
      }
    } catch {
      console.warn(`${colors.yellow}Warning: Could not load TypeScript config cache${colors.reset}`);
    }
    return { hashes: {}, mappings: {} };
  }

  saveCache() {
    try {
      writeFileSync(this.cacheFile, JSON.stringify(this.cache, null, 2));
    } catch {
      console.warn(`${colors.yellow}Warning: Could not save TypeScript config cache${colors.reset}`);
    }
  }

  getConfigHash(configPath) {
    try {
      const content = readFileSync(configPath, 'utf8');
      return createHash('sha256').update(content).digest('hex');
    } catch {
      return null;
    }
  }

  isValid() {
    const configFiles = this.findTsConfigFiles();

    // Check if same number of configs
    if (Object.keys(this.cache.hashes).length !== configFiles.length) {
      return false;
    }

    // Validate each config hash
    for (const configPath of configFiles) {
      const currentHash = this.getConfigHash(configPath);
      if (currentHash !== this.cache.hashes[configPath]) {
        return false;
      }
    }
    return true;
  }

  findTsConfigFiles() {
    const { execSync } = require('child_process');
    try {
      // Find all tsconfig*.json files in workspace
      const files = execSync('find . -name "tsconfig*.json" -not -path "./node_modules/*" -not -path "./dist/*" -not -path "./.nx/*"', {
        cwd: this.workspaceRoot,
        encoding: 'utf8'
      }).trim().split('\n').filter(Boolean);

      return files.map(file => join(this.workspaceRoot, file));
    } catch {
      // Fallback to common configs
      const fallbackConfigs = [
        'tsconfig.json',
        'tsconfig.base.json',
        'tsconfig.app.json',
        'tsconfig.lib.json',
        'tsconfig.spec.json',
        'tsconfig.e2e.json'
      ];

      return fallbackConfigs
        .map(config => join(this.workspaceRoot, config))
        .filter(existsSync);
    }
  }

  buildCache() {
    const configFiles = this.findTsConfigFiles();

    // Clear existing cache
    this.cache.hashes = {};
    this.cache.mappings = {};

    // Build hashes and mappings for each config
    for (const configPath of configFiles) {
      const hash = this.getConfigHash(configPath);
      if (hash) {
        this.cache.hashes[configPath] = hash;

        // Determine patterns for this config based on location and name
        const patterns = this.getPatternsForConfig(configPath);
        for (const pattern of patterns) {
          this.cache.mappings[pattern] = {
            configPath,
            excludes: ['node_modules/**', 'dist/**', '.nx/**', 'coverage/**']
          };
        }
      }
    }

    this.saveCache();
  }

  getPatternsForConfig(configPath) {
    const relativePath = configPath.replace(this.workspaceRoot, '').replace(/\\/g, '/');
    const patterns = [];

    // Nx-specific pattern mapping
    if (relativePath.includes('/apps/')) {
      const appName = relativePath.match(/\/apps\/([^\/]+)/)?.[1];
      if (appName) {
        if (configPath.includes('tsconfig.e2e.json')) {
          patterns.push(`apps/${appName}/**/*.cy.ts`);
          patterns.push(`apps/${appName}/**/*.e2e.ts`);
        } else if (configPath.includes('tsconfig.spec.json')) {
          patterns.push(`apps/${appName}/**/*.spec.ts`);
        } else {
          patterns.push(`apps/${appName}/**/*.ts`);
          patterns.push(`apps/${appName}/**/*.tsx`);
        }
      }
    } else if (relativePath.includes('/libs/')) {
      const libPath = relativePath.match(/\/libs\/([^\/]+)/)?.[1];
      if (libPath) {
        if (configPath.includes('tsconfig.spec.json')) {
          patterns.push(`libs/${libPath}/**/*.spec.ts`);
        } else {
          patterns.push(`libs/${libPath}/**/*.ts`);
          patterns.push(`libs/${libPath}/**/*.tsx`);
        }
      }
    } else {
      // Root level configs
      if (configPath.includes('tsconfig.base.json')) {
        patterns.push('**/*.ts');
        patterns.push('**/*.tsx');
      } else if (configPath.includes('tsconfig.json')) {
        patterns.push('*.ts');
        patterns.push('*.tsx');
      }
    }

    return patterns.length > 0 ? patterns : ['**/*.ts'];
  }

  matchesPattern(filePath, pattern) {
    // Normalize paths
    const normalizedPath = filePath.replace(/\\/g, '/');

    // Special handling for **/* patterns
    if (pattern.endsWith('/**/*')) {
      const baseDir = pattern.slice(0, -5);
      return normalizedPath.includes(baseDir);
    }

    // Convert glob patterns to regex
    let regexPattern = pattern
      .replace(/[.+^${}()|[\]\\]/g, '\\$&') // Escape regex chars
      .replace(/\*\*/g, '🌟') // Temporary placeholder
      .replace(/\*/g, '[^/]*') // * matches except /
      .replace(/🌟/g, '.*') // ** matches anything
      .replace(/\?/g, '.'); // ? matches single char

    const regex = new RegExp(`^${regexPattern}$`);
    return regex.test(normalizedPath);
  }

  findTsconfigForFile(filePath) {
    // Normalize file path relative to workspace
    const relativePath = filePath.replace(this.workspaceRoot, '').replace(/\\/g, '');

    // Fast heuristics for common file types
    if (relativePath.includes('/e2e/') || relativePath.endsWith('.cy.ts')) {
      // Look for E2E configs first
      const e2eConfig = this.findConfigByPattern(relativePath, 'tsconfig.e2e.json');
      if (e2eConfig) return e2eConfig;
    }

    if (relativePath.endsWith('.spec.ts') || relativePath.endsWith('.test.ts')) {
      // Look for test configs first
      const testConfig = this.findConfigByPattern(relativePath, 'tsconfig.spec.json');
      if (testConfig) return testConfig;
    }

    // Find best matching pattern with specificity scoring
    let bestMatch = null;
    let bestScore = -1;

    for (const [pattern, mapping] of Object.entries(this.cache.mappings)) {
      if (this.matchesPattern(relativePath, pattern)) {
        // Calculate specificity score
        const specificity = this.calculateSpecificity(pattern);
        if (specificity > bestScore) {
          bestScore = specificity;
          bestMatch = mapping.configPath;
        }
      }
    }

    // Fallback to base config if no match found
    if (!bestMatch) {
      const baseConfig = join(this.workspaceRoot, 'tsconfig.base.json');
      if (existsSync(baseConfig)) {
        bestMatch = baseConfig;
      } else {
        const mainConfig = join(this.workspaceRoot, 'tsconfig.json');
        if (existsSync(mainConfig)) {
          bestMatch = mainConfig;
        }
      }
    }

    return bestMatch;
  }

  findConfigByPattern(filePath, configPattern) {
    for (const [pattern, mapping] of Object.entries(this.cache.mappings)) {
      if (mapping.configPath.includes(configPattern) && this.matchesPattern(filePath, pattern)) {
        return mapping.configPath;
      }
    }
    return null;
  }

  calculateSpecificity(pattern) {
    // Higher score for more specific patterns
    let score = pattern.split('/').length;

    // Bonus for patterns without wildcards
    if (!pattern.includes('*')) {
      score += 10;
    }

    // Penalty for patterns with **
    if (pattern.includes('**')) {
      score -= 5;
    }

    return score;
  }
}

/**
 * Enhanced quality checker with structured issue parsing and AI-friendly output
 */
class QualityChecker {
  constructor(workspaceRoot, targetFile, options = {}) {
    this.workspaceRoot = workspaceRoot;
    this.targetFile = targetFile;
    this.tsCache = new TypeScriptConfigCache(workspaceRoot);
    this.issues = [];
    this.options = {
      outputFormat: options.outputFormat || 'console', // 'console', 'json', 'markdown'
      verbose: options.verbose || false,
      includeRecommendations: options.includeRecommendations !== false
    };
  }

  /**
   * Validates a file and returns structured results
   */
  async validateFile(filePath) {
    if (!this.shouldValidateFile(filePath)) {
      return this.formatOutput([], { targetFile: filePath });
    }

    // Determine file metadata for context
    const metadata = await this.gatherFileMetadata(filePath);

    try {
      // Run all checks and collect issues - don't fail fast
      await this.runTypeScriptCheck(filePath);
    } catch (error) {
      // Create structured issue for failed TypeScript execution
      const issue = new QualityIssue(
        9, // High severity for failed check execution
        'System',
        'execution',
        filePath,
        null,
        null,
        `TypeScript check execution failed: ${error.message}`,
        'typescript-execution-failure',
        false
      );
      this.issues.push(issue);
    }

    // Always continue with ESLint check even if TypeScript had issues
    try {
      await this.runESLintCheck(filePath);
    } catch (eslintError) {
      // If ESLint itself fails, create an execution issue but continue
      const issue = new QualityIssue(
        9, // High severity for failed ESLint execution
        'System',
        'execution',
        filePath,
        null,
        null,
        `ESLint check execution failed: ${eslintError.message}`,
        'eslint-execution-failure',
        false
      );
      this.issues.push(issue);
    }

    // Always continue with Prettier check even if other checks had issues
    try {
      await this.runPrettierCheck(filePath);
    } catch (prettierError) {
      // If Prettier itself fails, create an execution issue but continue
      const issue = new QualityIssue(
        9, // High severity for failed Prettier execution
        'System',
        'execution',
        filePath,
        null,
        null,
        `Prettier check execution failed: ${prettierError.message}`,
        'prettier-execution-failure',
        false
      );
      this.issues.push(issue);
    }

    // Enrich issues with Claude Code integration data
    ClaudeCodeIntegration.enrichIssues(this.issues);

    // Add project context to all issues
    this.issues.forEach(issue => {
      issue.setProjectContext(metadata.project, metadata.architectureLayer);
    });

    return this.formatOutput(this.issues, { ...metadata, targetFile: filePath });
  }

  /**
   * Gathers metadata about the file for context-aware analysis
   */
  async gatherFileMetadata(filePath) {
    const projectName = this.findProjectForFile(filePath);
    const architectureLayer = this.determineArchitectureLayer(filePath);
    const fileType = this.getFileType(filePath);

    return {
      project: projectName,
      projectType: this.getProjectType(projectName),
      architectureLayer,
      fileType,
      dependencies: [], // Could be enhanced with dependency analysis
      changeImpact: this.assessChangeImpact(filePath, architectureLayer)
    };
  }

  determineArchitectureLayer(filePath) {
    const normalizedPath = filePath.replace(/\\/g, '/');

    if (normalizedPath.includes('/domain/') || normalizedPath.includes('\\domain\\')) return 'domain';
    if (normalizedPath.includes('/application/') || normalizedPath.includes('\\application\\')) return 'application';
    if (normalizedPath.includes('/infrastructure/') || normalizedPath.includes('\\infrastructure\\')) return 'infrastructure';
    if (normalizedPath.includes('/features/') || normalizedPath.includes('\\features\\')) return 'feature';
    if (normalizedPath.includes('/shared/') || normalizedPath.includes('\\shared\\')) return 'shared';

    return 'unknown';
  }

  getProjectType(projectName) {
    if (!projectName) return 'unknown';
    if (projectName.includes('-e2e')) return 'e2e';
    if (projectName.includes('-ui')) return 'application';
    if (projectName.includes('domain')) return 'domain';
    if (projectName.includes('data-access')) return 'infrastructure';
    return 'library';
  }

  getFileType(filePath) {
    const normalizedPath = filePath.toLowerCase();
    if (normalizedPath.includes('.component.')) return 'component';
    if (normalizedPath.includes('.service.') || normalizedPath.includes('.service.')) return 'service';
    if (normalizedPath.includes('.model.') || normalizedPath.includes('.interface.') || normalizedPath.includes('.types.')) return 'type-definition';
    if (normalizedPath.includes('.endpoint.')) return 'endpoint';
    if (normalizedPath.includes('.interceptor.')) return 'interceptor';
    if (normalizedPath.includes('.spec.') || normalizedPath.includes('.test.')) return 'test';
    return 'utility';
  }

  assessChangeImpact(filePath, architectureLayer) {
    const fileType = this.getFileType(filePath);

    // Critical files that have high impact
    if (fileType === 'endpoint' || fileType === 'service') return 'high';
    if (architectureLayer === 'domain') return 'high';

    // Medium impact files
    if (fileType === 'component' || fileType === 'type-definition') return 'medium';

    return 'low';
  }

  shouldValidateFile(filePath) {
    if (!existsSync(filePath)) {
      return false;
    }

    const fileName = filePath.toLowerCase();

    // Exclude directories and patterns
    if (fileName.includes('node_modules') || fileName.includes('dist/') ||
        fileName.includes('build/') || fileName.includes('.nx/') ||
        fileName.includes('coverage/')) {
      return false;
    }

    // Include relevant file types
    return fileName.endsWith('.ts') || fileName.endsWith('.tsx') ||
           fileName.endsWith('.js') || fileName.endsWith('.jsx');
  }

  /**
   * Enhanced TypeScript check with structured error parsing
   */
  async runTypeScriptCheck(filePath) {
    try {
      const projectName = this.findProjectForFile(filePath);
      let cmd, context;

      if (projectName) {
        cmd = `npx nx typecheck ${projectName} --noEmit`;
        context = `project-aware (${projectName})`;
      } else {
        cmd = `npx tsc --noEmit "${filePath}"`;
        context = 'global fallback';
      }

      if (this.options.verbose) {
        console.log(`${colors.cyan}🔍 Running TypeScript check (${context})${colors.reset}`);
      }

      execSync(cmd, {
        cwd: this.workspaceRoot,
        encoding: 'utf8',
        stdio: 'pipe'
      });

      // No errors found
      return;

    } catch (error) {
      const output = error.stdout || error.message;

      if (this.options.verbose) {
        console.log(`${colors.yellow}🔍 TypeScript output captured:${colors.reset}`);
        console.log(output);
        console.log('');
      }

      if (this.hasCriticalTypeScriptErrors(output)) {
        this.parseTypeScriptErrors(output, filePath);
        // Continue to other checks - we want to collect all issues, not fail fast
      }

      // Return without throwing - we want to collect all issues, not fail fast
      return;
    }
  }

  /**
   * Parses TypeScript error output into structured QualityIssue objects
   */
  parseTypeScriptErrors(output, targetFile) {
    const lines = output.split('\n');
    let currentError = null;

    for (let i = 0; i < lines.length; i++) {
      const line = lines[i].trim();

      // TypeScript error format: file(line,column): error TScode: message
      const tsErrorMatch = line.match(/^(.+)\((\d+),(\d+)\):\s*error\s+TS(\d+):\s*(.+)$/);

      if (tsErrorMatch) {
        const [, file, lineNum, colNum, tsCode, message] = tsErrorMatch;

        // Only include errors for our target file or filter if needed
        if (!targetFile || file.includes(targetFile) || this.isTargetFileRelevant(file, targetFile)) {
          currentError = new QualityIssue(
            this.getTypeScriptSeverity(tsCode, message),
            'TypeScript',
            this.getTypeScriptCategory(tsCode, message),
            file,
            parseInt(lineNum),
            parseInt(colNum),
            message.trim(),
            `TS${tsCode}`,
            this.isTypeScriptFixable()
          );

          this.issues.push(currentError);
        }
      } else if (currentError && line.startsWith('Overload')) {
        // This is a continuation of the previous error (overload details)
        if (currentError.message) {
          currentError.message += ' ' + line;
        }
      } else if (currentError && (line.startsWith('Argument of type') || line.startsWith('Type'))) {
        // This is more detail about the error
        if (currentError.message) {
          currentError.message += ' ' + line;
        }
      } else if (line && !line.startsWith('>') && !line.includes('nx run') && !line.includes('tsc --noEmit')) {
        // Reset if we encounter a non-error line
        currentError = null;
      }
    }

    // If no specific errors were found but we have a compilation failure, create a general issue
    if (this.issues.length === 0 && output.includes('error TS')) {
      const issue = new QualityIssue(
        8, // High severity for general TypeScript failures
        'TypeScript',
        'compilation',
        targetFile,
        null,
        null,
        'TypeScript compilation failed - check output for specific errors',
        'compilation-error',
        false
      );

      this.issues.push(issue);
    }
  }

  /**
   * Checks if a file error is relevant to our target file
   */
  isTargetFileRelevant(errorFile, targetFile) {
    if (!targetFile) return true;

    // If the error file is the target file, it's relevant
    if (errorFile === targetFile) return true;

    // If the target file is in the error file's directory, it might be relevant
    const targetDir = targetFile.substring(0, targetFile.lastIndexOf('/'));
    const errorDir = errorFile.substring(0, errorFile.lastIndexOf('/'));

    return targetDir === errorDir || targetFile.includes(errorDir);
  }

  getTypeScriptSeverity(tsCode, message) {
    // Critical TypeScript errors
    if (tsCode === '2339' || tsCode === '2304' || message.includes('Cannot find name')) return 9;
    if (tsCode === '2322' || message.includes('not assignable')) return 8;
    if (tsCode === '2769') return 8; // Overload errors like cy.intercept

    // Type-related errors
    if (tsCode.startsWith('25') || tsCode.startsWith('23')) return 7;

    return 6;
  }

  getTypeScriptCategory(tsCode, message) {
    if (message.includes('type') || message.includes('assignable')) return 'type';
    if (message.includes('Cannot find name') || message.includes('find module')) return 'syntax';
    if (message.includes('overload')) return 'syntax';
    return 'type';
  }

  isTypeScriptFixable() {
    // Most TypeScript errors require manual fixes
    return false;
  }

  /**
   * Enhanced ESLint check with structured error parsing
   */
  async runESLintCheck(filePath) {
    if (this.options.verbose) {
      console.log(`${colors.cyan}🔍 Running ESLint check (issues flagged as critical)${colors.reset}`);
    }

    // Use direct ESLint on the specific file for more reliable results
    const checkCmd = `npx eslint "${filePath}" --format=json`;

    let output = '';
    let hasOutput = false;

    try {
      // Try to execute ESLint and capture output, even if it fails with non-zero exit code
      output = execSync(checkCmd, {
        cwd: this.workspaceRoot,
        encoding: 'utf8',
        stdio: 'pipe'
      });
      hasOutput = true;

    } catch (error) {
      // ESLint found issues and returned non-zero exit code - capture the output
      output = error.stdout || error.stderr || error.message || '';
      hasOutput = true;

      if (this.options.verbose) {
        console.log(`${colors.yellow}⚠️  ESLint command failed with non-zero exit code (this indicates found issues)${colors.reset}`);
      }
    }

    if (hasOutput && output) {
      // Strip ANSI color codes from ESLint output for parsing
      const cleanOutput = output.replace(/\x1b\[[0-9;]*m/g, '');

      // Try to parse as JSON first
      if (cleanOutput.trim().startsWith('[')) {
        try {
          const eslintResults = JSON.parse(cleanOutput);
          this.parseESLintJsonResults(eslintResults);
        } catch (parseError) {
          // If JSON parsing fails, run again in text format
          this.runESLintTextCheck(filePath);
        }
      } else {
        // Parse as text format
        this.parseESLintTextOutput(cleanOutput);
      }
    }

    // Do NOT auto-fix ESLint issues - just flag them as critical problems
    // The purpose is to identify and report issues, not automatically fix them
    if (this.options.verbose) {
      console.log(`${colors.yellow}⚠️  ESLint issues flagged as critical (auto-fix disabled)${colors.reset}`);
    }
  }

  /**
   * Fallback method to run ESLint in text format
   */
  runESLintTextCheck(filePath) {
    try {
      const textCmd = `npx eslint "${filePath}"`;
      let output = '';

      try {
        output = execSync(textCmd, {
          cwd: this.workspaceRoot,
          encoding: 'utf8',
          stdio: 'pipe'
        });
      } catch (error) {
        output = error.stdout || error.stderr || error.message || '';
      }

      if (output) {
        const cleanOutput = output.replace(/\x1b\[[0-9;]*m/g, '');
        this.parseESLintTextOutput(cleanOutput);
      }
    } catch (error) {
      if (this.options.verbose) {
        console.log(`${colors.red}❌ Failed to run ESLint text check fallback${colors.reset}`);
      }
    }
  }

  /**
   * Parses ESLint JSON results into structured QualityIssue objects
   */
  parseESLintJsonResults(eslintResults) {
    eslintResults.forEach(result => {
      const filePath = result.filePath;

      result.messages.forEach(message => {
        // Treat all ESLint issues as critical (severity 8-9)
        // No tolerance for any ESLint errors or warnings in a quality codebase
        let issueSeverity = 8; // Base critical severity for all ESLint issues

        // Boost severity for particularly problematic rules
        if (message.ruleId.includes('no-explicit-any') ||
            message.ruleId.includes('no-unused-vars') ||
            message.ruleId.includes('no-undef') ||
            message.ruleId.includes('typescript-eslint')) {
          issueSeverity = 9; // Highest severity for TypeScript-related ESLint issues
        }

        const issue = new QualityIssue(
          issueSeverity,
          'ESLint',
          this.getESLintCategory(message.ruleId),
          filePath,
          message.line || null,
          message.column || null,
          message.message,
          message.ruleId || 'unknown',
          this.isESLintFixable(message.ruleId)
        );

        // Set fix details for ESLint issues
        if (issue.fixable) {
          issue.setFixDetails(
            `npx eslint "${filePath}" --fix`,
            `Run ESLint auto-fix for ${message.ruleId} violation`,
            'lint'
          );
        }

        this.issues.push(issue);
      });
    });
  }

  /**
   * Parses ESLint text output into structured QualityIssue objects
   */
  parseESLintTextOutput(output) {
    const lines = output.split('\n');

    lines.forEach(line => {
      // ESLint error format: file:line:column: severity message [rule]
      const eslintErrorMatch = line.match(/^(.+):(\d+):(\d+):\s*(error|warning)\s*(.+?)\s*([^\s]+)$/);

      if (eslintErrorMatch) {
        const [, file, lineNum, colNum, severity, message, rule] = eslintErrorMatch;

        // Treat all ESLint issues as critical (severity 8-9)
        // No tolerance for any ESLint errors or warnings in a quality codebase
        let issueSeverity = 8; // Base critical severity for all ESLint issues

        // Boost severity for particularly problematic rules
        if (rule.includes('no-explicit-any') ||
            rule.includes('no-unused-vars') ||
            rule.includes('no-undef') ||
            rule.includes('typescript-eslint')) {
          issueSeverity = 9; // Highest severity for TypeScript-related ESLint issues
        }

        const issue = new QualityIssue(
          issueSeverity,
          'ESLint',
          this.getESLintCategory(rule),
          file,
          parseInt(lineNum),
          parseInt(colNum),
          message.trim(),
          rule.replace(/[[\]]/g, ''),
          this.isESLintFixable(rule)
        );

        // Set fix details for ESLint issues
        if (issue.fixable) {
          issue.setFixDetails(
            `npx eslint "${file}" --fix`,
            `Run ESLint auto-fix for ${rule} violation`,
            'lint'
          );
        }

        this.issues.push(issue);
      }
    });
  }

  getESLintCategory(rule) {
    if (rule.includes('typescript') || rule.includes('type')) return 'type';
    if (rule.includes('import') || rule.includes('module')) return 'architecture';
    if (rule.includes('format') || rule.includes('style')) return 'style';
    return 'style';
  }

  isESLintFixable(rule) {
    return TEENSYROM_CONFIG.autoFixableRules.some(autoFixRule =>
      rule.includes(autoFixRule)
    );
  }

  /**
   * Enhanced Prettier check
   */
  async runPrettierCheck(filePath) {
    try {
      const checkCmd = `npx prettier --check "${filePath}"`;
      execSync(checkCmd, {
        cwd: this.workspaceRoot,
        encoding: 'utf8',
        stdio: 'pipe'
      });

    } catch {
      // Prettier issues are fixable
      const issue = new QualityIssue(
        3, // Low severity for formatting
        'Prettier',
        'format',
        filePath,
        null,
        null,
        'Code formatting issues detected',
        'format',
        true
      );

      issue.setFixDetails(
        `npx prettier --write "${filePath}"`,
        'Format code with Prettier',
        'format'
      );

      this.issues.push(issue);

      // Auto-format
      try {
        const formatCmd = `npx prettier --write "${filePath}"`;
        execSync(formatCmd, {
          cwd: this.workspaceRoot,
          encoding: 'utf8',
          stdio: 'pipe'
        });
      } catch {
        // Formatting failed - keep the issue
      }
    }
  }

  findProjectForFile(filePath) {
    try {
      const cmd = `npx nx show project --file="${filePath}"`;
      const output = execSync(cmd, {
        cwd: this.workspaceRoot,
        encoding: 'utf8',
        stdio: 'pipe'
      }).trim();

      return output || null;
    } catch {
      return this.inferProjectFromPath(filePath);
    }
  }

  inferProjectFromPath(filePath) {
    const normalizedPath = filePath.replace(/\\/g, '/');

    if (normalizedPath.startsWith('apps/')) {
      const match = normalizedPath.match(/^apps\/([^/]+)/);
      return match ? match[1] : null;
    }

    if (normalizedPath.startsWith('libs/')) {
      const match = normalizedPath.match(/^libs\/([^/]+)/);
      return match ? match[1] : null;
    }

    return null;
  }

  hasCriticalTypeScriptErrors(output) {
    return TEENSYROM_CONFIG.criticalTypeErrors.some(pattern =>
      output.includes(pattern)
    );
  }

  /**
   * Formats output using the specified format
   */
  formatOutput(issues, metadata) {
    switch (this.options.outputFormat) {
      case 'json':
        return OutputFormatter.formatJson(issues, metadata);
      case 'markdown':
        return OutputFormatter.formatMarkdown(issues, metadata);
      case 'console':
      default:
        return OutputFormatter.formatConsole(issues, metadata, true);
    }
  }

  hasCriticalIssues() {
    return this.issues.some(issue => issue.severity >= 8);
  }

  getSummary() {
    return OutputFormatter.generateSummary(this.issues);
  }
}

/**
 * Parse command line arguments
 */
function parseArguments(args) {
  const options = {
    targetFiles: [],
    outputFormat: 'console',
    verbose: false,
    includeRecommendations: true,
    help: false
  };

  for (let i = 0; i < args.length; i++) {
    const arg = args[i];

    switch (arg) {
      case '--json':
        options.outputFormat = 'json';
        break;
      case '--markdown':
      case '--md':
        options.outputFormat = 'markdown';
        break;
      case '--console':
        options.outputFormat = 'console';
        break;
      case '--verbose':
      case '-v':
        options.verbose = true;
        break;
      case '--no-recommendations':
        options.includeRecommendations = false;
        break;
      case '--help':
      case '-h':
        options.help = true;
        break;
      default:
        if (!arg.startsWith('-')) {
          options.targetFiles.push(arg);
        }
        break;
    }
  }

  return options;
}

/**
 * Display help information
 */
function displayHelp() {
  console.log(`${colors.cyan}TeensyROM Quality Hook - Enhanced${colors.reset}`);
  console.log('');
  console.log(`${colors.yellow}Usage:${colors.reset}`);
  console.log('  quality-check.cjs [options] [file-path] [file-path2] [directory-pattern]');
  console.log('');
  console.log(`${colors.yellow}Options:${colors.reset}`);
  console.log('  --json                 Output structured JSON for AI consumption');
  console.log('  --markdown, --md       Output formatted markdown');
  console.log('  --console              Output enhanced console format (default)');
  console.log('  --verbose, -v          Enable verbose logging');
  console.log('  --no-recommendations   Skip AI recommendations');
  console.log('  --help, -h             Show this help message');
  console.log('');
  console.log(`${colors.yellow}Examples:${colors.reset}`);
  console.log('  quality-check.cjs --json src/app.component.ts');
  console.log('  quality-check.cjs --json "file1.ts" "file2.ts" "file3.ts"');
  console.log('  quality-check.cjs --verbose --markdown "libs/domain/**/*.ts"');
  console.log('  quality-check.cjs --no-recommendations src/types.ts');
  console.log('');
  console.log(`${colors.yellow}Output Formats:${colors.reset}`);
  console.log('  - JSON: Structured data for Claude Code agents');
  console.log('  - Markdown: Human-readable reports');
  console.log('  - Console: Enhanced terminal output with colors');
}

/**
 * Expands file patterns and resolves multiple files
 */
function expandFilePatterns(patterns, workspaceRoot, verbose = false) {
  const { execSync } = require('child_process');
  const allFiles = new Set();

  for (const pattern of patterns) {
    try {
      // Check if it's a glob pattern (contains * or ?)
      if (pattern.includes('*') || pattern.includes('?')) {
        // Use find to expand glob patterns
        const files = execSync(`find . -name "*.ts" -not -path "./node_modules/*" -not -path "./dist/*" -not -path "./.nx/*" | grep -E "${pattern.replace(/^\.\//, '').replace(/\//g, '/')}"`, {
          cwd: workspaceRoot,
          encoding: 'utf8',
          stdio: 'pipe'
        }).trim().split('\n').filter(Boolean);

        files.forEach(file => allFiles.add(file));
      } else {
        // Direct file path - check if it exists
        try {
          execSync(`test -f "${pattern}"`, { cwd: workspaceRoot, stdio: 'pipe' });
          allFiles.add(pattern);
        } catch {
          // File doesn't exist, skip it
          if (verbose) {
            console.warn(`${colors.yellow}Warning: File not found: ${pattern}${colors.reset}`);
          }
        }
      }
    } catch (error) {
      console.warn(`${colors.yellow}Warning: Could not process pattern: ${pattern} - ${error.message}${colors.reset}`);
    }
  }

  return Array.from(allFiles);
}

/**
 * Generates consolidated recommendations for multiple files
 */
function generateConsolidatedRecommendations(allIssues) {
  const recommendations = [];

  // Group fixable issues by tool
  const fixableByTool = allIssues
    .filter(i => i.fixable)
    .reduce((acc, issue) => {
      if (!acc[issue.tool]) acc[issue.tool] = [];
      acc[issue.tool].push(issue);
      return acc;
    }, {});

  // Auto-fix recommendations
  Object.entries(fixableByTool).forEach(([tool, toolIssues]) => {
    const files = [...new Set(toolIssues.map(i => i.file))];
    const command = tool === 'Prettier'
      ? `npx prettier --write ${files.map(f => `"${f}"`).join(' ')}`
      : `npx eslint ${files.map(f => `"${f}"`).join(' ')} --fix`;

    recommendations.push({
      priority: 1,
      action: `Auto-fix ${tool} issues across ${files.length} files`,
      command,
      expectedResults: `${toolIssues.length} issues resolved automatically`,
      suggestedTool: tool === 'Prettier' ? 'format' : 'lint'
    });
  });

  // Manual fix recommendations for critical issues
  const criticalIssues = allIssues.filter(i => i.severity >= 8 && !i.fixable);
  if (criticalIssues.length > 0) {
    const files = [...new Set(criticalIssues.map(i => i.file))];
    recommendations.push({
      priority: 2,
      action: `Fix ${criticalIssues.length} critical issues across ${files.length} files`,
      command: `/clean-code ${files.map(f => `"${f}"`).join(' ')}`,
      reason: 'Critical TypeScript and ESLint issues require manual intervention',
      suggestedTool: 'clean-code'
    });
  }

  return recommendations.sort((a, b) => a.priority - b.priority);
}

/**
 * Formats consolidated result as markdown
 */
function formatConsolidatedMarkdown(result) {
  const output = [];

  // Header
  output.push(`# Quality Check Results - Multiple Files`);
  output.push(`**Timestamp:** ${result.timestamp}`);
  output.push(`**Total Files:** ${result.summary.totalFiles}`);
  output.push(`**Files with Issues:** ${result.summary.filesWithIssues}`);
  output.push('');

  // Summary
  output.push(`## Summary`);
  output.push(`- **Total Issues:** ${result.summary.totalIssues}`);
  output.push(`- **Critical:** ${result.summary.critical}`);
  output.push(`- **Errors:** ${result.summary.errors}`);
  output.push(`- **Warnings:** ${result.summary.warnings}`);
  output.push(`- **Auto-Fixable:** ${result.summary.autoFixable}`);
  output.push(`- **Severity Score:** ${result.summary.severityScore}/10`);
  output.push('');

  // Files with issues
  if (result.files.length > 0) {
    output.push(`## Files Analysis`);

    result.files.forEach(file => {
      if (file.summary.totalIssues > 0) {
        output.push(`### ${file.file}`);
        output.push(`- **Issues:** ${file.summary.totalIssues} (Critical: ${file.summary.critical})`);
        output.push(`- **Project:** ${file.project || 'Unknown'}`);
        output.push('');
      }
    });
  }

  // Issues summary
  if (result.issues.length > 0) {
    output.push(`## Issues Summary`);
    output.push(`| Severity | Tool | File | Message |`);
    output.push(`|----------|------|------|---------|`);

    result.issues
      .sort((a, b) => b.severity - a.severity)
      .slice(0, 20) // Limit to top 20 issues for readability
      .forEach(issue => {
        const severity = issue.severity >= 8 ? '🔴 Critical' : issue.severity >= 6 ? '🟡 Error' : '🟠 Warning';
        const file = issue.file.length > 30 ? issue.file.substring(0, 27) + '...' : issue.file;
        const message = issue.message.length > 40 ? issue.message.substring(0, 37) + '...' : issue.message;

        output.push(`| ${severity} | ${issue.tool} | ${file} | ${message} |`);
      });
    output.push('');
  }

  // Recommendations
  if (result.recommendations.length > 0) {
    output.push(`## Recommendations`);
    result.recommendations.forEach((rec, index) => {
      output.push(`${index + 1}. **${rec.action}** (Priority: ${rec.priority})`);
      output.push(`   - Command: \`${rec.command}\``);
      if (rec.reason) {
        output.push(`   - Reason: ${rec.reason}`);
      }
      if (rec.suggestedTool) {
        output.push(`   - Suggested Tool: \`${rec.suggestedTool}\``);
      }
      output.push('');
    });
  }

  return output.join('\n');
}

/**
 * Formats consolidated result for console output
 */
function formatConsolidatedConsole(result, verbose = false) {
  const output = [];

  // Header
  output.push(`${colors.magenta}🚀 TeensyROM Quality Hook - Multi-File Analysis${colors.reset}`);
  output.push(`${colors.cyan}Files Analyzed: ${result.summary.totalFiles}${colors.reset}`);
  output.push(`${colors.cyan}Files with Issues: ${result.summary.filesWithIssues}${colors.reset}`);
  output.push(`${colors.cyan}Timestamp: ${result.timestamp}${colors.reset}`);
  output.push('');

  // Summary
  output.push(`${colors.cyan}📊 Quality Check Summary${colors.reset}`);
  output.push('='.repeat(50));

  if (result.summary.critical > 0) {
    output.push(`${colors.red}❌ ${result.summary.critical} critical issues${colors.reset}`);
  }
  if (result.summary.errors > 0) {
    output.push(`${colors.red}❌ ${result.summary.errors} errors${colors.reset}`);
  }
  if (result.summary.warnings > 0) {
    output.push(`${colors.yellow}⚠️  ${result.summary.warnings} warnings${colors.reset}`);
  }
  if (result.summary.autoFixable > 0) {
    output.push(`${colors.green}🔧 ${result.summary.autoFixable} auto-fixable${colors.reset}`);
  }

  if (result.summary.totalIssues === 0) {
    output.push(`${colors.green}✅ All quality checks passed!${colors.reset}`);
  }

  // Files with issues
  if (verbose && result.files.length > 0) {
    output.push('');
    output.push(`${colors.cyan}📁 Files with Issues:${colors.reset}`);

    result.files.forEach(file => {
      if (file.summary.totalIssues > 0) {
        output.push(`  ${colors.yellow}• ${file.file}${colors.reset} (${file.summary.totalIssues} issues, ${file.summary.critical} critical)`);
      }
    });
  }

  // Critical issues details
  const criticalIssues = result.issues.filter(i => i.severity >= 8);
  if (criticalIssues.length > 0) {
    output.push('');
    output.push(`${colors.red}🚨 Critical Issues (Top 10):${colors.reset}`);
    criticalIssues
      .slice(0, 10) // Limit to top 10 for readability
      .forEach(issue => {
        const location = issue.line ? `${issue.file}:${issue.line}` : issue.file;
        output.push(`  • ${location} - ${issue.message}`);
      });
  }

  return output.join('\n');
}

// Main execution
async function main() {
  const args = process.argv.slice(2);
  const options = parseArguments(args);
  const workspaceRoot = process.cwd();

  // Show help if requested
  if (options.help) {
    displayHelp();
    process.exit(0);
    return;
  }

  try {
    if (options.targetFiles.length === 0) {
      if (options.outputFormat === 'console') {
        console.log(`${colors.yellow}⚠️  No target files specified${colors.reset}`);
        console.log(`${colors.cyan}Use --help for usage information${colors.reset}`);
      }
      process.exit(0);
      return;
    }

    // Expand file patterns and resolve actual files
    const filesToCheck = expandFilePatterns(options.targetFiles, workspaceRoot, options.verbose);

    if (filesToCheck.length === 0) {
      if (options.outputFormat === 'console') {
        console.log(`${colors.yellow}⚠️  No matching files found${colors.reset}`);
      }
      process.exit(0);
      return;
    }

    if (options.verbose) {
      console.log(`${colors.cyan}🔍 Checking ${filesToCheck.length} files...${colors.reset}`);
    }

    // Collect all issues from all files
    const allIssues = [];
    const allFileResults = [];
    let hasCriticalIssues = false;

    for (const filePath of filesToCheck) {
      if (options.verbose) {
        console.log(`${colors.cyan}  Checking: ${filePath}${colors.reset}`);
      }

      // Create quality checker for each file
      const checker = new QualityChecker(workspaceRoot, filePath, {
        outputFormat: 'json', // Always use JSON internally for consistent parsing
        verbose: options.verbose,
        includeRecommendations: options.includeRecommendations
      });

      try {
        const output = await checker.validateFile(filePath);
        const result = JSON.parse(output);

        allFileResults.push(result);
        allIssues.push(...result.issues);

        if (result.summary.critical > 0) {
          hasCriticalIssues = true;
        }
      } catch (error) {
        // Create error result for failed file
        const errorResult = {
          timestamp: new Date().toISOString(),
          file: filePath,
          project: null,
          summary: {
            totalIssues: 1,
            critical: 1,
            errors: 0,
            warnings: 0,
            autoFixable: 0,
            severityScore: 9
          },
          issues: [{
            id: `error-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
            severity: 9,
            severityLevel: 'critical',
            tool: 'System',
            category: 'execution',
            file: filePath,
            line: null,
            column: null,
            message: `Quality check execution failed: ${error.message}`,
            rule: 'execution-failure',
            fixable: false,
            fixCommand: null,
            recommendation: '',
            suggestedTool: 'edit',
            project: null,
            architectureLayer: 'unknown'
          }],
          metadata: {
            fileType: 'unknown',
            architectureLayer: 'unknown',
            dependencies: [],
            changeImpact: 'unknown'
          },
          recommendations: []
        };

        allFileResults.push(errorResult);
        allIssues.push(...errorResult.issues);
        hasCriticalIssues = true;
      }
    }

    // Create consolidated result
    const consolidatedResult = {
      timestamp: new Date().toISOString(),
      files: allFileResults,
      summary: {
        totalFiles: filesToCheck.length,
        filesWithIssues: allFileResults.filter(r => r.summary.totalIssues > 0).length,
        totalIssues: allIssues.length,
        critical: allIssues.filter(i => i.severity >= 8).length,
        errors: allIssues.filter(i => i.severity >= 6 && i.severity < 8).length,
        warnings: allIssues.filter(i => i.severity >= 4 && i.severity < 6).length,
        autoFixable: allIssues.filter(i => i.fixable).length,
        severityScore: allIssues.length > 0 ? Math.max(...allIssues.map(i => i.severity)) : 0
      },
      issues: allIssues,
      recommendations: generateConsolidatedRecommendations(allIssues)
    };

    // Format and output the consolidated result
    let output;
    switch (options.outputFormat) {
      case 'json':
        output = JSON.stringify(consolidatedResult, null, 2);
        break;
      case 'markdown':
        output = formatConsolidatedMarkdown(consolidatedResult);
        break;
      case 'console':
      default:
        output = formatConsolidatedConsole(consolidatedResult, options.verbose);
        break;
    }

    console.log(output);

    // Exit with error code if critical issues found (except for JSON output where Claude can decide)
    if (options.outputFormat !== 'json' && hasCriticalIssues) {
      process.exit(2); // Exit code 2 tells Claude Code to not save the file
    } else {
      process.exit(0);
    }

  } catch (error) {
    // In console mode, show formatted error
    if (options.outputFormat === 'console') {
      console.error(`${colors.red}💥 Quality check failed: ${error.message}${colors.reset}`);
    } else {
      // In JSON/Markdown mode, output error in structured format
      const errorOutput = options.outputFormat === 'json' ? JSON.stringify({
        timestamp: new Date().toISOString(),
        error: true,
        message: error.message,
        stack: error.stack
      }, null, 2) : `# Quality Check Error\n\n**Error:** ${error.message}`;

      console.log(errorOutput);
    }
    process.exit(2);
  }
}

// Handle unhandled promise rejections
process.on('unhandledRejection', (reason, promise) => {
  console.error(`${colors.red}Unhandled Rejection at: ${promise}${colors.reset}`, reason);
  process.exit(2);
});

// Handle uncaught exceptions
process.on('uncaughtException', (error) => {
  console.error(`${colors.red}Uncaught Exception: ${error.message}${colors.reset}`);
  process.exit(2);
});

// Run main function
if (require.main === module) {
  main().catch(error => {
    console.error(`${colors.red}Fatal error: ${error.message}${colors.reset}`);
    process.exit(2);
  });
}