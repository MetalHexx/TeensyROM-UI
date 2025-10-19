# Phase 2: Infrastructure Integration & Service Error Handling

## 🎯 Objective

Integrate the alert system throughout the infrastructure layer by adding automatic error handling to all services, ensuring that API failures automatically display error messages extracted from API response objects. This phase proves the alert system works end-to-end with real services before investing in visual polish (Phase 3). Success notifications are deferred to the application layer for explicit control over when operations warrant user feedback.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [Alert System Plan](./ALERT_SYSTEM_PLAN.md) - High-level feature plan and phases
- [ ] [Phase 1 Documentation](./ALERT_SYSTEM_P1.md) - Foundation built in Phase 1
- [ ] [Phase Template](../../PHASE_TEMPLATE.md) - Template structure for phase implementation

**Standards & Guidelines:**
- [ ] [Service Standards](../../SERVICE_STANDARDS.md) - Infrastructure service patterns
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches by layer
- [ ] [Overview Context](../../OVERVIEW_CONTEXT.md) - Clean Architecture and dependency rules

---

## 📂 File Structure Overview

```
libs/
├── infrastructure/src/lib/
│   ├── device/
│   │   ├── device.service.ts                     📝 Modified - Add alert integration
│   │   ├── device.service.spec.ts                📝 Modified - Add error alert tests
│   │   ├── device-logs.service.ts                📝 Modified - Add alert integration
│   │   ├── device-logs.service.spec.ts           📝 Modified - Add error alert tests
│   │   └── device-events.service.ts              📝 Modified - Add alert integration
│   │       device-events.service.spec.ts         📝 Modified - Add error alert tests
│   │
│   ├── storage/
│   │   ├── storage.service.ts                    📝 Modified - Add alert integration
│   │   └── storage.service.spec.ts               📝 Modified - Add error alert tests
│   │
│   └── player/
│       ├── player.service.ts                     ✅ Already has error handling (reference pattern)
│       └── player.service.spec.ts                📝 Modified - Update alert tests
```

---

## 📊 Service Audit Results

### Services Requiring Alert Integration

| Service | Observable Methods | Current Error Handling | Notes |
|---------|-------------------|------------------------|-------|
| **DeviceService** | `findDevices()`, `getConnectedDevices()`, `connectDevice()`, `disconnectDevice()`, `resetDevice()`, `pingDevice()` | ❌ None | 6 methods need catchError + alert |
| **DeviceLogsService** | `connect()`, `disconnect()` | ⚠️ Partial (logs to console) | SignalR connection errors need alerts |
| **DeviceEventsService** | `connect()`, `disconnect()` | ⚠️ Partial (logs to console) | SignalR connection errors need alerts |
| **StorageService** | `getDirectory()`, `search()` | ⚠️ Partial (logs + rethrows) | Has catchError but no alerts |
| **PlayerService** | `launchFile()`, `launchRandom()`, `toggleMusic()` | ⚠️ Partial (logs + rethrows) | Has catchError but **no alerts** - needs alert integration |

### Key Findings

1. **ALL services need alert integration** - none currently have `alertService.error()` calls
2. **PlayerService** has the most complete error handling structure (log + catchError + rethrow) - good foundation to build upon
3. **StorageService** has catchError blocks with logging but missing alert integration
4. **DeviceService** has no error handling at all - needs complete integration
5. **SignalR services** (DeviceLogsService, DeviceEventsService) handle errors in promise chains - need alert integration

---

<details open>
<summary><h3>Task 1: Integrate Alert Service into DeviceService</h3></summary>

**Purpose**: Add comprehensive error handling with alert notifications to all DeviceService operations. This service manages device discovery, connection, disconnection, reset, and ping operations.

**Related Documentation:**
- [PlayerService Reference Pattern](../../../libs/infrastructure/src/lib/player/player.service.ts) - Error handling pattern to follow
- [Service Standards](../../SERVICE_STANDARDS.md) - Error handling patterns
- [IAlertService Contract](../../../libs/domain/src/lib/contracts/alert.contract.ts) - Alert service interface

**Implementation Subtasks:**
- [ ] **Inject IAlertService**: Add `private readonly alertService = inject(ALERT_SERVICE)` to DeviceService constructor
- [ ] **Add catchError to findDevices()**: Extract error message from API response or use fallback "Failed to find devices", display error alert, rethrow
- [ ] **Add catchError to getConnectedDevices()**: Extract error message or use fallback "Failed to retrieve connected devices", display error alert, rethrow
- [ ] **Add catchError to connectDevice()**: Extract error message or use fallback "Failed to connect to device", display error alert, rethrow
- [ ] **Add catchError to disconnectDevice()**: Extract error message or use fallback "Failed to disconnect device", display error alert, rethrow
- [ ] **Add catchError to resetDevice()**: Extract error message or use fallback "Failed to reset device", display error alert, rethrow
- [ ] **Add catchError to pingDevice()**: Extract error message or use fallback "Failed to ping device", display error alert, rethrow
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**
- [ ] **Write Tests**: Test error handling triggers alerts correctly (see Testing Focus below)

**Key Implementation Notes:**
- Follow the exact pattern from PlayerService: `logError()` → `alertService.error()` → `throwError()`
- Use `inject(ALERT_SERVICE)` for DI, not constructor injection
- Extract error messages using: `error?.message` or `error?.error?.message` or fallback
- All error handlers should: extract message from error object or use fallback, log the error to console, display error alert to user, then rethrow the error for upstream handling
- Keep existing RxJS operators and logic - only add catchError blocks

**Error Message Fallbacks:**

| Method | Fallback Message |
|--------|------------------|
| `findDevices()` | "Failed to find devices" |
| `getConnectedDevices()` | "Failed to retrieve connected devices" |
| `connectDevice()` | "Failed to connect to device" |
| `disconnectDevice()` | "Failed to disconnect device" |
| `resetDevice()` | "Failed to reset device" |
| `pingDevice()` | "Failed to ping device" |

**Testing Focus for Task 1:**

**Behaviors to Test:**
- [ ] **findDevices error displays alert**: Mock API rejection, verify error alert called with correct message
- [ ] **findDevices error rethrows**: Verify error propagates after alert
- [ ] **getConnectedDevices error displays alert**: Mock API rejection, verify error alert with message
- [ ] **connectDevice error displays alert**: Mock API rejection, verify error alert with device context
- [ ] **disconnectDevice error displays alert**: Mock API rejection, verify error alert
- [ ] **resetDevice error displays alert**: Mock API rejection, verify error alert
- [ ] **pingDevice error displays alert**: Mock API rejection, verify error alert
- [ ] **Error message extraction works**: Test with error.message, error.error.message, and fallback scenarios
- [ ] **Check off completed tests**: Mark each test above as complete in this document as you verify it passes

**Testing Reference:**
- See [player.service.spec.ts](../../../libs/infrastructure/src/lib/player/player.service.spec.ts) for error handling test patterns
- Use Vitest's `vi.fn()` to mock alert service
- Verify both `alertService.error()` call AND error propagation in each test

</details>

---

<details open>
<summary><h3>Task 2: Integrate Alert Service into StorageService</h3></summary>

**Purpose**: Enhance existing error handling in StorageService by adding alert notifications. This service already has catchError blocks with console logging - we need to add alert integration following the established pattern.

**Related Documentation:**
- [PlayerService Reference Pattern](../../../libs/infrastructure/src/lib/player/player.service.ts) - Error handling pattern to follow
- [Current StorageService](../../../libs/infrastructure/src/lib/storage/storage.service.ts) - Review existing catchError blocks

**Implementation Subtasks:**
- [ ] **Inject IAlertService**: Add `private readonly alertService = inject(ALERT_SERVICE)` to StorageService constructor
- [ ] **Enhance getDirectory() catchError**: Add alert notification before rethrow (keep existing console.error, add alertService.error with fallback "Failed to retrieve directory")
- [ ] **Enhance search() catchError**: Add alert notification before rethrow (keep existing console.error, add alertService.error with fallback "Search operation failed")
- [ ] **Enhance index() error handling**: Add catchError block with logging, alert ("Failed to index storage"), and rethrow
- [ ] **Enhance indexAll() error handling**: Add catchError block with logging, alert ("Failed to index all storage"), and rethrow
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**
- [ ] **Write Tests**: Test error handling triggers alerts correctly (see Testing Focus below)

**Key Implementation Notes:**
- StorageService already has catchError blocks - enhance them, don't replace
- Keep existing `console.error()` calls for logging
- Add `alertService.error(message)` after console.error, before throwError
- Extract error messages using: `error?.message || error?.error?.message || fallback`
- Enhanced error handling should: extract message from error object or use fallback, log to console (keep existing console.error), display error alert, then rethrow the error

**Error Message Fallbacks:**

| Method | Fallback Message |
|--------|------------------|
| `getDirectory()` | "Failed to retrieve directory" |
| `search()` | "Search operation failed" |
| `index()` | "Failed to index storage" |
| `indexAll()` | "Failed to index all storage" |

**Testing Focus for Task 2:**

**Behaviors to Test:**
- [ ] **getDirectory error displays alert**: Mock API rejection, verify error alert called with message
- [ ] **getDirectory error rethrows**: Verify error propagates after alert
- [ ] **search error displays alert**: Mock API rejection, verify error alert with search context
- [ ] **search error rethrows**: Verify error propagates after alert
- [ ] **index error displays alert**: Mock API rejection, verify error alert
- [ ] **indexAll error displays alert**: Mock API rejection, verify error alert
- [ ] **Error message extraction works**: Test error.message, error.error.message, and fallback paths
- [ ] **Check off completed tests**: Mark each test above as complete in this document as you verify it passes

**Testing Reference:**
- See [storage.service.spec.ts](../../../libs/infrastructure/src/lib/storage/storage.service.spec.ts) for existing test patterns
- Add new test cases for alert integration
- Mock both FilesApiService (already mocked) and ALERT_SERVICE (new)

</details>

---

<details open>
<summary><h3>Task 3: Integrate Alert Service into DeviceLogsService (SignalR)</h3></summary>

**Purpose**: Add alert notifications to DeviceLogsService SignalR connection error handling. This service uses SignalR hub connections with promise-based error handling rather than RxJS observables.

**Related Documentation:**
- [DeviceLogsService Current Implementation](../../../libs/infrastructure/src/lib/device/device-logs.service.ts) - Review existing error handling
- [SignalR Error Handling](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client) - SignalR patterns

**Implementation Subtasks:**
- [ ] **Inject IAlertService**: Add `private readonly alertService = inject(ALERT_SERVICE)` to DeviceLogsService
- [ ] **Enhance connect() error handling**: In `.catch()` block, add alert notification after logError but before setting isConnected=false
- [ ] **Add error handler for API call failures**: Add catchError to `from(this.deviceService.startLogs())` observable with alert notification
- [ ] **Add error handler for disconnect failures**: Add catchError to `from(this.deviceService.stopLogs())` observable with alert notification
- [ ] **Add hub reconnection error handler**: Add error handler for SignalR automatic reconnection failures (onreconnectionfailed)
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**
- [ ] **Write Tests**: Test error handling triggers alerts correctly (see Testing Focus below)

**Key Implementation Notes:**
- SignalR uses promise-based error handling with `.catch()`, not RxJS `catchError`
- Keep existing `logError()` calls
- Add `alertService.error(message)` in promise catch blocks
- For observable API calls (`startLogs`, `stopLogs`), use RxJS catchError pattern
- SignalR promise error handling should: extract message from error or use fallback, log the error, display error alert, then update connection state signal
- Observable API call error handling should: extract message, log error, display alert, then rethrow for upstream handling

**Error Message Fallbacks:**

| Operation | Fallback Message |
|-----------|------------------|
| Hub connection start | "Failed to start device logs connection" |
| Hub reconnection | "Device logs connection lost" |
| startLogs API call | "Failed to start device logs" |
| stopLogs API call | "Failed to stop device logs" |

**Testing Focus for Task 3:**

**Behaviors to Test:**
- [ ] **Hub connection error displays alert**: Mock SignalR .start() rejection, verify error alert called
- [ ] **startLogs API error displays alert**: Mock API rejection, verify error alert
- [ ] **stopLogs API error displays alert**: Mock API rejection, verify error alert
- [ ] **Error message extraction works**: Test with various error structures
- [ ] **isConnected signal updates correctly**: Verify signal state after errors
- [ ] **Check off completed tests**: Mark each test above as complete in this document as you verify it passes

**Testing Reference:**
- Mock SignalR HubConnection using Vitest's `vi.fn()`
- Mock API service methods
- Mock alert service to verify error calls

</details>

---

<details open>
<summary><h3>Task 4: Integrate Alert Service into DeviceEventsService (SignalR)</h3></summary>

**Purpose**: Add alert notifications to DeviceEventsService SignalR connection error handling. Similar to DeviceLogsService, this uses SignalR hub connections with promise-based error handling.

**Related Documentation:**
- [DeviceEventsService Current Implementation](../../../libs/infrastructure/src/lib/device/device-events.service.ts) - Review existing error handling
- [SignalR Error Handling](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client) - SignalR patterns

**Implementation Subtasks:**
- [ ] **Inject IAlertService**: Add `private readonly alertService = inject(ALERT_SERVICE)` to DeviceEventsService
- [ ] **Enhance connect() error handling**: In `.catch()` block, add alert notification after logError
- [ ] **Add error handler for API call failures**: Add catchError to `from(this.deviceService.startDeviceEvents())` observable with alert notification
- [ ] **Add error handler for disconnect failures**: Add catchError to `from(this.deviceService.stopDeviceEvents())` observable with alert notification
- [ ] **Add hub reconnection error handler**: Add error handler for SignalR automatic reconnection failures (onreconnectionfailed)
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**
- [ ] **Write Tests**: Test error handling triggers alerts correctly (see Testing Focus below)

**Key Implementation Notes:**
- Follow same pattern as DeviceLogsService (Task 3)
- SignalR uses promise-based `.catch()`, not RxJS `catchError`
- Observable API calls use RxJS `catchError` pattern
- Keep existing `logError()` calls
- Add `alertService.error(message)` in all error handlers
- SignalR promise error handling should: extract message from error or use fallback, log the error, display error alert
- Observable API call error handling should: extract message, log error, display alert, then rethrow for upstream handling

**Error Message Fallbacks:**

| Operation | Fallback Message |
|-----------|------------------|
| Hub connection start | "Failed to start device events connection" |
| Hub reconnection | "Device events connection lost" |
| startDeviceEvents API call | "Failed to start device events" |
| stopDeviceEvents API call | "Failed to stop device events" |

**Testing Focus for Task 4:**

**Behaviors to Test:**
- [ ] **Hub connection error displays alert**: Mock SignalR .start() rejection, verify error alert called
- [ ] **startDeviceEvents API error displays alert**: Mock API rejection, verify error alert
- [ ] **stopDeviceEvents API error displays alert**: Mock API rejection, verify error alert
- [ ] **Error message extraction works**: Test with various error structures
- [ ] **Device event map remains stable**: Verify signal state integrity after errors
- [ ] **Check off completed tests**: Mark each test above as complete in this document as you verify it passes

**Testing Reference:**
- Use same testing patterns as DeviceLogsService (Task 3)
- Mock SignalR HubConnection
- Mock API service methods
- Mock alert service to verify calls

</details>

---

<details open>
<summary><h3>Task 5: Integrate Alert Service into PlayerService</h3></summary>

**Purpose**: Add alert notifications to PlayerService error handling. PlayerService already has the most complete error handling structure (log + catchError + rethrow) - we need to add the alert integration following the established pattern.

**Related Documentation:**
- [PlayerService Implementation](../../../libs/infrastructure/src/lib/player/player.service.ts) - Current implementation with good error handling foundation
- [Service Standards](../../SERVICE_STANDARDS.md) - Error handling patterns
- [IAlertService Contract](../../../libs/domain/src/lib/contracts/alert.contract.ts) - Alert service interface

**Implementation Subtasks:**
- [ ] **Inject IAlertService**: Add `private readonly alertService = inject(ALERT_SERVICE)` to PlayerService
- [ ] **Enhance launchFile() catchError**: Add `alertService.error(message)` after logError, extract message from error object or use fallback "Failed to launch file"
- [ ] **Enhance launchRandom() catchError**: Add `alertService.error(message)` after console.error, extract message or use fallback "Failed to launch random file"
- [ ] **Enhance toggleMusic() catchError**: Add `alertService.error(message)` after logError, extract message or use fallback "Failed to toggle music"
- [ ] **Update error message extraction**: Use `error?.message || error?.error?.message || fallback` pattern for all methods
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**
- [ ] **Write Tests**: Update existing tests and add alert verification (see Testing Focus below)

**Key Implementation Notes:**
- PlayerService already has catchError blocks - enhance them, don't replace
- Keep existing `logError()` calls (and one `console.error()` in launchRandom)
- Add `alertService.error(message)` after logging, before throwError
- Extract error messages using: `error?.message || error?.error?.message || fallback`
- Use `inject(ALERT_SERVICE)` for DI, not constructor injection
- Enhanced error handling should: extract message from error object or use fallback, log the error (keeping existing logging approach), display error alert, then rethrow the error
- Note: `launchRandom()` uses `console.error()` instead of `logError()` - maintain consistency with existing code

**Error Message Fallbacks:**

| Method | Fallback Message |
|--------|------------------|
| `launchFile()` | "Failed to launch file" |
| `launchRandom()` | "Failed to launch random file" |
| `toggleMusic()` | "Failed to toggle music" |

**Testing Focus for Task 5:**

**Behaviors to Test:**
- [ ] **launchFile error displays alert**: Mock API rejection, verify alertService.error called with correct message
- [ ] **launchFile error rethrows**: Verify error propagates after alert
- [ ] **launchRandom error displays alert**: Mock API rejection, verify alertService.error called with correct message
- [ ] **launchRandom error rethrows**: Verify error propagates after alert
- [ ] **toggleMusic error displays alert**: Mock API rejection, verify alertService.error called with correct message
- [ ] **toggleMusic error rethrows**: Verify error propagates after alert
- [ ] **Error message extraction from error.message**: Test direct error message path
- [ ] **Error message extraction from error.error.message**: Test nested error message path
- [ ] **Fallback message used**: Test fallback when no message in error object
- [ ] **Check off completed tests**: Mark each test above as complete in this document as you verify it passes

**Testing Reference:**
- See [player.service.spec.ts](../../../libs/infrastructure/src/lib/player/player.service.spec.ts) for existing error test structure
- Add alert service mock to TestBed configuration
- Verify both `alertService.error()` call AND error propagation in each test

</details>

---

<details open>
<summary><h3>Task 6: End-to-End Validation</h3></summary>

**Purpose**: Validate that the alert system integrates correctly with real infrastructure services in a running application. This task ensures the system works end-to-end with actual API calls and error scenarios.

**Related Documentation:**
- [Alert System Plan - Success Criteria](./ALERT_SYSTEM_PLAN.md#success-criteria) - End-to-end requirements
- [Testing Standards - Integration Tests](../../TESTING_STANDARDS.md#infrastructure-layer-libsinfrastructure) - Integration testing approach

**Implementation Subtasks:**
- [ ] **Start development server**: Run `pnpm start` to launch the application with dev backend
- [ ] **Test DeviceService errors**: Trigger findDevices error (disconnect backend), verify error alert displays
- [ ] **Test StorageService errors**: Trigger getDirectory error (invalid path or disconnected device), verify error alert displays
- [ ] **Test PlayerService errors**: Trigger launchFile error (invalid file path), verify error alert displays
- [ ] **Test SignalR errors**: Trigger DeviceLogsService connection error (stop backend), verify error alert displays
- [ ] **Verify error message quality**: Confirm error messages are user-friendly and actionable
- [ ] **Verify alert positioning**: Confirm alerts appear in correct position (default: bottom-right from Phase 1)
- [ ] **Verify auto-dismiss**: Confirm alerts auto-dismiss after 3 seconds
- [ ] **Verify manual dismiss**: Confirm dismiss button works correctly
- [ ] **Test error propagation**: Verify application handles errors gracefully after alerts display
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**
- [ ] **Manual Testing**: No automated tests - manual validation of alert system behavior

**Key Implementation Notes:**
- This is MANUAL testing - no automated test code
- Use browser DevTools to verify alert service calls and error handling
- Test with REAL backend API to catch integration issues
- Document any issues found in this section for immediate fixing
- Error scenarios to test:
  - Network disconnect (stop backend mid-operation)
  - Invalid parameters (wrong device ID, bad file path)
  - API errors (trigger 4xx/5xx responses if possible)
  - SignalR connection failures (stop backend before connecting)

**Validation Checklist:**

| Scenario | Expected Behavior | ✅ Verified |
|----------|-------------------|------------|
| DeviceService.findDevices() fails | Red error alert with "Failed to find devices" or API message | [ ] |
| DeviceService.connectDevice() fails | Red error alert with "Failed to connect to device" or API message | [ ] |
| StorageService.getDirectory() fails | Red error alert with "Failed to retrieve directory" or API message | [ ] |
| StorageService.search() fails | Red error alert with "Search operation failed" or API message | [ ] |
| PlayerService.launchFile() fails | Red error alert with launch error message | [ ] |
| DeviceLogsService.connect() fails | Red error alert with connection error message | [ ] |
| DeviceEventsService.connect() fails | Red error alert with connection error message | [ ] |
| Alert auto-dismisses | Alert disappears after 3 seconds | [ ] |
| Alert manual dismiss | Clicking X button removes alert immediately | [ ] |
| Error propagates | Application handles error after alert (doesn't crash) | [ ] |

**Issue Tracking:**

If issues are found during validation, document them here:

**Issue 1:**
- **Service**: 
- **Scenario**: 
- **Expected**: 
- **Actual**: 
- **Status**: 

**Issue 2:**
- **Service**: 
- **Scenario**: 
- **Expected**: 
- **Actual**: 
- **Status**: 

</details>

---

## ✅ Phase 2 Completion Checklist

**Code Implementation:**
- [ ] Task 1: DeviceService alert integration complete
- [ ] Task 2: StorageService alert integration complete
- [ ] Task 3: DeviceLogsService alert integration complete
- [ ] Task 4: DeviceEventsService alert integration complete
- [ ] Task 5: PlayerService tests updated with alert verification

**Testing:**
- [ ] All service unit tests passing with alert verification
- [ ] Integration tests confirm error → alert → rethrow flow
- [ ] End-to-end validation successful (Task 6)
- [ ] No console errors or warnings in browser

**Quality Checks:**
- [ ] All error messages are user-friendly and actionable
- [ ] Error handling follows consistent pattern across all services
- [ ] Alert service is properly injected via ALERT_SERVICE token
- [ ] Errors are logged before displaying alerts
- [ ] Errors are rethrown after displaying alerts for upstream handling

**Documentation:**
- [ ] All task subtasks marked complete in this document
- [ ] Any issues found documented in Task 6 Issue Tracking
- [ ] Code comments explain error handling patterns

---

## 📚 Reference Patterns

### Error Handling Pattern (RxJS Observable)

**What to implement:**
- Inject `ALERT_SERVICE` using Angular's `inject()` function
- Wrap API calls with RxJS `catchError` operator
- Inside catchError: extract error message from `error.message` or `error.error.message`, falling back to a contextual message if neither exists
- Log the error to console using `logError()` for debugging
- Call `alertService.error(message)` to display the alert to the user
- Rethrow the error using `throwError()` so upstream handlers can respond

### Error Handling Pattern (SignalR Promise)

**What to implement:**
- Inject `ALERT_SERVICE` using Angular's `inject()` function
- Use promise `.catch()` blocks for SignalR hub connection errors (not RxJS catchError)
- Inside catch block: extract error message from `err.message` or use a contextual fallback
- Log the error to console using `logError()`
- Call `alertService.error(message)` to display the alert
- Update relevant state (e.g., connection status signal)

### Test Pattern (Service Error Alert Verification)

**What to test:**
- Mock the API service to reject with various error structures
- Mock the alert service to verify `error()` was called
- Configure TestBed with both mocked services using dependency injection tokens
- Test that API failures trigger `alertService.error()` with the correct message
- Test error message extraction from `error.message`, `error.error.message`, and fallback scenarios
- Verify errors are rethrown (using `expect().rejects`) so upstream handlers receive them
- Assert `alertService.error()` was called exactly once with the expected message

---

## 🎯 Success Criteria

Phase 2 is complete when:

- [ ] ✅ All infrastructure services inject IAlertService via ALERT_SERVICE token
- [ ] ✅ All observable methods have catchError blocks with alert integration
- [ ] ✅ All SignalR promise chains have error handlers with alert integration
- [ ] ✅ Error messages extracted from API responses (error.message or error.error.message)
- [ ] ✅ Contextual fallback messages provided for all operations
- [ ] ✅ Error handling follows consistent pattern: log → alert → rethrow
- [ ] ✅ All service unit tests include alert service verification
- [ ] ✅ Integration tests confirm error → alert → rethrow flow works correctly
- [ ] ✅ End-to-end validation passes (real API errors display alerts)
- [ ] ✅ No automatic success alerts from infrastructure (application layer controls success notifications)
- [ ] ✅ PlayerService tests serve as reference pattern for other services
- [ ] ✅ All tests passing with >90% coverage for error handling code
- [ ] ✅ Browser console shows no errors or warnings
- [ ] ✅ Alerts display with basic styling from Phase 1 (no visual polish needed yet)

---

## 📝 Notes

### Implementation Philosophy

**Phase 2 Goal**: Prove the alert system works end-to-end with real services before investing in visual polish.

**Why This Phase Matters**:
- Establishes consistent error handling patterns across all infrastructure services
- Ensures API errors automatically provide user feedback without individual component handling
- Validates error message extraction from API responses works correctly
- Tests error propagation behavior (alert + rethrow) integrates properly with application layer
- Provides foundation for Phase 3 visual enhancements

**Key Decisions**:
- **No Automatic Success Alerts**: Infrastructure services do not display success alerts - application layer decides when operations warrant success feedback
- **Error Message Priority**: API messages (error.message or error.error.message) take precedence over fallbacks for better user context
- **Consistent Pattern**: All services follow PlayerService pattern (log → alert → rethrow) for maintainability
- **SignalR Special Handling**: SignalR services use promise `.catch()` blocks, not RxJS `catchError`, but follow same alert pattern

### Future Enhancements (Phase 3+)

- Visual polish: semantic color coding, animations, positioning options
- Stacking behavior for multiple simultaneous errors
- Custom alert duration per error severity
- Error recovery suggestions in alert messages
- Alert history/notification center

---

## 🔗 Related Documentation

- **[Alert System Plan](./ALERT_SYSTEM_PLAN.md)** - Overall feature plan and architecture
- **[Phase 1 Documentation](./ALERT_SYSTEM_P1.md)** - Foundation implementation
- **[Phase 3 Preview](./ALERT_SYSTEM_PLAN.md#phase-3-visual-design-positioning-and-animations)** - Next phase goals
- **[Service Standards](../../SERVICE_STANDARDS.md)** - Infrastructure service patterns
- **[Testing Standards](../../TESTING_STANDARDS.md)** - Testing approaches by layer
- **[Overview Context](../../OVERVIEW_CONTEXT.md)** - Clean Architecture principles
