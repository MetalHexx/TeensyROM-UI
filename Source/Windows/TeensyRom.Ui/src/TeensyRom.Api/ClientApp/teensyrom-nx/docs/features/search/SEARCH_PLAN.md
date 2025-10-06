# Search Feature Implementation Plan

**Project Overview**: Implement comprehensive search functionality that allows users to search for files across TeensyROM storage devices with filtering capabilities, seamless UI integration, and proper player context coordination.

**Standards Documentation**:
- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **State Standards**: [STATE_STANDARDS.md](../../STATE_STANDARDS.md)
- **Store Testing**: [STORE_TESTING.md](../../STORE_TESTING.md)
- **Domain Standards**: [DOMAIN_STANDARDS.md](../../DOMAIN_STANDARDS.md)
- **Storage Design**: [STORAGE-DESIGN.md](../storage-navigation/STORAGE-DESIGN.md)
- **Player Design**: [PLAYER_DOMAIN_DESIGN.md](../player-state/PLAYER_DOMAIN_DESIGN.md)

## 🎯 Project Objective

Enable users to search for files across storage devices with filtering capabilities, displaying results in a dedicated component that integrates seamlessly with the existing player and storage domains. Search results should support navigation, file launching, and automatic clearing when users navigate away or perform directory operations.

## 📋 Implementation Phases

## Phase 1: Infrastructure & Contract Layer

### Objective

Establish the foundation by updating domain contracts, implementing infrastructure service methods, and ensuring proper API client integration with domain model mapping.

### Key Deliverables

- [ ] Updated `IStorageService` contract with search method signature
- [ ] `StorageService.search()` implementation with API integration
- [ ] `DomainMapper` integration for search result mapping
- [ ] Infrastructure layer unit tests for search functionality

### High-Level Tasks

1. **Update Storage Contract**: Add search method to [`libs/domain/src/lib/contracts/storage.contract.ts`](../../../libs/domain/src/lib/contracts/storage.contract.ts)
   - Method signature: `search(deviceId: string, storageType: StorageType, searchText: string, filterType?: PlayerFilterType): Observable<FileItem[]>`
   - Returns observable of FileItem array (no directories, only files)
   - Add JSDoc comments explaining search behavior

2. **Implement StorageService.search()**: Update [`libs/infrastructure/src/lib/storage/storage.service.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.ts)
   - Call `FilesApiService.search()` from API client ([`libs/data-access/api-client/src/lib/apis/FilesApiService.ts`](../../../libs/data-access/api-client/src/lib/apis/FilesApiService.ts))
   - Map `SearchResponse.files` array using `DomainMapper.toFileItem()` for each file
   - Extract base API URL from service configuration (same pattern as `getDirectory()`)
   - Pass base URL to mapper for image URL construction
   - Map API `NullableOfTeensyFilterType` to domain `PlayerFilterType` using mapper
   - Handle errors with proper logging and error propagation
   - Return `Observable<FileItem[]>` (not SearchResponse - only the files array)

3. **Update DomainMapper**: Enhance [`libs/infrastructure/src/lib/domain.mapper.ts`](../../../libs/infrastructure/src/lib/domain.mapper.ts)
   - Add filter type mapping methods if needed (may already exist for player domain)
   - Ensure `toFileItem()` properly constructs image URLs with base API URL
   - Verify all FileItem properties are mapped correctly for search results

4. **Infrastructure Testing**: Create/update [`libs/infrastructure/src/lib/storage/storage.service.spec.ts`](../../../libs/infrastructure/src/lib/storage/storage.service.spec.ts)
   - Test search method with mocked API service
   - Test successful search with multiple results
   - Test empty search results
   - Test error scenarios (API failures, network errors)
   - Test domain model mapping correctness
   - Test filter type conversion
   - Verify base URL extraction and image URL construction

---

## Phase 2: Storage Store State & Actions

### Objective

Extend the StorageStore with search state management, implementing actions for executing searches and clearing results while maintaining per-device state isolation and following established state standards.

### Key Deliverables

- [ ] Search state models in storage store
- [ ] Search action implementation
- [ ] Clear search action implementation
- [ ] Search results selector
- [ ] Storage store unit tests for search functionality

### High-Level Tasks

1. **Add Search State Models**: Update [`libs/application/src/lib/storage/storage-store.ts`](../../../libs/application/src/lib/storage/storage-store.ts)
   ```typescript
   export interface SearchState {
     searchText: string;
     filterType: PlayerFilterType | null;
     results: FileItem[];
     isSearching: boolean;
     hasSearched: boolean;
     error: string | null;
   }
   
   export interface StorageState {
     storageEntries: Record<string, StorageDirectoryState>;
     selectedDirectories: Record<string, SelectedDirectory>;
     navigationHistory: Record<string, NavigationHistory>;
     searchState: Record<string, SearchState>; // NEW: keyed by deviceId
   }
   ```
   - Add `searchState` record to main state (per-device isolation)
   - `hasSearched` flag distinguishes "not searched" from "no results"
   - `isSearching` for loading state during API calls
   - Store searchText and filterType for UI display/state

2. **Create search-files Action**: New file [`libs/application/src/lib/storage/actions/search-files.ts`](../../../libs/application/src/lib/storage/actions/search-files.ts)
   - Follow STATE_STANDARDS.md patterns (async/await, updateState with actionMessage)
   - Parameters: `{ deviceId, storageType, searchText, filterType }`
   - Create action message: `createAction('search-files')`
   - Set loading state: `isSearching: true, error: null`
   - Call `storageService.search()` with `firstValueFrom()`
   - On success: Update results, set `hasSearched: true`, clear loading/error
   - On error: Set error message, clear loading, preserve hasSearched
   - Use helper functions for state mutations with action message
   - Add logging with LogType enum (🔍 for search operations)

3. **Create clear-search Action**: New file [`libs/application/src/lib/storage/actions/clear-search.ts`](../../../libs/application/src/lib/storage/actions/clear-search.ts)
   - Follow STATE_STANDARDS.md patterns
   - Parameter: `{ deviceId }`
   - Create action message: `createAction('clear-search')`
   - Reset search state to initial values (empty results, cleared text/filter)
   - Synchronous operation (no service calls)
   - Use updateState with actionMessage for Redux DevTools tracking

4. **Auto-clear Search on Navigation**: Update existing navigation actions
   - [`navigate-to-directory.ts`](../../../libs/application/src/lib/storage/actions/navigate-to-directory.ts)
   - [`navigate-directory-backward.ts`](../../../libs/application/src/lib/storage/actions/navigate-directory-backward.ts)
   - [`navigate-directory-forward.ts`](../../../libs/application/src/lib/storage/actions/navigate-directory-forward.ts)
   - [`navigate-up-one-directory.ts`](../../../libs/application/src/lib/storage/actions/navigate-up-one-directory.ts)
   - Add helper call to clear search state for device (pass same actionMessage)
   - Ensures search clears when user navigates directories

5. **Create Search Selector**: New file [`libs/application/src/lib/storage/selectors/get-search-state.ts`](../../../libs/application/src/lib/storage/selectors/get-search-state.ts)
   - Return computed signal: `Signal<SearchState | null>`
   - Parameter: `deviceId`
   - Lookup search state by deviceId in searchState record
   - Follow existing selector patterns in [`selectors/index.ts`](../../../libs/application/src/lib/storage/selectors/index.ts)

6. **Update Action/Selector Exports**: 
   - Add new actions to [`actions/index.ts`](../../../libs/application/src/lib/storage/actions/index.ts)
   - Add new selector to [`selectors/index.ts`](../../../libs/application/src/lib/storage/selectors/index.ts)

7. **Storage Helpers**: Update [`libs/application/src/lib/storage/storage-helpers.ts`](../../../libs/application/src/lib/storage/storage-helpers.ts)
   - Add search state mutation helpers (require actionMessage parameter)
   - `setSearchLoading()`, `setSearchResults()`, `setSearchError()`, `clearSearchState()`
   - Add search state query helpers (no actionMessage needed)
   - `getSearchState()`, `hasActiveSearch()`

8. **Storage Store Testing**: Update [`libs/application/src/lib/storage/storage-store.spec.ts`](../../../libs/application/src/lib/storage/storage-store.spec.ts)
   - Test search-files action with successful results
   - Test search-files action with empty results
   - Test search-files action with errors
   - Test clear-search action
   - Test auto-clear on directory navigation
   - Test per-device search state isolation
   - Test search state selector
   - Follow STORE_TESTING.md patterns (TestBed, typed mocks, behaviors checklist)

---

## Phase 3: Search Results Component

### Objective

Build a dedicated search results component that displays search results in a clean, file-only list, supports file launching with Search launch mode, and integrates with player context for current file highlighting.

### Key Deliverables

- [ ] SearchResultsComponent implementation
- [ ] Component styling matching directory-files patterns
- [ ] File launching with Search launch mode
- [ ] Current file highlighting
- [ ] Loading and error states
- [ ] Component unit tests

### High-Level Tasks

1. **Create SearchResultsComponent**: New component at [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-results/search-results.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-results/search-results.component.ts)
   - Input: `deviceId` (required)
   - Inject: `StorageStore`, `PLAYER_CONTEXT`
   - Computed signal: `searchState` from `storageStore.getSearchState(deviceId())()`
   - Computed signal: `currentPlayingFile` from `playerContext.getCurrentFile(deviceId())()`
   - Computed signal: `hasError` from `playerContext.getError(deviceId())()`
   - Display search results (files only, no directories)
   - Similar to DirectoryFilesComponent but simpler (no directory items)

2. **File Selection & Launching**: 
   - Local signal for selected item: `selectedItem = signal<FileItem | null>(null)`
   - Single-click selection (updates selectedItem)
   - Double-click launches file with Search launch mode
   - Call `playerContext.launchFileWithContext()`:
     ```typescript
     await playerContext.launchFileWithContext({
       deviceId: deviceId(),
       storageType: storageType,
       file: clickedFile,
       directoryPath: file.parentPath, // From FileItem metadata
       files: searchState.results, // All search results
       launchMode: LaunchMode.Search
     });
     ```

3. **Current File Highlighting**: 
   - Use effect pattern from DirectoryFilesComponent
   - Auto-select and scroll to currently playing file in search results
   - Check launch mode is Search before highlighting
   - Only highlight if file path matches and launch mode is Search

4. **Component Styling**: Create [`search-results.component.scss`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-results/search-results.component.scss)
   - Copy patterns from directory-files.component.scss
   - Use ScalingCardComponent for file items
   - Highlight selected item, playing item, error states
   - Support virtual scrolling for large result sets

5. **Empty/Loading/Error States**: 
   - Show loading spinner when `isSearching` is true
   - Show "No results found" when `hasSearched` but results empty
   - Show "Enter search text to begin" when not searched
   - Show error message from search state
   - Use conditional rendering with @if directives

6. **Component Testing**: Create [`search-results.component.spec.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-results/search-results.component.spec.ts)
   - Test component rendering with search results
   - Test file selection behavior
   - Test file launching with Search launch mode
   - Test current file highlighting
   - Test loading/error/empty states
   - Mock StorageStore and PlayerContext
   - Follow TESTING_STANDARDS.md patterns

---

## Phase 4: Search Toolbar Integration

### Objective

Enhance the search toolbar component with functional search input, clear button, and integration with StorageStore search actions.

### Key Deliverables

- [ ] Functional search input with debouncing
- [ ] Search execution on Enter or button click
- [ ] Clear search button
- [ ] Filter type integration
- [ ] Toolbar component tests

### High-Level Tasks

1. **Update SearchToolbarComponent**: Enhance [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts)
   - Add input: `deviceId` (required)
   - Inject: `StorageStore`, `PLAYER_CONTEXT`
   - Get selected directory state to determine current storageType
   - Get player shuffle settings to access current filterType
   - Local signals: `searchText = signal<string>('')`
   - Computed: `hasActiveSearch` from store search state
   - Computed: `currentFilter` from player shuffle settings

2. **Search Input Handling**:
   - Bind input field to searchText signal
   - Add Enter key handler to trigger search
   - Add search button with click handler
   - Debounce input (300ms) before enabling search button
   - Disable search if searchText is empty or only whitespace

3. **Execute Search**:
   - Call `storageStore.searchFiles()`:
     ```typescript
     await storageStore.searchFiles({
       deviceId: deviceId(),
       storageType: selectedDirectoryState().storageType,
       searchText: searchText().trim(),
       filterType: currentFilter() || PlayerFilterType.All
     });
     ```
   - Use current storage type from selected directory
   - Use current filter from player shuffle settings
   - Trim whitespace from search text

4. **Clear Search Button**:
   - Show clear button (X icon) when `hasActiveSearch` is true
   - Click handler calls `storageStore.clearSearch({ deviceId })`
   - Clears searchText signal
   - Button styled as ScalingCompactCardComponent

5. **Visual States**:
   - Disable search button when input empty
   - Show loading indicator during search (from search state)
   - Show error state on search button if search failed
   - Clear button visible only when search active

6. **Toolbar Styling**: Update [`search-toolbar.component.scss`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.scss)
   - Match filter-toolbar styling patterns
   - Proper spacing for input + buttons
   - Responsive layout
   - Error state colors matching STYLE_GUIDE.md

7. **Component Testing**: Update [`search-toolbar.component.spec.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.spec.ts)
   - Test search input binding
   - Test search execution on Enter/button click
   - Test clear search behavior
   - Test disabled states
   - Test filter integration
   - Mock store and context

---

## Phase 5: Storage Container UI Integration

### Objective

Integrate search results component into storage container with conditional rendering, ensuring proper show/hide behavior between directory-files and search-results components.

### Key Deliverables

- [ ] Conditional rendering of directory-files vs search-results
- [ ] Updated storage container template
- [ ] Proper component imports and wiring
- [ ] Visual transition between modes

### High-Level Tasks

1. **Update StorageContainerComponent**: Modify [`libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.ts)
   - Add SearchResultsComponent to imports
   - Add computed signal: `hasActiveSearch = computed(() => this.storageStore.getSearchState(this.deviceId())()?.hasSearched || false)`
   - Pass deviceId to search-toolbar component (currently missing)

2. **Update StorageContainer Template**: Modify [`storage-container.component.html`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.html)
   - Add deviceId input to search-toolbar: `<lib-search-toolbar [deviceId]="deviceId()"></lib-search-toolbar>`
   - Replace directory-files with conditional rendering in right-container:
     ```html
     <div class="right-container">
       @if (hasActiveSearch()) {
         <lib-search-results [deviceId]="deviceId()"></lib-search-results>
       } @else {
         <lib-directory-files [deviceId]="deviceId()"></lib-directory-files>
       }
     </div>
     ```
   - Ensure smooth transition between components

3. **Hide Directory Tree During Search** (Optional Enhancement):
   - Consider computed signal to hide/disable directory tree when search active
   - Visual indication that tree navigation is unavailable during search
   - Or keep tree visible but indicate it's not interactive during search mode

4. **Component Testing**: Update [`storage-container.component.spec.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.spec.ts)
   - Test conditional rendering based on search state
   - Test component switching (directory-files ↔ search-results)
   - Test deviceId passing to child components
   - Mock store with different search states

---

## Phase 6: Player Context Integration

### Objective

Ensure player store properly handles Search launch mode, maintains search result context for navigation, and supports next/previous operations within search results.

### Key Deliverables

- [ ] Search launch mode support in player store
- [ ] Search context preservation during navigation
- [ ] Next/Previous operations within search results
- [ ] Auto-clear search when shuffle activated
- [ ] Player integration tests

### High-Level Tasks

1. **Verify Launch Mode Support**: Review [`libs/application/src/lib/player/actions/launch-file-with-context.ts`](../../../libs/application/src/lib/player/actions/launch-file-with-context.ts)
   - Ensure `LaunchMode.Search` is properly handled (likely already works)
   - Verify fileContext stores search results array
   - Verify currentIndex is calculated correctly within search results

2. **Review Navigation Actions**: Check [`navigate-next.ts`](../../../libs/application/src/lib/player/actions/navigate-next.ts) and [`navigate-previous.ts`](../../../libs/application/src/lib/player/actions/navigate-previous.ts)
   - Verify they respect fileContext regardless of launch mode
   - Navigation should work identically for Directory and Search modes
   - Both modes use fileContext array and currentIndex

3. **Search Clear on Shuffle**: Update [`launch-random-file.ts`](../../../libs/application/src/lib/player/actions/launch-random-file.ts)
   - When shuffle launches, it clears fileContext (already does this)
   - This automatically triggers search component to disappear
   - Verify behavior in player context service orchestration

4. **Player Context Service**: Review [`libs/application/src/lib/player/player-context.service.ts`](../../../libs/application/src/lib/player/player-context.service.ts)
   - Verify `launchFileWithContext()` properly handles Search launch mode
   - Ensure fileContext preservation for search results
   - Check that next/previous work with search context

5. **Player Store Testing**: Update [`libs/application/src/lib/player/player-store.spec.ts`](../../../libs/application/src/lib/player/player-store.spec.ts)
   - Test launching file with Search launch mode
   - Test navigation next/previous within search results
   - Test search context preservation
   - Test shuffle clearing search context (fileContext cleared)
   - Follow STORE_TESTING.md patterns

6. **Integration Testing**:
   - Test complete workflow: Search → Launch file → Next/Previous → Clear search
   - Test: Search → Launch file → Navigate directory → Search cleared
   - Test: Search → Launch file → Activate shuffle → Search cleared
   - Test: Directory mode → Search → Launch → Still in search mode
   - Verify UI components properly show/hide during state transitions

---

## 🏗️ Architecture Overview

### Key Design Decisions

- **Search State per Device**: Independent search state for each device in StorageStore following established multi-device patterns
- **FileItem Array Results**: Search returns `FileItem[]` directly, not full `SearchResponse` - simplifies domain boundary
- **Search Launch Mode**: Explicit `LaunchMode.Search` enum value distinguishes search navigation from directory/shuffle modes
- **Auto-Clear Behavior**: Search clears automatically on directory navigation or shuffle activation - maintains consistent UX
- **Shared File Rendering**: Search results component reuses file item rendering patterns from directory-files component
- **State Standards Compliance**: All actions use `updateState()` with `actionMessage`, async/await patterns, and helper functions

### Integration Points

- **Storage Domain**: Search extends storage service contract, adds state to StorageStore, integrates with navigation actions
- **Player Domain**: Search launch mode in PlayerStore, fileContext supports search results array, navigation actions work across all modes
- **Infrastructure Layer**: StorageService.search() calls FilesApiService, DomainMapper transforms search results to FileItem[]
- **Feature Components**: SearchResultsComponent parallels DirectoryFilesComponent, SearchToolbarComponent triggers store actions

## 🧪 Testing Strategy

### Unit Tests

- [ ] Infrastructure layer: StorageService.search() with API mocking
- [ ] Application layer: Search actions, clear actions, selectors with store behaviors
- [ ] Helpers: Search state mutation and query helpers
- [ ] Components: SearchToolbarComponent, SearchResultsComponent with mocked dependencies

### Integration Tests

- [ ] End-to-end search workflow: Input → Execute → Display → Launch → Navigate
- [ ] Cross-domain: Search → Player launch → Navigation → Clear search
- [ ] State transitions: Directory mode ↔ Search mode ↔ Shuffle mode
- [ ] Auto-clear scenarios: Search active → Directory navigation → Search cleared

### E2E Tests

- [ ] User searches for file, launches, navigates through results
- [ ] User clears search, returns to directory view
- [ ] User in search mode, navigates to directory, search auto-clears
- [ ] User in search mode, activates shuffle, search auto-clears

## ✅ Success Criteria

- [ ] Users can search for files using search toolbar with text input
- [ ] Search results display in dedicated component with loading/error/empty states
- [ ] Users can launch files from search results with proper player context
- [ ] Next/Previous navigation works within search results
- [ ] Search automatically clears when user navigates directories or activates shuffle
- [ ] Clear search button returns user to directory view
- [ ] Per-device search state isolation maintained
- [ ] All tests passing (unit, integration, e2e)
- [ ] Code follows STATE_STANDARDS.md patterns (updateState with actionMessage)
- [ ] Redux DevTools properly tracks all search operations
- [ ] Search works with current filter settings from player domain
- [ ] Image URLs properly constructed for search result files
- [ ] Component styling matches existing directory-files patterns

## 📚 Related Documentation

- **Storage Design**: [`STORAGE-DESIGN.md`](../storage-navigation/STORAGE-DESIGN.md) - Storage domain architecture
- **Player Design**: [`PLAYER_DOMAIN_DESIGN.md`](../player-state/PLAYER_DOMAIN_DESIGN.md) - Player domain architecture
- **State Standards**: [`STATE_STANDARDS.md`](../../STATE_STANDARDS.md) - NgRx Signal Store patterns
- **Store Testing**: [`STORE_TESTING.md`](../../STORE_TESTING.md) - Store testing methodology
- **API Client**: [`API_CLIENT_GENERATION.md`](../../API_CLIENT_GENERATION.md) - API client patterns
- **Component Library**: [`COMPONENT_LIBRARY.md`](../../COMPONENT_LIBRARY.md) - UI component standards

## 📝 Notes

### Key Architectural Considerations

- **LaunchMode.Search is Critical**: The Search enum value already exists in [`libs/domain/src/lib/models/launch-mode.enum.ts`](../../../libs/domain/src/lib/models/launch-mode.enum.ts) and is fundamental to distinguishing search navigation from directory/shuffle modes
- **FileItem.parentPath**: Search results include parentPath property which provides the directory context for each file, eliminating need to track directory separately
- **Cross-Domain Auto-Clear**: Navigation actions in StorageStore trigger search clear, while shuffle in PlayerStore clears fileContext - both achieve same UX goal through different mechanisms
- **Filter Integration**: Search uses current filter from player shuffle settings, maintaining consistency between shuffle and search filtering
- **No Search API Service**: Search endpoint already exists in FilesApiService - only need to wire up domain contract and implementation

### Implementation Order Rationale

1. **Phase 1 (Infrastructure)**: Foundation must be solid - API integration and domain mapping first
2. **Phase 2 (Store State)**: State management before UI ensures reactive architecture
3. **Phase 3 (Search Results)**: Display component before input ensures we can test end-to-end
4. **Phase 4 (Toolbar)**: Input mechanism after display allows immediate visual feedback
5. **Phase 5 (Container)**: UI integration after components are functional
6. **Phase 6 (Player Integration)**: Verify cross-domain behavior last, after all pieces in place

### Future Enhancements (Out of Scope)

- Search history/recent searches persistence
- Advanced search operators (AND, OR, NOT)
- Search within search (filter existing results)
- Save search as playlist
- Search result sorting options
- Fuzzy search/typo tolerance
- Search suggestions/autocomplete
