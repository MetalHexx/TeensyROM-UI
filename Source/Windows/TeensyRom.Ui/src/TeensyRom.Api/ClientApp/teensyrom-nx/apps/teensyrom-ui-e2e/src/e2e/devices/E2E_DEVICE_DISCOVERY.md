# Device Discovery E2E Tests

This directory contains comprehensive E2E tests for the device discovery workflow, validating that the device view correctly displays TeensyROM devices using mock fixtures and interceptors.

## Purpose

These tests verify that the device discovery feature works correctly by:
- Testing UI rendering with various device states
- Validating device information display
- Testing empty states and loading states
- Verifying error handling
- Ensuring proper state transitions

## Test Scenarios Covered

### ✅ Single Device Discovery
Tests that device view correctly displays a single TeensyROM device with all device information visible and properly formatted.

**Behaviors Tested:**
- Device card renders with fixture data
- Device name is displayed
- Device port (COM port) is visible
- Firmware version information shows
- Connection status indicator displays
- Storage status visible

**Test File:** `device-discovery.cy.ts` → "Single Device Discovery" suite

---

### ✅ Multiple Devices Discovery
Tests that device view correctly displays multiple TeensyROM devices simultaneously, showing all devices with unique information.

**Behaviors Tested:**
- Correct device count (3 devices from fixture)
- Each device shows unique name
- Each device shows different port
- All devices show connection status
- Devices maintain fixture order

**Test File:** `device-discovery.cy.ts` → "Multiple Devices Discovery" suite

---

### ✅ No Devices (Empty State)
Tests that device view correctly displays empty state UI when no devices are discovered.

**Behaviors Tested:**
- "No devices found" message displays
- No device cards render
- Empty state has appropriate styling
- Loading state has cleared

**Test File:** `device-discovery.cy.ts` → "No Devices (Empty State)" suite

---

### ✅ Disconnected Device
Tests that device view correctly displays a device that has lost its connection.

**Behaviors Tested:**
- Disconnected device card still renders
- Connection status shows "disconnected"
- Device information (name, port) preserved
- Device has different visual styling
- Connect/reconnect button available

**Test File:** `device-discovery.cy.ts` → "Disconnected Device" suite

---

### ✅ Unavailable Storage
Tests that device view correctly displays warnings when storage is unavailable.

**Behaviors Tested:**
- Device card renders (device is connected)
- Connection status shows as connected
- Storage warning/error indicator displays
- SD storage shows as unavailable
- USB storage shows as unavailable

**Test File:** `device-discovery.cy.ts` → "Unavailable Storage" suite

---

### ✅ Mixed Device States
Tests that device view correctly displays multiple devices in different states simultaneously.

**Behaviors Tested:**
- All devices render (3 total)
- First device shows connected
- Second device shows busy/processing
- Third device shows disconnected
- Each state visually distinct

**Test File:** `device-discovery.cy.ts` → "Mixed Device States" suite

---

### ✅ Loading States
Tests that device view shows appropriate loading state during device discovery API call.

**Behaviors Tested:**
- Loading indicator shows during API call
- Loading indicator disappears after response
- No device cards shown while loading
- Content appears after loading completes

**Test File:** `device-discovery.cy.ts` → "Loading States" suite

---

### ✅ Error Handling
Tests that device view correctly handles API errors and shows error states.

**Behaviors Tested:**
- Error message displays on API failure
- No devices shown on error
- Retry option available
- Loading state clears after error

**Test File:** `device-discovery.cy.ts` → "Error Handling" suite

---

## Running Tests

### Run all device discovery tests
```bash
npx nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts"
```

### Run all device tests (including navigation)
```bash
npx nx e2e teensyrom-ui-e2e --spec="**/devices/**"
```

### Run in headed mode (see browser)
```bash
npx nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts" --headed
```

### Run specific test suite (using grep)
```bash
npx nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts" --grep="Single Device"
```

### Run tests from project root (teensyrom-nx/)
```bash
# Headless (CI mode)
pnpm nx e2e teensyrom-ui-e2e --headless

# Headed (debugging)
pnpm nx e2e teensyrom-ui-e2e --headed
```

## Test Data

### Fixtures Used

**Fixtures** are pre-built device scenarios from `support/test-data/fixtures/devices.fixture.ts`:

- **singleDevice**: One connected device with available storage (happy path)
- **multipleDevices**: Three connected devices with unique identities
- **noDevices**: Empty array (no devices found)
- **disconnectedDevice**: Device with ConnectionLost state
- **unavailableStorageDevice**: Connected device with both SD and USB unavailable
- **mixedStateDevices**: Three devices in different states (Connected, Busy, ConnectionLost)

### Interceptors Used

**Interceptors** mock API calls from `support/interceptors/device.interceptors.ts`:

- **interceptFindDevices()**: Mocks GET /api/devices - returns device list
  - Default: Returns singleDevice fixture
  - Options: Custom fixture, error mode (HTTP 500)

## Selectors (data-testid Strategy)

Tests use `data-testid` attributes for reliable element selection. This approach is preferred because:
- Selectors don't break when styling changes
- Selectors reflect test intent (semantic)
- Selectors are resilient to CSS changes

### Device View Selectors

| Selector | Element | Purpose |
|----------|---------|---------|
| `device-view` | `.device-view` | Main device view container |
| `loading-indicator` | Loading state div | Visible while loading devices |
| `device-list` | `.device-list` | Container for device cards |
| `empty-state-message` | `<p>No devices found</p>` | Message when no devices |
| `device-item-{deviceId}` | `<lib-device-item>` | Individual device card |
| `device-card` | `<lib-scaling-card>` | Device card wrapper |
| `device-power-button` | Power button | Connect/disconnect button |

### Device Item Selectors

| Selector | Element | Purpose |
|----------|---------|---------|
| `device-info` | `.device-info` | Device information section |
| `device-id-label` | Device ID icon-label | Device ID display |
| `device-firmware-label` | Firmware icon-label | Firmware version |
| `device-port-label` | Port icon-label | COM port/USB path |
| `device-state-label` | State icon-label | Device state (Connected, etc.) |
| `device-compatible-label` | Compatible icon-label | Compatibility status |
| `device-storage` | `.device-storage` | Storage status section |
| `usb-storage-status` | USB storage component | USB storage status |
| `sd-storage-status` | SD storage component | SD card storage status |

## Test Helpers

Helper functions in `test-helpers.ts` encapsulate common test operations:

- **navigateToDeviceView()**: Navigate to `/devices` route
- **waitForDeviceDiscovery()**: Wait for device API to complete
- **verifyDeviceCard()**: Assert device card properties
- **verifyEmptyState()**: Assert empty state display
- **getDeviceCard()**: Get device card by device ID

## Troubleshooting

### Test Times Out Waiting for Elements

**Problem:** `cy.get('[data-testid="..."]')` times out

**Solutions:**
1. Verify component renders (check browser dev tools)
2. Verify data-testid attribute exists on element
3. Check selector spelling matches exactly
4. Ensure element is not hidden or display:none

### Loading State Not Visible

**Problem:** Can't capture loading state between requests

**Solutions:**
1. Use `cy.intercept()` with delay: `cy.intercept('GET', '/api/devices*', (req) => { req.reply({ delay: 1000, ... }) })`
2. Verify loading signal in component updates correctly
3. Check that API call happens on component initialization

### Device Data Doesn't Match Fixture

**Problem:** UI shows different data than fixture

**Solutions:**
1. Verify interceptor returns correct fixture
2. Check component maps API response to domain model correctly
3. Look for data transformations in component
4. Compare fixture data vs UI display in test output

### Flaky Tests (Intermittent Failures)

**Common Causes:**
1. Arbitrary waits (`cy.wait(1000)`) - replace with `cy.wait('@alias')`
2. Race conditions - ensure interceptor is set up before navigation
3. Timing issues - use Cypress retry-ability with proper assertions

**Solutions:**
1. Remove arbitrary timeouts
2. Use `.should()` for retry-able assertions
3. Wait for network requests with `cy.wait('@findDevices')`
4. Ensure test isolation (each test sets up its own interceptors)

### Assertions Failing

**Problem:** `.should('contain.text', 'value')` fails but text is visible

**Solutions:**
1. Use `.should('be.visible')` first to check element exists
2. Verify whitespace in assertion matches rendered text
3. Check for dynamic content (signals, observables) updates
4. Look at test output details - exact error message helps

## Test Execution Statistics

**Total Test Cases:** ~35-40 assertions across 8 test suites

**Estimated Execution Time:** < 2 minutes for full suite

**Coverage:**
- ✅ Happy paths (single/multiple devices)
- ✅ Edge cases (empty state, disconnected)
- ✅ Error scenarios (API failures)
- ✅ State transitions (loading → content)
- ✅ Async operations (API calls, state updates)

## Best Practices Used

✅ **User-Observable Behaviors** - Tests verify what users see/interact with, not implementation details

✅ **Deterministic Test Data** - Same fixture always produces same results

✅ **Test Independence** - Each test sets up its own state; no dependencies on execution order

✅ **Clear Test Names** - Test descriptions explain scenario and expected behavior

✅ **Reliable Selectors** - data-testid attributes are semantic and resilient

✅ **Proper Async Handling** - Use `cy.wait()` for network requests, avoid arbitrary timeouts

✅ **Cypress Best Practices** - Leverage retry-ability, command chaining, and assertions

## References

- **Cypress Best Practices**: https://docs.cypress.io/guides/references/best-practices
- **Cypress Assertions**: https://docs.cypress.io/guides/references/assertions
- **Phase 2: Device Fixtures**: `support/test-data/fixtures/devices.fixture.ts`
- **Phase 3: Device Interceptors**: `support/interceptors/device.interceptors.ts`
- **Fixtures Documentation**: `support/test-data/README.md`

---

**Last Updated:** October 19, 2025  
**Phase:** Phase 4 - Device Discovery E2E Tests  
**Status:** ✅ Complete
