# Phase 1: Basic File Launching with Context

**High Level Plan Documentation**:
- [Player Domain Design](./PLAYER_DOMAIN_DESIGN.md) - Complete architecture and phase planning
- [Player Domain Requirements](./PLAYER_DOMAIN.md) - Business requirements and use cases

**Standards Documentation**:

- **Coding Standards**: [../../CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **State Standards**: [../../STATE_STANDARDS.md](../../STATE_STANDARDS.md) - NgRx Signal Store patterns with custom features
- **Store Testing**: [../../STORE_TESTING.md](../../STORE_TESTING.md) - Store unit testing + optional facade integration testing
- **Smart Component Testing**: [../../SMART_COMPONENT_TESTING.md](../../SMART_COMPONENT_TESTING.md) - Testing components with mocked stores/services

## 🎯 Objective

Enable double-click file launching from directory listings with context tracking. When a user double-clicks a file in the directory view, the file launches and the current file + directory context is stored in player state for future navigation support.

## 🎭 Key Behaviors Being Implemented

### User Workflow
1. **User browses directory** → sees list of files (games, music, images)
2. **User double-clicks a file** → file launches on TeensyROM device
3. **System tracks context** → stores current file + all directory files for future navigation
4. **Multi-device support** → each TeensyROM device maintains independent player state

### Core Behaviors to Test
- **File Launch Coordination**: Double-click triggers API call to launch file on specific device
- **Context Preservation**: Directory file list stored alongside launched file for navigation context
- **Multi-Device Isolation**: Each device has independent player state (device1 can play file A while device2 plays file B)
- **Cross-Domain Integration**: Player state references storage data via StorageKey pattern without coupling
- **Error Handling**: Failed launches display errors without breaking UI or other device states
- **State Persistence**: Player state survives component unmount/remount cycles

## 📚 Required Reading

- [ ] [Player Domain Design](./PLAYER_DOMAIN_DESIGN.md) - Complete architecture overview and design principles
  - [Clean Architecture Structure](./PLAYER_DOMAIN_DESIGN.md#clean-architecture-structure) - File organization and layer boundaries
  - [Domain Models](./PLAYER_DOMAIN_DESIGN.md#domain-models) - Shared models and player-specific types
  - [Domain Contracts](./PLAYER_DOMAIN_DESIGN.md#domain-contracts) - IPlayerService interface for infrastructure
  - [Application Layer Design](./PLAYER_DOMAIN_DESIGN.md#application-layer-design) - IPlayerContext interface and orchestration
  - [PlayerStore Implementation](./PLAYER_DOMAIN_DESIGN.md#playerstore-implementation) - Store structure and action patterns
  - [Infrastructure Layer Design](./PLAYER_DOMAIN_DESIGN.md#infrastructure-layer-design) - PlayerService implementation patterns
  - [Phase 1 Scope](./PLAYER_DOMAIN_DESIGN.md#phase-1-basic-file-launching-with-context) - Specific Phase 1 implementation details
- [ ] [State Standards](../../STATE_STANDARDS.md) - NgRx Signal Store patterns with custom features
- [ ] [Store Testing](../../STORE_TESTING.md) - Store testing methodology and facade testing approach
- [ ] [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) - Component testing with mocked dependencies
- [ ] [Storage Key Util](../../../libs/application/src/lib/storage/storage-key.util.ts) - Cross-domain reference pattern
- [ ] [Domain Mapper](../../../libs/infrastructure/src/lib/domain.mapper.ts) - Centralized API transformation
- [ ] [FileItemComponent](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/file-item/file-item.component.ts) - Existing file display component

## 📋 Implementation Tasks

### Pre-Phase: Establish Baseline

**Purpose**: Ensure existing tests pass before beginning Phase 1 implementation.

- [ ] Run all existing tests: `npx nx test`
- [ ] Verify existing build works: `npx nx build teensyrom-ui`
- [ ] Document any failing tests or build issues that need to be addressed
- [ ] Establish clean baseline before adding new code

### Task 1: Domain Layer

**Purpose**: Add player-specific domain types as defined in [Domain Models](./PLAYER_DOMAIN_DESIGN.md#domain-models).

**Phase 1 Domain Types**: Basic enums required for file launching and state management.

#### Implementation Steps
- [x] Add `PlayerStatus` enum to `libs/domain/src/lib/models/player-status.enum.ts`
  - Values: `Stopped`, `Playing`, `Paused`, `Loading` (from [Player-Specific Domain Types](./PLAYER_DOMAIN_DESIGN.md#player-specific-domain-types))
- [x] Add `LaunchMode` enum to `libs/domain/src/lib/models/launch-mode.enum.ts`
  - Values: `Directory`, `Shuffle`, `Search` (from [Player-Specific Domain Types](./PLAYER_DOMAIN_DESIGN.md#player-specific-domain-types))
- [x] Export new enums in `libs/domain/src/lib/models/index.ts`
- [x] Update domain barrel export in `libs/domain/src/index.ts`

**Note**: Advanced control enums (SpeedCurveType, SeekMode, etc.) are deferred to future phases as per [Phase 1 scope](./PLAYER_DOMAIN_DESIGN.md#phase-1-basic-file-launching-with-context).

### Task 2: Domain Contracts Implementation

**Purpose**: Define clean domain contract for player operations as specified in [Domain Contracts](./PLAYER_DOMAIN_DESIGN.md#domain-contracts).

**Contract Focus**: Simple infrastructure operations only - file launching and external system integration without application logic.

#### Implementation Steps
- [x] Create `IPlayerService` interface in `libs/domain/src/lib/contracts/player.contract.ts`
  - `launchFile(deviceId, storageType, filePath): Observable<FileItem>` (core Phase 1 operation)
  - `launchRandom()` method signature (Phase 2 scope, but interface planning)
  - Follow [IPlayerService Interface specification](./PLAYER_DOMAIN_DESIGN.md#iplayerservice-interface-libsdomainsrclibcontractsplayercontractts)
- [x] Define `PLAYER_SERVICE` injection token
- [x] Export contract in `libs/domain/src/lib/contracts/index.ts`
- [x] Update contracts barrel export in `libs/domain/src/index.ts`

**Important**: Keep interface focused on infrastructure concerns only. All complex behaviors go in IPlayerContext (application layer).

### Task 3: Infrastructure Layer - TDD Implementation

**Purpose**: Implement file launching infrastructure following [Infrastructure Layer Design](./PLAYER_DOMAIN_DESIGN.md#infrastructure-layer-design) patterns.

**Implementation Focus**: Simple infrastructure operations as defined in [PlayerService Behaviors](./PLAYER_DOMAIN_DESIGN.md#playerservice-behaviors) - stateless API integration only.

**Behaviors Being Built**:
- Call TeensyROM API to launch specific file on specific device
- Transform API responses to domain `FileItem` models using centralized [Domain Mapper](../../../libs/infrastructure/src/lib/domain.mapper.ts)
- Handle API errors gracefully (network failures, device not found, invalid file paths)
- Return properly typed domain models for application layer consumption
- **No application logic** - pure infrastructure implementation

#### Step 3A: Write Failing Tests
- [x] **File Launch API Integration**: Test `launchFile(deviceId, storageType, filePath)` calls correct API endpoint
- [x] **Domain Model Mapping**: Test API responses mapped to `FileItem` using `DomainMapper.toFileItem()`
- [x] **Error Scenarios**: Test network failures, device errors, invalid paths return proper error messages
- [x] **Contract Compliance**: Test service implements `IPlayerService` interface correctly
- [x] Verify tests fail (red phase)

#### Step 3B: Implement to Pass Tests
- [x] Create `PlayerService` in `libs/infrastructure/src/lib/player/player.service.ts`
- [x] Implement `launchFile()` method calling `PlayerApiService.launchFile()`
- [x] Integrate with `DomainMapper.toFileItem()` for response transformation
- [x] Add proper error handling for HTTP failures and device errors
- [x] Create providers in `libs/infrastructure/src/lib/player/providers.ts`
- [x] Update infrastructure barrel exports
- [x] Verify tests pass (green phase)

### Task 4: Application Layer - Setup for TDD

**Purpose**: Create skeleton structure following [Application Layer Design](./PLAYER_DOMAIN_DESIGN.md#application-layer-design) to enable writing failing tests first.

**Architecture Setup**: Establish [PlayerStore Implementation](./PLAYER_DOMAIN_DESIGN.md#playerstore-implementation) structure and [IPlayerContext Interface](./PLAYER_DOMAIN_DESIGN.md#iplayercontext-interface-application-layer) as the orchestration wrapper.

#### Implementation Steps
- [x] Create player state interfaces following [Application State Models](./PLAYER_DOMAIN_DESIGN.md#application-state-models) with minimal Phase 1 structure
  - `LaunchedFile`, `PlayerFileContext`, `DevicePlayerState`, `PlayerState`
- [x] Create empty [PlayerStore Structure](./PLAYER_DOMAIN_DESIGN.md#playerstore-structure-libsapplicationsrclibplayerplayer-storets) with placeholder custom features
- [x] Create `IPlayerContext` interface in `libs/application/src/lib/player/player-context.interface.ts`
  - Phase 1 methods: `launchFileWithContext()`, `initializePlayer()`, `removePlayer()`
  - Signal getters: `getCurrentFile()`, `getFileContext()`, `isLoading()`, `getError()`
  - Follow [IPlayerContext Interface](./PLAYER_DOMAIN_DESIGN.md#iplayercontext-interface-application-layer) specification
- [x] Create `PLAYER_CONTEXT` injection token in the same interface file
- [x] Create `PlayerContextService` skeleton implementing `IPlayerContext` with method signatures but no implementation
- [x] Create empty action/selector/helper files with function signatures following [Action Behaviors](./PLAYER_DOMAIN_DESIGN.md#action-behaviors) patterns
- [x] Establish file structure per [Clean Architecture Structure](./PLAYER_DOMAIN_DESIGN.md#clean-architecture-structure)
- [x] Verify TypeScript compiles (implementation can be empty/throw errors)

### Task 5: Application Layer - TDD Implementation

**Purpose**: Implement [Player Context Service](./PLAYER_DOMAIN_DESIGN.md#player-context-service-application-layer) orchestration patterns and complete [PlayerStore Implementation](./PLAYER_DOMAIN_DESIGN.md#playerstore-implementation).

**Orchestration Focus**: PlayerContextService as the "smart wrapper" around PlayerStore, implementing all complex workflows per [Architecture Role](./PLAYER_DOMAIN_DESIGN.md#player-context-service-application-layer).

**Behaviors Being Built**:
- **File Launch Orchestration**: Coordinate infrastructure service calls with state updates via [Store Integration](./PLAYER_DOMAIN_DESIGN.md#player-context-service-application-layer) patterns
- **Context Tracking**: Store launched file alongside directory file context for future navigation
- **Multi-Device State**: Independent player state per device with proper isolation
- **Signal-Based API**: Expose reactive signals for UI components to consume
- **Error State Management**: Track loading/error states during launch operations

#### Step 5A: Write Failing Tests
- [x] **Launch Orchestration**: Test `launchFileWithContext(deviceId, file, contextFiles)` calls infrastructure and updates state
- [x] **State Management**: Test current file and file context stored correctly in device-specific state
- [x] **Multi-Device Isolation**: Test device1 state independent from device2 state
- [x] **Signal API**: Test `getCurrentFile(deviceId)` and `getFileContext(deviceId)` return correct reactive signals
- [x] **Error Handling**: Test infrastructure failures update error state without breaking other devices
- [x] **Loading States**: Test loading states set during launch operations
- [x] Verify tests fail (red phase) - methods should throw "Not implemented" errors

#### Step 5B: Implement Store, Actions, Selectors, Helpers
- [x] **State Structure**: Implement device-keyed player state with current file and context
- [x] **Launch Action**: Implement `launch-file-with-context` action that calls infrastructure and stores results
- [x] **Initialize/Cleanup**: Implement `initialize-player` and `remove-player` for device lifecycle
- [x] **Selectors**: Implement computed signals for current file, context, and state per device
- [x] **Helpers**: Implement state mutation helpers with action message correlation
- [x] Create actions/selectors indexes following STATE_STANDARDS patterns

#### Step 5C: Implement PlayerContextService
- [x] **Launch Coordination**: Implement `launchFileWithContext()` orchestrating store actions with infrastructure calls
- [ ] **Signal Exposure**: Implement signal-based methods (`getCurrentFile()`, `getFileContext()`) for UI consumption
- [ ] **Error Coordination**: Handle infrastructure errors and coordinate with store error states
- [ ] **Device Management**: Implement device initialization and cleanup coordination
- [x] Verify tests pass (green phase) - complete file launch workflow working end-to-end
- [ ] Run build: `npx nx build teensyrom-ui` to ensure no breaks

### Task 6: UI Component Integration - TDD Implementation

**Purpose**: Add double-click functionality using Test-Driven Development and SMART_COMPONENT_TESTING methodology.

**Design References**:
- [UI Component Integration](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#ui-component-integration) - Signal-based timer state and current file highlighting
- [Cross-Domain Integration](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#cross-domain-integration) - StorageKey pattern for file references

#### Step 6A: Write Failing Tests
- [x] Create failing tests for `FileItemComponent` double-click behavior (follow SMART_COMPONENT_TESTING.md)
- [x] Create failing tests for `DirectoryFilesComponent` player integration with mocked `IPlayerContext` interface
- [x] Verify tests fail (red phase)

#### Step 6B: Implement to Pass Tests
- [x] Add double-click handler and `itemDoubleClick` output to `FileItemComponent`
- [x] Update `FileItemComponent` template for double-click event binding
- [x] Inject `IPlayerContext` via `PLAYER_CONTEXT` token in `DirectoryFilesComponent`
- [x] Handle file double-click events and coordinate with existing `StorageStore`
- [x] Call `IPlayerContext.launchFileWithContext()` with directory context following [Storage Domain References](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#storage-domain-references)
- [x] Implement signal-based UI patterns from [UI Component Integration](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#ui-component-integration)
- [x] Verify tests pass (green phase)
- [ ] Run build: `npx nx build teensyrom-ui` to ensure no breaks

### Task 7: Application Integration - Dependency Injection Setup

**Purpose**: Wire up stores and services for dependency injection in main application.

**Design References**:
- [PlayerContextService](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#playercontextservice-libsapplicationsrclibplayerplayer-contextservicets) - Architecture role as smart wrapper around PlayerStore
- [Application Layer Design](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#application-layer-design) - Store and service coordination patterns

- [x] Create player context providers in `libs/application/src/lib/player/providers.ts` with `PLAYER_CONTEXT_PROVIDER`
- [x] Export `IPlayerContext`, `PLAYER_CONTEXT`, and provider from `libs/application/src/index.ts`
- [x] Add `PLAYER_CONTEXT_PROVIDER` to `apps/teensyrom-ui/src/app/app.config.ts`
- [x] Wire up infrastructure providers for `PLAYER_SERVICE` token
- [x] Verify `IPlayerContext` injection works correctly in components
- [ ] Follow [Integration Patterns](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#playercontextservice-libsapplicationsrclibplayerplayer-contextservicets) for store, infrastructure, and cross-domain coordination

### Task 8: Integration Testing & Final Validation

**Purpose**: Verify complete workflow with integration tests and final build validation.

**Design References**:
- [Cross-Domain Integration](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#cross-domain-integration) - Storage domain references and device coordination
- [Testing Strategy](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#testing-strategy) - Layer testing approach and behavioral coverage

- [ ] Create integration tests for complete file launch workflow
- [ ] Test cross-domain integration (player ↔ storage via StorageKey pattern) following [Storage Domain References](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#storage-domain-references)
- [ ] Test UI component + context service + store + infrastructure integration
- [ ] Verify [Device Domain Coordination](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#device-domain-coordination) with multi-device support
- [ ] Test [External Coordination Patterns](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#external-coordination-patterns) for player state monitoring
- [ ] Run full test suite: `npx nx test`
- [ ] Run final build: `npx nx build teensyrom-ui`
- [ ] Verify double-click file launch works end-to-end following complete [Testing Strategy](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#testing-strategy)

## 🗂️ File Changes

- [libs/domain/src/lib/models/player-status.enum.ts](../../../libs/domain/src/lib/models/) - New
- [libs/domain/src/lib/models/launch-mode.enum.ts](../../../libs/domain/src/lib/models/) - New
- [libs/domain/src/lib/contracts/player.contract.ts](../../../libs/domain/src/lib/contracts/) - New
- [libs/application/src/lib/player/player-store.ts](../../../libs/application/src/lib/player/) - New
- [libs/application/src/lib/player/actions/](../../../libs/application/src/lib/player/actions/) - New directory
- [libs/application/src/lib/player/selectors/](../../../libs/application/src/lib/player/selectors/) - New directory
- [libs/application/src/lib/player/player-context.interface.ts](../../../libs/application/src/lib/player/) - New
- [libs/application/src/lib/player/player-context.service.ts](../../../libs/application/src/lib/player/) - New
- [libs/application/src/lib/player/providers.ts](../../../libs/application/src/lib/player/) - New
- [libs/infrastructure/src/lib/player/player.service.ts](../../../libs/infrastructure/src/lib/player/) - New
- [FileItemComponent](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/file-item/file-item.component.ts) - Modified
- [DirectoryFilesComponent](../../../libs/features/player/src/lib/player-view/player-device-container/storage-container/directory-files/directory-files.component.ts) - Modified

## 🧪 Testing Requirements

**Testing Strategy**: Focus on testing behaviors at the right abstraction layers for durable tests that survive refactoring.

**Design References**: [Testing Strategy](../../../docs/features/player-state/PLAYER_DOMAIN_DESIGN.md#testing-strategy) - Layer testing approach, behavioral coverage, and facade testing patterns

### Application Layer Testing (Primary)

- [ ] **PlayerContextService Testing**: Test store behaviors through the context service (facade testing approach)
  - Mock infrastructure layer (PlayerService) using typed mocks
  - Test complete file launch workflow orchestration
  - Verify signal-based API exposure for UI components
  - Test store state management through context service methods
  - Multi-device state isolation and cleanup behaviors
  - Error handling and state transitions

### Infrastructure Layer Testing

- [ ] **PlayerService Testing**: Test infrastructure implementation
  - IPlayerService interface implementation
  - API integration with PlayerApiService and DomainMapper
  - Error handling scenarios and HTTP failure cases
  - Proper domain model mapping

### UI Component Testing (Following SMART_COMPONENT_TESTING.md)

- [ ] **FileItemComponent Testing**: Test component behavior with mocked dependencies
  - Double-click event handling and itemDoubleClick emission
  - Maintain existing single-click selection behavior
  - Mock any required services using strongly typed mocks

- [ ] **DirectoryFilesComponent Testing**: Test smart component integration
  - Mock `IPlayerContext` interface using strongly typed mocks via `PLAYER_CONTEXT` token
  - Test file double-click handling and context coordination
  - Test integration with existing StorageStore patterns
  - Verify `launchFileWithContext` calls with correct parameters

### Integration Testing

- [ ] **End-to-End Workflow Testing**: Test complete file launch flow
  - Double-click → component event → context service → store update → infrastructure call
  - Cross-domain integration (player ↔ storage via StorageKey pattern)
  - Error handling across all domain boundaries

- [ ] **Dependency Injection Testing**: Test DI setup and provider configuration
  - Verify `IPlayerContext` injection via `PLAYER_CONTEXT` token works
  - Test `PLAYER_SERVICE` token resolution to PlayerService
  - Verify `PLAYER_CONTEXT_PROVIDER` setup in app.config.ts

## ✅ Success Criteria

- [ ] Double-clicking a file in directory listing launches the file
- [ ] Current file and directory context stored in player state
- [ ] Player state isolated per device (multi-device support)
- [ ] No playback controls, timers, or navigation implemented (Phase 1 scope)
- [ ] All tests passing with proper behavioral coverage
- [ ] Ready to proceed to Phase 2 (Random File Launching)

## 📝 Notes

- **PlayerContextService**: Added to Phase 1 scope for launch workflow orchestration (not in original design but required)
- **Testing Strategy**: Focus on layer testing for durable behaviors - test PlayerContextService (application layer), PlayerService (infrastructure layer), and components (UI layer) rather than granular pieces
- **STATE_STANDARDS**: Follow NgRx Signal Store patterns with custom features (withPlayerActions, withPlayerSelectors)
- **No Timer/Playback**: Phase 1 focuses only on file launching and state tracking
- **Cross-Domain**: Use StorageKey pattern for referencing storage domain data
- **Existing Components**: Modify existing UI components rather than creating new ones
- **Store Testing**: All store behaviors (actions, selectors, helpers) tested through PlayerContextService for refactoring resilience








