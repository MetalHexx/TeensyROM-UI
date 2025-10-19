# Phase 1: Faker Setup and DTO Generators

## 🎯 Objective

Install Faker with a fixed seed and create type-safe generator functions for device-related API DTOs. These generators will produce realistic, deterministic test data for CartDto, DeviceState, and CartStorageDto objects used throughout the E2E testing system.

**Value**: Provides the foundation for all mock data generation with guaranteed reproducibility (same seed = same data) and type safety aligned with the backend API contracts.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [x] [E2E Testing Plan](./E2E_PLAN.md) - Overall E2E testing strategy and phase overview
- [ ] [API Client Types](../../../libs/data-access/api-client/src/lib/models/) - Generated API DTO types to generate

**Standards & Guidelines:**
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices

**External References:**
- [Faker.js Documentation](https://fakerjs.dev/guide/) - Faker API reference
- [Faker.js Seeding](https://fakerjs.dev/guide/usage.html#seeding) - Fixed seed configuration

---

## 📂 File Structure Overview

```
apps/teensyrom-ui-e2e/
├── vitest.config.ts                             ✨ New - Vitest configuration for E2E project
├── project.json                                 📝 Modified - Add test target
└── src/support/
    └── test-data/
        ├── FAKE_TEST_DATA.md                    ✨ New - Documentation for test data system
        ├── faker-config.ts                      ✨ New - Faker instance with fixed seed
        └── generators/
            ├── device.generators.ts             ✨ New - Device DTO generator functions
            └── device.generators.spec.ts        ✨ New - Generator validation tests
```

---

<details open>
<summary><h3>Task 1: Configure Vitest for E2E Project</h3></summary>

**Purpose**: Set up Vitest as the test runner for the E2E project to enable unit testing of generator functions.

**Related Documentation:**
- [Vitest Configuration](https://vitest.dev/config/) - Vitest configuration reference
- [Nx Vitest Plugin](https://nx.dev/packages/vite/executors/test) - Nx integration for Vitest

**Implementation Subtasks:**
- [ ] Create `apps/teensyrom-ui-e2e/vitest.config.ts` with proper configuration
- [ ] Configure test environment to use `node` (for unit tests, not browser)
- [ ] Set up coverage reporting (optional for Phase 1)
- [ ] Add `test` target to `apps/teensyrom-ui-e2e/project.json`
- [ ] Configure test file patterns to match `*.spec.ts` files
- [ ] Verify Vitest picks up the workspace configuration

**Testing Subtask:**
- [ ] **Verify Configuration**: Run `nx test teensyrom-ui-e2e --run` with no tests to confirm setup

**Key Implementation Notes:**
- E2E project needs Vitest for **generator unit tests**, not for actual E2E tests (those use Cypress)
- Test environment should be `node` since generators are pure TypeScript functions
- Vitest config should extend workspace vitest.workspace.ts pattern
- Keep configuration minimal - workspace defaults should handle most settings

**Vitest Configuration Template:**
```typescript
import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    name: 'teensyrom-ui-e2e-generators',
    globals: true,
    environment: 'node',
    include: ['src/support/**/*.spec.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
    },
  },
});
```

**Project.json Test Target:**
```json
{
  "test": {
    "executor": "@nx/vite:test",
    "outputs": ["{projectRoot}/coverage"],
    "options": {
      "passWithNoTests": true,
      "reportsDirectory": "../../coverage/apps/teensyrom-ui-e2e"
    }
  }
}
```

**Testing Focus for Task 1:**

**Behaviors to Test:**
- [ ] Vitest configuration loads without errors
- [ ] Test runner discovers spec files in `src/support/` directory
- [ ] `nx test teensyrom-ui-e2e` command executes successfully
- [ ] Workspace Vitest config picks up E2E project config

</details>

---

<details open>
<summary><h3>Task 2: Install Faker Dependencies</h3></summary>

**Purpose**: Add Faker library to the project as a dev dependency for generating realistic test data.

**Related Documentation:**
- [Faker Installation Guide](https://fakerjs.dev/guide/) - Installation instructions

**Implementation Subtasks:**
- [ ] Install `@faker-js/faker` package using pnpm as a dev dependency
- [ ] Verify package appears in `devDependencies` in `package.json`

**Testing Subtask:**
- [ ] **Verify Installation**: Confirm Faker can be imported in a test file

**Key Implementation Notes:**
- Use the scoped package `@faker-js/faker`, not the legacy `faker` package
- Install as dev dependency since it's only used in E2E tests
- Version should be latest stable (Faker 9.x as of early 2025)

**Installation Command:**
```bash
pnpm add -D @faker-js/faker
```

**Testing Focus for Task 1:**

**Behaviors to Test:**
- [ ] Faker imports successfully without errors
- [ ] Basic Faker methods are accessible (e.g., `faker.string.uuid()`)

</details>

---

<details open>
<summary><h3>Task 3: Create Faker Configuration</h3></summary>

**Purpose**: Configure a Faker instance with a fixed seed (12345) to ensure 100% reproducible test data across all test runs.

**Related Documentation:**
- [Faker Seeding Documentation](https://fakerjs.dev/guide/usage.html#seeding) - How to configure fixed seeds

**Implementation Subtasks:**
- [ ] Create `apps/teensyrom-ui-e2e/src/support/test-data/faker-config.ts`
- [ ] Import Faker from `@faker-js/faker`
- [ ] Configure Faker instance with seed value `12345`
- [ ] Export configured `faker` instance as default export
- [ ] Add JSDoc comments explaining seed purpose

**Testing Subtask:**
- [ ] **Write Tests**: Verify Faker determinism (see Testing section below)

**Key Implementation Notes:**
- Seed value `12345` chosen arbitrarily but documented in E2E_PLAN.md
- All generators must import from this config file, NOT directly from Faker
- Seed ensures identical data on every test run for reproducibility
- Determinism is critical for debugging failed tests

**Critical Configuration Structure**:
```typescript
// Configure with seed and export
import { faker as fakerInstance } from '@faker-js/faker';
fakerInstance.seed(12345);
export const faker = fakerInstance;
```

**Testing Focus for Task 2:**

**Behaviors to Test:**
- [ ] Faker instance generates identical values across multiple calls with same seed
- [ ] Resetting seed to 12345 produces same sequence
- [ ] Multiple imports of faker config share same seeded state

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for unit testing approach

</details>

---

<details open>
<summary><h3>Task 4: Create CartStorageDto Generator</h3></summary>

**Purpose**: Generate realistic CartStorageDto objects representing TeensyROM storage (SD card or USB).

**Related Documentation:**
- [CartStorageDto API Type](../../../libs/data-access/api-client/src/lib/models/CartStorageDto.ts) - DTO interface to implement

**Implementation Subtasks:**
- [ ] Create `apps/teensyrom-ui-e2e/src/support/test-data/generators/device.generators.ts`
- [ ] Import `CartStorageDto` and `TeensyStorageType` from `@teensyrom-nx/data-access/api-client`
- [ ] Import `faker` from `../faker-config`
- [ ] Create `generateCartStorage` function accepting optional `Partial<CartStorageDto>` overrides
- [ ] Generate `deviceId` using `faker.string.uuid()`
- [ ] Generate `type` using `faker.helpers.arrayElement()` with TeensyStorageType values
- [ ] Set `available` to `true` as default (tests override for unavailable scenarios)
- [ ] Merge overrides with generated values (overrides take precedence)
- [ ] Add JSDoc describing function purpose and parameters

**Testing Subtask:**
- [ ] **Write Tests**: Verify CartStorageDto generation (see Testing section below)

**Key Implementation Notes:**
- Default `available` to `true` for common scenarios; tests explicitly set to `false` when needed
- Use `faker.helpers.arrayElement(['SD', 'USB'])` for type selection
- Overrides parameter allows tests to specify exact values when needed
- Function signature: `generateCartStorage(overrides?: Partial<CartStorageDto>): CartStorageDto`

**Testing Focus for Task 3:**

**Behaviors to Test:**
- [ ] Generated object has all required CartStorageDto properties
- [ ] `deviceId` is a valid UUID format
- [ ] `type` is either 'SD' or 'USB' (TeensyStorageType values)
- [ ] `available` defaults to `true`
- [ ] Overrides correctly replace generated values
- [ ] Multiple calls produce consistent results (deterministic sequence from seed)

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for unit testing patterns

</details>

---

<details open>
<summary><h3>Task 5: Create CartDto Generator</h3></summary>

**Purpose**: Generate realistic CartDto objects representing complete TeensyROM device data with all properties populated.

**Related Documentation:**
- [CartDto API Type](../../../libs/data-access/api-client/src/lib/models/CartDto.ts) - DTO interface to implement
- [DeviceState API Type](../../../libs/data-access/api-client/src/lib/models/DeviceState.ts) - Device state enum values

**Implementation Subtasks:**
- [ ] Import `CartDto` and `DeviceState` from `@teensyrom-nx/data-access/api-client`
- [ ] Create `generateDevice` function in `device.generators.ts`
- [ ] Accept optional `Partial<CartDto>` overrides parameter
- [ ] Generate `deviceId` using `faker.string.uuid()`
- [ ] Set `isConnected` to `true` as default (tests override for disconnected scenarios)
- [ ] Set `deviceState` to `DeviceState.Connected` as default (tests override for other states)
- [ ] Generate `comPort` using `faker.helpers.arrayElement(['COM3', 'COM4', 'COM5', 'COM6'])`
- [ ] Generate `name` using `faker.company.name()` prefixed with "TeensyROM"
- [ ] Generate `fwVersion` using semantic version pattern (e.g., "1.2.3")
- [ ] Set `isCompatible` to `true` as default (tests override for incompatible scenarios)
- [ ] Generate `sdStorage` using `generateCartStorage` helper
- [ ] Generate `usbStorage` using `generateCartStorage` helper
- [ ] Merge overrides with generated values
- [ ] Add comprehensive JSDoc

**Testing Subtask:**
- [ ] **Write Tests**: Verify CartDto generation (see Testing section below)

**Key Implementation Notes:**
- Default to "happy path" values (connected, compatible, available storage)
- Tests explicitly override when they need specific failure scenarios
- COM ports should be realistic Windows serial port names (COM3-COM6 range)
- Firmware version format: `${faker.number.int({min:1,max:2})}.${faker.number.int({min:0,max:9})}.${faker.number.int({min:0,max:20})}`
- Device names should feel realistic: `TeensyROM ${faker.company.name()}`
- Storage objects must be generated via `generateCartStorage` for consistency

**Testing Focus for Task 4:**

**Behaviors to Test:**
- [ ] Generated object has all required CartDto properties
- [ ] `deviceId` is a valid UUID format
- [ ] `isConnected` defaults to `true`
- [ ] `deviceState` defaults to `DeviceState.Connected`
- [ ] `isCompatible` defaults to `true`
- [ ] `comPort` matches expected pattern (COMx)
- [ ] `fwVersion` matches semantic version format (x.y.z)
- [ ] `name` contains "TeensyROM" prefix
- [ ] `sdStorage` and `usbStorage` are properly formed CartStorageDto objects
- [ ] Overrides correctly replace generated values
- [ ] Multiple calls produce consistent results (deterministic sequence from seed)

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing approach

</details>

---

<details open>
<summary><h3>Task 6: Create Test Data Documentation</h3></summary>

**Purpose**: Document the test data system architecture, generator usage patterns, and best practices for future developers.

**Related Documentation:**
- [E2E Testing Plan](./E2E_PLAN.md) - Overall context for test data system

**Implementation Subtasks:**
- [ ] Create `apps/teensyrom-ui-e2e/src/support/test-data/FAKE_TEST_DATA.md`
- [ ] Document purpose of test-data directory
- [ ] Explain fixed seed strategy and why seed 12345 is used
- [ ] Provide usage examples for `generateDevice` and `generateCartStorage`
- [ ] Document override pattern for customizing generated data
- [ ] Explain determinism guarantees and reproducibility
- [ ] Show examples of testing different scenarios (connected, disconnected, unavailable storage, etc.)
- [ ] Add troubleshooting section for common issues
- [ ] Include examples of combining generators with custom data

**Testing Subtask:**
- [ ] **Review Documentation**: Have another developer review for clarity (manual verification)

**Key Implementation Notes:**
- Target audience: developers writing E2E tests or extending the test data system
- Include copy-paste-ready code examples showing default usage and override patterns
- Emphasize the importance of importing from `faker-config.ts`, not directly from Faker
- Document future expansion points (fixtures, interceptors in later phases)
- Show how tests control scenarios through explicit overrides, not randomness

**Documentation Sections:**
1. **Overview**: Purpose and architecture
2. **Faker Configuration**: Seed strategy and determinism
3. **Generators**: Available generator functions and signatures
4. **Default Values**: What defaults generators use and why
5. **Usage Examples**: Code snippets for common scenarios (default, disconnected, unavailable storage)
6. **Override Patterns**: How to customize generated data for specific test scenarios
7. **Best Practices**: Guidelines for maintaining test data
8. **Troubleshooting**: Common issues and solutions

</details>

---

## 🗂️ Files Modified or Created

**New Files:**
- `apps/teensyrom-ui-e2e/vitest.config.ts` - Vitest configuration for E2E project
- `apps/teensyrom-ui-e2e/src/support/test-data/FAKE_TEST_DATA.md` - Test data documentation
- `apps/teensyrom-ui-e2e/src/support/test-data/faker-config.ts` - Faker with fixed seed
- `apps/teensyrom-ui-e2e/src/support/test-data/generators/device.generators.ts` - Generator functions
- `apps/teensyrom-ui-e2e/src/support/test-data/generators/device.generators.spec.ts` - Generator tests

**Modified Files:**
- `package.json` - Add `@faker-js/faker` to devDependencies
- `apps/teensyrom-ui-e2e/project.json` - Add Vitest test target

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
> - **Test determinism**: Same seed = same data sequence
> - **Test type correctness**: Generated objects match DTO interfaces
> - **Test override behavior**: Custom values replace generated values
> - **Test realistic data**: Values should be plausible for TeensyROM devices
> - **Test default values**: Verify generators use correct defaults

### Where Tests Are Written

**Tests are embedded in Task 3, 4, and 5** with:
- **Testing Subtask**: "Write Tests" checkbox in each task
- **Testing Focus**: "Behaviors to Test" section with specific outcomes
- **Testing Reference**: Links to testing documentation

### Test File Location

All generator tests live in:
- `apps/teensyrom-ui-e2e/src/support/test-data/generators/device.generators.spec.ts`

### Test Execution Commands

```bash
# Run generator tests
pnpm nx test teensyrom-ui-e2e

# Run tests in watch mode during development
pnpm nx test teensyrom-ui-e2e --watch

# Run tests with coverage
pnpm nx test teensyrom-ui-e2e --coverage
```

**Note**: Task 1 configures Vitest for the E2E project, enabling these test commands.

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] Vitest configured for E2E project with test target in project.json
- [ ] Vitest config file created (vitest.config.ts)
- [ ] Faker installed and appears in `package.json` devDependencies
- [ ] Faker config file created with seed 12345
- [ ] `generateCartStorage` function implemented and exported
- [ ] `generateDevice` function implemented and exported
- [ ] All generators accept optional override parameters
- [ ] All generators use sensible deterministic defaults
- [ ] FAKE_TEST_DATA.md documentation completed with examples

**Testing Requirements:**
- [ ] Vitest runs successfully with `pnpm nx test teensyrom-ui-e2e`
- [ ] Test runner discovers spec files in src/support/ directory
- [ ] Faker determinism test passes (same seed = same sequence)
- [ ] CartStorageDto generator test passes (all properties validated)
- [ ] CartDto generator test passes (all properties validated)
- [ ] Default value tests pass (verify `isConnected: true`, `available: true`, etc.)
- [ ] Override behavior tests pass for both generators
- [ ] All tests pass with zero failures
- [ ] Tests demonstrate reproducibility

**Type Safety:**
- [ ] No TypeScript errors in generator files
- [ ] Generated objects fully satisfy DTO interfaces
- [ ] Overrides parameter types match DTO Partial types
- [ ] All imports resolve correctly

**Quality Checks:**
- [ ] Linting passes with no errors (`pnpm nx lint teensyrom-ui-e2e`)
- [ ] Code formatting is consistent
- [ ] JSDoc comments present on all exported functions
- [ ] No console warnings when running tests

**Documentation:**
- [ ] FAKE_TEST_DATA.md explains test data architecture
- [ ] Usage examples are clear and runnable
- [ ] Override patterns documented with examples
- [ ] Determinism guarantees explained
- [ ] Default values documented and explained

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] Generators produce realistic device data
- [ ] Tests demonstrate generators work correctly
- [ ] Ready to build fixtures in Phase 2

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Fixed Seed 12345**: Arbitrary but documented value ensuring all developers see identical test data. Alternative would be random seeds, but debugging failing tests requires reproducibility.
- **Override Pattern**: Generators accept `Partial<T>` to allow specific values while keeping other fields generated. Balances flexibility with convenience.
- **Deterministic Defaults**: Generators use sensible defaults (connected, available, compatible) rather than random probabilities. Tests explicitly override when they need failure scenarios.
- **Realistic COM Ports**: Using COM3-COM6 range mimics actual Windows serial port assignments for TeensyROM devices.
- **Separate Storage Generator**: `generateCartStorage` is its own function to enable reuse and testing in isolation, not just embedded in `generateDevice`.

### Implementation Constraints

- **Vitest Configuration**: E2E project uses Vitest for unit testing generators (not for E2E tests - those use Cypress). Test environment is `node` since generators are pure TypeScript functions.
- **Generator Tests Run in E2E Project**: Generator tests run in the E2E project context using the Vitest configuration added in Task 1.
- **API Client Dependency**: Generators depend on types from `@teensyrom-nx/data-access/api-client`. If API changes, generators must be updated to match new DTO structures.
- **Faker Version**: Using Faker 9.x API. If project uses older Faker versions, method names may differ (e.g., `faker.random.uuid()` vs `faker.string.uuid()`).

### Future Enhancements

- **Additional Device Generators**: May need generators for `FindDevicesResponse`, `ConnectDeviceResponse`, etc. in future phases
- **Storage Fixture Generators**: Phase 2+ will need generators for `FileItemDto`, `DirectoryItemDto`, etc.
- **Custom Faker Helpers**: Could create domain-specific helpers like `faker.teensyrom.deviceName()` for more realistic data

### External References

- [Faker.js API Documentation](https://fakerjs.dev/api/) - Complete Faker method reference
- [CartDto Source](../../../libs/data-access/api-client/src/lib/models/CartDto.ts) - Generated DTO definition
- [E2E Testing Plan](./E2E_PLAN.md) - Overall testing strategy context

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery**: DTO structures verified from API client:
  - `CartDto`: 9 required properties (deviceId, isConnected, deviceState, comPort, name, fwVersion, isCompatible, sdStorage, usbStorage)
  - `CartStorageDto`: 3 required properties (deviceId, type, available)
  - `DeviceState`: Enum with 5 values (Connected, Connectable, ConnectionLost, Busy, Unknown)
  - `TeensyStorageType`: Enum with 2 values ('SD', 'USB') - **Note: Use 'SD' and 'USB', not 'Sd' and 'Usb'**
- **Discovery**: E2E project uses Cypress for actual E2E tests; Vitest is added specifically for unit testing generator functions
- **Discovery**: Workspace already has `vitest.workspace.ts` configuration that auto-discovers vitest configs in projects
- **Discovery**: E2E project needs `scope:e2e` tag and ESLint rule to allow importing from `scope:data-access`

</details>

---

## 💡 Implementation Tips

### Before Starting

1. **Complete Task 1 First**: Set up Vitest configuration before writing any tests
2. **Review API Client Types**: Examine `CartDto.ts`, `DeviceState.ts`, `CartStorageDto.ts` to understand exact property names and types
3. **Check Faker Version**: Verify which Faker version gets installed - API may vary between versions
4. **Verify Test Runner**: Confirm Vitest runs with `pnpm nx test teensyrom-ui-e2e --run` (should pass with no tests initially)

### During Implementation

1. **Follow Task Order**: Complete tasks sequentially (Vitest → Faker → Config → Generators → Docs)
2. **Test as You Go**: Write generator tests immediately after creating each generator function
3. **Verify Determinism**: Run tests multiple times to confirm seed produces identical results
4. **Check Type Safety**: Ensure TypeScript validates generated objects against DTO interfaces
5. **Use Watch Mode**: Run `pnpm nx test teensyrom-ui-e2e --watch` for rapid feedback during development

### After Task Completion

1. **Run Linter**: Execute `pnpm nx lint teensyrom-ui-e2e` to catch issues early
2. **Test Multiple Seeds**: Temporarily change seed value to verify different data is generated
3. **Document Quirks**: Update "Discoveries During Implementation" with any findings
4. **Verify Documentation**: Ensure README examples actually work by copy-pasting them

### Common Pitfalls to Avoid

- **Don't import Faker directly**: Always import from `faker-config.ts` to maintain determinism
- **Don't use probabilistic logic**: Use deterministic defaults, let tests override explicitly
- **Don't skip tests**: Generator tests catch type mismatches and invalid data early
- **Don't forget overrides**: Test that override parameter actually overrides generated values

---

## 🚀 Next Steps After Phase 1

Once all success criteria are met:

1. **Phase 2**: Create device fixture constants using these generators
2. **Phase 3**: Build API interceptors that consume fixtures
3. **Phase 4**: Write actual E2E tests for device discovery

**Validation**: Before moving to Phase 2, confirm you can:
- Import and use `generateDevice` in a Cypress test file
- Generate multiple devices with varied properties (due to faker sequence)
- Override specific properties while keeping others from defaults/faker
- Run generator tests successfully with 100% pass rate
- Verify same seed produces same data every time
