# Phase 5: Search Context Integration with Player Domain

## 🎯 Objective

Integrate search results from StorageStore into PlayerStore's file context to enable proper player navigation (next/previous) within search results. This phase establishes the connection between search operations and player playback, allowing users to navigate through search results as if they were browsing a directory.

## 📚 Required Reading

- [ ] [Search Plan](./SEARCH_PLAN.md) - Overall feature architecture
- [ ] [Phase 1-4 Documents](./SEARCH_P1.md) - Previous search implementation phases
- [ ] [Player Domain Design](../../player-state/PLAYER_DOMAIN_DESIGN.md) - Understanding PlayerFileContext and LaunchMode
- [ ] [State Standards](../../STATE_STANDARDS.md) - NgRx Signal Store patterns
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing methodology

## 🔍 Problem Analysis

### Current State

1. **SearchResultsComponent** double-clicks files and calls `playerContext.launchFileWithContext()` with:
   - `launchMode: LaunchMode.Search`
   - `files: searchState.results` (search results array)
   - `directoryPath: file.parentPath`

2. **PlayerStore** stores this context in `PlayerFileContext`:
   ```typescript
   interface PlayerFileContext {
     storageKey: StorageKey;
     directoryPath: string;
     files: FileItem[];        // This contains search results
     currentIndex: number;
     launchMode: LaunchMode;   // Set to LaunchMode.Search
   }
   ```

3. **Navigation actions** (`navigateNext`, `navigatePrevious`) check launchMode and handle Shuffle/Directory modes

### The Primary Issue: Search Mode Not Handled in Navigation ❌

**CRITICAL BUG DISCOVERED**: The navigation actions (`navigate-next.ts` and `navigate-previous.ts`) only handle two launch modes:

```typescript
if (launchMode === LaunchMode.Shuffle) {
  // Launch random file
} else if (launchMode === LaunchMode.Directory && fileContext) {
  // Navigate through directory files
} else {
  logInfo(LogType.Info, `No file context available for navigation`);
}
```

**Missing**: No case for `LaunchMode.Search`! 

When a file is launched from search results with `LaunchMode.Search`, the navigation actions fall through to the `else` clause and do nothing. The next/previous buttons appear to work (no error) but don't actually navigate.

**Root Cause**: `LaunchMode.Search` was added to the enum but never integrated into the navigation logic.

### Secondary Enhancement Needed:

### Enhancement Needed: Search Context Updates

**Problem**: If the user performs a NEW search while a file is playing, the player's fileContext should update to reflect the new search results without launching a new file.

**Scenario**:
1. User searches "game", gets 10 results
2. User double-clicks result #3, starts playing
3. User searches "mario", gets 5 different results
4. Player is still playing result #3 from old search
5. **Current behavior**: Next/Previous still navigate through old "game" results
6. **Desired behavior**: Next/Previous should navigate through new "mario" results

## 📋 Implementation Plan

### Task 1: Fix Navigation Actions to Handle Search Mode ⚠️ CRITICAL

**Purpose**: Enable next/previous navigation when files are launched from search results.

**Files**: 
- [`libs/application/src/lib/player/actions/navigate-next.ts`](../../../libs/application/src/lib/player/actions/navigate-next.ts)
- [`libs/application/src/lib/player/actions/navigate-previous.ts`](../../../libs/application/src/lib/player/actions/navigate-previous.ts)

**Problem**: Navigation actions don't recognize `LaunchMode.Search`, causing next/previous to silently fail.

**Solution**: Add `LaunchMode.Search` case that uses the same logic as `LaunchMode.Directory` (both iterate through a files array).

**Changes to `navigate-next.ts`**:
```typescript
// BEFORE (lines ~39-68):
if (launchMode === LaunchMode.Shuffle) {
  // ... shuffle logic ...
} else if (launchMode === LaunchMode.Directory && fileContext) {
  // ... directory navigation logic ...
} else {
  logInfo(LogType.Info, `No file context available for navigation on ${deviceId}`);
}

// AFTER:
if (launchMode === LaunchMode.Shuffle) {
  // ... shuffle logic ...
} else if ((launchMode === LaunchMode.Directory || launchMode === LaunchMode.Search) && fileContext) {
  // Navigation logic works for both Directory and Search modes
  const modeLabel = launchMode === LaunchMode.Search ? 'Search' : 'Directory';
  const { files, currentIndex, storageKey } = fileContext;
  const nextIndex = (currentIndex + 1) % files.length; // Wraparound
  const nextFile = files[nextIndex];
  
  logInfo(LogType.Info, `${modeLabel} mode: advancing to next file (${nextIndex + 1}/${files.length}) for ${deviceId}`, { nextFile: nextFile.name });

  const { storageType } = StorageKeyUtil.parse(storageKey);
  
  // Launch the file via API
  const launchedFile = await firstValueFrom(
    playerService.launchFile(deviceId, storageType, nextFile.path)
  );

  const isCompatible = launchedFile.isCompatible;
  
  if (!isCompatible) {
    const errorMessage = 'File is not compatible with TeensyROM hardware';
    logError(`Navigate next: File ${launchedFile.name} is incompatible with device ${deviceId}: ${errorMessage}`);
    setDirectoryNavigationFailure(store, deviceId, launchedFile, fileContext, nextIndex, errorMessage, actionMessage);
    return;
  }

  setDirectoryNavigationSuccess(store, deviceId, launchedFile, fileContext, nextIndex, actionMessage);
} else {
  logInfo(LogType.Info, `No file context available for navigation on ${deviceId}`);
}
```

**Changes to `navigate-previous.ts`**:
```typescript
// BEFORE (lines ~39-68):
if (launchMode === LaunchMode.Shuffle) {
  // ... shuffle logic ...
} else if (launchMode === LaunchMode.Directory && fileContext) {
  // ... directory navigation logic ...
} else {
  logInfo(LogType.Info, `No file context available for navigation on ${deviceId}`);
}

// AFTER:
if (launchMode === LaunchMode.Shuffle) {
  // ... shuffle logic ...
} else if ((launchMode === LaunchMode.Directory || launchMode === LaunchMode.Search) && fileContext) {
  // Navigation logic works for both Directory and Search modes
  const modeLabel = launchMode === LaunchMode.Search ? 'Search' : 'Directory';
  const { files, currentIndex, storageKey } = fileContext;
  const previousIndex = currentIndex === 0 ? files.length - 1 : currentIndex - 1; // Wraparound
  const previousFile = files[previousIndex];
  
  logInfo(LogType.Info, `${modeLabel} mode: going to previous file (${previousIndex + 1}/${files.length}) for ${deviceId}`, { previousFile: previousFile.name });

  const { storageType } = StorageKeyUtil.parse(storageKey);
  
  // Launch the file via API
  const launchedFile = await firstValueFrom(
    playerService.launchFile(deviceId, storageType, previousFile.path)
  );

  const isCompatible = launchedFile.isCompatible;
  
  if (!isCompatible) {
    const errorMessage = 'File is not compatible with TeensyROM hardware';
    logError(`Navigate previous: File ${launchedFile.name} is incompatible with device ${deviceId}: ${errorMessage}`);
    setDirectoryNavigationFailure(store, deviceId, launchedFile, fileContext, previousIndex, errorMessage, actionMessage);
    return;
  }

  setDirectoryNavigationSuccess(store, deviceId, launchedFile, fileContext, previousIndex, actionMessage);
} else {
  logInfo(LogType.Info, `No file context available for navigation on ${deviceId}`);
}
```

**Key Points**:
- ✅ **Minimal change**: Just add `|| launchMode === LaunchMode.Search` to the condition
- ✅ **Code reuse**: Search uses exact same navigation logic as Directory
- ✅ **Logging enhancement**: Log shows "Search mode" vs "Directory mode" for debugging
- ✅ **Backwards compatible**: No changes to existing Directory or Shuffle behavior
- ✅ **Immediate fix**: This alone makes search navigation work without any other changes

---

### Task 2: Add `loadSearchContext` Method to PlayerContext Interface

**Purpose**: Provide a method to update player's file context with new search results without launching a file. (OPTIONAL - only needed for dynamic search context updates)

**File**: [`libs/application/src/lib/player/player-context.interface.ts`](../../../libs/application/src/lib/player/player-context.interface.ts)

**Changes**:
```typescript
export interface IPlayerContext {
  // ... existing methods ...
  
  /**
   * Load search results into player file context without launching a file.
   * Updates the navigation context when user performs a new search while a file is playing.
   * Only updates context if current launch mode is Search and a file is currently playing.
   * 
   * @param deviceId - Device identifier
   * @param storageType - Storage type (USB/SD)
   * @param searchResults - Array of FileItem from search results
   * @param currentFile - Currently playing file (to find new index in results)
   */
  loadSearchContext(
    deviceId: string,
    storageType: StorageType,
    searchResults: FileItem[],
    currentFile: FileItem
  ): void;
}
```

**Design Decisions**:
- **Synchronous operation**: No API calls, just state update
- **Conditional update**: Only updates if `launchMode === LaunchMode.Search`
- **Index recalculation**: Finds currently playing file in new results to set correct `currentIndex`
- **Falls back to 0**: If current file not in new results, sets index to 0

---

### Task 3: Implement `loadSearchContext` in PlayerContextService

**Purpose**: Orchestration layer that calls the store action with proper validation. (OPTIONAL - only needed for dynamic search context updates)

**File**: [`libs/application/src/lib/player/player-context.service.ts`](../../../libs/application/src/lib/player/player-context.service.ts)

**Implementation**:
```typescript
loadSearchContext(
  deviceId: string,
  storageType: StorageType,
  searchResults: FileItem[],
  currentFile: FileItem
): void {
  const launchMode = this.store.getLaunchMode(deviceId)();
  
  // Only update context if we're in Search mode
  if (launchMode !== LaunchMode.Search) {
    return;
  }
  
  // Only update if there's a currently playing file
  const currentLaunchedFile = this.store.getCurrentFile(deviceId)();
  if (!currentLaunchedFile) {
    return;
  }
  
  // Find current file's new index in search results
  const newIndex = searchResults.findIndex(file => file.path === currentFile.path);
  const safeIndex = newIndex >= 0 ? newIndex : 0;
  
  // Use existing loadFileContext action to update
  this.store.loadFileContext({
    deviceId,
    storageType,
    directoryPath: currentFile.parentPath,
    files: searchResults,
    currentFileIndex: safeIndex,
    launchMode: LaunchMode.Search,
  });
}
```

**Key Logic**:
- **Guard clause**: Only proceeds if `launchMode === LaunchMode.Search`
- **Current file check**: Only proceeds if a file is currently playing
- **Index calculation**: Finds current file in new results, defaults to 0
- **Reuses existing action**: Calls `loadFileContext` which already exists

---

### Task 4: Call `loadSearchContext` from SearchToolbarComponent

**Purpose**: Update player context when search completes successfully. (OPTIONAL - only needed for dynamic search context updates)

**File**: [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts)

**Changes**:

1. **Inject PlayerContext**:
```typescript
private readonly playerContext = inject(PLAYER_CONTEXT);
```

2. **Update `executeSearch` to call player context after successful search**:
```typescript
executeSearch(): void {
  // ... existing validation ...
  
  // Call storage store search action
  void this.storageStore.searchFiles({
    deviceId: this.deviceId(),
    storageType: storageType,
    searchText: trimmedText,
    filterType: this.currentFilter(),
  }).then(() => {
    // After search completes, check if we should update player context
    this.updatePlayerContextIfNeeded();
  });
}

private updatePlayerContextIfNeeded(): void {
  const searchState = this.searchState();
  const storageType = this.currentStorageType();
  
  // Only proceed if search was successful
  if (!searchState || !storageType || searchState.error) {
    return;
  }
  
  // Get current playing file from player
  const currentFile = this.playerContext.getCurrentFile(this.deviceId())();
  
  // Only update if there's a currently playing file
  if (!currentFile) {
    return;
  }
  
  // Load new search results into player context
  this.playerContext.loadSearchContext(
    this.deviceId(),
    storageType,
    searchState.results,
    currentFile.file
  );
}
```

**Alternative Approach**: Call from `clearSearch` as well to reset to directory mode when clearing.

---

### Task 5: Update `clearSearch` to Reset Player Context

**Purpose**: When user clears search, return player navigation to directory context. (OPTIONAL - may not be necessary)

**File**: [`search-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts)

**Enhancement to `clearSearch` method**:
```typescript
clearSearch(): void {
  const storageType = this.currentStorageType();
  
  if (!storageType) {
    return;
  }
  
  // Clear search state in store
  this.storageStore.clearSearch({
    deviceId: this.deviceId(),
    storageType: storageType,
  });
  
  // Clear local search text
  this.searchText.set('');
  
  // If a file is currently playing in search mode, reload directory context
  this.reloadDirectoryContextIfNeeded();
}

private async reloadDirectoryContextIfNeeded(): Promise<void> {
  const currentFile = this.playerContext.getCurrentFile(this.deviceId())();
  const launchMode = this.playerContext.getLaunchMode(this.deviceId())();
  
  // Only reload if there's a playing file and we're in Search mode
  if (!currentFile || launchMode !== LaunchMode.Search) {
    return;
  }
  
  // Get the directory files from storage store
  const storageType = this.currentStorageType();
  if (!storageType) {
    return;
  }
  
  // Navigate to the file's parent directory to load directory context
  try {
    await this.storageStore.navigateToDirectory({
      deviceId: this.deviceId(),
      storageType: storageType,
      path: currentFile.file.parentPath,
    });
    
    // Get directory files from storage state
    const directoryState = this.storageStore.getDirectoryState(
      this.deviceId(),
      storageType
    )();
    
    if (directoryState?.directory) {
      const files = directoryState.directory.files;
      const currentIndex = files.findIndex(f => f.path === currentFile.file.path);
      
      // Load directory context back into player
      this.playerContext.loadSearchContext(
        this.deviceId(),
        storageType,
        files,
        currentFile.file
      );
      
      // Update launch mode back to Directory
      this.playerContext.toggleShuffleMode(this.deviceId()); // This toggles between Search and Directory
    }
  } catch (error) {
    console.error('Failed to reload directory context:', error);
  }
}
```

**Note**: This is complex. An alternative simpler approach:
- Don't reload directory context
- Let user navigate normally, which will trigger directory reload
- Only update when user performs new search

---

### Task 6: Add Comprehensive Tests

**Purpose**: Ensure search navigation and context loading works correctly in all scenarios.

**Files**: 
- [`libs/application/src/lib/player/player-context.service.spec.ts`](../../../libs/application/src/lib/player/player-context.service.spec.ts)
- [`libs/application/src/lib/player/actions/navigate-next.spec.ts`](../../../libs/application/src/lib/player/actions/navigate-next.spec.ts) (if exists)
- [`libs/application/src/lib/player/actions/navigate-previous.spec.ts`](../../../libs/application/src/lib/player/actions/navigate-previous.spec.ts) (if exists)

**New Test Suites**:

#### A. Navigation with Search Mode Tests (CRITICAL)
```typescript
describe('Phase 5: Search Mode Navigation', () => {
  const deviceId = 'device-search';
  const file1 = createTestFileItem({ name: 'game1.prg', path: '/games/game1.prg' });
  const file2 = createTestFileItem({ name: 'game2.prg', path: '/games/game2.prg' });
  const file3 = createTestFileItem({ name: 'mario.prg', path: '/games/mario.prg' });
  
  describe('navigateNext with LaunchMode.Search', () => {
    it('should navigate to next file in search results', async () => {
      // Arrange: Launch file from search results
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file1,
        directoryPath: '/games',
        files: [file1, file2, file3],
        launchMode: LaunchMode.Search,
      });
      
      // Verify initial state
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('game1.prg');
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Search);
      
      // Act: Navigate next
      await service.next(deviceId);
      
      // Assert: Moved to next file in search results
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('game2.prg');
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Search); // Mode preserved
    });
    
    it('should wrap around to first file when at end of search results', async () => {
      // Arrange: Launch last file in search results
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file3,
        directoryPath: '/games',
        files: [file1, file2, file3],
        launchMode: LaunchMode.Search,
      });
      
      // Act: Navigate next (should wrap to first)
      await service.next(deviceId);
      
      // Assert: Wrapped to first file
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('game1.prg');
    });
    
    it('should maintain fileContext with search results after navigation', async () => {
      // Arrange
      const searchResults = [file1, file2, file3];
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file1,
        directoryPath: '/games',
        files: searchResults,
        launchMode: LaunchMode.Search,
      });
      
      // Act: Navigate next
      await service.next(deviceId);
      
      // Assert: FileContext still contains search results
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.files).toEqual(searchResults);
      expect(fileContext?.currentIndex).toBe(1);
    });
  });
  
  describe('navigatePrevious with LaunchMode.Search', () => {
    it('should navigate to previous file in search results', async () => {
      // Arrange: Launch second file in search results
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file2,
        directoryPath: '/games',
        files: [file1, file2, file3],
        launchMode: LaunchMode.Search,
      });
      
      // Act: Navigate previous
      await service.previous(deviceId);
      
      // Assert: Moved to previous file in search results
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('game1.prg');
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Search);
    });
    
    it('should wrap around to last file when at beginning of search results', async () => {
      // Arrange: Launch first file in search results
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file1,
        directoryPath: '/games',
        files: [file1, file2, file3],
        launchMode: LaunchMode.Search,
      });
      
      // Act: Navigate previous (should wrap to last)
      await service.previous(deviceId);
      
      // Assert: Wrapped to last file
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('mario.prg');
    });
  });
  
  describe('Search vs Directory mode navigation', () => {
    it('should navigate differently between Search and Directory modes', async () => {
      const directoryFiles = [file1, file2, file3];
      const searchResults = [file3, file1]; // Different order, fewer files
      
      // Launch in Directory mode
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file1,
        directoryPath: '/games',
        files: directoryFiles,
        launchMode: LaunchMode.Directory,
      });
      
      await service.next(deviceId);
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('game2.prg'); // Next in directory
      
      // Switch to Search mode with different results
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file1,
        directoryPath: '/games',
        files: searchResults,
        launchMode: LaunchMode.Search,
      });
      
      await service.next(deviceId);
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('mario.prg'); // Next in search results (different!)
    });
  });
});
```

#### B. Dynamic Search Context Loading Tests (OPTIONAL)
```typescript
describe('Phase 5: Dynamic Search Context Updates', () => {
  const deviceId = 'device-search';
  const file1 = createTestFileItem({ name: 'game1.prg', path: '/games/game1.prg' });
  const file2 = createTestFileItem({ name: 'game2.prg', path: '/games/game2.prg' });
  const file3 = createTestFileItem({ name: 'mario.prg', path: '/games/mario.prg' });
  
  describe('loadSearchContext', () => {
    it('should update file context with new search results while file is playing', async () => {
      // Arrange: Launch file with search results
      const initialSearchResults = [file1, file2];
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file1,
        directoryPath: '/games',
        files: initialSearchResults,
        launchMode: LaunchMode.Search,
      });
      
      // Verify initial state
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('game1.prg');
      expect(service.getFileContext(deviceId)()?.files).toHaveLength(2);
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Search);
      
      // Act: Load new search results
      const newSearchResults = [file3, file1];
      service.loadSearchContext(deviceId, StorageType.Usb, newSearchResults, file1);
      
      // Assert: Context updated with new results
      const fileContext = service.getFileContext(deviceId)();
      expect(fileContext?.files).toHaveLength(2);
      expect(fileContext?.files[0].name).toBe('mario.prg');
      expect(fileContext?.files[1].name).toBe('game1.prg');
      expect(fileContext?.currentIndex).toBe(1); // file1 is now at index 1
    });
    
    it('should NOT update context if launch mode is not Search', async () => {
      // Arrange: Launch file in Directory mode
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file1,
        directoryPath: '/games',
        files: [file1, file2],
        launchMode: LaunchMode.Directory,
      });
      
      const originalContext = service.getFileContext(deviceId)();
      
      // Act: Try to load search context
      service.loadSearchContext(deviceId, StorageType.Usb, [file3], file1);
      
      // Assert: Context unchanged
      expect(service.getFileContext(deviceId)()).toEqual(originalContext);
    });
    
    it('should NOT update context if no file is currently playing', () => {
      // Arrange: Initialize player but don't launch any file
      service.initializePlayer(deviceId);
      
      // Act: Try to load search context
      service.loadSearchContext(deviceId, StorageType.Usb, [file1, file2], file1);
      
      // Assert: Context remains null
      expect(service.getFileContext(deviceId)()).toBeNull();
    });
    
    it('should set currentIndex to 0 if current file not in new results', async () => {
      // Arrange: Launch file in search mode
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file1,
        directoryPath: '/games',
        files: [file1, file2],
        launchMode: LaunchMode.Search,
      });
      
      // Act: Load search results without current file
      service.loadSearchContext(deviceId, StorageType.Usb, [file3], file1);
      
      // Assert: Index defaults to 0
      expect(service.getFileContext(deviceId)()?.currentIndex).toBe(0);
    });
    
    it('should preserve launchMode as Search after context update', async () => {
      // Arrange: Launch in search mode
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file1,
        directoryPath: '/games',
        files: [file1, file2],
        launchMode: LaunchMode.Search,
      });
      
      // Act: Load new search context
      service.loadSearchContext(deviceId, StorageType.Usb, [file2, file3], file1);
      
      // Assert: Still in Search mode
      expect(service.getLaunchMode(deviceId)()).toBe(LaunchMode.Search);
    });
  });
  
  describe('Navigation with Search Context', () => {
    it('should navigate next through updated search results', async () => {
      // Arrange: Launch first file in search results
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file1,
        directoryPath: '/games',
        files: [file1, file2],
        launchMode: LaunchMode.Search,
      });
      
      // Update with new search results
      const newResults = [file1, file3, file2];
      service.loadSearchContext(deviceId, StorageType.Usb, newResults, file1);
      
      // Act: Navigate next
      await service.next(deviceId);
      
      // Assert: Moved to next file in NEW search results
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('mario.prg');
    });
    
    it('should navigate previous through updated search results', async () => {
      // Arrange: Launch second file in search results
      await service.launchFileWithContext({
        deviceId,
        storageType: StorageType.Usb,
        file: file2,
        directoryPath: '/games',
        files: [file1, file2, file3],
        launchMode: LaunchMode.Search,
      });
      
      // Update with new search results (different order)
      const newResults = [file3, file1, file2];
      service.loadSearchContext(deviceId, StorageType.Usb, newResults, file2);
      
      // Act: Navigate previous
      await service.previous(deviceId);
      
      // Assert: Moved to previous file in NEW search results
      expect(service.getCurrentFile(deviceId)()?.file.name).toBe('game1.prg');
    });
  });
});
```

---

## 🗂️ File Changes

### New Files
None - uses existing infrastructure

### Modified Files
- [`libs/application/src/lib/player/player-context.interface.ts`](../../../libs/application/src/lib/player/player-context.interface.ts) - Add `loadSearchContext` method signature
- [`libs/application/src/lib/player/player-context.service.ts`](../../../libs/application/src/lib/player/player-context.service.ts) - Implement `loadSearchContext` method
- [`libs/application/src/lib/player/player-context.service.spec.ts`](../../../libs/application/src/lib/player/player-context.service.spec.ts) - Add comprehensive tests
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts) - Call `loadSearchContext` after successful search

### Unchanged Files (Reference Only)
- [`libs/application/src/lib/player/actions/load-file-context.ts`](../../../libs/application/src/lib/player/actions/load-file-context.ts) - Existing action reused
- [`libs/application/src/lib/player/player-store.ts`](../../../libs/application/src/lib/player/player-store.ts) - No changes needed
- [`libs/application/src/lib/storage/storage-store.ts`](../../../libs/application/src/lib/storage/storage-store.ts) - No changes needed

## ✅ Success Criteria

### Critical (Must Have) ⚠️
- [ ] **Navigation actions handle `LaunchMode.Search`** (Task 1)
- [ ] **Next/previous buttons work when file launched from search results** (Task 1)
- [ ] **Navigation maintains search context (doesn't switch to directory files)** (Task 1)
- [ ] **Wraparound works at beginning/end of search results** (Task 1)
- [ ] **Tests cover Search mode navigation** (Task 6)
- [ ] **No breaking changes to existing Directory/Shuffle navigation** (Task 1)

### Optional (Nice to Have)
- [ ] `loadSearchContext` method added to `IPlayerContext` interface (Task 2)
- [ ] `loadSearchContext` implemented in `PlayerContextService` with proper validation (Task 3)
- [ ] Method only updates context when `launchMode === LaunchMode.Search` (Task 3)
- [ ] Method only proceeds if a file is currently playing (Task 3)
- [ ] Current file's index correctly recalculated in new results (Task 3)
- [ ] SearchToolbarComponent calls `loadSearchContext` after successful search (Task 4)
- [ ] Player navigation updates when user performs new search while playing (Tasks 2-4)
- [ ] Tests demonstrate dynamic search context updates (Task 6)

### Validation
- [ ] All unit tests passing
- [ ] Manual testing confirms next/previous work in Search mode
- [ ] No regression in Directory or Shuffle mode navigation

## 📝 Design Decisions

### Why Reuse Directory Navigation Logic for Search?

**Decision**: Make `LaunchMode.Search` use the same navigation logic as `LaunchMode.Directory`

**Rationale**:
- Both modes navigate through an array of files sequentially
- Both support wraparound (loop back to start/end)
- Both use `fileContext.files` and `fileContext.currentIndex`
- Search results are just a filtered/sorted subset of files
- Reduces code duplication and maintenance burden
- Consistent user experience between Directory and Search modes

**Implementation**: Change condition from `launchMode === LaunchMode.Directory` to `(launchMode === LaunchMode.Directory || launchMode === LaunchMode.Search)`

### Why Not Auto-Update on Every Search?

**Decision**: Only update player context explicitly via `loadSearchContext` call from UI component

**Rationale**:
- Separates concerns (search state vs player state)
- Player context service orchestrates the update
- UI component decides when to trigger update
- More explicit and testable

### Why Reuse `loadFileContext` Action?

**Decision**: Don't create a new store action, reuse existing `loadFileContext`

**Rationale**:
- `loadFileContext` already does exactly what we need
- Reduces code duplication
- Maintains consistency with existing patterns
- Action is designed for this use case

### Why Not Clear Search on Navigation?

**Decision**: Don't auto-clear search when user navigates next/previous

**Rationale**:
- User might want to see search results while playing
- Clearing would lose search context unnecessarily
- User can explicitly clear search with clear button
- Maintains UI state until user action

### Index Recalculation Strategy

**Decision**: Find current file in new results, default to 0 if not found

**Rationale**:
- Maintains continuity when current file is in new results
- Graceful fallback when current file filtered out
- Simple and predictable behavior

### When to Update Search Context

**Decision**: Update on successful search completion, not on error

**Rationale**:
- Only valid results should update player context
- Failed searches shouldn't disrupt playing experience
- Maintains last valid search context on error

## 🔄 Alternative Approaches Considered

### Approach 1: Auto-Update via Store Effect
**Considered**: Create rxMethod in player store that watches search state
**Rejected**: Too implicit, harder to test, couples stores unnecessarily

### Approach 2: Update in StorageStore Search Action
**Considered**: Call player context directly from search-files action
**Rejected**: Violates separation of concerns, circular dependency risk

### Approach 3: Create New Store Action
**Considered**: New `updateSearchContext` action in player store
**Rejected**: Duplicates functionality of existing `loadFileContext`

### Approach 4: Clear Search on Stop/Pause
**Considered**: Auto-clear search when playback stops
**Rejected**: User might pause and resume, losing search context is annoying

## 🚀 Future Enhancements (Out of Scope)

- **Search History Navigation**: Navigate back to previous search results
- **Search Result Highlighting**: Highlight which search results user has already played
- **Auto-Load Next Search**: When reaching end of search results, prompt for new search
- **Search Refinement**: Narrow search results while playing without disrupting playback
- **Search Persistence**: Persist search state across application restarts

## 📊 Testing Strategy

### Unit Test Coverage
- **PlayerContextService**: `loadSearchContext` method with all guard clauses
- **Edge Cases**: Empty results, null file, wrong launch mode
- **Index Calculation**: Current file in/not in results
- **Integration**: Navigation works with updated context

### Integration Test Coverage
- **Full Workflow**: Search → Launch → New Search → Navigate
- **Mode Switching**: Directory → Search → Directory
- **Error Scenarios**: Failed search, failed navigation

### Manual Testing Checklist
- [ ] Search for files, double-click to play
- [ ] Perform new search while file playing
- [ ] Use next/previous - should use NEW search results
- [ ] Clear search - navigation should ???
- [ ] Launch file from directory, perform search - no context update

## 🎯 Summary

This phase fixes the critical navigation bug and optionally adds dynamic search context updates:

### Critical Fix (Task 1) ⚠️
**Problem**: Next/Previous buttons don't work when playing files from search results because navigation actions don't handle `LaunchMode.Search`.

**Solution**: Add `|| launchMode === LaunchMode.Search` to the navigation condition in both `navigate-next.ts` and `navigate-previous.ts`. Search navigation then uses the same logic as Directory navigation (iterate through files array).

**Impact**: **This single change makes search navigation fully functional.** All other tasks are optional enhancements.

### Optional Enhancements (Tasks 2-5)
If you want to support **dynamic search context updates** (updating navigation context when user performs a new search while a file is playing), implement:

1. **Adding explicit control** via `loadSearchContext` method
2. **Reusing existing infrastructure** (`loadFileContext` action)
3. **Maintaining separation of concerns** (storage vs player domains)
4. **Enabling dynamic context updates** without relaunching files
5. **Preserving user experience** during search refinement

The critical implementation is minimal (2 lines changed), focused, and leverages existing patterns. Optional enhancements follow the same architectural principles.
