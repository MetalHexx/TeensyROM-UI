# Phase 1: Consolidate Storage Methods Into Store

**High Level Plan Documentation**: [Storage Store Restructuring](./STORAGE_STORE_RESTRUCTURING.md)

## Phase Navigation

- Back to Plan: [STORAGE_STORE_RESTRUCTURING.md](./STORAGE_STORE_RESTRUCTURING.md)
- Next: [Phase 2 — Extract Utilities](./STORAGE_STORE_RESTRUCTURING_P2.md)

**Standards Documentation**:

- [Coding Standards](./CODING_STANDARDS.md)
- [Store Testing](./STORE_TESTING.md)
- [State Standards](./STATE_STANDARDS.md)
- [Domain Standards](./DOMAIN_STANDARDS.md)
- [Testing Standards](./TESTING_STANDARDS.md)
- [Style Guide](./STYLE_GUIDE.md)

## Objective

Move all storage methods from separate files back into the main `StorageStore` using NgRx Signal Store best practices. Preserve all public method names, signatures, and behaviors so existing tests keep passing without modification.

Explicitly: inline `initializeStorage`, `navigateToDirectory`, `refreshDirectory`, and `cleanupStorage` (plus `cleanupStorageType`) into `storage-store.ts` and remove the dedicated `methods/` directory.

## Required Reading

- libs/domain/storage/state/src/lib/storage-store.ts
- libs/domain/storage/state/src/lib/methods/initialize-storage.ts
- libs/domain/storage/state/src/lib/methods/navigate-to-directory.ts
- libs/domain/storage/state/src/lib/methods/refresh-directory.ts
- libs/domain/storage/state/src/lib/methods/cleanup-storage.ts
- libs/domain/storage/state/src/lib/storage-key.util.ts
- libs/domain/storage/state/src/lib/storage-store.spec.ts
- libs/features/player/src/lib/player-view/player-view.component.ts

## Implementation Tasks

### Task 1: Inline existing method implementations

**Purpose**: Follow standard Signal Store patterns and reduce fragmentation.

- [ ] In `storage-store.ts`, remove imports of `./methods/*` and inline their returned method objects under the single `withMethods((store, storageService) => ({ ... }))` block.
- [ ] Keep method names and parameter/return types identical:
  - `initializeStorage({ deviceId, storageType }): Promise<void>`
  - `navigateToDirectory({ deviceId, storageType, path }): Promise<void>`
  - `refreshDirectory({ deviceId, storageType }): Promise<void>`
  - `cleanupStorage({ deviceId }): void`
  - `cleanupStorageType({ deviceId, storageType }): void`
- [ ] Copy existing logic exactly (including `patchState` patterns and `firstValueFrom(storageService.getDirectory(...))`).
- [ ] Preserve logging only if helpful; don’t change observable behavior.

### Task 2: Remove the methods directory

**Purpose**: Prevent drift and ensure single source of truth.

- [ ] Delete `libs/domain/storage/state/src/lib/methods/initialize-storage.ts`.
- [ ] Delete `libs/domain/storage/state/src/lib/methods/navigate-to-directory.ts`.
- [ ] Delete `libs/domain/storage/state/src/lib/methods/refresh-directory.ts`.
- [ ] Delete `libs/domain/storage/state/src/lib/methods/cleanup-storage.ts`.
- [ ] Remove the `methods/` directory if empty.

### Task 3: Verify consumer usage remains unchanged

**Purpose**: Ensure components and other stores still compile and behave the same.

- [ ] Confirm `PlayerViewComponent` and any other call sites still inject `StorageStore` and call the same methods.
- [ ] Confirm computed factory methods remain unchanged: `getDeviceStorageEntries`, `getDeviceDirectories`, `getSelectedDirectoryForDevice`, `getSelectedDirectoryState`.

## File Changes

- Modified: `libs/domain/storage/state/src/lib/storage-store.ts`
- Deleted: `libs/domain/storage/state/src/lib/methods/initialize-storage.ts`
- Deleted: `libs/domain/storage/state/src/lib/methods/navigate-to-directory.ts`
- Deleted: `libs/domain/storage/state/src/lib/methods/refresh-directory.ts`
- Deleted: `libs/domain/storage/state/src/lib/methods/cleanup-storage.ts`

## Testing Requirements

- Tests must continue to pass; the public contract is unchanged.
- Run the storage state spec and ensure zero behavioral regressions:
  - [ ] Store exposes all methods listed above.
  - [ ] Initialization loads root path and updates state as before.
  - [ ] Navigation updates `selectedDirectories`, sets loading, applies result, and respects cache behavior.
  - [ ] Refresh uses current path, sets loading, and updates `lastLoadTime`.
  - [ ] Cleanup removes device entries and storage-type entries as implemented.
  - [ ] Edge cases in the spec (empty deviceId/path, rapid navigation, concurrent ops) still pass.

## Success Criteria

- [ ] All storage methods live inline in `storage-store.ts`.
- [ ] All method file imports removed; `methods/` directory deleted.
- [ ] All unit tests pass unchanged; no API/behavior differences.

## Notes

- Don’t change names, signatures, or return types. This is purely an internal structure change.
- Keep `async/await` style and existing state shapes (`StorageDirectoryState`, `SelectedDirectory`, `StorageState`).
- If you must touch logs, keep their placement and timing identical to avoid side-effects in tests.
