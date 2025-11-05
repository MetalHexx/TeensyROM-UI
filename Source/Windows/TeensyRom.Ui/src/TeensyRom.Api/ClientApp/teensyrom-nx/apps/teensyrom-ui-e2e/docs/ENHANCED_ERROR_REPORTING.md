# Enhanced Error Reporting for E2E Test Suite

## Overview

This document describes the enhanced error reporting system implemented across the TeensyROM E2E test suite to provide better debugging information for timing-related assertions and async operations.

## Features

### 1. Enhanced Timing Assertions

**Functions with enhanced error reporting:**

#### URL Parameter Assertions

- **`expectUrlContainsParams()`** - Enhanced with detailed error messages showing:
  - Expected vs actual URL parameters
  - Elapsed time for the operation
  - Specific missing parameters
  - Debugging suggestions

#### Navigation Functions

- **`goBack()`** - Enhanced with error messages for:
  - History state issues
  - Navigation failures
  - URL verification problems
- **`goForward()`** - Same enhancements as `goBack()`

### 2. Enhanced API Wait Functions

#### File Operations

- **`waitForFileLaunch()`** - Enhanced with:
  - Request/response logging
  - File path extraction
  - Error status code detection
  - Device-specific debugging suggestions

#### Directory Operations

- **`waitForDirectoryLoad()`** - Enhanced with:
  - Directory path logging
  - Contents summary (file/directory counts)
  - File name logging for small directories
  - Filesystem-specific error handling

#### Favorite Operations

- **`waitForSaveFavorite()`** - Enhanced with:
  - Storage device status checking
  - Duplicate file detection hints
  - Filesystem permission guidance
- **`waitForRemoveFavorite()`** - Enhanced with similar error reporting

### 3. Enhanced Alert Operations

- **`dismissAlert()`** - Enhanced with:
  - Alert existence verification
  - Button state checking
  - DOM structure debugging
  - Auto-dismiss detection
- **`verifyAlertDismissed()`** - Enhanced with:
  - Timeout tracking
  - Multiple alert detection
  - Animation completion checks

### 4. Timing Utilities

New utility functions in `timing.utils.ts`:

#### TimingTracker Class

- Tracks operation start time and elapsed duration
- Provides success/failure logging with context
- Approaching timeout warnings
- Human-readable duration formatting

#### Enhanced Wait Functions

- **`enhancedWait()`** - For API interceptors with detailed error reporting
- **`enhancedElementWait()`** - For DOM elements with selector debugging
- **`retryOperation()`** - For flaky operations with retry logic

#### Performance Tracking

- **`PerformanceTracker`** - Monitor operation timing across test runs
- Performance reporting with averages, min/max times
- Identification of slow operations

## Error Message Format

All enhanced error messages follow this consistent format:

```
❌ [Operation Name] failed after [elapsed time] (timeout: [timeout])
[Specific error details]

💡 Common causes:
  • Cause 1
  • Cause 2
  • Cause 3

🔧 Debugging suggestions:
  • Suggestion 1
  • Suggestion 2
  • Suggestion 3
```

## Usage Examples

### Basic Usage (Enhanced Functions Work Automatically)

```typescript
// Enhanced URL parameter assertion
expectUrlContainsParams(
  {
    device: 'test-device',
    file: 'game.crt',
  },
  { logWaiting: true }
);

// Enhanced API wait with custom timeout
waitForFileLaunch(15000); // 15 second timeout

// Enhanced navigation with URL verification
goBack('/player', 10000); // 10 second timeout
```

### Advanced Usage with Timing Utilities

```typescript
import { TimingTracker, enhancedWait, retryOperation } from '../support/utils/timing.utils';

// Using TimingTracker for custom operations
const tracker = new TimingTracker({
  operation: 'Custom file processing',
  timeout: 20000,
  logTiming: true,
});

try {
  // Custom operation logic
  tracker.complete('Processing completed successfully');
} catch (error) {
  tracker.fail(error, {
    context: 'During file processing step 3',
    suggestion: 'Check file format compatibility',
  });
}

// Using enhanced wait for interceptors
enhancedWait('customApiCall', {
  operation: 'Waiting for custom API',
  timeout: 15000,
  context: 'During user authentication flow',
});

// Using retry for flaky operations
retryOperation(
  () => {
    return cy.get('.unstable-element').should('be.visible');
  },
  {
    maxRetries: 3,
    retryDelay: 1000,
    operationName: 'Waiting for unstable element',
    onRetry: (attempt, error) => {
      cy.log(`Retry attempt ${attempt} due to: ${error.message}`);
    },
  }
);
```

## Logging Levels

### Success Logging

- ✅ Green checkmarks for successful operations
- Elapsed time information
- Request/response details for API calls
- File/directory path information

### Warning Logging

- ⚠️ Warning signs for approaching timeouts
- ⚠️ Warning signs for error responses (4xx/5xx)
- ⚠️ Warning signs for non-critical issues

### Error Logging

- ❌ Red X marks for failures
- Detailed error context and suggestions
- Stack traces and debugging information

## Performance Monitoring

### Automatic Performance Tracking

All enhanced functions automatically track and report:

- Operation duration
- Success/failure rates
- Timeout occurrences

### Performance Reports

Use `PerformanceTracker` to generate reports:

```typescript
const tracker = new PerformanceTracker();

// During test execution
tracker.recordOperation('fileLaunch', 1200);
tracker.recordOperation('directoryLoad', 800);
tracker.recordOperation('fileLaunch', 1500);

// Generate report
tracker.logReport();
// Output:
// 📊 Performance Report:
//   fileLaunch:
//     Count: 2
//     Average: 1.4s
//     Range: 1.2s - 1.5s
//   directoryLoad:
//     Count: 1
//     Average: 800ms
//     Range: 800ms - 800ms
```

## Common Debugging Scenarios

### Timeout Issues

When timeouts occur, the enhanced error reporting provides:

1. **Root Cause Analysis** - Network vs. application vs. test issues
2. **Specific Suggestions** - Exact commands to increase timeouts
3. **Context Information** - What was being waited for and why

### Network Issues

Enhanced API wait functions detect and report:

1. **Request Details** - Method, URL, parameters
2. **Response Analysis** - Status codes, response bodies
3. **Connectivity Guidance** - Network-specific debugging steps

### DOM Issues

Enhanced element wait functions help with:

1. **Selector Problems** - Invalid or changed selectors
2. **Timing Issues** - Element not yet rendered
3. **Visibility Problems** - Hidden or covered elements

### Race Conditions

The retry utilities help resolve:

1. **Asynchronous Timing** - Operations completing out of order
2. **Resource Contention** - Multiple operations competing
3. **Transient Failures** - Temporary system issues

## Best Practices

### 1. Use Enhanced Functions by Default

- Replace all basic `cy.wait('@alias')` calls with enhanced versions
- Use `expectUrlContainsParams()` instead of manual URL assertions
- Use enhanced navigation functions for history operations

### 2. Enable Logging During Development

- Set `logWaiting: true` for critical operations
- Use `TimingTracker` for complex custom operations
- Monitor performance reports for slow tests

### 3. Configure Appropriate Timeouts

- Use default timeouts (10s) for most operations
- Increase to 15-30s for slow filesystem operations
- Use shorter timeouts (5s) for fast UI operations

### 4. Handle Expected Failures Gracefully

- Use try-catch blocks around operations that might fail
- Provide meaningful error context
- Use retry utilities for known flaky operations

### 5. Monitor Performance

- Track operation times across test runs
- Identify regressions in performance
- Optimize slow operations

## Migration Guide

### From Basic Functions to Enhanced

**Before:**

```typescript
cy.wait('@launchFile');
cy.location('search').should('include', 'file=test.crt');
cy.go('back');
```

**After:**

```typescript
waitForFileLaunch();
expectUrlContainsParams({ file: 'test.crt' }, { logWaiting: true });
goBack('/player', 10000);
```

### Updating Custom Wait Functions

**Before:**

```typescript
function waitForCustomElement() {
  cy.get('.custom-element', { timeout: 10000 }).should('be.visible');
}
```

**After:**

```typescript
function waitForCustomElement() {
  const tracker = new TimingTracker({
    operation: 'Waiting for custom element',
    timeout: 10000,
  });

  cy.get('.custom-element', { timeout: 10000 })
    .should('be.visible')
    .then(() => {
      tracker.complete();
    })
    .catch((err) => {
      tracker.fail(err, {
        suggestion: 'Check if the custom element is properly rendered',
      });
    });
}
```

## Troubleshooting

### Enhanced Errors Not Appearing

1. **Import Issues** - Ensure enhanced functions are imported correctly
2. **Version Conflicts** - Check for multiple versions of wait functions
3. **Configuration** - Verify that Cypress logging is enabled

### Performance Impact

1. **Over-Logging** - Reduce logging for stable operations
2. **Tracking Overhead** - Disable performance tracking for production runs
3. **Memory Usage** - Clear performance trackers between tests

### Custom Integration

1. **Inheritance** - Extend `TimingTracker` for custom functionality
2. **Formatting** - Use `formatDuration()` for consistent time display
3. **Patterns** - Follow established error message formats

## Future Enhancements

### Planned Features

1. **Visual Timing Charts** - Graphical performance visualization
2. **Integration with CI/CD** - Performance trend monitoring
3. **Automatic Timeout Adjustment** - AI-powered timeout optimization
4. **Cross-Browser Timing** - Browser-specific performance tuning

### Extension Points

1. **Custom Error Handlers** - Domain-specific error reporting
2. **Integration Hooks** - External monitoring system connections
3. **Template System** - Reusable error message templates
