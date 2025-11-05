# Device API Interceptors

## Purpose

Device API Interceptors provide a consistent, reusable pattern for mocking TeensyROM device endpoints in E2E tests using Cypress's `cy.intercept()` API. They enable realistic device workflow testing without requiring a live backend server.

**Key Benefits:**
- Mock realistic API responses from static test data (fixtures)
- Support error scenarios for testing failure handling
- Consistent naming and structure across all interceptors
- Simple, readable test code

---

## Philosophy

Interceptors follow these core principles:

1. **Consistency**: All interceptors use the same structure, naming conventions, and patterns
2. **Simplicity**: Configuration through optional parameters with sensible defaults
3. **Discoverability**: Named functions like `interceptFindDevices()` and `waitForFindDevices()` make intent clear
4. **Fixtures-First**: Leverage test fixtures for scenario variation rather than complex logic

---

## Interceptor Primitives

Interceptor Primitives are three low-level building blocks that form the foundation of all interceptors. They handle the core Cypress interception logic, allowing higher-level interceptors to focus on endpoint-specific behavior.  The help keep the interceptor patterns consistently in the test suite.

### The Three Primitives

```typescript
import { interceptSuccess, interceptError, interceptSequence } from './primitives/interceptor-primitives';
import type { EndpointDefinition } from './primitives/interceptor-primitives';
```

#### 1. `interceptSuccess()` - Successful Responses

Returns a 200 status with response data:

```typescript
const FIND_DEVICES_ENDPOINT: EndpointDefinition = {
  method: 'GET',
  pattern: 'http://localhost:5168/api/devices*',
  alias: 'findDevices',
};

interceptSuccess(FIND_DEVICES_ENDPOINT, mockDevices);
interceptSuccess(FIND_DEVICES_ENDPOINT, mockDevices, 1500); // with delay
```

#### 2. `interceptError()` - Error Responses

Returns an HTTP error with RFC 9110 ProblemDetails format:

```typescript
const CONNECT_DEVICE_ENDPOINT: EndpointDefinition = {
  method: 'POST',
  pattern: 'http://localhost:5168/api/devices/*/connect',
  alias: 'connectDevice',
};

interceptError(CONNECT_DEVICE_ENDPOINT);                           // 500 default
interceptError(CONNECT_DEVICE_ENDPOINT, 503, 'Device busy');       // custom status & message
interceptError(CONNECT_DEVICE_ENDPOINT, 500, 'Service error', 2000); // with delay
```

#### 3. `interceptSequence()` - Progressive Operations

Returns multiple responses in sequence for operations like indexing or state changes:

```typescript
const INDEX_FILES_ENDPOINT: EndpointDefinition = {
  method: 'POST',
  pattern: 'http://localhost:5168/api/storage/index*',
  alias: 'indexStorage',
};

// Indexing progress
interceptSequence(INDEX_FILES_ENDPOINT, [
  { status: 202, message: 'Indexing started' },
  { status: 202, message: 'Indexing in progress', progress: 50 },
  { status: 200, message: 'Indexing complete', totalFiles: 1250 },
]);

// Error recovery
interceptSequence(REFRESH_ENDPOINT, [
  { statusCode: 500, message: 'Service temporarily unavailable' },
  { devices: mockDevices, status: 'healthy' },
]);
```

### When to Use Primitives

Use primitives directly when:
- Building custom interceptors (like in `exampleEndpoint.interceptors.ts`)
- Testing scenarios not covered by pre-built interceptors
- Prototyping quickly before creating formal interceptor files
- Pretty much as often as you can.

---

## Core Concepts

### Options Pattern

All interceptors should accept an optional `options` parameter with sensible defaults:

```typescript
interceptFindDevices({
  fixture: multipleDevices,    // Override default response
  errorMode: true,              // Simulate API failure (500 status)
  responseDelayMs: 2000,         // Add network delay
});
```

### Error Mode

Simulate backend failures:

```typescript
interceptFindDevices({ errorMode: true });
```

Returns HTTP 500 with ProblemDetails structure:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An error occurred processing this request"
}
```

### Aliases

Each interceptor registers a named alias for Cypress assertions:

```typescript
interceptFindDevices();
cy.wait('@findDevices');
```

---

## Wait Functions

Each interceptor provides a standard `waitFor<EndpointName>()` function:

```typescript
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../support/interceptors/findDevices.interceptors';

interceptFindDevices();
waitForFindDevices(); // Waits for API call to complete
```

Wait functions are co-located with their interceptors for easy discovery. Import them directly from the interceptor files.

---

## Creating New Interceptors

Use `exampleEndpoint.interceptors.ts` as a template. The standard structure includes:

1. **Endpoint Definition** - `ENDPOINT` constant using `EndpointDefinition` type
2. **Interface Definitions** - Request/response types and options interface
3. **Interceptor Function** - Delegates to primitives (`interceptSuccess()`, `interceptError()`, or `interceptSequence()`)
4. **Wait Function** - `waitFor<Name>()` for test synchronization
5. **Helper Functions** - Optional verification and utility functions (keep minimal)

See `exampleEndpoint.interceptors.ts` in the `examples/` directory for a complete reference implementation.

---

## Best Practices

1. **Register Before Navigation**: Always set up interceptors before triggering API calls

2. **Use Specific Fixtures**: Avoid relying on defaults for non-trivial tests

3. **Chain Related Interceptors**: Group mocks for complete workflows together

4. **Always Wait for Aliases**: Ensure API calls complete before continuing test execution

5. **Test Both Success and Error**: Create test cases for both success and failure scenarios

---

## Further Reading

- [Cypress Intercept Documentation](https://docs.cypress.io/api/commands/intercept) - Official Cypress reference
- [Sample Interceptor](./examples/sampleEndpoint.interceptors.ts) - Template for creating new interceptors
- [Primitives Implementation](./primitives/interceptor-primitives.ts) - Core primitive functions
