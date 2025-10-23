# Favorite Files Feature - Phase 2 Plan

**Project Overview**: Complete the backend CRUD operations for the favorites system by implementing the remove favorite endpoint, storage service method, and comprehensive integration tests. This phase completes backend functionality, enabling the frontend to be built in subsequent phases.

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **State Standards**: [STATE_STANDARDS.md](../../STATE_STANDARDS.md)
- **API Client Generation**: [API_CLIENT_GENERATION.md](../../API_CLIENT_GENERATION.md)

---

## 🎯 Phase 2 Objective

Implement the remove favorite endpoint that allows users to remove files from their favorites collection. This endpoint will handle both removing the favorite copy and updating the favorite status of the original file, maintaining bidirectional relationship consistency.

**User Value**: Users can manage their favorite collection by unfavoriting files they no longer want in their curated list, keeping their favorites collection current and relevant.

**System Benefits**: Completes the backend CRUD operations for favorites, establishing full read/write capability for favorite management that frontend features will depend on. Maintains data consistency by updating both favorite and original files.

---

## 📋 Implementation Plan

<details open>
<summary><h3>Phase 2: Backend - Remove Favorite Endpoint & Tests</h3></summary>

### Objective

Create the backend API endpoint for removing files from favorites, along with comprehensive integration tests. This completes the backend CRUD operations for favorites management.

### Key Deliverables

- [ ] `RemoveFavoriteEndpoint.cs` created under [`Endpoints/Files/FavoriteFile/`](../../../../../TeensyRom.Api/Endpoints/) directory
- [ ] `RemoveFavorite` method added to [`StorageService.cs`](../../../../../TeensyRom.Core.Storage/StorageService.cs)
- [ ] Integration tests created in [`RemoveFavoriteTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/)
- [ ] Endpoint accepts filename/path, device ID, and storage type parameters
- [ ] Method removes favorite copy and updates original file's favorite status
- [ ] All tests passing for success and error scenarios

### High-Level Tasks

1. **Create RemoveFavoriteEndpoint**: Implement endpoint following the same pattern as `FavoriteFileEndpoint`
   - Accept device ID, storage type, and file path as route parameters
   - Validate device exists and requested storage is available
   - Retrieve the file using storage service (can be from original or favorite location)
   - Call storage service `RemoveFavorite()` method
   - Return success response with confirmation message
   - Return appropriate error responses for validation/not found scenarios

2. **Implement RemoveFavorite Storage Method**: Extract and adapt logic from `CachedStorageService`
   - Locate the corresponding favorite file copy in the appropriate favorites directory
   - Delete the favorite file copy
   - Update the original file's favorite status to false
   - Handle case where favorite file doesn't exist (idempotent operation)
   - Maintain parent path reference consistency
   - Return confirmation or null on failure

3. **Create Request/Response Models**: Add to `FavoriteFileModels.cs`
   - `RemoveFavoriteRequest` class with DeviceId, StorageType, and FilePath properties
   - `RemoveFavoriteRequest` validator following existing patterns
   - `RemoveFavoriteResponse` class with Message and confirmation details
   - Apply validation using FluentValidation

4. **Write Integration Tests**: Create comprehensive tests in `RemoveFavoriteTests.cs`
   - Test successful removal for each file type (games, music, images)
   - Test removal updates original file's favorite status correctly
   - Test removal idempotency (removing non-existent favorite succeeds)
   - Test error scenarios (device not found, file not found, storage unavailable)
   - Test validation failures (invalid device ID, invalid storage type, invalid path)
   - Test that removal removes the physical favorite file from disk
   - Test that favorite status is cleared on original file

### HTTP Method

- **Method**: DELETE
- **Route**: `/devices/{deviceId}/storage/{storageType}/favorite`
- **Parameters**: DeviceId (route), StorageType (route), FilePath (query)
- **Response**: RemoveFavoriteResponse (200 OK) or ProblemDetails (400/404/502)

### Open Questions (Phase 2)

- Should the response include the updated file object after removal?
- Should removing from favorite directory cascade to remove the original file's favorite status?
- Should there be a bulk "remove all favorites" endpoint?
- Should orphaned favorites (original file deleted) be handled automatically?

### Testing Checklist

- [ ] Happy path: Remove existing favorite file
- [ ] Happy path: Remove favorite returns success response
- [ ] Idempotency: Removing non-existent favorite succeeds
- [ ] Idempotency: Multiple removal attempts produce same result
- [ ] Error: Device not found returns 404
- [ ] Error: File not found returns 404
- [ ] Error: Storage unavailable returns 404
- [ ] Validation: Invalid device ID returns 400
- [ ] Validation: Invalid storage type returns 400
- [ ] Validation: Invalid file path returns 400
- [ ] Validation: Empty file path returns 400
- [ ] State: Original file's favorite status updated to false
- [ ] State: Favorite file physically removed from disk

</details>

---

## 🏗️ Pattern Reference

Follow these existing patterns from Phase 1 implementation:

### Endpoint Pattern
- [`FavoriteFileEndpoint.cs`](../../../../../TeensyRom.Api/Endpoints/Files/FavoriteFile/FavoriteFileEndpoint.cs) - Route configuration, validation flow, response handling
- [`FavoriteFileModels.cs`](../../../../../TeensyRom.Api/Endpoints/Files/FavoriteFile/FavoriteFileModels.cs) - Request/response/validator model structure

### Storage Service Pattern
- [`StorageService.SaveFavorite()`](../../../../../TeensyRom.Core.Storage/StorageService.cs) - Core business logic structure, path handling

### Integration Test Pattern
- [`SaveFavoriteTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/SaveFavoriteTests.cs) - Test organization, fixture usage, assertion patterns

### Comparative Reference
- [`LaunchFileEndpoint.cs`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileEndpoint.cs) - Similar DELETE endpoint structure
- [`LaunchFileTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/LaunchFileTests.cs) - Similar test patterns

---

## 📊 Success Criteria

- ✅ RemoveFavoriteEndpoint created and routes correctly
- ✅ RemoveFavorite storage method works for all file types
- ✅ All integration tests pass (happy path, error scenarios, validation)
- ✅ Favorite files are physically deleted from disk
- ✅ Original files' favorite status is updated to false
- ✅ Operation is idempotent (safe to call multiple times)
- ✅ Error messages are clear and actionable
- ✅ Code follows existing patterns and standards

---

## 🔄 Phase Progression

**Phase 1 Status**: ✅ Complete
- Save Favorite endpoint implemented and tested

**Phase 2 Status**: 🔄 In Progress
- Remove Favorite endpoint implementation

**Phase 3 Next**: Frontend API Client & Infrastructure Layer
- Regenerate TypeScript client for new endpoint
- Create storage service methods
- Add domain mapper transformations

**Phase 4 Next**: Frontend Store Actions
- Add store actions for favorite operations
- State management integration

**Phase 5 Next**: Frontend UI Integration
- Add favorite button to player toolbar
- Visual state management (heart icons)

**Phase 6 Next**: E2E Tests
- Complete workflow validation
- User interaction testing
