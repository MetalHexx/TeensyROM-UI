# E2E Interceptor Format Documentation

## Overview

This document defines the standard structure and patterns for per-endpoint interceptor files in the TeensyROM UI E2E test suite. Each endpoint consolidates all related testing functionality into a single, self-contained file.

## File Naming Convention

**Pattern**: `{endpointName}.interceptors.ts`

**Examples**:
- `findDevices.interceptors.ts`
- `connectDevice.interceptors.ts`
- `saveFavorite.interceptors.ts`

**Naming Rules**:
- Use camelCase for endpoint names
- End with `.interceptors.ts` suffix
- Name should clearly indicate the API endpoint being intercepted

## File Structure

Each interceptor file follows this **6-section structure**:

### 1. Endpoint Definition

**Purpose**: Define the endpoint configuration constant

```typescript
/**
 * Sample endpoint configuration
 */
export const SAMPLE_ENDPOINT = {
  method: 'GET',
  path: '/sample',
  full: 'http://localhost:5168/sample',
  pattern: 'http://localhost:5168/sample*',
  alias: 'sampleEndpoint'
} as const;
```

**Requirements**:
- Use descriptive endpoint name in uppercase
- Include HTTP method, API path, full URL, and pattern for cy.intercept()
- Define alias for cy.wait() calls
- Mark as `const` for type safety

### 2. Interface Definitions

**Purpose**: Define TypeScript interfaces for interceptor options and response types

```typescript
/**
 * Options for interceptSampleEndpoint interceptor
 */
export interface InterceptSampleEndpointOptions {
  /** Override default response with custom fixture */
  fixture?: MockResponseData | MockResponseData[];
  /** When true, return HTTP error to simulate API failure */
  errorMode?: boolean;
  /** Simulate network delay in milliseconds */
  responseDelayMs?: number;
  /** Custom HTTP status code for error responses */
  statusCode?: number;
}
```

**Requirements**:
- Interface name pattern: `Intercept{EndpointName}Options`
- Include JSDoc comments for all properties
- Use union types for optional fixture arrays
- Provide sensible defaults in documentation

### 3. Interceptor Function

**Purpose**: Main cy.intercept() implementation with fixture and error support

```typescript
/**
 * Intercepts sample endpoint calls with configurable responses
 */
export function interceptSampleEndpoint(options?: InterceptSampleEndpointOptions): void {
  cy.intercept(SAMPLE_ENDPOINT.method, SAMPLE_ENDPOINT.pattern, (req) => {
    if (options?.errorMode) {
      const statusCode = options.statusCode || 500;
      req.reply({
        statusCode,
        headers: { 'content-type': 'application/problem+json' },
        body: {
          type: 'https://tools.ietf.org/html/rfc9110#section-15.6.1',
          title: 'Internal Server Error',
          status: statusCode,
        },
      });
      return;
    }

    if (options?.responseDelayMs) {
      // Simulate network delay
      cy.wait(options.responseDelayMs).then(() => {
        sendResponse();
      });
    } else {
      sendResponse();
    }

    function sendResponse() {
      if (options?.fixture) {
        req.reply({
          statusCode: 200,
          body: options.fixture,
        });
      } else {
        // Default response
        req.reply({
          statusCode: 200,
          body: { message: 'Sample response' },
        });
      }
    }
  }).as(SAMPLE_ENDPOINT.alias);
}
```

**Requirements**:
- Function name pattern: `intercept{EndpointName}`
- Accept options parameter matching the interface
- Handle both success and error modes
- Support response delays
- Register Cypress alias using endpoint alias
- Use `req.reply()` for responses

### 4. Wait Function

**Purpose**: cy.wait() wrapper using the endpoint alias

```typescript
/**
 * Waits for sample endpoint call to complete
 */
export function waitForSampleEndpoint(): void {
  cy.wait(`@${SAMPLE_ENDPOINT.alias}`);
}
```

**Requirements**:
- Function name pattern: `waitFor{EndpointName}`
- Use template literal with endpoint alias
- No parameters required
- Simple cy.wait() call only

### 5. Helper Functions

**Purpose**: Endpoint-specific helper functions for common testing scenarios

```typescript
/**
 * Verifies sample endpoint completed successfully
 */
export function verifySampleEndpointCompleted(): void {
  cy.get('@sampleEndpoint').should('exist');
}

/**
 * Sets up sample endpoint with default response
 */
export function setupSampleEndpoint(): void {
  interceptSampleEndpoint();
}
```

**Requirements**:
- Focus on endpoint-specific functionality
- Use descriptive function names
- Keep functions simple and focused
- Follow naming pattern: `verify{EndpointName}{Action}` or `setup{EndpointName}`

### 6. Export Constants

**Purpose**: Backward compatibility exports during migration

```typescript
// Backward compatibility exports
export const SAMPLE_ENDPOINT_ALIAS = SAMPLE_ENDPOINT.alias;
export const INTERCEPT_SAMPLE_ENDPOINT = 'sampleEndpoint';
```

**Requirements**:
- Provide alias exports for existing import patterns
- Maintain compatibility during transition
- Export both endpoint object and string alias

## Import Patterns

### Explicit Imports (Recommended)

```typescript
import {
  interceptSampleEndpoint,
  waitForSampleEndpoint,
  verifySampleEndpointCompleted,
  SAMPLE_ENDPOINT
} from './sampleEndpoint.interceptors';
```

### Individual Function Imports

```typescript
import { interceptSampleEndpoint } from './sampleEndpoint.interceptors';
import { waitForSampleEndpoint } from './sampleEndpoint.interceptors';
```

## Error Handling Patterns

### Standard Error Response

```typescript
if (options?.errorMode) {
  const statusCode = options.statusCode || 500;
  req.reply({
    statusCode,
    headers: { 'content-type': 'application/problem+json' },
    body: {
      type: 'https://tools.ietf.org/html/rfc9110#section-15.6.1',
      title: getErrorTitle(statusCode),
      status: statusCode,
    },
  });
}
```

### Custom Error Messages

```typescript
function getErrorTitle(statusCode: number): string {
  switch (statusCode) {
    case 400: return 'Bad Request';
    case 404: return 'Not Found';
    case 500: return 'Internal Server Error';
    default: return 'Error';
  }
}
```

## Type Safety Guidelines

### Use TypeScript Interfaces

```typescript
interface MockResponseData {
  id: string;
  name: string;
  status: 'active' | 'inactive';
}
```

### Union Types for Fixtures

```typescript
interface InterceptSampleEndpointOptions {
  fixture?: MockResponseData | MockResponseData[];
}
```

### Const Assertions

```typescript
export const SAMPLE_ENDPOINT = {
  // ... properties
} as const;
```

## Migration Guidelines

### From Current Pattern

**Before** (scattered across files):
```typescript
// api.constants.ts
export const INTERCEPT_ALIASES = {
  SAMPLE_ENDPOINT: 'sampleEndpoint'
} as const;

// device.interceptors.ts
export function interceptSampleEndpoint(options?: any): void {
  // implementation
}

// test-helpers.ts
export function waitForSampleEndpoint(): void {
  cy.wait('@sampleEndpoint');
}
```

**After** (consolidated):
```typescript
// sampleEndpoint.interceptors.ts
export const SAMPLE_ENDPOINT = { /* ... */ } as const;
export interface InterceptSampleEndpointOptions { /* ... */ }
export function interceptSampleEndpoint(options?: InterceptSampleEndpointOptions): void { /* ... */ }
export function waitForSampleEndpoint(): void { /* ... */ }
export function verifySampleEndpointCompleted(): void { /* ... */ }
```

### Migration Steps

1. **Create new interceptor file** following the 6-section structure
2. **Move endpoint definition** from api.constants.ts
3. **Move interceptor function** from existing interceptor files
4. **Move wait function** from test-helpers.ts
5. **Update imports** in test files to use new explicit imports
6. **Remove old scattered code** after validation
7. **Update documentation** with new patterns

## Testing Patterns

### Success Scenario

```typescript
beforeEach(() => {
  interceptSampleEndpoint({
    fixture: { id: '1', name: 'Test Item', status: 'active' }
  });
});

it('should handle sample endpoint successfully', () => {
  // trigger endpoint call
  waitForSampleEndpoint();
  // assertions
});
```

### Error Scenario

```typescript
it('should handle sample endpoint error', () => {
  interceptSampleEndpoint({
    errorMode: true,
    statusCode: 404
  });

  // trigger endpoint call
  // verify error handling
});
```

### Delayed Response

```typescript
it('should handle delayed response', () => {
  interceptSampleEndpoint({
    responseDelayMs: 2000
  });

  // trigger endpoint call
  waitForSampleEndpoint();
  // verify loading states
});
```

## Best Practices

### File Organization

- Keep files focused on single endpoint
- Use consistent section ordering
- Include comprehensive JSDoc comments
- Maintain type safety throughout

### Performance Considerations

- Avoid heavy computations in interceptor functions
- Use response delays judiciously
- Keep fixture data reasonable in size
- Clean up interceptors between tests if needed

### Maintainability

- Follow naming conventions consistently
- Use descriptive function and variable names
- Document complex logic with comments
- Keep interfaces focused and minimal

This format provides a solid foundation for creating consistent, maintainable, and type-safe interceptor files that consolidate all endpoint-related testing functionality into single, self-contained units.