# Application Layer Behavioral Testing

This document describes the standard methodology for testing application layer services, stores, and workflows in this repository. It promotes behavioral testing through facades and context services rather than direct store testing.

## Overview

- Test application layer behaviors through facade/context services, not stores directly.
- Allow real integration between services, stores, and utilities within the application layer.
- Mock only at the infrastructure boundary using strongly typed interface contracts.
- Focus on observable outcomes and complete workflows, not implementation details.
- Avoid testing store actions, selectors, or internal methods in isolation.

## Philosophy: Behavioral Testing vs Unit Testing

**Behavioral Testing** validates that the application layer delivers correct outcomes for user workflows and use cases. Tests should answer "does this feature work correctly?" rather than "does this store method update state correctly?"

**Why Test Facades Instead of Stores**:

- Facades represent the actual API that components consume - test what's really used.
- Store actions and selectors are implementation details that may change during refactoring.
- Behavioral tests survive refactoring better because they validate outcomes, not mechanics.
- Integration between stores, services, and utilities catches coordination bugs that unit tests miss.
- Mirrors real application usage patterns more accurately.

**Integration Scope**: Within the application layer, use real implementations. Only mock at the infrastructure boundary where external systems (HTTP, SignalR, file system) are accessed.

## Environment & Setup

- **Test Runner**: Vitest via `@nx/vite:test`
- **Environment**: jsdom for Angular TestBed support
- **Setup File**: `src/test-setup.ts` initializes Angular TestBed and Zones
- **TestBed Configuration**: Provide facade under test, real stores, mocked infrastructure services

## Mocking Strategy

**Mock at Infrastructure Boundary Only**:

Infrastructure services are defined as interface contracts in the domain layer. Mock these contracts using strongly typed mocks with Vitest's `vi.fn()`. Never mock stores, utilities, or other application layer components.

**Test Boundary Pattern**:

- **Real**: Facade/context service under test
- **Real**: Application layer stores used by the facade
- **Real**: Shared utilities and helper functions
- **Mocked**: Infrastructure services implementing domain contracts (IDeviceService, IPlayerService, IStorageService)

**Mock Setup**:

Define typed mock functions matching interface signatures. Provide mocks via injection tokens in TestBed. Control mock return values to simulate various infrastructure responses.

## Facade Testing Pattern

**Service Under Test**: Inject the facade/context service as the primary subject. The facade coordinates stores and infrastructure to deliver behaviors.

**Async Handling**: Use helper function to flush microtask queue after invoking asynchronous facade methods. This ensures all reactive updates complete before assertions.

**State Assertions**: Read state through facade signal getters. Never access store internals directly. Assert on observable outcomes visible to consuming components.

## Behaviors to Test

Design tests around observable user workflows and feature behaviors. Focus on end-to-end outcomes rather than individual state mutations.

### 1. Feature Initialization & Lifecycle

- Feature initializes correctly for new contexts (devices, sessions, tenants)
- Re-initialization is idempotent and doesn't corrupt existing state
- Cleanup properly removes context-scoped state without affecting other contexts
- Multi-context isolation: independent state and behaviors per context key

### 2. Primary User Workflows

- Complete workflows from user action to final observable outcome
- Infrastructure calls made with correct parameters
- Success paths update state correctly and clear previous errors
- Loading states appear and clear appropriately during async operations

### 3. Error Handling & Recovery

- Infrastructure failures set observable error states without throwing
- Error states display meaningful information to consuming components
- Subsequent successful operations clear previous errors
- Failed operations leave system in consistent, recoverable state

### 4. State Transitions & Coordination

- Observable state changes match expected transitions for user actions
- Multiple coordinated updates complete atomically from component perspective
- State consistency maintained across rapid successive operations
- Workflow steps execute in correct order with proper state at each stage

### 5. Edge Cases & Boundary Conditions

- Empty or missing data handled gracefully
- Invalid inputs handled without corrupting state
- Concurrent operations resolve to consistent final state
- Operations on uninitialized or removed contexts fail safely

## Do / Don't

### Do

- Test facades/context services that components actually consume
- Use real stores and utilities within the application layer
- Mock only infrastructure services at domain contract boundaries
- Use strongly typed mocks implementing full interface contracts
- Assert on observable outcomes through facade signal getters
- Verify infrastructure calls with expected parameters
- Test complete workflows from user action to final state
- Focus on behaviors answering "does this feature work?"

### Don't

- Test store methods, actions, or selectors directly
- Mock application layer components (stores, utilities, helpers)
- Assert on internal store state or implementation details
- Test individual store methods in isolation
- Use `any` types in mocks - always strongly type mock functions
- Make real HTTP calls or access real external systems
- Test what state transitions occurred - test what outcomes are observable

## Test Organization

Structure tests by user-facing feature behaviors:

- **Initialization & Lifecycle**: Context setup, multi-context isolation, cleanup
- **Primary Workflows**: Complete user journeys from action to outcome (e.g., "launch file with context", "navigate to next file")
- **Playback Controls**: State transitions observable through user controls (play, pause, stop)
- **Navigation**: Sequential and shuffle mode behaviors, directory context loading
- **Error Handling**: Infrastructure failure scenarios and recovery
- **State Transitions**: Complex multi-step workflows and state consistency

## Quick Checklist

- [ ] TestBed setup with facade under test, real stores, mocked infrastructure services
- [ ] Strongly typed infrastructure mocks via injection tokens
- [ ] Initialization and multi-context isolation behaviors
- [ ] Primary user workflow scenarios tested end-to-end
- [ ] Error handling and recovery paths validated
- [ ] State transitions tested through observable outcomes
- [ ] Edge cases and concurrent operation handling
- [ ] All assertions via facade signals, never accessing store internals

## Reference Example

For a comprehensive example of behavioral application layer testing following this methodology:

- [`player-context.service.spec.ts`](../libs/application/src/lib/player/player-context.service.spec.ts) - Full facade testing with real store integration
