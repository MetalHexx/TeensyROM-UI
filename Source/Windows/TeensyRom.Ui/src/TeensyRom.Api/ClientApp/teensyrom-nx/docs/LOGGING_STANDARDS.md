# Logging Standards

## Overview

Use emoji-enhanced logging from `@teensyrom-nx/utils` for consistent operational visibility across all domains.

## Quick Reference

```typescript
import { LogType, logInfo, logError, logWarn } from '@teensyrom-nx/utils';

// Operation lifecycle
logInfo(LogType.Start, `Starting operation for ${key}`);
logInfo(LogType.NetworkRequest, `Making API call for ${key}`);
logInfo(LogType.Success, `API call successful for ${key}:`, data);
logInfo(LogType.Finish, `Operation completed for ${key}`);

// Cache hits and info
logInfo(LogType.Info, `${key} already loaded, skipping operation`);

// Errors and warnings
logError(`Operation failed for ${key}:`, error);
logWarn(`Cannot process - missing data for ${key}`);
```

## Usage Guidelines

### Operation Lifecycle Pattern

1. **Start** → **NetworkRequest** → **Success** → **Finish**
2. Use **Info** for cache hits and skipped operations
3. Use **Error**/**Warning** for failure cases
4. Include relevant context (keys, IDs, paths) in all messages

### Integration

Should be use everywhere.
