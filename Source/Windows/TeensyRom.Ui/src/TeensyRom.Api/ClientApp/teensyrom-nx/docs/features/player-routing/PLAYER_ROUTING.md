# Player View Routing & Initialization

## Overview

The player view route [Player View Component](../../../../apps/teensyrom-ui/src/app/features/player/player-view.component.ts) uses a [Player Route Resolver](../../../libs/app/navigation/src/lib/player-route.resolver.ts) to manage initialization in the background without blocking navigation. This allows the view to render immediately while storage setup and deep linking happen asynchronously.

## Architecture

### Non-Blocking Resolver Pattern

The `playerRouteResolver` ([`libs/app/navigation/src/lib/player-route.resolver.ts`](../../../libs/app/navigation/src/lib/player-route.resolver.ts)) executes all initialization asynchronously without awaiting:

```
Navigate to /player
    ↓
Resolver fires initialization (no await)
    ↓
Returns immediately
    ↓
Initialization continues in background
```

### Initialization Sequence

1. **Wait for DeviceStore** - Polls `hasInitialised()` until bootstrapper completes device discovery
2. **Initialize Storage** - Sets up StorageStore for all devices with available SD/USB storage
3. **Handle Deep Linking** - If route query parameters provided, navigates to specified directory and optionally launches file

## Deep Linking

Deep links trigger automatic navigation to a specific directory and optionally launch a file:

**URL Format:**

```
/player?device=DEVICE_ID&storage=SD|USB&path=/directory/path&file=filename.ext
```

**Query Parameters:**

- `device` (required) - Device ID to target
- `storage` (required) - Storage type ('SD' or 'USB')
- `path` (required) - Directory path to navigate to
- `file` (optional) - Filename to auto-launch after directory loads

**Example:**
[http://localhost:4200/player?device=PSM2ZAKI&storage=SD&path=/music/MUSICIANS/F/Freedom&file=Deep_Madness.sid](http://localhost:4200/player?device=PSM2ZAKI&storage=SD&path=/music/MUSICIANS/F/Freedom&file=Deep_Madness.sid)

## Key Benefits

- **Fast Navigation** - Page renders before storage initialization
- **Progressive Loading** - UI updates as data arrives
- **Deep Linking** - Direct navigation to specific directories/files
- **Asynchronous Initialization** - Non-blocking background processing
