# Phase 3: Component Integration Specification

**Related Documentation**: [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [`CODING_STANDARDS.md`](../../../CODING_STANDARDS.md) - Component and TypeScript standards.
- **Style Guide**: [`STYLE_GUIDE.md`](../../../STYLE_GUIDE.md) - CSS / Style / Theme design specifications and rules.
- **Testing Standards**: [`TESTING_STANDARDS.md`](../../../TESTING_STANDARDS.md) - Unit, integration, and E2E testing patterns.

## Objective

Integrate storage state with existing player components for hierarchical directory browsing UI with storage type selection.

## Prerequisites

- Phase 1 completed: Storage domain services available
- Phase 2 completed: Storage state management implemented
- Existing player components available for enhancement

## Implementation Steps

### Step 1: Component-Level Store Integration

**Purpose**: Integrate StorageStore with existing component hierarchy following established data flow patterns

**Architectural Decision**: Based on analysis of existing PlayerViewComponent architecture, use component-level coordination instead of service facades to maintain consistency with current patterns.

**Current Component Hierarchy**:
```
PlayerViewComponent (injects DeviceStore, filters connectedDevices)
└── PlayerDeviceContainerComponent (receives device input)
    └── StorageContainerComponent (handles storage operations)
        ├── DirectoryTreeComponent
        ├── DirectoryFilesComponent  
        └── SearchToolbarComponent
```

**Integration Approach**:

1. **PlayerViewComponent Updates**:
   - Add `StorageStore` injection alongside existing `DeviceStore`
   - Initialize storage state for connected devices using `initializeDeviceStorage`
   - Clean up storage state when devices disconnect using `cleanupDeviceStorage`

2. **StorageContainerComponent Updates**:
   - Add `device` input to receive device context from parent
   - Inject `StorageStore` for storage operations  
   - Implement component-level coordination logic:
     - Validate storage availability using device context before operations
     - Handle `loadDirectory`, `navigateToDirectory`, and `refreshDirectory` calls
     - Pass storage state and methods to child components

3. **Child Component Integration**:
   - **DirectoryTreeComponent**: Receive storage navigation state and emit navigation events
   - **DirectoryFilesComponent**: Display current directory contents from storage state
   - **SearchToolbarComponent**: Coordinate with storage operations as needed

**Key Implementation Requirements**:

- Follow existing input/output patterns used throughout player components
- Maintain clean separation: DeviceStore for device data, StorageStore for navigation state
- Handle storage availability validation at StorageContainerComponent level
- Use Angular signals for reactive data flow
- Preserve existing component responsibilities and boundaries

**Deliverables**:

- Updated `PlayerViewComponent` with storage state lifecycle management
- Enhanced `StorageContainerComponent` with device context and storage coordination
- Modified child components to work with storage state
- Component integration tests verifying cross-store coordination

**File Changes**:

```
libs/features/player/src/lib/player-view/
├── player-view.component.ts                    # Add StorageStore injection and lifecycle
└── player-device-container/
    └── storage-container/
        ├── storage-container.component.ts      # Add device input and storage coordination
        ├── directory-tree/
        │   └── directory-tree.component.ts     # Integrate with storage navigation state
        └── directory-files/
            └── directory-files.component.ts    # Display storage directory contents
```

### Future Steps

_Steps 2-7 will be defined in subsequent planning sessions based on Step 1 implementation results_

## Testing Requirements

_To be defined_

## Success Criteria

_To be defined_

## File Structure

_To be defined_

## Dependencies

_To be defined_

## Notes

- This phase integrates storage state with UI components
- Implements hierarchical directory tree with device → storage → directory structure
- Creates foundation for Phase 4 internal storage planning
