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
├── generators/
│   ├── device.generators.ts     # CartDto & CartStorageDto generators
│   └── device.generators.spec.ts # Generator validation tests
└── fixtures/
    ├── README.md                # Fixture system documentation
    ├── fixture.types.ts         # MockDeviceFixture interface
    ├── devices.fixture.ts       # Pre-built device scenarios
    ├── devices.fixture.spec.ts  # Fixture validation tests
    └── index.ts                 # Barrel exports
```

**Data Flow:**

1. Import seeded `faker` from `faker-config.ts`
2. Use generator functions from `generators/` to create individual DTOs
3. Use fixture constants from `fixtures/` for common scenarios
4. Override specific properties for test-specific scenarios
5. Use generated data or fixtures in Cypress interceptors

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

---

## 🎯 Available Generators

### `generateCartStorage(overrides?)`

Generates a `CartStorageDto` object representing TeensyROM storage (SD card or USB).

**Function Signature:**

```typescript
function generateCartStorage(overrides?: Partial<CartStorageDto>): CartStorageDto;
```

**Default Values:**

- `deviceId`: Random UUID
- `type`: Randomly selected from `TeensyStorageType.Sd` or `TeensyStorageType.Usb`
- `available`: `true`

---

### `generateDevice(overrides?)`

Generates a complete `CartDto` object representing a TeensyROM device.

**Function Signature:**

```typescript
function generateDevice(overrides?: Partial<CartDto>): CartDto;
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

---

## 🎨 Override Patterns

### Partial Overrides

Generators accept `Partial<T>` parameters, allowing you to override specific properties while defaults are applied to the rest.

### Nested Object Overrides

For nested properties like `sdStorage` and `usbStorage`, provide complete override objects rather than partial updates.

### Combining Generators

Build complex scenarios by composing generator calls—pass output from one generator as input to another for interconnected data.

---

## ✅ Best Practices

### 1. Reset Seed for Consistency

Reset the seed at the start of each test to ensure predictable data. Use `beforeEach()` in test suites to reset automatically, or call `faker.seed(12345)` manually when needed:

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
  sdStorage: { deviceId: '', type: TeensyStorageType.Usb, available: true },
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

Don't create giant fixture files. Generate data in tests where it's needed for clarity and maintainability.

---

## 🎯 Device Mock Fixtures

**What are fixtures?** Pre-built device scenarios using generators - ready-to-use constants for common testing scenarios.

**Why use fixtures?** Instead of generating devices in every test, use validated, deterministic fixture constants for common scenarios like "single connected device" or "no devices found".

### When to Use Fixtures vs Generators

**Use Fixtures:**

- ✅ Common scenarios (connected device, empty state, disconnected)
- ✅ Multiple tests need the same device setup
- ✅ Want deterministic, validated test data

**Use Generators:**

- ✅ Test-specific edge cases
- ✅ Need custom property combinations
- ✅ Temporary debugging scenarios

**📖 For detailed fixture documentation**, see [fixtures/README.md](./fixtures/README.md)

---

## 🧪 Testing the Generators

Generator functions are fully tested in `generators/device.generators.spec.ts`. Tests validate:

- ✅ All required DTO properties are populated
- ✅ Default values match documentation
- ✅ Override behavior works correctly
- ✅ Generated values are realistic (UUID format, semantic versions, valid COM ports)
- ✅ Determinism with fixed seed
- ✅ Type safety (TypeScript validates DTOs)

Run tests with:

```bash
pnpm nx test teensyrom-ui-e2e
```

---

## 🔧 Troubleshooting

### Issue: Generated Data Not Deterministic

**Symptom**: Different values on each test run

**Cause**: Importing Faker directly instead of from `faker-config.ts`

**Solution**: See the "Critical Import Rule" section above—always import from `faker-config.ts` to use the seeded instance.

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

## 📚 Related Documentation

- [Fixture Documentation](./fixtures/E2E_FIXTURES.md) - Comprehensive fixture usage guide
- [Faker.js Documentation](https://fakerjs.dev/) - Faker API reference
- [API Client Types](../../../libs/data-access/api-client/src/lib/models/) - Generated DTO interfaces
