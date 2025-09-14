# Phase 6: Breadcrumb and Back Navigation Specification

**Related Documentation**: [Player View Storage Navigation Plan](./BASIC_STORAGE_NAV_PLAN.md)

**Standards Documentation**:

- **Player Component Hierarchy**: ['PLAYER_COMPONENTS.md'](../../../../libs/features/player/src/PLAYER_COMPONENTS.md)
- **Coding Standards**: [`CODING_STANDARDS.md`](../../../CODING_STANDARDS.md) - Component and TypeScript standards.
- **Style Guide**: [`STYLE_GUIDE.md`](../../../STYLE_GUIDE.md) - CSS / Style / Theme design specifications and rules.
- **Testing Standards**: [`TESTING_STANDARDS.md`](../../../TESTING_STANDARDS.md) - Unit, integration, and E2E testing patterns.

## Objective

Add breadcrumb trail and back button navigation for intuitive directory traversal, completing the basic storage navigation user experience.

## Prerequisites

- Phase 5 completed: Basic File Listing Implementation
- Directory tree and file listing working together
- StorageStore managing directory navigation state properly

## Implementation Steps

### Step 1: Breadcrumb Trail Implementation

**Purpose**: Show current directory path as clickable breadcrumb navigation

**Tasks**:

1. **Breadcrumb Data Structure**

   - Parse current directory path into breadcrumb segments
   - Generate clickable path segments (Device > Storage > Dir1 > Dir2)
   - Handle root directory and empty path cases

2. **Breadcrumb Component**
   - Create or update component to display breadcrumb trail
   - Make each segment clickable for direct navigation
   - Connect breadcrumb clicks to StorageStore.navigateToDirectory()

### Step 2: Back Navigation Implementation

**Purpose**: Provide intuitive back button for parent directory navigation

**Tasks**:

1. **Back Button Logic**

   - Calculate parent directory path from current path
   - Enable/disable back button based on navigation depth
   - Handle root directory case (disable back button)

2. **Back Navigation Integration**
   - Add back button to appropriate UI location
   - Connect back action to StorageStore navigation
   - Coordinate with tree and file list updates

### Step 3: Navigation State Coordination

**Purpose**: Ensure all navigation methods work together seamlessly

**Tasks**:

1. **Multi-Method Navigation**

   - Coordinate tree clicks, breadcrumb clicks, and back button
   - Ensure all navigation updates tree selection and file list
   - Maintain consistent selected directory state

2. **User Experience Polish**
   - Add navigation loading states and transitions
   - Show appropriate feedback for navigation actions
   - Handle navigation errors gracefully

## Deliverables

- Breadcrumb trail component showing current directory path
- Back button for parent directory navigation
- Coordinated navigation between tree, breadcrumb, and back button
- Navigation state management and user feedback
- Component tests for breadcrumb and back navigation

## File Changes

```
libs/features/player/src/lib/player-view/player-device-container/storage-container/
├── breadcrumb-nav/                      # New breadcrumb component (optional)
│   ├── breadcrumb-nav.component.ts
│   ├── breadcrumb-nav.component.html
│   └── breadcrumb-nav.component.scss
├── storage-container.component.ts       # Add breadcrumb/back logic
├── storage-container.component.html     # Add breadcrumb + back button UI
└── storage-container.component.scss     # Navigation styling
```

## Testing Requirements

### Unit Tests

- Breadcrumb path parsing and segment generation
- Breadcrumb click navigation functionality
- Back button enabled/disabled state logic
- Parent directory path calculation

### Integration Tests

- Breadcrumb updates when directory changes via tree navigation
- Back button navigation coordinates with tree and file list
- Multiple navigation methods work together consistently
- Navigation error handling and recovery

## Success Criteria

- ✅ Breadcrumb trail shows current directory path with clickable segments
- ✅ Back button navigates to parent directory when available
- ✅ All navigation methods (tree, breadcrumb, back) work consistently
- ✅ Navigation state properly maintained across all components
- ✅ Loading states and error handling implemented
- ✅ User experience polished and intuitive
- ✅ Component tests verify breadcrumb and back navigation
- ✅ Basic storage navigation feature complete and ready for Phase 7

## Success Criteria (Complete Feature)

With Phase 6 completion, the basic storage navigation feature provides:

- ✅ **Multi-device support**: Independent navigation per device-storage combination
- ✅ **Hierarchical tree**: Device → Storage Type → Directory structure
- ✅ **File listing**: Current directory contents with file type classification
- ✅ **Multiple navigation methods**: Tree clicks, breadcrumb clicks, back button
- ✅ **Storage availability**: Only available storage shown (Internal always available)
- ✅ **State persistence**: Navigation state maintained across view changes
- ✅ **Performance optimized**: Flat state structure ready for virtual scrolling

## Notes

- This phase completes the core storage navigation user experience
- All major navigation patterns implemented and coordinated
- Foundation established for advanced features in Phases 7-8
- Ready for internal storage planning and virtual scrolling optimization
