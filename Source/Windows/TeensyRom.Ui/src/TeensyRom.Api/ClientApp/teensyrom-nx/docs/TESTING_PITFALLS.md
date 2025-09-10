# Testing Pitfalls and Solutions

This document captures common testing issues encountered in the TeensyROM Angular application and their proven solutions.

---

## Angular Dependency Coupling in Pure Utilities

### Problem

Pure utility functions (like `StorageKeyUtil`) fail tests when they import enums or types from libraries that have Angular dependencies.

**Error Example**:

```
JIT compilation failed for injectable HttpClient
Need to call TestBed.initTestEnvironment() first
```

### Root Cause

The api-client library uses `typescript-angular` generator which creates Angular services with HttpClient dependencies. When pure utilities import from this library, tests try to initialize Angular's dependency injection system.

### Solution

**Create domain-level enums and use mocking**:

1. **Create domain enums** (in `storage.models.ts`):

   ```typescript
   export const StorageType = {
     Sd: 'SD',
     Usb: 'USB',
     Internal: 'Internal',
   } as const;
   ```

2. **Mock the import** in test files:

   ```typescript
   vi.mock('@teensyrom-nx/domain/storage/services', () => {
     return {
       StorageType: { Sd: 'SD', Usb: 'USB', Internal: 'Internal' } as const,
     };
   });
   ```

3. **Update imports** to use barrel exports:
   ```typescript
   import { StorageType } from '@teensyrom-nx/domain/storage/services';
   ```

### Files Affected

- `libs/domain/storage/services/src/lib/storage.models.ts` - Domain enums
- `libs/domain/storage/services/src/lib/storage.mapper.ts` - Type mapping
- `*.spec.ts` files - Mock configuration

---

## Vitest Project Name Conflicts

### Problem

Multiple vitest configurations with overlapping cache directory paths cause "Project name not unique" errors.

**Error Example**:

```
Error: Project name "services" from "libs/domain/storage/services/vite.config.ts"
is not unique. The project is already defined by "libs/domain/device/services/vite.config.mts"
```

### Root Cause

Vitest derives project names from the `cacheDir` path structure. When multiple configs use similar paths like `libs/services/storage` and `libs/services/device`, they both get the name "services".

### Solution

**Use domain-specific cache directory paths**:

```typescript
// ❌ WRONG - Causes conflicts
cacheDir: '../../../node_modules/.vite/libs/services/storage';
cacheDir: '../../../node_modules/.vite/libs/services/device';

// ✅ CORRECT - Unique paths
cacheDir: '../../../node_modules/.vite/libs/storage/services';
cacheDir: '../../../node_modules/.vite/libs/device/services';
```

**Also update coverage paths**:

```typescript
coverage: {
  reportsDirectory: '../../../coverage/libs/storage/services',
}
```

### Files Affected

- `libs/domain/storage/services/vite.config.ts`
- `libs/domain/device/services/vite.config.mts`

---

## Import Path Issues

### Problem

Tests fail with module resolution errors when using incorrect import paths.

**Error Example**:

```
Failed to resolve import "@teensyrom-nx/domain/storage/services/src/lib/storage.models"
```

### Root Cause

Direct imports to specific files bypass barrel exports and can break in test environments or when library structures change.

### Solution

**Always use barrel exports**:

```typescript
// ❌ WRONG - Direct file imports
import { StorageType } from '@teensyrom-nx/domain/storage/services/src/lib/storage.models';

// ✅ CORRECT - Barrel exports
import { StorageType } from '@teensyrom-nx/domain/storage/services';
```

**Ensure barrel exports are configured**:

```typescript
// libs/domain/storage/services/src/index.ts
export * from './lib/storage.models';
export * from './lib/storage.service';
export * from './lib/storage.mapper';
```

### Files to Check

- `libs/domain/*/services/src/index.ts` - Barrel export configuration
- Import statements in all `.ts` and `.spec.ts` files

---

## Test Environment Setup

### Problem

Tests fail due to missing test environment configuration or Angular TestBed issues.

### Solution

**Use consistent vitest configuration**:

```typescript
// vite.config.mts
test: {
  watch: false,
  globals: true,
  environment: 'jsdom',
  include: ['{src,tests}/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}'],
  setupFiles: ['src/test-setup.ts'],
  reporters: ['default'],
}
```

**Mock Angular dependencies** instead of configuring TestBed for pure utilities:

```typescript
// Prefer mocking over TestBed for pure functions
vi.mock('@angular/core', () => ({
  inject: vi.fn(),
}));
```

---

## Running Tests

### Problem

Confusion about which test commands to use for different scenarios.

### Solution

**Use these commands based on your needs**:

```bash
# Run tests for specific library
cd libs/domain/storage/state
npx vitest run

# Run specific test file
npx vitest run src/lib/storage-key.util.spec.ts

# Run all tests in workspace (may have conflicts)
npx vitest run

# Run tests from workspace root (may not work due to config conflicts)
cd ClientApp/teensyrom-nx
npx nx test storage-state  # Often fails due to missing config
```

**Best Practice**: Run tests from the specific library directory to avoid workspace-level configuration conflicts.

---

## Prevention Strategies

### 1. Architecture Guidelines

- Keep pure utilities separate from Angular services
- Use domain-level enums to avoid coupling
- Follow barrel export patterns consistently

### 2. Development Workflow

- Always create tests alongside utilities
- Mock external dependencies in pure utility tests
- Use proper NgRx Signal Store patterns from the start

### 3. Configuration Management

- Use unique cache directory paths in vitest configs
- Keep test environment configurations consistent
- Document any special test setup requirements

---

## Quick Reference

### Mock Template for Pure Utilities

```typescript
vi.mock('@teensyrom-nx/domain/storage/services', () => {
  return {
    StorageType: { Sd: 'SD', Usb: 'USB', Internal: 'Internal' } as const,
    // Add other mocked exports as needed
  };
});
```

### Test Command

```bash
# From library directory
cd libs/domain/[domain]/[library]
npx vitest run
```

---

## Related Documentation

- **State Standards**: [`STATE_STANDARDS.md`](./STATE_STANDARDS.md) - NgRx Signal Store patterns and pitfalls
- **Testing Standards**: [`TESTING_STANDARDS.md`](./TESTING_STANDARDS.md) - General testing guidelines
- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md) - Code quality standards
