# E2E Constants & Selectors

## 📋 Overview

The constants layer provides **single source of truth** for all API endpoints, UI selectors, and error responses used in E2E tests. Centralizing these values eliminates duplication and ensures consistency across all test files.

**Key Principle**: All tests **MUST** use constants - hardcoding values is prohibited.

---

## 📁 File Structure

```
constants/
├── api.constants.ts       # API endpoints, methods, status codes, helpers
├── selector.constants.ts  # UI component selectors grouped by feature
├── index.ts               # Barrel export (public API)
└── CONSTANTS.md           # This documentation
```

---

## 🔌 API Constants (`api.constants.ts`)

### Purpose
Centralized configuration for all TeensyROM API endpoints, HTTP methods, status codes, and interceptor aliases.

**Key Point**: TeensyROM API has **NO /api prefix** - routes go directly to `http://localhost:5168`

### Configuration

#### `API_CONFIG`
Base URL and common settings:

```typescript
export const API_CONFIG = {
  BASE_URL: 'http://localhost:5168',
  TIMEOUT: 5000,
  CONTENT_TYPE_JSON: 'application/json',
  CONTENT_TYPE_PROBLEM_JSON: 'application/problem+json',
} as const;
```

**When to Change**: Only `BASE_URL` changes for different environments (dev, staging, production)

### Device Endpoints

```typescript
export const DEVICE_ENDPOINTS = {
  FIND_DEVICES: {
    method: 'GET',
    path: '/devices',
    full: `${API_CONFIG.BASE_URL}/devices`,
    pattern: `${API_CONFIG.BASE_URL}/devices*`,
  },
  CONNECT_DEVICE: {
    method: 'POST',
    path: (deviceId: string) => `/devices/${deviceId}/connect`,
    full: (deviceId: string) => `${API_CONFIG.BASE_URL}/devices/${deviceId}/connect`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/connect`,
  },
  DISCONNECT_DEVICE: { /* ... */ },
  PING_DEVICE: { /* ... */ },
} as const;
```

**Endpoint Properties**:
- `method` - HTTP verb (GET, POST, DELETE, etc.)
- `path` - Relative path (with param functions for dynamic routes)
- `full` - Complete URL (use for assertions, debugging)
- `pattern` - Cypress intercept pattern with wildcards (use for `cy.intercept()`)

### HTTP Status & Interceptor Aliases

```typescript
export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  NOT_FOUND: 404,
  INTERNAL_SERVER_ERROR: 500,
} as const;

export const INTERCEPT_ALIASES = {
  FIND_DEVICES: 'findDevices',
  CONNECT_DEVICE: 'connectDevice',
  DISCONNECT_DEVICE: 'disconnectDevice',
  PING_DEVICE: 'pingDevice',
} as const;
```

### Helper Functions

#### `createProblemDetailsResponse(statusCode, title, detail?)`
Build standardized ProblemDetails error responses:

```typescript
// Before: Verbose and repetitive
req.reply({
  statusCode: 404,
  headers: { 'content-type': 'application/problem+json' },
  body: {
    type: 'https://tools.ietf.org/html/rfc9110#section-15.5.5',
    title: 'Device not found',
    status: 404,
  }
});

// After: Clean and declarative
req.reply(createProblemDetailsResponse(404, 'Device not found'));
```

**Parameters**:
- `statusCode` - HTTP status (404, 500, etc.)
- `title` - User-friendly error message
- `detail` - Optional technical details

### Usage Example

```typescript
import { DEVICE_ENDPOINTS, API_ROUTE_ALIASES, createProblemDetailsResponse } from './test-helpers';

it('should handle device discovery error', () => {
  cy.intercept(
    DEVICE_ENDPOINTS.FIND_DEVICES.method,
    DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
    (req) => {
      req.reply(createProblemDetailsResponse(404, 'No devices found'));
    }
  ).as(API_ROUTE_ALIASES.FIND_DEVICES);

  // Test continues...
});
```

---

## 🎨 UI Selectors (`selector.constants.ts`)

### Purpose
Centralized DOM selectors organized by component type. Single source of truth for all element selection.

### Selector Groups

#### `ALERT_SELECTORS` - Error/notification display

```typescript
export const ALERT_SELECTORS = {
  container: '.alert-display',
  icon: '.alert-icon',
  message: '.alert-message',
  dismissButton: '.alert-display button[aria-label="Dismiss alert"]',
  messageInContainer: '.alert-display .alert-message',
  iconInContainer: '.alert-display .alert-icon',
} as const;
```

**Use When**: Verifying error messages, notifications, alert state

#### `DEVICE_VIEW_SELECTORS` - Device list page

```typescript
export const DEVICE_VIEW_SELECTORS = {
  container: '[data-testid="device-view"]',
  deviceList: '[data-testid="device-list"]',
  emptyStateMessage: '[data-testid="empty-state-message"]',
  loadingIndicator: '.busy-dialog-content',
  refreshButton: 'button:contains("Refresh Devices")',
} as const;
```

**Use When**: Navigating device view, checking page state

#### `DEVICE_CARD_SELECTORS` - Device cards

```typescript
export const DEVICE_CARD_SELECTORS = {
  card: '[data-testid="device-card"]',
  powerButton: '[data-testid="device-power-button"]',
  deviceInfo: '[data-testid="device-info"]',
  idLabel: '[data-testid="device-id-label"]',
  portLabel: '[data-testid="device-port-label"]',
  stateLabel: '[data-testid="device-state-label"]',
  usbStorageStatus: '[data-testid="usb-storage-status"]',
  sdStorageStatus: '[data-testid="sd-storage-status"]',
} as const;
```

**Use When**: Interacting with devices, verifying device information

#### `BUSY_DIALOG_SELECTORS` - Loading indicator

```typescript
export const BUSY_DIALOG_SELECTORS = {
  content: '.busy-dialog-content',
  backdrop: '.busy-dialog-backdrop',
  spinner: '.busy-dialog-spinner',
} as const;
```

**Use When**: Verifying loading states during async operations

#### `BUTTON_SELECTORS` - Common button patterns

```typescript
export const BUTTON_SELECTORS = {
  byText: (text: string) => `button:contains("${text}")`,
  closeButton: 'button[aria-label="Close"]',
  dismissButton: 'button[aria-label="Dismiss"]',
  confirmButton: 'button[aria-label="Confirm"]',
  acceptButton: 'button[aria-label="Accept"]',
} as const;
```

**Use When**: Finding buttons by text or ARIA labels

### Helper Functions

#### `getDeviceCardByIndexSelector(index)`
Build selector for device card at specific index:

```typescript
cy.get(getDeviceCardByIndexSelector(0)).should('be.visible'); // First device
```

#### `getByTestId(testId)` & `getByClass(className)`
Generic selector builders:

```typescript
cy.get(getByTestId('custom-element')).click();
cy.get(getByClass('my-class')).should('exist');
```

### Usage Example

```typescript
import { DEVICE_CARD_SELECTORS, ALERT_SELECTORS } from './test-helpers';

it('should display device card with error', () => {
  // Verify device visible
  cy.get(DEVICE_CARD_SELECTORS.card).should('be.visible');
  cy.get(DEVICE_CARD_SELECTORS.idLabel).should('contain.text', 'Device Name');

  // Verify error alert appears
  cy.get(ALERT_SELECTORS.container)
    .should('be.visible')
    .within(() => {
      cy.get(ALERT_SELECTORS.message).should('contain.text', 'Connection failed');
    });
});
```

---

## 📤 Barrel Export (`index.ts`)

Central export point for all constants:

```typescript
export { INTERCEPT_ALIASES, DEVICE_ENDPOINTS, API_CONFIG, createProblemDetailsResponse } from './api.constants';
export {
  ALERT_SELECTORS,
  DEVICE_CARD_SELECTORS,
  DEVICE_VIEW_SELECTORS,
  BUSY_DIALOG_SELECTORS,
  BUTTON_SELECTORS,
  // ... helper functions
} from './selector.constants';
```

### Re-export from Test Helpers

The device test-helpers file re-exports all constants for convenience:

```typescript
// From test-helpers.ts
export {
  INTERCEPT_ALIASES as API_ROUTE_ALIASES,
  DEVICE_ENDPOINTS,
  API_CONFIG,
  createProblemDetailsResponse,
} from '../../support/constants/api.constants';

export {
  ALERT_SELECTORS,
  DEVICE_CARD_SELECTORS,
  // ... etc
} from '../../support/constants/selector.constants';
```

### Import in Tests

```typescript
import {
  DEVICE_ENDPOINTS,
  ALERT_SELECTORS,
  DEVICE_CARD_SELECTORS,
  createProblemDetailsResponse,
} from './test-helpers';
```

---

## ✅ Best Practices

### 1. Always Use Constants

**❌ Hardcoded:**
```typescript
cy.intercept('GET', 'http://localhost:5168/devices*', ...)
cy.get('.alert-display').should('be.visible')
```

**✅ Constants:**
```typescript
cy.intercept(DEVICE_ENDPOINTS.FIND_DEVICES.method, DEVICE_ENDPOINTS.FIND_DEVICES.pattern, ...)
cy.get(ALERT_SELECTORS.container).should('be.visible')
```

### 2. Use Correct Endpoint Property

- **`method`** - HTTP verb (GET, POST, DELETE)
- **`pattern`** - Cypress intercept (has wildcards for dynamic routes)
- **`full`** - Complete URL (debugging, logging)
- **`path`** - Relative path (rarely used directly)

```typescript
// ✅ Correct for cy.intercept()
cy.intercept(
  DEVICE_ENDPOINTS.CONNECT_DEVICE.method,        // 'POST'
  DEVICE_ENDPOINTS.CONNECT_DEVICE.pattern,       // 'http://localhost:5168/devices/*/connect'
  ...
)

// ❌ Wrong - pattern includes wildcards, breaks matching
cy.intercept(DEVICE_ENDPOINTS.CONNECT_DEVICE.full(deviceId), ...)
```

### 3. Use Helper Functions for Errors

**❌ Manual construction:**
```typescript
req.reply({
  statusCode: 404,
  headers: { 'content-type': 'application/problem+json' },
  body: { type: '...', title, status: 404, detail }
})
```

**✅ Helper function:**
```typescript
req.reply(createProblemDetailsResponse(404, title, detail))
```

### 4. Use Grouped Selectors

**❌ Hardcoded:**
```typescript
cy.get('[data-testid="device-id-label"]').should('contain.text', name);
cy.get('[data-testid="device-port-label"]').should('contain.text', port);
```

**✅ Grouped selectors:**
```typescript
cy.get(DEVICE_CARD_SELECTORS.idLabel).should('contain.text', name);
cy.get(DEVICE_CARD_SELECTORS.portLabel).should('contain.text', port);
```

### 5. Prefer Compound Selectors for Context

**❌ Multiple queries:**
```typescript
cy.get(ALERT_SELECTORS.container).should('be.visible');
cy.get(ALERT_SELECTORS.message).should('contain.text', 'Error');
```

**✅ Within context:**
```typescript
cy.get(ALERT_SELECTORS.container)
  .should('be.visible')
  .within(() => {
    cy.get(ALERT_SELECTORS.message).should('contain.text', 'Error');
  });
```

---

## 🔄 Updating Constants

### Adding a New API Endpoint

1. Add to `DEVICE_ENDPOINTS` in `api.constants.ts`:
```typescript
export const DEVICE_ENDPOINTS = {
  // ... existing endpoints
  NEW_ENDPOINT: {
    method: 'POST',
    path: '/new-route',
    full: `${API_CONFIG.BASE_URL}/new-route`,
    pattern: `${API_CONFIG.BASE_URL}/new-route*`,
  },
} as const;
```

2. Add alias to `INTERCEPT_ALIASES`:
```typescript
export const INTERCEPT_ALIASES = {
  // ... existing aliases
  NEW_ENDPOINT: 'newEndpoint',
} as const;
```

3. Update barrel export if needed in `index.ts`

### Adding a New UI Selector Group

1. Add to `selector.constants.ts`:
```typescript
export const NEW_COMPONENT_SELECTORS = {
  container: '[data-testid="new-component"]',
  button: '[data-testid="new-component-button"]',
  // ... more selectors
} as const;
```

2. Update barrel export in `index.ts`

3. Update test-helpers re-exports if needed

---

## 🧪 Testing Constants Changes

After updating constants, verify:

1. **Selectors still match DOM**: Run affected tests
2. **Endpoints match API**: Cross-check against OpenAPI spec
3. **No typos**: TypeScript catches most errors (readonly/const)
4. **Patterns match**: Wildcard patterns should match actual URLs

**Run specific tests:**
```bash
pnpm nx e2e teensyrom-ui-e2e --grep="device discovery"
```

---

## 🔍 Troubleshooting

### Issue: Selector not finding element

**Check**:
1. Is selector in the right group? (device vs alert vs page)
2. Does it match the HTML data-testid or class?
3. Is the element actually rendered?

**Solution**: Update selector or check component HTML

### Issue: Endpoint pattern doesn't match

**Check**:
1. Does `FIND_DEVICES.pattern` match actual API call? (use Network tab)
2. Are there query parameters? (use `*` to match any)
3. Is BASE_URL correct?

**Solution**: Adjust pattern or BASE_URL

### Issue: Status code helper building wrong URL section

**Check**:
1. Is `statusCode` 404 or 500? (determines RFC section)
2. Does response structure match ProblemDetails?

**Solution**: Pass correct status code

---

## 📚 Related Documentation

- **[E2E_TESTS.md](../E2E_TESTS.md)** - Constants requirement section
- **[E2E_FIXTURES.md](../src/support/test-data/fixtures/E2E_FIXTURES.md)** - Test data patterns
- **[OpenAPI Spec](../../../../TeensyRom.Api/api-spec/TeensyRom.Api.json)** - API source of truth

---

## 🎯 Summary

**API Constants** (`api.constants.ts`):
- ✅ One `BASE_URL` for all environments
- ✅ Endpoints with method, path, full URL, and intercept pattern
- ✅ Helper functions for common tasks
- ✅ Status codes and interceptor aliases

**UI Selectors** (`ui.selectors.ts`):
- ✅ Grouped by component type
- ✅ Compound selectors for common assertions
- ✅ Helper functions for dynamic selectors
- ✅ Prevents hardcoded `data-testid` and CSS classes

**Best Practice**: **All tests use constants** - no hardcoded values allowed!
