# Phase 4: Filter System UI Integration + Error State Visual Feedback

**High Level Plan Documentation**:
- [Player Domain Design](./PLAYER_DOMAIN_DESIGN.md) - Complete architecture and phase planning
- [Player Do- [x] **Error State Display Tests**:
  - [x] Test all filter buttons show `color="error"` when error exists
  - [x] Test error state overrides active filter highlight
  - [x] Test filter buttons return to normal/highlighted when error clears Requirements](./PLAYER_DOMAIN.md) - Business requirements and use cases
- [Phase 1 Documentation](./PLAYER_DOMAIN_P1.md) - File launching with context implementation
- [Phase 2 Documentation](./PLAYER_DOMAIN_P2.md) - Random file launching and shuffle mode implementation
- [Phase 3 Documentation](./PLAYER_DOMAIN_P3.md) - Basic playback controls and file navigation implementation

**Standards Documentation**:

- **Coding Standards**: [../../CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **State Standards**: [../../STATE_STANDARDS.md](../../STATE_STANDARDS.md) - NgRx Signal Store patterns with custom features
- **Store Testing**: [../../STORE_TESTING.md](../../STORE_TESTING.md) - Store unit testing + optional facade integration testing
- **Smart Component Testing**: [../../SMART_COMPONENT_TESTING.md](../../SMART_COMPONENT_TESTING.md) - Testing components with mocked stores/services
- **Component Library**: [../../COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md) - Shared UI components and usage patterns

## 🎯 Objective

Wire up the filter toolbar UI to the existing filter infrastructure and add error state visual feedback across all interactive player buttons. This phase connects the filter-toolbar component to PlayerContextService and provides visual error indicators when player operations fail.

## 🎭 Key Behaviors Being Implemented

### User Workflow - Filter System
1. **User clicks "All" filter button** → filter is set to `PlayerFilterType.All`, button shows cyan highlight color
2. **User clicks "Games" filter button** → filter is set to `PlayerFilterType.Games`, button shows cyan highlight color, previous active button returns to normal
3. **User clicks "Music" filter button** → filter is set to `PlayerFilterType.Music`, button shows cyan highlight color
4. **User clicks "Images" filter button** → filter is set to `PlayerFilterType.Images`, button shows cyan highlight color
5. **User clicks "Random" button in shuffle mode** → random file selection respects active filter
6. **User clicks "Next" in shuffle mode** → next random file respects active filter
7. **User clicks "Previous" in shuffle mode** → previous random file respects active filter
8. **Filter persists across mode switches** → switching from Shuffle to Directory and back preserves filter selection

### User Workflow - Error State Visual Feedback
1. **Random file launch fails (no files match filter)** → all filter buttons turn red, navigation buttons turn red
2. **Navigation fails in shuffle mode** → all filter buttons turn red, navigation buttons turn red
3. **User changes filter after error** → error clears, buttons return to normal/highlighted colors
4. **Error state takes visual precedence** → red error color overrides cyan active filter highlight

### Core Behaviors to Test
- **Filter Click Handlers**: Each filter button calls `PlayerContextService.setFilterMode()` with correct `PlayerFilterType`
- **Active Filter Display**: Currently selected filter shows `color="highlight"` (cyan)
- **Filter State Persistence**: Selected filter persists in `DevicePlayerState.shuffleSettings.filter`
- **Filter API Integration**: Filter passes through to API calls in `launch-random-file`, `navigate-next`, `navigate-previous` actions
- **Error State Display**: All interactive buttons show `color="error"` (red) when `getError(deviceId)` returns truthy value
- **Error State Priority**: Error color takes precedence over active filter highlight
- **Multi-Device Error Isolation**: Error state is device-specific, one device's errors don't affect other devices
- **Error Recovery**: Error state clears when successful operation completes

## 📚 Required Reading

- [x] [Player Domain Design](./PLAYER_DOMAIN_DESIGN.md) - Complete architecture overview
  - [Application Layer Design](./PLAYER_DOMAIN_DESIGN.md#application-layer-design) - IPlayerContext interface patterns
  - [PlayerStore Implementation](./PLAYER_DOMAIN_DESIGN.md#playerstore-implementation) - ShuffleSettings state structure
  - [Action Behaviors](./PLAYER_DOMAIN_DESIGN.md#action-behaviors) - How filter is used in navigation actions
- [x] [Phase 3 Documentation](./PLAYER_DOMAIN_P3.md) - Previous phase implementation details
  - [Bugs Fixed](./PLAYER_DOMAIN_P3.md#-bugs-fixed) - State management patterns and lessons learned
- [x] [Component Library](../../COMPONENT_LIBRARY.md) - IconButtonComponent usage patterns
  - [IconButtonComponent](../../COMPONENT_LIBRARY.md#iconbuttoncomponent) - Color prop and states
- [x] [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) - Component testing with mocked dependencies
- [x] [Theme Styles](../../../libs/ui/styles/src/lib/theme/styles.scss) - CSS custom properties for colors

## 📋 Implementation Tasks

### Pre-Phase: Baseline Verification

**Purpose**: Ensure Phase 3 functionality remains intact and understand existing filter infrastructure.

- [x] Verify Phase 3 tests passing (186/186 player tests)
- [x] Confirm filter infrastructure exists:
  - [x] `PlayerFilterType` enum (All, Games, Music, Images, Hex)
  - [x] `ShuffleSettings.filter` property
  - [x] `setFilterMode(deviceId, filter)` method in PlayerContextService
  - [x] Filter already passes to API in shuffle mode actions
- [x] Understand error state tracking via `getError(deviceId)` signal

### Task 1: Filter Toolbar Component - Click Handlers

**Purpose**: Wire up filter button click handlers to call `PlayerContextService.setFilterMode()` with appropriate filter types.

**Implementation Focus**: Replace TODO console.log statements with actual `setFilterMode()` calls.

#### Implementation Steps
- [x] **Import PlayerFilterType**: Add `PlayerFilterType` to imports from `@teensyrom-nx/domain`
- [x] **Wire up All filter**: Replace `onAllClick()` console.log with `setFilterMode(deviceId, PlayerFilterType.All)`
- [x] **Wire up Games filter**: Replace `onGamesClick()` console.log with `setFilterMode(deviceId, PlayerFilterType.Games)`
- [x] **Wire up Music filter**: Replace `onMusicClick()` console.log with `setFilterMode(deviceId, PlayerFilterType.Music)`
- [x] **Wire up Images filter**: Replace `onImagesClick()` console.log with `setFilterMode(deviceId, PlayerFilterType.Images)`

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.ts`

**Implementation Pattern**:
```typescript
import { PlayerFilterType } from '@teensyrom-nx/domain';

onAllClick(): void {
  this.playerContext.setFilterMode(this.deviceId(), PlayerFilterType.All);
}

onGamesClick(): void {
  this.playerContext.setFilterMode(this.deviceId(), PlayerFilterType.Games);
}

onMusicClick(): void {
  this.playerContext.setFilterMode(this.deviceId(), PlayerFilterType.Music);
}

onImagesClick(): void {
  this.playerContext.setFilterMode(this.deviceId(), PlayerFilterType.Images);
}
```

### Task 2: Filter Toolbar Component - Active Filter State

**Purpose**: Add computed signals to track active filter and error state for visual feedback.

**Implementation Focus**: Create reactive signals that derive from PlayerContextService state.

#### Implementation Steps
- [x] **Add activeFilter computed signal**: Use `getShuffleSettings(deviceId)` to derive current filter, default to `PlayerFilterType.All`
- [x] **Add hasError computed signal**: Use `getError(deviceId)` to check for error state
- [x] **Add getButtonColor helper method**: Implement color logic - error takes precedence, then active highlight, then normal

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.ts`

**Implementation Pattern**:
```typescript
import { computed } from '@angular/core';
import { IconButtonColor } from '@teensyrom-nx/ui/components';

activeFilter = computed(() =>
  this.playerContext.getShuffleSettings(this.deviceId())()?.filter ?? PlayerFilterType.All
);

hasError = computed(() =>
  this.playerContext.getError(this.deviceId())() !== null
);

getButtonColor(filterType: PlayerFilterType): IconButtonColor {
  if (this.hasError()) return 'error';
  return this.activeFilter() === filterType ? 'highlight' : 'normal';
}
```

### Task 3: Filter Toolbar Component - Template Updates

**Purpose**: Bind color prop to filter buttons for active state and error state visual feedback.

**Implementation Focus**: Add `[color]` binding to all filter button components.

#### Implementation Steps
- [x] **Update All filter button**: Add `[color]="getButtonColor(PlayerFilterType.All)"`
- [x] **Update Games filter button**: Add `[color]="getButtonColor(PlayerFilterType.Games)"`
- [x] **Update Music filter button**: Add `[color]="getButtonColor(PlayerFilterType.Music)"`
- [x] **Update Images filter button**: Add `[color]="getButtonColor(PlayerFilterType.Images)"`

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.html`

**Implementation Pattern**:
```html
<lib-icon-button
  icon="all_inclusive"
  ariaLabel="Filter: Allow All Files"
  size="large"
  [color]="getButtonColor(PlayerFilterType.All)"
  (buttonClick)="onAllClick()"
  data-testid="filter-all-button"
/>
```

**Note**: PlayerFilterType enum will need to be accessible in template via component property or direct import.

### Task 4: Player Toolbar Component - Error State for Navigation

**Purpose**: Add error state visual feedback to next/previous navigation buttons.

**Implementation Focus**: Show red error color on navigation buttons when player operations fail.

#### Implementation Steps
- [x] **Add hasError computed signal**: Use `getError(deviceId)` to check for error state
- [x] **Add getNavigationButtonColor helper**: Return `'error'` if hasError, else `'normal'`
- [x] **Update next button template**: Add `[color]="getNavigationButtonColor()"`
- [x] **Update previous button template**: Add `[color]="getNavigationButtonColor()"`

**Files**:
- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.ts`
- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.html`

**Implementation Pattern**:
```typescript
hasError = computed(() =>
  this.playerContext.getError(this.deviceId())() !== null
);

getNavigationButtonColor(): IconButtonColor {
  return this.hasError() ? 'error' : 'normal';
}
```

### Task 5: Component Testing - Filter Toolbar

**Purpose**: Test filter selection behavior, active state display, and error state handling in filter-toolbar component.

**Testing Focus**: Smart component testing with mocked PlayerContext, following [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) patterns.

#### Test Cases to Add
- [x] **Filter Click Behavior Tests**:
  - [x] Test All filter calls `setFilterMode(deviceId, PlayerFilterType.All)`
  - [x] Test Games filter calls `setFilterMode(deviceId, PlayerFilterType.Games)`
  - [x] Test Music filter calls `setFilterMode(deviceId, PlayerFilterType.Music)`
  - [x] Test Images filter calls `setFilterMode(deviceId, PlayerFilterType.Images)`
- [x] **Active Filter Display Tests**:
  - [x] Test All filter shows `color="highlight"` when active
  - [x] Test Games filter shows `color="highlight"` when active
  - [x] Test Music filter shows `color="highlight"` when active
  - [x] Test Images filter shows `color="highlight"` when active
  - [x] Test inactive filters show `color="normal"`
- [x] **Error State Display Tests**:
  - [x] Test all filter buttons show `color="error"` when error exists
  - [x] Test error state overrides active filter highlight
  - [x] Test filter buttons return to normal/highlight when error clears
- [x] **Signal Integration Tests**:
  - [x] Test activeFilter signal updates when shuffleSettings change
  - [x] Test hasError signal updates when error state changes

**File**: `libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.spec.ts`

**Mock Setup Pattern**:
```typescript
// Mock getShuffleSettings to return specific filter
mockPlayerContext.getShuffleSettings.mockReturnValue(
  signal({ scope: PlayerScope.Storage, filter: PlayerFilterType.Music }).asReadonly()
);

// Mock getError for error state tests
mockPlayerContext.getError.mockReturnValue(signal('Random launch failed').asReadonly());
```

### Task 6: Component Testing - Player Toolbar Navigation Buttons

**Purpose**: Test error state display on next/previous navigation buttons.

**Testing Focus**: Verify navigation buttons show error color when player errors occur.

#### Test Cases to Add
- [x] **Navigation Button Error State Tests**:
  - [x] Test next button shows `color="error"` when error exists
  - [x] Test previous button shows `color="error"` when error exists
  - [x] Test navigation buttons show `color="normal"` when no error
  - [x] Test error state clears when error is null

**File**: `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.spec.ts`

### Task 7: Integration Testing - Filter API Integration

**Purpose**: Verify filter setting passes through to API calls in shuffle mode navigation actions.

**Testing Focus**: Integration testing at PlayerContextService level with mocked PlayerService infrastructure.

#### Test Cases to Add
- [x] **Filter Pass-Through Tests**:
  - [x] Test `launchRandomFile()` passes current filter to `PlayerService.launchRandom()`
  - [x] Test `next()` in shuffle mode passes current filter to `PlayerService.launchRandom()`
  - [x] Test `previous()` in shuffle mode passes current filter to `PlayerService.launchRandom()`
  - [x] Test changing filter affects subsequent random launches
- [x] **Filter State Persistence Tests**:
  - [x] Test filter persists when switching from Shuffle to Directory mode
  - [x] Test filter persists when switching from Directory back to Shuffle mode
  - [x] Test filter is device-specific (multiple devices have independent filters)

**File**: `libs/application/src/lib/player/player-context.service.spec.ts`

**Test Pattern**:
```typescript
it('should pass current filter to API when launching random file', async () => {
  const deviceId = 'test-device';
  service.initializePlayer(deviceId);

  // Set filter to Games
  service.setFilterMode(deviceId, PlayerFilterType.Games);

  // Launch random file
  mockPlayerService.launchRandom.mockReturnValue(of(createTestFileItem()));
  await service.launchRandomFile(deviceId);

  // Verify API was called with Games filter
  expect(mockPlayerService.launchRandom).toHaveBeenCalledWith(
    deviceId,
    PlayerScope.Storage,
    PlayerFilterType.Games, // Current filter
    undefined
  );
});
```

### Task 8: Documentation Updates

**Purpose**: Update design documentation to include Phase 4 in the incremental development roadmap.

#### Implementation Steps
- [x] **Update PLAYER_DOMAIN_DESIGN.md**: Insert Phase 4 section between Phase 3 and Phase 5 (formerly Phase 4)
- [x] **Renumber subsequent phases**: Phase 5 becomes "Timer System + Auto-Progression" (formerly Phase 4)
- [x] **Update cross-references**: Ensure all phase references are updated throughout documentation

**File**: `docs/features/player-state/PLAYER_DOMAIN_DESIGN.md`

**Phase 4 Section Content**:
```markdown
### Phase 4: Filter System UI Integration + Error State Visual Feedback
**Goal**: Wire up filter toolbar to existing filter infrastructure and add error state visual indicators

**Scope**:
- Connect filter button click handlers to `setFilterMode()`
- Visual feedback for active filter selection (cyan highlight)
- Visual feedback for error states (red color on all buttons)
- Error state takes precedence over active state
- **NO** new backend logic - all filter infrastructure already exists from Phase 2

**Implementation**:
- Filter toolbar click handlers call `setFilterMode()`
- Active filter signal derived from `getShuffleSettings()`
- Error state signal derived from `getError()`
- Color binding logic in components (error > highlight > normal)

**Demonstrable Value**: Users see which filter is active and receive clear visual feedback when operations fail
```

## 🗂️ File Changes

### Filter Toolbar Component (Modified)
- `libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.ts`
  - Add imports: `PlayerFilterType`, `IconButtonColor`, `computed`
  - Replace 4 click handler implementations
  - Add `activeFilter` computed signal
  - Add `hasError` computed signal
  - Add `getButtonColor(filterType)` helper method
  - Expose `PlayerFilterType` for template access

- `libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.html`
  - Add `[color]` binding to all 4 filter buttons

- `libs/features/player/src/lib/player-view/player-device-container/storage-container/filter-toolbar/filter-toolbar.component.spec.ts`
  - Add filter click behavior tests (4 tests)
  - Add active filter display tests (5 tests)
  - Add error state display tests (3 tests)
  - Add signal integration tests (2 tests)

### Player Toolbar Component (Modified)
- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.ts`
  - Add imports: `IconButtonColor`, `computed`
  - Add `hasError` computed signal
  - Add `getNavigationButtonColor()` helper method

- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.html`
  - Add `[color]` binding to next button
  - Add `[color]` binding to previous button

- `libs/features/player/src/lib/player-view/player-device-container/player-toolbar/player-toolbar.component.spec.ts`
  - Add navigation button error state tests (4 tests)

### Integration Tests (Modified)
- `libs/application/src/lib/player/player-context.service.spec.ts`
  - Add filter pass-through integration tests (4 tests)
  - Add filter persistence tests (3 tests)

### Documentation (Modified)
- `docs/features/player-state/PLAYER_DOMAIN_DESIGN.md`
  - Insert Phase 4 section
  - Renumber Phase 5 (Timer System)
  - Update phase cross-references

## 🧪 Testing Requirements

**Testing Strategy**: Focus on behavioral testing at component and integration layers, following established patterns from Phase 1-3.

### Component Testing - Filter Toolbar

**Purpose**: Test filter selection behavior and visual feedback using mocked PlayerContext.

**Key Test Areas**:
- **Click Handler Integration**: Verify each filter button calls `setFilterMode()` with correct `PlayerFilterType`
- **Active State Display**: Verify active filter shows `color="highlight"`, others show `color="normal"`
- **Error State Display**: Verify all buttons show `color="error"` when error exists
- **Error State Priority**: Verify error color overrides active filter highlight
- **Signal Reactivity**: Verify computed signals update when underlying state changes

**Mocking Strategy**: Mock `PLAYER_CONTEXT` with strongly typed mock, return signal mocks for `getShuffleSettings()` and `getError()`

### Component Testing - Player Toolbar

**Purpose**: Test error state visual feedback on navigation buttons.

**Key Test Areas**:
- **Navigation Button Error State**: Verify next/previous buttons show `color="error"` when error exists
- **Error State Recovery**: Verify buttons return to `color="normal"` when error clears

### Integration Testing - PlayerContextService

**Purpose**: Verify filter passes through to infrastructure API calls in shuffle mode.

**Key Test Areas**:
- **Filter API Integration**: Verify `launchRandom()` receives current filter value from `shuffleSettings`
- **Filter Persistence**: Verify filter survives mode switches (Shuffle → Directory → Shuffle)
- **Multi-Device Isolation**: Verify each device maintains independent filter settings

**Mocking Strategy**: Mock `PLAYER_SERVICE` infrastructure, use real PlayerStore for state management integration

## ✅ Success Criteria

- [x] **Backend verification**: Filter infrastructure already complete from Phase 2
- [x] All filter buttons call `setFilterMode()` with correct `PlayerFilterType` values
- [x] Active filter button displays with `color="highlight"` (cyan: #00f7ff)
- [x] Inactive filter buttons display with `color="normal"` (default theme color)
- [x] All filter buttons display with `color="error"` (red: #cc666c light, #ff6f6f dark) when player error exists
- [x] Next and previous navigation buttons display with `color="error"` when player error exists
- [x] Error state visual feedback takes precedence over active filter highlight
- [x] Filter setting passes through to `PlayerService.launchRandom()` API calls in shuffle mode
- [x] Filter persists across launch mode switches (Shuffle ↔ Directory)
- [x] Each device maintains independent filter settings
- [x] All component tests passing for filter-toolbar and player-toolbar
- [x] All integration tests passing for filter API pass-through
- [x] PLAYER_DOMAIN_DESIGN.md updated with Phase 4 section
- [x] Ready to proceed to Phase 5 (Timer System + Auto-Progression)

## 📝 Notes

### Filter Infrastructure Status
- **100% Complete**: All backend infrastructure exists from Phase 2 implementation
- **ShuffleSettings.filter**: Already tracked in PlayerStore per device
- **setFilterMode()**: Already implemented in PlayerContextService
- **API Integration**: All shuffle mode actions already read and pass filter to API
- **Default Value**: `PlayerFilterType.All` set in `createDefaultDeviceState()`

### Phase 4 Scope Clarification
- **UI Integration Only**: This phase is purely UI wiring - no new state management or API logic
- **Two Feature Areas**: Filter selection + Error state visual feedback
- **Small Phase**: Estimated ~200 lines of new code + tests
- **No Hex Filter**: UI exposes 4 filters (All, Games, Music, Images) - Hex enum value exists but not exposed

### IconButtonComponent Color System
- **Available Colors**: `'normal' | 'highlight' | 'success' | 'error' | 'dimmed'`
- **CSS Variables**:
  - `--color-highlight`: #00f7ff (cyan)
  - `--color-error`: #cc666c (light mode), #ff6f6f (dark mode)
- **Color Precedence**: Error > Highlight > Normal (implemented in `getButtonColor()` helper)

### Error State Behavior
- **Error Source**: `DevicePlayerState.error` populated by failed actions (launch-random-file, navigate-next, navigate-previous)
- **Error Clearing**: Error clears on next successful operation (see Phase 3 bug fixes)
- **Error Isolation**: Each device has independent error state
- **Visual Feedback Goal**: Users immediately see when operations fail without reading error messages

### Filter Scope
- **Shuffle Mode Only**: Filter only applies when `LaunchMode.Shuffle` is active
- **Directory Mode**: Filter does NOT affect directory navigation (shows all files in directory regardless of type)
- **State Persistence**: Filter setting persists even when switching to Directory mode, ready when switching back to Shuffle

### Testing Approach
- **Component Tests**: Use mocked `PLAYER_CONTEXT` for fast, isolated UI behavior testing
- **Integration Tests**: Use real PlayerStore + mocked `PLAYER_SERVICE` to verify filter flows through system
- **No E2E Required**: Phase 4 is UI-only, component + integration tests sufficient

### Design Patterns from Phase 3
- **Computed Signals**: Use `computed()` for derived state (activeFilter, hasError)
- **Helper Methods**: Extract color logic to testable helper methods (`getButtonColor()`, `getNavigationButtonColor()`)
- **Type Safety**: Use `IconButtonColor` type for all color values
- **Signal Exposure**: Expose signals for template consumption, don't subscribe in component code

## 🔗 Related Documentation

- [Phase 3 Documentation](./PLAYER_DOMAIN_P3.md) - Previous phase with error state management patterns
- [Phase 5 Documentation](./PLAYER_DOMAIN_P5.md) - Next phase: Timer System + Auto-Progression (to be created)
- [Player Domain Design](./PLAYER_DOMAIN_DESIGN.md) - Overall architecture and phase planning
- [Component Library - IconButtonComponent](../../COMPONENT_LIBRARY.md#iconbuttoncomponent) - Color prop documentation
- [Theme Styles](../../../libs/ui/styles/src/lib/theme/styles.scss) - CSS color variables
