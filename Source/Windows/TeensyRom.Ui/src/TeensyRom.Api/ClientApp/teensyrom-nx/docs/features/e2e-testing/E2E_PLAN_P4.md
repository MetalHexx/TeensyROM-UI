# Phase 4: Device Discovery E2E Test

## 🎯 Objective

Create comprehensive Cypress E2E tests for the device discovery workflow, validating that the device view correctly displays TeensyROM devices using the mock fixtures and interceptors built in Phases 1-3. This phase delivers the first complete E2E test suite that validates device management UI behaviors.

**Value Delivered**: Full E2E test coverage for device discovery scenarios (single device, multiple devices, no devices, various device states) with deterministic, reproducible test data. Tests validate UI rendering, empty states, loading states, and device information display without requiring a live backend.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [E2E Testing Plan](./E2E_PLAN.md) - High-level feature plan and context
- [ ] [Phase 2: Device Fixtures](./E2E_PLAN_P2.md) - Available fixtures to use in tests
- [ ] [Phase 3: Device Interceptors](./E2E_PLAN_P3.md) - Interceptor functions for mocking API calls

**Standards & Guidelines:**
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns

**External References:**
- [ ] [Cypress Best Practices](https://r.jina.ai/https://docs.cypress.io/app/core-concepts/best-practiceshttps://docs.cypress.io/guides/references/best-practices) - Cypress testing patterns
- [ ] [Cypress Assertions](https://r.jina.ai/https://docs.cypress.io/app/core-concepts/best-practiceshttps://docs.cypress.io/guides/references/assertions) - Available assertions

**Component Context:**
- [ ] [DeviceViewComponent](../../../libs/features/devices/src/lib/device-view/device-view.component.ts) - Component under test

---

## 📂 File Structure Overview

> Showing new files (✨) and existing files to reference

```
apps/teensyrom-ui-e2e/src/
├── e2e/
│   └── devices/                                    ✨ New directory
│       ├── README.md                               ✨ New - Test suite documentation
│       ├── device-discovery.cy.ts                  ✨ New - Device discovery test suite
│       └── test-helpers.ts                         ✨ New - Reusable test utilities
├── support/
│   ├── interceptors/                               # From Phase 3
│   │   ├── device.interceptors.ts                  # Interceptors to use
│   │   └── index.ts
│   └── test-data/                                  # From Phase 2
│       └── fixtures/
│           ├── devices.fixture.ts                  # Fixtures to use
│           └── index.ts
└── fixtures/                                       # Cypress built-in fixtures
    └── devices/                                    ✨ New directory (optional)
        └── example-response.json                   ✨ New - Optional JSON fixtures
```

---

## 📋 Implementation Guidelines

> **IMPORTANT - Test Writing Policy:**
> - Focus on **user-observable behaviors** - what users see and interact with
> - Use **data-testid attributes** for reliable selectors (to be added to components as needed)
> - Test **happy paths first**, then edge cases and error scenarios
> - Keep tests **independent** - each test should set up its own state
> - Use **descriptive test names** that explain the scenario being tested

> **IMPORTANT - Cypress Best Practices:**
> - **Don't use arbitrary waits** - use `cy.wait('@alias')` for network requests
> - **Chain commands** - Cypress commands are automatically retried until timeout
> - **Use proper assertions** - `.should()` commands retry until assertion passes
> - **Keep tests isolated** - don't depend on previous test state
> - **Use custom commands sparingly** - prefer helper functions for clarity

---

<details open>
<summary><h3>Task 1: Create Test Directory and Documentation</h3></summary>

**Purpose**: Establish test file structure and create documentation explaining the device discovery test suite, what scenarios are covered, and how to run the tests.

**Related Documentation:**
- [E2E Testing Plan](./E2E_PLAN.md) - Overall testing strategy
- [Cypress Best Practices](https://docs.cypress.io/guides/references/best-practices) - Testing patterns

**Implementation Subtasks:**
- [ ] **Create devices test directory**: `apps/teensyrom-ui-e2e/src/e2e/devices/`
- [ ] **Create README.md**: Document test suite purpose, scenarios covered, and how to run tests
- [ ] **Document test scenarios**: List all test cases planned for device discovery
- [ ] **Document data-testid conventions**: Explain selector strategy for device view elements
- [ ] **Document test execution commands**: Include commands for running individual tests vs full suite

**Testing Subtask:**
- [ ] **Write Tests**: N/A for documentation task

**Key Implementation Notes:**
- README should serve as test suite documentation for future developers
- Document the relationship between fixtures/interceptors and test scenarios
- Include troubleshooting tips for common test failures
- Explain how to add new device discovery test cases

**README Structure** (key sections to include):
```markdown
# Device Discovery E2E Tests

## Purpose
[Explain what these tests validate]

## Test Scenarios Covered
- Single device discovery
- Multiple devices discovery
- No devices (empty state)
- Mixed device states
- [etc.]

## Running Tests
[Commands for running tests]

## Test Data
[Explain fixtures and interceptors used]

## Selectors
[Document data-testid strategy]

## Troubleshooting
[Common issues and solutions]
```

</details>

---

<details open>
<summary><h3>Task 2: Create Test Helpers Utility</h3></summary>

**Purpose**: Create reusable helper functions for common device discovery test operations (navigation, waiting for API calls, verifying device cards, etc.).

**Related Documentation:**
- [Cypress Custom Commands](https://docs.cypress.io/api/cypress-api/custom-commands) - Custom command patterns

**Implementation Subtasks:**
- [ ] **Create test-helpers.ts**: Create helper file in devices directory
- [ ] **Create navigateToDeviceView helper**: Function to navigate to `/devices` route
- [ ] **Create waitForDeviceDiscovery helper**: Function to wait for device API call completion
- [ ] **Create verifyDeviceCard helper**: Function to assert device card properties
- [ ] **Create verifyEmptyState helper**: Function to assert empty state display
- [ ] **Create getDeviceCard helper**: Function to get device card element by device ID
- [ ] **Add JSDoc comments**: Document each helper's purpose and parameters

**Testing Subtask:**
- [ ] **Write Tests**: N/A for helper utilities (tested through usage in actual tests)

**Key Implementation Notes:**
- Helpers should encapsulate common test patterns for reuse
- Use typed parameters for type safety
- Return Cypress chainable objects to maintain Cypress command chain
- Keep helpers focused - one clear responsibility per function
- Export all helpers for use across device test files

</details>

---

<details open>
<summary><h3>Task 3: Single Device Discovery Test</h3></summary>

**Purpose**: Test that device view correctly displays a single TeensyROM device with all device information visible and properly formatted.

**Related Documentation:**
- [singleDevice fixture](../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts) - Default fixture to use
- [interceptFindDevices](../../apps/teensyrom-ui-e2e/src/support/interceptors/device.interceptors.ts) - API interceptor

**Implementation Subtasks:**
- [ ] **Create device-discovery.cy.ts**: Create main test file
- [ ] **Add test suite structure**: Describe block for "Device Discovery"
- [ ] **Add beforeEach hook**: Set up interceptor with singleDevice fixture
- [ ] **Test: Displays single device card**: Verify one device card renders
- [ ] **Test: Shows device name**: Assert device name from fixture is displayed
- [ ] **Test: Shows device port**: Assert COM port/connection info is visible
- [ ] **Test: Shows device version**: Assert firmware/version information is displayed
- [ ] **Test: Shows connection status**: Assert device shows as connected
- [ ] **Test: Shows storage information**: Assert SD/USB storage status visible

**Testing Subtask:**
- [ ] **Write Tests**: 6 test cases covering single device display (see Testing Focus below)

**Key Implementation Notes:**
- Use `interceptFindDevices()` with default fixture (singleDevice)
- Test should navigate to `/devices` and wait for API response
- Verify device properties match fixture data exactly
- Use data-testid selectors for reliability (may require adding to DeviceItemComponent)
- Test should be deterministic - same fixture always produces same result

**Testing Focus for Task 3:**

> Focus on **user-observable outcomes** - what appears on screen?

**Behaviors to Test:**
- [ ] **Device card renders**: Single device card is visible on page
- [ ] **Device name displayed**: Device name from fixture appears in UI
- [ ] **Device port displayed**: Connection port (COM port, USB path) visible
- [ ] **Firmware version displayed**: Version information shows correctly
- [ ] **Connection status visible**: Connected indicator/badge displays
- [ ] **Storage status visible**: SD and USB storage availability shows

**Testing Reference:**
- See [Cypress Assertions](https://docs.cypress.io/guides/references/assertions) for assertion patterns
- Use `.should('be.visible')`, `.should('contain.text', 'value')` for UI validation

</details>

---

<details open>
<summary><h3>Task 4: Multiple Devices Discovery Test</h3></summary>

**Purpose**: Test that device view correctly displays multiple TeensyROM devices simultaneously, showing all devices with unique information.

**Related Documentation:**
- [multipleDevices fixture](../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts) - Fixture with 3 devices

**Implementation Subtasks:**
- [ ] **Add test suite for multiple devices**: Describe block within device-discovery.cy.ts
- [ ] **Add beforeEach hook**: Set up interceptor with multipleDevices fixture
- [ ] **Test: Displays correct device count**: Verify 3 device cards render
- [ ] **Test: Shows unique device names**: Assert each device has distinct name
- [ ] **Test: Shows unique device ports**: Assert each device has different port
- [ ] **Test: All devices show connection status**: Verify connection indicators for all
- [ ] **Test: Device order preserved**: Assert devices display in same order as fixture

**Testing Subtask:**
- [ ] **Write Tests**: 5 test cases covering multiple device display (see Testing Focus below)

**Key Implementation Notes:**
- Use `interceptFindDevices({ fixture: multipleDevices })`
- Verify device count matches fixture (3 devices)
- Test that each device's unique properties are visible
- Ensure no devices are hidden or overlapping
- Verify devices maintain fixture order

**Testing Focus for Task 4:**

**Behaviors to Test:**
- [ ] **Correct device count**: Exactly 3 device cards render
- [ ] **Unique device names**: Each device shows different name from fixture
- [ ] **Unique device ports**: Each device shows different connection port
- [ ] **All connected**: All devices show connected status
- [ ] **Fixture order maintained**: Devices appear in same order as fixture array

**Testing Reference:**
- Use `.should('have.length', 3)` for counting elements
- Use `.each()` for iterating and asserting on multiple elements

</details>

---

<details open>
<summary><h3>Task 5: No Devices (Empty State) Test</h3></summary>

**Purpose**: Test that device view correctly displays empty state UI when no devices are discovered, with appropriate messaging.

**Related Documentation:**
- [noDevices fixture](../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts) - Empty fixture

**Implementation Subtasks:**
- [ ] **Add test suite for empty state**: Describe block within device-discovery.cy.ts
- [ ] **Add beforeEach hook**: Set up interceptor with noDevices fixture
- [ ] **Test: Shows empty state message**: Verify "No devices found" or similar message
- [ ] **Test: No device cards render**: Assert device list is empty
- [ ] **Test: Empty state styling applied**: Verify empty state container has correct classes
- [ ] **Test: No loading indicator**: Assert loading state is not shown

**Testing Subtask:**
- [ ] **Write Tests**: 4 test cases covering empty state display (see Testing Focus below)

**Key Implementation Notes:**
- Use `interceptFindDevices({ fixture: noDevices })`
- Verify empty state message is user-friendly
- Test that no device cards are rendered
- Ensure loading state transitions to empty state correctly
- Consider future "Scan for devices" button or similar action

**Testing Focus for Task 5:**

**Behaviors to Test:**
- [ ] **Empty state message shown**: "No devices found" (or equivalent) is visible
- [ ] **No device cards**: Device list container is empty
- [ ] **Empty state styling**: Empty state has appropriate visual treatment
- [ ] **Not loading**: Loading indicator is not displayed after API response

**Testing Reference:**
- Use `.should('not.exist')` to verify elements don't render
- Use `.should('be.visible')` for empty state message

</details>

---

<details open>
<summary><h3>Task 6: Disconnected Device Test</h3></summary>

**Purpose**: Test that device view correctly displays a device that has lost its connection, showing appropriate connection status indicators.

**Related Documentation:**
- [disconnectedDevice fixture](../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts) - Device with ConnectionLost state

**Implementation Subtasks:**
- [ ] **Add test suite for disconnected device**: Describe block within device-discovery.cy.ts
- [ ] **Add beforeEach hook**: Set up interceptor with disconnectedDevice fixture
- [ ] **Test: Displays disconnected device card**: Verify device card still renders
- [ ] **Test: Shows disconnected status**: Assert connection status shows "disconnected" or similar
- [ ] **Test: Device info still visible**: Verify device name/port remain visible
- [ ] **Test: Visual distinction from connected**: Assert disconnected device has different styling
- [ ] **Test: Reconnect option visible**: Verify connect/reconnect button is available

**Testing Subtask:**
- [ ] **Write Tests**: 5 test cases covering disconnected device state (see Testing Focus below)

**Key Implementation Notes:**
- Use `interceptFindDevices({ fixture: disconnectedDevice })`
- Device should still render but show disconnected state
- Test that device information (name, port) is preserved from previous connection
- Verify visual indicators differentiate connected vs disconnected
- Ensure reconnection action is available to user

**Testing Focus for Task 6:**

**Behaviors to Test:**
- [ ] **Device card renders**: Disconnected device still displays
- [ ] **Disconnected status shown**: Connection status indicates "disconnected" or "lost connection"
- [ ] **Device info preserved**: Name and port information still visible
- [ ] **Visual distinction**: Device card has different appearance than connected devices
- [ ] **Reconnect available**: Connect/reconnect button or action is present

**Testing Reference:**
- Use `.should('have.class', 'disconnected')` or similar for styling checks
- Verify button state changes (enabled vs disabled)

</details>

---

<details open>
<summary><h3>Task 7: Unavailable Storage Test</h3></summary>

**Purpose**: Test that device view correctly displays warnings or indicators when a connected device has unavailable storage.

**Related Documentation:**
- [unavailableStorageDevice fixture](../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts) - Device with no available storage

**Implementation Subtasks:**
- [ ] **Add test suite for unavailable storage**: Describe block within device-discovery.cy.ts
- [ ] **Add beforeEach hook**: Set up interceptor with unavailableStorageDevice fixture
- [ ] **Test: Displays device card**: Verify device card renders (device is connected)
- [ ] **Test: Shows connected status**: Assert device shows as connected
- [ ] **Test: Shows storage warning**: Verify warning/error indicator for storage issue
- [ ] **Test: SD storage unavailable indicator**: Assert SD storage shows as unavailable
- [ ] **Test: USB storage unavailable indicator**: Assert USB storage shows as unavailable

**Testing Subtask:**
- [ ] **Write Tests**: 5 test cases covering storage availability issues (see Testing Focus below)

**Key Implementation Notes:**
- Use `interceptFindDevices({ fixture: unavailableStorageDevice })`
- Device should show as connected but with storage issues
- Test for warning icons, messages, or badges indicating storage problems
- Verify both SD and USB storage show unavailable state
- Storage-dependent actions should be disabled or show warnings

**Testing Focus for Task 7:**

**Behaviors to Test:**
- [ ] **Device renders normally**: Device card displays as connected
- [ ] **Connection status normal**: Device shows as connected
- [ ] **Storage warning present**: Warning icon/badge indicates storage issues
- [ ] **SD unavailable shown**: SD storage status shows as unavailable
- [ ] **USB unavailable shown**: USB storage status shows as unavailable

**Testing Reference:**
- Look for warning icons, badges, or error states
- Verify storage status indicators show correct unavailable state

</details>

---

<details open>
<summary><h3>Task 8: Mixed Device States Test</h3></summary>

**Purpose**: Test that device view correctly displays multiple devices with different states simultaneously (connected, busy, disconnected).

**Related Documentation:**
- [mixedStateDevices fixture](../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts) - 3 devices in different states

**Implementation Subtasks:**
- [ ] **Add test suite for mixed states**: Describe block within device-discovery.cy.ts
- [ ] **Add beforeEach hook**: Set up interceptor with mixedStateDevices fixture
- [ ] **Test: Displays all devices**: Verify 3 device cards render
- [ ] **Test: First device connected**: Assert first device shows connected state
- [ ] **Test: Second device busy**: Assert second device shows busy/processing state
- [ ] **Test: Third device disconnected**: Assert third device shows disconnected state
- [ ] **Test: Visual distinction between states**: Verify each state has unique visual treatment

**Testing Subtask:**
- [ ] **Write Tests**: 5 test cases covering mixed device states (see Testing Focus below)

**Key Implementation Notes:**
- Use `interceptFindDevices({ fixture: mixedStateDevices })`
- Test that all three device states render correctly simultaneously
- Verify each device has appropriate visual indicators for its state
- Test that states don't interfere with each other
- Ensure device order and association with states is correct

**Testing Focus for Task 8:**

**Behaviors to Test:**
- [ ] **All devices render**: 3 device cards display
- [ ] **Connected device shown**: First device shows connected status
- [ ] **Busy device shown**: Second device shows busy/processing indicator
- [ ] **Disconnected device shown**: Third device shows disconnected status
- [ ] **States visually distinct**: Each state has unique visual appearance

**Testing Reference:**
- Use `.eq(index)` to target specific devices by position
- Verify each device has appropriate status class or indicator

</details>

---

<details open>
<summary><h3>Task 9: Loading State Test</h3></summary>

**Purpose**: Test that device view shows appropriate loading state while device discovery API call is in progress.

**Related Documentation:**
- [DeviceViewComponent](../../../libs/features/devices/src/lib/device-view/device-view.component.ts) - Component with isLoading signal

**Implementation Subtasks:**
- [ ] **Add test suite for loading state**: Describe block within device-discovery.cy.ts
- [ ] **Test: Shows loading indicator initially**: Verify loading state before API response
- [ ] **Test: Loading indicator disappears after response**: Assert loading state clears
- [ ] **Test: No devices shown while loading**: Verify device cards don't render during load
- [ ] **Use cy.intercept with delay**: Simulate slow network to test loading state visibility

**Testing Subtask:**
- [ ] **Write Tests**: 3 test cases covering loading state (see Testing Focus below)

**Key Implementation Notes:**
- Use `cy.intercept` with response delay to make loading state visible
- Test that loading indicator appears before devices
- Verify loading state transitions to content after API response
- Loading state should prevent interaction during load
- Consider skeleton loaders or spinners

**Testing Focus for Task 9:**

**Behaviors to Test:**
- [ ] **Loading indicator shown**: Loading spinner/skeleton appears during API call
- [ ] **Loading disappears on success**: Loading state clears after API response
- [ ] **No devices during load**: Device cards don't show until loading completes

**Testing Reference:**
- Use `cy.intercept` with `delay` option to test loading states
- Use `.should('exist')` then `.should('not.exist')` to verify state transitions

</details>

---

<details open>
<summary><h3>Task 10: API Error Handling Test</h3></summary>

**Purpose**: Test that device view correctly handles and displays error states when device discovery API call fails.

**Related Documentation:**
- [interceptFindDevices error mode](../../apps/teensyrom-ui-e2e/src/support/interceptors/device.interceptors.ts) - Error simulation

**Implementation Subtasks:**
- [ ] **Add test suite for error handling**: Describe block within device-discovery.cy.ts
- [ ] **Add beforeEach hook**: Set up interceptor with `errorMode: true`
- [ ] **Test: Shows error message**: Verify error message/notification displays
- [ ] **Test: No devices shown**: Assert device list remains empty on error
- [ ] **Test: Retry option available**: Verify user can retry device discovery
- [ ] **Test: Loading state clears**: Assert loading indicator disappears after error

**Testing Subtask:**
- [ ] **Write Tests**: 4 test cases covering error scenarios (see Testing Focus below)

**Key Implementation Notes:**
- Use `interceptFindDevices({ errorMode: true })` to simulate API failure
- Test that error is communicated clearly to user
- Verify system degrades gracefully - no crashes or blank screens
- Test that user has path to recovery (retry, refresh, etc.)
- Consider different error types (network, server, timeout)

**Testing Focus for Task 10:**

**Behaviors to Test:**
- [ ] **Error message displayed**: User sees clear error notification
- [ ] **No devices shown**: Device list is empty on error
- [ ] **Retry available**: User can attempt to reload devices
- [ ] **Loading state cleared**: Loading indicator disappears after error

**Testing Reference:**
- Use `interceptFindDevices({ errorMode: true })` for error simulation
- Look for error messages, snackbars, or inline error states

</details>

---

## 🗂️ Files Modified or Created

> All files are relative to workspace root: `apps/teensyrom-ui-e2e/`

**New Files:**
- `src/e2e/devices/README.md` - Device discovery test suite documentation
- `src/e2e/devices/device-discovery.cy.ts` - Main device discovery test file
- `src/e2e/devices/test-helpers.ts` - Reusable test helper functions

**Modified Files:**
- None (Phase 4 is purely additive)

**Potential Component Updates** (if data-testid attributes are missing):
- `libs/features/devices/src/lib/device-view/device-view.component.html` - Add data-testid="device-view"
- `libs/features/devices/src/lib/device-view/device-item/device-item.component.html` - Add data-testid attributes

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** This phase IS about writing tests. All tests are E2E tests validating UI behaviors.

> **Core Testing Philosophy:**
> - **User-focused**: Test what users see and interact with
> - **Deterministic**: Same fixture always produces same results
> - **Isolated**: Each test sets up its own state
> - **Descriptive**: Test names explain scenario being validated
> - **Observable**: Test visible outcomes, not internal implementation

> **Reference Documentation:**
> - **All tasks**: [Cypress Best Practices](https://docs.cypress.io/guides/references/best-practices)
> - **Assertions**: [Cypress Assertions](https://docs.cypress.io/guides/references/assertions)

### Test Execution Commands

**Running Tests:**
```bash
# Run all device discovery tests
npx nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts"

# Run all device tests
npx nx e2e teensyrom-ui-e2e --spec="**/devices/**"

# Run in headed mode (see browser)
npx nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts" --headed

# Run specific test suite
npx nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts" --grep="Single Device"
```

### Test Coverage Summary

**Device Discovery Scenarios:**
- ✅ Single device display
- ✅ Multiple devices display  
- ✅ No devices (empty state)
- ✅ Disconnected device
- ✅ Unavailable storage
- ✅ Mixed device states
- ✅ Loading states
- ✅ Error handling

**Total Expected Test Cases**: ~35-40 individual test assertions across 8 test suites

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)
- [ ] README documentation completed and accurate
- [ ] All test suites implemented:
  - [ ] Single device discovery
  - [ ] Multiple devices discovery
  - [ ] No devices (empty state)
  - [ ] Disconnected device
  - [ ] Unavailable storage
  - [ ] Mixed device states
  - [ ] Loading states
  - [ ] Error handling

**Testing Requirements:**
- [ ] All 35-40 test cases written and passing
- [ ] Tests use Phase 2 fixtures correctly
- [ ] Tests use Phase 3 interceptors correctly
- [ ] No flaky tests - all tests pass consistently
- [ ] Test execution time reasonable (< 2 minutes for full suite)

**Quality Checks:**
- [ ] No TypeScript errors or warnings
- [ ] Tests follow Cypress best practices
- [ ] Selectors are reliable (data-testid preferred)
- [ ] Test names are descriptive and clear
- [ ] No arbitrary waits or timeouts

**Documentation:**
- [ ] README includes all test scenarios
- [ ] Test helpers are documented with JSDoc
- [ ] Running instructions are clear
- [ ] Troubleshooting section is complete

**Component Updates:**
- [ ] data-testid attributes added to DeviceViewComponent (if needed)
- [ ] data-testid attributes added to DeviceItemComponent (if needed)
- [ ] Empty state has testable selectors
- [ ] Loading state has testable selectors

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Tests demonstrate device discovery works correctly
- [ ] Ready to proceed to Phase 5 (Device Connection Test)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Task-based test organization**: Tests grouped by scenario (single device, multiple devices, etc.) for clarity
- **Helper functions over custom commands**: Easier to understand and maintain
- **data-testid selectors**: More reliable than CSS selectors, less brittle than text content
- **Fixture-driven tests**: Each test uses appropriate fixture for its scenario
- **Independent tests**: Each test sets up its own interceptors and state

### Implementation Constraints

- **Component selectors**: May need to add data-testid attributes to existing components
- **Timing considerations**: Loading states may be fast - use delayed intercepts to test them
- **Cypress limitations**: Cannot test multiple browser tabs or windows
- **UI dependencies**: Tests depend on DeviceViewComponent and child components being implemented

### Test Scenarios Not Covered (Future Phases)

**Phase 5 will cover**:
- Device connection interactions (clicking connect button)
- Device disconnection interactions
- Connection state changes
- Ping device workflows

**Future phases**:
- Device toolbar interactions
- Device logs display
- Storage browsing
- Player launch

### Selector Strategy

**Preferred selector priority:**
1. `data-testid` attributes (most reliable)
2. Semantic HTML elements (`<button>`, `<nav>`, etc.)
3. ARIA attributes (`role`, `aria-label`)
4. CSS classes (least preferred - subject to styling changes)

### External References

- [Cypress Best Practices](https://docs.cypress.io/guides/references/best-practices)
- [Cypress Retry-ability](https://docs.cypress.io/guides/core-concepts/retry-ability)
- [Testing Library Principles](https://testing-library.com/docs/guiding-principles/) - User-centric testing approach

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery 1**: [Add findings about component structure]
- **Discovery 2**: [Add findings about timing/loading states]
- **Discovery 3**: [Add findings about selector strategies]

</details>

---

## 💡 Agent Implementation Guide

> **Instructions for AI agents implementing this phase**

### Before Starting Implementation

**Verify Prerequisites:**

1. **Phase 2 complete**: Fixtures exist and are tested
   - Run `npx nx test teensyrom-ui-e2e --testFile=**/devices.fixture.spec.ts`
   - All tests should pass

2. **Phase 3 complete**: Interceptors exist and are tested
   - Run `npx nx test teensyrom-ui-e2e --testFile=**/device.interceptors.spec.ts`
   - All tests should pass

3. **Component exists**: DeviceViewComponent is implemented
   - Navigate to `libs/features/devices/src/lib/device-view/`
   - Verify component files exist

4. **App runs**: Application can be served
   - Run `pnpm start` (from teensyrom-nx directory)
   - Navigate to `http://localhost:4200/devices`
   - Verify page loads (even if no data shows)

**Review Component Structure:**

1. Examine `device-view.component.html` - understand UI structure
2. Identify what data-testid attributes are needed
3. Understand loading/error state rendering
4. Identify child components (DeviceItemComponent, etc.)

### During Implementation

**Task Execution Order:**

1. ✅ **Task 1**: Create directory and README (establishes context)
2. ✅ **Task 2**: Create test helpers (reusable utilities)
3. ✅ **Task 3**: Single device test (simplest happy path)
4. ✅ **Task 4**: Multiple devices test (builds on Task 3)
5. ✅ **Task 5**: Empty state test (edge case)
6. ✅ **Task 6**: Disconnected device test (state variation)
7. ✅ **Task 7**: Unavailable storage test (error scenario)
8. ✅ **Task 8**: Mixed states test (complex scenario)
9. ✅ **Task 9**: Loading state test (timing/async)
10. ✅ **Task 10**: Error handling test (negative scenario)

**Adding data-testid Attributes:**

If components are missing data-testid attributes:

1. Identify which selectors you need
2. Document them in test file comments
3. Note in "Discoveries During Implementation" section
4. Add to components as needed (mark as component modification)

### Testing Best Practices

**DO:**
- ✅ Use `cy.wait('@alias')` for network requests
- ✅ Chain assertions for retryability
- ✅ Use fixture data to validate expected values
- ✅ Test user-visible outcomes
- ✅ Write descriptive test names

**DON'T:**
- ❌ Use `cy.wait(1000)` arbitrary timeouts
- ❌ Test implementation details
- ❌ Hardcode expected values - use fixtures
- ❌ Create test dependencies on execution order
- ❌ Use brittle CSS selectors

### Debugging Failed Tests

**Common Issues:**

1. **Element not found**: 
   - Add data-testid attribute to component
   - Verify component renders in browser
   - Check selector syntax

2. **Timing issues**:
   - Ensure `cy.wait('@alias')` is used
   - Check that interceptor is registered before navigation
   - Verify API response structure matches

3. **Assertion failures**:
   - Compare fixture data vs UI display
   - Check for data transformations in component
   - Verify selector targets correct element

4. **Flaky tests**:
   - Remove arbitrary waits
   - Use proper Cypress retry-ability
   - Ensure tests are independent

### After Completing Each Task

1. ✅ Run test suite: `npx nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts"`
2. ✅ Verify all tests pass consistently (run 3+ times)
3. ✅ Check test execution time is reasonable
4. ✅ Update task checkboxes
5. ✅ Document any discoveries or issues

### Final Validation

**Before marking phase complete:**

1. Run full test suite 5 times - verify 100% pass rate
2. Run in both headed and headless modes
3. Verify test execution time < 2 minutes
4. Review test names for clarity
5. Ensure README is accurate and complete
6. Verify all data-testid additions are documented

</details>

---

**Ready to start?** Begin with Task 1 and work sequentially through all tasks. Test as you go, document discoveries, and keep tests focused on user-observable behaviors. Good luck! 🚀
