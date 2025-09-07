# Player View Storage Navigation Plan

## Overview

This document outlines the plan for directory browsing and file navigation in the player view across multiple TeensyROM devices with SD, USB and internal storage support. The feature enables users to navigate file systems and maintain independent navigation state per device-storage combination.

---

## Architecture Summary

### Multi-Device Storage Management

- **Device-Scoped State**: Each `(deviceId, storageType)` combination maintains independent navigation state
- **Availability-Aware**: Only creates state for available storage devices (based on `Device.sdStorage.available` / `Device.usbStorage.available` / `Device.internalStorage.available`)
- **Persistent Navigation**: Preserves user location across view changes - no reset to root on return visits
- **Performance-Ready**: Flat state structure designed for future virtual scrolling implementation

### API Integration

- **Endpoint**: `GetDirectoryEndpoint` returns `StorageCacheDto` with directories and files
- **Request**: `{ deviceId, storageType, path }`
- **Response**: `{ directories: DirectoryItemDto[], files: FileItemDto[], path: string }`

---

## Implementation Phases

## Phase 1: HTTP Client & Domain Models

### Objective

Create HTTP client integration with clean domain model transformation layer.

### Deliverables

- Regenerated API client with GetDirectory support
- Storage domain models and mapper
- Storage service wrapper for API calls

### Tasks

1. **Regenerate API Client**: Ensure GetDirectory endpoint included
2. **Create Storage Services Library**: Generate nx library structure
3. **Domain Models**: Create interfaces for StorageDirectory, DirectoryItem, FileItem
4. **Data Mapping**: Transform API DTOs to clean domain models
5. **Service Wrapper**: HTTP client wrapper with domain model transformation

---

## Phase 2: State Management

### Objective

Implement NgRx Signal Store for multi-device storage navigation state.

### Deliverables

- Storage state library with signal store
- Device-scoped state management (`${deviceId}-${storageType}` keys)
- Store methods for directory loading and navigation

### Tasks

1. **Create Storage State Library**: Generate nx library structure
2. **State Structure**: Flat storage with device-storage keys
3. **Store Methods**: loadDirectory, navigateToDirectory, initializeUnloadedDevices
4. **Device Integration**: Cross-reference with existing device store
5. **Smart Initialization**: Load root only for uninitialized devices

---

## Phase 3: Component Integration

### Objective

Integrate storage state with existing player components for hierarchical directory browsing UI with storage type selection.

### Deliverables

- Updated player-view with smart initialization
- Enhanced player-device-container with storage availability
- Hierarchical directory tree showing device → storage type → directories
- Connected directory-tree and directory-files to storage state

### Directory Tree Structure

_Note: Each device renders independently in its own `player-device-container` with a dedicated `storage-container` housing the directory tree._

```
📱 Device ABC123 (Internal, SD, and USB available)
├── 💻 Internal Storage (Always available)
│   ├── 📁 system/
│   │   ├── 📁 config/
│   │   └── 📁 cache/
│   └── 📁 user/
├── 💾 SD Storage
│   ├── 📁 games/
│   │   ├── 📁 arcade/
│   │   │   ├── 🎮 pacman.prg
│   │   │   └── 🎮 galaga.prg
│   │   └── 📁 puzzle/
│   ├── 📁 music/
│   └── 📁 demos/
└── 🔌 USB Storage
    ├── 📁 roms/
    │   └── 📁 commodore/
    └── 📁 utilities/

────────────────────────────────────────────────────

📱 Device XYZ789 (Internal and USB available - SD not shown)
├── 💻 Internal Storage (Always available)
│   ├── 📁 system/
│   └── 📁 temp/
└── 🔌 USB Storage
    ├── 📁 homebrew/
    └── 📁 tools/
        ├── 🔧 monitor.prg
        └── 🔧 debugger.prg
```

**Note**: Internal storage is always available for every device. Only available external storage types (SD/USB) are displayed in the tree. Unavailable external storage devices are completely hidden from the UI.

### Tasks

1. **Player View**: Initialize storage state for connected devices
2. **Device Container**: Handle storage availability and pass device context
3. **Storage Container**: Coordinate directory tree and file list components
4. **Directory Tree**: Hierarchical tree with device → available storage → directory structure
5. **Directory Files**: Display current directory contents with storage context
6. **Storage Selection**: Enable switching between available storage types within tree
7. **Storage Filtering**: Hide unavailable storage types from tree display

---

## Phase 4: Internal Storage Integration Planning

### Objective

Plan and prepare for backend implementation of internal storage support through existing GetDirectoryEndpoint.

### Deliverables

- Backend architecture plan for internal storage access
- Serial command layer enhancement strategy
- Internal storage integration requirements
- Updated API contracts and data models

### Tasks

1. **Backend Planning**: Coordinate with Travis on serial command layer changes
2. **Storage Type Extension**: Plan TeensyStorageType enum extension for Internal
3. **Device Model Updates**: Plan Device.internalStorage property addition
4. **API Consistency**: Ensure GetDirectoryEndpoint supports internal storage paths
5. **Frontend Readiness**: Verify existing architecture supports third storage type

### Dependencies

- **Backend Development**: Serial command layer enhancements
- **Device Communication**: Internal storage access protocols
- **API Compatibility**: Maintain existing GetDirectory contract

**Note**: This phase focuses on planning and coordination. Implementation will follow after backend infrastructure is complete.

---

## Phase 5: Virtual Scrolling Preparation

### Objective

Prepare architecture for high-performance rendering of large directory listings.

### Deliverables

- Virtual scrolling readiness assessment
- Performance threshold identification
- Implementation strategy for large file lists

### Tasks

1. **Architecture Review**: Verify flat state structure supports virtualization
2. **Component Readiness**: Ensure directory-files uses flat arrays
3. **Performance Threshold**: Define when to implement virtual scrolling (~1000+ items)
4. **Integration Strategy**: Plan Angular CDK Virtual Scrolling integration
5. **Tree Virtualization**: Research tree virtualization libraries for directory tree

---

## Key Architecture Decisions

### State Structure

- **Flat Storage Keys**: `${deviceId}-${storageType}` for O(1) device-storage lookup
- **Availability-Based**: Only create state for available storage devices
- **Navigation Persistence**: Preserve user location across view changes

### Cross-Store Integration

- **Device Store**: Source of truth for storage availability
- **Storage Store**: Manages navigation within available storage only
- **Validation Pattern**: Check device availability before state operations

### Error Handling

- **Unavailable Storage**: Hidden from UI, not shown as disabled/grayed options
- **API Failures**: Actual errors stored in state
- **Graceful Degradation**: Clean UI showing only available functionality

---

## User Experience Flow

### Scenario 1: First Visit to Player View

```gherkin
Given I have connected TeensyROM devices with available storage
When I navigate to the player view for the first time
Then I should see a hierarchical tree showing each device
And each device should show Internal storage as the first child node
And each device should show available external storage types (SD/USB) as additional child nodes
And the root directory should be loaded for each available storage type
And I should see file lists showing root directory contents for the selected storage
```

### Scenario 2: Storage Type Selection

```gherkin
Given I have a device with Internal, SD, and USB storage available
When I click on "USB Storage" in the device tree
Then the directory tree should expand to show USB root directories
And the file list should update to show USB root directory contents
And the Internal and SD storage trees should remain collapsed/unexpanded
```

### Scenario 2b: Internal Storage Always Present

```gherkin
Given I have a device with only USB storage available (no SD)
When I view the device in the player
Then I should see "Internal Storage" as the first node under the device
And I should see "USB Storage" as the second node under the device
And no SD storage node should appear in the tree
And internal storage should always be available regardless of external storage
```

### Scenario 3: Return Visit to Player View

```gherkin
Given I have previously navigated to Internal:/system/config on Device A
And I have previously navigated to SD:/games/arcade on Device A
And I have previously navigated to USB:/music on Device A
And I have navigated away from the player view
When I return to the player view
Then I should see Device A's Internal storage showing "/system/config"
And I should see Device A's SD storage showing "/games/arcade"
And I should see Device A's USB storage showing "/music"
And the directory tree should show previously expanded folders for all storage types
And no API calls should be made to reload existing directories
```

### Scenario 4: Cross-Storage Navigation

```gherkin
Given I am viewing SD:/games on Device A
When I click on "Internal Storage" in the same device's tree
Then the file list should switch to show Internal storage contents
And the SD storage should remain expanded at "/games"
And the Internal storage should show its current/root directory
```

### Scenario 5: Multi-Device Independent Navigation

```gherkin
Given I have Device A and Device B both connected
When I navigate to Internal:/system on Device A
And I navigate to USB:/music on Device B
Then Device A should show Internal storage expanded to "/system"
And Device B should show USB storage expanded to "/music"
And each device-storage combination should maintain independent state
And both devices should show their Internal storage as always available
```

### Scenario 6: Unavailable External Storage Handling

```gherkin
Given I have a device with only SD storage available (USB unavailable)
When I view the device in the player
Then I should see "Internal Storage" as the first node in the device tree
And I should see "SD Storage" as the second node in the device tree
And I should NOT see any USB storage node in the tree
And no API calls should be made for unavailable USB storage
And internal storage should always be present regardless of external storage availability
```

### Scenario 7: Device Disconnection

```gherkin
Given I have navigated to Internal:/system on Device A
And I have navigated to SD:/games/arcade on Device A
And I have navigated to USB:/music on Device A
When Device A becomes disconnected
Then Device A should be removed from the device tree
And all storage state for Device A should be cleaned up (Internal, SD, USB)
And other connected devices should remain unaffected
And when Device A reconnects, all storage types should start from root directories
```

---

## Testing Strategy

### Unit Tests

- Storage service with mocked API calls
- Storage store methods with state validation
- Component integration with device inputs

### Integration Tests

- Cross-store device availability integration
- Navigation flow state updates
- Initial loading vs return visit behavior

### E2E Tests

- Multi-device scenarios with different storage availability
- Navigation state persistence across view changes

---

## Related Documentation

- **State Standards**: [`STATE_STANDARDS.md`](../../../STATE_STANDARDS.md) - NgRx Signal Store patterns.
- **Coding Standards**: [`CODING_STANDARDS.md`](../../../CODING_STANDARDS.md) - Component and TypeScript standards.
- **Architecture Overview**: [`OVERVIEW_CONTEXT.md`](../../../OVERVIEW_CONTEXT.md) - Overall application architecture.
- **Style Guide**: [`STYLE_GUIDE.md`](../../../STYLE_GUIDE.md) - CSS / Style / Theme design specifications and rules.
