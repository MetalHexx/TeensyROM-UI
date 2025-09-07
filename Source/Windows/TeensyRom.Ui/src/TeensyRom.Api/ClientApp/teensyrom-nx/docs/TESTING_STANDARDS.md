# Testing Standards

This document defines testing standards and patterns for the teensyrom-nx Angular application.

## Testing Framework Stack

### Unit Testing

- **Framework**: [Vitest](https://vitest.dev/) - Fast Vite-native test runner
- **Mocking**: Vitest built-in mocking capabilities
- **Assertions**: Expect API (Jest-compatible)
- **Coverage**: Built-in coverage reporting with c8

### Integration Testing

- **Framework**: Vitest for service integration tests
- **HTTP Mocking**: MSW (Mock Service Worker) for all API integration tests
- **Environment**: Isolated test environment with mocked backend services

### End-to-End Testing

- **Framework**: Cypress (configured in workspace)
- **Target**: Real browser automation for user journey testing
- **Backend**: Full API available and running for realistic end-to-end scenarios

## Test File Organization

### File Naming Conventions

- **Unit Tests**: `[filename].spec.ts`
- **Integration Tests**: `[filename].integration.spec.ts`
- **E2E Tests**: `[feature].e2e.spec.ts` (in cypress directory)

### Test File Location

**Co-located**: Test files placed in same directory as source files

```
libs/domain/storage/services/src/lib/
├── storage.service.ts
├── storage.service.spec.ts              # Unit tests
├── storage.service.integration.spec.ts  # Integration tests
├── storage.mapper.ts
└── storage.mapper.spec.ts               # Unit tests
```

## Unit Testing Standards

### Test Structure

- **Pattern**: Arrange-Act-Assert (AAA)
- **Grouping**: Use `describe` blocks for logical test organization
- **Naming**: Descriptive test names that explain behavior

```typescript
describe('ExampleService', () => {
  describe('getData', () => {
    it('should return transformed domain model when API call succeeds', () => {
      // Arrange
      const mockResponse = {
        /* API DTO */
      };
      const expectedDomain = {
        /* Domain model */
      };

      // Act
      const result = service.getData(request);

      // Assert
      expect(result).toEqual(expectedDomain);
    });
  });
});
```

### Mocking Patterns

- **Dependency Injection**: Mock services through TestBed configuration
- **HTTP Calls**: Use Vitest mocks for API service dependencies
- **External Dependencies**: Mock at service boundaries

```typescript
describe('ExampleService', () => {
  let service: ExampleService;
  let mockApiService: Mock<ExampleApiService>;

  beforeEach(() => {
    mockApiService = {
      getData: vi.fn(),
    } as any;

    TestBed.configureTestingModule({
      providers: [ExampleService, { provide: ExampleApiService, useValue: mockApiService }],
    });

    service = TestBed.inject(ExampleService);
  });
});
```

### Test Coverage Requirements

- **Minimum Coverage**: 80% line coverage for services and utilities
- **Critical Paths**: 100% coverage for error handling and business logic
- **Exclusions**: Barrel exports, simple interfaces, test utilities

### What to Test in Unit Tests

- **Services**:

  - Public method functionality
  - Error handling scenarios
  - Data transformation accuracy
  - Input validation
  - Observable streams behavior

- **Mappers**:

  - DTO to domain model transformation
  - Edge cases and null/undefined handling
  - Type coercion and validation
  - Error scenarios with malformed data

- **Stores** (NgRx Signal Stores):
  - State initialization
  - Method behavior and state updates
  - Computed signal derivations
  - Error state management

## Integration Testing Standards

### Purpose and Scope

- **Integration Tests**: Test interaction between multiple components/services using mocked APIs
- **API Integration**: Test HTTP communication patterns with mocked backend responses
- **Cross-Library Integration**: Test interaction between domain libraries in isolated environment

### Integration Test Structure

```typescript
describe('ExampleService Integration', () => {
  let service: ExampleService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ExampleService, ExampleApiService],
    });

    service = TestBed.inject(ExampleService);
  });

  describe('with mocked API', () => {
    // All integration tests use MSW for API mocking
    // Tests HTTP communication patterns and data flow
  });
});
```

### API Integration Testing

- **Mock Service Worker**: Use MSW for all HTTP API mocking in integration tests
- **Response Simulation**: Mock realistic API responses, including success and error scenarios
- **Network Patterns**: Test request/response cycles, headers, and data transformation

```typescript
// MSW setup for API mocking
import { setupServer } from 'msw/node';
import { rest } from 'msw';

const server = setupServer(
  rest.get('/api/example/data', (req, res, ctx) => {
    return res(
      ctx.json({
        /* mock response */
      })
    );
  })
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

### Cross-Store Integration Testing

- **Store Dependencies**: Test interaction between related domain stores
- **State Synchronization**: Verify state updates across dependent stores
- **Error Propagation**: Test error handling across store boundaries

## Testing Utilities and Helpers

### Test Data Factories

- **Purpose**: Consistent test data generation
- **Location**: `libs/testing/` or `src/test-utils/`
- **Pattern**: Factory functions for creating mock data

```typescript
// test-data.factories.ts
export const createMockDomainModel = (overrides?: Partial<DomainModel>): DomainModel => ({
  id: 'test-id',
  name: 'Test Name',
  isActive: true,
  ...overrides,
});

export const createMockApiResponse = (overrides?: Partial<ApiResponse>): ApiResponse => ({
  data: [],
  status: 'success',
  message: 'Operation completed',
  ...overrides,
});
```

### Custom Test Matchers

- **Purpose**: Domain-specific assertions
- **Pattern**: Extend expect with custom matchers
- **Examples**: `toBeValidModel()`, `toHaveProperty()`, `toMatchPattern()`

### Test Setup Utilities

- **Purpose**: Common test configuration and setup
- **TestBed Configuration**: Reusable TestBed setups for different scenarios
- **Mock Providers**: Standard mock configurations for common dependencies

## E2E Testing Standards

### Test Organization

- **Feature-Based**: Organize tests by user-facing features
- **User Journeys**: Test complete user workflows
- **Page Objects**: Use page object pattern for element interactions

### E2E Test Structure

```typescript
describe('Feature Navigation E2E', () => {
  before(() => {
    // Ensure backend API is running and accessible
    cy.request('GET', 'http://localhost:5168/api/health').should('have.property', 'status', 200);
  });

  beforeEach(() => {
    // Setup test environment with real backend
    cy.visit('/feature-page');
  });

  describe('User Workflow', () => {
    it('should complete user journey with real API', () => {
      // Given the application is loaded with real backend data
      cy.get('[data-cy=feature-list]').should('be.visible');

      // When user interacts with feature
      cy.get('[data-cy=feature-button]').click();

      // Then expected results should be visible (populated by real API)
      cy.get('[data-cy=results-container]').should('be.visible');
      cy.get('[data-cy=result-item]').should('have.length.greaterThan', 0);
    });
  });
});
```

### E2E Environment Requirements

- **Backend API**: Full application API must be running on expected port
- **Test Data**: Known test data setup for consistent E2E scenarios
- **External Dependencies**: Any required external services or databases

### Data Test Attributes

- **Pattern**: Use `data-cy` attributes for E2E test selectors
- **Naming**: Descriptive, kebab-case names
- **Stability**: Separate from CSS classes to avoid test breakage

## Performance Testing

### Unit Test Performance

- **Timeout Limits**: Set appropriate test timeouts (default 5s)
- **Mock Performance**: Ensure mocks don't introduce performance issues
- **Large Data Sets**: Test with realistic data volumes

### Integration Test Performance

- **API Response Times**: Monitor and assert on reasonable response times
- **Memory Usage**: Watch for memory leaks in long-running tests
- **Async Operations**: Proper handling of async operations and timing

## Error Testing Standards

### Error Scenarios to Test

- **Network Failures**: HTTP errors, timeouts, connection issues
- **Invalid Data**: Malformed API responses, missing fields
- **Business Logic Errors**: Domain validation failures
- **State Errors**: Invalid state transitions, concurrent modifications

### Error Testing Patterns

```typescript
describe('error handling', () => {
  it('should handle API errors gracefully', async () => {
    // Arrange
    mockApiService.getData.mockRejectedValue(new Error('Network error'));

    // Act & Assert
    await expect(service.getData(request)).rejects.toThrow('Network error');
  });

  it('should provide user-friendly error messages', () => {
    // Test error message transformation
  });
});
```

## Test Commands and Configuration

### Running Tests

```bash
# Run all unit tests
npx nx test

# Run tests for specific library
npx nx test domain-example-services

# Run tests in watch mode
npx nx test --watch

# Run integration tests (filtered by name pattern)
npx nx test --testNamePattern="integration"

# Run E2E tests
npx nx e2e teensyrom-ui-e2e

# Generate coverage report
npx nx test --coverage
```

### Test Configuration

- **Vitest Config**: Configured in `vite.config.ts` or `vitest.config.ts`
- **Test Environment**: jsdom for DOM testing, node for pure logic tests
- **Setup Files**: Global test setup and teardown utilities

### CI/CD Integration

- **Pre-commit**: Run affected tests before commits

## Testing Best Practices

### Development Approach

**Test-Driven Development (TDD)**: Write tests first, then implement code to make tests pass. This approach:

- **Ensures Testability**: Code is designed to be testable from the start
- **Provides Clear Requirements**: Tests serve as living documentation of expected behavior
- **Enables Confident Refactoring**: Existing tests catch regressions during code changes
- **Improves Code Quality**: Forces consideration of edge cases and error scenarios upfront

**TDD Cycle**:

1. **Red**: Write a failing test that describes desired behavior
2. **Green**: Write minimal code to make the test pass
3. **Refactor**: Improve code structure while keeping tests green

### General Principles

1. **Test Behavior, Not Implementation**: Focus on what the code does, not how
2. **Arrange-Act-Assert**: Clear test structure with distinct phases
3. **Single Responsibility**: Each test should verify one specific behavior
4. **Descriptive Names**: Test names should explain the expected behavior
5. **Independent Tests**: Tests should not depend on each other's execution order

### Mock Guidelines

1. **Mock at Boundaries**: Mock external dependencies and services
2. **Avoid Over-Mocking**: Don't mock everything; test real interactions when valuable
3. **Realistic Mocks**: Mocks should behave like real implementations
4. **Mock Consistency**: Use consistent mock patterns across the codebase

### Data Testing

1. **Edge Cases**: Test boundary conditions and edge cases
2. **Invalid Input**: Test with null, undefined, and invalid data
3. **Realistic Data**: Use representative data volumes and structures
4. **Data Factories**: Use factory patterns for consistent test data generation

### Async Testing

1. **Proper Async Handling**: Use async/await or proper promise handling
2. **Timeout Management**: Set appropriate timeouts for async operations
3. **Race Conditions**: Test for timing-related issues
4. **Error Propagation**: Ensure async errors are properly caught and tested

## Related Documentation

- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md) - Component and TypeScript standards.
