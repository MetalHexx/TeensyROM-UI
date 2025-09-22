# Phase 2: Presentational Components

**High Level Plan Documentation**: [Directory Trail Plan](./DIRECTORY_TRAIL_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **Style Guide**: [STYLE_GUIDE.md](../../STYLE_GUIDE.md)
- **Component Library**: [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md)

## 🎯 Objective

Create pure presentational components for navigation buttons and breadcrumb path display. This phase delivers two reusable components with comprehensive unit tests.

## 📚 Required Reading

- [ ] [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md) - Available reusable components and patterns
- [ ] [STYLE_GUIDE.md](../../STYLE_GUIDE.md) - Component styling standards
- [ ] [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md) - Component testing methodology
- [ ] [IconButtonComponent](../../../libs/ui/components/src/lib/icon-button/icon-button.component.ts) - Button integration reference
- [ ] [Player toolbar component](../../../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.html) - Icon button usage example

## 📋 Implementation Tasks

### Task 1: Directory Navigate Component

**Purpose**: Create pure presentational component for navigation buttons using IconButtonComponent from COMPONENT_LIBRARY.md.

- [ ] Create component directory structure
- [ ] Implement component class with inputs/outputs
- [ ] Create template with four navigation buttons using IconButtonComponent
- [ ] Add basic styling for button layout
- [ ] Export component from feature library

### Task 2: Directory Breadcrumb Component

**Purpose**: Create pure presentational component for breadcrumb path display.

- [ ] Create component directory structure
- [ ] Implement component class with path segmentation logic
- [ ] Create template with Material chips
- [ ] Add styling for responsive breadcrumb layout
- [ ] Export component from feature library

### Task 3: Component Unit Testing

**Purpose**: Comprehensive testing of both presentational components.

- [ ] Test DirectoryNavigateComponent input/output behavior
- [ ] Test button states and event emission
- [ ] Test DirectoryBreadcrumbComponent path calculation
- [ ] Test breadcrumb chip rendering and click events
- [ ] Test accessibility features and ARIA labels

## 🗂️ File Changes

- [directory-navigate/](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-navigate/) - New component directory
- [directory-breadcrumb/](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-breadcrumb/) - New component directory
- [features/player/src/index.ts](../../../libs/features/player/src/index.ts) - Export components

## 🧪 Testing Requirements

### Unit Tests

- [ ] Navigation component renders all four buttons correctly
- [ ] Button states (enabled/disabled) respond to inputs
- [ ] All button click events emit correct outputs
- [ ] Breadcrumb component calculates path segments correctly
- [ ] Breadcrumb chips render with correct labels and paths
- [ ] Chip click events emit correct navigation paths
- [ ] Edge cases handled (root paths, empty paths, special characters)

### Integration Tests

- [ ] Components work together in test harness
- [ ] Material Design integration functions correctly
- [ ] Components respect Angular theme settings
- [ ] Accessibility features work as expected

## ✅ Success Criteria

- [ ] DirectoryNavigateComponent renders navigation buttons with proper states
- [ ] DirectoryBreadcrumbComponent displays path as clickable chips
- [ ] All components emit events correctly when interacted with
- [ ] Components follow established styling patterns
- [ ] 100% unit test coverage with meaningful assertions
- [ ] Components are properly exported and reusable
- [ ] Ready to proceed to Phase 3 (smart container)

## 📝 Notes

- Components must be pure presentational (no store dependencies)
- Use existing IconButtonComponent from COMPONENT_LIBRARY.md for consistent navigation buttons
- Back/forward buttons should be disabled initially for future implementation
- Breadcrumb path calculation must handle all edge cases robustly
- Follow COMPONENT_LIBRARY.md patterns for Angular 19 standalone components with signal-based inputs
