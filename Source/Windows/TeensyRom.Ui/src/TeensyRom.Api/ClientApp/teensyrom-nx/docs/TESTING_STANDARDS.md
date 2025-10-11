# Testing Standards

This document defines the testing philosophy and standards for the teensyrom-nx Angular application, aligned with our Clean Architecture implementation.

## Testing Philosophy

This application uses **Clean Architecture** with strict layer separation. Testing strategy varies by architectural layer, with **behavioral testing** for application and features layers (allowing real components to integrate), and **unit testing** for infrastructure and UI layers.

### Testing Strategy by Layer

| Layer | Location | Testing Approach | Mock Boundary |
|-------|----------|------------------|---------------|
| **Domain** | `libs/domain` | Don't test contracts/models.  Test domain logic. | N/A - Interfaces used as mocks |
| **Infrastructure** | `libs/infrastructure` | Unit test in isolation | Mock generated API clients |
| **Application** | `libs/application` | **Behavioral** - integrate stores/services | Mock infrastructure only |
| **Features** | `libs/features` | Unit test - Mock the application layer. |
| **UI Components** | `libs/ui` | Unit test |
| **Utilities** | `libs/utils` | Unit test |

**Key Principle**: Mock only at infrastructure boundaries. Application and features layers integrate real stores, services, and application logic together.

**What NOT to Test**:
- ❌ Domain models or interfaces (use as mocks)
- ❌ Contract definitions (they define test mocks)
- ❌ Backend APIs (E2E only)

## Testing Framework

- **Framework**: [Vitest](https://vitest.dev/) with Angular TestBed
- **Mocking**: Vitest built-in mocking capabilities
- **E2E**: Cypress (only layer that may use real backend APIs)

## Testing by Clean Architecture Layer

### Domain Layer (`libs/domain`)

**Test Domain Logic only** - Pure contracts and models are not tested directly. They are used as mocks in other layers.  Unit test domain logic if applicable.

### Infrastructure Layer (`libs/infrastructure`)

**Unit test with mocked API clients** - Test service implementations and data mapping in isolation.

```typescript
// libs/infrastructure/device/device.service.spec.ts
TestBed.configureTestingModule({
  providers: [
    DeviceService,                                        // Infrastructure implementation
    { provide: DevicesApiService, useValue: mockApiClient }, // Mock generated client
  ],
});
```

**Test Focus**:
- Service contract implementation correctness
- DTO → Domain model mapping (via mappers)
- Error handling and edge cases
- RxJS observable patterns

**File Organization**:
```
libs/infrastructure/device/
├── device.service.ts
├── device.service.spec.ts       # Unit test
├── device.mapper.ts
└── device.mapper.spec.ts        # Unit test
```

### Application Layer (`libs/application`)

**Behavioral testing** - Integrate real stores, context services, and application logic. Mock infrastructure only.

```typescript
// libs/application/player/player-context.service.spec.ts
TestBed.configureTestingModule({
  providers: [
    PlayerContextService,                                      // Real context service
    PlayerStore,                                               // Real store
    { provide: PLAYER_SERVICE, useValue: mockPlayerService },  // Mock infrastructure
  ],
});
```

**Test Focus**:
- Complete workflow behaviors and state coordination
- Store state updates and computed signals
- Multi-component integration (stores + services working together) 
- Error handling and recovery workflows
- Complex business logic paths
- Do not test individual store actions nor selectors.

**Example**: [`player-context.service.spec.ts`](../libs/application/src/lib/player/player-context.service.spec.ts)

**Detailed Methodology**: See [STORE_TESTING.md](./STORE_TESTING.md)

**File Organization**:
```
libs/application/player/
├── player-context.service.ts
├── player-context.service.spec.ts       # Behavioral test
└── player.store.ts
```

### Features Layer (`libs/features`)

**Behavioral testing** - Integrate with real application state. Mock infrastructure only.

```typescript
// libs/features/devices/device-view.component.spec.ts  
TestBed.configureTestingModule({
  providers: [
    DeviceStore,                                               // Real store
    StorageStore,                                              // Real store
    { provide: DEVICE_SERVICE, useValue: mockDeviceService },  // Mock infrastructure
  ],
});
```

**Test Focus**:
- Feature component workflows with realistic state
- User interactions trigger correct application behaviors
- Component rendering based on application state
- Input/output event handling
- Navigation and routing behaviors

**Detailed Methodology**: See [SMART_COMPONENT_TESTING.md](./SMART_COMPONENT_TESTING.md)

**File Organization**:
```
libs/features/devices/
├── device-view.component.ts
└── device-view.component.spec.ts        # Unit test
```

### UI Components Layer (`libs/ui`)

**Unit test presentational logic** - Test pure presentation with minimal mocking.

**Test Focus**:
- Input property handling
- Output event emissions
- Rendering based on inputs
- User interaction handlers (clicks, keyboard, etc.)
- Accessibility features

**File Organization**:
```
libs/ui/components/action-button/
├── action-button.component.ts
└── action-button.component.spec.ts      # Unit test
```

### Utilities Layer (`libs/utils`)

**Unit test ** - Test in complete isolation.

**Test Focus**:
- Pure function logic and transformations
- Edge cases and boundary conditions
- Error handling

**Examples**: 
- [`storage-key.util.spec.ts`](../libs/application/src/lib/storage/storage-key.util.spec.ts)
- [`log-helper.spec.ts`](../libs/utils/src/lib/log-helper.spec.ts)

**File Organization**:
```
libs/utils/
├── log-helper.ts
└── log-helper.spec.ts                   # Unit test
```

## Test Commands and Configuration

### Running Tests

```bash
npx nx test                    # Run all tests
npx nx test application        # Run application layer tests
npx nx test player             # Run player feature tests
npx nx test --watch            # Watch mode
npx nx test --coverage         # With coverage
npx nx lint application        # Lint application layer
npx nx e2e teensyrom-ui-e2e    # E2E tests
```

### Configuration

- **Vitest Config**: `vite.config.ts` or `vitest.config.ts`
- **Test Environment**: jsdom (DOM testing) or node (pure logic)
- **Coverage Target**: 80% line coverage for application/utility code

## Testing Best Practices

1. **Prefer Testing Behavior over Implementation** - Focus on observable outcomes
3. **Use Strongly Typed Mocks** - Leverage interfaces and injection tokens
4. **Test Edge Cases** - Null/undefined, empty data, errors
5. **Async Handling** - Use `async/await` and `nextTick()` helper for microtask queue
6. **Descriptive Names** - Test names explain expected behavior
7. **Independent Tests** - No execution order dependencies

## Related Documentation

- **Application Layer Testing**: [`STORE_TESTING.md`](./STORE_TESTING.md) - Behavioral testing for stores and context services
- **Features Layer Testing**: [`SMART_COMPONENT_TESTING.md`](./SMART_COMPONENT_TESTING.md) - Testing feature components with mocked dependencies
- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md) - Component and TypeScript standards
