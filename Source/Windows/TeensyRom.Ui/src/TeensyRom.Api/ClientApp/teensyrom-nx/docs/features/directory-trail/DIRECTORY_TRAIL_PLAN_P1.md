# Phase 1: Store Action Foundation

**High Level Plan Documentation**: [Directory Trail Plan](./DIRECTORY_TRAIL_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../STORE_TESTING.md)
- **State Standards**: [STATE_STANDARDS.md](../../STATE_STANDARDS.md)

## 🎯 Objective

Create and test the `navigateUpOneDirectory` store action following established patterns. This phase delivers a production-ready store action with comprehensive unit tests.

## 📚 Required Reading

- [ ] [STATE_STANDARDS.md](../../STATE_STANDARDS.md) - Store action implementation patterns
- [ ] [STORE_TESTING.md](../../STORE_TESTING.md) - Testing methodology for stores
- [ ] [Existing navigate-to-directory.ts](../../../libs/domain/storage/state/src/lib/actions/navigate-to-directory.ts) - Pattern reference
- [ ] [storage-store.spec.ts](../../../libs/domain/storage/state/src/lib/storage-store.spec.ts) - Test pattern reference

## 📋 Implementation Tasks

### Task 1: Store Action Implementation

**Purpose**: Create navigate-up-one-directory.ts following STATE_STANDARDS.md patterns.

- [ ] Create file with proper imports and function signature
- [ ] Implement entry validation and early returns
- [ ] Implement parent path calculation with edge case handling
- [ ] Implement navigation logic with error handling
- [ ] Add proper logging and TypeScript typing

### Task 2: Store Integration

**Purpose**: Integrate new action into storage store exports.

- [ ] Add import to actions/index.ts
- [ ] Add to withStorageActions() method exports
- [ ] Verify TypeScript compilation and store interface

### Task 3: Comprehensive Unit Testing

**Purpose**: Test store action following STORE_TESTING.md methodology.

- [ ] Add test section to storage-store.spec.ts
- [ ] Test basic navigation up functionality
- [ ] Test root directory edge case handling
- [ ] Test missing entry validation
- [ ] Test API error scenarios
- [ ] Test path calculation edge cases
- [ ] Test caching behavior integration

## 🗂️ File Changes

- [navigate-up-one-directory.ts](../../../libs/domain/storage/state/src/lib/actions/navigate-up-one-directory.ts) - New file
- [actions/index.ts](../../../libs/domain/storage/state/src/lib/actions/index.ts) - Add export
- [storage-store.spec.ts](../../../libs/domain/storage/state/src/lib/storage-store.spec.ts) - Add tests

## 🧪 Testing Requirements

### Unit Tests

- [ ] Navigation up one level updates state correctly
- [ ] Selected directory updates to parent path
- [ ] Root directory is handled as no-op
- [ ] Missing storage entry is handled gracefully
- [ ] API errors set proper error state
- [ ] Path calculation works for all formats
- [ ] Cached parent directories are utilized

### Integration Tests

- [ ] Store action integrates with existing storage actions
- [ ] TypeScript typing works correctly with store interface
- [ ] Action can be called from components without errors

## ✅ Success Criteria

- [ ] Store action implements navigate up functionality correctly
- [ ] All unit tests pass with 100% line coverage
- [ ] Action follows STATE_STANDARDS.md patterns exactly
- [ ] TypeScript compilation succeeds with no errors
- [ ] Ready to proceed to Phase 2 (presentational components)

## 📝 Notes

- Follow existing `navigateToDirectory` pattern for consistency
- Use `calculateParentPath` helper function for path manipulation
- Leverage existing storage helpers for state management
- Ensure proper async/await patterns throughout implementation
