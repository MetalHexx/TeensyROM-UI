# TypeScript API Client Migration Plan

## Overview

This document outlines the migration from `typescript-angular` to `typescript-fetch` OpenAPI generator to remove Angular dependencies from the generated API client code. This will solve the domain layer testing issues while preserving existing Observable contracts.

## Problem Statement

The TeensyROM Angular application is experiencing testing issues due to Angular dependencies being pulled into domain layer code through the generated API clients:

1. **Domain State Layer** imports generated Angular services (`FilesApiService`, `DevicesApiService`)
2. **Testing Complexity**: Unit tests require Angular TestBed setup even for pure domain logic
3. **Dependency Chain**: Domain → Wrapper Services → Generated API Clients → Angular HTTP → Angular DI system

## Solution Strategy

**Use `typescript-fetch` generator** to create plain TypeScript API clients, then wrap them in Angular services that maintain the existing Observable contracts. This provides:

- ✅ Domain layer free of Angular dependencies
- ✅ Easy testing without Angular TestBed
- ✅ Existing Observable contracts preserved
- ✅ Minimal changes to existing code
- ✅ Keep established OpenAPI workflow

## Current Architecture

```
Angular App
├── DeviceService, StorageService (@Injectable wrappers)
│   └── Inject DevicesApiService, FilesApiService (Angular services)
│
Domain Layer
├── navigateToDirectory(store, storageService: StorageService)
│   └── Uses wrapper services (good)
│
Generated API Clients
├── DevicesApiService, FilesApiService (@Injectable with Angular deps)
```

## Target Architecture

```
Angular App
├── DeviceService, StorageService (@Injectable wrappers)
│   └── Use DevicesApiService, FilesApiService (plain TypeScript)
│   └── Convert Promise → Observable
│
Domain Layer
├── navigateToDirectory(store, storageService: StorageService)
│   └── Uses wrapper services (unchanged)
│
Generated API Clients
├── DevicesApiService, FilesApiService (plain TypeScript, no Angular deps)
```

## Migration Plan (Domain-by-Domain)

### Phase 1: Update API Client Generation

**Files to modify**: `libs/data-access/api-client/scripts/generate-client.js`

- [x] Change generator from `'typescript-angular'` to `'typescript-fetch'`
- [x] Remove Angular-specific properties: `ngVersion`, `providedIn`
- [x] Keep `withSeparateModelsAndApi: true`
- [x] Test generation: `dotnet build ../../TeensyRom.Api.csproj`
- [x] Test generation: `pnpm run generate:api-client`
- [x] Verify output: Classes named `FilesApiService`, `DevicesApiService` (no Angular deps)

**Expected Output Changes:**

```typescript
// Before (typescript-angular)
@Injectable({ providedIn: 'any' })
export class FilesApiService extends BaseService {
  constructor(protected httpClient: HttpClient, ...)
  getDirectory(...): Observable<GetDirectoryResponse>
}

// After (typescript-fetch + post-processing)
export class FilesApiService {
  constructor(configuration?: Configuration, basePath?: string, fetch?: FetchAPI)
  getDirectory(...): Promise<GetDirectoryResponse>
}
```

### Phase 2: Migrate Device Domain

**Files to modify**:

- [x] `libs/domain/device/services/src/lib/device.service.ts`
- [x] `libs/domain/device/services/src/lib/device.service.integration.spec.ts`
- [x] Any other device service test files

#### 2A: Update DeviceService

- [x] Keep same import: `DevicesApiService` (naming preserved by post-processing)
- [x] Update constructor to accept plain class instead of Angular DI
- [x] Convert all methods: `findDevices()`, `connectDevice()`, `disconnectDevice()`, etc.
- [x] Use `from()` to convert Promise→Observable for each method
- [x] Maintain existing method signatures and return types
- [x] Test service builds without errors

**Example conversion:**

```typescript
// Before
constructor(private readonly apiService: DevicesApiService) {}

findDevices(autoConnectNew: boolean): Observable<Device[]> {
  return this.apiService.findDevices(autoConnectNew)
    .pipe(map(response => DeviceMapper.toDeviceList(response.devices)));
}

// After
constructor(private readonly apiService: DevicesApiService) {}

findDevices(autoConnectNew: boolean): Observable<Device[]> {
  return from(this.apiService.findDevices(autoConnectNew))
    .pipe(map(response => DeviceMapper.toDeviceList(response.devices)));
}
```

#### 2B: Update DeviceService Tests

- [x] Update tests to mock plain `DevicesApiService` class
- [x] Remove Angular TestBed requirements for API service mocking
- [x] Update expectations for Promise-based API calls
- [x] Verify all device tests pass

**Example test update:**

```typescript
// Before
beforeEach(() => {
  TestBed.configureTestingModule({
    providers: [{ provide: DevicesApiService, useValue: mockDevicesApi }],
  });
});

// After
const mockDevicesApi = {
  findDevices: vi.fn().mockResolvedValue(mockResponse),
};
const deviceService = new DeviceService(mockDevicesApi);
```

#### 2C: Test Device Domain Integration

- [x] Run device domain tests: `npx nx test domain-device-services`
- [x] Verify no Angular dependencies in device domain
- [x] Test device connection, discovery, and management functionality

### Phase 3: Migrate Storage Domain

**Files to modify**:

- [x] `libs/domain/storage/services/src/lib/storage.service.ts`
- [x] `libs/domain/storage/services/src/lib/storage.service.spec.ts`
- [x] `libs/domain/storage/services/src/lib/storage.service.integration.spec.ts`

#### 3A: Update StorageService

- [x] Keep same import: `FilesApiService` (naming preserved by post-processing)
- [x] Update constructor to accept plain class instead of Angular DI
- [x] Convert all methods from Observable to Promise→Observable using `from()`
- [x] Maintain existing method signatures and return types
- [x] Test service builds without errors

**Example conversion:**

```typescript
// Before
getDirectory(deviceId: string, storageType: StorageType, path?: string): Observable<StorageDirectory> {
  const apiStorageType = StorageMapper.toApiStorageType(storageType);
  return this.apiService.getDirectory(deviceId, apiStorageType, path).pipe(
    map((response: GetDirectoryResponse) => {
      if (!response.storageItem) {
        throw new Error('Invalid response: storageItem is missing');
      }
      return StorageMapper.toStorageDirectory(response.storageItem);
    })
  );
}

// After
getDirectory(deviceId: string, storageType: StorageType, path?: string): Observable<StorageDirectory> {
  const apiStorageType = StorageMapper.toApiStorageType(storageType);
  return from(this.apiService.getDirectory(deviceId, apiStorageType, path)).pipe(
    map((response: GetDirectoryResponse) => {
      if (!response.storageItem) {
        throw new Error('Invalid response: storageItem is missing');
      }
      return StorageMapper.toStorageDirectory(response.storageItem);
    })
  );
}
```

#### 3B: Update StorageService Tests

- [x] Update unit tests to mock plain `FilesApiService` class
- [x] Remove Angular TestBed requirements for API service mocking
- [x] Update expectations for Promise-based API calls
- [x] Verify all storage tests pass

#### 3C: Test Storage Domain Integration

- [x] Run domain storage state tests: `npx nx test domain-storage-state`
- [x] Verify no Angular dependencies pulled into domain layer
- [x] Test that existing Observable contracts work unchanged
- [x] Validate navigation, refresh, and cleanup functionality

### Phase 4: Update Dependency Injection Configuration

**Files to modify**: `apps/teensyrom-ui/src/app/app.config.ts`

- [x] Add factory providers for plain API clients (`FilesApiService`, `DevicesApiService`)
- [x] Configure proper base paths and configuration for API clients
- [x] Update existing service providers to receive plain instances
- [x] Test application builds successfully

**Example DI configuration:**

```typescript
import { Configuration } from '@teensyrom-nx/data-access/api-client';
import { FilesApiService, DevicesApiService } from '@teensyrom-nx/data-access/api-client';

export const appConfig: ApplicationConfig = {
  providers: [
    // Plain API client factories
    {
      provide: 'API_CONFIGURATION',
      useFactory: () =>
        new Configuration({
          basePath: environment.apiUrl,
          // other config
        }),
    },
    {
      provide: FilesApiService,
      useFactory: (config: Configuration) => new FilesApiService(config),
      deps: ['API_CONFIGURATION'],
    },
    {
      provide: DevicesApiService,
      useFactory: (config: Configuration) => new DevicesApiService(config),
      deps: ['API_CONFIGURATION'],
    },

    // Existing wrapper services (no changes)
    StorageService,
    DeviceService,
    // ... other providers
  ],
};
```

### Phase 5: Final Integration Testing

- [x] Build full application: `npx nx build teensyrom-ui`
- [x] Serve application: `npx nx serve teensyrom-ui`
- [x] Test device discovery and connection functionality manually
- [x] Test storage navigation functionality works end-to-end
- [x] Verify API calls function correctly in browser network tab
- [x] Run full test suite: `npx nx test`
- [x] Validate no "JIT compilation failed" errors in domain tests

### Phase 6: Documentation Update

**Files to modify**: `docs/API_CLIENT_GENERATION.md`

- [x] Update documentation to reflect typescript-fetch generator usage
- [x] Document any changes to the generated client structure
- [x] Update notes about dependencies and architecture

## Testing Strategy

### Unit Testing

- **Domain Services**: Mock plain TypeScript API clients (easier than Angular services)
- **Angular Components**: Continue mocking wrapper services
- **Generated Clients**: No direct testing needed (generated code)

### Integration Testing

- **API Calls**: Test that Promise→Observable conversion works correctly
- **Error Handling**: Verify error propagation through the chain
- **Domain Logic**: Ensure domain state management continues to work

### Manual Testing

- **Device Features**: Connection, discovery, reset, ping functionality
- **Storage Features**: Directory navigation, file operations
- **Error Scenarios**: Network failures, invalid responses

## Benefits

1. **Clean Architecture**: Domain logic depends only on abstractions (wrapper services)
2. **Easy Testing**: Domain layer tests run without Angular dependencies
3. **Better Separation**: Clear boundary between domain and infrastructure layers
4. **Maintainability**: Changes to API client isolated to wrapper services
5. **Testability**: Mock implementations trivial to create using plain classes
6. **Preserved Contracts**: Existing Observable interfaces maintained

## Rollback Plan

If issues arise, the migration can be rolled back by:

1. Reverting `generate-client.js` to use `typescript-angular`
2. Regenerating the API client: `pnpm run generate:api-client`
3. Reverting wrapper service changes
4. Running full test suite to ensure functionality restored

## Success Criteria

- [x] Domain layer tests pass without `@angular/core/testing` imports
- [x] No more "JIT compilation failed" errors in domain tests
- [x] All existing functionality continues to work unchanged
- [x] Clean dependency graph with proper separation
- [x] Mock services are easy to create using plain classes
- [x] Generated clients have zero Angular dependencies

## References

- [Current API Client Generation Workflow](../API_CLIENT_GENERATION.md)
- [OpenAPI Generator typescript-fetch Documentation](https://openapi-generator.tech/docs/generators/typescript-fetch/)
- [Original Dependency Inversion Plan](api-dependency-inversion/di-plan.md)
