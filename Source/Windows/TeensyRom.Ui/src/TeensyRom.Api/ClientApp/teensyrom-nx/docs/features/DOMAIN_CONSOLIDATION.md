# Domain Consolidation Plan: Create Single Domain Library

## Overview

This refactor consolidates the remaining domain libraries (`libs/domain/device/services` and `libs/domain/storage/services`) into a single `libs/domain` library, following clean architecture principles. The new library will contain only pure domain models, interfaces, and injection tokens - no concrete implementations or application state.

This represents the final step in the domain refactoring process, creating a clean, consolidated domain layer that aligns with the established infrastructure and application layers.

## Goals

- ✅ **Domain Consolidation**: Merge device and storage domain contracts into single library
- ✅ **Clean Architecture**: Pure domain models and interfaces only
- ✅ **Simplified Dependencies**: Single domain import path for all consuming libraries
- ✅ **Consistency**: Follow NX_LIBRARY_STANDARDS.md specifications
- ✅ **Future-Ready**: Extensible structure for additional domains

## Current Domain Structure Analysis

### Files to Consolidate from Device Domain (`libs/domain/device/services/`)

**Models & Contracts (5 files):**

- `src/lib/device.models.ts` - Device domain models
- `src/lib/contracts/device.contract.ts` - IDeviceService interface + token
- `src/lib/contracts/device-logs.contract.ts` - IDeviceLogsService interface + token
- `src/lib/contracts/device-events.contract.ts` - IDeviceEventsService interface + token
- `src/lib/contracts/storage.contract.ts` - IStorageService interface + token (device-specific)

**Configuration Files:**

- `src/index.ts`, `project.json`, `tsconfig.*`, `eslint.config.mjs`, `README.md`, etc.

### Files to Consolidate from Storage Domain (`libs/domain/storage/services/`)

**Models & Contracts (2 files):**

- `src/lib/storage.models.ts` - Storage domain models
- `src/lib/contracts/storage.contract.ts` - IStorageService interface + token

**Configuration Files:**

- `src/index.ts`, `project.json`, `tsconfig.*`, `vite.config.ts`, etc.

## Import Usage Analysis

### Application Library Dependencies (20+ files)

**Device Imports:**

- `libs/application/src/lib/device/device-store.ts` → Device models + contracts
- `libs/application/src/lib/device/methods/*.ts` (8 files) → Device + storage interfaces/tokens

**Storage Imports:**

- `libs/application/src/lib/storage/storage-store.ts` → Storage models
- `libs/application/src/lib/storage/actions/*.ts` (9 files) → Storage interfaces/tokens
- `libs/application/src/lib/storage/*.ts` (3 files) → Storage models

### Infrastructure Library Dependencies (20+ files)

**Device Infrastructure:**

- `libs/infrastructure/src/lib/device/*.ts` (6 files) → Device models + interfaces

**Storage Infrastructure:**

- `libs/infrastructure/src/lib/storage/*.ts` (6 files) → Storage models + interfaces

### Feature Library Dependencies (20+ files)

**Device Features:**

- `libs/features/devices/src/lib/device-view/device-logs/*.ts` → Device interfaces/tokens
- `libs/features/devices/src/lib/device-view/device-item/*.ts` → Device models

**Player Features:**

- `libs/features/player/src/lib/player-view/*.ts` (10+ files) → Storage models + Device models
- Various component files → DirectoryItem, FileItem, FileItemType, StorageType, etc.

## Refactor Tasks

### Phase 1: Create Consolidated Domain Library

#### 1.1 Generate Domain Library

- [ ] **Create domain Nx library using NX standards**
  ```bash
  npx nx generate @nrwl/angular:library \
    --name=domain \
    --directory=libs \
    --buildable=false \
    --publishable=false \
    --importPath=@teensyrom-nx/domain
  ```
- [ ] **Verify library structure matches NX_LIBRARY_STANDARDS.md**
  - Check `project.json` configuration (lint target only)
  - Verify `tsconfig.base.json` path mapping: `@teensyrom-nx/domain`
  - Confirm barrel export in `src/index.ts`

#### 1.2 Configure Domain Library Structure

- [ ] **Create domain-specific folders within domain**
  ```
  libs/domain/src/lib/
  ├── device/                    # Device domain
  │   ├── models/
  │   │   └── device.models.ts   # Device, DeviceStorage, etc.
  │   └── contracts/
  │       ├── device.contract.ts          # IDeviceService + token
  │       ├── device-logs.contract.ts     # IDeviceLogsService + token
  │       ├── device-events.contract.ts   # IDeviceEventsService + token
  │       └── storage.contract.ts         # IStorageService (device) + token
  ├── storage/                   # Storage domain
  │   ├── models/
  │   │   └── storage.models.ts  # StorageDirectory, FileItem, etc.
  │   └── contracts/
  │       └── storage.contract.ts         # IStorageService + token
  └── shared/                    # Future shared domain concepts
      └── (future common models/interfaces)
  ```

#### 1.3 Configure Dependencies

- [ ] **Update `project.json` with minimal dependencies**
  ```json
  {
    "name": "domain",
    "projectType": "library",
    "tags": ["domain"],
    "targets": {
      "lint": {
        "executor": "@nx/eslint:lint"
      }
    }
  }
  ```

### Phase 2: Move Domain Files to Consolidated Library

#### 2.1 Move Device Domain Files

- [ ] **Copy device models to `libs/domain/src/lib/device/models/device.models.ts`**

  - Source: `libs/domain/device/services/src/lib/device.models.ts`
  - No import changes needed (pure models)

- [ ] **Copy device contracts to `libs/domain/src/lib/device/contracts/`**
  - `device.contract.ts` (IDeviceService + DEVICE_SERVICE token)
  - `device-logs.contract.ts` (IDeviceLogsService + DEVICE_LOGS_SERVICE token)
  - `device-events.contract.ts` (IDeviceEventsService + DEVICE_EVENTS_SERVICE token)
  - `storage.contract.ts` (IStorageService for device + DEVICE_STORAGE_SERVICE token)
  - Verify all Angular imports remain correct

#### 2.2 Move Storage Domain Files

- [ ] **Copy storage models to `libs/domain/src/lib/storage/models/storage.models.ts`**

  - Source: `libs/domain/storage/services/src/lib/storage.models.ts`
  - No import changes needed (pure models/enums)

- [ ] **Copy storage contracts to `libs/domain/src/lib/storage/contracts/storage.contract.ts`**
  - Source: `libs/domain/storage/services/src/lib/contracts/storage.contract.ts`
  - Verify Angular imports remain correct

#### 2.3 Configure Domain Library Exports

- [ ] **Update `libs/domain/src/index.ts`**

  ```typescript
  // Device domain exports
  export * from './lib/device/models/device.models';
  export * from './lib/device/contracts/device.contract';
  export * from './lib/device/contracts/device-logs.contract';
  export * from './lib/device/contracts/device-events.contract';
  export * from './lib/device/contracts/storage.contract';

  // Storage domain exports
  export * from './lib/storage/models/storage.models';
  export * from './lib/storage/contracts/storage.contract';

  // Future: Additional domain exports will go here
  // export * from './lib/player/models/player.models';
  // export * from './lib/user/contracts/user.contract';
  ```

### Phase 3: Update TypeScript Configuration

#### 3.1 Add Domain Library Path Mapping

- [ ] **Update `tsconfig.base.json` paths**
  ```json
  "paths": {
    "@teensyrom-nx/domain": ["libs/domain/src/index.ts"],
    // ... keep existing paths temporarily for migration
    "@teensyrom-nx/domain/device/services": ["libs/domain/device/services/src/index.ts"],
    "@teensyrom-nx/domain/storage/services": ["libs/domain/storage/services/src/index.ts"],
    // ... other paths
  }
  ```

#### 3.2 Copy Configuration Files

- [ ] **Create proper configuration files for domain library**
  - Copy `tsconfig.lib.json` from device services (adjust paths)
  - Copy `tsconfig.spec.json` from device services (adjust coverage directory)
  - Copy `vite.config.mts` from device services (adjust cache/coverage paths)
  - Copy `eslint.config.mjs` from device services (no changes needed)

### Phase 4: Update Application Library Dependencies

#### 4.1 Update Device Store and Methods

- [ ] **Update `libs/application/src/lib/device/device-store.ts`**

  ```typescript
  // Before:
  import {
    Device,
    IDeviceService,
    DEVICE_SERVICE,
    IStorageService,
    DEVICE_STORAGE_SERVICE,
  } from '@teensyrom-nx/domain/device/services';

  // After:
  import {
    Device,
    IDeviceService,
    DEVICE_SERVICE,
    IStorageService,
    DEVICE_STORAGE_SERVICE,
  } from '@teensyrom-nx/domain';
  ```

- [ ] **Update device methods in `libs/application/src/lib/device/methods/`** (8 files)
  - `connect-device.ts`, `disconnect-device.ts`, `find-devices.ts`, etc.
  - Change imports from `@teensyrom-nx/domain/device/services` to `@teensyrom-nx/domain`

#### 4.2 Update Storage Store and Actions

- [ ] **Update `libs/application/src/lib/storage/storage-store.ts`**

  ```typescript
  // Before:
  import { StorageType, StorageDirectory } from '@teensyrom-nx/domain/storage/services';

  // After:
  import { StorageType, StorageDirectory } from '@teensyrom-nx/domain';
  ```

- [ ] **Update storage utilities**

  - `storage-key.util.ts` → Change storage imports to `@teensyrom-nx/domain`
  - `storage-helpers.ts` → Change storage imports to `@teensyrom-nx/domain`

- [ ] **Update storage actions in `libs/application/src/lib/storage/actions/`** (9 files)
  - `initialize-storage.ts`, `navigate-*.ts`, `refresh-directory.ts`, etc.
  - Change imports from `@teensyrom-nx/domain/storage/services` to `@teensyrom-nx/domain`

### Phase 5: Update Infrastructure Library Dependencies

#### 5.1 Update Device Infrastructure

- [ ] **Update device services in `libs/infrastructure/src/lib/device/`** (6 files)
  - `device.service.ts` → Change device imports to `@teensyrom-nx/domain`
  - `device.mapper.ts` → Change device model imports to `@teensyrom-nx/domain`
  - `device-logs.service.ts` → Change interface imports to `@teensyrom-nx/domain`
  - `device-events.service.ts` → Change interface imports to `@teensyrom-nx/domain`
  - `storage.service.ts` → Change interface imports to `@teensyrom-nx/domain`
  - `providers.ts` → Change token imports to `@teensyrom-nx/domain`

#### 5.2 Update Storage Infrastructure

- [ ] **Update storage services in `libs/infrastructure/src/lib/storage/`** (6 files)
  - `storage.service.ts` → Change all imports to `@teensyrom-nx/domain`
  - `storage.mapper.ts` → Change model imports to `@teensyrom-nx/domain`
  - `providers.ts` → Change token imports to `@teensyrom-nx/domain`
  - `*.spec.ts` files (3 files) → Update test imports to `@teensyrom-nx/domain`

### Phase 6: Update Feature Library Dependencies

#### 6.1 Update Device Features

- [ ] **Update device components in `libs/features/devices/src/lib/device-view/`**
  - `device-item/device-item.component.ts` → Change device model imports to `@teensyrom-nx/domain`
  - `device-logs/device-logs.component.ts` → Change interface/token imports to `@teensyrom-nx/domain`

#### 6.2 Update Player Features (Major Updates)

- [ ] **Update player components with storage/device model imports** (10+ files)
  - `player-view.component.ts` → Update StorageType, Device imports to `@teensyrom-nx/domain`
  - `player-device-container.component.ts` → Update Device imports to `@teensyrom-nx/domain`
  - `storage-container/*.ts` → Update storage model imports to `@teensyrom-nx/domain`
  - `directory-tree/*.ts` → Update StorageType, StorageDirectory imports to `@teensyrom-nx/domain`
  - `directory-files/*.ts` → Update DirectoryItem, FileItem imports to `@teensyrom-nx/domain`
  - `file-item/*.ts` → Update FileItem, FileItemType imports to `@teensyrom-nx/domain`

#### 6.3 Update Feature Test Files

- [ ] **Update test imports in feature libraries**
  - All `*.spec.ts` files referencing domain imports
  - Change from domain-specific paths to `@teensyrom-nx/domain`

### Phase 7: Build and Test Verification

#### 7.1 Lint Verification

- [ ] **Lint new domain library**
  ```bash
  npx nx lint domain
  ```
- [ ] **Lint all affected libraries**
  ```bash
  npx nx lint application
  npx nx lint infrastructure
  npx nx lint features-devices
  npx nx lint features-player
  ```

#### 7.2 Test Verification

- [ ] **Run domain library tests** (should have minimal/no tests)
  ```bash
  npx nx test domain --run
  ```
- [ ] **Run all affected library tests**
  ```bash
  npx nx test application --run
  npx nx test infrastructure --run
  npx nx test features-devices --run
  npx nx test features-player --run
  ```

#### 7.3 Build Verification

- [ ] **Build main application**
  ```bash
  npx nx build teensyrom-ui
  ```
- [ ] **Run all affected tests**
  ```bash
  npx nx affected:test --run
  ```

#### 7.4 Runtime Verification

- [ ] **Serve application**
  ```bash
  npx nx serve teensyrom-ui
  ```
- [ ] **Verify all domain functionality**
  - Device discovery and connection works
  - Device logs and events work
  - Storage directory navigation works
  - File and directory display works correctly
  - All domain operations function identically to before refactor

### Phase 8: Remove Legacy Domain Libraries

#### 8.1 Remove Device Domain Library

- [ ] **Delete entire `libs/domain/device/` directory**
  - Remove all source files, configuration files, and tests
  - This includes: `services/src/`, `project.json`, `tsconfig.*`, etc.

#### 8.2 Remove Storage Domain Library

- [ ] **Delete entire `libs/domain/storage/` directory**
  - Remove all source files, configuration files, and tests
  - This includes: `services/src/`, `project.json`, `tsconfig.*`, etc.

#### 8.3 Remove Domain Directory (Optional)

- [ ] **Remove empty `libs/domain/` directory**
  - Should now be empty after removing device and storage
  - Clean up any remaining documentation files

#### 8.4 Clean Up TypeScript Configuration

- [ ] **Remove old path mappings from `tsconfig.base.json`**

  ```json
  // Remove these lines:
  "@teensyrom-nx/domain/device/services": ["libs/domain/device/services/src/index.ts"],
  "@teensyrom-nx/domain/storage/services": ["libs/domain/storage/services/src/index.ts"],

  // Keep this line:
  "@teensyrom-nx/domain": ["libs/domain/src/index.ts"],
  ```

#### 8.5 Verify No Broken References

- [ ] **Search for any remaining old domain references**
  ```bash
  # Search for old import paths
  grep -r "domain/device/services" libs/ apps/
  grep -r "domain/storage/services" libs/ apps/
  grep -r "@teensyrom-nx/domain/" libs/ apps/
  ```
- [ ] **Ensure no broken imports or dependencies remain**

### Phase 9: Cleanup and Documentation

#### 9.1 Update Library Dependencies

- [ ] **Update `libs/application/project.json`**

  ```json
  "implicitDependencies": [
    "domain",        // New dependency
    "infrastructure"  // Existing dependency
  ]
  ```

- [ ] **Update `libs/infrastructure/project.json`**

  ```json
  "implicitDependencies": [
    "domain",        // New dependency
    "api-client"      // Existing dependency
  ]
  ```

- [ ] **Update feature library dependencies**
  - `libs/features/devices/project.json` → Add "domain" dependency
  - `libs/features/player/project.json` → Add "domain" dependency

#### 9.2 Update Documentation

- [ ] **Create `libs/domain/README.md`**

  - Document purpose of consolidated domain library
  - Explain device and storage domain structure
  - Document how to add new domains
  - Provide examples of models vs contracts

- [ ] **Update existing documentation**
  - Update architecture documentation to reflect consolidated domain
  - Update any references to old domain structure in `docs/` folder
  - Update component library documentation with new import paths

#### 9.3 Dependency Graph Verification

- [ ] **Generate dependency graph**
  ```bash
  npx nx graph
  ```
- [ ] **Verify clean dependencies**
  - Domain library has no dependencies (pure domain)
  - Infrastructure depends only on domain and external APIs
  - Application depends on domain and infrastructure
  - Features depend on domain, application, and infrastructure
  - No circular dependencies exist
  - Clean separation of concerns maintained

## Architecture After Refactor

### Current State (Post-Consolidation):

```
libs/domain/                    # Single consolidated domain library
├── src/lib/device/             # Device domain
│   ├── models/device.models.ts # Device, DeviceStorage models
│   └── contracts/              # All device service interfaces + tokens
├── src/lib/storage/            # Storage domain
│   ├── models/storage.models.ts# StorageDirectory, FileItem models
│   └── contracts/              # Storage service interfaces + tokens
└── src/index.ts               # Barrel exports (all models + contracts)

libs/infrastructure/             # All concrete implementations
├── src/lib/device/             # Device implementations → uses @teensyrom-nx/domain
└── src/lib/storage/            # Storage implementations → uses @teensyrom-nx/domain

libs/application/                # All application state management
├── src/lib/device/             # Device state → uses @teensyrom-nx/domain
└── src/lib/storage/            # Storage state → uses @teensyrom-nx/domain

libs/features/                   # Feature UI components
├── devices/                    # Device features → uses @teensyrom-nx/domain
└── player/                     # Player features → uses @teensyrom-nx/domain
```

### Final Target Architecture:

```
libs/domain/                    # Pure domain layer
├── device/                     # Device models + contracts
├── storage/                    # Storage models + contracts
└── (future)/                   # Future domains (player, user, etc.)

libs/infrastructure/             # Implementation layer
├── device/                     # Device service implementations
└── storage/                    # Storage service implementations

libs/application/                # Application state layer
├── device/                     # Device state management
├── storage/                    # Storage state management
└── (future)/                   # Future application state

libs/features/                   # Presentation layer
├── devices/                    # Device UI features
└── player/                     # Player UI features
```

## Success Criteria

- [ ] Single `libs/domain` library contains all domain models and interfaces
- [ ] All application, infrastructure, and feature libraries use `@teensyrom-nx/domain` imports
- [ ] Legacy `libs/domain/device/` and `libs/domain/storage/` libraries completely removed
- [ ] Application builds and runs without errors
- [ ] All tests pass in new consolidated structure
- [ ] Domain functionality works identically to before refactor
- [ ] Clean dependency graph with proper separation of concerns
- [ ] No circular dependencies or import issues
- [ ] Foundation set for future domain expansion (player, user, etc.)

## Risk Mitigation

- **Incremental Approach**: Copy files first, update imports systematically, then remove old libraries
- **Test Continuously**: Run tests after each major library update
- **Build Verification**: Ensure application builds after each phase
- **Import Path Validation**: Carefully verify all import path changes across 50+ files
- **Runtime Testing**: Verify functionality works after each major change
- **Rollback Strategy**: Keep old libraries until all tests pass with new structure

## Rollback Plan

If issues arise, rollback can be performed by:

1. Reverting all import paths back to original domain-specific paths
2. Restoring `libs/domain/device/services` and `libs/domain/storage/services` libraries
3. Removing `libs/domain` library
4. Restoring original `tsconfig.base.json` path mappings
5. Reverting library dependency configurations in `project.json` files

---

**Estimated Time**: 4-5 hours
**Complexity**: High (requires systematic import path updates across 50+ files in 4 libraries)
**Dependencies**: Requires completed device and storage infrastructure/application refactors
**Testing Required**: Build verification, unit tests, integration testing, runtime verification

This plan provides a systematic approach to consolidating the domain layer into a clean, single library structure while maintaining all functionality and following established architectural patterns. The consolidated domain will serve as a solid foundation for future domain expansion and maintains clean separation of concerns across the entire application.
