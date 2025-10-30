# Interceptor Primitives Usage Guide

This guide demonstrates how to use the three core primitive functions for E2E testing.

## Quick Start

```typescript
import {
  interceptSuccess,
  interceptError,
  interceptSequence
} from './interceptor-primitives';
import { FIND_DEVICES_ENDPOINT } from '../constants/api.constants';
```

## Success Responses

### Basic Success Response
```typescript
interceptSuccess(FIND_DEVICES_ENDPOINT);
```

### Success with Custom Data
```typescript
const mockDevices = [
  { id: 'COM3', name: 'TeensyROM Device', connected: true }
];

interceptSuccess(FIND_DEVICES_ENDPOINT, mockDevices);
```

### Success with Delay
```typescript
// Simulate network delay
interceptSuccess(FIND_DEVICES_ENDPOINT, mockDevices, 1500);
```

## Error Responses

### Generic Error (defaults to 500)
```typescript
interceptError(CONNECT_DEVICE_ENDPOINT);
```

### Custom Status Code
```typescript
interceptError(FIND_DEVICES_ENDPOINT, 404);
```

### Custom Status and Message
```typescript
interceptError(SAVE_FAVORITE_ENDPOINT, 400, 'Invalid file path');
```

### Error with Delay
```typescript
interceptError(GET_DIRECTORY_ENDPOINT, 503, 'Service unavailable', 2000);
```

## Sequence Responses

### Progressive Operation (Indexing)
```typescript
interceptSequence(INDEX_FILES_ENDPOINT, [
  { status: 202, message: 'Indexing started' },
  { status: 202, message: 'Indexing in progress', progress: 50 },
  { status: 200, message: 'Indexing complete', totalFiles: 1250 }
]);
```

### Device State Changes
```typescript
interceptSequence(CONNECT_DEVICE_ENDPOINT, [
  { status: 'connecting' },
  { status: 'connected' },
  { status: 'ready' }
]);
```

### Error Recovery Scenario
```typescript
interceptSequence(REFRESH_DEVICES_ENDPOINT, [
  { statusCode: 500, message: 'Service temporarily unavailable' },
  { devices: mockDevices, status: 'healthy' }
], 500);
```

## Integration with Tests

```typescript
describe('Device Management', () => {
  beforeEach(() => {
    // Set up successful device discovery
    interceptSuccess(FIND_DEVICES_ENDPOINT, mockDevices);
  });

  it('should display available devices', () => {
    cy.visit('/');
    cy.wait('@findDevices');
    cy.get('[data-testid=device-list]').should('be.visible');
  });

  it('should handle connection errors', () => {
    // Override with error for this test
    interceptError(CONNECT_DEVICE_ENDPOINT, 503, 'Device busy');

    cy.get('[data-testid=connect-device]').click();
    cy.wait('@connectDevice');
    cy.get('[data-testid=error-message]').should('contain', 'Device busy');
  });
});
```

## Key Benefits

- **Simple API**: Only 3 functions to learn
- **Type Safe**: Full TypeScript support
- **Consistent**: Same patterns across all endpoints
- **Flexible**: Handles 85% of common scenarios
- **RFC Compliant**: Proper ProblemDetails error format