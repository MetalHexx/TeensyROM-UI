# TeensyROM Angular Nx Monorepo Context & Architecture

## Project Overview

This is a **hybrid .NET/Angular application** for TeensyROM device management and media playback. The backend is a .NET 9 Web API using **RadEndpoints** for minimal APIs and **MediatR** for CQRS patterns. The frontend is an **Angular 19 application** built with **Nx monorepo** architecture.

### Technology Stack

**Backend (.NET API)**:

- .NET 9 with RadEndpoints minimal APIs
- MediatR for CQRS patterns
- SignalR for real-time communication
- Scalar API documentation (replaces Swagger)

**Frontend (Angular/Nx)**:

- Angular 19 with standalone components
- Nx workspace with domain-driven library organization
- NgRx Signal Store for state management
- Angular Material UI components
- Vitest for unit testing, Cypress for E2E

## Architecture Goals

- Provide a scalable, modular structure for an Angular app that interacts with TeensyROM devices
- Use a shared generated OpenAPI client library with post-processing
- Keep services domain-specific and state reactive and isolated using Signal Stores
- Implement clean architecture with proper dependency boundaries
- Maintain application-wide storage metadata state, including file launch history
- Prefer Signals over Observables where appropriate (Observables remain for async operations)

---

## Key Domain Responsibilities

### 1. App Bootstrap & Shell

- **Bootstrap Service**: `libs/app/bootstrap` - Handles application startup, device discovery, SignalR connections
- **Shell Components**: `libs/app/shell` - Layout, navigation, header components with integrated device state
- **Navigation Service**: `libs/app/navigation` - Routing and navigation management

### 2. Device Domain

- **Services**: `libs/domain/device/services` - API calls for device discovery, connection, and lifecycle management
- **State**: `libs/domain/device/state` - NgRx Signal Store for device state management with methods
- **Models**: Pure domain models with DTOs mapped via `device.mapper.ts`
- **Real-time**: SignalR integration for device logs and events

### 3. Storage Domain

- **Services**: `libs/domain/storage/services` - File system navigation, directory listing, metadata retrieval
- **State**: `libs/domain/storage/state` - Signal Store for storage state keyed by `(deviceId, storageType)`
- **Models**: Domain models for directories, files, and metadata with proper type transformations
- **Integration**: Full HTTP client integration with MSW testing support

### 4. Player Features

- **Components**: `libs/features/player` - File browser UI, player controls, device-specific player containers
- **Integration**: Connects storage domain services with UI components
- **Navigation**: Directory tree navigation and file listing functionality

### 5. API Client Integration

- **Location**: `libs/data-access/api-client` - Generated TypeScript client from OpenAPI spec
- **Post-processing**: Automatic renaming of `*Service` to `*ApiService` for clarity
- **Architecture**: Promise-based client wrapped with RxJS Observables in domain services
- **Generation**: Build-time OpenAPI generation without requiring running server

---

## Implementation Status

### ✅ Completed Features

- **Device Management**: Full discovery, connection, disconnection, and real-time status monitoring
- **SignalR Integration**: Real-time device logs and events with proper connection management
- **OpenAPI Client**: Complete TypeScript client generation with post-processing pipeline
- **Application Shell**: Navigation, layout, header with integrated device state and busy dialogs
- **Storage Domain Services**: HTTP client integration with domain model transformations
- **Storage State Management**: Signal Store implementation with storage key utilities
- **Player UI Structure**: Complete component hierarchy with device-specific containers

### 🚧 In Progress

- **Storage Navigation**: Directory tree and file listing UI components (scaffolded)
- **File Operations**: Launch, search, and metadata handling functionality
- **Testing Infrastructure**: MSW integration testing and comprehensive test coverage

### 📋 Planned Features

- **Player Controls**: Playback controls, file launching, and media management
- **Settings Domain**: User preferences, application configuration, and persistence
- **DJ Mixer Features**: Advanced audio mixing and MIDI I/O capabilities
- **Theme System**: Complete theming infrastructure and customization
- **File Launch History**: Persistent metadata and usage tracking

---

## Current Nx Workspace Structure

```bash
apps/                                           # [group] Applications
├── teensyrom-ui/                               # [app] Main Angular UI (standalone components)
│   └── src/app/
│       ├── app.component.ts                    # [file] Root component with routing
│       ├── app.routes.ts                       # [file] Route configuration (devices, player)
│       └── app.config.ts                       # [file] App providers and bootstrap config
└── teensyrom-ui-e2e/                          # [e2e] Cypress end-to-end tests

libs/
├── app/                                        # [group] Application-level libraries
│   ├── bootstrap/                              # [library] App initialization service
│   │   └── app-bootstrap.service.ts            # [file] Handles startup, device discovery, SignalR
│   ├── navigation/                             # [library] Navigation service and utilities
│   └── shell/                                  # [library] Layout components and app shell
│       ├── layout.component.ts                 # [file] Main layout with navigation and busy dialogs
│       ├── header/                             # [component] App header with device status
│       ├── nav-menu/                           # [component] Side navigation menu
│       └── busy-dialog/                        # [component] Loading dialogs for operations

├── domain/                                     # [group] Domain logic libraries
│   ├── device/                                 # [domain] Device management domain
│   │   ├── services/                           # [library] Device API and SignalR services
│   │   │   ├── device.service.ts               # [file] HTTP API client wrapper (Promise→Observable)
│   │   │   ├── device.mapper.ts                # [file] API DTO → domain model transformations
│   │   │   ├── device.models.ts                # [file] Domain device models and enums
│   │   │   ├── device-logs.service.ts          # [file] SignalR real-time logging service
│   │   │   └── device-events.service.ts        # [file] SignalR device event handling
│   │   └── state/                              # [library] NgRx Signal Store for device state
│   │       ├── device-store.ts                 # [file] Main SignalStore with computed selectors
│   │       └── methods/                        # [folder] Store methods (extracted for reusability)
│   │           ├── connect-device.ts           # [file] Device connection logic
│   │           ├── disconnect-device.ts        # [file] Device disconnection logic
│   │           ├── find-devices.ts             # [file] Device discovery and auto-connect
│   │           ├── index-all-storage.ts        # [file] Bulk storage indexing across devices
│   │           ├── index-storage.ts            # [file] Individual storage indexing logic
│   │           ├── ping-devices.ts             # [file] Device connectivity checking
│   │           ├── reset-all-devices.ts        # [file] Bulk device reset operations
│   │           └── index.ts                    # [file] Method barrel exports
│
│   └── storage/                                # [domain] Storage/file system domain
│       ├── STORAGE_DOMAIN.md                   # [doc] Domain documentation and overview
│       ├── services/                           # [library] Storage API services with full implementation
│       │   ├── storage.service.ts              # [file] HTTP client for directory/file operations
│       │   ├── storage.mapper.ts               # [file] API DTO → domain model mapping
│       │   ├── storage.models.ts               # [file] Directory, file, and metadata models
│       │   └── *.spec.ts                       # [files] Comprehensive unit and integration tests
│       └── state/                              # [library] Storage state management
│           ├── storage-store.ts                # [file] SignalStore keyed by (deviceId, storageType)
│           ├── storage-key.util.ts             # [file] Storage key utilities for composite keys
│           ├── storage-helpers.ts              # [file] Reusable state update helpers shared across actions
│           ├── actions/                        # [folder] Storage state actions (navigation and management)
│           │   ├── initialize-storage.ts       # [file] Storage initialization logic
│           │   ├── navigate-directory-backward.ts # [file] Backward directory navigation
│           │   ├── navigate-directory-forward.ts # [file] Forward directory navigation
│           │   ├── navigate-to-directory.ts    # [file] Direct directory navigation
│           │   ├── navigate-up-one-directory.ts # [file] Parent directory navigation
│           │   ├── refresh-directory.ts        # [file] Directory refresh and reload
│           │   ├── remove-all-storage.ts       # [file] Bulk storage cleanup
│           │   ├── remove-storage.ts           # [file] Individual storage cleanup
│           │   └── index.ts                    # [file] Action barrel exports
│           └── selectors/                      # [folder] Storage state selectors
│               ├── get-device-directories.ts   # [file] Device directory selection logic
│               ├── get-device-storage-entries.ts # [file] Storage entry retrieval selectors
│               ├── get-selected-directory-for-device.ts # [file] Device-specific directory selection
│               ├── get-selected-directory-state.ts # [file] Selected directory state selectors
│               └── index.ts                    # [file] Selector barrel exports

├── features/                                   # [group] Feature-level UI libraries
│   ├── devices/                                # [library] Device management UI (feature-device)
│   │   ├── device-view.component.ts            # [file] Main device list view with toolbar
│   │   ├── device-item/                        # [component] Individual device card display
│   │   ├── device-logs/                        # [component] Real-time device log display
│   │   └── device-toolbar/                     # [component] Device management actions
│
│   └── player/                                 # [library] Player UI with storage navigation
│       ├── player-view.component.ts            # [file] Main player view with device containers
│       └── player-device-container/            # [component] Device-specific player interface
│           ├── storage-container/              # [component] File browser container
│           │   ├── directory-tree/             # [component] Folder tree navigation (scaffolded)
│           │   ├── directory-files/            # [component] File listing display (scaffolded)
│           │   └── search-toolbar/             # [component] File search UI (scaffolded)
│           ├── file-image/                     # [component] Image file preview (scaffolded)
│           ├── file-other/                     # [component] Generic file display (scaffolded)
│           └── player-toolbar/                 # [component] Playback controls (scaffolded)

├── ui/                                         # [group] Shared UI libraries
│   ├── components/                             # [library] Reusable presentational components (ui-components)
│   │   ├── icon-label/                         # [component] Icon with label display utility
│   │   └── storage-status/                     # [component] USB/SD card status indicators
│   └── styles/                                 # [library] Design system and theming (ui-styles)
│       └── theme.scss                          # [file] SCSS design tokens and Material theme

├── utils/                                      # [library] Shared utilities and helpers
│   ├── log-helper.ts                           # [file] Logging utilities (info, warn, error)
│   └── store-helper.ts                         # [file] Store action message generation utilities

└── data-access/
    └── api-client/                             # [library] Generated OpenAPI TypeScript client
        ├── apis/                               # [folder] Generated API services (DevicesApiService, etc.)
        │   ├── DevicesApiService.ts            # [file] Device API client (post-processed naming)
        │   ├── FilesApiService.ts              # [file] Storage/Files API client
        │   └── PlayerApiService.ts             # [file] Player API client
        ├── models/                             # [folder] Generated DTO models and types
        ├── scripts/                            # [folder] OpenAPI generation and post-processing
        └── runtime.ts                          # [file] Generated runtime configuration
```

---

## Development Patterns & Standards

### Angular 19 Modern Patterns

- **Standalone Components**: All components use standalone architecture with direct imports
- **Modern Control Flow**: Uses `@if`, `@for`, `@switch` instead of structural directives
- **Signal-based APIs**: Prefers `input()` and `output()` over `@Input()` and `@Output()`
- **Signal State Management**: NgRx Signal Store for reactive state with computed selectors

### API Client Integration

- **Generation**: Build-time OpenAPI generation from .NET API (no running server required)
- **Post-processing**: Automatic renaming of `*Service` to `*ApiService` for clarity
- **Architecture**: Promise-based TypeScript client wrapped with RxJS in domain services
- **Domain Mapping**: Never import API types directly - always map through domain services

### Domain-Driven Design

- **Service Boundaries**: Clear separation between domain services and API clients
- **Dependency Injection**: Use injection tokens and provider patterns for testability
- **Model Transformation**: API DTOs → Domain Models via dedicated mapper classes
- **Error Handling**: Consistent error handling and transformation at service boundaries

### State Management Patterns

- **Signal Stores**: NgRx Signal Store with actions/methods extracted to separate files
- **Store Helpers**: Reusable state update functions shared across actions (e.g., `setLoadingStorage`, `updateStorage`)
- **Computed Values**: Reactive computed properties for derived state
- **Key-based Storage**: Storage state keyed by `(deviceId, storageType)` combinations using utility functions
- **Selectors**: Dedicated selector functions for complex state queries and derivations
- **Effect Management**: Proper effect lifecycle and cleanup management

### Testing Standards

- **Unit Testing**: Vitest for fast unit tests with comprehensive coverage
- **Integration Testing**: MSW (Mock Service Worker) for HTTP client testing
- **E2E Testing**: Cypress for end-to-end user workflows
- **Test Patterns**: Mock domain interfaces, not API clients, for better test isolation

### Code Organization

- **Barrel Exports**: Clean public APIs for all libraries via `index.ts`
- **File Naming**: Consistent naming conventions (`.service.ts`, `.mapper.ts`, `.models.ts`)
- **Library Boundaries**: Strict import rules between domain, features, and UI layers
- **Documentation**: Domain overview docs (`DOMAIN_NAME.md`) for each domain library
