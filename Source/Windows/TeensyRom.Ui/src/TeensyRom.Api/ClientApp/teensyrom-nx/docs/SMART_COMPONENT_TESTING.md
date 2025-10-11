# Smart Component Testing Methodology

This document describes the standard methodology for testing Angular smart components (feature components in `libs/features`) that depend on application layer services and state.

## Overview

**Smart Components** are feature layer components that:
- Inject and depend on application stores or context services
- Coordinate between multiple data sources
- Handle complex user interactions and workflows
- Manage local UI state in conjunction with global application state

**Testing Strategy**:
- Test components through their public interface (inputs, outputs, DOM)
- **Mock application layer dependencies using interfaces** (e.g., `IDeviceService`, `IStorageService`)
- Focus on component behavior, not implementation details
- Use Angular TestBed for realistic component instantiation

## Mocking Strategy

### Interface-Based Mocking

**Always mock using interfaces** - Feature components should depend on service contracts, not concrete implementations. This allows clean mocking at the test boundary.

**Setup Pattern**:
1. Create strongly-typed mocks implementing interfaces (e.g., `IDeviceService`, `IPlayerService`)
2. Use Vitest's `vi.fn()` for method mocks
3. Provide mocks via injection tokens in TestBed
4. For signal-based services, return writable signals from mock methods

**Signal Mocking**:
- Create writable signals in test setup for mutable state
- Return readonly versions from mock service methods
- Update signals during tests to trigger component reactions
- Allows testing component behavior in response to state changes

## Component Testing Patterns

### Input/Output Testing

**Test component inputs and outputs** - Verify that:
- Component correctly responds to input property changes
- Input changes trigger appropriate service method calls
- User interactions emit expected output events with correct data
- Event handlers are properly wired to user actions

### State-Dependent Rendering

**Test UI rendering based on state** - Verify that:
- Loading states display appropriate loading indicators
- Error states show error messages to the user
- Success states render data correctly in the DOM
- Empty states display placeholder or empty state UI
- DOM updates correctly when mock signal values change

### User Interaction Testing

**Test user interaction handling** - Verify that:
- Button clicks trigger expected service method calls
- Form submissions call appropriate service methods
- Keyboard interactions work as expected
- Double-clicks, context menus, and other interactions behave correctly
- Service methods are called with correct parameters from user input

## Testing Philosophy

### Component Layer vs Application Layer

**Component Tests (Feature Layer)**: Mock application layer dependencies using interfaces

- Focus on component-specific UI logic and user interactions
- Mock stores and context services using contract interfaces
- Test inputs, outputs, rendering, and event handling
- Keep tests fast and isolated from application state
- Verify correct service method calls with expected parameters

**Application Layer Tests**: Use real integrated components (see [STORE_TESTING.md](./STORE_TESTING.md))

- Test stores, context services, and application logic working together
- Mock only infrastructure layer (API services) using interfaces
- Validate complete workflows and state coordination
- See [`player-context.service.spec.ts`](../libs/application/src/lib/player/player-context.service.spec.ts) for examples

**Key Principle**: Always mock using interfaces - never mock concrete classes. This enforces proper dependency inversion and makes tests resilient to implementation changes.

## Behaviors to Test

Use this checklist to design comprehensive smart component tests:

### 1. Initialization
- [ ] Component initializes with correct default state
- [ ] Required services are injected properly
- [ ] Initial data loading triggered when appropriate
- [ ] Input signals properly bound to component properties

### 2. Input Handling
- [ ] Input changes trigger appropriate service calls
- [ ] Input validation and error handling
- [ ] Multiple input combinations work correctly
- [ ] Input changes update component state appropriately

### 3. Output Events
- [ ] User interactions emit correct output events
- [ ] Event payloads contain expected data
- [ ] Events fired at appropriate times
- [ ] No unwanted event emissions

### 4. State-Dependent Rendering
- [ ] Loading states display appropriate UI
- [ ] Error states show error messages
- [ ] Success states render data correctly
- [ ] Empty states handled gracefully

### 5. User Interactions
- [ ] Click handlers trigger expected actions
- [ ] Form submissions call appropriate services
- [ ] Keyboard interactions work correctly
- [ ] Touch/mobile interactions (if applicable)

### 6. Service Integration
- [ ] Service methods called with correct parameters
- [ ] Service responses handled appropriately
- [ ] Service errors displayed to user
- [ ] Service state changes update component UI

### 7. Complex Workflows
- [ ] Multi-step user workflows complete successfully
- [ ] State transitions between different UI modes
- [ ] Conditional logic based on service state
- [ ] Integration between multiple service dependencies

### 8. Edge Cases
- [ ] Null/undefined input handling
- [ ] Empty data sets handled gracefully
- [ ] Network failures and recovery
- [ ] Rapid user interactions don't cause issues

## Do / Don't

### Do
- **Use interfaces for all mocks** (e.g., `IPlayerContext`)
- Test through component's public interface (inputs, outputs, DOM)
- Use strongly typed mocks that implement contracts
- Test user interactions and their effects on the component
- Assert on DOM changes and component-specific state
- Use TestBed for realistic component instantiation
- Test error scenarios and edge cases in UI behavior
- Use injection tokens when providing mocked services

### Don't
- Mock concrete classes - always use interfaces when available.
- Test application layer logic in component tests (test that in application layer - see [STORE_TESTING.md](./STORE_TESTING.md))
- Use real stores or context services in component unit tests
- Make real HTTP calls in component tests
- Test internal component methods directly
- Mock Angular framework features unnecessarily
- Ignore accessibility in component tests
- Create mocks without implementing the full interface contract

## Quick Checklist

- [ ] TestBed setup with interface-based mocked service dependencies
- [ ] Strongly typed service mocks implementing contracts
- [ ] Injection tokens used for service providers
- [ ] Component initialization behavior
- [ ] Input handling and validation
- [ ] Output event emissions
- [ ] State-dependent rendering
- [ ] User interaction handling
- [ ] Service integration points (verify method calls)
- [ ] Complex workflow scenarios
- [ ] Edge cases and error handling

## Test Structure

### Recommended Organization

Structure tests by feature area, grouping related behaviors together:

**Initialization** - Component setup, dependency injection, default state

**Input Handling** - Response to input property changes, validation, edge cases

**User Interactions** - Click handlers, form submissions, keyboard events

**State-Dependent Rendering** - DOM updates based on service state (loading, error, success, empty)

**Service Integration** - Verification of service method calls with correct parameters

**Error Handling** - Error state display, recovery behaviors, edge cases

This structure ensures comprehensive coverage while keeping tests organized and maintainable.

## Related Documentation

- **Application Layer Testing**: See [STORE_TESTING.md](./STORE_TESTING.md) for testing stores, context services, and integrated application workflows
- **Testing Standards**: See [TESTING_STANDARDS.md](./TESTING_STANDARDS.md) for overall testing philosophy and layer-specific guidance
- **Utility Testing**: See examples like [`storage-key.util.spec.ts`](../libs/application/src/lib/storage/storage-key.util.spec.ts) and [`log-helper.spec.ts`](../libs/utils/src/lib/log-helper.spec.ts)