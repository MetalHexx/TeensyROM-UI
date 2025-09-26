# Player Domain Design

## Overview

This document outlines the design for the Player Domain, focusing on domain models, behaviors, and architecture. The Player Domain manages the current file state, directory context, and playback operations for TeensyROM devices while maintaining clean separation from the Storage domain.

## Key Design Principles

- **Domain Separation**: Player domain maintains its own models (`PlayerFileItem`) separate from Storage domain (`FileItem`)
- **Directory Context Awareness**: Track currently playing file and its co-located directory files for player navigation
- **Multi-Device Support**: Independent player state per device using flat state structure
- **Cross-Domain Integration**: Reference Storage domain via `StorageKey` pattern
- **Rich File Metadata**: Leverage `FileItemDto` from API for complete file information

---

## Domain File Structure

```
libs/domain/player/
├── PLAYER_DOMAIN.md                    # Domain overview documentation
├── services/                           # Services library
│   ├── project.json                   # Nx project configuration
│   ├── src/
│   │   ├── index.ts                   # Public API exports
│   │   ├── lib/
│   │   │   ├── player.service.ts      # HTTP service with DI pattern
│   │   │   ├── player.mapper.ts       # API ↔ Domain mapping
│   │   │   └── player.models.ts       # Domain types and enums
│   │   └── test-setup.ts              # Test configuration
│   ├── tsconfig.json                  # TypeScript configuration
│   ├── tsconfig.lib.json             # Library-specific config
│   ├── tsconfig.spec.json            # Test-specific config
│   ├── eslint.config.mjs             # ESLint configuration
│   └── vite.config.ts                # Vite configuration
└── state/                             # State library
    ├── project.json                   # Nx project configuration
    ├── src/
    │   ├── index.ts                   # Public API exports
    │   ├── lib/
    │   │   ├── player-store.ts        # NgRx Signal Store
    │   │   ├── player-key.util.ts     # State key utilities
    │   │   ├── player-helpers.ts      # State mutation helpers
    │   │   ├── actions/               # File-per-action pattern
    │   │   │   ├── index.ts           # withPlayerActions() custom feature
    │   │   │   ├── initialize-player.ts
    │   │   │   ├── launch-file.ts
    │   │   │   ├── launch-random-file.ts
    │   │   │   └── remove-player.ts
    │   │   └── selectors/             # File-per-selector pattern
    │   │       ├── index.ts           # withPlayerSelectors() custom feature
    │   │       ├── get-device-player.ts
    │   │       ├── get-current-file.ts
    │   │       ├── get-player-directory-context.ts
    │   │       └── get-player-status.ts
    │   └── test-setup.ts              # Test configuration
    ├── tsconfig.json                  # TypeScript configuration
    ├── tsconfig.lib.json             # Library-specific config
    ├── tsconfig.spec.json            # Test-specific config
    ├── eslint.config.mjs             # ESLint configuration
    └── vite.config.ts                # Vite configuration
```

---

## Domain Models

### Core Domain Types (player.models.ts)

```typescript
import { StorageKey } from '@teensyrom-nx/domain/storage/state';

// Domain representation of a file in player context
export interface PlayerFileItem {
  name: string;
  path: string;
  size: number;
  isFavorite: boolean;
  title: string;
  creator: string;
  releaseInfo: string;
  description: string;
  shareUrl: string;
  metadataSource: string;
  meta1: string;
  meta2: string;
  metadataSourcePath: string;
  parentPath: string;
  playLength: string;
  subtuneLengths: string[];
  startSubtuneNum: number;
  images: PlayerItemImage[];
  type: PlayerFileType;
}

// Domain representation of file images in player context
export interface PlayerItemImage {
  fileName: string;
  path: string;
  source: string;
}

// Domain file types for player operations
export enum PlayerFileType {
  Unknown = 'Unknown',
  Song = 'Song',
  Game = 'Game',
  Image = 'Image',
  Hex = 'Hex',
}

// Domain filter types for random launches
export enum PlayerFilterType {
  All = 'All',
  Games = 'Games',
  Music = 'Music',
  Images = 'Images',
  Hex = 'Hex',
}

// Domain scope types for random launches
export enum PlayerScope {
  Storage = 'Storage',
  DirectoryDeep = 'DirectoryDeep',
  DirectoryShallow = 'DirectoryShallow',
}

// Player status enumeration
export enum PlayerStatus {
  Stopped = 'Stopped',
  Playing = 'Playing',
  Paused = 'Paused',
  Loading = 'Loading',
}

// Currently launched file with context
export interface LaunchedFile {
  deviceId: string;
  storageKey: StorageKey; // Foreign key to Storage domain
  file: PlayerFileItem;
  launchedAt: number; // Timestamp
}

// Directory context for player navigation
export interface PlayerDirectoryContext {
  path: string;
  files: PlayerFileItem[]; // All files in same directory as current file
  currentFileIndex: number; // Index of current file in files array
}
```

### State Models (player-store.ts)

```typescript
// Per-device player state
export interface DevicePlayerState {
  deviceId: string;
  currentFile: LaunchedFile | null;
  directoryContext: PlayerDirectoryContext | null;
  status: PlayerStatus;
  isLoading: boolean;
  error: string | null;
  lastLaunchTime: number | null;
}

// Root player state structure
export interface PlayerState {
  players: Record<string, DevicePlayerState>; // key: deviceId
}

// Initial state
const initialState: PlayerState = {
  players: {},
};
```

---

## Service Layer Design

### IPlayerService Interface

```typescript
export interface IPlayerService {
  launchFile(
    deviceId: string,
    storageType: StorageType,
    filePath: string
  ): Observable<PlayerFileItem>;

  launchRandom(
    deviceId: string,
    storageType: StorageType,
    filterType?: PlayerFilterType,
    scope?: PlayerScope,
    startingDirectory?: string
  ): Observable<PlayerFileItem>;

  getDirectoryFiles(
    deviceId: string,
    storageType: StorageType,
    directoryPath: string
  ): Observable<PlayerFileItem[]>;
}

// Dependency injection setup
export const PLAYER_SERVICE = new InjectionToken<IPlayerService>('PLAYER_SERVICE');
export const PLAYER_SERVICE_PROVIDER = {
  provide: PLAYER_SERVICE,
  useExisting: PlayerService,
};
```

### PlayerService Behaviors

- **Launch Operations**: Calls PlayerApiService, maps response to PlayerFileItem
- **Directory Fetching**: Calls FilesApiService to get sibling files when needed
- **Smart Caching**: Only fetches directory files if not already loaded
- **Error Handling**: Maps HTTP errors to domain error messages
- **Type Mapping**: Uses PlayerMapper for API ↔ Domain transformation
- **Logging**: Comprehensive operation logging for debugging

### PlayerMapper Behaviors

```typescript
export class PlayerMapper {
  // Map API FileItemDto to domain PlayerFileItem
  static toPlayerFileItem(dto: FileItemDto): PlayerFileItem;

  // Map API launch response to LaunchedFile
  static toLaunchedFile(
    deviceId: string,
    storageType: StorageType,
    playerFileItem: PlayerFileItem
  ): LaunchedFile;

  // Map Storage API directory response to PlayerFileItems
  static toPlayerFileItems(storageDirectory: StorageDirectory): PlayerFileItem[];

  // Map domain enums to API enums
  static toApiFilterType(filterType: PlayerFilterType): LaunchRandomFilterTypeEnum;
  static toApiScope(scope: PlayerScope): LaunchRandomScopeEnum;

  // Map API enums to domain enums
  static toDomainFilterType(filterType: LaunchRandomFilterTypeEnum): PlayerFilterType;
  static toDomainScope(scope: LaunchRandomScopeEnum): PlayerScope;
}
```

---

## State Layer Design

### PlayerStore Structure

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

#### initialize-player.ts

- **Purpose**: Setup player state for device
- **Behavior**: Creates empty player state entry if not exists
- **State Changes**: Adds device to players record

#### launch-file.ts

- **Purpose**: Launch specific file and update current file state
- **Behavior**:
  - Calls PlayerService.launchFile()
  - Updates currentFile with launched file
  - Checks if directory context already loaded for file's directory
  - Calls PlayerService.getDirectoryFiles() if directory context missing
  - Updates directoryContext with sibling files and current file index
  - Sets loading states appropriately
- **State Changes**: Updates currentFile, directoryContext, status, timestamps

#### launch-random-file.ts

- **Purpose**: Launch random file based on filters and scope
- **Behavior**:
  - Calls PlayerService.launchRandom() with parameters
  - Updates currentFile with randomly selected file
  - Checks if directory context already loaded for file's directory
  - Calls PlayerService.getDirectoryFiles() if directory context missing
  - Updates directoryContext with sibling files and current file index
  - Sets loading states appropriately
- **State Changes**: Updates currentFile, directoryContext, status, timestamps

#### remove-player.ts

- **Purpose**: Clean up player state for device
- **Behavior**: Removes device entry from players record
- **State Changes**: Removes device from players record

### Selector Behaviors

#### get-device-player.ts

```typescript
// Returns complete player state for device
getDevicePlayer: (deviceId: string) => computed(() => DevicePlayerState | null);
```

#### get-current-file.ts

```typescript
// Returns currently launched file for device
getCurrentFile: (deviceId: string) => computed(() => LaunchedFile | null);
```

#### get-player-directory-context.ts

```typescript
// Returns complete directory context for current file
getPlayerDirectoryContext: (deviceId: string) => computed(() => PlayerDirectoryContext | null);
```

#### get-player-status.ts

```typescript
// Returns current player status
getPlayerStatus: (deviceId: string) => computed(() => PlayerStatus);
```

### Helper Functions (player-helpers.ts)

#### State Mutation Helpers (with action message)

- `setLoadingPlayer()`: Set loading state with action message correlation
- `setPlayerLoaded()`: Set success state with action message correlation
- `setPlayerError()`: Set error state with action message correlation
- `updatePlayer()`: Generic player state update with action message correlation

#### Query Helpers (read-only, no action message)

- `getPlayer()`: Get player state for device
- `hasPlayer()`: Check if device has player state
- `isPlayerLoading()`: Check if player is in loading state

### PlayerKeyUtil

```typescript
export const PlayerKeyUtil = {
  // Generate player state key from deviceId
  create(deviceId: string): string,

  // Extract deviceId from player key
  parse(key: string): string,

  // Filter function for device keys
  forDevice(deviceId: string): (key: string) => boolean,
} as const;
```

---

## Cross-Domain Integration

### Storage Domain References

- **StorageKey Pattern**: Use existing StorageKeyUtil for file references
- **Foreign Key Relationship**: LaunchedFile.storageKey references Storage domain
- **Directory Context**: Future feature will coordinate with Storage domain for directory file lists

### Device Domain Coordination

- **Device Independence**: Each device maintains separate player state
- **Cleanup Integration**: Remove player state when device disconnects
- **Multi-Device Support**: Simultaneous operation across multiple devices

---

## Future Extension Points

### Directory Navigation (Phase 2+)

- **Next/Previous**: Use directoryContext.files and currentFileIndex
- **Directory Mode**: Sequential playback within directory
- **File Ordering**: Respect directory file ordering for navigation

### Playback State Management (Phase 2+)

- **Play/Pause/Stop**: Extend PlayerStatus enum and state management
- **Timer Integration**: Add timer state for automatic progression
- **Speed Control**: Add speed control state for music playback

### Advanced Features (Phase 3+)

- **Shuffle Mode**: Random file selection with history
- **Play History**: Browser-like navigation history
- **Filter Systems**: Content filtering for navigation
- **Voice Management**: SID voice control state

---

## Testing Strategy

### Service Layer Testing

- **Mock PlayerApiService**: Use strongly typed mocks via dependency injection
- **Mapper Testing**: Verify API ↔ Domain transformations
- **Error Handling**: Test HTTP error scenarios and domain error mapping

### State Layer Testing

- **Store Methods**: Test all actions with async/await patterns
- **Selectors**: Test computed signal factories and return values
- **Multi-Device**: Verify state isolation across devices
- **Helper Functions**: Test state mutations and query operations
- **Action Message Correlation**: Verify debugging support

### Integration Testing

- **Cross-Domain**: Test StorageKey references and coordination
- **Device Lifecycle**: Test initialization and cleanup scenarios
- **Error Recovery**: Test failure and recovery workflows

---

## Success Criteria

### Architecture Goals

- **Domain Separation**: PlayerFileItem distinct from Storage FileItem
- **Clean Boundaries**: Well-defined interfaces between domains
- **Extensibility**: Easy to add future player features
- **Testability**: Comprehensive test coverage with mocked dependencies

### Functional Goals

- **File Launching**: Both specific and random file launching work correctly
- **Current File Tracking**: Accurate current file state management
- **Multi-Device**: Independent operation across multiple devices
- **Error Handling**: Robust error scenarios and recovery

### Quality Goals

- **Type Safety**: Full TypeScript coverage with proper typing
- **Performance**: Efficient state management with minimal overhead
- **Maintainability**: Clear code organization following established patterns
- **Documentation**: Complete domain documentation and examples
