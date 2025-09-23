# Phase 6: Enhanced Navigation History with Storage Type Tracking

**High Level Plan Documentation**: [Directory Browser Plan](./DIRECTORY_BROWSER_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../../CODING_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../../STORE_TESTING.md)
- **State Standards**: [STATE_STANDARDS.md](../../../STATE_STANDARDS.md)

## 🎯 Objective

Enhance the NavigationHistory system to track both path and storage type for each history entry, enabling accurate cross-storage-type navigation that remembers which storage device each path belongs to.

## 📚 Problem Analysis

### Current Issue

The existing `NavigationHistory` only tracks paths as strings without associating storage type context:

```typescript
class NavigationHistory {
  history: string[] = []; // ❌ Only paths, no storage type
  currentIndex = -1;
  maxHistorySize: number;
}
```

### Problem Scenario

1. User navigates to `/games` on **SD storage** → History: `['/games']`
2. User switches to **USB storage** and navigates to `/music` → History: `['/games', '/music']`
3. User clicks **back button** → System retrieves `/games` from history
4. ❌ **BUG**: System uses current storage type (USB) instead of original storage type (SD)
5. Result: Attempts to load `/games` on USB instead of SD

### Root Cause

- Navigation actions (`navigateDirectoryBackward`/`navigateDirectoryForward`) get storage type from **current selection** (line 66-72 in both files)
- No association between history entries and their original storage types
- History entries are just path strings with no metadata

## 📋 Implementation Tasks

### Task 1: Create NavigationHistoryItem Interface ✅

**Purpose**: Replace string-based history with structured objects containing path and storage type.

**File**: `libs/domain/storage/state/src/lib/storage-store.ts`

**Changes**:

```typescript
// Add new interface
export interface NavigationHistoryItem {
  path: string;
  storageType: StorageType;
}

// Update NavigationHistory class
export class NavigationHistory {
  history: NavigationHistoryItem[] = []; // ✅ Now includes storage type
  currentIndex = -1;
  maxHistorySize: number;

  constructor(maxHistorySize = 50) {
    this.maxHistorySize = maxHistorySize;
  }
}
```

### Task 2: Update navigate-to-directory.ts ✅

**Purpose**: Create NavigationHistoryItem when adding paths to history.

**File**: `libs/domain/storage/state/src/lib/actions/navigate-to-directory.ts`

**Current Code (lines 71-88)**:

```typescript
updatedHistory.history = [
  ...currentHistory.history.slice(0, currentHistory.currentIndex + 1),
  path, // ❌ Just a string
];
```

**Updated Code**:

```typescript
updatedHistory.history = [
  ...currentHistory.history.slice(0, currentHistory.currentIndex + 1),
  { path, storageType }, // ✅ NavigationHistoryItem
];
```

### Task 3: Update navigate-up-one-directory.ts ✅

**Purpose**: Create NavigationHistoryItem for parent directory navigation.

**File**: `libs/domain/storage/state/src/lib/actions/navigate-up-one-directory.ts`

**Current Code (lines 72-74)**:

```typescript
updatedHistory.history = [
  ...currentHistory.history.slice(0, currentHistory.currentIndex + 1),
  parentPath, // ❌ Just a string
];
```

**Updated Code**:

```typescript
updatedHistory.history = [
  ...currentHistory.history.slice(0, currentHistory.currentIndex + 1),
  { path: parentPath, storageType }, // ✅ NavigationHistoryItem
];
```

### Task 4: Update navigate-directory-backward.ts ✅

**Purpose**: Read storage type from history item instead of current selection.

**File**: `libs/domain/storage/state/src/lib/actions/navigate-directory-backward.ts`

**Current Code (lines 42-43, 65-72)**:

```typescript
const targetPath = currentHistory.history[newIndex]; // ❌ Just string

// Get current selected directory to determine storage type
const currentSelection = store.selectedDirectories()[deviceId];
const { storageType } = currentSelection; // ❌ Wrong storage type
```

**Updated Code**:

```typescript
const targetItem = currentHistory.history[newIndex]; // ✅ NavigationHistoryItem
const targetPath = targetItem.path;
const storageType = targetItem.storageType; // ✅ Correct storage type from history

// No longer need to get current selection for storage type
```

### Task 5: Update navigate-directory-forward.ts ✅

**Purpose**: Read storage type from history item instead of current selection.

**File**: `libs/domain/storage/state/src/lib/actions/navigate-directory-forward.ts`

**Current Code (lines 42-43, 65-72)**:

```typescript
const targetPath = currentHistory.history[newIndex]; // ❌ Just string

// Get current selected directory to determine storage type
const currentSelection = store.selectedDirectories()[deviceId];
const { storageType } = currentSelection; // ❌ Wrong storage type
```

**Updated Code**:

```typescript
const targetItem = currentHistory.history[newIndex]; // ✅ NavigationHistoryItem
const targetPath = targetItem.path;
const storageType = targetItem.storageType; // ✅ Correct storage type from history

// No longer need to get current selection for storage type
```

### Task 6: Update initialize-storage.ts ✅

**Purpose**: Create NavigationHistoryItem when initializing root directory.

**File**: `libs/domain/storage/state/src/lib/actions/initialize-storage.ts`

**Update history initialization to use NavigationHistoryItem structure**.

### Task 7: Update All Store Tests ✅

**Purpose**: Update test expectations for NavigationHistoryItem structure.

**File**: `libs/domain/storage/state/src/lib/storage-store.spec.ts`

**Changes Needed**:

- Update all assertions from `history: ['/path']` to `history: [{ path: '/path', storageType: StorageType.Sd }]`
- Add cross-storage navigation test scenarios
- Test backward/forward navigation correctly switches storage types
- Verify cache keys use history item's storage type

### Task 8: Add Cross-Storage Navigation Tests ✅

**Purpose**: Comprehensive testing of new cross-storage capabilities.

**New Test Scenarios**:

```typescript
it('navigates backward across different storage types correctly', async () => {
  // Navigate SD:/games then USB:/music
  // Back button should return to SD:/games (not USB:/games)
});

it('navigates forward across different storage types correctly', async () => {
  // Navigate SD:/games, USB:/music, then back
  // Forward should return to USB:/music
});

it('maintains separate history entries for same path on different storage', async () => {
  // SD:/games and USB:/games should be distinct history entries
});
```

### Task 9: Component Integration Verification ✅

**Purpose**: Ensure Phase 5 UI components continue working with enhanced navigation.

**Verification Points**:

- ✅ `canNavigateBack`/`canNavigateForward` signals only check index bounds (no changes needed)
- ✅ Navigation event handlers call store actions (actions handle storage type internally)
- ✅ UI updates correctly when storage type changes during navigation

## 🧪 Testing Strategy

### Unit Tests (Store Level)

- ✅ Test NavigationHistoryItem creation with correct storage type
- ✅ Test backward navigation retrieves correct storage type from history
- ✅ Test forward navigation retrieves correct storage type from history
- ✅ Test cross-storage navigation updates selection correctly
- ✅ Test history entries preserve storage type metadata

### Integration Tests (Cross-Storage Scenarios)

- ✅ Test navigation flow: SD → USB → Back (should return to SD)
- ✅ Test navigation flow: SD → USB → Back → Forward (should return to USB)
- ✅ Test mixed storage history maintains correct sequence
- ✅ Test cache isolation between storage types with same paths

### Edge Cases

- ✅ Test backward navigation when history has mixed storage types
- ✅ Test forward navigation when history has mixed storage types
- ✅ Test history size limit with cross-storage entries
- ✅ Test cleanup preserves history integrity

## ✅ Success Criteria

- ✅ **Storage Type Accuracy**: Back/forward navigation uses correct storage type from history
- ✅ **Cross-Storage Navigation**: Users can navigate between SD and USB entries seamlessly
- ✅ **Backward Compatibility**: Single-storage navigation behavior unchanged
- ✅ **UI Reactivity**: Components update when navigation switches storage types
- ✅ **Test Coverage**: All navigation scenarios tested (100% coverage maintained)
- ✅ **No Breaking Changes**: Existing Phase 5 component integration works without modifications

## 📝 Technical Design

### New Data Structure

```typescript
export interface NavigationHistoryItem {
  path: string;
  storageType: StorageType;
}

export class NavigationHistory {
  history: NavigationHistoryItem[] = [];
  currentIndex = -1;
  maxHistorySize: number;

  constructor(maxHistorySize = 50) {
    this.maxHistorySize = maxHistorySize;
  }
}
```

### Enhanced Navigation Flow

**Before (Broken)**:

1. User navigates SD:/games → History: `['/', '/games']`
2. User switches to USB, navigates /music → History: `['/', '/games', '/music']`
3. User clicks back → Gets `/games` from history
4. ❌ Uses **current** storage type (USB) → Tries to load USB:/games (wrong!)

**After (Fixed)**:

1. User navigates SD:/games → History: `[{path: '/', storageType: SD}, {path: '/games', storageType: SD}]`
2. User switches to USB, navigates /music → History: `[..., {path: '/music', storageType: USB}]`
3. User clicks back → Gets `{path: '/games', storageType: SD}` from history
4. ✅ Uses **history** storage type (SD) → Correctly loads SD:/games!

### Storage Type Resolution

**Current (Incorrect)**:

```typescript
// Gets storage type from CURRENT selection
const currentSelection = store.selectedDirectories()[deviceId];
const { storageType } = currentSelection; // ❌ May not match history item
```

**Enhanced (Correct)**:

```typescript
// Gets storage type from HISTORY item
const targetItem = currentHistory.history[newIndex];
const { path, storageType } = targetItem; // ✅ Matches original navigation
```

## 🔗 Related Documentation

- **Phase 4**: [Navigation Actions Implementation](./DIRECTORY_BROWSER_PLAN_P4.md) ✅ COMPLETED
- **Phase 5**: [Component Integration](./DIRECTORY_BROWSER_PLAN_P5.md) ✅ COMPLETED
- **Main Plan**: [Directory Browser Plan](./DIRECTORY_BROWSER_PLAN.md)
- **StorageStore**: [libs/domain/storage/state](../../../../libs/domain/storage/state/)

## 🚀 Phase 6 Status: ✅ COMPLETED

**Implementation Summary**:

- ✅ Created NavigationHistoryItem interface with path and storageType
- ✅ Updated all 6 navigation actions to use new structure
- ✅ Updated all 82 store tests with NavigationHistoryItem expectations
- ✅ Added 3 comprehensive cross-storage navigation tests
- ✅ Verified Phase 5 component integration (41 tests passing)

**Final Test Results**:

- Storage State Tests: 82 passing
- Player Component Tests: 41 passing
- Total: 123 tests passing ✅

**Key Achievement**: Navigation history now correctly preserves storage type context, enabling accurate cross-storage-type back/forward navigation. Users can seamlessly navigate between SD and USB storage, with the system correctly remembering which storage device each path belongs to.

This phase completes the browser-like navigation system by adding true cross-storage-type navigation support! 🎯
