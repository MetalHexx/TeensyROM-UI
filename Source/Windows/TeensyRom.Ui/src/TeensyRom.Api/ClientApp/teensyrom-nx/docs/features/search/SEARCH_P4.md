# Phase 4: Search Toolbar Integration & User Interface

## 🎯 Objective

Enhance the search toolbar component with functional search input, debounced search execution, clear button, and seamless integration with StorageStore search actions and filter settings. This phase completes the user-facing search mechanism by connecting the search input UI to the store actions and search results component.

## 📚 Required Reading

- [ ] [Search Plan](./SEARCH_PLAN.md) - Overall feature architecture
- [ ] [Phase 1 Document](./SEARCH_P1.md) - Infrastructure layer foundation
- [ ] [Phase 2 Document](./SEARCH_P2.md) - Store state and actions
- [ ] [Phase 3 Document](./SEARCH_P3.md) - Search results component
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - Component structure, signals, templates
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Component testing methodology
- [ ] [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) - Testing components with store dependencies
- [ ] [Style Guide](../../STYLE_GUIDE.md) - UI styling standards and utility classes
- [ ] [Component Library](../../COMPONENT_LIBRARY.md) - Reusable UI components

## File Tree

```
libs/features/player/src/lib/player-view/player-device-container/storage-container/
├── search-toolbar/
│   ├── search-toolbar.component.ts       # UPDATE: Add search functionality
│   ├── search-toolbar.component.html     # UPDATE: Add clear button and bindings
│   ├── search-toolbar.component.scss     # UPDATE: Style clear button
│   └── search-toolbar.component.spec.ts  # UPDATE: Add comprehensive tests
├── search-results/
│   └── (Phase 3 files - already complete)
├── filter-toolbar/
│   └── filter-toolbar.component.ts       # REFERENCE: Filter integration pattern
└── storage-container.component.ts        # UPDATE: Add deviceId to search-toolbar
```

## 📋 Implementation Tasks

### Task 1: Update SearchToolbarComponent Class with Inputs and Dependencies

**Purpose**: Add required inputs, inject dependencies, and create computed signals for reactive state management.

**File**: [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts)

- [ ] Add required input property
  - `deviceId = input.required<string>()`
  - Used to identify which device's search state to manage
- [ ] Inject StorageStore dependency
  - `private readonly storageStore = inject(StorageStore)`
  - Provides access to search actions and state
- [ ] Inject PlayerContext dependency
  - `private readonly playerContext = inject(PLAYER_CONTEXT)`
  - Provides access to filter settings for search integration
- [ ] Create computed signal for selected directory state
  - `readonly selectedDirectoryState = computed(() => this.storageStore.getSelectedDirectoryState(this.deviceId())())`
  - Provides current storageType for search operations
  - Type: `Signal<SelectedDirectoryState | null>`
- [ ] Create computed signal for current storage type
  - `readonly currentStorageType = computed(() => this.selectedDirectoryState()?.storageType ?? null)`
  - Extracts storageType from selected directory
  - Type: `Signal<StorageType | null>`
- [ ] Create computed signal for search state
  - `readonly searchState = computed(() => this.storageStore.getSearchState(this.deviceId())())`
  - Provides reactive access to search state from store
  - Type: `Signal<SearchState | null>`
- [ ] Create computed signal for active search status
  - `readonly hasActiveSearch = computed(() => this.searchState()?.hasSearched ?? false)`
  - Determines if search results are currently displayed
  - Type: `Signal<boolean>`
- [ ] Create computed signal for searching status
  - `readonly isSearching = computed(() => this.searchState()?.isSearching ?? false)`
  - Indicates if search is currently in progress
  - Type: `Signal<boolean>`
- [ ] Create computed signal for current filter
  - `readonly currentFilter = computed(() => this.playerContext.getShuffleSettings(this.deviceId())()?.filter ?? PlayerFilterType.All)`
  - Gets active filter from player context (same filter used for shuffle)
  - Type: `Signal<PlayerFilterType>`
- [ ] Create local signal for search input text
  - `readonly searchText = signal<string>('')`
  - Tracks user input in real-time
  - Type: `WritableSignal<string>`
- [ ] Create computed signal for search button enabled state
  - `readonly canSearch = computed(() => this.searchText().trim().length > 0 && !this.isSearching())`
  - Disables search if text empty or search in progress
  - Type: `Signal<boolean>`

**Reference Pattern**: See [`filter-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.ts) for similar deviceId input and playerContext integration patterns.

---

### Task 2: Implement Search Execution with Debouncing

**Purpose**: Handle search execution with proper debouncing, validation, and error handling.

**File**: [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts)

- [ ] Add `executeSearch(): void` method
  - Validates search text is not empty/whitespace
  - Validates currentStorageType is not null
  - Calls `storageStore.searchFiles()` with proper parameters:
    ```typescript
    void this.storageStore.searchFiles({
      deviceId: this.deviceId(),
      storageType: this.currentStorageType()!,
      searchText: this.searchText().trim(),
      filterType: this.currentFilter(),
    });
    ```
  - Uses `void` keyword to handle Promise without awaiting
  - Trims whitespace from search text before execution
  - Uses current filter from player shuffle settings
- [ ] Add `onSearchInputChange(value: string): void` method
  - Called by InputFieldComponent's `valueChange` event
  - Updates `searchText` signal: `this.searchText.set(value)`
  - Provides reactive binding between input and component state
- [ ] Add debounced search effect (OPTIONAL - for auto-search as you type)
  - Import `effect`, `untracked` from `@angular/core`
  - Create effect in constructor to watch `searchText()` changes
  - Use `setTimeout()` with 2000ms delay for debouncing
  - Clear previous timeout on each change
  - Auto-execute search after 2 seconds of no typing
  - Only execute if `canSearch()` is true
  - Pattern:
    ```typescript
    private searchTimeout?: ReturnType<typeof setTimeout>;
    
    constructor() {
      effect(() => {
        const text = this.searchText();
        
        // Clear previous timeout
        if (this.searchTimeout) {
          clearTimeout(this.searchTimeout);
        }
        
        // Don't auto-search on empty text
        if (text.trim().length === 0) {
          return;
        }
        
        // Use untracked to prevent infinite loops when checking other signals
        untracked(() => {
          if (this.canSearch()) {
            this.searchTimeout = setTimeout(() => {
              this.executeSearch();
            }, 2000);
          }
        });
      });
    }
    ```

**Alternative Approach** (Manual Search Only):
- If debouncing is not desired, remove the effect
- Search only executes on Enter key press or button click
- Simpler implementation, more explicit user control

---

### Task 3: Implement Clear Search Functionality

**Purpose**: Allow users to clear search results and return to directory view.

**File**: [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts)

- [ ] Add `clearSearch(): void` method
  - Calls `storageStore.clearSearch()` action:
    ```typescript
    this.storageStore.clearSearch({
      deviceId: this.deviceId(),
      storageType: this.currentStorageType()!,
    });
    ```
  - Clears local `searchText` signal: `this.searchText.set('')`
  - Validates storageType is not null before calling
  - Synchronous operation (no await needed)
- [ ] Add computed signal for clear button visibility
  - `readonly showClearButton = computed(() => this.hasActiveSearch() && this.searchState()?.results && this.searchState()!.results.length > 0)`
  - Clear button only visible when search has been executed AND results exist
  - Prevents showing clear button if search returned no results
  - Type: `Signal<boolean>`

---

### Task 4: Update Component Template with Search Controls

**Purpose**: Bind search input, add clear button, and implement keyboard shortcuts.

**File**: [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.html`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.html)

- [ ] Update InputFieldComponent binding
  - Add `(valueChange)="onSearchInputChange($event)"` event binding
  - Add `(keydown.enter)="executeSearch()"` for Enter key search
  - Keep existing properties: `label`, `placeholder`, `suffixIcon`
  - Result:
    ```html
    <lib-input-field
      label="Search"
      placeholder="Search files and folders..."
      suffixIcon="search"
      class="search-field"
      (valueChange)="onSearchInputChange($event)"
      (keydown.enter)="executeSearch()"
    >
    </lib-input-field>
    ```
- [ ] Add clear search button after input field
  - Use IconButtonComponent with 'close' icon (X symbol)
  - Set size to 'small' for compact toolbar appearance
  - Bind to `showClearButton()` computed signal with `@if`
  - Only appears when search has results (not just searched with no results)
  - Click handler calls `clearSearch()`
  - Add aria-label for accessibility
  - Structure:
    ```html
    @if (showClearButton()) {
      <lib-icon-button
        icon="close"
        ariaLabel="Clear search results"
        color="normal"
        size="small"
        (buttonClick)="clearSearch()"
      >
      </lib-icon-button>
    }
    ```
  - **Icon Choice**: Use 'close' (X) instead of 'clear' for better visual clarity
  - **Size**: 'small' matches toolbar scale and doesn't overpower input field
  - **Visibility Logic**: Only shows when `hasActiveSearch()` AND results array has items
- [ ] Add loading indicator (OPTIONAL)
  - Show spinner/loading state when `isSearching()` is true
  - Replace suffix icon or add as separate element
  - Use Material spinner or custom loading indicator
  - Pattern:
    ```html
    @if (isSearching()) {
      <mat-spinner diameter="20"></mat-spinner>
    }
    ```

**Layout Considerations**:
- Maintain horizontal layout with input and buttons
- Use flexbox with gap for proper spacing
- Ensure clear button doesn't cause layout shift when appearing/disappearing
- Match filter-toolbar height and styling

---

### Task 5: Update Component Styles

**Purpose**: Style search controls, clear button, and loading states to match design system.

**File**: [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.scss`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.scss)

- [ ] Update host styling
  - Maintain existing display/flex properties
  - Ensure full width for container
- [ ] Style ScalingCompactCardComponent container
  - Flexbox layout with horizontal direction
  - Gap between input and buttons (e.g., `gap: 0.5rem`)
  - Align items center for vertical alignment
  - Padding consistent with filter-toolbar
- [ ] Style search input field
  - Flex: 1 to take remaining space
  - Min-width to prevent shrinking too small
  - Match height with filter buttons
- [ ] Style clear button
  - Fixed width for consistent layout (small size: 18px icon)
  - No margin/padding issues on show/hide
  - Hover effects matching other icon buttons (automatic from IconButtonComponent)
  - Transition for smooth appearance
  - Small size ensures button doesn't dominate toolbar
  - Close (X) icon provides clear affordance for dismissal action
- [ ] Loading state styling (if implemented)
  - Position spinner appropriately
  - Size matching input field height
  - Color matching theme
- [ ] Responsive considerations
  - Proper sizing for mobile screens
  - Touch-friendly button sizes
  - Input field min-width on small screens

**Example Structure**:
```scss
:host {
  display: flex;
  width: 100%;
}

.search-toolbar-content {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  width: 100%;
  padding: 0.5rem;

  .search-field {
    flex: 1;
    min-width: 200px;
  }

  lib-icon-button {
    flex-shrink: 0;
  }
}
```

**Reference Styles**: See [`filter-toolbar.component.scss`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.scss) for layout patterns.

---

### Task 6: Add Component Imports

**Purpose**: Import necessary components and types for template usage.

**File**: [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts)

- [ ] Update imports array in component decorator
  - Keep existing: `CommonModule`, `ScalingCompactCardComponent`, `InputFieldComponent`
  - Add: `IconButtonComponent` from `@teensyrom-nx/ui/components`
  - Add Material spinner if using loading indicator: `MatProgressSpinnerModule`
- [ ] Import required types
  - `StorageStore` from `@teensyrom-nx/application`
  - `PLAYER_CONTEXT` from `@teensyrom-nx/application`
  - `PlayerFilterType` from `@teensyrom-nx/domain`
  - `StorageType` from `@teensyrom-nx/domain`
- [ ] Import Angular core features
  - `input`, `inject`, `computed`, `signal`, `effect`, `untracked` (if using debouncing)

---

### Task 7: Update StorageContainerComponent Integration

**Purpose**: Pass deviceId to search toolbar and prepare for conditional rendering with search results.

**File**: [`libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.ts)

- [ ] Add computed signal for active search state
  - `readonly hasActiveSearch = computed(() => this.storageStore.getSearchState(this.deviceId())()?.hasSearched ?? false)`
  - Used to conditionally render search-results vs directory-files
  - Type: `Signal<boolean>`
- [ ] Import SearchResultsComponent
  - Add to component imports array
  - Import from relative path: `'./search-results/search-results.component'`

**File**: [`libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.html`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.html)

- [ ] Add deviceId input to search-toolbar
  - Update existing `<lib-search-toolbar>` tag
  - Add: `[deviceId]="deviceId()"`
  - Enables search toolbar to access device-specific state
- [ ] Implement conditional rendering in right-container
  - Replace current `<lib-directory-files>` with conditional logic
  - Show `<lib-search-results>` when `hasActiveSearch()` is true
  - Show `<lib-directory-files>` when `hasActiveSearch()` is false
  - Pattern:
    ```html
    <div class="right-container">
      @if (hasActiveSearch()) {
        <lib-search-results [deviceId]="deviceId()"></lib-search-results>
      } @else {
        <lib-directory-files [deviceId]="deviceId()"></lib-directory-files>
      }
    </div>
    ```

---

### Task 8: Component Testing

**Purpose**: Comprehensive unit tests for search toolbar functionality.

**File**: [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.spec.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.spec.ts)

**Test Setup**:
- [ ] Import testing utilities
  - `ComponentFixture, TestBed` from `@angular/core/testing`
  - `MockedFunction, vi` from `vitest`
  - Component under test and dependencies
- [ ] Create mock StorageStore
  - Mock `getSelectedDirectoryState()` returning signal with storageType
  - Mock `getSearchState()` returning signal with test SearchState
  - Mock `searchFiles()` method (async)
  - Mock `clearSearch()` method (sync)
  - Type: Object with mocked store methods
- [ ] Create mock PlayerContext
  - Mock `getShuffleSettings()` returning signal with filter
  - Type: Object with mocked context methods
- [ ] Configure TestBed
  - Import SearchToolbarComponent
  - Provide mocked dependencies via tokens
  - Use `provideExperimentalZonelessChangeDetection()` if needed
- [ ] Create component fixture and instance
  - `fixture = TestBed.createComponent(SearchToolbarComponent)`
  - `component = fixture.componentInstance`
  - Set required input: `fixture.componentRef.setInput('deviceId', 'test-device')`

**Test Suites**:

**Component Initialization**:
- [ ] Test: Component creates successfully
- [ ] Test: DeviceId input is required and set correctly
- [ ] Test: Initial searchText is empty string
- [ ] Test: Computed signals initialize with correct values

**Search Input Handling**:
- [ ] Test: onSearchInputChange updates searchText signal
  - Set input value: `component.onSearchInputChange('test query')`
  - Assert: `component.searchText() === 'test query'`
- [ ] Test: canSearch returns false when searchText is empty
  - Assert: `component.canSearch() === false`
- [ ] Test: canSearch returns true when searchText has content
  - Set: `component.searchText.set('test')`
  - Assert: `component.canSearch() === true`
- [ ] Test: canSearch returns false when isSearching is true
  - Mock: `isSearching: true` in search state
  - Set: `component.searchText.set('test')`
  - Assert: `component.canSearch() === false`

**Search Execution**:
- [ ] Test: executeSearch calls searchFiles with correct parameters
  - Mock: Selected directory with storageType
  - Set: `component.searchText.set('test query')`
  - Call: `component.executeSearch()`
  - Assert: `mockStorageStore.searchFiles` called with:
    ```typescript
    {
      deviceId: 'test-device',
      storageType: StorageType.Usb,
      searchText: 'test query',
      filterType: PlayerFilterType.All
    }
    ```
- [ ] Test: executeSearch trims whitespace from search text
  - Set: `component.searchText.set('  test query  ')`
  - Call: `component.executeSearch()`
  - Assert: Called with `searchText: 'test query'` (trimmed)
- [ ] Test: executeSearch uses current filter from player context
  - Mock: Player shuffle settings with `filter: PlayerFilterType.Games`
  - Call: `component.executeSearch()`
  - Assert: Called with `filterType: PlayerFilterType.Games`
- [ ] Test: executeSearch does nothing when storageType is null
  - Mock: Selected directory state returns null
  - Call: `component.executeSearch()`
  - Assert: `searchFiles` not called
- [ ] Test: Enter key triggers search execution
  - Set: `component.searchText.set('test')`
  - Simulate: Enter key press on input field
  - Assert: `searchFiles` called

**Debouncing (if implemented)**:
- [ ] Test: Auto-search triggers after 2 second delay
  - Set: `component.searchText.set('test')`
  - Wait: 2000ms (use `vi.advanceTimersByTime()` with `vi.useFakeTimers()`)
  - Assert: `searchFiles` called automatically
- [ ] Test: Typing resets debounce timer
  - Set: `component.searchText.set('test')`
  - Wait: 1000ms
  - Set: `component.searchText.set('test2')`
  - Wait: 1000ms
  - Assert: `searchFiles` not called yet (timer reset)
  - Wait: 1000ms more
  - Assert: `searchFiles` called with 'test2'

**Clear Search**:
- [ ] Test: clearSearch calls clearSearch action
  - Mock: Active search state (`hasSearched: true`)
  - Call: `component.clearSearch()`
  - Assert: `mockStorageStore.clearSearch` called with:
    ```typescript
    { deviceId: 'test-device', storageType: StorageType.Usb }
    ```
- [ ] Test: clearSearch clears searchText signal
  - Set: `component.searchText.set('test query')`
  - Call: `component.clearSearch()`
  - Assert: `component.searchText() === ''`
- [ ] Test: Clear button visible when search has results
  - Mock: Search state with `hasSearched: true, results: [mockFile1, mockFile2]`
  - Detect changes
  - Query: Clear button element
  - Assert: Button element exists
- [ ] Test: Clear button hidden when hasActiveSearch is false
  - Mock: Search state with `hasSearched: false`
  - Detect changes
  - Query: Clear button element
  - Assert: Button element is null
- [ ] Test: Clear button hidden when search has no results
  - Mock: Search state with `hasSearched: true, results: []`
  - Detect changes
  - Query: Clear button element
  - Assert: Button element is null (don't show clear button for empty results)

**Computed Signals**:
- [ ] Test: hasActiveSearch reflects search state
  - Mock: `hasSearched: false`
  - Assert: `component.hasActiveSearch() === false`
  - Update: `hasSearched: true`
  - Assert: `component.hasActiveSearch() === true`
- [ ] Test: isSearching reflects search state
  - Mock: `isSearching: false`
  - Assert: `component.isSearching() === false`
  - Update: `isSearching: true`
  - Assert: `component.isSearching() === true`
- [ ] Test: currentFilter gets value from player context
  - Mock: Shuffle settings with `filter: PlayerFilterType.Music`
  - Assert: `component.currentFilter() === PlayerFilterType.Music`
- [ ] Test: currentStorageType extracts from selected directory
  - Mock: Selected directory with `storageType: StorageType.Sd`
  - Assert: `component.currentStorageType() === StorageType.Sd`

**Edge Cases**:
- [ ] Test: Component handles null selected directory state
  - Mock: `getSelectedDirectoryState()` returns null
  - Detect changes
  - Assert: Component doesn't crash
  - Assert: `currentStorageType() === null`
- [ ] Test: Component handles null search state
  - Mock: `getSearchState()` returns null
  - Detect changes
  - Assert: Component doesn't crash
  - Assert: `hasActiveSearch() === false`
  - Assert: `isSearching() === false`
- [ ] Test: Component handles empty deviceId gracefully
  - Set input: `fixture.componentRef.setInput('deviceId', '')`
  - Detect changes
  - Assert: Component doesn't crash

**Follow TESTING_STANDARDS.md**:
- Use Vitest mocking patterns
- Use TestBed for component instantiation
- Test behavior, not implementation
- Arrange-Act-Assert pattern
- Descriptive test names

---

## 🗂️ File Changes

### Updated Files
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.ts) - Add search functionality
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.html`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.html) - Add clear button and bindings
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.scss`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.scss) - Style clear button
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.spec.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-toolbar/search-toolbar.component.spec.ts) - Add comprehensive tests
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.ts) - Add hasActiveSearch computed signal and imports
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.html`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/storage-container.component.html) - Add conditional rendering

### Reference Files (No Changes)
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.ts) - Pattern reference for filter integration
- [`libs/application/src/lib/storage/actions/search-files.ts`](../../../libs/application/src/lib/storage/actions/search-files.ts) - Search action being called
- [`libs/application/src/lib/storage/actions/clear-search.ts`](../../../libs/application/src/lib/storage/actions/clear-search.ts) - Clear action being called
- [`libs/features/player/src/lib/player-view/player-device-container/storage-container/search-results/search-results.component.ts`](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/search-results/search-results.component.ts) - Search results display (Phase 3)

## 🧪 Testing Requirements

### Unit Tests

**Search Input**:
- [ ] Input change updates searchText signal
- [ ] canSearch computed signal correctness
- [ ] Empty text disables search
- [ ] Searching state disables search

**Search Execution**:
- [ ] Execute search calls searchFiles with correct params
- [ ] Search text is trimmed
- [ ] Current filter is used
- [ ] Enter key triggers search
- [ ] Null storageType prevents search

**Debouncing** (if implemented):
- [ ] Auto-search after 2 second delay
- [ ] Typing resets timer
- [ ] No auto-search on empty text

**Clear Search**:
- [ ] clearSearch calls clear action
- [ ] searchText signal cleared
- [ ] Clear button visible only when results exist
- [ ] Clear button hidden when no results
- [ ] Clear button uses small size

**Computed Signals**:
- [ ] hasActiveSearch reflects state
- [ ] isSearching reflects state
- [ ] currentFilter gets from context
- [ ] currentStorageType extracts correctly

**Edge Cases**:
- [ ] Null selected directory handled
- [ ] Null search state handled
- [ ] Empty deviceId doesn't crash

**Test Coverage Requirements**:
- Minimum 80% line coverage
- 100% coverage for search/clear execution paths
- All user interactions tested
- All state variations covered

## ✅ Success Criteria

- [ ] Search input updates searchText signal reactively
- [ ] Search executes on Enter key or after 2 second debounce
- [ ] Search calls searchFiles action with correct parameters
- [ ] Search uses current filter from player context
- [ ] Clear button (small size) appears only when search has results
- [ ] Clear button does NOT appear when search returns no results
- [ ] Clear button clears search state and input
- [ ] canSearch prevents execution when text empty or searching
- [ ] StorageContainer passes deviceId to search-toolbar
- [ ] StorageContainer conditionally renders search-results vs directory-files
- [ ] Component styling matches filter-toolbar patterns
- [ ] All component tests passing
- [ ] Test coverage meets requirements (80% minimum)
- [ ] Code follows CODING_STANDARDS.md (signals, modern control flow)
- [ ] Template uses modern Angular syntax (@if)
- [ ] Styles follow STYLE_GUIDE.md patterns
- [ ] No duplicate code from filter-toolbar component
- [ ] Search integration seamless with existing UI

## 📝 Notes

### Key Component Patterns

- **Input Property**: Use `input.required<string>()` for deviceId following CODING_STANDARDS.md
- **Computed Signals**: All derived state uses `computed()` for reactivity
- **Effect for Debouncing**: Constructor effect with setTimeout for auto-search
- **Signal Updates**: Use `.set()` for local state changes
- **Void Async Calls**: Use `void` keyword when not awaiting Promises

### Design Decisions

- **2 Second Debounce**: Balances responsiveness with API call efficiency
- **Optional Auto-Search**: Can be removed for manual-only search
- **Enter Key Shortcut**: Standard UX pattern for search execution
- **Clear Button Visibility**: Only show when search has results (not just executed)
  - Prevents confusing UX when search returns no results
  - User only sees clear button when there's something to clear
  - Small size (18px icon) maintains compact toolbar appearance
- **Clear Button Icon**: Uses 'close' (X) icon for universal dismissal affordance
- **Clear Button Size**: Small size matches toolbar scale without overpowering input
- **Filter Integration**: Uses same filter as shuffle for consistency
- **Trim Whitespace**: Prevents searches with only spaces
- **Null Storage Type Check**: Prevents errors before directory selected

### Integration with Other Components

- **StorageContainer**: Conditional rendering shows search-results or directory-files
- **SearchResultsComponent**: Displays results from store search state
- **FilterToolbarComponent**: Shares filter settings from player context
- **StorageStore**: Provides search actions and state selectors
- **PlayerContext**: Provides filter settings for search integration

### Styling Strategy

- **Match Filter Toolbar**: Consistent height, padding, layout
- **Flexbox Layout**: Input expands, buttons fixed width
- **Smooth Transitions**: Clear button appearance doesn't shift layout
- **Responsive Design**: Touch-friendly sizes, proper mobile layout
- **CSS Variables**: Use semantic colors from STYLE_GUIDE.md

### Testing Strategy

- **Component Testing**: Test through public API and template interactions
- **Mock Dependencies**: StorageStore and PlayerContext mocked via TestBed
- **Signal Testing**: Verify computed signals update correctly with state changes
- **DOM Testing**: Query rendered elements to verify correct display
- **User Interaction**: Simulate typing, Enter key, button clicks
- **Debounce Testing**: Use fake timers for time-based tests

### Performance Considerations

- **OnPush Change Detection**: Optimal performance with signal-based reactivity
- **Debouncing**: Reduces API calls during typing
- **Trim Input**: Prevents unnecessary searches with whitespace
- **Effect Cleanup**: Clear timeout on component destroy
- **Minimal Re-renders**: Computed signals only update when dependencies change

### Accessibility Considerations

- **Enter Key Support**: Standard keyboard shortcut for search
- **ARIA Labels**: Clear button has proper aria-label
- **Button States**: Disabled states clearly indicated
- **Focus Management**: Input field maintains focus during search
- **Loading States**: Screen readers informed of searching state

### Future Enhancements (Out of Scope)

- Search suggestions/autocomplete
- Recent searches history
- Advanced search operators
- Search within results
- Keyboard shortcuts (Ctrl+F)
- Search result count display
- Search result pagination

### Alternative Implementation: Manual Search Only

If auto-search debouncing is not desired:
1. Remove effect from constructor
2. Remove searchTimeout property
3. Search only on Enter key or button click
4. Simpler implementation
5. More explicit user control
6. Less API calls

Benefits:
- Simpler code
- Fewer API calls
- More predictable behavior
- User controls when to search

Trade-offs:
- Less convenient (requires Enter/click)
- Less modern UX pattern
- May feel less responsive
