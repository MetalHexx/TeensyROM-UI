# Clean Architecture Refactoring - One-Shot Implementation

## 🎯 Target Architecture

### Domain Layer (`libs/domain/`)

**Pure business logic - ZERO external dependencies**

- Domain models/entities (Device, Storage, Player)
- Service interfaces (IDeviceService, IStorageService, IPlayerService)
- Domain enums and value objects
- Business rules and domain logic
- NO Angular, NO API client imports, NO framework dependencies

### Application Layer (`libs/application/`)

**State management and orchestration**

- NgRx Signal Stores (DeviceStore, StorageStore, PlayerStore)
- Application services (use cases)
- State utilities (key utils, helpers)
- Store actions, selectors, effects
- Depends ONLY on Domain interfaces

### Infrastructure Layer (`libs/infrastructure/`)

**External concerns and implementations**

- Concrete service implementations
- API client integrations
- Data mappers (API DTOs ↔ Domain models)
- Dependency injection providers
- Depends on Domain interfaces + API client

## 📊 Current Architecture Issues

### Dependency Violations Found

- `libs/domain/device/services/src/lib/device.models.ts:1` - Imports from API client
- `libs/domain/device/services/src/lib/device.service.ts` - Concrete implementation in domain
- `libs/domain/storage/services/src/lib/storage.service.ts` - Concrete implementation in domain
- All domain mappers - Should be infrastructure concern
- Mixed state/service concerns in domain libraries

### Files Requiring Migration

- **Domain TypeScript files**: 40+ files
- **API client integration points**: 30+ integration points
- **Feature components**: 30+ components with import updates
- **State management files**: 15+ store-related files
- **Service implementations**: 6+ concrete services
- **Mappers**: 4+ mapping utilities
- **Project configurations**: 8+ project.json files
- **Test files**: 25+ test files requiring updates

## 📋 Comprehensive Migration Plan

### Phase 1: Create New Library Structure

1. **Create `libs/domain/` library**

   - Pure TypeScript project (no Angular dependencies)
   - Configure as publishable library
   - Set up ESLint for pure TypeScript
   - Create barrel exports

2. **Create `libs/application/` library**

   - Angular library with NgRx dependencies
   - Configure Signal Store support
   - Set up testing configuration
   - Create barrel exports

3. **Create `libs/infrastructure/` library**

   - Angular library with API client dependencies
   - Configure service registration
   - Set up integration testing
   - Create barrel exports

4. **Configure project.json files**
   - Set proper dependency relationships
   - Configure build order
   - Set up linting and testing targets

### Phase 2: Extract Pure Domain Models

#### Device Domain

- **Extract from**: `libs/domain/device/services/src/lib/device.models.ts`
- **Remove**: API client imports (`DeviceState`, `TeensyStorageType`)
- **Create**: Pure domain models in `libs/domain/src/lib/device/models/`
- **Models**: Device, DeviceStorage
- **Interfaces**: IDeviceService

#### Storage Domain

- **Extract from**: `libs/domain/storage/services/src/lib/storage.models.ts`
- **Keep**: Pure domain models (already clean)
- **Move to**: `libs/domain/src/lib/storage/models/`
- **Models**: DirectoryItem, FileItem, ViewableItemImage, StorageDirectory
- **Enums**: FileItemType, StorageType
- **Interfaces**: IStorageService

#### Player Domain

- **Create new**: Based on PLAYER_DOMAIN_DESIGN.md
- **Location**: `libs/domain/src/lib/player/models/`
- **Models**: PlayerFileItem, PlayerItemImage, LaunchedFile, PlayerDirectoryContext
- **Enums**: PlayerFileType, PlayerFilterType, PlayerScope, PlayerStatus
- **Interfaces**: IPlayerService

### Phase 3: Move State Management to Application

#### DeviceStore Migration

- **From**: `libs/domain/device/state/`
- **To**: `libs/application/src/lib/device/`
- **Files**:
  - `device-store.ts`
  - `methods/` → `actions/`
  - All store utilities and helpers
- **Updates**: Import domain interfaces instead of concrete services

#### StorageStore Migration

- **From**: `libs/domain/storage/state/`
- **To**: `libs/application/src/lib/storage/`
- **Files**:
  - `storage-store.ts`
  - `storage-helpers.ts`
  - `storage-key.util.ts`
  - `actions/` (9 action files)
  - `selectors/` (4 selector files)
- **Updates**: Use domain interfaces and models

#### PlayerStore Creation

- **Create new**: `libs/application/src/lib/player/`
- **Based on**: PLAYER_DOMAIN_DESIGN.md patterns
- **Files**:
  - `player-store.ts`
  - `player-helpers.ts`
  - `player-key.util.ts`
  - `actions/` (4 action files)
  - `selectors/` (4 selector files)

### Phase 4: Move Infrastructure Implementations

#### Service Implementations

- **DeviceService**: Move from `libs/domain/device/services/src/lib/device.service.ts`
- **StorageService**: Move from `libs/domain/storage/services/src/lib/storage.service.ts`
- **PlayerService**: Create new implementation
- **Target**: `libs/infrastructure/src/lib/{domain}/{service}.ts`

#### Mappers

- **DeviceMapper**: Move from domain to infrastructure
- **StorageMapper**: Move from domain to infrastructure
- **PlayerMapper**: Create new based on design
- **Target**: `libs/infrastructure/src/lib/{domain}/{mapper}.ts`

#### Dependency Injection

- **Create**: `libs/infrastructure/src/lib/providers.ts`
- **Register**: All service implementations with their interfaces
- **Pattern**:
  ```typescript
  export const INFRASTRUCTURE_PROVIDERS = [
    { provide: IDeviceService, useClass: DeviceService },
    { provide: IStorageService, useClass: StorageService },
    { provide: IPlayerService, useClass: PlayerService },
  ];
  ```

### Phase 5: Update All Dependencies

#### Feature Components (30+ files)

- **DeviceView components**: Update imports from domain/state to application
- **PlayerView components**: Update imports to new architecture
- **Pattern**:
  ```typescript
  // OLD: import { DeviceStore } from '@teensyrom-nx/domain/device/state';
  // NEW: import { DeviceStore } from '@teensyrom-nx/application';
  ```

#### Project Configurations (8+ files)

- **Update**: All `project.json` dependency arrays
- **Domain**: Remove all external dependencies
- **Application**: Depend only on domain
- **Infrastructure**: Depend on domain + api-client
- **Features**: Depend on application + infrastructure

#### Barrel Exports

- **Domain**: Export models, interfaces, enums
- **Application**: Export stores, utilities
- **Infrastructure**: Export providers
- **Pattern**: Layer-specific public APIs only

### Phase 6: Comprehensive Testing & Verification

#### Test Updates (25+ files)

- **Unit tests**: Update mocks to use domain interfaces
- **Integration tests**: Update service registrations
- **Store tests**: Update import paths
- **Component tests**: Update dependency injections

#### Build Verification

1. **Type checking**: `npx nx run-many -t typecheck`
2. **Linting**: `npx nx run-many -t lint`
3. **Unit tests**: `npx nx run-many -t test`
4. **Integration tests**: `npx nx run-many -t test:integration`
5. **Build**: `npx nx build app`

## 🗂️ Target File Structure

```
libs/
├── domain/                          # Pure business logic
│   ├── project.json                 # Pure TypeScript, no dependencies
│   ├── src/
│   │   ├── index.ts                 # Public API exports
│   │   └── lib/
│   │       ├── device/
│   │       │   ├── models/
│   │       │   │   ├── device.model.ts
│   │       │   │   └── device-storage.model.ts
│   │       │   └── interfaces/
│   │       │       └── device.service.interface.ts
│   │       ├── storage/
│   │       │   ├── models/
│   │       │   │   ├── file-item.model.ts
│   │       │   │   ├── directory-item.model.ts
│   │       │   │   └── storage-directory.model.ts
│   │       │   ├── enums/
│   │       │   │   ├── file-item-type.enum.ts
│   │       │   │   └── storage-type.enum.ts
│   │       │   └── interfaces/
│   │       │       └── storage.service.interface.ts
│   │       └── player/
│   │           ├── models/
│   │           │   ├── player-file-item.model.ts
│   │           │   ├── launched-file.model.ts
│   │           │   └── player-directory-context.model.ts
│   │           ├── enums/
│   │           │   ├── player-file-type.enum.ts
│   │           │   ├── player-filter-type.enum.ts
│   │           │   ├── player-scope.enum.ts
│   │           │   └── player-status.enum.ts
│   │           └── interfaces/
│   │               └── player.service.interface.ts
│   └── tsconfig.json                # Pure TypeScript config
├── application/                     # State management
│   ├── project.json                 # Depends: domain
│   ├── src/
│   │   ├── index.ts                 # Store exports
│   │   └── lib/
│   │       ├── device/
│   │       │   ├── device-store.ts
│   │       │   ├── device-helpers.ts
│   │       │   ├── actions/
│   │       │   │   ├── index.ts
│   │       │   │   ├── find-devices.ts
│   │       │   │   ├── connect-device.ts
│   │       │   │   └── disconnect-device.ts
│   │       │   └── selectors/
│   │       │       ├── index.ts
│   │       │       └── get-connected-devices.ts
│   │       ├── storage/
│   │       │   ├── storage-store.ts
│   │       │   ├── storage-helpers.ts
│   │       │   ├── storage-key.util.ts
│   │       │   ├── actions/
│   │       │   │   ├── index.ts
│   │       │   │   ├── initialize-storage.ts
│   │       │   │   ├── navigate-to-directory.ts
│   │       │   │   ├── navigate-up-one-directory.ts
│   │       │   │   ├── navigate-directory-backward.ts
│   │       │   │   ├── navigate-directory-forward.ts
│   │       │   │   ├── refresh-directory.ts
│   │       │   │   ├── remove-storage.ts
│   │       │   │   └── remove-all-storage.ts
│   │       │   └── selectors/
│   │       │       ├── index.ts
│   │       │       ├── get-device-storage-entries.ts
│   │       │       ├── get-selected-directory-for-device.ts
│   │       │       ├── get-selected-directory-state.ts
│   │       │       └── get-device-directories.ts
│   │       └── player/
│   │           ├── player-store.ts
│   │           ├── player-helpers.ts
│   │           ├── player-key.util.ts
│   │           ├── actions/
│   │           │   ├── index.ts
│   │           │   ├── initialize-player.ts
│   │           │   ├── launch-file.ts
│   │           │   ├── launch-random-file.ts
│   │           │   └── remove-player.ts
│   │           └── selectors/
│   │               ├── index.ts
│   │               ├── get-device-player.ts
│   │               ├── get-current-file.ts
│   │               ├── get-player-directory-context.ts
│   │               └── get-player-status.ts
│   └── tsconfig.json                # Angular + NgRx config
├── infrastructure/                  # Implementations
│   ├── project.json                 # Depends: domain, api-client
│   ├── src/
│   │   ├── index.ts                 # Provider exports
│   │   └── lib/
│   │       ├── device/
│   │       │   ├── device.service.ts
│   │       │   ├── device.mapper.ts
│   │       │   ├── device.events.service.ts
│   │       │   └── device.logs.service.ts
│   │       ├── storage/
│   │       │   ├── storage.service.ts
│   │       │   └── storage.mapper.ts
│   │       ├── player/
│   │       │   ├── player.service.ts
│   │       │   └── player.mapper.ts
│   │       └── providers.ts         # DI configuration
│   └── tsconfig.json                # Angular + API client config
└── features/                        # UI components (updated imports)
    ├── devices/                     # Uses application stores
    │   └── src/lib/device-view/
    │       └── device-view.component.ts
    └── player/                      # Uses application stores
        └── src/lib/player-view/
            └── player-view.component.ts
```

## ⚡ Migration Benefits

1. **True Separation of Concerns** - Clear layer boundaries enforced
2. **Testable Business Logic** - Domain layer completely isolated
3. **Swappable Implementations** - Infrastructure can be replaced
4. **Dependency Inversion** - Proper direction of dependencies
5. **Future-Proof Architecture** - Easy to extend and maintain
6. **Framework Independence** - Domain logic not tied to Angular
7. **Compile-Time Safety** - TypeScript enforces architecture rules

## 🧪 Verification Strategy

### Build Process

1. **Domain**: `npx nx build domain` - Must succeed with zero dependencies
2. **Application**: `npx nx build application` - Must only depend on domain
3. **Infrastructure**: `npx nx build infrastructure` - Can depend on domain + api-client
4. **Features**: `npx nx build features-devices features-player` - Uses application layer

### Test Strategy

1. **Domain Tests**: Pure unit tests, no mocking needed
2. **Application Tests**: Mock domain interfaces
3. **Infrastructure Tests**: Integration tests with real API client
4. **Feature Tests**: Mock application stores

### Dependency Verification

```bash
# Verify no circular dependencies
npx nx graph

# Verify dependency direction
npx nx show projects --with-target=build | grep -E "(domain|application|infrastructure)"
```

## 🚀 Success Criteria

- [ ] All TypeScript compilation passes
- [ ] No circular dependencies in dependency graph
- [ ] All existing tests pass with new structure
- [ ] Application builds and runs correctly
- [ ] Domain layer has zero external dependencies
- [ ] Feature components successfully use application stores
- [ ] Infrastructure properly implements domain interfaces
- [ ] Clean architecture principles fully enforced

This refactoring transforms the codebase into a textbook clean architecture implementation with proper separation of concerns, dependency inversion, and maintainable structure.
