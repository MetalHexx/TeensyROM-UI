# Phase 2: Device Mock Fixtures

## 🎯 Objective

Create pre-built device fixture constants representing realistic TeensyROM device scenarios for E2E testing. These fixtures will provide deterministic, validated device data that can be consumed by API interceptors in Phase 3.

**Value**: Establishes a library of reusable device scenarios (single device, multiple devices, no devices, disconnected states, etc.) that provide consistent test data across all E2E tests. All fixtures use the Phase 1 generators to ensure type safety and determinism.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**

- [ ] [E2E Testing Plan](./E2E_PLAN.md) - Overall E2E testing strategy and phase overview
- [ ] [Phase 1 Implementation](./E2E_PLAN_P1.md) - Context on generators created in Phase 1
- [ ] [Phase 1 Test Data Documentation](../../../apps/teensyrom-ui-e2e/src/support/test-data/FAKE_TEST_DATA.md) - Generator usage patterns

**Standards & Guidelines:**

- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices

**API References:**

- [CartDto Type](../../../libs/data-access/api-client/src/lib/models/CartDto.ts) - Device DTO structure
- [DeviceState Type](../../../libs/data-access/api-client/src/lib/models/DeviceState.ts) - Device state enum values
- [CartStorageDto Type](../../../libs/data-access/api-client/src/lib/models/CartStorageDto.ts) - Storage DTO structure

---

## 📂 File Structure Overview

```
apps/teensyrom-ui-e2e/src/support/test-data/
├── FAKE_TEST_DATA.md                            📝 Modified - Add fixture documentation
├── faker-config.ts                              (Existing from Phase 1)
├── generators/
│   ├── device.generators.ts                     (Existing from Phase 1)
│   └── device.generators.spec.ts                (Existing from Phase 1)
└── fixtures/
    ├── README.md                                ✨ New - Fixture system documentation
    ├── fixture.types.ts                         ✨ New - MockDeviceFixture interface
    ├── devices.fixture.ts                       ✨ New - Device fixture constants
    ├── devices.fixture.spec.ts                  ✨ New - Fixture validation tests
    └── index.ts                                 ✨ New - Barrel exports
```

---

<details open>
<summary><h3>Task 1: Define Fixture Type Interface</h3></summary>

**Purpose**: Create the `MockDeviceFixture` interface that defines the structure for all device fixtures used throughout E2E tests.

**Related Documentation:**

- [E2E Testing Plan - Phase 2](./E2E_PLAN.md#phase-2-device-mock-fixtures) - Fixture structure definition

**Implementation Subtasks:**

- [ ] Create `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/fixture.types.ts`
- [ ] Import `CartDto` from `@teensyrom-nx/data-access/api-client`
- [ ] Define `MockDeviceFixture` interface with `devices` property (array of CartDto)
- [ ] Add JSDoc comments explaining fixture purpose and usage
- [ ] Add example JSDoc showing fixture structure

**Testing Subtask:**

- [ ] **Write Tests**: Verify fixture type structure (see Testing section below)

**Key Implementation Notes:**

- Interface is intentionally simple - just wraps an array of devices
- Designed to align with API response structure for `/api/devices`
- Future phases may extend this interface for more complex scenarios
- Use readonly array to prevent accidental mutation in tests

**Critical Interface Structure**:

```typescript
interface MockDeviceFixture {
  readonly devices: readonly CartDto[];
}
```

**Testing Focus for Task 1:**

**Behaviors to Test:**

- [ ] `MockDeviceFixture` interface compiles without errors
- [ ] Interface correctly types device arrays
- [ ] Readonly constraint prevents modification
- [ ] Type checking enforces CartDto structure

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for type validation testing

</details>

---

<details open>
<summary><h3>Task 2: Create Single Device Fixture</h3></summary>

**Purpose**: Create a fixture representing a single connected TeensyROM device with available storage - the most common "happy path" scenario.

**Related Documentation:**

- [Phase 1 Generators](./E2E_PLAN_P1.md#task-5-create-cartdto-generator) - Using generateDevice function

**Implementation Subtasks:**

- [ ] Create `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts`
- [ ] Import `generateDevice` from `../generators/device.generators`
- [ ] Import `MockDeviceFixture` from `./fixture.types`
- [ ] Import `DeviceState` from `@teensyrom-nx/data-access/api-client`
- [ ] Import `faker` from `../faker-config`
- [ ] Create `singleDevice` constant of type `MockDeviceFixture`
- [ ] Reset faker seed to 12345 before generation
- [ ] Use `generateDevice()` with default values (connected, available storage)
- [ ] Add JSDoc describing scenario: "Single connected device with available SD and USB storage"

**Testing Subtask:**

- [ ] **Write Tests**: Verify single device fixture (see Testing section below)

**Key Implementation Notes:**

- Use generator defaults (isConnected: true, deviceState: Connected, storage available: true)
- This represents the most common successful scenario
- Faker seed reset ensures this fixture always produces the same device
- No overrides needed - generator defaults match this scenario perfectly

**Testing Focus for Task 2:**

**Behaviors to Test:**

- [ ] Fixture contains exactly 1 device
- [ ] Device `isConnected` is true
- [ ] Device `deviceState` is `DeviceState.Connected`
- [ ] Device `isCompatible` is true
- [ ] Both `sdStorage` and `usbStorage` have `available: true`
- [ ] Device has valid `deviceId`, `comPort`, `name`, `fwVersion`
- [ ] Fixture structure matches `MockDeviceFixture` interface
- [ ] Multiple test runs produce identical device data

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for fixture validation patterns

</details>

---

<details open>
<summary><h3>Task 3: Create Multiple Devices Fixture</h3></summary>

**Purpose**: Create a fixture representing 3 connected TeensyROM devices with varied names and ports to test multi-device scenarios.

**Related Documentation:**

- [Phase 1 Generators](./E2E_PLAN_P1.md#task-5-create-cartdto-generator) - Using generateDevice with overrides

**Implementation Subtasks:**

- [ ] Add `multipleDevices` constant to `devices.fixture.ts`
- [ ] Reset faker seed to 12345 before generation
- [ ] Generate 3 devices using `generateDevice()` with default values
- [ ] Allow faker sequence to naturally produce different COM ports and names
- [ ] Add JSDoc describing scenario: "Three connected devices with unique ports and names"
- [ ] Export constant with explicit `MockDeviceFixture` type

**Testing Subtask:**

- [ ] **Write Tests**: Verify multiple devices fixture (see Testing section below)

**Key Implementation Notes:**

- Faker's seeded sequence naturally produces different values on each call
- Reset seed at start to ensure consistent fixture across runs
- All devices use default "connected" state - tests can override if needed
- Device names and ports will differ due to faker's sequential generation after seed reset

**Testing Focus for Task 3:**

**Behaviors to Test:**

- [ ] Fixture contains exactly 3 devices
- [ ] All devices have unique `deviceId` values
- [ ] All devices have unique `comPort` values
- [ ] All devices have unique `name` values
- [ ] All devices are connected (`isConnected: true`, `deviceState: Connected`)
- [ ] All devices have available storage (both SD and USB)
- [ ] Fixture structure matches `MockDeviceFixture` interface
- [ ] Multiple test runs produce identical set of 3 devices

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for fixture validation

</details>

---

<details open>
<summary><h3>Task 4: Create No Devices Fixture</h3></summary>

**Purpose**: Create a fixture representing the scenario where no TeensyROM devices are found - tests empty state handling.

**Related Documentation:**

- [E2E Testing Plan - Phase 2](./E2E_PLAN.md#phase-2-device-mock-fixtures) - Empty fixture requirement

**Implementation Subtasks:**

- [ ] Add `noDevices` constant to `devices.fixture.ts`
- [ ] Create fixture with empty `devices` array
- [ ] Add JSDoc describing scenario: "No devices found - tests empty state handling"
- [ ] Export constant with explicit `MockDeviceFixture` type

**Testing Subtask:**

- [ ] **Write Tests**: Verify no devices fixture (see Testing section below)

**Key Implementation Notes:**

- Simplest fixture - just an empty array
- Critical for testing UI empty states and "no devices found" messages
- No faker generation needed - static empty array

**Testing Focus for Task 4:**

**Behaviors to Test:**

- [ ] Fixture contains empty array (length 0)
- [ ] Fixture structure matches `MockDeviceFixture` interface
- [ ] Array is properly typed as `CartDto[]`

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for fixture validation

</details>

---

<details open>
<summary><h3>Task 5: Create Disconnected Device Fixture</h3></summary>

**Purpose**: Create a fixture with a device that was previously connected but has lost connection - tests connection loss handling.

**Related Documentation:**

- [DeviceState Enum](../../../libs/data-access/api-client/src/lib/models/DeviceState.ts) - ConnectionLost state
- [Phase 1 Generators](./E2E_PLAN_P1.md#task-5-create-cartdto-generator) - Using overrides

**Implementation Subtasks:**

- [ ] Add `disconnectedDevice` constant to `devices.fixture.ts`
- [ ] Reset faker seed to 12345 before generation
- [ ] Generate device using `generateDevice()` with overrides:
  - [ ] Set `isConnected: false`
  - [ ] Set `deviceState: DeviceState.ConnectionLost`
- [ ] Add JSDoc describing scenario: "Device that lost connection - tests reconnection workflows"
- [ ] Export constant with explicit `MockDeviceFixture` type

**Testing Subtask:**

- [ ] **Write Tests**: Verify disconnected device fixture (see Testing section below)

**Key Implementation Notes:**

- Device has valid properties (name, port, etc.) from previous connection
- Storage availability remains true (storage is still physically present)
- This tests UI's ability to show disconnected state and offer reconnection
- Override pattern demonstrates how to customize generator defaults

**Testing Focus for Task 5:**

**Behaviors to Test:**

- [ ] Fixture contains exactly 1 device
- [ ] Device `isConnected` is false
- [ ] Device `deviceState` is `DeviceState.ConnectionLost`
- [ ] Device still has valid `comPort`, `name`, `fwVersion` (from before disconnection)
- [ ] Storage objects still exist with valid properties
- [ ] Fixture structure matches `MockDeviceFixture` interface

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for fixture validation

</details>

---

<details open>
<summary><h3>Task 6: Create Unavailable Storage Fixture</h3></summary>

**Purpose**: Create a fixture with a connected device but unavailable storage - tests storage error handling.

**Related Documentation:**

- [Phase 1 Generators](./E2E_PLAN_P1.md#task-4-create-cartstoragedto-generator) - CartStorageDto generation
- [CartStorageDto Type](../../../libs/data-access/api-client/src/lib/models/CartStorageDto.ts) - Storage structure

**Implementation Subtasks:**

- [ ] Add `unavailableStorageDevice` constant to `devices.fixture.ts`
- [ ] Import `generateCartStorage` from `../generators/device.generators`
- [ ] Reset faker seed to 12345 before generation
- [ ] Generate device using `generateDevice()` with storage overrides:
  - [ ] Override `sdStorage` with `generateCartStorage({ available: false })`
  - [ ] Override `usbStorage` with `generateCartStorage({ available: false })`
- [ ] Add JSDoc describing scenario: "Connected device with unavailable storage - tests storage error handling"
- [ ] Export constant with explicit `MockDeviceFixture` type

**Testing Subtask:**

- [ ] **Write Tests**: Verify unavailable storage fixture (see Testing section below)

**Key Implementation Notes:**

- Device is connected but storage is not accessible (hardware issue, unmounted, etc.)
- Both SD and USB storage are unavailable in this scenario
- Tests UI's ability to show storage warnings and prevent file operations
- Demonstrates nested override pattern (overriding complex objects)

**Testing Focus for Task 6:**

**Behaviors to Test:**

- [ ] Fixture contains exactly 1 device
- [ ] Device `isConnected` is true
- [ ] Device `deviceState` is `DeviceState.Connected`
- [ ] Both `sdStorage.available` and `usbStorage.available` are false
- [ ] Storage objects have valid `deviceId` and `type` properties
- [ ] Fixture structure matches `MockDeviceFixture` interface

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for fixture validation

</details>

---

<details open>
<summary><h3>Task 7: Create Mixed State Devices Fixture</h3></summary>

**Purpose**: Create a fixture with multiple devices in different states (connected, busy, disconnected) - tests complex multi-device scenarios.

**Related Documentation:**

- [DeviceState Enum](../../../libs/data-access/api-client/src/lib/models/DeviceState.ts) - All device states
- [E2E Testing Plan](./E2E_PLAN.md#phase-2-device-mock-fixtures) - Multi-device testing

**Implementation Subtasks:**

- [ ] Add `mixedStateDevices` constant to `devices.fixture.ts`
- [ ] Reset faker seed to 12345 before generation
- [ ] Generate 3 devices with different states:
  - [ ] Device 1: Connected (default state)
  - [ ] Device 2: Busy (override `deviceState: DeviceState.Busy`)
  - [ ] Device 3: ConnectionLost (override `isConnected: false`, `deviceState: DeviceState.ConnectionLost`)
- [ ] Add JSDoc describing scenario: "Multiple devices in various states - tests state filtering and display"
- [ ] Export constant with explicit `MockDeviceFixture` type

**Testing Subtask:**

- [ ] **Write Tests**: Verify mixed state devices fixture (see Testing section below)

**Key Implementation Notes:**

- Represents realistic scenario where some devices work and others don't
- Tests UI's ability to differentiate and display different device states
- Each device should have unique port and name (faker sequence)
- Busy state indicates device is processing a command

**Testing Focus for Task 7:**

**Behaviors to Test:**

- [ ] Fixture contains exactly 3 devices
- [ ] Device 1 has `deviceState: DeviceState.Connected` and `isConnected: true`
- [ ] Device 2 has `deviceState: DeviceState.Busy` and `isConnected: true`
- [ ] Device 3 has `deviceState: DeviceState.ConnectionLost` and `isConnected: false`
- [ ] All devices have unique `deviceId`, `comPort`, and `name`
- [ ] Fixture structure matches `MockDeviceFixture` interface
- [ ] State variety covers key testing scenarios

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for fixture validation

</details>

---

<details open>
<summary><h3>Task 8: Create Barrel Exports</h3></summary>

**Purpose**: Create index file that exports all fixtures for clean imports in tests.

**Related Documentation:**

- [Coding Standards](../../CODING_STANDARDS.md) - Barrel export patterns

**Implementation Subtasks:**

- [ ] Create `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/index.ts`
- [ ] Export all fixture constants from `devices.fixture.ts`
- [ ] Export `MockDeviceFixture` interface from `fixture.types.ts`
- [ ] Add file-level JSDoc explaining fixture library usage

**Testing Subtask:**

- [ ] **Verify Exports**: Confirm barrel exports resolve correctly (manual verification)

**Key Implementation Notes:**

- Enables clean imports: `import { singleDevice, multipleDevices } from '../fixtures'`
- Re-export types alongside fixtures for convenience
- Keep exports organized and alphabetical

**Barrel Export Pattern**:

```typescript
export * from './fixture.types';
export * from './devices.fixture';
```

**Testing Focus for Task 8:**

**Behaviors to Test:**

- [ ] All fixtures importable via barrel export
- [ ] `MockDeviceFixture` type importable via barrel export
- [ ] No TypeScript errors when importing from `./fixtures`
- [ ] Imports work in test files

</details>

---

<details open>
<summary><h3>Task 9: Create Fixture Documentation</h3></summary>

**Purpose**: Document the fixture system, available fixtures, and usage patterns for E2E test authors.

**Related Documentation:**

- [E2E Testing Plan](./E2E_PLAN.md) - Overall testing context
- [Phase 1 Documentation](../../../apps/teensyrom-ui-e2e/src/support/test-data/FAKE_TEST_DATA.md) - Related generator docs

**Implementation Subtasks:**

- [ ] Create `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/README.md`
- [ ] Document fixture system purpose and architecture
- [ ] List all available device fixtures with descriptions
- [ ] Provide usage examples showing how to import and use fixtures
- [ ] Show examples of using fixtures with interceptors (preview of Phase 3)
- [ ] Document fixture properties and structure
- [ ] Add guidelines for when to create new fixtures
- [ ] Include troubleshooting section

**Testing Subtask:**

- [ ] **Review Documentation**: Validate clarity and completeness (manual review)

**Key Implementation Notes:**

- Target audience: developers writing E2E tests
- Provide copy-paste-ready examples
- Link to Phase 1 generator docs for context
- Show both simple and complex usage patterns
- Document relationship between fixtures and interceptors

**Documentation Sections:**

1. **Overview**: Fixture system purpose
2. **Available Fixtures**: Table listing all fixtures with scenarios
3. **Fixture Structure**: Explain `MockDeviceFixture` interface
4. **Usage Examples**: Import and usage patterns
5. **Integration with Interceptors**: Preview of Phase 3 usage
6. **Best Practices**: Guidelines for fixture usage
7. **Creating New Fixtures**: When and how to extend
8. **Troubleshooting**: Common issues and solutions

</details>

---

<details open>
<summary><h3>Task 10: Update Test Data Documentation</h3></summary>

**Purpose**: Update the main test data documentation to reference the new fixture system.

**Related Documentation:**

- [Phase 1 Documentation](../../../apps/teensyrom-ui-e2e/src/support/test-data/FAKE_TEST_DATA.md) - Existing test data docs

**Implementation Subtasks:**

- [ ] Open `apps/teensyrom-ui-e2e/src/support/test-data/FAKE_TEST_DATA.md`
- [ ] Add section on fixture system after generator documentation
- [ ] Link to `fixtures/README.md` for detailed fixture documentation
- [ ] Show quick example of using fixtures with generators
- [ ] Update table of contents if present

**Testing Subtask:**

- [ ] **Review Documentation**: Verify links and examples work (manual review)

**Key Implementation Notes:**

- Keep this update lightweight - detailed docs are in fixtures/README.md
- Show how fixtures build on generators from Phase 1
- Provide navigation path to fixture documentation

</details>

---

## 🗂️ Files Modified or Created

**New Files:**

- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/README.md` - Fixture system documentation
- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/fixture.types.ts` - MockDeviceFixture interface
- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts` - Device fixture constants
- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.spec.ts` - Fixture validation tests
- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/index.ts` - Barrel exports

**Modified Files:**

- `apps/teensyrom-ui-e2e/src/support/test-data/FAKE_TEST_DATA.md` - Add fixture system section

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
>
> - **Test fixture integrity**: Validate structure, required properties, and type correctness
> - **Test fixture determinism**: Same faker seed = same fixture data across runs
> - **Test fixture variety**: Ensure different fixtures provide distinct scenarios
> - **Test fixture realism**: Values should be plausible for TeensyROM devices

### Where Tests Are Written

**Tests are embedded in Tasks 1-7** with:

- **Testing Subtask**: "Write Tests" checkbox in each task
- **Testing Focus**: "Behaviors to Test" section with specific validation criteria
- **Testing Reference**: Links to testing documentation

### Test File Location

All fixture validation tests live in:

- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.spec.ts`

### Test Execution Commands

```bash
# Run fixture tests (uses Phase 1 Vitest config)
pnpm nx test teensyrom-ui-e2e

# Run in watch mode during development
pnpm nx test teensyrom-ui-e2e --watch

# Run with coverage
pnpm nx test teensyrom-ui-e2e --coverage
```

**Note**: These tests run in the E2E project using the Vitest configuration created in Phase 1.

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**

- [ ] `MockDeviceFixture` interface created and exported
- [ ] `singleDevice` fixture created (1 connected device)
- [ ] `multipleDevices` fixture created (3 connected devices)
- [ ] `noDevices` fixture created (empty array)
- [ ] `disconnectedDevice` fixture created (connection lost state)
- [ ] `unavailableStorageDevice` fixture created (storage unavailable)
- [ ] `mixedStateDevices` fixture created (3 devices in varied states)
- [ ] All fixtures exported via barrel index
- [ ] Fixture README documentation completed
- [ ] Test data documentation updated with fixture references

**Testing Requirements:**

- [ ] All fixture validation tests written and passing
- [ ] Tests verify fixture structure matches `MockDeviceFixture` interface
- [ ] Tests verify device properties are properly populated
- [ ] Tests verify fixture determinism (same data on multiple runs)
- [ ] Tests verify device uniqueness in multi-device fixtures
- [ ] Tests pass consistently with zero failures
- [ ] All tests run successfully via `pnpm nx test teensyrom-ui-e2e`

**Type Safety:**

- [ ] No TypeScript errors in fixture files
- [ ] All fixtures properly typed as `MockDeviceFixture`
- [ ] Fixtures use proper DTO types from API client
- [ ] Barrel exports resolve correctly

**Quality Checks:**

- [ ] Linting passes with no errors (`pnpm nx lint teensyrom-ui-e2e`)
- [ ] Code formatting is consistent
- [ ] JSDoc comments present on all exported fixtures
- [ ] No console warnings when running tests

**Documentation:**

- [ ] Fixture README explains system architecture clearly
- [ ] All fixtures listed with scenario descriptions
- [ ] Usage examples are runnable and clear
- [ ] Integration examples preview Phase 3 interceptor usage
- [ ] Troubleshooting section addresses common issues

**Ready for Next Phase:**

- [ ] All success criteria met
- [ ] Fixtures produce realistic device scenarios
- [ ] Tests demonstrate fixtures are structurally valid
- [ ] Documentation enables test authors to use fixtures
- [ ] Ready to build interceptors in Phase 3

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Simple Interface**: `MockDeviceFixture` is intentionally minimal (just device array) - complex scenarios use varied device properties, not complex fixture structure
- **Seed Reset per Fixture**: Each fixture resets faker seed to ensure consistent data - this makes fixtures truly deterministic
- **Override Pattern**: Fixtures demonstrate how to use generator overrides for specific scenarios (disconnected, unavailable storage, etc.)
- **Variety over Volume**: 7 fixtures cover key scenarios without overwhelming test authors - quality over quantity
- **Readonly Arrays**: Fixtures use readonly to prevent accidental mutation in tests - fixtures should be immutable reference data

### Implementation Constraints

- **Generator Dependency**: All fixtures must use Phase 1 generators - never manually construct DTOs
- **Type Alignment**: Fixtures must match API client DTO types exactly - any API changes require fixture updates
- **Faker Determinism**: Seed resets must happen before each fixture generation to maintain consistency
- **No Runtime State**: Fixtures are static constants - they don't track state or change during tests

### Future Enhancements

- **Device Groups**: May add fixtures for device groups or clusters in future phases
- **Firmware Variants**: Could add fixtures for different firmware versions (compatible/incompatible)
- **Error State Fixtures**: Could add fixtures for devices in error states beyond ConnectionLost
- **Large Device Sets**: Could add fixtures with 10+ devices for performance testing

### Fixture Naming Conventions

- **Descriptive Names**: Fixture names describe the scenario, not the data (e.g., `disconnectedDevice` not `device1`)
- **Consistent Suffix**: All fixtures end with logical type (e.g., `Device` or `Devices`)
- **camelCase**: Follow TypeScript naming conventions
- **No Abbreviations**: Use full words for clarity (e.g., `unavailableStorageDevice` not `unavailStrgDev`)

### Fixture Usage Guidelines

**When to use existing fixtures:**

- Default/happy path testing → `singleDevice` or `multipleDevices`
- Empty state testing → `noDevices`
- Connection error testing → `disconnectedDevice`
- Storage error testing → `unavailableStorageDevice`
- Complex state testing → `mixedStateDevices`

**When to create custom fixtures:**

- Specific device property combinations not covered by existing fixtures
- Test-specific edge cases that don't generalize
- Temporary fixtures for debugging specific scenarios

### External References

- [Phase 1 Plan](./E2E_PLAN_P1.md) - Generator implementation details
- [E2E Testing Plan](./E2E_PLAN.md) - Overall testing strategy
- [CartDto Definition](../../../libs/data-access/api-client/src/lib/models/CartDto.ts) - Device DTO structure

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

</details>

---

## 💡 Implementation Tips

### Before Starting

1. **Verify Phase 1 Complete**: Ensure all Phase 1 generators and tests are working
2. **Review Generator API**: Understand `generateDevice()` and `generateCartStorage()` usage patterns
3. **Check DTO Structures**: Review API client types to understand device properties
4. **Plan Fixture Scenarios**: Think through what device scenarios are most valuable for testing

### During Implementation

1. **Follow Task Order**: Complete tasks sequentially (types → fixtures → tests → docs)
2. **Reset Seed Consistently**: Always reset faker seed before generating fixture data
3. **Test as You Go**: Write fixture validation tests immediately after creating each fixture
4. **Verify Determinism**: Run tests multiple times to confirm fixtures produce identical data
5. **Check Type Safety**: Ensure TypeScript validates fixture structures

### Testing Strategy

1. **Structure Tests**: Verify fixtures match `MockDeviceFixture` interface
2. **Property Tests**: Validate required device properties are populated
3. **Determinism Tests**: Confirm same fixture data across multiple test runs
4. **Uniqueness Tests**: Verify multi-device fixtures have unique IDs/ports/names
5. **Edge Case Tests**: Validate empty fixture and error state fixtures

### After Task Completion

1. **Run Linter**: Execute `pnpm nx lint teensyrom-ui-e2e` after completing each fixture
2. **Verify Imports**: Test that barrel exports work in a dummy test file
3. **Review Documentation**: Ensure examples in docs actually work by testing them
4. **Update Discovery Notes**: Document any unexpected findings

### Common Pitfalls to Avoid

- **Don't forget seed reset**: Each fixture must reset faker seed for consistency
- **Don't manually construct DTOs**: Always use generators, never create objects by hand
- **Don't skip tests**: Fixture validation catches structural issues early
- **Don't skip readonly**: Use readonly arrays to prevent mutation
- **Don't over-complicate**: Keep fixtures simple - use generator overrides for variety

---

## 🚀 Next Steps After Phase 2

Once all success criteria are met:

1. **Phase 3**: Create API interceptors that consume these fixtures
2. **Phase 4**: Write device discovery E2E tests using fixtures + interceptors
3. **Phase 5**: Write device connection E2E tests

**Validation**: Before moving to Phase 3, confirm you can:

- Import any fixture in a test file via barrel export
- Access fixture devices array and individual device properties
- Verify fixture data is identical across multiple test runs
- All fixture validation tests pass with 100% success rate
- Documentation clearly explains how to use each fixture
