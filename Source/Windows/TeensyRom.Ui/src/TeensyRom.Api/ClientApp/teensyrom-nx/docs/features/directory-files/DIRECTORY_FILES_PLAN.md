# Directory Files Listing - High-Level Plan

**Project Overview**: Build a Windows Explorer-style file listing interface for browsing TeensyROM storage directories, displaying both directories and files with appropriate icons, metadata, and navigation capabilities.

**Standards Documentation**:

- **Coding Standards**: [../../CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [../../TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **State Standards**: [../../STATE_STANDARDS.md](../../STATE_STANDARDS.md)
- **Style Guide**: [../../STYLE_GUIDE.md](../../STYLE_GUIDE.md)
- **Component Library**: [../../COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md)

## 🎯 Project Objective

Create a comprehensive file and directory listing UI that displays the contents of the currently selected storage directory. Users can navigate directories via double-click, select items with single-click, and view relevant metadata like file sizes and type-specific icons.

## 📋 Implementation Phases

## Phase 1: Component Architecture

### Objective

Establish the foundational component structure with separate presentational components for directories and files.

### Key Deliverables

- [ ] DirectoryItemComponent (presentational)
- [ ] FileItemComponent (presentational)
- [ ] Component event system (click, double-click)
- [ ] Type-safe props and outputs

### High-Level Tasks

1. **Create DirectoryItemComponent**: Displays directory with folder icon and name
2. **Create FileItemComponent**: Displays file with type-specific icon, name, and size
3. **Define Component Interfaces**: Event outputs and input props
4. **Icon Mapping Logic**: Map FileItemType enum to Material icons

---

## Phase 2: Table Integration

### Objective

Integrate components into a Material table within the DirectoryFilesComponent with selection and navigation logic.

### Key Deliverables

- [ ] Material table implementation
- [ ] Combined data source (directories + files)
- [ ] Row selection highlighting
- [ ] Double-click navigation

### High-Level Tasks

1. **Update DirectoryFilesComponent**: Replace placeholder with Material table
2. **Implement Data Merging**: Combine directories and files with type discrimination
3. **Add Selection Logic**: Track selected item, highlight row
4. **Wire Navigation**: Double-click directory calls store.navigateToDirectory()

---

## Phase 3: Polish & Testing

### Objective

Add visual polish, comprehensive testing, and ensure proper integration with existing storage navigation.

### Key Deliverables

- [ ] Component unit tests
- [ ] Integration tests
- [ ] Styling (hover, selected, cursor states)
- [ ] File size formatting

### High-Level Tasks

1. **Component Testing**: Test all click/dblclick behaviors, icon mapping
2. **Integration Testing**: Verify store integration, navigation flow
3. **Visual Styling**: Hover effects, selection highlighting, cursor changes
4. **Helper Functions**: File size formatting, type guards

## 🏗️ Architecture Overview

### Key Design Decisions

- **Presentational Components**: DirectoryItem and FileItem are pure presentational - no store access
- **Smart Container**: DirectoryFilesComponent is the only component that accesses StorageStore
- **Event-Driven**: Child components emit events, parent handles business logic
- **Type Discrimination**: Use type guards to differentiate directories from files in template
- **Reuse IconLabelComponent**: Leverage existing enhanced component for icon+text display

### Integration Points

- **StorageStore**: Reads current directory contents, calls navigateToDirectory() on double-click
- **IconLabelComponent**: Reused for displaying icons with text (folder, file types)
- **Material Table**: Standard Angular Material table for data display
- **DirectoryTreeComponent**: Works alongside tree for navigation consistency

### Component Hierarchy

```
DirectoryFilesComponent (smart - accesses StorageStore)
├── Material Table (mat-table)
├── DirectoryItemComponent (presentational)
│   └── IconLabelComponent (folder icon, directory color)
└── FileItemComponent (presentational)
    ├── IconLabelComponent (file type icon, normal color)
    └── File size display
```

### Data Flow

1. **StorageStore** → provides current directory state (directories[], files[])
2. **DirectoryFilesComponent** → merges into table data source
3. **Table rows** → render DirectoryItem or FileItem based on type
4. **User interaction** → components emit events
5. **DirectoryFilesComponent** → handles events (selection, navigation)
6. **Navigation** → calls StorageStore.navigateToDirectory()

## 🧪 Testing Strategy

### Unit Tests

- [ ] DirectoryItemComponent renders correctly with DirectoryItem data
- [ ] DirectoryItemComponent emits events on click/dblclick
- [ ] FileItemComponent renders with correct icon per FileItemType
- [ ] FileItemComponent displays formatted file size
- [ ] Icon mapping returns correct Material icon for each type

### Integration Tests

- [ ] DirectoryFilesComponent displays combined directories and files
- [ ] Single click on item highlights row and sets selection
- [ ] Double click on directory calls navigateToDirectory() with correct params
- [ ] Table updates when StorageStore directory contents change
- [ ] Selection clears when directory changes

### E2E Tests

- [ ] User can navigate directories by double-clicking folders
- [ ] Selected item remains highlighted until different item clicked
- [ ] File type icons display correctly for all supported types
- [ ] File sizes display in human-readable format

## ✅ Success Criteria

- [ ] Users can view all directories and files in current storage location
- [ ] Double-clicking a directory navigates into that directory
- [ ] Single-clicking any item highlights the row
- [ ] File type icons clearly differentiate file types (Song, Game, Image, etc.)
- [ ] File sizes display in appropriate units (B, KB, MB)
- [ ] All components have comprehensive unit tests
- [ ] Integration with StorageStore navigation works seamlessly

## 📚 Related Documentation

- **Storage Models**: [`libs/domain/storage/services/src/lib/storage.models.ts`](../../../libs/domain/storage/services/src/lib/storage.models.ts)
- **Storage Store**: [`libs/domain/storage/state/src/lib/storage-store.ts`](../../../libs/domain/storage/state/src/lib/storage-store.ts)
- **IconLabelComponent**: [Component Library - IconLabelComponent](../../COMPONENT_LIBRARY.md#iconlabelcomponent)
- **StyledIconComponent**: [Component Library - StyledIconComponent](../../COMPONENT_LIBRARY.md#stylediconcomponent)

## 📝 Notes

- **Design Pattern**: Follow established pattern from directory-tree-node for consistency
- **Reusability**: DirectoryItem and FileItem are generic enough to be reused elsewhere if needed
- **Performance**: Consider virtual scrolling for directories with hundreds of files (future enhancement)
- **Accessibility**: Ensure keyboard navigation works (arrow keys, enter for navigation)
- **Future**: May add context menus, drag-and-drop, multi-selection later
