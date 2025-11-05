# Favorite Files Feature Plan

**Project Overview**: Implement a comprehensive favorites system that enables users to mark files (games, music, images) as favorites and manage those favorites throughout the TeensyROM UI. The system creates favorite copies in designated favorite directories while maintaining bidirectional links between original files and their favorite counterparts, ensuring the UI consistently reflects favorite status regardless of where the file is viewed.

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **State Standards**: [STATE_STANDARDS.md](../../STATE_STANDARDS.md)
- **API Client Generation**: [API_CLIENT_GENERATION.md](../../API_CLIENT_GENERATION.md)
- **E2E Testing Guide**: [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)
- **Store Testing**: [STORE_TESTING.md](../../STORE_TESTING.md)

---

## 🎯 Project Objective

Enable users to mark files as favorites, creating a personalized collection of their preferred content across games, music, and images. When users favorite a file, the system creates a copy in a type-specific favorite directory while maintaining a bidirectional relationship between the original and favorite versions. This relationship ensures that when viewing the original file location, users can see it's already been favorited, preventing duplicate favorites and providing consistent visual feedback throughout the application.

**User Value**: Users can curate their favorite content for quick access later, similar to bookmarking functionality in browsers or favoriting songs in music applications. The visual indicator (heart icon) appears next to favorited files in both their original locations and in dedicated favorite directories, providing immediate recognition of favorite status without requiring users to remember what they've already favorited.

**System Benefits**: The feature follows existing patterns established by the Launch File functionality, reusing infrastructure service patterns, storage service methods, and UI interaction patterns. The backend already contains the core favorite management logic in `CachedStorageService`, which will be extracted into reusable methods in the base `StorageService` for use by the new endpoints.

---

## 📋 Implementation Phases

<details open>
<summary><h3>Phase 1: Backend - Save Favorite Endpoint & Tests</h3></summary>

### Objective

Create the backend API endpoint for saving files as favorites, along with the storage service method and comprehensive integration tests. This phase establishes the server-side foundation that will be consumed by the frontend in later phases.

### Key Deliverables

- [ ] `FavoriteFileEndpoint.cs` created under [`Endpoints/Files/`](../../../../../TeensyRom.Api/Endpoints/) directory
- [ ] `SaveFavorite` method added to [`StorageService.cs`](../../../../../TeensyRom.Core.Storage/StorageService.cs)
- [ ] Integration tests created in [`SaveFavoriteTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/)
- [ ] Endpoint accepts filename/path, device ID, and storage type parameters
- [ ] Method creates favorite copy with parent path reference preserved
- [ ] All tests passing for success and error scenarios

### High-Level Tasks

1. **Create FavoriteFileEndpoint**: Implement a new endpoint following the pattern established by [`LaunchFileEndpoint.cs`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileEndpoint.cs)

   - Accept device ID, storage type, and file path as parameters
   - Validate device exists and has requested storage available
   - Retrieve the file using storage service
   - Call storage service to save as favorite
   - Return appropriate success/error responses

2. **Extract SaveFavorite Logic**: Copy the `SaveFavorite` method from [`CachedStorageService.cs`](../../../../../TeensyRom.Core.Storage/CachedStorageService.cs) to [`StorageService.cs`](../../../../../TeensyRom.Core.Storage/StorageService.cs)

   - Adapt method to work with `StorageService` infrastructure
   - Maintain parent path reference preservation logic
   - Ensure favorite type-specific directory targeting works correctly

3. **Write Integration Tests**: Create comprehensive integration tests following patterns in [`LaunchFileTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/LaunchFileTests.cs)
   - Test successful favorite creation for each file type (game, music, image)
   - Test error scenarios (device not found, file not found, storage unavailable)
   - Test validation failures (invalid device ID, invalid storage type)
   - Test that parent path references are correctly preserved
   - Test duplicate favorite handling

### Open Questions (Phase 1)

- Should the endpoint return the newly created favorite file information in the response?
- How should the endpoint handle attempting to favorite an already favorited file?
- Should there be rate limiting or validation on the number of favorites a user can create?

</details>

<details open>
<summary><h3>Phase 2: Backend - Remove Favorite Endpoint & Tests</h3></summary>

### Objective

Create the backend API endpoint for removing files from favorites, along with comprehensive integration tests. This completes the backend CRUD operations for favorites management.

### Key Deliverables

- [ ] `RemoveFavoriteEndpoint.cs` created under [`Endpoints/Files/`](../../../../../TeensyRom.Api/Endpoints/) directory
- [ ] `RemoveFavorite` method added to [`StorageService.cs`](../../../../../TeensyRom.Core.Storage/StorageService.cs)
- [ ] Integration tests created in [`RemoveFavoriteTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/)
- [ ] Endpoint accepts filename/path, device ID, and storage type parameters
- [ ] Method removes favorite copy and updates original file's favorite status
- [ ] All tests passing for success and error scenarios

### High-Level Tasks

1. **Create RemoveFavoriteEndpoint**: Implement endpoint following similar patterns to FavoriteFileEndpoint

   - Accept device ID, storage type, and file path as parameters
   - Validate device and storage availability
   - Retrieve the file using storage service
   - Call storage service to remove favorite
   - Return appropriate success/error responses

2. **Extract RemoveFavorite Logic**: Copy the `RemoveFavorite` method from [`CachedStorageService.cs`](../../../../../TeensyRom.Core.Storage/CachedStorageService.cs) to [`StorageService.cs`](../../../../../TeensyRom.Core.Storage/StorageService.cs)

   - Adapt method to work with `StorageService` infrastructure
   - Ensure favorite file deletion occurs
   - Maintain favorite status synchronization with original file

3. **Write Integration Tests**: Create comprehensive integration tests
   - Test successful favorite removal for each file type
   - Test error scenarios (device not found, file not found, not actually favorited)
   - Test validation failures (invalid parameters)
   - Test that original file favorite status is correctly updated
   - Test removing non-existent favorite gracefully

### Open Questions (Phase 2)

- Should removing a favorite from the favorite directory automatically unfavorite the original?
- How should the system handle orphaned favorites (original file deleted)?
- Should there be a "remove all favorites" bulk operation?

</details>

<details open>
<summary><h3>Phase 3: Frontend - API Client Generation & Infrastructure Layer</h3></summary>

### Objective

Generate the TypeScript API client for the new favorite endpoints and create infrastructure layer services that map API responses to domain models. This establishes the frontend's communication layer with the backend.

### Key Deliverables

- [ ] API client regenerated using [`API_CLIENT_GENERATION.md`](../../API_CLIENT_GENERATION.md) process
- [ ] New favorite methods added to [`storage.service.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.ts)
- [ ] Response mapping added to [`domain.mapper.ts`](../../../libs/infrastructure/src/lib/domain.mapper.ts)
- [ ] Unit tests for storage service methods
- [ ] Unit tests for domain mapper transformations
- [ ] All infrastructure tests passing

### High-Level Tasks

1. **Generate API Client**: Follow the documented process to regenerate the TypeScript client

   - Build the .NET API project to produce OpenAPI spec
   - Run `pnpm run generate:api-client` from the Angular workspace
   - Verify new favorite endpoints appear in generated client

2. **Add Infrastructure Methods**: Extend the storage service with favorite operations

   - Add `saveFavorite()` method that calls generated API client
   - Add `removeFavorite()` method that calls generated API client
   - Map parameters from domain types to API types
   - Return Observables that emit domain models
   - Inject and use [`alert.service.ts`](../../../libs/app/src/lib/alert.service.ts) to display notifications
   - Follow pattern from [`device.service.ts`](../../../libs/infrastructure/src/lib/device/device.service.ts)
   - Use `extractErrorMessage()` utility to extract API response messages
   - Display success messages from API response on successful operations
   - Display error messages from API response (via extractErrorMessage) on failures

3. **Extend Domain Mapper**: Add transformation methods for favorite operations

   - Map API response types to domain `FileItem` models
   - Ensure `isFavorite` flag is correctly set in mappings
   - Handle error responses appropriately

4. **Write Unit Tests**: Create comprehensive unit tests for infrastructure layer
   - Test storage service methods call API with correct parameters
   - Test successful response mapping
   - Test error response handling with extractErrorMessage utility
   - Test alert service called with success messages from API response
   - Test alert service called with error messages extracted from API response
   - Test mapper transformations preserve all field data
   - Mock infrastructure boundary following [`TESTING_STANDARDS.md`](../../TESTING_STANDARDS.md)

### Open Questions (Phase 3)

- Should favorite operations return the updated file or just a success indicator?
- How should optimistic UI updates be handled (update before API confirms)?
- Should there be a retry mechanism for failed favorite operations?

</details>

<details open>
<summary><h3>Phase 4: Frontend - Application Layer Store Actions</h3></summary>

### Objective

Create application layer store actions for favorite operations, integrating infrastructure services with state management. These actions will be consumed by UI components to trigger favorite/unfavorite operations.

### Key Deliverables

- [ ] `saveFavorite` action added to [`storage-store.ts`](../../../libs/application/src/lib/storage/storage-store.ts)
- [ ] `removeFavorite` action added to storage store
- [ ] Actions follow patterns in [`STATE_STANDARDS.md`](../../STATE_STANDARDS.md)
- [ ] Store tests following [`STORE_TESTING.md`](../../STORE_TESTING.md) methodology
- [ ] Loading states managed during operations
- [ ] Error states set on failures
- [ ] All store tests passing

### High-Level Tasks

1. **Create Store Actions**: Add favorite management actions to the storage store

   - Implement `saveFavorite` action using async/await pattern
   - Implement `removeFavorite` action using async/await pattern
   - Use `updateState` with `actionMessage` for Redux DevTools correlation
   - Set loading states at operation start, clear on completion
   - Update file `isFavorite` flag on success
   - Set error states on failure with meaningful messages

2. **Update State Shape**: Extend storage state if needed for favorite tracking

   - Ensure files in storage directories have `isFavorite` flag
   - Update cached directory entries when favorite status changes
   - Maintain consistency between original and favorite file states

3. **Write Store Tests**: Create behavioral tests for favorite actions
   - Test successful save favorite updates file state correctly
   - Test successful remove favorite updates file state correctly
   - Test loading states appear and clear appropriately
   - Test error handling sets error state without corrupting data
   - Test that parent file favorite status updates when favorite created
   - Mock infrastructure service boundary per testing standards

### Open Questions (Phase 4)

- Should the action automatically refresh the current directory after favoriting?
- How should the store handle rapid successive favorite/unfavorite operations?
- Should favorite status be persisted across application sessions?

</details>

<details open>
<summary><h3>Phase 5: Frontend - Player Toolbar Favorite Button UI</h3></summary>

### Objective

Add a favorite button to the player toolbar that displays favorite status and triggers favorite/unfavorite actions. The button shows visual state changes and integrates with existing player controls.

### Key Deliverables

- [ ] Favorite button added to [`player-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.ts)
- [ ] Button uses [`icon-button.component.ts`](../../../libs/ui/components/src/lib/icon-button/icon-button.component.ts)
- [ ] Empty heart icon shown when file not favorited
- [ ] Solid heart icon shown when file is favorited
- [ ] Clicking button calls appropriate store action based on current state
- [ ] Button positioned next to shuffle button
- [ ] Component unit tests passing
- [ ] UI responds to loading and error states

### High-Level Tasks

1. **Add Favorite Button Component**: Integrate icon button into player toolbar

   - Position button next to existing shuffle button
   - Use Material icon 'favorite_border' for unfavorited state
   - Use Material icon 'favorite' for favorited state
   - Bind click handler to favorite toggle logic
   - Add appropriate aria-label for accessibility

2. **Implement Toggle Logic**: Add methods to handle favorite state changes

   - Check current file's `isFavorite` flag from player context
   - Call `saveFavorite` action when not favorited
   - Call `removeFavorite` action when favorited
   - Disable button during loading operations
   - Show error state visually if operation fails

3. **Wire Up State**: Connect component to application state

   - Subscribe to current file from player context
   - Subscribe to loading state from storage store
   - Subscribe to error state from storage store
   - Update button appearance reactively based on state

4. **Write Component Tests**: Create unit tests following [`SMART_COMPONENT_TESTING.md`](../../SMART_COMPONENT_TESTING.md)
   - Test button shows correct icon for favorited file
   - Test button shows correct icon for non-favorited file
   - Test clicking button calls save action when not favorited
   - Test clicking button calls remove action when favorited
   - Test button disabled during loading operations
   - Test error state displays appropriately

### Open Questions (Phase 5)

- Should there be a tooltip showing favorite status on hover?
- Should the button show a loading spinner during operations?
- Should there be animation when toggling favorite state?
- Should the button be hidden when no file is loaded?

</details>

<details open>
<summary><h3>Phase 6: Frontend - E2E Tests for Favorite Operations</h3></summary>

### Objective

Create end-to-end Cypress tests that validate the complete favorite workflow from user interaction through backend integration. Tests use fixture-driven, interceptor-based approach following established E2E patterns.

### Key Deliverables

- [ ] New `/storage` test directory created under [`apps/teensyrom-ui-e2e/src/e2e/`](../../../apps/teensyrom-ui-e2e/src/e2e/)
- [ ] `favorite-operations.cy.ts` test spec created
- [ ] Storage interceptors created in [`apps/teensyrom-ui-e2e/src/support/interceptors/`](../../../apps/teensyrom-ui-e2e/src/support/interceptors/)
- [ ] Fixture data for favorite scenarios
- [ ] Test helpers for favorite operations
- [ ] All E2E tests passing

### High-Level Tasks

1. **Create Storage Test Directory**: Organize tests by feature area

   - Create `/storage` folder for storage-related E2E tests
   - Follow organizational pattern from [`device-connection.cy.ts`](../../../apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts)
   - Create test helpers file for reusable favorite test functions

2. **Create Storage Interceptors**: Build API mocking for favorite endpoints

   - Create `storage.interceptors.ts` with `interceptSaveFavorite()` function
   - Create `interceptRemoveFavorite()` function
   - Support success and error modes for testing failure scenarios
   - Follow patterns from [`device.interceptors.ts`](../../../apps/teensyrom-ui-e2e/src/support/interceptors/device.interceptors.ts)

3. **Create Fixture Data**: Build realistic test data for favorite scenarios

   - Create file fixtures with `isFavorite: false` for save tests
   - Create file fixtures with `isFavorite: true` for remove tests
   - Create directory fixtures containing favorite files
   - Follow fixture patterns from [`devices.fixture.ts`](../../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts)

4. **Write E2E Test Scenarios**: Comprehensive workflow validation

   - Test favoriting a non-favorited file shows solid heart icon
   - Test unfavoriting a favorited file shows empty heart icon
   - Test favorite button disabled during operation
   - Test API called with correct parameters
   - Test success alert appears with message from API response after successful favorite
   - Test success alert appears with message from API response after successful unfavorite
   - Test error alert appears with message from API error response on failure
   - Test alerts display in correct position (bottom-right)
   - Test alerts auto-dismiss after default timeout
   - Test multiple alerts display correctly for successive operations
   - Test favorite status persists on navigation
   - Test viewing original file shows favorited status

5. **Create Test Helpers**: Reusable functions for favorite testing
   - `clickFavoriteButton()` - Click favorite button in player toolbar
   - `verifyFavoriteStatus(isFavorite)` - Check visual favorite indicator
   - `waitForFavoriteOperation()` - Wait for API call completion
   - `navigateToFavoriteDirectory()` - Navigate to favorites folder
   - `verifyAlert(message)` - Verify alert appears with specific message text
   - `waitForAlertDismissal()` - Wait for auto-dismiss timeout
   - `verifyAlertPosition()` - Verify alert displays in bottom-right corner

### Open Questions (Phase 6)

- Should tests validate favorite file appears in favorites directory?
- Should tests cover all file types (games, music, images)?
- Should tests validate bidirectional favorite status (original ↔ favorite)?
- What level of error scenario coverage is needed?

</details>

---

## 🏗️ Architecture Overview

### Backend Architecture

The backend follows Clean Architecture patterns with clear separation between API endpoints, application logic, and storage operations.

**Endpoint Layer** (`TeensyRom.Api/Endpoints/Files/`):

- `FavoriteFileEndpoint.cs` - REST endpoint for saving favorites
- `RemoveFavoriteEndpoint.cs` - REST endpoint for removing favorites
- Follow RadEndpoints minimal API pattern
- Handle request validation and device management
- Delegate to storage services for file operations

**Storage Service Layer** (`TeensyRom.Core.Storage/`):

- `StorageService.cs` contains core favorite operations
- `SaveFavorite()` - Creates favorite copy with parent path link
- `RemoveFavorite()` - Deletes favorite copy, updates original status
- Methods adapted from existing `CachedStorageService.SaveFavorite()` implementation
- Maintains bidirectional relationship between original and favorite files

**Storage Behavior**:

- Favorite files are copies, not references
- Original files retain `parentPath` reference to location before favoriting
- Favorite directories are type-specific: `/favorites/games`, `/favorites/music`, `/favorites/images`
- When viewing original file, UI shows favorited status by checking if favorite exists
- When viewing favorite file, UI shows favorited status inherently

### Frontend Architecture

The frontend follows Clean Architecture with distinct layers: Domain → Application → Infrastructure → Features.

**Infrastructure Layer** (`libs/infrastructure/src/lib/storage/`):

- `storage.service.ts` - Implements `IStorageService` domain contract
- Calls generated API client methods
- Maps API DTOs to domain models via `domain.mapper.ts`
- Returns Observables for async operations
- Uses [`alert.service.ts`](../../../libs/app/src/lib/alert.service.ts) to display success/error notifications to users

**Application Layer** (`libs/application/src/lib/storage/`):

- `storage-store.ts` - NgRx Signal Store managing storage state
- Actions: `saveFavorite()`, `removeFavorite()`
- Manages loading states, error states, and file state updates
- Uses async/await pattern with `firstValueFrom()` for Promise-based flow

**Feature Layer** (`libs/features/player/`):

- `player-toolbar.component.ts` - Smart component with favorite button
- Consumes player context for current file information
- Dispatches favorite actions to storage store
- Reactively updates UI based on state signals

**Domain Layer** (`libs/domain/src/lib/models/`):

- `file-item.model.ts` already contains `isFavorite: boolean` flag
- No changes needed to domain models

### Integration Points

**Backend to Frontend**:

- OpenAPI spec generation during .NET build
- TypeScript client generation via `pnpm run generate:api-client`
- API client consumed by infrastructure services

**Application to Infrastructure**:

- Store actions call infrastructure service methods
- Infrastructure returns Observables mapped to domain models
- Error handling bubbles up through layers

**Feature to Application**:

- Components inject store via dependency injection
- Components call store action methods
- Components subscribe to store state signals
- Reactive updates trigger UI changes

---

## 🧪 Testing Strategy

### Backend Testing (Integration Tests)

**Test Files**:

- `SaveFavoriteTests.cs` - Integration tests for save favorite endpoint
- `RemoveFavoriteTests.cs` - Integration tests for remove favorite endpoint

**Coverage Areas**:

- Success scenarios for all file types (games, music, images)
- Error scenarios (device not found, file not found, storage unavailable)
- Validation failures (invalid device ID, invalid storage type, invalid paths)
- Parent path preservation verification
- Duplicate favorite handling
- Favorite/unfavorite idempotency

**Test Pattern**:

- Follow patterns from [`LaunchFileTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/LaunchFileTests.cs)
- Use EndpointFixture for test infrastructure
- Test against real connected TeensyROM device (integration test environment)
- Verify HTTP status codes and response content
- Validate file system state changes

### Frontend Testing

**Unit Tests** (Infrastructure Layer):

- Storage service methods call API client correctly
- Domain mapper transforms favorite responses accurately
- Error responses handled appropriately
- Mock API client at infrastructure boundary

**Unit Tests** (Application Layer):

- Store actions update state correctly on success
- Store actions set error state on failure
- Loading states managed properly
- File `isFavorite` flag updates correctly
- Mock infrastructure service per [`STORE_TESTING.md`](../../STORE_TESTING.md)

**Component Tests** (Feature Layer):

- Favorite button displays correct icon for state
- Button click dispatches correct action
- Button disabled during loading
- Error state displayed visually
- Mock player context and storage store

**E2E Tests** (Cypress):

- Complete workflow: click button → API call → state update → UI reflects change
- Favorite operation success path
- Unfavorite operation success path
- Error handling displays to user
- Favorite status persists across navigation
- Use interceptor-based mocking per [`E2E_TESTS.md`](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)

---

## 🎭 Given-When-Then Scenarios

<details open>
<summary><h3>Save Favorite Scenarios</h3></summary>

<details open>
<summary><strong>Scenario 1: Favorite a Non-Favorited File</strong></summary>

```gherkin
Given a user is viewing a file that is not favorited
And the file is displayed in the player toolbar
When the user clicks the favorite button showing an empty heart icon
Then a save favorite API call is made with the file path, device ID, and storage type
And a favorite copy is created in the appropriate favorites directory
And the original file's isFavorite flag is set to true
And the favorite button icon changes to a solid heart
And a success alert message is displayed with text from the API response
And the alert auto-dismisses after the default timeout
```

</details>

<details open>
<summary><strong>Scenario 2: Save Favorite API Failure</strong></summary>

```gherkin
Given a user is viewing a non-favorited file
When the user clicks the favorite button
And the save favorite API call fails with an error
Then the file's isFavorite flag remains false
And the favorite button icon remains an empty heart
And an error alert message is displayed with text extracted from the API error response
And the alert auto-dismisses after the default timeout
And the user can retry the operation
```

</details>

<details open>
<summary><strong>Scenario 3: Favorite Button Disabled During Operation</strong></summary>

```gherkin
Given a user is viewing a non-favorited file
When the user clicks the favorite button
And the save favorite operation is in progress
Then the favorite button is disabled
And a loading indicator may be shown
And clicking the button again has no effect
When the operation completes
Then the button is re-enabled
And the icon updates to reflect the new state
```

</details>

<details open>
<summary><strong>Scenario 4: View Favorited File at Original Location</strong></summary>

```gherkin
Given a user has favorited a file
And a favorite copy exists in the favorites directory
When the user navigates to the original file location
And views the file in the player toolbar
Then the favorite button shows a solid heart icon
And the file's isFavorite flag is true
When the user clicks to unfavorite
Then both the original and favorite copy are updated
```

</details>

</details>

---

<details open>
<summary><h3>Remove Favorite Scenarios</h3></summary>

<details open>
<summary><strong>Scenario 5: Unfavorite a Favorited File</strong></summary>

```gherkin
Given a user is viewing a file that is favorited
And the favorite button shows a solid heart icon
When the user clicks the favorite button
Then a remove favorite API call is made with the file path, device ID, and storage type
And the favorite copy is deleted from the favorites directory
And the original file's isFavorite flag is set to false
And the favorite button icon changes to an empty heart
And a success alert message is displayed with text from the API response
And the alert auto-dismisses after the default timeout
```

</details>

<details open>
<summary><strong>Scenario 6: Remove Favorite API Failure</strong></summary>

```gherkin
Given a user is viewing a favorited file
When the user clicks the favorite button to unfavorite
And the remove favorite API call fails with an error
Then the file's isFavorite flag remains true
And the favorite button icon remains a solid heart
And an error alert message is displayed with text extracted from the API error response
And the alert auto-dismisses after the default timeout
And the favorite copy remains in the favorites directory
And the user can retry the operation
```

</details>

<details open>
<summary><strong>Scenario 7: Remove Favorite from Favorites Directory</strong></summary>

```gherkin
Given a user is browsing the favorites directory
And viewing a favorited file in the player
When the user clicks the favorite button to unfavorite
Then the favorite copy is removed from the directory
And if the user navigates to the original location
Then the original file shows as not favorited
And the bidirectional link is properly severed
```

</details>

</details>

---

<details open>
<summary><h3>Favorite Status Display Scenarios</h3></summary>

<details open>
<summary><strong>Scenario 8: Empty Heart Icon for Non-Favorited File</strong></summary>

```gherkin
Given a user is viewing a file
And the file has not been favorited
When the player toolbar displays the current file
Then the favorite button shows an empty heart icon (favorite_border)
And the button is enabled and clickable
And hovering may show "Add to Favorites" tooltip
```

</details>

<details open>
<summary><strong>Scenario 9: Solid Heart Icon for Favorited File</strong></summary>

```gherkin
Given a user is viewing a file
And the file has been favorited
When the player toolbar displays the current file
Then the favorite button shows a solid heart icon (favorite)
And the button is enabled and clickable
And hovering may show "Remove from Favorites" tooltip
```

</details>

<details open>
<summary><strong>Scenario 10: Favorite Status Persists Across Navigation</strong></summary>

```gherkin
Given a user favorites a file
And the solid heart icon is displayed
When the user navigates away to another file
And then navigates back to the previously favorited file
Then the solid heart icon is still displayed
And the file's favorite status is maintained in state
```

</details>

</details>

---

<details open>
<summary><h3>Alert Notification Scenarios</h3></summary>

<details open>
<summary><strong>Scenario 15: Success Alert Display on Favorite</strong></summary>

```gherkin
Given a user successfully favorites a file
When the save favorite operation completes
Then a success alert message appears with text from the API response
And the alert displays in the bottom-right corner
And the alert auto-dismisses after the default timeout
And the alert can be manually dismissed by clicking the close button
```

</details>

<details open>
<summary><strong>Scenario 16: Error Alert Display on Favorite Failure</strong></summary>

```gherkin
Given a user attempts to favorite a file
When the save favorite operation fails
Then an error alert message appears with text extracted from the API error response
And the alert displays in the bottom-right corner
And the alert auto-dismisses after the default timeout
And the alert can be manually dismissed by clicking the close button
```

</details>

<details open>
<summary><strong>Scenario 17: Success Alert Display on Unfavorite</strong></summary>

```gherkin
Given a user successfully unfavorites a file
When the remove favorite operation completes
Then a success alert message appears with text from the API response
And the alert displays in the bottom-right corner
And the alert auto-dismisses after the default timeout
And the alert can be manually dismissed by clicking the close button
```

</details>

<details open>
<summary><strong>Scenario 18: Multiple Alerts for Successive Operations</strong></summary>

```gherkin
Given a user performs multiple favorite operations in quick succession
When each operation completes
Then each operation displays its own alert message
And alerts stack vertically in the bottom-right corner
And each alert auto-dismisses independently after the default timeout
And the alert display area handles multiple simultaneous alerts gracefully
```

</details>

</details>

---

<details open>
<summary><h3>Edge Case Scenarios</h3></summary>

<details open>
<summary><strong>Scenario 11: Favorite Already Favorited File</strong></summary>

```gherkin
Given a user is viewing a favorited file
And the solid heart icon is displayed
When the user clicks the favorite button
Then the remove favorite operation is triggered (not save)
And the system does not attempt to create a duplicate favorite
And the favorite status is toggled off
```

</details>

<details open>
<summary><strong>Scenario 12: No File Loaded in Player</strong></summary>

```gherkin
Given no file is currently loaded in the player
When the player toolbar is displayed
Then the favorite button may be hidden or disabled
And clicking it has no effect
And no API calls are made
```

</details>

<details open>
<summary><strong>Scenario 13: Rapid Favorite Toggle Operations</strong></summary>

```gherkin
Given a user is viewing a file
When the user rapidly clicks the favorite button multiple times
Then only the first operation is processed
And subsequent clicks are ignored while loading
And the button is disabled during the operation
When the operation completes
Then the final state reflects the successful operation
And the button is re-enabled for further interaction
```

</details>

<details open>
<summary><strong>Scenario 14: Favorite Different File Types</strong></summary>

```gherkin
Given a user has files of type game, music, and image
When the user favorites a game file
Then it is saved to /favorites/games directory
When the user favorites a music file
Then it is saved to /favorites/music directory
When the user favorites an image file
Then it is saved to /favorites/images directory
And each favorite is properly categorized by type
```

</details>

</details>

---

## ✅ Success Criteria

### Backend Success Criteria

- [ ] `FavoriteFileEndpoint` successfully creates favorite copies for all supported file types
- [ ] `RemoveFavoriteEndpoint` successfully removes favorite copies and updates original status
- [ ] Parent path references are preserved in favorite files for bidirectional linking
- [ ] Favorite files are created in correct type-specific directories
- [ ] All backend integration tests pass with 100% coverage of success and error paths
- [ ] Endpoints return appropriate HTTP status codes for all scenarios
- [ ] API follows OpenAPI specification and generates correct client code

### Frontend Infrastructure Success Criteria

- [ ] TypeScript API client includes generated methods for favorite endpoints
- [ ] Infrastructure storage service methods call API client with correct parameters
- [ ] Domain mapper correctly transforms favorite API responses to domain models
- [ ] Alert service displays success messages from API responses on successful operations
- [ ] Alert service displays error messages extracted from API error responses on failures
- [ ] Service follows pattern from [`device.service.ts`](../../../libs/infrastructure/src/lib/device/device.service.ts)
- [ ] Uses `extractErrorMessage()` utility for error message extraction
- [ ] All alerts auto-dismiss after default timeout
- [ ] All infrastructure unit tests pass with proper mocking at boundary
- [ ] Alert service integration is tested in unit tests
- [ ] Error responses are handled gracefully without throwing exceptions

### Frontend Application Success Criteria

- [ ] Storage store actions update file `isFavorite` flags correctly
- [ ] Loading states appear and clear appropriately during operations
- [ ] Error states are set with meaningful messages on failures
- [ ] State remains consistent after successful and failed operations
- [ ] All store behavioral tests pass following [`STORE_TESTING.md`](../../STORE_TESTING.md)
- [ ] Redux DevTools properly tracks all state mutations with action messages

### Frontend Feature Success Criteria

- [ ] Favorite button displays in player toolbar next to shuffle button
- [ ] Empty heart icon shows for non-favorited files
- [ ] Solid heart icon shows for favorited files
- [ ] Clicking button triggers appropriate action based on current state
- [ ] Button is disabled during loading operations
- [ ] UI updates reactively when favorite status changes
- [ ] All component unit tests pass

### E2E Success Criteria

- [ ] Complete workflow tests pass for save favorite operation
- [ ] Complete workflow tests pass for remove favorite operation
- [ ] Success alerts appear with messages from API responses after successful operations
- [ ] Error alerts appear with messages from API error responses on failures
- [ ] Alerts display in correct position (bottom-right corner)
- [ ] Alerts auto-dismiss after default timeout
- [ ] Multiple alerts display correctly for successive operations
- [ ] Alert messages match API response text
- [ ] Favorite status persists across navigation
- [ ] Tests follow fixture-driven, interceptor-based patterns from [`E2E_TESTS.md`](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)
- [ ] All E2E tests pass reliably without flakiness

### Overall Feature Success Criteria

- [ ] Users can favorite and unfavorite files from player toolbar
- [ ] Favorite status is visually indicated with heart icon
- [ ] Favorite copies are created and deleted correctly on backend
- [ ] Bidirectional link between original and favorite files is maintained
- [ ] Original file shows favorited status when favorite exists
- [ ] Success alerts notify users with messages from API responses
- [ ] Error alerts notify users with messages extracted from API error responses
- [ ] All alerts auto-dismiss after default timeout
- [ ] Alert notifications follow established infrastructure patterns
- [ ] System handles errors gracefully with user feedback
- [ ] All tests (backend integration, frontend unit, frontend E2E) pass

---

## 📚 Related Documentation

- **Architecture Overview**: [OVERVIEW_CONTEXT.md](../../OVERVIEW_CONTEXT.md)
- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **State Standards**: [STATE_STANDARDS.md](../../STATE_STANDARDS.md)
- **API Client Generation**: [API_CLIENT_GENERATION.md](../../API_CLIENT_GENERATION.md)
- **Store Testing**: [STORE_TESTING.md](../../STORE_TESTING.md)
- **Smart Component Testing**: [SMART_COMPONENT_TESTING.md](../../SMART_COMPONENT_TESTING.md)
- **E2E Testing Guide**: [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)

---

## 📝 Notes

### Design Considerations

- **Bidirectional Linking**: The parent path reference is critical for showing favorite status at original file locations. Without this link, users viewing the original location wouldn't know they've already favorited that file.

- **Copy vs Reference**: Favorites are physical copies, not symbolic links. This ensures favorites remain accessible even if originals are moved or deleted, but requires synchronization logic for status updates.

- **Type-Specific Directories**: Favorites are organized by file type (games, music, images) in separate subdirectories. This enables filtering and organizing favorites by content type.

- **Existing Backend Logic**: The `CachedStorageService.SaveFavorite()` method already contains the complete favorite creation logic including parent path linking. Extraction to `StorageService` is straightforward code reuse.

- **Visual Consistency**: The heart icon pattern (empty vs solid) follows common UX conventions from streaming services, social media, and other bookmark/favorite systems users are familiar with.

- **Alert Service Integration**: The infrastructure layer follows the established pattern from [`device.service.ts`](../../../libs/infrastructure/src/lib/device/device.service.ts), using the [`alert.service.ts`](../../../libs/app/src/lib/alert.service.ts) for user notifications. Success messages come directly from API responses, while error messages are extracted using the `extractErrorMessage()` utility that parses ProblemDetails from error responses.

- **Consistent Error Handling**: The `extractErrorMessage()` utility in [`api-error.utils.ts`](../../../libs/infrastructure/src/lib/error/api-error.utils.ts) provides centralized error message extraction from API responses, ensuring consistent error handling across all infrastructure services. All alerts auto-dismiss after the default timeout, maintaining consistency with existing application behavior.

### Future Enhancement Ideas

- **Favorite Directories Navigation**: Add dedicated UI section for browsing favorites by type
- **Favorite Count Badge**: Show count of favorites in navigation
- **Bulk Favorite Operations**: Select multiple files and favorite/unfavorite at once
- **Favorite Import/Export**: Save favorite lists and share with other users
- **Smart Favorites**: Auto-favorite highly-rated or frequently played files
- **Favorite Playlists**: Create playlists specifically from favorited files
- **Search Favorites**: Add search functionality scoped to favorites only

### Summary of Open Questions

**Phase 1 (Backend - Save Favorite)**:

- Should the endpoint return the newly created favorite file information in the response?
- How should the endpoint handle attempting to favorite an already favorited file?
- Should there be rate limiting or validation on the number of favorites a user can create?

**Phase 2 (Backend - Remove Favorite)**:

- Should removing a favorite from the favorite directory automatically unfavorite the original?
- How should the system handle orphaned favorites (original file deleted)?
- Should there be a "remove all favorites" bulk operation?

**Phase 3 (Frontend - Infrastructure)**:

- Should favorite operations return the updated file or just a success indicator?
- How should optimistic UI updates be handled (update before API confirms)?
- Should there be a retry mechanism for failed favorite operations?

**Phase 4 (Frontend - Application)**:

- Should the action automatically refresh the current directory after favoriting?
- How should the store handle rapid successive favorite/unfavorite operations?
- Should favorite status be persisted across application sessions?

**Phase 5 (Frontend - UI)**:

- Should there be a tooltip showing favorite status on hover?
- Should the button show a loading spinner during operations?
- Should there be animation when toggling favorite state?
- Should the button be hidden when no file is loaded?

**Phase 6 (Frontend - E2E)**:

- Should tests validate favorite file appears in favorites directory?
- Should tests cover all file types (games, music, images)?
- Should tests validate bidirectional favorite status (original ↔ favorite)?
- What level of error scenario coverage is needed?

---

## 💡 Reference Implementations

This feature follows established patterns from existing functionality. Key reference implementations:

**Backend Patterns**:

- [`LaunchFileEndpoint.cs`](../../../../../TeensyRom.Api/Endpoints/Player/LaunchFile/LaunchFileEndpoint.cs) - Endpoint structure, device validation, storage service usage
- [`LaunchFileTests.cs`](../../../../../TeensyRom.Api.Tests.Integration/LaunchFileTests.cs) - Integration test patterns, fixture usage, assertions
- [`CachedStorageService.SaveFavorite()`](../../../../../TeensyRom.Core.Storage/CachedStorageService.cs) - Complete favorite logic to be extracted

**Frontend Patterns**:

- [`storage.service.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.ts) - Infrastructure service structure
- [`device.service.ts`](../../../libs/infrastructure/src/lib/device/device.service.ts) - Alert service integration pattern and error handling
- [`api-error.utils.ts`](../../../libs/infrastructure/src/lib/error/api-error.utils.ts) - Error message extraction utility
- [`domain.mapper.ts`](../../../libs/infrastructure/src/lib/domain.mapper.ts) - API to domain mapping patterns
- [`storage-store.ts`](../../../libs/application/src/lib/storage/storage-store.ts) - Store action patterns
- [`player-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.ts) - Toolbar button integration
- [`icon-button.component.ts`](../../../libs/ui/components/src/lib/icon-button/icon-button.component.ts) - Reusable button component

**Testing Patterns**:

- [`device-connection.cy.ts`](../../../apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts) - E2E test organization
- [`device.interceptors.ts`](../../../apps/teensyrom-ui-e2e/src/support/interceptors/device.interceptors.ts) - Interceptor patterns
- [`devices.fixture.ts`](../../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/devices.fixture.ts) - Fixture data structure
