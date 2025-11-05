# Device Mock Fixtures

## 📋 Overview

The **Device Mock Fixtures** system provides pre-built, deterministic device scenarios for E2E testing. Fixtures are constants representing realistic TeensyROM device states that can be directly consumed by API interceptors.

**Key Benefits:**

- ✅ **Deterministic**: Same data on every test run (seeded generation)
- ✅ **Type-Safe**: All fixtures match `MockDeviceFixture` interface and use API DTOs
- ✅ **Ready-to-Use**: Import and use directly in interceptors - no manual construction
- ✅ **Comprehensive**: Covers common scenarios (connected, disconnected, empty, errors)
- ✅ **Tested**: All fixtures validated for structure, properties, and determinism

---

## 🏗️ Architecture

### Fixture Generation Flow

```
Phase 1 Generators → Seeded Faker → Device DTOs → Fixture Constants → E2E Tests
```

1. **Phase 1 Generators**: `generateDevice()` and `generateCartStorage()` create DTOs
2. **Seeded Faker**: Fixed seed (12345) ensures deterministic data
3. **Fixture Constants**: Pre-built scenarios using IIFE pattern for seed isolation
4. **E2E Tests**: Import fixtures and use in interceptors

### File Structure

```
fixtures/
├── README.md                    # This documentation
├── fixture.types.ts             # MockDeviceFixture interface
├── devices.fixture.ts           # Device fixture constants
├── devices.fixture.spec.ts      # Fixture validation tests
└── index.ts                     # Barrel exports
```

---

## 🎯 Available Fixtures

### Summary Table

| Fixture                    | Devices | Scenario                                | Use Cases                              |
| -------------------------- | ------- | --------------------------------------- | -------------------------------------- |
| `singleDevice`             | 1       | Connected device with available storage | Default discovery, connection tests    |
| `multipleDevices`          | 3       | Multiple connected devices              | Multi-device scenarios, list rendering |
| `noDevices`                | 0       | No devices found                        | Empty state, first-time experience     |
| `disconnectedDevice`       | 1       | Lost connection                         | Reconnection workflows, error recovery |
| `unavailableStorageDevice` | 1       | Connected but storage unavailable       | Storage errors, hardware issues        |
| `mixedStateDevices`        | 3       | Devices in varied states                | Complex scenarios, state filtering     |

---

## 📖 Fixture Details

### `singleDevice`

**Scenario**: Single connected device with available storage - the most common "happy path".

**Device Properties:**

- ✅ Connected
- ✅ Device State: `Connected`
- ✅ Compatible
- ✅ SD Storage: Available
- ✅ USB Storage: Available

**Use Cases:**

- Default device discovery tests
- Single device connection workflows
- Storage browsing with available storage
- Typical user scenarios

**Example:**

```typescript
import { singleDevice } from '../support/test-data/fixtures';

cy.intercept('GET', '/api/devices', {
  statusCode: 200,
  body: singleDevice,
}).as('getDevices');

cy.visit('/devices');
cy.wait('@getDevices');

// Verify device appears
cy.contains('TeensyROM').should('be.visible');
```

---

### `multipleDevices`

**Scenario**: Three connected devices with unique ports and names.

**Device Properties (all devices):**

- ✅ Connected
- ✅ Device State: `Connected`
- ✅ Compatible
- ✅ SD & USB Storage: Available
- ✅ Unique IDs, ports, names

**Use Cases:**

- Multi-device discovery tests
- Device selection workflows
- Testing UI with multiple devices
- Device list rendering
- Device filtering and sorting

**Example:**

```typescript
import { multipleDevices } from '../support/test-data/fixtures';

cy.intercept('GET', '/api/devices', {
  statusCode: 200,
  body: multipleDevices,
}).as('getDevices');

cy.visit('/devices');
cy.wait('@getDevices');

// Verify all 3 devices appear
cy.get('[data-testid="device-card"]').should('have.length', 3);
```

---

### `noDevices`

**Scenario**: No TeensyROM devices are connected or discovered.

**Device Properties:**

- Device count: 0 (empty array)

**Use Cases:**

- Empty state display
- "No devices found" messaging
- First-time user experience
- All devices disconnected

**Example:**

```typescript
import { noDevices } from '../support/test-data/fixtures';

cy.intercept('GET', '/api/devices', {
  statusCode: 200,
  body: noDevices,
}).as('getDevices');

cy.visit('/devices');
cy.wait('@getDevices');

// Verify empty state message
cy.contains('No devices found').should('be.visible');
```

---

### `disconnectedDevice`

**Scenario**: Device that lost connection but retains previous connection data.

**Device Properties:**

- ❌ Connected: `false`
- ❌ Device State: `ConnectionLost`
- ✅ Valid previous data (name, port, version)
- ✅ Storage objects still exist

**Use Cases:**

- Connection loss handling
- Reconnection workflows
- Device state change testing
- Error recovery scenarios

**Example:**

```typescript
import { disconnectedDevice } from '../support/test-data/fixtures';

cy.intercept('GET', '/api/devices', {
  statusCode: 200,
  body: disconnectedDevice,
}).as('getDevices');

cy.visit('/devices');
cy.wait('@getDevices');

// Verify reconnect button appears
cy.get('[data-testid="reconnect-button"]').should('be.visible');
```

---

### `unavailableStorageDevice`

**Scenario**: Connected device but storage is unavailable (unmounted, corrupted, etc.).

**Device Properties:**

- ✅ Connected
- ✅ Device State: `Connected`
- ✅ Compatible
- ❌ SD Storage: **Unavailable**
- ❌ USB Storage: **Unavailable**

**Use Cases:**

- Storage error handling
- Storage warning displays
- Preventing file operations on unavailable storage
- Hardware issue scenarios

**Example:**

```typescript
import { unavailableStorageDevice } from '../support/test-data/fixtures';

cy.intercept('GET', '/api/devices', {
  statusCode: 200,
  body: unavailableStorageDevice,
}).as('getDevices');

cy.visit('/devices');
cy.wait('@getDevices');

// Verify storage warning appears
cy.contains('Storage unavailable').should('be.visible');
```

---

### `mixedStateDevices`

**Scenario**: Three devices in different states (connected, busy, disconnected).

**Device Properties:**

- **Device 1**: Connected and available
- **Device 2**: Busy (processing command)
- **Device 3**: ConnectionLost (disconnected)

**Use Cases:**

- State filtering and display
- Multi-device state management
- Device list with varied states
- Complex device scenarios

**Example:**

```typescript
import { mixedStateDevices } from '../support/test-data/fixtures';

cy.intercept('GET', '/api/devices', {
  statusCode: 200,
  body: mixedStateDevices,
}).as('getDevices');

cy.visit('/devices');
cy.wait('@getDevices');

// Verify state variety in UI
cy.get('[data-testid="device-connected"]').should('have.length', 1);
cy.get('[data-testid="device-busy"]').should('have.length', 1);
cy.get('[data-testid="device-disconnected"]').should('have.length', 1);
```

---

## 🔧 Usage Patterns

### Basic Import and Usage

```typescript
import { singleDevice } from '../support/test-data/fixtures';

it('should display connected device', () => {
  cy.intercept('GET', '/api/devices', {
    statusCode: 200,
    body: singleDevice,
  }).as('getDevices');

  cy.visit('/devices');
  cy.wait('@getDevices');

  // Test assertions...
});
```

### Using Multiple Fixtures in Same Test

```typescript
import { noDevices, singleDevice } from '../support/test-data/fixtures';

it('should handle device connection', () => {
  // Start with no devices
  cy.intercept('GET', '/api/devices', {
    statusCode: 200,
    body: noDevices,
  }).as('getDevicesEmpty');

  cy.visit('/devices');
  cy.wait('@getDevicesEmpty');
  cy.contains('No devices found').should('be.visible');

  // Simulate device connection
  cy.intercept('GET', '/api/devices', {
    statusCode: 200,
    body: singleDevice,
  }).as('getDevicesConnected');

  cy.get('[data-testid="refresh-button"]').click();
  cy.wait('@getDevicesConnected');
  cy.contains('TeensyROM').should('be.visible');
});
```

### Accessing Individual Devices

```typescript
import { multipleDevices } from '../support/test-data/fixtures';

it('should select specific device', () => {
  const targetDevice = multipleDevices.devices[0];

  cy.intercept('GET', '/api/devices', {
    statusCode: 200,
    body: multipleDevices,
  }).as('getDevices');

  cy.visit('/devices');
  cy.wait('@getDevices');

  // Click specific device by name
  cy.contains(targetDevice.name).click();

  // Verify device details
  cy.contains(targetDevice.comPort).should('be.visible');
  cy.contains(targetDevice.fwVersion).should('be.visible');
});
```

### Combining Fixtures with Custom Responses

```typescript
import { singleDevice } from '../support/test-data/fixtures';

it('should handle connection error', () => {
  const device = singleDevice.devices[0];

  // Mock successful discovery
  cy.intercept('GET', '/api/devices', {
    statusCode: 200,
    body: singleDevice,
  }).as('getDevices');

  // Mock connection failure
  cy.intercept('POST', `/api/devices/${device.deviceId}/connect`, {
    statusCode: 500,
    body: { error: 'Connection failed' },
  }).as('connectDevice');

  cy.visit('/devices');
  cy.wait('@getDevices');

  cy.get('[data-testid="connect-button"]').click();
  cy.wait('@connectDevice');

  // Verify error handling
  cy.contains('Connection failed').should('be.visible');
});
```

---

## 🎨 Fixture Type Interface

All fixtures implement the `MockDeviceFixture` interface:

```typescript
interface MockDeviceFixture {
  readonly devices: readonly CartDto[];
}
```

**Key Points:**

- `devices` is a readonly array of `CartDto` objects
- Readonly constraint prevents accidental mutation in tests
- Matches API response structure for `/api/devices`
- Simple, intentional design - complexity comes from device variety, not fixture structure

---

## ✅ Best Practices

### 1. Use Fixtures Over Manual Construction

**❌ Don't do this:**

```typescript
const device = {
  deviceId: '123',
  isConnected: true,
  // ... manually construct 50+ properties
};
```

**✅ Do this:**

```typescript
import { singleDevice } from '../support/test-data/fixtures';
// All properties validated and deterministic
```

### 2. Choose the Right Fixture for the Scenario

```typescript
// Testing empty state? Use noDevices
import { noDevices } from '../support/test-data/fixtures';

// Testing reconnection? Use disconnectedDevice
import { disconnectedDevice } from '../support/test-data/fixtures';

// Testing multi-device? Use multipleDevices
import { multipleDevices } from '../support/test-data/fixtures';
```

### 3. Access Devices Directly When Needed

```typescript
import { singleDevice } from '../support/test-data/fixtures';

const device = singleDevice.devices[0];
const deviceId = device.deviceId;
const comPort = device.comPort;

// Use extracted values in test logic
```

### 4. Don't Mutate Fixtures

**❌ Don't do this:**

```typescript
singleDevice.devices[0].name = 'Modified'; // TypeScript prevents this
```

**✅ Do this:**

```typescript
// If you need custom data, use generators directly
import { generateDevice } from '../generators/device.generators';

const customDevice = generateDevice({ name: 'Custom Name' });
```

### 5. Keep Fixtures at Top of Test File

```typescript
import { singleDevice, multipleDevices, noDevices } from '../support/test-data/fixtures';

describe('Device Management', () => {
  it('test 1', () => {
    /* use singleDevice */
  });
  it('test 2', () => {
    /* use multipleDevices */
  });
  it('test 3', () => {
    /* use noDevices */
  });
});
```

---

## 🧪 Fixture Validation

All fixtures are validated by automated tests in `devices.fixture.spec.ts`:

**Validated Properties:**

- ✅ Structure matches `MockDeviceFixture` interface
- ✅ Device count is correct
- ✅ All required DTO properties are populated
- ✅ Connection states match scenario
- ✅ Storage availability matches scenario
- ✅ Device uniqueness (IDs, ports, names) in multi-device fixtures
- ✅ Determinism (same data across multiple test runs)

**Run validation tests:**

```bash
pnpm nx test teensyrom-ui-e2e --testFile=fixtures/devices.fixture.spec.ts
```

---

## 🔍 Troubleshooting

### Issue: Fixture data looks random/inconsistent

**Cause**: Fixtures use seeded Faker for determinism - data should be identical across runs.

**Solution**: If data changes, check:

1. Faker seed is properly reset in fixture (should be `faker.seed(12345)`)
2. Generator functions haven't changed
3. No external state is leaking into generators

---

### Issue: TypeScript errors when importing fixtures

**Cause**: Incorrect import path or missing barrel export

**Solution**:

```typescript
// ✅ Correct - import from fixtures barrel
import { singleDevice } from '../support/test-data/fixtures';

// ❌ Wrong - importing from implementation file
import { singleDevice } from '../support/test-data/fixtures/devices.fixture';
```

---

### Issue: Need custom device scenario not covered by fixtures

**Cause**: Fixtures cover common scenarios - custom needs require generators

**Solution**: Use Phase 1 generators directly:

```typescript
import { faker } from '../support/test-data/faker-config';
import { generateDevice } from '../support/test-data/generators/device.generators';

// Generate custom scenario
faker.seed(12345);
const customDevice = generateDevice({
  name: 'Custom Device',
  isCompatible: false,
  fwVersion: '0.1.0',
});

cy.intercept('GET', '/api/devices', {
  statusCode: 200,
  body: { devices: [customDevice] },
});
```

---

### Issue: Fixture not matching API response structure

**Cause**: Fixtures mirror API response - should match `/api/devices` endpoint structure

**Solution**: Verify API response matches fixture structure:

```typescript
// Fixture structure
{
  devices: CartDto[]
}

// API response should be identical
{
  "devices": [{ /* CartDto */ }]
}
```

---

## 🚀 Next Steps

### Phase 3: API Interceptors

Fixtures will be consumed by interceptor utilities that mock API endpoints:

```typescript
// Preview: Phase 3 usage
import { mockDeviceDiscovery } from '../support/interceptors';
import { singleDevice } from '../support/test-data/fixtures';

it('should discover device', () => {
  mockDeviceDiscovery(singleDevice);
  // Test continues...
});
```

### Phase 4: E2E Test Implementation

Complete E2E tests will combine fixtures and interceptors:

```typescript
// Preview: Phase 4 usage
import { mockDeviceDiscovery, mockDeviceConnection } from '../support/interceptors';
import { singleDevice } from '../support/test-data/fixtures';

describe('Device Connection Flow', () => {
  it('should connect to discovered device', () => {
    mockDeviceDiscovery(singleDevice);

    cy.visit('/devices');
    cy.contains('TeensyROM').click();

    mockDeviceConnection(singleDevice.devices[0]);
    cy.get('[data-testid="connect-button"]').click();

    cy.contains('Connected').should('be.visible');
  });
});
```

---

## 📚 Related Documentation

- **[E2E Testing Plan](../../docs/features/e2e-testing/E2E_PLAN.md)** - Overall E2E strategy
- **[Phase 1: Generators](../FAKE_TEST_DATA.md)** - Generator documentation
- **[Phase 2 Plan](../../docs/features/e2e-testing/E2E_PLAN_P2.md)** - This phase's implementation details
- **[Testing Standards](../../docs/TESTING_STANDARDS.md)** - Testing best practices

---

## 🎯 Summary

## 🎮 Storage Favorites Fixtures

Task 3 introduces deterministic filesystem fixtures that build on the MockFilesystem core and Task 2 generators. These fixtures power Cypress favorite workflow tests and can be imported alongside device fixtures.

### Summary Table

| Fixture                     | Scenario                                       | Favorites         | Usage                                         |
| --------------------------- | ---------------------------------------------- | ----------------- | --------------------------------------------- |
| `emptyFilesystem`           | Fresh filesystem                               | None              | Baseline navigation, first favorite creation  |
| `favoriteReadyDirectory`    | Identical to `emptyFilesystem`, semantic alias | None              | Tests that jump directly into favoriting flow |
| `alreadyFavoritedDirectory` | Pac-Man favorited                              | Game              | Verifying favorite removal, UI badge display  |
| `mixedFavoritesDirectory`   | Favorites across media types                   | Game, Song, Image | Synchronization checks, toolbar indicators    |

### Usage Guidelines

- Fixtures return shared `MockFilesystem` instances. Call `reset()` before each test to rehydrate the seeded state.
- Fixtures with pre-applied favorites override `reset()` to keep their scenario intact.
- Use `createMockFilesystem(seed)` from generators when a unique one-off state is required.

**Example:**

```typescript
import { alreadyFavoritedDirectory, mixedFavoritesDirectory } from '../support/test-data/fixtures';

beforeEach(() => {
  alreadyFavoritedDirectory.reset();
  mixedFavoritesDirectory.reset();
});

cy.intercept('GET', STORAGE_ENDPOINTS.directory, {
  statusCode: 200,
  body: mixedFavoritesDirectory.getDirectory('/games'),
});
```

---

## 🎯 Summary

**Fixtures provide:**

- ✅ Pre-built device scenarios ready for E2E tests
- ✅ Deterministic, validated test data
- ✅ Clean imports via barrel exports
- ✅ Type-safe DTO objects
- ✅ Comprehensive scenario coverage

**When to use fixtures:**

- Default/happy path testing → `singleDevice` or `multipleDevices`
- Empty state testing → `noDevices`
- Connection error testing → `disconnectedDevice`
- Storage error testing → `unavailableStorageDevice`
- Complex state testing → `mixedStateDevices`

**When to use generators:**

- Custom device scenarios not covered by fixtures
- Temporary test-specific edge cases
- Debugging specific property combinations

**Remember**: Fixtures are constants - deterministic, validated, and ready to use. They're the foundation for clean, maintainable E2E tests.
