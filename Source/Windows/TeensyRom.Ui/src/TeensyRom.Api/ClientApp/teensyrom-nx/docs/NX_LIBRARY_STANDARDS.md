# Nx Library Standards

This document defines standards for creating and managing libraries within the teensyrom-nx Nx workspace.

## Library Types and Organization

### Domain Libraries

**Purpose**: Business logic, data models, and services for specific domains

**Structure**: `libs/domain/[domain]/[type]/`

**Types**:

- `services` - API clients, HTTP services, business logic
- `state` - NgRx Signal stores and state management
- `models` - Shared domain models and types (when needed across multiple libraries)

**Examples**:

- `libs/domain/user/services/` → `@teensyrom-nx/domain/user/services`
- `libs/domain/product/state/` → `@teensyrom-nx/domain/product/state`

### Feature Libraries

**Purpose**: UI components, pages, and feature-specific logic

**Structure**: `libs/features/[feature]/`

**Examples**:

- `libs/features/dashboard/` → `@teensyrom-nx/features/dashboard`
- `libs/features/settings/` → `@teensyrom-nx/features/settings`

### UI Libraries

**Purpose**: Reusable presentational components and styling

**Structure**: `libs/ui/[category]/`

**Examples**:

- `libs/ui/components/` → `@teensyrom-nx/ui/components`
- `libs/ui/styles/` → `@teensyrom-nx/ui/styles`

### Data Access Libraries

**Purpose**: External API clients and data fetching

**Structure**: `libs/data-access/[source]/`

**Examples**:

- `libs/data-access/api-client/` → `@teensyrom-nx/data-access/api-client`

## Library Creation Commands

### Domain Services Library

```bash
npx nx generate @nrwl/angular:library \
  --name=services \
  --directory=libs/domain/[domain] \
  --buildable=false \
  --publishable=false \
  --importPath=@teensyrom-nx/domain/[domain]/services
```

### Domain State Library

```bash
npx nx generate @nrwl/angular:library \
  --name=state \
  --directory=libs/domain/[domain] \
  --buildable=false \
  --publishable=false \
  --importPath=@teensyrom-nx/domain/[domain]/state
```

### Feature Library

```bash
npx nx generate @nrwl/angular:library \
  --name=[feature] \
  --directory=libs/features \
  --buildable=false \
  --publishable=false \
  --importPath=@teensyrom-nx/features/[feature]
```

### UI Component Library

```bash
npx nx generate @nrwl/angular:library \
  --name=[category] \
  --directory=libs/ui \
  --buildable=false \
  --publishable=false \
  --importPath=@teensyrom-nx/ui/[category]
```

## Configuration Standards

### Library Configuration

- **Buildable**: Set to `false` for most libraries to optimize build performance
- **Publishable**: Set to `false` unless library needs to be published to npm
- **Import Path**: Always use `@teensyrom-nx/` prefix with descriptive path structure

### TSConfig Path Mapping

Nx automatically updates `tsconfig.base.json` with path mappings:

```json
{
  "compilerOptions": {
    "paths": {
      "@teensyrom-nx/domain/storage/services": ["libs/domain/storage/services/src/index.ts"],
      "@teensyrom-nx/domain/storage/state": ["libs/domain/storage/state/src/index.ts"]
    }
  }
}
```

## File Structure Standards

### Domain Services Library Structure

```
libs/domain/[domain]/services/src/
├── lib/
│   ├── [domain].models.ts          # Domain interfaces and types
│   ├── [domain].mapper.ts          # DTO → domain transformation
│   ├── [domain].service.ts         # HTTP client wrapper
│   ├── [domain].service.spec.ts    # Unit tests
│   ├── [domain].mapper.spec.ts     # Mapper unit tests
│   └── [domain].service.integration.spec.ts  # Integration tests
└── index.ts                        # Barrel exports
```

### Domain State Library Structure

```
libs/domain/[domain]/state/src/
├── lib/
│   ├── [domain].store.ts           # NgRx Signal Store
│   ├── [domain].store.spec.ts      # Store unit tests
│   └── [domain].store.integration.spec.ts  # Store integration tests
└── index.ts                        # Barrel exports
```

## Barrel Export Standards

### Index.ts Structure

**Domain Services**:

```typescript
// Public API exports
export * from './lib/[domain].models';
export * from './lib/[domain].service';

// Do not export mappers or internal utilities
// Do not export test files
```

**Domain State**:

```typescript
// Public API exports
export * from './lib/[domain].store';

// Export store instance if using singleton pattern
export { [domain]Store } from './lib/[domain].store';
```

## Dependency Management

### Library Dependencies

- **Internal Dependencies**: Use import paths, not relative imports

```typescript
// Good
import { UserService } from '@teensyrom-nx/domain/user/services';

// Bad
import { UserService } from '../../user/services/src';
```

- **External Dependencies**: Add to library's `project.json` if needed, but prefer workspace-level dependencies

### Circular Dependency Prevention

- **Rule**: Libraries in the same domain can depend on each other, but avoid circular references
- **Pattern**: Services → Models, State → Services + Models

## Integration Verification

### Required Verification Steps

1. **Library Build Test**:

```bash
npx nx build [library-name]
```

2. **Application Integration Test**:

```bash
npx nx build teensyrom-ui
```

3. **Import Path Test**:

```typescript
import { ExampleService } from '@teensyrom-nx/domain/example/services';
```

4. **Dependency Graph Check**:

```bash
npx nx graph
```

5. **Runtime Test**:

```bash
npx nx serve teensyrom-ui
```

### Integration Requirements Checklist

- [ ] Library appears in `tsconfig.base.json` path mappings
- [ ] Library builds without errors
- [ ] Application builds with library imported
- [ ] No circular dependencies in dependency graph
- [ ] Runtime imports work without errors
- [ ] Library follows established naming conventions

## Naming Conventions

### Library Names

- **Domain Services**: `domain-[domain]-services`
- **Domain State**: `domain-[domain]-state`
- **Features**: `features-[feature]`
- **UI Components**: `ui-[category]`

### Import Paths

- **Consistent Prefix**: Always use `@teensyrom-nx/`
- **Descriptive Hierarchy**: `domain/[domain]/[type]`, `features/[feature]`, `ui/[category]`
- **Kebab Case**: Use kebab-case for domains and features in paths

### File Names

- **Services**: `[domain].service.ts`
- **Models**: `[domain].models.ts`
- **Mappers**: `[domain].mapper.ts`
- **Stores**: `[domain].store.ts`
- **Tests**: Add `.spec.ts` or `.integration.spec.ts` suffix

## Common Patterns

### Service Injection

```typescript
@Injectable({ providedIn: 'root' })
export class StorageService {
  constructor(private readonly apiClient: FilesApiService) {}
}
```

### Store Creation

```typescript
export const storageStore = signalStore(
  { providedIn: 'root' }
  // Store configuration
);
```

### Error Handling

```typescript
getData(request: DataRequest): Observable<DomainModel> {
  return this.apiClient.getData(request).pipe(
    map(response => DomainMapper.toDomainModel(response)),
    catchError(error => {
      console.error('Data fetch failed:', error);
      return throwError(() => error);
    })
  );
}
```
