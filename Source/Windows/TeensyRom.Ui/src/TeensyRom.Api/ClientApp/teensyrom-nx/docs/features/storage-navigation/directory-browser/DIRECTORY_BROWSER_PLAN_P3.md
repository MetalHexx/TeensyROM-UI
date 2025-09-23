# Phase 3: Update Existing Actions

**High Level Plan Documentation**: [Directory Browser Plan](./DIRECTORY_BROWSER_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../../CODING_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../../STORE_TESTING.md)
- **State Standards**: [STATE_STANDARDS.md](../../../STATE_STANDARDS.md)

## 🎯 Objective

Integrate navigation history tracking into existing storage navigation actions to ensure all directory changes are properly recorded in browsing history. This phase modifies existing actions without changing their external interfaces or breaking existing functionality.

## 📚 Required Reading

- [ ] [STATE_STANDARDS.md](../../../STATE_STANDARDS.md) - Action patterns and cross-store communication
- [ ] [STORE_TESTING.md](../../../STORE_TESTING.md) - Testing methodology for action updates
- [ ] [Phase 2 Implementation](./DIRECTORY_BROWSER_PLAN_P2.md) - History helper functions
- [ ] [Existing storage actions](../../../../libs/domain/storage/state/src/lib/storage-store.ts) - Current action implementations

## 📋 Implementation Tasks

### Task 1: Update Initialize Storage Action

**Purpose**: Add initial directory to navigation history when storage is first initialized.

- [ ] Modify `initializeStorage` action to call device store for history initialization
- [ ] Add history entry for root directory (/) when storage is successfully initialized
- [ ] Ensure cross-store communication follows STATE_STANDARDS.md patterns
- [ ] Maintain existing action interface and behavior
- [ ] Add appropriate action message tracking with `createAction()`

**Integration Points**:

- Call device store `addHistoryEntry()` method after successful storage initialization
- Use root path ("/") as initial history entry
- Pass deviceId and storageType from action payload

### Task 2: Update Navigate to Directory Action

**Purpose**: Record directory navigation in browsing history for all navigate-to-directory operations.

- [ ] Modify `navigateToDirectory` action to update navigation history
- [ ] Add history entry for target directory after successful navigation
- [ ] Handle both successful and failed navigation scenarios
- [ ] Ensure history is only updated on successful directory loading
- [ ] Maintain existing error handling and loading states

**Integration Points**:

- Call device store `addHistoryEntry()` method after successful directory load
- Use target path from action payload as history entry
- Only update history on successful API response

### Task 3: Update Navigate Up One Directory Action

**Purpose**: Track parent directory navigation in browsing history.

- [ ] Modify `navigateUpOneDirectory` action to record parent navigation
- [ ] Calculate parent path and add to navigation history
- [ ] Handle root directory edge cases (can't navigate up from /)
- [ ] Maintain existing parent path calculation logic
- [ ] Add history entry after successful parent directory load

**Integration Points**:

- Call device store `addHistoryEntry()` method with calculated parent path
- Use existing parent path calculation logic
- Handle root directory boundary conditions

### Task 4: Cross-Store Communication Setup

**Purpose**: Establish proper communication between storage store and device store for history tracking.

- [ ] Add device store injection to storage store if not already present
- [ ] Follow STATE_STANDARDS.md patterns for cross-store communication
- [ ] Ensure proper dependency injection and store access
- [ ] Add appropriate error handling for cross-store operations
- [ ] Document cross-store communication patterns

**Implementation Approach**:

- Inject device store into storage store constructor/setup
- Call device store methods from storage actions
- Handle cases where device store operations might fail
- Maintain loose coupling between stores

### Task 5: Action Message Tracking Enhancement

**Purpose**: Update existing actions to include proper message tracking for Redux DevTools correlation.

- [ ] Add `createAction()` calls to existing actions for message tracking
- [ ] Ensure action messages clearly identify the operation being performed
- [ ] Follow established message tracking patterns from STATE_STANDARDS.md
- [ ] Update existing actions to use consistent messaging approach
- [ ] Add history tracking actions to message correlation

**Message Examples**:

- "initializeStorage: Adding initial directory to navigation history"
- "navigateToDirectory: Recording directory navigation in history"
- "navigateUpOneDirectory: Adding parent directory to navigation history"

### Task 6: Error Handling and Rollback

**Purpose**: Ensure robust error handling when history operations fail.

- [ ] Add try-catch blocks around device store history calls
- [ ] Implement appropriate fallback behavior when history tracking fails
- [ ] Log warnings for history tracking failures without breaking navigation
- [ ] Ensure main navigation functionality works even if history tracking fails
- [ ] Add unit tests for error scenarios

**Error Scenarios**:

- Device store history methods throw exceptions
- Invalid device ID or storage type parameters
- Device store is unavailable or not initialized
- History helper functions return error states

## 🗂️ File Changes

- [libs/domain/storage/state/src/lib/storage-store.ts](../../../../libs/domain/storage/state/src/lib/storage-store.ts) - Updated actions with history tracking
- [libs/domain/storage/state/src/lib/storage-store.spec.ts](../../../../libs/domain/storage/state/src/lib/storage-store.spec.ts) - Updated tests for modified actions

## 🧪 Testing Requirements

### Unit Tests

- [ ] Test `initializeStorage` adds initial directory to history
- [ ] Test `navigateToDirectory` records navigation in history
- [ ] Test `navigateUpOneDirectory` adds parent directory to history
- [ ] Test history tracking doesn't occur on failed navigation
- [ ] Test error handling when device store operations fail
- [ ] Test existing functionality remains unchanged
- [ ] Test cross-store communication works correctly
- [ ] Test action message tracking includes history operations

### Integration Tests

- [ ] Test complete navigation flow updates history correctly
- [ ] Test multiple navigation operations create proper history sequence
- [ ] Test storage and device stores work together correctly
- [ ] Test history tracking doesn't impact performance
- [ ] Test error scenarios don't break existing navigation

### Regression Tests

- [ ] Verify all existing storage store functionality continues to work
- [ ] Test existing components that use storage actions are unaffected
- [ ] Verify no breaking changes to storage store public interface
- [ ] Test backward compatibility with existing usage patterns

## ✅ Success Criteria

- [ ] All existing storage actions enhanced with history tracking
- [ ] Navigation history properly recorded for all directory changes
- [ ] Cross-store communication follows established patterns
- [ ] No breaking changes to existing storage store interface
- [ ] Comprehensive error handling for history tracking failures
- [ ] Complete test coverage for updated actions
- [ ] Action message tracking includes history operations
- [ ] Ready for Phase 4 (new navigation actions) implementation

## 📝 Implementation Details

### Cross-Store Communication Pattern

Following STATE_STANDARDS.md, the storage store will communicate with the device store using dependency injection:

```typescript
export class StorageStore {
  constructor(
    private apiClient: ApiClient,
    private deviceStore: DeviceStore // Add device store injection
  ) {
    // Store setup
  }

  async navigateToDirectory(payload: NavigateToDirectoryPayload) {
    try {
      // Existing navigation logic
      const result = await this.apiClient.getDirectory(payload);

      // Update local state
      this.updateDirectoryState(result);

      // Add to navigation history
      this.deviceStore.addHistoryEntry({
        deviceId: payload.deviceId,
        entry: {
          deviceId: payload.deviceId,
          storageType: payload.storageType,
          path: payload.path,
        },
      });
    } catch (error) {
      // Handle errors without affecting main functionality
    }
  }
}
```

### Error Handling Strategy

- History tracking failures should not break main navigation functionality
- Log warnings for history tracking errors but continue normal operation
- Use try-catch blocks around all device store calls
- Provide fallback behavior when history tracking is unavailable

### Action Message Integration

Each updated action will include action messages for history operations:

```typescript
patchState(state, {
  isLoading: true,
  error: null,
  [createAction('initializeStorage: Starting storage initialization')]
});

// After successful initialization
this.deviceStore.addHistoryEntry(historyPayload);
patchState(state, {
  [createAction('initializeStorage: Added initial directory to navigation history')]
});
```

## 🔗 Related Documentation

- **Phase 2**: [Storage Helper Functions](./DIRECTORY_BROWSER_PLAN_P2.md)
- **Phase 4**: [New Navigation Actions](./DIRECTORY_BROWSER_PLAN_P4.md)
- **State Standards**: [STATE_STANDARDS.md](../../../STATE_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../../STORE_TESTING.md)
