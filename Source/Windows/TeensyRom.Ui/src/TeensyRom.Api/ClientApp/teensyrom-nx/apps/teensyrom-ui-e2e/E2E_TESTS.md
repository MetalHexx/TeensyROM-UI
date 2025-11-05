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

**Fixtures** (`src/support/test-data/fixtures/devices.fixture.ts`):

- Pre-built, realistic device data for common scenarios
- Examples: `singleDevice`, `multipleDevices`, `noDevices`, `disconnectedDevice`
- Type-safe with `MockDeviceFixture` interface
- Reusable across multiple tests
- See [E2E_FIXTURES.md](./src/support/test-data/fixtures/E2E_FIXTURES.md) for detailed fixture reference

**Generators** (`src/support/test-data/generators/device.generators.ts`):

- Factory functions using `@faker-js/faker` for dynamic test data
- Create custom scenarios: `generateDevice()`, `generateDeviceWithState()`
- Useful for edge cases and property-based testing
- See [E2E_TEST_GENERATORS.md](./src/support/test-data/generators/E2E_TEST_GENERATORS.md) for generator reference

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

**Interceptors** (`src/support/interceptors/device.interceptors.ts`):

- Wrapper functions around `cy.intercept()` for consistent API mocking
- Handle request/response structure automatically
- Support error modes for failure testing
- Register Cypress aliases for test assertions
- See [E2E_INTERCEPTORS.md](./src/support/interceptors/E2E_INTERCEPTORS.md) for detailed interceptor reference

**Available Interceptors**:

- `interceptFindDevices()` - Device discovery (`GET /devices`)
- `interceptConnectDevice()` - Device connection (`POST /devices/{id}/connect`)
- `interceptDisconnectDevice()` - Device disconnection (`DELETE /devices/{id}`)
- `interceptPingDevice()` - Device health check (`GET /devices/{id}/ping`)
- `interceptGetDirectory()` - Directory listing (`GET /devices/{id}/storage/{type}/directory`)
- `interceptIndexStorage()` - Storage indexing (`POST /devices/{id}/storage/{type}/index`)
- `interceptLaunchFile()` - File launch (`POST /devices/{id}/storage/{type}/launch`)
- `interceptSaveFavorite()` - Save favorite (`POST /devices/{id}/favorites`)
- `interceptRemoveFavorite()` - Remove favorite (`DELETE /devices/{id}/favorites/{id}`)

**Wait Functions**: Each interceptor provides co-located `waitFor<EndpointName>()` functions for standardized timing control. See [E2E_INTERCEPTORS.md](./src/support/interceptors/E2E_INTERCEPTORS.md) for complete reference.

**Example Usage**:

```typescript
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';

beforeEach(() => {
  interceptFindDevices({ fixture: singleDevice });
  navigateToDeviceView();
  waitForFindDevices(); // Use co-located wait function
});

// Error mode example
it('should handle API errors', () => {
  interceptFindDevices({ errorMode: true });
  navigateToDeviceView();
  waitForFindDevices(); // Wait functions work with error mode too
  // Verify error UI displays
});
```

#### 3. E2E Tests (`src/e2e/devices/`)

**Test Helpers** (`src/e2e/devices/test-helpers.ts`):

- Centralized selectors (single source of truth)
- Reusable navigation and assertion functions
- Type-safe constants for routes, timeouts, CSS classes

**Constants** (`src/support/constants/`):

- See [E2E_CONSTANTS.md](./src/support/constants/E2E_CONSTANTS.md) for centralized constants reference

**Test Specs** (`src/e2e/devices/device-discovery.cy.ts`, etc.):

- Organized by feature/workflow
- Use descriptive test names
- Leverage helpers for consistency

**Example Test Structure**:

```typescript
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';

describe('Device Discovery', () => {
  beforeEach(() => {
    interceptFindDevices({ fixture: multipleDevices });
    navigateToDeviceView();
    waitForFindDevices(); // Use co-located wait function
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

# Run all E2E tests with JSON reporting (for agents)
pnpm nx e2e:report teensyrom-ui-e2e

# Run specific test file
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/devices/device-discovery.cy.ts"

# Run specific test file with JSON reporting
pnpm nx e2e:report teensyrom-ui-e2e --spec="src/e2e/app.cy.ts"

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

## 📊 Test Reporting

### JSON Reports for Agent Consumption

The E2E test suite includes **Mochawesome JSON reporting** for deterministic test result analysis. This is particularly useful for automated agents (like Claude Code) to parse test outcomes and fix issues programmatically.

### Running Tests with Reports

**Primary Command** (for agent use):

```bash
pnpm nx e2e:report teensyrom-ui-e2e
```

**Run Specific Spec**:

```bash
pnpm nx e2e:report teensyrom-ui-e2e --spec=src/e2e/app.cy.ts
pnpm nx e2e:report teensyrom-ui-e2e --spec=src/e2e/devices/device-discovery.cy.ts
```

**Note**: The `e2e:report` target automatically uses `serve-static` for faster startup and is optimized for headless test execution with JSON report generation.

### Report Output Location

Reports are generated in the Nx output directory:

```
dist/cypress/apps/teensyrom-ui-e2e/reports/
├── index.json              # Main merged JSON report with all test results
└── .jsons/                 # Individual test file reports (for multi-spec runs)
    └── mochawesome.json
```

**Primary Report**: `apps/teensyrom-ui-e2e/dist/cypress/apps/teensyrom-ui-e2e/reports/index.json`

### JSON Report Structure

The JSON report contains complete test execution details:

```json
{
  "stats": {
    "suites": 2,
    "tests": 10,
    "passes": 8,
    "failures": 2,
    "pending": 0,
    "start": "2025-01-15T10:30:00.000Z",
    "end": "2025-01-15T10:30:45.123Z",
    "duration": 45123
  },
  "results": [
    {
      "uuid": "...",
      "title": "Device Discovery",
      "fullFile": "src/e2e/devices/device-discovery.cy.ts",
      "file": "src/e2e/devices/device-discovery.cy.ts",
      "suites": [
        {
          "title": "Single Device",
          "tests": [
            {
              "title": "should display device name",
              "state": "passed",
              "speed": "fast",
              "duration": 1234,
              "pass": true,
              "fail": false,
              "pending": false,
              "code": "..."
            },
            {
              "title": "should display device connection status",
              "state": "failed",
              "speed": null,
              "duration": 2345,
              "pass": false,
              "fail": true,
              "pending": false,
              "err": {
                "message": "expected 'Disconnected' to equal 'Connected'",
                "estack": "AssertionError: expected 'Disconnected' to equal 'Connected'\n    at Context.eval (webpack://...)",
                "diff": "..."
              },
              "code": "..."
            }
          ]
        }
      ]
    }
  ]
}
```

### Using Reports for Debugging

**For Agents/Automation**:

1. Parse `stats` object for high-level test outcome (passes vs failures)
2. Iterate through `results[].suites[].tests[]` for individual test details
3. Check `test.state` field: `"passed"`, `"failed"`, or `"pending"`
4. For failures, examine `test.err.message` and `test.err.estack` for root cause
5. Use `test.fullFile` to identify which spec file contains the failure
6. Use `test.title` and parent suite titles for precise test location

**Example Agent Logic**:

```typescript
const report = JSON.parse(
  fs.readFileSync(
    'apps/teensyrom-ui-e2e/dist/cypress/apps/teensyrom-ui-e2e/reports/index.json',
    'utf8'
  )
);

if (report.stats.failures > 0) {
  report.results.forEach((result) => {
    result.suites?.forEach((suite) => {
      suite.tests?.forEach((test) => {
        if (test.fail) {
          console.log(`FAILED: ${result.file} - ${suite.title} - ${test.title}`);
          console.log(`Error: ${test.err.message}`);
          console.log(`Stack: ${test.err.estack}`);
        }
      });
    });
  });
}
```

### Reporter Configuration

The reporter is configured in `cypress.config.ts`:

- **JSON only**: HTML reports are disabled (agents don't need visual reports)
- **No charts**: Reduces report size and generation time
- **No embedded screenshots**: Screenshots remain separate for debugging
- **Deterministic output**: Same test run produces identical JSON structure

---

## 🔍 Advanced Troubleshooting

### Chrome DevTools MCP Server

For deep debugging of interceptor timing issues, network traffic, and application state during test execution, you can use the **Chrome DevTools Model Context Protocol (MCP)** server.

**What It Provides**:

- Real-time network request inspection during Cypress tests
- Console log monitoring during test execution
- DOM state verification at specific test breakpoints
- JavaScript execution in page context to inspect signals/stores

**Setup Guide**: See `src/support/constants/E2E_CONSTANTS.md`

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
cy.get('.alert-display').should('be.visible');
cy.get('[data-testid="device-card"]').should('have.length', 5);

// ✅ Correct - uses centralized selector constants
cy.get(ALERT_SELECTORS.container).should('be.visible');
cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 5);
```

**Error Responses** - Use `createProblemDetailsResponse()` helper:

```typescript
// ❌ Verbose & repetitive - not allowed
req.reply({
  statusCode: 404,
  headers: { 'content-type': 'application/problem+json' },
  body: { type: '...', title: msg, status: 404 },
});

// ✅ Correct - uses generic helper
req.reply(createProblemDetailsResponse(404, errorMessage));
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

### Wait Function Patterns

**Co-located Wait Functions** - Import wait functions directly from interceptor files:

```typescript
// ❌ Old pattern - wait functions in separate files
import { interceptFindDevices } from '../support/interceptors';
import { waitForDeviceDiscovery } from '../e2e/devices/test-helpers';

// ✅ Current pattern - wait functions co-located with interceptors
import {
  interceptFindDevices,
  waitForFindDevices,
  waitForFindDevicesToStart, // For race condition testing
} from '../support/interceptors/findDevices.interceptors';
```

**Unified Naming Convention** - All wait functions follow `waitFor<EndpointName>` pattern:

```typescript
// Standard waits (wait for completion)
waitForFindDevices();
waitForConnectDevice();
waitForLaunchFile();

// Race condition variants (wait for start, not completion)
waitForFindDevicesToStart();
waitForConnectDeviceToStart();
waitForLaunchFileToStart();
```

**Import Strategy** - Direct imports from interceptor files (no barrel exports):

```typescript
// Import both interceptor and its wait functions from same file
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';

import {
  interceptLaunchFile,
  waitForLaunchFile,
} from '../support/interceptors/launchFile.interceptors';
```

**Benefits**:

- ✅ **Co-location** - Wait functions located with their interceptors
- ✅ **Unified naming** - Consistent `waitFor<EndpointName>` pattern
- ✅ **Race condition testing** - `*ToStart()` variants for precise timing
- ✅ **Type safety** - Uses same endpoint definitions as interceptors
- ✅ **Maintainability** - Changes to interceptors automatically reflected in wait functions

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
- Use co-located wait functions for timing control
- Import wait functions directly from interceptor files (not from barrel exports)

### Test Naming

- Be descriptive: `should display device name` not `test 1`
- Use BDD style: `should [expected behavior] when [condition]`
- Group related tests in `describe` blocks

---

## 📖 Additional Documentation

### Phase Documentation

Detailed implementation guides for each testing phase:

- **Phase 2**: Fixtures & Generators - [E2E_FIXTURES.md](./src/support/test-data/fixtures/E2E_FIXTURES.md)
- **Phase 3**: API Interceptors & Wait Functions - [E2E_INTERCEPTORS.md](./src/support/interceptors/E2E_INTERCEPTORS.md) _(Updated with wait helper functions)_
- **Phase 4**: Device Discovery Tests - [E2E_DEVICE_DISCOVERY.md](./src/e2e/devices/E2E_DEVICE_DISCOVERY.md)
- **Constants & Selectors**: [E2E_CONSTANTS.md](./src/support/constants/E2E_CONSTANTS.md)

### Related Guides

- **Test Data Generators**: [E2E_TEST_GENERATORS.md](./src/support/test-data/generators/E2E_TEST_GENERATORS.md)
- **API Client Generation**: `../../docs/API_CLIENT_GENERATION.md`
- **Testing Standards**: `../../docs/TESTING_STANDARDS.md`

---

## ✅ Quick Reference

**Run Tests**: `pnpm nx e2e teensyrom-ui-e2e`
**Run Tests with JSON Reports**: `pnpm nx e2e:report teensyrom-ui-e2e`
**Debug Tests**: `pnpm nx e2e teensyrom-ui-e2e:open-cypress`
**Test Reports**: `apps/teensyrom-ui-e2e/dist/cypress/apps/teensyrom-ui-e2e/reports/index.json`
**API Spec**: `../../TeensyRom.Api/api-spec/TenesyRom.Api.json`
**Fixtures**: `src/support/test-data/fixtures/`
**Generators**: `src/support/test-data/generators/`
**Interceptors**: `src/support/interceptors/`
**Wait Functions**: Co-located in interceptor files (e.g., `waitForFindDevices()` in `findDevices.interceptors.ts`)
**Constants**: `src/support/constants/`
**Test Helpers**: `src/e2e/devices/test-helpers.ts`
**Test Specs**: `src/e2e/devices/`

**Current Status**: ✅ 33/33 active tests passing | 6 tests pending (Phase 5)
