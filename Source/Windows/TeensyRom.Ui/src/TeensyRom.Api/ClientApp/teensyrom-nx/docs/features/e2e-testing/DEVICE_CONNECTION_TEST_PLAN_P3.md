# Device Connection Test Plan - Phase 3: Connection State Refresh and Recovery

**Phase Goal**: Validate device refresh workflows maintain connection states correctly, test reconnection after connection loss, and ensure refresh operations integrate properly with connection management.

**Prerequisites**: 
- ✅ Phase 1 complete (single device connection tests)
- ✅ Phase 2 complete (multi-device connection tests)
- ✅ Existing refresh helper: `clickRefreshDevices()` from Phase 4 (Device Discovery)
- ✅ All Phase 1 & 2 test helpers and fixtures available

---

## 📋 Tasks Breakdown

### Task 1: Refresh Connected Device Tests

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-connection.cy.ts` (NEW)

**Objective**: Test that clicking "Refresh Devices" button maintains connection states for already-connected devices.

**Test Structure**:
```
describe('Device Connection - Refresh & Recovery', () => {
  describe('Refresh Connected Devices', () => {
    // Test cases here
  });
});
```

**Test Cases**:

1. **"should maintain single connected device after refresh"**
   - Setup: `interceptFindDevices({ fixture: singleDevice })` (connected)
   - Navigate, wait for initial discovery
   - Verify device connected: `verifyConnected(0)`
   - Re-register interceptor: `interceptFindDevices({ fixture: singleDevice })`
   - Click refresh: `clickRefreshDevices()`
   - Wait for discovery: `waitForDeviceDiscovery()`
   - Verify device still connected: `verifyConnected(0)`

2. **"should maintain all connected devices after refresh"**
   - Setup: `multipleDevices` fixture (3 connected devices)
   - Verify all 3 connected initially
   - Refresh devices
   - Verify all 3 still connected after refresh

3. **"should preserve device information after refresh"**
   - Setup: single connected device
   - Note device ID, port, firmware (or verify with `verifyFullDeviceInfo(0)`)
   - Refresh
   - Verify same device information displayed

4. **"should not trigger reconnection API for already-connected devices"**
   - Setup: connected device
   - Use inline interceptor to spy on connect API
   - Refresh devices
   - Verify NO additional connect API calls made
   - Only discover API should be called

**Fixtures Used**: `singleDevice`, `multipleDevices`

**Interceptors Used**: 
- `interceptFindDevices()` - Re-register before refresh to respond to new request
- Verify no calls to `interceptConnectDevice()` during refresh

**Notes**:
- May need to track API calls to verify connection API not called
- Refresh should use discovery data to determine existing connection state

---

### Task 2: Refresh Disconnected Device Tests

**File**: Same file - `device-refresh-connection.cy.ts`

**Objective**: Test that refresh maintains disconnected state for disconnected devices.

**Test Structure**:
```
describe('Refresh Disconnected Devices', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should maintain single disconnected device after refresh"**
   - Setup: `disconnectedDevice` fixture
   - Verify disconnected initially: `verifyDisconnected(0)`
   - Refresh devices
   - Verify still disconnected: `verifyDisconnected(0)`

2. **"should maintain all disconnected devices after refresh"**
   - Setup: `threeDisconnectedDevices` fixture (from Phase 2)
   - Verify all disconnected initially
   - Refresh
   - Verify all still disconnected

3. **"should not auto-reconnect previously disconnected devices"**
   - Setup: disconnected device
   - Refresh
   - Verify device remains disconnected (not auto-reconnected)
   - User must manually click power button to reconnect

4. **"should preserve disconnected device information after refresh"**
   - Setup: disconnected device
   - Verify device info displayed despite disconnected state
   - Refresh
   - Verify device info still displayed

**Fixtures Used**: `disconnectedDevice`, `threeDisconnectedDevices`

**Interceptors Used**: `interceptFindDevices()` with disconnected fixtures

---

### Task 3: Refresh Mixed State Tests

**File**: Same file - `device-refresh-connection.cy.ts`

**Objective**: Test refresh with devices in various connection states simultaneously.

**Test Structure**:
```
describe('Refresh Mixed Connection States', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should maintain mixed connection states after refresh"**
   - Setup: `mixedConnectionDevices` (from Phase 2)
   - Device 1: connected, Device 2: disconnected, Device 3: connected
   - Verify initial states
   - Refresh devices
   - Verify states maintained: device 1 connected, device 2 disconnected, device 3 connected

2. **"should handle user-created mixed states through refresh"**
   - Setup: `threeDisconnectedDevices`
   - Connect device 1 and device 3 (leave device 2 disconnected)
   - Refresh devices
   - Verify device 1 connected, device 2 disconnected, device 3 connected after refresh

3. **"should update device list while preserving connection states"**
   - Setup: mixed states
   - Refresh devices
   - Verify device count correct (may need `verifyDeviceCount()`)
   - Verify each device's connection state preserved

4. **"should maintain visual state indicators after refresh"**
   - Setup: mixed states
   - After refresh, verify:
     - Connected devices: not dimmed, power button highlighted
     - Disconnected devices: dimmed, power button normal

**Fixtures Used**: `mixedConnectionDevices`, `threeDisconnectedDevices`

**Notes**:
- These tests validate the most complex scenario: mixed states persisting through refresh
- Critical for multi-device user workflows

---

### Task 4: Reconnection After Refresh Tests

**File**: Same file - `device-refresh-connection.cy.ts`

**Objective**: Test that users can reconnect to previously disconnected devices after refresh.

**Test Structure**:
```
describe('Reconnection After Refresh', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should allow reconnection to disconnected device after refresh"**
   - Setup: `disconnectedDevice` fixture
   - Refresh devices
   - Device remains disconnected after refresh
   - Click power button to connect: `clickPowerButton(0)`
   - Add `interceptConnectDevice()`
   - Wait for connection: `waitForConnection()`
   - Verify connected: `verifyConnected(0)`

2. **"should reconnect with correct device ID after refresh"**
   - Setup: disconnected device
   - Refresh
   - Use inline interceptor to capture connect API request
   - Click power button
   - Verify request URL contains correct device ID from fixture

3. **"should allow selective reconnection after refresh"**
   - Setup: `threeDisconnectedDevices`
   - Refresh
   - Connect only device 2 (index 1)
   - Verify device 2 connected, devices 1 and 3 remain disconnected

4. **"should handle full reconnection workflow after refresh"**
   - Setup: disconnected device
   - Refresh
   - Connect device (full workflow)
   - Refresh again
   - Verify device still connected (connection persists through second refresh)

**Fixtures Used**: `disconnectedDevice`, `threeDisconnectedDevices`

**Interceptors Used**: 
- `interceptFindDevices()` for refresh
- `interceptConnectDevice()` for reconnection attempts

---

### Task 5: Refresh During Connection Tests

**File**: Same file - `device-refresh-connection.cy.ts`

**Objective**: Test edge case where user refreshes while connection operation is in progress.

**Test Structure**:
```
describe('Refresh During Connection', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should handle refresh clicked during connection in progress"**
   - Setup: `disconnectedDevice` fixture
   - Add `interceptConnectDevice()` with delay to simulate slow connection
   - Click power button to start connection
   - Before connection completes, click refresh
   - Verify app doesn't crash
   - Verify final state is consistent (either connected or disconnected, not stuck)

2. **"should handle refresh clicked during disconnection in progress"**
   - Setup: `singleDevice` (connected)
   - Add `interceptDisconnectDevice()` with delay
   - Click power button to start disconnection
   - Before disconnection completes, click refresh
   - Verify final state is consistent

3. **"should allow connection after interrupted refresh"**
   - Setup: disconnected device
   - Start refresh (with delay)
   - Before refresh completes, click power button
   - Verify operation completes without error
   - Final state should be clear (connected or disconnected)

**Implementation Notes**:
- Use `delay` option in interceptors to create timing windows
- These are edge case tests - behavior may vary based on implementation
- Focus: app should not crash or enter inconsistent state

**Pattern**:
```typescript
cy.intercept('POST', 'http://localhost:5168/devices/*/connect', (req) => {
  req.reply({
    delay: 1000, // Delay to create timing window
    statusCode: 200,
    body: response,
  });
}).as('connectDevice');
```

---

### Task 6: Refresh Error Handling Tests

**File**: Same file - `device-refresh-connection.cy.ts`

**Objective**: Test that refresh errors are handled gracefully without losing connection state.

**Test Structure**:
```
describe('Refresh Error Handling', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should preserve connection state when refresh fails"**
   - Setup: `singleDevice` (connected)
   - Verify device connected
   - Register error mode: `interceptFindDevices({ errorMode: true })`
   - Click refresh - should fail
   - Wait for error
   - Verify device connection state unchanged (still connected)

2. **"should preserve mixed states when refresh fails"**
   - Setup: `mixedConnectionDevices`
   - Note initial states (device 1 connected, device 2 disconnected, device 3 connected)
   - Refresh with error mode
   - Verify states unchanged after error

3. **"should display error message on refresh failure"**
   - Setup: any fixture
   - Refresh with error mode
   - Verify error message displayed (use `verifyErrorMessage()` from test-helpers)
   - **Note**: Detailed alert validation in Phase 4 (DEVICE_CONNECTION_TEST_PLAN_P4_ALERTS.md)

4. **"should allow retry after refresh failure"**
   - Setup: connected device
   - First refresh fails (error mode)
   - Re-register success mode: `interceptFindDevices({ fixture: singleDevice })`
   - Refresh again - should succeed
   - Verify device list updated
   - Verify connection state preserved

**Fixtures Used**: `singleDevice`, `mixedConnectionDevices`

**Interceptors Used**: `interceptFindDevices({ errorMode: true })` then switch to success mode

---

### Task 7: Connection State Persistence Tests

**File**: Same file - `device-refresh-connection.cy.ts`

**Objective**: Test that connection states persist correctly through multiple refresh cycles.

**Test Structure**:
```
describe('Connection State Persistence', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should persist connection through multiple refreshes"**
   - Setup: `singleDevice` (connected)
   - Verify connected
   - Refresh #1 - verify still connected
   - Refresh #2 - verify still connected
   - Refresh #3 - verify still connected
   - Connection state stable through multiple refreshes

2. **"should persist disconnection through multiple refreshes"**
   - Setup: `disconnectedDevice`
   - Verify disconnected
   - Multiple refreshes (3x)
   - Verify remains disconnected after all refreshes

3. **"should persist user-initiated state changes through refreshes"**
   - Setup: `disconnectedDevice`
   - Connect device (user action)
   - Refresh - verify connected
   - Disconnect device (user action)
   - Refresh - verify disconnected
   - User actions persist through refresh cycles

4. **"should handle connect-refresh-disconnect-refresh workflow"**
   - Setup: disconnected device
   - Connect → Refresh → still connected
   - Disconnect → Refresh → still disconnected
   - Full lifecycle with refreshes interspersed

**Fixtures Used**: `singleDevice`, `disconnectedDevice`

**Notes**:
- These tests validate long-running sessions with multiple operations
- Ensures state management is robust through repeated operations

---

### Task 8: Add Test Helper for Refresh Workflows (Optional)

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts`

**Objective**: Optionally add helper for refresh-specific workflows if patterns emerge.

**Potential New Helpers**:

1. **`refreshAndWait()`** - Combines click and wait
   ```typescript
   export function refreshAndWait(): void {
     clickRefreshDevices();
     waitForDeviceDiscovery();
   }
   ```

2. **`verifyStateUnchanged(deviceIndex: number, isConnected: boolean)`**
   - Comprehensive check for state persistence
   - Verifies connection state + visual indicators match expected state

**Decision Point**:
- Only add if these patterns are used frequently across Phase 3 tests
- May not be necessary if existing helpers suffice

---

### Task 9: Phase 3 Test Documentation

**File**: Header comments in `device-refresh-connection.cy.ts`

**Objective**: Document refresh and recovery test patterns.

**Actions**:
1. Comprehensive header explaining refresh workflow testing
2. Document timing considerations (delays for in-progress operations)
3. Document state persistence expectations
4. Note integration with Phase 1 and Phase 2 tests
5. Add troubleshooting notes for timing-sensitive tests

**Example Header**:
```typescript
/**
 * Device Connection Refresh & Recovery E2E Tests (Phase 3)
 * 
 * Tests device refresh workflows and connection state persistence.
 * Validates that "Refresh Devices" button maintains connection states correctly
 * and that reconnection workflows function after refresh.
 * 
 * Key Workflows:
 * - Refresh maintains connected device states
 * - Refresh maintains disconnected device states
 * - Reconnection after refresh works correctly
 * - Refresh during in-progress operations handled gracefully
 * - Multiple refresh cycles maintain state correctly
 * 
 * Timing Considerations:
 * - Some tests use interceptor delays to create timing windows
 * - Edge case tests may be flaky if timing is too tight
 * - Focus on consistent final state, not intermediate states
 * 
 * Integration:
 * - Builds on Phase 1 (single device) and Phase 2 (multi-device) patterns
 * - Uses all existing test helpers and fixtures
 * - Adds refresh complexity to connection workflows
 */
```

---

## 🎯 Success Criteria

**Task Completion**:
- [ ] New test file `device-refresh-connection.cy.ts` created
- [ ] ~20-24 test cases implemented covering:
  - [ ] Refresh connected devices (4 tests)
  - [ ] Refresh disconnected devices (4 tests)
  - [ ] Refresh mixed states (4 tests)
  - [ ] Reconnection after refresh (4 tests)
  - [ ] Refresh during connection (3 tests)
  - [ ] Refresh error handling (4 tests)
  - [ ] Connection state persistence (4 tests)
- [ ] Test documentation complete
- [ ] All tests passing consistently (100% pass rate over 3 runs)
- [ ] Phase 1 and Phase 2 tests still passing (regression check)

**Test Quality**:
- Tests validate state persistence through refresh
- Tests handle timing edge cases (refresh during operations)
- Tests verify no unwanted reconnections
- Tests verify manual reconnection works after refresh
- Error scenarios handled gracefully

**Integration Quality**:
- All Phase 1 and Phase 2 helpers reused
- Consistent test patterns across all 3 phases
- No regressions in previous test suites
- Combined test suite runs in reasonable time (< 5 minutes total)

---

## 📝 Implementation Notes

### Key Files Modified
1. `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-connection.cy.ts` - NEW file
2. `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts` - Optional: Add `refreshAndWait()` helper

### Key Files Reused (No Changes)
1. All test helpers from Phase 1
2. All fixtures from Phase 1 and Phase 2
3. All interceptors from existing infrastructure
4. `clickRefreshDevices()` from Phase 4 (Device Discovery)

### Refresh Testing Pattern
```typescript
// Standard refresh pattern
beforeEach(() => {
  interceptFindDevices({ fixture: singleDevice });
  navigateToDeviceView();
  waitForDeviceDiscovery();
});

it('maintain connection after refresh', () => {
  verifyConnected(0); // Initial state
  
  // Re-register interceptor for refresh
  interceptFindDevices({ fixture: singleDevice });
  clickRefreshDevices();
  waitForDeviceDiscovery();
  
  verifyConnected(0); // State maintained
});
```

### Timing Edge Case Pattern
```typescript
it('refresh during connection', () => {
  interceptConnectDevice({ delay: 1000 }); // Not a real option - use inline interceptor
  
  // Inline interceptor with delay
  cy.intercept('POST', '*/devices/*/connect', (req) => {
    req.reply({
      delay: 1000,
      statusCode: 200,
      body: response,
    });
  }).as('connectDevice');
  
  clickPowerButton(0); // Start connection
  
  // Click refresh before connection completes
  cy.wait(200); // Partial wait
  clickRefreshDevices();
  
  // Verify final state is consistent (exact state may vary)
  cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
});
```

### Interceptor Re-registration Pattern
- Before each refresh, re-register `interceptFindDevices()` to respond to new request
- Interceptors are route handlers - re-registering updates the response

### Connection State Verification Strategy
- After refresh, verify both visual state (dimmed class, power button color) AND functional state
- Functional: Can perform operations (connect/disconnect) correctly
- Visual: UI reflects current state accurately

---

## ❓ Questions for Phase 3

**Resolved**:
- ✅ Refresh helper exists: `clickRefreshDevices()` from Phase 4
- ✅ Interceptor re-registration: Standard Cypress pattern, re-register before refresh

**To Verify During Implementation**:
1. **State Persistence Mechanism**: How does app track connection state? SignalR events? Local state?
2. **Refresh Behavior**: Does refresh query backend for current connection state, or rely on local state?
3. **Auto-Reconnect**: Any scenarios where refresh triggers auto-reconnect?
4. **Timing Sensitivity**: Are timing edge case tests too flaky? May need to adjust or skip if unreliable.

**Potential Open Questions**:
1. Should connection preferences persist across browser sessions (localStorage)?
2. Should app attempt automatic reconnection periodically in background?
3. What's the expected behavior if device physically disconnected then refresh clicked?

---

## 🔗 Integration with Phases 1 & 2

**Dependencies**:
- All Phase 1 helpers reused
- All Phase 2 fixtures reused
- Builds on established connection testing patterns

**Regression Testing**:
- After Phase 3, run full test suite (Phases 1-3)
- All ~50 tests should pass
- No performance degradation (reasonable test execution time)

**Shared Infrastructure**:
- Same test helpers, interceptors, fixtures
- Phase 3 adds new test file, minimal/no changes to existing files

---

## 🚀 Ready to Implement

Phase 3 completes the Device Connection test coverage by adding refresh and recovery scenarios. Combined with Phases 1 and 2, this provides comprehensive validation of all connection workflows.

**Estimated Test Count**: 20-24 test scenarios
**Estimated Implementation Time**: 5-7 hours for experienced developer
**Priority**: High - Completes connection testing coverage
**Dependencies**: Phases 1 and 2 must be complete and passing

**Note**: Alert message validation for refresh scenarios is in Phase 4 (see DEVICE_CONNECTION_TEST_PLAN_P4_ALERTS.md).

**Total Phase 1-3 Coverage**: ~50-60 test scenarios across all connection workflows

---

## 📊 Full Test Suite Summary (After Phase 3)

**Phase 1** (Single Device): 12-15 tests
- Connection success
- Disconnection success
- Connection errors
- Disconnection errors
- Visual feedback

**Phase 2** (Multi-Device): 18-22 tests
- Independent connection/disconnection
- Sequential operations
- Mixed states
- Error isolation
- Device count validation

**Phase 3** (Refresh & Recovery): 20-24 tests
- Refresh connected/disconnected devices
- Refresh mixed states
- Reconnection after refresh
- Refresh during operations
- Error handling
- State persistence

**Total**: 50-61 comprehensive connection test scenarios

**Test Execution Time Estimate**: 4-6 minutes for full suite
**Coverage**: Complete connection lifecycle from discovery → connect → disconnect → refresh → reconnect
