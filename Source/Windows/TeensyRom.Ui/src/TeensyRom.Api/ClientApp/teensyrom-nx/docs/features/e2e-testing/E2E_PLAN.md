# Mock Backend Fixture System Plan

## 🎯 Project Objective

Build a type-safe mock backend fixture system that provides realistic device and storage data for Cypress E2E tests. This enables testing of device management workflows by intercepting API calls and returning consistent, deterministic responses without requiring a live backend.

**Primary Goal**: Test device discovery, connection, and management workflows starting from the device view.

**System Value**: Creates reusable mock data structures (generators, fixtures, interceptors) that provide realistic API responses. All test data will be deterministic through fixed-seed Faker for reproducible test runs.

**Core Principles**:

- **Static Fixtures**: Pre-built data structures, not runtime state tracking
- **Device-First**: Start with device management before player features
- **API-Focused**: Fixtures designed to support specific API endpoint responses
- **Validated**: Fixtures tested for structural integrity and type correctness
- **Incremental**: Build up from device loading → connection → storage → player

---

## 📋 Implementation Phases

### Phase 0: Cypress Sanity Check

**Objective**: Verify Cypress E2E testing is properly configured and can run a trivial test against the existing application before investing in fixture infrastructure.

**Deliverables**:

- [ ] One simple smoke test that navigates to device view and verifies page renders
- [ ] Cypress runs successfully via Nx command
- [ ] Test passes consistently
- [ ] Validates E2E infrastructure is working

**File Structure**:

```
apps/teensyrom-ui-e2e/src/e2e/
└── smoke.cy.ts                    # Simple navigation test
```

**Test Requirements**:

- Navigate to `/devices` route
- Verify page loads without errors
- Assert on presence of a key element (heading, container, etc.)
- No API mocking needed - just basic navigation and rendering

**Success Criteria**:

- [ ] Test file created
- [ ] Cypress test runs via `npx nx e2e teensyrom-ui-e2e`
- [ ] Test passes (green)
- [ ] No console errors during test execution

**Why This Phase**:
Before building complex fixture infrastructure, confirm Cypress integration works with the existing Angular app. This catches configuration issues early and provides a working baseline.

---

### Phase 1: Faker Setup and DTO Generators

**Objective**: Install Faker and create generator functions for device-related API DTOs (CartDto, DeviceState, etc.)

**Deliverables**:

- [ ] Faker installed with fixed seed (12345)
- [ ] Generator functions for device API client DTO types
- [ ] Generator validation tests
- [ ] Documentation

**File Structure**:

```
apps/teensyrom-ui-e2e/src/support/test-data/
├── README.md
├── faker-config.ts
└── generators/
    ├── device.generators.ts        # CartDto, DeviceState
    └── device.generators.spec.ts
```

**Key Functions Needed**:

- `generateDevice(overrides?: Partial<CartDto>): CartDto`
- `generateDeviceState(overrides?: Partial<DeviceState>): DeviceState`

**Testing Focus**: Determinism (same seed = same output), required field population, type safety

---

### Phase 2: Device Mock Fixtures

**Objective**: Create pre-built device fixtures representing realistic TeensyROM device scenarios

**Deliverables**:

- [ ] MockDeviceFixture interface
- [ ] Device fixture constants (single device, multiple devices, no devices)
- [ ] Fixture validation tests
- [ ] Documentation

**File Structure**:

```
apps/teensyrom-ui-e2e/src/support/test-data/fixtures/
├── README.md
├── fixture.types.ts            # MockDeviceFixture interface
├── devices.fixture.ts          # singleDevice, multipleDevices, noDevices exports
├── fixtures.spec.ts            # Validation tests
└── index.ts                    # Barrel exports
```

**MockDeviceFixture Structure**:

```typescript
interface MockDeviceFixture {
  devices: CartDto[]; // Array of available devices
}
```

**Fixture Contents**:

- **singleDevice**: One TeensyROM device with realistic properties
- **multipleDevices**: 2-3 devices with different ports/names
- **noDevices**: Empty array (edge case for "no devices found")

**Testing Focus**: Device property completeness, realistic values (ports, firmware, etc.)

---

### Phase 3: Device API Interceptors

**Objective**: Create interceptor functions that consume device fixtures and return realistic API responses

**Deliverables**:

- [ ] Interceptor functions for device endpoints
- [ ] Error mode support
- [ ] Alias naming conventions
- [ ] Documentation

**File Structure**:

```
apps/teensyrom-ui-e2e/src/support/interceptors/
├── README.md
└── device.interceptors.ts      # interceptFindDevices, interceptConnectDevice, interceptGetDeviceState
```

**Key Functions Needed**:

- `interceptFindDevices(options?: { fixture?: MockDeviceFixture; errorMode?: boolean })`
- `interceptConnectDevice(options?: { device?: CartDto; errorMode?: boolean })`
- `interceptGetDeviceState(options?: { device?: CartDto; errorMode?: boolean })`

**Interceptor Behavior**:

- `interceptFindDevices`: Return fixture.devices array
- `interceptConnectDevice`: Return connection success with device data
- `interceptGetDeviceState`: Return device state information

**Alias Conventions**: `@findDevices`, `@connectDevice`, `@getDeviceState`

---

### Phase 4: Device Discovery Test

**Objective**: Test that device view can load and display TeensyROM devices using mock fixtures

**Deliverables**:

- [ ] Test file for device discovery workflow
- [ ] Tests for single device, multiple devices, no devices scenarios
- [ ] Tests verify UI displays device information correctly
- [ ] All tests passing consistently

**File Structure**:

```
apps/teensyrom-ui-e2e/src/e2e/devices/
├── device-discovery.cy.ts      # Device loading tests
└── README.md                   # Test documentation
```

**Test Scenarios**:

- Navigate to `/devices` route
- Intercept `findDevices` API call with fixture
- Verify device cards render with correct device names/info
- Test single device scenario
- Test multiple devices scenario
- Test no devices scenario (empty state)

**Success Criteria**:

- [ ] Device view loads successfully
- [ ] Devices from fixture display in UI
- [ ] Device names, ports, firmware versions visible
- [ ] Empty state shows when no devices in fixture
- [ ] Tests pass consistently with zero flakiness

---

### Phase 5: Device Connection Test

**Objective**: Test device connection workflow using mock interceptors

**Deliverables**:

- [ ] Test file for device connection workflow
- [ ] Tests for successful connection, connection failure
- [ ] Tests verify connection state updates in UI
- [ ] All tests passing consistently

**File Structure**:

```
apps/teensyrom-ui-e2e/src/e2e/devices/
├── device-connection.cy.ts     # Device connection tests
└── README.md                   # Updated documentation
```

**Test Scenarios**:

- Load device view with devices
- Click connect button on a device
- Intercept `connectDevice` API call
- Verify connection success state in UI
- Test connection error scenario
- Verify error message displays correctly

**Success Criteria**:

- [ ] Connect button triggers API call
- [ ] Successful connection updates device state
- [ ] Connected device shows connected indicator
- [ ] Error scenario displays error message
- [ ] Tests pass consistently

---

### Future Phases (Out of Scope for Now)

**Phase 6**: Storage fixture system for file/directory structures  
**Phase 7**: Player random launch testing  
**Phase 8**: Additional player workflows

These will be planned after device management testing is working.

---

## 🏗️ Architecture Overview

### API Endpoints to Support (Device-Focused)

**Phase 1-5 Scope**:

- `GET /api/devices` → Returns FindDevicesResponse (list of CartDto)
- `POST /api/devices/{id}/connect` → Returns ConnectDeviceResponse
- `GET /api/devices/{id}` → Returns device state

**Future Phases**:

- Storage/directory endpoints
- Player launch endpoints

### Data Flow

```
Fixture (static data)
    ↓
Interceptor (consumes fixture, responds to API call)
    ↓
Test (triggers UI interaction, verifies response)
```

### Type Filtering Logic

When `launchRandom` interceptor receives request:

1. Extract `filterType` from request body (Program, SID, Image, or null/undefined for All)
2. Flatten all files from fixture.directories
3. Filter by type if specified
4. Pick random file using seeded Faker
5. Return as LaunchRandomResponse

---

## ✅ Success Criteria

**Phase 0**:

- [ ] Cypress runs successfully against existing application
- [ ] Simple smoke test passes
- [ ] No configuration issues blocking E2E testing
- [ ] Ready to proceed with fixture infrastructure

**Phase 1**:

- [ ] Faker installed and configured with seed 12345
- [ ] Generator functions return properly typed API client DTOs
- [ ] Generator tests validate determinism and required fields
- [ ] Documentation explains generator usage

**Phase 1.5**:

- [ ] 5 fixture constants created and exported
- [ ] All fixtures follow MockStorageFixture interface
- [ ] Fixture validation tests pass (path consistency, type correctness)
- [ ] Programs fixture has 8-12 Program files organized by genre
- [ ] Music fixture has 8-12 SID files organized by composer
- [ ] Images fixture has 6-10 Image files
- [ ] Mixed fixture has 2-3 files of each type
- [ ] Empty fixture has zero files

**Phase 3**:

- [ ] Three interceptor functions created
- [ ] Interceptors consume fixtures and return correct API responses
- [ ] Type filtering works in `interceptLaunchRandom`
- [ ] Error mode supported for negative testing
- [ ] Aliases registered consistently
- [ ] Documentation explains interceptor options

---

## 📝 Notes

### What This Plan Covers

✅ Cypress sanity check (Phase 0)  
✅ Mock backend data layer (generators, fixtures, interceptors)  
✅ Support for random file launch with type filtering  
✅ Device connection mocking  
✅ Directory listing mocking

### What This Plan Does NOT Cover

❌ Test suites or comprehensive test writing  
❌ Page objects  
❌ Setup hooks  
❌ Complex UI interactions  
❌ Navigation testing  
❌ History features  
❌ Multiple devices

These will be addressed in future planning sessions once the fixture foundation is established.

### Key Design Decisions

- **Phase 0 First**: Validate E2E setup before building fixtures
- **Fixed Seed (12345)**: Ensures 100% reproducible test data
- **Type-Specific Fixtures**: Separate fixtures by file type for focused testing
- **Static Data**: Fixtures are pre-built constants, not generated at runtime
- **Validated Fixtures**: Tests ensure fixtures are structurally sound before use
- **Simple Interceptors**: Straightforward functions that lookup data from fixtures

---

## 🚀 Next Steps

**To Start Phase 0**:

1. Verify Cypress is configured in `apps/teensyrom-ui-e2e`
2. Create `apps/teensyrom-ui-e2e/src/e2e/smoke.cy.ts`
3. Write simple test: navigate to `/devices`, assert page renders
4. Run: `npx nx e2e teensyrom-ui-e2e`
5. Verify test passes

**Once Phase 0 Passes**:
Proceed to Phase 1 (Faker + Generators)

**Phase Planning Sessions**:

- Each phase will have a detailed planning session using PHASE_TEMPLATE.md
- Planning will define specific tasks, testing approach, and implementation details
- Focus on one phase at a time

---
