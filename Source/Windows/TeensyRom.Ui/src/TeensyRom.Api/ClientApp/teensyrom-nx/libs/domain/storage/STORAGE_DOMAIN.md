# Storage Domain Overview

## Purpose

Manages storage device navigation and directory browsing for TeensyROM devices. Handles SD cards and USB storage with flat state structure for multi-device support.

## Architecture

- **Services**: API communication and data mapping
- **State**: NgRx Signal Store for navigation state management

---

## Services (`@teensyrom-nx/domain/storage/services`)

### StorageService

HTTP service for storage operations.

- `getDirectory(deviceId, storageType, path)` - Fetches directory contents from API

### StorageMapper

Maps between API client types and domain types.

- `toApiStorageType()` - Domain enum → API client type
- `toDomainStorageType()` - API client type → domain enum

### Models

- `StorageType` enum - SD, USB storage types
- `FileItemType` enum - Game, Song, Image, etc.
- `StorageDirectory` interface - Directory structure with files/folders
- `FileItem` interface - File metadata (size, title, creator, etc.)

---

## State (`@teensyrom-nx/domain/storage/state`)

### StorageStore

NgRx Signal Store managing navigation state with flat key structure.

**State Structure**: `${deviceId}-${storageType}` keys for O(1) lookups

### Methods

- `navigateToDirectory({ deviceId, storageType, path })` - Navigate and load directory contents
- `refreshDirectory({ deviceId, storageType })` - Reload current directory
- `initializeStorage({ deviceId, storageType })` - Setup storage entry for device
- `cleanupStorage({ deviceId, storageType })` - Remove storage entry on disconnect

### StorageKeyUtil

Utility for storage state key management.

- `create(deviceId, storageType)` - Generate flat state key
- `parse(key)` - Extract deviceId/storageType from key
- `forDevice(deviceId)` - Filter function for device keys
- `forStorageType(storageType)` - Filter function for storage type keys

---

## Key Concepts

**Flat State**: Uses `device123-SD` style keys instead of nested objects for performance
**Multi-Device**: Supports multiple devices with multiple storage types simultaneously  
**Domain Decoupling**: Maps API client types to domain enums to avoid Angular dependencies
