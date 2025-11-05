# Phase 1 Execution Plan: Backend - Save Favorite Endpoint & Tests

**Phase**: 1 of 6  
**Status**: Ready for Implementation  
**Estimated Effort**: 4-6 hours  
**Last Updated**: 2025-10-22

---

## 📋 Overview

This execution plan details the implementation steps for Phase 1 of the Favorites feature, which establishes the backend API endpoint for saving files as favorites. The phase includes creating the endpoint, extracting storage service logic, and implementing comprehensive integration tests.

**Prerequisites**: None - This is the foundational phase.

**Dependent Phases**: Phase 2 (Remove Favorite), Phase 3 (Frontend Infrastructure)

---

## 🎯 Phase 1 Objectives

1. Create a new `FavoriteFileEndpoint` that accepts device ID, storage type, and file path
2. Extract and adapt `SaveFavorite` logic from `CachedStorageService` to `StorageService`
3. Implement comprehensive integration tests covering success and error scenarios
4. Ensure proper validation, error handling, and response formatting
5. Maintain consistency with existing endpoint patterns (LaunchFileEndpoint)

---

## 📁 File Structure

### Files to Create

```
TeensyRom.Api/
└── Endpoints/
    └── Files/
        └── FavoriteFile/
            ├── FavoriteFileEndpoint.cs          (NEW)
            └── FavoriteFileModels.cs            (NEW)

TeensyRom.Core.Storage/
└── StorageService.cs                            (MODIFY - add SaveFavorite method)

TeensyRom.Api.Tests.Integration/
└── SaveFavoriteTests.cs                         (NEW)
```

### Reference Files

**Endpoint Patterns**:

- [`LaunchFileEndpoint.cs`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileEndpoint.cs) - Endpoint structure, device validation, error handling
- [`LaunchFileModels.cs`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileModels.cs) - Request/response models, FluentValidation patterns

**Storage Logic**:

- [`CachedStorageService.cs`](../../../../../TeensyRom.Core.Storage/CachedStorageService.cs) - Lines 57-104 contain SaveFavorite logic to extract
- [`StorageService.cs`](../../../../../TeensyRom.Core.Storage/StorageService.cs) - Target file for new SaveFavorite method
- [`StorageHelper.cs`](../../../../../TeensyRom.Core/Entities/Storage/StorageHelper.cs) - GetFavoritePath() method, favorite directory constants

**Testing Patterns**:

- [`LaunchFileTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/LaunchFileTests.cs) - Test structure, assertion patterns, fixture usage
- [`EndpointFixture.cs`](../../../../../TeensyRom.Api.Tests.Integration/Common/EndpointFixture.cs) - Helper methods: GetConnectedDevice(), Preindex()

---

## 🔨 Implementation Tasks

### Task 1: Create FavoriteFileModels.cs

**Location**: [`TeensyRom.Api/Endpoints/Files/FavoriteFile/FavoriteFileModels.cs`](../../../../../TeensyRom.Api/Endpoints/Files/FavoriteFile/)

**Reference**: [`LaunchFileModels.cs`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileModels.cs)

**Components to Create**:

1. **SaveFavoriteRequest**

   - Properties: `DeviceId` (route), `StorageType` (route), `FilePath` (query)
   - Mirror [`LaunchFileRequest`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileModels.cs) structure exactly
   - Use `[FromRoute]` and `[FromQuery]` attributes

2. **SaveFavoriteRequestValidator**

   - Copy [`LaunchFileRequestValidator`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileModels.cs) pattern
   - Validate: DeviceId format, FilePath format, StorageType enum

3. **SaveFavoriteResponse**
   - Properties: `Message`, `OriginalFile`, `FavoriteFile`, `FavoritePath`
   - All marked `[Required]`
   - Use `FileItemDto` type for file properties

**Key Decisions**:

- Return both original and favorite file for UI state management
- Include FavoritePath to show user where favorite was saved

---

### Task 2: Create FavoriteFileEndpoint.cs

**Location**: [`TeensyRom.Api/Endpoints/Files/FavoriteFile/FavoriteFileEndpoint.cs`](../../../../../TeensyRom.Api/Endpoints/Files/FavoriteFile/)

**Reference**: [`LaunchFileEndpoint.cs`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileEndpoint.cs) - Mirror this structure exactly

**Endpoint Configuration**:

- Route: `POST /devices/{deviceId}/storage/{storageType}/favorite`
- Name: "SaveFavorite"
- Tags: "Files"
- Status codes: 200 OK, 400 Bad Request, 404 Not Found, 502 Bad Gateway

**Handle Method Flow** (copy [`LaunchFileEndpoint`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileEndpoint.cs) structure):

1. Get device from `deviceManager.GetConnectedDevice(r.DeviceId)` → SendNotFound if null
2. Get storage service (check SD/USB availability) → SendNotFound if unavailable
3. Get file with `storage.GetFile(new FilePath(r.FilePath))` → SendNotFound if null
4. Cast to `LaunchableItem` → SendValidationError if not castable
5. Call `storage.SaveFavorite(launchItem)` → SendExternalError if null
6. Build response with `FileItemDto.FromLaunchable()` → Send success

**Error Handling**:

- Device not found → 404
- Storage unavailable → 404
- File not found → 404
- File not launchable → 400
- Save failed → 502

---

### Task 3: Add SaveFavorite Method to StorageService

**Location**: [`StorageService.cs`](../../../../../TeensyRom.Core.Storage/StorageService.cs) - Add after `MapFile` method (line ~217)

**Source**: [`CachedStorageService.cs`](../../../../../TeensyRom.Core.Storage/CachedStorageService.cs) lines 57-104 - Copy this method

**Method Signature**:

```csharp
public async Task<LaunchableItem?> SaveFavorite(LaunchableItem launchItem)
```

**Implementation Steps**:

1. Get favorite path from [`StorageHelper.GetFavoritePath()`](../../../../../TeensyRom.Core/Entities/Storage/StorageHelper.cs)
2. Create `FavoriteFileCommand` with source and target paths
3. Send command via `mediator.Send()`
4. Mark original file as favorite in cache
5. Clone file, update path, add to cache as favorite
6. Update parent file (bidirectional link)
7. Update siblings (all copies)
8. Write cache to disk
9. Return favorite copy

**Variable Adaptations** (from [`CachedStorageService`](../../../../../TeensyRom.Core.Storage/CachedStorageService.cs)):

- `_settings.StorageType` → `settings.CartStorage.Type`
- `_mediator` → `mediator`
- `_alert` → `alert`
- `_log` → `log`
- `_storageCache` → `cache`

**Dependencies**:

- Existing `FavoriteFileCommand` from TeensyRom.Core.Serial
- `IStorageCache` methods: `UpsertFile()`, `FindParentFile()`, `FindSiblings()`, `WriteToDisk()`

---

### Task 4: Create SaveFavoriteTests.cs

**Location**: [`TeensyRom.Api.Tests.Integration/SaveFavoriteTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/)

**Reference**: [`LaunchFileTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/LaunchFileTests.cs) - Copy test structure and patterns

**Test Class Setup**:

- Attribute: `[Collection("Endpoint")]`
- Constructor: `SaveFavoriteTests(EndpointFixture f)`
- Implement: `IDisposable` with `f.Reset()` in Dispose
- Constants: TestMusicFile, TestGameFile, TestImageFile, NonExistentFile paths

**10 Test Methods to Implement**:

**Success Scenarios** (4 tests):

1. **When_SavingMusicFileAsFavorite_ReturnsSuccessWithFavoriteInfo**

   - Use [`f.GetConnectedDevice()`](../../../../../TeensyRom.Api.Tests.Integration/Common/EndpointFixture.cs) and [`f.Preindex()`](../../../../../TeensyRom.Api.Tests.Integration/Common/EndpointFixture.cs)
   - Assert 200 OK with SaveFavoriteResponse
   - Verify OriginalFile.Path matches request
   - Verify FavoritePath contains "/favorites/music"

2. **When_SavingGameFileAsFavorite_ReturnsSuccessWithCorrectPath**

   - Verify FavoritePath contains "/favorites/games"

3. **When_SavingImageFileAsFavorite_ReturnsSuccessWithCorrectPath**

   - Verify FavoritePath contains "/favorites/images"

4. **When_SavingMultipleFavorites_InSequence_AllSucceed**
   - Test music, game, image files sequentially
   - Add brief delays between operations

**Error Scenarios** (6 tests): 5. **When_SavingFavorite_WithNonExistentFile_ReturnsNotFound** → 404 6. **When_SavingFavorite_WithInvalidDeviceId_ReturnsValidationError** → 400 7. **When_SavingFavorite_WithDeviceThatDoesntExist_ReturnsNotFound** → 404 8. **When_SavingFavorite_WithUnavailableSDStorage_ReturnsNotFound** → 404 (skip if SD available) 9. **When_SavingFavorite_WithInvalidStorageType_ReturnsBadRequest** → 400 10. **When_SavingFavorite_WithEmptyFilePath_ReturnsBadRequest** → 400

**Test Patterns** (from [`LaunchFileTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/LaunchFileTests.cs)):

- Use [`EndpointFixture`](../../../../../TeensyRom.Api.Tests.Integration/Common/EndpointFixture.cs) helper methods
- FluentAssertions: `.Should().BeSuccessful<T>()`, `.Should().BeProblem()`, `.Should().BeValidationProblem()`
- Check `.WithStatusCode()`, `.WithContentNotNull()`
- Verify response properties match expectations

---

## 🔍 Validation & Testing Strategy

### Pre-Implementation Validation

Before starting implementation, verify:

1. ✅ All reference files exist and are accessible
2. ✅ `FavoriteFileCommand` exists in [`TeensyRom.Core.Serial/Commands/FavoriteFile/`](../../../../../TeensyRom.Core.Serial/Commands/FavoriteFile/)
3. ✅ `IStorageCache` interface has required methods
4. ✅ [`LaunchFileEndpoint`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileEndpoint.cs) builds successfully
5. ✅ Integration test fixture works with existing tests

### Development Testing Workflow

1. **Task 1 Completion** (Models):

   - Build the project, verify no compilation errors
   - Validators should compile without issues

2. **Task 2 Completion** (Endpoint):

   - Build the project
   - Endpoint should register in Swagger/Scalar docs at `/scalar/v1`
   - Verify endpoint appears at `/devices/{deviceId}/storage/{storageType}/favorite`
   - Test with Swagger UI (expect 404 initially until method implemented)

3. **Task 3 Completion** (Storage Service):

   - Build the project
   - Run existing storage service tests (should still pass)
   - Manually test endpoint with Swagger (should work end-to-end)

4. **Task 4 Completion** (Integration Tests):
   - Run individual test methods one at a time
   - Verify each test passes independently
   - Run full test suite: `dotnet test --filter "SaveFavoriteTests"`
   - Ensure all 10 tests pass

### Final Validation

```bash
# From: Source/Windows/TeensyRom.Ui/

# Build entire solution
dotnet build

# Run only SaveFavorite integration tests
dotnet test --filter "SaveFavoriteTests"

# Verify endpoint registration
dotnet run --project src/TeensyRom.Api
# Navigate to: http://localhost:5000/scalar/v1
# Confirm "SaveFavorite" endpoint appears in Files section
```

**Expected Results**:

- ✅ Solution builds with zero errors
- ✅ All 10 SaveFavoriteTests pass
- ✅ Endpoint accessible via Scalar docs
- ✅ Existing tests still pass (no regressions)

---

## 🚨 Known Issues & Considerations

### Technical Considerations

1. **Duplicate Favorite Handling**:

   - Current implementation doesn't check if file is already favorited
   - Will create duplicate if called twice on same file
   - **Resolution**: Open question for Phase 1 - document behavior, address in Phase 2

2. **Cache Consistency**:

   - SaveFavorite updates cache immediately
   - Parent/sibling relationships maintained via [`IStorageCache.FindParentFile()`](../../../../../TeensyRom.Core.Storage/IStorageCache.cs) and [`FindSiblings()`](../../../../../TeensyRom.Core.Storage/IStorageCache.cs)
   - Cache persisted to disk after operation
   - **Action**: No changes needed, existing logic is sound

3. **Transaction Safety**:
   - No rollback if cache update fails after FavoriteFileCommand succeeds
   - Physical file created but cache might not reflect it
   - **Resolution**: Acceptable for Phase 1, monitor for issues

### Open Questions (Phase 1 Specific)

**Q1: Should the endpoint return the newly created favorite file information in the response?**

- **Answer**: YES - Response includes both OriginalFile and FavoriteFile
- **Rationale**: Frontend needs both for UI updates and state management

**Q2: How should the endpoint handle attempting to favorite an already favorited file?**

- **Answer**: DEFER TO PHASE 2
- **Current Behavior**: Will create duplicate
- **Rationale**: Phase 1 focuses on happy path, Phase 2 adds remove/duplicate detection

**Q3: Should there be rate limiting or validation on the number of favorites a user can create?**

- **Answer**: NO for Phase 1
- **Rationale**: Not a requirement, can add later if needed
- **Storage Impact**: Minimal - favorites are small file references

---

## 📝 Implementation Checklist

### Pre-Implementation

- [ ] Read and understand [FAVORITE_PLAN.md](./FAVORITE_PLAN.md) Phase 1 section
- [ ] Review [`LaunchFileEndpoint.cs`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileEndpoint.cs) implementation
- [ ] Review [`CachedStorageService.SaveFavorite`](../../../../../TeensyRom.Core.Storage/CachedStorageService.cs) method (lines 57-104)
- [ ] Review [`LaunchFileTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/LaunchFileTests.cs) test patterns
- [ ] Verify development environment has .NET 9 SDK
- [ ] Ensure TeensyROM device available for integration testing

### Task 1: Models (Est. 30 minutes)

- [ ] Create `TeensyRom.Api/Endpoints/Files/FavoriteFile/` directory
- [ ] Create `FavoriteFileModels.cs`
- [ ] Implement `SaveFavoriteRequest` class (reference [`LaunchFileRequest`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileModels.cs))
- [ ] Implement `SaveFavoriteRequestValidator` class (reference [`LaunchFileRequestValidator`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileModels.cs))
- [ ] Implement `SaveFavoriteResponse` class
- [ ] Add necessary using statements
- [ ] Build project - verify no compilation errors

### Task 2: Endpoint (Est. 1-2 hours)

- [ ] Create `FavoriteFileEndpoint.cs` in same directory
- [ ] Implement endpoint configuration (Configure method) - reference [`LaunchFileEndpoint.Configure()`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileEndpoint.cs)
- [ ] Implement Handle method with device validation
- [ ] Add storage service retrieval logic
- [ ] Add file retrieval and validation
- [ ] Add SaveFavorite method call (will be implemented in Task 3)
- [ ] Add response building logic
- [ ] Add error handling for all scenarios
- [ ] Build project - verify no compilation errors
- [ ] Test endpoint registration in Swagger/Scalar docs

### Task 3: Storage Service (Est. 1-2 hours)

- [ ] Open [`TeensyRom.Core.Storage/StorageService.cs`](../../../../../TeensyRom.Core.Storage/StorageService.cs)
- [ ] Add necessary using statements
- [ ] Copy SaveFavorite method from [`CachedStorageService`](../../../../../TeensyRom.Core.Storage/CachedStorageService.cs) lines 57-104
- [ ] Adapt method to use StorageService dependencies (see variable adaptations in Task 3)
- [ ] Update variable names to match StorageService conventions
- [ ] Verify all cache methods are available (UpsertFile, FindParentFile, etc.)
- [ ] Build project - verify no compilation errors
- [ ] Manual test with Swagger/Postman - verify end-to-end functionality

### Task 4: Integration Tests (Est. 2 hours)

- [ ] Create `SaveFavoriteTests.cs` in [`TeensyRom.Api.Tests.Integration`](../../../../../TeensyRom.Api.Tests.Integration/)
- [ ] Add test class with constructor and IDisposable (reference [`LaunchFileTests`](../../../../../TeensyRom.Api.Tests.Integration/LaunchFileTests.cs))
- [ ] Define test file path constants
- [ ] Implement all 10 test methods (see Task 4 for list)
- [ ] Build project - verify no compilation errors
- [ ] Run each test individually - verify passes
- [ ] Run full test suite - verify all pass

### Post-Implementation Validation

- [ ] Run full solution build - zero errors
- [ ] Run `dotnet test --filter "SaveFavoriteTests"` - all pass
- [ ] Run existing integration tests - no regressions
- [ ] Test endpoint via Scalar docs - manual verification
- [ ] Test with real TeensyROM device - save favorite works
- [ ] Verify favorite file appears in appropriate `/favorites/*` directory
- [ ] Verify original file IsFavorite status updated
- [ ] Code review - adherence to [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- [ ] Update this document with any issues encountered

---

## 🔗 Related Documentation

- **Main Plan**: [FAVORITE_PLAN.md](./FAVORITE_PLAN.md) - Complete feature overview
- **Phase 2**: Phase 2 execution plan (to be created)
- **Coding Standards**: [../../CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [../../TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **API Client Generation**: [../../API_CLIENT_GENERATION.md](../../API_CLIENT_GENERATION.md)

---

## 📊 Success Criteria

Phase 1 is complete when:

1. ✅ `FavoriteFileEndpoint` successfully handles save favorite requests
2. ✅ `StorageService.SaveFavorite` creates favorite copies with bidirectional links
3. ✅ All 10 integration tests pass consistently
4. ✅ Endpoint properly validates all inputs (device ID, storage type, file path)
5. ✅ Endpoint returns detailed response with original and favorite file info
6. ✅ Error scenarios return appropriate HTTP status codes
7. ✅ Favorite files appear in correct `/favorites/*` directory (see [`StorageHelper`](../../../../../TeensyRom.Core/Entities/Storage/StorageHelper.cs))
8. ✅ No regressions in existing tests
9. ✅ Code follows project [coding standards](../../CODING_STANDARDS.md)
10. ✅ Endpoint documented in Scalar API docs

**Acceptance Test**: Using Scalar docs, save a music file as favorite. Verify:

- Returns 200 OK with SaveFavoriteResponse
- Original file path matches request
- Favorite file path is in `/favorites/music/`
- Message confirms success
- File appears in TeensyROM device favorites directory

---

## 🎉 Next Steps

After Phase 1 completion:

1. **Code Review**: Submit PR for team review
2. **Phase 2 Planning**: Create FAVORITE_PLAN_P2.md for Remove Favorite endpoint
3. **Documentation**: Update [FAVORITE_PLAN.md](./FAVORITE_PLAN.md) with any lessons learned
4. **Frontend Preparation**: Share API contract with frontend team for Phase 3

---

**Document Version**: 1.0  
**Created**: 2025-10-22  
**Author**: AI Assistant  
**Status**: Ready for Implementation
