# Phase 1: Directory Files Implementation Tasks

**High Level Plan Documentation**: [DIRECTORY_FILES_PLAN.md](./DIRECTORY_FILES_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [../../CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Store Testing**: [../../STORE_TESTING.md](../../STORE_TESTING.md)
- **State Standards**: [../../STATE_STANDARDS.md](../../STATE_STANDARDS.md)
- **Style Guide**: [../../STYLE_GUIDE.md](../../STYLE_GUIDE.md)
- **Component Library**: [../../COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md)

## 🎯 Objective

Build a complete file and directory listing UI with separate presentational components for directories and files, integrated into a Material table with selection and navigation capabilities.

## 📚 Required Reading & Context Files

### Data Models & Types

- [ ] [`libs/domain/storage/services/src/lib/storage.models.ts`](../../../libs/domain/storage/services/src/lib/storage.models.ts) - DirectoryItem, FileItem, FileItemType enum
- [ ] [`libs/domain/storage/state/src/lib/storage-store.ts`](../../../libs/domain/storage/state/src/lib/storage-store.ts) - StorageState, SelectedDirectory

### Store Actions & Selectors

- [ ] [`libs/domain/storage/state/src/lib/actions/navigate-to-directory.ts`](../../../libs/domain/storage/state/src/lib/actions/navigate-to-directory.ts) - Navigation action to call on double-click
- [ ] [`libs/domain/storage/state/src/lib/selectors/get-selected-directory-state.ts`](../../../libs/domain/storage/state/src/lib/selectors/get-selected-directory-state.ts) - Selector for current directory

### Existing Components (for patterns)

- [ ] [`libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree-node/`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-tree/directory-tree-node/) - Pattern for directory display with IconLabelComponent
- [ ] [`libs/ui/components/src/lib/icon-label/`](../../../libs/ui/components/src/lib/icon-label/) - Enhanced component for icon + text display
- [ ] [`libs/ui/components/src/lib/styled-icon/`](../../../libs/ui/components/src/lib/styled-icon/) - Icon with color/size support

### Parent Component (to modify)

- [ ] [`libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.ts) - Current placeholder component
- [ ] [`libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.html`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.html) - Template to replace

## 📋 Implementation Tasks

### Task 1: Create DirectoryItemComponent

**Purpose**: Presentational component for displaying directory rows with folder icon and name.

**Location**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-item/`

- [ ] Create `directory-item.component.ts`

  - Signal input: `directoryItem` (DirectoryItem type)
  - Signal input: `selected` (boolean, default false)
  - Output: `itemSelected` (emits DirectoryItem)
  - Output: `itemDoubleClicked` (emits DirectoryItem)
  - Import IconLabelComponent from `@teensyrom-nx/ui/components`

- [ ] Create `directory-item.component.html`

  - Use `<lib-icon-label>` with:
    - icon="folder"
    - color="directory"
    - size="medium"
    - [truncate]="false"
    - [label]="directoryItem().name"
  - Add click handlers: `(click)` and `(dblclick)`

- [ ] Create `directory-item.component.scss`

  - `:host` with cursor: pointer
  - Apply `.selected` class when selected input is true
  - Hover state styling

- [ ] Create `directory-item.component.spec.ts`
  - Test component renders with DirectoryItem
  - Test itemSelected emits on single click
  - Test itemDoubleClicked emits on double click
  - Test selected class applied when selected=true

### Task 2: Create FileItemComponent

**Purpose**: Presentational component for displaying file rows with type-specific icons, name, and size.

**Location**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/file-item/`

- [ ] Create `file-item.component.ts`

  - Signal input: `fileItem` (FileItem type)
  - Signal input: `selected` (boolean, default false)
  - Output: `itemSelected` (emits FileItem)
  - Computed: `fileIcon()` - maps FileItemType to icon name:
    ```typescript
    Unknown → 'insert_drive_file'
    Song → 'music_note'
    Game → 'sports_esports'
    Image → 'image'
    Hex → 'code'
    ```
  - Helper: `formatFileSize(bytes)` - returns human-readable size

- [ ] Create `file-item.component.html`

  - Container with two parts:
    - `<lib-icon-label>` with computed icon, normal color, file name
    - Separate span for formatted file size
  - Click handler: `(click)` emits itemSelected

- [ ] Create `file-item.component.scss`

  - `:host` with cursor: pointer
  - Layout for icon-label + size display
  - Selected and hover states

- [ ] Create `file-item.component.spec.ts`
  - Test icon mapping for each FileItemType
  - Test file size formatting (bytes, KB, MB, GB)
  - Test itemSelected emits on click
  - Test selected class applied correctly

### Task 3: Update DirectoryFilesComponent

**Purpose**: Replace placeholder with Material table using DirectoryItem and FileItem components.

**Location**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/`

- [ ] Update `directory-files.component.ts`

  - Import MatTableModule
  - Import DirectoryItemComponent and FileItemComponent
  - Add signal: `selectedItem = signal<DirectoryItem | FileItem | null>(null)`
  - Computed: `combinedItems()` - merges directories and files with type field
  - Method: `isDirectory(item)` - type guard
  - Method: `isSelected(item)` - checks if item === selectedItem()
  - Method: `onItemSelected(item)` - sets selectedItem
  - Method: `onDirectoryDoubleClick(dir: DirectoryItem)` - calls storageStore.navigateToDirectory()
  - Inject StorageStore (already present)

- [ ] Update `directory-files.component.html`

  - Replace placeholder with `<table mat-table>`
  - Column: "name" - shows DirectoryItem or FileItem component based on type
  - Column: "size" - shows file size or empty for directories
  - Use `*matCellDef="let item"` with conditional rendering

- [ ] Update `directory-files.component.scss`

  - Table styling (full width, proper spacing)
  - Row hover effects
  - Selected row highlighting

- [ ] Update `directory-files.component.spec.ts`
  - Test combinedItems merges directories and files
  - Test type guard correctly identifies directories
  - Test selection updates on item click
  - Test navigateToDirectory called on directory double-click
  - Test table renders correct number of rows

### Task 4: Helper Functions & Utilities

**Purpose**: Reusable utility functions for file operations.

- [ ] File size formatter

  - Handles bytes → B, KB, MB, GB conversion
  - Returns string with unit (e.g., "1.5 MB")
  - Edge cases: 0 bytes, very large files

- [ ] Type guards
  - `isDirectoryItem(item)` - checks for DirectoryItem type
  - `isFileItem(item)` - checks for FileItem type

### Task 5: Integration & Styling

**Purpose**: Ensure components work together and look polished.

- [ ] Import new components in DirectoryFilesComponent
- [ ] Verify StorageStore integration:

  - Directory contents display correctly
  - Navigation updates directory-files view
  - Selection persists correctly

- [ ] Style polish:
  - Consistent padding/margins
  - Cursor changes (pointer on items)
  - Visual feedback on hover
  - Selection highlight color
  - Disabled state if no directory loaded

## 🗂️ File Changes Summary

### New Files (to create):

- `libs/features/player/.../directory-files/directory-item/directory-item.component.{ts,html,scss,spec.ts}`
- `libs/features/player/.../directory-files/file-item/file-item.component.{ts,html,scss,spec.ts}`

### Modified Files:

- `libs/features/player/.../directory-files/directory-files.component.{ts,html,scss,spec.ts}`

## 🧪 Testing Requirements

### Unit Tests

**DirectoryItemComponent**:

- [ ] Renders directory name correctly
- [ ] Uses folder icon with directory color
- [ ] Emits itemSelected on single click
- [ ] Emits itemDoubleClicked on double click
- [ ] Applies selected class when selected=true

**FileItemComponent**:

- [ ] Renders file name and size correctly
- [ ] Maps Unknown → insert_drive_file icon
- [ ] Maps Song → music_note icon
- [ ] Maps Game → sports_esports icon
- [ ] Maps Image → image icon
- [ ] Maps Hex → code icon
- [ ] Formats file sizes correctly (0 B, 1.5 KB, 2.3 MB, etc.)
- [ ] Emits itemSelected on click

**DirectoryFilesComponent**:

- [ ] Combines directories and files into single data source
- [ ] Type guard correctly identifies directories
- [ ] Selection updates when item clicked
- [ ] Calls navigateToDirectory with correct params on directory double-click
- [ ] Clears selection when directory changes

### Integration Tests

- [ ] Full user flow: click directory in tree → see files → double-click folder → navigate
- [ ] Selection state managed correctly across navigation
- [ ] Table updates reactively when StorageStore state changes
- [ ] Icons display correctly for all file types in real data

## ✅ Success Criteria

- [ ] DirectoryItemComponent displays with folder icon and directory color
- [ ] FileItemComponent shows correct icon per file type
- [ ] File sizes display in human-readable format
- [ ] Single click highlights row
- [ ] Double click on directory navigates into that directory
- [ ] All unit tests pass (100% coverage for new components)
- [ ] Integration with StorageStore works correctly
- [ ] UI is polished with proper hover/selected states

## 📝 Notes

### Component Patterns to Follow

- Use directory-tree-node as reference for IconLabelComponent usage
- Follow signal-based input/output pattern (Angular 19)
- Use `@if` / `@for` modern control flow (no `*ngIf`)
- ChangeDetection.OnPush for performance

### Type Discrimination Strategy

Add a discriminator field when merging:

```typescript
type CombinedItem = (DirectoryItem & { itemType: 'directory' }) | (FileItem & { itemType: 'file' });
```

### File Size Formatting

```typescript
formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${(bytes / Math.pow(k, i)).toFixed(1)} ${sizes[i]}`;
}
```
