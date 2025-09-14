# Phase 5: Basic File Listing Implementation Specification

**Related Documentation**: [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

**Standards Documentation**:

- **Player Component Hierarchy**: ['PLAYER_COMPONENTS.md'](../../../../libs/features/player/src/PLAYER_COMPONENTS.md)
- **Coding Standards**: [`CODING_STANDARDS.md`](../../../CODING_STANDARDS.md) - Component and TypeScript standards.
- **Style Guide**: [`STYLE_GUIDE.md`](../../../STYLE_GUIDE.md) - CSS / Style / Theme design specifications and rules.
- **Testing Standards**: [`TESTING_STANDARDS.md`](../../../TESTING_STANDARDS.md) - Unit, integration, and E2E testing patterns.

## Objective

Implement directory contents display showing files and subdirectories for the currently selected path with proper file type handling and visual presentation.

## Prerequisites

- Phase 4 completed: Basic Navigation Tree Implementation
- Directory tree navigation working and tested
- StorageStore properly managing selected directory state

## Implementation Steps

### Step 1: File List Data Integration

**Purpose**: Connect DirectoryFilesComponent to StorageStore and display current directory contents

**Tasks**:

1. **Store Connection**

   - Inject StorageStore in DirectoryFilesComponent
   - Subscribe to selected directory state changes
   - Display files and subdirectories from current StorageDirectory

2. **Data Binding**
   - Show files from current directory using @for control flow
   - Display directory metadata (path, file count, etc.)
   - Handle empty directories gracefully

### Step 2: File Type Classification and Display

**Purpose**: Properly classify and visually distinguish different file types

**Tasks**:

1. **File Type Handling**

   - Use existing FileItemType enum for file classification
   - Create file type icon mapping (images, programs, documents, etc.)
   - Implement different visual treatments per file type

2. **File Metadata Display**
   - Show file names, sizes, and other metadata
   - Format file sizes appropriately (bytes, KB, MB)
   - Display file type indicators or extensions

### Step 3: Directory Interaction

**Purpose**: Enable navigation from file list to subdirectories

**Tasks**:

1. **Directory Navigation**

   - Make subdirectories clickable to navigate deeper
   - Connect directory clicks to StorageStore.navigateToDirectory()
   - Coordinate with tree component for selection updates

2. **File Actions**
   - Implement basic file selection/highlighting
   - Prepare hooks for future file launch/download actions
   - Show loading states during directory changes

## Deliverables

- Updated DirectoryFilesComponent with StorageStore integration
- File type classification and visual treatment
- Subdirectory navigation functionality
- File list styling with Material Design components
- Component tests for file listing functionality

## File Changes

```
libs/features/player/src/lib/player-view/player-device-container/storage-container/
├── directory-files/
│   ├── directory-files.component.ts     # File list with StorageStore integration
│   ├── directory-files.component.html   # File/directory listing template
│   ├── directory-files.component.scss   # File list styling
│   └── directory-files.component.spec.ts # File list component tests
└── storage-container.component.html     # Layout tree + file list side-by-side
```

## Testing Requirements

### Unit Tests

- File list rendering with different directory contents
- File type classification and icon display
- Subdirectory navigation click handling
- Empty directory state handling
- Loading and error states

### Integration Tests

- File list updates when directory selection changes
- Coordination between tree navigation and file list updates
- File metadata display accuracy
- Subdirectory navigation integration with tree

## Success Criteria

- ✅ File list displays current directory contents from StorageStore
- ✅ Files classified by type with appropriate icons/styling
- ✅ Subdirectories clickable for deeper navigation
- ✅ File metadata (name, size, type) displayed correctly
- ✅ Empty directories handled gracefully with appropriate messaging
- ✅ Loading states shown during directory changes
- ✅ Component tests verify file listing functionality
- ✅ Ready to proceed to Phase 6 (Breadcrumb and Back Navigation)

## Notes

- This phase completes the basic directory browsing functionality
- File list provides detailed view of current directory contents
- Foundation for advanced file operations in future phases
- Prepares for Phase 6 breadcrumb navigation integration
