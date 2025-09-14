# Phase 2 Testing Specification: NgRx Signal Store Testing

**Related Documentation**: [Phase 2 Implementation](./BASIC_STORAGE_NAV_SPEC_P2.md)

**Standards Documentation**:

- **NgRx Official Testing Guide**: [SignalStore Testing](https://ngrx.io/guide/signals/signal-store/testing#testing-the-signalstore)
- **Testing Standards**: [`TESTING_STANDARDS.md`](../../../TESTING_STANDARDS.md)

## 🎯 **Objectives**

This specification defines comprehensive testing for the `StorageStore` NgRx Signal Store, leveraging our successful migration away from Angular dependencies in the API client. **Note: We eliminated Angular dependencies from the API client, not from NgRx testing itself. Angular dependencies are acceptable and normal for NgRx Signal Store testing - we'll use proper Angular TestBed and NgRx testing patterns.**

### **Key Testing Goals**

1. **Store Interface Testing**: Test all store behaviors through the store interface (not individual method files)
2. **Service-Level Mocking**: Use mocks for `StorageService` dependency while using proper NgRx testing infrastructure
3. **Dual-State Verification**: Test both local navigation state and global selection state
4. **Smart Caching Logic**: Verify cache hits, cache misses, and invalidation scenarios
5. **Multi-Device Coordination**: Test state isolation and cross-device selection management
6. **NgRx Best Practices**: Follow official NgRx testing patterns and create reusable documentation

## 📚 **Pre-Implementation Research**

### **Required Reading**

- [ ] **NgRx Official Guide**: [SignalStore Testing Patterns](https://ngrx.io/guide/signals/signal-store/testing#testing-the-signalstore)
- [ ] **Store Architecture Review**: Understand current `StorageStore` implementation and method behaviors
- [ ] **Service Dependencies**: Review `StorageService` interface for proper mocking strategy

## 📋 **Implementation Tasks**

### **Task 1: Create Mock Infrastructure** ✅

**File**: `libs/domain/storage/state/src/lib/storage-store.spec.ts`

**Mock Strategy (Using Real Methods)**:

```typescript
import { vi } from 'vitest';
import { of, throwError } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { StorageService } from '@teensyrom-nx/domain/storage/services';
import { StorageStore } from './storage-store';

// Mock only the StorageService - use real NgRx infrastructure
const mockStorageService = {
  getDirectory: vi.fn(),
} as StorageService;

// Proper NgRx Signal Store testing setup
const createTestStore = () => {
  TestBed.configureTestingModule({
    providers: [{ provide: StorageService, useValue: mockStorageService }],
  });

  return TestBed.inject(StorageStore);
};
```

**Test Data Factories**:

- [ ] `createMockStorageDirectory()` - Factory for StorageDirectory test data
- [ ] `createMockStorageDirectoryState()` - Factory for StorageDirectoryState test data
- [ ] `createMockSelectedDirectory()` - Factory for SelectedDirectory test data
- [ ] Support different scenarios: empty directories, error states, large datasets

### **Task 2: Test Store Initialization & Configuration** ✅

**Test Group**: Store Setup

- [ ] **Initial State**: `storageEntries: {}`, `selectedDirectory: null`
- [ ] **Root Provider**: Store uses `providedIn: 'root'` correctly
- [ ] **DevTools Integration**: Store configured with correct devtools name
- [ ] **Method Availability**: All expected methods available on store instance
- [ ] **Dependency Injection**: StorageService dependency properly injected

### **Task 3: Test `initializeStorage()` Method** ✅

**Test Group**: Storage Initialization

**Success Scenarios**:

- [ ] **New Entry Creation**: Creates initial storage entry when key doesn't exist
- [ ] **Default State**: Sets correct initial values (`currentPath: '/'`, `isLoaded: false`, `directory: null`)
- [ ] **Key Generation**: Uses `StorageKeyUtil.create(deviceId, storageType)` correctly
- [ ] **State Preservation**: Other storage entries remain unchanged

**Edge Cases**:

- [ ] **Existing Entry**: Does not overwrite existing storage entry
- [ ] **Multiple Devices**: Can initialize multiple device-storage combinations
- [ ] **Duplicate Calls**: Handles multiple initialization calls for same device-storage

### **Task 4: Test `navigateToDirectory()` Method - Smart Caching** ✅

**Test Group**: Navigation & Caching Logic

**4.1 Global Selection Updates (Always Happens)**

- [ ] **Selection Update**: Always updates `selectedDirectory` with new device/storage/path
- [ ] **Previous Selection Clear**: Clears previous selection when navigating to different directory
- [ ] **Cross-Device Selection**: Selection works across different devices
- [ ] **Cached Data Selection**: Updates selection even when using cached data (no API call)

**4.2 Smart Caching Logic**

- [ ] **Cache Hit Scenario**: No API call when directory already loaded (same path, isLoaded=true, has directory, no error)
- [ ] **Cache Miss Scenario**: Makes API call for new directory path
- [ ] **Cache Invalid Scenario**: Makes API call when existing entry has error state
- [ ] **Path Change Scenario**: Makes API call when navigating to different path in same storage
- [ ] **Multiple Storage Types**: Caching works independently per device-storage combination

**4.3 Loading State Management**

- [ ] **Loading During API Call**: Sets `isLoading: true` only when API call needed
- [ ] **No Loading for Cache**: Does not set loading state when using cached data
- [ ] **Inline Entry Creation**: Creates storage entry inline if not initialized
- [ ] **Loading State Reset**: Properly resets loading state after API completion

**4.4 Success State Transitions**

- [ ] **Directory Data Update**: Updates directory data from API response
- [ ] **State Flags**: Sets `isLoaded: true`, `isLoading: false`, `error: null`
- [ ] **Timestamp Recording**: Records `lastLoadTime` with current timestamp
- [ ] **State Isolation**: Preserves other storage entries unchanged
- [ ] **Path Update**: Updates `currentPath` to navigated path

**4.5 Error Handling**

- [ ] **Error State**: Sets `isLoading: false`, preserves error message
- [ ] **Data Preservation**: Does not update directory data on error
- [ ] **Existing Data**: Preserves existing directory on error (if any)
- [ ] **Error Recovery**: Can navigate successfully after error state

### **Task 5: Test `refreshDirectory()` Method** ✅

**Test Group**: Directory Refresh

**Core Behaviors**:

- [ ] **Force API Call**: Always makes API call (bypasses cache)
- [ ] **Loading State**: Sets loading state immediately
- [ ] **Success Update**: Updates directory on success, preserves on error
- [ ] **Non-Existent Entry**: Does nothing when storage entry doesn't exist
- [ ] **Cache Invalidation**: Refreshed data updates cache with new timestamp

**Refresh Scenarios**:

- [ ] **Fresh Data**: Updates with new directory data from API
- [ ] **Error During Refresh**: Handles API errors gracefully
- [ ] **Concurrent Refresh**: Handles multiple refresh calls appropriately

### **Task 6: Test `cleanupStorage()` Method** ✅

**Test Group**: Storage Cleanup

**Device Cleanup (All Storage Types)**:

- [ ] **Complete Device Removal**: Removes all entries for specific device (both SD and USB)
- [ ] **Other Device Preservation**: Does not affect other devices' storage entries
- [ ] **Selection Cleanup**: Clears `selectedDirectory` if it matches cleaned device
- [ ] **Selection Preservation**: Preserves selection if it's from different device

**Storage Type Cleanup (Specific Device-Storage)**:

- [ ] **Targeted Removal**: Removes specific device-storage combination only
- [ ] **Sibling Preservation**: Preserves other storage types for same device
- [ ] **Precise Selection Cleanup**: Clears `selectedDirectory` only if it matches exact device-storage combo
- [ ] **Related Selection Preservation**: Preserves selection for other storage types of same device

### **Task 7: Test Multi-Device State Management** ✅

**Test Group**: Cross-Device Coordination

**State Isolation**:

- [ ] **Independent State**: Maintains separate state per device-storage combination
- [ ] **Independent Caching**: Caching works independently per device-storage
- [ ] **Independent Navigation**: Navigation in one device doesn't affect others
- [ ] **Independent Loading**: Loading states are isolated per device-storage

**Global Selection Coordination**:

- [ ] **Cross-Device Selection**: Global selection works across different devices
- [ ] **Selection Exclusivity**: Only one directory selected across all devices at a time
- [ ] **Selection Switching**: Selection change clears previous device's selection
- [ ] **Selection Persistence**: Selection persists when navigating within same device-storage

### **Task 8: Test Complex Workflows** ✅

**Test Group**: Real-World Usage Scenarios

**Device Connection Flow**:

- [ ] **Initialize → Navigate → Cache**: Initialize storage → Navigate to directory → Verify cache behavior
- [ ] **Multiple Devices**: Connect multiple devices → Navigate each → Verify state isolation
- [ ] **Device Reconnection**: Cleanup device → Initialize again → Verify clean state

**Multi-Device Navigation**:

- [ ] **Device Switching**: Navigate in Device A → Switch to Device B → Navigate → Verify selection management
- [ ] **Cross-Storage Navigation**: Navigate SD storage → Navigate USB storage → Verify independent state
- [ ] **Selection Coordination**: Verify global selection updates across device switches

**Error Recovery**:

- [ ] **Error → Success Flow**: Navigate to error state → Navigate to success → Verify cache behavior
- [ ] **Error → Refresh Flow**: Encounter error → Refresh directory → Verify recovery
- [ ] **Network Recovery**: Simulate network error → Recovery → Verify state restoration

**Refresh Workflow**:

- [ ] **Navigate → Cache Hit → Refresh**: Navigate → Cache hit → Refresh → Verify new data loaded
- [ ] **Refresh After Error**: Error state → Refresh → Verify data update
- [ ] **Concurrent Operations**: Navigate while refresh in progress → Verify state consistency

### **Task 9: Test Edge Cases & Error Conditions** ✅

**Test Group**: Edge Cases & Error Handling

**Invalid Input Handling**:

- [ ] **Invalid Device IDs**: Empty, null, undefined, malformed device IDs
- [ ] **Invalid Storage Types**: Undefined, invalid enum values
- [ ] **Invalid Paths**: Empty, null, malformed paths

**Network & API Errors**:

- [ ] **Network Errors**: Simulate connection failures, timeouts
- [ ] **HTTP Errors**: 404, 500, other HTTP error responses
- [ ] **Malformed Responses**: Invalid JSON, missing required fields
- [ ] **Service Unavailable**: API temporarily unavailable scenarios

**Concurrency & Timing**:

- [ ] **Concurrent Navigation**: Multiple navigation calls to same directory
- [ ] **Navigation During Loading**: Navigate while previous navigation still loading
- [ ] **Cleanup During Navigation**: Cleanup called while navigation in progress
- [ ] **Rapid Navigation**: Quick succession of navigation calls

### **Task 10: Performance & Memory Testing** ✅

**Test Group**: Performance & Resource Management

**Memory Management**:

- [ ] **No Memory Leaks**: Verify no memory leaks in state management
- [ ] **Proper Cleanup**: Verify cleanup prevents state bloat
- [ ] **Subscription Management**: Verify RxJS subscriptions are properly managed

**Performance Testing**:

- [ ] **Large State**: Test performance with large numbers of storage entries
- [ ] **Rapid Operations**: Performance with rapid navigation/refresh operations
- [ ] **Cache Efficiency**: Verify cache provides performance benefits

### **Task 11: NgRx Testing Best Practices Validation** ✅

**Test Group**: NgRx Pattern Compliance

**Signal Store Patterns**:

- [ ] **State Signal Testing**: Proper testing of signal-based state
- [ ] **rxMethod Testing**: Correct async testing patterns for rxMethod
- [ ] **Patch State Testing**: Verify patchState operations work correctly
- [ ] **Side Effect Testing**: Test side effects and state mutations

**Testing Architecture**:

- [ ] **Test Organization**: Clear describe/test structure following NgRx patterns
- [ ] **Assertion Patterns**: Use store signal getters for state verification
- [ ] **Mock Strategies**: Follow NgRx recommended mocking approaches
- [ ] **Async Testing**: Proper handling of async operations and state changes

## 🧪 **Testing Patterns & Code Examples**

### **Store Instantiation Pattern**

```typescript
describe('StorageStore', () => {
  let store: any;
  let mockStorageService: StorageService;

  beforeEach(() => {
    mockStorageService = {
      getDirectory: vi.fn(),
    } as StorageService;

    store = createTestStore(mockStorageService);
  });

  // Tests...
});
```

### **State Assertion Pattern**

```typescript
it('should update selectedDirectory when navigating', () => {
  // Arrange
  const deviceId = 'device-1';
  const storageType = StorageType.Sd;
  const path = '/games';

  // Act
  store.navigateToDirectory({ deviceId, storageType, path });

  // Assert
  expect(store.selectedDirectory()).toEqual({
    deviceId,
    storageType,
    path,
  });
});
```

### **Async Testing Pattern**

```typescript
it('should load directory data on cache miss', async () => {
  // Arrange
  const mockDirectory = createMockStorageDirectory();
  mockStorageService.getDirectory.mockReturnValue(of(mockDirectory));

  // Act
  store.navigateToDirectory({ deviceId: 'device-1', storageType: StorageType.Sd, path: '/games' });

  // Wait for async operation
  await new Promise((resolve) => setTimeout(resolve, 0));

  // Assert
  const storageEntries = store.storageEntries();
  const key = StorageKeyUtil.create('device-1', StorageType.Sd);
  expect(storageEntries[key].directory).toEqual(mockDirectory);
  expect(storageEntries[key].isLoaded).toBe(true);
});
```

### **Cache Testing Pattern**

```typescript
it('should skip API call when directory already loaded', () => {
  // Arrange - Set up cached state
  store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.Sd });
  const mockDirectory = createMockStorageDirectory();

  // Simulate already loaded state
  const key = StorageKeyUtil.create('device-1', StorageType.Sd);
  store.patchState({
    storageEntries: {
      [key]: {
        ...store.storageEntries()[key],
        currentPath: '/games',
        directory: mockDirectory,
        isLoaded: true,
        error: null,
      },
    },
  });

  // Act
  store.navigateToDirectory({ deviceId: 'device-1', storageType: StorageType.Sd, path: '/games' });

  // Assert - No API call should be made
  expect(mockStorageService.getDirectory).not.toHaveBeenCalled();

  // But selection should still update
  expect(store.selectedDirectory()).toEqual({
    deviceId: 'device-1',
    storageType: StorageType.Sd,
    path: '/games',
  });
});
```

## 🎯 **Success Criteria**

### **Functional Requirements**

- [ ] All store methods tested comprehensively through store interface
- [ ] Smart caching logic verified with cache hits/misses/invalidation
- [ ] Dual-state management (local navigation + global selection) tested
- [ ] Multi-device coordination verified with state isolation
- [ ] Error handling and recovery scenarios covered
- [ ] Edge cases and invalid input handling tested

### **Testing Quality**

- [ ] No Angular dependencies in tests (pure TypeScript mocks)
- [ ] Follows NgRx Signal Store testing best practices
- [ ] High test coverage (>95%) for store behaviors
- [ ] Tests run fast and reliably without Angular compilation
- [ ] Clear, maintainable test organization following NgRx patterns

### **Documentation**

- [ ] All testing patterns documented and reusable
- [ ] Examples provided for future NgRx Signal Store testing
- [ ] Lessons learned captured for knowledge sharing

## 🚀 **Key Advantages of This Approach**

1. **Angular-Free Testing**: Tests run purely with mocked `StorageService` - no TestBed complexity
2. **True Unit Testing**: Test store behavior without infrastructure concerns
3. **Real-World Coverage**: Test complex workflows that users actually perform
4. **Performance**: Fast tests without Angular compilation overhead
5. **Future-Proof**: Patterns work for any NgRx Signal Store testing
6. **Best Practices**: Follows official NgRx testing recommendations

This comprehensive testing specification leverages our successful API client migration to achieve the NgRx Signal Store testing foundation that was previously blocked by Angular dependencies!

## 📝 **Task Completion Tracking**

All tasks below have been implemented and verified as passing in the `storage-state` library test suite.

- [x] Task 1: Create Mock Infrastructure
- [x] Task 2: Store Initialization & Configuration
- [x] Task 3: initializeStorage()
- [x] Task 4: navigateToDirectory() – selection + smart caching
- [x] Task 5: refreshDirectory()
- [x] Task 6: cleanupStorage()/cleanupStorageType()
- [x] Task 7: Multi-Device State Management
- [x] Task 8: Complex Workflows
- [x] Task 9: Edge Cases & Error Conditions
- [x] Task 10: Performance & Memory Testing (light checks in unit context)
- [x] Task 11: NgRx Testing Best Practices Validation
