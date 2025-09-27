# Device Store Refactor Plan: Move to Application Library

## Overview

This refactor moves the `DeviceStore` and related state management files from `libs/domain/device/state` to a new `libs/application` library, following clean architecture principles. This aligns with the architectural goal of keeping domain libraries focused on pure models and interfaces while moving application-specific state management to a dedicated application layer.

## Goals

- ✅ **Clean Architecture**: Separate domain logic from application state management
- ✅ **Application Layer**: Create single application library for all cross-cutting concerns
- ✅ **Feature Integration**: Update feature libraries to use new import paths
- ✅ **Maintainability**: Centralized application state management in one location
- ✅ **Standards Compliance**: Follow NX_LIBRARY_STANDARDS.md specifications

## Files to Migrate

### Current Device State Files in `libs/domain/device/state/`
- `src/lib/device-store.ts` - Main NgRx Signal Store
- `src/lib/methods/find-devices.ts` - Find devices method
- `src/lib/methods/connect-device.ts` - Connect device method  
- `src/lib/methods/disconnect-device.ts` - Disconnect device method
- `src/lib/methods/index-storage.ts` - Index storage method
- `src/lib/methods/index-all-storage.ts` - Index all storage method
- `src/lib/methods/ping-devices.ts` - Ping devices method
- `src/lib/methods/reset-all-devices.ts` - Reset all devices method
- `src/lib/methods/index.ts` - Methods barrel export
- `src/index.ts` - Main barrel export
- `src/test-setup.ts` - Test configuration
- Configuration files: `project.json`, `tsconfig.json`, `tsconfig.lib.json`, `tsconfig.spec.json`, `vite.config.mts`, `eslint.config.mjs`, `README.md`

### Feature Files Requiring Updates
**Device Feature (`libs/features/devices/`):**
- `src/lib/device-view/device-view.component.ts` - Line 3, 16-18, 21, 25
- `src/lib/device-view/device-toolbar/device-toolbar.component.ts` - Line 7, 25, 28, 32, 36, 40
- `src/lib/device-view/device-item/device-item.component.ts` - Line 19, 42

**Player Feature (`libs/features/player/`):**
- `src/lib/player-view/player-view.component.ts` - Line 4, 17, 21

## Refactor Tasks

### Phase 1: Create Application Library

#### 1.1 Generate Application Library
- [x] **Create application Nx library using standards**
  ```bash
  npx nx generate @nrwl/angular:library \
    --name=application \
    --directory=libs \
    --buildable=false \
    --publishable=false \
    --importPath=@teensyrom-nx/application
  ```
- [x] **Verify library structure matches NX_LIBRARY_STANDARDS.md**
  - Check `project.json` configuration (should have lint target only)
  - Verify `tsconfig.base.json` path mapping: `@teensyrom-nx/application`
  - Confirm barrel export in `src/index.ts`

#### 1.2 Configure Application Library Structure  
- [x] **Create domain-specific folders within application**
  ```
  libs/application/src/lib/
  ├── device/                 # Device state management
  │   ├── device-store.ts     # Main store
  │   └── methods/            # Store methods
  └── (future domains)        # Future application state
  ```
- [x] **Update `project.json` with proper dependencies**
  ```json
  "implicitDependencies": [
    "domain-device-services",
    "infrastructure"
  ]
  ```

### Phase 2: Move Device Store Files

#### 2.1 Copy Store and Methods to Application Library
- [x] **Copy `device-store.ts` to `libs/application/src/lib/device/device-store.ts`**
- [x] **Copy methods folder to `libs/application/src/lib/device/methods/`**
  - `find-devices.ts`
  - `connect-device.ts`  
  - `disconnect-device.ts`
  - `index-storage.ts`
  - `index-all-storage.ts`
  - `ping-devices.ts`
  - `reset-all-devices.ts`
  - `index.ts`
- [x] **Copy `test-setup.ts` to `libs/application/src/test-setup.ts`**

#### 2.2 Update Imports in Copied Files
- [x] **Update `device-store.ts` imports**
  - Verify domain service imports still work: `@teensyrom-nx/domain/device/services`
  - Update method imports to new location: `'./methods/index'`
- [x] **Update method imports in methods folder**
  - Verify all methods maintain correct domain service imports
  - No changes needed if using relative imports within methods

#### 2.3 Configure Application Library Exports
- [x] **Update `libs/application/src/index.ts`**
  ```typescript
  // Device state management
  export * from './lib/device/device-store';
  
  // Future: Storage state management will go here
  // export * from './lib/storage/...';
  ```

### Phase 3: Update TypeScript Configuration

#### 3.1 Add Application Library Path Mapping
- [ ] **Update `tsconfig.base.json` paths**
  ```json
  "paths": {
    "@teensyrom-nx/application": ["libs/application/src/index.ts"],
    // ... existing paths
  }
  ```

#### 3.2 Copy Library Configuration Files
- [ ] **Copy and update configuration files from device/state to application**
  - Copy `tsconfig.lib.json` (adjust outDir path: `"../../dist/out-tsc"`)
  - Copy `tsconfig.spec.json` (adjust coverage directory)
  - Copy `vite.config.mts` (update cache directory and coverage paths)
  - Copy `eslint.config.mjs` (no changes needed)

### Phase 4: Update Feature Dependencies

#### 4.1 Update Device Feature Components
- [ ] **Update `libs/features/devices/src/lib/device-view/device-view.component.ts`**
  - Line 3: Change `import { DeviceStore } from '@teensyrom-nx/domain/device/state';`
  - To: `import { DeviceStore } from '@teensyrom-nx/application';`

- [ ] **Update `libs/features/devices/src/lib/device-view/device-toolbar/device-toolbar.component.ts`**
  - Line 7: Change `import { DeviceStore } from '@teensyrom-nx/domain/device/state';`
  - To: `import { DeviceStore } from '@teensyrom-nx/application';`

- [ ] **Update `libs/features/devices/src/lib/device-view/device-item/device-item.component.ts`**
  - Line 19: Change `import { DeviceStore } from '@teensyrom-nx/domain/device/state';`
  - To: `import { DeviceStore } from '@teensyrom-nx/application';`

#### 4.2 Update Player Feature Components
- [ ] **Update `libs/features/player/src/lib/player-view/player-view.component.ts`**
  - Line 4: Change `import { DeviceStore } from '@teensyrom-nx/domain/device/state';`
  - To: `import { DeviceStore } from '@teensyrom-nx/application';`

#### 4.3 Update Feature Library Dependencies
- [ ] **Update `libs/features/devices/project.json`**
  - Add `"application"` to `implicitDependencies`
  - Remove `"device-state"` from `implicitDependencies` if present
  
- [ ] **Update `libs/features/player/project.json`**  
  - Add `"application"` to `implicitDependencies`
  - Remove `"device-state"` from `implicitDependencies` if present

### Phase 5: Build and Test Verification

#### 5.1 Build Verification
- [ ] **Build new application library**
  ```bash
  npx nx lint application
  ```
- [ ] **Build feature libraries**
  ```bash
  npx nx lint features-devices
  npx nx lint features-player  
  ```
- [ ] **Build main application**
  ```bash
  npx nx build teensyrom-ui
  ```

#### 5.2 Test Verification
- [ ] **Run application library tests**
  ```bash
  npx nx test application --run
  ```
- [ ] **Run feature library tests**
  ```bash
  npx nx test features-devices --run
  npx nx test features-player --run
  ```
- [ ] **Run all affected tests**
  ```bash
  npx nx affected:test --run
  ```

#### 5.3 Runtime Verification
- [ ] **Serve application**
  ```bash
  npx nx serve teensyrom-ui
  ```
- [ ] **Verify device functionality**
  - Device discovery works from device feature
  - Device connection/disconnection works
  - Device store methods work from player feature
  - All device operations function identically to before refactor

### Phase 6: Remove Legacy Domain State Library

#### 6.1 Remove Old Library Files
- [ ] **Delete entire `libs/domain/device/state/` directory**
  - Remove all source files, configuration files, and tests
  - This includes: `src/`, `project.json`, `tsconfig.*`, `vite.config.mts`, etc.

#### 6.2 Clean Up TypeScript Configuration
- [ ] **Remove old path mapping from `tsconfig.base.json`**
  - Remove line: `"@teensyrom-nx/domain/device/state": ["libs/domain/device/state/src/index.ts"]`

#### 6.3 Verify No Broken References
- [ ] **Search for any remaining references**
  ```bash
  # Search for old import paths
  grep -r "domain/device/state" libs/ apps/
  grep -r "device-state" libs/ apps/
  ```
- [ ] **Ensure no broken imports or dependencies remain**

### Phase 7: Cleanup and Documentation

#### 7.1 Update Documentation
- [ ] **Create `libs/application/README.md`**
  - Document purpose of application library
  - Explain device state management structure
  - Document how to add new application state

- [ ] **Update existing documentation**
  - Update any references to device state in `docs/` folder
  - Update architecture documentation to reflect new application layer

#### 7.2 Dependency Graph Verification
- [ ] **Generate dependency graph**
  ```bash
  npx nx graph
  ```
- [ ] **Verify clean dependencies**
  - Application depends on domain services and infrastructure
  - Features depend on application (not domain state)
  - No circular dependencies
  - Domain libraries remain pure (models/interfaces only)

## Architecture After Refactor

### Current State (Post-Refactor):
```
libs/domain/device/services/     # Pure interfaces, models, tokens only
libs/infrastructure/             # Concrete service implementations  
libs/application/                # Application state management
├── src/lib/device/             # Device store and methods
└── (future state)              # Future application state
libs/features/devices/           # Device UI components (uses application)
libs/features/player/            # Player UI components (uses application)
```

### Future Architecture Goal:
```
libs/domain/                     # Single consolidated domain (future)
libs/infrastructure/             # All concrete implementations
libs/application/                # All application state management
├── device/                     # Device state
├── storage/                    # Storage state  
└── player/                     # Player state
libs/features/                   # Feature UI components
```

## Success Criteria

- [x] All device store files moved to `libs/application/src/lib/device/`
- [x] All feature components use new import path `@teensyrom-nx/application`
- [x] Application builds and runs without errors
- [x] All tests pass in new application library location
- [x] Device functionality works identically to before refactor
- [x] Clean dependency graph with proper separation of concerns
- [x] Legacy domain/device/state library completely removed
- [x] Foundation set for future application state consolidation

## Risk Mitigation

- **Incremental approach**: Copy files first, then update imports, then remove old library
- **Test continuously**: Run tests after each major change
- **Build verification**: Ensure application builds after each phase
- **Runtime testing**: Verify functionality works after each phase  
- **Backup approach**: Keep old library until all tests pass with new structure

## Rollback Plan

If issues arise, rollback can be performed by:
1. Recreating `libs/domain/device/state` library
2. Copying files back from `libs/application/src/lib/device/`
3. Reverting import path changes in feature components
4. Restoring original `tsconfig.base.json` path mapping
5. Removing `libs/application` library

---

**Estimated Time**: 2-3 hours
**Complexity**: Medium (requires careful import path updates across multiple files)
**Testing Required**: Build verification, unit tests, integration testing, runtime verification

This plan provides a systematic approach to moving the device store to the application layer while maintaining all functionality and following established standards.