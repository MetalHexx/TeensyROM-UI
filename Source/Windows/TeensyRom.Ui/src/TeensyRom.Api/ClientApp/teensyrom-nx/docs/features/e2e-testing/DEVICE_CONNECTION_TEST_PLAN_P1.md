# Device Connection Test Plan - Phase 1: Single Device Connection Workflows

**Phase Goal**: Establish core connection/disconnection testing patterns for a single TeensyROM device. This phase validates the complete connection lifecycle and creates reusable test helpers for subsequent phases.

**Prerequisites**:

- Phase 4 (Device Discovery) tests passing
- Existing test infrastructure: `test-helpers.ts`, `device.interceptors.ts`, `devices.fixture.ts`
- Power button UI element exists with `data-testid="device-power-button"`

---

### Task 0: Verify and Test Existing Infrastructure

**Files**:

- `apps/teensyrom-ui-e2e/src/support/interceptors/device.interceptors.ts`
- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts`
- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.spec.ts`

**Objective**: Verify that `interceptConnectDevice()` and `interceptDisconnectDevice()` interceptors exist and add fixture validation tests if needed.

**Actions**:

1. **Verify Interceptors Exist**:

   - ✅ Confirmed: `interceptConnectDevice()` exists with `errorMode` support
   - ✅ Confirmed: `interceptDisconnectDevice()` exists with `errorMode` support
   - ✅ Both use wildcard routes (`/devices/*/connect`, `/devices/*`)
   - No changes needed - interceptors ready to use

2. **Verify Fixtures Exist**:

   - ✅ Confirmed: `disconnectedDevice` fixture exists
   - ✅ Confirmed: `singleDevice` fixture exists (for disconnection tests)
   - No additional fixtures needed for Phase 1

3. **Verify Fixture Tests Exist**:
   - ✅ Confirmed: `devices.fixture.spec.ts` exists with comprehensive Vitest tests
   - ✅ Tests validate `disconnectedDevice` fixture structure
   - ✅ Tests validate determinism and type safety
   - No additional tests needed for Phase 1 fixtures

**Result**: All infrastructure exists and is tested. Proceed to Task 1.

---

### Task 1: Extend Test Helpers for Connection Workflows

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts`

**Objective**: Add connection-specific helper functions to centralize connection workflow interactions.

**Actions**:

1. Add `clickPowerButton(deviceIndex: number)` helper
   - Uses existing `getDeviceCard()` helper
   - Finds power button by `data-testid="device-power-button"`
   - Clicks the button
2. Add `waitForConnection()` helper
   - Uses `cy.wait('@connectDevice')` with alias from interceptor
   - Default timeout can leverage existing `CONSTANTS.DEFAULT_TIMEOUT`
3. Add `waitForDisconnection()` helper
   - Uses `cy.wait('@disconnectDevice')` with alias from interceptor
4. Add `verifyConnected(deviceIndex: number)` helper
   - Checks device card does NOT have `dimmed` class (uses existing `CSS_CLASSES.DIMMED`)
   - Checks power button has `color="highlight"` attribute/styling
   - Gets device card, checks power button color state
5. Add `verifyDisconnected(deviceIndex: number)` helper
   - Checks device card HAS `dimmed` class
   - Checks power button has `color="normal"` attribute/styling
6. Add `verifyConnectionError(deviceIndex: number)` helper
   - Can reuse existing `verifyErrorMessage()` pattern
   - Optionally check for device-specific error indicators

**Notes**:

- Leverage existing selectors: `DEVICE_CARD_SELECTORS.powerButton`, `CSS_CLASSES.DIMMED`
- Follow existing helper patterns for consistency
- Power button color determined by `[color]` input binding in template

---

### Task 2: Create Connection Success Test Suite

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts` (NEW)

**Objective**: Test successful connection workflow - clicking power button on disconnected device connects it.

**Test Structure**:

```
describe('Device Connection - Single Device', () => {
  describe('Connection Success', () => {
    // Test cases here
  });
});
```

**Test Cases**:

1. **"should connect to disconnected device when power button clicked"**

   - Setup: `interceptFindDevices({ fixture: disconnectedDevice })`, `interceptConnectDevice()`
   - Navigate to device view, wait for discovery
   - Verify initial disconnected state with `verifyDisconnected(0)`
   - Click power button with `clickPowerButton(0)`
   - Wait for connection API with `waitForConnection()`
   - Verify connected state with `verifyConnected(0)`

2. **"should update power button color after connection"**

   - Same setup as above
   - After connection, explicitly verify power button has `color="highlight"` or highlighted styling
   - Use `getDeviceCard(0).find(DEVICE_CARD_SELECTORS.powerButton)` to check color attribute

3. **"should remove dimmed styling from device card after connection"**

   - Same setup as above
   - Verify `getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED)`

4. **"should call connection API with correct device ID"**
   - Same setup but use inline interceptor to capture request
   - Verify request URL contains device ID from `disconnectedDevice` fixture
   - Verify request method is POST

**Fixtures Used**: `disconnectedDevice` (already exists in `devices.fixture.ts`)

**Interceptors Used**:

- `interceptFindDevices({ fixture: disconnectedDevice })`
- `interceptConnectDevice()` (default success mode)

---

### Task 3: Create Disconnection Success Test Suite

**File**: Same file - `device-connection.cy.ts`

**Objective**: Test successful disconnection workflow - clicking power button on connected device disconnects it.

**Test Structure**:

```
describe('Disconnection Success', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should disconnect from connected device when power button clicked"**

   - Setup: `interceptFindDevices({ fixture: singleDevice })` (connected device)
   - Add `interceptDisconnectDevice()`
   - Navigate, wait for discovery
   - Verify initial connected state with `verifyConnected(0)`
   - Click power button with `clickPowerButton(0)`
   - Wait with `waitForDisconnection()`
   - Verify disconnected state with `verifyDisconnected(0)`

2. **"should update power button color after disconnection"**

   - Same setup
   - After disconnection, verify power button has `color="normal"`

3. **"should apply dimmed styling to device card after disconnection"**

   - Same setup
   - Verify `getDeviceCard(0).should('have.class', CSS_CLASSES.DIMMED)`

4. **"should call disconnection API with correct device ID"**
   - Use inline interceptor to capture DELETE request
   - Verify URL contains device ID
   - Verify request method is DELETE

**Fixtures Used**: `singleDevice` (already exists - connected device)

**Interceptors Used**:

- `interceptFindDevices({ fixture: singleDevice })`
- `interceptDisconnectDevice()` (default success mode)

---

### Task 4: Create Connection Error Test Suite

**File**: Same file - `device-connection.cy.ts`

**Objective**: Test connection failure scenarios - API errors are handled gracefully.

**Test Structure**:

```
describe('Connection Errors', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should handle connection API failure"**

   - Setup: `interceptFindDevices({ fixture: disconnectedDevice })`
   - Use `interceptConnectDevice({ errorMode: true })` for 500 error
   - Navigate, wait for discovery
   - Click power button
   - Wait for connection attempt
   - Device should remain disconnected - verify with `verifyDisconnected(0)`

2. **"should display error message after connection failure"**

   - Same setup as above
   - After connection failure, verify error message appears
   - **Note**: Detailed alert validation moved to Phase 4 (DEVICE_CONNECTION_TEST_PLAN_P4_ALERTS.md)
   - This test focuses on device state, alert testing happens in P4

3. **"should allow retry after connection failure"**

   - Setup with error mode initially
   - After first failure, change interceptor to success mode
   - Click power button again
   - Verify successful connection on retry

4. **"should maintain device information after connection failure"**
   - Same error setup
   - After connection failure, verify device card still displays all device info
   - Use `verifyFullDeviceInfo(0)` to check device details persist

**Fixtures Used**: `disconnectedDevice`

**Interceptors Used**:

- `interceptFindDevices({ fixture: disconnectedDevice })`
- `interceptConnectDevice({ errorMode: true })` for failures

---

### Task 5: Create Disconnection Error Test Suite

**File**: Same file - `device-connection.cy.ts`

**Objective**: Test disconnection failure scenarios - API errors are handled gracefully.

**Test Structure**:

```
describe('Disconnection Errors', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should handle disconnection API failure"**

   - Setup: `interceptFindDevices({ fixture: singleDevice })`
   - Use `interceptDisconnectDevice({ errorMode: true })`
   - Navigate, verify connected state
   - Click power button to disconnect
   - Wait for disconnection attempt
   - Device should remain connected - verify with `verifyConnected(0)`

2. **"should display error message after disconnection failure"**

   - Same setup
   - Verify error message appears after failure
   - **Note**: Detailed alert validation in Phase 4 (DEVICE_CONNECTION_TEST_PLAN_P4_ALERTS.md)

3. **"should allow retry after disconnection failure"**

   - Setup with error mode initially
   - After first failure, reset interceptor to success mode
   - Click power button again
   - Verify successful disconnection on retry

4. **"should maintain connected state after disconnection failure"**
   - Same error setup
   - After failure, verify device stays connected (not dimmed, power button highlighted)

**Fixtures Used**: `singleDevice` (connected device)

**Interceptors Used**:

- `interceptFindDevices({ fixture: singleDevice })`
- `interceptDisconnectDevice({ errorMode: true })` for failures

---

### Task 6: Create Visual Feedback Test Suite

**File**: Same file - `device-connection.cy.ts`

**Objective**: Explicitly test visual state changes during connection/disconnection.

**Test Structure**:

```
describe('Visual Feedback', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should show correct initial visual state for disconnected device"**

   - Setup: `disconnectedDevice` fixture
   - Verify dimmed class present
   - Verify power button color is normal

2. **"should show correct initial visual state for connected device"**

   - Setup: `singleDevice` fixture
   - Verify dimmed class NOT present
   - Verify power button color is highlight

3. **"should transition visual state on connection"**

   - Start disconnected, connect
   - Verify visual state changes from dimmed/normal → not dimmed/highlight

4. **"should transition visual state on disconnection"**

   - Start connected, disconnect
   - Verify visual state changes from not dimmed/highlight → dimmed/normal

5. **"should preserve device information through state changes"**
   - Connect then disconnect (or vice versa)
   - Verify device info labels persist and remain visible

**Notes**:

- These tests may overlap with previous tests but explicitly focus on visual validation
- Can check for CSS classes, color attributes, opacity, etc.

---

### Task 7: Test Documentation

**File**: Update existing test file header comments

**Objective**: Document test patterns, helpers, and conventions for Phase 2 and Phase 3.

**Actions**:

1. Add comprehensive header comment to `device-connection.cy.ts`
   - Explain test suite structure
   - Document fixtures used
   - Document interceptors used
   - Note dependencies on Phase 4 tests
2. Add JSDoc comments to new test helpers in `test-helpers.ts`

   - Explain what each helper does
   - Document parameters
   - Provide usage examples

3. Update `test-helpers.ts` header comment
   - Add new helpers to overview section

---

## 🎯 Success Criteria

**Task Completion**:

- [ ] 6 new test helper functions added to `test-helpers.ts`
- [ ] New test file `device-connection.cy.ts` created
- [ ] ~12-15 test cases implemented covering:
  - [ ] Connection success (4 tests)
  - [ ] Disconnection success (4 tests)
  - [ ] Connection errors (4 tests)
  - [ ] Disconnection errors (4 tests)
  - [ ] Visual feedback (5 tests)
- [ ] Test documentation updated
- [ ] All tests passing consistently (100% pass rate over 3 runs)

**Test Quality**:

- Tests use existing helpers and selectors for consistency
- Tests follow Cypress best practices (proper waiting, no hard-coded delays)
- Tests are independent and can run in any order
- Error scenarios validated alongside happy paths
- Visual state changes explicitly verified

**Code Quality**:

- New helpers follow existing patterns in `test-helpers.ts`
- Proper TypeScript types for all functions
- Consistent naming conventions
- Reuse of existing constants and selectors

---

## 📝 Implementation Notes

### Key Files Modified

1. `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts` - Add 6 new helpers
2. `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts` - NEW file

### Key Files Referenced (No Changes)

1. `apps/teensyrom-ui-e2e/src/support/interceptors/device.interceptors.ts` - Use `interceptConnectDevice`, `interceptDisconnectDevice`
2. `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts` - Use `singleDevice`, `disconnectedDevice`
3. `libs/features/devices/src/lib/device-view/device-item/device-item.component.html` - Power button template reference

### Testing Workflow Pattern

```typescript
// Standard connection test pattern
beforeEach(() => {
  interceptFindDevices({ fixture: disconnectedDevice });
  interceptConnectDevice(); // or with { errorMode: true }
  navigateToDeviceView();
  waitForDeviceDiscovery();
});

it('test case', () => {
  verifyDisconnected(0); // Initial state
  clickPowerButton(0);
  waitForConnection();
  verifyConnected(0); // Final state
});
```

### Component Behavior Reference

Based on `device-item.component.html`:

- Power button binding: `(buttonClick)="connectionStatus() ? onDisconnect() : onConnect()"`
- Power button color: `[color]="connectionStatus() ? 'highlight' : 'normal'"`
- Card dimming: `[ngClass]="{ dimmed: !connectionStatus() }"`

### Store Methods Tested

- `DeviceStore.connectDevice(deviceId)` - Called from `DeviceViewComponent.onConnect()`
- `DeviceStore.disconnectDevice(deviceId)` - Called from `DeviceViewComponent.onDisconnect()`

---

## ❓ Open Questions for Phase 1

**Resolved by Investigation**:

- ✅ Power button selector: `data-testid="device-power-button"` confirmed in template
- ✅ Connection state visual: Controlled by `dimmed` CSS class
- ✅ Power button color: Controlled by `[color]` input (highlight vs normal)

**To Determine During Implementation**:

1. **Loading State**: Does power button show loading state during API call? (May need to add delay to interceptor to observe)
2. **Error Message Placement**: Where do error messages appear? (Snackbar, inline, modal?)
3. **Error Message Selectors**: What `data-testid` attributes exist for error displays?

**For Phase 2 Consideration**:

- Concurrent connection handling for multiple devices
- State isolation validation between devices

---

## 🚀 Ready to Implement

This phase establishes all core connection testing patterns. Once complete, Phase 2 will build on these helpers and patterns to test multi-device scenarios, and Phase 3 will add refresh/recovery complexity.

**Estimated Test Count**: 12-15 test scenarios
**Estimated Implementation Time**: 4-6 hours for experienced developer
**Priority**: High - Foundation for all connection testing

**Note**: Alert message validation is separated into Phase 4 (see DEVICE_CONNECTION_TEST_PLAN_P4_ALERTS.md) to keep Phase 1 focused on connection state behavior.
