# Device State & Logs E2E Testing Plan

## 🎯 Project Objective

Build E2E test infrastructure to validate **SignalR-based real-time features**: device state updates and device log streaming. These features use SignalR hubs for push-based communication rather than REST APIs.

**Primary Goal**: Test that device state changes and log messages properly flow from backend SignalR hubs through the infrastructure layer to UI components.

**System Value**: Validates the real-time communication layer that provides live device status updates and streaming logs, ensuring users see accurate device state and diagnostic information.

**Core Principles**:

- **SignalR Hub Mocking**: Intercept SignalR negotiation and message protocols
- **Event-Driven Testing**: Verify UI responds to pushed events, not polled data
- **Signal-Based State**: Test Angular signals react to SignalR event updates
- **Realistic Scenarios**: Simulate common device state transitions and log patterns
- **Independent of REST API**: These features use separate communication channels

---

## 📋 Architecture Overview

### Real-Time Communication Flow

```
Backend SignalR Hub
    ↓ (WebSocket/SSE)
Infrastructure Service (DeviceEventsService/DeviceLogsService)
    ↓ (Angular Signals)
Feature Components (device-item, device-logs)
    ↓ (UI Updates)
User sees live state/logs
```

### Two SignalR Systems

**1. Device Events Hub** (`/deviceEventHub`):

- **Purpose**: Push device state changes (Connected, Busy, ConnectionLost, etc.)
- **Infrastructure**: `DeviceEventsService` maintains state map, exposes signals
- **UI Consumer**: `device-item.component.ts` shows device state via computed signal
- **Event Format**: `{ deviceId: string, state: DeviceState }`
- **API Endpoints**: `POST /devices/start-events`, `POST /devices/stop-events`

**2. Device Logs Hub** (`/logHub`):

- **Purpose**: Stream device diagnostic logs
- **Infrastructure**: `DeviceLogsService` maintains log buffer (200 lines max)
- **UI Consumer**: `device-logs.component.ts` displays logs with auto-scroll
- **Event Format**: `string` (log line)
- **API Endpoints**: `POST /devices/start-logs`, `POST /devices/stop-logs`

### Key Architecture Notes

- **Dual Communication**: Each hub requires both REST API call (start/stop) AND WebSocket connection
- **Signal-Based**: Both services expose Angular signals that components consume
- **Connection Lifecycle**: Services handle connect/disconnect/reconnect logic
- **Error Handling**: Both services integrate with alert service for connection failures

---

## 🔍 What Needs Testing

### Device State Features

**UI Behaviors to Validate**:

- Device cards display correct state text (Connected, Busy, ConnectionLost, etc.)
- Device cards apply correct styling based on state (dimmed for disconnected)
- State updates happen in real-time when SignalR events arrive
- Multiple devices can have different states simultaneously
- State persists correctly in the signal-based state map

**Technical Requirements**:

- Mock SignalR negotiation handshake (`/deviceEventHub/negotiate`)
- Mock SignalR WebSocket connection and message protocol
- Emit device state change events (`DeviceEvent` with deviceId + state)
- Verify `DeviceEventsService` signal updates trigger UI changes
- Test state map updates for multiple devices

### Device Logs Features

**UI Behaviors to Validate**:

- Logs component displays streaming log lines
- Logs auto-scroll to bottom as new lines arrive
- Start/stop/clear buttons work correctly
- Connection status indicator shows connected/disconnected state
- Log buffer respects 200-line limit
- Logs component handles rapid message bursts

**Technical Requirements**:

- Mock SignalR negotiation handshake (`/logHub/negotiate`)
- Mock SignalR WebSocket connection and message protocol
- Emit log line events (`LogProduced` with string payload)
- Verify `DeviceLogsService` signal updates trigger UI changes
- Test auto-scroll behavior
- Test log buffer overflow (201st message removes 1st)

---

## 📊 Test Scenarios

### Device State Test Scenarios

**Scenario 1: Single Device State Updates**

- Given a connected device
- When state changes from Connected → Busy → Connected
- Then UI reflects each state transition correctly

**Scenario 2: Multiple Device States**

- Given 3 devices with different states (Connected, Busy, ConnectionLost)
- When viewing device list
- Then each device shows its correct state independently

**Scenario 3: State Styling Updates**

- Given a connected device (not dimmed)
- When device state changes to ConnectionLost
- Then device card applies dimmed styling

**Scenario 4: Initial State vs Live Updates**

- Given device view loads with devices from REST API
- When SignalR events arrive with updated states
- Then UI shows SignalR state, not REST API state

**Scenario 5: State During Connection Lifecycle**

- Given device view opens (SignalR not connected)
- When SignalR connection establishes
- When state events start arriving
- Then UI updates correctly after connection

### Device Logs Test Scenarios

**Scenario 1: Log Streaming**

- Given logs component opened
- When start logs button clicked
- When log events arrive from hub
- Then logs display in real-time

**Scenario 2: Connection Status**

- Given logs component opened
- When start logs clicked → connection indicator shows connected
- When stop logs clicked → connection indicator shows disconnected

**Scenario 3: Auto-Scroll Behavior**

- Given logs component with several lines
- When new log arrives
- Then viewport scrolls to bottom automatically

**Scenario 4: Clear Logs**

- Given logs component with 10 lines
- When clear button clicked
- Then all logs removed from display

**Scenario 5: Log Buffer Overflow**

- Given logs component with 200 lines
- When 5 more log lines arrive
- Then only most recent 200 lines remain (oldest 5 removed)

**Scenario 6: Rapid Log Bursts**

- Given logs component connected
- When 50 log lines arrive within 1 second
- Then all 50 lines display correctly
- Then auto-scroll keeps up with burst

---

## 🛠️ Implementation Phases

### Phase 1: SignalR Mocking Research & Spike

**Objective**: Understand how to mock SignalR hubs in Cypress and validate approach

**Deliverables**:

- [ ] Research Cypress SignalR mocking strategies
- [ ] Document SignalR negotiation protocol (JSON payload format)
- [ ] Document SignalR message protocol (how events are sent)
- [ ] Create proof-of-concept interceptor for negotiation
- [ ] Verify event messages can be injected into client
- [ ] Document chosen approach and rationale

**Key Questions to Answer**:

- Can we intercept SignalR negotiate endpoint and return mock connection info?
- Can we inject SignalR events via `window.postMessage` or direct hub mock?
- Do we need to stub `@microsoft/signalr` library or just intercept HTTP?
- What's the format of SignalR event messages?

**Success Criteria**:

- [ ] Clear documentation of SignalR mocking approach
- [ ] Working proof-of-concept that injects one device state event
- [ ] Verified Angular component receives and displays event

---

### Phase 2: Device State Event Interceptors

**Objective**: Create interceptor utilities for mocking device state events

**Deliverables**:

- [ ] `device-state-events.interceptors.ts` with SignalR mocking functions
- [ ] Support for negotiation handshake mocking
- [ ] Support for injecting device state change events
- [ ] Support for multiple device states in single test
- [ ] Documentation of interceptor usage

**File Structure**:

```
apps/teensyrom-ui-e2e/src/support/interceptors/
├── device.interceptors.ts              # Existing REST API interceptors
└── device-state-events.interceptors.ts # NEW: SignalR state event interceptors
```

**Key Functions Needed**:

- `interceptDeviceEventHub(options)` - Mock negotiation and connection
- `emitDeviceStateEvent(deviceId, state)` - Inject state change event
- `emitMultipleDeviceStates(events[])` - Inject multiple state events

**Testing Focus**: Verify events properly trigger `DeviceEventsService` signal updates

---

### Phase 3: Device State E2E Tests

**Objective**: Test device state display and updates in device view

**Deliverables**:

- [ ] Test file: `device-state.cy.ts`
- [ ] Tests for single device state updates
- [ ] Tests for multiple device states
- [ ] Tests for state styling (dimmed/not dimmed)
- [ ] Tests for state persistence across navigation
- [ ] All tests passing consistently

**File Structure**:

```
apps/teensyrom-ui-e2e/src/e2e/devices/
├── device-discovery.cy.ts        # Existing (REST API tests)
└── device-state.cy.ts            # NEW: SignalR state tests
```

**Success Criteria**:

- [ ] All 6 skipped tests from Phase 4 now passing with SignalR mocking
- [ ] Additional state transition tests passing
- [ ] Tests validate signal-based reactivity
- [ ] Zero flakiness

---

### Phase 4: Device Logs Event Interceptors

**Objective**: Create interceptor utilities for mocking device log streaming

**Deliverables**:

- [ ] `device-logs-events.interceptors.ts` with SignalR log mocking functions
- [ ] Support for log hub negotiation handshake
- [ ] Support for injecting log line events (single and burst)
- [ ] Support for connection status simulation
- [ ] Documentation of interceptor usage

**File Structure**:

```
apps/teensyrom-ui-e2e/src/support/interceptors/
├── device.interceptors.ts              # Existing REST API interceptors
├── device-state-events.interceptors.ts # Phase 2
└── device-logs-events.interceptors.ts  # NEW: SignalR log event interceptors
```

**Key Functions Needed**:

- `interceptLogHub(options)` - Mock negotiation and connection
- `emitLogLine(logText)` - Inject single log line
- `emitLogBurst(logLines[])` - Inject multiple log lines rapidly
- `simulateLogConnection(connected: boolean)` - Control connection state

**Testing Focus**: Verify log events properly update `DeviceLogsService` signals

---

### Phase 5: Device Logs E2E Tests

**Objective**: Test device logs display, streaming, and controls

**Deliverables**:

- [ ] Test file: `device-logs.cy.ts`
- [ ] Tests for log streaming (start/stop/clear)
- [ ] Tests for connection status indicator
- [ ] Tests for auto-scroll behavior
- [ ] Tests for log buffer overflow
- [ ] Tests for rapid log bursts
- [ ] All tests passing consistently

**File Structure**:

```
apps/teensyrom-ui-e2e/src/e2e/devices/
├── device-discovery.cy.ts        # Existing (REST API tests)
├── device-state.cy.ts            # Phase 3
└── device-logs.cy.ts             # NEW: SignalR logs tests
```

**Success Criteria**:

- [ ] All log streaming scenarios test passing
- [ ] Auto-scroll behavior validated
- [ ] Log buffer management verified
- [ ] Connection lifecycle tested
- [ ] Zero flakiness

---

### Phase 6 (Optional): Integration Tests

**Objective**: Test interactions between device state, logs, and other features

**Deliverables**:

- [ ] Tests combining device discovery + state updates
- [ ] Tests for state/logs interaction with device connection workflow
- [ ] Tests for navigation with active SignalR connections
- [ ] Tests for reconnection scenarios

**Out of Scope (for now)**:

- Player-related SignalR features
- Complex multi-device orchestration
- Performance testing of SignalR under load

---

## 🔧 Technical Considerations

### SignalR Protocol Challenges

**Negotiation Handshake**:

- Client calls `POST /deviceEventHub/negotiate` (or `/logHub/negotiate`)
- Backend responds with connection details (connectionId, availableTransports, etc.)
- Client establishes WebSocket or SSE connection using connectionId

**Message Protocol**:

- SignalR uses specific message format for events: `{ type: 1, target: "DeviceEvent", arguments: [...] }`
- Need to understand exact format to properly inject events
- May need to mock at WebSocket protocol level or stub SignalR client library

**Connection Lifecycle**:

- Services call REST API (`/devices/start-events`) then connect to hub
- Tests must mock both REST endpoint AND hub connection
- Automatic reconnection logic may complicate testing

### Mocking Strategies (To Be Researched in Phase 1)

**Option 1: HTTP Intercept + Event Injection**

- Intercept negotiate endpoint with `cy.intercept`
- Stub WebSocket connection
- Inject events via `cy.window().then(win => win.postMessage(...))`

**Option 2: Library Stubbing**

- Stub `@microsoft/signalr` library imports
- Replace `HubConnection` with mock that exposes event emitter
- Inject events via mock hub connection

**Option 3: Hybrid Approach**

- Intercept negotiate for connection setup
- Use Cypress tasks to control event emission timing
- Leverage Angular TestBed for signal verification in component tests (not E2E)

**Decision**: Will be made in Phase 1 after spike research

### Testing Approach

**Signal Reactivity Validation**:

- E2E tests will verify UI updates, not signal internals
- Signal updates are implementation detail; focus on DOM changes
- Use `cy.get()` assertions on visible text/classes/styles

**Timing and Synchronization**:

- SignalR events are asynchronous
- Tests must `cy.wait()` or use retries for event propagation
- Auto-scroll behavior may need explicit timing controls

**Test Isolation**:

- Each test should clean up SignalR connections
- Tests should not leak event subscriptions between specs
- Use `beforeEach`/`afterEach` to ensure clean state

---

## ✅ Success Criteria

**Phase 1 (Research)**:

- [ ] SignalR mocking approach documented and validated
- [ ] Proof-of-concept works for one event type
- [ ] Technical feasibility confirmed

**Phase 2-3 (Device State)**:

- [ ] Device state interceptors created
- [ ] All 6 previously skipped tests now passing
- [ ] Device state updates validated in multiple scenarios
- [ ] Zero flakiness in state tests

**Phase 4-5 (Device Logs)**:

- [ ] Device logs interceptors created
- [ ] All log streaming scenarios tested
- [ ] Log buffer management validated
- [ ] Connection lifecycle tested
- [ ] Zero flakiness in log tests

**Overall Success**:

- [ ] 100% of device state E2E tests passing
- [ ] 100% of device logs E2E tests passing
- [ ] Documentation explains SignalR testing approach
- [ ] Reusable interceptor utilities for future SignalR testing

---

## 📝 Open Questions

### For Phase 1 Research

1. **SignalR Protocol**: What is the exact format of SignalR negotiate response and message payloads?
2. **Mocking Strategy**: Can we intercept at HTTP level, or must we stub the SignalR client library?
3. **Event Injection**: How do we trigger events in a way that the Angular services receive them?
4. **Connection Lifecycle**: How do we handle automatic reconnection logic in tests?
5. **WebSocket vs SSE**: Does backend use WebSocket or Server-Sent Events? Does it matter for testing?

### For Implementation Phases

6. **Test Isolation**: How do we ensure SignalR connections don't leak between tests?
7. **Timing**: What's the typical latency between event emission and UI update? How do we handle it?
8. **Error Scenarios**: Should we test SignalR connection failures and reconnection?
9. **Performance**: Should we test log streaming under high-volume scenarios (1000+ logs)?
10. **Browser Compatibility**: Do SignalR mocks work consistently across Electron and Chrome?

---

## 🚀 Next Steps

**To Start Phase 1**:

1. Research SignalR client library documentation
2. Inspect network traffic when connecting to real backend hub
3. Document negotiate endpoint request/response format
4. Document WebSocket message format for events
5. Create spike test that attempts to mock one device state event
6. Validate Angular component receives mocked event
7. Document findings and choose mocking approach

**Once Phase 1 Complete**:

- Proceed to Phase 2 (Device State Interceptors)
- Use research findings to implement robust interceptor utilities

**Phase Planning Sessions**:

- Each implementation phase (2-5) will have detailed planning using `PHASE_TEMPLATE.md`
- Focus on one phase at a time
- Validate each phase before moving to next

---

## 📚 Related Documentation

- **E2E_PLAN.md**: Overall E2E testing plan (REST API focus)
- **E2E_PLAN_P4_RESULTS.md**: Phase 4 results showing 6 skipped device state tests
- **TESTING_STANDARDS.md**: General testing approach and patterns
- **OVERVIEW_CONTEXT.md**: Architecture overview including infrastructure layer

---

## 🎯 Why This Matters

**User Impact**:

- Device state display is critical for users to know if their device is connected/busy
- Device logs are essential diagnostic tool for troubleshooting connection issues
- Real-time updates provide better UX than polling

**Technical Impact**:

- Validates signal-based reactivity in Angular components
- Tests integration between infrastructure services and UI
- Ensures SignalR communication layer works correctly
- Prevents regressions in real-time features

**Project Impact**:

- Completes Phase 4 by enabling the 6 skipped device state tests
- Establishes pattern for testing other SignalR features (player events, etc.)
- Provides reusable interceptors for future real-time feature testing
- Achieves 100% E2E test coverage for device management workflows
