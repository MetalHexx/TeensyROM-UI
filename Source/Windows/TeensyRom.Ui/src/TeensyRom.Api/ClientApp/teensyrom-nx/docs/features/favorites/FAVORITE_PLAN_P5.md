# Phase 5: Player Toolbar Favorite Button UI

## 🎯 Objective

Add a favorite button to the player toolbar that displays favorite status and triggers favorite/unfavorite actions. The button shows visual state changes (empty heart vs solid heart) based on the current file's favorite status and integrates with existing player controls. The button is disabled during favorite operations to prevent concurrent requests.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**

- [ ] [Favorite Files Feature Plan](./FAVORITE_PLAN.md) - High-level feature plan with architecture overview
- [ ] [Favorite Files Feature Plan - Phase 5](./FAVORITE_PLAN.md#phase-5-frontend---player-toolbar-favorite-button-ui) - Specific phase 5 requirements

**Standards & Guidelines:**

- [ ] [Coding Standards](../../CODING_STANDARDS.md) - Component structure, input/output patterns, modern control flow
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing philosophy and layer-specific guidance
- [ ] [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) - Testing feature components with mocked dependencies
- [ ] [Style Guide](../../STYLE_GUIDE.md) - Material icon usage and component styling patterns

---

## 📂 File Structure Overview

> Files showing new (✨) and modified (📝) components for Phase 5

```
libs/features/player/src/lib/player-view/player-device-container/player-toolbar/
└── player-toolbar-actions/
    ├── player-toolbar-actions.component.ts       📝 Modified - Add favorite button logic
    ├── player-toolbar-actions.component.html     📝 Modified - Add favorite button markup
    ├── player-toolbar-actions.component.spec.ts  📝 Modified - Add favorite button tests
    └── player-toolbar-actions.component.scss     📝 Modified - Add favorite button styles (if needed)
```

---

<details open>
<summary><h3>Task 1: Add Favorite Button to Player Toolbar Actions Component</h3></summary>

**Purpose**: Integrate the favorite button into the existing `player-toolbar-actions.component.ts` alongside the shuffle button, following the same patterns for state management and user interaction.

**Related Documentation:**

- [player-toolbar-actions.component.ts](../../../../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar-actions/player-toolbar-actions.component.ts) - Existing shuffle button implementation to follow
- [Icon Button Component](../../../../libs/ui/components/src/lib/icon-button/icon-button.component.ts) - UI component to use
- [Storage Store Actions](../../../../libs/application/src/lib/storage/actions/index.ts) - Store actions to call

**Implementation Subtasks:**

- [ ] **Import StorageStore**: Add import for `StorageStore` from `@teensyrom-nx/application`
- [ ] **Inject StorageStore**: Add `private readonly storageStore = inject(StorageStore)` following the same pattern as `playerContext`
- [ ] **Create `toggleFavorite()` method**: Async method that reads current file from player context, checks favorite status, and calls appropriate storage store action
- [ ] **Create `isFavorite()` method**: Returns boolean by reading `isFavorite` flag from current file obtained via `playerContext.getCurrentFile(deviceId)()`
- [ ] **Create `isFavoriteOperationInProgress()` method**: Returns `storageStore.favoriteOperationsState().isProcessing` to disable button during operations

**Testing Subtask:**

- [ ] **Write Tests**: Test favorite button behaviors (see Testing section below)

**Key Implementation Notes:**

- Follow the exact same pattern as `toggleShuffleMode()` and `isShuffleMode()` methods
- Use `playerContext.getCurrentFile(deviceId)()?.file` to get current file (may be null)
- Handle null file gracefully - button should be disabled if no file loaded
- Call `storageStore.saveFavorite()` when `isFavorite()` returns false
- Call `storageStore.removeFavorite()` when `isFavorite()` returns true
- Pass `deviceId`, `storageType`, and `filePath` to storage store actions from current file
- Button disabled when `!currentFile || isFavoriteOperationInProgress()`

**Testing Focus for Task 1:**

**Behaviors to Test:**

- [ ] **Initialization**: Component initializes with both `playerContext` and `storageStore` injected
- [ ] **isFavorite() returns true**: When current file has `isFavorite: true` flag
- [ ] **isFavorite() returns false**: When current file has `isFavorite: false` flag
- [ ] **isFavorite() handles null file**: Returns false when no current file loaded
- [ ] **toggleFavorite() calls saveFavorite**: When current file is not favorited, calls `storageStore.saveFavorite()` with correct parameters
- [ ] **toggleFavorite() calls removeFavorite**: When current file is favorited, calls `storageStore.removeFavorite()` with correct parameters
- [ ] **toggleFavorite() handles null file**: Does nothing when no current file loaded
- [ ] **isFavoriteOperationInProgress() reflects store state**: Returns true when `storageStore.favoriteOperationsState().isProcessing` is true

**Testing Reference:**

- See [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) for component testing patterns
- Mock `IPlayerContext` and `StorageStore` using interfaces
- Use signal mocks for reactive state (`getCurrentFile`, `favoriteOperationsState`)

</details>

---

<details open>
<summary><h3>Task 2: Add Favorite Button Markup to Template</h3></summary>

**Purpose**: Add the favorite button to the component template alongside the shuffle button, using appropriate Material icons and binding to component methods.

**Related Documentation:**

- [player-toolbar-actions.component.html](../../../../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar-actions/player-toolbar-actions.component.html) - Existing shuffle button markup to follow
- [Icon Button Component](../../../../libs/ui/components/src/lib/icon-button/icon-button.component.ts) - Component API reference
- [Material Icons](https://fonts.google.com/icons) - Icon names reference

**Implementation Subtasks:**

- [ ] **Add lib-icon-button**: Add second `<lib-icon-button>` element inside `.toolbar-actions` div
- [ ] **Set icon binding**: Use `@if/@else` control flow - `icon="favorite"` when `isFavorite()` is true, `icon="favorite_border"` when false
- [ ] **Set ariaLabel binding**: Use descriptive label that changes based on state - "Remove from Favorites" vs "Add to Favorites"
- [ ] **Set size**: Use `size="large"` to match shuffle button
- [ ] **Set variant**: Use `variant="rounded-transparent"` to match shuffle button
- [ ] **Set color binding**: Use `color="highlight"` when `isFavorite()` is true, `color="normal"` when false
- [ ] **Set disabled binding**: Use `[disabled]="!currentFile() || isFavoriteOperationInProgress()"` to disable when no file or operation in progress
- [ ] **Bind click event**: Use `(buttonClick)="toggleFavorite()"` to trigger action
- [ ] **Add computed signal for currentFile**: Add `currentFile = computed(() => this.playerContext.getCurrentFile(this.deviceId())())` to component for template access

**Testing Subtask:**

- [ ] **Write Tests**: Test template rendering and user interactions (see Testing section below)

**Key Implementation Notes:**

- Position favorite button after shuffle button in the DOM order
- Use `@if (isFavorite()) { } @else { }` control flow for conditional icon
- Both icons are Material icons: `favorite` (solid heart) and `favorite_border` (empty heart)
- Color changes automatically through binding - highlight when favorited, normal otherwise
- Aria label should clearly communicate the action that will occur on click
- Disabled state prevents clicks during operations or when no file is loaded

**Testing Focus for Task 2:**

**Behaviors to Test:**

- [ ] **Empty heart icon displayed**: When `isFavorite()` returns false, template shows `favorite_border` icon
- [ ] **Solid heart icon displayed**: When `isFavorite()` returns true, template shows `favorite` icon
- [ ] **Normal color displayed**: When not favorited, button uses `color="normal"`
- [ ] **Highlight color displayed**: When favorited, button uses `color="highlight"`
- [ ] **Button enabled**: When file loaded and no operation in progress, button is enabled
- [ ] **Button disabled - no file**: When no file loaded, button is disabled
- [ ] **Button disabled - operation in progress**: When favorite operation processing, button is disabled
- [ ] **Click triggers toggleFavorite**: Clicking button calls `toggleFavorite()` method
- [ ] **Aria label reflects action**: Label changes based on current favorite state

**Testing Reference:**

- See [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) for DOM testing patterns
- Use `fixture.debugElement.query()` to locate button element
- Assert on icon content, color classes, and disabled attribute
- Simulate click events to verify method calls

</details>

---

<details open>
<summary><h3>Task 3: Add Styling (Optional)</h3></summary>

**Purpose**: Add any necessary styling to position or style the favorite button, though it should inherit most styling from the icon-button component and toolbar layout.

**Related Documentation:**

- [Style Guide](../../STYLE_GUIDE.md) - Global utility classes and design tokens
- [player-toolbar-actions.component.scss](../../../../libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar-actions/player-toolbar-actions.component.scss) - Existing component styles

**Implementation Subtasks:**

- [ ] **Review existing styles**: Check if additional styling is needed for button spacing or layout
- [ ] **Add spacing rules**: If needed, add margin or gap between shuffle and favorite buttons
- [ ] **Verify responsive behavior**: Ensure button displays correctly on mobile, tablet, and desktop layouts
- [ ] **Test visual consistency**: Verify button matches design system and aligns with shuffle button

**Testing Subtask:**

- [ ] **Manual Visual Testing**: Verify button appearance and spacing across different screen sizes

**Key Implementation Notes:**

- Most styling should be inherited from `.toolbar-actions` container and `lib-icon-button` component
- If buttons need spacing, use CSS gap or margin-left on favorite button
- Follow existing SCSS patterns in the component's stylesheet
- Consult [Style Guide](../../STYLE_GUIDE.md) for utility classes if needed

**Testing Focus for Task 3:**

**Visual Behaviors to Verify:**

- [ ] **Button spacing**: Appropriate space between shuffle and favorite buttons
- [ ] **Alignment**: Buttons align vertically and horizontally with other toolbar controls
- [ ] **Responsive layout**: Buttons remain usable on mobile, tablet, and desktop
- [ ] **Hover states**: Button hover effects work correctly (inherited from icon-button)
- [ ] **Focus states**: Button focus indicators are visible and accessible

**Testing Reference:**

- Manual testing in browser at different viewport sizes
- Use browser DevTools to inspect layout and spacing
- Verify against existing shuffle button as reference

</details>

---

## 🗂️ Files Modified or Created

**Modified Files:**

- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar-actions/player-toolbar-actions.component.ts`
- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar-actions/player-toolbar-actions.component.html`
- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar-actions/player-toolbar-actions.component.spec.ts`
- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar-actions/player-toolbar-actions.component.scss` (if styling needed)

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **Tests are written within each task above, not here.** This section summarizes the testing approach.

### Testing Philosophy

- **Component Layer Testing**: Mock application layer dependencies (`IPlayerContext`, `StorageStore`) using interfaces
- **Behavioral Focus**: Test observable outcomes (what users see and interact with), not implementation details
- **Signal-Based Mocking**: Create writable signals in test setup, return readonly versions from mock methods
- **Public API Testing**: Test through component's public interface (methods, template bindings, DOM interactions)

### Test Organization

**Task 1 Tests** (Component Logic):

- Service injection and initialization
- Method behaviors (`isFavorite()`, `toggleFavorite()`, `isFavoriteOperationInProgress()`)
- Store action calls with correct parameters
- Null file handling

**Task 2 Tests** (Template & DOM):

- Icon rendering based on favorite state
- Color changes based on favorite state
- Button disabled/enabled states
- Click event handling
- Aria label updates

**Task 3 Tests** (Visual):

- Manual browser testing at different viewport sizes
- Spacing and alignment verification
- Responsive layout validation

### Mock Setup Pattern

```typescript
// Mock IPlayerContext with signal-based state
let mockPlayerContext: Partial<IPlayerContext>;
let currentFileSignal: WritableSignal<LaunchedFile | null>;

// Mock StorageStore with signal-based state
let mockStorageStore: Partial<StorageStore>;
let favoriteOperationsStateSignal: WritableSignal<FavoriteOperationsState>;

beforeEach(() => {
  currentFileSignal = signal(null);
  favoriteOperationsStateSignal = signal({ isProcessing: false, error: null });

  mockPlayerContext = {
    getCurrentFile: vi.fn(() => signal(currentFileSignal()).asReadonly()),
  };

  mockStorageStore = {
    saveFavorite: vi.fn(),
    removeFavorite: vi.fn(),
    favoriteOperationsState: signal(favoriteOperationsStateSignal()).asReadonly(),
  };

  TestBed.configureTestingModule({
    providers: [
      { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
      { provide: StorageStore, useValue: mockStorageStore },
    ],
  });
});
```

### Test Execution

```bash
# Run component tests
npx nx test player

# Run tests in watch mode during development
npx nx test player --watch

# Run with coverage
npx nx test player --coverage
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

**Functional Requirements:**

- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Favorite button appears in player toolbar actions alongside shuffle button
- [ ] Button displays empty heart icon when file not favorited
- [ ] Button displays solid heart icon when file is favorited
- [ ] Button changes color based on favorite state (normal/highlight)
- [ ] Button disabled when no file loaded
- [ ] Button disabled during favorite operations
- [ ] Clicking button calls appropriate storage store action (`saveFavorite` or `removeFavorite`)
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)

**Testing Requirements:**

- [ ] All testing subtasks completed within each task
- [ ] Component logic tests pass (Task 1)
- [ ] Template rendering tests pass (Task 2)
- [ ] Visual styling verified (Task 3)
- [ ] Mock setup uses interfaces (`IPlayerContext`, `StorageStore`)
- [ ] Tests verify behaviors, not implementation details
- [ ] All tests passing with no failures

**Quality Checks:**

- [ ] No TypeScript errors or warnings
- [ ] Linting passes (`npx nx lint player`)
- [ ] Component follows Angular 19 modern patterns (signals, control flow)
- [ ] Accessibility verified (aria labels, keyboard navigation)
- [ ] No console errors when running application

**Integration Verification:**

- [ ] Favorite button works correctly with existing player controls
- [ ] Alert notifications appear on success/error (handled by infrastructure service)
- [ ] Button state updates correctly when file changes in player
- [ ] Storage store actions update file favorite status correctly
- [ ] UI remains responsive during favorite operations

**Ready for Next Phase:**

- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Ready to proceed to Phase 6 (E2E tests)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Component Placement**: Favorite button added to `player-toolbar-actions.component.ts` alongside shuffle button for consistent grouping of secondary player controls
- **State Source**: Favorite status read from current file via `playerContext.getCurrentFile()` rather than maintaining separate state
- **Store Integration**: Component calls storage store actions directly, following same pattern as shuffle mode with player context
- **Loading State**: Button disabled during operations by checking `storageStore.favoriteOperationsState().isProcessing` to prevent concurrent requests
- **Alert Handling**: Success/error notifications handled automatically by infrastructure service, no component-level error display needed

### Implementation Constraints

- **Null File Handling**: Button must handle null current file gracefully (no file loaded in player)
- **Signal Reactivity**: All state must be signal-based for automatic UI updates when favorite status changes
- **Operation Safety**: Button must be disabled during operations to prevent race conditions
- **Consistent UX**: Button behavior and styling must match existing shuffle button patterns

### Integration Points

- **Player Context**: Component uses `IPlayerContext.getCurrentFile()` to get current file with favorite status
- **Storage Store**: Component calls `StorageStore.saveFavorite()` and `StorageStore.removeFavorite()` actions
- **Infrastructure Service**: Alert notifications displayed automatically by `StorageService` on success/error
- **Icon Button Component**: Reuses existing `lib-icon-button` UI component for consistent styling

### Testing Approach

- **Mock Interfaces**: Test uses interface-based mocks (`IPlayerContext`, `StorageStore`) not concrete classes
- **Signal Mocking**: Writable signals created in tests to simulate reactive state changes
- **Behavioral Testing**: Tests verify observable outcomes (icon display, button state, method calls)
- **DOM Testing**: Template tests verify correct rendering and event handling

### Future Enhancements

- **Keyboard Shortcut**: Add keyboard shortcut (e.g., Ctrl+D) to toggle favorite status
- **Tooltip**: Add tooltip showing favorite status on hover
- **Animation**: Animate icon transition when toggling favorite state
- **Batch Operations**: Support favoriting multiple selected files at once
- **Favorite Count Badge**: Show count of favorited items in navigation

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

</details>

---

## 💡 Key Takeaways for Implementation

1. **Follow Shuffle Button Pattern**: The existing shuffle button in `player-toolbar-actions.component.ts` provides the exact pattern to follow for the favorite button
2. **Signal-Based State**: All state access uses signals for automatic reactivity - `playerContext.getCurrentFile()()` and `storageStore.favoriteOperationsState()`
3. **Disable During Operations**: Critical to disable button when `favoriteOperationsState().isProcessing` is true to prevent concurrent requests
4. **Infrastructure Handles Alerts**: No need to display alerts in component - infrastructure service automatically shows success/error messages
5. **Null File Safety**: Handle null current file gracefully - button disabled when no file loaded
6. **Interface-Based Testing**: Mock `IPlayerContext` and `StorageStore` using interfaces in tests, not concrete implementations
