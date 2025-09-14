# Phase 3: Component/Store Integration Implementation Plan

**High Level Plan Documentation**: [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

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

- [ ] Import and use `withComputed` from `@ngrx/signals` in `storage-store.ts`
- [ ] Implement computed signals via factory-function pattern
- [ ] `selectedDirectoryState`: computed signal for currently selected directory's StorageDirectoryState
- [ ] `getDeviceStorageEntries(deviceId)`: factory returning filtered entries by device.
- [ ] `getDeviceDirectories(deviceId)`: factory returning directories for tree display

### Task 2: Store Tests For Computed Signals (before component work)

**Purpose**: Create tests for our new computed signals. Follow the guidance in [`STORE_TESTING.md`](../../../STORE_TESTING.md).

- [ ] Add unit tests in `storage-store.spec.ts` covering.
  - [ ] `selectedDirectoryState` selection and reactivity
  - [ ] `getDeviceStorageEntries` filtering accuracy across multiple devices/storage types
  - [ ] `getDeviceDirectories` returns directories-only projections

### Task 3: Core Component Store Integration (deviceId-based, Signals inputs)

**Purpose**: Establish data flow using computed signals and deviceId propagation.

- [ ] PlayerViewComponent

  - [ ] Inject `DeviceStore` and `StorageStore`
  - [ ] Initialize storage entries for connected devices (on init/changes)
  - [ ] Clean up storage state for disconnected devices
  - [ ] Pass `device` signal to `PlayerDeviceContainerComponent` (existing pattern)

- [ ] PlayerDeviceContainerComponent

  - [ ] Keep existing `device` input (signal input)
  - [ ] Extract `deviceId: string` from `device`
  - [ ] Pass `deviceId` signal to `StorageContainerComponent`

- [ ] StorageContainerComponent
  - [ ] Add `deviceId = input.required<string>()` (signal input)
  - [ ] Inject `StorageStore`
  - [ ] Use computed signals to filter data for this device
  - [ ] Pass filtered data to child components (directory-tree.component.ts and directory-files.component.ts)

### Task 4: Directory Tree JSON Display (Signals)

**Purpose**: Display device storage directories above the existing (hardcoded) tree for verification.

- [ ] DirectoryTreeComponent (TS)

  - [ ] Receive filtered device storage data from parent via signal input(s)
  - [ ] Use store computed signals to access directories from all StorageDirectoryState entries
  - [ ] Extract directories only (omit files)

- [ ] DirectoryTreeComponent (HTML)
  - [ ] Add `<pre>` formatted JSON (directories-only) above the Material tree
  - [ ] Show directories for all storage types for the device
  - [ ] Keep existing hardcoded tree intact below the JSON

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
