# Phase 3: Component Integration Specification

**Related Documentation**: [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [`CODING_STANDARDS.md`](../../../CODING_STANDARDS.md) - Component and TypeScript standards.
- **Style Guide**: [`STYLE_GUIDE.md`](../../../STYLE_GUIDE.md) - CSS / Style / Theme design specifications and rules.
- **Testing Standards**: [`TESTING_STANDARDS.md`](../../../TESTING_STANDARDS.md) - Unit, integration, and E2E testing patterns.

## Objective

Integrate storage state with existing player components. Phase 3 focuses first on verifying state functionality by emitting a JSON string representation of storage state into the view for quick testing and verification. Tree navigation UI will be implemented in Step 2. Step 3 is TBD.

## Prerequisites

- Phase 1 completed: Storage domain services available
- Phase 2 completed: Storage state management implemented
- Existing player components available for enhancement

## Implementation Steps

### Step 1: JSON Output for State Verification (Primary)

**Purpose**: Verify StorageStore state functionality in the UI by outputting a JSON string of relevant storage data to the view. This enables fast testing and validation of store initialization, updates, and lifecycle handling without building the directory tree UI.

**Approach**:

- Keep component responsibilities minimal: inject StorageStore, initialize/cleanup storage, and expose a read-only JSON string of current storage entries and selected device/storage state.
- Use Angular signals (or an RxJS-to-signal adapter) to compute a JSON string derived from the store for binding to a <pre> or similar element in the template.
- Do not implement tree navigation or file lists in this step — those move to Step 2.

**Tasks**:

1. PlayerViewComponent

   - Inject `StorageStore` alongside existing `DeviceStore`.
   - Initialize storage entries for connected devices using existing initialize method(s).
   - Clean up storage state when devices disconnect.
   - Pass devices to PlayerDeviceContainerComponents.

2. PlayerDeviceContainerComponent

   - Pass device Storage data into StorageContainerComponents

3. StorageContainerComponent

   - Add `@Input() device` to receive device context from parent.
   - Inject `StorageStore`.
   - Listen for
   - Validate storage availability before exposing data.

4. Child components
   - directory-tree.component.ts
     - Display a list of buttons representing the directories in the currentDirectory
     - call state to navigate to the directory when clicked.
     - include a back button to go back up to the parent directory
   - directory-files.component.ts
     - Display JSON representation of of

**Deliverables**:

- PlayerViewComponent updated to initialize/cleanup storage and expose `storageJson`.
- StorageContainerComponent updated with device input and `deviceStorageJson`.
- Simple template additions to render JSON (e.g., <pre>{{ storageJson }}</pre> and <pre>{{ deviceStorageJson }}</pre>).
- Component tests verifying JSON output reflects store state changes (initialization, navigation method calls, refresh, cleanup).

**File Changes**:

```
libs/features/player/src/lib/player-view/
├── player-view.component.ts                    # Add StorageStore injection, lifecycle, storageJson signal
└── player-device-container/
     └── storage-container/
          ├── storage-container.component.ts      # Add device input, StorageStore injection, deviceStorageJson
          ├── directory-tree/
          │   └── directory-tree.component.ts     # No changes in Step 1
          └── directory-files/
                └── directory-files.component.ts    # No changes in Step 1
```

**Testing Notes**:

- Unit tests should assert that `storageJson` contains expected keys and values after store initialization and after calling core methods (navigate/refresh).
- Integration tests may mount PlayerViewComponent to ensure JSON output appears and updates in the DOM.

### Step 2: Tree Navigation UI (moved)

- Implement hierarchical directory tree, files listing, and interactive navigation using the verified StorageStore API.
- Tasks from previous Step 1 (original plan) that built the tree are now part of this step.
- Will include hooking up DirectoryTreeComponent, DirectoryFilesComponent, and SearchToolbarComponent to StorageStore methods (navigateToDirectory, refreshDirectory, etc.).

### Step 3: TBD

- Details will be defined after Step 1 validation and Step 2 planning results.

## Success Criteria (for Phase 3 Step 1)

- JSON representation of storage state renders in PlayerView and StorageContainer views.
- JSON updates in response to store actions (initialize, navigate, refresh, cleanup).
- Tests assert correctness of serialized data and basic lifecycle behavior.
- Ready to proceed to Step 2 tree navigation once verification passes.

## Notes

- This phased approach reduces UI complexity while enabling rapid verification of state logic.
- Keep UI bindings read-only for Step 1 to avoid coupling input handling with unverified navigation logic.
- After Step 1 validation, proceed to implement interactive navigation and richer UI in Step 2.
