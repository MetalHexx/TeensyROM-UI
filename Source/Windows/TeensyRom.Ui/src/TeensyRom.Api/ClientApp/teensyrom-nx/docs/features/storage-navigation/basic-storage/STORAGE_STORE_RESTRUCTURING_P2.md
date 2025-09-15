# Phase 2: Extract Common Patterns and Create Utilities

**High Level Plan Documentation**: [Storage Store Restructuring](./STORAGE_STORE_RESTRUCTURING.md)

## Phase Navigation

- Back to Plan: [STORAGE_STORE_RESTRUCTURING.md](./STORAGE_STORE_RESTRUCTURING.md)
- Previous: [Phase 1 — Consolidate Methods](./STORAGE_STORE_RESTRUCTURING_P1.md)
- Next: [Phase 3 — Documentation Updates](./STORAGE_STORE_RESTRUCTURING_P3.md)

**Standards Documentation**:

- [Coding Standards](./CODING_STANDARDS.md)
- [Store Testing](./STORE_TESTING.md)
- [State Standards](./STATE_STANDARDS.md)
- [Domain Standards](./DOMAIN_STANDARDS.md)
- [Testing Standards](./TESTING_STANDARDS.md)
- [Style Guide](./STYLE_GUIDE.md)

## Objective

Create reusable helper utilities to remove duplication in `StorageStore` while preserving the store’s public contract and behavior. Focus on standardizing loading/error state transitions, validating entries, and wrapping async operations.

Tests must continue to pass unchanged in this phase.

## Required Reading

- libs/domain/storage/state/src/lib/storage-store.ts
- libs/domain/storage/state/src/lib/storage-key.util.ts
- libs/domain/storage/state/src/lib/storage-store.spec.ts
- docs/STATE_STANDARDS.md

## Implementation Tasks

### Task 1: Add storage helper utilities

**Purpose**: Centralize repeated state patterns and validations.

- [ ] Create `libs/domain/storage/state/src/lib/storage-helpers.ts` with:
  - [ ] `type SignalStore<T> = { [K in keyof T]: () => T[K] }` (local type to match current method signatures)
  - [ ] `export type WritableStore<T> = SignalStore<T> & import('@ngrx/signals').WritableStateSource<T>`
  - [ ] `export function setLoading(store: WritableStore<StorageState>, key: StorageKey, error: string | null = null)`
  - [ ] `export function setSuccess(store: WritableStore<StorageState>, key: StorageKey, updates: Partial<StorageDirectoryState>)`
  - [ ] `export function setError(store: WritableStore<StorageState>, key: StorageKey, error: string)`
  - [ ] `export function ensureEntry(store: WritableStore<StorageState>, key: StorageKey, create: () => StorageDirectoryState): void` (no-op if exists, otherwise inserts)
  - [ ] `export async function withAsync<T>(op: () => Promise<T>, onError: (e: unknown) => void): Promise<T | undefined>` (helper wrapper)

Notes:

- Keep helpers internal to the storage domain. Do not export them outside this domain yet.
- Keep types identical to existing store state shapes.

### Task 2: Apply helpers in store methods (no behavior changes)

**Purpose**: Reduce duplication and standardize state transitions.

- [ ] In `initializeStorage`:

  - [ ] Use `ensureEntry` when first creating a key to avoid manual initial entry duplication.
  - [ ] Use `setLoading` before API call.
  - [ ] After success, call `setSuccess` with: `{ directory, isLoaded: true, isLoading: false, error: null, lastLoadTime: Date.now(), currentPath: '/' }` (ensure all properties match current behavior).
  - [ ] On error, call `setError` with the error message string (match current message format exactly).

- [ ] In `navigateToDirectory`:

  - [ ] Preserve selection update logic first (do not move into helpers; behavior must remain identical and explicit).
  - [ ] Use `setLoading` prior to API call.
  - [ ] On success, call `setSuccess` with updates that include `currentPath` and `directory` and timestamp.
  - [ ] On error, call `setError` with `'Failed to navigate to directory'`.

- [ ] In `refreshDirectory`:

  - [ ] Guard on missing entry as currently done; do not change log text.
  - [ ] Use `setLoading` prior to API call.
  - [ ] On success, call `setSuccess` with updated `directory` and timestamp.
  - [ ] On error, call `setError` with `'Failed to refresh directory'`.

- [ ] In `cleanupStorage` and `cleanupStorageType` leave logic as-is; helpers unnecessary here.

### Task 3: Keep method contracts and logs stable

**Purpose**: Ensure tests remain green with zero expectation changes.

- [ ] Do not change method names, parameter shapes, or return types.
- [ ] Preserve logging placements and messages where they may be relevant to timing/flow in tests.

## File Changes

- Created: `libs/domain/storage/state/src/lib/storage-helpers.ts`
- Modified: `libs/domain/storage/state/src/lib/storage-store.ts` (refactor to use helpers)

## Testing Requirements

- Tests must continue to pass; the public contract is unchanged.
- Unit focus:
  - [ ] Initialization still loads root and sets flags/timestamps identically.
  - [ ] Navigate respects cache hit and selection update behavior.
  - [ ] Refresh updates `lastLoadTime` and leaves `currentPath` intact.
  - [ ] Edge cases (empty deviceId/path, rapid navigation, concurrent ops) behave identically.
- Optional: add unit tests for helpers if they can be tested in isolation without changing existing public tests.

## Success Criteria

- [ ] Helpers exist and are used in store methods.
- [ ] Store methods read cleaner, with no behavior changes.
- [ ] All unit tests pass unchanged.

## Notes

- If a helper risks changing timing or intermediate state, don’t use it. Match current semantics exactly.
- Avoid over-engineering; keep helpers minimal and specific to current duplication.
