# Storage State Machine Documentation

This document provides comprehensive state machine diagrams for the StorageStore, illustrating how each method transitions state and manages the dual-state architecture.

## Overview

The StorageStore implements a **dual-state architecture**:

- **Local State**: Per device-storage combination using flat keys (`${deviceId}-${storageType}`)
- **Global State**: Single `selectedDirectory` across all devices for UI highlighting

## State Properties Reference

### StorageDirectoryState

```typescript
interface StorageDirectoryState {
  deviceId: string; // Device identifier
  storageType: StorageType; // SD or USB
  currentPath: string; // Current directory path
  directory: StorageDirectory | null; // Loaded directory data
  isLoaded: boolean; // Successfully loaded from API
  isLoading: boolean; // Currently loading from API
  error: string | null; // Last error message
  lastLoadTime: number | null; // Timestamp of last successful load
}
```

### SelectedDirectory

```typescript
interface SelectedDirectory {
  deviceId: string; // Selected device
  storageType: StorageType; // Selected storage type
  path: string; // Selected directory path
}
```

### StorageState

```typescript
interface StorageState {
  storageEntries: Record<StorageKey, StorageDirectoryState>; // Flat key structure
  selectedDirectory: SelectedDirectory | null; // Global selection
}
```

## Key Design Decisions

### Flat State Structure

- **Reason**: O(1) lookups for device-storage combinations
- **Key Format**: `${deviceId}-${storageType}` (e.g., "device123-SD")
- **Benefit**: Scales efficiently with multiple devices

### Dual-State Architecture

- **Local State**: Independent navigation per device-storage
- **Global State**: Single UI selection across all devices
- **Benefit**: Supports complex multi-device UI requirements

### Smart Caching

- **Strategy**: Avoid redundant API calls for already-loaded directories
- **Logic**: Check path, loaded state, data existence, and error state
- **Benefit**: Improved performance and reduced server load

### Inline Entry Creation

- **Pattern**: `navigateToDirectory()` creates storage entries if they don't exist
- **Reason**: Simplifies component usage - no explicit initialization required
- **Trade-off**: Less explicit but more convenient

## Complete Storage Entry State Machine

```mermaid
stateDiagram-v2
    [*] --> Empty

    Empty --> Initialized : initializeStorage()

    state "Storage Entry Lifecycle" as StorageEntry {
        state "Initialized" as Initialized {
            Initialized --> Loading : navigateToDirectory()<br/>(needs API call)
            Initialized --> Initialized : navigateToDirectory()<br/>(already loaded - cached)
        }

        state "Loading" as Loading {
            Loading --> Loaded : API Success
            Loading --> Error : API Error
        }

        state "Loaded" as Loaded {
            Loaded --> Loading : navigateToDirectory()<br/>(different path)
            Loaded --> Loading : refreshDirectory()
            Loaded --> Loaded : navigateToDirectory()<br/>(same path - cached)
        }

        state "Error" as Error {
            Error --> Loading : navigateToDirectory()
            Error --> Loading : refreshDirectory()
        }
    }

    Initialized --> [*] : cleanupStorage()
    Loading --> [*] : cleanupStorage()
    Loaded --> [*] : cleanupStorage()
    Error --> [*] : cleanupStorage()

    note right of StorageEntry
        Local State Properties:
        - deviceId: string
        - storageType: StorageType
        - currentPath: string
        - directory: StorageDirectory | null
        - isLoading: boolean
        - isLoaded: boolean
        - error: string | null
        - lastLoadTime: number | null
    end note
```

## Method-Specific State Diagrams

### 1. initializeStorage()

```mermaid
stateDiagram-v2
    [*] --> CheckExists

    CheckExists --> AlreadyExists : Entry exists
    CheckExists --> CreateEntry : Entry does not exist

    CreateEntry --> Initialized : Create with default state
    AlreadyExists --> NoChange : Skip creation

    Initialized --> [*]
    NoChange --> [*]

    note right of CreateEntry
        Creates entry with:
        - currentPath: "/"
        - directory: null
        - isLoaded: false
        - isLoading: false
        - error: null
        - lastLoadTime: null
    end note
```

### 2. navigateToDirectory()

```mermaid
stateDiagram-v2
    [*] --> UpdateGlobalSelection

    UpdateGlobalSelection --> CheckCaching : Always update selectedDirectory

    CheckCaching --> SkipAPI : Directory already loaded
    CheckCaching --> SetLoading : Need to load directory

    SkipAPI --> Complete : Use cached data

    SetLoading --> CallAPI : Set isLoading=true, error=null

    CallAPI --> APISuccess : StorageService.getDirectory()
    CallAPI --> APIError : API failure

    APISuccess --> UpdateSuccess : Set directory, isLoaded=true, lastLoadTime
    APIError --> UpdateError : Set error, isLoading=false

    UpdateSuccess --> Complete
    UpdateError --> Complete

    Complete --> [*]

    note right of CheckCaching
        Caching Logic:
        - Same currentPath?
        - isLoaded = true?
        - directory exists?
        - no error?

        If all true: Skip API call
    end note
```

### 3. refreshDirectory()

```mermaid
stateDiagram-v2
    [*] --> CheckEntryExists

    CheckEntryExists --> NoOp : Entry does not exist
    CheckEntryExists --> SetLoading : Entry exists

    NoOp --> [*]

    SetLoading --> CallAPI : Set isLoading=true, error=null

    CallAPI --> APISuccess : StorageService.getDirectory(currentPath)
    CallAPI --> APIError : API failure

    APISuccess --> UpdateSuccess : Update directory, isLoaded=true, lastLoadTime
    APIError --> UpdateError : Set error, preserve existing directory

    UpdateSuccess --> [*]
    UpdateError --> [*]

    note right of UpdateError
        On Error:
        - isLoading = false
        - error = message
        - Preserve existing directory data
        - Keep isLoaded state
    end note
```

### 4. cleanupStorage()

```mermaid
stateDiagram-v2
    [*] --> DeviceCleanup

    DeviceCleanup --> FindMatches : Find all entries with deviceId
    FindMatches --> RemoveEntries : Delete matching entries
    RemoveEntries --> [*]

    note right of FindMatches
        Uses StorageKeyUtil.forDevice()
        to find all keys matching:
        "${deviceId}-*"
    end note
```

```mermaid
stateDiagram-v2
    [*] --> TypeCleanup

    TypeCleanup --> FindSpecificEntry : Find specific deviceId-storageType
    FindSpecificEntry --> RemoveEntry : Delete single entry
    RemoveEntry --> [*]

    note right of FindSpecificEntry
        Targets specific key:
        "${deviceId}-${storageType}"
    end note
```

## Global Selection State Machine

```mermaid
stateDiagram-v2
    [*] --> NoSelection

    NoSelection --> Selected : navigateToDirectory()
    Selected --> Selected : navigateToDirectory()<br/>(updates to new selection)
    Selected --> NoSelection : cleanupStorage()<br/>(if selected storage cleaned)

    note right of Selected
        Global selectedDirectory:
        {
          deviceId: string
          storageType: StorageType
          path: string
        }

        Updated on EVERY navigateToDirectory() call
        regardless of caching
    end note
```

## Smart Caching Flow

```mermaid
flowchart TD
    A[navigateToDirectory called] --> B[Update global selection]
    B --> C{Entry exists?}

    C -->|No| D[Create entry inline]
    C -->|Yes| E{Caching check}

    D --> F[Set loading state]

    E -->|Cache hit| G[Skip API call - use cached data]
    E -->|Cache miss| F

    F --> H[Call StorageService.getDirectory]
    G --> I[Complete]

    H --> J{API Success?}
    J -->|Yes| K[Update directory data]
    J -->|No| L[Set error state]

    K --> I
    L --> I

    I --> M[End]

    subgraph "Caching Logic"
        N[Same currentPath?]
        O[isLoaded = true?]
        P[directory exists?]
        Q[no error?]

        N --> R{All conditions true?}
        O --> R
        P --> R
        Q --> R

        R -->|Yes| S[Cache Hit]
        R -->|No| T[Cache Miss]
    end
```

## Multi-Device State Interaction

```mermaid
sequenceDiagram
    participant UI
    participant Store
    participant Service

    Note over UI,Service: Initialize multiple devices
    UI->>Store: initializeStorage(dev1, SD)
    UI->>Store: initializeStorage(dev1, USB)
    UI->>Store: initializeStorage(dev2, SD)

    Note over UI,Service: Navigate to different devices
    UI->>Store: navigateToDirectory(dev1, SD, /games)
    Store->>Service: getDirectory(dev1, SD, /games)
    Service-->>Store: directory data
    Note over Store: selectedDirectory = dev1, SD, /games

    UI->>Store: navigateToDirectory(dev2, SD, /music)
    Store->>Service: getDirectory(dev2, SD, /music)
    Service-->>Store: directory data
    Note over Store: selectedDirectory = dev2, SD, /music
    Note over Store: dev1-SD state preserved with /games

    Note over UI,Service: Cleanup on device disconnect
    UI->>Store: cleanupStorage(dev1)
    Note over Store: Removes dev1-SD and dev1-USB entries
    Note over Store: selectedDirectory unchanged points to dev2
```

## Error Handling State Transitions

```mermaid
stateDiagram-v2
    [*] --> OperationStart

    OperationStart --> ClearError : Set error = null
    ClearError --> SetLoading : Set isLoading = true

    SetLoading --> APICall

    state "API Call" as APICall {
        [*] --> Pending
        Pending --> Success : API responds successfully
        Pending --> Failure : API throws error

        Success --> [*] : (transition to outer state)
        Failure --> [*] : (transition to outer state)
    }

    Success --> UpdateSuccess : Update state with data
    Failure --> UpdateError : Set error message

    UpdateSuccess --> CompleteSuccess : isLoading = false, isLoaded = true
    UpdateError --> CompleteError : Set isLoading = false, preserve data

    CompleteSuccess --> [*]
    CompleteError --> [*]

    note right of UpdateError
        Error Recovery Strategy:
        - Preserve existing directory data
        - Set descriptive error message
        - Allow retry through navigation/refresh
        - Global selection still updates
    end note
```
