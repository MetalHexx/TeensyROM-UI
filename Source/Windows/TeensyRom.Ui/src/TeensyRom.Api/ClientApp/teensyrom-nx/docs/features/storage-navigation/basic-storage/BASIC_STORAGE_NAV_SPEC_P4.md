# Phase 4: Basic Navigation Tree Implementation Specification

**Related Documentation**: [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

**Standards Documentation**:

- **Player Component Hierarchy**: ['PLAYER_COMPONENTS.md'](../../../../libs/features/player/src/PLAYER_COMPONENTS.md)
- **Coding Standards**: [`CODING_STANDARDS.md`](../../../CODING_STANDARDS.md) - Component and TypeScript standards.
- **Style Guide**: [`STYLE_GUIDE.md`](../../../STYLE_GUIDE.md) - CSS / Style / Theme design specifications and rules.
- **Testing Standards**: [`TESTING_STANDARDS.md`](../../../TESTING_STANDARDS.md) - Unit, integration, and E2E testing patterns.

## Objective

Build hierarchical directory tree component showing device → storage type → directories with interactive click navigation functionality.

## Prerequisites

- Phase 3 completed: Component/Store Integration & JSON Verification
- StorageStore verified working with manual testing interface
- Component integration established and tested

## Implementation Steps

### Step 1: Tree Structure Design

**Purpose**: Define the hierarchical tree structure showing device → storage type → directories

**Tasks**:

1. **Tree Node Data Model**

   - Define TreeNode interface for consistent tree structure
   - Support device nodes, storage type nodes, and directory nodes
   - Include node type, path, children, and expanded state

2. **Tree State Management**
   - Add tree expansion state to StorageStore or local component state
   - Track expanded/collapsed state per node
   - Manage tree navigation and selection highlighting

### Step 2: Directory Tree Component Implementation

**Purpose**: Implement interactive directory tree component with proper Angular patterns

**Tasks**:

1. **Component Structure**

   - Update DirectoryTreeComponent with tree rendering logic
   - Use Angular control flow (@if, @for) for tree rendering
   - Implement recursive tree node rendering

2. **Tree Navigation**

   - Connect tree clicks to StorageStore.navigateToDirectory()
   - Handle storage type switching within device tree
   - Implement directory expansion/collapse functionality

3. **Visual Design**
   - Implement Material Design tree styling
   - Add icons for devices, storage types, and directories
   - Show loading states and error states in tree

### Step 3: Storage Availability Integration

**Purpose**: Show only available storage types and handle storage unavailability gracefully

**Tasks**:

1. **Availability Filtering**

   - Filter tree to show only available storage types
   - Hide unavailable storage completely (not disabled/grayed)
   - Always show Internal storage as available

2. **Dynamic Updates**
   - Update tree when storage availability changes
   - Handle device connection/disconnection in tree
   - Refresh tree structure when needed

## Deliverables

- Updated DirectoryTreeComponent with hierarchical tree rendering
- Tree navigation connected to StorageStore methods
- Storage availability filtering implemented
- Visual tree styling with Material Design components
- Component tests for tree functionality and storage integration

## File Changes

```
libs/features/player/src/lib/player-view/player-device-container/storage-container/
├── directory-tree/
│   ├── directory-tree.component.ts     # Tree implementation with StorageStore integration
│   ├── directory-tree.component.html   # Hierarchical tree template
│   ├── directory-tree.component.scss   # Tree styling
│   └── directory-tree.component.spec.ts # Tree component tests
└── storage-container.component.html     # Remove JSON, show tree + file list
```

## Testing Requirements

### Unit Tests

- Tree rendering with different device/storage configurations
- Tree navigation click handling and store method calls
- Storage availability filtering logic
- Tree expansion/collapse state management

### Integration Tests

- Full tree interaction with live StorageStore
- Tree updates in response to storage state changes
- Storage availability changes reflected in tree structure

## Success Criteria

- ✅ Hierarchical tree renders showing device → storage → directories
- ✅ Tree clicks properly navigate directories via StorageStore
- ✅ Only available storage types shown in tree (unavailable hidden)
- ✅ Tree expansion/collapse state managed correctly
- ✅ Visual styling follows Material Design patterns
- ✅ Component tests verify tree functionality
- ✅ Ready to proceed to Phase 5 (Basic File Listing)

## Notes

- This phase removes JSON verification UI and replaces with functional tree
- Tree provides the primary navigation interface for directory browsing
- Foundation for advanced tree features in later phases (search, virtual scrolling)
- Prepares for Phase 5 file listing integration
