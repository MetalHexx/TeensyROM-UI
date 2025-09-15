# Phase 7: Fix Global Selected Directory State - Per-Device Selection

**High Level Plan Documentation**: [Basic Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [Coding Standards](../../CODING_STANDARDS.md)
- **Store Testing**: [Store Testing](../../STORE_TESTING.md) [State Standards](../../STATE_STANDARDS.md)
- **Testing Standards**: [Testing Standards](../../TESTING_STANDARDS.md)

## 🎯 Objective

Refactor the storage store from a single global `selectedDirectory` state to per-device selection model. This enables independent navigation state management for each connected device, improving UX and supporting multi-device scenarios.

## 📚 Required Reading

- [x] [Player Component Structure](../../../libs/features/player/src/PLAYER_COMPONENTS.md)
- [x] [Storage Store Implementation](../../../libs/domain/storage/state/src/lib/storage-store.ts)
- [x] [Storage Store Tests](../../../libs/domain/storage/state/src/lib/storage-store.spec.ts)
- [x] [State Standards](../../STATE_STANDARDS.md)
- [x] [Store Testing Guidelines](../../STORE_TESTING.md)

## 📋 Implementation Tasks

### Task 1: Storage Store Core Refactor

**Purpose**: Update storage state structure and methods to support per-device selected directories.

- [x] Update `StorageState` interface to replace `selectedDirectory: SelectedDirectory | null` with `selectedDirectories: Record<string, SelectedDirectory>`
- [x] Update `initialState` to use `selectedDirectories: {}` instead of `selectedDirectory: null`
- [x] Refactor `selectedDirectoryState` computed signal to factory pattern: `getSelectedDirectoryState: (deviceId: string) => computed<StorageDirectoryState | null>`
- [x] Update `initializeStorage` method to set `selectedDirectories[deviceId]` instead of global selection
- [x] Update `navigateToDirectory` method to update `selectedDirectories[deviceId]` for the specific device
- [x] Update `cleanupStorage` method to remove `selectedDirectories[deviceId]` when device disconnects
- [x] Add helper method `getSelectedDirectoryForDevice(deviceId: string)` for convenient access

### Task 2: Storage Store Tests Updates

**Purpose**: Update all existing tests to work with per-device selection model and add multi-device coverage.

- [x] Update all test assertions from `store.selectedDirectory()` to `store.selectedDirectories()[deviceId]`
- [x] Update computed signal tests to use factory pattern `getSelectedDirectoryState(deviceId)()`
- [x] Update `initializeStorage` tests to verify per-device selection setting
- [x] Update `navigateToDirectory` tests to verify per-device selection updates
- [x] Update `cleanupStorage` tests to verify device selection removal
- [x] Add multi-device selection independence tests (verify Device A selection doesn't affect Device B)
- [x] Add concurrent device operations tests
- [x] Update edge case tests for empty deviceId handling

### Task 3: Component Updates

**Purpose**: Update components to work with per-device selection model and maintain device context.

- [x] Update `PlayerViewComponent` effect to work with per-device selections (if needed)
- [x] Identify components that consume `selectedDirectory` and update to use device-scoped access
- [x] Update `StorageContainerComponent` to accept deviceId prop and use device-scoped selection
- [x] Update `DirectoryTreeComponent` to use device-scoped selection via device context
- [x] Update `DirectoryFilesComponent` to use device-scoped selection via device context
- [x] Ensure device context is properly passed through component hierarchy
- [x] Add deviceId parameter to any component methods that interact with selection

## 🗂️ File Changes

- [Storage Store](../../../libs/domain/storage/state/src/lib/storage-store.ts)
- [Storage Store Tests](../../../libs/domain/storage/state/src/lib/storage-store.spec.ts)
- [Initialize Storage Method](../../../libs/domain/storage/state/src/lib/methods/initialize-storage.ts)
- [Navigate to Directory Method](../../../libs/domain/storage/state/src/lib/methods/navigate-to-directory.ts)
- [Cleanup Storage Method](../../../libs/domain/storage/state/src/lib/methods/cleanup-storage.ts)
- [Player View Component](../../../libs/features/player/src/lib/player-view/player-view.component.ts)
- [Storage Container Component](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.ts) (if exists)
- [Directory Tree Component](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.ts) (if exists)
- [Directory Files Component](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.ts) (if exists)

## 🧪 Testing Requirements

### Unit Tests

- [x] Per-device selection independence (Device A selection doesn't affect Device B)
- [x] Selection persistence during device operations (navigate, refresh)
- [x] Selection cleanup on device disconnect
- [x] Computed signal reactivity with device-scoped parameters
- [x] Edge cases: empty deviceId, non-existent device selection
- [x] Multi-device concurrent operations
- [x] Selection state consistency after store method calls

### Integration Tests

- [x] End-to-end device connection → initialization → selection → navigation flow
- [x] Multi-device selection management in PlayerViewComponent effect
- [x] Component hierarchy properly maintains device context for selection
- [x] Device disconnect/reconnect preserves independent selections
- [x] Error handling maintains per-device selection integrity

## ✅ Success Criteria

- [x] Multiple devices can maintain independent selected directories simultaneously
- [x] Switching between devices preserves each device's navigation context
- [x] All existing functionality works with per-device selection model
- [x] All storage store tests pass with updated per-device assertions
- [x] Components properly consume device-scoped selection state
- [x] Device disconnect/reconnect handles selection state correctly
- [x] No regressions in storage navigation behavior
- [x] Ready to proceed with UI component implementation phases

## 📝 Notes

- This is a foundational change that affects the core storage state management
- Components will need device context to access correct selection - consider prop drilling vs context patterns
- Consider backward compatibility if any external consumers exist
- The change improves scalability for future multi-device features
- Test coverage should be comprehensive as this affects all storage navigation functionality
