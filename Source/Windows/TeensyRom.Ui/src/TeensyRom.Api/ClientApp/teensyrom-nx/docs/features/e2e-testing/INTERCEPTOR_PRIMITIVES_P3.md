# Phase 3: Direct Intercept Cleanup

## 🎯 Objective

Complete the interceptor primitives architecture transformation by eliminating all remaining direct `cy.intercept()` usage throughout the E2E test suite while maintaining test functionality and improving readability through standardized wrapper functions.

**Key Benefits:**
- Eliminate 12 remaining direct `cy.intercept()` instances across 3 test files
- Achieve 100% consistency in interceptor usage patterns across the entire E2E test suite
- Improve test maintainability through centralized validation and behavior utilities
- Complete the primitive-based architecture foundation for future testing capabilities

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [x] [INTERCEPTOR_PRIMITIVES_PLAN.md](./INTERCEPTOR_PRIMITIVES_PLAN.md) - Complete architecture overview and design rationale
- [x] [INTERCEPTOR_PRIMITIVES_P2.md](./INTERCEPTOR_PRIMITIVES_P2.md) - Phase 2 completion status and implementation details
- [ ] [INTERCEPTOR_PRIMITIVES_P3.md](./INTERCEPTOR_PRIMITIVES_P3.md) - This Phase 3 implementation plan
- [ ] [E2E_TESTS.md](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Current E2E testing patterns and standards

**Standards & Guidelines:**
- [ ] [INTERCEPTOR_PRIMITIVES.md](../../../apps/teensyrom-ui-e2e/src/support/interceptors/primitives/INTERCEPTOR_PRIMITIVES.md) - Primitive library documentation
- [ ] [CODING_STANDARDS.md](./CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [TESTING_STANDARDS.md](./TESTING_STANDARDS.md) - Testing approaches and best practices

---

## 📂 File Structure Overview

> Provide a clear file tree showing new files (✨) and modified files (📝) to understand the implementation scope.

**Existing Endpoint Interceptors (Enhanced with New Helper Functions):**
```
apps/teensyrom-ui-e2e/src/support/interceptors/
├── connectDevice.interceptors.ts                📝 Enhanced - Add validation & counting helpers
├── disconnectDevice.interceptors.ts             📝 Enhanced - Add validation & counting helpers
├── findDevices.interceptors.ts                  ✅ Existing - Already migrated in Phase 2
├── pingDevice.interceptors.ts                   ✅ Existing - Already migrated in Phase 2
└── primitives/
    ├── interceptor-primitives.ts                ✅ Existing - Core primitive functions
    └── interceptor-primitives.spec.ts           ✅ Existing - Comprehensive test coverage
```

**Test Files (Modified - Direct Intercepts Replaced):**
```
apps/teensyrom-ui-e2e/src/e2e/devices/
├── device-connection.cy.ts                      📝 Modified - Replace 2 direct intercepts
├── device-refresh-connection.cy.ts              📝 Modified - Replace 4 direct intercepts, document 2
└── device-refresh-error.cy.ts                   📝 Modified - Replace 6 direct intercepts
```

---

## 📋 Implementation Guidelines

> **IMPORTANT - MANDATORY WORKFLOW FOR ALL FILE CHANGES:**
> - **Code Implementation**: Use `@agent-clean-coder` to write/implement the code changes
> - **Initial Testing**: Use `@agent-e2e-runner` to test the affected file and validate functionality
> - **Code Cleaning**: Use `@agent-code-cleaner` to clean and optimize the code
> - **Final Testing**: Use `@agent-e2e-runner` again to ensure no regressions from cleaning
> - **Repeat as Needed**: If any step fails, repeat the cycle until perfect

> **IMPORTANT - Testing Policy:**
> - **Test after each individual file change** - Never batch multiple file changes without testing
> - **Zero regression policy** - All existing tests must continue passing throughout the entire process
> - **Complete isolation** - Each file change is tested independently before proceeding

> **IMPORTANT - Progress Tracking:**
> - **Mark checkboxes ✅ as you complete each subtask** - Update progress throughout implementation
> - **Document workflow results** - Record results from each agent execution
> - **One file at a time** - Never modify multiple files in the same task

---

<details open>
<summary><h3>Task 1: Enhance connectDevice & disconnectDevice with Validation Helpers</h3></summary>

**Purpose**: Add new helper functions to existing `connectDevice.interceptors.ts` and `disconnectDevice.interceptors.ts` files for device ID validation scenarios currently handled by direct intercepts.

**Related Documentation:**
- [Device Connection Patterns](./INTERCEPTOR_PRIMITIVES_PLAN.md#primitive-function-categories) - Primitive usage patterns
- [Device Domain Migration](./INTERCEPTOR_PRIMITIVES_P2.md#phase-21-device-domain-migration) - Device domain patterns from Phase 2
- [Existing connectDevice Interceptor](../../../apps/teensyrom-ui-e2e/src/support/interceptors/connectDevice.interceptors.ts) - Current implementation

**Implementation Subtasks:**
- [ ] **Subtask 1.1**: Enhance `connectDevice.interceptors.ts` with device ID validation helper functions
- [ ] **Subtask 1.2**: Add `setupConnectDeviceWithValidation()` helper using `interceptSuccess()` and `interceptError()` primitives
- [ ] **Subtask 1.3**: Add device ID request inspection logic to validation helper
- [ ] **Subtask 1.4**: Enhance `disconnectDevice.interceptors.ts` with validation helper functions
- [ ] **Subtask 1.5**: Add `setupDisconnectDeviceWithValidation()` helper using primitives
- [ ] **Subtask 1.6**: Add device ID request inspection logic for disconnection validation

**Testing Subtask:**
- [ ] **Test Enhanced Helpers**: Execute `@agent-e2e-runner` on `device-connection.cy.ts` to validate new helpers work correctly

**Workflow Execution:**
- [ ] **Code Implementation**: Use `@agent-clean-coder` to add new helpers to existing interceptor files
- [ ] **Initial Testing**: Use `@agent-e2e-runner` to test affected functionality
- [ ] **Code Cleaning**: Use `@agent-code-cleaner` to clean the enhanced files
- [ ] **Final Testing**: Use `@agent-e2e-runner` to ensure no regressions from cleaning

**Key Implementation Notes:**
- New helpers follow the established pattern in existing interceptor files (see `connectDevice.interceptors.ts` structure)
- Validation helpers use primitive functions (interceptSuccess, interceptError) under the hood
- Request inspection logic validates device IDs in connection/disconnection requests
- All function signatures maintain backward compatibility with existing test patterns
- Error scenarios follow RFC 9110 compliance standards established in Phase 2
- Add helpers to the "HELPER FUNCTIONS" section of each interceptor file

**Testing Focus for Task 1:**

> Focus on **request validation behavior** - device ID validation should work correctly

**Behaviors to Test:**
- [ ] **Device ID Validation**: Connection requests with correct device IDs are accepted
- [ ] **Invalid Device ID Handling**: Connection requests with invalid device IDs trigger appropriate errors
- [ ] **Request Inspection**: Custom validation logic correctly examines request parameters
- [ ] **Primitive Integration**: Validation helpers properly use interceptor primitives

**Testing Reference:**
- See [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) for behavioral testing patterns
- Use `@agent-e2e-runner` for comprehensive test validation

</details>

---

<details open>
<summary><h3>Task 2: Enhance connectDevice & disconnectDevice with Counting Helpers</h3></summary>

**Purpose**: Add new helper functions to existing `connectDevice.interceptors.ts` and `disconnectDevice.interceptors.ts` files for API call counting scenarios currently handled by direct intercepts in refresh connection tests.

**Related Documentation:**
- [API Call Counting Patterns](./INTERCEPTOR_PRIMITIVES_PLAN.md#dynamic-primitives) - Dynamic behavior patterns
- [Behavior Validation Approach](./INTERCEPTOR_PRIMITIVES_P2.md#phase-22-player-domain-migration) - Validation patterns
- [Existing connectDevice Interceptor](../../../apps/teensyrom-ui-e2e/src/support/interceptors/connectDevice.interceptors.ts) - Current implementation

**Implementation Subtasks:**
- [ ] **Subtask 2.1**: Enhance `connectDevice.interceptors.ts` with API call counting helper functions
- [ ] **Subtask 2.2**: Add `setupConnectDeviceWithCounting()` helper for API call tracking
- [ ] **Subtask 2.3**: Add state management logic for tracking connection call counts
- [ ] **Subtask 2.4**: Enhance `disconnectDevice.interceptors.ts` with API call counting helper functions
- [ ] **Subtask 2.5**: Add `setupDisconnectDeviceWithCounting()` helper for API call tracking
- [ ] **Subtask 2.6**: Add utility functions for resetting and validating call counts

**Testing Subtask:**
- [ ] **Test Counting Helpers**: Execute `@agent-e2e-runner` on `device-refresh-connection.cy.ts` to validate counting works correctly

**Workflow Execution:**
- [ ] **Code Implementation**: Use `@agent-clean-coder` to add counting helpers to existing interceptor files
- [ ] **Initial Testing**: Use `@agent-e2e-runner` to test affected functionality
- [ ] **Code Cleaning**: Use `@agent-code-cleaner` to clean the enhanced files
- [ ] **Final Testing**: Use `@agent-e2e-runner` to ensure no regressions from cleaning

**Key Implementation Notes:**
- New helpers follow the established pattern in existing interceptor files (see `connectDevice.interceptors.ts` structure)
- Counting helpers maintain state across multiple test steps using Cypress aliases or shared state
- Functions integrate seamlessly with existing device connection interceptors
- State management is isolated between different test scenarios
- Reset functionality works correctly for test isolation
- Add helpers to the "HELPER FUNCTIONS" section of each interceptor file

**Testing Focus for Task 2:**

> Focus on **API call tracking behavior** - counting must be accurate across test scenarios

**Behaviors to Test:**
- [ ] **Call Counting**: API calls are counted correctly during connection operations
- [ ] **State Management**: Count state persists across multiple test steps
- [ ] **Reset Functionality**: Count state resets properly between test scenarios
- [ ] **Integration with Existing Tests**: New helpers work with existing test patterns

**Testing Reference:**
- See [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) for behavioral testing patterns
- Use `@agent-e2e-runner` for comprehensive test validation

</details>

---

<details open>
<summary><h3>Task 3: Migrate device-connection.cy.ts Direct Intercepts</h3></summary>

**Purpose**: Replace 2 direct `cy.intercept()` instances in device-connection.cy.ts with the validation helper functions added to `connectDevice.interceptors.ts` and `disconnectDevice.interceptors.ts` in Task 1.

**Related Documentation:**
- [Direct Intercept Analysis](./INTERCEPTOR_PRIMITIVES_P2.md#phase-23-storage-domain-migration) - Analysis of remaining direct intercepts
- [Test Migration Patterns](./INTERCEPTOR_PRIMITIVES_PLAN.md#phase-3-direct-intercept-cleanup) - Direct intercept cleanup guidance
- [connectDevice Interceptor](../../../apps/teensyrom-ui-e2e/src/support/interceptors/connectDevice.interceptors.ts) - Enhanced validation helpers

**Implementation Subtasks:**
- [ ] **Subtask 3.1**: Analyze existing direct intercepts in `device-connection.cy.ts` (lines 89-106, 145-159)
- [ ] **Subtask 3.2**: Replace connection API validation intercept with `setupConnectDeviceWithValidation()` call from Task 1
- [ ] **Subtask 3.3**: Replace disconnection API validation intercept with `setupDisconnectDeviceWithValidation()` call from Task 1
- [ ] **Subtask 3.4**: Update imports to include new validation helper functions
- [ ] **Subtask 3.5**: Verify all existing test assertions and logic remain unchanged

**Testing Subtask:**
- [ ] **Test Migrated File**: Execute `@agent-e2e-runner` on `device-connection.cy.ts` to validate complete functionality

**Workflow Execution:**
- [ ] **Code Implementation**: Use `@agent-clean-coder` to replace direct intercepts with wrapper calls
- [ ] **Initial Testing**: Use `@agent-e2e-runner` to test the modified test file comprehensively
- [ ] **Code Cleaning**: Use `@agent-code-cleaner` to clean the modified test file
- [ ] **Final Testing**: Use `@agent-e2e-runner` to ensure no regressions from cleaning

**Key Implementation Notes:**
- All existing test assertions must pass without modification
- Test behavior must remain identical to before migration
- Import statements must be updated to include new interceptor functions
- Comments should be updated to reflect the new wrapper function usage

**Testing Focus for Task 3:**

> Focus on **complete behavioral preservation** - device connection tests must work identically

**Behaviors to Test:**
- [ ] **Connection Validation**: Connection API validation works with new wrapper functions
- [ ] **Disconnection Validation**: Disconnection API validation works with new wrapper functions
- [ ] **Test Assertions**: All existing test assertions continue to pass
- [ ] **No Regressions**: Test functionality is identical to before migration

**Testing Reference:**
- See [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) for behavioral testing patterns
- Use `@agent-e2e-runner` for comprehensive test validation

</details>

---

<details open>
<summary><h3>Task 4: Migrate device-refresh-error.cy.ts Direct Intercepts</h3></summary>

**Purpose**: Replace 6 direct `cy.intercept()` instances in device-refresh-error.cy.ts with enhanced error wrapper functions, maintaining complex error response validation.

**Related Documentation:**
- [Error Handling Patterns](./INTERCEPTOR_PRIMITIVES_PLAN.md#response-primitives) - Error response primitives
- [Complex Error Scenarios](./INTERCEPTOR_PRIMITIVES_P2.md#phase-23-storage-domain-migration) - Error handling from Phase 2

**Implementation Subtasks:**
- [ ] **Subtask 4.1**: Analyze existing error intercepts in `device-refresh-error.cy.ts` (lines 122-135, 140-146, 177-183, 264-270, 280-286, 303-309)
- [ ] **Subtask 4.2**: Create enhanced error wrapper functions for ProblemDetails response validation
- [ ] **Subtask 4.3**: Replace ProblemDetails error message extraction test with enhanced wrapper
- [ ] **Subtask 4.4**: Replace fallback error message handling with enhanced wrapper
- [ ] **Subtask 4.5**: Replace recovery scenario with custom enhanced wrapper
- [ ] **Subtask 4.6**: Replace network error simulation with enhanced wrapper using `forceNetworkError`
- [ ] **Subtask 4.7**: Replace bootstrap error handling with enhanced wrapper
- [ ] **Subtask 4.8**: Replace error recovery timing test with enhanced wrapper
- [ ] **Subtask 4.9**: Update imports to include new enhanced error wrapper functions

**Testing Subtask:**
- [ ] **Test Migrated File**: Execute `@agent-e2e-runner` on `device-refresh-error.cy.ts` to validate complete functionality

**Workflow Execution:**
- [ ] **Code Implementation**: Use `@agent-clean-coder` to replace direct intercepts with enhanced error wrappers
- [ ] **Initial Testing**: Use `@agent-e2e-runner` to test the modified test file comprehensively
- [ ] **Code Cleaning**: Use `@agent-code-cleaner` to clean the modified test file
- [ ] **Final Testing**: Use `@agent-e2e-runner` to ensure no regressions from cleaning

**Key Implementation Notes:**
- Enhanced error wrappers must maintain RFC 9110 compliance from Phase 2
- Network error simulation must continue to use `forceNetworkError: true`
- ProblemDetails response validation must be preserved exactly
- All timing scenarios and delays must work identically to before migration

**Testing Focus for Task 4:**

> Focus on **error scenario preservation** - complex error handling must work identically

**Behaviors to Test:**
- [ ] **ProblemDetails Validation**: Complex error response validation works with enhanced wrappers
- [ ] **Network Error Simulation**: Network errors are properly simulated with enhanced wrappers
- [ ] **Error Recovery**: Error recovery scenarios work identically with enhanced wrappers
- [ ] **Timing Scenarios**: All error timing and delay scenarios work correctly

**Testing Reference:**
- See [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) for behavioral testing patterns
- Use `@agent-e2e-runner` for comprehensive test validation

</details>

---

<details open>
<summary><h3>Task 5: Migrate device-refresh-connection.cy.ts Direct Intercepts</h3></summary>

**Purpose**: Replace 4 direct `cy.intercept()` instances in device-refresh-connection.cy.ts with wrapper functions, documenting 2 complex timing scenarios that should remain as direct intercepts.

**Related Documentation:**
- [Complex Timing Scenarios](./INTERCEPTOR_PRIMITIVES_PLAN.md#sequence-primitives) - Complex timing patterns
- [Direct Intercept Retention Criteria](./INTERCEPTOR_PRIMITIVES_PLAN.md#open-questions-for-phase-3) - When to retain direct intercepts

**Implementation Subtasks:**
- [ ] **Subtask 5.1**: Analyze existing direct intercepts in `device-refresh-connection.cy.ts` (lines 150-153, 209-212, 359-368, 419-427, 447-453)
- [ ] **Subtask 5.2**: Replace API call counting intercepts (lines 150-153, 209-212) with counting helpers from Task 2
- [ ] **Subtask 5.3**: Replace custom device ID validation in reconnection (lines 359-368) with validation helper from Task 1
- [ ] **Subtask 5.4**: Replace delayed connection handling (lines 419-427) with enhanced helper from Task 1
- [ ] **Subtask 5.5**: Replace delayed disconnection handling (lines 447-453) with enhanced helper from Task 1
- [ ] **Subtask 5.6**: Document any scenarios that must remain as direct intercepts due to complexity
- [ ] **Subtask 5.7**: Update imports to include new helper functions

**Testing Subtask:**
- [ ] **Test Migrated File**: Execute `@agent-e2e-runner` on `device-refresh-connection.cy.ts` to validate complete functionality

**Workflow Execution:**
- [ ] **Code Implementation**: Use `@agent-clean-coder` to replace direct intercepts with wrapper calls
- [ ] **Initial Testing**: Use `@agent-e2e-runner` to test the modified test file comprehensively
- [ ] **Code Cleaning**: Use `@agent-code-cleaner` to clean the modified test file
- [ ] **Final Testing**: Use `@agent-e2e-runner` to ensure no regressions from cleaning

**Key Implementation Notes:**
- API call counting must integrate seamlessly with counting helpers from Task 2 endpoint interceptors
- Complex timing scenarios may need to remain as direct intercepts with clear documentation
- Device ID validation should use validation helpers from Task 1 endpoint interceptors
- All refresh connection behavior must remain identical to before migration

**Testing Focus for Task 5:**

> Focus on **refresh behavior preservation** - complex refresh scenarios must work identically

**Behaviors to Test:**
- [ ] **API Call Counting**: Refresh connection API call counting works with counting utilities
- [ ] **Device ID Validation**: Refresh device ID validation works with validation wrappers
- [ ] **Delayed Operations**: Delayed connection/disconnection scenarios work correctly
- [ ] **Refresh Integration**: All refresh functionality works identically to before migration

**Testing Reference:**
- See [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) for behavioral testing patterns
- Use `@agent-e2e-runner` for comprehensive test validation

</details>

---

<details open>
<summary><h3>Task 6: Final Documentation and Standards Update</h3></summary>

**Purpose**: Update all documentation to reflect the new standardized patterns and complete the interceptor primitives architecture transformation.

**Related Documentation:**
- [Documentation Standards](./CODING_STANDARDS.md#documentation) - Documentation patterns
- [Architecture Completion Criteria](./INTERCEPTOR_PRIMITIVES_PLAN.md#success-criteria) - Final validation requirements

**Implementation Subtasks:**
- [ ] **Subtask 6.1**: Update inline documentation in all modified test files to reflect new wrapper functions
- [ ] **Subtask 6.2**: Update `INTERCEPTOR_PRIMITIVES.md` with new validation and counting wrapper functions
- [ ] **Subtask 6.3**: Create documentation explaining when direct intercepts are appropriate vs wrapper functions
- [ ] **Subtask 6.4**: Add examples of new wrapper functions to documentation
- [ ] **Subtask 6.5**: Update Phase 2 completion status in `INTERCEPTOR_PRIMITIVES_P2.md`

**Testing Subtask:**
- [ ] **Final Comprehensive Test**: Execute `@agent-e2e-runner` on complete E2E test suite to validate all changes

**Workflow Execution:**
- [ ] **Code Implementation**: Use `@agent-clean-coder` to update all documentation
- [ ] **Initial Testing**: Use `@agent-e2e-runner` to run comprehensive E2E test suite
- [ ] **Code Cleaning**: Use `@agent-code-cleaner` to clean all documentation files
- [ ] **Final Testing**: Use `@agent-e2e-runner` to run final comprehensive E2E test suite

**Key Implementation Notes:**
- Documentation must clearly explain the rationale for any remaining direct intercepts
- Examples should show both the new wrapper functions and their usage patterns
- All cross-references between documents must be updated
- Completion status must reflect the final architecture state

**Testing Focus for Task 6:**

> Focus on **complete system validation** - entire E2E test suite must work perfectly

**Behaviors to Test:**
- [ ] **Complete Suite**: All E2E tests pass with new wrapper functions
- [ ] **No Regressions**: No functionality is lost across the entire test suite
- [ ] **Performance**: Test execution time meets or exceeds baseline performance
- [ ] **Documentation**: All examples and documentation are accurate and functional

**Testing Reference:**
- See [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) for behavioral testing patterns
- Use `@agent-e2e-runner` for comprehensive test validation

</details>

---

## 🗂️ Files Modified or Created

> List all files that will be changed or created during this phase with full relative paths from project root.

**Modified Endpoint Interceptor Files:**
- `apps/teensyrom-ui-e2e/src/support/interceptors/connectDevice.interceptors.ts` - Add validation & counting helpers
- `apps/teensyrom-ui-e2e/src/support/interceptors/disconnectDevice.interceptors.ts` - Add validation & counting helpers

**Modified Test Files:**
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-connection.cy.ts`
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-connection.cy.ts`
- `apps/teensyrom-ui-e2e/src/e2e/devices/device-refresh-error.cy.ts`

**Updated Documentation Files:**
- `apps/teensyrom-ui-e2e/src/support/interceptors/primitives/INTERCEPTOR_PRIMITIVES.md` - Add documentation for new helpers
- `docs/features/e2e-testing/INTERCEPTOR_PRIMITIVES_P3.md` - This phase implementation plan

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are executed **within each task above** using the mandatory workflow, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
> - **Mandatory Workflow**: Each file change must follow clean-coder → e2e-runner → code-cleaner → e2e-runner cycle
> - **Test after each file change** - Never batch multiple file changes without testing
> - **Zero regression policy** - All existing tests must continue passing throughout entire Phase 3
> - **Behavioral testing** - Test observable outcomes, not implementation details
> - **Complete isolation** - Each file change is tested independently before proceeding

> **Reference Documentation:**
> - **All tasks**: [E2E Testing Standards](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Core behavioral testing approach
> - **Agent Usage**: [Agent Implementation Guide](./PHASE_TEMPLATE.md#agent-implementation-guide) - Mandatory workflow execution

### Where Tests Are Executed

**Tests are embedded in each task above** with:
- **Workflow Execution**: Mandatory 4-step cycle for each file change
- **Testing Subtask**: Checkbox in the task's subtask list (e.g., "Test Migrated File")
- **Testing Focus**: "Behaviors to Test" section listing observable outcomes
- **Agent Integration**: Use of `@agent-clean-coder`, `@agent-e2e-runner`, and `@agent-code-cleaner`

**Complete each task's workflow execution before moving to the next task.**

### Test Execution Commands

**Agent-Based Testing:**
```bash
# Mandatory workflow for each file change:
1. @agent-clean-coder - Implement code changes
2. @agent-e2e-runner - Test affected functionality
3. @agent-code-cleaner - Clean and optimize code
4. @agent-e2e-runner - Final validation testing
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All 5 implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] 10 of 12 direct `cy.intercept()` instances eliminated (~83% reduction)
- [ ] Code follows [Coding Standards](./CODING_STANDARDS.md)
- [ ] Maintains "1 interceptor per endpoint" architecture pattern
- [ ] New helpers added to existing endpoint interceptor files (connectDevice, disconnectDevice)

**Testing Requirements:**
- [ ] All workflow execution steps completed for each file change
- [ ] All behavioral test checkboxes verified through agent execution
- [ ] All tests passing with no failures across entire E2E test suite
- [ ] Zero regressions from Phase 2 completion state
- [ ] Performance meets or exceeds baseline execution times

**Quality Checks:**
- [ ] No TypeScript errors or warnings in any modified files
- [ ] Linting passes with no errors for all modified files (`npm run lint`)
- [ ] Code formatting is consistent across all modified files
- [ ] All agent workflow steps completed successfully
- [ ] Documentation is accurate and up-to-date

**Architecture Requirements:**
- [ ] 100% consistency in interceptor usage patterns across all test files
- [ ] All new wrapper functions use primitive-based architecture
- [ ] RFC 9110 compliant error handling maintained across all scenarios
- [ ] Clear documentation for any remaining direct intercept scenarios
- [ ] Complete primitive-based architecture foundation established

**Documentation:**
- [ ] Inline code comments added for new wrapper functions
- [ ] Public API methods documented with JSDoc for new interceptor functions
- [ ] INTERCEPTOR_PRIMITIVES.md updated with new wrapper functions
- [ ] Phase completion status accurately reflected in all documentation

**Ready for Production:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Complete interceptor primitives architecture transformation
- [ ] Foundation established for future testing capabilities

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **One Interceptor Per Endpoint Pattern**: Maintain established architecture where each endpoint has a single interceptor file containing all wrapper functions for that endpoint
- **Selective Migration Strategy**: Chose to eliminate ~83% of direct intercepts while documenting complex scenarios that should remain direct, avoiding over-engineering for unique test cases
- **Enhanced Helper Functions**: Add specialized helper functions to existing endpoint interceptors (connectDevice, disconnectDevice) for validation and counting patterns
- **Mandatory Workflow Policy**: Established strict clean-coder → e2e-runner → code-cleaner → e2e-runner cycle to ensure perfect code quality throughout the phase

### Implementation Constraints

- **Zero Breaking Changes**: Cannot modify any existing test assertions or expected behaviors
- **One-File-at-a-Time Policy**: Never modify multiple files in the same task without testing
- **Complex Scenario Preservation**: Some timing-sensitive scenarios may need to remain as direct intercepts
- **Backward Compatibility**: All existing test patterns must continue working without modification

### Future Enhancements

- **Advanced Timing Utilities**: Future development could create timing primitives for the remaining 2 complex direct intercepts
- **Behavior Validation Framework**: The counting utilities established in this phase could be expanded for more complex behavior validation
- **Cross-Domain Patterns**: The validation patterns created could be extended to other domains beyond device connections

### External References

- [Phase 2 Completion Analysis](./INTERCEPTOR_PRIMITIVES_P2.md#current-results-summary) - Foundation for Phase 3 scope
- [Primitive Library Documentation](../../../apps/teensyrom-ui-e2e/src/support/interceptors/primitives/INTERCEPTOR_PRIMITIVES.md) - Core primitive functions
- [E2E Test Infrastructure](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Testing standards and patterns

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery 1**: [Workflow execution details and any issues encountered]
- **Discovery 2**: [Complexity assessment of direct intercept scenarios]
- **Discovery 3**: [Integration patterns between new wrapper functions and existing primitives]

</details>

---

## 💡 Agent Implementation Guide

> **Instructions for AI agents implementing this phase**

### Before Starting Implementation

**Clarifying Questions to Consider:**

1. **Workflow Execution**:
   - What are the specific failure criteria for each agent in the workflow?
   - How should workflow failures be documented and resolved?
   - What defines "perfect" code quality for each agent step?

2. **Implementation Strategy**:
   - Which direct intercept scenarios are too complex to migrate and should be documented as exceptions?
   - How should new wrapper functions be integrated with existing primitive functions?
   - What are the performance implications of the new wrapper functions?

3. **Success Definition**:
   - What specific test results define successful completion of each task?
   - How should regressions be identified and resolved during workflow execution?
   - What defines the optimal balance between migration completeness and complexity?

### During Implementation

**Workflow Execution Guidelines:**

1. **Follow Mandatory Workflow Exactly**:
   - **Step 1**: Use `@agent-clean-coder` to implement code changes
   - **Step 2**: Use `@agent-e2e-runner` to test affected functionality
   - **Step 3**: Use `@agent-code-cleaner` to clean and optimize code
   - **Step 4**: Use `@agent-e2e-runner` again for final validation
   - **Repeat**: If any step fails, repeat the cycle until perfect

2. **Test-Isolation Policy**:
   - Test after each individual file change
   - Never proceed to next task until current task passes all workflow steps
   - Document any issues discovered during agent execution
   - Resolve all failures before moving forward

3. **Quality Assurance**:
   - Verify all existing test assertions pass without modification
   - Ensure new wrapper functions follow established patterns from Phase 1-2
   - Maintain RFC 9110 compliance for all error scenarios
   - Document any deviations from expected behavior

### Progress Tracking

1. ✅ **Mark Checkboxes**: Check off each subtask as you complete it
2. 📝 **Document Workflow Results**: Record results from each agent execution
3. 🚧 **Track Workflow Issues**: Document any workflow failures and resolutions
4. 📊 **Update Success Criteria**: Keep validation criteria current as work progresses

### Testing Integration

1. **Behavioral Focus**: Test observable outcomes, not implementation details
2. **Agent Coordination**: Use each agent for its specialized purpose
3. **Comprehensive Validation**: Ensure complete functionality preservation
4. **Performance Monitoring**: Compare against baseline execution times

### Remember

- **This completes the interceptor primitives architecture** - Phase 3 is the final transformation
- **Workflow compliance is mandatory** - No exceptions to the 4-step cycle
- **Zero regression policy** - All existing functionality must be preserved
- **Documentation is critical** - Remaining direct intercepts must be clearly justified

---

## 🎓 Examples of Agent Workflow Execution

### ❌ Bad (Skipping Workflow Steps)

```markdown
**Task**: Replace direct intercepts
- [ ] Implement all changes in one step
- [ ] Test everything at the end
```

### ✅ Good (Following Mandatory Workflow)

```markdown
**Task**: Migrate device-connection.cy.ts Direct Intercepts
- [ ] **Code Implementation**: Use `@agent-clean-coder` to replace direct intercepts with wrapper calls
- [ ] **Initial Testing**: Use `@agent-e2e-runner` to test the modified test file comprehensively
- [ ] **Code Cleaning**: Use `@agent-code-cleaner` to clean the modified test file
- [ ] **Final Testing**: Use `@agent-e2e-runner` to ensure no regressions from cleaning
- [ ] **Test Migrated File**: Execute `@agent-e2e-runner` on `device-connection.cy.ts` to validate complete functionality

**Behaviors to Test:**
- [ ] **Connection Validation**: Connection API validation works with new wrapper functions
- [ ] **Disconnection Validation**: Disconnection API validation works with new wrapper functions
- [ ] **Test Assertions**: All existing test assertions continue to pass
- [ ] **No Regressions**: Test functionality is identical to before migration
```