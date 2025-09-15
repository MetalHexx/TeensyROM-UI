# Storage Store Restructuring

**Project Overview**: Restructure the Storage Store to follow NgRx Signal Store best practices by consolidating method files into the main store and extracting common patterns into reusable utilities.

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](./CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](./TESTING_STANDARDS.md)
- **State Standards**: [STATE_STANDARDS.md](./STATE_STANDARDS.md)
- **Domain Standards**: [DOMAIN_STANDARDS.md](./DOMAIN_STANDARDS.md)
- **Style Guide**: [STYLE_GUIDE.md](./STYLE_GUIDE.md)
- **API Client Generation**: [API_CLIENT_GENERATION.md](./API_CLIENT_GENERATION.md)

## Phase Links

- Phase 1: [STORAGE_STORE_RESTRUCTURING_P1.md](./STORAGE_STORE_RESTRUCTURING_P1.md)
- Phase 2: [STORAGE_STORE_RESTRUCTURING_P2.md](./STORAGE_STORE_RESTRUCTURING_P2.md)
- Phase 3: [STORAGE_STORE_RESTRUCTURING_P3.md](./STORAGE_STORE_RESTRUCTURING_P3.md)

## 🎯 Project Objective

Restructure the Storage Store to follow NgRx Signal Store best practices by moving from separate method files to inline methods within the store, while creating reusable utilities to eliminate code duplication and improve maintainability. This refactoring will make the code more declarative, DRY, and easier to maintain while preserving all existing functionality and API contracts.

## 📋 Implementation Phases

## Phase 1: Consolidate Methods into Store

Open detailed phase plan: [STORAGE_STORE_RESTRUCTURING_P1.md](./STORAGE_STORE_RESTRUCTURING_P1.md)

### Objective

Move all method files back into the main storage store file following standard NgRx Signal Store patterns, while maintaining our async/await approach and preserving all existing functionality.

### Key Deliverables

- [ ] All storage methods consolidated into main store file
- [ ] Method files removed from separate directory
- [ ] Import statements updated in storage store
- [ ] All existing tests continue to pass without modification

### High-Level Tasks

1. **Consolidate initializeStorage method**: Move from `methods/initialize-storage.ts` into `storage-store.ts` withMethods
2. **Consolidate navigateToDirectory method**: Move from `methods/navigate-to-directory.ts` into `storage-store.ts` withMethods
3. **Consolidate refreshDirectory method**: Move from `methods/refresh-directory.ts` into `storage-store.ts` withMethods
4. **Consolidate cleanupStorage method**: Move from `methods/cleanup-storage.ts` into `storage-store.ts` withMethods
5. **Update imports**: Remove method file imports from `storage-store.ts`
6. **Delete method files**: Remove all files in `methods/` directory
7. **Verify tests**: Ensure all existing tests pass without changes

### Files Modified

- **Modified**: `libs/domain/storage/state/src/lib/storage-store.ts`
- **Deleted**: `libs/domain/storage/state/src/lib/methods/initialize-storage.ts`
- **Deleted**: `libs/domain/storage/state/src/lib/methods/navigate-to-directory.ts`
- **Deleted**: `libs/domain/storage/state/src/lib/methods/refresh-directory.ts`
- **Deleted**: `libs/domain/storage/state/src/lib/methods/cleanup-storage.ts`
- **Deleted**: `libs/domain/storage/state/src/lib/methods/` directory (if empty)

### Testing Impact

✅ **No test changes required** - All method signatures, return types, and behavior remain identical. Existing tests should pass without modification as this is purely internal restructuring.

---

## Phase 2: Extract Common Patterns and Create Utilities

Open detailed phase plan: [STORAGE_STORE_RESTRUCTURING_P2.md](./STORAGE_STORE_RESTRUCTURING_P2.md)

### Objective

Create reusable helper functions to eliminate repetitive code and make the store more declarative and DRY, focusing on common state update patterns, error handling, and async operation management.

### Key Deliverables

- [ ] State update helper utilities created
- [ ] Async operation wrapper functions implemented
- [ ] Entry validation helpers extracted
- [ ] Directory loading pattern standardized
- [ ] All existing functionality preserved with cleaner implementation

### High-Level Tasks

1. **Create storage-helpers.ts**: New file with reusable state update utilities
2. **Extract loading state helpers**: Functions for setting loading/error states consistently
3. **Create async operation wrapper**: Higher-order function for common async patterns
4. **Extract entry validation**: Standardize entry existence and validation checks
5. **Create directory loading helper**: Common pattern for API calls and state updates
6. **Refactor store methods**: Update methods to use new helpers while preserving behavior
7. **Verify functionality**: Ensure all operations work identically with new implementation

### Common Patterns to Extract

#### State Entry Update Helpers

```typescript
// Helper for setting loading state
const setLoadingState = (key: StorageKey, isLoading: boolean, error: string | null = null)

// Helper for updating entry data
const updateEntryData = (key: StorageKey, updates: Partial<StorageDirectoryState>)

// Helper for setting error state
const setErrorState = (key: StorageKey, error: string)
```

#### Async Operation Wrapper

```typescript
const withAsyncOperation = <T>(
  operation: () => Promise<T>,
  onSuccess: (result: T) => void,
  onError: (error: any) => void,
  key: StorageKey
)
```

#### Entry Validation Helper

```typescript
const validateEntry = (entries: Record<string, StorageDirectoryState>, key: StorageKey)
```

#### Directory Loading Helper

```typescript
const loadDirectory = async (
  storageService: IStorageService,
  deviceId: string,
  storageType: StorageType,
  path: string
)
```

### Files Modified

- **Created**: `libs/domain/storage/state/src/lib/storage-helpers.ts`
- **Modified**: `libs/domain/storage/state/src/lib/storage-store.ts`

### Testing Impact

✅ **No test changes required** - Helper functions are internal implementation details. All public method contracts remain unchanged, so existing tests continue to validate the same behavior.

---

## Phase 3: Documentation Updates

Open detailed phase plan: [STORAGE_STORE_RESTRUCTURING_P3.md](./STORAGE_STORE_RESTRUCTURING_P3.md)

### Objective

Update the STATE_STANDARDS.md documentation to reflect the new inline method pattern and helper utility approach, providing clear guidance for future NgRx Signal Store implementations.

### Key Deliverables

- [ ] STATE_STANDARDS.md updated with inline method patterns
- [ ] Helper utility documentation added
- [ ] Code examples updated to reflect new structure
- [ ] Best practices documented for future implementations

### High-Level Tasks

1. **Update method organization section**: Change from separate files to inline methods
2. **Add helper utility guidance**: Document when and how to create reusable helpers
3. **Update code examples**: Replace method file examples with inline method examples
4. **Document DRY principles**: Guidelines for extracting common patterns
5. **Update reference implementation**: Point to refactored storage store as exemplar
6. **Add helper utility patterns**: Document common helper patterns and their usage

### Files Modified

- **Modified**: `docs/STATE_STANDARDS.md`

### Testing Impact

✅ **No testing impact** - Documentation updates only.

## 🏗️ Architecture Overview

### Key Design Decisions

- **Inline Methods**: Move from separate method files to inline methods within the store following NgRx Signal Store best practices
- **Helper Utilities**: Extract common patterns into reusable functions to eliminate code duplication
- **Preserved Contracts**: Maintain all existing method signatures and behavior to ensure zero breaking changes
- **Async/Await Pattern**: Continue using our successful async/await pattern for deterministic execution

### Integration Points

- **Storage Services**: Helper utilities will standardize how we interact with IStorageService
- **State Management**: Centralized state update patterns through helper functions
- **Error Handling**: Consistent error management across all storage operations
- **Component Integration**: No changes required in components as store API remains identical

## 🧪 Testing Strategy

### Unit Tests

- [ ] All existing storage store tests continue to pass
- [ ] Helper utility functions tested independently
- [ ] State update patterns validated through existing test coverage

### Integration Tests

- [ ] Storage initialization flow continues to work
- [ ] Directory navigation operations remain functional
- [ ] Error handling scenarios preserve existing behavior

### E2E Tests

- [ ] Device storage initialization in player view
- [ ] Directory browsing and navigation
- [ ] Error recovery and retry scenarios

## ✅ Success Criteria

- [ ] All storage methods consolidated into main store file following NgRx patterns
- [ ] Common patterns extracted into reusable helper utilities
- [ ] Code is more DRY, maintainable, and declarative
- [ ] All existing tests pass without modification
- [ ] No breaking changes to public API contracts
- [ ] Documentation updated to reflect new patterns
- [ ] Project ready for production deployment

## 📚 Related Documentation

- **Architecture Overview**: [`OVERVIEW_CONTEXT.md`](./OVERVIEW_CONTEXT.md)
- **Technical Debt**: [`TECHNICAL_DEBT.md`](./TECHNICAL_DEBT.md)
- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md)
- **Testing Standards**: [`TESTING_STANDARDS.md`](./TESTING_STANDARDS.md)
- **State Standards**: [`STATE_STANDARDS.md`](./STATE_STANDARDS.md)

## 📝 Notes

- This refactoring is purely internal restructuring with zero breaking changes
- All method signatures, return types, and observable behavior remain identical
- Focus on improving code organization and maintainability without affecting functionality
- The async/await pattern has proven successful and will be preserved throughout
- Helper utilities should be generic enough for reuse but not over-engineered
- Comprehensive logging and error handling patterns will be maintained
