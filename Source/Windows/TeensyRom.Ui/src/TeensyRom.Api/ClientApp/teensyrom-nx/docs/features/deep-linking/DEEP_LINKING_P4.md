# Phase 4: Deep Linking E2E Tests

## 🎯 Objective

Create Cypress E2E tests validating deep linking functionality (Phases 1-3) through real browser interactions. Tests will verify route resolver behavior, file auto-launch, and URL updates using the established three-layer testing pattern (fixtures → interceptors → tests).

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [Deep Linking Plan](./DEEP_LINKING_PLAN.md) - High-level feature plan and architecture
- [ ] [Player Route Resolver](../../../libs/app/navigation/src/lib/player-route.resolver.ts) - Resolver implementation
- [ ] [Player Context Service](../../../libs/application/src/lib/player/player-context.service.ts) - Service with URL updates

**E2E Testing Standards:**
- [ ] [E2E Tests Overview](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Testing architecture and patterns
- [ ] [E2E Fixtures](../../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/E2E_FIXTURES.md) - Fixture reference
- [ ] [E2E Interceptors](../../../apps/teensyrom-ui-e2e/src/support/interceptors/E2E_INTERCEPTORS.md) - Interceptor reference
- [ ] [Favorite Operations Tests](../../../apps/teensyrom-ui-e2e/src/e2e/storage/favorite-operations.cy.ts) - Similar pattern example

**General Standards:**
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices

---

## 📂 File Structure Overview

> New files to create following the established three-layer testing pattern.

```
apps/teensyrom-ui-e2e/src/e2e/player/
├── deep-linking.cy.ts                    ✨ New - Main test spec
└── test-helpers.ts                       ✨ New - Helper functions and selectors

apps/teensyrom-ui-e2e/src/support/constants/
└── selector.constants.ts                 📝 Modified - Add player selectors (if needed)

libs/ui/components/src/lib/
├── icon-button/icon-button.component.*   📝 Modified - Add testId input
└── storage-item/storage-item.component.* 📝 Modified - Add testId input

libs/features/player/src/lib/player-view/player-device-container/
├── player-toolbar/player-toolbar.component.html                          📝 Modified - Add data-testid
├── player-toolbar/file-info/file-info.component.html                    📝 Modified - Add data-testid
├── storage-container/directory-files/directory-files.component.html     📝 Modified - Add data-testid
└── storage-container/directory-files/file-item/file-item.component.html 📝 Modified - Pass data-testid
```

---

<details open>
<summary><h3>Task 0: Add data-testid Attributes to Components</h3></summary>

**Purpose**: Add data-testid attributes to UI components and templates to enable reliable E2E test selectors.

**Related Documentation:**
- [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Selector strategy guidelines
- [Coding Standards](../../CODING_STANDARDS.md) - Component input patterns

**Implementation Subtasks:**
- [ ] **Update IconButtonComponent**: Add optional `testId` input and bind to button element
- [ ] **Update StorageItemComponent**: Add optional `testId` input and bind to container div
- [ ] **Update PlayerToolbarComponent**: Add data-testid="player-toolbar" to container
- [ ] **Update FileInfoComponent**: Add data-testid="current-file-info" to container div
- [ ] **Update DirectoryFilesComponent**: Add data-testid="directory-files-container" to wrapper div
- [ ] **Update FileItemComponent**: Pass data-testid to lib-storage-item with file name

### Files and Elements Requiring data-testid

**1. IconButtonComponent** (`libs/ui/components/src/lib/icon-button/`)
- **TypeScript**: Add optional `testId` input signal
- **HTML**: Bind `testId` input to the `<button>` element's `data-testid` attribute

**2. StorageItemComponent** (`libs/ui/components/src/lib/storage-item/`)
- **TypeScript**: Add optional `testId` input signal
- **HTML**: Bind `testId` input to the root `<div class="storage-item">` element's `data-testid` attribute

**3. PlayerToolbarComponent** (`libs/features/player/src/lib/player-view/player-device-container/player-toolbar/`)
- **HTML**: Add `testId` prop to all `<lib-icon-button>` components in all 3 layout sections (desktop, tablet, mobile):
  - Previous button: `testId="player-previous-button"`
  - Play/Pause button: `testId="player-play-pause-button"`
  - Stop button: `testId="player-stop-button"`
  - Next button: `testId="player-next-button"`

**4. FileInfoComponent** (`libs/features/player/src/lib/player-view/player-device-container/player-toolbar/file-info/`)
- **HTML**: Add `data-testid="current-file-info"` to the root `<div class="file-info">` element

**5. DirectoryFilesComponent** (`libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/`)
- **HTML**: Add `data-testid="directory-files-container"` to the root `<div class="directory-files">` element

**6. FileItemComponent** (`libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/file-item/`)
- **HTML**: Pass `[testId]="'file-item-' + fileItem().name"` to the `<lib-storage-item>` component (creates unique IDs like `file-item-Pac-Man (J1).crt`)

**7. RandomRollButtonComponent** (already has data-testid in FilterToolbarComponent)
- **No changes needed**: Already has `data-testid="random-launch-button"` in parent template

</details>

---

<details open>
<summary><h3>Task 1: Create Player Test Helpers</h3></summary>

**Purpose**: Centralize player-specific selectors, navigation helpers, and assertion utilities for reuse across tests.

**Related Documentation:**
- [Storage Test Helpers](../../../apps/teensyrom-ui-e2e/src/e2e/storage/test-helpers.ts) - Similar pattern example
- [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Helper function patterns

**Implementation Subtasks:**
- [ ] **Create File**: `apps/teensyrom-ui-e2e/src/e2e/player/test-helpers.ts`
- [ ] **Define Selectors**: Player toolbar buttons, file list items, current file display
- [ ] **Navigation Helpers**: `navigateToPlayerWithParams()`, `loadPlayerView()`
- [ ] **Interaction Helpers**: `clickFileInDirectory()`, `clickNextButton()`, `clickPreviousButton()`, `clickRandomButton()`
- [ ] **Assertion Helpers**: `expectUrlContainsParams()`, `expectFileIsLaunched()`, `expectPlayerToolbarVisible()`, `expectNoFileLaunched()`
- [ ] **Wait Helpers**: `waitForFileToLoad()`, `waitForDirectoryLoad()` (reuse from storage helpers)

**Key Implementation Notes:**
- Follow existing test-helpers.ts pattern from storage tests
- Re-export commonly used constants from other files for convenience
- All selectors must use constants, no hardcoded strings
- Helper functions should be composable and reusable

**Critical Selectors to Define**:
```typescript
const PLAYER_SELECTORS = {
  // Player controls (using data-testid attributes)
  nextButton: '[data-testid="player-next-button"]',
  previousButton: '[data-testid="player-previous-button"]',
  playPauseButton: '[data-testid="player-play-pause-button"]',
  stopButton: '[data-testid="player-stop-button"]',
  randomButton: '[data-testid="random-launch-button"]',

  // Player info display (using data-testid attributes)
  currentFileInfo: '[data-testid="current-file-info"]',

  // File list (using data-testid attributes)
  directoryFilesContainer: '[data-testid="directory-files-container"]',
  fileListItem: (fileName: string) => `[data-testid="file-item-${fileName}"]`,
}
```

**Critical Helper Functions**:
```typescript
// Navigate to player with query parameters
export function navigateToPlayerWithParams(params: {
  device?: string;
  storage?: string;
  path?: string;
  file?: string;
}): Cypress.Chainable<Cypress.AUTWindow>;

// Assert URL contains expected parameters
export function expectUrlContainsParams(params: {
  device?: string;
  storage?: string;
  path?: string;
  file?: string;
}): void;

// Click a file in the directory listing
export function clickFileInDirectory(fileName: string): void;

// Assert a file is currently launched/playing
export function expectFileIsLaunched(fileName: string): void;

// Wait for file metadata to load after launch
export function waitForFileToLoad(): void;
```

</details>

---

<details open>
<summary><h3>Task 2: Create Deep Linking Test Spec - Phase 1 & 2 Scenarios</h3></summary>

**Purpose**: Validate route resolver behavior and file auto-launch functionality (Phases 1 & 2).

**Related Documentation:**
- [Deep Linking Plan - Phase 1 & 2](./DEEP_LINKING_PLAN.md#implementation-phases) - Requirements
- [Player Route Resolver](../../../libs/app/navigation/src/lib/player-route.resolver.ts) - Implementation under test

**Implementation Subtasks:**
- [ ] **Create Test File**: `apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts`
- [ ] **Setup beforeEach**: Mock filesystem, intercept device/storage APIs
- [ ] **Scenario 1**: Directory navigation without file parameter
- [ ] **Scenario 2**: File auto-launch with file parameter
- [ ] **Scenario 3**: Missing parameters - no deep linking

**Scenario 1: Directory Navigation Without File Parameter**
```gherkin
Given a user navigates to /player?device=teensy-01&storage=SD&path=/games
When the page loads
Then the file list displays /games directory contents
And the player toolbar is visible
And no file is launched
And the URL remains unchanged
```

**Test Implementation**:
```typescript
it('navigates to directory without launching file', () => {
  navigateToPlayerWithParams({
    device: 'teensy-01',
    storage: 'SD',
    path: '/games'
  });

  waitForDirectoryLoad();

  // Verify directory loaded
  verifyFileInDirectory('Pac-Man (J1).crt', true);
  verifyFileInDirectory('Donkey Kong (Ocean).crt', true);

  // Verify no file launched
  expectNoFileLaunched();

  // Verify URL unchanged
  expectUrlContainsParams({
    device: 'teensy-01',
    storage: 'SD',
    path: '/games'
  });
});
```

**Scenario 2: File Auto-Launch With File Parameter**
```gherkin
Given a user navigates to /player?device=teensy-01&storage=SD&path=/games&file=Pac-Man (J1).crt
When the page loads
Then the file "Pac-Man (J1).crt" is launched
And the player toolbar shows file info
And playback controls are active
And the URL contains all 4 parameters
```

**Test Implementation**:
```typescript
it('auto-launches file when file parameter provided', () => {
  navigateToPlayerWithParams({
    device: 'teensy-01',
    storage: 'SD',
    path: '/games',
    file: 'Pac-Man (J1).crt'
  });

  waitForFileToLoad();

  // Verify file launched
  expectFileIsLaunched('Pac-Man (J1).crt');
  expectPlayerToolbarVisible();

  // Verify URL contains all parameters
  expectUrlContainsParams({
    device: 'teensy-01',
    storage: 'SD',
    path: '/games',
    file: 'Pac-Man (J1).crt'
  });
});
```

**Scenario 3: Missing Parameters - No Deep Linking**
```gherkin
Given a user navigates to /player with no query parameters
When the page loads
Then the default player view displays
And no directory is pre-loaded
And no file is launched
And the player toolbar may be hidden or disabled
```

**Test Implementation**:
```typescript
it('displays default view when no parameters provided', () => {
  navigateToPlayerView(); // No params

  // Verify no deep linking occurred
  expectNoFileLaunched();

  // Verify URL has no parameters
  cy.location('search').should('eq', '');
});
```

**Testing Notes:**
- Use `createMockFilesystem(12345)` for deterministic test data
- Mock device with `singleDevice` fixture
- Intercept `GET_DIRECTORY` with filesystem
- Verify clean URL formatting (forward slashes preserved)

</details>

---

<details open>
<summary><h3>Task 3: Create Deep Linking Test Spec - Phase 3 Scenarios</h3></summary>

**Purpose**: Validate URL updates when files are launched through UI interactions (Phase 3).

**Related Documentation:**
- [Deep Linking Plan - Phase 3](./DEEP_LINKING_PLAN.md#phase-3-url-update-on-file-launch) - Requirements
- [Player Context Service](../../../libs/application/src/lib/player/player-context.service.ts) - URL update implementation

**Implementation Subtasks:**
- [ ] **Scenario 4**: URL updates when file clicked from directory
- [ ] **Scenario 5**: URL updates when next button clicked
- [ ] **Scenario 6**: URL updates when random button clicked

**Scenario 4: URL Updates After Directory File Click**
```gherkin
Given a user is viewing /player?device=teensy-01&storage=SD&path=/games
When the user clicks "Pac-Man (J1).crt" in the file list
Then the file launches via PlayerContextService
And the URL updates to include &file=Pac-Man (J1).crt
And all 4 parameters are present (device, storage, path, file)
And browser history records the navigation
```

**Test Implementation**:
```typescript
it('updates URL when file clicked from directory', () => {
  // Start at directory view
  navigateToPlayerWithParams({
    device: 'teensy-01',
    storage: 'SD',
    path: '/games'
  });

  waitForDirectoryLoad();

  // Click file to launch
  clickFileInDirectory('Pac-Man (J1).crt');
  waitForFileToLoad();

  // Verify file launched
  expectFileIsLaunched('Pac-Man (J1).crt');

  // Verify URL updated with file parameter
  expectUrlContainsParams({
    device: 'teensy-01',
    storage: 'SD',
    path: '/games',
    file: 'Pac-Man (J1).crt'
  });

  // Verify clean URL formatting (no %2F encoding)
  cy.location('search').should('include', 'path=/games');
  cy.location('search').should('not.include', '%2F');
});
```

**Scenario 5: Next File Updates URL**
```gherkin
Given a file is playing at /player?device=teensy-01&storage=SD&path=/games&file=Pac-Man (J1).crt
When the user clicks "Next File" button
Then the next file launches
And the URL updates with new file parameter
And all 4 parameters are present
And browser history records the navigation
```

**Test Implementation**:
```typescript
it('updates URL when next file button clicked', () => {
  // Start with file playing
  navigateToPlayerWithParams({
    device: 'teensy-01',
    storage: 'SD',
    path: '/games',
    file: 'Pac-Man (J1).crt'
  });

  waitForFileToLoad();

  // Click next button
  clickNextButton();
  waitForFileToLoad();

  // Verify URL updated with new file
  // Note: Exact file depends on directory order, verify pattern
  cy.location('search').should((search) => {
    expect(search).to.include('device=teensy-01');
    expect(search).to.include('storage=SD');
    expect(search).to.include('path=/games');
    expect(search).to.include('file='); // Some file name
    expect(search).to.not.include('Pac-Man'); // Changed to different file
  });

  // Verify browser history updated
  cy.go('back');
  cy.location('search').should('include', 'file=Pac-Man');
});
```

**Scenario 6: Random File Updates URL**
```gherkin
Given a user is viewing /player?device=teensy-01&storage=SD&path=/games
When the user clicks "Launch Random File" button
Then a random file launches
And the URL updates with all 4 parameters
And the file parameter matches the launched file name
And browser history records the navigation
```

**Test Implementation**:
```typescript
it('updates URL when random file button clicked', () => {
  // Start at directory view
  navigateToPlayerWithParams({
    device: 'teensy-01',
    storage: 'SD',
    path: '/games'
  });

  waitForDirectoryLoad();

  // Click random button
  clickRandomButton();
  waitForFileToLoad();

  // Verify a file was launched
  expectFileIsLaunched(); // Any file is OK

  // Verify URL updated with all 4 parameters
  cy.location('search').should((search) => {
    expect(search).to.include('device=teensy-01');
    expect(search).to.include('storage=SD');
    expect(search).to.include('path=/games');
    expect(search).to.include('file='); // Some random file
  });
});
```

**Testing Notes:**
- Verify browser history with `cy.go('back')` and `cy.go('forward')`
- Check for clean URL formatting (forward slashes preserved)
- Handle timing with proper waits after button clicks
- Mock filesystem ensures deterministic file order for next/previous

</details>

---

<details open>
<summary><h3>Task 4: Update Selector Constants (If Needed)</h3></summary>

**Purpose**: Add player-specific selectors to centralized constants if they don't exist.

**Related Documentation:**
- [E2E Constants](../../../apps/teensyrom-ui-e2e/src/support/constants/E2E_CONSTANTS.md) - Constants reference
- [Selector Constants](../../../apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts) - Existing selectors

**Implementation Subtasks:**
- [ ] **Check Existing Selectors**: Review selector.constants.ts for player controls
- [ ] **Add Missing Selectors**: Only if not already defined
- [ ] **Export from Test Helpers**: Re-export for convenience

**Selectors to Add** (if not present):
```typescript
// Player toolbar and controls
export const PLAYER_TOOLBAR_SELECTORS = {
  toolbar: '[data-testid="player-toolbar"]',
  nextButton: '[data-testid="player-next-button"]',
  previousButton: '[data-testid="player-previous-button"]',
  playPauseButton: '[data-testid="player-play-pause-button"]',
  stopButton: '[data-testid="player-stop-button"]',
  randomButton: '[data-testid="random-launch-button"]',
  currentFileInfo: '[data-testid="current-file-info"]',
  directoryFilesContainer: '[data-testid="directory-files-container"]',
} as const;

// Player file list
export const PLAYER_FILE_LIST_SELECTORS = {
  fileItem: (fileName: string) => `[data-testid="file-item-${fileName}"]`,
} as const;
```

**Implementation Notes:**
- All selectors use data-testid attributes
- If a component lacks data-testid, add it to the component (Task 0)
- Follow existing selector naming conventions
- Add to exports at bottom of file

</details>

---

## 🗂️ Files Created or Modified

**New Files:**
- `apps/teensyrom-ui-e2e/src/e2e/player/deep-linking.cy.ts` - Main test spec (6 scenarios)
- `apps/teensyrom-ui-e2e/src/e2e/player/test-helpers.ts` - Helper functions and selectors

**Modified Files:**
- `apps/teensyrom-ui-e2e/src/support/constants/selector.constants.ts` - Add player selectors (if needed)

**Reused Infrastructure:**
- `createMockFilesystem()` from `storage.generators.ts`
- `singleDevice` fixture from `devices.fixture.ts`
- `interceptFindDevices()` from `device.interceptors.ts`
- `interceptConnectDevice()` from `device.interceptors.ts`
- `interceptGetDirectory()` from `storage.interceptors.ts`

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **Test Count**: 6 E2E tests validating deep linking functionality

> **Testing Philosophy:**
> - E2E tests validate complete browser workflows (resolver → service → URL)
> - Use established three-layer pattern (fixtures → interceptors → tests)
> - Focus on user-visible behavior, not implementation details
> - Verify URL state synchronization with browser history

### Test Organization

**Phase 1 & 2 Tests (Route Resolver)**:
1. Directory navigation without file parameter
2. File auto-launch with file parameter
3. Missing parameters - default view

**Phase 3 Tests (URL Updates)**:
4. URL updates when file clicked from directory
5. URL updates when next button clicked
6. URL updates when random button clicked

### Test Coverage Areas

**Route Resolver Behavior**:
- Query parameter parsing (device, storage, path, file)
- Non-blocking initialization pattern
- Directory navigation with deep linking
- File auto-launch when file parameter provided
- Graceful handling of missing parameters

**URL Update Behavior**:
- URL updates after file launch from directory click
- URL updates after next/previous navigation
- URL updates after random file launch
- Clean URL formatting (forward slashes preserved)
- Browser history integration (back/forward buttons)

### Test Execution Commands

```bash
# Run all E2E tests
pnpm nx e2e teensyrom-ui-e2e

# Run only deep linking tests
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/player/deep-linking.cy.ts"

# Run in headed mode (see browser)
pnpm nx e2e teensyrom-ui-e2e --headed --browser=chrome --spec="src/e2e/player/deep-linking.cy.ts"

# Open Cypress Test Runner UI
pnpm nx e2e teensyrom-ui-e2e:open-cypress
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Implementation Requirements:**
- [ ] All implementation tasks completed and checked off
- [ ] Test helpers file created with all selectors and functions
- [ ] Main test spec created with all 6 scenarios
- [ ] Selector constants updated (if needed)
- [ ] Code follows [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)

**Test Execution Requirements:**
- [ ] All 6 tests passing
- [ ] No flakiness (tests pass consistently)
- [ ] No timing issues (proper waits in place)
- [ ] Tests run independently (proper beforeEach setup)
- [ ] Clean console (no errors during test execution)

**Functionality Verification:**
- [ ] Phase 1 & 2: Route resolver loads directories and launches files
- [ ] Phase 3: URL updates after file launches
- [ ] URL contains all 4 parameters when file launched
- [ ] Clean URL formatting (no %2F encoding for forward slashes)
- [ ] Browser history works (back/forward buttons)
- [ ] Missing parameters handled gracefully

**Quality Checks:**
- [ ] All selectors use constants (no hardcoded strings)
- [ ] Helper functions are reusable
- [ ] Tests follow established patterns
- [ ] Code formatting is consistent
- [ ] Comments explain complex assertions

**Documentation:**
- [ ] Inline comments added for complex test logic
- [ ] Phase 4 marked complete in [DEEP_LINKING_PLAN.md](./DEEP_LINKING_PLAN.md)
- [ ] E2E test documentation updated if needed

**Ready for Production:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Deep linking feature fully validated end-to-end

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Testing Approach

- **Fixture-Driven**: Use `createMockFilesystem()` for deterministic test data
- **Interceptor-Based**: Mock all API calls for fast, reliable tests
- **Three-Layer Pattern**: Follows established E2E architecture
- **User-Focused**: Tests validate behavior from user perspective

### Implementation Constraints

- **No Backend Required**: All API calls mocked with interceptors
- **Deterministic**: Same filesystem seed ensures consistent results
- **Independent Tests**: Each test sets up own state in beforeEach
- **Fast Execution**: No network latency, typical run time ~10-15 seconds

### Selector Strategy

**Use ONLY `data-testid` attributes.** If a component is missing a data-testid, add it to the component template (Task 0). Never use aria-labels, CSS classes, or text content matching for E2E selectors.

### Common Pitfalls to Avoid

- ❌ **Don't hardcode selectors** - Use constants from selector.constants.ts
- ❌ **Don't skip waits** - Always wait for async operations to complete
- ❌ **Don't assume order** - File order depends on mock filesystem
- ❌ **Don't check exact file names in next/previous** - Verify pattern instead
- ✅ **Do verify URL parameters** - Check all 4 are present
- ✅ **Do verify browser history** - Test back/forward navigation
- ✅ **Do verify clean URLs** - Check forward slashes preserved

### External References

- [Deep Linking Plan](./DEEP_LINKING_PLAN.md) - Overall feature architecture
- [E2E Testing Guide](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Testing patterns and best practices
- [Player Route Resolver](../../../libs/app/navigation/src/lib/player-route.resolver.ts) - Implementation under test
- [Player Context Service](../../../libs/application/src/lib/player/player-context.service.ts) - URL update logic

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

</details>

---

## 💡 Agent Implementation Guide

> **Instructions for AI agents implementing this phase**

### Implementation Order

1. **Start with Helpers**: Create test-helpers.ts with selectors and utility functions
2. **Implement Phase 1 & 2 Tests**: Directory navigation and file auto-launch (3 tests)
3. **Verify Basic Functionality**: Run tests, ensure resolver works correctly
4. **Implement Phase 3 Tests**: URL update scenarios (3 tests)
5. **Verify URL Updates**: Run tests, ensure URL synchronization works
6. **Fix Timing Issues**: Add proper waits if tests are flaky
7. **Update Constants**: Add any missing selectors to selector.constants.ts
8. **Final Validation**: Run full suite, verify all tests pass consistently

### Key Integration Points

**Test Setup Pattern** (following favorite-operations.cy.ts):
```typescript
beforeEach(() => {
  filesystem = createMockFilesystem(12345);
  interceptFindDevices({ fixture: singleDevice });
  interceptConnectDevice();
  interceptGetDirectory({ filesystem });
});
```

**Navigation Pattern**:
```typescript
// Helper function builds URL with query params
navigateToPlayerWithParams({
  device: 'teensy-01',
  storage: 'SD',
  path: '/games',
  file: 'Pac-Man (J1).crt' // optional
});
```

**URL Assertion Pattern**:
```typescript
expectUrlContainsParams({
  device: 'teensy-01',
  storage: 'SD',
  path: '/games',
  file: 'Pac-Man (J1).crt'
});

// Or manual check
cy.location('search').should('include', 'path=/games');
cy.location('search').should('not.include', '%2F'); // Clean URLs
```

### Common Pitfalls to Avoid

- ❌ **Don't navigate before interceptors registered** - Setup all mocks in beforeEach
- ❌ **Don't forget waits** - `waitForDirectoryLoad()`, `waitForFileToLoad()`
- ❌ **Don't hardcode device IDs** - Use fixture device ID or make it configurable
- ❌ **Don't assume specific file in next/previous** - Verify pattern, not exact name

### Validation Checklist

After implementing each test:
- [ ] Test passes consistently (run 3+ times)
- [ ] Selectors use constants (no hardcoded strings)
- [ ] Proper waits in place (no race conditions)
- [ ] URL assertions check all expected parameters
- [ ] Browser history verified where applicable

### Debugging Tips

**If tests are flaky**:
- Add explicit waits before assertions
- Check interceptor aliases are correct
- Verify filesystem has expected files
- Use `cy.debug()` to inspect state

**If selectors don't match**:
- Check component template has data-testid attribute
- Add data-testid to component if missing (Task 0)
- Use Cypress selector playground in Test Runner UI
- Add selectors to constants file for reuse

### Remember

- This is an **E2E validation phase** - tests must validate real browser behavior
- **Follow established patterns** from favorite-operations.cy.ts
- **Use three-layer architecture** - fixtures, interceptors, tests
- **Keep tests independent** - each test sets up own state
- **Verify user-visible behavior** - not implementation details

</details>
