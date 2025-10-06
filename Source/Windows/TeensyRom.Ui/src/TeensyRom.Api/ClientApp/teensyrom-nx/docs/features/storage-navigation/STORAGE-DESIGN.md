# Storage Domain Design

## Overview

This document outlines the design for the Storage Domain, focusing on domain models, navigation behaviors, and Clean Architecture implementation. The Storage Domain manages directory navigation, file browsing, and storage operations for TeensyROM devices while maintaining clean separation of concerns and supporting multi-device scenarios.

## Key Design Principles

- **Clean Architecture**: Strict separation between Domain, Infrastructure, Application, and Presentation layers
- **Multi-Device Support**: Independent storage state per device using flat state structure with composite keys
- **Browser-Like Navigation**: Forward/backward/up navigation with history tracking (max 50 entries per device)
- **Shared Domain Models**: Leverage FileItem, DirectoryItem, and StorageDirectory models across domains
- **Storage Key Pattern**: Composite keys (`deviceId-storageType`) for flat state organization
- **Async/Await First**: Deterministic Promise-based operations following STATE_STANDARDS.md patterns
- **Redux DevTools Integration**: Action message correlation for all state mutations

---

## Clean Architecture Structure

```
libs/domain/src/lib/
├── models/                              # Shared domain models
│   ├── file-item.model.ts              # FileItem interface (shared with player)
│   ├── file-item-type.enum.ts          # FileItemType enum
│   ├── directory-item.model.ts         # DirectoryItem interface
│   ├── storage-directory.model.ts      # StorageDirectory interface
│   ├── storage-type.enum.ts            # StorageType enum (SD, USB)
│   └── viewable-item-image.model.ts    # Image metadata for files
└── contracts/                          # Domain service contracts
    ├── storage.contract.ts             # IStorageService + injection token
    └── index.ts                        # Contract exports

libs/infrastructure/src/lib/
└── storage/                            # Storage service implementations
    ├── storage.service.ts              # StorageService implementing IStorageService
    ├── providers.ts                    # DI providers for storage services
    └── storage.service.spec.ts         # Unit tests for storage service

libs/application/src/lib/
└── storage/                            # Storage state management
    ├── storage-store.ts                # NgRx Signal Store
    ├── storage-key.util.ts             # Storage key utilities (create, parse, filter)
    ├── storage-helpers.ts              # State mutation and query helpers
    ├── actions/                        # File-per-action pattern
    │   ├── index.ts                    # withStorageActions() custom feature
    │   ├── initialize-storage.ts       # Initial directory load at root
    │   ├── navigate-to-directory.ts    # Navigate to specific path
    │   ├── navigate-up-one-directory.ts # Navigate to parent directory
    │   ├── navigate-directory-backward.ts # Browser-like back navigation
    │   ├── navigate-directory-forward.ts  # Browser-like forward navigation
    │   ├── refresh-directory.ts        # Reload current directory
    │   ├── remove-storage.ts           # Remove single storage entry
    │   └── remove-all-storage.ts       # Remove all storage for device
    └── selectors/                      # File-per-selector pattern
        ├── index.ts                    # withStorageSelectors() custom feature
        ├── get-selected-directory-state.ts      # Current directory state
        ├── get-selected-directory-for-device.ts # Selected directory metadata
        ├── get-device-storage-entries.ts        # All storage for device
        └── get-device-directories.ts            # All directories for device

libs/features/player/src/lib/player-view/player-device-container/
└── storage-container/                  # Feature components
    ├── storage-container.component.ts  # Container orchestrating all storage features
    ├── directory-tree/                 # Hierarchical tree navigation
    │   ├── directory-tree.component.ts
    │   └── directory-tree-node/
    ├── directory-files/                # File and folder list display
    │   ├── directory-files.component.ts
    │   ├── file-item/                  # File rendering component
    │   └── directory-item/             # Directory rendering component
    ├── directory-trail/                # Breadcrumb + navigation controls
    │   ├── directory-trail.component.ts
    │   ├── directory-breadcrumb/       # Path breadcrumb display
    │   └── directory-navigate/         # Back/forward/up/refresh buttons
    ├── search-toolbar/                 # Search functionality (future)
    └── filter-toolbar/                 # Filter controls
```

---

## Domain Models

The storage domain uses shared models from `libs/domain` and defines application-specific state structures. All models follow TypeScript interface patterns with comprehensive metadata support.

### Shared Domain Models (libs/domain/src/lib/models)

**Core Storage Models**:
- **StorageDirectory**: Container holding directories, files, and current path
- **DirectoryItem**: Simple name/path reference for subdirectories
- **FileItem**: Comprehensive file metadata including type, size, media info, images, and compatibility flags (shared with player domain)
- **ViewableItemImage**: Image asset metadata with URLs constructed from API base path

**Enumerations**:
- **StorageType**: Device types (SD, USB)
- **FileItemType**: File classification (Song, Game, Image, Hex, Unknown)

**Key Model Characteristics**:
- **Shared with Player**: FileItem and ViewableItemImage used across both domains
- **Rich Metadata**: FileItem includes 20+ properties for media files (title, creator, playLength, subtunes, etc.)
- **Image URLs**: Full URLs constructed in DomainMapper from baseAssetPath + baseApiUrl
- **Type Safety**: Strong TypeScript typing throughout

### Application State Models (libs/application/src/lib/storage)

**StorageDirectoryState**: Per-storage entry in flat state structure
- Contains: deviceId, storageType, currentPath, directory, loading/loaded flags, error, timestamp
- Represents single storage (SD or USB) on a specific device

**SelectedDirectory**: Tracks currently selected directory per device
- Contains: deviceId, storageType, path
- Enables UI to highlight/display current location

**NavigationHistory**: Browser-like history per device
- Array of NavigationHistoryItem (path + storageType)
- Tracks currentIndex pointer into history array
- Max 50 entries per device with automatic pruning

**StorageState**: Root state structure
- `storageEntries`: Record keyed by `"${deviceId}-${storageType}"` (flat structure)
- `selectedDirectories`: Record keyed by deviceId
- `navigationHistory`: Record keyed by deviceId

**State Design Decisions**:
- **Flat Structure**: Composite keys instead of nested objects for performance
- **Multi-Device Independence**: Separate state/history per device
- **Error Resilience**: Null directory on errors, explicit error messages
- **Timestamp Tracking**: lastLoadTime for cache management

---

## Domain Contracts

Domain contracts define the interface boundaries between layers. The IStorageService contract specifies how the domain communicates with infrastructure services.

### IStorageService Interface (libs/domain/src/lib/contracts/storage.contract.ts)

**Purpose**: Domain contract for infrastructure layer operations focused on external API integration.

**Core Responsibilities**:
- Directory retrieval from TeensyROM API
- Storage indexing operations on devices
- Observable-based async operations

**Interface Operations**:
- `getDirectory(deviceId, storageType, path?)`: Returns Observable<StorageDirectory>
- `index(deviceId, storageType, startingPath?)`: Triggers indexing for specific storage
- `indexAll()`: Triggers indexing across all devices

**Injection Token**: `STORAGE_SERVICE` for dependency injection

**Design Focus**:
- Pure infrastructure boundary (no business logic)
- Simple API integration without orchestration
- Observable streams for async operations
- Application layer handles all complex workflows

---

## Infrastructure Layer

The infrastructure layer handles external system integration and implements domain contracts with concrete services.

### StorageService (libs/infrastructure/src/lib/storage/storage.service.ts)

**Purpose**: Implements IStorageService for API integration and domain model mapping.

**Key Behaviors**:
- Calls FilesApiService (generated TypeScript client) for directory operations
- Transforms API DTOs to domain models via DomainMapper
- Extracts base API URL for image asset URL construction
- Maps HTTP errors to domain exceptions
- Stateless operations (no caching or state management)

**Implementation Focus**:
- Pure infrastructure (no business logic)
- Direct API integration
- Observable-based responses
- Error handling with diagnostics logging

### DomainMapper (libs/infrastructure/src/lib/domain.mapper.ts)

**Purpose**: Centralized transformation between API DTOs and domain models.

**Key Transformations**:
- `toStorageDirectory()`: Maps API StorageCacheDto to StorageDirectory
- `toDirectoryItem()`: Maps API DirectoryItemDto to DirectoryItem
- `toFileItem()`: Maps API FileItemDto to FileItem (20+ properties)
- `toViewableItemImage()`: Constructs full image URLs from baseAssetPath
- `toApiStorageType()` / `toDomainStorageType()`: Bidirectional enum conversion

**Mapping Characteristics**:
- Null safety with fallback values (`??` operators)
- URL construction for image assets (baseApiUrl + baseAssetPath)
- Shared by both Storage and Player infrastructure services
- Comprehensive metadata mapping for all domain models

---

## Application Layer

The application layer orchestrates storage workflows through NgRx Signal Store, managing state and coordinating infrastructure service calls.

### StorageStore (libs/application/src/lib/storage/storage-store.ts)

**Purpose**: Reactive state management for storage operations via NgRx Signal Store.

**Store Structure**:
- Root-level injection (global singleton)
- Redux DevTools integration ('storage' namespace)
- Custom features for selectors (`withStorageSelectors()`) and actions (`withStorageActions()`)
- Empty initial state (populated on device connect)

**State Organization**:
- Flat structure with composite keys for performance
- Follows STATE_STANDARDS.md patterns (file-per-action, file-per-selector)
- All state mutations via `updateState()` with action message correlation

### Storage Key Utilities (libs/application/src/lib/storage/storage-key.util.ts)

**Purpose**: Consistent composite key generation for flat state structure.

**Key Pattern**: `"${deviceId}-${storageType}"` template literal type

**Utility Functions**:
- `create()`: Generate key from deviceId + storageType
- `parse()`: Extract deviceId and storageType from key
- `forDevice()`: Filter predicate for device-specific keys
- `forStorageType()`: Filter predicate for storage-type-specific keys

**Benefits**: Type safety, consistent key format, query utilities

### State Helpers (libs/application/src/lib/storage/storage-helpers.ts)

**Purpose**: Reusable functions for state operations following STATE_STANDARDS.md.

**State Mutation Helpers** (require `actionMessage` for Redux DevTools):
- `setLoadingStorage()`: Set loading state, clear errors
- `setStorageLoaded()`: Mark loaded with timestamp, clear errors
- `setStorageError()`: Set error message, clear loading
- `setDeviceSelectedDirectory()`: Update selected directory for device
- `updateStorage()`: Generic state updates with partial properties
- `insertStorage()` / `removeStorage()`: Add/remove entries
- `createStorage()`: Initialize new storage entry

**State Query Helpers** (read-only, no actionMessage):
- `getStorage()`: Retrieve entry by key
- `isDirectoryLoadedAtPath()`: Check if directory loaded at path
- `isSelectedDirectory()`: Verify if directory is currently selected
- `getAllDeviceStorage()`: Filter all entries for device

**Helper Benefits**: Consistency, Redux DevTools correlation, maintainability, testability

---

## Storage Actions

All actions follow STATE_STANDARDS.md patterns: async/await with `firstValueFrom()`, helper functions for mutations, action message correlation for Redux DevTools.

### Core Actions (libs/application/src/lib/storage/actions/)

**initialize-storage.ts**: Initial root directory load
- Sets selected directory to root
- Cache check (skip if already loaded)
- Creates storage entry, loads from API
- Initializes navigation history

**navigate-to-directory.ts**: Navigate to specific path
- Updates selection if needed
- Cache optimization (skip API if loaded)
- Loads directory, adds to history
- Truncates forward history (browser behavior)
- Maintains 50-entry history limit

**navigate-up-one-directory.ts**: Navigate to parent directory
- Calculates parent path (handles edge cases)
- No-op at root
- Loads parent, adds to history

**navigate-directory-backward.ts**: Browser back navigation
- Decrements history index (if possible)
- Updates selection to history target
- Cache check before API call
- Preserves history array (no truncation)

**navigate-directory-forward.ts**: Browser forward navigation
- Increments history index (if possible)
- Same pattern as backward (opposite direction)

**refresh-directory.ts**: Force reload from API
- Bypasses cache
- Updates current entry with fresh data
- No history modification

**remove-storage.ts**: Remove single storage entry
- Called when storage unavailable
- No history/selection changes

**remove-all-storage.ts**: Device cleanup
- Removes all entries for device
- Clears selection and history
- Called on device disconnect

---

## Storage Selectors

All selectors return computed signals following STATE_STANDARDS.md patterns. Pure read-only operations with no state mutations.

### Core Selectors (libs/application/src/lib/storage/selectors/)

**get-selected-directory-state.ts**: Current directory state for device
- Returns: `Signal<StorageDirectoryState | null>`
- Combines selectedDirectories[deviceId] with storageEntries lookup
- Use: Directory contents, loading/error states, current path

**get-selected-directory-for-device.ts**: Selected directory metadata
- Returns: `SelectedDirectory | null`
- Simple lookup in selectedDirectories
- Use: Breadcrumb display, storage type indicator, navigation params

**get-device-storage-entries.ts**: All storage entries for device
- Returns: `Signal<Record<string, StorageDirectoryState>>`
- Filters storageEntries by device prefix
- Use: Storage type switcher, device storage overview

**get-device-directories.ts**: Flattened list of loaded directories
- Returns: `Signal<StorageDirectoryState[]>`
- Filters all entries by deviceId
- Use: Directory tree component, cache analysis

---

## Navigation System

Browser-like navigation with forward/backward/up operations and history tracking.

### Navigation History

**Architecture**:
- Per-device independent history
- Array of NavigationHistoryItem (path + storageType)
- currentIndex pointer to current position
- Max 50 entries (oldest pruned)

**Browser Behavior**: Back/forward use index, direct navigation truncates forward entries (same as browser address bar)

### Navigation Operations

**Forward/Backward**: Change index only, preserve history array, cache check before API
**Up**: Calculate parent path, add to history (truncates forward)
**Direct**: Navigate to path, add to history (truncates forward)

**Key Behaviors**:
- Index-based traversal (no array modification for back/forward)
- New navigation truncates forward entries
- Size limit enforcement via array slicing
- Cache optimization for instant navigation

### Cache Optimization

**Pattern**: Check `isDirectoryLoadedAtPath()` before API calls

**Benefits**: Instant cached navigation, reduced bandwidth, better UX
**Invalidation**: Refresh bypasses cache, device disconnect clears all

---

## Cross-Domain Integration

Clean boundaries with loose coupling between domains.

### Device Domain

**Lifecycle Integration**:
- Device connect → `initializeStorage()` for available storage types (SD/USB)
- Device disconnect → `removeAllStorage()` clears all state

**State Ownership**:
- Device domain: Connection state
- Storage domain: Navigation state
- No circular dependencies

### Player Domain

**File Launch**:
- Player reads FileItem from storage directory
- Storage provides file lists for player navigation
- Player stores StorageKey reference to launch location

**Shared Models**: FileItem, DirectoryItem, StorageType (from libs/domain)

**State Isolation**: Storage unaware of player state, player doesn't modify storage

### Search (Future)

Search results as FileItem arrays rendered by storage-container components. Search state separate, results launchable via player.

---

## UI Component Architecture

Feature layer components consume StorageStore with proper presentation/state separation.

### Storage Container (libs/features/player/.../storage-container/)

**Purpose**: Top-level orchestrator, no business logic

**Structure**: Injects StorageStore, passes deviceId to children (directory-tree, directory-files, directory-trail, search, filter)

### Directory Tree

**Purpose**: Hierarchical tree navigation

**Key Features**:
- Material Tree with lazy loading (placeholder nodes)
- Auto-expansion of device/storage nodes
- Component-local cache synced with store via effect
- Click/keyboard navigation

**State**: `getDeviceDirectories()`, `getSelectedDirectoryState()` selectors

**Actions**: `navigateToDirectory()` on click

### Directory Files

**Purpose**: File/directory list in current folder

**Key Features**:
- Renders DirectoryItem and FileItem lists
- Auto-scroll to playing file
- Keyboard navigation, virtual scrolling

**State**: `getSelectedDirectoryState()` → files/directories arrays

**Actions**: `navigateToDirectory()` on directory double-click, player launch on file double-click

### Directory Trail

**Purpose**: Breadcrumb + navigation controls

**Sub-components**:
- **DirectoryBreadcrumb**: Clickable path segments
- **DirectoryNavigate**: Back/Forward/Up/Refresh buttons

**State**: Computed canNavigate signals from history (currentIndex checks)

**Actions**: `navigateDirectoryBackward/Forward/UpOneDirectory/refreshDirectory()`

### Filter/Search Toolbars

**Filter**: UI-only (future player integration)
**Search**: Future feature (results in directory-files component)

---

## State Management Standards

Storage domain follows `docs/STATE_STANDARDS.md` patterns strictly.

### Key Patterns

**Store Organization**: Custom features (`withStorageSelectors()`, `withStorageActions()`) with file-per-action and file-per-selector structure

**Action Message Tracking**: All mutations use `createAction()` with same message for Redux DevTools correlation (e.g., `[navigate-to-directory] [1234]`)

**Async/Await**: Primary pattern with `firstValueFrom()` for Observable→Promise conversion, deterministic execution, try/catch error handling

**State Mutations**: MUST use `updateState()` with `actionMessage` (NOT `patchState()`). Mutation helpers require actionMessage parameter, query helpers don't.

**Logging**: Operation lifecycle with LogType enum (🧭 Navigate, 📡 NetworkRequest, ✅ Success, 🏁 Finish, ℹ️ Info, ❌ Error)

---

## Testing Strategy

The storage domain testing strategy focuses on behavioral testing at appropriate architectural layers, ensuring durability and maintainability through testing public interfaces rather than implementation details.

### Domain Layer Testing

**Focus**: Contract and model validation

**Test Coverage**:
- **IStorageService Interface**: Verify contract structure and injection token
- **Domain Models**: Test TypeScript interfaces compile correctly
- **Enum Types**: Validate StorageType and FileItemType values

**No Implementation Testing**: Domain layer has no implementations, only contracts and types

### Infrastructure Layer Testing

**Focus**: External integration and domain mapping

**Test Coverage**:
- **StorageService**: Mock FilesApiService via dependency injection
- **API Integration**: Test getDirectory, index, indexAll methods
- **Error Handling**: HTTP error scenarios and error propagation
- **DomainMapper**: Test all transformation functions with edge cases

**Testing Pattern**:
```typescript
describe('StorageService', () => {
  let service: StorageService;
  let mockApiService: jasmine.SpyObj<FilesApiService>;

  beforeEach(() => {
    mockApiService = jasmine.createSpyObj('FilesApiService', ['getDirectory', 'index', 'indexAll']);
    service = new StorageService(mockApiService);
  });

  it('should transform API response to domain model', async () => {
    const apiResponse = { storageItem: { /* DTO */ } };
    mockApiService.getDirectory.and.returnValue(Promise.resolve(apiResponse));

    const result = await firstValueFrom(service.getDirectory('device-1', StorageType.Sd, '/'));

    expect(result).toEqual(expectedDomainModel);
  });
});
```

### Application Layer Testing

**Focus**: Behavioral testing with full application layer integration

**Test Coverage**:
- **Store Actions**: Test all action workflows with mocked IStorageService
- **Selectors**: Verify computed signals react to state changes
- **Helpers**: Test state mutation and query helpers independently
- **Multi-Device**: Verify independent device state management
- **Navigation**: Test history forward/backward/up operations
- **Cache Behavior**: Verify API call optimization and cache checks

**Testing Pattern** (from storage-store.spec.ts):
```typescript
describe('StorageStore', () => {
  let store: InstanceType<typeof StorageStore>;
  let mockStorageService: jasmine.SpyObj<IStorageService>;

  beforeEach(() => {
    mockStorageService = jasmine.createSpyObj<IStorageService>('IStorageService',
      ['getDirectory', 'index', 'indexAll']
    );

    TestBed.configureTestingModule({
      providers: [
        StorageStore,
        { provide: STORAGE_SERVICE, useValue: mockStorageService },
      ],
    });

    store = TestBed.inject(StorageStore);
  });

  it('should initialize storage and load root directory', async () => {
    const mockDirectory = { directories: [], files: [], path: '/' };
    mockStorageService.getDirectory.and.returnValue(of(mockDirectory));

    await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });

    const state = store.getSelectedDirectoryState('device-1')();
    expect(state?.currentPath).toBe('/');
    expect(state?.directory).toEqual(mockDirectory);
    expect(state?.isLoaded).toBe(true);
  });

  it('should skip API call if directory already loaded', async () => {
    // Load directory first
    mockStorageService.getDirectory.and.returnValue(of(mockDirectory));
    await store.navigateToDirectory({ deviceId: 'device-1', storageType: StorageType.Sd, path: '/foo' });

    // Navigate to same path again
    mockStorageService.getDirectory.calls.reset();
    await store.navigateToDirectory({ deviceId: 'device-1', storageType: StorageType.Sd, path: '/foo' });

    expect(mockStorageService.getDirectory).not.toHaveBeenCalled();
  });
});
```

**Key Testing Principles**:
- **Mock Infrastructure Only**: Use strongly typed mocks for IStorageService
- **Test Behaviors**: Focus on action outcomes, not implementation details
- **Signal Reactivity**: Test computed signals react to state changes
- **Multi-Device Scenarios**: Verify device isolation and independence
- **Error Paths**: Test error handling and recovery workflows

### Feature Layer Testing

**Focus**: Component integration with store

**Test Coverage**:
- **Storage Container**: Test orchestration of child components
- **Directory Tree**: Test tree building, lazy loading, selection
- **Directory Files**: Test file/directory rendering, selection, navigation
- **Directory Trail**: Test breadcrumb display, button states, navigation

**Testing Pattern**:
```typescript
describe('DirectoryTreeComponent', () => {
  let component: DirectoryTreeComponent;
  let fixture: ComponentFixture<DirectoryTreeComponent>;
  let mockStore: jasmine.SpyObj<StorageStore>;

  beforeEach(() => {
    mockStore = createStorageStoreMock();

    TestBed.configureTestingModule({
      imports: [DirectoryTreeComponent],
      providers: [{ provide: StorageStore, useValue: mockStore }],
    });

    fixture = TestBed.createComponent(DirectoryTreeComponent);
    component = fixture.componentInstance;
  });

  it('should call navigateToDirectory when directory clicked', () => {
    spyOn(mockStore, 'navigateToDirectory');
    const node = { deviceId: 'device-1', storageType: StorageType.Sd, path: '/foo' };

    component.onDirectoryClick(node);

    expect(mockStore.navigateToDirectory).toHaveBeenCalledWith({
      deviceId: 'device-1',
      storageType: StorageType.Sd,
      path: '/foo',
    });
  });
});
```

### Integration Testing

**Focus**: Cross-domain and end-to-end workflows

**Test Coverage**:
- **Device Lifecycle**: Test storage initialization on device connect
- **Cleanup Integration**: Test storage removal on device disconnect
- **Player Integration**: Test file launch from storage context
- **Navigation Workflows**: Test complete navigation sequences
- **Error Recovery**: Test failure and recovery scenarios

---

## Success Criteria

Success criteria define the measurable outcomes that validate the storage system's effectiveness across architectural, functional, and business dimensions.

### Architecture Goals

- **Clean Boundaries**: Well-defined interfaces between Domain, Infrastructure, Application, and Feature layers
- **Shared Domain Models**: Consistent FileItem, DirectoryItem usage across Storage and Player domains
- **Centralized Mapping**: Single DomainMapper for all API transformations
- **Flat State Structure**: Efficient storage state with composite key pattern
- **Extensibility**: Easy to add new storage features (search, favorites, playlists)
- **Testability**: Comprehensive test coverage with mocked dependencies
- **Standards Compliance**: Full adherence to STATE_STANDARDS.md patterns

### Functional Goals

- **Multi-Device Support**: Independent storage state and navigation per device
- **Browser-Like Navigation**: Forward/backward/up navigation with history tracking
- **Cache Optimization**: Intelligent API call reduction with cache checking
- **Error Resilience**: Robust error handling and graceful degradation
- **Storage Operations**: Reliable directory browsing, file listing, and metadata display
- **Device Lifecycle**: Proper initialization and cleanup on connect/disconnect
- **Cross-Domain Integration**: Seamless integration with Device and Player domains

### Quality Goals

- **Type Safety**: Full TypeScript coverage with strict typing
- **Performance**: Efficient state management with minimal overhead
- **Maintainability**: Clear code organization following Clean Architecture
- **Documentation**: Complete domain documentation and examples
- **Developer Experience**: Intuitive APIs following established patterns
- **User Experience**: Fast, responsive directory navigation with visual feedback

### Operational Goals

- **Redux DevTools Integration**: Full visibility into state mutations with action correlation
- **Logging Standards**: Comprehensive operation logging following LOGGING_STANDARDS.md
- **Error Visibility**: Clear error messages and diagnostic information
- **Cache Transparency**: Visible cache hits in logs for optimization analysis
- **History Management**: Reliable history tracking with size limits and cleanup
