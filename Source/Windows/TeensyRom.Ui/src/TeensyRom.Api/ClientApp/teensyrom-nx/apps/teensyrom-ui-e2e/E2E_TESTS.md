# E2E Test Suite Overview

## 📋 Purpose

This E2E test suite validates the TeensyROM UI using Cypress with a **fixture-driven, interceptor-based** approach. Tests run independently of the backend API by mocking HTTP responses with realistic device data.

**Key Benefits**:
- ✅ Deterministic tests (no flakiness from real API)
- ✅ Fast execution (no network latency)
- ✅ Isolated scenarios (test specific device states)
- ✅ No backend required during development

---

## 🏗️ Architecture

### Three-Layer Testing Pattern

```
Test Data Layer (Fixtures & Generators)
    ↓
API Mocking Layer (Interceptors)
    ↓
E2E Tests (Cypress Specs)
```

#### 1. Test Data Layer (`src/support/test-data/`)

**Fixtures** (`fixtures/devices.fixture.ts`):
- Pre-built, realistic device data for common scenarios
- Examples: `singleDevice`, `multipleDevices`, `noDevices`, `disconnectedDevice`
- Type-safe with `MockDeviceFixture` interface
- Reusable across multiple tests

**Generators** (`generators/device.generators.ts`):
- Factory functions using `@faker-js/faker` for dynamic test data
- Create custom scenarios: `generateDevice()`, `generateDeviceWithState()`
- Useful for edge cases and property-based testing

**Example Usage**:
```typescript
import { singleDevice, multipleDevices } from '../support/test-data/fixtures';
import { generateDevice } from '../support/test-data/generators';

// Use pre-built fixture
interceptFindDevices({ fixture: singleDevice });

// Or generate custom device
const customDevice = generateDevice({ firmwareVersion: 'v2.0.0' });
```

#### 2. API Mocking Layer (`src/support/interceptors/`)

**Interceptors** (`interceptors/device.interceptors.ts`):
- Wrapper functions around `cy.intercept()` for consistent API mocking
- Handle request/response structure automatically
- Support error modes for failure testing
- Register Cypress aliases for test assertions

**Available Interceptors**:
- `interceptFindDevices()` - Device discovery (`GET /devices`)
- `interceptConnectDevice()` - Device connection (`POST /devices/{id}/connect`)
- `interceptDisconnectDevice()` - Device disconnection (`DELETE /devices/{id}`)
- `interceptPingDevice()` - Device health check (`GET /devices/{id}/ping`)

**Example Usage**:
```typescript
import { interceptFindDevices } from '../support/interceptors';

beforeEach(() => {
  interceptFindDevices({ fixture: singleDevice });
  navigateToDeviceView();
  waitForDeviceDiscovery();
});

// Error mode example
it('should handle API errors', () => {
  interceptFindDevices({ errorMode: true });
  navigateToDeviceView();
  // Verify error UI displays
});
```

#### 3. E2E Tests (`src/e2e/devices/`)

**Test Helpers** (`test-helpers.ts`):
- Centralized selectors (single source of truth)
- Reusable navigation and assertion functions
- Type-safe constants for routes, timeouts, CSS classes

**Test Specs** (`device-discovery.cy.ts`, etc.):
- Organized by feature/workflow
- Use descriptive test names
- Leverage helpers for consistency

**Example Test Structure**:
```typescript
describe('Device Discovery', () => {
  beforeEach(() => {
    interceptFindDevices({ fixture: multipleDevices });
    navigateToDeviceView();
    waitForDeviceDiscovery();
  });

  it('should display all devices', () => {
    verifyDeviceCount(3);
  });

  it('should show device information', () => {
    verifyFullDeviceInfo(0);
  });
});
```

---

## 🧪 Current Test Coverage

### Device Discovery Test Suite (`device-discovery.cy.ts`)

**Total Tests**: 39 tests
- **Passing**: 33 active tests
- **Pending**: 6 tests (skipped for Phase 5 - SignalR event mocking)
- **Duration**: ~13 seconds

**Test Scenarios**:
1. **Single Device Discovery** - Display single device with all information
2. **Multiple Devices** - Display multiple devices with unique data
3. **Empty State** - No devices found UI
4. **Disconnected Device** - Device with lost connection
5. **Unavailable Storage** - Device with storage issues
6. **Mixed Device States** - Multiple devices in different states simultaneously
7. **Loading States** - Bootstrap busy dialog during API calls
8. **Error Handling** - API failure scenarios

**Pending Tests** (SignalR Required):
- Device state display tests require SignalR hub mocking
- Will be implemented in Phase 5 with `device-events.interceptors.ts`

### Device View Navigation Test Suite (`device-view-navigation.cy.ts`)

Basic routing and navigation validation.

---

## 🔧 Running Tests

### Command Reference

```bash
# Run all E2E tests
pnpm nx e2e teensyrom-ui-e2e

# Run specific test file
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/device-discovery.cy.ts"

# Run in headed mode (see browser)
pnpm nx e2e teensyrom-ui-e2e --headed --browser=chrome

# Open Cypress Test Runner UI
pnpm nx e2e teensyrom-ui-e2e:open-cypress

# Run with specific grep pattern
pnpm nx e2e teensyrom-ui-e2e --grep="Loading States"
```

### Debugging Tests

**Screenshots**: Auto-captured on failure in `dist/cypress/apps/teensyrom-ui-e2e/screenshots/`

**Cypress Test Runner**: Use `open-cypress` for interactive debugging with time-travel

**Console Logs**: Available in browser DevTools during headed mode execution

---

## 🔍 Advanced Troubleshooting

### Chrome DevTools MCP Server

For deep debugging of interceptor timing issues, network traffic, and application state during test execution, you can use the **Chrome DevTools Model Context Protocol (MCP)** server.

**What It Provides**:
- Real-time network request inspection during Cypress tests
- Console log monitoring during test execution
- DOM state verification at specific test breakpoints
- JavaScript execution in page context to inspect signals/stores

**Setup Guide**: See `docs/features/e2e-testing/E2E_PLAN_P4_MCP_SETUP.md`

**Quick Setup**:
1. Install MCP server: `npm install -g @modelcontextprotocol/server-chrome-devtools`
2. Launch Chrome with debugging: `chrome.exe --remote-debugging-port=9222`
3. Run tests in headed Chrome mode
4. Connect via VS Code Copilot: `@chrome connect to localhost:9222`

**Common Debug Commands**:
```
@chrome get network requests       # See all API calls
@chrome get console logs           # See application logs
@chrome execute script: window.X   # Inspect application state
@chrome take screenshot            # Capture current state
```

**Use Cases**:
- Investigate why `cy.wait('@alias')` times out
- Verify interceptor patterns match actual API calls
- Check if API calls happen before interceptor registration
- Inspect Angular store state during test execution

---

## 📚 API Reference & Troubleshooting

### OpenAPI Specification

The backend API specification is the source of truth for endpoint structures, request/response formats, and available operations.

**Location**: `TeensyRom.Api/api-spec/TeensyRom.Api.json`

**When to Consult**:
- ✅ Interceptor pattern doesn't match actual API calls
- ✅ Response structure differs from expectations
- ✅ New endpoints added to backend need E2E coverage
- ✅ API contract changes require test updates

**Key Information**:
- **Base Path**: `http://localhost:5168` (cross-origin from Angular app at `localhost:4200`)
- **Endpoint Paths**: No `/api` prefix (e.g., `/devices`, not `/api/devices`)
- **Response Schemas**: Detailed type definitions for all DTOs
- **HTTP Methods**: GET, POST, DELETE patterns

**Common Pitfall**:
```typescript
// ❌ Incorrect - assumes /api prefix
cy.intercept('GET', '/api/devices*', ...)

// ✅ Correct - use full URL for cross-origin
cy.intercept('GET', 'http://localhost:5168/devices*', ...)
```

### Generated API Client

The TypeScript API client is auto-generated from the OpenAPI spec and provides type-safe service definitions.

**Location**: `libs/data-access/api-client/src/lib/apis/DevicesApiService.ts`

**Use For**:
- Verifying exact endpoint signatures
- Understanding request/response types
- Checking query parameters
- Identifying HTTP methods

**Regeneration** (when API changes):
```bash
# From backend project
dotnet build TeensyRom.Api.csproj

# From Angular workspace
pnpm run generate:api-client
```

---

## 🎯 Best Practices

### Constants & Selectors Requirement

All E2E tests **MUST** use centralized constants from the constants layer - hardcoding values is strictly prohibited.

**API Endpoints** - Use `DEVICE_ENDPOINTS` from `api.constants.ts`:
```typescript
// ❌ Hardcoded - not allowed
cy.intercept('GET', 'http://localhost:5168/devices*', ...)

// ✅ Correct - uses centralized constant
cy.intercept(DEVICE_ENDPOINTS.FIND_DEVICES.method, DEVICE_ENDPOINTS.FIND_DEVICES.pattern, ...)
```

**UI Selectors** - Use selector groups from `selector.constants.ts`:
```typescript
// ❌ Hardcoded - not allowed
cy.get('.alert-display').should('be.visible')
cy.get('[data-testid="device-card"]').should('have.length', 5)

// ✅ Correct - uses centralized selector constants
cy.get(ALERT_SELECTORS.container).should('be.visible')
cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 5)
```

**Error Responses** - Use `createProblemDetailsResponse()` helper:
```typescript
// ❌ Verbose & repetitive - not allowed
req.reply({
  statusCode: 404,
  headers: { 'content-type': 'application/problem+json' },
  body: { type: '...', title: msg, status: 404 }
})

// ✅ Correct - uses generic helper
req.reply(createProblemDetailsResponse(404, errorMessage))
```

**Benefits**:
- ✅ Single source of truth - change once, affects all tests
- ✅ Maintainability - selectors/endpoints update automatically
- ✅ Type safety - constants are typed, catch breakage early
- ✅ Consistency - all tests follow same pattern
- ✅ Reduced duplication - no copy-paste errors

**Re-export Pattern** - Import from test-helpers for convenience:
```typescript
import {
  DEVICE_ENDPOINTS,
  ALERT_SELECTORS,
  DEVICE_CARD_SELECTORS,
  createProblemDetailsResponse,
} from './test-helpers';
```

### Test Independence
- Each test sets up its own interceptors and state
- Use `beforeEach` for common setup
- Clear storage in `navigateToDeviceView()` helper
- Never depend on execution order

### Selector Strategy
1. **Preferred**: `data-testid` attributes (e.g., `[data-testid="device-card"]`)
2. **Acceptable**: Semantic HTML elements (`button`, `nav`)
3. **Avoid**: CSS classes (brittle, change with styling)

### Fixture Management
- Use existing fixtures for common scenarios
- Create new fixtures for reusable edge cases
- Use generators for one-off custom scenarios
- Keep fixtures in sync with backend DTOs

### Interceptor Patterns
- Always use full URLs for cross-origin requests
- Register interceptors before navigation (`beforeEach`)
- Use descriptive aliases (`@findDevices`, not `@api`)
- Support both success and error modes

### Test Naming
- Be descriptive: `should display device name` not `test 1`
- Use BDD style: `should [expected behavior] when [condition]`
- Group related tests in `describe` blocks

---

## 📖 Additional Documentation

### Phase Documentation
Detailed implementation guides for each testing phase:

- **Phase 2**: Fixtures & Generators - `src/support/test-data/fixtures/README.md`
- **Phase 3**: API Interceptors - `src/support/interceptors/E2E_INTERCEPTORS.md`
- **Phase 4**: Device Discovery Tests - `src/e2e/devices/E2E_DEVICE_DISCOVERY.md`
- **Phase 4 Results**: Timing Issues Resolution - `docs/features/e2e-testing/E2E_PLAN_P4_RESULTS.md`

### Related Guides
- **Overall E2E Plan**: `docs/features/e2e-testing/E2E_PLAN.md`
- **Testing Standards**: `docs/TESTING_STANDARDS.md`
- **API Client Generation**: `docs/API_CLIENT_GENERATION.md`

---

## ✅ Quick Reference

**Run Tests**: `pnpm nx e2e teensyrom-ui-e2e`  
**Debug Tests**: `pnpm nx e2e teensyrom-ui-e2e:open-cypress`  
**API Spec**: `TeensyRom.Api/api-spec/TeensyRom.Api.json`  
**Fixtures**: `src/support/test-data/fixtures/`  
**Interceptors**: `src/support/interceptors/`  
**Test Helpers**: `src/e2e/devices/test-helpers.ts`  
**MCP Setup**: `docs/features/e2e-testing/E2E_PLAN_P4_MCP_SETUP.md`

**Current Status**: ✅ 33/33 active tests passing | 6 tests pending (Phase 5)
