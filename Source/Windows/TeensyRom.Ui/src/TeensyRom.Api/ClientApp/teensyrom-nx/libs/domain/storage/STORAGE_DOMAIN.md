# Storage Domain Overview

## Purpose

Manages storage device navigation and directory browsing for TeensyROM devices. Handles SD cards and USB storage with flat state structure, navigation history, and selection state for multi-device support.

## Architecture

- **Services**: API communication, data mapping, and dependency injection patterns
- **State**: NgRx Signal Store with navigation history, selection tracking, and computed selectors

---

## Services (`@teensyrom-nx/domain/storage/services`)

### StorageService

HTTP service implementing `IStorageService` interface for dependency injection.

- `getDirectory(deviceId, storageType, path)` - Fetches directory contents from API with error handling

### StorageMapper

Maps between API client types and domain types.

- `toApiStorageType()` - Domain enum → API client type
- `toDomainStorageType()` - API client type → domain enum
- `toStorageDirectory()` - API response → domain StorageDirectory

### Models

- `StorageType` enum - SD, USB storage types
- `FileItemType` enum - Game, Song, Image, Hex, Unknown
- `StorageDirectory` interface - Directory structure with files/folders
- `FileItem` interface - Rich file metadata (size, favorites, images, subtunes, creator info)
- `DirectoryItem` interface - Directory metadata (name, path)
- `ViewableItemImage` interface - Image metadata for files

### Dependency Injection

- `IStorageService` interface - Service contract for testability
- `STORAGE_SERVICE` token - Injection token for interface binding
- `STORAGE_SERVICE_PROVIDER` - Provider configuration for app root

---

## State (`@teensyrom-nx/domain/storage/state`)

### StorageStore

NgRx Signal Store with comprehensive navigation and selection management.

**State Structure**:

- `storageEntries` - `${deviceId}-${storageType}` keyed storage data
- `selectedDirectories` - Per-device selected directory tracking
- `navigationHistory` - Per-device navigation history with forward/backward support

### Core Methods

- `initializeStorage({ deviceId, storageType })` - Setup storage entry for device
- `navigateToDirectory({ deviceId, storageType, path })` - Navigate and load directory with history tracking
- `navigateDirectoryBackward({ deviceId })` - Navigate backward in history
- `navigateDirectoryForward({ deviceId })` - Navigate forward in history
- `navigateUpOneDirectory({ deviceId })` - Navigate to parent directory
- `refreshDirectory({ deviceId, storageType })` - Reload current directory
- `removeStorage({ deviceId, storageType })` - Remove specific storage entry
- `removeAllStorage({ deviceId })` - Remove all storage entries for device

### Selectors

- `getSelectedDirectoryForDevice(deviceId)` - Get selected directory for device
- `getSelectedDirectoryState(deviceId)` - Get full state for selected directory
- `getDeviceStorageEntries(deviceId)` - Get all storage entries for device
- `getDeviceDirectories(deviceId)` - Get directory data for device

### NavigationHistory

Per-device navigation history with configurable size limits.

- Tracks path and storage type for each navigation
- Supports backward/forward navigation with index tracking
- Automatic history size management with configurable limits

### StorageDirectoryState

Enhanced directory state tracking per storage entry.

- `directory` - StorageDirectory data or null on error
- `currentPath` - Current directory path
- `isLoaded/isLoading` - Loading state flags
- `error` - Error message if navigation failed
- `lastLoadTime` - Timestamp for cache invalidation

### StorageKeyUtil

Utility for storage state key management.

- `create(deviceId, storageType)` - Generate flat state key
- `parse(key)` - Extract deviceId/storageType from key
- `forDevice(deviceId)` - Filter function for device keys
- `forStorageType(storageType)` - Filter function for storage type keys

---

## Key Concepts

**Flat State**: Uses `device123-SD` style keys instead of nested objects for O(1) performance
**Multi-Device**: Supports multiple devices with multiple storage types simultaneously
**Navigation History**: Per-device backward/forward navigation with configurable history limits
**Selection State**: Tracks selected directory per device across storage type switches
**Domain Decoupling**: Interface-based dependency injection with API client type mapping
**Error Resilience**: Comprehensive error handling with null directory states and error messages
