# Player Feature Component Structure

This document provides a visual overview of the Player feature component hierarchy, showing the relationships between components and their input/output properties.

## Component Hierarchy

```mermaid
graph TD
    PV[PlayerViewComponent<br/>lib-player-view]

    PV --> |"connectedDevices: Device[]"| PDC[PlayerDeviceContainerComponent<br/>lib-player-device-container]

    PDC --> |"device: Device"| PT[PlayerToolbarComponent<br/>lib-player-toolbar]
    PDC --> |"device: Device"| SC[StorageContainerComponent<br/>lib-storage-container]
    PDC --> |"device: Device"| FI[FileImageComponent<br/>lib-file-image]
    PDC --> |"device: Device"| FO[FileOtherComponent<br/>lib-file-other]

    SC --> ST[SearchToolbarComponent<br/>lib-search-toolbar]
    SC --> DT[DirectoryTreeComponent<br/>lib-directory-tree]
    SC --> DF[DirectoryFilesComponent<br/>lib-directory-files]

    classDef rootComponent fill:#1565c0,stroke:#0d47a1,stroke-width:3px,color:#ffffff
    classDef containerComponent fill:#7b1fa2,stroke:#4a148c,stroke-width:2px,color:#ffffff
    classDef leafComponent fill:#2e7d32,stroke:#1b5e20,stroke-width:2px,color:#ffffff
    classDef storageComponent fill:#f57c00,stroke:#e65100,stroke-width:2px,color:#ffffff

    class PV rootComponent
    class PDC containerComponent
    class SC,PT storageComponent
    class FI,FO,ST,DT,DF leafComponent
```

## Data Flow

```mermaid
sequenceDiagram
    participant DS as DeviceStore
    participant PV as PlayerView
    participant PDC as PlayerDeviceContainer
    participant SC as StorageContainer
    participant DT as DirectoryTree
    participant DF as DirectoryFiles

    DS->>PV: devices()
    PV->>PV: filter connected devices
    PV->>PDC: device: Device
    PDC->>SC: (device context)
    PDC->>PT: (device context)
    PDC->>FI: creatorName, metadataSource
    PDC->>FO: filename, releaseInfo, meta1, meta2, metadataSource
    SC->>DT: (tree data - Phase 4)
    SC->>DF: (directory content - Phase 5)
    SC->>ST: (search context)
```

## File Structure

```
libs/features/player/src/lib/player-view/
├── player-view.component.ts              # Root component
├── player-device-container/
│   ├── player-device-container.component.ts  # Device container
│   ├── player-toolbar/                   # Device actions
│   ├── file-image/                       # Image file display
│   ├── file-other/                       # Generic file display
│   └── storage-container/                # Storage coordinator
│       ├── storage-container.component.ts
│       ├── search-toolbar/               # Search interface
│       ├── directory-tree/               # Navigation tree
│       └── directory-files/              # File listing
```
