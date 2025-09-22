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

- [ ] [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md) - Available reusable components (CompactCardLayoutComponent)
- [ ] [Phase 1 Documentation](./DIRECTORY_TRAIL_PLAN_P1.md) - Store action implementation
- [ ] [Phase 2 Documentation](./DIRECTORY_TRAIL_PLAN_P2.md) - Presentational components
- [ ] [STORE_TESTING.md](../../STORE_TESTING.md) - Store integration testing patterns

## 📋 Implementation Tasks

### Task 1: Smart Container Implementation

**Purpose**: Create DirectoryTrailComponent that integrates store and coordinates child components using CompactCardLayoutComponent.

- [ ] Create component directory structure
- [ ] Implement component class with store injection
- [ ] Create template integrating both child components with CompactCardLayoutComponent wrapper
- [ ] Wire up event handlers to store actions
- [ ] Add proper TypeScript typing and error handling

### Task 2: Store Integration

**Purpose**: Connect component to storage store and implement event coordination.

- [ ] Inject StorageStore and implement computed state selectors
- [ ] Implement navigation event handlers
- [ ] Handle loading states and error scenarios
- [ ] Add proper component lifecycle management

### Task 3: Container Integration Testing

**Purpose**: Test smart component store integration and event coordination.

- [ ] Test store injection and state reactivity
- [ ] Test event handler coordination
- [ ] Test child component integration
- [ ] Test error handling and edge cases

## 🗂️ File Changes

- [directory-trail/](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-trail/) - New smart component directory
- [features/player/src/index.ts](../../../libs/features/player/src/index.ts) - Export smart component

## 🧪 Testing Requirements

### Unit Tests

- [ ] Component initializes with proper store injection
- [ ] State selectors provide correct computed values
- [ ] Event handlers call correct store actions
- [ ] Loading and error states are handled appropriately
- [ ] Child component inputs are computed correctly
- [ ] Child component outputs trigger correct actions

## ✅ Success Criteria

- [ ] DirectoryTrailComponent successfully integrates with storage store
- [ ] All navigation events are properly coordinated
- [ ] Component provides proper data to child components
- [ ] Store actions are called correctly from event handlers
- [ ] Component follows established smart component patterns
- [ ] All unit tests pass with full coverage
- [ ] Ready to proceed to Phase 4 (styling and final integration)

## 📝 Notes

- Component should inject StorageStore and use computed selectors
- Event coordination must be comprehensive (up, refresh, breadcrumb navigation)
- Use CompactCardLayoutComponent from COMPONENT_LIBRARY.md for consistent layout wrapper
- Follow COMPONENT_LIBRARY.md patterns for Angular 19 standalone components
- Ensure proper error handling and loading state management
