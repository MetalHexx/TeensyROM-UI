# Player URL Routing & Deep Linking System

**Project Overview**: Enable URL-based navigation and deep linking for the TeensyROM player, allowing users to share and bookmark specific files, directories, and storage locations across multi-device configurations. The system provides progressive enhancement from simple `/player` navigation to fully parameterized URLs that automatically load specific content.

**Standards Documentation**:
- **Architecture Overview**: [OVERVIEW_CONTEXT.md](../../OVERVIEW_CONTEXT.md)
- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../STORE_TESTING.md)
- **State Standards**: [STATE_STANDARDS.md](../../STATE_STANDARDS.md)
- **E2E Testing**: [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)

---

## 🎯 Project Objective

Create a comprehensive URL routing system that enables users to navigate directly to specific content in the TeensyROM player through browser URLs. The system supports progressive specificity—from basic `/player` navigation showing all devices, to fully parameterized URLs like `/player?device=X&storage=SD&path=/games&file=sonic.prg` that automatically load and launch specific files.

**User Value**: Users can bookmark favorite files, share direct links to content with friends, and use browser back/forward buttons to navigate through directories. Multi-device scenarios gracefully fallback when specific devices aren't available, ensuring shared URLs work reliably across different hardware configurations. The system feels like a native web application with full URL state synchronization.

**System Benefits**: Clean separation of routing concerns using Angular route guards, maintaining Clean Architecture principles with routing logic isolated from player components. The `PlayerContextService` facade coordinates state initialization, keeping the player view component focused on presentation while the application layer handles route-driven workflows.

---

## 📋 Implementation Phases

<details open>
<summary><h3>Phase 1: Route Resolver with Direct Store Coordination ✅ COMPLETE</h3></summary>

### Objective

Create a simple, non-blocking route resolver that coordinates DeviceStore and StorageStore initialization, then handles deep linking for directory navigation and optional file launching. All logic lives in the resolver—no complex actions needed in stores.

### Key Deliverables

- [x] Route resolver with query parameter parsing (`device`, `storage`, `path`, `file`)
- [x] Non-blocking fire-and-forget initialization pattern
- [x] Wait for DeviceStore initialization before storage setup
- [x] Direct StorageStore method calls (no special route actions)
- [x] Deep linking with directory navigation and file auto-launch
- [x] Integration tests verifying resolver behavior
- [x] Documentation of resolver architecture

### Implementation Summary

**Simple Resolver Pattern:**
- Resolver returns immediately (non-blocking)
- Background initialization: Wait for DeviceStore → Initialize all storage → Handle deep linking
- Direct store method calls: `initializeStorage()`, `navigateToDirectory()`
- PlayerContextService for file launching: `launchFileWithContext()`
- No dedicated store actions—keeps business logic minimal

**Completed Tasks:**
1. ✅ Created `playerRouteResolver` with fire-and-forget pattern
2. ✅ Implemented `waitForDeviceStoreInitialization()` polling function
3. ✅ Implemented `initializeAllDeviceStorage()` for all devices
4. ✅ Implemented `initDeeplinking()` for route parameter handling
5. ✅ Wired resolver to `/player` route in app.routes.ts
6. ✅ All 27+ tests passing (component + resolver tests)
7. ✅ Documented architecture in `PLAYER_ROUTING.md`

### Testing Focus

**What to Test:**
- Route parameter parsing (device, storage, path, file)
- DeviceStore initialization wait logic
- Storage initialization for all connected devices
- Deep link navigation to specified directory
- File auto-launch when file parameter provided
- Missing/invalid parameter handling
- E2E testing of complete workflows

**Testing Approach:**
- Unit tests for resolver helper functions
- Integration tests with real stores + mocked services
- E2E tests validating browser navigation
- Manual verification with various URL combinations

</details>

---

<details open>
<summary><h3>Phase 2: File Auto-Launch from URL ✅ COMPLETE</h3></summary>

### Objective

Enable direct file launching via URL parameters. Already implemented in Phase 1 resolver—file parameter triggers directory load followed by automatic file launch.

### Implementation Summary

**File Launch Pattern:**
- Optional `file` query parameter in URL
- Resolver waits for directory to load
- Finds file in directory's files array
- Launches via `PlayerContextService.launchFileWithContext()`
- Handles file-not-found with warning log

**Already Complete:**
- ✅ File parameter parsing in `initDeeplinking()`
- ✅ Directory pre-loading before file lookup
- ✅ File launching with full context (directory, files array)
- ✅ Error handling for missing files

### Testing Focus

- File auto-launch with valid file parameter
- File not found handling (warning logged, directory shows)
- File metadata fully loaded before launch
- Launch mode set to `LaunchMode.Directory`

</details>

---

<details open>
<summary><h3>Phase 3: URL Update on File Launch</h3></summary>

### Objective

Update browser URL when a file is launched through `PlayerContextService.launchFileWithContext()`. This enables sharing of currently playing files and maintains URL accuracy without complex bidirectional sync.

### Key Deliverables

- [ ] Inject Angular `Location` service in `PlayerContextService`
- [ ] Update URL after successful file launch in `launchFileWithContext()`
- [ ] Build URL with device, storage, path, and file parameters
- [ ] Use `location.go()` to update URL without triggering navigation
- [ ] Handle URL encoding for file names and paths

### Implementation Summary

**URL Update Pattern:**
- After file launches successfully (no error)
- Extract current file info from store
- Build query parameters: `?device=X&storage=SD&path=/dir&file=filename`
- Call `location.go('/player', queryParams)` to update URL silently
- No navigation triggered—just updates browser URL bar and history

**Implementation Tasks:**
1. Import `Location` from `@angular/common` in `PlayerContextService`
2. Inject `Location` service in constructor
3. Add `updateUrlForLaunchedFile(deviceId: string)` private method
4. Call from `launchFileWithContext()` after successful launch (after error check)
5. Call from navigation methods (`next()`, `previous()`, `navigateToHistoryPosition()`) after successful launch
6. Handle URL encoding for special characters in paths and file names

**Key Implementation Notes:**
- Use `location.go()` not `router.navigate()` (avoids re-triggering resolver)
- Only update URL on successful launches (check for errors first)
- URL encoding: Angular handles encoding automatically in query params
- Keep method simple—no debouncing needed for file launches

</details>

---

<details open>
<summary><h3>Phase 4: Cypress E2E Testing</h3></summary>

### Objective

Create simple Cypress E2E tests validating resolver behavior and file launching through real browser interactions. Focus on core deep linking scenarios.

### Key Deliverables

- [ ] E2E test for directory navigation via URL (without file parameter)
- [ ] E2E test for file auto-launch via URL (with file parameter)
- [ ] E2E test for missing parameters (player toolbar not displayed)
- [ ] E2E test for URL update after file launch (Phase 3 functionality)

### Test Scenarios

**Scenario 1: Directory Navigation Without File Parameter**
```gherkin
Given a user navigates to /player?device=teensy-01&storage=SD&path=/games
When the page loads
And the file list should be visible
And no file should be launched
```

**Scenario 2: File Auto-Launch With File Parameter**
```gherkin
Given a user navigates to /player?device=teensy-01&storage=SD&path=/games&file=sonic.prg
When the page loads
Then the current file name "sonic.prg" should be displayed
And the file should be launched in the player
```

**Scenario 3: Missing Parameters - Player Toolbar Not Displayed**
```gherkin
Given a user navigates to /player with no query parameters
When the page loads
Then the player toolbar should not be displayed
And no file launched.
```

**Scenario 4: URL Updates After File Launch (Phase 3)**
```gherkin
Given a user is viewing /player?device=teensy-01&storage=SD&path=/games
When the user double clicks a file "sonic.prg" in the file list
Then the file launches via PlayerContextService
And all four parameters are present (device, storage, path, file)
```

**Scenario 5: Next File Updates URL (Phase 3)**
```gherkin
Given a file is playing at /player?device=teensy-01&storage=SD&path=/games&file=sonic.prg
When the user clicks "Next" to play the next file
Then the next file launches
And all four parameters are present (device, storage, path, file)
And browser history records the navigation
```

**Scenario 5: Previous File Updates URL (Phase 3)**
```gherkin
Given a file is playing at /player?device=teensy-01&storage=SD&path=/games&file=sonic.prg
When the user clicks "Previous" to play the next file
Then the next file launches
And all four parameters are present (device, storage, path, file)
And browser history records the navigation
```

**Scenario 5: Random Launch Updates URL (Phase 3)**
```gherkin
Given no file playing
When the user clicks the "Random Button" 
Then the next random file launches
And all four parameters are present (device, storage, path, file)
And browser history records the navigation
```

### Implementation Notes

- Use existing Cypress test helpers for device setup
- Add `data-cy` attributes if needed for selectors
- Keep tests simple—focus on URL and basic UI state
- No need for complex assertions—verify core behavior only
- Run tests with real backend or mocked API responses

</details>


<details open>
<summary><h2>🏗️ Architecture Overview</h2></summary>

### Key Design Decisions

- **Non-Blocking Resolver Pattern**: Resolver returns immediately, initialization continues in background via fire-and-forget orchestrator pattern. Enables instant navigation with progressive UI updates as data loads.

- **Query Parameters Over Route Segments**: URLs use query parameters (`?device=X&storage=SD&path=/games`) instead of path segments to handle dynamic device counts, optional parameters, and directory paths containing slashes.

- **Direct Store Coordination**: Resolver directly calls store methods (`initializeStorage()`, `navigateToDirectory()`) without intermediate actions or context service methods. Keeps business logic simple and centralized in resolver.

- **DeviceStore Wait Pattern**: Resolver polls `DeviceStore.hasInitialised()` until true before proceeding with storage initialization. Ensures devices are discovered before attempting storage setup.

- **Progressive Enhancement**: URLs work at multiple specificity levels—from bare `/player` showing all devices, to fully parameterized URLs auto-launching specific files.

### Integration Points

- **DeviceStore**: Resolver waits for `hasInitialised()` signal via polling loop. Gets devices from `devices()` signal for storage initialization.

- **StorageStore**: Resolver directly calls `initializeStorage()` and `navigateToDirectory()` methods. No special route actions needed.

- **PlayerContextService**: Used only for file launching via `launchFileWithContext()`. Maintains existing file launch patterns.

- **Player View Component**: Component simplified to pure rendering—no initialization logic. All setup handled by resolver before component loads.

- **Angular Router**: Route resolver runs before component loads, enabling background initialization while component renders loading states.

</details>

---

<details open>
<summary><h2>🧪 Testing Strategy</h2></summary>

### Resolver Integration Tests

Test resolver behavior with real stores and mocked infrastructure services:

- [ ] Parse all query parameters correctly (device, storage, path, file)
- [ ] Wait for DeviceStore initialization before proceeding
- [ ] Initialize storage for all connected devices
- [ ] Navigate to specified directory when path provided
- [ ] Launch file when file parameter provided
- [ ] Handle missing parameters (no deep linking)
- [ ] Handle invalid device ID (fallback to first device)
- [ ] Handle invalid storage type (fallback to first available)
- [ ] Handle missing file (log warning, show directory)
- [ ] URL encoding/decoding (slashes in paths)
- [ ] Non-blocking pattern (resolver returns immediately)

### E2E Tests (End-to-End - Full System)

- [ ] Direct URL navigation to specific directories loads correctly
- [ ] File auto-launch from URL parameters launches correct file
- [ ] Browser back button navigates through directory history
- [ ] UI directory navigation updates URL in real-time (Phase 3)
- [ ] Invalid device parameter gracefully handled
- [ ] Multi-device scenarios work correctly
- [ ] Missing parameters show default state (all devices)
- [ ] URL encoding works for special characters in paths

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

**Phase 1 & 2 (Complete):**
- [x] Users can navigate to `/player?device=X&storage=SD&path=/games` to load specific directories
- [x] Users can share URLs like `/player?storage=USB&path=/music&file=song.sid` that auto-launch files
- [x] URL parameters correctly parsed and handled by resolver
- [x] DeviceStore initialization coordinated before storage setup
- [x] Storage initialized for all connected devices (SD and USB where available)
- [x] Deep linking navigates to specified directory
- [x] File parameter triggers automatic file launch with full context
- [x] Missing parameters handled gracefully (no deep linking)
- [x] Component remains simple with no initialization logic
- [x] Non-blocking pattern maintains responsive UI

**Phase 3 (In Progress):**
- [ ] URL updates when file launched from UI
- [ ] PlayerContextService updates browser URL/history after successful launch
- [ ] URL reflects currently playing file

**Phase 4 (Future):**
- [ ] E2E test for directory navigation (without file parameter)
- [ ] E2E test for file auto-launch (with file parameter)
- [ ] E2E test for missing parameters (default state)
- [ ] E2E test for URL update after file launch

</details>

---

<details open>
<summary><h2>📚 Related Documentation</h2></summary>

- **Architecture Overview**: [OVERVIEW_CONTEXT.md](../../OVERVIEW_CONTEXT.md)
- **Player Context Service**: [`player-context.service.ts`](../../../libs/application/src/lib/player/player-context.service.ts)
- **Storage Store**: [`storage-store.ts`](../../../libs/application/src/lib/storage/storage-store.ts)
- **Player Store**: [`player-store.ts`](../../../libs/application/src/lib/player/player-store.ts)
- **Storage Store Favorites Tests**: [`storage-store.favorites.spec.ts`](../../../libs/application/src/lib/storage/storage-store.favorites.spec.ts)
- **Player Context Favorites Tests**: [`player-context-favorite.service.spec.ts`](../../../libs/application/src/lib/player/player-context-favorite.service.spec.ts)
- **Store Testing Methodology**: [STORE_TESTING.md](../../STORE_TESTING.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **E2E Testing Guide**: [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)
- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)

</details>

---

<details open>
<summary><h2>📝 Notes</h2></summary>

### Design Considerations

- **Non-Blocking Resolver**: Resolver returns immediately, initialization continues in background. Enables instant page render with progressive loading as stores populate.

- **DeviceStore First**: Resolver waits for DeviceStore initialization (polls `hasInitialised()`) before proceeding. Ensures device discovery completes before storage setup attempts.

- **Direct Store Calls**: No intermediate actions or facade methods—resolver directly calls `storageStore.initializeStorage()` and `storageStore.navigateToDirectory()`. Keeps logic simple and centralized.

- **All Devices Initialized**: Resolver initializes storage for all connected devices (both SD and USB where available), not just deep link target. Provides complete state for UI.

- **Fire-and-Forget Pattern**: `initPlayer()` orchestrator called without await—returns immediately while background work continues asynchronously.

- **URL Encoding**: Browser automatically encodes/decodes slashes in path parameters (`%2F`). Angular handles this transparently.

### Testing Philosophy

Following simplified testing approach:

**Resolver Integration Tests:**
- Test resolver helper functions with real stores + mocked infrastructure
- Mock `IStorageService`, `IDeviceService`, `IPlayerService` at boundaries
- Assert on store state after resolver execution
- Cover parameter parsing, initialization sequencing, deep linking

**E2E Tests:**
- Validate complete browser navigation workflows
- Test URL parameter combinations
- Verify UI responds correctly to route changes
- Cover edge cases (missing params, invalid values)

**No Complex Store Actions:**
- No dedicated `storage-store.routing.spec.ts` file needed
- No special route actions in stores
- Testing focused on resolver coordination logic only

</details>

---

## 💡 Phase 1 & 2 Implementation Summary

### Completed Implementation

**1. Route Resolver (`libs/app/navigation/src/lib/player-route.resolver.ts`)** ✅
   - Non-blocking ResolveFn pattern (returns immediately)
   - `playerRouteResolver`: Kicks off background initialization
   - `waitForDeviceStoreInitialization()`: Polls DeviceStore until ready
   - `initPlayer()`: Orchestrator coordinating initialization sequence
   - `initializeAllDeviceStorage()`: Initializes SD/USB for all devices
   - `initDeeplinking()`: Handles route parameters for navigation + file launch

**2. Player View Component** ✅
   - Simplified to 19-line render-only component
   - All initialization moved to resolver
   - Component just renders state from stores

**3. Route Configuration** ✅
   - Resolver wired to `/player` route in `app.routes.ts`
   - Runs before component loads

**4. Documentation** ✅
   - Architecture documented in `PLAYER_ROUTING.md`
   - Deep linking examples with clickable URLs
   - References to resolver and component files

### What Works Now

- ✅ Navigate to `/player?device=X&storage=SD&path=/games` loads directory
- ✅ Navigate to `/player?device=X&storage=SD&path=/music&file=song.sid` auto-launches file
- ✅ Missing parameters handled gracefully (no deep linking)
- ✅ All devices initialized for complete UI state
- ✅ Non-blocking pattern maintains responsive UI
- ✅ URL encoding/decoding works transparently

### Next Steps (Future Phases)

**Phase 3: URL Update on File Launch**
- Inject Angular Location service in PlayerContextService
- Update URL after successful file launches
- Build query parameters with device, storage, path, file
- Use `location.go()` to update URL without re-triggering resolver
- No unit tests needed—simple URL update logic

**Phase 4: Cypress E2E Testing**
- Test directory navigation via URL (without file parameter)
- Test file auto-launch via URL (with file parameter)
- Test missing parameters (default state)
- Test URL update after file launch from UI
- Keep tests simple—focus on core behaviors

---

**Phase 1 & 2 are complete and working. Phase 3 & 4 ready to implement.**
