# Domain Model Refactor Plan

## Overview

This refactor lifts all domain models from their domain-specific folders to a shared `models/` folder at the domain root level, splitting each interface/enum into individual files for better reusability and to prevent cross-domain coupling.

## Goals

- ✅ **Shared Models**: Move models to `libs/domain/src/lib/models/` for universal access
- ✅ **Individual Files**: Split each interface/enum/type into its own file
- ✅ **Clean Imports**: Update all consuming libraries to use new import paths
- ✅ **No Cross-Domain Coupling**: Models are domain-agnostic and reusable
- ✅ **Maintainability**: Better organization for future contract development

---

## Current Structure Analysis

### Files to Refactor

**Device Models** (`libs/domain/src/lib/device/models/device.models.ts`):
- `Device` interface 
- `DeviceStorage` interface

**Storage Models** (`libs/domain/src/lib/storage/models/storage.models.ts`):
- `DirectoryItem` interface
- `FileItem` interface 
- `ViewableItemImage` interface
- `FileItemType` enum
- `StorageType` enum
- `StorageDirectory` interface

### Impact Analysis

**Libraries with Breaking Changes:**
- `libs/domain/` - Internal contract imports, main barrel exports
- `libs/application/` - Device/storage stores import models (~8 files)
- `libs/infrastructure/` - Services and mappers import models (~6 files)  
- `libs/features/devices/` - Device components import models (~4 files)
- `libs/features/player/` - Player components import models (~12 files)

**Good News**: Most consumers already import via `@teensyrom-nx/domain` barrel export, so changes are limited to:
1. Internal domain contract files (2 files)
2. Domain barrel export (1 file)  
3. Any direct model file imports (minimal)

---

## Target Structure

```
libs/domain/src/lib/
├── models/                          # NEW: Shared models folder
│   ├── device.model.ts             # Device interface
│   ├── device-storage.model.ts     # DeviceStorage interface  
│   ├── directory-item.model.ts     # DirectoryItem interface
│   ├── file-item.model.ts          # FileItem interface
│   ├── file-item-type.enum.ts      # FileItemType enum
│   ├── storage-directory.model.ts   # StorageDirectory interface
│   ├── storage-type.enum.ts        # StorageType enum
│   ├── viewable-item-image.model.ts # ViewableItemImage interface
│   └── index.ts                    # Barrel export
├── device/contracts/               # Existing contracts remain
└── storage/contracts/              # Existing contracts remain
```

---

## Refactor Tasks

### Phase 0: Establish Baseline ⏱️ ~15 min

#### 0.1 Pre-Refactor Test Baseline
- [ ] **Clear Nx cache**: `npx nx reset`
- [ ] **Run full workspace linting**: `npx nx run-many --target=lint --all`
- [ ] **Run full workspace tests**: `npx nx run-many --target=test --all --run`
- [ ] **Build main application**: `npx nx build teensyrom-ui`
- [ ] **Verify serve works**: `npx nx serve teensyrom-ui` (quick startup check)

#### 0.2 Document Baseline Results
- [ ] **Record any existing test failures** (note in refactor log)
- [ ] **Verify workspace is in good state** before proceeding
- [ ] **Stop if baseline tests fail** - fix issues before refactor

> **🚨 GATE**: All baseline tests must pass before proceeding to Phase 1

### Phase 1: Create Shared Models Structure ⏱️ ~30 min

#### 1.1 Create Models Directory & Files
- [ ] **Create `libs/domain/src/lib/models/` directory**
- [ ] **Create individual model files**:
  - `device.model.ts`
  - `device-storage.model.ts` 
  - `directory-item.model.ts`
  - `file-item.model.ts`
  - `viewable-item-image.model.ts`
  - `file-item-type.enum.ts`
  - `storage-type.enum.ts`
  - `storage-directory.model.ts`
- [ ] **Create `models/index.ts` barrel export**

#### 1.2 Update Domain Internal Dependencies  
- [ ] **Update `libs/domain/src/lib/device/contracts/device.contract.ts`**
  - Change: `import { Device } from '../models/device.models';`
  - To: `import { Device } from '../../models';`
- [ ] **Update `libs/domain/src/lib/storage/contracts/storage.contract.ts`**
  - Change: `import { StorageDirectory, StorageType } from '../models/storage.models';`
  - To: `import { StorageDirectory, StorageType } from '../../models';`

#### 1.3 Update Domain Barrel Export
- [ ] **Update `libs/domain/src/index.ts`**
  - Remove: Device/storage model exports from old paths
  - Add: `export * from './lib/models';`

### Phase 2: Verify No Breaking Changes ⏱️ ~20 min

#### 2.1 Incremental Testing (Test Each Library After Changes)
- [ ] **Clear Nx cache**: `npx nx reset`
- [ ] **Lint domain library**: `npx nx lint domain`
- [ ] **Test domain library**: `npx nx test domain --run`

#### 2.2 Test All Consuming Libraries  
- [ ] **Test application layer**: `npx nx test application --run`
- [ ] **Test infrastructure layer**: `npx nx test infrastructure --run`
- [ ] **Test device features**: `npx nx test features-devices --run`
- [ ] **Test player features**: `npx nx test player --run`
- [ ] **Test UI components**: `npx nx test ui-components --run`

#### 2.3 Build & Serve Verification
- [ ] **Build main application**: `npx nx build teensyrom-ui`
- [ ] **Verify serve works**: `npx nx serve teensyrom-ui` (quick startup check)

#### 2.4 Full Workspace Test Suite
- [ ] **Run all workspace tests**: `npx nx run-many --target=test --all --run`
- [ ] **Run all workspace linting**: `npx nx run-many --target=lint --all`

> **🚨 GATE**: All tests must pass at same level as baseline before proceeding

### Phase 3: Remove Legacy Files ⏱️ ~10 min

#### 3.1 Remove Old Model Files
- [ ] **Delete `libs/domain/src/lib/device/models/device.models.ts`**
- [ ] **Delete `libs/domain/src/lib/storage/models/storage.models.ts`** 
- [ ] **Remove empty model directories if no other files**

#### 3.2 Final Verification & Testing
- [ ] **Search for broken references**: 
  ```bash
  grep -r "device/models\|storage/models" libs/ --include="*.ts"
  ```
- [ ] **Clear Nx cache**: `npx nx reset`
- [ ] **Final workspace linting**: `npx nx run-many --target=lint --all`
- [ ] **Final workspace test suite**: `npx nx run-many --target=test --all --run`
- [ ] **Final build test**: `npx nx build teensyrom-ui`
- [ ] **Final serve verification**: `npx nx serve teensyrom-ui` (startup check)

> **✅ SUCCESS CRITERIA**: All tests pass at same level as Phase 0 baseline

### Phase 4: Documentation Update ⏱️ ~5 min
- [ ] **Update OVERVIEW_CONTEXT.md** - Reflect new shared model structure in domain layer description

---

## Import Pattern Changes

### Before:
```typescript
// Internal domain contracts
import { Device } from '../models/device.models';
import { StorageDirectory } from '../models/storage.models';

// Domain barrel (remains same)  
import { Device, StorageDirectory } from '@teensyrom-nx/domain';
```

### After:
```typescript  
// Internal domain contracts
import { Device } from '../../models';
import { StorageDirectory } from '../../models';

// Domain barrel (unchanged - consumers won't break)
import { Device, StorageDirectory } from '@teensyrom-nx/domain';
```

---

## Benefits

- ✅ **Minimal Breaking Changes**: Most imports via barrel export remain unchanged
- ✅ **Shared Models**: Universal access from single import path  
- ✅ **Individual Files**: Better maintainability and tree-shaking
- ✅ **No Cross-Domain Coupling**: Contracts can import any model cleanly
- ✅ **Future-Proof**: Foundation for new contracts using multiple models

## Risk Assessment

**Low Risk**: 
- Most consumers use barrel exports (`@teensyrom-nx/domain`)
- Only internal domain contract files need import changes
- Systematic testing after each phase

**Estimated Time**: ~75 minutes (including baseline testing)
**Complexity**: Low-Medium (limited scope, mostly file movements, comprehensive testing)

---

## Rollback Plan

If issues arise:
1. Restore old `device/models/device.models.ts` and `storage/models/storage.models.ts`
2. Revert domain contract import changes  
3. Revert domain barrel export changes
4. Delete new `models/` directory