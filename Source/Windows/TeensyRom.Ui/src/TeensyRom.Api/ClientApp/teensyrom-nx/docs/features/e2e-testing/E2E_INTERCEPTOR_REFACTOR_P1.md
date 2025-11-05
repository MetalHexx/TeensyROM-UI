# Phase 1: Foundation & Infrastructure

## 🎯 Objective

Establish the architectural foundation, naming conventions, and validation framework that will guide the entire refactoring process. This phase creates the systematic approach and tooling necessary for successful per-endpoint migration while ensuring we can validate each step and prevent regressions.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**

- [ ] [E2E Interceptor Refactoring Plan](./E2E_INTERCEPTOR_REFACTOR_PLAN.md) - High-level feature plan
- [ ] [E2E Testing Overview](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Current E2E testing infrastructure

---

## 📂 File Structure Overview

> Provide a clear file tree showing new files (✨) and modified files (📝) to understand the implementation scope.

```
docs/features/e2e-testing/
├── INTERCEPTOR_FORMAT.md                  ✨ New - Format documentation and guidelines
├── E2E_INTERCEPTOR_REFACTOR_P1.md         ✨ New - Phase 1 implementation plan
└── E2E_INTERCEPTOR_REFACTOR_PLAN.md       📝 Referenced - Main refactoring plan

apps/teensyrom-ui-e2e/src/support/interceptors/
├── examples/                              ✨ New - Example directory for Phase 1
│   └── sampleEndpoint.interceptors.ts     ✨ New - Generic working example following format
├── device.interceptors.ts                 📝 Referenced - Current device interceptors
├── storage.interceptors.ts                📝 Referenced - Current storage interceptors
├── player.interceptors.ts                 📝 Referenced - Current player interceptors
└── storage-indexing.interceptors.ts       📝 Referenced - Current indexing interceptors

apps/teensyrom-ui-e2e/src/support/constants/
└── api.constants.ts                       📝 Referenced - Current API constants and aliases
```

<details open>
<summary><h3>Task 1: Create Format Documentation</h3></summary>

**Purpose**: Establish comprehensive documentation that defines the standard structure and patterns for all per-endpoint interceptor files.

**Related Documentation:**

- [E2E Interceptor Refactoring Plan - Phase 1](./E2E_INTERCEPTOR_REFACTOR_PLAN.md#phase-1-foundation--infrastructure) - Context for format requirements

**Implementation Subtasks:**

- [x] **Create INTERCEPTOR_FORMAT.md**: New file with comprehensive format guidelines
- [x] **Define File Naming Convention**: Specify `{endpointName}.interceptors.ts` pattern
- [x] **Document 6-Section Structure**: Define endpoint definition, interfaces, interceptors, waits, helpers, exports
- [x] **Establish Import Patterns**: Define explicit import conventions and examples
- [x] **Add Type Definition Guidelines**: Document interface patterns for options and responses
- [x] **Create Error Handling Patterns**: Define consistent error simulation approaches
- [x] **Add Migration Guidelines**: Document how to transition from current scattered approach

**Key Implementation Notes:**

- Format must be comprehensive enough for any developer to create endpoint files without additional guidance
- Include concrete examples for each section while keeping documentation focused on patterns
- Ensure alignment with existing E2E testing standards and conventions
- Document backward compatibility requirements during migration

**Critical Type Definition:**

```typescript
// Endpoint definition pattern
const ENDPOINT_NAME = {
  method: 'HTTP_METHOD',
  path: '/api/path',
  full: 'http://localhost:5168/api/path',
  pattern: 'http://localhost:5168/api/path*',
  alias: 'endpointAlias',
} as const;
```

</details>

---

<details open>
<summary><h3>Task 2: Create Working Example File</h3></summary>

**Purpose**: Create a complete, working example that demonstrates the format documentation in practice using a generic endpoint pattern.

**Related Documentation:**

- [INTERCEPTOR_FORMAT.md](./INTERCEPTOR_FORMAT.md) - Format to follow exactly
- [Current device.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/device.interceptors.ts) - Reference existing implementation patterns

**Implementation Subtasks:**

- [x] **Create examples directory**: New directory structure for Phase 1 examples
- [x] **Create sampleEndpoint.interceptors.ts**: Complete example file following format documentation
- [x] **Implement Endpoint Definition**: Create SAMPLE_ENDPOINT constant demonstrating pattern
- [x] **Create Interface Definition**: Define InterceptSampleEndpointOptions interface
- [x] **Implement Interceptor Function**: Create interceptSampleEndpoint() function with full functionality
- [x] **Create Wait Function**: Implement waitForSampleEndpoint() using endpoint alias
- [x] **Add Helper Functions**: Include example helpers like verifySampleEndpointCompleted()
- [x] **Add Backward Compatibility**: Export patterns that work with existing imports

**Key Implementation Notes:**

- Example must be fully compilable TypeScript with no errors
- Follow the 6-section structure exactly as defined in format documentation
- Include inline comments explaining each section's purpose
- Demonstrate both success and error handling patterns
- Use generic patterns that can be applied to any endpoint type

**Critical Interface:**

```typescript
interface InterceptSampleEndpointOptions {
  fixture?: MockResponseData | MockResponseData[];
  errorMode?: boolean;
  responseDelayMs?: number;
  statusCode?: number;
}
```

</details>

---

<details open>
<summary><h3>Task 3: Validate Format and Example Alignment</h3></summary>

**Purpose**: Ensure the example file perfectly demonstrates the format documentation and that both deliverables are fully aligned.

**Related Documentation:**

- [INTERCEPTOR_FORMAT.md](./INTERCEPTOR_FORMAT.md) - Format documentation to validate against
- [sampleEndpoint.interceptors.ts](../../../apps/teensyrom-ui-e2e/src/support/interceptors/examples/sampleEndpoint.interceptors.ts) - Example to validate

**Implementation Subtasks:**

- [x] **Cross-Reference Format Sections**: Verify each format guideline is demonstrated in example
- [x] **Validate TypeScript Compilation**: Ensure example file compiles without errors
- [x] **Test Import Patterns**: Verify example imports work as documented
- [x] **Check Naming Convention**: Validate file and artifact naming follows format standards
- [x] **Verify Backward Compatibility**: Test that example exports maintain existing functionality
- [x] **Document Any Gaps**: Identify and resolve any misalignment between format and example

**Key Implementation Notes:**

- Format documentation and example must be perfectly consistent
- Any contradictions should be resolved by updating both documents
- Example should demonstrate best practices for all documented patterns
- Validation should include both structural and functional consistency

**Testing Focus for Task 3:**

**Behaviors to Validate:**

- [ ] Example file compiles successfully with TypeScript
- [ ] All documented patterns are demonstrated in example
- [ ] Import patterns work exactly as documented
- [ ] Backward compatibility exports function correctly
- [ ] Error handling follows documented patterns

</details>

---

## 🗂️ Files Modified or Created

> List all files that will be changed or created during this phase with full relative paths from project root.

**New Files:**

- `docs/features/e2e-testing/INTERCEPTOR_FORMAT.md`
- `docs/features/e2e-testing/E2E_INTERCEPTOR_REFACTOR_P1.md`
- `apps/teensyrom-ui-e2e/src/support/interceptors/examples/sampleEndpoint.interceptors.ts`

**Referenced Files (Not Modified):**

- `docs/features/e2e-testing/E2E_INTERCEPTOR_REFACTOR_PLAN.md`
- `apps/teensyrom-ui-e2e/src/support/interceptors/device.interceptors.ts`
- `apps/teensyrom-ui-e2e/src/support/constants/api.constants.ts`

---

## ✅ Success Criteria

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Documentation Requirements:**

- [x] INTERCEPTOR_FORMAT.md created with comprehensive format guidelines
- [x] All 6 sections of file structure clearly documented
- [x] Import patterns and conventions explicitly defined
- [x] Migration guidelines for transitioning from current approach included

**Example Implementation:**

- [x] sampleEndpoint.interceptors.ts created following format documentation exactly
- [x] Example file compiles without TypeScript errors
- [x] All sections from format documentation demonstrated in example
- [x] Backward compatibility exports included and functional

**Alignment and Validation:**

- [x] Format documentation and example file are perfectly aligned
- [x] No contradictions between documentation and example
- [x] All documented patterns are practically demonstrated
- [x] Example serves as clear reference for future endpoint migrations

**Quality Checks:**

- [x] No TypeScript errors in example file
- [x] Documentation follows established writing standards
- [x] File naming conventions are consistent
- [x] Import patterns are clear and unambiguous

**Ready for Next Phase:**

- [x] All success criteria met
- [x] Format documentation can guide any endpoint migration
- [x] Example file serves as working template for Phase 2
- [x] Foundation established for systematic endpoint migration

---

## 📝 Notes & Considerations

### Design Decisions

- **Format Documentation First**: Creating comprehensive format guidelines before any actual endpoint migration ensures consistent patterns across all future work
- **Working Example as Proof**: The findDevices example validates that the documented format is practical and implementable
- **Backward Compatibility Focus**: Ensuring existing functionality continues working during the migration process

### Implementation Constraints

- **No Existing Code Changes**: Phase 1 focuses on documentation and example creation only, no modifications to existing interceptor files
- **Template-Driven Approach**: All future endpoint migrations should follow the format established in this phase
- **Explicit Import Pattern**: Avoiding barrel exports to maintain clear dependency visibility

### Future Enhancements

- **Validation Scripts**: Future phases could include automated validation scripts that check new endpoint files against the format documentation
- **Migration Tools**: Potential for automated helper scripts that use the format to assist with endpoint migrations
- **Additional Examples**: More complex endpoint examples could be added to demonstrate advanced patterns

### External References

- [E2E Testing Documentation](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Current E2E infrastructure patterns
- [Phase Template](../../PHASE_TEMPLATE.md) - Standard structure for phase implementation documents
