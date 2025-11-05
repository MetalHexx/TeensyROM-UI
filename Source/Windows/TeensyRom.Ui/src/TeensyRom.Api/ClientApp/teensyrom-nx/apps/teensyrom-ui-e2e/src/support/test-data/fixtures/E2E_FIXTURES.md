# Device Mock Fixtures

## 📋 Overview

Device Mock Fixtures provide pre-built, deterministic test data for E2E testing. Fixtures are TypeScript constants representing realistic device scenarios that can be directly imported and used in API interceptors.

**Key Benefits:**
- ✅ **Deterministic**: Identical data on every test run (seeded generation)
- ✅ **Type-Safe**: Fully typed using API DTOs
- ✅ **Ready-to-Use**: Import and use directly—no manual construction
- ✅ **Integrated**: Designed to work seamlessly with API interceptors

---

## 🎯 Philosophy

### Why Fixtures?

Fixtures embody a critical principle: **test data should be deterministic, type-safe, and discoverable.** Rather than manually constructing device objects in each test (error-prone and scattered), fixtures provide centralized, validated constants ready for immediate use.

### Core Principles

1. **Determinism First**: Seeded Faker ensures identical data across runs
2. **Type Safety**: All fixtures match API DTO interfaces
3. **No Mutation**: Readonly constants prevent accidental changes
4. **Scenario-Driven**: Fixtures represent realistic test scenarios, not arbitrary data
5. **Composability**: Fixtures integrate directly with interceptors for clean tests

---

## 🏗️ Architecture

### Fixture Generation Flow

```
Generators → Seeded Faker → Device DTOs → Fixture Constants → E2E Tests
```

**Layers:**
1. **Generators**: Core functions create base DTOs with configurable properties
2. **Seeded Faker**: Fixed seed (12345) ensures deterministic generation
3. **Fixture Constants**: Pre-built scenarios using IIFE pattern for seed isolation
4. **E2E Tests**: Import and use fixtures with interceptors

### File Structure

```
fixtures/
├── E2E_FIXTURES.md             # This documentation
├── fixture.types.ts            # MockDeviceFixture interface
├── devices.fixture.ts          # Device fixture constants
├── devices.fixture.spec.ts     # Fixture validation tests
└── index.ts                    # Barrel exports
```

---

## 🔧 Usage Patterns

### Pattern 1: Basic Fixture Usage

Import a fixture and use it directly with an interceptor. All required device properties are pre-configured and validated.

### Pattern 2: Accessing Individual Data

Extract specific devices or properties from a fixture when you need fine-grained control. Access the `devices` array directly to get individual items.

### Pattern 3: Scenario Switching

Chain different fixtures in a single test to simulate state transitions. Use one fixture for initial setup, then switch to another fixture to simulate a state change (e.g., device connection, storage unavailability).

---

## ✅ Best Practices

### 1. Prefer Fixtures Over Manual Construction

Fixtures are pre-validated, deterministic, and maintainable. Always use them instead of building device objects by hand.

### 2. Choose the Right Fixture for Your Scenario

- Testing normal flow → Use fixtures for connected devices
- Testing empty state → Use empty fixture
- Testing error recovery → Use disconnected or error fixtures
- Testing edge cases → Use generators directly for custom scenarios

### 3. Keep Fixtures at Top of Test File

Import all required fixtures before the test suite for easy discovery and clear intent.

### 4. Don't Mutate Fixtures

Fixtures are readonly constants to prevent accidental modifications. If you need custom data, use generators directly to create new scenarios.

### 5. Test Both Success and Failure Scenarios

Use fixtures to test happy paths, error handling, and edge cases. Pair success fixtures with error mode options for comprehensive coverage.

---

## 🧪 Fixture Validation

All fixtures are validated by automated tests in `devices.fixture.spec.ts`:

**Validated Properties:**
- ✅ Structure matches `MockDeviceFixture` interface
- ✅ Device counts are correct
- ✅ All required DTO properties are populated
- ✅ Connection and storage states match scenario
- ✅ Device uniqueness in multi-device fixtures
- ✅ Determinism (consistent data across runs)

Run validation tests regularly to ensure fixture integrity.

---

## 🔍 Troubleshooting

### Issue: Fixture Data Appears Inconsistent

**Solution**: Verify that fixture generators are using the correct seeded Faker instance. Fixtures should be deterministic—data should never change between runs. Check that the seed value hasn't changed.

### Issue: Import Errors

**Solution**: Always import from the fixtures barrel export file, not from implementation files directly. This ensures you're using the official public API.

### Issue: Need Custom Scenario

**Solution**: If no fixture matches your scenario, use Phase 1 generators directly to create a custom device with specific properties.

---

## 📚 Related Documentation

- **[E2E Interceptors](./E2E_INTERCEPTORS.md)** - How interceptors work with fixtures
- **[Testing Standards](../../docs/TESTING_STANDARDS.md)** - General testing best practices
- **[Phase 1: Generators](../FAKE_TEST_DATA.md)** - How test data is generated

---

## 🎯 Summary

**Fixtures provide:**
- Pre-built, deterministic test data
- Type-safe constants ready for immediate use
- Integration with interceptors for clean test code
- Validation ensuring data integrity

**Use fixtures when:**
- Setting up API responses in tests
- Creating realistic device scenarios
- Testing state transitions
- Validating UI with known data

**Use generators when:**
- Creating one-off custom scenarios
- Testing edge cases not covered by fixtures
- Prototyping quick variations
