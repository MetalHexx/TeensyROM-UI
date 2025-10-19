# Phase 4 E2E Test Timing Issues

## ✅ STATUS: RESOLVED

**Resolution Date**: January 19, 2025  
**See**: [E2E_PLAN_P4_RESULTS.md](./E2E_PLAN_P4_RESULTS.md) for complete analysis

## 🚨 Problem Summary (HISTORICAL)

Phase 4 Device Discovery E2E tests were experiencing systematic timeout failures. **12 out of 39 tests were failing** with the error:

```
CypressError: Timed out retrying after 10000ms: `cy.wait()` timed out waiting `10000ms` 
for the 1st request to the route: `findDevices`. No request ever occurred.
```

**Test Results (Before Fix)**: 2 passing, 12 failing, 25 skipped  
**Duration**: ~2 minutes (tests timing out at 10-second limit)

## ✅ Root Cause Identified

**Interceptor URL pattern mismatch**: Tests used `/api/devices*` but actual API calls were to `http://localhost:5168/devices`

### Discovery Process
1. Examined OpenAPI spec at `api-spec/TeensyRom.Api.json`
2. Confirmed endpoint is `/devices` (no `/api` prefix)
3. Checked generated API client configuration: `basePath: 'http://localhost:5168'`
4. Verified Angular app makes cross-origin calls from `http://localhost:4200` to `http://localhost:5168`

## ✅ Solution Applied

Updated all device interceptors in `device.interceptors.ts`:

```typescript
// ❌ Before (incorrect pattern)
cy.intercept('GET', '/api/devices*', ...)

// ✅ After (correct pattern)
cy.intercept('GET', 'http://localhost:5168/devices*', ...)
```

**Result**: **32 out of 39 tests now passing** (82% pass rate)  
All 12 timeout failures resolved. Remaining 7 failures are unrelated to timing (SignalR events and loading indicators).

---

## 🔍 Root Cause Analysis

### The Application Bootstrap Flow

The Angular application has a bootstrap service that **automatically calls `findDevices()` on app initialization**:

```typescript
// libs/app/bootstrap/src/lib/app-bootstrap.service.ts
async init(): Promise<void> {
  return new Promise((resolve) => {
    // ... effect setup ...
    this.deviceLogsService.connect();
    this.deviceEventsService.connect();
    this.deviceStore.findDevices(); // ← CALLED ON APP INIT
  });
}
```

This means:
- When Angular boots, it immediately calls the device discovery API
- This happens **before** the user navigates to any specific route
- The `/devices` route does NOT trigger device loading - it just displays already-loaded devices

### Our Test Pattern

Our tests follow this sequence:

```typescript
beforeEach(() => {
  interceptFindDevices({ fixture: singleDevice });  // 1. Setup interceptor
  navigateToDeviceView();                           // 2. cy.visit('/devices')
  waitForDeviceDiscovery();                         // 3. cy.wait('@findDevices', { timeout: 10000 })
});
```

### The Timing Problem

**Expected**: Interceptor catches the API call triggered by `cy.visit()` → app bootstrap → `findDevices()`

**Actual**: `cy.wait('@findDevices')` times out because "No request ever occurred"

This suggests one of these scenarios:

1. **URL Pattern Mismatch**: The interceptor pattern `/api/devices*` doesn't match the actual API call
2. **Interceptor Registration Lag**: The interceptor isn't fully registered before the app makes the API call
3. **State Persistence**: The DeviceStore is caching devices from a previous test, so bootstrap skips the API call
4. **Race Condition**: Angular's bootstrap completes faster than Cypress can set up the interception

---

## 🧪 What We've Tried

### Attempt 1: Increase Timeout
**Change**: Extended timeout from 5000ms to 10000ms  
**Result**: ❌ Same timeout error, just takes longer to fail

### Attempt 2: Clear Storage
**Change**: Added `localStorage.clear()` and `sessionStorage.clear()` in `onBeforeLoad` hook  
**Result**: ❌ No change - still timing out

### Attempt 3: Add Import
**Change**: Fixed missing `waitForDeviceDiscovery` import  
**Result**: ✅ Tests now run but still timeout on API wait

### Attempt 4: Centralize Navigation
**Change**: Created `navigateToDeviceView()` helper with storage clearing  
**Result**: ❌ No change - interceptor still not catching request

---

## 🎯 The Two Passing Tests

Interestingly, **2 tests pass consistently**:

```typescript
it('should show loading indicator during API call', () => {
  // Sets up cy.intercept() INLINE with delay
  cy.intercept('GET', '/api/devices*', (req) => {
    req.reply({
      delay: 500,
      statusCode: 200,
      body: { devices: singleDevice.devices, message: 'Found 1 device(s)' },
    });
  }).as('findDevices');

  navigateToDeviceView();
  verifyLoadingState();
});
```

**Why these pass**:
- They use `cy.intercept()` directly in the test body (not in beforeEach)
- They set up the interceptor immediately before navigation
- One uses a delay, making the timing more explicit

This suggests **the interceptor setup timing is critical** - setting it up in `beforeEach` may be too early or may be getting overridden.

---

## 📊 Test Infrastructure

### Interceptor Function
```typescript
// apps/teensyrom-ui-e2e/src/support/interceptors/device.interceptors.ts
export function interceptFindDevices(options: InterceptFindDevicesOptions = {}): void {
  const fixture = options.fixture ?? singleDevice;

  cy.intercept('GET', '/api/devices*', (req) => {
    if (options.errorMode) {
      req.reply({ statusCode: 500, body: createErrorResponse() });
    } else {
      const response: FindDevicesResponse = {
        devices: [...fixture.devices],
        message: `Found ${fixture.devices.length} device(s)`,
      };
      req.reply({ statusCode: 200, body: response });
    }
  }).as('findDevices');
}
```

### Test Helper
```typescript
// apps/teensyrom-ui-e2e/src/e2e/devices/test-helpers.ts
export function navigateToDeviceView(): Cypress.Chainable<Cypress.AUTWindow> {
  return cy.visit('/devices', {
    onBeforeLoad: (win) => {
      win.localStorage.clear();
      win.sessionStorage.clear();
    },
  });
}

export function waitForDeviceDiscovery(timeout = 10000): void {
  cy.wait(`@findDevices`, { timeout });
}
```

### Centralized Constants
```typescript
export const API_ROUTES = {
  BASE: '/api',
  DEVICES: '/api/devices',
  DEVICE_CONNECT: (deviceId: string) => `/api/devices/${deviceId}/connect`,
  DEVICE_DISCONNECT: (deviceId: string) => `/api/devices/${deviceId}`,
  DEVICE_PING: (deviceId: string) => `/api/devices/${deviceId}/ping`,
} as const;

export const API_ROUTE_ALIASES = {
  FIND_DEVICES: 'findDevices',
  CONNECT_DEVICE: 'connectDevice',
  DISCONNECT_DEVICE: 'disconnectDevice',
  PING_DEVICE: 'pingDevice',
} as const;
```

---

## 🤔 Questions to Investigate

### 1. What is the Actual API Call?
We need to verify:
- What is the **exact URL** of the API call? (Is it `/api/devices` or something else?)
- Does it include query parameters? (e.g., `/api/devices?refresh=true`)
- What HTTP method is used? (We assume GET)

**How to check**: Open Chrome DevTools Network tab, load `http://localhost:4200/devices`, observe the request

### 2. Is the DeviceStore Caching Devices?
We need to verify:
- Does the store check `hasInitialised` before calling the API?
- Does clearing storage actually clear the store state?
- Is there a way to force the store to re-initialize?

**Relevant code**:
```typescript
// libs/application/src/lib/device/device-store.ts
const initialState: DeviceState = {
  devices: [],
  hasInitialised: false,  // ← Does this prevent re-fetching?
  isLoading: true,
  isIndexing: false,
  error: null,
};
```

### 3. Are Multiple Interceptors Conflicting?
We need to verify:
- When a test completes, does the interceptor persist?
- Do multiple interceptors for the same route conflict?
- Should we be clearing routes between tests?

### 4. Is the Timing a Race Condition?
We need to verify:
- Does `cy.intercept()` register asynchronously?
- Does `cy.visit()` wait for intercept registration?
- Should we add an artificial delay between setup and navigation?

---

## 🛠️ Potential Solutions

### Solution 1: Use Inline Interceptors (Like Passing Tests)
Move interceptor setup into each test body instead of beforeEach:

```typescript
it('should display single device', () => {
  interceptFindDevices({ fixture: singleDevice });
  navigateToDeviceView();
  waitForDeviceDiscovery();
  
  verifyDeviceCount(1);
});
```

**Pros**: Matches the pattern of the 2 passing tests  
**Cons**: Violates DRY principle, duplicates setup code

### Solution 2: Mock the Bootstrap Service
Instead of intercepting the API, mock the AppBootstrapService:

```typescript
beforeEach(() => {
  cy.visit('/devices', {
    onBeforeLoad: (win) => {
      // Inject mock bootstrap service
      win['__mockBootstrap'] = true;
    }
  });
});
```

**Pros**: Avoids timing issues entirely  
**Cons**: Requires application code changes, less realistic testing

### Solution 3: Add Explicit Wait for Intercept Registration
Add a delay or cy.wait() after interceptor setup:

```typescript
beforeEach(() => {
  interceptFindDevices({ fixture: singleDevice });
  cy.wait(100); // Give interceptor time to register
  navigateToDeviceView();
  waitForDeviceDiscovery();
});
```

**Pros**: Simple to try  
**Cons**: Arbitrary delays are unreliable, may not solve the root cause

### Solution 4: Verify URL Pattern and Fix Match
Check the actual API call and ensure our pattern matches:

```typescript
// If actual call is /api/devices (no trailing *)
cy.intercept('GET', '/api/devices', (req) => { ... }).as('findDevices');

// Or if it includes query params
cy.intercept('GET', '/api/devices?*', (req) => { ... }).as('findDevices');
```

**Pros**: Fixes the root cause if it's a pattern mismatch  
**Cons**: Need to observe the actual API call first

### Solution 5: Use cy.intercept() with URL Matcher Object
More explicit URL matching:

```typescript
cy.intercept({
  method: 'GET',
  url: '**/api/devices',
}, (req) => {
  const response: FindDevicesResponse = { ... };
  req.reply({ statusCode: 200, body: response });
}).as('findDevices');
```

**Pros**: More precise matching, less ambiguous  
**Cons**: Still doesn't solve timing if that's the issue

---

## 🎬 Next Steps

### Immediate Actions

1. **Observe the Actual API Call**
   - Open Chrome DevTools Network tab
   - Navigate to `http://localhost:4200/devices`
   - Record the exact URL, method, headers, response of the device discovery call
   - Screenshot the Network tab

2. **Test URL Pattern Variations**
   - Try exact match: `'/api/devices'`
   - Try with trailing slash: `'/api/devices/'`
   - Try wildcard variations: `'**/api/devices'`, `'**/api/devices*'`
   - Try URL object matcher

3. **Test Interceptor Timing**
   - Move one test's interceptor setup inline (like the passing tests)
   - If it passes, we know it's a timing issue
   - If it still fails, we know it's a pattern matching issue

4. **Check Store State**
   - Add console logging to AppBootstrapService to see if findDevices() is actually called
   - Add console logging to DeviceStore to see if API call is made
   - Verify if `hasInitialised` is preventing re-fetching

### Tools to Consider

**Chrome DevTools MCP Integration**:
Yes, setting up the Chrome DevTools MCP would be extremely valuable for:
- Real-time network traffic observation
- Console log inspection during test execution
- DOM state verification
- Source code debugging

This would allow us to:
1. See exactly when the API call happens
2. Verify the interceptor is registered before the call
3. Check if there are multiple calls or cached responses
4. Inspect the DeviceStore state during test execution

**Setup**: https://github.com/ChromeDevTools/chrome-devtools-mcp/

---

## 📝 Test Failure Pattern

**Failing Tests** (12 total):
- All tests in: Single Device Discovery (6 tests)
- All tests in: Multiple Devices Discovery (5 tests)
- All tests in: No Devices (4 tests)
- All tests in: Disconnected Device (5 tests)
- All tests in: Unavailable Storage (5 tests)
- All tests in: Mixed Device States (6 tests)
- Some tests in: Loading States (2 tests)
- All tests in: Error Handling (4 tests)

**Passing Tests** (2 total):
- Loading States: "should show loading indicator during API call"
- Loading States: "should not show devices while loading"

**Pattern**: Tests that use inline `cy.intercept()` pass. Tests that use `interceptFindDevices()` in `beforeEach` fail.

---

## 🔧 Hypothesis

**Most Likely Cause**: The interceptor function `interceptFindDevices()` is not fully registered before `cy.visit()` triggers the app bootstrap, which immediately calls the API. The passing tests work because they set up `cy.intercept()` inline, ensuring the interceptor is ready before navigation.

**Next Test**: Move interceptor setup inline for one failing test suite and verify it passes. If it does, we need to refactor all tests to use inline interceptors or find a way to ensure registration completes before navigation.

---

## 📊 Summary

- **Problem**: 12/39 tests timeout waiting for `@findDevices` request that "never occurred"
- **Root Cause**: Likely timing issue between interceptor registration and app bootstrap API call
- **Evidence**: Tests using inline `cy.intercept()` pass; tests using `interceptFindDevices()` in `beforeEach` fail
- **Next Steps**: Observe actual API call in DevTools, test inline interceptor pattern, consider Chrome DevTools MCP for deeper debugging

**Status**: Investigation in progress, ready for collaborative troubleshooting with Chrome DevTools MCP assistance.
