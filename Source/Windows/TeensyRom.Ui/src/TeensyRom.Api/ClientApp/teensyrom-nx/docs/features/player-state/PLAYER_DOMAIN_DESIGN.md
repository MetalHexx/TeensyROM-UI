# Player Domain Design

## Overview

This document outlines the design for the Player Domain, focusing on domain models, behaviors, and Clean Architecture implementation. The Player Domain manages the current file state, directory context, and playback operations for TeensyROM devices while leveraging shared domain models and maintaining clean separation of concerns.

## Key Design Principles

- **Shared Domain Models**: Leverage existing domain models (`FileItem`, `DirectoryItem`) from the shared domain layer
- **Directory Context Awareness**: Track currently playing file and its co-located directory files for player navigation
- **Multi-Device Support**: Independent player state per device using flat state structure
- **Cross-Domain Integration**: Reference Storage domain via `StorageKey` pattern
- **Clean Architecture**: Proper separation between Domain, Application, and Infrastructure layers

---

## Clean Architecture Structure

```
libs/domain/src/lib/
├── models/                              # Shared domain models
│   ├── file-item.model.ts              # FileItem interface (shared)
│   ├── file-item-type.enum.ts          # FileItemType enum (shared)
│   ├── viewable-item-image.model.ts    # ViewableItemImage interface (shared)
│   └── storage-type.enum.ts            # StorageType enum (shared)
└── contracts/                          # Domain service contracts
    ├── player.contract.ts              # IPlayerService + injection token
    └── index.ts                        # Contract exports

libs/application/src/lib/
├── storage/                            # Existing storage state management
└── player/                             # Player state management (NEW)
    ├── player-store.ts                 # NgRx Signal Store
    ├── player-key.util.ts              # State key utilities
    ├── player-helpers.ts               # State mutation helpers
    ├── timer.service.ts                # RxJS timer functionality (application-level)
    ├── player-timer-manager.ts         # Device-aware timer coordination
    ├── player-context.interface.ts     # IPlayerContext interface + injection token
    ├── player-context.service.ts       # PlayerContextService implementing IPlayerContext
    ├── providers.ts                    # DI providers for player context service
    ├── actions/                        # File-per-action pattern
    │   ├── index.ts                    # withPlayerActions() custom feature
    │   ├── initialize-player.ts
    │   ├── launch-file-with-context.ts
    │   ├── launch-random-file.ts
    │   ├── load-file-context.ts
    │   ├── update-timer-state.ts
    │   └── remove-player.ts
    └── selectors/                      # File-per-selector pattern
        ├── index.ts                    # withPlayerSelectors() custom feature
        ├── get-device-player.ts
        ├── get-current-file.ts
        ├── get-player-file-context.ts
        ├── get-player-status.ts
        ├── get-launch-mode.ts
        ├── get-shuffle-settings.ts
        └── get-timer-state.ts

libs/infrastructure/src/lib/
└── player/                             # Player service implementations (NEW)
    ├── player.service.ts               # PlayerService implementing IPlayerService
    └── providers.ts                    # DI providers for player services
```

---

## Domain Models

This section defines the core data structures and types that form the foundation of the player domain. It establishes shared models used across storage and player domains, player-specific enums for advanced controls, and comprehensive application state interfaces that support the full range of player features from basic playback to professional DJ functionality.

### Shared Domain Models (from libs/domain)

The Player domain leverages existing shared domain models:

```typescript
import {
  FileItem,
  FileItemType,
  ViewableItemImage,
  StorageType
} from '@teensyrom-nx/domain';
```

### Player-Specific Domain Types

Additional player-specific enums to be added to `libs/domain/src/lib/models/`:

**Core Enums**:
- **PlayerFilterType**: Content filtering (All, Games, Music, Images, Hex)
- **PlayerScope**: Shuffle scope options (Storage, DirectoryDeep, DirectoryShallow)
- **PlayerStatus**: Playback states (Stopped, Playing, Paused, Loading)
- **LaunchMode**: Navigation modes (Directory, Shuffle, Search)

**Advanced Control Enums**:
- **SpeedCurveType**: Speed control curves (Linear, Logarithmic)
- **SeekMode**: Seeking speed options (Accurate, Insane)
- **NudgeDirection**: Nudging directions (Positive, Negative)
- **FastForwardStep**: Multi-step speed progression (Fast, Faster, EvenFaster, Fastest)
- **VoiceNumber**: SID voice channels (Voice1, Voice2, Voice3)
- **TimerDuration**: Custom timer options (5s, 10s, 15s, 30s, 1m, 3m, 5m, 10m, 15m, 30m, 1hr)

**Navigation Enums**:
- **HistoryDirection**: Navigation direction (Forward, Backward)
- **ProgressionTrigger**: Auto-progression triggers (TimerExpired, SongCompleted, UserTriggered)

### Application State Models

**Core State Interfaces**: Comprehensive state structure supporting all player features.

- **LaunchedFile**: Currently active media file with storage key reference and launch timestamp
- **PlayerFileContext**: Navigation context including file array and current position index
- **PlayHistory**: Browser-style navigation history with forward/backward tracking
- **TimerState**: Comprehensive timing including custom durations, speed multipliers, and pause states
- **SpeedState**: Speed control state including base speed, nudging, jumping, and seeking
- **VoiceState**: SID voice management with toggle and momentary controls per voice
- **ShuffleSettings**: Scope and filter configuration for random playback
- **CustomTimerConfig**: User-defined timer settings for games and images
- **DevicePlayerState**: Complete per-device state encompassing all player aspects
- **PlayerState**: Root state with device-keyed player states for multi-device support

**Key State Features**:
- **Multi-Device Independence**: Separate state per TeensyROM device
- **Cross-Domain References**: StorageKey pattern for storage domain integration
- **History Management**: Browser-style navigation with forward/backward support
- **Advanced Timing**: Speed control, seeking, custom timers, and override capabilities
- **Professional Controls**: DJ features including nudging, voice manipulation, and hold functions
- **Flexible Navigation**: Support for directory, shuffle, and search context modes

---

## Domain Contracts

Domain contracts define the interface boundaries between layers in the Clean Architecture. The IPlayerService contract specifies how the domain layer communicates with infrastructure services, focusing on simple external system integration without application logic. These contracts ensure that the domain layer remains independent of implementation details while enabling clean dependency injection.

### IPlayerService Interface (libs/domain/src/lib/contracts/player.contract.ts)

**Purpose**: Domain contract for infrastructure layer operations - focused exclusively on external system integration and hardware communication.

**Core Infrastructure Responsibilities**:
- **File Launch Operations**: Launch specific files on TeensyROM hardware
- **Random File Selection**: Server-side random file selection with filtering
- **External System Integration**: Direct communication with TeensyROM API endpoints
- **Hardware State Synchronization**: Maintain consistency with physical device state

**Key Interface Operations**:
- **Launch Operations**: `launchFile()`, `launchRandom()` - simple infrastructure calls
- **External System Coordination**: All operations trigger corresponding hardware actions
- **Domain Model Mapping**: Infrastructure responses mapped to domain models via DomainMapper

**Domain Contract Focus**:
- **Pure Infrastructure**: No application logic, state management, or UI concerns
- **Simple Operations**: Direct API integration without orchestration or workflow management
- **External System Boundary**: Interface between domain and external TeensyROM systems
- **Dependency Injection**: Via `PLAYER_SERVICE` token following established domain contract patterns

**Note**: All complex behaviors, orchestration, and application logic are handled by `IPlayerContext` in the application layer.

---

## Infrastructure Layer Design

The infrastructure layer handles all external system integration and API communication. It implements domain contracts by providing concrete services that interact with TeensyROM hardware and external APIs. This layer focuses exclusively on technical integration concerns, transforming external data to domain models while remaining stateless and free of business logic.

### PlayerService Behaviors

**Purpose**: Simple infrastructure implementation focused exclusively on API integration and domain model mapping.

**Core Implementation Focus**:
- **Launch Operations**: Calls PlayerApiService for file launching, maps response using DomainMapper.toFileItem()
- **Random Selection**: Integrates with server-side random file selection APIs
- **Error Handling**: Maps HTTP errors to domain error messages
- **Type Mapping**: Uses DomainMapper for all API ↔ Domain transformation
- **Pure Infrastructure**: No application logic, state management, or orchestration

**Service Characteristics**:
- **Simple API Integration**: Direct calls to TeensyROM API endpoints
- **Domain Model Focus**: Returns properly mapped domain models (FileItem, etc.)
- **Stateless Operations**: No internal state management or caching
- **Domain Isolation**: No awareness of application state, storage domain, or UI concerns
- **Clean Boundaries**: Focused solely on external system integration

**Integration Pattern**: PlayerService provides the foundation for IPlayerContext orchestration but handles no complex behaviors itself.

---

## Application Layer Design

The application layer serves as the orchestration hub of the player system, coordinating between infrastructure services and UI components. It provides the primary business interface through IPlayerContext, which wraps and orchestrates the underlying PlayerStore. This layer implements all complex player workflows, business rules, and state management while exposing a clean, signal-based API for UI consumption.

### IPlayerContext Interface (Application Layer)

**Purpose**: Application layer interface for comprehensive player workflow orchestration, providing a clean wrapper around PlayerStore and infrastructure services.

**Key Responsibilities**:
- **Store Wrapper**: High-level API that orchestrates PlayerStore actions and selectors
- **Infrastructure Coordination**: Bridges between PlayerStore state and IPlayerService operations
- **Business Logic Layer**: Implements all complex player workflows and business rules
- **Signal-Based API**: Provides reactive state access for UI components
- **Multi-Device Orchestration**: Coordinates independent player state across multiple devices

**Complete Interface Shape**:
- **Launch Operations**: `launchFileWithContext()`, `launchRandomFile()`, `launchFile()`
- **Playback Controls**: `play()`, `pause()`, `stop()`, `restart()`
- **Navigation**: `next()`, `previous()`, `navigateHistory()`
- **Speed Controls**: `setBaseSpeed()`, `nudge()`, `jump()`, `fastForward()`, `homeSpeed()`
- **Timer Controls**: `setCustomTimer()`, `enableTimerOverride()`, `holdFunction()`
- **SID Voice Controls**: `toggleVoice()`, `killVoice()`, `activateVoice()`
- **Mode Management**: `setLaunchMode()`, `setFilterMode()`, `setShuffleScope()`
- **Device Lifecycle**: `initializePlayer()`, `removePlayer()`
- **Signal API**: Comprehensive reactive state getters for all player aspects
- **Dependency Injection**: Via `PLAYER_CONTEXT` token following established patterns

**Architecture Role**: IPlayerContext is the primary interface for all player operations - it wraps PlayerStore complexity and provides a clean, business-focused API for UI components.

### Player Context Service (Application Layer)

**Purpose**: Concrete implementation of IPlayerContext - the central orchestration service that wraps PlayerStore and coordinates all player workflows.

**Store Orchestration**:
- **Action Coordination**: Calls appropriate PlayerStore actions based on business logic
- **State Management**: Manages complex state transitions through store actions
- **Selector Delegation**: Exposes PlayerStore selectors as clean signal-based API
- **Multi-Device Coordination**: Orchestrates device-specific state operations

**Business Logic Implementation**:
- **Workflow Orchestration**: Implements complex player workflows by coordinating multiple store actions
- **Infrastructure Integration**: Calls IPlayerService and processes results through store updates
- **Timer Integration**: Orchestrates TimerService and PlayerTimerManager operations, synchronizing timer events with store state
- **Error Handling**: Manages error states and coordinates error recovery workflows

**Integration Patterns**:
- **Store Integration**: Primary interface to PlayerStore - calls actions, exposes selectors
- **Infrastructure Integration**: Calls PlayerService for external operations, processes results
- **Timer Integration**: Uses TimerService and PlayerTimerManager to facilitate timer events and operations, coordinating timer state with store updates
- **Cross-Domain Integration**: Works with storage domain via StorageKey pattern

**Architecture Role**: PlayerContextService is the "smart wrapper" around PlayerStore - all complex logic lives here while PlayerStore remains focused on state management.

---

## PlayerStore Implementation

The PlayerStore implementation contains all the state management components that work together to provide reactive player state. This includes the NgRx Signal Store structure, comprehensive action library, selector patterns, and utility functions. The store is wrapped by PlayerContextService but remains focused purely on state management, following the single responsibility principle.

### PlayerStore Structure (libs/application/src/lib/player/player-store.ts)

```typescript
export const PlayerStore = signalStore(
  { providedIn: 'root' },
  withDevtools('player'),
  withState(initialState),
  withPlayerSelectors(),
  withPlayerActions()
);
```

### Action Behaviors

**Purpose**: Complete action library supporting all player features including launch operations, playback controls, advanced music features, timer management, and multi-device coordination.

#### initialize-player.ts

- **Purpose**: Setup player state for device
- **Behavior**: Creates empty player state entry if not exists
- **State Changes**: Adds device to players record

#### launch-file-with-context.ts (Primary Action)

- **Purpose**: Launch file with provided file context (directory files, search results, playlists, etc.)
- **Key Behaviors**: Coordinates infrastructure calls with state updates, manages context preservation, sets launch mode
- **State Changes**: Updates currentFile, fileContext, launchMode, status, and timestamps
- **Use Cases**: Directory navigation, search results, playlists, any file collection with context
- **Integration Points**: Calls PlayerService for infrastructure, updates store state, manages loading states

#### launch-random-file.ts (Shuffle Mode)

- **Purpose**: Launch random file based on filters and scope
- **Key Behaviors**: Calls infrastructure for random selection, clears context navigation, sets shuffle mode
- **State Changes**: Updates currentFile, clears fileContext, sets launchMode to Shuffle, updates shuffleSettings
- **Integration Points**: PlayerService.launchRandom() for infrastructure, external coordinator handles context loading

#### load-file-context.ts

- **Purpose**: Load file context for navigation without launching a file
- **Key Behaviors**: Creates navigation context from file arrays, finds current file position
- **State Changes**: Updates fileContext, sets launchMode based on context source
- **Use Cases**: Shuffle mode disengagement, external context loading, search result navigation setup

#### update-timer-state.ts

- **Purpose**: Update timer state from external timer service
- **Key Behaviors**: Synchronizes external timer state with store state
- **State Changes**: Updates timer state (totalTime, currentTime, isRunning, isPaused, speed)
- **Integration Points**: Called by PlayerContextService when timer changes occur

#### update-player-status.ts

- **Purpose**: Update player status for device
- **Key Behaviors**: Single responsibility action for status changes
- **State Changes**: Updates status property only
- **Use Cases**: Play/pause/stop operations, loading states, external playback status changes

#### update-launch-mode.ts

- **Purpose**: Update launch mode for device
- **Key Behaviors**: Single responsibility action for mode changes
- **State Changes**: Updates launchMode property only
- **Use Cases**: Mode switching, external mode changes, user-initiated mode selection

#### update-shuffle-settings.ts

- **Purpose**: Update shuffle settings for device
- **Key Behaviors**: Single responsibility action for shuffle configuration
- **State Changes**: Updates shuffleSettings property only
- **Use Cases**: Shuffle configuration, scope changes, filter updates, clearing shuffle settings

#### remove-player.ts

- **Purpose**: Clean up player state for device
- **Key Behaviors**: Removes device entry from players record
- **State Changes**: Removes device from players record
- **Integration Points**: PlayerContextService handles timer cleanup separately via TimerManager

### Selector Behaviors

**Purpose**: Computed signal getters that provide reactive access to player state for UI components.

**Key Selectors**:
- **get-device-player**: Returns complete player state for device
- **get-current-file**: Returns currently launched file for device
- **get-player-file-context**: Returns file context for navigation (directories, search results, playlists)
- **get-player-status**: Returns current player status (Playing, Paused, Stopped, Loading)
- **get-launch-mode**: Returns current launch mode (Directory, Shuffle, Search)
- **get-shuffle-settings**: Returns shuffle mode settings
- **get-timer-state**: Returns timer state for device playback

**Signal Pattern**: All selectors follow device-scoped computed signal pattern for reactive UI integration.

### Helper Functions (player-helpers.ts)

**Purpose**: Utility functions for consistent state management patterns and action message correlation.

**State Mutation Helpers**: Loading state management, success/error state updates, generic player state updates with action message correlation
**Query Helpers**: Read-only state queries for device existence, loading states, and player retrieval

### PlayerKeyUtil (player-key.util.ts)

**Purpose**: Utility functions for player state key management and device filtering.

**Key Functions**: State key generation from deviceId, deviceId extraction from keys, device filtering for state operations

---

## Timer Management Architecture

The timer management system provides sophisticated timing control for all media types, supporting both automatic progression and professional DJ features. TimerService handles core RxJS-based timing functionality, while PlayerTimerManager coordinates device-specific timer operations. These components are orchestrated by PlayerContextService to synchronize timer events with player state, enabling features like custom play durations, speed control, and automatic file progression.

### Timer Service (Application Layer)

**Purpose**: RxJS-based timer functionality for application-level timing concerns.

**Key Responsibilities**:
- **Timer Lifecycle**: Create, start, pause, stop, and reset individual timers
- **Speed Control**: Variable playback speed with real-time adjustment
- **Observable Integration**: Provide RxJS streams for timer state updates
- **Time Management**: Handle total duration and current time progression

### Player Timer Manager (Application Layer)

**Purpose**: Device-aware timer coordination that bridges TimerService with PlayerStore integration.

**Key Responsibilities**:
- **Device-Specific Timers**: Manage independent timer instances per device
- **Timer Coordination**: Coordinate timer operations with store state updates
- **State Synchronization**: Bridge timer observables with store state management
- **Lifecycle Management**: Handle timer creation/destruction with device lifecycle


### Multi-Device Timer Support

**Independent Timers:**
- Each device has its own timer state in PlayerStore
- Timer lifecycle tied to device lifecycle
- Automatic cleanup when device disconnects

**Signal Integration:**
- NgRx Signal Store provides reactive state for UI components
- PlayerFacadeService exposes signal-based API for modern Angular 19 patterns
- Internal RxJS timer observables converted to signals for UI consumption
- Consistent state synchronization across components
- Efficient updates without polling or subscription management

### Timer Behaviors

**Automatic Timer Setup:**
- Music files: Use metadata duration as totalTime
- Games/Images: Custom timer duration or no timer
- Timer state persists across pause/resume operations

**Speed Control Integration:**
- Timer speed affects both currentTime progression and totalTime calculations
- Speed changes immediately affect active timers
- Speed state persists in PlayerStore for consistency

**UI Component Integration:**
- Player toolbar uses signal-based timer state for progress display
- Directory files highlight current playing file via getCurrentFile() signal
- Consistent signal-based API across all player UI components
- No subscription management required in UI components
- Templates use direct signal references without async pipes

---

## Cross-Domain Integration

Cross-domain integration defines how the player domain interacts with other domains in the system while maintaining clean boundaries. The player domain references storage domain data through StorageKey patterns, coordinates with device domain for multi-device support, and integrates with search functionality. This architecture ensures loose coupling between domains while enabling seamless user experiences across different system areas.

### Storage Domain References

- **StorageKey Pattern**: Use existing StorageKeyUtil for file references
- **Foreign Key Relationship**: LaunchedFile.storageKey references Storage domain
- **Shared Models**: Use common FileItem model across domains for consistency
- **Domain Isolation**: Player domain operates independently from storage state

### Search Integration Patterns

- **External Search**: Search results provided to launch-file-with-context action with Search launchMode
- **Context Navigation**: Next/Previous operates within search results array using Search launch mode
- **No Player Search**: Player domain doesn't perform searches, only receives results
- **Search Mode State**: LaunchMode.Search explicitly indicates search result navigation context
- **State Flexibility**: FileContext supports any file collection regardless of source

### Device Domain Coordination

- **Device Independence**: Each device maintains separate player state
- **Cleanup Integration**: Remove player state when device disconnects
- **Multi-Device Support**: Simultaneous operation across multiple devices

### Shuffle Mode Behaviors

- **Mode Switching**: External coordinator handles shuffle mode disengagement
- **Context Restoration**: External coordinator loads directory via storage store, then calls load-file-context
- **Index Discovery**: load-file-context action finds current file position within directory
- **Seamless Transition**: Switch from random to sequential navigation via external coordination

### External Coordination Patterns

- **Player State Monitoring**: External services listen to currentFile changes from player store
- **Storage Integration**: Coordinate with storage store for directory loading when context is needed
- **Context Loading**: Use load-file-context action to set up navigation context from external sources
- **Domain Separation**: Player store focuses on player state, storage store handles directory fetching

---

## Incremental Development Phases

The incremental development approach breaks down the complex player system into manageable phases, each delivering independently valuable functionality. This strategy allows for early validation of core concepts while progressively building toward the complete feature set. Each phase establishes solid foundations for subsequent phases while maintaining system coherence and avoiding technical debt.

### Phase 1: Basic File Launching with Context
**Goal**: Launch a file from directory listing with navigation context (no playback controls)

**Scope**:
- Launch file from directory file listing (double-click behavior)
- Store current file and file context for future navigation
- Basic domain models and state structure
- **NO** playback controls, timers, or navigation yet

**Implementation**:
- `DevicePlayerState` (minimal: deviceId, currentFile, fileContext)
- `LaunchedFile` and `PlayerFileContext` interfaces
- `launch-file-with-context` action (Directory mode only)
- `getCurrentFile()` and `getFileContext()` selectors
- `PlayerService.launchFile()` infrastructure call
- Basic PlayerStore with minimal state

**Demonstrable Value**: Click file in directory → file launches → current file tracked in state

---

### Phase 2: Random File Launching (Shuffle Mode)
**Goal**: Launch random files with shuffle settings (still no playback controls)

**Scope**:
- Add shuffle file launching capability
- Introduce shuffle settings and launch modes
- **NO** navigation between random files yet, just launching

**Additions**:
- `ShuffleSettings` interface (scope, filter, startingDirectory)
- `LaunchMode` enum (Directory, Shuffle)
- `launch-random-file` action
- `update-launch-mode` and `update-shuffle-settings` actions
- `PlayerService.launchRandom()` infrastructure call

**Demonstrable Value**: User can launch random files with different scopes/filters

---

### Phase 3: Basic Playback Controls + File Navigation
**Goal**: Add play/pause/stop status + next/previous navigation within file context

**Scope**:
- Track playback status (Playing, Paused, Stopped, Loading)
- Navigate through directory files using existing context
- Different playback behaviors: Songs (play/pause), Games/Images (play/stop)
- **Shuffle Mode Previous**: Hardcode previous to launch another random file (no history tracking yet)
- **NO** timers, progress tracking, auto-progression, or file history yet

**Additions**:
- `PlayerStatus` enum
- `update-player-status` action
- Next/Previous navigation using `currentFileIndex` in `PlayerFileContext` for Directory mode
- Hardcoded random previous behavior for Shuffle mode
- Status display and navigation controls in UI

**Demonstrable Value**: Users can control playback and navigate through directory files, with basic navigation in shuffle mode

---

### Phase 4: Filter System UI Integration + Error State Visual Feedback
**Goal**: Wire up filter toolbar to existing filter infrastructure and add error state visual indicators

**Scope**:
- Connect filter button click handlers to `setFilterMode()`
- Visual feedback for active filter selection (cyan highlight)
- Visual feedback for error states (red color on all interactive buttons)
- Error state takes precedence over active state
- **NO** new backend logic - all filter infrastructure already exists from Phase 2

**Implementation**:
- Filter toolbar click handlers call `setFilterMode()`
- Active filter signal derived from `getShuffleSettings()`
- Error state signal derived from `getError()`
- Color binding logic in components (error > highlight > normal)
- Navigation button error state indicators

**Demonstrable Value**: Users see which filter is active and receive clear visual feedback when operations fail

---

### Phase 5: Timer System + Auto-Progression
**Goal**: Add basic timer functionality with automatic file progression

**Scope**:
- Timer state for progress tracking
- Song progress display (metadata duration)
- timer duration only for songs right now.
- Automatic next file when timer expires
- **NO** custom timer durations or file history yet

**Additions**:
- `TimerState` interface
- `TimerService`, `PlayerTimerManager`, `PlayerContextService`
- `update-timer-state` action
- Progress bars for all file types
- Auto-progression logic with hardcoded durations

**Demonstrable Value**: Complete basic playback experience with timing and automatic progression

---

### Design Evolution Notes

**Important**: The `PLAYER_DOMAIN_DESIGN.md` document should continue being referenced and updated as new concepts emerge during implementation. Each phase may reveal design adjustments or additional patterns that need to be addressed.

**Key Principles**:
1. **Each phase is independently valuable and demonstrable**
2. **Additive changes - don't break previous phases**
3. **Continuously update this document as concepts emerge**
4. **Validate design decisions before adding complexity**

---

## Future Extension Points (Post Phase 5)

Future extension points identify advanced features that build upon the core player system foundation. These extensions represent the full vision of professional DJ capabilities, advanced user experiences, and sophisticated media management features. The modular architecture established in the core phases provides extension points for these advanced features without requiring fundamental redesign.

### Advanced Features (Future Phases)

- **File History System**: Browser-like navigation history across modes for proper previous functionality
- **Custom Timer Durations**: User-configurable timer durations for Games/Images (5s, 10s, 15s, 30s, 1m, 3m, 5m, 10m, etc.)
- **Search Mode Integration**: Navigation within search results with proper file context
- **Shuffle Mode Navigation**: Advanced random file navigation with history tracking
- **Speed Control System**: Music playback speed manipulation and DJ features
- **SID Voice Management**: Individual voice control for SID files
- **Filter Systems**: Advanced content filtering for navigation
- **Seek Functionality**: Time-based navigation within tracks
- **Advanced Timer Features**: Nudge controls, custom presets, multi-device synchronization

---

## Testing Strategy

The testing strategy focuses on behavioral testing at appropriate architectural layers to ensure durability and maintainability. Rather than testing implementation details, the approach emphasizes testing through public interfaces and contracts. Application layer testing validates complete workflows, infrastructure testing ensures reliable external integration, and UI testing confirms component behavior with mocked dependencies.

### Domain Layer Testing

- **Pure Contracts**: Test service interfaces and injection tokens
- **Model Validation**: Test domain enums and type safety

### Application Layer Testing

**Focus**: Behavioral testing with full application layer integration - allows all application layer components (PlayerStore, TimerService, PlayerTimerManager, PlayerContextService) to integrate together while mocking only the infrastructure layer.

- **Store Behaviors**: Test all actions and selectors working together in realistic scenarios
- **Timer Integration**: Test complete timer workflows (TimerService → PlayerTimerManager → PlayerContextService → Store)
- **Multi-Device Behaviors**: Verify independent device state management and timer coordination
- **File Launch Workflows**: Test complete file launching scenarios with context loading and timer setup
- **State Coordination**: Test how PlayerContextService orchestrates store updates and timer management
- **Signal-Based API**: Test PlayerContextService signal exposure and reactivity
- **Cross-Action Behaviors**: Test action sequences and state transitions
- **Mock Infrastructure Only**: Use strongly typed mocks for PlayerService via dependency injection

### Infrastructure Layer Testing

- **Mock API Services**: Use strongly typed mocks via dependency injection
- **DomainMapper Integration**: Test centralized mapping functionality
- **Error Handling**: Test HTTP error scenarios and domain error mapping
- **Domain Isolation**: Test independence from storage domain state
- **PlayerService Implementation**: Test IPlayerService implementation and API integration

### Integration Testing

- **Cross-Domain**: Test StorageKey references and coordination
- **Device Lifecycle**: Test initialization and cleanup scenarios
- **Error Recovery**: Test failure and recovery workflows
- **Context Switching**: Test transitions between shuffle and directory modes
- **Timer Lifecycle**: Test timer creation/destruction with device lifecycle (application layer coordination)
- **Multi-Device Timers**: Test independent timer operation across devices (application layer)
- **End-to-End Player Workflows**: Test complete file launching, timer management, and state coordination
- **Infrastructure Integration**: Test actual PlayerService calls with real API integration

---

## Success Criteria

Success criteria define the measurable outcomes that validate the player system's effectiveness across architectural, functional, and business dimensions. These criteria ensure that the system meets both technical excellence standards and user experience goals, while supporting the intended use cases from casual media consumption to professional DJ performance.

### Architecture Goals

- **Shared Domain Models**: Consistent FileItem usage across Storage and Player domains
- **Clean Boundaries**: Well-defined interfaces between layers
- **Centralized Mapping**: Single DomainMapper for all API transformations
- **Flexible Context**: PlayerFileContext supports any file collection source
- **Extensibility**: Easy to add future player features
- **Testability**: Comprehensive test coverage with mocked dependencies

### Functional Goals

- **File Launching**: Specific, random, and context-based file launching work correctly
- **Current File Tracking**: Accurate current file state management
- **Context Navigation**: Support for directories, search results, and any file collections
- **Multi-Device**: Independent operation across multiple devices
- **Mode Switching**: Seamless transitions between shuffle and directory modes
- **Domain Independence**: Clean separation from storage domain concerns
- **Timer Management**: Accurate timing control with multi-device support
- **UI Integration**: Seamless timer state binding for reactive UI components
- **Error Handling**: Robust error scenarios and recovery

### Quality Goals

- **Type Safety**: Full TypeScript coverage with proper typing
- **Performance**: Efficient state management with minimal overhead
- **Maintainability**: Clear code organization following Clean Architecture patterns
- **Documentation**: Complete domain documentation and examples