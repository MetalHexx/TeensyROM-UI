# Device Connection Test Plan - Phase 4: Alert and Error Message Validation

**Phase Goal**: Validate that connection/disconnection errors display proper alert messages to users. Ensure alerts appear, contain appropriate error information, are dismissible, and auto-dismiss correctly.

**Prerequisites**: 
- ✅ Phase 1 complete (single device connection error tests structure exists)
- ✅ Phase 2 complete (multi-device error isolation tests structure exists)
- ✅ Alert system implemented (`AlertService`, `AlertContainerComponent`, `AlertDisplayComponent`)
- ✅ Infrastructure services call `alertService.error()` on failures

**Alert System Overview**:
- **Service**: `AlertService` implements `IAlertService` (domain contract)
- **Display**: `AlertContainerComponent` + `AlertDisplayComponent` render alerts
- **Auto-dismiss**: 3000ms default (configurable)
- **Position**: Bottom-right by default
- **Severity Types**: Success, Error, Warning, Info

---

## 📋 Tasks Breakdown

### Task 1: Add Alert Test Helpers

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts`

**Objective**: Create reusable helpers for alert validation in E2E tests.

**New Helpers to Add**:

1. **`verifyAlertDisplayed(expectedMessage?: string)`**
   - Checks alert container is visible
   - Optionally verifies specific message text
   - Usage: `verifyAlertDisplayed('Connection failed')`
   
2. **`verifyErrorAlert(expectedMessage?: string)`**
   - Verifies alert has error severity styling
   - Optionally checks message text
   - Usage: `verifyErrorAlert('Failed to connect to device')`
   
3. **`verifyAlertDismissed()`**
   - Verifies no alerts are currently displayed
   - Usage: `verifyAlertDismissed()`
   
4. **`dismissAlert()`**
   - Clicks alert close/dismiss button
   - Usage: `dismissAlert()`
   
5. **`waitForAlertAutoDismiss(timeout = 4000)`**
   - Waits for alert to auto-dismiss
   - Default timeout slightly longer than 3000ms auto-dismiss
   - Usage: `waitForAlertAutoDismiss()`

**Selector Constants to Add**:
```typescript
export const ALERT_SELECTORS = {
  container: '[data-testid="alert-container"]',
  alert: '[data-testid="alert-message"]',
  errorAlert: '[data-testid="alert-message"][data-severity="error"]',
  dismissButton: '[data-testid="alert-dismiss-button"]',
  messageText: '[data-testid="alert-message-text"]',
} as const;
```

**Implementation Pattern**:
```typescript
export function verifyErrorAlert(expectedMessage?: string): void {
  cy.get(ALERT_SELECTORS.errorAlert).should('be.visible');
  if (expectedMessage) {
    cy.get(ALERT_SELECTORS.messageText).should('contain', expectedMessage);
  }
}
```

**Notes**:
- Alert components will need `data-testid` attributes added
- Helpers should be flexible (optional message checking)
- Follow existing helper patterns for consistency

---

### Task 2: Add `data-testid` Attributes to Alert Components

**Files**: 
- `libs/app/src/lib/alert-container.component.html`
- `libs/app/src/lib/alert-display.component.ts` (or .html if template exists)

**Objective**: Make alerts testable by adding data attributes for Cypress selection.

**Attributes to Add**:

1. **Alert Container**:
   ```html
   <div class="alert-container" data-testid="alert-container">
   ```

2. **Individual Alert**:
   ```html
   <div class="alert-message" 
        data-testid="alert-message"
        [attr.data-severity]="alert.severity">
   ```

3. **Alert Message Text**:
   ```html
   <span data-testid="alert-message-text">{{ alert.message }}</span>
   ```

4. **Dismiss Button** (if exists):
   ```html
   <button data-testid="alert-dismiss-button" (click)="dismiss()">
   ```

**Notes**:
- Use `[attr.data-severity]` for dynamic severity binding
- Allows filtering alerts by severity in tests
- Keep existing classes/styling intact

---

### Task 3: Single Device Connection Error Alert Tests

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts`

**Objective**: Extend existing Phase 1 connection error tests to validate alert messages.

**Test Suite**: Add to existing `describe('Connection Errors')` suite

**New Test Cases**:

1. **"should display error alert when connection fails"**
   - Setup: `disconnectedDevice` fixture
   - Use `interceptConnectDevice({ errorMode: true })`
   - Click power button to attempt connection
   - Wait for connection attempt
   - Verify error alert displayed: `verifyErrorAlert()`
   - Optional: verify message contains "connection" or "failed"

2. **"should allow manual dismissal of connection error alert"**
   - Same setup as above
   - After error alert appears
   - Click dismiss button: `dismissAlert()`
   - Verify alert dismissed: `verifyAlertDismissed()`

3. **"should auto-dismiss connection error alert after timeout"**
   - Same setup
   - After error alert appears
   - Wait for auto-dismiss: `waitForAlertAutoDismiss()`
   - Verify alert dismissed: `verifyAlertDismissed()`

4. **"should preserve device state after error alert dismissed"**
   - After connection error and alert dismissed
   - Verify device still disconnected: `verifyDisconnected(0)`
   - Device state unchanged by alert dismissal

**Pattern**:
```typescript
it('should display error alert when connection fails', () => {
  interceptFindDevices({ fixture: disconnectedDevice });
  interceptConnectDevice({ errorMode: true });
  navigateToDeviceView();
  waitForDeviceDiscovery();
  
  verifyDisconnected(0);
  clickPowerButton(0);
  cy.wait('@connectDevice');
  
  verifyErrorAlert(); // New alert verification
  verifyDisconnected(0); // State unchanged
});
```

---

### Task 4: Single Device Disconnection Error Alert Tests

**File**: Same file - `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts`

**Objective**: Extend existing Phase 1 disconnection error tests to validate alert messages.

**Test Suite**: Add to existing `describe('Disconnection Errors')` suite

**New Test Cases**:

1. **"should display error alert when disconnection fails"**
   - Setup: `singleDevice` fixture (connected)
   - Use `interceptDisconnectDevice({ errorMode: true })`
   - Click power button to attempt disconnection
   - Wait for disconnection attempt
   - Verify error alert displayed: `verifyErrorAlert()`

2. **"should allow manual dismissal of disconnection error alert"**
   - Same setup
   - After error alert appears, dismiss manually
   - Verify alert dismissed

3. **"should auto-dismiss disconnection error alert after timeout"**
   - Same setup
   - Wait for auto-dismiss
   - Verify alert dismissed after ~3 seconds

4. **"should preserve connected state after error alert dismissed"**
   - After disconnection error and alert dismissed
   - Verify device still connected: `verifyConnected(0)`
   - Connection state unchanged by alert dismissal

---

### Task 5: Multi-Device Error Alert Isolation Tests

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection-multi.cy.ts`

**Objective**: Extend Phase 2 multi-device error tests to validate alert behavior.

**Test Suite**: Add to existing `describe('Error State Isolation')` suite

**New Test Cases**:

1. **"should display single error alert for single device connection failure"**
   - Setup: `threeDisconnectedDevices` fixture
   - Use `interceptConnectDevice({ errorMode: true })`
   - Try to connect device 1 - should fail
   - Verify exactly ONE error alert displayed (not multiple)
   - Verify alert message references device or connection failure

2. **"should not display duplicate alerts for same error"**
   - Same setup
   - After first error alert appears
   - Verify only one alert visible (check alert count)
   - No duplicate error messages

3. **"should allow operations on other devices while error alert displayed"**
   - Connection error on device 1, alert displayed
   - Change interceptor to success mode
   - Connect device 2 successfully while alert still visible
   - Device 2 connects, error alert still shows (or auto-dismissed)
   - Both states coexist correctly

4. **"should show new alert for different device error"**
   - Connection error on device 1 → alert appears
   - Wait for first alert to dismiss
   - Connection error on device 2 → new alert appears
   - Each device error gets its own alert

**Notes**:
- Verify alerts don't stack inappropriately
- Single error = single alert
- Multiple errors can show multiple alerts (if timing allows)

---

### Task 6: Refresh Error Alert Tests

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-connection.cy.ts`

**Objective**: Extend Phase 3 refresh error tests to validate alert messages.

**Test Suite**: Add to existing `describe('Refresh Error Handling')` suite

**New Test Cases**:

1. **"should display error alert when refresh fails"**
   - Setup: `singleDevice` fixture (connected)
   - Use `interceptFindDevices({ errorMode: true })`
   - Click refresh - should fail
   - Verify error alert displayed: `verifyErrorAlert()`
   - Optional: verify message contains "refresh" or "discovery"

2. **"should auto-dismiss refresh error alert"**
   - Same setup
   - Wait for auto-dismiss: `waitForAlertAutoDismiss()`
   - Verify alert dismissed: `verifyAlertDismissed()`

3. **"should allow retry after dismissing refresh error alert"**
   - First refresh fails, alert appears
   - Dismiss alert: `dismissAlert()`
   - Change interceptor to success mode
   - Refresh again - should succeed
   - Verify device list updated, no error alert

4. **"should preserve device states after refresh error alert"**
   - Setup: `mixedConnectionDevices`
   - Note initial states
   - Refresh fails, alert appears
   - After alert dismissed or auto-dismissed
   - Verify device states unchanged (still mixed)

---

### Task 7: Alert Message Content Validation Tests

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-alerts.cy.ts` (NEW)

**Objective**: Validate alert message content is meaningful and helpful to users.

**Test Structure**:
```
describe('Device Connection - Alert Messages', () => {
  describe('Alert Message Content', () => {
    // Test cases here
  });
});
```

**Test Cases**:

1. **"should display meaningful connection error message"**
   - Connection fails
   - Verify alert message is not empty
   - Verify message contains helpful keywords (e.g., "connection", "failed", "device")
   - Message should help user understand what went wrong

2. **"should display meaningful disconnection error message"**
   - Disconnection fails
   - Verify message is descriptive
   - Message should indicate disconnection problem

3. **"should display meaningful refresh error message"**
   - Refresh fails
   - Verify message indicates discovery/refresh problem
   - Message should be distinct from connection errors

4. **"should use consistent error message format"**
   - Test multiple error scenarios
   - Verify messages follow consistent pattern
   - Professional, user-friendly language

**Notes**:
- These tests validate UX quality of error messages
- Messages should be clear, actionable when possible
- Avoid technical jargon in user-facing messages

---

### Task 8: Alert Auto-Dismiss Timing Tests

**File**: Same file - `device-alerts.cy.ts`

**Objective**: Validate alert auto-dismiss timing is correct and configurable.

**Test Structure**:
```
describe('Alert Auto-Dismiss Behavior', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should auto-dismiss error alert after default timeout"**
   - Trigger connection error
   - Start timer
   - Wait for alert to disappear
   - Verify dismissal happens around 3000ms (within reasonable tolerance)

2. **"should not dismiss alert before timeout"**
   - Trigger error
   - Wait 2000ms (less than 3000ms default)
   - Verify alert still visible
   - Then wait remaining time, verify dismissed

3. **"should cancel auto-dismiss when manually dismissed"**
   - Trigger error
   - Manually dismiss before auto-dismiss timer
   - Verify immediate dismissal
   - Verify no weird behavior after timeout would have expired

4. **"should handle multiple alerts with independent timers"**
   - If multiple errors can stack (verify behavior first)
   - Each alert should have independent auto-dismiss timer
   - First alert dismissed first, second alert persists

**Notes**:
- These tests validate timing behavior
- Use Cypress clock for precise timing control if needed
- Or use tolerance (e.g., 2900-3200ms acceptable range)

---

### Task 9: Alert Visual State Tests

**File**: Same file - `device-alerts.cy.ts`

**Objective**: Validate alert visual presentation and positioning.

**Test Structure**:
```
describe('Alert Visual Presentation', () => {
  // Test cases here
});
```

**Test Cases**:

1. **"should display error alert with error severity styling"**
   - Trigger connection error
   - Verify alert has error severity attribute: `[data-severity="error"]`
   - Verify error styling applied (red color, error icon, etc.)

2. **"should position alert at bottom-right by default"**
   - Trigger error
   - Verify alert container positioned at bottom-right
   - Check computed styles or positioning classes

3. **"should make alert visible and readable"**
   - Trigger error
   - Verify alert is not obscured by other UI elements
   - Verify text is readable (contrast, font size)
   - Verify alert has appropriate z-index

4. **"should animate alert entrance and exit"**
   - Trigger error
   - Verify alert has entrance animation class
   - Dismiss or wait for auto-dismiss
   - Verify alert has exit animation class

**Notes**:
- Focus on user experience quality
- Alert should be noticeable but not intrusive
- Professional, polished appearance

---

### Task 10: Alert Integration Documentation

**File**: Update `apps/teensyrom-ui-e2e/E2E_TESTS.md`

**Objective**: Document alert testing patterns for future reference.

**Sections to Add/Update**:

1. **Alert Testing Section**:
   - Add new section explaining alert validation
   - Document alert helpers and selectors
   - Explain alert system architecture briefly

2. **Common Testing Patterns**:
   - Add alert verification to error handling pattern
   - Show combined device state + alert verification example

**Example Addition**:
```markdown
### Alert Validation

The application uses a custom alert system to display errors, warnings, and success messages to users.

**Alert Helpers**:
- `verifyErrorAlert()` - Verify error alert is displayed
- `dismissAlert()` - Manually dismiss alert
- `waitForAlertAutoDismiss()` - Wait for auto-dismiss (3 seconds)

**Testing Pattern**:
```typescript
it('should handle error with alert', () => {
  // Trigger error condition
  interceptConnectDevice({ errorMode: true });
  clickPowerButton(0);
  
  // Verify error alert
  verifyErrorAlert('Connection failed');
  
  // Verify device state
  verifyDisconnected(0);
  
  // Verify alert dismisses
  waitForAlertAutoDismiss();
  verifyAlertDismissed();
});
```
```

---

## 🎯 Success Criteria

**Task Completion**:
- [ ] 5 new alert test helpers added to `test-helpers.ts`
- [ ] `data-testid` attributes added to alert components
- [ ] Alert tests added to existing P1 test file (8 new tests)
- [ ] Alert tests added to existing P2 test file (4 new tests)
- [ ] Alert tests added to existing P3 test file (4 new tests)
- [ ] New test file `device-alerts.cy.ts` created (12 new tests)
- [ ] ~28 total new alert-focused test cases
- [ ] Documentation updated in `E2E_TESTS.md`
- [ ] All tests passing consistently (100% pass rate)

**Test Quality**:
- Alert visibility verified in error scenarios
- Alert content validated for user-friendliness
- Auto-dismiss timing verified
- Manual dismissal tested
- Alert state doesn't interfere with device state
- Multi-device scenarios don't create duplicate alerts

**Code Quality**:
- Alert helpers follow existing helper patterns
- Selectors centralized in constants
- Tests reuse existing setup patterns
- Clear, descriptive test names

---

## 📝 Implementation Notes

### Key Files Modified
1. `apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts` - Add 5 alert helpers
2. `libs/app/src/lib/alert-container.component.html` - Add data-testid
3. `libs/app/src/lib/alert-display.component.ts` - Add data-testid
4. `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts` - Add 8 alert tests
5. `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection-multi.cy.ts` - Add 4 alert tests
6. `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-connection.cy.ts` - Add 4 alert tests
7. `apps/teensyrom-ui-e2e/src/e2e/devices/device-alerts.cy.ts` - NEW file (12 tests)
8. `apps/teensyrom-ui-e2e/E2E_TESTS.md` - Update documentation

### Alert Testing Pattern
```typescript
// Standard alert verification pattern
it('should show error alert on failure', () => {
  // Setup error condition
  interceptConnectDevice({ errorMode: true });
  
  // Trigger operation
  clickPowerButton(0);
  cy.wait('@connectDevice');
  
  // Verify alert displayed
  verifyErrorAlert();
  
  // Optionally check message content
  verifyErrorAlert('Failed to connect');
  
  // Verify device state independent of alert
  verifyDisconnected(0);
  
  // Verify alert dismisses
  waitForAlertAutoDismiss();
  verifyAlertDismissed();
});
```

### Alert Service Implementation Notes
- `AlertService` auto-dismisses after 3000ms by default
- `AlertService.error()` called by infrastructure services on HTTP errors
- Alerts positioned bottom-right by default
- Alerts managed via Observable stream (`alerts$`)
- Each alert has unique ID for dismissal tracking

### Component Template Considerations
- Need to verify actual template structure before finalizing selectors
- Alert component may use separate template file or inline template
- Dismiss button may be icon-only or text button
- May need to adjust selectors based on actual implementation

---

## ❓ Questions for Phase 4

**To Verify During Implementation**:
1. **Alert Template Structure**: Exact template structure of `AlertDisplayComponent` - is there a separate `.html` file?
2. **Dismiss Button**: Does alert have manual dismiss button? What selector?
3. **Alert Stacking**: Can multiple alerts display simultaneously? How are they positioned?
4. **Alert Animation**: Are there CSS animation classes to verify entrance/exit?
5. **Error Message Source**: What error messages come from backend vs frontend? Can we rely on specific text?

**Potential Issues**:
1. **Timing Sensitivity**: Auto-dismiss tests may be flaky - may need Cypress clock control
2. **Alert Positioning**: Bottom-right position may be viewport-dependent
3. **Multiple Alerts**: If alerts stack, need different selection strategy (first, last, by message, etc.)

---

## 🔗 Integration with Phases 1-3

**Dependencies**:
- Phase 4 extends Phases 1-3 with alert validation
- Existing error tests provide setup - just add alert verification
- No changes to existing test logic, only additions

**Test File Updates**:
- **P1 file**: Add 8 alert tests to existing error suites
- **P2 file**: Add 4 alert tests to existing error suite
- **P3 file**: Add 4 alert tests to existing error suite
- **New P4 file**: 12 dedicated alert tests

**Regression Prevention**:
- Alert tests are additive, not replacements
- Existing tests still verify device state behavior
- Alert verification enhances coverage, doesn't replace existing checks

---

## 🚀 Ready to Implement

Phase 4 completes the connection testing by validating user-facing error feedback. This ensures not only that errors are handled correctly (Phases 1-3), but that users are properly informed when errors occur.

**Estimated Test Count**: 28 new alert-focused test scenarios
**Estimated Implementation Time**: 3-4 hours for experienced developer
**Priority**: Medium-High - Validates UX quality of error handling
**Dependencies**: Phases 1-3 error tests should exist (structure in place)

**Total P1-P4 Coverage**: ~80-90 comprehensive connection test scenarios

---

## 📊 Complete Test Suite Summary (After Phase 4)

**Phase 1** (Single Device): 12-15 tests → **+8 alert tests** = 20-23 tests
- Connection/disconnection workflows
- Error handling with alert validation

**Phase 2** (Multi-Device): 18-22 tests → **+4 alert tests** = 22-26 tests
- Multi-device operations
- Error isolation with alert validation

**Phase 3** (Refresh & Recovery): 20-24 tests → **+4 alert tests** = 24-28 tests
- Refresh workflows
- State persistence with alert validation

**Phase 4** (Alert Validation): **+12 dedicated tests** = 12 tests
- Alert content, timing, visual state
- Alert system behavior validation

**Grand Total**: ~80-90 comprehensive connection and alert test scenarios

**Test Execution Time Estimate**: 5-7 minutes for full suite
**Coverage**: Complete connection lifecycle + user feedback validation
