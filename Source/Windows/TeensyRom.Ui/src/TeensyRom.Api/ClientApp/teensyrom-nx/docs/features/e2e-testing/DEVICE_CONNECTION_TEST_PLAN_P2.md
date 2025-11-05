# Device Connection Test Plan - Phase 2: Multi-Device Connection Workflows

**Phase Goal**: Validate connection/disconnection state isolation across multiple TeensyROM devices. Ensure connection state changes for one device don't affect other devices, and UI correctly displays mixed connection states.

**Prerequisites**:

- ✅ Phase 1 complete (single device connection tests passing)
- ✅ Test helpers from Phase 1 working: `clickPowerButton`, `verifyConnected`, `verifyDisconnected`, etc.
- ✅ Existing multi-device fixture: `multipleDevices` (3 connected devices)

---

## 📋 Tasks Breakdown

### Task 0: Verify Existing Infrastructure

**Files**:

- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts`
- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.spec.ts`
- `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts`

**Objective**: Verify Phase 1 helpers exist and are ready for reuse in multi-device scenarios.

**Actions**:

1. **Verify Phase 1 Helpers Exist**:

   - ✅ Connection helpers from Phase 1: `clickPowerButton`, `waitForConnection`, `waitForDisconnection`
   - ✅ Verification helpers: `verifyConnected`, `verifyDisconnected`, `verifyConnectionError`
   - ✅ All ready to use with device index parameter for multi-device testing

2. **Verify Existing Fixtures**:
   - ✅ `multipleDevices` fixture exists (3 connected devices)
   - ✅ Ready to use for disconnection tests
   - Need to create: `threeDisconnectedDevices`, `mixedConnectionDevices` (Task 1)

**Result**: Phase 1 infrastructure ready. Proceed to Task 1 to create new fixtures.

---

### Task 1: Create Multi-Device Fixture Variants

**File**: `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts`

**Objective**: Add fixture variants for multi-device connection testing scenarios.

**New Fixtures to Add**:

1. **`threeDisconnectedDevices`** - All devices disconnected

   - Uses faker seed for reproducibility
   - Generates 3 devices with `isConnected: false`, `deviceState: DeviceState.ConnectionLost`
   - Provides clean slate for sequential connection tests

2. **`mixedConnectionDevices`** - Devices in different connection states
   - Device 1: Connected (default)
   - Device 2: Disconnected (`isConnected: false`, `deviceState: DeviceState.ConnectionLost`)
   - Device 3: Connected (default)
   - Tests realistic multi-device scenarios with varied states

**Pattern to Follow**:

```typescript
export const threeDisconnectedDevices: MockDeviceFixture = (() => {
  faker.seed(12345);
  return {
    devices: [
      generateDevice({ isConnected: false, deviceState: DeviceState.ConnectionLost }),
      generateDevice({ isConnected: false, deviceState: DeviceState.ConnectionLost }),
      generateDevice({ isConnected: false, deviceState: DeviceState.ConnectionLost }),
    ],
  };
})();
```

**Notes**:

- Follow existing fixture documentation patterns
- Use same faker seed for consistency
- Add JSDoc comments explaining use cases

**Vitest Validation**:

- Add test suite to `devices.fixture.spec.ts` for each new fixture
- Validate fixture structure matches `MockDeviceFixture` interface
- Verify device states are correct (disconnected, mixed)
- Verify device uniqueness (IDs, ports, names)
- Follow patterns from existing `singleDevice`, `multipleDevices` tests

**Example Vitest Test**:

```typescript
describe('threeDisconnectedDevices Fixture', () => {
  it('should contain exactly 3 devices', () => {
    expect(threeDisconnectedDevices.devices).toHaveLength(3);
  });

  it('should have all devices disconnected', () => {
    threeDisconnectedDevices.devices.forEach((device) => {
      expect(device.isConnected).toBe(false);
      expect(device.deviceState).toBe(DeviceState.ConnectionLost);
    });
  });

  it('should have unique device IDs', () => {
    const deviceIds = threeDisconnectedDevices.devices.map((d) => d.deviceId);
    const uniqueIds = new Set(deviceIds);
    expect(uniqueIds.size).toBe(3);
  });
});
```

---

### Task 2: Independent Connection Tests

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection-multi.cy.ts` (NEW)

**Objective**: Test that connecting one device doesn't affect other devices' connection states.

**Test Structure**:

```
describe('Device Connection - Multi-Device', () => {
  describe('Independent Connection', () => {
    // Test cases here
  });
});
```

**Test Cases**:

1. **"should connect device 1 while devices 2 and 3 remain disconnected"**

   - Setup: `interceptFindDevices({ fixture: threeDisconnectedDevices })`
   - Add `interceptConnectDevice()` (responds to any device ID)
   - Navigate, wait for discovery
   - Verify all 3 devices initially disconnected
   - Connect device 1: `clickPowerButton(0)`, `waitForConnection()`
   - Verify device 1 connected: `verifyConnected(0)`
   - Verify devices 2 and 3 still disconnected: `verifyDisconnected(1)`, `verifyDisconnected(2)`

2. **"should connect device 2 while devices 1 and 3 remain disconnected"**

   - Same pattern but click device 2's power button
   - Verify only device 2 is connected after operation

3. **"should connect device 3 while devices 1 and 2 remain disconnected"**

   - Same pattern but click device 3's power button
   - Verify only device 3 is connected after operation

4. **"should maintain independent power button states"**
   - Start with all disconnected
   - Connect device 1
   - Check power button colors: device 1 = highlight, devices 2-3 = normal
   - Verify device card styling: device 1 not dimmed, devices 2-3 dimmed

**Fixtures Used**: `threeDisconnectedDevices` (new)

**Interceptors Used**:

- `interceptFindDevices({ fixture: threeDisconnectedDevices })`
- `interceptConnectDevice()` - Works for any device ID via wildcard route

---

### Task 3: Independent Disconnection Tests

**File**: Same file - `device-connection-multi.cy.ts`

**Objective**: Test that disconnecting one device doesn't affect other devices' connection states.

**Test Structure**:

```
describe('Independent Disconnection', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should disconnect device 1 while devices 2 and 3 remain connected"**

   - Setup: `interceptFindDevices({ fixture: multipleDevices })` (all connected)
   - Add `interceptDisconnectDevice()`
   - Navigate, verify all connected initially
   - Disconnect device 1: `clickPowerButton(0)`, `waitForDisconnection()`
   - Verify device 1 disconnected: `verifyDisconnected(0)`
   - Verify devices 2 and 3 still connected: `verifyConnected(1)`, `verifyConnected(2)`

2. **"should disconnect device 2 while devices 1 and 3 remain connected"**

   - Same pattern but disconnect device 2
   - Verify only device 2 is disconnected

3. **"should disconnect device 3 while devices 1 and 2 remain connected"**

   - Same pattern but disconnect device 3
   - Verify only device 3 is disconnected

4. **"should maintain device information for all devices after partial disconnection"**
   - Disconnect one device (e.g., device 2)
   - Use `verifyFullDeviceInfo(0)`, `verifyFullDeviceInfo(1)`, `verifyFullDeviceInfo(2)`
   - All devices should retain their device info regardless of connection state

**Fixtures Used**: `multipleDevices` (existing - all connected)

**Interceptors Used**:

- `interceptFindDevices({ fixture: multipleDevices })`
- `interceptDisconnectDevice()` - Works for any device ID

---

### Task 4: Sequential Connection Tests

**File**: Same file - `device-connection-multi.cy.ts`

**Objective**: Test connecting multiple devices one after another, ensuring state is maintained through multiple operations.

**Test Structure**:

```
describe('Sequential Connections', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should connect three devices sequentially"**

   - Setup: `threeDisconnectedDevices` fixture
   - Connect device 1, wait, verify connected
   - Connect device 2, wait, verify connected
   - Connect device 3, wait, verify connected
   - Final state: all 3 devices connected
   - Verify all using loop or individual assertions

2. **"should maintain connection order and state"**

   - Sequential connection (device 1 → 2 → 3)
   - After each connection, verify previous devices remain connected
   - After connecting device 2: verify device 1 still connected
   - After connecting device 3: verify devices 1 and 2 still connected

3. **"should handle mixed connect/disconnect operations"**

   - Start all disconnected
   - Connect device 1
   - Connect device 2
   - Disconnect device 1
   - Connect device 3
   - Final state: device 1 disconnected, devices 2-3 connected
   - Verify with `verifyDisconnected(0)`, `verifyConnected(1)`, `verifyConnected(2)`

4. **"should make independent API calls for each connection"**
   - Use inline interceptor to count API calls
   - Connect 3 devices sequentially
   - Verify 3 separate POST requests to connect endpoint
   - Each request should have different device ID in URL

**Fixtures Used**: `threeDisconnectedDevices`, `multipleDevices`

**Notes**:

- These tests may take longer due to multiple sequential operations
- Each operation needs proper `cy.wait()` to avoid race conditions

---

### Task 5: Mixed State Display Tests

**File**: Same file - `device-connection-multi.cy.ts`

**Objective**: Test UI correctly displays devices in various mixed connection states simultaneously.

**Test Structure**:

```
describe('Mixed Connection States', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should display mixed connection states correctly"**

   - Setup: `mixedConnectionDevices` fixture (new)
   - Device 1: connected, Device 2: disconnected, Device 3: connected
   - Navigate, wait for discovery
   - Verify device 1 connected: `verifyConnected(0)`
   - Verify device 2 disconnected: `verifyDisconnected(1)`
   - Verify device 3 connected: `verifyConnected(2)`

2. **"should show correct visual state for each device in mixed state"**

   - Same setup
   - Device 1: not dimmed, power button highlighted
   - Device 2: dimmed, power button normal
   - Device 3: not dimmed, power button highlighted

3. **"should allow operations on any device regardless of other states"**

   - Setup: mixed state fixture
   - Disconnect device 1 (currently connected)
   - Device 2 state should not change (remain disconnected)
   - Device 3 state should not change (remain connected)

4. **"should render all devices in correct order with mixed states"**
   - Verify device count is 3
   - Verify each device card is visible
   - Verify devices maintain order from fixture

**Fixtures Used**: `mixedConnectionDevices` (new)

**Interceptors Used**:

- `interceptFindDevices({ fixture: mixedConnectionDevices })`
- `interceptConnectDevice()` and `interceptDisconnectDevice()` as needed

---

### Task 6: State Isolation Error Tests

**File**: Same file - `device-connection-multi.cy.ts`

**Objective**: Ensure connection errors for one device don't affect other devices.

**Test Structure**:

```
describe('Error State Isolation', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should isolate connection error to single device"**

   - Setup: `threeDisconnectedDevices` fixture
   - Use `interceptConnectDevice({ errorMode: true })`
   - Try to connect device 1 - should fail
   - Verify device 1 remains disconnected
   - Change interceptor to success mode
   - Connect device 2 successfully
   - Verify device 2 connected, device 1 still disconnected

2. **"should allow retry on failed device while others are connected"**

   - Setup: all disconnected
   - Connect device 1 successfully
   - Try to connect device 2 with error mode - fails
   - Device 1 should remain connected
   - Retry device 2 with success mode - succeeds
   - Both devices now connected

3. **"should isolate disconnection error to single device"**

   - Setup: `multipleDevices` fixture (all connected)
   - Use `interceptDisconnectDevice({ errorMode: true })`
   - Try to disconnect device 2 - should fail
   - Verify device 2 remains connected
   - Verify devices 1 and 3 remain connected
   - Change to success mode, disconnect device 3 successfully
   - Final: devices 1-2 connected, device 3 disconnected

4. **"should not display duplicate error messages"**
   - Connection error on one device
   - Should show error once, not repeated per device
   - Error should be dismissible without affecting device states
   - **Note**: Detailed alert validation in Phase 4 (DEVICE_CONNECTION_TEST_PLAN_P4_ALERTS.md)

**Fixtures Used**: `threeDisconnectedDevices`, `multipleDevices`

**Interceptors Used**: Both interceptors with `errorMode: true` then switching to success

**Notes**:

- Need to re-register interceptors mid-test to switch from error to success mode
- Pattern: `interceptConnectDevice({ errorMode: true })` before error operation, then re-register `interceptConnectDevice()` for success

---

### Task 7: Device Count Validation Tests

**File**: Same file - `device-connection-multi.cy.ts`

**Objective**: Verify device count remains stable through connection/disconnection operations.

**Test Structure**:

```
describe('Device Count Stability', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should maintain device count through multiple connections"**

   - Setup: 3 disconnected devices
   - Verify count is 3 using `verifyDeviceCount(3)` (existing helper)
   - Connect all 3 devices sequentially
   - Verify count is still 3 after all operations

2. **"should maintain device count through mixed operations"**

   - Setup: 3 devices (any state)
   - Verify initial count
   - Perform various connect/disconnect operations
   - Verify count remains constant - no devices added or removed

3. **"should render exactly 3 device cards throughout test"**
   - At various points during test, verify:
   - `cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 3)`
   - Cards don't disappear or duplicate during state changes

**Fixtures Used**: `threeDisconnectedDevices`, `multipleDevices`, `mixedConnectionDevices`

**Notes**:

- These tests ensure connection state changes don't inadvertently affect device list rendering
- Validates no duplicate cards or missing cards during state transitions

---

### Task 8: Multi-Device Test Documentation

**File**: Header comments in `device-connection-multi.cy.ts`

**Objective**: Document multi-device test patterns for Phase 3 and future maintenance.

**Actions**:

1. Add comprehensive header comment explaining multi-device testing approach
2. Document fixtures used and when to use each
3. Document interceptor patterns for multi-device scenarios
4. Note differences from single-device tests (Phase 1)
5. Add usage examples for common patterns

**Example Header**:

```typescript
/**
 * Multi-Device Connection E2E Tests (Phase 2)
 *
 * Tests connection/disconnection workflows with multiple TeensyROM devices.
 * Validates state isolation - connection changes to one device don't affect others.
 *
 * Fixtures:
 * - threeDisconnectedDevices: All 3 devices disconnected (sequential connection tests)
 * - multipleDevices: All 3 devices connected (disconnection tests)
 * - mixedConnectionDevices: Mixed states (device 1 & 3 connected, device 2 disconnected)
 *
 * Key Patterns:
 * - Independent operations: Change one device, verify others unaffected
 * - Sequential operations: Multiple operations in sequence, state maintained
 * - Error isolation: Errors on one device don't affect others
 */
```

---

## 🎯 Success Criteria

**Task Completion**:

- [ ] Task 0: Infrastructure verification complete
- [ ] 2 new fixtures added: `threeDisconnectedDevices`, `mixedConnectionDevices`
- [ ] Vitest tests added for new fixtures in `devices.fixture.spec.ts`
- [ ] New test file `device-connection-multi.cy.ts` created
- [ ] ~18-22 test cases implemented covering:
  - [ ] Independent connection (4 tests)
  - [ ] Independent disconnection (4 tests)
  - [ ] Sequential connections (4 tests)
  - [ ] Mixed state display (4 tests)
  - [ ] Error state isolation (4 tests)
  - [ ] Device count validation (3 tests)
- [ ] Test documentation complete
- [ ] All tests passing consistently (100% pass rate over 3 runs)
- [ ] Phase 1 tests still passing (regression check)

**Test Quality**:

- Tests reuse Phase 1 helpers (`clickPowerButton`, `verifyConnected`, etc.)
- Tests verify state isolation between devices
- Tests handle multiple devices cleanly (loops or individual assertions as appropriate)
- Error scenarios isolated to single devices
- Device count remains stable throughout operations

**Code Quality**:

- New fixtures follow existing patterns
- Proper faker seeding for reproducibility
- Vitest tests validate fixture structure and determinism
- Comprehensive JSDoc documentation
- Consistent with Phase 1 test style

---

## 📝 Implementation Notes

### Key Files Modified

1. `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts` - Add 2 new fixtures
2. `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection-multi.cy.ts` - NEW file

### Key Files Reused (No Changes)

1. `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts` - Use all Phase 1 helpers
2. `apps/teensyrom-ui-e2e/src/support/interceptors/device.interceptors.ts` - Same interceptors
3. Existing fixtures: `multipleDevices` (already exists)

### Multi-Device Testing Pattern

```typescript
// Standard multi-device pattern
beforeEach(() => {
  interceptFindDevices({ fixture: threeDisconnectedDevices });
  interceptConnectDevice();
  navigateToDeviceView();
  waitForDeviceDiscovery();
});

it('independent connection', () => {
  // Verify initial state - all disconnected
  verifyDisconnected(0);
  verifyDisconnected(1);
  verifyDisconnected(2);

  // Connect only device 1
  clickPowerButton(0);
  waitForConnection();

  // Verify isolation - only device 1 affected
  verifyConnected(0);
  verifyDisconnected(1); // Still disconnected
  verifyDisconnected(2); // Still disconnected
});
```

### Interceptor Behavior Notes

- `interceptConnectDevice()` uses wildcard route: `/devices/*/connect` - works for any device ID
- `interceptDisconnectDevice()` uses wildcard route: `/devices/*` - works for any device ID
- No need for device-specific interceptors - single interceptor handles all devices

### Error Mode Switching Pattern

```typescript
it('test with error then success', () => {
  // Initial error mode
  interceptConnectDevice({ errorMode: true });
  clickPowerButton(0); // Fails

  // Switch to success mode mid-test
  interceptConnectDevice(); // Re-register without error mode
  clickPowerButton(1); // Succeeds
});
```

---

## ❓ Questions for Phase 2

**Resolved**:

- ✅ Interceptor handling: Wildcards handle all device IDs - no special configuration needed
- ✅ Fixture approach: Create specialized fixtures for test scenarios

**To Verify During Implementation**:

1. **Concurrent Operations**: If user clicks multiple power buttons rapidly, are operations queued or do they run concurrently?
2. **API Call Tracking**: How to verify each device makes separate API calls (inline interceptor with request logging?)
3. **Error Message Behavior**: Do error messages disappear when other devices succeed, or persist?

**For Phase 3 Consideration**:

- How does refresh affect mixed connection states?
- Should refresh attempt reconnection for previously connected devices?

---

## 🔗 Integration with Phase 1

**Dependencies**:

- All Phase 1 test helpers are reused
- Test patterns established in Phase 1 are followed
- Same interceptors, just applied to multiple devices

**Regression Testing**:

- After Phase 2 implementation, run Phase 1 tests to ensure no regressions
- Single device tests should still pass

**Shared Infrastructure**:

- `test-helpers.ts` - No changes needed, just reuse
- `device.interceptors.ts` - No changes needed
- Selectors and constants - All reused

---

## 🚀 Ready to Implement

This phase builds directly on Phase 1's foundation. All helpers and patterns are established - Phase 2 focuses on applying them to multi-device scenarios and validating state isolation.

**Estimated Test Count**: 18-22 test scenarios
**Estimated Implementation Time**: 4-6 hours for experienced developer
**Priority**: High - Validates multi-device support
**Dependencies**: Phase 1 must be complete and passing

**Note**: Alert message validation for multi-device scenarios is in Phase 4 (see DEVICE_CONNECTION_TEST_PLAN_P4_ALERTS.md).
