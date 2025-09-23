# Phase 1: Store Action Foundation

**High Level Plan Documentation**: [Directory Trail Plan](./DIRECTORY_TRAIL_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../STORE_TESTING.md)
- **State Standards**: [STATE_STANDARDS.md](../../STATE_STANDARDS.md)

## 🎯 Objective

Create and test the `navigateUpOneDirectory` store action following established patterns. This phase delivers a production-ready store action with comprehensive unit tests.

## 📚 Required Reading

- [x] [STATE_STANDARDS.md](../../STATE_STANDARDS.md) - Store action implementation patterns
- [x] [STORE_TESTING.md](../../STORE_TESTING.md) - Testing methodology for stores
- [x] [Existing navigate-to-directory.ts](../../../libs/domain/storage/state/src/lib/actions/navigate-to-directory.ts) - Pattern reference
- [x] [storage-store.spec.ts](../../../libs/domain/storage/state/src/lib/storage-store.spec.ts) - Test pattern reference

## 📋 Implementation Tasks

### Task 1: Store Action Implementation

**Purpose**: Create navigate-up-one-directory.ts following STATE_STANDARDS.md patterns.

- [x] Create file with proper imports and function signature
- [x] Implement entry validation and early returns
- [x] Implement parent path calculation with edge case handling
- [x] Implement navigation logic with error handling
- [x] Add proper logging and TypeScript typing

### Task 2: Store Integration

**Purpose**: Integrate new action into storage store exports.

- [x] Add import to actions/index.ts
- [x] Add to withStorageActions() method exports
- [x] Verify TypeScript compilation and store interface

### Task 3: Comprehensive Unit Testing

**Purpose**: Test store action following STORE_TESTING.md methodology.

- [x] Add test section to storage-store.spec.ts
- [x] Test basic navigation up functionality
- [x] Test root directory edge case handling
- [x] Test missing entry validation
- [x] Test API error scenarios
- [x] Test path calculation edge cases
- [x] Test caching behavior integration

## 🗂️ File Changes

- [navigate-up-one-directory.ts](../../../libs/domain/storage/state/src/lib/actions/navigate-up-one-directory.ts) - New file
- [actions/index.ts](../../../libs/domain/storage/state/src/lib/actions/index.ts) - Add export
- [storage-store.spec.ts](../../../libs/domain/storage/state/src/lib/storage-store.spec.ts) - Add tests

## 🧪 Testing Requirements

### Unit Tests

- [x] Navigation up one level updates state correctly
- [x] Selected directory updates to parent path
- [x] Root directory is handled as no-op
- [x] Missing storage entry is handled gracefully
- [x] API errors set proper error state
- [x] Path calculation works for all formats
- [x] Cached parent directories are utilized

### Integration Tests

- [x] Store action integrates with existing storage actions
- [x] TypeScript typing works correctly with store interface
- [x] Action can be called from components without errors

## ✅ Success Criteria

- [x] Store action implements navigate up functionality correctly
- [x] All unit tests pass with 100% line coverage
- [x] Action follows STATE_STANDARDS.md patterns exactly
- [x] TypeScript compilation succeeds with no errors
- [x] Ready to proceed to Phase 2 (presentational components)

## 📝 Notes

- Follow existing `navigateToDirectory` pattern for consistency
- Use `calculateParentPath` helper function for path manipulation
- Leverage existing storage helpers for state management
- Ensure proper async/await patterns throughout implementation
