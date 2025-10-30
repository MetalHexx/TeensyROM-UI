# Phase 3: Storage Domain Migration

## 🎯 Objective

Systematically migrate all storage-related endpoints (getDirectory, saveFavorite, removeFavorite, and storage indexing endpoints) using the established one-endpoint-at-a-time approach. The storage domain introduces more complex path resolution logic, filesystem state management, and batch operations, building upon the refined patterns established during the device domain migration.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [E2E Interceptor Refactoring Plan](./E2E_INTERCEPTOR_REFACTOR_PLAN.md) - High-level feature plan
- [ ] [Phase 1 Implementation](./E2E_INTERCEPTOR_REFACTOR_P1.md) - Foundation and infrastructure
- [ ] [Phase 2 Device Migration](./E2E_INTERCEPTOR_REFACTOR_P2.md) - Completed device domain patterns
- [ ] [Interceptor Format Documentation](./INTERCEPTOR_FORMAT.md) - Format guidelines and patterns

**Standards & Guidelines:**
- [ ] [E2E Testing Overview](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Current E2E testing infrastructure

**Reference Materials:**
- [ ] [Sample Endpoint Example](../../../apps/teensyrom-ui-e2e/src/support/interceptors/examples/sampleEndpoint.interceptors.ts) - Working example following format
- [ ] [Current Storage Interceptors](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) - Source for existing implementations
- [ ] [Current Storage Indexing](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage-indexing.interceptors.ts) - Indexing implementations

---

## 📂 File Structure Overview

> Provide a clear file tree showing new files (✨) and modified files (📝) to understand the implementation scope.

```
apps/teensyrom-ui-e2e/src/support/interceptors/
├── getDirectory.interceptors.ts                 ✨ New - getDirectory endpoint consolidation
├── saveFavorite.interceptors.ts                 ✨ New - saveFavorite endpoint consolidation
├── removeFavorite.interceptors.ts              ✨ New - removeFavorite endpoint consolidation
├── indexStorage.interceptors.ts                ✨ New - indexStorage endpoint consolidation
├── indexAllStorage.interceptors.ts             ✨ New - indexAllStorage endpoint consolidation
├── storage.interceptors.ts                     📝 Modified - Remove migrated endpoints
├── storage-indexing.interceptors.ts            📝 Modified - Remove migrated endpoints
├── device.interceptors.ts                      📝 Referenced - Device domain from Phase 2
├── examples/                                   📝 Referenced - Phase 1 examples
│   └── sampleEndpoint.interceptors.ts          📝 Referenced - Format reference

apps/teensyrom-ui-e2e/src/e2e/
├── player/
│   ├── favorite-functionality.cy.ts           📝 Modified - Update imports for favorites
│   └── deep-linking.cy.ts                      📝 Modified - Update imports for getDirectory
├── devices/
│   └── device-indexing.cy.ts                   📝 Modified - Update imports for indexing
└── storage/
    └── test-helpers.ts                         📝 Modified - Update imports for storage endpoints

apps/teensyrom-ui-e2e/src/support/constants/
└── api.constants.ts                            📝 Modified - Remove migrated endpoint constants
```

---

## 📋 Implementation Guidelines

> **IMPORTANT - Progress Tracking:**
> - **Mark checkboxes ✅ as you complete each subtask**
> - Update progress throughout implementation, not just at the end
> - This helps track what's done and what remains

> **IMPORTANT - Migration Approach:**
> - **One endpoint at a time** - Complete getDirectory before starting saveFavorite
> - **Systematic validation** - Run full test suite after each endpoint migration
> - **Explicit imports only** - Use direct imports from new endpoint files
> - **Maintain backward compatibility** - Keep existing functionality working during transition

---

<details open>
<summary><h3>Task 1: Migrate getDirectory Endpoint</h3></summary>

**Purpose**: Create self-contained getDirectory.interceptors.ts file following the established format, with focus on complex path resolution logic and mock filesystem integration.

**Related Documentation:**
- [Interceptor Format Documentation](./INTERCEPTOR_FORMAT.md) - Format to follow exactly
- [Sample Endpoint Example](../../../apps/teensyrom-ui-e2e/src/support/interceptors/examples/sampleEndpoint.interceptors.ts) - Reference implementation
- [Current getDirectory Implementation](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) - Source code to migrate
- [Phase 2 Device Patterns](./E2E_INTERCEPTOR_REFACTOR_P2.md) - Learn from device domain migration

**Implementation Subtasks:**
- [ ] **Create getDirectory.interceptors.ts**: New file following 6-section structure
- [ ] **Section 1 - Endpoint Definition**: Create GET_DIRECTORY_ENDPOINT constant moved from api.constants.ts
- [ ] **Section 2 - Interface Definitions**: Define InterceptGetDirectoryOptions interface with filesystem and path resolution
- [ ] **Section 3 - Interceptor Function**: Create interceptGetDirectory() function with complex path handling
- [ ] **Section 4 - Wait Function**: Create waitForGetDirectory() function
- [ ] **Section 5 - Helper Functions**: Include setupGetDirectory() and filesystem navigation helpers
- [ ] **Section 6 - Export Constants**: Add backward compatibility exports
- [ ] **Update Test Imports**: Update deep-linking.cy.ts to use new explicit imports
- [ ] **Update Storage Helpers**: Update storage/test-helpers.ts to import from new file
- [ ] **Remove from Old Files**: Remove getDirectory from storage.interceptors.ts and api.constants.ts

**Testing Subtask:**
- [ ] **Write Tests**: Run deep-linking.cy.ts to ensure getDirectory functionality unchanged
- [ ] **Validate Filesystem Integration**: Test mock filesystem integration works correctly
- [ ] **Path Resolution Testing**: Test complex path resolution and navigation scenarios

**Key Implementation Notes:**
- Source all interface and implementation details from current storage.interceptors.ts
- Integrate with MockFilesystem class for realistic directory structure testing
- Handle complex path resolution scenarios including nested directories and special cases
- Use explicit imports pattern: `import { interceptGetDirectory, waitForGetDirectory } from './getDirectory.interceptors'`

**Critical Interface:**
```typescript
interface InterceptGetDirectoryOptions {
  filesystem?: MockFilesystem;
  path?: string;
  errorMode?: boolean;
  responseDelayMs?: number;
  customFiles?: FileEntry[];
}
```

**Testing Focus for Task 1:**

**Behaviors to Test:**
- [ ] Directory browsing returns expected filesystem structure
- [ ] Error mode triggers appropriate error responses for invalid paths
- [ ] waitForGetDirectory() works correctly with new alias
- [ ] Complex path resolution handles nested directories correctly
- [ ] All existing deep-linking.cy.ts tests pass without modification

**Testing Reference:**
- Run specific deep-linking tests: `npx nx e2e teensyrom-ui-e2e --spec="src/e2e/player/deep-linking.cy.ts"`

</details>

---

<details open>
<summary><h3>Task 2: Migrate saveFavorite Endpoint</h3></summary>

**Purpose**: Create self-contained saveFavorite.interceptors.ts with focus on state management patterns and filesystem favorite persistence.

**Related Documentation:**
- [INTERCEPTOR_FORMAT.md](./INTERCEPTOR_FORMAT.md) - Format to follow exactly
- [getDirectory.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/getDirectory.interceptors.ts) - Pattern from Task 1
- [Current saveFavorite Implementation](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) - Source code to migrate

**Implementation Subtasks:**
- [ ] **Create saveFavorite.interceptors.ts**: New file following 6-section structure
- [ ] **Section 1 - Endpoint Definition**: Create SAVE_FAVORITE_ENDPOINT constant
- [ ] **Section 2 - Interface Definitions**: Define InterceptSaveFavoriteOptions interface
- [ ] **Section 3 - Interceptor Function**: Create interceptSaveFavorite() function
- [ ] **Section 4 - Wait Function**: Create waitForSaveFavorite() function
- [ ] **Section 5 - Helper Functions**: Include setupSaveFavorite() and favorite state management
- [ ] **Section 6 - Export Constants**: Add backward compatibility exports
- [ ] **Update Test Imports**: Update favorite-functionality.cy.ts to use new explicit imports
- [ ] **Remove from Old Files**: Remove saveFavorite from storage.interceptors.ts

**Testing Subtask:**
- [ ] **Write Tests**: Run favorite-functionality.cy.ts to ensure saveFavorite functionality unchanged
- [ ] **State Management Testing**: Test favorite persistence across filesystem operations
- [ ] **Cross-Endpoint Testing**: Test saveFavorite integration with getDirectory

**Key Implementation Notes:**
- Apply learnings and refinements from getDirectory migration
- Maintain filesystem favorite state consistency across multiple operations
- Use consistent naming conventions established in Task 1 and Phase 2
- Ensure favorite scenarios work correctly with filesystem navigation

**Critical Interface:**
```typescript
interface InterceptSaveFavoriteOptions {
  filesystem?: MockFilesystem;
  favoriteFile?: FileEntry;
  errorMode?: boolean;
  responseDelayMs?: number;
}
```

**Testing Focus for Task 2:**

**Behaviors to Test:**
- [ ] Favorite saving succeeds with valid file entries
- [ ] Favorite errors are handled correctly for invalid files
- [ ] waitForSaveFavorite() works with new alias
- [ ] Integration with getDirectory works seamlessly
- [ ] Favorite state persists correctly across multiple operations

**Testing Reference:**
- Run favorite functionality tests: `npx nx e2e teensyrom-ui-e2e --spec="src/e2e/player/favorite-functionality.cy.ts"`
- Test integration with directory navigation scenarios

</details>

---

<details open>
<summary><h3>Task 3: Migrate removeFavorite Endpoint</h3></summary>

**Purpose**: Create self-contained removeFavorite.interceptors.ts with focus on cleanup logic and state synchronization.

**Related Documentation:**
- [INTERCEPTOR_FORMAT.md](./INTERCEPTOR_FORMAT.md) - Format to follow exactly
- [getDirectory.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/getDirectory.interceptors.ts) - Established pattern
- [Current removeFavorite Implementation](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) - Source code to migrate

**Implementation Subtasks:**
- [ ] **Create removeFavorite.interceptors.ts**: New file following 6-section structure
- [ ] **Section 1 - Endpoint Definition**: Create REMOVE_FAVORITE_ENDPOINT constant
- [ ] **Section 2 - Interface Definitions**: Define InterceptRemoveFavoriteOptions interface
- [ ] **Section 3 - Interceptor Function**: Create interceptRemoveFavorite() function
- [ ] **Section 4 - Wait Function**: Create waitForRemoveFavorite() function
- [ ] **Section 5 - Helper Functions**: Include setupRemoveFavorite() and cleanup logic
- [ ] **Section 6 - Export Constants**: Add backward compatibility exports
- [ ] **Update Test Imports**: Update favorite functionality test files to use new imports
- [ ] **Remove from Old Files**: Remove removeFavorite from storage.interceptors.ts

**Testing Subtask:**
- [ ] **Write Tests**: Validate favorite removal scenarios work correctly
- [ ] **Cleanup Validation**: Ensure proper cleanup of favorite state after removal
- [ ] **State Synchronization Testing**: Test state sync between save/remove operations

**Key Implementation Notes:**
- Focus on consistent error handling patterns across storage endpoints
- Ensure cleanup logic works correctly with favorite state management
- Maintain integration with saveFavorite functionality
- Apply refined patterns from previous endpoint migrations

**Critical Interface:**
```typescript
interface InterceptRemoveFavoriteOptions {
  filesystem?: MockFilesystem;
  errorMode?: boolean;
  responseDelayMs?: number;
}
```

**Testing Focus for Task 3:**

**Behaviors to Test:**
- [ ] Favorite removal succeeds for existing favorites
- [ ] Removal errors are handled appropriately for non-existent favorites
- [ ] waitForRemoveFavorite() functions correctly
- [ ] Favorite state cleanup works properly
- [ ] Integration with saveFavorite functionality maintains consistency

**Testing Reference:**
- Test favorite removal scenarios in favorite functionality test files
- Validate cleanup in multi-operation favorite scenarios

</details>

---

<details open>
<summary><h3>Task 4: Migrate Storage Indexing Endpoints</h3></summary>

**Purpose**: Create self-contained indexing endpoint files with focus on batch operations and complex error handling patterns.

**Related Documentation:**
- [INTERCEPTOR_FORMAT.md](./INTERCEPTOR_FORMAT.md) - Format to follow exactly
- [getDirectory.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/getDirectory.interceptors.ts) - Established pattern
- [Current Storage Indexing Implementation](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage-indexing.interceptors.ts) - Source code to migrate

**Implementation Subtasks:**
- [ ] **Create indexStorage.interceptors.ts**: New file for individual device storage indexing
- [ ] **Section 1 - Endpoint Definition**: Create INDEX_STORAGE_ENDPOINT constant
- [ ] **Section 2 - Interface Definitions**: Define InterceptIndexStorageOptions interface
- [ ] **Section 3 - Interceptor Function**: Create interceptIndexStorage() function
- [ ] **Section 4 - Wait Function**: Create waitForIndexStorage() function
- [ ] **Section 5 - Helper Functions**: Include setupIndexStorage() and progress tracking
- [ ] **Section 6 - Export Constants**: Add backward compatibility exports
- [ ] **Create indexAllStorage.interceptors.ts**: New file for batch indexing operations
- [ ] **Batch Processing Logic**: Handle complex batch scenarios with multiple devices
- [ ] **Update Test Imports**: Update device-indexing.cy.ts to use new explicit imports
- [ ] **Remove from Old Files**: Remove indexing endpoints from storage-indexing.interceptors.ts

**Testing Subtask:**
- [ ] **Write Tests**: Validate individual indexing scenarios work correctly
- [ ] **Batch Operation Testing**: Test complex batch indexing scenarios
- [ ] **Error Pattern Testing**: Test error handling consistency across indexing operations

**Key Implementation Notes:**
- Focus on batch processing patterns and progress tracking
- Handle complex multi-device indexing scenarios
- Apply all refined patterns from previous endpoint migrations
- Complete storage domain consolidation with comprehensive indexing support

**Critical Interface:**
```typescript
interface InterceptIndexStorageOptions {
  deviceIds?: string[];
  storageTypes?: string[];
  errorMode?: boolean;
  responseDelayMs?: number;
  batchMode?: boolean;
}

interface InterceptIndexAllStorageOptions {
  errorMode?: boolean;
  responseDelayMs?: number;
  partialFailureMode?: boolean;
}
```

**Testing Focus for Task 4:**

**Behaviors to Test:**
- [ ] Individual device indexing returns correct progress states
- [ ] Batch indexing handles multiple devices correctly
- [ ] Indexing errors are handled consistently across endpoints
- [ ] waitForIndexStorage() and waitForIndexAllStorage() work with timing scenarios
- [ ] Progress tracking works correctly during long indexing operations

**Testing Reference:**
- Test device indexing scenarios in device-indexing.cy.ts
- Validate batch operations and progress tracking patterns

</details>

---

<details open>
<summary><h3>Task 5: Storage Domain Integration</h3></summary>

**Purpose**: Ensure all storage endpoints work cohesively and validate complex cross-interceptor scenarios involving filesystem operations, favorite management, and batch indexing.

**Related Documentation:**
- [All Created Storage Interceptors](../../../apps/teensyrom-ui-e2e/src/support/interceptors/) - Complete set of migrated endpoints
- [Storage Test Files](../../../apps/teensyrom-ui-e2e/src/e2e/player/ and ../../../apps/teensyrom-ui-e2e/src/e2e/devices/) - All storage domain tests
- [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Integration testing patterns
- [Phase 2 Device Integration](./E2E_INTERCEPTOR_REFACTOR_P2.md) - Learn from device domain integration

**Implementation Subtasks:**
- [ ] **Run Full Storage Test Suite**: Execute all storage-related E2E tests
- [ ] **Cross-Endpoint Validation**: Test scenarios using multiple storage endpoints (browse → favorite → index)
- [ ] **Import Pattern Validation**: Verify all explicit imports work correctly across all test files
- [ ] **Performance Baseline**: Establish performance metrics for storage domain with filesystem operations
- [ ] **Complex Scenario Testing**: Validate complete user workflows involving storage operations
- [ ] **Cleanup Old Files**: Remove storage.interceptors.ts and storage-indexing.interceptors.ts if all endpoints migrated
- [ ] **Update Documentation**: Update storage domain documentation with new patterns

**Testing Subtask:**
- [ ] **Write Integration Tests**: Test complete storage domain workflows
- [ ] **Filesystem State Testing**: Test filesystem consistency across complex operations
- [ ] **Performance Regression Tests**: Compare performance with baseline
- [ ] **Documentation Validation**: Ensure all documentation reflects new structure

**Key Implementation Notes:**
- This task validates the entire storage domain migration
- Focus on complex filesystem operations and state management scenarios
- Establish baseline metrics for future domain migrations
- Ensure all storage test files work with new import patterns
- Validate complex workflows like: device discovery → directory browsing → favorite management → indexing

**Testing Focus for Task 5:**

**Behaviors to Test:**
- [ ] Complete storage workflows (browse → favorite → index) work end-to-end
- [ ] Filesystem state remains consistent across multiple storage operations
- [ ] Error handling is consistent across all storage endpoints
- [ ] Performance meets or exceeds baseline metrics with complex filesystem operations
- [ ] Cross-domain integration with device endpoints works correctly

**Testing Reference:**
- Run complete storage test suite: `npx nx e2e teensyrom-ui-e2e --spec="src/e2e/player/*.cy.ts" --spec="src/e2e/devices/device-indexing.cy.ts"`
- Validate integration patterns and cross-endpoint scenarios

</details>

---

## 🗂️ Files Modified or Created

> List all files that will be changed or created during this phase with full relative paths from project root.

**New Files:**
- `apps/teensyrom-ui-e2e/src/support/interceptors/getDirectory.interceptors.ts`
- `apps/teensyrom-ui-e2e/src/support/interceptors/saveFavorite.interceptors.ts`
- `apps/teensyrom-ui-e2e/src/support/interceptors/removeFavorite.interceptors.ts`
- `apps/teensyrom-ui-e2e/src/support/interceptors/indexStorage.interceptors.ts`
- `apps/teensyrom-ui-e2e/src/support/interceptors/indexAllStorage.interceptors.ts`

**Modified Files:**
- `apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts` (remove migrated endpoints)
- `apps/teensyrom-ui-e2e/src/support/interceptors/storage-indexing.interceptors.ts` (remove migrated endpoints)
- `apps/teensyrom-ui-e2e/src/support/constants/api.constants.ts` (remove migrated constants)
- `apps/teensyrom-ui-e2e/src/e2e/player/favorite-functionality.cy.ts` (update imports)
- `apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts` (update imports)
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts` (update imports)
- `apps/teensyrom-ui-e2e/src/e2e/storage/test-helpers.ts` (update imports)

**Referenced Files (Not Modified):**
- `docs/features/e2e-testing/INTERCEPTOR_FORMAT.md`
- `apps/teensyrom-ui-e2e/src/support/interceptors/examples/sampleEndpoint.interceptors.ts`
- `apps/teensyrom-ui-e2e/src/support/storage.generators.ts` (MockFilesystem class)

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
> - **Favor behavioral testing** - test what users/consumers observe, not how it's implemented
> - **Test as you go** - tests are integrated into each task's subtasks, not deferred to the end
> - **Test through public APIs** - components, stores, services should be tested through their public interfaces
> - **Mock at boundaries** - mock external dependencies (HTTP, infrastructure services), not internal logic

> **Reference Documentation:**
> - **All tasks**: [Testing Standards](../../TESTING_STANDARDS.md) - Core behavioral testing approach
> - **E2E Testing**: [E2E Testing Overview](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - E2E-specific patterns

### Where Tests Are Written

**Tests are embedded in each task above** with:
- **Testing Subtask**: Checkbox in the task's subtask list (e.g., "Write Tests: Test behaviors for this task")
- **Testing Focus**: "Behaviors to Test" section listing observable outcomes
- **Testing Reference**: Links to relevant testing documentation

**Complete each task's testing subtask before moving to the next task.**

### Test Execution Commands

**Running Tests:**
```bash
# Run all storage tests
npx nx e2e teensyrom-ui-e2e --spec="src/e2e/player/*.cy.ts" --spec="src/e2e/devices/device-indexing.cy.ts"

# Run specific storage test files
npx nx e2e teensyrom-ui-e2e --spec="src/e2e/player/favorite-functionality.cy.ts"
npx nx e2e teensyrom-ui-e2e --spec="src/e2e/player/deep-linking.cy.ts"
npx nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/device-indexing.cy.ts"

# Run all E2E tests
npx nx e2e teensyrom-ui-e2e
```

</details>

---

## ✅ Success Criteria

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Storage Endpoint Migrations:**
- [ ] getDirectory.interceptors.ts created and fully functional with filesystem integration
- [ ] saveFavorite.interceptors.ts created and fully functional with state management
- [ ] removeFavorite.interceptors.ts created and fully functional with cleanup logic
- [ ] indexStorage.interceptors.ts created and fully functional for individual indexing
- [ ] indexAllStorage.interceptors.ts created and fully functional for batch operations

**Testing Requirements:**
- [ ] All storage domain tests continue passing after migration
- [ ] Cross-endpoint integration scenarios work correctly (browse → favorite → index)
- [ ] No test failures introduced by migration
- [ ] Performance meets or exceeds baseline metrics with filesystem operations
- [ ] Complex filesystem state management works correctly

**Code Quality:**
- [ ] All new files follow INTERCEPTOR_FORMAT.md guidelines exactly
- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors
- [ ] Code follows established coding standards

**Migration Requirements:**
- [ ] All explicit import patterns work correctly across all test files
- [ ] Backward compatibility maintained during transition
- [ ] Old scattered code properly removed from storage.interceptors.ts and storage-indexing.interceptors.ts
- [ ] Documentation updated to reflect new structure

**Domain Integration:**
- [ ] All storage endpoints work cohesively
- [ ] Cross-interceptor scenarios validated (complex workflows)
- [ ] Complete storage workflows tested end-to-end
- [ ] Filesystem state management works correctly across operations
- [ ] Integration with device endpoints from Phase 2 works seamlessly

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] Storage domain patterns established for other domains
- [ ] No regressions or known issues
- [ ] Ready to proceed to Phase 4 (Player & Indexing Domain Consolidation)

---

## 📝 Notes & Considerations

### Design Decisions

- **Filesystem-First Approach**: Prioritizing realistic filesystem mock integration for complex directory structure testing
- **State Management Focus**: Emphasis on consistent favorite and filesystem state across multiple operations
- **Batch Processing Patterns**: Special attention to complex indexing scenarios and progress tracking
- **Cross-Domain Integration**: Building on Phase 2 device patterns to ensure seamless device-to-storage workflows

### Implementation Constraints

- **MockFilesystem Integration**: Must maintain compatibility with existing MockFilesystem class and storage generators
- **Complex Path Resolution**: Handle nested directories, special cases, and error scenarios in getDirectory operations
- **State Synchronization**: Ensure filesystem and favorite state remain consistent across multiple endpoint interactions
- **Performance Requirements**: Complex filesystem operations must not introduce performance regressions

### Open Questions

- **Batch Operation Complexity**: How should we handle partial failures in batch indexing operations across multiple devices?
- **Filesystem State Management**: What consistent patterns should we establish for maintaining filesystem state across multiple storage operations?
- **Cross-Domain Dependencies**: How should storage endpoints integrate with device endpoints from Phase 2 for complete user workflows?

### Future Enhancements

- **Enhanced Filesystem Testing**: Potential for more sophisticated filesystem scenarios and edge case testing
- **Performance Optimization**: Opportunities for optimizing complex filesystem and batch operations
- **Advanced State Management**: Enhanced patterns for complex state synchronization across storage operations
- **Cross-Domain Patterns**: Established patterns for Phase 4 player domain integration

### External References

- [Phase 2 Device Migration](./E2E_INTERCEPTOR_REFACTOR_P2.md) - Foundation and patterns from device domain
- [Format Documentation](./INTERCEPTOR_FORMAT.md) - Format guidelines and patterns
- [Sample Example](../../../apps/teensyrom-ui-e2e/src/support/interceptors/examples/sampleEndpoint.interceptors.ts) - Reference implementation
- [Storage Generators](../../../apps/teensyrom-ui-e2e/src/support/storage.generators.ts) - MockFilesystem and file generation utilities

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery 1**: [Something learned during implementation that affects approach]
- **Discovery 2**: [Unexpected complexity or simplification found]