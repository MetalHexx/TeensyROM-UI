# Phase 3: Component/Store Integration & JSON Verification Specification

**Related Documentation**: [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [`CODING_STANDARDS.md`](../../../CODING_STANDARDS.md) - Component and TypeScript standards.

## Objective

Integrate StorageStore with existing player components and implement JSON state output verification to validate store functionality before building navigation UI components.

## Prerequisites

- Phase 1 completed: Storage domain services available
- Phase 2 completed: Storage state management implemented
- Existing player components available for enhancement

## Implementation Steps

### Step 1: Component Store Integration

**Purpose**: Connect StorageStore to player components and establish proper dependency injection and lifecycle management.

**Tasks**:

1. **PlayerViewComponent**

   - Inject `StorageStore` alongside existing `DeviceStore`
   - Initialize storage entries for connected devices on component init
   - Clean up storage state when devices disconnect
   - Pass device context to PlayerDeviceContainerComponents

2. **PlayerDeviceContainerComponent**

   - Add `device` input to receive device context from parent
   - Pass device data to StorageContainerComponent

3. **StorageContainerComponent**
   - Add `device` input to receive device context from parent
   - Inject `StorageStore`
   - Validate storage availability before exposing data

### Step 2: JSON State Verification

**Purpose**: Verify StorageStore state functionality by outputting JSON representation of storage state to the UI for testing and validation.

**Tasks**:

1. **Add JSON Output Signals**

   - Add `storageJson` computed signal to PlayerViewComponent
   - Add `deviceStorageJson` computed signal to StorageContainerComponent
   - Filter storage entries by device ID in device-specific components

2. **Update Templates**
   - Add `<pre>{{ storageJson() | json }}</pre>` to PlayerView template
   - Add `<pre>{{ deviceStorageJson() | json }}</pre>` to StorageContainer template

### Step 3: Manual Testing Interface

**Purpose**: Add simple UI controls to test store methods and observe state changes via JSON output.

**Tasks**:

1. **Add Test Buttons**

   - Add buttons to test `navigateToDirectory()` with hardcoded paths
   - Add buttons to test `refreshDirectory()` calls
   - Add buttons to test storage initialization/cleanup

2. **Wire Button Actions**
   - Connect buttons to store methods with proper parameters
   - Enable manual testing of state changes via JSON output

## Deliverables

- PlayerViewComponent with StorageStore integration and lifecycle management
- StorageContainerComponent with device input and store injection
- JSON output verification via computed signals and template display
- Manual testing interface with buttons for core store operations
- Component tests verifying store integration and JSON output accuracy

## File Changes

```
libs/features/player/src/lib/player-view/
├── player-view.component.ts                    # Add StorageStore injection, lifecycle, storageJson signal
├── player-view.component.html                  # Add JSON output display
└── player-device-container/
    ├── player-device-container.component.ts    # Add device input
    └── storage-container/
         ├── storage-container.component.ts      # Add device input, StorageStore injection, deviceStorageJson
         └── storage-container.component.html    # Add JSON output + test buttons
```

## Testing Requirements

### Unit Tests

- Component integration tests verifying store injection and method calls
- JSON output accuracy tests ensuring state changes reflect correctly
- Lifecycle tests for initialization and cleanup behavior

### Integration Tests

- Full component mounting tests with live store integration
- JSON output rendering and update verification in DOM
- Manual testing button functionality validation

## Success Criteria

- ✅ StorageStore successfully injected and accessible in components
- ✅ JSON representation of storage state renders in PlayerView and StorageContainer
- ✅ JSON updates in response to store actions (initialize, navigate, refresh, cleanup)
- ✅ Manual testing buttons trigger visible state changes in JSON output
- ✅ Component tests verify store integration and JSON output correctness
- ✅ All existing tests continue passing
- ✅ Ready to proceed to Phase 4 (Basic Navigation Tree) after verification

## Notes

- This phase establishes the foundation for all subsequent UI phases
- JSON verification approach enables rapid validation of state management
- Manual testing interface provides immediate feedback during development
- Prepares components for Phase 4 navigation tree implementation
