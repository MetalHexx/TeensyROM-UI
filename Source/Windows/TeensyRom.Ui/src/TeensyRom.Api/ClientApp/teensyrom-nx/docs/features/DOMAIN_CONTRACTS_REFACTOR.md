# Domain Contracts Refactor Plan

## Overview

This refactor lifts all domain contracts from their domain-specific folders to a shared `contracts/` folder at the domain root level, splitting each contract interface and injection token into individual files for better reusability and to prevent cross-domain coupling.

## Goals

- ✅ **Shared Contracts**: Move contracts to `libs/domain/src/lib/contracts/` for universal access
- ✅ **Individual Files**: Split each interface/token into its own file
- ✅ **Clean Imports**: Update all consuming libraries to use new import paths
- ✅ **No Cross-Domain Coupling**: Contracts are domain-agnostic and reusable
- ✅ **Maintainability**: Better organization for future contract development

---

## Current Structure Analysis

### Files to Refactor

**Device Contracts** (`libs/domain/src/lib/device/contracts/`):

- `device.contract.ts` - `IDeviceService` interface + `DEVICE_SERVICE` token
- `device-events.contract.ts` - `IDeviceEventsService` interface + `DEVICE_EVENTS_SERVICE` token
- `device-logs.contract.ts` - `IDeviceLogsService` interface + `DEVICE_LOGS_SERVICE` token

**Storage Contracts** (`libs/domain/src/lib/storage/contracts/`):

- `storage.contract.ts` - `IStorageService` interface + `STORAGE_SERVICE` token

**Additional Tokens** (`libs/domain/src/index.ts`):

- `DEVICE_STORAGE_SERVICE` - Uses `IStorageService` interface

### Impact Analysis

**Libraries with Breaking Changes:**

- `libs/domain/` - Internal barrel exports (1 file)
- `libs/infrastructure/` - Service implementations and providers (~6 files)
- `libs/features/player/` - Test files importing providers (~1 file)

**Good News**: Most consumers already import via `@teensyrom-nx/domain` barrel export, so changes are limited to:

1. Domain barrel export (1 file)
2. Infrastructure service implementations (~4 files)
3. Infrastructure provider files (~2 files)
4. Any test files with direct provider imports (~1 file)

---

## Target Structure

```
libs/domain/src/lib/
├── contracts/                           # NEW: Shared contracts folder
│   ├── device.contract.ts              # IDeviceService + DEVICE_SERVICE
│   ├── device-events.contract.ts       # IDeviceEventsService + DEVICE_EVENTS_SERVICE
│   ├── device-logs.contract.ts         # IDeviceLogsService + DEVICE_LOGS_SERVICE
│   ├── storage.contract.ts             # IStorageService + STORAGE_SERVICE
│   ├── device-storage.token.ts         # DEVICE_STORAGE_SERVICE token
│   └── index.ts                        # Barrel export
├── models/                             # Existing shared models
└── (device/storage folders can be removed)
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

### Phase 1: Create Shared Contracts Structure ⏱️ ~30 min

#### 1.1 Create Contracts Directory & Files

- [ ] **Create `libs/domain/src/lib/contracts/` directory**
- [ ] **Create individual contract files**:
  - `device.contract.ts` (IDeviceService + DEVICE_SERVICE)
  - `device-events.contract.ts` (IDeviceEventsService + DEVICE_EVENTS_SERVICE)
  - `device-logs.contract.ts` (IDeviceLogsService + DEVICE_LOGS_SERVICE)
  - `storage.contract.ts` (IStorageService + STORAGE_SERVICE)
  - `device-storage.token.ts` (DEVICE_STORAGE_SERVICE token)
- [ ] **Create `contracts/index.ts` barrel export**

#### 1.2 Update Domain Barrel Export

- [ ] **Update `libs/domain/src/index.ts`**
  - Remove: All contract exports from old paths
  - Add: `export * from './lib/contracts';`
  - Move: `DEVICE_STORAGE_SERVICE` token to new file and export via barrel

### Phase 2: Verify No Breaking Changes ⏱️ ~20 min

#### 2.1 Incremental Testing (Test Each Library After Changes)

- [ ] **Clear Nx cache**: `npx nx reset`
- [ ] **Lint domain library**: `npx nx lint domain`
- [ ] **Test domain library**: `npx nx test domain --run`

#### 2.2 Test All Consuming Libraries

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

### Phase 3: Remove Legacy Files ⏱️ ~15 min

#### 3.1 Remove Old Contract Files & Directories

- [ ] **Delete `libs/domain/src/lib/device/contracts/device.contract.ts`**
- [ ] **Delete `libs/domain/src/lib/device/contracts/device-events.contract.ts`**
- [ ] **Delete `libs/domain/src/lib/device/contracts/device-logs.contract.ts`**
- [ ] **Delete `libs/domain/src/lib/storage/contracts/storage.contract.ts`**
- [ ] **Remove empty contract directories**:
  - `libs/domain/src/lib/device/contracts/` (if empty)
  - `libs/domain/src/lib/storage/contracts/` (if empty)
- [ ] **Remove empty domain directories if no other files**:
  - `libs/domain/src/lib/device/` (if empty)
  - `libs/domain/src/lib/storage/` (if empty)

#### 3.2 Final Verification & Testing

- [ ] **Search for broken references**:
  ```bash
  grep -r "device/contracts\|storage/contracts" libs/ --include="*.ts"
  ```
- [ ] **Clear Nx cache**: `npx nx reset`
- [ ] **Final workspace linting**: `npx nx run-many --target=lint --all`
- [ ] **Final workspace test suite**: `npx nx run-many --target=test --all --run`
- [ ] **Final build test**: `npx nx build teensyrom-ui`
- [ ] **Final serve verification**: `npx nx serve teensyrom-ui` (startup check)

> **✅ SUCCESS CRITERIA**: All tests pass at same level as Phase 0 baseline

### Phase 4: Documentation Update ⏱️ ~5 min

- [ ] **Update OVERVIEW_CONTEXT.md** - Reflect new shared contract structure in domain layer description

---

## File Content Details

### New Contract Files Content

**`contracts/device.contract.ts`**:

```typescript
import { InjectionToken } from '@angular/core';
import {
  DisconnectDeviceResponse,
  PingDeviceResponse,
  ResetDeviceResponse,
} from '@teensyrom-nx/data-access/api-client';
import { Observable } from 'rxjs';
import { Device } from '../models';

export interface IDeviceService {
  findDevices(autoConnectNew: boolean): Observable<Device[]>;
  getConnectedDevices(): Observable<Device[]>;
  connectDevice(deviceId: string): Observable<Device>;
  disconnectDevice(deviceId: string): Observable<DisconnectDeviceResponse>;
  resetDevice(deviceId: string): Observable<ResetDeviceResponse>;
  pingDevice(deviceId: string): Observable<PingDeviceResponse>;
}

export const DEVICE_SERVICE = new InjectionToken<IDeviceService>('DEVICE_SERVICE');
```

**`contracts/device-events.contract.ts`**:

```typescript
import { InjectionToken, Signal } from '@angular/core';
import { DeviceState } from '@teensyrom-nx/data-access/api-client';

export interface IDeviceEventsService {
  readonly allEvents: Signal<Map<string, DeviceState>>;
  connect(): void;
  disconnect(): void;
  getDeviceState(deviceId: string): Signal<DeviceState | null>;
}

export const DEVICE_EVENTS_SERVICE = new InjectionToken<IDeviceEventsService>(
  'DEVICE_EVENTS_SERVICE'
);
```

**`contracts/device-logs.contract.ts`**:

```typescript
import { InjectionToken, Signal } from '@angular/core';

export interface IDeviceLogsService {
  readonly isConnected: Signal<boolean>;
  readonly logs: Signal<string[]>;
  connect(): void;
  disconnect(): void;
  clear(): void;
}

export const DEVICE_LOGS_SERVICE = new InjectionToken<IDeviceLogsService>('DEVICE_LOGS_SERVICE');
```

**`contracts/storage.contract.ts`**:

```typescript
import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { StorageDirectory, StorageType } from '../models';

/**
 * Storage service contract defining the interface for storage operations.
 * This interface is implemented by concrete storage services in the infrastructure layer.
 */
export interface IStorageService {
  /**
   * Retrieves directory contents for a specific device and storage type.
   * @param deviceId - The unique identifier of the device
   * @param storageType - The type of storage (USB, SD, etc.)
   * @param path - Optional path within the storage (defaults to root)
   * @returns Observable of StorageDirectory containing directory contents
   */
  getDirectory(
    deviceId: string,
    storageType: StorageType,
    path?: string
  ): Observable<StorageDirectory>;

  /**
   * Index storage on a device.
   * @param deviceId - The unique identifier of the device
   * @param storageType - The type of storage (USB, SD, etc.)
   * @param startingPath - Optional starting path for indexing
   * @returns Observable of index operation result
   */
  index(deviceId: string, storageType: StorageType, startingPath?: string): Observable<unknown>;

  /**
   * Index all storage on all devices.
   * @returns Observable of index all operation result
   */
  indexAll(): Observable<unknown>;
}

/**
 * Injection token for IStorageService to enable dependency injection by interface.
 * This allows the domain to depend on the interface while the infrastructure
 * provides the concrete implementation.
 */
export const STORAGE_SERVICE = new InjectionToken<IStorageService>('STORAGE_SERVICE');
```

**`contracts/device-storage.token.ts`**:

```typescript
import { InjectionToken } from '@angular/core';
import { IStorageService } from './storage.contract';

/**
 * Injection token for device-specific storage service.
 * Uses the same IStorageService interface but represents a different implementation
 * or configuration for device-specific storage operations.
 */
export const DEVICE_STORAGE_SERVICE = new InjectionToken<IStorageService>('DEVICE_STORAGE_SERVICE');
```

**`contracts/index.ts`**:

```typescript
// Device contracts
export * from './device.contract';
export * from './device-events.contract';
export * from './device-logs.contract';

// Storage contracts
export * from './storage.contract';
export * from './device-storage.token';
```

---

## Import Pattern Changes

### Before:

```typescript
// Domain barrel (what consumers use - remains same)
import { IDeviceService, DEVICE_SERVICE } from '@teensyrom-nx/domain';
import { IStorageService, STORAGE_SERVICE } from '@teensyrom-nx/domain';
```

### After:

```typescript
// Domain barrel (unchanged - consumers won't break)
import { IDeviceService, DEVICE_SERVICE } from '@teensyrom-nx/domain';
import { IStorageService, STORAGE_SERVICE } from '@teensyrom-nx/domain';
```

### Domain Barrel Export Changes:

```typescript
// Before:
export * from './lib/device/contracts/device.contract';
export * from './lib/device/contracts/device-logs.contract';
export * from './lib/device/contracts/device-events.contract';
export * from './lib/storage/contracts/storage.contract';

// After:
export * from './lib/contracts';
```

---

## Benefits

- ✅ **No Breaking Changes**: All imports via barrel export remain unchanged
- ✅ **Shared Contracts**: Universal access from single import path
- ✅ **Individual Files**: Better maintainability and tree-shaking
- ✅ **No Cross-Domain Coupling**: All contracts accessible equally
- ✅ **Clean Organization**: Logical separation of contracts from models
- ✅ **Future-Proof**: Foundation for new cross-domain contracts

## Risk Assessment

**Low Risk**:

- All consumers use barrel exports (`@teensyrom-nx/domain`)
- Only domain barrel export needs changes
- No import path changes for consuming libraries
- Systematic testing after each phase

**Estimated Time**: ~70 minutes (including baseline testing)
**Complexity**: Low (mainly file movements, no breaking changes for consumers)

---

## Rollback Plan

If issues arise:

1. Restore old contract files in original locations:
   - `device/contracts/device.contract.ts`
   - `device/contracts/device-events.contract.ts`
   - `device/contracts/device-logs.contract.ts`
   - `storage/contracts/storage.contract.ts`
2. Revert domain barrel export changes in `index.ts`
3. Restore `DEVICE_STORAGE_SERVICE` token in `index.ts`
4. Delete new `contracts/` directory

---

## Dependencies

**Should be run AFTER**:

- Domain Model Refactor (if not already completed)
- All baseline tests passing

**Can be run INDEPENDENTLY**:

- This refactor doesn't depend on the model refactor
- Can be done before or after model refactor
- Both refactors together will create clean domain architecture

---

## Combined Result (After Both Refactors)

```
libs/domain/src/lib/
├── models/           # Shared domain models (after model refactor)
├── contracts/        # Shared domain contracts (this refactor)
└── index.ts         # Clean barrel export for all domain concerns
```
