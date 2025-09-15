# Storage Store Restructuring — Tech Debt & Side Quests

This document tracks follow-ups discovered during the Storage Store restructuring. These are not required for Phases 1–3, but are useful cleanups or improvements. Unless explicitly noted, do not change the store’s public contract; all existing tests must continue to pass.

## Logging and Encodings

- [ ] Normalize log strings: there are some odd/unicode characters in current logs (likely encoding artifacts) in method files. Standardize to readable ASCII where feasible without affecting behavior.
- [ ] Consider a simple structured logger wrapper for store methods (info/warn/error) to unify patterns and ease future de-duplication.

## Error Messages and Consistency

- [ ] Centralize error message strings in helpers (Phase 2 follow-up) while keeping exact text used by tests: ‘Failed to initialize storage’, ‘Failed to navigate to directory’, ‘Failed to refresh directory’.
- [ ] Ensure all branches set `isLoading` to `false` on error paths.

## Selection and Cleanup Semantics

- [ ] Re-validate semantics when cleaning up a device: current behavior removes all entries and selected directory for the device. Verify consumers aren’t relying on selection to persist post-cleanup. If changes are made, update tests accordingly in a dedicated PR.
- [ ] Add an optional `preserveSelection?: boolean` for `cleanupStorage` if a use case emerges.

## Concurrency & Caching

- [ ] Evaluate introducing a lightweight in-flight map to avoid duplicate API calls for the same key/path when rapid navigation occurs (preserve current behavior in tests; add new tests if behavior is improved).
- [ ] Consider TTL-based refresh logic for `refreshDirectory` (configurable), if product requirements expect periodic refresh.

## Helpers and Reuse (Post-Phase 2)

- [ ] Promote helper utilities to a shared `state/utils` submodule if used by multiple stores; otherwise keep scoped to storage domain.
- [ ] Add targeted unit tests for helpers once stabilized (without modifying existing public tests).

## Spec Hygiene

- [ ] Extract common factory functions (e.g., `createMockStorageDirectory`) into a local test utils module.
- [ ] Verify test coverage for edge cases like empty deviceId/path remains robust; consider parameterized tests to reduce duplication.

## Documentation Polishing (Post-Phase 3)

- [ ] Add a brief migration guide: moving from separate method files to inline methods, with before/after examples.
- [ ] Add a cookbook snippet for ‘navigate with cache + refresh’ pattern to `STATE_STANDARDS.md`.

## Non-Goals (for now)

- Changing public API surface of `StorageStore`.
- Replacing `async/await` style with alternative patterns.
- Introducing new dependencies for logging or state orchestration.

## Test Contract Reminder

In all related phases and side quests, tests must continue to pass and the existing public contract must be adhered to. Any behavior change requires explicit agreement and updated tests in a separate, clearly scoped PR.
