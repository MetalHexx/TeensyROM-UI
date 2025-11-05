# Device Connection E2E Testing Plan (Phase 5)

## 🎯 Project Objective

Create comprehensive E2E tests for device connection and disconnection workflows, validating that users can successfully connect to and disconnect from TeensyROM devices through the UI. This phase builds on Phase 4 (Device Discovery) by testing the interactive connection behaviors triggered by user actions (clicking power buttons).

**User Value**: Users must be able to connect to discovered devices to use device features (file browsing, launching, storage management). Connection/disconnection is the gateway interaction for all device-specific workflows.

**Testing Value**: Validates the complete connection lifecycle - initial state, API interaction, state updates, visual feedback, error handling, and reconnection flows. Tests ensure the UI correctly reflects connection state changes and provides appropriate user feedback throughout the process.

**Core Testing Focus**:

- **Connection Initiation**: Clicking power button triggers connection API call
- **State Synchronization**: UI updates correctly after successful connection
- **Visual Feedback**: Connection status indicators reflect current state
- **Error Recovery**: Failed connections display errors and allow retry
- **Disconnection Flow**: Disconnecting clears connection state appropriately
- **Multi-Device**: Connection state isolated per device

---

## 📋 Implementation Phases

<details open>
<summary><h3>Phase 5.1: Single Device Connection Workflows</h3></summary>

### Objective

Test basic connection and disconnection workflows for a single TeensyROM device, validating the complete lifecycle from discovered → connected → disconnected states with proper UI feedback and state synchronization.

**Value Delivered**: Core connection/disconnection functionality validated with comprehensive coverage of happy paths, error scenarios, and state transitions. Establishes testing patterns for connection workflows that Phase 5.2 and 5.3 will build upon.

### Key Deliverables

- [ ] Test suite for connecting to a discovered device
- [ ] Test suite for disconnecting from a connected device
- [ ] Test suite for connection error handling
- [ ] Test suite for disconnection error handling
- [ ] Updated test helpers for connection interactions
- [ ] Documentation for connection test patterns

### High-Level Tasks

1. **Extend Test Helpers**: Add connection-specific helpers (`clickPowerButton`, `verifyConnected`, `verifyDisconnected`, `waitForConnection`, `waitForDisconnection`)
2. **Connection Success Tests**: Test clicking power button on disconnected device triggers connection API and updates UI state
3. **Disconnection Success Tests**: Test clicking power button on connected device triggers disconnection API and updates UI state
4. **Visual Feedback Tests**: Validate power button color, device card styling, and connection status labels update correctly
5. **Connection Error Tests**: Test failed connection API calls display errors and maintain disconnected state
6. **Disconnection Error Tests**: Test failed disconnection API calls display errors and maintain connected state
7. **State Persistence Tests**: Verify connection state persists across API calls (device remains connected while performing other operations)

### User Scenarios Tested

**Connection Scenarios**:

```gherkin
Given a disconnected device is displayed
When user clicks the power button
Then connection API is called
And device shows connected status
And power button shows connected state (highlighted)
And device card is not dimmed
```

**Disconnection Scenarios**:

```gherkin
Given a connected device is displayed
When user clicks the power button
Then disconnection API is called
And device shows disconnected status
And power button shows disconnected state (normal color)
And device card is dimmed
```

**Error Recovery Scenarios**:

```gherkin
Given a disconnected device is displayed
When user clicks the power button
And connection API fails
Then error message is displayed
And device remains disconnected
And user can retry connection
```

### Open Questions for Phase 5.1

- **Loading State During Connection**: Should power button show loading spinner during API call? Or entire device card?
- **Error Message Placement**: Display inline on device card, snackbar notification, or modal dialog?
- **Retry Mechanism**: Automatic retry after failure, or require explicit user action?
- **Connection Timeout**: What timeout value for connection attempts? How to handle timeouts?

</details>

---

<details open>
<summary><h3>Phase 5.2: Multi-Device Connection Workflows</h3></summary>

### Objective

Test connection and disconnection workflows with multiple devices simultaneously, validating that connection state is properly isolated per device and that users can independently manage connections for multiple TeensyROM devices.

**Value Delivered**: Validates multi-device scenarios where users have multiple TeensyROM devices connected to their system. Tests ensure connection state changes for one device don't affect other devices, and that UI correctly displays mixed connection states.

### Key Deliverables

- [ ] Test suite for connecting one device while others remain disconnected
- [ ] Test suite for disconnecting one device while others remain connected
- [ ] Test suite for sequential connections (connect device 1, then device 2, then device 3)
- [ ] Test suite for simultaneous connection attempts (if supported)
- [ ] Test suite for mixed connection states display
- [ ] Documentation for multi-device connection patterns

### High-Level Tasks

1. **Independent Connection Tests**: Connect one device and verify other devices remain unaffected
2. **Independent Disconnection Tests**: Disconnect one device and verify other devices remain connected
3. **Sequential Connection Tests**: Connect multiple devices one after another, verifying each maintains its state
4. **Mixed State Display Tests**: Verify UI correctly displays devices in various connection states simultaneously
5. **State Isolation Tests**: Ensure connection errors for one device don't affect other devices
6. **Connection Order Tests**: Verify connection order doesn't affect device state management

### User Scenarios Tested

**Independent Connection**:

```gherkin
Given three disconnected devices are displayed
When user connects device 1
Then device 1 shows connected status
And devices 2 and 3 remain disconnected
And each device maintains independent state
```

**Sequential Connections**:

```gherkin
Given three disconnected devices are displayed
When user connects device 1
And user connects device 2
And user connects device 3
Then all three devices show connected status
And connection states are maintained correctly
```

**Mixed State Management**:

```gherkin
Given device 1 is connected
And device 2 is disconnected
And device 3 is connected
When user disconnects device 1
Then device 1 shows disconnected
And devices 2 and 3 states are unchanged
```

### Open Questions for Phase 5.2

- **Concurrent Connections**: Can users click connect on multiple devices simultaneously? How should UI handle this?
- **Connection Limits**: Is there a maximum number of simultaneously connected devices? How is this enforced?
- **State Synchronization**: If backend detects connection loss for one device, how does UI reflect this without affecting other devices?

</details>

---

<details open>
<summary><h3>Phase 5.3: Connection State Refresh and Recovery</h3></summary>

### Objective

Test device refresh workflows that rediscover devices and validate connection state persistence and recovery scenarios. This includes using the "Refresh Devices" toolbar button, handling connection loss scenarios, and validating reconnection workflows.

**Value Delivered**: Validates device state refresh mechanisms work correctly, connection states persist appropriately across refresh operations, and users have clear paths to recover from connection issues through refresh and reconnect actions.

### Key Deliverables

- [ ] Test suite for refresh devices button maintaining connection states
- [ ] Test suite for reconnecting to previously disconnected devices
- [ ] Test suite for connection loss detection and recovery
- [ ] Test suite for refresh during active connections
- [ ] Test suite for refresh error handling
- [ ] Documentation for refresh and recovery workflows

### High-Level Tasks

1. **Refresh Connected Device Tests**: Click "Refresh Devices" button while device is connected, verify connection state maintained after refresh
2. **Refresh Disconnected Device Tests**: Refresh devices and verify disconnected devices remain disconnected
3. **Reconnection Tests**: Connect to a device that was previously disconnected after refresh
4. **Connection Loss Recovery Tests**: Simulate connection loss, refresh devices, and reconnect
5. **Refresh During Connection Tests**: Click refresh while connection is in progress, verify proper handling
6. **Partial Refresh Tests**: Verify refresh updates device list while preserving connection states where appropriate

### User Scenarios Tested

**Refresh Maintains State**:

```gherkin
Given device 1 is connected
And device 2 is disconnected
When user clicks "Refresh Devices" button
Then device discovery runs
And device 1 remains connected after refresh
And device 2 remains disconnected after refresh
```

**Reconnection After Refresh**:

```gherkin
Given a device was previously disconnected
When user clicks "Refresh Devices"
And device is rediscovered
When user clicks power button on device
Then device connects successfully
And connection state is maintained
```

**Connection Loss Recovery**:

```gherkin
Given a connected device loses connection (ConnectionLost state)
When user clicks power button to reconnect
Then reconnection API is called
And device returns to connected state
And visual indicators update correctly
```

### Open Questions for Phase 5.3

- **Refresh Behavior**: Should refresh automatically reconnect previously connected devices? Or require manual reconnection?
- **Connection Loss Detection**: How is connection loss detected and communicated to UI?
- **State Persistence**: Should connection preferences persist across app sessions (local storage)?
- **Auto-Reconnect**: Should the app attempt automatic reconnection after connection loss?

</details>

---

## 🏗️ Architecture Overview

### Key Design Decisions

- **Build on Discovery Tests**: Phase 5 extends Phase 4 (Device Discovery) test infrastructure - same fixtures, interceptors, and helpers with additions for connection workflows
- **Interceptor Composition**: Use `interceptFindDevices()` + `interceptConnectDevice()` + `interceptDisconnectDevice()` together to test complete workflows
- **State-Driven Testing**: Tests focus on state transitions (disconnected → connecting → connected → disconnecting → disconnected) and visual representation
- **Fixture Reuse**: Leverage existing `disconnectedDevice` and `singleDevice` fixtures for connection state testing
- **Error Mode Testing**: Use `errorMode: true` in connection/disconnection interceptors to test failure scenarios
- **Incremental Complexity**: Start simple (single device) then add multi-device complexity (Phase 5.2) then refresh complexity (Phase 5.3)

### Integration Points

- **Phase 4 Foundation**: Uses test helpers, fixtures, and base interceptors from Device Discovery tests
- **Device Store**: Connection tests validate `connectDevice()` and `disconnectDevice()` store methods work correctly
- **API Client**: Tests validate proper API client usage through intercepted HTTP calls
- **Device Item Component**: Power button interactions trigger connection state changes
- **Device View Component**: Connection state updates reflected in parent component device list
- **Device Toolbar**: Refresh Devices button (Phase 5.3) tests integration with discovery and connection state

### Testing Infrastructure Extensions

**New Test Helpers Needed** (`test-helpers.ts`):

- `clickPowerButton(deviceIndex: number)` - Click power button on device card
- `verifyConnected(deviceIndex: number)` - Assert device shows connected state
- `verifyDisconnected(deviceIndex: number)` - Assert device shows disconnected state
- `verifyConnectionError(deviceIndex: number)` - Assert connection error displayed
- `waitForConnection()` - Wait for connect API alias
- `waitForDisconnection()` - Wait for disconnect API alias
- `clickRefreshDevices()` - Click refresh button in toolbar (already exists, may need updates)

**Interceptor Usage Patterns**:

```typescript
// Connection success workflow
interceptFindDevices({ fixture: disconnectedDevice });
interceptConnectDevice(); // Default success
navigateToDeviceView();
clickPowerButton(0);
waitForConnection();
verifyConnected(0);

// Connection error workflow
interceptFindDevices({ fixture: disconnectedDevice });
interceptConnectDevice({ errorMode: true });
navigateToDeviceView();
clickPowerButton(0);
waitForConnection();
verifyConnectionError(0);
verifyDisconnected(0);
```

---

## 🧪 Testing Strategy

### Unit Tests

- [ ] N/A - This phase focuses exclusively on E2E tests

### Integration Tests

- [ ] N/A - Connection workflows tested end-to-end through Cypress

### E2E Tests

- [ ] **Phase 5.1**: Single device connection/disconnection (8-12 test scenarios)
- [ ] **Phase 5.2**: Multi-device connection workflows (6-10 test scenarios)
- [ ] **Phase 5.3**: Refresh and recovery workflows (8-12 test scenarios)
- [ ] Connection success happy paths
- [ ] Disconnection success happy paths
- [ ] Connection error scenarios
- [ ] Disconnection error scenarios
- [ ] State transition validation
- [ ] Visual feedback validation
- [ ] Multi-device state isolation
- [ ] Refresh state persistence
- [ ] Reconnection workflows
- [ ] **Total Estimated**: 22-34 test scenarios across all phases

---

## ✅ Success Criteria

### Phase 5.1 Success Criteria

- [ ] Single device connection workflow tests pass consistently
- [ ] Single device disconnection workflow tests pass consistently
- [ ] Connection error scenarios handled correctly
- [ ] Disconnection error scenarios handled correctly
- [ ] Visual feedback (power button, card styling, status labels) validates correctly
- [ ] Test helpers for connection interactions created and documented
- [ ] Connection test patterns documented for reuse

### Phase 5.2 Success Criteria

- [ ] Multi-device connection tests pass consistently
- [ ] Independent connection/disconnection validated
- [ ] Sequential connection workflows work correctly
- [ ] Mixed connection state display validated
- [ ] State isolation between devices verified
- [ ] Multi-device connection patterns documented

### Phase 5.3 Success Criteria

- [ ] Refresh devices maintains connection states correctly
- [ ] Reconnection workflows validated
- [ ] Connection loss recovery tested
- [ ] Refresh error handling works correctly
- [ ] Refresh during connection handled properly
- [ ] All Phase 5 tests pass consistently (100% pass rate over 5 runs)
- [ ] Test execution time reasonable (< 3 minutes for full Phase 5 suite)
- [ ] All connection workflows documented
- [ ] Ready for Phase 6 (Device Storage/Player workflows)

### Overall Testing Success Criteria

- [ ] 22-34 E2E test scenarios implemented and passing
- [ ] No flaky tests - consistent pass rate
- [ ] Tests follow Cypress best practices
- [ ] Connection workflows comprehensively covered
- [ ] Error scenarios validated
- [ ] Documentation complete and accurate

---

## 🎭 User Scenarios

### Connection Lifecycle Scenarios

<details open>
<summary><strong>Scenario 1: Connect to Disconnected Device</strong></summary>

```gherkin
Given a TeensyROM device is discovered and disconnected
When user clicks the power button on the device card
Then connection API is called with device ID
And device card shows connecting/loading state
And after API success, device shows connected status
And power button changes to highlighted color
And device card styling removes dimmed appearance
```

</details>

<details open>
<summary><strong>Scenario 2: Disconnect from Connected Device</strong></summary>

```gherkin
Given a TeensyROM device is connected
When user clicks the power button on the device card
Then disconnection API is called with device ID
And device card shows disconnecting/loading state
And after API success, device shows disconnected status
And power button changes to normal color
And device card styling adds dimmed appearance
```

</details>

<details open>
<summary><strong>Scenario 3: Connection Fails with API Error</strong></summary>

```gherkin
Given a disconnected device is displayed
When user clicks the power button
And connection API returns error (500, network failure, etc.)
Then error message is displayed to user
And device remains in disconnected state
And power button remains in disconnected appearance
And user can attempt to reconnect (retry)
```

</details>

---

### Multi-Device Connection Scenarios

<details open>
<summary><strong>Scenario 4: Connect One Device, Others Unaffected</strong></summary>

```gherkin
Given three disconnected devices are displayed
When user connects device 1
Then device 1 transitions to connected state
And devices 2 and 3 remain disconnected
And each device maintains independent connection state
And visual indicators reflect each device's state correctly
```

</details>

<details open>
<summary><strong>Scenario 5: Disconnect One Device, Others Remain Connected</strong></summary>

```gherkin
Given three connected devices are displayed
When user disconnects device 2
Then device 2 transitions to disconnected state
And devices 1 and 3 remain connected
And connection states are isolated per device
```

</details>

<details open>
<summary><strong>Scenario 6: Sequential Connections</strong></summary>

```gherkin
Given three disconnected devices are displayed
When user connects device 1
And waits for connection to complete
And connects device 2
And waits for connection to complete
And connects device 3
Then all three devices show connected status
And each connection was independent
And all connection states are maintained
```

</details>

---

### Refresh and Recovery Scenarios

<details open>
<summary><strong>Scenario 7: Refresh Maintains Connection States</strong></summary>

```gherkin
Given device 1 is connected
And device 2 is disconnected
When user clicks "Refresh Devices" button
Then device discovery API is called
And after refresh completes, device 1 remains connected
And device 2 remains disconnected
And device list is updated with fresh data
```

</details>

<details open>
<summary><strong>Scenario 8: Reconnect After Connection Loss</strong></summary>

```gherkin
Given a device previously lost connection (ConnectionLost state)
And device is still displayed as disconnected/dimmed
When user clicks the power button to reconnect
Then connection API is called
And device transitions back to connected state
And connection status indicators update
And device card is no longer dimmed
```

</details>

<details open>
<summary><strong>Scenario 9: Refresh During Active Connection</strong></summary>

```gherkin
Given a device is in the process of connecting
When user clicks "Refresh Devices" before connection completes
Then in-progress connection is handled appropriately
And refresh completes successfully
And device state reflects actual connection status after both operations
```

</details>

---

### Error Handling and Edge Cases

<details open>
<summary><strong>Scenario 10: Disconnection Fails with API Error</strong></summary>

```gherkin
Given a connected device is displayed
When user clicks power button to disconnect
And disconnection API returns error
Then error message is displayed
And device remains in connected state
And user can retry disconnection
```

</details>

<details open>
<summary><strong>Scenario 11: Connection Timeout</strong></summary>

```gherkin
Given a disconnected device is displayed
When user clicks power button
And connection API takes longer than timeout threshold
Then timeout error is displayed
And device returns to disconnected state
And user can retry connection
```

</details>

<details open>
<summary><strong>Scenario 12: Rapid Connect/Disconnect Clicks</strong></summary>

```gherkin
Given a disconnected device is displayed
When user rapidly clicks power button multiple times
Then only one connection attempt is made (debouncing)
Or connection state changes are queued and processed sequentially
And UI reflects correct final state after operations complete
```

</details>

---

## 📚 Related Documentation

- **Phase 4 (Device Discovery)**: `E2E_PLAN_P4.md` - Foundation for connection tests
- **Phase 3 (Interceptors)**: `E2E_PLAN_P3.md` - Connection/disconnection interceptors
- **Phase 2 (Fixtures)**: `E2E_PLAN_P2.md` - Device fixtures for connection testing
- **E2E Overview**: `E2E_TESTS.md` - Complete test suite documentation
- **Testing Standards**: `../../TESTING_STANDARDS.md` - General testing patterns
- **Cypress Best Practices**: https://docs.cypress.io/guides/references/best-practices

---

## 📝 Notes

### Design Considerations

- **Loading States**: Connection/disconnection operations may be fast - consider adding interceptor delays to validate loading state UI
- **Error Display**: Coordinate with design on error message placement (inline, snackbar, modal) for connection failures
- **State Transitions**: Some transitions may require intermediate states (connecting, disconnecting) - verify component supports these
- **Button Debouncing**: Rapid clicking should be handled gracefully - one connection attempt per click with proper debouncing

### Future Enhancement Ideas

- **Connection Preferences**: Persist connection preferences across sessions (auto-connect on app launch)
- **Connection Health**: Periodic ping to validate connection is still active
- **Connection Notifications**: Toast/snackbar notifications for successful connections
- **Bulk Actions**: "Connect All" or "Disconnect All" buttons for multi-device scenarios
- **Connection History**: Track connection/disconnection events with timestamps for debugging

### Summary of Open Questions

**Phase 5.1:**

- Loading state UI during connection (power button or entire card?)
- Error message placement strategy
- Retry mechanism design (auto vs manual)
- Connection timeout values

**Phase 5.2:**

- Concurrent connection support and handling
- Maximum simultaneous connection limits
- Backend-initiated state synchronization handling

**Phase 5.3:**

- Refresh behavior for connected devices (maintain vs reconnect)
- Connection loss detection mechanism
- Connection state persistence across sessions
- Automatic reconnection after connection loss

---

## 💡 Implementation Tips

**Before Starting Phase 5.1:**

1. Verify Phase 4 (Device Discovery) tests all pass
2. Review existing connection interceptors (`interceptConnectDevice`, `interceptDisconnectDevice`)
3. Understand device card power button implementation
4. Identify what `data-testid` attributes exist for power button and connection status

**Test Development Order:**

1. **Phase 5.1**: Master single-device workflows before multi-device complexity
2. **Phase 5.2**: Build on Phase 5.1 patterns with multi-device scenarios
3. **Phase 5.3**: Add refresh complexity last as it integrates both discovery and connection

**Key Testing Patterns:**

- Always register interceptors before navigation (`beforeEach`)
- Use `cy.wait('@connectDevice')` to wait for connection API completion
- Verify visual state changes (button color, card styling, status labels)
- Test error scenarios alongside happy paths
- Keep tests independent - each test sets up its own initial state

**Debugging Tips:**

- Use Cypress Test Runner (headed mode) to see visual state changes
- Add `cy.pause()` to inspect state during test execution
- Check DevTools Network tab for API call patterns
- Use `data-testid` selectors for reliable element targeting

---

**Ready to implement?** Start with Phase 5.1 to establish connection testing patterns, then progress to multi-device (5.2) and refresh workflows (5.3). Each phase builds on the previous, creating comprehensive connection coverage. 🚀
