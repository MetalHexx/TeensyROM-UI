# Technical Debt

This document tracks known technical debt items that should be addressed in future iterations.

## Testing Infrastructure

### HTTP Backend Mocking in Integration Tests

**Priority**: Medium  
**Effort**: 2-3 days  
**Created**: 2025-01-08

**Issue**: Several integration tests are using real HTTP backends instead of mocked services, which creates dependencies on external services and makes tests less reliable.

**Affected Files**:

- `libs/domain/device/services/src/lib/device.service.integration.spec.ts` - Uses real HTTP client with `localhost:5168`
- `libs/domain/device/services/src/lib/storage.service.integration.spec.ts` - Uses real HTTP client with `localhost:5168`

**Current Problems**:

- Tests require backend API to be running on `localhost:5168`
- Tests are fragile and can fail due to network issues
- Difficult to test error scenarios consistently
- CI/CD pipelines require backend setup
- Slower test execution due to real HTTP calls

**Recommended Solution**:
Migrate to MSW (Mock Service Worker) for all integration tests:

- Replace real HTTP clients with MSW handlers
- Mock all API endpoints used in integration tests
- Create reusable MSW setup utilities
- Test both success and error scenarios with controlled responses
- Follow the patterns established in `TESTING_STANDARDS.md`

**Benefits**:

- Faster, more reliable tests
- Better error scenario coverage
- No backend dependencies
- CI/CD friendly
- Consistent with testing standards

**Breaking Changes**: None - tests will continue to work the same way

### Angular TestBed Initialization Timing in Domain Library Tests

**Priority**: Low  
**Effort**: 1-2 days  
**Created**: 2025-01-09

**Issue**: Domain library tests that import Angular dependencies experience TestBed initialization timing issues, causing the first test in each file to fail with "Need to call TestBed.initTestEnvironment() first".

**Affected Files**:

- `libs/domain/storage/state/src/lib/storage-key.util.spec.ts` - First test fails, 11/12 pass
- `libs/domain/storage/state/src/lib/storage-store.spec.ts` - First test fails, 6/7 pass

**Current Problems**:

- First test in each domain library test file fails due to TestBed timing
- Subsequent tests pass normally, indicating logic is correct
- Tests work properly in application context (app builds successfully)
- Inconsistent test results due to initialization race condition

**Root Cause**:
TestBed environment initialization occurs after the first test begins execution, but before subsequent tests run. The `test-setup.ts` calls `initTestEnvironment()` but timing with individual test execution is inconsistent.

**Current Workaround**:
Tests demonstrate correct implementation logic (11/12 and 6/7 tests pass), and application integration works properly. The failing tests are environment setup issues, not implementation problems.

**Recommended Solution**:

- Investigate TestBed initialization timing in isolated library tests
- Compare with working component tests that use similar patterns
- Consider moving pure utility tests (like StorageKeyUtil) to not require Angular TestBed
- Align test setup with successful patterns from `libs/app/navigation/`

**Benefits**:

- 100% test pass rate for domain libraries
- More reliable CI/CD test execution
- Better developer experience during testing

**Breaking Changes**: None - implementation logic is already correct

---

## Code Quality

### Replace String Literals with Strongly Typed Button Colors

**Priority**: Medium  
**Effort**: 1-2 days  
**Created**: 2025-09-30

**Issue**: Button color properties are currently using string literals instead of strongly typed values, which reduces type safety and can lead to runtime errors from invalid color values.

**Affected Files**:

- `libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/random-roll-button/random-roll-button.component.ts` - Uses `input<IconButtonColor>('normal')`
- `libs/ui/components/src/lib/icon-button/icon-button.component.ts` - Likely has similar string literal usage
- Any other components that accept color properties as inputs

**Current Problems**:

- String literals like `'normal'` can be mistyped without compile-time errors
- No IntelliSense/autocomplete for available color options
- Runtime errors possible from invalid color values
- Inconsistent color naming across components
- Difficult to refactor color values across the codebase

**Example of Current Issue**:

```typescript
getButtonColor = input<IconButtonColor>('normal'); // 'normal' is a magic string
```

**Recommended Solution**:
Create strongly typed color constants and enums:

1. Define `IconButtonColor` enum or const assertion object with all valid colors
2. Replace string literals with typed constants (e.g., `IconButtonColor.NORMAL`)
3. Update all components to use typed color values
4. Create type-safe color utilities and validation
5. Ensure consistent color naming and theming across components

**Example of Desired Solution**:

```typescript
// Define typed colors
export const IconButtonColors = {
  NORMAL: 'normal',
  ERROR: 'error',
  HIGHLIGHT: 'highlight',
  SUCCESS: 'success',
} as const;

export type IconButtonColor = (typeof IconButtonColors)[keyof typeof IconButtonColors];

// Usage with type safety
getButtonColor = input<IconButtonColor>(IconButtonColors.NORMAL);
```

**Benefits**:

- Compile-time type safety for color values
- IntelliSense support and autocomplete
- Easier refactoring and renaming of colors
- Consistent color system across components
- Prevention of runtime errors from typos
- Better maintainability and documentation

**Breaking Changes**: Minor - existing string values will continue to work, but imports may need updating

### Remove Remaining `any` Usage

**Priority**: Low  
**Effort**: 1 day  
**Created**: 2025-01-08

**Issue**: There may still be some `any` types in the codebase that should be replaced with proper TypeScript types.

**Action Items**:

- Search codebase for remaining `any` usage
- Replace with proper types where possible
- Add type annotations for better type safety
- Update ESLint rules to prevent new `any` usage

---

## Architecture

### Create Domain Enum Types for API Client Enums

**Priority**: Medium  
**Effort**: 2-3 days  
**Created**: 2025-01-09

**Issue**: The generated API client contains enum types that create unwanted Angular dependencies in pure utility functions and domain logic that should be framework-agnostic.

**Current Problems**:

- Pure utility functions are forced to import Angular-specific types from `@teensyrom-nx/data-access/api-client`
- Testing pure utility functions becomes difficult due to Angular dependencies
- Domain logic is tightly coupled to API client implementation details
- Violates separation of concerns between domain and infrastructure layers

**Examples**:

- `TeensyStorageType` from API client is used in storage domain services
- `FileItemType` from API client is used in storage mappers and utilities
- Domain logic cannot be tested in isolation without Angular test setup

**Recommended Solution**:

1. Create domain-specific enum types in appropriate domain libraries:
   - `libs/domain/storage/models/` for storage-related enums
   - `libs/domain/device/models/` for device-related enums
2. Create mapper functions to convert between API client enums and domain enums
3. Update domain services and utilities to use domain enums only
4. Keep API client enum usage confined to infrastructure/API layers

**Benefits**:

- Pure domain logic can be tested without Angular dependencies
- Better separation of concerns between domain and infrastructure
- Easier to unit test utility functions
- More maintainable and flexible architecture
- Aligns with domain-driven design principles

**Breaking Changes**: Minor - will require updating imports in affected files

---

## UI/UX

### Player View Responsiveness Issues

**Priority**: Medium
**Effort**: 1-2 days
**Created**: 2025-01-20

**Issue**: The player view layout has general responsiveness problems at various screen resolutions that need further refinement and testing across different device sizes.

**Affected Files**:

- `libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.scss`
- `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.ts`
- `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.ts`

**Current Problems**:

- Layout behavior needs refinement across different screen sizes
- May need additional breakpoint adjustments for optimal user experience
- Component sizing and responsive behavior could be improved

**Recommended Solution**:

- Comprehensive testing across different screen sizes and devices
- Fine-tune responsive breakpoints and component sizing
- Implement additional responsive improvements as needed
- Consider user feedback on layout behavior

**Benefits**:

- Better user experience across all screen sizes
- Optimal layout behavior on various devices
- Smoother responsive transitions
- Improved overall usability

**Breaking Changes**: None - purely layout and responsiveness improvements

### Directory Tree Placeholder Animation Issue

**Priority**: Low
**Effort**: 0.5-1 day
**Created**: 2025-01-19

**Issue**: The directory tree view uses placeholders for unloaded directories to ensure collapse/expand chevrons are always rendered. However, when a directory loads and contains no subdirectories, it causes a quick expand/collapse animation that creates a jarring user experience.

**Affected Files**:

- `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.html`

**Current Problems**:

- Placeholder nodes ensure chevrons are visible before directory loading
- When directory loads with zero subdirectories, node briefly expands then immediately collapses
- Creates visual flicker and poor user experience
- Unnecessary DOM manipulation and animation triggers

**Recommended Solution**:

- Modify directory loading logic to check subdirectory count before rendering
- Only show chevron for directories that actually contain subdirectories
- Remove placeholder nodes or make them conditional based on actual directory contents
- Consider lazy loading of chevron visibility based on directory metadata

**Benefits**:

- Smoother user experience without jarring animations
- Reduced unnecessary DOM updates
- More intuitive visual feedback (chevron only appears when expandable)
- Better performance by avoiding placeholder manipulation

**Breaking Changes**: None - purely visual behavior improvement

---

## Performance

_No current items_

---

## Security

_No current items_

---

## Notes

- Items should be prioritized as: Critical > High > Medium > Low
- Effort estimates: < 1 day (Small), 1-3 days (Medium), 1+ weeks (Large)
- Include creation date for tracking
- Archive completed items to a "Completed" section with resolution date
