# Logging Standards

## Overv Refresh = '🔄', // Refresh operations

Cleanup = '🧹', // Cleanup operations
Error = '❌', // Error conditions
Warning = '⚠️'**LogType.Select** (`🖱️`):

- Lookup and search operations
- Signal read operations in selectors
- Example: `🖱️ Looking up storage entry for device-1-SD`

**LogType.Error** (`❌`):

- Error conditions and failures
- Always include error context
- Example: `❌ API error for device-1-SD:`, error

**LogType.Warning** (`⚠️`):

- Warning conditions that don't break functionality
- Missing data or unexpected states
- Example: `⚠️ Cannot refresh - no entry found for device-1-SD`

**LogType.Critical** (`🛑`):

- Critical system events that require immediate attention
- System failures or security issues
- Example: `🛑 Critical system failure - unable to connect to device-1`

**LogType.Debug** (`🐞`):

- Detailed debugging information for development
- Internal state changes and diagnostic data
- Example: `🐞 Debug: Processing 15 items in storage cache`

**LogType.Midi** (`🎹`):

- MIDI-specific operations and events
- Musical instrument digital interface communications
- Example: `🎹 MIDI device connected: TeensyROM-1`itions
  Unknown = '❓', // Unknown states
  Select = '🖱️', // Lookup/search operations
  Info = 'ℹ️', // Informational messages
  Critical = '🛑', // Critical system events
  Debug = '🐞', // Debug information
  Midi = '🎹', // MIDI-specific operationsdocument establishes standards for logging within NgRx Signal Store implementations and across the application**LogType.Select** (`🖱️`):

- Lookup and search operations
- Signal read operations in selectors
- Example: `🖱️ Looking up storage entry for device-1-SD`se standards ensure consistent, meaningful, and visually distinctive logging that provides clear visibility into system operations and state changes.

**Primary Goal**: Use emoji-enhanced logging to create clear operational visibility while maintaining consistency across all store implementations.

---

## LogType Enum System

### Standard LogType Implementation

**Standard**: Use the centralized `LogType` enum from `@teensyrom-nx/utils` with emoji values for consistent logging across all store operations.

**Implementation**:

```typescript
export enum LogType {
  Start = '🚀', // Beginning of operations
  Finish = '🏁', // Operation completion
  Success = '✅', // Successful API responses
  NetworkRequest = '🌐', // API calls being made
  Navigate = '🧭', // Navigation operations
  Refresh = '🔄', // Refresh operations
  Cleanup = '🧹', // Cleanup operations
  Error = '❌', // Error conditions
  Warning = '⚠️', // Warning conditions
  Unknown = '❓', // Unknown states
  Select = '�️', // Lookup/search operations
  Info = 'ℹ️', // Informational messages
  Critical = '�', // Critical system events
  Debug = '�', // Debug information
  Midi = '�', // MIDI-specific operations
}
```

**Key Design Principles**:

- **Direct Emoji Values**: Enum values are emojis themselves, not string references
- **Visual Distinction**: Each operation type has a unique, recognizable emoji
- **Semantic Meaning**: Emojis match the conceptual meaning of the operation
- **Extensible**: Easy to add new types for domain-specific operations

### Logging Helper Functions

**Standard**: Use the standardized logging helper functions from `@teensyrom-nx/utils` that provide the LogType enum.

**Requirements**:

- **Consistent Formatting**: All logs follow `${emoji} ${message}` format
- **Optional Data**: Support for additional data objects in info logs
- **Error Preservation**: Error logs preserve original error objects
- **Type Safety**: All functions use LogType enum for operation classification

---

## Logging Patterns by Operation Type

### Operation Lifecycle Logging

**Standard**: Use consistent logging patterns that clearly show operation lifecycle from start to completion.

**Pattern Example**:

```typescript
export const initializeStorage = async ({ deviceId, storageType }) => {
  const key = StorageKeyUtil.create(deviceId, storageType);

  // 1. Start Operation
  logInfo(LogType.Start, `Starting async initialization for ${key}`);

  // 2. Check for Cache Hit
  if (isDirectoryLoadedAtPath(existingEntry, '/')) {
    logInfo(LogType.Info, `${key} already loaded, skipping initialization`);
    return;
  }

  try {
    // 3. Network Request
    logInfo(LogType.NetworkRequest, `Making API call for ${key}`);

    const directory = await firstValueFrom(storageService.getDirectory(deviceId, storageType, '/'));

    // 4. Success Response
    logInfo(LogType.Success, `API call successful for ${key}:`, directory);

    // Update state...

    // 5. Operation Complete
    logInfo(LogType.Finish, `Initialization completed for ${key}`);
  } catch (error) {
    // 6. Error Handling
    logError(`API error for ${key}:`, error);
  }
};
```

### Specific LogType Usage Guidelines

**LogType.Start** (`🚀`):

- Use at the beginning of all major operations
- Include operation context (keys, identifiers)
- Example: `🚀 Starting async initialization for device-1-SD`

**LogType.NetworkRequest** (`🌐`):

- Log immediately before making API calls
- Include endpoint context and parameters
- Example: `🌐 Making API call for device-1-SD`

**LogType.Success** (`✅`):

- Log successful API responses
- Include response data when helpful for debugging
- Example: `✅ API call successful for device-1-SD:`, data

**LogType.Finish** (`🏁`):

- Log when operations are fully completed
- Marks the end of the operation lifecycle
- Example: `🏁 Initialization completed for device-1-SD`

**LogType.Info** (`ℹ️`):

- Cache hits and informational messages
- Non-critical status updates
- Example: `ℹ️ device-1-SD already loaded, skipping initialization`

**LogType.Navigate** (`🧭`):

- Directory or route navigation operations
- Include source and destination context
- Example: `🧭 Navigating to device-1-SD at path: /games`

**LogType.Refresh** (`🔄`):

- Data refresh operations
- Include what is being refreshed
- Example: `🔄 Refreshing directory for device-1-SD at path: /games`

**LogType.Cleanup** (`🧹`):

- Cleanup and removal operations
- Include what is being cleaned up
- Example: `🧹 Cleaning up all storage entries for device: device-1`

**LogType.Select** (`�️`):

- Lookup and search operations
- Signal read operations in selectors
- Example: `�️ Looking up storage entry for device-1-SD`

**LogType.Error** (`❌`):

- Error conditions and failures
- Always include error context
- Example: `❌ API error for device-1-SD:`, error

**LogType.Warning** (`⚠️`):

- Warning conditions that don't break functionality
- Missing data or unexpected states
- Example: `⚠️ Cannot refresh - no entry found for device-1-SD`

---

## Store Integration Standards

### NgRx Signal Store Logging

**Standard**: Integrate logging consistently across all store method implementations following the established lifecycle patterns.

**Requirements**:

- **Import LogType**: Always import LogType enum and helper functions
- **Operation Lifecycle**: Log Start → NetworkRequest → Success → Finish
- **Cache Hit Handling**: Use LogType.Info for cache hits and skipped operations
- **Error Consistency**: Use logError helper for all error conditions
- **Context Inclusion**: Always include relevant identifiers (keys, IDs, paths)

**Integration Pattern**:

```typescript
import { LogType, logInfo, logError, logWarn } from '@teensyrom-nx/utils';

export const StorageStore = signalStore(
  { providedIn: 'root' },
  withDevtools('storage'),
  withState(initialState),
  withMethods((store, storageService = inject(STORAGE_SERVICE)) => ({
    operationMethod: async ({ param }) => {
      logInfo(LogType.Start, `Starting operation for ${param}`);

      // Check cache
      if (cached) {
        logInfo(LogType.Info, `Data already loaded for ${param}`);
        return;
      }

      try {
        logInfo(LogType.NetworkRequest, `Making API call for ${param}`);
        const result = await firstValueFrom(service.call(param));
        logInfo(LogType.Success, `API call successful for ${param}:`, result);

        // Update state...

        logInfo(LogType.Finish, `Operation completed for ${param}`);
      } catch (error) {
        logError(`Operation failed for ${param}:`, error);
      }
    },
  }))
);
```

### Logging Density Guidelines

**Standard**: Maintain reasonable logging density that provides operational visibility without overwhelming the console.

**Guidelines**:

- **Major Operations**: Always log start, network requests, success, and finish
- **Cache Hits**: Log info messages for cache hits to show optimization
- **State Changes**: Log significant state transitions
- **Error Conditions**: Always log errors with full context
- **Avoid Over-logging**: Don't log every minor state update or computed value access

**Good Logging Example**:

```
🚀 Starting async initialization for device-1-SD
📡 Making API call for device-1-SD
✅ API call successful for device-1-SD: {data}
🏁 Initialization completed for device-1-SD
```

**Avoid Over-logging**:

```typescript
// ❌ Don't do this - too verbose
logInfo(LogType.Debug, `Setting loading state to true`);
logInfo(LogType.Debug, `Clearing error state`);
logInfo(LogType.Debug, `Updating storage entry`);
logInfo(LogType.Debug, `Setting loaded state to true`);
```

---

## Testing Considerations

### Test Environment Logging

**Standard**: Logging should work seamlessly in test environments and provide debugging value during test development.

**Guidelines**:

- **Console Output**: Test logs appear in console during test runs
- **Test Debugging**: Use logs to understand test execution flow
- **Log Verification**: Tests can verify that appropriate logs are generated
- **Mock Considerations**: Consider mocking logging functions for focused unit tests

**Example Test with Logging**:

```typescript
it('should log complete initialization lifecycle', async () => {
  const consoleSpy = jest.spyOn(console, 'log');

  await store.initializeStorage({ deviceId: 'device-1', storageType: StorageType.SD });

  expect(consoleSpy).toHaveBeenCalledWith('🚀 Starting async initialization for device-1-SD');
  expect(consoleSpy).toHaveBeenCalledWith('📡 Making API call for device-1-SD');
  expect(consoleSpy).toHaveBeenCalledWith('🏁 Initialization completed for device-1-SD');
});
```

---

## Implementation Examples

### Reference Implementation

**Primary Example**: See [`storage-store.ts`](../libs/domain/storage/state/src/lib/storage-store.ts) and [`storage-helpers.ts`](../libs/domain/storage/state/src/lib/storage-helpers.ts) for complete implementation.

**Key Features Demonstrated**:

- LogType enum with direct emoji values
- Consistent operation lifecycle logging
- Cache hit detection with Info logs
- Comprehensive error logging
- Operation completion markers

### Console Output Example

**Well-Implemented Logging Output**:

```
🚀 Starting async initialization for device-1-SD
📡 Making API call for device-1-SD
✅ API call successful for device-1-SD: {path: '/', directories: [...]}
🏁 Initialization completed for device-1-SD

🧭 Navigating to device-1-SD at path: /games
ℹ️ Directory already loaded for device-1-SD at path: /games

🔄 Refreshing directory for device-1-SD at path: /games
📡 Loading directory for device-1-SD at path: /games
✅ Directory refresh successful for device-1-SD: {path: '/games', directories: [...]}
🏁 Refresh completed for device-1-SD

🧹 Cleaning up all storage entries for device: device-1
```

---

## Related Documentation

- **State Standards**: [`STATE_STANDARDS.md`](./STATE_STANDARDS.md) - NgRx Signal Store patterns and logging integration
- **Testing Standards**: [`TESTING_STANDARDS.md`](./TESTING_STANDARDS.md) - Testing patterns including log verification
- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md) - General coding patterns and conventions

---

## Migration Guidelines

### Adopting Logging Standards

**For New Stores**:

1. Import LogType enum and helpers from shared utilities
2. Implement operation lifecycle logging from the start
3. Follow the established patterns for cache hits and errors
4. Include appropriate context in all log messages

**For Existing Stores**:

1. Add LogType enum and helper imports
2. Review existing console.log statements and convert to LogType system
3. Add missing lifecycle logs (Start, Finish)
4. Ensure cache hit detection uses LogType.Info
5. Standardize error logging with logError helper

**Best Practice**: Start with logging integration during store development rather than retrofitting, as it provides valuable debugging during implementation.

---

## Centralized Logging Library

### Utils Library Integration

**Standard**: All logging utilities are now centralized in the `@teensyrom-nx/utils` library for consistent usage across the entire application.

**Import Pattern**:

```typescript
import { LogType, logInfo, logError, logWarn } from '@teensyrom-nx/utils';
```

**Benefits**:

- **Cross-Domain Consistency**: Same logging patterns across all domains (storage, device, UI)
- **Centralized Management**: Single source of truth for LogType definitions and helper functions
- **Reduced Duplication**: No need to duplicate logging code in each domain
- **Enhanced Maintainability**: Easy to modify logging behavior globally
- **Tree Shakable**: Optimized for bundler tree shaking

**Migration**: All existing logging code has been updated to use the centralized library:

- Storage domain actions and helpers
- Device services (events and logs)
- Application bootstrap
- UI components
- Test utilities

**Quick Reference**:

```typescript
import { LogType, logInfo, logError, logWarn } from '@teensyrom-nx/utils';

// Operation lifecycle pattern
logInfo(LogType.Start, `Starting operation for ${key}`);
logInfo(LogType.NetworkRequest, `Making API call for ${key}`);
logInfo(LogType.Success, `API call successful for ${key}:`, data);
logInfo(LogType.Finish, `Operation completed for ${key}`);

// Cache hits and informational
logInfo(LogType.Info, `${key} already loaded, skipping operation`);

// Error handling
logError(`Operation failed for ${key}:`, error);

// Warnings
logWarn(`Cannot process - missing data for ${key}`);
```
