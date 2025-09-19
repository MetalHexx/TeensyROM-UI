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

## Phase 2: Error Handling Utilities

### Objective

Extract and standardize common error handling patterns into reusable utility functions.

### Key Deliverables

- [ ] Error utility functions implemented
- [ ] Existing code updated to use utilities
- [ ] Comprehensive test coverage
- [ ] Documentation for utilities

### High-Level Tasks

1. **Create Error Utils Module**: Implement `extractErrorMessage` and related functions
2. **Update Existing Usage**: Replace inline patterns with utility functions
3. **Comprehensive Codebase Update**: Find and update all instances across the codebase
4. **Add Unit Tests**: Test all error scenarios and edge cases

---

## Phase 3: Logging Utilities

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

## Phase 4: StorageKey Value Type Refactoring

### Objective

Refactor StorageKeyUtil into a more concise StorageKey value type with factory methods and improved ergonomics while maintaining full backward compatibility.

### Key Deliverables

- [ ] StorageKey value type implemented with factory methods
- [ ] All StorageKeyUtil references updated to StorageKey
- [ ] Improved API with shorter method names
- [ ] Comprehensive test coverage maintained
- [ ] Documentation updated across all references

### Current Usage Analysis

- **60+ occurrences** across 11 files (storage store, tests, player component, docs)
- **Main methods**: `create()` (28 uses), `forDevice()` (2 uses), `parse()` (8 uses), `forStorageType()` (6 uses)
- **Files affected**: Storage state, player view, tests, documentation

### Proposed API Changes

```typescript
// Current API
StorageKeyUtil.create(deviceId, storageType);
StorageKeyUtil.parse(key);
StorageKeyUtil.forDevice(deviceId);
StorageKeyUtil.forStorageType(storageType);

// New API
StorageKey.of(deviceId, storageType);
StorageKey.parse(key);
StorageKey.forDevice(deviceId);
StorageKey.forStorageType(storageType);
```

### High-Level Tasks

1. **Create StorageKey Value Type**: Implement new value type with factory methods
2. **Update Storage Helpers**: Migrate storage helper functions to use new API
3. **Update Storage Store**: Replace all StorageKeyUtil calls in storage store
4. **Update Tests**: Migrate all test files to use new API
5. **Update Components**: Migrate player component and other consumers
6. **Update Documentation**: Replace all documentation references
7. **Remove Legacy Code**: Clean up old StorageKeyUtil exports

### Benefits

- **Shorter API**: `StorageKey.of()` vs `StorageKeyUtil.create()`
- **Better ergonomics**: More intuitive naming and TypeScript support
- **Value type pattern**: Consistent with modern TypeScript practices
- **Extensibility**: Easier to add instance methods if needed
- **Maintainability**: Single responsibility and cleaner imports

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
