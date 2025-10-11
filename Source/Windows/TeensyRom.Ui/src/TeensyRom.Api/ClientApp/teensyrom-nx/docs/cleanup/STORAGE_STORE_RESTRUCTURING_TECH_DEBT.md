# Storage Store Restructuring - Technical Debt

This document tracks technical debt and future improvements identified during the Storage Store restructuring project.

## Deferred Improvements

### 1. **State Update Granularity**

**Issue**: Helper functions currently require callers to provide complete update objects, but the actual updates often need to preserve existing state.

**Current Workaround**: Helpers do shallow merging with existing entry state manually.

**Future Improvement**:

- Make helpers that handle the merging logic inside the helpers automatically
- Create more sophisticated state update utilities that understand the complete context

**Impact**: Medium - Would simplify calling code and reduce boilerplate

### 2. **Error Message Handling Standardization**

**Issue**: Error handling varies across methods - some preserve original error messages, others use hardcoded strings.

**Current Approach**: Preserved existing varied error handling to maintain test compatibility.

**Future Improvement**:

- Standardize error message extraction: `(error as any)?.message || 'fallback'`
- Create error helper that accepts either error objects or strings
- Consistent error formatting across all methods

**Impact**: Low - Mainly for consistency and debugging

### 3. **Complex State Logic Helpers**

**Issue**: Some methods have nuanced state update patterns that couldn't be easily extracted into helpers.

**Examples**:

- `initializeStorage` has different logic for "entry doesn't exist" vs "entry exists but needs reset"
- `navigateToDirectory` updates `currentPath` even on error, but `directory: null` only on error

**Future Improvement**:

- Create more sophisticated helpers that can handle these variations
- Consider state machine patterns for complex state transitions
- Extract validation and conditional logic into reusable utilities

**Impact**: Medium - Would significantly reduce code duplication

### 4. **Console Logging Consistency**

**Issue**: Console logging is inconsistent and mixed with business logic.

**Current Approach**: Preserved all existing logging to avoid breaking tests.

**Future Improvement**:

- Move logging into helpers where appropriate
- Create logging utilities that handle formatting consistently
- Consider separating logging concerns from business logic

**Impact**: Low - Mainly for maintainability and debugging

### 5. **Type Safety Improvements**

**Issue**: Store method parameters don't have strong typing constraints.

**Current Approach**: Kept simple typing to avoid adding complexity.

**Future Improvement**:

- Add stronger typing for store method parameters
- Use more restrictive typed approaches in helpers
- Gradually migrate to stronger typing across the store

**Impact**: Medium - Would improve development experience and catch errors earlier

## Implementation Priority

1. **High Priority**: Complex State Logic Helpers (#3)

   - Would provide the most significant reduction in code duplication

2. **Medium Priority**: State Update Granularity (#1), Type Safety Improvements (#5)

   - Good developer experience improvements

3. **Low Priority**: Error Message Handling (#2), Console Logging Consistency (#4)
   - Nice to have for consistency

## Notes

- All deferred improvements should maintain backward compatibility
- Test coverage must remain at 100% for any future changes
- Consider these improvements only after Phase 3 (Documentation Updates) is complete
