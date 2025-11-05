# Phase 1: Infrastructure & Contract Layer

## 🎯 Objective

Establish the foundation for search functionality by updating domain contracts, implementing infrastructure service methods, and ensuring proper API client integration with domain model mapping. This phase provides the clean boundary between the API layer and domain layer needed for search operations.

## 📚 Required Reading

- [ ] [Search Plan](./SEARCH_PLAN.md) - Overall feature architecture and multi-phase plan
- [ ] [API Client Generation](../../API_CLIENT_GENERATION.md) - Understanding API client patterns
      patterns
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - TypeScript and Angular patterns
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Unit testing methodology

## File Tree

```
libs/
├── domain/src/lib/
│   └── contracts/
│       └── storage.contract.ts          # UPDATED: Add search method
├── infrastructure/src/lib/
│   ├── storage/
│   │   ├── storage.service.ts           # UPDATED: Implement search()
│   │   └── storage.service.spec.ts      # UPDATED: Add search tests
│   └── domain.mapper.ts                 # REVIEWED: Verify filter mapping
├── data-access/api-client/src/lib/
│   └── apis/
│       └── FilesApiService.ts           # EXISTING: search() already exists
```

## 📋 Implementation Tasks

### Task 1: Update Storage Contract Interface

**Purpose**: Define the domain-level contract for search operations that infrastructure must implement.

**File**: [`libs/domain/src/lib/contracts/storage.contract.ts`](../../../libs/domain/src/lib/contracts/storage.contract.ts)

- [ ] Add `search()` method signature to `IStorageService` interface
  - **Parameters**:
    - `deviceId: string` - Device identifier
    - `storageType: StorageType` - Domain enum (USB, SD)
    - `searchText: string` - Text to search for
    - `filterType?: PlayerFilterType` - Optional domain enum filter
  - **Return Type**: `Observable<FileItem[]>` (files only, no directories)
  - Files only because search results show flat list of matching files
- [ ] Add JSDoc comment documenting search behavior
  - Explain that search returns files only (no directories)
  - Explain that files come from the entire storage hierarchy
  - Mention that searchText searches file names, titles, creators, descriptions
  - Note that filterType filters by file type (All, Games, Music, Images, Hex)
  - Reference the API endpoint behavior from FilesApiService documentation
- [ ] Maintain consistency with existing contract method patterns
  - Observable return type for async operations
  - Domain types only (no API types)
  - Clear parameter naming

**Reference Files**:

- Current IStorageService interface shows patterns for `getDirectory()` and `index()` methods
- FilesApiService.ts shows API-level search method already exists with `SearchRequest` and `SearchResponse` types

---

### Task 2: Implement StorageService.search()

**Purpose**: Implement the infrastructure layer search method that calls the API client and maps responses to domain models.

**File**: [`libs/infrastructure/src/lib/storage/storage.service.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.ts)

- [ ] Add `search()` method implementation following existing service patterns
  - Use same pattern as `getDirectory()` method for consistency
  - Method signature must match `IStorageService` contract
- [ ] Convert domain `StorageType` to API `TeensyStorageType`
  - Use existing `DomainMapper.toApiStorageType()` method
- [ ] Convert domain `PlayerFilterType` to API `NullableOfTeensyFilterType`
  - Use existing `DomainMapper.toApiPlayerFilter()` method (already exists)
  - Handle optional filterType parameter (undefined should pass as undefined to API)
- [ ] Call `FilesApiService.search()` with mapped parameters
  - API method signature: `search({ deviceId, storageType, searchText, filterType })`
  - Use `from()` to convert Promise to Observable (same pattern as getDirectory)
- [ ] Map `SearchResponse.files` array to `FileItem[]` using DomainMapper
  - **Important**: Return ONLY the files array, not the full SearchResponse
  - Each file needs base URL for image URL construction
  - Use `this.baseApiUrl` from service configuration (already extracted in constructor)
  - Map each file: `response.files?.map(file => DomainMapper.toFileItem(file, this.baseApiUrl)) ?? []`
- [ ] Handle errors with proper logging and propagation
  - Use `catchError()` operator to log search failures
  - Use `throwError()` to propagate errors to callers
  - Log message: `'Storage search failed:'` with error details
- [ ] Return `Observable<FileItem[]>` to match contract

**Implementation Notes**:

- FilesApiService already has search() method - no API client regeneration needed
- Base API URL extraction already handled in constructor: `this.baseApiUrl = (this.apiService as any).configuration?.basePath || 'http://localhost:5168'`
- DomainMapper.toFileItem() already handles image URL construction when given base URL
- Follow exact same pattern as getDirectory() for consistency

**Reference Pattern** from existing getDirectory():

```typescript
getDirectory(deviceId: string, storageType: StorageType, path?: string): Observable<StorageDirectory> {
  const apiStorageType = DomainMapper.toApiStorageType(storageType);
  return from(this.apiService.getDirectory({ deviceId, storageType: apiStorageType, path })).pipe(
    map((response: GetDirectoryResponse) => {
      if (!response.storageItem) {
        throw new Error('Invalid response: storageItem is missing');
      }
      return DomainMapper.toStorageDirectory(response.storageItem, this.baseApiUrl);
    }),
    catchError((error) => {
      console.error('Storage directory fetch failed:', error);
      return throwError(() => error);
    })
  );
}
```

---

### Task 3: Verify DomainMapper Filter Support

**Purpose**: Ensure DomainMapper has proper filter type conversion for search operations.

**File**: [`libs/infrastructure/src/lib/domain.mapper.ts`](../../../libs/infrastructure/src/lib/domain.mapper.ts)

- [ ] Verify `toApiPlayerFilter()` method exists and handles all PlayerFilterType values
  - Method signature: `static toApiPlayerFilter(filter: PlayerFilterType): LaunchRandomFilterTypeEnum`
  - Already exists in file - just verify it's complete
  - Mapping: All, Games, Music, Images, Hex → API equivalents
- [ ] Verify `toFileItem()` properly constructs image URLs with base API URL
  - Method already handles this - just verify the pattern
  - Image URLs constructed as: `${baseApiUrl}${baseAssetPath}`
  - All FileItem properties properly mapped from API DTO
- [ ] Review error handling in mapping methods
  - Throw descriptive errors for missing required data
  - Handle optional fields with appropriate defaults

**No Changes Expected**: This task is verification only. Mapper should already support all needed conversions.

---

### Task 4: Infrastructure Layer Testing

**Purpose**: Comprehensive unit tests for search functionality in infrastructure layer.

**File**: [`libs/infrastructure/src/lib/storage/storage.service.spec.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.spec.ts)

- [ ] Create test suite for `search()` method
  - Use `describe('search', () => {})` block
  - Follow existing test patterns from `getDirectory()` tests
- [ ] Test successful search with multiple results
  - Mock `FilesApiService.search()` to return SearchResponse with files array
  - Verify `toApiStorageType()` called with correct parameter
  - Verify `toApiPlayerFilter()` called with correct parameter (if filterType provided)
  - Verify API service called with mapped parameters
  - Verify each file mapped with `toFileItem()` including base URL
  - Assert returned array contains correct number of FileItems
  - Assert FileItem properties properly mapped
- [ ] Test successful search with empty results
  - Mock API to return SearchResponse with empty files array
  - Verify service returns empty FileItem array
  - Verify no errors thrown
- [ ] Test search without filter parameter
  - Pass undefined for filterType
  - Verify API called with undefined filterType
  - Verify search still works correctly
- [ ] Test search with each filter type
  - Test with PlayerFilterType.All
  - Test with PlayerFilterType.Games
  - Test with PlayerFilterType.Music
  - Test with PlayerFilterType.Images
  - Test with PlayerFilterType.Hex
  - Verify correct API filter enum passed in each case
- [ ] Test error scenarios
  - Mock API to throw network error - verify error logged and propagated
  - Mock API to throw HTTP error - verify error logged and propagated
  - Mock API to return null/undefined files array - verify empty array returned
- [ ] Test domain model mapping correctness
  - Verify FileItem.name, path, size mapped correctly
  - Verify FileItem metadata (title, creator, description) mapped
  - Verify FileItem.images array mapped with correct URLs
  - Verify FileItem.type mapped correctly
  - Verify FileItem.parentPath preserved for directory context
- [ ] Test base URL extraction and image URL construction
  - Verify base URL passed to mapper for each file
  - Mock file with images and verify URLs constructed as `${baseUrl}${assetPath}`
  - Test with multiple images per file

**Testing Notes**:

- Use Vitest mocking patterns: `vi.fn()`, `mockReturnValue()`, `mockRejectedValue()`
- Use TestBed for service instantiation with mocked dependencies
- Mock FilesApiService with typed mocks
- Use `firstValueFrom()` to convert observables to promises in async tests
- Follow TESTING_STANDARDS.md patterns for test structure

**Mock Data Examples Needed**:

- `createMockSearchResponse()` - Factory for SearchResponse with FileItemDto array
- `createMockFileItemDto()` - Factory for individual file DTOs
- `createMockFileItem()` - Factory for expected domain FileItem

---

## 🗂️ File Changes

### Modified Files

- [`libs/domain/src/lib/contracts/storage.contract.ts`](../../../libs/domain/src/lib/contracts/storage.contract.ts) - Add search method signature
- [`libs/infrastructure/src/lib/storage/storage.service.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.ts) - Implement search method
- [`libs/infrastructure/src/lib/storage/storage.service.spec.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.spec.ts) - Add search tests

### Reviewed Files (No Changes)

- [`libs/infrastructure/src/lib/domain.mapper.ts`](../../../libs/infrastructure/src/lib/domain.mapper.ts) - Verify existing filter mapping
- [`libs/data-access/api-client/src/lib/apis/FilesApiService.ts`](../../../libs/data-access/api-client/src/lib/apis/FilesApiService.ts) - API method already exists

## 🧪 Testing Requirements

### Unit Tests

**StorageService.search()**:

- [ ] Successful search returns mapped FileItem array
- [ ] Empty search results return empty array
- [ ] Search without filter parameter works correctly
- [ ] Each PlayerFilterType correctly mapped to API enum
- [ ] Network errors properly logged and propagated
- [ ] HTTP errors properly logged and propagated
- [ ] Null/undefined API responses handled gracefully
- [ ] FileItem properties correctly mapped from API DTOs
- [ ] Image URLs constructed with base API URL
- [ ] Multiple images per file handled correctly
- [ ] ParentPath preserved for directory context
- [ ] Domain type conversions (StorageType, PlayerFilterType) verified

**Test Coverage Requirements**:

- Minimum 80% line coverage for new search method
- 100% coverage for error handling paths
- All filter type variations tested
- Edge cases covered (empty results, null values, missing data)

## ✅ Success Criteria

- [ ] IStorageService contract includes search method signature with proper typing
- [ ] StorageService.search() implemented and returns Observable<FileItem[]>
- [ ] Domain types properly converted to API types (StorageType, PlayerFilterType)
- [ ] API client search method called with correct parameters
- [ ] SearchResponse.files array mapped to FileItem[] using DomainMapper
- [ ] Base API URL passed to mapper for image URL construction
- [ ] Error handling implemented with logging and propagation
- [ ] All infrastructure unit tests passing
- [ ] Test coverage meets requirements (80% minimum, 100% error paths)
- [ ] Code follows TypeScript and Angular standards
- [ ] JSDoc documentation complete for public API
- [ ] No breaking changes to existing storage functionality

## 📝 Notes

### Key Architectural Considerations

- **Files Only Response**: Search returns `FileItem[]` not `SearchResponse` - simplifies domain boundary by returning only what's needed
- **Image URL Construction**: Base API URL must be passed to mapper for proper absolute URLs in FileItem.images
- **Optional Filter**: filterType is optional - undefined should be passed through to API (which treats it as "All")
- **ParentPath Preservation**: FileItem.parentPath provides directory context for each file, eliminating need to track separately
- **No API Regeneration**: FilesApiService.search() already exists - just need to integrate it

### Design Decisions

- **Observable Return Type**: Maintains consistency with other storage service methods (getDirectory, index)
- **Domain Type Conversions**: All conversions happen in infrastructure layer - domain/application layers never see API types
- **Error Propagation**: Errors logged but propagated to callers for proper handling at application layer
- **Existing Patterns**: Follows exact same structure as getDirectory() for team consistency

### Dependencies

- **Upstream**: Requires API endpoint to be functional (already exists)
- **Downstream**: Phase 2 will consume this service method in StorageStore actions
- **No Breaking Changes**: This is purely additive - no impact on existing functionality

### Testing Strategy

- **Unit Tests Only**: Infrastructure layer tested in isolation with mocked API client
- **Mock Data Factories**: Create reusable factories for test data generation
- **Edge Case Coverage**: Null/undefined handling, empty results, error scenarios
- **Type Safety**: Use strongly typed mocks - no `any` types in tests

### Future Considerations

- **Caching**: Phase 2 will handle search result caching in StorageStore
- **Debouncing**: Phase 4 will handle input debouncing in UI components
- **Performance**: Large result sets will be handled by virtual scrolling in Phase 3
