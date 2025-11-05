# Phase 3: Device API Interceptors

## 🎯 Objective

Create Cypress interceptor functions that consume Phase 2 device fixtures and return realistic API responses, enabling E2E tests to simulate backend interactions without requiring a live server. This phase builds the bridge between static fixture data and dynamic test scenarios.

**Value Delivered**: Tests can now mock complete device workflows (discovery, connection, state retrieval) by calling simple interceptor functions that handle API response structure, HTTP status codes, and request aliasing automatically.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**

- [x] [E2E Testing Plan](./E2E_PLAN.md) - High-level feature plan
- [x] [Phase 2 Fixtures](../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts) - Available fixtures to consume

**Standards & Guidelines:**

- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices

**External References:**

- [ ] [Cypress Intercept Documentation](https://docs.cypress.io/api/commands/intercept) - cy.intercept API reference
- [ ] [Generated API Client](../../libs/data-access/api-client/src/lib/apis/DevicesApiService.ts) - API endpoint structure

---

## 📂 File Structure Overview

> Showing new files (✨) and existing files to reference

```
apps/teensyrom-ui-e2e/src/support/
├── test-data/                                      # Existing from Phase 2
│   ├── fixtures/
│   │   ├── devices.fixture.ts                      # Phase 2 fixtures to consume
│   │   └── fixture.types.ts                        # MockDeviceFixture interface
│   └── generators/
│       └── device.generators.ts                    # Generators for error scenarios
└── interceptors/                                   ✨ New directory
    ├── README.md                                   ✨ New - Interceptor usage guide
    ├── device.interceptors.ts                      ✨ New - Device endpoint interceptors
    └── device.interceptors.spec.ts                 ✨ New - Interceptor validation tests
```

---

## 📋 Implementation Guidelines

> **IMPORTANT - Code Reference Policy:**
>
> - Focus on **WHAT** to implement (function names, parameters, return types)
> - Use small snippets (2-5 lines) only for critical type definitions
> - Link to Cypress docs for detailed API usage
> - Describe behavior over showing implementation

> **IMPORTANT - Testing Policy:**
>
> - **Behavioral testing**: Verify interceptors register correct routes and return correct data
> - Tests embedded in each task as subtasks
> - Test observable outcomes: correct aliases, correct response bodies, correct status codes

---

<details open>
<summary><h3>Task 1: Create Interceptors Directory and Documentation</h3></summary>

**Purpose**: Establish interceptor file structure and create documentation explaining interceptor patterns, usage, and conventions for Phase 4 test authors.

**Related Documentation:**

- [Cypress Intercept Guide](https://docs.cypress.io/api/commands/intercept) - Intercept API patterns
- [Phase 2 Fixtures README](../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/README.md) - Available fixtures

**Implementation Subtasks:**

- [ ] **Create interceptors directory**: `apps/teensyrom-ui-e2e/src/support/interceptors/`
- [ ] **Create README.md**: Document interceptor purpose, naming conventions, and usage examples
- [ ] **Document alias conventions**: Explain `@findDevices`, `@connectDevice`, `@getDeviceState` pattern
- [ ] **Document error mode usage**: Explain how `errorMode: true` simulates API failures
- [ ] **Document fixture parameter**: Explain how to override default fixtures

**Testing Subtask:**

- [ ] **Write Tests**: N/A for documentation task

**Key Implementation Notes:**

- README should include code examples showing interceptor usage in tests
- Document the options pattern used by all interceptor functions
- Explain relationship between fixtures and interceptors
- Include examples of both success and error scenarios

**README Structure** (key sections to include):

```markdown
# Device API Interceptors

## Purpose

[Explain what interceptors do and why they exist]

## Usage Pattern

[Show basic usage example]

## Interceptor Functions

[List each function with signature and description]

## Options

- fixture: Override default fixture
- errorMode: Simulate API errors

## Alias Conventions

[Explain @findDevices, @connectDevice, @getDeviceState]

## Examples

[Success scenario, error scenario, custom fixture]
```

</details>

---

<details open>
<summary><h3>Task 2: Implement Find Devices Interceptor</h3></summary>

**Purpose**: Create interceptor for `GET /api/devices` endpoint that returns device discovery responses using Phase 2 fixtures.

**Related Documentation:**

- [DevicesApiService.findDevices](../../libs/data-access/api-client/src/lib/apis/DevicesApiService.ts#L155) - API method signature
- [singleDevice fixture](../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts) - Default fixture

**Implementation Subtasks:**

- [ ] **Create device.interceptors.ts**: Create interceptor file in interceptors directory
- [ ] **Add imports**: Import Cypress types, fixture types, and Phase 2 fixtures
- [ ] **Create InterceptFindDevicesOptions interface**: Define options with `fixture?` and `errorMode?` properties
- [ ] **Create interceptFindDevices function**: Implement function that calls `cy.intercept` with correct route
- [ ] **Default to singleDevice fixture**: Use `singleDevice` when no fixture provided
- [ ] **Register alias @findDevices**: Use `as('findDevices')` for test assertions
- [ ] **Support error mode**: Return 500 status with error body when `errorMode: true`

**Testing Subtask:**

- [ ] **Write Tests**: Test interceptor registration and response structure (see Testing Focus below)

**Key Implementation Notes:**

- Route must match DevicesApiService: `GET /api/devices*` (wildcard for query params)
- Response body structure matches `FindDevicesResponse` from API client
- Error mode should return realistic error response (500 status, ProblemDetails body)
- Fixture parameter allows test-specific device scenarios

**Critical Interface**:

```typescript
interface InterceptFindDevicesOptions {
  fixture?: MockDeviceFixture;
  errorMode?: boolean;
}
```

**Testing Focus for Task 2:**

> Focus on **behavioral testing** - verify interceptor registers correctly and returns expected data

**Behaviors to Test:**

- [ ] **Default behavior**: Intercepts `GET /api/devices` and returns `singleDevice` fixture
- [ ] **Custom fixture**: Returns provided fixture when `fixture` option specified
- [ ] **Error mode**: Returns 500 status when `errorMode: true`
- [ ] **Alias registration**: Registers `@findDevices` alias for test assertions
- [ ] **Response structure**: Response body matches `FindDevicesResponse` shape

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns
- Use `cy.wait('@findDevices')` to verify intercept registration
- Assert on response body structure and status codes

</details>

---

<details open>
<summary><h3>Task 3: Implement Connect Device Interceptor</h3></summary>

**Purpose**: Create interceptor for `POST /api/devices/{deviceId}/connect` endpoint that returns connection success/failure responses.

**Related Documentation:**

- [DevicesApiService.connectDevice](../../libs/data-access/api-client/src/lib/apis/DevicesApiService.ts#L104) - API method signature

**Implementation Subtasks:**

- [ ] **Create InterceptConnectDeviceOptions interface**: Define options with `device?` and `errorMode?` properties
- [ ] **Create interceptConnectDevice function**: Implement function that calls `cy.intercept` with correct route
- [ ] **Default to first device from singleDevice**: Extract `singleDevice.devices[0]` as default device
- [ ] **Match dynamic deviceId in URL**: Use wildcard pattern to match any deviceId parameter
- [ ] **Register alias @connectDevice**: Use `as('connectDevice')` for test assertions
- [ ] **Support error mode**: Return 500 status with error body when `errorMode: true`
- [ ] **Return ConnectDeviceResponse structure**: Include device data in response body

**Testing Subtask:**

- [ ] **Write Tests**: Test interceptor with various device scenarios (see Testing Focus below)

**Key Implementation Notes:**

- Route pattern: `POST /api/devices/*/connect` (wildcard for deviceId)
- Response includes connected device information (CartDto)
- Error mode simulates connection failures (device not found, communication error, etc.)
- Device parameter allows specifying which device was connected

**Critical Interface**:

```typescript
interface InterceptConnectDeviceOptions {
  device?: CartDto;
  errorMode?: boolean;
}
```

**Testing Focus for Task 3:**

**Behaviors to Test:**

- [ ] **Default behavior**: Intercepts connect endpoint and returns first device from `singleDevice`
- [ ] **Custom device**: Returns provided device when `device` option specified
- [ ] **Error mode**: Returns 500 status when `errorMode: true`
- [ ] **Alias registration**: Registers `@connectDevice` alias
- [ ] **Response structure**: Response body matches `ConnectDeviceResponse` shape
- [ ] **Dynamic URL matching**: Intercepts requests with any deviceId value

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for testing approach
- Test with different deviceId values to verify wildcard matching
- Verify error response structure matches ProblemDetails

</details>

---

<details open>
<summary><h3>Task 4: Implement Disconnect Device Interceptor</h3></summary>

**Purpose**: Create interceptor for `DELETE /api/devices/{deviceId}` endpoint that returns disconnection responses.

**Related Documentation:**

- [DevicesApiService.disconnectDevice](../../libs/data-access/api-client/src/lib/apis/DevicesApiService.ts#L119) - API method signature

**Implementation Subtasks:**

- [ ] **Create InterceptDisconnectDeviceOptions interface**: Define options with `errorMode?` property
- [ ] **Create interceptDisconnectDevice function**: Implement function that calls `cy.intercept` with correct route
- [ ] **Match dynamic deviceId in URL**: Use wildcard pattern to match any deviceId parameter
- [ ] **Register alias @disconnectDevice**: Use `as('disconnectDevice')` for test assertions
- [ ] **Support error mode**: Return 500 status with error body when `errorMode: true`
- [ ] **Return DisconnectDeviceResponse structure**: Include success message in response

**Testing Subtask:**

- [ ] **Write Tests**: Test interceptor registration and response structure (see Testing Focus below)

**Key Implementation Notes:**

- Route pattern: `DELETE /api/devices/*` (wildcard for deviceId)
- Response includes success confirmation
- Error mode simulates disconnection failures
- No device parameter needed - disconnection confirms by deviceId only

**Critical Interface**:

```typescript
interface InterceptDisconnectDeviceOptions {
  errorMode?: boolean;
}
```

**Testing Focus for Task 4:**

**Behaviors to Test:**

- [ ] **Default behavior**: Intercepts disconnect endpoint and returns success response
- [ ] **Error mode**: Returns 500 status when `errorMode: true`
- [ ] **Alias registration**: Registers `@disconnectDevice` alias
- [ ] **Response structure**: Response body matches `DisconnectDeviceResponse` shape
- [ ] **Dynamic URL matching**: Intercepts requests with any deviceId value

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for testing approach

</details>

---

<details open>
<summary><h3>Task 5: Implement Ping Device Interceptor</h3></summary>

**Purpose**: Create interceptor for `POST /api/devices/{deviceId}/ping` endpoint that returns ping responses for testing device state.

**Related Documentation:**

- [DevicesApiService.pingDevice](../../libs/data-access/api-client/src/lib/apis/DevicesApiService.ts) - API method signature

**Implementation Subtasks:**

- [ ] **Create InterceptPingDeviceOptions interface**: Define options with `isAlive?` and `errorMode?` properties
- [ ] **Create interceptPingDevice function**: Implement function that calls `cy.intercept` with correct route
- [ ] **Default to alive state**: Return success when `isAlive` not specified
- [ ] **Match dynamic deviceId in URL**: Use wildcard pattern to match any deviceId parameter
- [ ] **Register alias @pingDevice**: Use `as('pingDevice')` for test assertions
- [ ] **Support error mode**: Return 500 status when `errorMode: true` or `isAlive: false`

**Testing Subtask:**

- [ ] **Write Tests**: Test ping responses for alive/dead states (see Testing Focus below)

**Key Implementation Notes:**

- Route pattern: `POST /api/devices/*/ping` (wildcard for deviceId)
- `isAlive: false` simulates device not responding (different from server error)
- Error mode simulates server/communication errors
- Used for connection health checks in tests

**Critical Interface**:

```typescript
interface InterceptPingDeviceOptions {
  isAlive?: boolean;
  errorMode?: boolean;
}
```

**Testing Focus for Task 5:**

**Behaviors to Test:**

- [ ] **Default behavior**: Returns success ping response (device alive)
- [ ] **Device not alive**: Returns failure when `isAlive: false`
- [ ] **Error mode**: Returns 500 status when `errorMode: true`
- [ ] **Alias registration**: Registers `@pingDevice` alias
- [ ] **Response structure**: Response body matches `PingDeviceResponse` shape

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for testing approach

</details>

---

<details open>
<summary><h3>Task 6: Create Barrel Export</h3></summary>

**Purpose**: Create index.ts to export all interceptor functions for clean imports in test files.

**Implementation Subtasks:**

- [ ] **Create index.ts**: Create barrel export file in interceptors directory
- [ ] **Export all interceptor functions**: Re-export `interceptFindDevices`, `interceptConnectDevice`, `interceptDisconnectDevice`, `interceptPingDevice`
- [ ] **Export all option interfaces**: Re-export all `Intercept*Options` interfaces
- [ ] **Add JSDoc comment**: Document barrel export purpose

**Testing Subtask:**

- [ ] **Write Tests**: N/A for barrel export

**Key Implementation Notes:**

- Enables clean imports: `import { interceptFindDevices } from '../support/interceptors';`
- Export both functions and types for full type safety
- Follow existing barrel export pattern from Phase 2 fixtures

</details>

---

## 🗂️ Files Modified or Created

> All files are relative to workspace root: `apps/teensyrom-ui-e2e/`

**New Files:**

- `src/support/interceptors/README.md` - Interceptor documentation and usage guide
- `src/support/interceptors/device.interceptors.ts` - All device interceptor functions
- `src/support/interceptors/device.interceptors.spec.ts` - Interceptor validation tests
- `src/support/interceptors/index.ts` - Barrel exports

**Modified Files:**

- None (Phase 3 is purely additive)

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
>
> - **Behavioral testing**: Verify interceptors register correct routes and return correct data structures
> - **Test as you go**: Complete each task's testing subtask before moving to next task
> - **Test through Cypress API**: Use `cy.intercept`, `cy.wait`, assertions on response structure
> - **No mocking needed**: Interceptors ARE the mocks - tests verify they work correctly

> **Reference Documentation:**
>
> - **All tasks**: [Testing Standards](../../TESTING_STANDARDS.md) - Core behavioral testing approach
> - **Cypress Intercept**: [cy.intercept docs](https://docs.cypress.io/api/commands/intercept) - Intercept API reference

### Where Tests Are Written

**Tests are embedded in each task above** with:

- **Testing Subtask**: Checkbox in task's subtask list (e.g., "Write Tests: Test behaviors for this task")
- **Testing Focus**: "Behaviors to Test" section listing observable outcomes
- **Testing Reference**: Links to relevant testing documentation

**Complete each task's testing subtask before moving to the next task.**

### Test Execution Commands

**Running Tests:**

```bash
# Run interceptor unit tests
npx nx test teensyrom-ui-e2e --testFile=**/device.interceptors.spec.ts

# Run all E2E support tests
npx nx test teensyrom-ui-e2e
```

**Note**: These are NOT full E2E tests - they are unit tests validating that interceptor functions correctly register cy.intercept with proper parameters.

### Test File Structure

```typescript
// device.interceptors.spec.ts
describe('Device Interceptors', () => {
  describe('interceptFindDevices', () => {
    it('should register intercept with default fixture', () => {
      // Test default behavior
    });

    it('should use custom fixture when provided', () => {
      // Test fixture override
    });

    it('should return error response in error mode', () => {
      // Test error mode
    });
  });

  // Similar structure for other interceptors
});
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**

- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)
- [ ] README documentation completed and accurate
- [ ] All interceptor functions implemented:
  - [ ] `interceptFindDevices`
  - [ ] `interceptConnectDevice`
  - [ ] `interceptDisconnectDevice`
  - [ ] `interceptPingDevice`

**Testing Requirements:**

- [ ] All testing subtasks completed within each task
- [ ] All behavioral test checkboxes verified
- [ ] Tests written alongside implementation (not deferred)
- [ ] All tests passing with no failures
- [ ] Test coverage validates:
  - [ ] Default fixture behavior
  - [ ] Custom fixture behavior
  - [ ] Error mode behavior
  - [ ] Alias registration
  - [ ] Response structure correctness

**Quality Checks:**

- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors (`pnpm nx lint teensyrom-ui-e2e`)
- [ ] Code formatting is consistent
- [ ] Barrel exports working correctly

**Documentation:**

- [ ] README includes all required sections
- [ ] Code examples in README are accurate
- [ ] Inline comments explain non-obvious logic
- [ ] Option interfaces fully documented with JSDoc

**Ready for Next Phase:**

- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Interceptors ready for use in Phase 4 tests
- [ ] Ready to proceed to Phase 4 (Device Discovery Test)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Function-based interceptors**: Chose functions over classes for simplicity and Cypress idiom alignment
- **Options pattern**: Consistent `{ fixture?, errorMode? }` pattern across all interceptors for predictability
- **Default fixtures**: Use `singleDevice` as default to support simple test cases without configuration
- **Alias naming**: Use `@interceptName` pattern matching function names for easy reference

### Implementation Constraints

- **Cypress cy.intercept API**: Interceptors must work within Cypress intercept limitations (route matching, response structure)
- **Generated API client types**: Must align with OpenAPI-generated response types exactly
- **Phase 2 dependency**: Requires completed Phase 2 fixtures - cannot proceed without them

### API Endpoint Coverage

**Phase 3 Scope (Implemented)**:

- ✅ `GET /api/devices` - Find devices
- ✅ `POST /api/devices/{id}/connect` - Connect device
- ✅ `DELETE /api/devices/{id}` - Disconnect device
- ✅ `POST /api/devices/{id}/ping` - Ping device

**Future Phases (Not Implemented)**:

- ⏳ `POST /api/devices/{id}/reset` - Reset device (if needed)
- ⏳ Storage/directory endpoints (Phase 6+)
- ⏳ Player launch endpoints (Phase 7+)

### Future Enhancements

- **Request validation**: Add interceptors that validate request body structure
- **Delay simulation**: Add configurable response delays to test loading states
- **Network failure modes**: Add partial failure scenarios (timeout, connection reset)
- **Chained interceptors**: Support interceptor composition for complex scenarios

### External References

- [Cypress Intercept API](https://docs.cypress.io/api/commands/intercept) - Official documentation
- [Phase 2 Plan](./PHASE_2_PLAN.md) - Fixture system implementation
- [Phase 4 Plan](./PHASE_4_PLAN.md) - Will consume these interceptors

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery 1**: [Add findings here]
- **Discovery 2**: [Add findings here]

</details>

---

## 💡 Agent Implementation Guide

> **Instructions for AI agents implementing this phase**

### Before Starting Implementation

**Verify Phase 2 Completion:**

1. Confirm these files exist and are complete:

   - `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts`
   - `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/fixture.types.ts`
   - `apps/teensyrom-ui-e2e/src/support/test-data/generators/device.generators.ts`

2. Verify fixtures are tested and working:
   - Run `npx nx test teensyrom-ui-e2e --testFile=**/devices.fixture.spec.ts`
   - All tests should pass

**Review API Client Structure:**

1. Examine `DevicesApiService.ts` to understand endpoint signatures
2. Review response types: `FindDevicesResponse`, `ConnectDeviceResponse`, etc.
3. Understand route patterns used by generated client

### During Implementation

**Task Execution Order:**

1. ✅ Complete Task 1 (Directory + README) first - provides context for remaining work
2. ✅ Implement Task 2 (Find Devices) - most commonly used interceptor
3. ✅ Implement Task 3 (Connect Device) - critical for device workflows
4. ✅ Implement Task 4 (Disconnect Device) - complementary to Task 3
5. ✅ Implement Task 5 (Ping Device) - health check support
6. ✅ Complete Task 6 (Barrel Export) last - requires all functions complete

**Testing Integration:**

1. Write interceptor unit tests that verify `cy.intercept` is called correctly
2. Tests should NOT run actual HTTP requests - verify registration only
3. Use Cypress test utilities to validate route matching and response structure
4. Test error modes to ensure failures return correct status codes

**Code Quality:**

1. Follow existing patterns from Phase 2 (barrel exports, JSDoc, etc.)
2. Use TypeScript strict mode - all parameters and returns fully typed
3. Keep functions pure and predictable - no side effects beyond cy.intercept
4. Add JSDoc comments for all public functions and interfaces

### After Completing Each Task

1. ✅ Mark all subtask checkboxes
2. ✅ Run tests to verify functionality
3. ✅ Run linter: `pnpm nx lint teensyrom-ui-e2e`
4. ✅ Update "Discoveries During Implementation" section with any learnings

### Common Pitfalls to Avoid

1. **Route matching issues**: Use wildcards correctly (`*` for path segments, `**` for query params)
2. **Response structure mismatch**: Ensure response body matches OpenAPI-generated types exactly
3. **Missing aliases**: Always register aliases for test assertions
4. **Forgetting error modes**: All interceptors should support `errorMode: true`
5. **Testing real requests**: Tests should verify interceptor registration, not make actual HTTP calls

### Ready to Start?

1. Read all required documentation (check boxes in Required Reading section)
2. Review Phase 2 fixture files to understand available data
3. Start with Task 1 and work sequentially through Task 6
4. Test continuously - don't defer testing to the end
5. Update this document with discoveries as you learn

---

## 🎓 Examples of Interceptor Patterns

### ✅ Good Interceptor Function

```typescript
/**
 * Intercepts find devices API calls with mock fixture data
 */
export function interceptFindDevices(options: InterceptFindDevicesOptions = {}) {
  const fixture = options.fixture ?? singleDevice;

  cy.intercept('GET', '/api/devices*', (req) => {
    if (options.errorMode) {
      req.reply(500, { message: 'Device discovery failed' });
    } else {
      req.reply(200, { devices: fixture.devices });
    }
  }).as('findDevices');
}
```

**Why Good:**

- Clear function name matching endpoint purpose
- Options parameter with sensible defaults
- Error mode support
- Alias registration
- Proper route matching with wildcard

### ❌ Bad Interceptor Function

```typescript
// No types, no options, hardcoded data
export function intercept() {
  cy.intercept('GET', '/api/devices', {
    body: [{ name: 'Device 1' }],
  });
}
```

**Why Bad:**

- Vague function name
- No TypeScript types
- No options for customization
- No error mode support
- No alias registration
- Response structure doesn't match API contract
