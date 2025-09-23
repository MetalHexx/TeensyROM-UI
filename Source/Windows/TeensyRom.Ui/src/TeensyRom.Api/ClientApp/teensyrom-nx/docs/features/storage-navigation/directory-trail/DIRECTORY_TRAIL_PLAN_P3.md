# Phase 3: Smart Container Component

**High Level Plan Documentation**: [Directory Trail Plan](./DIRECTORY_TRAIL_PLAN.md)

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../STORE_TESTING.md)
- **Component Library**: [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md)

## 🎯 Objective

Create the smart container component that orchestrates store integration and event handling. This phase delivers the main DirectoryTrailComponent that coordinates the presentational components with storage state.

## 📚 Required Reading

- [x] [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md) - Available reusable components (CompactCardLayoutComponent)
- [x] [Phase 1 Documentation](./DIRECTORY_TRAIL_PLAN_P1.md) - Store action implementation
- [x] [Phase 2 Documentation](./DIRECTORY_TRAIL_PLAN_P2.md) - Presentational components
- [x] [STORE_TESTING.md](../../STORE_TESTING.md) - Store integration testing patterns

## 📋 Implementation Tasks

### Task 1: Smart Container Implementation

**Purpose**: Create DirectoryTrailComponent that integrates store and coordinates child components using CompactCardLayoutComponent. Move the breadcrumb and directory navigation components directories under this one.

- [x] Create component directory structure
- [x] Implement component class with store injection
- [x] Create template integrating both child components with CompactCardLayoutComponent wrapper
- [x] Wire up event handlers to store actions
- [x] Add proper TypeScript typing and error handling
- [x] Move the directory-breadcumb component directory to a directory under this one
- [x] Move the directory-navigation component directory to a directory under this one

### Task 2: Store Integration

**Purpose**: Connect component to storage store and implement event coordination.

- [x] Inject StorageStore and implement computed state selectors
- [x] Implement navigation event handlers
- [x] Handle loading states and error scenarios
- [x] Add proper component lifecycle management

### Task 3: Container Integration Testing

**Purpose**: Test smart component store integration and event coordination.

- [x] Test store injection and state reactivity
- [x] Test event handler coordination
- [x] Test child component integration
- [x] Test error handling and edge cases

## 🗂️ File Changes

- [directory-trail/](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-trail/) - New smart component directory
- [features/player/src/index.ts](../../../libs/features/player/src/index.ts) - Export smart component

## 🧪 Testing Requirements

### Unit Tests

- [x] Component initializes with proper store injection
- [x] State selectors provide correct computed values
- [x] Event handlers call correct store actions
- [x] Loading and error states are handled appropriately
- [x] Child component inputs are computed correctly
- [x] Child component outputs trigger correct actions

## ✅ Success Criteria

- [x] DirectoryTrailComponent successfully integrates with storage store
- [x] All navigation events are properly coordinated
- [x] Component provides proper data to child components
- [x] Store actions are called correctly from event handlers
- [x] Component follows established smart component patterns
- [x] All unit tests pass with full coverage
- [x] Ready to proceed to Phase 4 (styling and final integration)

## 📝 Notes

- Component should inject StorageStore and use computed selectors
- Event coordination must be comprehensive (up, refresh, breadcrumb navigation)
- Use CompactCardLayoutComponent from COMPONENT_LIBRARY.md for consistent layout wrapper
- Follow COMPONENT_LIBRARY.md patterns for Angular 19 standalone components
- Ensure proper error handling and loading state management
