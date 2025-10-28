# Phase 1: E2E Tests for Player Toolbar Favorite Functionality

## 🎯 Objective

Implement comprehensive E2E tests for the player toolbar favoriting feature using deep linking, proper fixtures, and established testing patterns. Tests will verify user-observable behaviors when favoriting and unfavoriting files from the player toolbar, ensuring proper UI updates, API interactions, and cross-component state synchronization.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check boxes as you read them.

**Feature Documentation:**
- [] [Player Toolbar Actions Component](../../../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar-actions/player-toolbar-actions.component.ts) - Favorite button implementation with toggle logic
- [] [Player Toolbar Template](../../../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar-actions/player-toolbar-actions.component.html) - UI selectors and icon states
- [] [Storage Store Documentation](../../../libs/application/src/lib/storage/) - State management for favorite operations
- [] [Storage Store Actions](../../../libs/application/src/lib/storage/actions/) - saveFavorite and removeFavorite implementations
- [] [Mock Filesystem Documentation](../../../apps/teensyrom-ui-e2e/src/support/test-data/mock-filesystem/) - Test data infrastructure

**Standards & Guidelines:**
- [] [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Testing architecture and patterns
- [] [Deep Linking Tests Example](../../../apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts) - Reference for navigation and file launch patterns
- [] [E2E_INTERCEPTORS.md](../../../apps/teensyrom-ui-e2e/src/support/interceptors/E2E_INTERCEPTORS.md) - API mocking approach
- [] [E2E_CONSTANTS.md](../../../apps/teensyrom-ui-e2e/src/support/constants/E2E_CONSTANTS.md) - Centralized constants usage
- [] [API Client Documentation](../../../libs/data-access/api-client/) - Endpoint contracts

**Testing Infrastructure:**
- [] [Storage Interceptors](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) - Save/remove favorite interceptors
- [] [API Constants](../../../apps/teensyrom-ui-e2e/src/support/constants/api.constants.ts) - Endpoint patterns and aliases
- [] [Selector Constants](../../../apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts) - UI element selectors
- [] [Storage Constants](../../../apps/teensyrom-ui-e2e/src/support/constants/storage.constants.ts) - Test files and paths

---

## 📂 File Structure Overview

> Provide a clear file tree showing new files (✨) and modified files (📝) to understand implementation scope.

```
apps/teensyrom-ui-e2e/
├── src/e2e/favorites/
│   ├── favorite-functionality.cy.ts        ✨ New - Main test suite
│   └── test-helpers.ts                    ✨ New - Favorite-specific test helpers
├── src/support/test-data/fixtures/
│   └── favorites.fixture.ts              ✨ New - Favorite-specific test fixtures
├── src/support/test-data/mock-filesystem/
│   └── favorite-fs-setup.ts             ✨ New - Filesystem setup for favorites

```

---

## 📋 Implementation Guidelines

> **IMPORTANT - Testing Policy:**
> - See [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) for behavioral testing guidance

> **IMPORTANT - Progress Tracking:**
> - **Mark checkboxes ✅ as you complete each subtask**
> - Update progress throughout implementation, not just at end
> - This helps track what's done and what remains

---

<details open>
<summary><h3>Task 1: Navigate to File and Verify Initial Favorite State</h3></summary>

**Purpose**: Establish baseline behavior for loading a non-favorited file via deep link and verifying the initial empty heart icon state.

**Related Documentation:**
- [Deep Link Navigation](../../../apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts) - Reference for navigation patterns using `navigateToPlayerWithParams`
- [Player Toolbar Selectors](../../../apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts) - UI selectors including `PLAYER_TOOLBAR_SELECTORS.favoriteButton` and `PLAYER_TOOLBAR_SELECTORS.favoriteIcon`

**Implementation Subtasks:**
- [ ] **Create Favorite Filesystem Setup**: Add `setupFavoriteFilesystem` function using `MockFilesystem` with `TEST_FILES.GAMES.PAC_MAN` set to `isFavorite: false`
- [ ] **Create Test Helper**: Add `navigateToFileWithDeepLink` function based on `navigateToPlayerWithParams` from deep-linking.cy.ts
- [ ] **Create Test Fixture**: Add `favoriteTestFiles` fixture with `TEST_FILES.GAMES.PAC_MAN` data structure

**Testing Subtask:**
- [ ] **Write Tests**: Test deep link navigation and verify `PLAYER_TOOLBAR_SELECTORS.favoriteIcon` contains "favorite_border"

**Key Implementation Notes:**
- Deep link format: `/?deviceId=test-device&storageType=sd&filePath=/games/file.crt` using `TeensyStorageType.Sd`
- Use `TEST_FILES.GAMES.PAC_MAN.filePath` for test file path and `TEST_FILES.GAMES.PAC_MAN.fileName` for file parameter
- Filesystem should start with non-favorited files: `isFavorite: false` in `MockFilesystem`
- Use `interceptGetDirectory({ filesystem })` and `interceptLaunchFile({ filesystem })` from storage.interceptors.ts
- Use `MOCK_SEEDS.DEFAULT` for deterministic test behavior

**Critical Types/Interfaces:**
```typescript
// MockFilesystem setup for favorite testing
function setupFavoriteFilesystem(): MockFilesystem

// Navigation parameters
interface NavigationParams {
  device: string;
  storage: TeensyStorageType;
  path: string;
  file?: string;
}

// Storage interceptors
interceptGetDirectory(options: InterceptGetDirectoryOptions)
interceptLaunchFile(options: InterceptLaunchFileOptions)
```

**Testing Focus for Task 1:**

> Focus on **behavioral testing** - what observable outcomes occur?

**Behaviors to Test:**
> - **GIVEN**: User navigates to `TEST_FILES.GAMES.PAC_MAN.filePath` via deep link
> - **WHEN**: page loads and file launches
> - **THEN**: `PLAYER_TOOLBAR_SELECTORS.favoriteIcon` contains "favorite_border" text content

**Testing Reference:**
- See [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) for behavioral testing patterns
- Use existing [deep-linking.cy.ts patterns](../../../apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts) for navigation and file launch setup
- Use [storage.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) for API mocking

</details>

---

<details open>
<summary><h3>Task 2: Favorite a File and Verify UI Updates</h3></summary>

**Purpose**: Verify the complete favoriting workflow from clicking the favorite button to observing the solid heart icon and successful API interaction.

**Related Documentation:**
- [Storage Interceptors](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) - `interceptSaveFavorite` with `InterceptSaveFavoriteOptions`
- [Mock Filesystem](../../../apps/teensyrom-ui-e2e/src/support/test-data/mock-filesystem/mock-filesystem.ts) - `saveFavorite` method implementation
- [Storage Service](../../../libs/infrastructure/src/lib/storage/storage.service.ts) - Real service implementation for reference

**Implementation Subtasks:**
- [ ] **Configure Save Favorite Interceptor**: Set up `interceptSaveFavorite({ filesystem })` before navigation
- [ ] **Create Favorite Button Helper**: Add `clickFavoriteButton()` function using `PLAYER_TOOLBAR_SELECTORS.favoriteButton` with `cy.click()`
- [ ] **Create Icon State Helper**: Add `verifyFavoriteIconState(expectedIcon: string)` function checking `PLAYER_TOOLBAR_SELECTORS.favoriteIcon` text content

**Testing Subtask:**
- [ ] **Write Tests**: Test favoriting behavior with API call waiting using `cy.wait('@saveFavorite')`

**Key Implementation Notes:**
- Use `STORAGE_ENDPOINTS.SAVE_FAVORITE.pattern` and `INTERCEPT_ALIASES.SAVE_FAVORITE` from api.constants.ts
- Verify icon content changes from "favorite_border" to "favorite" using exact text matching
- Check that `ALERT_SELECTORS.container` does NOT appear on success (no error alerts)
- Use `responseDelayMs: 500` in interceptor options to simulate network latency
- MockFilesystem.saveFavorite should return `SaveFavoriteResponse` with `favoriteFile.isFavorite: true`

**Critical Types/Interfaces:**
```typescript
// Save favorite interceptor options
interface InterceptSaveFavoriteOptions {
  filesystem?: MockFilesystem;
  errorMode?: boolean;
  responseDelayMs?: number;
}

// API response structure
interface SaveFavoriteResponse {
  message: string;
  favoriteFile: FileItemDto;
  favoritePath: string;
}
```

**Testing Focus for Task 2:**

> Focus on **behavioral testing** - what observable outcomes occur?

**Behaviors to Test:**
> - **GIVEN**: User is viewing `TEST_FILES.GAMES.PAC_MAN.filePath` with `isFavorite: false`
> - **WHEN**: `PLAYER_TOOLBAR_SELECTORS.favoriteButton` is clicked
> - **THEN**: `PLAYER_TOOLBAR_SELECTORS.favoriteIcon` contains "favorite" after API call

**Testing Reference:**
- See [storage.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) for save favorite patterns
- Use [ALERT_SELECTORS](../../../apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts) for error validation
- Reference [deep-linking.cy.ts waitForFileLaunch](../../../apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts) patterns for async waiting

</details>

---

<details open>
<summary><h3>Task 3: Unfavorite a File and Verify UI Updates</h3></summary>

**Purpose**: Verify the complete unfavoriting workflow from clicking the favorite button to observing the empty heart icon and successful API interaction.

**Related Documentation:**
- [Storage Interceptors](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) - `interceptRemoveFavorite` with `InterceptRemoveFavoriteOptions`
- [Remove Favorite Action](../../../libs/application/src/lib/storage/actions/remove-favorite.ts) - State management logic for favorites directory handling
- [API Constants](../../../apps/teensyrom-ui-e2e/src/support/constants/api.constants.ts) - Endpoint constants for remove favorite

**Implementation Subtasks:**
- [ ] **Configure Remove Favorite Interceptor**: Set up `interceptRemoveFavorite({ filesystem })` for API mocking
- [ ] **Create Unfavorite Test Helper**: Add `unfavoriteFileAndVerify()` function combining click and verification
- [ ] **Create State Transition Helper**: Add `verifyFavoriteStateChange(expectedState: string)` function to track icon changes

**Testing Subtask:**
- [ ] **Write Tests**: Test unfavoriting behavior with proper API call waiting

**Key Implementation Notes:**
- Use `STORAGE_ENDPOINTS.REMOVE_FAVORITE.pattern` and `INTERCEPT_ALIASES.REMOVE_FAVORITE`
- Verify icon content changes from "favorite" to "favorite_border" using exact text matching
- Test both scenarios: unfavoriting from regular directory and from favorites directory
- MockFilesystem.removeFavorite should handle two paths: `filePath.startsWith('/favorites/')` vs regular paths
- For favorites directory: file should be removed from listing (`removeFavorite` action in remove-favorite.ts:46)
- For regular directory: file's `isFavorite` flag should be set to `false`

**Critical Types/Interfaces:**
```typescript
// Remove favorite interceptor options
interface InterceptRemoveFavoriteOptions {
  filesystem?: MockFilesystem;
  errorMode?: boolean;
  responseDelayMs?: number;
}

// API response structure
interface RemoveFavoriteResponse {
  message: string;
}
```

**Testing Focus for Task 3:**

> Focus on **behavioral testing** - what observable outcomes occur?

**Behaviors to Test:**
> - **GIVEN**: User is viewing a favorited file (`isFavorite: true`)
> - **WHEN**: `PLAYER_TOOLBAR_SELECTORS.favoriteButton` is clicked
> - **THEN**: `PLAYER_TOOLBAR_SELECTORS.favoriteIcon` contains "favorite_border" after API call

**Testing Reference:**
- See [MockFilesystem.removeFavorite](../../../apps/teensyrom-ui-e2e/src/support/test-data/mock-filesystem/mock-filesystem.ts) for expected behavior
- Use [player toolbar selectors](../../../apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts) for UI validation
- Reference [remove-favorite.ts](../../../libs/application/src/lib/storage/actions/remove-favorite.ts) for state management understanding

</details>


<details open>
<summary><h3>Task 5: File Disappears from Favorites Directory After Unfavoriting</h3></summary>

**Purpose**: Verify cross-component behavior where unfavoriting a file from the favorites directory causes it to disappear from the listing.

**Related Documentation:**
- [Storage Interceptors](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) - Remove favorite API behavior with favorites path handling
- [Directory Files Selectors](../../../apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts) - File listing selectors
- [Remove Favorite Action](../../../libs/application/src/lib/storage/actions/remove-favorite.ts) - Logic for favorites directory handling (lines 46-79)

**Implementation Subtasks:**
- [ ] **Create Favorites Test Setup**: Add `setupFavoritedFilesInDirectory()` function with pre-populated favorites
- [ ] **Create File Removal Verification**: Add `verifyFileNotInDirectory(filePath: string)` function using file path selectors
- [ ] **Create Launch from Favorites Helper**: Add `launchFileFromFavorites()` function for realistic workflow

**Testing Subtask:**
- [ ] **Write Tests**: Test file disappearance from favorites directory after unfavoriting

**Key Implementation Notes:**
- When file path starts with `/favorites/`, remove operation should remove file from listing
- Use `DIRECTORY_FILES_SELECTORS.fileListItem` with specific file path for verification: `[data-item-path="/favorites/games/Pac-Man (J1).crt"]`
- Verify immediate UI update without page refresh (real-time state sync)
- MockFilesystem.removeFavorite should handle favorites directory differently (remove from array vs update isFavorite flag)
- Test launching from favorites first, then unfavoriting while viewing the launched file
- Use `cy.get(selector).should('not.exist')` for verification of file removal

**Critical Types/Interfaces:**
```typescript
// Favorites directory setup
function setupFavoritedFilesInDirectory(files: FileItemDto[]): MockFilesystem

// File removal verification
function verifyFileNotInDirectory(filePath: string): void

// Launch from favorites workflow
function launchFileFromFavorites(filePath: string): void
```

**Testing Focus for Task 5:**

> Focus on **behavioral testing** - what observable outcomes occur?

**Behaviors to Test:**
> - **GIVEN**: User launches file from favorites directory and file is currently playing
> - **WHEN**: file is unfavorited via player toolbar
> - **THEN**: file disappears from favorites directory listing

**Testing Reference:**
- See [MockFilesystem.removeFavorite](../../../apps/teensyrom-ui-e2e/src/support/test-data/mock-filesystem/mock-filesystem.ts) for favorites directory handling
- Use [alert selectors](../../../apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts) for error state verification
- Reference [remove-favorite.ts](../../../libs/application/src/lib/storage/actions/remove-favorite.ts:46-79) for favorites directory logic

</details>

---

<details open>
<summary><h3>Task 6: API Error Handling and User Feedback</h3></summary>

**Purpose**: Verify proper error handling and user feedback when favorite operations fail due to API errors.

**Related Documentation:**
- [Storage Interceptors](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) - Error mode configuration with `errorMode: true`
- [Alert Selectors](../../../apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts) - Error notification UI elements
- [API Error Constants](../../../apps/teensyrom-ui-e2e/src/support/constants/api.constants.ts) - Error response patterns and `createProblemDetailsResponse`
- [Storage Store Actions](../../../libs/application/src/lib/storage/actions/) - Error state management in save-favorite.ts and remove-favorite.ts

**Implementation Subtasks:**
- [ ] **Configure Error Mode Interceptors**: Set up `interceptSaveFavorite({ errorMode: true })` and `interceptRemoveFavorite({ errorMode: true })`
- [ ] **Create Error Verification Helper**: Add `verifyErrorAlertDisplayed(expectedMessage: string)` function using `ALERT_SELECTORS.container`
- [ ] **Create Error State Test Setup**: Add `setupFavoriteErrorScenario()` function for consistent error testing

**Testing Subtask:**
- [ ] **Write Tests**: Test error handling and user feedback display for both save and remove operations

**Key Implementation Notes:**
- Use `createProblemDetailsResponse(502, 'Failed to save favorite. Please try again.')` for proper error response format
- Verify alert contains error message from interceptor using `ALERT_SELECTORS.messageInContainer`
- Ensure favorite state remains unchanged on error (icon doesn't change from original state)
- Test both save favorite and remove favorite error scenarios
- Use `cy.wait('@saveFavorite')` to ensure API call completes before checking for error
- Verify favorite button is re-enabled after error (not stuck in disabled state)

**Critical Types/Interfaces:**
```typescript
// Error interceptor configuration
interface ErrorInterceptorOptions extends InterceptSaveFavoriteOptions {
  errorMode: true;
}

// Error verification function
function verifyErrorAlertDisplayed(expectedMessage: string): void

// Problem details response
function createProblemDetailsResponse(statusCode: number, title: string): ProblemDetailsResponse
```

**Testing Focus for Task 6:**

> Focus on **behavioral testing** - what observable outcomes occur?

**Behaviors to Test:**
> - **GIVEN**: API returns error when favoriting (`errorMode: true`)
> - **WHEN**: user clicks favorite button
> - **THEN**: error alert appears with failure message
> - **AND**: favorite icon state remains unchanged

**Testing Reference:**
- See [storage.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) for error mode implementation
- Use [createProblemDetailsResponse](../../../apps/teensyrom-ui-e2e/src/support/constants/api.constants.ts) for error response format
- Reference [save-favorite.ts error handling](../../../libs/application/src/lib/storage/actions/save-favorite.ts:92-101) for state management

</details>


## 🗂️ Files Modified or Created

> List all files that will be changed or created during this phase with full relative paths from project root.

**New Files:**
- `apps/teensyrom-ui-e2e/src/e2e/favorites/favorite-functionality.cy.ts`
- `apps/teensyrom-ui-e2e/src/e2e/favorites/test-helpers.ts`
- `apps/teensyrom-ui-e2e/src/support/test-data/fixtures/favorites.fixture.ts`
- `apps/teensyrom-ui-e2e/src/support/test-data/mock-filesystem/favorite-fs-setup.ts`
- `apps/teensyrom-ui-e2e/docs/features/favorites/FAVORITES_E2E_PLAN.md`

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
> - **Favor behavioral testing** - test what users observe, not how it's implemented
> - **Test as you go** - tests are integrated into each task's subtasks, not deferred to the end
> - **Test through public APIs** - components, stores, services should be tested through their public interfaces
> - **Mock at boundaries** - mock external dependencies (HTTP, infrastructure services), not internal logic

> **Reference Documentation:**
> - **All tasks**: [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Core behavioral testing approach
> - **Storage Interceptors**: [storage.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/storage.interceptors.ts) - API mocking patterns
> - **Player Interceptors**: [player.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/player.interceptors.ts) - File launch mocking

### Where Tests Are Written

**Tests are embedded in each task above** with:
- **Testing Subtask**: Checkbox in task's subtask list (e.g., "Write Tests: Test behaviors for this task")
- **Testing Focus**: "Behaviors to Test" section listing observable outcomes with GWT format
- **Testing Reference**: Links to relevant testing documentation

**Complete each task's testing subtask before moving to the next task.**

### Test Execution Commands

**Running Tests:**
```bash
# Run all E2E tests
pnpm nx e2e teensyrom-ui-e2e

# Run specific favorite tests
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/favorites/favorite-functionality.cy.ts"

# Run in headed mode for debugging
pnpm nx e2e teensyrom-ui-e2e --headed --spec="src/e2e/favorites/favorite-functionality.cy.ts"
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Tests follow E2E testing patterns from [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)
- [ ] No hardcoded values - all use centralized constants

**Testing Requirements:**
- [ ] All testing subtasks completed within each task
- [ ] All behavioral test scenarios verified with GWT format
- [ ] Each task tests exactly one behavior at a time
- [ ] Tests written alongside implementation (not deferred)
- [ ] All tests passing with no failures

**Quality Checks:**
- [ ] No TypeScript errors in test files
- [ ] Cypress tests follow established patterns
- [ ] Proper use of centralized constants and selectors
- [ ] Test fixtures are deterministic and reusable

**Documentation:**
- [ ] This plan document created and approved
- [ ] Test file comments explain complex scenarios
- [ ] Helper functions documented with JSDoc

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] Tests can be executed in CI/CD pipeline
- [ ] No known bugs or issues with favorite functionality

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Behavioral Testing Focus**: Chosen GWT format over technical implementation testing to focus on user-observable outcomes
- **One Behavior Per Task**: Each task focuses on a single specific behavior to ensure clear, maintainable tests
- **Deep Link Approach**: Using deep linking to navigate directly to files for test isolation and speed
- **Mock Filesystem Integration**: Leveraging existing `MockFilesystem` for deterministic state management
- **Constants-Only Policy**: Strict adherence to no-hardcoding rule using existing constants infrastructure

### Implementation Constraints

- **No Hardcoded Values**: All paths, selectors, and test data must use centralized constants
- **Cypress Pattern Adherence**: Must follow established patterns from [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)
- **Behavioral Focus**: Tests verify what users observe, not internal implementation details
- **Single Responsibility**: Each test focuses on one specific behavior with clear GWT structure

### Key Implementation Details

**Player Toolbar Component Analysis:**
- Uses `data-testid="favorite-button"` for button selection
- Icon toggles between "favorite_border" (empty) and "favorite" (filled) mat-icon
- Implements `toggleFavorite()` method with pessimistic update pattern
- Button disabled during operation with `isFavoriteOperationInProgress()`
- Uses `PLAYER_CONTEXT` and `STORAGE_STORE` for state management

**Storage Service Integration:**
- `saveFavorite()` returns `FileItemDto` with updated `isFavorite: true`
- `removeFavorite()` returns `void` but updates filesystem state
- Both methods handle `alertService.success()` and `alertService.error()` calls
- Uses `DomainMapper.toApiStorageType()` for storage type conversion

**MockFilesystem Behavior:**
- `saveFavorite(filePath)` creates file in appropriate favorites directory
- `removeFavorite(filePath)` handles both regular and favorites directory paths
- Maintains `isFavorite` flag on files for UI state consistency
- Returns proper API response shapes (`SaveFavoriteResponse`, `RemoveFavoriteResponse`)

**Interceptor Patterns:**
- `interceptSaveFavorite({ filesystem, errorMode, responseDelayMs })` for API mocking
- `interceptRemoveFavorite({ filesystem, errorMode, responseDelayMs })` for removal testing
- Uses `STORAGE_ENDPOINTS.SAVE_FAVORITE.pattern` and `STORAGE_ENDPOINTS.REMOVE_FAVORITE.pattern`
- Aliases as `@saveFavorite` and `@removeFavorite` for test waiting

### Future Enhancements

- **Multi-Device Testing**: Extend tests to cover favorite independence across multiple devices
- **Performance Testing**: Add tests for large numbers of favorited files
- **Accessibility Testing**: Include keyboard navigation and screen reader verification for favorite functionality
- **Cross-Browser Testing**: Extend test coverage to different browsers if needed

### External References

- [Player Toolbar Component](../../../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar-actions/player-toolbar-actions.component.ts) - Implementation details
- [Storage Store Actions](../../../libs/application/src/lib/storage/actions/) - Favorite state management
- [Mock Filesystem Implementation](../../../apps/teensyrom-ui-e2e/src/support/test-data/mock-filesystem/mock-filesystem.ts) - Test data infrastructure
- [Deep Linking Test Patterns](../../../apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts) - Navigation and async handling patterns

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery 1**: Importance of proper favorites directory path handling in MockFilesystem
- **Discovery 2**: Need for proper deep link URL encoding in test helpers
- **Discovery 3**: Cross-component state synchronization complexity between player and storage
- **Discovery 4**: Pessimistic update pattern in player toolbar affects testing approach
- **Discovery 5**: Error handling requires both API interceptor testing and UI alert verification

</details>