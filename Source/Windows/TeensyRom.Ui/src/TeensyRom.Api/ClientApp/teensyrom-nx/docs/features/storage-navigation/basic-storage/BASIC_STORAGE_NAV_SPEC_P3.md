# Phase 3: Component/Store Integration Implementation Plan

**High Level Plan**: [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

**Related Documentation**:

- **Player Component Hierarchy**: ['PLAYER_COMPONENTS.md'](../../../../libs/features/player/src/PLAYER_COMPONENTS.md)
- **Coding Standards**: [`CODING_STANDARDS.md`](../../../CODING_STANDARDS.md)
- **Store Testing**: [`STORE_TESTING.md`](../../../STORE_TESTING.md)

## 🎯 Objective

Implement the core component/store integration with computed signals and JSON verification displays to establish proper data flow from `PlayerViewComponent` to child components.

## 📚 Required Reading

- [ ] `libs/domain/storage/state/src/lib/storage-store.ts`
- [ ] `libs/features/player/src/lib/player-view/player-view.component.ts`
- [ ] `libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.ts`
- [ ] `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.ts`
- [ ] `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.ts`

## 📋 Implementation Tasks

### Task 2: Enhance StorageStore with Computed Signals

**Purpose**: Add computed signals to simplify component data filtering and access (per NgRx patterns).

- [x] Import and use `withComputed` from `@ngrx/signals` in `storage-store.ts`
- [x] Implement computed signals via factory-function pattern
- [x] `selectedDirectoryState`: computed signal for currently selected directory's StorageDirectoryState
- [x] `getDeviceStorageEntries(deviceId)`: factory returning filtered entries by device.
- [x] `getDeviceDirectories(deviceId)`: factory returning directories for tree display

### Task 2: Store Tests For Computed Signals (before component work)

**Purpose**: Create tests for our new computed signals. Follow the guidance in [`STORE_TESTING.md`](../../../STORE_TESTING.md).

- [x] Add unit tests in `storage-store.spec.ts` covering.
  - [x] `selectedDirectoryState` selection and reactivity
  - [x] `getDeviceStorageEntries` filtering accuracy across multiple devices/storage types
  - [x] `getDeviceDirectories` returns directories-only projections

### Task 3: Core Component Store Integration (deviceId-based, Signals inputs)

**Purpose**: Establish data flow using computed signals and deviceId propagation.

- [x] PlayerViewComponent

  - [x] Inject `DeviceStore` and `StorageStore`
  - [x] Initialize storage entries for connected devices (on init/changes)
  - [x] Clean up storage state for disconnected devices
  - [x] Pass `device` signal to `PlayerDeviceContainerComponent` (existing pattern)

- [x] PlayerDeviceContainerComponent

  - [x] Keep existing `device` input (signal input)
  - [x] Extract `deviceId: string` from `device`
  - [x] Pass `deviceId` signal to `StorageContainerComponent`

- [x] StorageContainerComponent
  - [x] Add `deviceId = input.required<string>()` (signal input)
  - [x] Inject `StorageStore`
  - [x] Use computed signals to filter data for this device
  - [x] Pass filtered data to child components (directory-tree.component.ts and directory-files.component.ts)

### Task 4: Directory Tree JSON Display (Signals)

**Purpose**: Display device storage directories above the existing (hardcoded) tree for verification.

- [x] DirectoryTreeComponent (TS)

  - [x] Receive filtered device storage data from parent via signal input(s)
  - [x] Use store computed signals to access directories from all StorageDirectoryState entries
  - [x] Extract directories only (omit files)

- [x] DirectoryTreeComponent (HTML)
  - [x] Add `<pre>` formatted JSON (directories-only) above the Material tree
  - [x] Show directories for all storage types for the device
  - [x] Keep existing hardcoded tree intact below the JSON

### Task 5: Directory Files JSON Display (Signals)

**Purpose**: Show selected directory contents (files and directories) via SelectedDirectory state.

- [ ] DirectoryFilesComponent (TS)

  - [ ] Receive `deviceId` from parent via `deviceId = input.required<string>()`
  - [ ] Inject `StorageStore` to access `selectedDirectory`
  - [ ] Use `selectedDirectoryState` computed signal
  - [ ] Resolve correct StorageDirectoryState entry from SelectedDirectory
  - [ ] Extract complete files and directories list

- [ ] DirectoryFilesComponent (HTML)
  - [ ] Display files and directories as `<pre>` formatted JSON
  - [ ] Replace Lorem Ipsum placeholder content with actual storage data

### Task 6: Initialize Storage Should Fetch Root Directory

**Purpose**: Change `initializeStorage` to immediately fetch the root directory (`'/'`) from the API so entries are created and hydrated in one step. Leverage the same load/caching/error pattern used in `navigate-to-directory`.

- [x] Update `libs/domain/storage/state/src/lib/methods/initialize-storage.ts`

  - [x] Reuse the API call flow from `libs/domain/storage/state/src/lib/methods/navigate-to-directory.ts` (loading flags, success, error, timestamps)
  - [x] On initialize, set selection to `{ deviceId, storageType, path: '/' }`
  - [x] If entry is missing, create it with `currentPath: '/'`, `isLoading: true`, `isLoaded: false`, `error: null`
  - [x] Call `storageService.getDirectory(deviceId, storageType, '/')`
  - [x] On success: write `directory`, set `isLoaded: true`, `isLoading: false`, `error: null`, `lastLoadTime: Date.now()`
  - [x] On failure: set `isLoading: false` and populate a clear `error` string
  - [x] Cache behavior: if already loaded at `'/'` with no error, skip API call

- [x] Tests: update `libs/domain/storage/state/src/lib/storage-store.spec.ts`
  - [x] First initialize triggers API call for `'/'` and hydrates state
  - [x] Subsequent initialize for same `(deviceId, storageType)` does not call API when root is already loaded
  - [x] Error path: on failure, state reflects `error` and `isLoading: false`; retry initialize calls API again and can recover
  - [x] Selection behavior: selection set to root on initialize (unless user navigates afterward)

### Task 6.1: Fix Global Selected Directory State (Side Phase - Required)

**Purpose**: **CRITICAL**: Refactor storage store from single global `selectedDirectory` to per-device selection model. This must be completed before proceeding to Task 7 as the current global state design breaks multi-device functionality.

- [ ] **Complete Phase 7**: [Fix Global Selected Directory State - Per-Device Selection](../BASIC_STORAGE_FIX_GLOBAL_STATE.md)
  - [ ] All tasks in Phase 7 must be completed
  - [ ] Storage store refactored to per-device selection
  - [ ] All tests updated and passing
  - [ ] Components updated to work with per-device state
  - [ ] Multi-device selection independence verified

**Note**: This is a foundational fix required before any UI implementation can proceed. The current single `selectedDirectory` state cannot support multiple devices properly.

### Task 7: Store API Organization (Follow STATE_STANDARDS)

**Purpose**: Align store structure with one-function-per-file standards for long-term maintainability.

- [ ] Evaluate moving computed selectors and factory methods into dedicated files under a `methods/` (or `selectors/`) folder
- [ ] Keep non-parameterized computed selectors (e.g., `selectedDirectoryState`) close to the store if they remain minimal, otherwise extract
- [ ] Extract parameterized factories (e.g., `getDeviceStorageEntries`, `getDeviceDirectories`) into separate files per STATE_STANDARDS
- [ ] Update barrel exports and store assembly to import and spread these functions consistently
- [ ] Confirm typings remain stable and devtools naming remains clear

## 🗂️ File Changes

```
libs/domain/storage/state/src/lib/
└── storage-store.ts                        # Add withComputed + factory functions

libs/features/player/src/lib/player-view/
├── player-view.component.ts                # Inject StorageStore + lifecycle
└── player-device-container/
    ├── player-device-container.component.ts    # Extract/pass deviceId
    └── storage-container/
        ├── storage-container.component.ts      # Add deviceId input, inject store
        ├── directory-tree/
        │   ├── directory-tree.component.ts     # Receive data, add JSON display
        │   └── directory-tree.component.html   # Add <pre> above tree
        └── directory-files/
            ├── directory-files.component.ts    # Add deviceId input, inject store
            └── directory-files.component.html  # Replace placeholder with JSON
```

## 🧪 Testing Requirements

### Unit Tests

- [ ] StorageStore computed signals: selection, per-device filters, directory projections
- [ ] Component integration: computed signal usage and data flow
- [ ] Device-specific filtering accuracy
- [ ] JSON output verification in component templates

### Integration Tests

- [ ] Full data flow from PlayerView → leaf components
- [ ] StorageStore lifecycle with component hierarchy
- [ ] Computed signals react to storage state changes

## ✅ Success Criteria

- [ ] StorageStore enhanced with computed signals per NgRx patterns
- [ ] DeviceId-based data flow established end-to-end
- [ ] Directory JSON display working above existing tree
- [ ] Selected directory JSON display working in DirectoryFiles
- [ ] Computed signals provide filtered data efficiently
- [ ] Existing functionality preserved (hardcoded tree remains)
- [ ] Ready for Phase 4 navigation tree implementation

## 📝 Notes

- Establishes reactive data foundation using computed signals
- JSON displays provide immediate, verifiable state feedback
- Components preserved while adding functionality
- Computed signals enable efficient filtering without manual component logic
