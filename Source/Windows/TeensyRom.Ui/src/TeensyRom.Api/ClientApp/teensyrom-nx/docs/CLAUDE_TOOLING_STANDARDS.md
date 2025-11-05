# Claude Tooling Standards

## Overview

This document defines the architecture and design patterns for Claude Code slash commands and subagents in the TeensyROM project. Following these standards ensures maximum composability, minimal duplication, and clear separation of concerns.

---

## Core Architecture Pattern

### Slash Commands = Composable Primitives

Slash commands are **focused, reusable prompts** that perform specific operations.

**Characteristics**:

- Single, well-defined responsibility
- Can be invoked manually by users
- Can be invoked programmatically by subagents via `SlashCommand` tool
- Can use any tools (Read, Write, Edit, Bash, etc.)
- Can modify files and execute operations
- Output should be clear and structured
- Minimal iteration - do the task and report
- Think of them as "specialized prompt templates" that can take action

**Location**: `.claude/commands/*.md`

**Tool Access**: Commands inherit tools from the conversation by default, or can specify `allowed-tools` in frontmatter to limit access (e.g., `allowed-tools: Bash(pnpm:*), Bash(nx:*)`).

**Example**: `/clean-code <file-path>`

- Reads the file and analyzes errors
- Fixes TypeScript and ESLint issues
- Runs tests to validate
- Reports results
- Does NOT find all files with errors (that's orchestration)
- Does NOT iterate across multiple files (that's orchestration)

### Subagents = Intelligent Orchestrators

Subagents are **specialized AI personalities** that operate in their own context window and can use slash commands as tools.

**Characteristics**:

- Operate in separate context window (preserves main conversation)
- Can use slash commands via `SlashCommand` tool
- Can use any tools (Read, Write, Edit, Bash, etc.)
- Add context-aware decision making and orchestration
- Provide domain expertise and multi-step workflows
- Handle complex iterations and retry logic
- Actually modify files and fix issues
- Inherit tools from main thread by default, or specify `tools` in frontmatter
- Think of them as "specialized AI experts with their own workspace"

**Location**: `.claude/agents/*.md`

**Tool Access**: Subagents inherit all tools from the main thread by default. Specify `tools: Read, Write, Edit, Grep, Glob, Bash, SlashCommand` in frontmatter to limit or customize access.

**Example**: `code-cleaner`

- Finds all files with errors (via Grep, Bash)
- Calls `/clean-code` for each file (via SlashCommand tool)
- Tracks progress across multiple files
- Validates entire workspace after all fixes
- Iterates if needed
- Reports summary of all changes made

---

## Design Principles

### 1. Avoid Redundancy

**❌ Anti-Pattern**: Subagent duplicates command logic

```markdown
# Bad: e2e-runner parses Cypress output the same way /run-e2e-test does

Subagent workflow:

1. Run Cypress via Bash: pnpm nx e2e ...
2. Parse Cypress JSON output
3. Format test results
4. Locate screenshots in specific directories
5. Analyze failures
6. Fix issues
```

**✅ Correct Pattern**: Subagent uses command as primitive

```markdown
# Good: e2e-runner orchestrates using /run-e2e-test

Subagent workflow:

1. Call /run-e2e-test <files> # Command handles execution + parsing
2. Interpret structured results from command
3. Analyze failure patterns
4. Fix issues by modifying files
5. Call /run-e2e-test again to validate
```

**Key Insight**: Don't duplicate "how to run tests and parse output" - that's the command's job. Focus on "what to do with the results" - that's the subagent's job.

### 2. Maximize Composability

Commands should be useful on their own and in combination:

**Manual invocation** (user calls directly):

```
/clean-code libs/infrastructure/device/device.service.ts
```

**Programmatic invocation** (subagent calls):

```
code-cleaner agent:
  1. Find all files with errors
  2. For each file: call /clean-code <file>
  3. Validate results
  4. Report summary
```

### 3. Single Responsibility

Each command should do one thing well:

**✅ Good** - Focused commands:

- `/clean-code <file>` - Clean one file
- `/run-e2e-test [file]` - Run tests

**❌ Bad** - Kitchen sink command:

- `/fix-everything` - Lints, tests, formats, builds, deploys

### 4. Clear Separation of Concerns

**Commands** handle focused tasks:

- Perform specific operations (run tests, clean code, etc.)
- Can read/write files, run bash commands, etc.
- Parse output and format results
- Provide clear reporting
- Execute and report - no complex iteration

**Subagents** handle orchestration:

- Decide what commands to run and when
- Interpret results in broader context
- Make strategic decisions
- Orchestrate multi-step workflows
- Iterate until goals achieved
- Operate in separate context window

---

## Implementation Guidelines

### Creating Slash Commands

#### Frontmatter Configuration

```markdown
---
description: Brief description shown in /help
argument-hint: [arg1] [arg2]  # Optional - shown in autocomplete
allowed-tools: Bash(pnpm:*), Bash(nx:*), Read, Write, Edit  # Optional - inherits all by default
model: sonnet  # Optional - inherit, sonnet, opus, or haiku
disable-model-invocation: false  # Optional - prevent SlashCommand tool from calling this
---
```

**Key Points**:

- Commands inherit all tools from the conversation if `allowed-tools` is omitted
- Use `allowed-tools` to restrict tools for security or focus
- Bash tools can be scoped: `Bash(git:*)` allows only git commands

#### Command Structure

```markdown
# Command Name

Brief description of what the command does.

## Context

- Argument 1: $1 # Individual positional arg
- All arguments: $ARGUMENTS # All args as string
- Pre-execution info: !`bash-command-for-context` # Runs before command
- File reference: @path/to/important-file.md # Includes file in context

## Your Task

### Step 1: Validate Input

[Check arguments, ensure prerequisites]

### Step 2: Execute Operation

[Core operation - can use Read, Write, Edit, Bash, etc.]
[Commands CAN modify files, fix issues, run tests, etc.]

### Step 3: Report Results

[Provide clear, structured output]

## Guidelines

- Focus on ONE specific task
- Can take action (read/write files, run commands)
- Don't orchestrate across multiple files/iterations
- Provide clear, parseable output
```

#### Best Practices

**DO**:

- Use `$ARGUMENTS` or `$1, $2` for parameters
- Use `!`command`` for pre-execution context gathering
- Use `@file/path` to reference documentation files
- Use `allowed-tools` to limit scope when needed
- Can read files, write files, run tests, fix issues, etc.
- Return clear, structured output
- Handle error cases gracefully
- Focus on ONE well-defined task

**DON'T**:

- Orchestrate across multiple files (that's for subagents)
- Iterate with complex retry logic (that's for subagents)
- Make strategic decisions about "what to do next" (that's for subagents)
- Duplicate functionality of other commands
- Try to do too much in one command

### Creating Subagents

#### Frontmatter Configuration

```markdown
---
name: agent-name # Required - lowercase with hyphens
description: When this agent should be invoked (use PROACTIVELY for auto-invoke) # Required
model: inherit # Optional - inherit, sonnet, opus, or haiku (default: sonnet)
tools: Read, Write, Edit, Grep, Glob, Bash, SlashCommand # Optional - inherits all by default
color: cyan # Optional - visual identifier in UI
---
```

**Key Points**:

- Subagents inherit ALL tools from main thread if `tools` is omitted
- Include `SlashCommand` in tools to let agent invoke slash commands
- Use `model: inherit` to match the main conversation's model
- Separate context window - doesn't pollute main conversation

#### Subagent Structure

```markdown
You are a [domain] expert specializing in [specific capability].

## Core Philosophy

Define the agent's primary responsibility and how it adds value beyond just calling commands.

## When to invoke

You MUST BE USED proactively in these scenarios:

- [Scenario 1]
- [Scenario 2]
- Use PROACTIVELY keyword to encourage auto-invocation

## Your workflow

### 1. Analyze context

[Understand the situation before acting]

### 2. Use slash commands as primitives

[Call commands to gather information or execute operations]

### 3. Interpret results

[Add intelligence - what do the results mean?]

### 4. Take action

[Make code changes, fix issues, etc.]

### 5. Validate

[Re-run commands to verify your changes]

### 6. Iterate if needed

[Continue until goal is achieved]

### 7. Report

[Summarize what you did, not just what you found]

## Decision-making guidelines

When to act automatically vs. ask for help:

- ✅ Fix automatically: [List auto-fixable issues]
- ❌ Ask for help: [List cases requiring human judgment]

## Integration with other agents

How this agent works with others in the system.

## Best practices

Agent-specific guidelines for effective operation.

## Key resources

Links to relevant documentation.

## Remember

Your goal: [Clear success criteria]

You are an orchestrator, not just a reporter:

- Use slash commands as composable tools
- Add value through intelligence and action
- Iterate until success
- Report your journey
```

#### Best Practices

**DO**:

- Use `/slash-commands` via SlashCommand tool as primitives
- Make actual code changes (you have Write, Edit, etc.)
- Iterate until goals achieved
- Operate in your own context window
- Report what you DID, not just what you FOUND
- Include concrete examples of fixes
- Specify when to act vs. ask for help
- Show before/after for changes

**DON'T**:

- Duplicate command logic (use commands via SlashCommand tool)
- Just report without taking action
- Stop after first failure
- Make assumptions about business logic
- Modify files outside your domain
- Forget you have separate context - you won't pollute main conversation

---

## Examples from TeensyROM

### Example 1: Code Cleaning

**Command**: `/clean-code <file-path>`

- Focused on cleaning ONE file
- Reads the file, analyzes errors
- Fixes TypeScript and ESLint issues
- Runs tests to validate the file
- Reports what was fixed

**Subagent**: `code-cleaner`

- Orchestrates across multiple files
- Finds all files with errors (Bash, Grep)
- Calls `/clean-code` for each file (SlashCommand tool)
- Tracks progress across all files
- Validates entire workspace after all fixes
- Reports summary of all changes made

**Why this works**:

- Command: focused task (one file)
- Subagent: orchestration (many files)
- Command is useful standalone
- Subagent adds workspace-level intelligence
- No duplication - command does the work, agent coordinates
- Both have full tool access (Read, Write, Edit, Bash)

### Example 2: E2E Testing

**Command**: `/run-e2e-test [file-path]`

- Focused on running tests and reporting
- Executes Cypress via Bash (pnpm nx e2e ...)
- Parses output (pass/fail counts, errors)
- Locates debug artifacts (screenshots, videos)
- Reports structured results
- Does NOT fix issues or iterate

**Subagent**: `e2e-runner`

- Orchestrates the testing workflow
- Determines which tests to run (git diff, feature scope)
- Calls `/run-e2e-test` to execute (SlashCommand tool)
- Analyzes failure patterns (timing, selectors, assertions)
- Fixes issues by modifying test/component files (Write, Edit tools)
- Calls `/run-e2e-test` again to validate
- Iterates until all tests pass
- Reports what it fixed

**Why this works**:

- Command: focused task (run tests, report results)
- Subagent: orchestration (analyze, fix, retry, iterate)
- Command is useful for manual test runs
- Subagent adds intelligent failure analysis and fixing
- No duplication - command handles execution, agent handles strategy

---

## Anti-Patterns to Avoid

### ❌ Command with Too Much Orchestration

```markdown
# /fix-and-test-everything

1. Find all errors across workspace
2. Fix errors in all files
3. Run all tests
4. Fix test failures
5. Iterate until perfect
6. Deploy to production
```

**Problem**: Command is doing multi-file orchestration and iteration. This should be a subagent that calls focused commands like `/clean-code` and `/run-tests` for each file/suite.

### ❌ Subagent that Duplicates Command Logic

```markdown
# code-fixer agent

For each file:

1. Run `pnpm nx lint <file>` directly via Bash
2. Parse linting JSON output manually
3. Identify error categories (unused vars, type errors, etc.)
4. Fix the specific error types
5. Run tests via Bash for that file
6. Parse test output manually
```

**Problem**: This duplicates the "how to lint, parse, and test a file" logic that should be in `/clean-code` command. Agent should call `/clean-code` via SlashCommand tool instead.

### ❌ Command and Subagent Doing the Same Thing

```markdown
# /run-and-fix-e2e-tests - runs tests and fixes failures

# e2e-runner - runs tests and fixes failures
```

**Problem**: Complete redundancy. Pick one approach or separate concerns.

### ❌ Subagent That Just Reports

```markdown
# test-analyzer agent

1. Call /run-e2e-test
2. Read the results
3. Analyze failure patterns
4. Tell user what's wrong
5. Suggest fixes
6. Done (doesn't actually fix anything)
```

**Problem**: Not adding value beyond what the command already provides. Agent should FIX issues, not just analyze and report. Use the Write/Edit tools!

---

## Testing Your Design

Ask these questions:

### For Slash Commands:

1. **Can I call this manually and get value?** (Should be yes)
2. **Does it do ONE focused task?** (Should be yes)
3. **Does it orchestrate across many files/iterations?** (Should be no - that's for subagents)
4. **Is the output clear and useful?** (Should be yes)

### For Subagents:

1. **Does it orchestrate multiple operations?** (Should be yes)
2. **Does it use slash commands via SlashCommand tool?** (Should be yes, when applicable)
3. **Does it add intelligence beyond individual commands?** (Should be yes)
4. **Does it actually FIX things (not just report)?** (Should be yes)
5. **Does it iterate until success?** (Should be yes, when applicable)
6. **Does it duplicate command logic?** (Should be no)

---

## Refactoring Existing Tools

If you find redundancy or poor separation:

### Step 1: Identify the Focused Task

What is the core, reusable task that does ONE thing? → Make this a command

### Step 2: Identify the Orchestration

What decisions, multi-step workflows, or iteration is needed? → Make this a subagent

### Step 3: Refactor

- Move focused task execution to command
- Move orchestration and iteration to subagent
- Ensure command is usable standalone
- Ensure subagent uses command via SlashCommand tool
- Avoid duplication of logic

### Example Refactor:

**Before**: `e2e-tester` agent does everything

```
1. Parse arguments
2. Run Cypress via Bash
3. Parse output
4. Format results
5. Analyze failures
6. Fix issues
7. Re-run tests
```

**After**: Split into command + agent

```
/run-e2e-test command:
1. Parse arguments
2. Run Cypress via Bash
3. Parse output
4. Format results

e2e-runner agent:
1. Call /run-e2e-test
2. Analyze failures
3. Fix issues
4. Call /run-e2e-test again
```

---

## Summary

**Slash Commands**:

- Focused task execution (not just reporting!)
- Can use all tools (Read, Write, Edit, Bash, etc.)
- Single, well-defined responsibility
- No complex orchestration across multiple files
- Useful standalone

**Subagents**:

- Intelligent orchestrators
- Separate context window (preserves main conversation)
- Use commands via SlashCommand tool
- Can use all tools directly
- Multi-step workflows and iteration
- Add strategic decision-making

**Together**:

- Maximum composability
- Minimal duplication
- Clear separation of concerns
- Powerful and flexible

**Key Insight**: Both commands and subagents can read/write files and take action. The difference is **scope and orchestration**: commands do focused tasks, subagents orchestrate workflows.

Follow these patterns to build a robust, maintainable Claude tooling ecosystem.
