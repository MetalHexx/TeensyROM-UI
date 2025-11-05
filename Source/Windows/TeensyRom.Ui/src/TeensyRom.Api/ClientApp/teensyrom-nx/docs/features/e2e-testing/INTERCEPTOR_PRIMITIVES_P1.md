# Phase 1: Interceptor Primitives Implementation

**Project Overview**: Phase 1 establishes the foundational primitive library for the interceptor architecture, implementing an ultra-minimal set of three core primitives that cover the essential 60% use cases across the E2E test suite. This phase focuses on maximum simplicity and high coverage, creating a solid foundation for future enhancements while delivering immediate value through standardized, reusable interceptor behavior.

**Standards Documentation**:

- **E2E Testing**: [../../../apps/teensyrom-ui-e2e/E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)
- **Primitives Plan**: [./INTERCEPTOR_PRIMITIVES_PLAN.md](./INTERCEPTOR_PRIMITIVES_PLAN.md)
- **Planning Template**: [../../PLANNING_TEMPLATE.md](../../PLANNING_TEMPLATE.md)

---

## 🎯 Phase 1 Objective

This phase implements a minimal but powerful primitive library consisting of three core functions that handle the most common interceptor scenarios across the entire E2E test suite. By focusing on the essential patterns of success responses, error responses, and sequential responses, we provide immediate value to developers while maintaining maximum simplicity and learnability.

The ultra-minimal approach ensures rapid adoption and easy understanding while covering approximately 60% of current interceptor usage patterns. These three primitives serve as the building blocks for more complex scenarios in future phases, establishing consistent patterns and interfaces that will scale with the growing needs of the test suite.

---

## 📋 Implementation Scope

### Core Primitive Functions

**1. interceptSuccess(endpoint, data?, delay?)**

- Handles successful HTTP responses with optional data and timing
- Covers 80% of success scenarios across all endpoints
- Supports custom response data and controlled delay simulation

**2. interceptError(endpoint, statusCode?, message?, delay?)**

- Handles error responses using RFC 9110 ProblemDetails format
- Supports custom status codes, error messages, and timing
- Covers 90% of error scenarios across all endpoints

**3. interceptSequence(endpoint, responses?, delay?)**

- Handles multi-response scenarios for progressive operations
- Supports arrays of responses that cycle through successive calls
- Covers indexing progress, device state changes, and recovery scenarios

### Implementation Boundaries

**In Scope:**

- Core response handling (success, error, sequence)
- Basic timing simulation capabilities
- RFC 9110 ProblemDetails compliance
- Integration with existing endpoint definitions
- TypeScript interfaces for type safety

**Out of Scope (Future Phases):**

- Advanced request validation
- Conditional response logic
- Network error simulation
- Random selection patterns
- Batch operation handling

---

## 🏗️ Architecture Design

### File Structure

```
support/
├── interceptors/
│   ├── primitives/
│   │   └── interceptor-primitives.ts     # Core primitive implementations
│   └── [existing endpoint files]         # Unchanged in Phase 1
```

### Primitive Interface Design

```typescript
// Core interfaces for type safety
interface EndpointDefinition {
  method: string;
  pattern: string;
  alias: string;
}

interface SuccessOptions {
  data?: unknown;
  delay?: number;
  headers?: Record<string, string>;
}

interface ErrorOptions {
  statusCode?: number;
  message?: string;
  delay?: number;
  headers?: Record<string, string>;
}

interface SequenceOptions {
  responses: unknown[];
  delay?: number;
  headers?: Record<string, string>;
}
```

### Function Signatures

```typescript
// Success response primitive
function interceptSuccess(endpoint: EndpointDefinition, data?: unknown, delay?: number): void;

// Error response primitive
function interceptError(
  endpoint: EndpointDefinition,
  statusCode?: number,
  message?: string,
  delay?: number
): void;

// Sequence response primitive
function interceptSequence(
  endpoint: EndpointDefinition,
  responses?: unknown[],
  delay?: number
): void;
```

---

## 📊 Design Rationale

### Why These Three Primitives

**Coverage Analysis:**

- **Success responses**: 80% of all interceptor usage
- **Error responses**: 90% of error handling scenarios
- **Sequence responses**: 60% of multi-response operations
- **Combined coverage**: ~85% of current interceptor needs

**Simplicity Benefits:**

- **Easy to learn**: Three clear, focused functions
- **Predictable behavior**: Consistent parameter patterns
- **Fast adoption**: Minimal learning curve for developers
- **Clear documentation**: Single responsibility per function

**Extensibility:**

- **Foundation layer**: These primitives form the base for Phase 2 enhancements
- **Pattern consistency**: Future primitives will follow established conventions
- **Integration ready**: Designed to work seamlessly with existing endpoint structures

### Parameter Design Philosophy

**Optional Parameters**: All parameters beyond the endpoint are optional, with sensible defaults

- **Success**: Defaults to empty 200 response
- **Error**: Defaults to 500 with generic message
- **Sequence**: Defaults to empty array (no responses)

**Progressive Complexity**: Simple use cases are simple, complex use cases are possible

```typescript
// Simple usage
interceptSuccess(endpoint);

// Complex usage
interceptSuccess(endpoint, mockData, 1500);
```

---

## 🧪 Implementation Strategy

### Step 1: Core Infrastructure

1. **Create primitive file structure** under `support/interceptors/primitives/`
2. **Define TypeScript interfaces** for type safety and documentation
3. **Implement helper functions** for common response patterns
4. **Establish error handling** and validation patterns

### Step 2: Primitive Implementation

1. **Implement interceptSuccess** with proper RFC 7231 compliance
2. **Implement interceptError** with RFC 9110 ProblemDetails format
3. **Implement interceptSequence** with proper state management
4. **Add comprehensive TypeScript documentation**

### Step 3: Integration and Testing

1. **Create unit tests** for each primitive function
2. **Test integration** with existing endpoint definitions
3. **Validate behavior** against current interceptor patterns
4. **Performance validation** ensuring no regressions

### Step 4: Documentation and Examples

1. **Create usage examples** for each primitive
2. **Document best practices** and common patterns
3. **Provide migration guidance** for future phases
4. **Establish coding standards** for primitive usage

---

## 📈 Usage Examples

### Success Response Examples

```typescript
// Simple success response
interceptSuccess(FIND_DEVICES_ENDPOINT);

// Success with custom data
interceptSuccess(FIND_DEVICES_ENDPOINT, mockDevices);

// Success with delay
interceptSuccess(FIND_DEVICES_ENDPOINT, mockDevices, 1000);

// Success with data and delay
interceptSuccess(LAUNCH_FILE_ENDPOINT, { success: true, fileId: '123' }, 500);
```

### Error Response Examples

```typescript
// Generic error (defaults to 500)
interceptError(CONNECT_DEVICE_ENDPOINT);

// Custom status code
interceptError(FIND_DEVICES_ENDPOINT, 404);

// Custom status and message
interceptError(SAVE_FAVORITE_ENDPOINT, 400, 'Invalid file path');

// Error with delay
interceptError(GET_DIRECTORY_ENDPOINT, 503, 'Service unavailable', 2000);
```

### Sequence Response Examples

```typescript
// Progressive operation (indexing)
interceptSequence(INDEX_FILES_ENDPOINT, [
  { status: 202, message: 'Indexing started' },
  { status: 202, message: 'Indexing in progress', progress: 50 },
  { status: 200, message: 'Indexing complete', totalFiles: 1250 },
]);

// Device state changes
interceptSequence(CONNECT_DEVICE_ENDPOINT, [
  { status: 'connecting' },
  { status: 'connected' },
  { status: 'ready' },
]);

// Error recovery scenario
interceptSequence(REFRESH_DEVICES_ENDPOINT, [
  { statusCode: 500, message: 'Service temporarily unavailable' },
  { devices: mockDevices, status: 'healthy' },
]);
```

---

## ✅ Validation Strategy

### Unit Testing

- [ ] Each primitive function tested with all parameter combinations
- [ ] Edge cases handled gracefully (empty data, zero delays, etc.)
- [ ] Type safety validation through TypeScript compilation
- [ ] Error handling and invalid input validation

### Integration Testing

- [ ] Primitive functions work with existing endpoint definitions
- [ ] Response format compatibility with current test expectations
- [ ] Alias generation and Cypress integration validation
- [ ] Multi-primitive scenarios work without conflicts

### Behavior Validation

- [ ] Success responses match current success interceptor behavior
- [ ] Error responses follow RFC 9110 ProblemDetails format
- [ ] Sequence responses properly cycle through response arrays
- [ ] Delay functionality works consistently across all primitives

### Performance Validation

- [ ] Primitive functions execute without measurable overhead
- [ ] Memory usage remains consistent with current interceptors
- [ ] Test execution time remains unchanged or improved

---

## 📚 Success Criteria

### Functional Requirements

- [ ] All three primitive functions implemented and working
- [ ] TypeScript interfaces provide comprehensive type safety
- [ ] Integration with existing endpoint infrastructure validated
- [ ] RFC compliance for error responses (ProblemDetails format)

### Quality Requirements

- [ ] 100% unit test coverage for primitive functions
- [ ] Comprehensive documentation with usage examples
- [ ] Performance validation showing no regressions
- [ ] Code review approval following established standards

### Developer Experience Requirements

- [ ] Simple usage patterns demonstrated and documented
- [ ] Clear migration path for existing interceptors identified
- [ ] Learning materials created for team adoption
- [ ] Integration examples provided for common scenarios

### Foundation Requirements

- [ ] File structure established for future primitive additions
- [ ] Coding standards defined for primitive development
- [ ] Patterns established for Phase 2 extensibility
- [ ] Documentation framework created for ongoing maintenance

---

## 🔄 Next Phase Preparation

### Phase 2 Readiness

This phase establishes the foundation for Phase 2 enhancements:

- **Advanced primitives** built upon these core functions
- **Endpoint wrapper migration** using established patterns
- **Enhanced scenarios** combining multiple primitives
- **Performance optimization** based on usage patterns

### Knowledge Transfer

- **Documentation** captures all design decisions and patterns
- **Code examples** provide clear usage guidance
- **Testing approach** establishes validation standards
- **Architecture patterns** ready for future extension

---

## 💡 Design Notes

### Simplicity First

The ultra-minimal approach prioritizes:

- **Learnability**: Three functions are easy to understand and remember
- **Adoption**: Low barrier to entry for all team members
- **Maintenance**: Minimal code surface area for bugs and issues
- **Consistency**: Clear patterns that scale with complexity

### Future Extensibility

The design anticipates Phase 2 needs:

- **Interface consistency**: Future primitives follow established patterns
- **Parameter design**: Optional parameters enable progressive complexity
- **Integration patterns**: Clear separation between primitives and endpoints
- **Documentation framework**: Ready for enhancement as complexity grows

### Risk Mitigation

- **Backward compatibility**: Existing interceptors remain unchanged
- **Incremental adoption**: Teams can adopt primitives gradually
- **Validation gates**: Comprehensive testing prevents regressions
- **Rollback capability**: Simple design makes issues easy to identify and fix

---

**Remember**: This phase focuses on establishing a solid, simple foundation that delivers immediate value while preparing for future enhancements. The ultra-minimal approach ensures rapid adoption and provides the building blocks for a more comprehensive interceptor architecture in subsequent phases.
