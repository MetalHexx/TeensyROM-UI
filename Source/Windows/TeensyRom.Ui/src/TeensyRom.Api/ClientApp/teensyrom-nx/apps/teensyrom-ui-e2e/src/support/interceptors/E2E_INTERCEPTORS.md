# Device API Interceptors

## Purpose

Device API Interceptors provide functions that mock TeensyROM device endpoints using Cypress's `cy.intercept()` API. These interceptors consume Phase 2 fixtures to return realistic device discovery, connection, and state responses without requiring a live backend server.

Interceptors bridge static test data (fixtures) with dynamic test scenarios by:

- Handling HTTP route matching and response structure
- Supporting error modes for testing failure scenarios
- Registering named aliases for test assertions
- Allowing custom fixtures to override defaults

**Key Benefit**: Test authors can now write realistic device workflow tests by calling simple interceptor functions, with all backend API behavior mocked.

---

## Usage Pattern

### Basic Usage

```typescript
import { cy } from 'cypress';
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';

describe('Device Discovery', () => {
  it('should display discovered devices', () => {
    // Register mock API interceptor with default fixture
    interceptFindDevices();

    cy.visit('/devices');

    // Wait for intercepted request using co-located wait function
    waitForFindDevices();
    cy.get('[data-testid="device-list"]').should('be.visible');
  });
});
```

### With Custom Fixture

```typescript
import { multipleDevices } from '../support/test-data/fixtures';
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';

describe('Device Discovery', () => {
  it('should handle multiple devices', () => {
    // Override default fixture with custom scenario
    interceptFindDevices({ fixture: multipleDevices });

    cy.visit('/devices');
    waitForFindDevices();
    cy.get('[data-testid="device-item"]').should('have.length', 3);
  });
});
```

### Error Mode Testing

```typescript
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';

describe('Device Discovery', () => {
  it('should handle discovery errors', () => {
    // Simulate API error
    interceptFindDevices({ errorMode: true });

    cy.visit('/devices');
    waitForFindDevices();
    cy.get('[data-testid="error-message"]').should('contain', 'failed');
  });
});
```

---

## Interceptor Functions

All device interceptors follow the same consistent pattern:

- Named function: `intercept[Action]`
- Options parameter with defaults
- Cypress route matching
- Alias registration
- Error mode support

### interceptFindDevices()

Intercepts `GET /api/devices` - Device discovery endpoint.

```typescript
function interceptFindDevices(options?: InterceptFindDevicesOptions): void;
```

**Parameters:**

- `fixture?`: Override default `singleDevice` fixture
- `errorMode?`: Return 500 error when true

**Alias**: `@findDevices`

**Default Response**: `singleDevice` fixture wrapped in `FindDevicesResponse` structure

**Error Response**: HTTP 500 with `ProblemDetails` body

### interceptConnectDevice()

Intercepts `POST /api/devices/{deviceId}/connect` - Device connection endpoint.

```typescript
function interceptConnectDevice(options?: InterceptConnectDeviceOptions): void;
```

**Parameters:**

- `device?`: Override default device (first device from `singleDevice` fixture)
- `errorMode?`: Return 500 error when true

**Alias**: `@connectDevice`

**Route Pattern**: `POST /api/devices/*/connect` (wildcard for any deviceId)

**Default Response**: First device from `singleDevice` wrapped in `ConnectDeviceResponse` structure

**Error Response**: HTTP 500 with `ProblemDetails` body

### interceptDisconnectDevice()

Intercepts `DELETE /api/devices/{deviceId}` - Device disconnection endpoint.

```typescript
function interceptDisconnectDevice(options?: InterceptDisconnectDeviceOptions): void;
```

**Parameters:**

- `errorMode?`: Return 500 error when true

**Alias**: `@disconnectDevice`

**Route Pattern**: `DELETE /api/devices/*` (wildcard for any deviceId)

**Default Response**: Success message wrapped in `DisconnectDeviceResponse` structure

**Error Response**: HTTP 500 with `ProblemDetails` body

### interceptPingDevice()

Intercepts `GET /api/devices/{deviceId}/ping` - Device health check endpoint.

```typescript
function interceptPingDevice(options?: InterceptPingDeviceOptions): void;
```

**Parameters:**

- `isAlive?`: Return success (true, default) or failure (false) response
- `errorMode?`: Return 500 error when true

**Alias**: `@pingDevice`

**Route Pattern**: `GET /api/devices/*/ping` (wildcard for any deviceId)

**Default Response**: Success response indicating device is alive

**Alive=false Response**: Failure response indicating device not responding

**Error Response**: HTTP 500 with `ProblemDetails` body

---

## Options Pattern

All interceptors use consistent options pattern with sensible defaults:

```typescript
interface InterceptFindDevicesOptions {
  fixture?: MockDeviceFixture;
  errorMode?: boolean;
}

interface InterceptConnectDeviceOptions {
  device?: CartDto;
  errorMode?: boolean;
}

interface InterceptDisconnectDeviceOptions {
  errorMode?: boolean;
}

interface InterceptPingDeviceOptions {
  isAlive?: boolean;
  errorMode?: boolean;
}
```

**Key Conventions:**

- All options are optional with sensible defaults
- `errorMode: true` always returns HTTP 500 status
- Custom parameters (`fixture`, `device`, `isAlive`) override defaults
- When both `errorMode` and custom parameter provided, `errorMode` takes precedence

---

## Alias Conventions

All interceptors register named aliases matching their function names for test assertions:

| Function                      | Alias               | Usage                          |
| ----------------------------- | ------------------- | ------------------------------ |
| `interceptFindDevices()`      | `@findDevices`      | `cy.wait('@findDevices')`      |
| `interceptConnectDevice()`    | `@connectDevice`    | `cy.wait('@connectDevice')`    |
| `interceptDisconnectDevice()` | `@disconnectDevice` | `cy.wait('@disconnectDevice')` |
| `interceptPingDevice()`       | `@pingDevice`       | `cy.wait('@pingDevice')`       |

**Usage Example:**

```typescript
interceptFindDevices();
cy.visit('/devices');

// Wait for intercepted request
cy.wait('@findDevices').then((interception) => {
  // Assert on intercepted request
  expect(interception.request.url).to.include('/api/devices');
  expect(interception.response.statusCode).to.equal(200);

  // Assert on response body
  expect(interception.response.body.devices).to.have.length.greaterThan(0);
});
```

---

## Error Mode

All interceptors support `errorMode: true` to simulate backend failures:

```typescript
// Simulate device discovery failure
interceptFindDevices({ errorMode: true });

// Simulate connection failure
interceptConnectDevice({ errorMode: true });

// Simulate disconnection failure
interceptDisconnectDevice({ errorMode: true });

// Simulate ping failure (server error)
interceptPingDevice({ errorMode: true });
```

When `errorMode: true`, interceptor returns:

- **HTTP Status**: 500 Internal Server Error
- **Response Body**: `ProblemDetails` structure
  ```typescript
  {
    type: 'https://api.example.com/errors/internal-error',
    title: 'Internal Server Error',
    status: 500,
    detail: 'Device API call failed'
  }
  ```

**Exception**: `interceptPingDevice()` distinguishes between:

- `isAlive: false` - Device not responding (200 status, failure message)
- `errorMode: true` - Server error (500 status)

---

## Fixture Reference

Interceptors consume Phase 2 fixtures. Available fixtures:

### singleDevice

Single connected device with available storage (most common scenario).

```typescript
import { singleDevice } from '../support/test-data/fixtures';
import { interceptFindDevices } from '../support/interceptors';

interceptFindDevices({ fixture: singleDevice });
```

### multipleDevices

Three connected devices with unique identification.

```typescript
import { multipleDevices } from '../support/test-data/fixtures';

interceptFindDevices({ fixture: multipleDevices });
```

### noDevices

Empty device list - no devices found.

```typescript
import { noDevices } from '../support/test-data/fixtures';

interceptFindDevices({ fixture: noDevices });
```

See `test-data/fixtures/README.md` for complete fixture documentation.

---

## Route Matching

Interceptors handle URL pattern matching automatically:

### Exact Routes

```typescript
// GET /api/devices
interceptFindDevices();
```

### Wildcard Routes

Interceptors with deviceId parameters use wildcards:

```typescript
// POST /api/devices/{deviceId}/connect → POST /api/devices/*/connect
interceptConnectDevice();

// DELETE /api/devices/{deviceId} → DELETE /api/devices/*
interceptDisconnectDevice();

// GET /api/devices/{deviceId}/ping → GET /api/devices/*/ping
interceptPingDevice();
```

Cypress automatically matches any deviceId value with the `*` wildcard.

---

## Response Structure

All interceptors return responses matching the generated API client types:

### FindDevicesResponse

```typescript
{
  devices: CartDto[]
}
```

### ConnectDeviceResponse

```typescript
{
  device: CartDto;
}
```

### DisconnectDeviceResponse

```typescript
{
  message: string;
}
```

### PingDeviceResponse

```typescript
{
  isAlive: boolean;
}
```

### ProblemDetails (Error Response)

```typescript
{
  type: string;
  title: string;
  status: number;
  detail: string;
}
```

---

## Common Scenarios

### Device Discovery Workflow

```typescript
// Scenario: User discovers and connects to a device
describe('Device Connection Flow', () => {
  it('should discover and connect to device', () => {
    // Mock device discovery
    interceptFindDevices();

    // Mock device connection
    interceptConnectDevice();

    cy.visit('/devices');
    cy.wait('@findDevices');

    cy.get('[data-testid="device-item"]').first().click();
    cy.wait('@connectDevice');

    cy.get('[data-testid="connected-indicator"]').should('be.visible');
  });
});
```

### Multiple Devices Scenario

```typescript
// Scenario: User selects from multiple devices
describe('Multiple Devices', () => {
  it('should display and allow selection of multiple devices', () => {
    interceptFindDevices({ fixture: multipleDevices });
    interceptConnectDevice();

    cy.visit('/devices');
    cy.wait('@findDevices');

    cy.get('[data-testid="device-item"]').should('have.length', 3);
    cy.get('[data-testid="device-item"]').eq(1).click();

    cy.wait('@connectDevice');
  });
});
```

### Error Handling Scenario

```typescript
// Scenario: Handle device discovery failure
describe('Error Handling', () => {
  it('should display error when device discovery fails', () => {
    interceptFindDevices({ errorMode: true });

    cy.visit('/devices');
    cy.wait('@findDevices');

    cy.get('[data-testid="error-banner"]').should('be.visible');
    cy.get('[data-testid="error-message"]').should('contain', 'discovery failed');
  });
});
```

### Device Health Check Scenario

```typescript
// Scenario: Ping device to verify it's responsive
describe('Device Health Check', () => {
  it('should confirm device is alive', () => {
    interceptFindDevices();
    interceptConnectDevice();
    interceptPingDevice({ isAlive: true });

    cy.visit('/devices');
    cy.wait('@findDevices');

    cy.get('[data-testid="device-item"]').first().click();
    cy.wait('@connectDevice');

    cy.get('[data-testid="health-check-btn"]').click();
    cy.wait('@pingDevice');

    cy.get('[data-testid="device-status"]').should('contain', 'Alive');
  });
});
```

---

## Best Practices

1. **Register interceptors before navigation**: Always call interceptor functions before `cy.visit()` or API calls

   ```typescript
   interceptFindDevices(); // ✅ Do this first
   cy.visit('/devices'); // Then navigate
   ```

2. **Use specific fixtures for scenarios**: Don't rely on defaults for non-trivial tests

   ```typescript
   // ❌ Less clear what you're testing
   interceptFindDevices();

   // ✅ Clear test intent
   interceptFindDevices({ fixture: multipleDevices });
   ```

3. **Chain related interceptors**: Register all mocks for a workflow together

   ```typescript
   // ✅ Clear workflow intent
   interceptFindDevices();
   interceptConnectDevice();
   interceptDisconnectDevice();

   // Then test the full workflow
   ```

4. **Wait for aliases**: Always use `cy.wait()` with aliases to ensure requests complete

   ```typescript
   interceptFindDevices();
   cy.visit('/devices');
   cy.wait('@findDevices'); // ✅ Ensures request completed

   cy.get('[data-testid="device-list"]').should('be.visible');
   ```

5. **Test both success and error**: Always test error scenarios
   ```typescript
   describe('Device Discovery', () => {
     it('should display devices', () => {
       interceptFindDevices();
       // ... success test
     });

     it('should handle discovery errors', () => {
       interceptFindDevices({ errorMode: true });
       // ... error test
     });
   });
   ```

---

## Wait Helper Functions

Wait helper functions are co-located with their corresponding interceptors and provide standardized timing control for API calls. All wait functions follow the `waitFor<EndpointName>` naming convention for consistency and discoverability.

### Standard Wait Functions

Each interceptor exports a standard `waitFor<EndpointName>()` function that waits for the API call to complete:

```typescript
import { interceptFindDevices, waitForFindDevices } from './interceptors/findDevices.interceptors';

// Standard usage pattern
beforeEach(() => {
  interceptFindDevices({ fixture: multipleDevices });
  navigateToDeviceView();
  waitForFindDevices(); // Wait for request completion
});
```

### Race Condition Testing Variants

For precise timing control and race condition testing, each interceptor also exports a `waitFor<EndpointName>ToStart()` variant:

```typescript
import {
  interceptFindDevices,
  waitForFindDevicesToStart,
} from './interceptors/findDevices.interceptors';

// Race condition testing - wait for request to start (not complete)
it('should handle UI state during API call', () => {
  interceptFindDevices({ responseDelayMs: 2000 });
  navigateToDeviceView();

  // Wait for API call to start, then test loading states
  waitForFindDevicesToStart();
  cy.get('[data-testid="loading-indicator"]').should('be.visible');

  // Then wait for completion
  waitForFindDevices();
  cy.get('[data-testid="device-list"]').should('be.visible');
});
```

### Available Wait Functions

All interceptors provide these wait functions:

| Interceptor File                   | Standard Wait Function      | Race Condition Variant             |
| ---------------------------------- | --------------------------- | ---------------------------------- |
| `findDevices.interceptors.ts`      | `waitForFindDevices()`      | `waitForFindDevicesToStart()`      |
| `connectDevice.interceptors.ts`    | `waitForConnectDevice()`    | `waitForConnectDeviceToStart()`    |
| `disconnectDevice.interceptors.ts` | `waitForDisconnectDevice()` | `waitForDisconnectDeviceToStart()` |
| `pingDevice.interceptors.ts`       | `waitForPingDevice()`       | `waitForPingDeviceToStart()`       |
| `getDirectory.interceptors.ts`     | `waitForGetDirectory()`     | `waitForGetDirectoryToStart()`     |
| `indexStorage.interceptors.ts`     | `waitForIndexStorage()`     | `waitForIndexStorageToStart()`     |
| `indexAllStorage.interceptors.ts`  | `waitForIndexAllStorage()`  | `waitForIndexAllStorageToStart()`  |
| `launchFile.interceptors.ts`       | `waitForLaunchFile()`       | `waitForLaunchFileToStart()`       |
| `launchRandom.interceptors.ts`     | `waitForLaunchRandom()`     | `waitForLaunchRandomToStart()`     |
| `player.interceptors.ts`           | `waitForPlayer()`           | `waitForPlayerToStart()`           |
| `saveFavorite.interceptors.ts`     | `waitForSaveFavorite()`     | `waitForSaveFavoriteToStart()`     |
| `removeFavorite.interceptors.ts`   | `waitForRemoveFavorite()`   | `waitForRemoveFavoriteToStart()`   |

### Import Patterns

**Direct Import from Interceptor Files** (Recommended):

```typescript
// Import both interceptor and wait function from same file
import {
  interceptFindDevices,
  waitForFindDevices,
  waitForFindDevicesToStart,
} from '../support/interceptors/findDevices.interceptors';

// Import multiple interceptors and their wait functions
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';
import {
  interceptConnectDevice,
  waitForConnectDevice,
} from '../support/interceptors/connectDevice.interceptors';
```

**Barrel Import Pattern** (From index.ts):

```typescript
import {
  interceptFindDevices,
  interceptConnectDevice,
  // Note: Wait functions are NOT exported from barrel - import directly from interceptor files
} from '../support/interceptors';

// Still need to import wait functions directly
import { waitForFindDevices } from '../support/interceptors/findDevices.interceptors';
import { waitForConnectDevice } from '../support/interceptors/connectDevice.interceptors';
```

### Usage Examples

**Basic Test Setup**:

```typescript
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';
import {
  interceptConnectDevice,
  waitForConnectDevice,
} from '../support/interceptors/connectDevice.interceptors';

describe('Device Connection', () => {
  beforeEach(() => {
    interceptFindDevices({ fixture: singleDevice });
    interceptConnectDevice();
    navigateToDeviceView();
    waitForFindDevices();
  });

  it('should connect to device', () => {
    cy.get('[data-testid="device-item"]').first().click();
    waitForConnectDevice();
    cy.get('[data-testid="connected-status"]').should('be.visible');
  });
});
```

**Loading State Testing**:

```typescript
import {
  interceptFindDevices,
  waitForFindDevicesToStart,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';

describe('Loading States', () => {
  it('should show loading indicator during device discovery', () => {
    interceptFindDevices({ responseDelayMs: 2000 });
    navigateToDeviceView();

    // Wait for API call to start, then test loading state
    waitForFindDevicesToStart();
    cy.get('[data-testid="loading-indicator"]').should('be.visible');

    // Wait for completion and verify final state
    waitForFindDevices();
    cy.get('[data-testid="device-list"]').should('be.visible');
    cy.get('[data-testid="loading-indicator"]').should('not.exist');
  });
});
```

**Multiple API Calls**:

```typescript
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';
import {
  interceptGetDirectory,
  waitForGetDirectory,
} from '../support/interceptors/getDirectory.interceptors';

describe('File Browser Workflow', () => {
  it('should browse device storage', () => {
    interceptFindDevices({ fixture: singleDevice });
    interceptGetDirectory({ fixture: mockDirectory });

    navigateToDeviceView();
    waitForFindDevices();

    cy.get('[data-testid="device-item"]').first().click();
    cy.get('[data-testid="browse-storage-btn"]').click();
    waitForGetDirectory();

    cy.get('[data-testid="file-list"]').should('be.visible');
  });
});
```

### Migration Changes

**Previous Pattern** (Wait functions in separate files):

```typescript
// ❌ Old pattern - wait functions in separate test-helpers files
import { interceptFindDevices } from '../support/interceptors';
import { waitForDeviceDiscovery } from '../e2e/devices/test-helpers';
```

**Current Pattern** (Wait functions co-located with interceptors):

```typescript
// ✅ Current pattern - wait functions in interceptor files
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';
```

### Key Benefits

1. **Unified Naming Convention**: All wait functions follow `waitFor<EndpointName>` pattern
2. **Co-location**: Wait functions are located with their corresponding interceptors
3. **Race Condition Testing**: `*ToStart()` variants for precise timing control
4. **Direct Imports**: No barrel exports for wait functions - import directly from interceptor files
5. **Type Safety**: Wait functions are typed and use the same endpoint definitions
6. **Consistency**: All interceptors provide the same wait function patterns

### Best Practices

1. **Use Standard Waits**: Use `waitFor<EndpointName>()` for most test scenarios
2. **Race Condition Testing**: Use `waitFor<EndpointName>ToStart()` when testing loading states or race conditions
3. **Import Together**: Import interceptors and their wait functions from the same file
4. **Chain Waits**: Chain multiple wait functions for multi-step workflows
5. **Error Testing**: Wait functions work with both success and error interceptor modes

---

## API Endpoint Reference

All interceptors correspond to endpoints in `DevicesApiService`:

| Interceptor                 | Endpoint         | HTTP Method | Route                             |
| --------------------------- | ---------------- | ----------- | --------------------------------- |
| `interceptFindDevices`      | findDevices      | GET         | `/api/devices`                    |
| `interceptConnectDevice`    | connectDevice    | POST        | `/api/devices/{deviceId}/connect` |
| `interceptDisconnectDevice` | disconnectDevice | DELETE      | `/api/devices/{deviceId}`         |
| `interceptPingDevice`       | pingDevice       | GET         | `/api/devices/{deviceId}/ping`    |

See `libs/data-access/api-client/src/lib/apis/DevicesApiService.ts` for full API signatures.

---

## Further Reading

- [Cypress Intercept Documentation](https://docs.cypress.io/api/commands/intercept) - Official Cypress docs
- [Phase 2 Fixtures](./test-data/fixtures/README.md) - Available fixture data
- [E2E Testing Plan](../../docs/features/e2e-testing/E2E_PLAN.md) - Overall E2E strategy
- [Phase 4 Device Discovery Test](../../docs/features/e2e-testing/E2E_PLAN_P4.md) - Consuming these interceptors
