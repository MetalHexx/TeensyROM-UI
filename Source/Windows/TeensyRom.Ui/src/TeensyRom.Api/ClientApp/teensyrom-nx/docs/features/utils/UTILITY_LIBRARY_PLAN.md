# Utility Library Plan

**Project Overview**: Centralized utility library for highly reusable functions across the TeensyROM application, starting with error handling utilities.

**Standards Documentation**:

- **Coding Standards**: [](./CODING_STANDARDS.md)
- **Testing Standards**: [](./TESTING_STANDARDS.md)
- **Domain Standards**: [](./DOMAIN_STANDARDS.md)
- **Nx Library Standards**: [](./NX_LIBRARY_STANDARDS.md)

## 🎯 Project Objective

Create a centralized utility library (`@teensyrom-nx/utils`) containing highly reusable functions for common patterns like error handling that are used across multiple domains and features, promoting code consistency and reducing duplication.

## 📋 Implementation Phases

## Phase 1: NX Library Creation

### Objective

Set up the foundational Nx library structure for utilities in `/libs/utils` with proper configuration and build setup.

### Key Deliverables

- [x] Nx utility library created and configured
- [x] Proper TypeScript and ESLint setup
- [x] Barrel exports configured
- [x] Library added to Nx workspace

### High-Level Tasks

1. **✅ Generate Library**: Create `@teensyrom-nx/utils` library using Nx CLI
2. **✅ Configure Structure**: Set up proper directory structure and barrel exports
3. **✅ Setup Build**: Configure TypeScript, ESLint, and build settings
4. **✅ Workspace Integration**: Add library to Nx workspace configuration

---

## Phase 2: Logging Utilities

### Objective

Extract standardized logging patterns and LogType enum into reusable utility functions that can be used across all domains and features for consistent operational visibility.

### Key Deliverables

- [x] LogType enum and logging utilities moved to utils library
- [x] Centralized logging functions with emoji-enhanced output
- [x] All existing logging code updated to use utilities
- [x] Comprehensive test coverage for logging functions
- [x] Documentation integration with logging standards

### Current State Analysis

- **LogType enum** with 15+ emoji-enhanced operation types (🚀 Start, 📡 NetworkRequest, ✅ Success, etc.)
- **Helper functions** (`logInfo`, `logError`, `logWarn`) currently in storage helpers
- **Usage patterns** established across storage store operations
- **Standards documentation** complete in [`LOGGING_STANDARDS.md`](../../LOGGING_STANDARDS.md)

### Proposed Utility API

```typescript
// @teensyrom-nx/utils/logging
export enum LogType {
  Start = '🚀',
  Finish = '🏁',
  Success = '✅',
  NetworkRequest = '📡',
  Navigate = '🧭',
  Refresh = '🔄',
  Cleanup = '🧹',
  Error = '❌',
  Warning = '⚠️',
  Unknown = '❓',
  Select = '🔍',
  Info = 'ℹ️',
  Critical = '🔥',
  Debug = '🐛',
  Midi = '🎵',
}

export function logInfo(operation: LogType, message: string, data?: unknown): void;
export function logError(message: string, error?: unknown): void;
export function logWarn(message: string): void;
```

### High-Level Tasks

1. **✅ Create Logging Utils Module**: Move LogType enum and helper functions to utils library
2. **✅ Update Storage Domain**: Replace storage-specific logging with centralized utilities
3. **Expand to Other Domains**: Integrate logging utilities across device and UI domains
4. **Add Domain-Specific LogTypes**: Extend LogType enum for domain-specific operations
5. **Update Documentation**: Reference centralized logging utilities in standards

### Benefits

- **Cross-Domain Consistency**: Same logging patterns across all features
- **Centralized Management**: Single source of truth for LogType definitions
- **Extensibility**: Easy to add new operation types for any domain
- **Standards Compliance**: Enforces established logging patterns project-wide
- **Debugging Support**: Consistent operational visibility across entire application

### Integration Points

- **Storage Domain**: Operation lifecycle logging (initialization, navigation, refresh)
- **Device Domain**: Device connection and communication logging
- **UI Components**: User interaction and state change logging
- **API Layer**: HTTP request/response logging with network operations

---

## 🏗️ Architecture Overview

### Key Design Decisions

- **Pure Functions**: All utilities designed as pure functions for maximum reusability
- **Type Safety**: Full TypeScript support with proper type guards
- **Tree Shakable**: Optimized for bundler tree shaking
- **Consistent API**: Standardized function signatures and naming conventions

### Integration Points

- **Storage Domain**: Error handling in storage operations
- **Device Domain**: Error handling in device communication
- **UI Components**: Error display and user feedback
- **API Layer**: HTTP error processing and transformation

## 🧪 Testing Strategy

### Unit Tests

- [ ] Error utility function coverage for all error types
- [ ] Logging utility function coverage for all LogType operations
- [ ] Type guard functionality verification
- [ ] Edge cases and null/undefined handling
- [ ] Pure function behavior validation

### Integration Tests

- [ ] Utility integration with existing domain code
- [ ] Error handling and logging in NgRx signal stores
- [ ] Component integration with error and logging utilities
- [ ] Cross-domain utility usage verification

### E2E Tests

- [ ] End-to-end error scenarios in user workflows
- [ ] Error handling during device operations
- [ ] Error recovery and user feedback flows

## ✅ Success Criteria

- [ ] NX utility library created and properly configured
- [ ] Error handling patterns standardized across codebase
- [ ] Logging utilities centralized with emoji-enhanced patterns
- [ ] All existing error handling and logging updated to use utilities
- [ ] Comprehensive test coverage for all utilities
- [ ] Library follows Nx and project standards
- [ ] Ready for future utility additions

## 📚 Related Documentation

- **Logging Standards**: [`LOGGING_STANDARDS.md`](../../LOGGING_STANDARDS.md) - Emoji-enhanced logging patterns and standards
- **State Standards**: [`STATE_STANDARDS.md`](../../STATE_STANDARDS.md) - NgRx Signal Store patterns with logging integration
- **Architecture Overview**: [`OVERVIEW_CONTEXT.md`](./OVERVIEW_CONTEXT.md)
- **Technical Debt**: [`TECHNICAL_DEBT.md`](./TECHNICAL_DEBT.md)
- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md)
- **Testing Standards**: [`TESTING_STANDARDS.md`](./TESTING_STANDARDS.md)

## 📝 Notes

- Start with error handling utilities as the foundation
- Expand to other utility categories in future phases
- Maintain backward compatibility during refactoring
- Focus on pure functions for maximum reusability
- Consider performance implications for frequently used utilities
