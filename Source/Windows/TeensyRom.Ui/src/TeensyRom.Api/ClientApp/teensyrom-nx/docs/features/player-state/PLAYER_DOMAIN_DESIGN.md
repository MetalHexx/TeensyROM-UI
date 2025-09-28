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
    ├── actions/                        # File-per-action pattern
    │   ├── index.ts                    # withPlayerActions() custom feature
    │   ├── initialize-player.ts
    │   ├── launch-file.ts
    │   ├── launch-random-file.ts
    │   └── remove-player.ts
    └── selectors/                      # File-per-selector pattern
        ├── index.ts                    # withPlayerSelectors() custom feature
        ├── get-device-player.ts
        ├── get-current-file.ts
        ├── get-player-directory-context.ts
        └── get-player-status.ts

libs/infrastructure/src/lib/
└── player/                             # Player service implementations (NEW)
    ├── player.service.ts               # PlayerService implementing IPlayerService
    ├── player.mapper.ts                # API ↔ Domain mapping
    └── providers.ts                    # DI providers for player services
```

---

## Domain Models

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

```typescript
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
```

### Application State Models

```typescript
import { StorageKey } from '@teensyrom-nx/application/storage';
import { FileItem, PlayerStatus } from '@teensyrom-nx/domain';

// Currently launched file with context
export interface LaunchedFile {
  deviceId: string;
  storageKey: StorageKey; // Foreign key to Storage domain
  file: FileItem; // Uses shared domain model
  launchedAt: number; // Timestamp
}

// Directory context for player navigation
export interface PlayerDirectoryContext {
  path: string;
  files: FileItem[]; // Uses shared domain model
  currentFileIndex: number; // Index of current file in files array
}

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

## Domain Contracts

### IPlayerService Interface (libs/domain/src/lib/contracts/player.contract.ts)

```typescript
import { Observable } from 'rxjs';
import { InjectionToken } from '@angular/core';
import { FileItem, StorageType } from '@teensyrom-nx/domain';
import { PlayerFilterType, PlayerScope } from '@teensyrom-nx/domain';

export interface IPlayerService {
  launchFile(
    deviceId: string,
    storageType: StorageType,
    filePath: string
  ): Observable<FileItem>;

  launchRandom(
    deviceId: string,
    storageType: StorageType,
    filterType?: PlayerFilterType,
    scope?: PlayerScope,
    startingDirectory?: string
  ): Observable<FileItem>;

  getDirectoryFiles(
    deviceId: string,
    storageType: StorageType,
    directoryPath: string
  ): Observable<FileItem[]>;
}

// Dependency injection setup
export const PLAYER_SERVICE = new InjectionToken<IPlayerService>('PLAYER_SERVICE');
```

---

## Infrastructure Layer Design

### PlayerService Implementation (libs/infrastructure/src/lib/player/player.service.ts)

```typescript
@Injectable()
export class PlayerService implements IPlayerService {
  constructor(
    @Inject(PLAYER_API_SERVICE) private readonly playerApiService: PlayerApiService,
    @Inject(FILES_API_SERVICE) private readonly filesApiService: FilesApiService
  ) {}

  launchFile(
    deviceId: string,
    storageType: StorageType,
    filePath: string
  ): Observable<FileItem> {
    return from(
      this.playerApiService.launchFile({
        deviceId,
        storageType: PlayerMapper.toApiStorageType(storageType),
        filePath,
      })
    ).pipe(
      map(response => PlayerMapper.toFileItem(response.data)),
      catchError(error => throwError(() => PlayerMapper.toPlayerError(error)))
    );
  }

  launchRandom(
    deviceId: string,
    storageType: StorageType,
    filterType?: PlayerFilterType,
    scope?: PlayerScope,
    startingDirectory?: string
  ): Observable<FileItem> {
    return from(
      this.playerApiService.launchRandom({
        deviceId,
        storageType: PlayerMapper.toApiStorageType(storageType),
        filterType: filterType ? PlayerMapper.toApiFilterType(filterType) : undefined,
        scope: scope ? PlayerMapper.toApiScope(scope) : undefined,
        startingDirectory,
      })
    ).pipe(
      map(response => PlayerMapper.toFileItem(response.data)),
      catchError(error => throwError(() => PlayerMapper.toPlayerError(error)))
    );
  }

  getDirectoryFiles(
    deviceId: string,
    storageType: StorageType,
    directoryPath: string
  ): Observable<FileItem[]> {
    return from(
      this.filesApiService.getDirectory({
        deviceId,
        storageType: PlayerMapper.toApiStorageType(storageType),
        directoryPath,
      })
    ).pipe(
      map(response => PlayerMapper.toFileItems(response.data)),
      catchError(error => throwError(() => PlayerMapper.toPlayerError(error)))
    );
  }
}
```

### PlayerMapper Behaviors (libs/infrastructure/src/lib/player/player.mapper.ts)

```typescript
export class PlayerMapper {
  // Map API FileItemDto to domain FileItem
  static toFileItem(dto: FileItemDto): FileItem {
    return {
      name: dto.name,
      path: dto.path,
      size: dto.size,
      isFavorite: dto.isFavorite,
      title: dto.title,
      creator: dto.creator,
      releaseInfo: dto.releaseInfo,
      description: dto.description,
      shareUrl: dto.shareUrl,
      metadataSource: dto.metadataSource,
      meta1: dto.meta1,
      meta2: dto.meta2,
      metadataSourcePath: dto.metadataSourcePath,
      parentPath: dto.parentPath,
      playLength: dto.playLength,
      subtuneLengths: dto.subtuneLengths,
      startSubtuneNum: dto.startSubtuneNum,
      images: dto.images?.map(img => ({
        fileName: img.fileName,
        path: img.path,
        source: img.source,
      })) || [],
      type: PlayerMapper.toFileItemType(dto.type),
    };
  }

  // Map API launch response to LaunchedFile
  static toLaunchedFile(
    deviceId: string,
    storageType: StorageType,
    fileItem: FileItem
  ): LaunchedFile {
    return {
      deviceId,
      storageKey: StorageKeyUtil.create(deviceId, storageType),
      file: fileItem,
      launchedAt: Date.now(),
    };
  }

  // Map Storage API directory response to FileItems
  static toFileItems(storageDirectory: StorageDirectoryDto): FileItem[] {
    return storageDirectory.files?.map(dto => PlayerMapper.toFileItem(dto)) || [];
  }

  // Map domain enums to API enums
  static toApiFilterType(filterType: PlayerFilterType): LaunchRandomFilterTypeEnum {
    // Implementation mapping logic
  }

  static toApiScope(scope: PlayerScope): LaunchRandomScopeEnum {
    // Implementation mapping logic
  }

  // Map API enums to domain enums
  static toDomainFilterType(filterType: LaunchRandomFilterTypeEnum): PlayerFilterType {
    // Implementation mapping logic
  }

  static toDomainScope(scope: LaunchRandomScopeEnum): PlayerScope {
    // Implementation mapping logic
  }
}
```

---

## Application Layer Design

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

#### initialize-player.ts

- **Purpose**: Setup player state for device
- **Behavior**: Creates empty player state entry if not exists
- **State Changes**: Adds device to players record

#### launch-file.ts

- **Purpose**: Launch specific file and update current file state
- **Behavior**:
  - Calls PlayerService.launchFile()
  - Updates currentFile with launched file using shared FileItem model
  - Checks if directory context already loaded for file's directory
  - Calls PlayerService.getDirectoryFiles() if directory context missing
  - Updates directoryContext with sibling files and current file index
  - Sets loading states appropriately
- **State Changes**: Updates currentFile, directoryContext, status, timestamps

#### launch-random-file.ts

- **Purpose**: Launch random file based on filters and scope
- **Behavior**:
  - Calls PlayerService.launchRandom() with parameters
  - Updates currentFile with randomly selected file using shared FileItem model
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

### PlayerKeyUtil (libs/application/src/lib/player/player-key.util.ts)

```typescript
export const PlayerKeyUtil = {
  // Generate player state key from deviceId
  create(deviceId: string): string {
    return deviceId;
  },

  // Extract deviceId from player key
  parse(key: string): string {
    return key;
  },

  // Filter function for device keys
  forDevice(deviceId: string): (key: string) => boolean {
    return (key) => key === deviceId;
  },
} as const;
```

---

## Cross-Domain Integration

### Storage Domain References

- **StorageKey Pattern**: Use existing StorageKeyUtil for file references
- **Foreign Key Relationship**: LaunchedFile.storageKey references Storage domain
- **Shared Models**: Use common FileItem model across domains for consistency

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

### Domain Layer Testing

- **Pure Contracts**: Test service interfaces and injection tokens
- **Model Validation**: Test domain enums and type safety

### Application Layer Testing

- **Store Methods**: Test all actions with async/await patterns
- **Selectors**: Test computed signal factories and return values
- **Multi-Device**: Verify state isolation across devices
- **Helper Functions**: Test state mutations and query operations
- **Action Message Correlation**: Verify debugging support

### Infrastructure Layer Testing

- **Mock API Services**: Use strongly typed mocks via dependency injection
- **Mapper Testing**: Verify API ↔ Domain transformations
- **Error Handling**: Test HTTP error scenarios and domain error mapping

### Integration Testing

- **Cross-Domain**: Test StorageKey references and coordination
- **Device Lifecycle**: Test initialization and cleanup scenarios
- **Error Recovery**: Test failure and recovery workflows

---

## Success Criteria

### Architecture Goals

- **Shared Domain Models**: Consistent FileItem usage across Storage and Player domains
- **Clean Boundaries**: Well-defined interfaces between layers
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
- **Maintainability**: Clear code organization following Clean Architecture patterns
- **Documentation**: Complete domain documentation and examples