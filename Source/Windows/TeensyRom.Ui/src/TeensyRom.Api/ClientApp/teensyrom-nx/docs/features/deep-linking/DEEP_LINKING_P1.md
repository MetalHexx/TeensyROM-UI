# Phase 1: Route Guard Infrastructure & Basic Navigation

## 🎯 Objective

Establish route guard infrastructure with query parameter parsing and basic directory navigation. This phase delivers immediate value by enabling URL-based directory navigation while proving the architectural pattern for subsequent phases. Testing focuses on integrated store behavior and context service coordination.

**Delivered Value**: Users can navigate directly to specific directories via URLs like `/player?device=X&storage=SD&path=/games`, with graceful fallback when devices aren't available. The system provides a solid foundation for file auto-launch (Phase 2) and bidirectional URL sync (Phase 3).

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [x] [Deep Linking Plan](./DEEP_LINKING_PLAN.md) - Complete feature overview and architecture
- [ ] [Overview Context](../../OVERVIEW_CONTEXT.md) - Clean Architecture layers and patterns

**Standards & Guidelines:**
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices
- [ ] [State Standards](../../STATE_STANDARDS.md) - NgRx Signal Store patterns (critical for this phase)
- [ ] [Store Testing Guide](../../STORE_TESTING.md) - Behavioral testing methodology (critical for this phase)

---

## 📂 File Structure Overview

```
libs/application/src/lib/storage/
├── storage-store.ts                         📝 Modified - Export type for resolver
├── actions/
│   ├── index.ts                             📝 Modified - Export initializeFromRoute
│   └── initialize-from-route.ts             ✨ New - Route-driven initialization
└── storage-store.routing.spec.ts            ✨ New - Behavioral tests for routing

libs/application/src/lib/player/
├── player-context.service.ts                📝 Modified - Add initializeFromRoute method
└── player-context-routing.service.spec.ts   ✨ New - Behavioral tests for route coordination

libs/app/navigation/src/lib/
└── player-route.resolver.ts                 ✨ New - Route parameter parsing and resolution

apps/teensyrom-ui/src/app/
└── app.routes.ts                            📝 Modified - Add resolver to player route

apps/teensyrom-ui-e2e/src/e2e/storage/
└── test-helpers.ts                          📝 Modified - Extend loadFileInPlayer helper
```

---

## 📋 Implementation Guidelines

> **Testing Policy:**
> - **Behavioral testing** - test observable outcomes through public APIs
> - Tests embedded **within each task** as work progresses
> - See [Store Testing](../../STORE_TESTING.md) for integrated store testing patterns
> - See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing guidance

> **Progress Tracking:**
> - **Mark checkboxes ✅ as you complete each subtask**
> - Update throughout implementation, not just at the end

---

<details open>
<summary><h3>Task 1: Create Storage Store Route Action</h3></summary>

**Purpose**: Add `initializeFromRoute()` action that combines storage initialization with directory navigation in a single atomic operation. This action handles device/storage validation, fallback logic, and provides resolved parameters for the route guard.

**Related Documentation:**
- [Deep Linking Plan - Storage Store Integration](./DEEP_LINKING_PLAN.md#integration-points) - Integration pattern
- [State Standards - Action Pattern](../../STATE_STANDARDS.md#function-organization) - Action structure
- [initialize-storage.ts](../../../libs/application/src/lib/storage/actions/initialize-storage.ts) - Reference implementation

**Implementation Subtasks:**
- [ ] **Create Action File**: Create `libs/application/src/lib/storage/actions/initialize-from-route.ts`
- [ ] **Define Parameters Interface**: Create `RouteInitParams` interface with `deviceId?`, `storageType?`, `path?` properties
- [ ] **Implement Device Validation**: Inject `IDeviceService`, get connected devices, validate/fallback device selection
- [ ] **Implement Storage Validation**: Parse `storageType` (uppercase `SD`/`USB`), validate availability, fallback to first available
- [ ] **Implement Path Handling**: Default missing path to `/`, normalize path format
- [ ] **Storage Initialization**: Call storage creation/update helpers (reuse from `initialize-storage.ts`)
- [ ] **Directory Navigation**: Navigate to resolved path using existing `navigateToDirectory` logic
- [ ] **Return Resolved Parameters**: Return object with resolved `deviceId`, `storageType`, `path` for route guard use
- [ ] **Export Action**: Add to `actions/index.ts` barrel export in `withStorageActions()`

**Key Implementation Notes:**
- Use `createAction()` for Redux DevTools tracking with message `'initialize-from-route'`
- Pass `actionMessage` to all helper functions for state mutation correlation
- Device fallback: Select first connected device, log warning about fallback
- Storage fallback: Prefer SD over USB when not specified (existing pattern)
- Path fallback: Silent fallback to `/` when invalid or missing (no notification per decision)
- Reuse existing helpers: `createStorage`, `setLoadingStorage`, `setStorageLoaded`, `setStorageError`

**Critical Type Definition:**
```typescript
interface RouteInitParams {
  deviceId?: string;
  storageType?: 'SD' | 'USB'; // Uppercase per plan
  path?: string;
}
```

**Testing Focus for Task 1:**

> **Test Pattern**: Direct integrated store testing (no facade exists for storage store)
> See [storage-store.favorites.spec.ts](../../../libs/application/src/lib/storage/storage-store.favorites.spec.ts) for reference pattern

**Behaviors to Test:**
- [ ] **Initialize with all parameters**: Initializes storage and navigates to specified directory
- [ ] **Default to first device**: Missing `deviceId` uses first connected device
- [ ] **Default to first storage**: Missing `storageType` uses first available (SD over USB)
- [ ] **Default to root path**: Missing `path` defaults to `/`
- [ ] **Invalid device fallback**: Non-existent device falls back to first connected
- [ ] **Invalid storage fallback**: Unavailable storage falls back to first available
- [ ] **Invalid path fallback**: Non-existent path silently falls back to root `/`
- [ ] **Returns resolved params**: Action returns object with resolved `deviceId`, `storageType`, `path`
- [ ] **State updates correctly**: Storage entry created/updated with loading → loaded states
- [ ] **Uppercase storage type**: Accepts uppercase `SD`/`USB` strings (not enum)

**Testing Reference:**
- See [Store Testing](../../STORE_TESTING.md) - Integrated store testing methodology
- Use real `StorageStore` instance with mocked `IStorageService` and `IDeviceService`
- Assert on observable state through store signals

</details>

---

<details open>
<summary><h3>Task 2: Write Storage Store Routing Behavioral Tests</h3></summary>

**Purpose**: Create comprehensive test suite validating route-driven storage initialization through integrated store with mocked infrastructure. Tests prove the action works correctly before wiring to route guard.

**Related Documentation:**
- [Store Testing](../../STORE_TESTING.md) - Testing methodology for integrated stores
- [storage-store.favorites.spec.ts](../../../libs/application/src/lib/storage/storage-store.favorites.spec.ts) - Reference test pattern

**Implementation Subtasks:**
- [ ] **Create Test File**: Create `libs/application/src/lib/storage/storage-store.routing.spec.ts`
- [ ] **Setup Test Infrastructure**: Configure TestBed with real `StorageStore`, mocked `IStorageService`, mocked `IDeviceService`
- [ ] **Create Mock Factories**: Helper functions for mock devices, mock directories, mock storage responses
- [ ] **Test All Parameter Combinations**: Complete, partial, missing parameters
- [ ] **Test Fallback Scenarios**: Invalid device, invalid storage, invalid path
- [ ] **Test State Transitions**: Loading → loaded → error states
- [ ] **Test Return Values**: Verify resolved parameters returned correctly
- [ ] **Test Storage Key Generation**: Verify `StorageKeyUtil.create()` called correctly

**Key Implementation Notes:**
- Follow `storage-store.favorites.spec.ts` pattern exactly - real store + mocked infrastructure
- Use `TestBed.inject(StorageStore)` to get store instance
- Mock `IDeviceService.getConnectedDevices()` to return test devices
- Mock `IStorageService.getDirectory()` to return test directories
- Use Vitest `vi.fn()` for strongly typed mocks
- Assert on `store.storageEntries()` signal for state validation
- Use `async/await` for all action calls

**Testing Focus for Task 2:**

**Test Suite Structure:**
```typescript
describe('StorageStore - Route Initialization', () => {
  describe('initializeFromRoute() - Complete Parameters', () => {
    // Tests with all params provided
  });
  
  describe('initializeFromRoute() - Missing Parameters & Defaults', () => {
    // Tests with missing deviceId, storageType, path
  });
  
  describe('initializeFromRoute() - Invalid Parameters & Fallback', () => {
    // Tests with invalid deviceId, storageType, path
  });
  
  describe('initializeFromRoute() - State Management', () => {
    // Tests for loading/loaded/error states
  });
});
```

**Behaviors to Test (Complete Checklist):**
- [ ] **All params provided**: Creates entry, navigates, returns resolved params
- [ ] **Missing deviceId**: Uses first connected device
- [ ] **Missing storageType**: Uses first available (SD over USB)
- [ ] **Missing path**: Defaults to root `/`
- [ ] **All params missing**: Uses all defaults, still succeeds
- [ ] **Invalid deviceId**: Falls back to first device
- [ ] **Invalid storageType**: Falls back to first available
- [ ] **Invalid path**: Falls back to root `/` (no error)
- [ ] **Loading state**: Sets `isLoading: true` before API call
- [ ] **Loaded state**: Sets `isLoaded: true` after success
- [ ] **Error state**: Sets error message on API failure
- [ ] **Storage key**: Generates correct key `${deviceId}-${storageType}`
- [ ] **Return value**: Returns object with resolved params

**Testing Reference:**
- Must use `async/await` for action calls
- Assert on signals: `store.storageEntries()[key]`
- Mock returns: `mockService.getDirectory.mockReturnValue(of(mockDirectory))`

</details>

---

<details open>
<summary><h3>Task 3: Create Route Guard Resolver Scaffold</h3></summary>

**Purpose**: Build Angular ResolveFn that parses query parameters and provides infrastructure for route-driven initialization. This establishes the routing layer → application layer coordination pattern.

**Related Documentation:**
- [Deep Linking Plan - Route Guard Pattern](./DEEP_LINKING_PLAN.md#key-design-decisions) - Resolver pattern choice
- [Angular ResolveFn Docs](https://angular.io/api/router/ResolveFn) - Angular routing resolver pattern

**Implementation Subtasks:**
- [ ] **Create Resolver File**: Create `libs/app/navigation/src/lib/player-route.resolver.ts`
- [ ] **Define Resolver Function**: Export `playerRouteResolver: ResolveFn<ResolvedRouteData>`
- [ ] **Define Return Type**: Create `ResolvedRouteData` interface with `deviceId`, `storageType`, `path` properties
- [ ] **Parse Query Parameters**: Extract `device`, `storage`, `path` from `ActivatedRouteSnapshot.queryParamMap`
- [ ] **Inject PlayerContextService**: Use `inject()` to get context service instance
- [ ] **Call Context Service**: Call `initializeFromRoute()` method (to be created in Task 4)
- [ ] **Return Resolved Data**: Return data for component to receive via `ActivatedRoute.snapshot.data`
- [ ] **Handle Errors**: Wrap in try/catch, log errors, return safe defaults

**Key Implementation Notes:**
- Use functional `ResolveFn` pattern (modern Angular 19 style)
- Query params: `device`, `storage`, `path` (lowercase in URL)
- Storage type: Convert to uppercase (`SD`, `USB`) before passing to context service
- Return data structure matches `ResolvedRouteData` interface
- Resolver located in `libs/app/navigation` per Clean Architecture guidance

**Critical Type Definitions:**
```typescript
interface ResolvedRouteData {
  deviceId: string;
  storageType: StorageType;
  path: string;
}

export const playerRouteResolver: ResolveFn<ResolvedRouteData> = async (route) => {
  // Implementation
};
```

**Testing Focus for Task 3:**

> **Note**: This task focuses on structure/scaffolding. Full behavioral testing happens in Task 5 when wired to context service.

**Initial Validation:**
- [ ] **Resolver compiles**: TypeScript compilation succeeds
- [ ] **Resolver exports**: Function exported correctly from module
- [ ] **Parameter parsing**: Query params extracted correctly (manual verification)

**Testing Reference:**
- Full resolver testing in Task 5 after context service integration
- Use manual verification in browser during development

</details>

---

<details open>
<summary><h3>Task 4: Implement PlayerContextService Route Coordination</h3></summary>

**Purpose**: Add `initializeFromRoute()` method to PlayerContextService that coordinates storage store route action with player initialization. This creates the application layer orchestration point for route-driven workflows.

**Related Documentation:**
- [Deep Linking Plan - PlayerContextService Coordination](./DEEP_LINKING_PLAN.md#integration-points) - Coordination pattern
- [player-context.service.ts](../../../libs/application/src/lib/player/player-context.service.ts) - Existing service patterns

**Implementation Subtasks:**
- [ ] **Add Method Signature**: Add `initializeFromRoute(params: RouteInitParams): Promise<ResolvedRouteData>`
- [ ] **Inject StorageStore**: Already injected as `storageStore`
- [ ] **Call Storage Action**: Call `storageStore.initializeFromRoute(params)`
- [ ] **Handle Resolved Data**: Receive resolved params from storage action
- [ ] **Initialize Player State**: Call `store.initializePlayer({ deviceId: resolvedParams.deviceId })`
- [ ] **Return Resolved Data**: Return resolved params to resolver
- [ ] **Error Handling**: Wrap in try/catch, log errors, throw for resolver to catch

**Key Implementation Notes:**
- Method is `async` returning `Promise<ResolvedRouteData>`
- Coordinates two stores: `StorageStore` (via action) and `PlayerStore` (via internal store)
- Player initialization ensures device-specific state exists
- No UI notifications needed (handled by storage action)
- Method name matches storage action: `initializeFromRoute`

**Testing Focus for Task 4:**

> **Test Pattern**: Facade testing with real stores + mocked infrastructure
> See [player-context-favorite.service.spec.ts](../../../libs/application/src/lib/player/player-context-favorite.service.spec.ts) for reference pattern

**Behaviors to Test:**
- [ ] **Coordinates storage initialization**: Calls storage store action with params
- [ ] **Initializes player state**: Calls `store.initializePlayer()` with resolved deviceId
- [ ] **Returns resolved data**: Returns object with resolved params
- [ ] **Handles all parameter combinations**: Complete, partial, missing params
- [ ] **Handles invalid parameters**: Invalid device/storage/path fall back correctly
- [ ] **Maintains device isolation**: Multiple devices have independent state
- [ ] **Error propagation**: Storage errors propagate to resolver

**Testing Reference:**
- See [Store Testing](../../STORE_TESTING.md) - Facade testing methodology
- Use real `PlayerContextService`, real stores, mocked infrastructure services
- Mock `IStorageService`, `IDeviceService`, `IPlayerService` via injection tokens

</details>

---

<details open>
<summary><h3>Task 5: Write PlayerContext Routing Behavioral Tests</h3></summary>

**Purpose**: Create comprehensive test suite validating complete route-driven workflows through PlayerContextService facade. Tests prove the coordination between storage and player stores works correctly.

**Related Documentation:**
- [Store Testing](../../STORE_TESTING.md) - Facade testing methodology
- [player-context-favorite.service.spec.ts](../../../libs/application/src/lib/player/player-context-favorite.service.spec.ts) - Reference test pattern

**Implementation Subtasks:**
- [ ] **Create Test File**: Create `libs/application/src/lib/player/player-context-routing.service.spec.ts`
- [ ] **Setup Test Infrastructure**: Configure TestBed with real facade, real stores, mocked infrastructure
- [ ] **Create Mock Factories**: Helper functions for mock devices, directories, services
- [ ] **Test Route Coordination**: Verify storage + player initialization coordination
- [ ] **Test Parameter Resolution**: Verify all parameter combinations resolve correctly
- [ ] **Test Fallback Scenarios**: Verify invalid params fall back gracefully
- [ ] **Test Multi-Device**: Verify device isolation and multi-device scenarios
- [ ] **Test Error Handling**: Verify errors propagate correctly to resolver

**Key Implementation Notes:**
- Follow `player-context-favorite.service.spec.ts` pattern exactly
- Provide real `PlayerContextService`, real `PlayerStore`, real `StorageStore`
- Mock infrastructure: `IStorageService`, `IDeviceService`, `IPlayerService`
- Use `TestBed.inject()` for all services
- Assert through facade signals: `service.getCurrentFile()`, `service.getStatus()`
- Use `async/await` for all method calls

**Testing Focus for Task 5:**

**Test Suite Structure:**
```typescript
describe('PlayerContextService - Route Initialization', () => {
  describe('initializeFromRoute() - Complete Workflows', () => {
    // Tests for complete parameter sets
  });
  
  describe('initializeFromRoute() - Parameter Resolution', () => {
    // Tests for missing/defaulted params
  });
  
  describe('initializeFromRoute() - Fallback Scenarios', () => {
    // Tests for invalid params
  });
  
  describe('initializeFromRoute() - Multi-Device Coordination', () => {
    // Tests for device isolation
  });
});
```

**Behaviors to Test (Complete Checklist):**
- [ ] **Complete params workflow**: Initializes storage, player, navigates to directory
- [ ] **Returns resolved data**: Method returns object with resolved params
- [ ] **Missing deviceId workflow**: Uses first device, initializes correctly
- [ ] **Missing storageType workflow**: Uses first storage, initializes correctly
- [ ] **Missing path workflow**: Defaults to root, initializes correctly
- [ ] **Invalid device workflow**: Falls back to first device
- [ ] **Invalid storage workflow**: Falls back to first storage
- [ ] **Invalid path workflow**: Falls back to root silently
- [ ] **Player state initialized**: Player state exists for resolved deviceId
- [ ] **Storage state initialized**: Storage entry exists with correct key
- [ ] **Multi-device isolation**: Independent initialization for different devices
- [ ] **Error propagation**: Storage errors throw and propagate to caller
- [ ] **Coordination order**: Storage initializes before player initialization

**Testing Reference:**
- Assert through facade: `service.isLoading(deviceId)()`
- Mock infrastructure boundaries only
- Test complete workflows, not individual steps

</details>

---

<details open>
<summary><h3>Task 6: Wire Route Resolver to Routes Configuration</h3></summary>

**Purpose**: Integrate the route resolver into Angular routing configuration so it executes before the player component loads. This completes the routing infrastructure.

**Related Documentation:**
- [app.routes.ts](../../../apps/teensyrom-ui/src/app/app.routes.ts) - Existing route configuration
- [Angular Resolver Integration](https://angular.io/guide/router#resolve-pre-fetching-component-data) - Angular docs

**Implementation Subtasks:**
- [ ] **Import Resolver**: Add import for `playerRouteResolver` from `@teensyrom-nx/app/navigation`
- [ ] **Add Resolve Property**: Add `resolve: { routeData: playerRouteResolver }` to player route
- [ ] **Verify Route Configuration**: Ensure resolver runs before component loads
- [ ] **Test in Browser**: Manually verify resolver executes on player route navigation

**Key Implementation Notes:**
- Resolver runs before `PlayerViewComponent` instantiates
- Component receives data via `ActivatedRoute.snapshot.data['routeData']`
- Resolver blocks navigation until Promise resolves
- Error in resolver prevents route activation

**Route Configuration:**
```typescript
{
  path: 'player',
  data: { title: 'Player' },
  resolve: { routeData: playerRouteResolver },
  loadComponent: () => import('@teensyrom-nx/features/player').then(m => m.PlayerViewComponent)
}
```

**Testing Focus for Task 6:**

> **Manual Verification**: Use browser to test resolver integration

**Manual Test Cases:**
- [ ] **Navigate with params**: URL `/player?device=X&storage=SD&path=/games` loads correctly
- [ ] **Navigate without params**: URL `/player` loads with defaults
- [ ] **Invalid params**: URL with invalid device/storage falls back gracefully
- [ ] **Browser DevTools**: Check Network tab for API calls before component renders
- [ ] **Redux DevTools**: Verify `initialize-from-route` action appears

**Testing Reference:**
- Full E2E tests in Task 8 validate complete routing workflows
- Use manual testing to verify basic integration

</details>

---

<details open>
<summary><h3>Task 7: Update E2E Test Helpers for URL Parameters</h3></summary>

**Purpose**: Extend existing E2E test helper `loadFileInPlayer()` to support new URL parameter structure while maintaining backward compatibility. Enables E2E tests to use parameterized URLs.

**Related Documentation:**
- [test-helpers.ts](../../../apps/teensyrom-ui-e2e/src/e2e/storage/test-helpers.ts) - Existing helpers
- [E2E Tests](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - E2E testing patterns

**Implementation Subtasks:**
- [ ] **Add Function Overload**: Add overload signature accepting `{ device?, storage?, path?, file? }` object
- [ ] **Maintain Original Signature**: Keep existing `loadFileInPlayer(filePath)` signature for backward compatibility
- [ ] **Implement Parameter Building**: Build query string from object parameters
- [ ] **URL Construction**: Create `/player?device=X&storage=Y&path=Z&file=W` URLs
- [ ] **Session Storage Handling**: Maintain existing session storage clearing logic
- [ ] **Add Helper Function**: Create `buildPlayerUrl(params)` helper for URL construction
- [ ] **Export New Helper**: Export for use in other E2E test files

**Key Implementation Notes:**
- TypeScript function overloading for two signatures
- URLSearchParams for query string building
- Encode all parameters with `encodeURIComponent()`
- Preserve existing behavior for old signature
- New signature is optional parameters object

**Function Signatures:**
```typescript
// Existing signature (keep for backward compatibility)
export function loadFileInPlayer(filePath: string): Cypress.Chainable<Cypress.AUTWindow>;

// New signature (object parameter)
export function loadFileInPlayer(params: {
  device?: string;
  storage?: 'SD' | 'USB';
  path?: string;
  file?: string;
}): Cypress.Chainable<Cypress.AUTWindow>;
```

**Testing Focus for Task 7:**

> **Manual Verification**: Use in E2E tests to validate helper works

**Validation Steps:**
- [ ] **Old signature works**: Existing tests still pass with `loadFileInPlayer('/path/to/file')`
- [ ] **New signature works**: Can call with object `loadFileInPlayer({ storage: 'SD', path: '/games' })`
- [ ] **URL construction**: Generated URLs have correct format and encoding
- [ ] **Parameter omission**: Missing params don't break URL construction

**Testing Reference:**
- No dedicated tests for helpers (tested via E2E tests that use them)
- Verify backward compatibility by running existing E2E tests

</details>

---

<details open>
<summary><h3>Task 8: Manual Verification & Browser Testing</h3></summary>

**Purpose**: Perform comprehensive manual testing of route-driven navigation in running application. Validates complete user workflows and catches integration issues not covered by unit tests.

**Related Documentation:**
- [Deep Linking Plan - User Scenarios](./DEEP_LINKING_PLAN.md#user-scenarios) - Complete scenario list

**Implementation Subtasks:**
- [ ] **Start Development Server**: Run `pnpm start` to launch app
- [ ] **Test Complete Parameters**: Navigate to `/player?device=X&storage=SD&path=/games`
- [ ] **Test Partial Parameters**: Navigate to `/player?path=/music` (device/storage defaults)
- [ ] **Test No Parameters**: Navigate to `/player` (all defaults)
- [ ] **Test Invalid Device**: Navigate with non-existent device parameter
- [ ] **Test Invalid Storage**: Navigate with unavailable storage type
- [ ] **Test Invalid Path**: Navigate with non-existent directory path
- [ ] **Test Multiple Devices**: Navigate with different devices when multiple connected
- [ ] **Verify Browser DevTools**: Check Network tab, Console tab, Redux DevTools

**Key Implementation Notes:**
- Use multiple browser tabs to test concurrent navigation
- Check Redux DevTools for `initialize-from-route` actions
- Verify loading states appear/disappear correctly
- Verify error states don't occur for fallback scenarios
- Test with real TeensyROM device if available, or use mock data

**Manual Test Scenarios:**

**Scenario 1: Complete Parameters**
- [ ] Navigate: `/player?device=teensy-01&storage=SD&path=/games`
- [ ] Verify: Device teensy-01 selected
- [ ] Verify: SD storage selected
- [ ] Verify: Directory tree shows `/games` path
- [ ] Verify: File list shows `/games` contents

**Scenario 2: Minimal Parameters**
- [ ] Navigate: `/player?path=/music`
- [ ] Verify: First device automatically selected
- [ ] Verify: First storage automatically selected (SD if available)
- [ ] Verify: Directory tree shows `/music` path

**Scenario 3: Invalid Device Fallback**
- [ ] Navigate: `/player?device=nonexistent&path=/games`
- [ ] Verify: First device used instead
- [ ] Verify: Console warning about fallback
- [ ] Verify: `/games` still loads correctly

**Scenario 4: Invalid Path Fallback**
- [ ] Navigate: `/player?storage=SD&path=/invalid/path`
- [ ] Verify: Root directory `/` loads instead
- [ ] Verify: No error notification (silent fallback)
- [ ] Verify: Directory tree shows root

**Scenario 5: No Parameters (Defaults)**
- [ ] Navigate: `/player`
- [ ] Verify: First device selected
- [ ] Verify: First storage selected
- [ ] Verify: Root directory `/` loads

**Browser DevTools Checks:**
- [ ] **Network Tab**: API calls happen before component renders
- [ ] **Console Tab**: No errors, only expected warnings for fallbacks
- [ ] **Redux DevTools**: Action `initialize-from-route` appears with correct parameters
- [ ] **Redux DevTools**: State updates show correct loading → loaded transitions

**Testing Reference:**
- Manual testing catches UI/UX issues unit tests miss
- Use real devices when possible, mock data when not
- Document any unexpected behaviors for follow-up

</details>

---

## 🗂️ Files Modified or Created

**New Files:**
- `libs/application/src/lib/storage/actions/initialize-from-route.ts`
- `libs/application/src/lib/storage/storage-store.routing.spec.ts`
- `libs/application/src/lib/player/player-context-routing.service.spec.ts`
- `libs/app/navigation/src/lib/player-route.resolver.ts`

**Modified Files:**
- `libs/application/src/lib/storage/actions/index.ts`
- `libs/application/src/lib/storage/storage-store.ts`
- `libs/application/src/lib/player/player-context.service.ts`
- `apps/teensyrom-ui/src/app/app.routes.ts`
- `apps/teensyrom-ui-e2e/src/e2e/storage/test-helpers.ts`

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
> - **Behavioral testing** - test observable outcomes through public APIs
> - **Test as you go** - tests integrated into each task, not deferred
> - **Test through facades** - PlayerContext tests use facade with real stores
> - **Test integrated stores** - Storage tests use real store with mocked infrastructure
> - **Mock at boundaries** - mock infrastructure services only (IStorageService, IDeviceService)

### Where Tests Are Written

**Tests are embedded in task subtasks above:**
- **Task 1**: Testing subtask for storage route action (within task)
- **Task 2**: Complete test suite creation (dedicated task)
- **Task 4**: Testing subtask for context service method (within task)
- **Task 5**: Complete test suite creation (dedicated task)
- **Task 6**: Manual verification checklist (within task)
- **Task 7**: Helper validation checklist (within task)
- **Task 8**: Comprehensive manual testing scenarios (dedicated task)

### Test Execution Commands

**Running Unit Tests:**
```bash
# Run storage store tests
pnpm nx test application --testFile=storage-store.routing.spec.ts

# Run player context tests
pnpm nx test application --testFile=player-context-routing.service.spec.ts

# Run all application layer tests
pnpm nx test application --watch=false

# Run tests in watch mode during development
pnpm nx test application --watch
```

**Running E2E Tests:**
```bash
# Open Cypress interactive runner
pnpm nx run teensyrom-ui-e2e:open-cypress

# Run E2E tests headlessly
pnpm nx run teensyrom-ui-e2e:e2e
```

**Manual Testing:**
```bash
# Start dev server for manual testing
pnpm start

# Navigate to URLs in browser:
# http://localhost:4200/player?device=X&storage=SD&path=/games
# http://localhost:4200/player?path=/music
# http://localhost:4200/player
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)
- [ ] State management follows [State Standards](../../STATE_STANDARDS.md)
- [ ] Route resolver executes before player component loads
- [ ] Query parameter parsing handles all combinations correctly

**Testing Requirements:**
- [ ] Storage store routing tests pass: `pnpm nx test application --testFile=storage-store.routing.spec.ts`
- [ ] Player context routing tests pass: `pnpm nx test application --testFile=player-context-routing.service.spec.ts`
- [ ] All behavioral test checkboxes verified in each task
- [ ] Manual testing checklist (Task 8) completed
- [ ] No test failures in application layer: `pnpm nx test application --watch=false`

**Quality Checks:**
- [ ] No TypeScript errors: `pnpm nx run-many --target=type-check --all`
- [ ] Linting passes: `pnpm nx lint application`
- [ ] Linting passes: `pnpm nx lint app-navigation`
- [ ] No console errors in browser during manual testing
- [ ] Redux DevTools shows correct action tracking

**Route Navigation Requirements:**
- [ ] `/player?device=X&storage=SD&path=/games` navigates to specific directory
- [ ] `/player?path=/music` navigates with device/storage defaults
- [ ] `/player` navigates with all defaults (first device, first storage, root path)
- [ ] Invalid device falls back to first device (silent, no crash)
- [ ] Invalid storage falls back to first available (silent, no crash)
- [ ] Invalid path falls back to root `/` (silent, no notification)

**Documentation:**
- [ ] Inline code comments added for complex route logic
- [ ] JSDoc added to `playerRouteResolver` function
- [ ] JSDoc added to `PlayerContextService.initializeFromRoute()` method

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Phase 2 can build on this foundation (file auto-launch)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

**Decision 1: ResolveFn over CanActivateFn**
- **Rationale**: Resolvers provide pre-loaded data to components, ensuring storage state is ready before rendering. This avoids race conditions between component initialization and route-driven state loading.
- **Trade-offs**: Resolvers block navigation until complete, but this is acceptable for the initialization workload.

**Decision 2: Silent Path Fallback**
- **Rationale**: Invalid paths are often from old bookmarks or typos. Silent fallback to root reduces alert fatigue while still providing a functional state.
- **Trade-offs**: Users don't get explicit feedback about invalid paths, but the device/storage notifications handle more critical failures.

**Decision 3: Storage Store Direct Testing**
- **Rationale**: No facade exists for storage store, so tests interact directly with integrated store. This follows the established pattern from `storage-store.favorites.spec.ts`.
- **Trade-offs**: Tests are slightly more coupled to store structure, but still behavioral (test outcomes, not implementation).

**Decision 4: PlayerContextService Coordination**
- **Rationale**: Context service coordinates between storage and player stores, maintaining Clean Architecture boundaries. Route resolver stays in navigation layer, business logic in application layer.
- **Trade-offs**: Extra layer of indirection, but maintains proper separation of concerns.

### Implementation Constraints

**Constraint 1: Storage Type Casing**
- URL parameters use uppercase strings `'SD'` and `'USB'` (not StorageType enum values)
- Must convert/validate before passing to domain models
- Backend API expects uppercase storage type strings

**Constraint 2: Device Availability**
- Route parameters may reference disconnected devices
- Must handle graceful fallback without breaking user experience
- First device fallback ensures URL always works across configurations

**Constraint 3: Browser Navigation Timing**
- Resolver must complete before component loads
- API calls in resolver block navigation (acceptable for small payloads)
- Loading states may not be visible if resolution is fast

### Future Enhancements (Phase 2+)

**Enhancement 1: File Auto-Launch**
- Phase 2 will extend resolver to parse `file` parameter
- Will coordinate directory loading + file launching atomically
- Builds directly on Phase 1 infrastructure

**Enhancement 2: Bidirectional URL Sync**
- Phase 3 will add URL updates when user navigates in UI
- Will enable browser back/forward button support
- Debounced to prevent history spam

**Enhancement 3: Search Integration**
- Phase 4 will add `search` and `filter` parameters
- Will coordinate with search state in storage store
- Reuses same resolver infrastructure

### External References

- [Angular Routing Docs](https://angular.io/guide/router) - Official Angular routing guide
- [ResolveFn API](https://angular.io/api/router/ResolveFn) - Functional resolver pattern
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Uncle Bob's original article

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

**Example Discovery Template:**
- **Discovery**: [What was learned]
- **Impact**: [How it affects implementation]
- **Resolution**: [How it was addressed]

</details>

---

## 💡 Agent Implementation Guide

> **Instructions for Clean Coder agent implementing this document**

### Before Starting Implementation

1. **Read All Required Documentation** (checklist at top of document)
2. **Review Task Dependencies**:
   - Task 1 (storage action) must complete before Task 2 (storage tests)
   - Task 4 (context method) must complete before Task 5 (context tests)
   - Task 3 (resolver scaffold) must complete before Task 6 (wire resolver)
   - All tasks must complete before Task 8 (manual verification)

3. **Ask Clarifying Questions** (if needed):
   - Any unclear requirements in tasks?
   - Any missing context for implementation?
   - Any architectural concerns?

### During Implementation

**Work Sequentially Through Tasks:**
1. Complete Task 1 (storage action) + test it in Task 2
2. Complete Task 3 (resolver scaffold)
3. Complete Task 4 (context method) + test it in Task 5
4. Complete Task 6 (wire resolver)
5. Complete Task 7 (E2E helpers)
6. Complete Task 8 (manual verification)

**For Each Task:**
1. ✅ **Check Prerequisites**: Verify previous tasks completed
2. 📖 **Review Reference Docs**: Read linked documentation sections
3. 🔨 **Implement Subtasks**: Check off each subtask as completed
4. 🧪 **Write Tests**: Complete testing subtask (behavioral focus)
5. ✅ **Verify Task**: All subtasks checked, tests passing
6. 📝 **Document Discoveries**: Add any learnings to Notes section

**Testing Integration:**
1. **Baseline First**: Run existing tests before changes to understand pre-existing issues
2. **Test As You Go**: Write tests for each task before moving to next
3. **Behavioral Focus**: Test observable outcomes, not implementation details
4. **Run Frequently**: Execute tests after each change to catch issues early

### Code Quality Standards

**Follow These Patterns:**
- Use `async/await` for all async operations
- Use `createAction()` for Redux DevTools tracking
- Pass `actionMessage` to all helper functions
- Use `updateState()` with actionMessage (not `patchState()`)
- Follow naming conventions: kebab-case files, PascalCase classes
- Add JSDoc comments to public methods
- Use TypeScript strict mode (no `any` types)

**Testing Patterns:**
- Storage tests: Real store + mocked infrastructure
- Context tests: Real facade + real stores + mocked infrastructure
- Assert on signals: `store.property()` or `service.getProperty()()`
- Use `vi.fn()` for mocked functions
- Use `async/await` for all async test operations

### After Completing Each Task

1. ✅ **Verify all subtasks checked off**
2. 🧪 **Confirm all tests passing**
3. 📝 **Update discoveries section if needed**
4. 🔍 **Review code quality**
5. ➡️ **Proceed to next task**

### After Completing All Tasks

1. ✅ **Verify all success criteria**
2. 🧪 **Run full test suite**: `pnpm nx test application --watch=false`
3. 🔍 **Run linting**: `pnpm nx lint application && pnpm nx lint app-navigation`
4. 🌐 **Manual verification**: Complete Task 8 checklist
5. 📋 **Review phase completion**: All checkboxes marked

### Remember

- **Quality over speed** - take time to do it right
- **Test as you go** - don't defer testing to the end
- **Ask questions early** - clarify before implementing
- **Document learnings** - update discoveries section
- **Mark progress incrementally** - check boxes as you complete items

---

**Ready to begin implementation? Start with Task 1: Create Storage Store Route Action.**
