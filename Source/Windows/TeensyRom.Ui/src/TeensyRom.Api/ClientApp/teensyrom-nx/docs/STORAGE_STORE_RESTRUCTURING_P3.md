# Phase 3: Documentation Updates for Store Patterns

**High Level Plan Documentation**: [Storage Store Restructuring](./STORAGE_STORE_RESTRUCTURING.md)

## Phase Navigation

- Back to Plan: [STORAGE_STORE_RESTRUCTURING.md](./STORAGE_STORE_RESTRUCTURING.md)
- Previous: [Phase 2 — Extract Utilities](./STORAGE_STORE_RESTRUCTURING_P2.md)

**Standards Documentation**:

- [Coding Standards](./CODING_STANDARDS.md)
- [Store Testing](./STORE_TESTING.md)
- [State Standards](./STATE_STANDARDS.md)
- [Domain Standards](./DOMAIN_STANDARDS.md)
- [Testing Standards](./TESTING_STANDARDS.md)
- [Style Guide](./STYLE_GUIDE.md)

## Objective

Update standards to reflect the “inline methods + shared helpers” approach for NgRx Signal Store implementations. Provide clear code examples and guidance derived from the refactored `StorageStore`. No code behavior changes; tests must continue to pass exactly as before.

## Required Reading

- docs/STATE_STANDARDS.md
- libs/domain/storage/state/src/lib/storage-store.ts (post-Phase 2)
- libs/domain/storage/state/src/lib/storage-helpers.ts (post-Phase 2)
- libs/domain/storage/state/src/lib/storage-store.spec.ts

## Implementation Tasks

### Task 1: Update State Standards for inline methods

**Purpose**: Make inline methods the recommended pattern.

- [ ] Replace language recommending separate `methods/` files with guidance on inlining methods inside `withMethods`.
- [ ] Add reasons: tighter cohesion, fewer files, simpler discovery, easier refactors.

### Task 2: Document helper utility usage

**Purpose**: Provide consistent guidance for common patterns.

- [ ] Add a “Helpers” section with examples mirroring `setLoading`, `setSuccess`, `setError`, `ensureEntry`, and `withAsync`.
- [ ] Explain when to use (and when not to use) helpers to avoid semantic changes.
- [ ] Include notes about preserving public contracts and behavior during refactors.

### Task 3: Provide code examples

**Purpose**: Enable teams to adopt the pattern quickly.

- [ ] Add a small example store showing `withState`, `withComputed`, and `withMethods` with inline methods.
- [ ] Show how to call a service via `firstValueFrom`, perform state transitions with helpers, and handle errors.
- [ ] Reference `StorageStore` as a real-world exemplar.

### Task 4: Testing guidance reminder

**Purpose**: Reinforce that refactors must keep tests green.

- [ ] Add explicit note: “During phased refactoring, all existing tests must continue to pass; the public API contract must be adhered to.”
- [ ] Link to `STORE_TESTING.md` and `TESTING_STANDARDS.md`.

## File Changes

- Modified: `docs/STATE_STANDARDS.md`

## Testing Requirements

- None beyond documentation proofreading. Reiterate that tests must continue to pass during all phases.

## Success Criteria

- [ ] `STATE_STANDARDS.md` reflects inline methods as the default.
- [ ] Helper usage is clearly documented with examples.
- [ ] References point to `StorageStore` as an exemplar.

## Notes

- Keep examples minimal and idiomatic to this codebase.
- Do not introduce new architectural patterns beyond those established in Phases 1 and 2.
