# Phase 1: HTTP Client & Domain Models Specification

## Objective

Create HTTP client integration with clean domain model transformation layer for TeensyROM storage navigation.

## Prerequisites

- Backend API running on `http://localhost:5168`
- OpenAPI endpoint accessible at `/openapi/v1.json`
- Node.js and npm installed for client generation

## Implementation Steps

### Step 1: API Client Regeneration

**Purpose**: Ensure API client includes latest `GetDirectoryEndpoint` and related models

**Commands**:

```bash
cd ClientApp/teensyrom-nx
npm run generate:api-client
```

**Process Overview**:

- Fetches OpenAPI spec from `http://localhost:5168/openapi/v1.json`
- Generates TypeScript Angular client using `openapi-generator-cli`
- Post-processes files: renames `*Service` → `*ApiService`
- Updates barrel exports in `api.ts`

**Verification**:

- Confirm `FilesApiService.getDirectory()` method exists
- Verify `GetDirectoryResponse`, `StorageCacheDto`, `DirectoryItemDto`, `FileItemDto` models exist
- Check compilation: `npx nx build data-access/api-client`

### Step 2: Domain Models Creation

**Purpose**: Create clean domain interfaces separate from API DTOs

**Deliverables**:

- `StorageDirectory` interface for directory contents
- `DirectoryItem` interface for subdirectories
- `FileItem` interface for files with metadata
- `FileItemType` enum for file classification

**File**: `libs/domain/storage/services/src/lib/storage.models.ts`

**Key Requirements**:

- Follow existing naming conventions (see `device.models.ts`)
- Include all relevant metadata fields from DTOs
- Use appropriate TypeScript types (string, number, boolean, etc.)

### Step 3: Data Transformation Mapper

**Purpose**: Transform API DTOs to clean domain models

**Deliverables**:

- `StorageMapper` class with static transformation methods
- `toStorageDirectory()` - main transformation method
- `toDirectoryItem()` - directory transformation
- `toFileItem()` - file transformation

**File**: `libs/domain/storage/services/src/lib/storage.mapper.ts`

**Key Requirements**:

- Follow existing `DeviceMapper` patterns
- Handle all DTO properties appropriately
- Provide type-safe transformations
- Include error handling for malformed data

### Step 4: Storage Service Implementation

**Purpose**: HTTP client wrapper with domain model transformation

**Deliverables**:

- `StorageService` class with dependency injection
- `getDirectory()` method returning `Observable<StorageDirectory>`
- Integration with `FilesApiService`
- Proper error handling and transformation

**File**: `libs/domain/storage/services/src/lib/storage.service.ts`

**Key Requirements**:

- Use `providedIn: 'root'` for dependency injection
- Follow existing `DeviceService` patterns
- Transform API responses to domain models
- Return clean Observable interfaces

### Step 5: Barrel Exports

**Purpose**: Create clean public API for the library

**File**: `libs/domain/storage/services/src/index.ts`

**Requirements**:

- Export all public interfaces and classes
- Follow existing domain library patterns
- Enable clean imports: `import { StorageService } from '@teensyrom-nx/domain/storage/services'`

## Testing Requirements

### Unit Tests

**Files to Create**:

- `storage.service.spec.ts` - Unit tests with mocked dependencies
- `storage.mapper.spec.ts` - Transformation logic tests

**Test Categories**:

- Service method functionality with mocked `FilesApiService`
- Mapper transformation accuracy
- Error handling scenarios
- Input validation

**Framework**: Vitest (following existing patterns)

### Integration Tests

**Files to Create**:

- `storage.service.integration.spec.ts` - Live API integration tests

**Test Categories**:

- End-to-end API communication
- Real backend integration (requires running API)
- Error scenarios (invalid device IDs, unavailable storage)
- Data transformation accuracy with real API responses

**Pattern**: Follow `device.service.integration.spec.ts` structure

## Success Criteria

### Functional Requirements

- [ ] `StorageService.getDirectory()` returns `Observable<StorageDirectory>`
- [ ] Domain models cleanly represent API data without DTO coupling
- [ ] All TypeScript compilation passes without errors
- [ ] Service integrates properly with existing dependency injection

### Code Quality Requirements

- [ ] Follows established naming conventions and file organization
- [ ] Includes comprehensive unit and integration tests
- [ ] Proper error handling for API failures and malformed data
- [ ] Clean separation between API layer and domain models

### Integration Requirements

- [ ] Service can be imported and injected in other libraries
- [ ] Compatible with existing `DeviceStore` patterns for Phase 2
- [ ] API client regeneration process works without manual intervention
- [ ] Ready for Phase 2 state management integration

## File Structure

```
libs/domain/storage/services/src/lib/
├── storage.models.ts          # Domain interfaces and types
├── storage.mapper.ts          # DTO → domain transformation
├── storage.service.ts         # HTTP client wrapper
├── storage.service.spec.ts    # Unit tests
├── storage.mapper.spec.ts     # Mapper unit tests
└── storage.service.integration.spec.ts  # Integration tests

libs/domain/storage/services/src/
└── index.ts                   # Barrel exports
```

## Dependencies

- `@teensyrom-nx/data-access/api-client` - Generated API client
- `@angular/core` - Dependency injection
- `rxjs` - Observable patterns
- `vitest` - Testing framework

## Notes

- This phase creates foundation for Phase 2 state management
- Domain models will be reused across all storage-related features
- Service follows established patterns for consistency
- Integration tests require backend API to be running
