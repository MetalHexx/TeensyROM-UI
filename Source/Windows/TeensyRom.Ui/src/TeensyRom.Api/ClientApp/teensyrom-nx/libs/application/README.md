# Application Library

This library contains the application-level state management for the TeensyROM UI application, following clean architecture principles by separating domain logic from application state.

## Purpose

The application library serves as the centralized location for:
- Cross-cutting application state management
- Application-level business logic coordination  
- State orchestration between different domains

## Current Structure

### Device State Management
- **Location**: `src/lib/device/`
- **Store**: `device-store.ts` - NgRx Signal Store for device state management
- **Methods**: `methods/` - Individual store method implementations
  - `find-devices.ts` - Device discovery functionality
  - `connect-device.ts` - Device connection logic
  - `disconnect-device.ts` - Device disconnection logic
  - `index-storage.ts` - Storage indexing for single device
  - `index-all-storage.ts` - Storage indexing for all devices
  - `ping-devices.ts` - Device health check functionality
  - `reset-all-devices.ts` - Reset all connected devices

### Storage State Management ✨ NEW!
- **Location**: `src/lib/storage/`
- **Store**: `storage-store.ts` - NgRx Signal Store for storage state management
- **Utilities**: `storage-key.util.ts`, `storage-helpers.ts` - Storage utilities
- **Actions**: `actions/` - Storage action implementations (8 methods)
  - `initialize-storage.ts` - Initialize storage for devices
  - `navigate-to-directory.ts` - Navigate to specific directory
  - `navigate-directory-backward.ts` - Navigate backward in history
  - `navigate-directory-forward.ts` - Navigate forward in history
  - `navigate-up-one-directory.ts` - Navigate up one level
  - `refresh-directory.ts` - Refresh current directory
  - `remove-storage.ts` - Remove storage for single device
  - `remove-all-storage.ts` - Remove all storage
- **Selectors**: `selectors/` - Storage selector implementations (5 methods)
  - `get-device-directories.ts` - Get directories for device
  - `get-device-storage-entries.ts` - Get storage entries for device
  - `get-selected-directory-for-device.ts` - Get selected directory
  - `get-selected-directory-state.ts` - Get directory state

## Usage

Import the DeviceStore from the application library:

```typescript
import { DeviceStore } from '@teensyrom-nx/application';

@Component({...})
export class MyComponent {
  private readonly deviceStore = inject(DeviceStore);
}
```

## Dependencies

- **Domain Services**: `@teensyrom-nx/domain/device/services` - Interfaces and models
- **Infrastructure**: `@teensyrom-nx/infrastructure` - Concrete service implementations

## Architecture Compliance

This library follows the established clean architecture pattern:
- **Domain libraries** contain pure models and interfaces
- **Application library** contains state management and coordination logic
- **Feature libraries** consume application state for UI components
- **Infrastructure libraries** provide concrete implementations

## Future Expansion

This library is designed to accommodate additional application state:
- Storage state management (planned migration from domain layer)
- Player state coordination
- Global application settings
- Cross-domain state synchronization

## Standards Compliance

This library follows the Nx Library Standards:
- Non-buildable library (consumed as source code)
- Proper import path: `@teensyrom-nx/application`
- Clean barrel exports in `src/index.ts`
- Proper dependency declarations in `project.json`
