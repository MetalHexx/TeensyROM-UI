# 📋 E2E Indexing Behaviors Testing Plan

## Phase Objective

Add comprehensive Cypress E2E test coverage for all storage indexing behaviors: single storage device indexing (USB/SD independently), batch "Index All" operation, busy dialog display during indexing, button state management, and error handling scenarios.

---

## Required Reading

- `apps/teensyrom-ui-e2e/E2E_TESTS.md` - Test architecture and patterns
- `apps/teensyrom-ui-e2e/src/support/constants/E2E_CONSTANTS.md` - Centralized constants
- `docs/TESTING_STANDARDS.md` - Behavioral testing approach
- DeviceStore indexing methods: `indexStorage()` and `indexStorageAllStorage()`

---

## 🗂️ File Structure

### New Files (✨)

```
apps/teensyrom-ui-e2e/
├── src/
│   ├── e2e/devices/
│   │   └── device-indexing.cy.ts                    ✨ Main test spec
│   ├── support/
│   │   ├── interceptors/
│   │   │   └── storage-indexing.interceptors.ts     ✨ Index API mocks
│   │   ├── test-data/
│   │   │   ├── fixtures/
│   │   │   │   └── indexing.fixture.ts              ✨ Device fixtures with storage states
│   │   │   └── generators/
│   │   │       └── indexing.generators.ts           ✨ Dynamic indexing scenarios
│   │   └── constants/
│   │       └── indexing.constants.ts                ✨ Selectors for index buttons/UI
│   └── e2e/devices/
│       └── indexing-test-helpers.ts                 ✨ Reusable assertions & navigation
```

### Modified Files (📝)

```
libs/features/devices/src/lib/device-view/
├── device-item/
│   └── device-item.component.html                   📝 Add data-testid for index buttons
├── storage-item/
│   └── storage-item.component.html                  📝 Add data-testid for USB/SD index buttons
└── device-toolbar/
    └── device-toolbar.component.html                📝 Add data-testid for "Index All" button

libs/app/shell/src/lib/layout/
└── layout.component.ts                              📝 Add data-testid for busy dialog

apps/teensyrom-ui-e2e/src/support/constants/
└── selector.constants.ts                            📝 Add new selectors for indexing components
```

---

## 🎯 Tasks

### Task 1: Add Data-TestID Attributes to UI Components

**Purpose**: Provide reliable selectors for E2E tests without coupling to CSS/HTML structure

**Subtasks:**

1. Add `data-testid="storage-index-button-{storageType}"` to `storage-item.component.html` index button
   - Use computed attribute based on storage type: `usb` or `sd`
   - Example: `[attr.data-testid]="'storage-index-button-' + (icon() === 'usb' ? 'usb' : 'sd')"`

2. Add `data-testid="device-toolbar-index-all"` to "Index All" button in `device-toolbar.component.html`

3. Add `data-testid="busy-dialog-indexing"` to busy dialog container in `layout.component.html`
   - Wrap BusyDialogComponent when `showIndexDialog` is true
   - Add to Material Dialog panel element

4. Add `data-testid="busy-dialog-message"` to the message text inside busy dialog
   - Allows verification of correct dialog content

**Testing Subtask**: Verify all selectors are present and unique in markup—run component specs to confirm HTML renders without errors

**Key Implementation Notes:**
- Use conditional attributes: `[attr.data-testid]="'storage-index-button-' + storageType"` pattern
- Busy dialog uses Material Dialog—add testid to the panel element, not the component
- Ensure testid attributes do NOT affect styling or functionality

---

### Task 2: Create Indexing Test Data Layer

**Purpose**: Build fixtures and generators for consistent, reusable indexing test scenarios

**File**: `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/indexing.fixture.ts`

**Subtasks:**

1. Create fixture `deviceWithAvailableStorage` - Device with both USB and SD available (ready for indexing)
2. Create fixture `deviceWithUnavailableStorage` - Device with USB unavailable, SD available (mixed state)
3. Create fixture `multipleDevicesForIndexing` - 2 connected devices with different storage availability
4. Create fixture `allStorageUnavailable` - Device with both USB and SD unavailable (no indexing possible)

**Testing Subtask**: Verify fixture structure matches `Device` model from domain and contains correct `isConnected`, `sdStorage.available`, `usbStorage.available` values

**Critical Type** (reference only):
```typescript
// Expected device storage structure
interface Device {
  deviceId: string;
  isConnected: boolean;
  sdStorage?: { available: boolean };
  usbStorage?: { available: boolean };
}
```

**File**: `apps/teensyrom-ui-e2e/src/support/test-data/generators/indexing.generators.ts`

**Subtasks:**

1. Create generator `generateDeviceForIndexing(options?: { sdAvailable?, usbAvailable?, connected? })` - Flexible device generation
2. Implement generator `generateMultipleDevicesForIndexing(count, options?)` - Create N devices with custom storage states

**Testing Subtask**: Verify generators produce valid devices with all required properties and honor override options

---

### Task 3: Create Storage Indexing Interceptors

**Purpose**: Mock API responses for indexing operations and track interceptor calls

**File**: `apps/teensyrom-ui-e2e/src/support/interceptors/storage-indexing.interceptors.ts`

**Subtasks:**

1. Create `interceptIndexStorage(deviceId: string, storageType: 'USB' | 'SD', options?: { delay?, statusCode?, errorMode? })` interceptor
   - Mock `POST http://localhost:5168/devices/{deviceId}/storage/{storageType}/index` endpoint
   - **Endpoint Pattern**: `/devices/*/storage/SD/index*` (use glob for cross-device matching)
   - Support configurable delay to simulate real indexing (e.g., 2-5 seconds)
   - Support error mode for failure testing (statusCode: 400 for bad request)
   - Register alias `@indexStorage_${deviceId}_${storageType}` for verification
   - **Success Response**: `{ statusCode: 200, body: { message: 'string' } }`
   - **Error Response**: `{ statusCode: 400, body: { title: string, status: 400, detail: string } }`

2. Create `interceptIndexAllStorage(options?: { delay?, statusCode?, errorMode?, devicesCount? })` interceptor
   - Mock `POST http://localhost:5168/files/index/all` endpoint
   - Track all intercepted calls for verification
   - Support staggered delays or uniform delay
   - **Success Response**: `{ statusCode: 200, body: { message: 'string' } }`
   - **Error Response**: `{ statusCode: 404, body: { title: string, status: 404, detail: string } }`

3. Create `interceptIndexStorageBatch(deviceStorageCombos: Array<{deviceId, storageType}>, options?)` helper
   - Batch setup multiple indexing interceptors in one call
   - Simplify test setup for "Index All" scenarios

**Testing Subtask**: Verify interceptors register successfully and intercept requests with correct status codes (200 success, 400/404 for errors)

**Key Implementation Notes:**
- API endpoint pattern: Cross-origin POST `http://localhost:5168/...`
- Storage type must be uppercase: `"USB"` or `"SD"` (enum values)
- Simulate realistic indexing delay (no instant completion)
- Error response uses `createProblemDetailsResponse()` helper
- Use glob patterns in cy.intercept for flexible path matching

---

### Task 4: Create Indexing Selector Constants

**Purpose**: Centralize all UI selectors used in indexing tests

**File**: `apps/teensyrom-ui-e2e/src/support/constants/indexing.constants.ts`

**Subtasks:**

1. Create selector group for storage index buttons:
   ```typescript
   export const STORAGE_INDEX_BUTTON_USB = '[data-testid="storage-index-button-usb"]';
   export const STORAGE_INDEX_BUTTON_SD = '[data-testid="storage-index-button-sd"]';
   export const STORAGE_INDEX_BUTTON_BY_TYPE = (type: string) => 
     `[data-testid="storage-index-button-${type.toLowerCase()}"]`;
   ```

2. Create selector for toolbar "Index All" button:
   ```typescript
   export const TOOLBAR_INDEX_ALL_BUTTON = '[data-testid="device-toolbar-index-all"]';
   ```

3. Create selector group for busy dialog indexing:
   ```typescript
   export const BUSY_DIALOG_INDEXING = '[data-testid="busy-dialog-indexing"]';
   export const BUSY_DIALOG_MESSAGE = '[data-testid="busy-dialog-message"]';
   export const BUSY_DIALOG_INDEXING_TEXT = 'Indexing Storage';
   export const BUSY_DIALOG_INDEXING_MESSAGE = 'This can take a few minutes. Do not touch your commodore device.';
   ```

4. Create selector for storage availability status:
   ```typescript
   export const STORAGE_STATUS_AVAILABLE = '[data-testid="storage-status-available"]';
   export const STORAGE_STATUS_UNAVAILABLE = '[data-testid="storage-status-unavailable"]';
   ```

**Testing Subtask**: Verify all selectors are unique and resolve correctly in the component templates

**Key Implementation Notes:**
- Export all constants from a single barrel file for easy import
- Use `STORAGE_INDEX_BUTTON_BY_TYPE()` helper for dynamic storage type selection
- Follow naming pattern: `COMPONENT_ELEMENT_STATE` (e.g., `STORAGE_INDEX_BUTTON_USB`)

---

### Task 5: Create Indexing Test Helpers

**Purpose**: Reusable navigation and assertion functions for indexing tests

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/indexing-test-helpers.ts`

**Subtasks:**

1. Create helper `setupIndexingScenario(deviceFixture, interceptorOptions?)` - Setup devices, interceptors, and navigate to device view
2. Create helper `verifyBusyDialogDisplayed(expectedMessage)` - Assert busy dialog is visible with correct message
3. Create helper `verifyBusyDialogHidden()` - Assert busy dialog is closed after indexing completes
4. Create helper `verifyStorageIndexButtonState(storageType, shouldBeDisabled)` - Assert button is enabled/disabled based on storage availability
5. Create helper `clickStorageIndexButton(storageType)` - Click index button for USB or SD
6. Create helper `clickIndexAllButton()` - Click the toolbar "Index All" button
7. Create helper `waitForIndexingComplete(alias, timeout?)` - Wait for indexing API call to complete
8. Create helper `verifyIndexingCallMade(deviceId, storageType)` - Assert index API was called for specific device/storage

**Testing Subtask**: Verify helpers interact correctly with interceptors and return proper assertions

**Key Implementation Notes:**
- Import all selector constants from `indexing.constants.ts`
- Use `cy.wait()` with registered aliases for interceptor calls
- Add configurable timeouts for slow operations (indexing can take several seconds)

---

### Task 6: Create Single Storage Device Indexing Tests

**Purpose**: Verify independent USB and SD indexing behavior for individual devices

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts`

**Test Suite**: "Device Storage Indexing - Single Device"

**Test Cases:**

**Test 6.1**: "Should display index button for available USB storage"
- Setup: Device with USB available
- Action: Navigate to device view
- Verify: USB index button is visible and enabled
- Verify: SD index button is visible but disabled (storage unavailable)

**Test 6.2**: "Should display index button for available SD storage"
- Setup: Device with SD available, USB unavailable
- Action: Navigate to device view
- Verify: SD index button is visible and enabled
- Verify: USB index button is visible but disabled

**Test 6.3**: "Should disable all storage index buttons when storage unavailable"
- Setup: Device with both USB and SD unavailable
- Action: Navigate to device view
- Verify: Both USB and SD index buttons are disabled

**Test 6.4**: "Should show busy dialog when indexing USB storage"
- Setup: Device with USB available, mock index interceptor with 2-second delay
- Action: Click USB index button
- Verify: Busy dialog appears immediately with message "Indexing Storage"
- Verify: Dialog remains visible during indexing (wait 1 second, assert still visible)
- Verify: Dialog closes after indexing completes

**Test 6.5**: "Should show busy dialog when indexing SD storage"
- Setup: Device with SD available, mock index interceptor with 2-second delay
- Action: Click SD index button
- Verify: Busy dialog appears with message "Indexing Storage"
- Verify: Dialog persists during indexing
- Verify: Dialog closes when complete

**Test 6.6**: "Should disable index button during indexing operation"
- Setup: Device with USB available, mock index interceptor with 3-second delay
- Action: Click USB index button
- Verify: USB index button becomes disabled
- Verify: Button remains disabled until indexing completes
- Verify: Button re-enables after completion

**Test 6.7**: "Should handle indexing error with error alert"
- Setup: Device with USB available, mock index interceptor with error response (400 bad request)
- Action: Click USB index button
- Verify: Busy dialog appears
- Verify: Dialog closes when error occurs
- Verify: Error alert displayed with error message

**Testing Subtask**: All tests use helpers from `indexing-test-helpers.ts` and selector constants from `indexing.constants.ts`. Verify assertions use observable behaviors (UI visible/hidden, buttons enabled/disabled) not implementation details.

**Key Implementation Notes:**
- Use realistic indexing delays (simulate 2-5 second operations)
- Test both success and error paths
- Verify button state changes during operation
- Always verify dialog message content (not just presence)

---

### Task 7: Create Index All / Batch Indexing Tests

**Purpose**: Verify "Index All" button behavior indexing all connected devices and their storage simultaneously

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts` (same file)

**Test Suite**: "Device Storage Indexing - Index All"

**Test Cases:**

**Test 7.1**: "Should disable Index All button when no devices connected"
- Setup: No connected devices
- Action: Navigate to device view
- Verify: "Index All" button is disabled

**Test 7.2**: "Should enable Index All button when devices connected"
- Setup: 2 connected devices with available storage
- Action: Navigate to device view
- Verify: "Index All" button is enabled

**Test 7.3**: "Should index all connected devices and storage when Index All clicked"
- Setup: 3 connected devices (each with USB + SD available), mock interceptors for all 6 combinations (3 devices × 2 storage types)
- Action: Click "Index All" button
- Verify: All 6 intercepted calls are made
- Verify: Busy dialog displays once (not per device)
- Verify: Dialog shows "Indexing Storage" message
- Verify: Devices are indexed in expected order or all concurrently

**Test 7.4**: "Should handle partial storage availability in Index All"
- Setup: 2 devices - Device 1 has USB+SD, Device 2 has only USB available
- Action: Click "Index All"
- Verify: 3 total API calls made (Device1 USB, Device1 SD, Device2 USB)
- Verify: Busy dialog persists for full operation
- Verify: Dialog closes after all complete

**Test 7.5**: "Should show busy dialog during Index All operation"
- Setup: 2 devices with storage, interceptor with 3-second delay
- Action: Click "Index All"
- Verify: Busy dialog appears immediately
- Verify: Dialog persists during full indexing duration
- Verify: Dialog closes after all indexing complete

**Test 7.6**: "Should handle Index All with partial failures"
- Setup: 2 devices with storage, 1st device USB indexing succeeds, 2nd device SD indexing fails (400 error)
- Action: Click "Index All"
- Verify: Dialog shows during operation
- Verify: Dialog closes when all operations complete (success + error)
- Verify: Error alert displayed with error details

**Test 7.7**: "Should not allow Index All during single device indexing"
- Setup: 2 devices, mock USB indexing with 5-second delay
- Action: Click USB index on Device 1, then immediately try to click "Index All"
- Verify: "Index All" button is disabled while any indexing is in progress
- Verify: Can click "Index All" after Device 1 indexing completes

**Test 7.8**: "Should disable Index All button when only disconnected devices present"
- Setup: 3 devices, 2 disconnected, 1 connected
- Action: Navigate to device view
- Verify: "Index All" button enabled (at least 1 connected device)
- Action: Disconnect the final connected device (or mock disconnection)
- Verify: "Index All" button disabled

**Testing Subtask**: All tests verify button state, dialog visibility, interceptor calls, and error handling. Use mocking to control timing and simulate realistic delays.

**Key Implementation Notes:**
- Batch setup all interceptors upfront for realistic concurrent indexing
- Test that single indexing blocks "Index All" (if `isIndexing` flag prevents multiple operations)
- Verify all storage combos are indexed even with partial failures

---

### Task 8: Create Error Handling and Edge Case Tests

**Purpose**: Verify robust error handling and edge cases in indexing workflows

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts` (same file)

**Test Suite**: "Device Storage Indexing - Error Handling & Edge Cases"

**Test Cases:**

**Test 8.1**: "Should handle network timeout during indexing"
- Setup: Device with storage, mock interceptor that never responds (timeout)
- Action: Click index button with short timeout (e.g., 5 seconds)
- Verify: Busy dialog appears
- Verify: Request times out and error is displayed
- Verify: Dialog closes on error

**Test 8.2**: "Should handle 400 Bad Request error during indexing"
- Setup: Device with storage, mock interceptor with 400 error response
- Action: Click index button
- Verify: Dialog appears then closes
- Verify: Error alert displayed with error message from server

**Test 8.3**: "Should handle 404 Not Found error during Index All"
- Setup: No connected devices, call to Index All returns 404
- Action: Mock interceptor returns 404
- Verify: Error alert displayed

**Test 8.4**: "Should recover and allow retry after indexing error"
- Setup: Device with storage, first indexing fails, second succeeds
- Action: Click index button (fails), then click again (succeeds)
- Verify: First attempt shows error
- Verify: Button is re-enabled after error
- Verify: Second attempt succeeds

**Test 8.5**: "Should handle indexing with disconnected device"
- Setup: Device shown as available, but connection is lost during indexing
- Action: Start indexing on Device 1, simulate device disconnect
- Verify: Request completes (API error or timeout)
- Verify: UI properly handles error state

**Test 8.6**: "Should handle rapid successive index clicks on same storage"
- Setup: Device with storage, indexing interceptor with 3-second delay
- Action: Click USB index button, immediately click again before first completes
- Verify: Only one indexing operation proceeds (button disabled prevents second click)
- Verify: Dialog shows once
- Verify: Only one API call is made

**Test 8.7**: "Should handle indexing when storage becomes unavailable mid-operation"
- Setup: Device with USB storage available, start indexing
- Action: Simulate storage becoming unavailable (via device event) mid-indexing
- Verify: Operation continues to completion
- Verify: UI updates storage availability status after operation completes

**Testing Subtask**: Verify error messages are user-friendly, operations don't hang, and UI recovers gracefully from errors.

**Key Implementation Notes:**
- Use realistic error responses from API spec (ProblemDetails format)
- Test both user-induced errors (rapid clicks) and system errors (network/server)
- Verify button state recovery after errors

---

### Task 9: Create Device List Integration Tests

**Purpose**: Verify indexing behavior works correctly with multiple devices in various states

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts` (same file)

**Test Suite**: "Device Storage Indexing - Multi-Device Integration"

**Test Cases:**

**Test 9.1**: "Should index specific device without affecting others"
- Setup: 3 devices displayed, each with storage
- Action: Index Device 1 USB storage
- Verify: Only Device 1 USB indexing API called
- Verify: Other devices' index buttons remain enabled
- Verify: "Index All" button remains disabled (single indexing in progress)

**Test 9.2**: "Should display correct storage status for each device independently"
- Setup: 3 devices - Device 1 (USB available), Device 2 (SD available), Device 3 (both unavailable)
- Action: Navigate to device view
- Verify: Device 1 - USB enabled, SD disabled
- Verify: Device 2 - USB disabled, SD enabled
- Verify: Device 3 - both disabled

**Test 9.3**: "Should handle device connecting/disconnecting during indexing"
- Setup: 2 devices connected, Device 2 indexing in progress
- Action: Device 1 disconnects (simulated)
- Verify: Device 1 removed from view (or marked disconnected)
- Verify: Device 2 indexing continues and completes normally

**Test 9.4**: "Should maintain correct button state across multiple sequential indexing operations"
- Setup: 2 devices, each index USB then SD
- Action: Index Device 1 USB → wait complete → Index Device 1 SD → Index Device 2 USB
- Verify: Each operation completes successfully
- Verify: Dialog shown/hidden for each operation
- Verify: All buttons are enabled between operations

**Testing Subtask**: Verify complex multi-device scenarios work as expected with proper isolation between devices.

**Key Implementation Notes:**
- Use fixture with multiple devices in different storage states
- Verify UI updates correctly for each device independently
- Test sequential and concurrent scenarios

---

### Task 10: Create Busy Dialog Content Verification Tests

**Purpose**: Ensure busy dialog displays correct content and behavior

**File**: `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts` (same file)

**Test Suite**: "Busy Dialog - Indexing Content"

**Test Cases:**

**Test 10.1**: "Should display correct dialog title and message during indexing"
- Setup: Device with storage, mock indexing interceptor
- Action: Click index button
- Verify: Dialog title is "Indexing Storage"
- Verify: Dialog message is "This can take a few minutes. Do not touch your commodore device."
- Verify: Message is visible and readable

**Test 10.2**: "Should display different dialog for finding devices vs indexing"
- Setup: Mock both find devices and indexing operations
- Action: Navigate to device view (find devices), verify dialog content
- Then: Click index button, verify dialog content changed to indexing message
- Verify: Two different dialogs are used for different operations

**Test 10.3**: "Should have non-dismissible busy dialog (no close button)"
- Setup: Device with storage, indexing in progress
- Action: Click index button, dialog appears
- Verify: No close button visible on dialog
- Verify: Dialog cannot be dismissed by clicking background (if applicable)
- Verify: Dialog only closes when operation completes

**Test 10.4**: "Should display busy indicator in dialog"
- Setup: Device with storage, mock indexing with delay
- Action: Click index button
- Verify: Dialog displays loading spinner or progress indicator
- Verify: Indicator is visible throughout indexing

**Testing Subtask**: Verify dialog content, message text, and UX (non-dismissible, shows progress indicator).

**Key Implementation Notes:**
- Extract exact dialog text from component (defined in layout.component.ts)
- Verify both "Finding Devices" and "Indexing Storage" dialogs exist and differ
- Ensure dialog is non-dismissible (Material Dialog `disableClose: true`)

---

## ✅ Data-TestID Summary

All components require new `data-testid` attributes:

| Component | Element | TestID | Used By |
|-----------|---------|--------|---------|
| `storage-item.component` | USB Index Button | `storage-index-button-usb` | Tests 6.2, 6.4, 6.6, 7.3, 7.4, 7.7 |
| `storage-item.component` | SD Index Button | `storage-index-button-sd` | Tests 6.3, 6.5, 7.3, 7.4 |
| `device-toolbar.component` | Index All Button | `device-toolbar-index-all` | Tests 7.1-8 |
| `layout.component` | Busy Dialog (Indexing) | `busy-dialog-indexing` | Tests 6.4-8 |
| `layout.component` | Busy Dialog Message | `busy-dialog-message` | Test 10.1 |

---

## 📝 Files Modified or Created

### New Components / Test Files:
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-indexing.cy.ts` (main test spec - ~700 lines, 30 test cases)
- `apps/teensyrom-ui-e2e/src/support/interceptors/storage-indexing.interceptors.ts`
- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/indexing.fixture.ts`
- `apps/teensyrom-ui-e2e/src/support/test-data/generators/indexing.generators.ts`
- `apps/teensyrom-ui-e2e/src/support/constants/indexing.constants.ts`
- `apps/teensyrom-ui-e2e/src/e2e/devices/indexing-test-helpers.ts`

### Modified Component Files (add data-testid):
- `libs/features/devices/src/lib/device-view/storage-item/storage-item.component.html`
- `libs/features/devices/src/lib/device-view/device-toolbar/device-toolbar.component.html`
- `libs/app/shell/src/lib/layout/layout.component.html` (or .ts if dialog opened from there)

### Modified Constants (add new selectors):
- `apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts`

---

## 🎯 Testing Summary

### Test Count & Organization:
- **Total Test Cases**: 30
- **Single Device Indexing**: 7 tests
- **Index All / Batch**: 8 tests
- **Error Handling & Edge Cases**: 7 tests
- **Multi-Device Integration**: 4 tests
- **Busy Dialog Content**: 4 tests

### Behaviors Covered:

✅ **Single Storage Indexing**
- USB/SD independently triggerable
- Buttons enable/disable based on storage availability
- Busy dialog displayed during operation
- Dialog closes on completion or error

✅ **Index All Functionality**
- Indexes all devices and storage combos in one operation
- Button disabled when no devices connected
- Button disabled during any indexing operation
- Handles partial availability (some storage unavailable)

✅ **Busy Dialog**
- Shows correct title/message for indexing
- Non-dismissible (no close button)
- Displays progress indicator
- Closes automatically on completion

✅ **Error Handling**
- Network timeouts
- HTTP error responses (400 Bad Request, 404 Not Found)
- Recoverable errors (can retry)
- Partial failures in batch operations

✅ **UI State Management**
- Buttons correctly disabled/enabled based on storage availability and operation state
- Multiple devices show correct individual states
- Sequential and concurrent operations work correctly

---

## 🔧 API Endpoints Reference

### Single Storage Index Endpoint

```
POST http://localhost:5168/devices/{deviceId}/storage/{storageType}/index
```

**Path Parameters:**
- `deviceId`: string (device identifier)
- `storageType`: enum - `"USB"` or `"SD"` (case-sensitive, uppercase)

**Query Parameters:**
- `StartingPath`: optional string (omit for full storage index)

**Success Response (200 OK):**
```json
{
  "message": "string"
}
```

**Error Response (400 Bad Request):**
```json
{
  "type": "string",
  "title": "string",
  "status": 400,
  "detail": "string",
  "instance": "string"
}
```

---

### Index All Devices Endpoint

```
POST http://localhost:5168/files/index/all
```

**No parameters** - Indexes all connected devices' storage

**Success Response (200 OK):**
```json
{
  "message": "string"
}
```

**Error Response (404 Not Found):**
```json
{
  "type": "string",
  "title": "string",
  "status": 404,
  "detail": "string",
  "instance": "string"
}
```

---

## ✅ Implementation Readiness

### API Verification Checklist

- ✅ Endpoints exist and match component expectations
- ✅ `StorageType.Usb`/`StorageType.Sd` correctly map to `"USB"`/`"SD"` enum values
- ✅ Response structure is simple (`{ message }`) - tests focus on API completion, not content
- ✅ Error handling uses `ProblemDetails` standard format
- ✅ Busy dialog logic driven by store state, not API response
- ✅ Single index (`POST /devices/{id}/storage/{type}/index`) and batch index (`POST /files/index/all`) endpoints confirmed
- ✅ Cross-origin URLs required for Cypress mocking (`http://localhost:5168/...`)

### Interceptor Implementation Notes

**Pattern for single device indexing:**
```typescript
cy.intercept(
  'POST',
  'http://localhost:5168/devices/*/storage/SD/index*',
  { statusCode: 200, body: { message: 'Indexing complete' } }
).as('indexStorage_sd');
```

**Pattern for batch indexing:**
```typescript
cy.intercept(
  'POST',
  'http://localhost:5168/files/index/all',
  { statusCode: 200, body: { message: 'All storage indexed' } }
).as('indexAllStorage');
```

**Error pattern (400 Bad Request):**
```typescript
cy.intercept(
  'POST',
  'http://localhost:5168/devices/*/storage/USB/index*',
  { 
    statusCode: 400,
    body: { 
      title: 'Invalid Request', 
      status: 400, 
      detail: 'Device not available'
    }
  }
).as('indexStorage_error');
```

---

## Success Criteria

Phase is complete when:
- ✅ All 30 Cypress tests pass consistently
- ✅ All data-testid attributes added to components
- ✅ Test helpers and interceptors properly mock indexing operations
- ✅ Coverage includes single device, batch, errors, and multi-device scenarios
- ✅ Dialog visibility and content verified
- ✅ Button state management tested thoroughly
- ✅ Tests run in < 2 minutes total

---

## Implementation Notes

### Architecture Alignment:
- Tests follow fixture + interceptor + spec three-layer pattern from existing E2E suite
- Selectors centralized in constants layer for maintainability
- Test helpers encapsulate common setup/assertion patterns

### Timing & Flakiness:
- Interceptor delays simulate realistic operations but must be consistent
- Use `cy.intercept()` aliases (`@alias`) with `cy.wait()` to avoid flaky timing assertions
- Dialog assertions should wait for visibility, not assume instant appearance

### Data Consistency:
- Fixtures match real Device/Storage models from domain layer
- Generators support custom scenarios while maintaining type safety
- All mock responses follow API spec (OpenAPI contract)

### Test Independence:
- Each test sets up own interceptors and fixtures
- `beforeEach` clears state and registers interceptors
- No test depends on execution order

### Future Extensibility:
- Plan can extend to SignalR device events if needed (Phase 5)
- Interceptor system supports adding more storage operations (e.g., delete, rename)
- Test structure allows easy addition of new device types or storage states
