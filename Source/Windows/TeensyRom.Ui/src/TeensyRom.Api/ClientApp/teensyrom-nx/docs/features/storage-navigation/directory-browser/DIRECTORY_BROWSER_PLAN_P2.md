# Phase 2: Navigation Action Implementation

**High Level Plan Documentation**: [Directory Browser Plan](./DIRECTORY_BROWSER_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../../CODING_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../../STORE_TESTING.md)
- **State Standards**: [STATE_STANDARDS.md](../../../STATE_STANDARDS.md)

## 🎯 Objective

Implement the new navigation actions (navigate-directory-backward and navigate-directory-forward) following the established 1-action-per-file pattern. These actions will provide browser-like back/forward navigation by manipulating NavigationHistory instances and calling existing directory loading when needed.

## 📚 Required Reading

- [ ] [STATE_STANDARDS.md](../../../STATE_STANDARDS.md) - Action implementation patterns and store methods
- [ ] [STORE_TESTING.md](../../../STORE_TESTING.md) - Testing methodology for store actions
- [ ] [Phase 1 Implementation](./DIRECTORY_BROWSER_PLAN_P1.md) - NavigationHistory class in StorageState
- [ ] [navigate-to-directory.ts](../../../../libs/domain/storage/state/src/lib/actions/navigate-to-directory.ts) - Existing action pattern reference

## 📋 Implementation Tasks

### Task 1: Create Navigate Directory Backward Action

**Purpose**: Implement the navigate-directory-backward action following the established action pattern.

- [ ] Create `navigate-directory-backward.ts` in storage state actions folder
- [ ] Follow exact same pattern as `navigate-to-directory.ts` for consistency
- [ ] Implement NavigationHistory manipulation logic within the action
- [ ] Handle boundary conditions (can't go back from beginning of history)
- [ ] Call existing `navigateToDirectory` action when target directory not cached
- [ ] Add comprehensive JSDoc comments and error handling
- [ ] Include action message tracking with `createAction()`

### Task 2: Create Navigate Directory Forward Action

**Purpose**: Implement the navigate-directory-forward action following the established action pattern.

- [ ] Create `navigate-directory-forward.ts` in storage state actions folder
- [ ] Follow exact same pattern as `navigate-to-directory.ts` for consistency
- [ ] Implement NavigationHistory manipulation logic within the action
- [ ] Handle boundary conditions (can't go forward from end of history)
- [ ] Call existing `navigateToDirectory` action when target directory not cached
- [ ] Add comprehensive JSDoc comments and error handling
- [ ] Include action message tracking with `createAction()`

### Task 3: Add New Actions to Storage Store

**Purpose**: Register the new actions in the storage store actions index.

- [ ] Update `actions/index.ts` to import new action functions
- [ ] Add `navigateDirectoryBackward` and `navigateDirectoryForward` to store methods
- [ ] Follow established patterns for action registration
- [ ] Ensure proper TypeScript typing with `WritableStore<StorageState>`
- [ ] Verify service injection patterns match existing actions

### Task 4: NavigationHistory Manipulation Logic

**Purpose**: Implement the core navigation history logic within the actions.

- [ ] **Browser-like behavior**: Moving backward/forward only changes currentIndex
- [ ] **History creation**: Create new NavigationHistory instance if none exists for device
- [ ] **Boundary handling**: Return early if navigation not possible
- [ ] **Target path retrieval**: Get target path from history array at new index
- [ ] **Memory management**: Respect maxHistorySize limits when needed
- [ ] **State updates**: Use `patchState` to update navigationHistory field

### Task 5: Directory Loading Logic Integration

**Purpose**: Duplicate necessary directory loading logic using storage-helpers.ts functions.

- [ ] **Cache-first approach**: Use `isDirectoryLoadedAtPath()` to check if target already loaded
- [ ] **API calls when needed**: Use `firstValueFrom(storageService.getDirectory())` for cache misses
- [ ] **Loading state management**: Use `setLoadingStorage()` and `setStorageLoaded()` helpers
- [ ] **Error handling**: Use `setStorageError()` helper for failed directory loads
- [ ] **Selection updates**: Use `setDeviceSelectedDirectory()` helper to update selection

### Task 6: Action Function Signatures

**Purpose**: Define consistent action interfaces following established patterns.

```typescript
// navigate-directory-backward.ts
export function navigateDirectoryBackward(
  store: WritableStore<StorageState>,
  storageService: IStorageService
) {
  return {
    navigateDirectoryBackward: async ({ deviceId }: { deviceId: string }): Promise<void> => {
      // Implementation
    },
  };
}

// navigate-directory-forward.ts
export function navigateDirectoryForward(
  store: WritableStore<StorageState>,
  storageService: IStorageService
) {
  return {
    navigateDirectoryForward: async ({ deviceId }: { deviceId: string }): Promise<void> => {
      // Implementation
    },
  };
}
```

## 🗂️ File Changes

- [libs/domain/storage/state/src/lib/actions/navigate-directory-backward.ts](../../../../libs/domain/storage/state/src/lib/actions/navigate-directory-backward.ts) - New backward navigation action
- [libs/domain/storage/state/src/lib/actions/navigate-directory-forward.ts](../../../../libs/domain/storage/state/src/lib/actions/navigate-directory-forward.ts) - New forward navigation action
- [libs/domain/storage/state/src/lib/actions/index.ts](../../../../libs/domain/storage/state/src/lib/actions/index.ts) - Updated actions registration

## 🧪 Testing Requirements

Following the established behavioral testing patterns from [storage-store.spec.ts](../../../../libs/domain/storage/state/src/lib/storage-store.spec.ts):

### Unit Tests

- [ ] **navigateDirectoryBackward()**: Test navigation moves backward in history correctly
- [ ] **navigateDirectoryBackward()**: Test boundary condition when at beginning of history (no-op)
- [ ] **navigateDirectoryBackward()**: Test with empty/non-existent NavigationHistory
- [ ] **navigateDirectoryBackward()**: Test calls navigateToDirectory when target not cached
- [ ] **navigateDirectoryBackward()**: Test uses cached data when target already loaded
- [ ] **navigateDirectoryForward()**: Test navigation moves forward in history correctly
- [ ] **navigateDirectoryForward()**: Test boundary condition when at end of history (no-op)
- [ ] **navigateDirectoryForward()**: Test with empty/non-existent NavigationHistory
- [ ] **navigateDirectoryForward()**: Test calls navigateToDirectory when target not cached
- [ ] **navigateDirectoryForward()**: Test uses cached data when target already loaded
- [ ] **NavigationHistory State**: Test NavigationHistory instances created and updated correctly
- [ ] **Error Handling**: Test graceful handling when target directory loading fails

### Integration Tests

- [ ] **Store Actions Registration**: Test new actions accessible from StorageStore
- [ ] **Multi-Device Independence**: Test navigation history isolated per device
- [ ] **Cache Integration**: Test navigation works with existing storage cache behavior
- [ ] **Selection Updates**: Test selectedDirectories updates correctly during navigation
- [ ] **Complete Navigation Flow**: Test forward → back → forward sequences

## ✅ Success Criteria

- [ ] Both navigation actions implemented following established action patterns
- [ ] NavigationHistory manipulation logic correctly implemented within actions
- [ ] Browser-like navigation behavior working (back/forward only changes index)
- [ ] Actions registered in storage store and accessible from components
- [ ] Comprehensive error handling for boundary conditions and edge cases
- [ ] Complete test coverage following storage-store.spec.ts patterns
- [ ] Integration with existing directory loading and caching works seamlessly
- [ ] Ready for Phase 3 (updating existing actions to populate history)

## 📝 Implementation Details

### NavigationHistory Manipulation Pattern

The actions will manipulate NavigationHistory instances using the updateState helper:

```typescript
// Get or create NavigationHistory for device
const currentHistory = store.navigationHistory()[deviceId] || new NavigationHistory();

// Check if navigation is possible
if (currentHistory.currentIndex <= 0) {
  // Already at beginning, cannot go back
  return;
}

// Update index and get target path
const newIndex = currentHistory.currentIndex - 1;
const targetPath = currentHistory.history[newIndex];

// Update NavigationHistory state using updateState helper
const updatedHistory = new NavigationHistory(currentHistory.maxHistorySize);
updatedHistory.history = [...currentHistory.history];
updatedHistory.currentIndex = newIndex;
updatedHistory.maxHistorySize = currentHistory.maxHistorySize;

updateState(store, actionMessage, (state) => ({
  navigationHistory: {
    ...state.navigationHistory,
    [deviceId]: updatedHistory,
  },
}));
```

### Directory Loading Integration

Rather than calling other actions, duplicate the necessary logic using storage-helpers.ts:

```typescript
// After updating history index, load the target directory using helpers
const key = StorageKeyUtil.create(deviceId, storageType);
const existingEntry = getStorage(store, key);

// Check cache first
if (isDirectoryLoadedAtPath(existingEntry, targetPath)) {
  // Just update selection, directory already loaded
  setDeviceSelectedDirectory(store, deviceId, storageType, targetPath, actionMessage);
  return;
}

// Load directory using same pattern as navigate-to-directory.ts
setLoadingStorage(store, key, actionMessage);

try {
  const directory = await firstValueFrom(
    storageService.getDirectory(deviceId, storageType, targetPath)
  );

  setStorageLoaded(
    store,
    key,
    {
      currentPath: targetPath,
      directory,
    },
    actionMessage
  );

  setDeviceSelectedDirectory(store, deviceId, storageType, targetPath, actionMessage);
} catch (error) {
  setStorageError(store, key, 'Failed to load directory from history', actionMessage);
}
```

### Error Handling Strategy

- Use early returns for boundary conditions (no console.warn spam)
- Gracefully handle missing NavigationHistory instances
- Don't break if target directory loading fails
- Maintain existing directory selection if navigation fails

## 🔗 Related Documentation

- **Phase 1**: [Navigation History Class Implementation](./DIRECTORY_BROWSER_PLAN_P1.md)
- **Phase 3**: [Update Existing Actions](./DIRECTORY_BROWSER_PLAN_P3.md)
- **State Standards**: [STATE_STANDARDS.md](../../../STATE_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../../STORE_TESTING.md)
- **Action Reference**: [navigate-to-directory.ts](../../../../libs/domain/storage/state/src/lib/actions/navigate-to-directory.ts)
