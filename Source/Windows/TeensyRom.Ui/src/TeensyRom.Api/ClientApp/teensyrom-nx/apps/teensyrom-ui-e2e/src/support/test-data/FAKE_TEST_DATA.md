# Test Data Generation System

## 📋 Overview

This directory contains the **test data generation system** for E2E tests. It provides type-safe, deterministic generator functions that create realistic API DTO objects for mocking backend responses.

**Key Benefits:**
- ✅ **Type-Safe**: Generates objects that fully satisfy API client DTO interfaces
- ✅ **Deterministic**: Fixed seed ensures identical data on every test run
- ✅ **Flexible**: Override any property for specific test scenarios
- ✅ **Realistic**: Uses Faker to generate plausible device names, ports, versions
- ✅ **Maintainable**: Single source of truth for test data patterns

---

## 🏗️ System Architecture

```
test-data/
├── faker-config.ts              # Seeded Faker instance (seed: 12345)
├── faker-config.spec.ts         # Faker determinism tests
├── FAKE_TEST_DATA.md            # This documentation
└── generators/
    ├── device.generators.ts     # CartDto & CartStorageDto generators
    └── device.generators.spec.ts # Generator validation tests
```

**Data Flow:**
1. Import seeded `faker` from `faker-config.ts`
2. Use generator functions from `generators/`
3. Override specific properties for test scenarios
4. Use generated data in Cypress interceptors

---

## 🔒 Faker Configuration & Determinism

### Fixed Seed Strategy

All test data generation uses a **fixed seed value of 12345**. This ensures:

- **Reproducibility**: Same seed = same data sequence on every test run
- **Debuggability**: Failing tests produce identical data when re-run
- **Consistency**: All developers see the same generated values

### Critical Import Rule

**❌ NEVER import Faker directly:**
```typescript
// ❌ Wrong - breaks determinism
import { faker } from '@faker-js/faker';
```

**✅ ALWAYS import from faker-config:**
```typescript
// ✅ Correct - uses seeded instance
import { faker } from '../faker-config';
```

### Resetting the Seed

If you need to reset the sequence in tests:

```typescript
import { faker } from '../faker-config';

// Reset to initial seed
faker.seed(12345);

// Now generates from start of sequence
const device1 = generateDevice();
const device2 = generateDevice();
```

---

## 🎯 Available Generators

### `generateCartStorage(overrides?)`

Generates a `CartStorageDto` object representing TeensyROM storage (SD card or USB).

**Function Signature:**
```typescript
function generateCartStorage(
  overrides?: Partial<CartStorageDto>
): CartStorageDto
```

**Default Values:**
- `deviceId`: Random UUID
- `type`: Randomly selected from `TeensyStorageType.Sd` or `TeensyStorageType.Usb`
- `available`: `true`

**Example - Default Usage:**
```typescript
import { generateCartStorage } from './generators/device.generators';

const storage = generateCartStorage();
// {
//   deviceId: "a1b2c3d4-e5f6-...",
//   type: "SD",
//   available: true
// }
```

**Example - Unavailable USB Storage:**
```typescript
const usbStorage = generateCartStorage({
  type: TeensyStorageType.Usb,
  available: false
});
// {
//   deviceId: "...",
//   type: "USB",
//   available: false
// }
```

---

### `generateDevice(overrides?)`

Generates a complete `CartDto` object representing a TeensyROM device.

**Function Signature:**
```typescript
function generateDevice(
  overrides?: Partial<CartDto>
): CartDto
```

**Default Values:**
- `deviceId`: Random UUID
- `isConnected`: `true`
- `deviceState`: `DeviceState.Connected`
- `comPort`: Random selection from `['COM3', 'COM4', 'COM5', 'COM6']`
- `name`: `"TeensyROM {CompanyName}"` (e.g., "TeensyROM Acme Corp")
- `fwVersion`: Semantic version (e.g., "1.5.3")
- `isCompatible`: `true`
- `sdStorage`: Generated via `generateCartStorage` with type SD
- `usbStorage`: Generated via `generateCartStorage` with type USB

**Example - Default Connected Device:**
```typescript
import { generateDevice } from './generators/device.generators';

const device = generateDevice();
// {
//   deviceId: "a1b2c3d4-...",
//   isConnected: true,
//   deviceState: "Connected",
//   comPort: "COM4",
//   name: "TeensyROM Industrial Solutions",
//   fwVersion: "1.3.8",
//   isCompatible: true,
//   sdStorage: { deviceId: "a1b2c3d4-...", type: "SD", available: true },
//   usbStorage: { deviceId: "a1b2c3d4-...", type: "USB", available: true }
// }
```

**Example - Disconnected Device:**
```typescript
const disconnectedDevice = generateDevice({
  isConnected: false,
  deviceState: DeviceState.Connectable
});
```

**Example - Incompatible Device:**
```typescript
const incompatibleDevice = generateDevice({
  isCompatible: false,
  fwVersion: "0.1.0"
});
```

**Example - Device with Unavailable Storage:**
```typescript
const noStorageDevice = generateDevice({
  sdStorage: {
    deviceId: deviceId,
    type: TeensyStorageType.Sd,
    available: false
  },
  usbStorage: {
    deviceId: deviceId,
    type: TeensyStorageType.Usb,
    available: false
  }
});
```

---

## 🎨 Override Patterns

### Partial Overrides

Generators accept `Partial<T>` parameters, so you only specify what you want to change:

```typescript
// Override just one property
const device = generateDevice({ name: "Test Device" });
// All other properties use defaults/generated values

// Override multiple properties
const device = generateDevice({
  isConnected: false,
  deviceState: DeviceState.Busy,
  isCompatible: false
});
```

### Nested Object Overrides

For nested properties like `sdStorage` and `usbStorage`, provide complete override objects:

```typescript
const device = generateDevice({
  sdStorage: {
    deviceId: "custom-id",
    type: TeensyStorageType.Sd,
    available: false
  }
  // usbStorage uses default generated value
});
```

### Combining Generators

Build complex scenarios by combining generators:

```typescript
const deviceId = faker.string.uuid();

const device = generateDevice({
  deviceId,
  sdStorage: generateCartStorage({ deviceId, type: TeensyStorageType.Sd, available: true }),
  usbStorage: generateCartStorage({ deviceId, type: TeensyStorageType.Usb, available: false })
});
```

---

## 📖 Usage Examples

### Example 1: Single Connected Device

```typescript
import { generateDevice } from '../support/test-data/generators/device.generators';

// Cypress test
it('should display connected device', () => {
  const device = generateDevice();

  cy.intercept('GET', '/api/devices', {
    statusCode: 200,
    body: { devices: [device] }
  }).as('getDevices');

  cy.visit('/devices');
  cy.wait('@getDevices');

  // Verify device appears
  cy.contains(device.name).should('be.visible');
  cy.contains(device.comPort).should('be.visible');
});
```

### Example 2: Multiple Devices

```typescript
import { faker } from '../support/test-data/faker-config';
import { generateDevice } from '../support/test-data/generators/device.generators';

faker.seed(12345); // Reset seed

const devices = [
  generateDevice(),
  generateDevice(),
  generateDevice()
];

cy.intercept('GET', '/api/devices', {
  statusCode: 200,
  body: { devices }
}).as('getDevices');
```

### Example 3: Disconnected Device Scenario

```typescript
const disconnectedDevice = generateDevice({
  isConnected: false,
  deviceState: DeviceState.Connectable
});

cy.intercept('GET', '/api/devices', {
  statusCode: 200,
  body: { devices: [disconnectedDevice] }
});

// Test shows "Connect" button instead of "Connected" state
```

### Example 4: Device Connection Error

```typescript
const device = generateDevice();

// Mock successful discovery
cy.intercept('GET', '/api/devices', {
  statusCode: 200,
  body: { devices: [device] }
});

// Mock connection failure
cy.intercept('POST', `/api/devices/${device.deviceId}/connect`, {
  statusCode: 500,
  body: { error: 'Connection failed' }
});

// Test error handling
```

---

## ✅ Best Practices

### 1. Reset Seed for Consistency

Reset the seed at the start of each test to ensure predictable data:

```typescript
beforeEach(() => {
  faker.seed(12345);
});

it('test with consistent data', () => {
  const device = generateDevice(); // Always same device
  // ...
});
```

### 2. Override Explicitly for Scenarios

Don't rely on random generation for test logic. Be explicit about what matters:

```typescript
// ❌ Bad - hoping type is randomly USB
const device = generateDevice();
expect(device.sdStorage.type).toBe(TeensyStorageType.Usb); // Fails if random picks SD

// ✅ Good - explicit override
const device = generateDevice({
  sdStorage: { deviceId: '', type: TeensyStorageType.Usb, available: true }
});
```

### 3. Use Defaults for Happy Path

Let generators provide defaults for "normal" scenarios:

```typescript
// ✅ Good - default is connected, compatible device
const device = generateDevice();

// Only override when testing edge cases
const incompatibleDevice = generateDevice({ isCompatible: false });
```

### 4. Generate Multiple Devices with Sequence

```typescript
faker.seed(12345);

// Generate 3 unique devices
const devices = Array.from({ length: 3 }, () => generateDevice());

// Each device has different ID, name, port due to faker sequence
```

### 5. Keep Test Data Close to Test

Don't create giant fixture files. Generate data in tests where it's needed:

```typescript
// ✅ Good - data generated in test
it('should handle multiple devices', () => {
  const devices = [generateDevice(), generateDevice()];
  // Use devices in test
});

// ❌ Avoid - disconnected fixture far from test
const DEVICES_FIXTURE = [/* 50 devices */];
it('should handle multiple devices', () => {
  // Which device am I testing?
});
```

---

## 🧪 Testing the Generators

Generator functions are fully tested in `generators/device.generators.spec.ts`. Tests validate:

- ✅ All required DTO properties are populated
- ✅ Default values match documentation
- ✅ Override behavior works correctly
- ✅ Generated values are realistic (UUID format, semantic versions, valid COM ports)
- ✅ Determinism with fixed seed
- ✅ Type safety (TypeScript validates DTOs)

Run generator tests:

```bash
pnpm nx test teensyrom-ui-e2e
```

---

## 🔧 Troubleshooting

### Issue: Generated Data Not Deterministic

**Symptom**: Different values on each test run

**Cause**: Importing Faker directly instead of from `faker-config.ts`

**Solution**:
```typescript
// ❌ Wrong
import { faker } from '@faker-js/faker';

// ✅ Correct
import { faker } from '../support/test-data/faker-config';
```

---

### Issue: Tests Fail with "Module Not Found"

**Symptom**: Cannot import generators in Cypress tests

**Cause**: Incorrect relative path

**Solution**:
```typescript
// From e2e/ directory
import { generateDevice } from '../support/test-data/generators/device.generators';
```

---

### Issue: Type Errors on Overrides

**Symptom**: TypeScript error when providing overrides

**Cause**: Incorrect property types

**Solution**: Check generated DTO types in `libs/data-access/api-client/src/lib/models/`

```typescript
// ✅ Correct - matches DeviceState enum
const device = generateDevice({ deviceState: DeviceState.Busy });

// ❌ Wrong - string doesn't match enum
const device = generateDevice({ deviceState: 'Busy' });
```

---

## 🚀 Future Enhancements

Planned additions to the test data system:

- **Storage Fixture Generators**: `generateFileItem`, `generateDirectoryItem`
- **Response Generators**: `generateFindDevicesResponse`, `generateConnectDeviceResponse`
- **Custom Faker Helpers**: Domain-specific generators like `faker.teensyrom.deviceName()`
- **Fixture Factories**: Pre-built collections like `threeConnectedDevices()`, `mixedDevices()`

---

## 📚 Related Documentation

- [E2E Testing Plan](../../../docs/features/e2e-testing/E2E_PLAN.md) - Overall E2E strategy
- [Phase 1 Plan](../../../docs/features/e2e-testing/E2E_PLAN_P1.md) - This phase's implementation details
- [Faker.js Documentation](https://fakerjs.dev/) - Faker API reference
- [API Client Types](../../../libs/data-access/api-client/src/lib/models/) - Generated DTO interfaces

---

**Need Help?** Check generator tests for working examples of all patterns documented here.
