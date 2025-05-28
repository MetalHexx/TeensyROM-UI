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

### 6. API Client

- Located in `libs/api-client`, generated using OpenAPI generator CLI.
- Consumed by services only (not directly by components).
- API responses are mapped into frontend models using domain-specific `.mapper.ts` files.

---

## Nx Workspace Tree (Explicit Structure)

```bash
apps/
├── teensyrom-ui/                          # [app] Main Angular standalone application
│   └── src/app/
│       ├── app.component.ts
│       ├── routes.ts
│       └── app.config.ts                 # Boot providers

libs/
├── app-boot/                              # [library] Domain: Bootstrapping
│   └── app-bootstrap/
│       └── app-bootstrap.service.ts

├── app-state/                             # [library group] Domain: Global State Stores
│   ├── device-store/                      # [library] Connected device state
│   ├── storage-store/                     # [library] File/directory state
│   └── settings-store/                    # [library] App/user settings state

├── services/                              # [library group] Domain: Business Logic Services
│   ├── device/                            # [library] Device: houses device domain business logic
│   │   ├── device.service.ts              # Device orchestration logic
│   │   ├── device.mapper.ts               # Maps API responses to frontend models
│   │   └── device.models.ts               # Domain model definitions for device logic
│   ├── storage/                           # [library] Storage: houses storage domain business logic
│   │   ├── storage.service.ts             # File metadata, launch logic
│   │   ├── storage.mapper.ts              # Maps API responses to frontend models
│   │   └── storage.models.ts              # Domain model definitions for storage logic
│   └── settings/                          # [library] Settings: houses settings domain business logic
│       └── settings.service.ts            # Settings persistence logic

├── features/                              # [library group] Domain: Feature Modules
│   ├── devices/                           # [library] UI for device overview
│   └── settings/                          # [library] UI for application settings

├── layout/                                # [library group] Domain: App Layout
│   └── shell/                             # [library] App frame with nav + header

├── api-client/                            # [library] Generated OpenAPI client
│   ├── apis/                              # [folder] API service classes
│   ├── models/                            # [folder] Data models
│   └── scripts/generate-client.ts         # [script] OpenAPI generator CLI integration

├── ui/                                    # [library] Presentational-only reusable components
```
