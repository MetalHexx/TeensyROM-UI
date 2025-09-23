# Directory Browser Navigation Plan

**Project Overview**: Implement browser-like back/forward navigation for directory browsing with per-device history tracking. Users can navigate back and forward through their browsing history, with history behavior matching standard browser expectations.

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../../TESTING_STANDARDS.md)
- **State Standards**: [STATE_STANDARDS.md](../../../STATE_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../../STORE_TESTING.md)
- **Domain Standards**: [DOMAIN_STANDARDS.md](../../../DOMAIN_STANDARDS.md)
- **Style Guide**: [STYLE_GUIDE.md](../../../STYLE_GUIDE.md)

## 🎯 Project Objective

Create a browser-like navigation experience for directory browsing where users can:

- Navigate back to previously visited directories
- Navigate forward through history when available
- Have navigation history tracked per device
- Experience standard browser history behavior (new navigation clears forward history)

This enhances the user experience by providing familiar navigation patterns and reducing the need to manually navigate through directory hierarchies when revisiting locations.

## 📋 Implementation Phases

## Phase 1: Device Store State Enhancement

### Objective

Add navigation history tracking to the device store to maintain per-device browsing history and current position indicators.

### Key Deliverables

- [ ] Enhanced DeviceState with navigation history fields
- [ ] DirectoryHistoryEntry interface definition
- [ ] Initial state structure for history tracking

### High-Level Tasks

1. **State Interface Updates**: Add navigationHistory and currentHistoryIndex to DeviceState
2. **Type Definitions**: Create DirectoryHistoryEntry interface for history entries
3. **Initial State**: Configure default values for new state properties

**Detailed Documentation**: [Phase 1 - Device Store State Enhancement](./DIRECTORY_BROWSER_PLAN_P1.md)

---

## Phase 2: Storage Helper Functions

### Objective

Create centralized helper functions for managing directory navigation history with browser-like behavior patterns.

### Key Deliverables

- [ ] History management helper functions
- [ ] Browser-like history behavior logic
- [ ] History query and manipulation utilities

### High-Level Tasks

1. **History Helpers**: Create functions for adding, navigating, and querying history
2. **Browser Logic**: Implement standard browser history clearing behavior
3. **Query Utilities**: Add functions to check navigation availability

**Detailed Documentation**: [Phase 2 - Storage Helper Functions](./DIRECTORY_BROWSER_PLAN_P2.md)

---

## Phase 3: Update Existing Actions

### Objective

Integrate history tracking into existing navigation actions to ensure all directory changes are properly recorded in browsing history.

### Key Deliverables

- [ ] Updated initialize-storage action with history tracking
- [ ] Enhanced navigate-to-directory action with history integration
- [ ] Modified navigate-up-one-directory action with history support

### High-Level Tasks

1. **Initialize Storage**: Add initial directory to history on storage initialization
2. **Directory Navigation**: Record directory changes in browsing history
3. **Up Navigation**: Track parent directory navigation in history

**Detailed Documentation**: [Phase 3 - Update Existing Actions](./DIRECTORY_BROWSER_PLAN_P3.md)

---

## Phase 4: New Navigation Actions

### Objective

Create dedicated actions for backward and forward navigation that operate purely on history without making new API calls.

### Key Deliverables

- [ ] navigate-directory-backward action implementation
- [ ] navigate-directory-forward action implementation
- [ ] Comprehensive test coverage for new actions

### High-Level Tasks

1. **Backward Navigation**: Implement history-based backward navigation
2. **Forward Navigation**: Implement history-based forward navigation
3. **Testing**: Add comprehensive test coverage for new navigation patterns

**Detailed Documentation**: [Phase 4 - New Navigation Actions](./DIRECTORY_BROWSER_PLAN_P4.md)

---

## Phase 5: Component Integration

### Objective

Wire up the back/forward button functionality in the directory navigation component and integrate with the new store actions.

### Key Deliverables

- [ ] Enhanced directory-trail component with history integration
- [ ] Updated directory-navigate component with navigation state
- [ ] Proper event handling for back/forward actions

### High-Level Tasks

1. **Trail Component**: Add computed signals and event handlers for navigation
2. **Navigate Component**: Update inputs for navigation state and button enabling
3. **Event Integration**: Wire up back/forward button clicks to store actions

**Detailed Documentation**: [Phase 5 - Component Integration](./DIRECTORY_BROWSER_PLAN_P5.md)

## 🏗️ Architecture Overview

### Key Design Decisions

- **Device Store Location**: History tracking belongs in device store (not storage store) since it's device-specific behavior
- **Browser-like Behavior**: Standard browser history semantics where new navigation clears forward history when in middle of history
- **No API Calls for History Navigation**: Back/forward operations only change selected directory based on history, no new data loading

### Integration Points

- **Device Store**: Maintains navigation history and current position per device
- **Storage Store**: Continues to handle directory content and selected directory state
- **Directory Components**: React to history state for button enabling and handle navigation events

## 🧪 Testing Strategy

### Unit Tests

- [ ] Device store history management functionality
- [ ] Storage helper functions for history manipulation
- [ ] New backward/forward navigation actions
- [ ] Updated existing actions with history integration

### Integration Tests

- [ ] End-to-end navigation flow with history tracking
- [ ] Component integration with store actions
- [ ] Cross-store communication between device and storage stores

### E2E Tests

- [ ] Complete user navigation scenarios with back/forward
- [ ] History clearing behavior on new navigation
- [ ] Multi-device history isolation

## ✅ Success Criteria

- [ ] Users can navigate backward through their browsing history
- [ ] Users can navigate forward when available in history
- [ ] History behaves like standard browser navigation
- [ ] Navigation history is maintained per device independently
- [ ] All existing navigation functionality continues to work unchanged
- [ ] Comprehensive test coverage for new functionality
- [ ] Project ready for production deployment

## 📚 Related Documentation

- **Architecture Overview**: [`OVERVIEW_CONTEXT.md`](../../../OVERVIEW_CONTEXT.md)
- **Technical Debt**: [`TECHNICAL_DEBT.md`](../../../TECHNICAL_DEBT.md)
- **State Standards**: [`STATE_STANDARDS.md`](../../../STATE_STANDARDS.md)
- **Store Testing**: [`STORE_TESTING.md`](../../../STORE_TESTING.md)

## 📝 Notes

- History tracking should be lightweight and not impact performance
- Implementation should follow established STATE_STANDARDS.md patterns
- All testing should follow STORE_TESTING.md methodology
- Consider future enhancements like history persistence across sessions
- Maintain clear separation between navigation state and content state
