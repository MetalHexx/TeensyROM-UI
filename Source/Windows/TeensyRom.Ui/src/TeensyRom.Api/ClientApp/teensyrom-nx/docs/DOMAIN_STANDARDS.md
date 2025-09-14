# Domain Standards

## Overview

This document establishes standards for domain library organization and documentation in the TeensyROM Angular application.

---

## Domain Overview Documentation

### Requirement

**Standard**: Create a `[DOMAIN-NAME]_DOMAIN.md` file for each domain library to provide lightweight context

**Purpose**: Enable efficient understanding without reading implementation files

**Location**: `libs/domain/[domain]/[DOMAIN-NAME]_DOMAIN.md`

**Naming**: Use UPPER_CASE with domain name (e.g., `STORAGE_DOMAIN.md`, `DEVICE_DOMAIN.md`)

### Format

```markdown
# Domain Name Overview

## Purpose

Brief description of domain responsibility

## Architecture

- Services: What they do (not how)
- State: What it manages (not implementation)

## Key Classes/Methods

- Class/Method names with purpose (no implementation details)

## Key Concepts

- Important patterns or design decisions
```

### Requirements

- Focus on **what** not **how**
- Include class/method names and purposes
- Mention key architectural patterns (Signal Store, flat state, etc.)
- Keep concise for AI context efficiency
- Update when domain structure changes

### Benefits

- **Faster context building** - Quick understanding without file exploration
- **Better human onboarding** - Clear entry point for new developers
- **Reduced token usage** - Efficient AI context loading
- **Documentation consistency** - Standard format across domains

### Example

See [`libs/domain/storage/STORAGE_DOMAIN.md`](../libs/domain/storage/STORAGE_DOMAIN.md) for a complete implementation following this standard.

---

## Domain Library Structure

### Standard Organization

```
libs/domain/[domain]/
├── [DOMAIN-NAME]_DOMAIN.md     # Required overview documentation (e.g., STORAGE_DOMAIN.md)
├── services/                   # Domain services and models
│   ├── src/lib/
│   │   ├── *.service.ts       # API communication
│   │   ├── *.mapper.ts        # API to domain mapping
│   │   └── *.models.ts        # Domain types and enums
│   └── src/index.ts           # Public API exports
└── state/                     # State management (if needed)
    ├── src/lib/
    │   ├── *-store.ts         # NgRx Signal Store
    │   ├── *.util.ts          # Store utilities
    │   └── methods/           # Store methods (one per file)
    └── src/index.ts           # Public API exports
```

### Naming Conventions

- Domain folders use singular names (`storage`, `device`, `user`)
- Service files end with `.service.ts`
- Mapper files end with `.mapper.ts`
- Model files end with `.models.ts`
- Store files end with `-store.ts`

---

## Domain Services Configuration

### Goal

Provide a clean, testable boundary for domain services by exposing an interface and an InjectionToken, while wiring the concrete class at the application shell. This enables strong typing, easy mocking in unit tests, and swappable implementations if needed.

### Pattern

1. Define an interface and token in the service file (or a colocated `*.tokens.ts`).

```ts
// domain services (example)
export interface IExampleService {
  getData(id: string): Observable<ExampleModel>;
}

export const EXAMPLE_SERVICE = new InjectionToken<IExampleService>('EXAMPLE_SERVICE');

@Injectable({ providedIn: 'root' })
export class ExampleService implements IExampleService {
  constructor(private readonly api: ApiClient) {}
  getData(id: string) {
    /* ... */
  }
}

export const EXAMPLE_SERVICE_PROVIDER = {
  provide: EXAMPLE_SERVICE,
  useExisting: ExampleService,
};
```

2. Barrel export the interface, token, and provider.

```ts
// libs/domain/example/services/src/index.ts
export * from './lib/example.service';
export { IExampleService, EXAMPLE_SERVICE, EXAMPLE_SERVICE_PROVIDER } from './lib/example.service';
```

3. Wire the provider in the application shell.

```ts
// apps/app/src/app/app.config.ts
providers: [, /* other providers */ EXAMPLE_SERVICE_PROVIDER];
```

4. Inject the token at usage sites (e.g., stores) to depend on the interface rather than the concrete class.

```ts
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, service: IExampleService = inject(EXAMPLE_SERVICE)) => ({
    // ... methods using service
  }))
);
```

### Testing

- In unit tests, provide the interface token with a small, strongly typed mock:

```ts
type GetDataFn = (id: string) => Observable<ExampleModel>;
let getDataMock: MockedFunction<GetDataFn>;

TestBed.configureTestingModule({
  providers: [
    { provide: EXAMPLE_SERVICE, useValue: { getData: (getDataMock = vi.fn<GetDataFn>()) } },
  ],
});
```

### Example in Repository

- Storage domain service follows this pattern:
  - Interface + token + provider: `libs/domain/storage/services/src/lib/storage.service.ts`
  - Barrel export: `libs/domain/storage/services/src/index.ts`
  - App wiring: `apps/teensyrom-ui/src/app/app.config.ts` (adds `STORAGE_SERVICE_PROVIDER`)
  - Store injection of token: `libs/domain/storage/state/src/lib/storage-store.ts`

---

## Related Documentation

- **State Standards**: [`STATE_STANDARDS.md`](./STATE_STANDARDS.md) - NgRx Signal Store patterns
- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md) - General coding conventions
- **Nx Library Standards**: [`NX_LIBRARY_STANDARDS.md`](./NX_LIBRARY_STANDARDS.md) - Library creation patterns
