# E2E Constants & Selectors

## 🎯 Purpose

E2E tests must be **declarative**—they describe *what* behavior is expected, not *how* to interact with the UI. By centralizing all infrastructure details (API endpoints, DOM selectors, navigation routes) into typed constant files, we achieve:

- **Single source of truth** - Update selectors, URLs, or routes in one place
- **Readable tests** - `ALERT_SELECTORS.container` is self-documenting; `.alert-display` is not
- **Type safety** - TypeScript catches typos and breaking changes at compile time
- **Maintainability** - When the app changes, constants change—not dozens of test files

## 📋 The Standard (Required)

**All E2E tests MUST follow this pattern:**

```
Test Spec → Test Helpers → Constants
```

- **Test specs** (`.cy.ts` files) describe user scenarios using helper functions
- **Test helpers** orchestrate actions and assertions using constants
- **Constants** store all infrastructure: endpoints, selectors, routes, timeouts

**No test file should contain hardcoded values.** Period.

Examples:

✅ **Good**: `cy.visit(APP_ROUTES.devices)` or `navigateToDeviceView()`  
❌ **Bad**: `cy.visit('/devices')`

✅ **Good**: `cy.get(DEVICE_CARD_SELECTORS.powerButton)`  
❌ **Bad**: `cy.get('[data-testid="device-power-button"]')`

✅ **Good**: `cy.intercept(DEVICE_ENDPOINTS.FIND_DEVICES.method, DEVICE_ENDPOINTS.FIND_DEVICES.pattern, ...)`  
❌ **Bad**: `cy.intercept('GET', 'http://localhost:5168/devices*', ...)`

---

## 📁 Where to Find Constants

All constants live in `apps/teensyrom-ui-e2e/src/support/constants/`:

```
constants/
├── api.constants.ts        # API configuration
├── selector.constants.ts   # UI selectors
├── app-routes.constants.ts # Navigation paths
└── index.ts                # Barrel export
```

Import from test helpers or directly:
```typescript
import { navigateToDeviceView, DEVICE_CARD_SELECTORS, APP_ROUTES } from '../support/devices/test-helpers';
// or
import { APP_ROUTES } from '../support/constants/app-routes.constants';
```

---

## 🔌 api.constants.ts

**Purpose**: Centralize all API endpoint configuration for Cypress intercepts.

**Contains**:
- `API_CONFIG` - Base URL (`http://localhost:5168`—no /api prefix), timeout settings
- `DEVICE_ENDPOINTS` - Endpoint objects with `method` (GET/POST/DELETE) and `pattern` (URL with wildcards)
- `INTERCEPT_ALIASES` - Readable names for `cy.intercept().as()` aliases
- `createProblemDetailsResponse()` - Helper to build error responses matching ProblemDetails spec

**Key insight**: The API has no `/api` prefix—routes go directly to `localhost:5168/devices`, not `localhost:5168/api/devices`.

**Usage**:
```typescript
cy.intercept(
  DEVICE_ENDPOINTS.FIND_DEVICES.method,
  DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
  { body: [] }
).as(INTERCEPT_ALIASES.FIND_DEVICES);
```

---

## 🎨 selector.constants.ts

**Purpose**: Centralize all DOM selectors, CSS classes, and UI-related constants.

**Contains**:
- `ALERT_SELECTORS` - Alert container, message, dismiss button
- `DEVICE_VIEW_SELECTORS` - Device list, empty state, loading indicator
- `DEVICE_CARD_SELECTORS` - Card, power button, ID/port labels, status icons
- `BUSY_DIALOG_SELECTORS` - Dialog container, message
- `BUTTON_SELECTORS` - Refresh button, action buttons
- `CSS_CLASSES` - Reusable classes like `dimmed`, `unavailable`
- `CONSTANTS` - Timeouts, error text patterns

**Organized by component** to make finding the right selector obvious. Mix of `data-testid` attributes (preferred) and CSS class selectors (when needed).

**Usage**:
```typescript
cy.get(DEVICE_CARD_SELECTORS.card).first().within(() => {
  cy.get(DEVICE_CARD_SELECTORS.powerButton).click();
});

cy.get(ALERT_SELECTORS.container).should('be.visible');
```

---

## 🛣️ app-routes.constants.ts

**Purpose**: Centralize all navigation paths and route-related utilities.

**Contains**:
- `APP_ROUTES` - Route path constants: `root: '/'`, `devices: '/devices'`, `player: '/player'`
- `ROUTE_NAMES` - Logical names for routes (used in guards, breadcrumbs, etc.)
- `getRoute()` - Helper to normalize route strings

**Wrapped by test helpers** like `navigateToDeviceView()` that add pre-visit setup (clearing storage, etc.).

**Usage**:
```typescript
cy.visit(APP_ROUTES.devices);

// Or use helper (preferred):
navigateToDeviceView();
```

---

## ➕ Adding New Constants

**When you add a new feature**, update constants first, then write tests:

### New API Endpoint
1. Add to `DEVICE_ENDPOINTS`: `{ method: 'POST', pattern: 'url*' }`
2. Add to `INTERCEPT_ALIASES`: `NEW_FEATURE: 'newFeature'`

### New UI Component
1. Create selector group: `NEW_COMPONENT_SELECTORS = { container: '[data-testid="..."]', button: '...' }`
2. Organize by component type (keep related selectors together)

### New Route
1. Add to `APP_ROUTES`: `newFeature: '/new-feature'`
2. Optionally create navigation helper in `test-helpers.ts`

---

## 🚨 Enforcement

This is not optional. Tests that hardcode values will be rejected in code review. The pattern exists to keep tests maintainable as the codebase grows. When you're tempted to hardcode, ask: "Will this value ever change?" If yes, it belongs in constants.
