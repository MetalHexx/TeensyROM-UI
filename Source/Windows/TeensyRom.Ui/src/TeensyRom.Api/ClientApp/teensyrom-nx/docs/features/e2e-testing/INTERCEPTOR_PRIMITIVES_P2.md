# Phase 2 Implementation Plan: Interceptor Primitive Migration

## Overview

This plan details the systematic refactoring of existing E2E interceptor files to use the new primitive-based architecture. The migration focuses on **interceptor files only** - test files will remain unchanged as all function signatures and interfaces will be preserved.

## Phase Objectives

1. **Eliminate Code Duplication**: Replace custom interceptor logic with reusable primitive functions
2. **Ensure Consistency**: Standardize error handling using RFC 9110 ProblemDetails format
3. **Maintain Compatibility**: Zero changes to test files - all existing contracts preserved
4. **Improve Maintainability**: Centralize interceptor behavior in primitive functions

## Migration Strategy

### **Core Principles**

- **Zero Test Changes**: All test files remain exactly as they are
- **Interface Preservation**: All function signatures and behavior remain identical
- **Primitive Integration**: Replace custom `cy.intercept()` logic with primitive function calls
- **Continuous Validation**: Test after each migration to ensure no regressions

### **Migration Process for Each Interceptor**

1. **Establish Baseline**: Run E2E tests to verify current working state
2. **Analyze Current Implementation**: Document existing functionality and interfaces
3. **Refactor to Primitives**: Replace custom logic with appropriate primitive functions
4. **Validate Functionality**: Run tests to ensure migration success
5. **Apply Clean-Code**: Use `/clean-code` command for quality assurance
6. **Final Validation**: Comprehensive test run to confirm no regressions

## Interceptor Migration Plan

### **Phase 2.1: Device Domain Migration (4 interceptors)**

These interceptors already follow individual file structure and are ready for primitive migration.

#### **2.1.1 findDevices.interceptors.ts**

**Current State Analysis:**

- 287 lines with extensive custom logic
- Custom RFC 9110 error handling (lines 94-98)
- Manual response delay implementation (lines 116-118)
- Network error handling (lines 247-253)
- Multiple convenience functions with duplicate patterns

**Migration Target:**

- Replace `req.reply()` error logic with `interceptError()` primitive
- Replace success response with `interceptSuccess()` primitive
- Replace network error with `interceptError()` network error mode
- Simplify convenience functions using primitive parameters

**Expected Reduction:** ~60% (from 287 to ~115 lines)

**Before Example:**

```typescript
// Current custom error handling
if (options.errorMode) {
  const statusCode = options.statusCode || 500;
  const errorMessage = options.errorMessage || 'Internal Server Error';
  req.reply({
    statusCode,
    headers: { 'content-type': 'application/problem+json' },
    body: {
      type: `https://tools.ietf.org/html/rfc9110#section-${getRfcSection(statusCode)}`,
      title: errorMessage,
      status: statusCode,
      detail: getErrorTitle(statusCode),
    },
  });
  return;
}
```

**After Example:**

```typescript
// Primitive-based error handling
if (options.errorMode) {
  interceptError(req, {
    statusCode: options.statusCode || 500,
    title: options.errorMessage || 'Internal Server Error',
  });
  return;
}
```

#### **2.1.2 connectDevice.interceptors.ts**

**Migration Focus:**

- Replace custom connection success/error handling with primitives
- Maintain device-specific response formatting
- Preserve all existing convenience functions

#### **2.1.3 disconnectDevice.interceptors.ts**

**Migration Focus:**

- Simplify disconnection response handling using primitives
- Maintain device-specific response patterns
- Preserve error simulation capabilities

#### **2.1.4 pingDevice.interceptors.ts**

**Migration Focus:**

- Replace health check response logic with primitives
- Maintain device ping-specific response format
- Preserve timeout and error simulation

### **Phase 2.2: Player Domain Migration (2 interceptors)**

#### **2.2.1 launchFile.interceptors.ts**

**Migration Focus:**

- Replace file launch response handling with primitives
- Maintain file-specific response formatting
- Preserve launch error simulation

#### **2.2.2 launchRandom.interceptors.ts**

**Migration Focus:**

- Replace random launch response logic with primitives
- Maintain random selection logic
- Preserve error handling patterns

#### **2.2.3 player.interceptors.ts cleanup**

- Remove barrel export file
- Ensure individual interceptors are properly exported
- Update any import statements if needed

### **Phase 2.3: Storage Domain Migration (5 interceptors)**

These interceptors need extraction from consolidated files first, then primitive migration.

#### **2.3.1 getDirectory.interceptors.ts**

**Current State:** Consolidated file with multiple endpoints
**Migration Steps:**

1. Extract directory browsing logic to individual file
2. Refactor to use primitives
3. Maintain all existing functionality
4. Preserve backward compatibility

#### **2.3.2 saveFavorite.interceptors.ts**

**Migration Steps:**

1. Extract save favorite logic to individual file
2. Refactor to use primitives
3. Maintain favorite-specific response format
4. Preserve error handling patterns

#### **2.3.3 removeFavorite.interceptors.ts**

**Migration Steps:**

1. Extract remove favorite logic to individual file
2. Refactor to use primitives
3. Maintain favorite-specific response format
4. Preserve error handling patterns

#### **2.3.4 indexStorage.interceptors.ts**

**Migration Steps:**

1. Extract storage indexing logic to individual file
2. Refactor to use primitives
3. Maintain indexing-specific response format
4. Preserve progress tracking capabilities

#### **2.3.5 indexAllStorage.interceptors.ts**

**Migration Steps:**

1. Extract all storage indexing logic to individual file
2. Refactor to use primitives
3. Maintain bulk indexing response format
4. Preserve progress tracking and error handling

## Implementation Details

### **Primitive Function Integration**

#### **interceptSuccess() Usage Pattern**

```typescript
// Replace custom success responses
const response: FindDevicesResponse = {
  devices: [...fixture.devices],
  message: `Found ${fixture.devices.length} device(s)`,
};

interceptSuccess(req, {
  data: response,
  delayMs: options.responseDelayMs,
});
```

#### **interceptError() Usage Pattern**

```typescript
// Replace custom error responses
interceptError(req, {
  statusCode: options.statusCode || 500,
  title: options.errorMessage || 'Internal Server Error',
});
```

#### **Network Error Pattern**

```typescript
// Replace custom network error
interceptError(req, {
  networkError: true,
});
```

### **Interface Preservation**

All existing interfaces will be preserved exactly:

```typescript
export interface InterceptFindDevicesOptions {
  fixture?: MockDeviceFixture;
  errorMode?: boolean;
  responseDelayMs?: number;
  statusCode?: number;
  errorMessage?: string;
}
```

### **Backward Compatibility**

All existing exports will be maintained:

- Function signatures remain identical
- All convenience functions preserved
- Export constants maintained
- Import statements continue to work

## Validation Gates

### **Pre-Migration Validation**

- [ ] Full E2E test suite passes to establish baseline
- [ ] No TypeScript errors or warnings
- [ ] All interceptors properly exported and accessible

### **Per-Interceptor Validation**

- [ ] E2E tests pass after migration
- [ ] All existing functionality preserved
- [ ] Function signatures unchanged
- [ ] Clean-code validation passes
- [ ] No TypeScript errors

### **Post-Migration Validation**

- [ ] Full E2E test suite passes
- [ ] All interceptors use primitive functions
- [ ] Code reduction targets achieved
- [ ] Performance maintained or improved
- [ ] Clean-code validation completed for all files

## Success Criteria

### **Functional Requirements**

- ✅ All 11 interceptors successfully migrated to primitive-based architecture
- ✅ Zero test failures throughout migration process
- ✅ All existing functionality preserved without changes
- ✅ Complete backward compatibility maintained

### **Quality Requirements**

- ✅ Code duplication reduced by 40-60% per interceptor
- ✅ All error responses use RFC 9110 ProblemDetails format
- ✅ Clean-code validation completed for all migrated files
- ✅ Consistent patterns established across all interceptors

### **Maintainability Requirements**

- ✅ Primitive functions centralized and reusable
- ✅ Future interceptor development simplified
- ✅ Documentation updated to reflect new patterns
- ✅ Architecture prepared for future enhancements

## Risk Mitigation

### **Migration Risks**

- **Risk**: Breaking existing functionality
- **Mitigation**: Comprehensive testing after each migration
- **Risk**: Interface changes affecting tests
- **Mitigation**: Zero interface changes - preserve all signatures

### **Quality Risks**

- **Risk**: Introduction of bugs during refactoring
- **Mitigation**: One-by-one migration with continuous validation
- **Risk**: Performance regression
- **Mitigation**: Baseline benchmarking and per-migration validation

## Timeline Estimation

### **Device Domain (4 interceptors)**: 2-3 days

- findDevices: 4-6 hours (most complex)
- connectDevice: 2-3 hours
- disconnectDevice: 2-3 hours
- pingDevice: 2-3 hours

### **Player Domain (2 interceptors)**: 1-2 days

- launchFile: 2-3 hours
- launchRandom: 2-3 hours
- barrel cleanup: 1-2 hours

### **Storage Domain (5 interceptors)**: 3-4 days

- Each interceptor: 4-6 hours (extraction + migration)

### **Validation and Cleanup**: 1-2 days

- Comprehensive testing
- Final clean-code validation
- Documentation updates

**Total Estimated Timeline**: 7-11 days

## Next Steps

1. **Begin Baseline Testing**: Run full E2E test suite to establish working state
2. **Start Device Domain Migration**: Begin with findDevices.interceptors.ts
3. **Continuous Validation**: Test after each interceptor migration
4. **Quality Assurance**: Apply clean-code validation throughout process
5. **Final Validation**: Comprehensive testing and documentation updates

This plan provides a systematic approach to migrating all interceptors to use the primitive infrastructure while maintaining complete backward compatibility and ensuring zero test file changes.
