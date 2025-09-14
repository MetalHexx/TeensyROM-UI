# Store Testing Methodology

This document describes the standard methodology for testing NgRx Signal Stores in this repository. It focuses on testing stores via their public API, using Angular TestBed with Vitest, and mocking external dependencies at the DI boundary.

## Overview

- Test stores through their public methods and state signal getters.
- Use Angular TestBed to instantiate the store as it runs in the app.
- Mock external services (HTTP/data) with strongly typed mocks.
- Prefer fast, reliable unit tests; reserve integration tests for cross‑library scenarios.

## Test Types

- Unit (Store-Level):

  - Exercise store methods and assert state via signal getters.
  - Mock dependencies at the injection boundary.
  - Cover normal, error, and edge case paths.

- Method-Level (Optional):

  - Only when isolating single function behavior adds value.
  - Prefer store-level tests to validate composition and state interaction.

- Integration:
  - Multiple services/stores interacting; use MSW for HTTP.
  - No real backend calls (see TESTING_STANDARDS.md).

## Environment & Setup

- Runner: Vitest via `@nx/vite:test`.
- Environment: `jsdom`.
- Per-library setup: `src/test-setup.ts` ensures Angular TestBed + Zones are initialized (e.g., `@analogjs/vitest-angular/setup-zone`).
- Devtools: `withDevtools()` is fine; Node logs about missing Redux DevTools are harmless.

## Mocking Strategy

1. Define an interface for each external service the store depends on (e.g., `IExampleService`).
2. Provide an InjectionToken for the interface (e.g., `EXAMPLE_SERVICE`).
3. In the app shell, map the token to the concrete class with `useExisting`.
4. In tests, provide the token with a small, strongly typed mock.

Example:

```ts
// interface + token (production code)
export interface IExampleService {
  getData(id: string): Observable<Data>;
}
export const EXAMPLE_SERVICE = new InjectionToken<IExampleService>('EXAMPLE_SERVICE');

// app providers (production code)
export const EXAMPLE_SERVICE_PROVIDER = { provide: EXAMPLE_SERVICE, useExisting: ExampleService };

// test setup (spec code)
type GetDataFn = (id: string) => Observable<Data>;
let getDataMock: MockedFunction<GetDataFn>;
TestBed.configureTestingModule({
  providers: [
    { provide: EXAMPLE_SERVICE, useValue: { getData: (getDataMock = vi.fn<GetDataFn>()) } },
  ],
});
```

## Store Instantiation Pattern

```ts
let store: StoreUnderTest;

beforeEach(() => {
  // configure TestBed with mocked tokens
  store = TestBed.inject(StoreUnderTestToken) as unknown as StoreUnderTest;
});
```

## Assertion Patterns

- Read state via getters: `store.someSlice()` and `store.someSelector()`.
- For async `rxMethod` flows, allow the microtask queue to flush:

```ts
const nextTick = () => new Promise<void>((r) => setTimeout(r, 0));
// ... invoke store method
await nextTick();
```

- Verify service calls and parameters on typed mocks.

## Behaviors to Test

Use this checklist to design comprehensive store tests.

1. Initialization

   - Creates default entry/key when missing.
   - Idempotent for duplicate calls.
   - Supports multiple contexts (e.g., multi-device/tenant/environment).

2. Actions / Transitions

   - Updates any user-facing selection/highlighting state immediately (if applicable).
   - Sets loading flags and clears previous errors appropriately.

3. Smart Caching

   - Cache hit: recognises already-loaded state and skips service calls.
   - Cache miss: calls service and updates state.
   - Cache invalidation: refresh or error transitions update timestamps and flags correctly.

4. Loading & Error Handling

   - Sets `isLoading` at start; clears on finish/error.
   - Sets clear error messages on failure; preserves relevant data when appropriate.

5. Refresh / Invalidation

   - Uses current context (e.g., active key/parameters) when reloading.
   - Updates data and refresh timestamps.
   - No-ops safely when context is missing.

6. Cleanup

   - Removes context-scoped entries (e.g., by entity and/or subtype).
   - Leaves unrelated entries intact.
   - Selection behavior: assert current implementation (clear/preserve) as designed.

7. Multi-Context Isolation

   - Independent state per context key (e.g., tenant+resourceType).
   - Independent caching per context key.
   - Selection switching and persistence rules validated.

8. Complex Workflows

   - Initialize context → Perform action → Cache hit (second attempt avoids service call).
   - Disconnect/Reconnect: Cleanup context → Re-initialize → Verify clean defaults.
   - Failure → Retry success: First attempt fails, subsequent attempt clears error and loads data.
   - Cache hit → Refresh: Subsequent refresh calls service and updates timestamp/data.
   - Concurrent operations: Overlapping actions resolve to a consistent final state.

9. Edge Cases & Concurrency

   - Empty / malformed inputs where applicable.
   - Rapid successive actions; ensure “latest wins” semantics.
   - Cleanup during in-flight operations does not throw and leaves consistent state.

10. Performance & Memory (lightweight)

- Sanity checks for large numbers of entries.
- Verify cleanup prevents bloat; no dangling async flows.

## Do / Don’t

- Do test through the store’s public API.
- Do mock at the service boundary using interface tokens.
- Do keep mocks minimal but strongly typed (no `any`).
- Don’t assert internal implementation details; assert observable state and effects.
- Don’t call other store methods from within a method implementation (one-function-per-file pattern).
- Don’t perform real HTTP in unit tests.

## Example Async Test Snippet

```ts
getDataMock.mockReturnValue(of(mockData));
store.loadData({ id: '123' });
await nextTick();
expect(store.data()).toEqual(mockData);
expect(store.isLoading()).toBe(false);
expect(store.error()).toBeNull();
```

## Quick Checklist

- [ ] Test setup via Angular TestBed
- [ ] Typed service mocks provided via tokens
- [ ] Initialization behavior
- [ ] Navigation/Actions + selection updates
- [ ] Caching hit/miss/invalidation
- [ ] Loading + error handling
- [ ] Refresh semantics
- [ ] Cleanup semantics
- [ ] Multi-context isolation
- [ ] Complex workflows
- [ ] Edge cases & concurrency
- [ ] Performance & memory sanity

## Example

For a concrete, end-to-end example of a properly tested NgRx Signal Store using the methodology in this document, see:

- [`storage-store.spec.ts`](../libs/domain/storage/state/src/lib/storage-store.spec.ts)
