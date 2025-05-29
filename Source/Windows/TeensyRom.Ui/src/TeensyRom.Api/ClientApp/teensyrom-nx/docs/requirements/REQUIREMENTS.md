# TeensyROM Angular Nx Monorepo Requirements

📄 [View this document in ChatGPT Canvas](https://chatgpt.com/canvas/shared/683696ad4bc88191b3a667189eba0196)

---

## Goals

- Provide a scalable, modular structure for an Angular app that interacts with TeensyROM devices.
- Centralize application and user preferences in a global settings store.
- Coordinate bootstrapping logic (e.g. auto-connect devices on startup).
- Use a shared generated API client library based on OpenAPI.
- Keep services domain-specific and state reactive and isolated.
- Maintain application-wide storage metadata state, including file launch history.

---

## Key Responsibilities

### 1. App Bootstrap

- Boot logic that runs on app startup.
- Reads app settings (e.g. auto-connect toggle).
- Initiates device discovery and connection.
- Sets initial application state.

### 2. Settings

- Covers both application-level configuration and user preferences.
- Stored in `settings-store`, updated via `settings.service.ts`.
- Can be persisted to local storage or synced to a backend.
- Loaded by `AppBootstrapService` during startup.

### 3. Device Services

- Perform backend API calls to discover, connect, and manage TeensyROM devices.
- Isolated in `services/device`, consuming the generated API client.
- Each device can have multiple associated storage types (SD and USB).
- All mapping between API response types and frontend application models must be implemented in `device.mapper.ts`.
- Domain models used by the device service are defined in `device.models.ts` within the same library.

### 4. Storage Services

- Operate independently from the device service domain.
- Accessed via `(deviceId, storageType)` pair.
- Responsible for file metadata, directory structures, indexing, and file launch logic.
- Encapsulation improves maintainability and isolates storage concerns from device connectivity.
- All mapping between API response types and frontend application models must be implemented in `storage.mapper.ts`.
- Domain models used by the storage service are defined in `storage.models.ts` within the same library.

### 5. Application State

- `device-store` manages state about all connected/available devices.
- `settings-store` manages global configuration and preferences.
- `storage-store` maintains directory and file metadata per connected device and per storage type (SD or USB).
  - Includes most recently launched files, cached directory indexes, or file filtering state.
- Reactive approach (Signals or RxJS) for change propagation.
- All application state libraries use **NgRx Signal Store** for consistency, immutability, and effect-driven updates.
- State is organized and owned per domain; shared state across domains must be intentional and minimal.

### 6. API Client

- Located in `libs/api-client`, generated using OpenAPI generator CLI.
- Consumed by services only (not directly by components).
- API responses are mapped into frontend models using domain-specific `.mapper.ts` files.

---

## Nx Workspace Tree (Explicit Structure + Annotated)

```bash
apps/                                       # [group] Applications
├── teensyrom-ui/                           # [app] Main Angular application
│   └── src/app/
│       ├── app.component.ts               # [file] Root component
│       ├── routes.ts                      # [file] Application routes
│       └── app.config.ts                  # [file] Root providers/bootstrap config

libs/                                       # [group] Libraries
├── app-boot/                               # [library] Application startup logic
│   └── app-bootstrap/
│       └── app-bootstrap.service.ts       # [file] Bootstrap orchestrator

├── app-state/                              # [group] Global state libraries (NgRx Signal Store)
│   ├── device-store/                       # [library] Device state
│   │   ├── device-store.ts                 # [file] SignalStore definition
│   │   └── device-store.models.ts          # [file] Optional: View model types
│
│   ├── storage-store/                      # [library] Storage metadata state
│   │   ├── storage-store.ts
│   │   └── storage-store.models.ts
│
│   └── settings-store/                     # [library] Settings and preferences state
│       ├── settings-store.ts
│       └── settings-store.models.ts

├── services/                               # [group] Domain service libraries
│   ├── device/                             # [library] Device business logic
│   │   ├── device.service.ts               # [file] Device orchestration
│   │   ├── device.mapper.ts                # [file] Maps API DTOs to frontend models
│   │   └── device.models.ts                # [file] Device domain model types
│
│   ├── storage/                            # [library] Storage business logic
│   │   ├── storage.service.ts
│   │   ├── storage.mapper.ts
│   │   └── storage.models.ts
│
│   └── settings/                           # [library] Settings persistence logic
│       └── settings.service.ts

├── features/                               # [group] Feature UI modules
│   ├── devices/                            # [library] Device-related views and components
│   └── settings/                           # [library] Settings UI and route logic

├── layout/                                 # [group] Global layout libraries
│   └── shell/                              # [library] Application shell (header/nav)

├── api-client/                             # [library] OpenAPI-generated client
│   ├── apis/                               # [folder] API service classes
│   ├── models/                             # [folder] Generated types/interfaces
│   └── scripts/generate-client.ts          # [file] OpenAPI generator script

├── ui/                                     # [library] Shared presentational components
```
