# TeensyROM Angular Nx Monorepo Requirements

## Goals

- Provide a scalable, modular structure for an Angular app that interacts with TeensyROM devices.
- Centralize application and user preferences in a global settings store.
- Coordinate bootstrapping logic (e.g. auto-connect devices on startup).
- Use a shared generated API client library based on OpenAPI.
- Keep services domain-specific and state reactive and isolated.
- Maintain application-wide storage metadata state, including file launch history.
- When at all possible (and pragmatic) use Signals over observables.
- Observables are fine when its well suited to the task.

---

## Key Responsibilities

### 1. App Bootstrap

- Boot logic that runs on app startup.
- Reads app settings (e.g. auto-connect toggle).
- Initiates device discovery and connection.
- Sets initial application state.
- Located in `libs/app/bootstrap`.

### 2. Settings

- Covers both application-level configuration and user preferences.
- Will be implemented in `libs/domain/settings` (planned).
- Uses a signal store (`settings-store.ts`).
- Can be persisted to local storage or synced to a backend.
- Bootstrapped via `AppBootstrapService`.

### 3. Device Services

- Backend API calls for device discovery and lifecycle.
- Lives in `libs/domain/device/services`.
- DTOs mapped to domain models via `device.mapper.ts`.
- Strong types in `device.models.ts`.
- Includes real-time logging via `device.logs.service.ts`.
- SignalR integration through `device.events.service.ts`.

### 4. Storage Services _(Planned)_

- Will exist in `libs/domain/storage`.
- Targeted around `(deviceId, storageType)` scoped operations.
- Will manage file indexing, launching, and metadata.
- Organized like `device/` with `services/`, `state/`, and mappers/models.

### 5. Application State

- Built with **NgRx Signal Store**.
- Located in `libs/domain/<context>/state`.
- Domain-specific: device, settings, and soon storage.
- Methods are extracted into `methods/` for separation of logic and reusability.

### 6. API Client

- Located in `libs/data-access/api-client`.
- Generated from OpenAPI.
- Postprocessed to rename `*Service` to `*ApiService`.
- Used only by service layer libraries (not UI).

---

## Implementation Status

### ✅ Completed Features

- **Device Management**: Discovery, connection, disconnection, and status monitoring
- **Real-time Communication**: SignalR integration for device logs and events
- **API Integration**: Complete OpenAPI-generated client with post-processing
- **Application Shell**: Navigation, layout, and routing infrastructure

### 🚧 In Progress

- **Player Interface**: Basic layout and component structure created, but functionality not implemented
- **Storage Domain**: Partially implemented through player features, needs dedicated domain library
- **Settings Management**: Bootstrap service exists, full settings UI and persistence pending

### 📋 Planned Features

- **Player Functionality**: File browsing, directory navigation, playback controls, and storage management
- **DJ Mixer**: Advanced audio mixing and MIDI I/O capabilities
- **Theme System**: Complete theming infrastructure and customization
- **File Launch History**: Persistent metadata and usage tracking

---

## Nx Workspace Tree (Explicit Structure + Annotated)

```bash
apps/                                           # [group] Applications
├── teensyrom-ui/                               # [app] Main Angular UI
│   └── src/app/
│       ├── app.component.ts                    # [file] Root component
│       ├── routes.ts                           # [file] App routing config
│       └── app.config.ts                       # [file] Root providers/bootstrap config

libs/
├── app/
│   └── bootstrap/                              # [library] App initialization logic
│       └── app-bootstrap.service.ts            # [file] Runs during app startup to init state

├── domain/                                     # [group] Domain logic libraries
│   ├── device/
│   │   ├── services/                            # [library] Device API logic
│   │   │   ├── device.service.ts                # [file] Core orchestration logic
│   │   │   ├── device.mapper.ts                 # [file] Translates DTOs to domain models
│   │   │   ├── device.models.ts                 # [file] Domain-side device models
│   │   │   ├── device.logs.service.ts           # [file] Real-time device logging
│   │   │   └── device.events.service.ts         # [file] SignalR event handling
│   │   └── state/                               # [library] Signal store for devices
│   │       ├── device-store.ts                  # [file] Main SignalStore definition
│   │       └── methods/                         # [folder] Store methods (RxJS powered)
│   │           ├── connect-device.ts            # [file] Connects a device
│   │           ├── disconnect-device.ts         # [file] Disconnects a device
│   │           └── find-devices.ts              # [file] Queries backend for device list

│   ├── storage/ *(planned)*                    # [library] Storage domain (coming soon)
│   │   ├── services/                            # [library] Storage API/service layer
│   │   │   ├── storage.service.ts               # [file] File/directory logic
│   │   │   ├── storage.mapper.ts                # [file] DTO → model mapping
│   │   │   └── storage.models.ts                # [file] Strong types for file/directory metadata
│   │   └── state/                               # [library] Signal store for storage
│   │       ├── storage-store.ts                 # [file] SignalStore for directory index
│   │       └── methods/                         # [folder] Store effects (indexing, launching)

│   └── settings/ *(planned)*                   # [library] Global app preferences
│       ├── services/                            # [library] Settings logic
│       │   └── settings.service.ts              # [file] Load/save settings
│       └── state/                               # [library] Signal store for settings
│           ├── settings-store.ts                # [file] SignalStore definition
│           └── settings-store.models.ts         # [file] Preferences and toggle models

├── features/                                   # [group] Route-level features (UI logic)
│   ├── devices/                                 # [library] Device UI
│   │   ├── device-view.component.ts             # [file] Device list with connect/disconnect
│   │   ├── device-item/                         # [component] Individual device display
│   │   ├── device-logs/                         # [component] Real-time device logging
│   │   └── device-toolbar/                      # [component] Device management actions
│
│   ├── settings/ *(planned)*                    # [library] Settings UI and forms
│   │   └── settings-view.component.ts           # [file] Preferences panel and UI toggles
│
│   ├── player/ *(in progress)*                  # [library] File browsing and playback features
│   │   └── player-view/                         # [component] Main player interface (layout only)
│   │       ├── player-device-container/         # [component] Device-specific player UI (scaffolding)
│   │       │   ├── storage-container/           # [component] File system browser (placeholder)
│   │       │   │   ├── directory-tree/          # [component] Folder navigation (not implemented)
│   │       │   │   ├── directory-files/         # [component] File listing (not implemented)
│   │       │   │   └── search-toolbar/          # [component] File search functionality (not implemented)
│   │       │   ├── file-image/                  # [component] Image file preview (not implemented)
│   │       │   ├── file-other/                  # [component] Generic file display (not implemented)
│   │       │   └── player-toolbar/              # [component] Playback controls (not implemented)
│
│   └── dj/ *(planned)*                          # [library] DJ-oriented features
│       └── dj-panel.component.ts                # [file] Advanced track control and MIDI I/O

├── ui/                                         # [group] Shared presentational components
│   ├── ui-components/
│   │   ├── icon-label/                          # [component] Icon+label display
│   │   └── storage-status/                      # [component] USB/SD card status indicators
│   └── styles/
│       └── theme.scss                           # [file] SCSS design tokens and theme settings

├── data-access/
│   └── api-client/                              # [library] Generated OpenAPI client
│       ├── apis/                                # [folder] Generated API service classes
│       ├── models/                              # [folder] Generated DTOs
│       └── scripts/                             # [folder] OpenAPI generator & patch logic
```
