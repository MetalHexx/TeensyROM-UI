# Phase 2: Random File Launching (Shuffle Mode)

**High Level Plan Documentation**:

- [Player Domain Design](./PLAYER_DOMAIN_DESIGN.md) - Complete architecture and phase planning
- [Player Domain Requirements](./PLAYER_DOMAIN.md) - Business requirements and use cases
- [Phase 1 Documentation](./PLAYER_DOMAIN_P1.md) - Previous phase implementation

**Standards Documentation**:

- **Coding Standards**: [../../CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **State Standards**: [../../STATE_STANDARDS.md](../../STATE_STANDARDS.md) - NgRx Signal Store patterns with custom features
- **Store Testing**: [../../STORE_TESTING.md](../../STORE_TESTING.md) - Store unit testing + optional facade integration testing
- **Smart Component Testing**: [../../SMART_COMPONENT_TESTING.md](../../SMART_COMPONENT_TESTING.md) - Testing components with mocked stores/services
- **Component Library**: [../../COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md) - Shared UI components and usage patterns

## 🎯 Objective

Add shuffle mode capability with two UI controls: a random launch button (dice icon) for launching random files and a shuffle toggle button (shuffle icon) for enabling/disabling shuffle mode. Both buttons use the existing `IconButtonComponent` with state-driven styling that reflects the current shuffle state.

## 🎭 Key Behaviors Being Implemented

### User Workflow

1. **User clicks shuffle toggle button** → enables/disables shuffle mode (shuffle icon highlights when active)
2. **User configures shuffle scope and filters** → sets preferences for random file selection
3. **User clicks random launch button** → launches random file based on current shuffle settings (dice icon)
4. **System tracks shuffle state** → buttons reflect current mode with appropriate highlighting
5. **Multi-device support** → each TeensyROM device maintains independent shuffle state and UI controls

### Core Behaviors to Test

- **Shuffle Toggle Button**: Shows current shuffle mode state with highlight color when active, normal when inactive
- **Random Launch Button**: Launches random files using configured shuffle settings and scope
- **State-Driven Styling**: Buttons use `highlight` color when active/enabled, `normal` when inactive following [IconButtonComponent](../../COMPONENT_LIBRARY.md#iconbuttoncomponent) patterns
- **Store Integration**: UI reads shuffle state from PlayerStore signals and calls PlayerContext actions
- **Multi-Device Isolation**: Each device UI reflects independent shuffle state without cross-device interference
- **Scope Configuration**: Support for TeensyROM Global, Storage Global, Directory Shallow/Deep modes
- **Filter Management**: Content filtering for All, Games, Music, Images file types

## 📚 Required Reading

- [ ] [Player Domain Design](./PLAYER_DOMAIN_DESIGN.md) - Complete architecture overview and design principles
  - [Clean Architecture Structure](./PLAYER_DOMAIN_DESIGN.md#clean-architecture-structure) - File organization and layer boundaries
  - [Domain Models](./PLAYER_DOMAIN_DESIGN.md#domain-models) - Shared models and player-specific types for shuffle functionality
  - [Domain Contracts](./PLAYER_DOMAIN_DESIGN.md#domain-contracts) - IPlayerService interface extension for random launching
  - [Application Layer Design](./PLAYER_DOMAIN_DESIGN.md#application-layer-design) - IPlayerContext interface and orchestration patterns
  - [PlayerStore Implementation](./PLAYER_DOMAIN_DESIGN.md#playerstore-implementation) - Store structure and action patterns for shuffle
  - [Infrastructure Layer Design](./PLAYER_DOMAIN_DESIGN.md#infrastructure-layer-design) - PlayerService implementation patterns
  - [Phase 2 Scope](./PLAYER_DOMAIN_DESIGN.md#phase-2-random-file-launching-shuffle-mode) - Specific Phase 2 implementation details
  - [Shuffle Mode Behaviors](./PLAYER_DOMAIN_DESIGN.md#shuffle-mode) - Scope options, filter management, and random selection
  - [Application State Models](./PLAYER_DOMAIN_DESIGN.md#application-state-models) - ShuffleSettings interface and DevicePlayerState extensions
- [ ] [Component Library](../../COMPONENT_LIBRARY.md) - IconButtonComponent usage patterns and color system
  - [IconButtonComponent](../../COMPONENT_LIBRARY.md#iconbuttoncomponent) - Properties, events, color mapping, and accessibility features
  - [Style Integration](../../COMPONENT_LIBRARY.md#style-integration) - Color mapping to design system (`highlight` vs `normal`)
- [ ] [State Standards](../../STATE_STANDARDS.md) - NgRx Signal Store patterns with custom features
- [ ] [Store Testing](../../STORE_TESTING.md) - Store testing methodology and facade testing approach
- [ ] [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) - Component testing with mocked dependencies
- [ ] [Storage Key Util](../../../libs/application/src/lib/storage/storage-key.util.ts) - Cross-domain reference pattern
- [ ] [Domain Mapper](../../../libs/infrastructure/src/lib/domain.mapper.ts) - Centralized API transformation

## 📋 Implementation Tasks

### Pre-Phase: Establish Baseline

**Purpose**: Ensure existing tests pass and Phase 1 functionality intact before beginning Phase 2 implementation.

- [x] Run all existing tests: `npx nx test`
- [x] Verify existing build works: `npx nx build teensyrom-ui`
- [x] Confirm Phase 1 functionality works (file launching with context from double-click)
- [x] Document any failing tests or build issues that need to be addressed
- [x] Establish clean baseline before adding new code

### Task 1: Domain Layer Extensions

**Purpose**: Add player-specific domain types for shuffle functionality as defined in [Domain Models](./PLAYER_DOMAIN_DESIGN.md#domain-models).

**Phase 2 Domain Types**: Shuffle-specific enums required for random file launching and scope management.

#### Implementation Steps

- [x] Add `PlayerFilterType` enum to `libs/domain/src/lib/models/player-filter-type.enum.ts`
  - Values: `All`, `Games`, `Music`, `Images`, `Hex` (from [Player-Specific Domain Types](./PLAYER_DOMAIN_DESIGN.md#player-specific-domain-types))
- [x] Add `PlayerScope` enum to `libs/domain/src/lib/models/player-scope.enum.ts`
  - Values: `Storage`, `DirectoryDeep`, `DirectoryShallow` (from [Player-Specific Domain Types](./PLAYER_DOMAIN_DESIGN.md#player-specific-domain-types))
- [x] Export new enums in `libs/domain/src/lib/models/index.ts`
- [x] Update domain barrel export in `libs/domain/src/index.ts`

**Note**: Advanced control enums (SpeedCurveType, SeekMode, etc.) remain deferred to future phases as per [Phase 2 scope](./PLAYER_DOMAIN_DESIGN.md#phase-2-random-file-launching-shuffle-mode).

### Task 2: Domain Contracts Extension

**Purpose**: Extend clean domain contract for random file launching as specified in [Domain Contracts](./PLAYER_DOMAIN_DESIGN.md#domain-contracts).

**Contract Focus**: Simple infrastructure operations for random file selection with filtering and scope configuration.

#### Implementation Steps

- [x] Extend `IPlayerService` interface in `libs/domain/src/lib/contracts/player.contract.ts`
  - Add `launchRandom(deviceId: string, scope: PlayerScope, filter: PlayerFilterType, startingDirectory?: string): Observable<FileItem>` (core Phase 2 operation)
  - Follow [IPlayerService Interface specification](./PLAYER_DOMAIN_DESIGN.md#iplayerservice-interface-libsdomainsrclibcontractsplayercontractts)
- [x] Maintain focus on infrastructure concerns only - no application logic

**Important**: Keep interface focused on infrastructure concerns only. All complex behaviors go in IPlayerContext (application layer).

### Task 3: Infrastructure Layer - TDD Implementation

**Purpose**: Implement random file launching infrastructure following [Infrastructure Layer Design](./PLAYER_DOMAIN_DESIGN.md#infrastructure-layer-design) patterns.

**Implementation Focus**: Simple infrastructure operations as defined in [PlayerService Behaviors](./PLAYER_DOMAIN_DESIGN.md#playerservice-behaviors) - stateless API integration only.

**Behaviors Being Built**:

- Call TeensyROM API to launch random file on specific device with scope and filter parameters
- Transform API responses to domain `FileItem` models using centralized [Domain Mapper](../../../libs/infrastructure/src/lib/domain.mapper.ts)
- Handle API errors gracefully (network failures, device not found, invalid scope/filter combinations)
- Return properly typed domain models for application layer consumption
- **No application logic** - pure infrastructure implementation

#### Step 3A: Write Failing Tests

- [x] **Random Launch API Integration**: Test `launchRandom(deviceId, scope, filter, startingDirectory)` calls correct API endpoint
- [x] **Parameter Mapping**: Test scope and filter enums properly mapped to API parameters
- [x] **Domain Model Mapping**: Test API responses mapped to `FileItem` using `DomainMapper.toFileItem()`
- [x] **Error Scenarios**: Test random selection failures, scope errors, filter errors return proper error messages
- [x] **Contract Compliance**: Test service implements extended `IPlayerService` interface correctly
- [x] Verify tests fail (red phase)

#### Step 3B: Implement to Pass Tests

- [x] Extend `PlayerService` in `libs/infrastructure/src/lib/player/player.service.ts`
- [x] Implement `launchRandom()` method calling `PlayerApiService.launchRandom()` (or equivalent endpoint)
- [x] Integrate with `DomainMapper.toFileItem()` for response transformation
- [x] Add proper error handling for HTTP failures and random selection errors
- [x] Verify tests pass (green phase)

### Task 4: Application Layer State Extensions - TDD Implementation

**Purpose**: Extend PlayerStore with shuffle state following [PlayerStore Implementation](./PLAYER_DOMAIN_DESIGN.md#playerstore-implementation).

**Architecture Setup**: Add shuffle settings to [Application State Models](./PLAYER_DOMAIN_DESIGN.md#application-state-models) and implement shuffle-specific actions and selectors following established [Action Behaviors](./PLAYER_DOMAIN_DESIGN.md#action-behaviors) and [Selector Behaviors](./PLAYER_DOMAIN_DESIGN.md#selector-behaviors) patterns.

#### Step 4A: Add State Models

- [x] Create `ShuffleSettings` interface in player state models following [Application State Models](./PLAYER_DOMAIN_DESIGN.md#application-state-models)
  - Properties: `scope: PlayerScope`, `filter: PlayerFilterType`, `startingDirectory?: string`
- [x] Extend `DevicePlayerState` interface to include `shuffleSettings: ShuffleSettings`
- [x] Update initial state structure with default shuffle settings (scope: Storage, filter: All)

#### Step 4B: Write Failing Tests

- [x] **Launch Random Action**: Test `launch-random-file` action calls infrastructure and updates state
- [x] **Shuffle Settings Management**: Test `update-shuffle-settings` action updates device-specific settings
- [x] **Launch Mode Switching**: Test mode switching between Directory and Shuffle clears file context but preserves current file
- [x] **State Isolation**: Test shuffle settings independent per device
- [x] **Selector Access**: Test `get-shuffle-settings` and `get-launch-mode` selectors return correct signals
- [x] Verify tests fail (red phase)

#### Step 4C: Implement Actions & Selectors

- [x] Implement `launch-random-file.ts` action following [launch-random-file action behavior](./PLAYER_DOMAIN_DESIGN.md#launch-random-file-shuffle-mode)
  - Coordinates infrastructure calls with state updates, clears context navigation, sets shuffle mode
- [x] Implement `update-shuffle-settings.ts` action following [Action Behaviors](./PLAYER_DOMAIN_DESIGN.md#action-behaviors)
  - Single responsibility action for shuffle configuration changes
- [x] Implement `update-launch-mode.ts` action (if not already present from Phase 1) following [Action Behaviors](./PLAYER_DOMAIN_DESIGN.md#action-behaviors)
  - Single responsibility action for mode changes
- [x] Add `get-shuffle-settings.ts` selector following [Selector Behaviors](./PLAYER_DOMAIN_DESIGN.md#selector-behaviors)
  - Returns shuffle settings for device
- [x] Add `get-launch-mode.ts` selector (if not already present from Phase 1) following [Selector Behaviors](./PLAYER_DOMAIN_DESIGN.md#selector-behaviors)
  - Returns current launch mode (Directory, Shuffle, Search)
- [x] Update `withPlayerActions()` and `withPlayerSelectors()` custom features following [PlayerStore Structure](./PLAYER_DOMAIN_DESIGN.md#playerstore-structure-libsapplicationsrclibplayerplayer-storets)
- [x] Verify tests pass (green phase)

### Task 5: Application Layer Context Service - TDD Implementation

**Purpose**: Extend PlayerContextService with shuffle orchestration following [Player Context Service](./PLAYER_DOMAIN_DESIGN.md#player-context-service-application-layer).

**Orchestration Focus**: PlayerContextService as the "smart wrapper" around PlayerStore, implementing complex shuffle workflows per [Architecture Role](./PLAYER_DOMAIN_DESIGN.md#player-context-service-application-layer).

**Behaviors Being Built**:

- **Random Launch Orchestration**: Coordinate infrastructure service calls with state updates via [Store Integration](./PLAYER_DOMAIN_DESIGN.md#integration-patterns) patterns
- **Shuffle Mode Management**: Toggle shuffle mode and update launch mode state following [Business Logic Implementation](./PLAYER_DOMAIN_DESIGN.md#business-logic-implementation)
- **Settings Configuration**: Manage shuffle scope and filter settings per device
- **Signal-Based API**: Expose reactive signals for UI components to consume shuffle state per [Signal-Based API](./PLAYER_DOMAIN_DESIGN.md#signal-based-api)
- **Error State Management**: Track loading/error states during random launch operations

#### Step 5A: Write Failing Tests

- [x] **Random Launch Orchestration**: Test `launchRandomFile(deviceId)` calls infrastructure and updates state
- [x] **Shuffle Mode Toggle**: Test `toggleShuffleMode(deviceId)` switches between Directory and Shuffle modes
- [x] **Settings Management**: Test `setShuffleScope()`, `setFilterMode()` methods update device settings
- [x] **Signal API**: Test `getShuffleSettings(deviceId)` and `getLaunchMode(deviceId)` return correct reactive signals
- [x] **Multi-Device Coordination**: Test shuffle operations target correct device without affecting others
- [x] **Error Handling**: Test infrastructure failures update error state without breaking other devices
- [x] **Loading States**: Test loading states set during random launch operations
- [x] Verify tests fail (red phase) - methods should throw "Not implemented" errors

#### Step 5B: Implement PlayerContextService Extensions

- [x] Extend `IPlayerContext` interface in `libs/application/src/lib/player/player-context.interface.ts`
  - Add shuffle methods: `launchRandomFile()`, `toggleShuffleMode()`, `setShuffleScope()`, `setFilterMode()`
  - Add signal getters: `getShuffleSettings()`, `getLaunchMode()`
- [x] **Random Launch Coordination**: Implement `launchRandomFile()` orchestrating store actions with infrastructure calls
- [x] **Mode Management**: Implement `toggleShuffleMode()` coordinating mode switches with state updates
- [x] **Settings Coordination**: Implement shuffle settings methods coordinating with store updates
- [x] **Signal Exposure**: Implement signal-based methods for UI consumption
- [x] **Error Coordination**: Handle infrastructure errors and coordinate with store error states
- [x] Verify tests pass (green phase) - complete random launch workflow working end-to-end
- [x] Run build: `npx nx build teensyrom-ui` to ensure no breaks

### Task 6: Cross-Domain Orchestration - Directory Context Loading

**Purpose**: Enable automatic directory context loading when random files are launched in shuffle mode, allowing directory-files component to show sibling files.

**Architecture Goal**: When a random file is launched in shuffle mode, automatically load the parent directory into StorageStore and pass the directory files to player file-context for UI display.

**Flow Requirements**:

1. **Random File Launch**: User clicks dice button → `PlayerContextService.launchRandomFile()` executes
2. **Infrastructure Call**: Call `PlayerService.launchRandom()` to get random file
3. **Shuffle Mode Check**: If current launch mode is shuffle, proceed with directory loading
4. **Directory Extraction**: Extract parent directory path from random file path
5. **StorageStore Coordination**: Call `StorageStore.navigateToDirectory()` to load parent directory
6. **Directory Loading Wait**: Wait for directory to be loaded in StorageStore
7. **File Context Update**: Extract directory files and call existing `load-file-context` action
8. **UI Update**: Directory-files component automatically shows sibling files

#### Step 6A: Extend PlayerContextService Dependencies - TDD Implementation

**Dependencies Extension**:

- [x] **StorageStore Injection**: Add `StorageStore` dependency injection to `PlayerContextService`
- [x] **Optional Storage Service**: Consider if `STORAGE_SERVICE` token needed for additional directory operations

#### Step 6B: Write Failing Tests for Cross-Domain Orchestration

**Test Categories** (in `player-context.service.spec.ts`):

- [x] **Random Launch with Directory Loading**: Test complete workflow from random launch → directory load → context update
- [x] **Shuffle Mode Directory Context**: Test directory context loaded only when shuffle mode active
- [x] **Parent Path Extraction**: Test correct parent directory path extracted from file paths
- [x] **StorageStore Coordination**: Test `StorageStore.navigateToDirectory()` called with correct parameters
- [x] **File Context Integration**: Test directory files converted and loaded into player file-context
- [x] **Multi-Device Isolation**: Test directory loading targets correct device without affecting others
- [x] **Error Handling**: Test graceful degradation when directory loading fails
- [x] **StorageStore Mocking**: Test with mocked StorageStore to isolate player domain testing
- [x] Verify all tests fail (red phase)

#### Step 6C: Implement Cross-Domain Orchestration Logic

**Implementation Focus**: Extend `PlayerContextService.launchRandomFile()` with directory context loading while maintaining domain boundaries.

**Key Methods to Implement**:

- [x] **Enhanced Random Launch**: Extend `launchRandomFile()` with shuffle mode directory loading logic
- [x] **Directory Context Loading**: Private method `loadDirectoryContextForRandomFile()` for directory coordination
- [x] **Path Utility**: `getParentPath(filePath: string): string` for extracting parent directory
- [x] **File Conversion**: `convertStorageFilesToFileItems()` for domain model transformation
- [x] **Index Finding**: `findFileIndexInContext()` for locating current file position
- [x] **Error Handling**: Graceful degradation when directory operations fail

**Orchestration Logic Flow**:

1. Execute existing random launch logic (infrastructure call + player state update)
2. Check if current device is in shuffle mode
3. Extract parent directory path from launched file
4. Coordinate with StorageStore to load directory (await completion)
5. Extract directory files from StorageStore state
6. Convert storage domain files to player domain FileItem models
7. Call existing `load-file-context` action with directory files and current file index
8. Handle errors gracefully (random file still launches even if directory loading fails)

#### Step 6D: Testing Cross-Domain Integration

**StorageStore Mocking Strategy**:

- [x] **Mock StorageStore**: Create strongly typed mocks for StorageStore methods in tests
- [x] **Directory State Mocking**: Mock `getSelectedDirectoryState()` to return test directory data
- [x] **Navigate Method Mocking**: Mock `navigateToDirectory()` to simulate directory loading
- [x] **Async Coordination Testing**: Test async coordination between player and storage domains

**Integration Test Scenarios**:

- [x] **Complete Random Launch Flow**: Test user interaction → random launch → directory load → context update → UI display
- [x] **Shuffle Mode Activation**: Test directory loading only occurs when shuffle mode is active
- [x] **Directory Loading Coordination**: Test StorageStore receives correct deviceId, storageType, and path parameters
- [x] **File Context Population**: Test directory files properly converted and loaded into player context
- [x] **Error Recovery**: Test random file launch succeeds even when directory loading fails

### Task 7: UI Component Integration - TDD Implementation

**Purpose**: Add shuffle control buttons using `IconButtonComponent` with Test-Driven Development following [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) methodology.

**UI Design**: Two buttons using `IconButtonComponent` from [Component Library](../../COMPONENT_LIBRARY.md):

- **Random Launch Button**: `icon="casino"` (dice), `ariaLabel="Launch Random File"`, `color="normal"`
- **Shuffle Toggle Button**: `icon="shuffle"`, `ariaLabel="Toggle Shuffle Mode"`, `color` based on shuffle state
- **State-Driven Colors**: `highlight` when shuffle mode active, `normal` when inactive, following [IconButtonComponent color mapping](../../COMPONENT_LIBRARY.md#style-integration)

**Design References**:

- [IconButtonComponent](../../COMPONENT_LIBRARY.md#iconbuttoncomponent) - Properties, events, and accessibility features
- [UI Component Integration](./PLAYER_DOMAIN_DESIGN.md#ui-component-integration) - Signal-based integration patterns
- [Cross-Domain Integration](./PLAYER_DOMAIN_DESIGN.md#cross-domain-integration) - Multi-device coordination

#### Step 6A: Write Failing Tests

- [x] **Random Launch Button Tests**: Test dice button calls `IPlayerContext.launchRandomFile()` with correct deviceId
- [x] **Shuffle Toggle Button Tests**: Test shuffle button calls `IPlayerContext.toggleShuffleMode()` and reflects current mode
- [x] **Color State Tests**: Test shuffle button uses `highlight` color when LaunchMode.Shuffle active, `normal` when Directory mode
- [x] **Signal Integration Tests**: Test UI reacts to launch mode changes from PlayerStore via PlayerContext signals
- [x] **Multi-Device Tests**: Test buttons operate on correct device ID without affecting other devices
- [x] **Component Integration Tests**: Test `IconButtonComponent` integration, event binding, and accessibility
- [x] **Loading State Tests**: Test buttons show appropriate state during random launch operations
- [x] Verify tests fail (red phase)

#### Step 6B: Implement UI Components

- [x] Identify appropriate UI component location (likely player toolbar or controls area based on existing Phase 1 integration)
- [x] Import `IconButtonComponent` from `@teensyrom-nx/ui/components`
- [x] Inject `IPlayerContext` via `PLAYER_CONTEXT` token in component
- [x] Add Random Launch Button following [IconButtonComponent usage patterns](../../COMPONENT_LIBRARY.md#usage-examples):
  ```html
  <lib-icon-button
    icon="casino"
    ariaLabel="Launch Random File"
    color="normal"
    [disabled]="isLoading()"
    (buttonClick)="launchRandomFile(deviceId())"
  />
  ```
- [x] Add Shuffle Toggle Button with state-driven styling:
  ```html
  <lib-icon-button
    icon="shuffle"
    ariaLabel="Toggle Shuffle Mode"
    [color]="isShuffleMode() ? 'highlight' : 'normal'"
    (buttonClick)="toggleShuffleMode(deviceId())"
  />
  ```
- [x] Implement component methods:
  - `launchRandomFile(deviceId: string)` → calls `playerContext.launchRandomFile(deviceId)`
  - `toggleShuffleMode(deviceId: string)` → calls `playerContext.toggleShuffleMode(deviceId)`
  - `isShuffleMode()` → computed signal from `playerContext.getLaunchMode(deviceId) === LaunchMode.Shuffle`
  - `isLoading()` → signal from player loading state
- [x] Follow signal-based patterns from [UI Component Integration](./PLAYER_DOMAIN_DESIGN.md#ui-component-integration)
- [x] Verify tests pass (green phase)
- [x] Run build: `npx nx build teensyrom-ui` to ensure no breaks

### Task 8: Integration Testing & Final Validation

**Purpose**: Verify complete shuffle workflow with integration tests and final build validation following [Testing Strategy](./PLAYER_DOMAIN_DESIGN.md#testing-strategy).

**Design References**:

- [Cross-Domain Integration](./PLAYER_DOMAIN_DESIGN.md#cross-domain-integration) - Storage domain references and device coordination
- [Testing Strategy](./PLAYER_DOMAIN_DESIGN.md#testing-strategy) - Layer testing approach and behavioral coverage

- [x] **End-to-End Shuffle Tests**: Test complete workflow from UI button → context service → store → infrastructure
- [x] **Mode Switching Integration**: Test transitions between Directory and Shuffle modes preserve device independence
- [x] **State Persistence Tests**: Test shuffle settings survive component unmount/remount cycles
- [x] **Cross-Domain Integration**: Test shuffle mode interactions with storage domain via StorageKey pattern following [Storage Domain References](./PLAYER_DOMAIN_DESIGN.md#storage-domain-references)
- [x] **Multi-Device Coordination**: Test shuffle operations isolated per device following [Device Domain Coordination](./PLAYER_DOMAIN_DESIGN.md#device-domain-coordination)
- [x] **Phase 1 Regression Tests**: Verify Phase 1 functionality (file launching with context) remains intact
- [x] **UI Integration Tests**: Test IconButtonComponent integration with PlayerContext and state-driven styling
- [x] Run full test suite: `npx nx test`
- [x] Run final build: `npx nx build teensyrom-ui`
- [x] Verify shuffle controls work end-to-end following complete [Testing Strategy](./PLAYER_DOMAIN_DESIGN.md#testing-strategy)

## 🗂️ File Changes

### Domain Layer (New)

- [libs/domain/src/lib/models/player-filter-type.enum.ts](../../../libs/domain/src/lib/models/) - New
- [libs/domain/src/lib/models/player-scope.enum.ts](../../../libs/domain/src/lib/models/) - New

### Domain Contracts (Modified)

- [libs/domain/src/lib/contracts/player.contract.ts](../../../libs/domain/src/lib/contracts/) - Extend with launchRandom method

### Application Layer (New)

- [libs/application/src/lib/player/actions/launch-random-file.ts](../../../libs/application/src/lib/player/actions/) - New
- [libs/application/src/lib/player/actions/update-shuffle-settings.ts](../../../libs/application/src/lib/player/actions/) - New
- [libs/application/src/lib/player/selectors/get-shuffle-settings.ts](../../../libs/application/src/lib/player/selectors/) - New
- [libs/application/src/lib/player/selectors/get-launch-mode.ts](../../../libs/application/src/lib/player/selectors/) - New (if not from Phase 1)

### Application Layer (Modified)

- [libs/application/src/lib/player/player-context.interface.ts](../../../libs/application/src/lib/player/) - Extend with shuffle methods
- [libs/application/src/lib/player/player-context.service.ts](../../../libs/application/src/lib/player/) - Implement shuffle orchestration
- [PlayerStore state models](../../../libs/application/src/lib/player/) - Add ShuffleSettings interface and extend DevicePlayerState

### Infrastructure Layer (Modified)

- [libs/infrastructure/src/lib/player/player.service.ts](../../../libs/infrastructure/src/lib/player/) - Extend with launchRandom implementation

### UI Components (Modified)

- Player toolbar/controls component - Add dice and shuffle IconButtons with state-driven styling
- Associated test files for new UI integration

## 🧪 Testing Requirements

**Testing Strategy**: Focus on testing behaviors at the right abstraction layers for durable tests that survive refactoring, following established [Testing Strategy](./PLAYER_DOMAIN_DESIGN.md#testing-strategy) patterns.

### Application Layer Testing (Primary)

- [x] **PlayerContextService Shuffle Testing**: Test store behaviors through the context service (facade testing approach)

  - Mock infrastructure layer (PlayerService) using typed mocks
  - Mock StorageStore for cross-domain coordination testing
  - Test complete random launch workflow orchestration
  - Verify signal-based API exposure for UI components
  - Test shuffle settings management through context service methods
  - Multi-device shuffle state isolation and mode switching behaviors
  - Error handling and state transitions during random launch operations

- [x] **Cross-Domain Orchestration Testing**: Test directory context loading integration
  - Test random launch → directory loading → file context update workflow
  - Mock StorageStore methods (`navigateToDirectory`, `getSelectedDirectoryState`) with typed mocks
  - Test directory loading only occurs when shuffle mode is active
  - Test parent directory path extraction from file paths
  - Test storage domain file conversion to player domain FileItem models
  - Test graceful degradation when directory loading fails
  - Test multi-device isolation during cross-domain operations

### Infrastructure Layer Testing

- [x] **PlayerService Shuffle Testing**: Test infrastructure implementation
  - Extended IPlayerService interface implementation with launchRandom method
  - API integration with PlayerApiService and DomainMapper for random file calls
  - Error handling scenarios and HTTP failure cases for random selection
  - Proper domain model mapping for random launch responses
  - Parameter mapping for scope and filter enums

### UI Component Testing (Following [Smart Component Testing](../../SMART_COMPONENT_TESTING.md))

- [x] **IconButton Shuffle Integration**: Test component behavior with mocked dependencies

  - Random launch button (dice) event handling and `launchRandomFile` calls
  - Shuffle toggle button event handling and `toggleShuffleMode` calls
  - State-driven color changes (`highlight` when shuffle active, `normal` when inactive)
  - Mock `IPlayerContext` interface using strongly typed mocks via `PLAYER_CONTEXT` token

- [x] **Signal-Based UI Integration**: Test smart component integration
  - Test UI reactivity to shuffle state changes from PlayerStore via PlayerContext signals
  - Test `isShuffleMode()` computed signal reflects LaunchMode changes
  - Test button state updates when shuffle settings change
  - Verify IconButtonComponent integration with accessibility and proper event binding

### Integration Testing

- [x] **End-to-End Shuffle Workflow Testing**: Test complete shuffle flow

  - UI button click → component event → context service → store update → infrastructure call
  - Mode switching between Directory and Shuffle preserves device independence
  - Random launch operations with different scope and filter configurations
  - Error handling across all domain boundaries

- [x] **Cross-Domain Integration Testing**: Test domain coordination

  - Shuffle mode interactions with storage domain via StorageKey pattern
  - Multi-device shuffle operations maintain independence
  - Phase 1 regression testing (file launching with context still works)

- [x] **Directory Context Loading Integration Testing**: Test complete cross-domain workflow

  - Random file launch → directory loading → file context update → UI display
  - StorageStore and PlayerStore coordination without breaking domain boundaries
  - Directory-files component receives and displays sibling files correctly
  - Error scenarios: directory loading fails but random launch succeeds
  - Multi-device cross-domain operations remain isolated

- [x] **IconButtonComponent Integration Testing**: Test shared component usage
  - Verify `IconButtonComponent` integration via `@teensyrom-nx/ui/components` import
  - Test state-driven styling with design system color mapping
  - Verify accessibility features and proper ARIA handling

## ✅ Success Criteria

- [x] Random launch button (dice icon) successfully launches random files based on current shuffle settings
- [x] Shuffle toggle button (shuffle icon) enables/disables shuffle mode with visual feedback
- [x] Button colors reflect state using IconButtonComponent color system (`highlight` when active, `normal` when inactive)
- [x] Each device maintains independent shuffle state and UI controls without cross-device interference
- [x] Shuffle scope and filter settings properly configure random file selection
- [x] Phase 1 functionality (file launching with context from double-click) continues working unchanged
- [x] All tests passing with comprehensive behavioral coverage at appropriate abstraction layers
- [x] IconButtonComponent integration follows established component library patterns
- [x] Ready to proceed to Phase 3 (Basic Playback Controls + Navigation)

## 📝 Notes

- **IconButtonComponent Integration**: Leverages existing [IconButtonComponent](../../COMPONENT_LIBRARY.md#iconbuttoncomponent) for consistent styling and behavior patterns
- **State-Driven UI**: Uses PlayerStore signals via PlayerContext for reactive button color changes following signal-based architecture
- **TDD Approach**: Write failing tests first, then implement to pass (red-green cycle) for all layers
- **Multi-Device Focus**: All shuffle functionality maintains device independence as established in Phase 1
- **Phase 2 Scope**: Only shuffle mode toggling and random launching - no navigation between random files yet (deferred to Phase 3)
- **Cross-Domain**: Uses StorageKey pattern for referencing storage domain data without coupling
- **Existing Components**: Modifies existing UI components rather than creating new ones, following Phase 1 patterns
- **Store Testing**: All store behaviors (actions, selectors, helpers) tested through PlayerContextService for refactoring resilience
