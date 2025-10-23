# Phase 3: Frontend - API Client Generation & Infrastructure Layer

**Implementation Guide for Favorites Feature**

This document provides detailed implementation instructions for Phase 3 of the Favorites feature. It is designed to be followed by a Haiku agent or junior developer to independently complete the infrastructure layer integration.

---

## 📋 Overview

**Objective**: Generate the TypeScript API client for the new favorite endpoints (created in Phase 1 & 2) and extend the infrastructure layer services to map API responses to domain models with proper error handling and user notifications.

**Prerequisites**:
- Phase 1 & 2 backend endpoints completed (`FavoriteFileEndpoint` and `RemoveFavoriteEndpoint`)
- .NET API building successfully
- OpenAPI spec includes favorite endpoints

---

## 🎯 Deliverables

1. ✅ API client regenerated with favorite endpoint methods
2. ✅ `saveFavorite()` method added to [`storage.service.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.ts)
3. ✅ `removeFavorite()` method added to [`storage.service.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.ts)
4. ✅ Domain mapper updated (if new response types exist)
5. ✅ Unit tests for `saveFavorite()` in [`storage.service.spec.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.spec.ts)
6. ✅ Unit tests for `removeFavorite()` in [`storage.service.spec.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.spec.ts)
7. ✅ All tests passing

---

## 📚 Reference Files

### Key Implementation References

- **Storage Service**: [`libs/infrastructure/src/lib/storage/storage.service.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.ts)
- **Storage Service Tests**: [`libs/infrastructure/src/lib/storage/storage.service.spec.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.spec.ts)
- **Device Service Pattern**: [`libs/infrastructure/src/lib/device/device.service.ts`](../../../libs/infrastructure/src/lib/device/device.service.ts)
- **Domain Mapper**: [`libs/infrastructure/src/lib/domain.mapper.ts`](../../../libs/infrastructure/src/lib/domain.mapper.ts)
- **Error Utils**: [`libs/infrastructure/src/lib/error/api-error.utils.ts`](../../../libs/infrastructure/src/lib/error/api-error.utils.ts)
- **API Client Location**: [`libs/data-access/api-client/src/lib/apis/FilesApiService.ts`](../../../libs/data-access/api-client/src/lib/apis/FilesApiService.ts)

### Documentation References

- **API Client Generation**: [`docs/API_CLIENT_GENERATION.md`](../../API_CLIENT_GENERATION.md)
- **Testing Standards**: [`docs/TESTING_STANDARDS.md`](../../TESTING_STANDARDS.md)
- **Coding Standards**: [`docs/CODING_STANDARDS.md`](../../CODING_STANDARDS.md)

---

## 🔧 Implementation Steps

## Step 1: Generate API Client

### 1.1 Build .NET API

From the Angular workspace (`ClientApp/teensyrom-nx`):

```bash
dotnet build ../../TeensyRom.Api.csproj
```

**Expected Output**: Build succeeds, OpenAPI spec generated to `../../api-spec/TeensyRom.Api.json`

### 1.2 Generate TypeScript Client

```bash
pnpm run generate:api-client
```

**Expected Output**:
- TypeScript client regenerated in `libs/data-access/api-client/src/lib/`
- New methods in `FilesApiService.ts`: `saveFavorite()` and `removeFavorite()`
- New response types: `SaveFavoriteResponse` and `RemoveFavoriteResponse`

### 1.3 Verify Generated Methods

Check [`libs/data-access/api-client/src/lib/apis/FilesApiService.ts`](../../../libs/data-access/api-client/src/lib/apis/FilesApiService.ts):

```typescript
// Expected method signatures (exact signatures depend on backend implementation)
async saveFavorite(requestParameters: SaveFavoriteRequest): Promise<SaveFavoriteResponse>
async removeFavorite(requestParameters: RemoveFavoriteRequest): Promise<RemoveFavoriteResponse>
```

**Request Parameters Expected**:
- `deviceId: string`
- `storageType: TeensyStorageType`
- `path: string` (file path to favorite/unfavorite)

**Response Expected**:
- `file: FileItemDto` (the updated file with `isFavorite` flag set correctly)
- `message: string` (success message to display to user)

---

## Step 2: Update Domain Mapper (If Needed)

### 2.1 Check If New Response Types Exist

Inspect generated response types in `libs/data-access/api-client/src/lib/models/`:
- `SaveFavoriteResponse.ts`
- `RemoveFavoriteResponse.ts`

### 2.2 Add Mapper Methods (Only If Needed)

If response types are complex and different from existing `FileItemDto`, add to [`domain.mapper.ts`](../../../libs/infrastructure/src/lib/domain.mapper.ts):

```typescript
/**
 * Convert Save Favorite API response to domain FileItem
 */
static toSaveFavoriteResponse(response: SaveFavoriteResponse, baseApiUrl: string): FileItem {
  return this.toFileItem(response.file, baseApiUrl);
}
```

**Note**: If the response simply contains `FileItemDto`, no new mapper is needed - use existing `toFileItem()`.

---

## Step 3: Extend Storage Service

### 3.1 Add Save Favorite Method

Add to [`storage.service.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.ts):

```typescript
saveFavorite(
  deviceId: string,
  storageType: StorageType,
  path: string
): Observable<FileItem> {
  const apiStorageType = DomainMapper.toApiStorageType(storageType);
  return from(this.apiService.saveFavorite({ deviceId, storageType: apiStorageType, path })).pipe(
    map((response) => {
      // Display success message from API response
      if (response.message) {
        this.alertService.success(response.message);
      }
      // Map API response to domain model
      return DomainMapper.toFileItem(response.file, this.baseApiUrl);
    }),
    catchError((error) => this.handleError(error, 'saveFavorite', 'Failed to save favorite'))
  );
}
```

**Key Points**:
- Use `from()` to convert Promise to Observable
- Map `storageType` from domain to API type using `DomainMapper.toApiStorageType()`
- Display success message using `alertService.success()`
- Map response using existing `DomainMapper.toFileItem()`
- Handle errors using existing `handleError()` method (shows error alert automatically)

### 3.2 Add Remove Favorite Method

Add to [`storage.service.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.ts):

```typescript
removeFavorite(
  deviceId: string,
  storageType: StorageType,
  path: string
): Observable<FileItem> {
  const apiStorageType = DomainMapper.toApiStorageType(storageType);
  return from(this.apiService.removeFavorite({ deviceId, storageType: apiStorageType, path })).pipe(
    map((response) => {
      // Display success message from API response
      if (response.message) {
        this.alertService.success(response.message);
      }
      // Map API response to domain model
      return DomainMapper.toFileItem(response.file, this.baseApiUrl);
    }),
    catchError((error) => this.handleError(error, 'removeFavorite', 'Failed to remove favorite'))
  );
}
```

**Key Points**:
- Same pattern as `saveFavorite()`
- Different error message: "Failed to remove favorite"
- Returns updated `FileItem` with `isFavorite` flag set to `false`

### 3.3 Update handleError Method (If Needed)

The existing `handleError()` method already:
- Extracts error messages from API responses
- Displays error alerts via `alertService.error()`
- Logs errors for debugging
- Rethrows the error

**No changes needed** unless you want specific error handling for favorites.

---

## Step 4: Write Unit Tests

### 4.1 Add Mock API Methods

Update the mock in [`storage.service.spec.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.spec.ts) `beforeEach()`:

```typescript
let mockFilesApiService: {
  getDirectory: ReturnType<typeof vi.fn>;
  search: ReturnType<typeof vi.fn>;
  index: ReturnType<typeof vi.fn>;
  indexAll: ReturnType<typeof vi.fn>;
  saveFavorite: ReturnType<typeof vi.fn>;      // ADD THIS
  removeFavorite: ReturnType<typeof vi.fn>;    // ADD THIS
};

beforeEach(() => {
  mockFilesApiService = {
    getDirectory: vi.fn(),
    search: vi.fn(),
    index: vi.fn(),
    indexAll: vi.fn(),
    saveFavorite: vi.fn(),      // ADD THIS
    removeFavorite: vi.fn(),    // ADD THIS
  };

  // ... rest of setup
});
```

### 4.2 Add Test Helper Function

Add near the existing `createMockFileItemDto` helper:

```typescript
const createMockFavoriteResponse = (isFavorite: boolean, message: string) => ({
  file: createMockFileItemDto({ isFavorite }),
  message,
});
```

### 4.3 Add saveFavorite Tests

Add new describe block after existing tests:

```typescript
describe('saveFavorite', () => {
  it('should return FileItem with isFavorite=true when save succeeds', async () => {
    // Arrange
    const deviceId = 'test-device';
    const storageType = StorageType.Sd;
    const path = '/games/test.prg';
    const mockResponse = createMockFavoriteResponse(true, 'File saved to favorites');

    mockFilesApiService.saveFavorite.mockResolvedValue(mockResponse);

    // Act
    const result = await new Promise<FileItem>((resolve, reject) => {
      service.saveFavorite(deviceId, storageType, path).subscribe({
        next: resolve,
        error: reject,
      });
    });

    // Assert
    expect(mockFilesApiService.saveFavorite).toHaveBeenCalledWith({
      deviceId,
      storageType: TeensyStorageType.Sd,
      path,
    });
    expect(result).toBeDefined();
    expect(result.isFavorite).toBe(true);
    expect(result.name).toBe('test.prg');
  });

  it('should display success alert with message from API response', async () => {
    // Arrange
    const successMessage = 'File successfully added to favorites';
    const mockResponse = createMockFavoriteResponse(true, successMessage);
    mockFilesApiService.saveFavorite.mockResolvedValue(mockResponse);

    // Act
    await new Promise<FileItem>((resolve, reject) => {
      service.saveFavorite('device-1', StorageType.Sd, '/test.prg').subscribe({
        next: resolve,
        error: reject,
      });
    });

    // Assert
    expect(mockAlertService.success).toHaveBeenCalledWith(successMessage);
  });

  it('should call API with correct parameters', async () => {
    // Arrange
    const deviceId = 'device-123';
    const storageType = StorageType.Usb;
    const path = '/music/song.sid';
    const mockResponse = createMockFavoriteResponse(true, 'Success');

    mockFilesApiService.saveFavorite.mockResolvedValue(mockResponse);

    // Act
    await new Promise((resolve, reject) => {
      service.saveFavorite(deviceId, storageType, path).subscribe({
        next: resolve,
        error: reject,
      });
    });

    // Assert
    expect(mockFilesApiService.saveFavorite).toHaveBeenCalledTimes(1);
    expect(mockFilesApiService.saveFavorite).toHaveBeenCalledWith({
      deviceId,
      storageType: TeensyStorageType.Usb,
      path,
    });
  });

  it('should handle API errors and display error alert', async () => {
    // Arrange
    const errorMessage = 'Device not found';
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

    mockFilesApiService.saveFavorite.mockRejectedValue(new Error(errorMessage));

    // Act & Assert
    await expect(
      new Promise((resolve, reject) => {
        service.saveFavorite('device-1', StorageType.Sd, '/test.prg').subscribe({
          next: resolve,
          error: reject,
        });
      })
    ).rejects.toThrow(errorMessage);

    // Verify error alert displayed
    expect(mockAlertService.error).toHaveBeenCalledWith(errorMessage);

    // Verify error logged
    expect(consoleErrorSpy).toHaveBeenCalledWith(
      '❌ StorageService.saveFavorite error:',
      expect.any(Error)
    );

    // Cleanup
    consoleErrorSpy.mockRestore();
  });

  it('should use fallback message when API error has no message', async () => {
    // Arrange
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockFilesApiService.saveFavorite.mockRejectedValue({});

    // Act & Assert
    await expect(
      new Promise((resolve, reject) => {
        service.saveFavorite('device-1', StorageType.Sd, '/test.prg').subscribe({
          error: reject,
        });
      })
    ).rejects.toThrow();

    expect(mockAlertService.error).toHaveBeenCalledWith('Failed to save favorite');

    // Cleanup
    consoleErrorSpy.mockRestore();
  });

  it('should correctly convert StorageType.Sd to API type', async () => {
    // Arrange
    const mockResponse = createMockFavoriteResponse(true, 'Success');
    mockFilesApiService.saveFavorite.mockResolvedValue(mockResponse);

    // Act
    await new Promise((resolve, reject) => {
      service.saveFavorite('device', StorageType.Sd, '/test.prg').subscribe({
        next: resolve,
        error: reject,
      });
    });

    // Assert
    expect(mockFilesApiService.saveFavorite).toHaveBeenCalledWith(
      expect.objectContaining({ storageType: TeensyStorageType.Sd })
    );
  });

  it('should correctly convert StorageType.Usb to API type', async () => {
    // Arrange
    const mockResponse = createMockFavoriteResponse(true, 'Success');
    mockFilesApiService.saveFavorite.mockResolvedValue(mockResponse);

    // Act
    await new Promise((resolve, reject) => {
      service.saveFavorite('device', StorageType.Usb, '/test.prg').subscribe({
        next: resolve,
        error: reject,
      });
    });

    // Assert
    expect(mockFilesApiService.saveFavorite).toHaveBeenCalledWith(
      expect.objectContaining({ storageType: TeensyStorageType.Usb })
    );
  });
});
```

### 4.4 Add removeFavorite Tests

Add new describe block:

```typescript
describe('removeFavorite', () => {
  it('should return FileItem with isFavorite=false when remove succeeds', async () => {
    // Arrange
    const deviceId = 'test-device';
    const storageType = StorageType.Sd;
    const path = '/games/test.prg';
    const mockResponse = createMockFavoriteResponse(false, 'File removed from favorites');

    mockFilesApiService.removeFavorite.mockResolvedValue(mockResponse);

    // Act
    const result = await new Promise<FileItem>((resolve, reject) => {
      service.removeFavorite(deviceId, storageType, path).subscribe({
        next: resolve,
        error: reject,
      });
    });

    // Assert
    expect(mockFilesApiService.removeFavorite).toHaveBeenCalledWith({
      deviceId,
      storageType: TeensyStorageType.Sd,
      path,
    });
    expect(result).toBeDefined();
    expect(result.isFavorite).toBe(false);
    expect(result.name).toBe('test.prg');
  });

  it('should display success alert with message from API response', async () => {
    // Arrange
    const successMessage = 'File removed from favorites';
    const mockResponse = createMockFavoriteResponse(false, successMessage);
    mockFilesApiService.removeFavorite.mockResolvedValue(mockResponse);

    // Act
    await new Promise<FileItem>((resolve, reject) => {
      service.removeFavorite('device-1', StorageType.Sd, '/test.prg').subscribe({
        next: resolve,
        error: reject,
      });
    });

    // Assert
    expect(mockAlertService.success).toHaveBeenCalledWith(successMessage);
  });

  it('should call API with correct parameters', async () => {
    // Arrange
    const deviceId = 'device-456';
    const storageType = StorageType.Usb;
    const path = '/favorites/games/test.prg';
    const mockResponse = createMockFavoriteResponse(false, 'Success');

    mockFilesApiService.removeFavorite.mockResolvedValue(mockResponse);

    // Act
    await new Promise((resolve, reject) => {
      service.removeFavorite(deviceId, storageType, path).subscribe({
        next: resolve,
        error: reject,
      });
    });

    // Assert
    expect(mockFilesApiService.removeFavorite).toHaveBeenCalledTimes(1);
    expect(mockFilesApiService.removeFavorite).toHaveBeenCalledWith({
      deviceId,
      storageType: TeensyStorageType.Usb,
      path,
    });
  });

  it('should handle API errors and display error alert', async () => {
    // Arrange
    const errorMessage = 'File not found';
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

    mockFilesApiService.removeFavorite.mockRejectedValue(new Error(errorMessage));

    // Act & Assert
    await expect(
      new Promise((resolve, reject) => {
        service.removeFavorite('device-1', StorageType.Sd, '/test.prg').subscribe({
          next: resolve,
          error: reject,
        });
      })
    ).rejects.toThrow(errorMessage);

    // Verify error alert displayed
    expect(mockAlertService.error).toHaveBeenCalledWith(errorMessage);

    // Verify error logged
    expect(consoleErrorSpy).toHaveBeenCalledWith(
      '❌ StorageService.removeFavorite error:',
      expect.any(Error)
    );

    // Cleanup
    consoleErrorSpy.mockRestore();
  });

  it('should use fallback message when API error has no message', async () => {
    // Arrange
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockFilesApiService.removeFavorite.mockRejectedValue({});

    // Act & Assert
    await expect(
      new Promise((resolve, reject) => {
        service.removeFavorite('device-1', StorageType.Sd, '/test.prg').subscribe({
          error: reject,
        });
      })
    ).rejects.toThrow();

    expect(mockAlertService.error).toHaveBeenCalledWith('Failed to remove favorite');

    // Cleanup
    consoleErrorSpy.mockRestore();
  });

  it('should correctly convert StorageType.Sd to API type', async () => {
    // Arrange
    const mockResponse = createMockFavoriteResponse(false, 'Success');
    mockFilesApiService.removeFavorite.mockResolvedValue(mockResponse);

    // Act
    await new Promise((resolve, reject) => {
      service.removeFavorite('device', StorageType.Sd, '/test.prg').subscribe({
        next: resolve,
        error: reject,
      });
    });

    // Assert
    expect(mockFilesApiService.removeFavorite).toHaveBeenCalledWith(
      expect.objectContaining({ storageType: TeensyStorageType.Sd })
    );
  });

  it('should correctly convert StorageType.Usb to API type', async () => {
    // Arrange
    const mockResponse = createMockFavoriteResponse(false, 'Success');
    mockFilesApiService.removeFavorite.mockResolvedValue(mockResponse);

    // Act
    await new Promise((resolve, reject) => {
      service.removeFavorite('device', StorageType.Usb, '/test.prg').subscribe({
        next: resolve,
        error: reject,
      });
    });

    // Assert
    expect(mockFilesApiService.removeFavorite).toHaveBeenCalledWith(
      expect.objectContaining({ storageType: TeensyStorageType.Usb })
    );
  });
});
```

### 4.5 Update Alert Service Mock

Ensure `mockAlertService` includes `success` method in the test setup:

```typescript
mockAlertService = {
  error: vi.fn(),
  success: vi.fn(),  // ADD THIS if not present
};
```

---

## Step 5: Run Tests

### 5.1 Run Infrastructure Tests

```bash
npx nx test infrastructure
```

**Expected Output**: All tests pass, including new favorite tests

### 5.2 Run Specific Test File (Optional)

```bash
npx nx test infrastructure --testFile=storage.service.spec.ts
```

### 5.3 Check Coverage (Optional)

```bash
npx nx test infrastructure --coverage
```

---

## ✅ Success Criteria

Verify all items are complete:

### API Client Generation
- [ ] API client regenerated successfully
- [ ] `saveFavorite()` method exists in `FilesApiService`
- [ ] `removeFavorite()` method exists in `FilesApiService`
- [ ] Request types include `deviceId`, `storageType`, `path`
- [ ] Response types include `file: FileItemDto` and `message: string`

### Storage Service Implementation
- [ ] `saveFavorite()` method added to `storage.service.ts`
- [ ] `removeFavorite()` method added to `storage.service.ts`
- [ ] Methods return `Observable<FileItem>`
- [ ] Methods call API with correct parameters
- [ ] Success messages displayed via `alertService.success()`
- [ ] Errors handled via `handleError()` with appropriate fallback messages
- [ ] Storage type mapping uses `DomainMapper.toApiStorageType()`
- [ ] Response mapping uses `DomainMapper.toFileItem()`

### Domain Mapper (if applicable)
- [ ] New mapper methods added (only if response types differ from `FileItemDto`)
- [ ] Mapper preserves all field data
- [ ] `isFavorite` flag correctly set in mappings

### Unit Tests
- [ ] Test helper `createMockFavoriteResponse()` added
- [ ] Mock API service includes `saveFavorite` and `removeFavorite` methods
- [ ] Mock alert service includes `success` method
- [ ] `saveFavorite` tests cover:
  - [ ] Success case returns `FileItem` with `isFavorite=true`
  - [ ] Success alert displayed with API message
  - [ ] Correct API parameters
  - [ ] Error handling with error alert
  - [ ] Fallback error message
  - [ ] Storage type conversion (both Sd and Usb)
- [ ] `removeFavorite` tests cover:
  - [ ] Success case returns `FileItem` with `isFavorite=false`
  - [ ] Success alert displayed with API message
  - [ ] Correct API parameters
  - [ ] Error handling with error alert
  - [ ] Fallback error message
  - [ ] Storage type conversion (both Sd and Usb)
- [ ] All tests passing: `npx nx test infrastructure`
- [ ] No linting errors: `npx nx lint infrastructure`

### Integration Validation
- [ ] Infrastructure layer follows patterns from `device.service.ts`
- [ ] Error extraction uses existing `extractErrorMessage()` utility
- [ ] Alert service integration tested in unit tests
- [ ] Observables properly mapped from Promises
- [ ] RxJS operators used correctly (`from`, `map`, `catchError`)

---

## 🚨 Common Issues & Solutions

### Issue: API Client Generation Fails

**Solution**:
1. Ensure .NET API builds without errors
2. Check OpenAPI spec exists at `../../api-spec/TeensyRom.Api.json`
3. Verify `pnpm` is installed and up to date
4. Check `libs/data-access/api-client/scripts/generate-client.js` exists

### Issue: Generated Methods Have Different Signatures

**Solution**:
- Inspect generated `FilesApiService.ts` to see actual signatures
- Update `storage.service.ts` methods to match generated signatures
- Update test mocks to match actual response structure

### Issue: Tests Fail with "alertService.success is not a function"

**Solution**:
- Ensure `mockAlertService` includes `success: vi.fn()` in test setup
- Verify `ALERT_SERVICE` token is provided in `TestBed.configureTestingModule()`

### Issue: Domain Mapper Import Errors

**Solution**:
- Ensure `DomainMapper` is imported at top of `storage.service.ts`
- Check that mapper methods exist (may need to add if new types introduced)

### Issue: Type Errors with StorageType Conversion

**Solution**:
- Use `DomainMapper.toApiStorageType(storageType)` to convert domain type to API type
- Ensure `TeensyStorageType` is imported from `@teensyrom-nx/data-access/api-client`

---

## 📝 Notes

### Alert Service Integration

The infrastructure layer is responsible for displaying notifications to users:
- **Success alerts**: Call `this.alertService.success(message)` with message from API response
- **Error alerts**: Handled automatically by `handleError()` method via `extractErrorMessage()` utility

### Error Handling Pattern

The existing `handleError()` method in `storage.service.ts`:
1. Extracts user-friendly message from error (or uses fallback)
2. Logs error to console with `logError()` utility
3. Displays error alert via `alertService.error()`
4. Rethrows error for upstream handling

**Do not modify `handleError()`** - it already follows the correct pattern.

### Observable vs Promise

Infrastructure services use RxJS Observables:
- API client returns Promises
- Use `from()` to convert Promise to Observable
- Use `pipe()` with operators: `map()`, `catchError()`
- Return `Observable<T>` from service methods

### Testing Philosophy

Infrastructure layer tests are **unit tests** that:
- Mock the API client at the infrastructure boundary
- Test service contract implementation correctness
- Verify DTO → Domain model mapping
- Test error handling and alert integration
- Use Vitest with Angular TestBed

**Reference**: [`TESTING_STANDARDS.md`](../../TESTING_STANDARDS.md) - Infrastructure Layer section

---

## 🎯 Next Phase

After Phase 3 is complete and all tests pass:
- **Phase 4**: Application Layer - Add store actions for favorite operations
- **Phase 5**: Feature Layer - Add favorite button to player toolbar
- **Phase 6**: E2E Tests - Validate complete user workflows

---

## 📞 Questions?

If you encounter issues not covered in this document:
1. Review reference implementations in `device.service.ts` and `device.service.spec.ts`
2. Check testing patterns in existing `storage.service.spec.ts`
3. Consult documentation links provided throughout this guide
4. Verify backend Phase 1 & 2 endpoints are working correctly

**Document Version**: 1.0
**Last Updated**: Phase 3 Implementation Guide
**Target Agent**: Haiku or Junior Developer
