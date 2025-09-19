# Phase 4: Basic Navigation Tree Implementation

**High Level Plan Documentation**: [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [`CODING_STANDARDS.md`](../../../CODING_STANDARDS.md)
- **Style Guide**: [`STYLE_GUIDE.md`](../../../STYLE_GUIDE.md)
- **Testing Standards**: [`TESTING_STANDARDS.md`](../../../TESTING_STANDARDS.md)
- **State Standards**: [`STATE_STANDARDS.md`](../../../STATE_STANDARDS.md)
- **Storage Store Reference**: [`STORAGE_STATE_MACHINE.md`](../../../../libs/domain/storage/state/src/lib/STORAGE_STATE_MACHINE.md)

## Objective

Deliver an interactive, Material-styled directory tree that surfaces every connected device and its available storage types, keeps previously visited directories visible, and issues navigation requests through `StorageStore.navigateToDirectory`. The component must:

- maintain local expansion state and a component-scoped cache keyed by `{deviceId, storageType, path}` so revisited nodes do not require new fetches;
- render Internal storage first for each device while filtering out unavailable external storage types;
- trigger navigation and selection updates via single-click interactions that coordinate with the file list;
- display loading and error feedback that mirrors the underlying store state without blocking cached content.

## Current Decisions & Context

- Aggregation of directory results stays inside `DirectoryTreeComponent` for now; revisit lifting to the store in a later phase if broader persistence is needed.
- Single-click drives both expansion and navigation actions (no double-click semantics).
- Existing Phase 3 JSON debug output will be removed once the tree is live; file list integration must remain intact.
- Testing focus is on verifying cache persistence, navigation payloads, and availability filtering alongside Material styling compliance.

## Execution Approach

Plan to implement Tasks 1 through 5 sequentially, validating each in turn (starting with data modeling/cache state before wiring the template). This staged flow gives fast feedback on the new caching behavior and keeps later steps straightforward once upstream pieces are proven.

## Required Reading

- [ ] [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)
- [ ] [StorageStore definition](../../../../libs/domain/storage/state/src/lib/storage-store.ts)
- [ ] [navigate-to-directory action](../../../../libs/domain/storage/state/src/lib/actions/navigate-to-directory.ts)
- [ ] [Storage domain models](../../../../libs/domain/storage/services/src/lib/storage.models.ts)
- [ ] [Player component hierarchy](../../../../libs/features/player/src/PLAYER_COMPONENTS.md)

## Implementation Tasks

### Task 1: Tree Data Modeling & State

**Purpose**: Establish the in-component data structures required to render devices, storage types, and directories with expansion and selection state.

- [ ] Define a `DirectoryTreeNode` interface covering device, storage type, and directory nodes with IDs, labels, icons, and children
- [ ] Maintain an in-component cache of visited directories keyed by `{deviceId, storageType, path}` that merges new results from `directories()` while preserving previous branches
- [ ] Create local state for expanded node tracking keyed by node ID
- [ ] Map cached directory data into the tree node view-model without losing previously loaded branches
- [ ] Ensure selection highlights align with `StorageStore` selected directory state

### Task 2: `DirectoryTreeComponent` Template & Interaction

**Purpose**: Render the hierarchical tree using Angular control flow and connect user actions to store navigation.

- [ ] Replace placeholder tree data with computed tree data and remove JSON debug output
- [ ] Implement recursive node rendering via Angular structural directives (`@for`, `@if`)
- [ ] Add Material icons/buttons styled per node type and show loading/error states when applicable
- [ ] Handle single-click events: toggle expansion/selection and trigger `navigateToDirectory` as appropriate
- [ ] Emit navigation requests with `{ deviceId, storageType, path }` to `StorageStore.navigateToDirectory`

### Task 3: Storage Availability & Dynamic Updates

**Purpose**: Filter tree content to only available storage, reflect store changes, and clean up removed devices.

- [ ] Source availability information from `StorageStore` selectors or device input to include Internal storage and hide unavailable SD/USB nodes
- [ ] Update tree when storage availability or directory data changes without full rebuild of expansion state or cache
- [ ] Remove device nodes when devices disconnect and clear associated cache entries
- [ ] Ensure refreshing or navigating new paths updates tree nodes and maintains expansion where appropriate

### Task 4: Styling & Accessibility

**Purpose**: Apply Material design styling consistent with the app and ensure accessibility.

- [ ] Update `directory-tree.component.scss` to style indentation, selection, hover, and loading indicators
- [ ] Use ARIA attributes/roles on tree elements for screen readers
- [ ] Display error messages or skeleton states inline when directories fail to load or are loading

### Task 5: Testing & Cleanup

**Purpose**: Validate behavior and remove legacy JSON debugging from Phase 3.

- [ ] Update `directory-tree.component.spec.ts` with unit tests covering tree rendering, selection, navigation, availability filtering, and cache persistence
- [ ] Add tests for expansion state persistence and navigation call parameters
- [ ] Remove Phase 3 JSON verification output from `storage-container.component.html` and ensure file list integration remains intact
- [ ] Document any follow-up tasks or risks

## File Changes

- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.ts`](../../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.ts)
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.html`](../../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.html)
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.scss`](../../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.scss)
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.spec.ts`](../../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree.component.spec.ts)

## Testing Requirements

- [ ] Tree structure renders per device with correct storage children and directories
- [ ] Clicking storage or directory nodes updates selection and triggers expected navigation
- [ ] Expansion state persists across tree updates and re-renders
- [ ] Availability filtering hides unavailable storage while always showing Internal storage
- [ ] Loading and error states display as expected during navigation
- [ ] In-component cache retains previously visited directories across navigation events

### Unit Tests

- [ ] Component creates correct view-model from store directory data and cached results
- [ ] Node clicks call `navigateToDirectory` with expected payloads
- [ ] Expansion state utilities toggle nodes and survive re-computation
- [ ] Cache merges new directory responses without dropping earlier children

### Integration Tests

- [ ] Tree reacts to store updates from multiple devices without state bleed-over
- [ ] Navigating across storage types updates file list while maintaining other expansions
- [ ] Device removal removes tree nodes, clears cache entries, and leaves other devices untouched

## Success Criteria

- [ ] Interactive tree matches hierarchy and availability rules from the plan
- [ ] Navigation actions update store without redundant reloads for cached paths
- [ ] UI reflects loading/error states and adheres to Material styling guidance
- [ ] All identified unit/integration tests implemented and passing
- [ ] Ready to proceed to Phase 5 (Basic File Listing)

## Notes

- Tests focus on observable component behavior (DOM structure, selection state, store call assertions) rather than internal caching mechanics so they survive a later store-level caching refactor.
- Internal storage node must always appear first under each device, regardless of availability flags
- Expansion state and caching start local to the component; revisit store-level persistence in later phases if needed
- Consider future enhancements (search, virtual scrolling) when designing node IDs and state structures
