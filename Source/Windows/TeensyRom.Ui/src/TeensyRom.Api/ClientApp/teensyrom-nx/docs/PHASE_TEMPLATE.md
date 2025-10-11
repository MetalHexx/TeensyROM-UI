# Phase N: [Descriptive Phase Title]

## 🎯 Objective

[1-2 sentence description of what this phase delivers and why it's valuable. Focus on the user-facing outcome or technical capability being added.]

**Example:**
> Implement the core infrastructure for tracking and managing user activity history, enabling navigation through previously accessed content with browser-style forward/backward controls.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [Feature Planning Document](./RELATIVE_LINK_TO_PLANNING.md) - High-level feature plan
- [ ] [Domain Design Document](./RELATIVE_LINK_TO_DESIGN.md) - Technical design and architecture (if applicable)
- [ ] [Related Feature Documentation](./RELATIVE_LINK.md) - Any related feature context

**Standards & Guidelines:**
- [ ] [Coding Standards](./CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](./TESTING_STANDARDS.md) - Testing approaches and best practices
- [ ] [State Standards](./STATE_STANDARDS.md) - **ONLY if this phase modifies or creates store state**
- [ ] [Store Testing Guide](./STORE_TESTING.md) - **ONLY if this phase includes store testing**
- [ ] [Smart Component Testing](./SMART_COMPONENT_TESTING.md) - **ONLY if testing smart components**
- [ ] [Style Guide](./STYLE_GUIDE.md) - **ONLY if this phase includes components or styling**
- [ ] [API Client Generation](./API_CLIENT_GENERATION.md) - **ONLY if this phase requires API client regeneration**

---

## 📂 File Structure Overview

> Provide a clear file tree showing new files (✨) and modified files (📝) to understand the implementation scope.

**Example:**
```
libs/application/src/lib/example-domain/
├── domain-store.ts                          📝 Modified - Add new state properties
├── actions/
│   ├── index.ts                             📝 Modified - Export new actions
│   ├── perform-operation.ts                 ✨ New - Primary operation action
│   ├── update-settings.ts                   ✨ New - Settings update action
│   └── cleanup-resources.ts                 ✨ New - Cleanup action
├── selectors/
│   ├── index.ts                             📝 Modified - Export new selectors
│   ├── get-active-items.ts                  ✨ New - Active items selector
│   └── get-operation-state.ts               ✨ New - Operation state selector
└── helpers/
    └── domain-helpers.ts                    📝 Modified - Add helper functions

libs/features/example-feature/
├── components/
│   ├── feature-container.component.ts       ✨ New - Main feature component
│   ├── feature-container.component.html     ✨ New - Component template
│   └── feature-container.component.scss     ✨ New - Component styles
```

---

## 📋 Implementation Guidelines
_These guardrails are for how to create the doc, but should not actually be included in the final doc._
> **IMPORTANT - Code Reference Policy:**
> - Focus on **WHAT** to implement, not **HOW** to implement it
> - Use **class names**, **method names**, **interface names**, **property names**
> - Small code snippets (2-5 lines) are OK for critical type definitions or state structures only
> - **NO large code blocks** - link to standards docs or existing implementations instead
> - Prefer describing behavior over showing implementation
> - Cross-reference relevant documentation for detailed context

> **IMPORTANT - Testing Policy:**
> - **Favor behavioral testing** - test observable behaviors, not implementation details
> - Include tests **within each task** as work progresses, not at the end
> - Each task should have its own testing subtask
> - See [Testing Standards](./TESTING_STANDARDS.md) for behavioral testing guidance
> - Reference specialized testing docs only when relevant:
>   - [Store Testing](./STORE_TESTING.md) for store state testing
>   - [Smart Component Testing](./SMART_COMPONENT_TESTING.md) for component testing

> **IMPORTANT - Progress Tracking:**
> - **Mark checkboxes ✅ as you complete each subtask**
> - Update progress throughout implementation, not just at the end
> - This helps track what's done and what remains

---

<details open>
<parameter name="summary"><h3>Task 1: [High-Level Task Name]</h3></summary>

**Purpose**: [1-2 sentences explaining why this task is needed and what it accomplishes]

**Related Documentation:**
- [Link to relevant planning section](./RELATIVE_LINK.md#section) - Context for this task
- [Link to similar implementation](./RELATIVE_LINK.md#section) - Reference pattern to follow

**Implementation Subtasks:**
- [ ] **Subtask A**: [Specific action with artifact name - e.g., "Add `propertyName` to `InterfaceName` interface"]
- [ ] **Subtask B**: [Specific action - e.g., "Create `methodName` action in new file"]
- [ ] **Subtask C**: [Specific action - e.g., "Update `helperFunction` to handle new parameter"]
- [ ] **Subtask D**: [Add more as needed]

**Testing Subtask:**
- [ ] **Write Tests**: Test behaviors for this task (see Testing section below for details)

**Key Implementation Notes:**
- Important consideration or constraint for this task
- Integration point with existing code to be aware of
- Potential edge case to handle

**Critical Type/Interface** (only if essential):
```typescript
// Small snippet showing critical structure only (2-5 lines max)
interface ExampleStructure {
  key: string;
  value: SomeType;
}
```

**Testing Focus for Task 1:**

> Focus on **behavioral testing** - what observable outcomes occur?

**Behaviors to Test:**
- [ ] **Behavior A**: [Observable outcome - e.g., "State initializes with default values"]
- [ ] **Behavior B**: [Observable outcome - e.g., "Method updates state correctly on success"]
- [ ] **Behavior C**: [Observable outcome - e.g., "Error state is set when operation fails"]

**Testing Reference:**
- See [Testing Standards](./TESTING_STANDARDS.md) for behavioral testing patterns
- **IF testing store state**: See [Store Testing](./STORE_TESTING.md)
- **IF testing components**: See [Smart Component Testing](./SMART_COMPONENT_TESTING.md)

</details>

---

<details open>
<summary><h3>Task 2: [High-Level Task Name]</h3></summary>

**Purpose**: [1-2 sentences explaining why this task is needed]

**Related Documentation:**
- [Link to relevant section](./RELATIVE_LINK.md#section)

**Implementation Subtasks:**
- [ ] **Subtask A**: [Specific action]
- [ ] **Subtask B**: [Specific action]
- [ ] **Subtask C**: [Specific action]

**Testing Subtask:**
- [ ] **Write Tests**: Test behaviors for this task (see Testing section below)

**Key Implementation Notes:**
- Important consideration or pattern to follow

**Testing Focus for Task 2:**

**Behaviors to Test:**
- [ ] **Behavior A**: [Observable outcome]
- [ ] **Behavior B**: [Observable outcome]

**Testing Reference:**
- See [Testing Standards](./TESTING_STANDARDS.md) for testing approach

</details>

---

<details open>
<summary><h3>Task 3: [High-Level Task Name]</h3></summary>

**Purpose**: [Why this task is needed]

**Related Documentation:**
- [Link to relevant section](./RELATIVE_LINK.md#section)

**Implementation Subtasks:**
- [ ] **TypeScript Changes**: [What TS changes are needed - class/method/property names]
- [ ] **Template Changes**: [What HTML/template changes are needed - structural/binding changes]
- [ ] **Styling Changes**: [What CSS/SCSS changes are needed - classes/selectors to add]

**Testing Subtask:**
- [ ] **Write Tests**: Test behaviors for this task (see Testing section below)

**Key Implementation Notes:**
- Styling considerations or component patterns to use

**Testing Focus for Task 3:**

**Behaviors to Test:**
- [ ] **Behavior A**: [Observable UI outcome]
- [ ] **Behavior B**: [Observable interaction outcome]

**Testing Reference:**
- See [Smart Component Testing](./SMART_COMPONENT_TESTING.md) **ONLY IF** testing smart components
- See [Testing Standards](./TESTING_STANDARDS.md) for general testing guidance

</details>

---

## 🗂️ Files Modified or Created

> List all files that will be changed or created during this phase with full relative paths from project root.

**New Files:**
- `libs/application/src/lib/domain/actions/new-action.ts`
- `libs/application/src/lib/domain/selectors/new-selector.ts`
- `libs/features/feature-name/components/new-component.ts`
- `libs/features/feature-name/components/new-component.html`

**Modified Files:**
- `libs/application/src/lib/domain/domain-store.ts`
- `libs/application/src/lib/domain/actions/index.ts`
- `libs/application/src/lib/domain/selectors/index.ts`

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

> **Core Testing Philosophy:**
> - **Favor behavioral testing** - test what users/consumers observe, not how it's implemented
> - **Test as you go** - tests are integrated into each task's subtasks, not deferred to the end
> - **Test through public APIs** - components, stores, services should be tested through their public interfaces
> - **Mock at boundaries** - mock external dependencies (HTTP, infrastructure services), not internal logic

> **Reference Documentation:**
> - **All tasks**: [Testing Standards](./TESTING_STANDARDS.md) - Core behavioral testing approach
> - **IF testing stores**: [Store Testing](./STORE_TESTING.md) - Store-specific testing patterns
> - **IF testing smart components**: [Smart Component Testing](./SMART_COMPONENT_TESTING.md) - Component testing patterns

### Where Tests Are Written

**Tests are embedded in each task above** with:
- **Testing Subtask**: Checkbox in the task's subtask list (e.g., "Write Tests: Test behaviors for this task")
- **Testing Focus**: "Behaviors to Test" section listing observable outcomes
- **Testing Reference**: Links to relevant testing documentation

**Complete each task's testing subtask before moving to the next task.**

### Test Execution Commands

**Running Tests:**
```bash
# Run tests for specific library
npx nx test [library-name]

# Run tests in watch mode during development
npx nx test [library-name] --watch

# Run all tests
npx nx run-many --target=test --all
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Code follows [Coding Standards](./CODING_STANDARDS.md)
- [ ] State management follows [State Standards](./STATE_STANDARDS.md) **(if applicable)**

**Testing Requirements:**
- [ ] All testing subtasks completed within each task
- [ ] All behavioral test checkboxes verified
- [ ] Tests written alongside implementation (not deferred)
- [ ] All tests passing with no failures
- [ ] Test coverage meets or exceeds project standards

**Quality Checks:**
- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors (`npm run lint`)
- [ ] Code formatting is consistent
- [ ] No console errors in browser/terminal when running application

**Documentation:**
- [ ] Inline code comments added for complex logic
- [ ] Public API methods documented with JSDoc **(if creating new public APIs)**
- [ ] README or related docs updated **(if needed)**

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Code reviewed and approved **(if applicable)**
- [ ] Ready to proceed to next phase

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Decision 1**: [Explanation of why this approach was chosen over alternatives]
- **Decision 2**: [Rationale for implementation choice and trade-offs considered]

### Implementation Constraints

- **Constraint 1**: [Technical or business constraint affecting this phase]
- **Constraint 2**: [Known limitation to be addressed in future phases]

### Future Enhancements

- **Enhancement 1**: [Potential improvement to consider later]
- **Enhancement 2**: [Feature that could build on this phase's foundation]

### External References

- [Link to mockup/design](URL) - Visual design reference
- [Link to related issue](URL) - GitHub issue or ticket number
- [Link to discussion](URL) - Team discussion or decision documentation

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

- **Discovery 1**: [Something learned during implementation that affects approach]
- **Discovery 2**: [Unexpected complexity or simplification found]

</details>

---

## 💡 Agent Implementation Guide

> **Instructions for AI agents creating and using this document**

### Before Creating This Document

**Ask Clarifying Questions:**

1. **Testing Strategy**:
   - What are the critical behaviors that need testing?
   - What edge cases should be covered?
   - Are there specific error scenarios to test?
   - What integration points are most critical?

2. **Implementation Approach**:
   - What are the key integration points with existing code?
   - Are there any performance considerations?
   - What are the most complex parts of this phase?
   - Are there any accessibility requirements?

3. **Success Definition**:
   - What defines "done" for this phase?
   - What must be demonstrated to validate completion?
   - Are there any user acceptance criteria?

### While Creating This Document

**Content Guidelines:**

1. **Keep Code References Minimal**:
   - Favor class/method/interface names over code blocks
   - Use small snippets (2-5 lines) only for critical structures
   - Link to standards docs for detailed examples
   - Describe **what** to build, not **how** to build it

2. **Embed Testing in Tasks**:
   - Each task must include a testing subtask
   - Describe observable behaviors to test, not implementation
   - Reference specialized testing docs only when relevant
   - Focus on what users/consumers will observe

3. **Be Specific in Subtasks**:
   - Each checkbox is a clear, actionable item
   - Include artifact names (class, method, property, file)
   - Avoid vague descriptions like "implement feature"
   - Make subtasks independently completable

4. **Cross-Reference Documentation**:
   - Link to relevant sections of planning docs
   - Reference similar implementations as examples
   - Point to standards for patterns to follow
   - Only link to specialized docs when actually needed

### During Implementation

**Progress Tracking:**
1. ✅ **Mark Checkboxes**: Check off each item as you complete it
2. 📝 **Update Notes**: Add discoveries or decisions made during implementation
3. 🚧 **Track Blockers**: Document any blockers or questions that arise
4. 📊 **Update Status**: Keep success criteria current as work progresses

**Testing Integration:**
1. **Test as you go**: Complete each task's testing subtask before moving on
2. **Behavioral focus**: Test observable outcomes, not implementation details
3. **Public API**: Test through public interfaces (methods, signals, outputs)
4. **Reference docs**: Use specialized testing docs only when needed for that task type

### After Completing a Task

1. Verify all subtasks are checked off
2. Ensure testing subtask is complete
3. Confirm all behavioral tests pass
4. Update any relevant notes or discoveries
5. Mark task as complete in document

### Remember

- This is a **living document** - update it as you learn during implementation
- **Testing happens during development**, not after
- **Behavioral tests** tell you what changed for users, not how code works internally
- **Mark progress incrementally** - don't wait until the end
- **Ask questions early** if you're unsure about approach or priorities

---

## 🎓 Examples of Good vs Bad Task Descriptions

### ❌ Bad (Too Technical, Too Detailed)

```markdown
**Task**: Implement history tracking
- [ ] Add `private historyEntries: HistoryEntry[] = []` to PlayerStore
- [ ] Create recordHistory method:
      ```typescript
      recordHistory(file: FileItem) {
        this.historyEntries.push({
          file,
          timestamp: Date.now(),
        });
      }
      ```
```

### ✅ Good (What, Not How)

```markdown
**Task**: Implement history tracking
- [ ] Add `historyEntries` property to state interface
- [ ] Create `recordHistory` action that adds entries with timestamps
- [ ] Create `clearHistory` action that removes all entries
- [ ] **Write Tests**: Verify history records entries and clears correctly
```

### ❌ Bad (Missing Testing, Vague)

```markdown
**Task**: Add navigation
- [ ] Implement navigation features
- [ ] Make it work with history
```

### ✅ Good (Specific, Includes Testing)

```markdown
**Task**: Add backward/forward navigation
- [ ] Create `navigateBackward` action in actions folder
- [ ] Create `navigateForward` action in actions folder
- [ ] Add `canNavigateBackward` selector
- [ ] Add `canNavigateForward` selector
- [ ] **Write Tests**: Verify navigation moves through history correctly and handles boundaries

**Behaviors to Test:**
- [ ] `navigateBackward` moves to previous entry when available
- [ ] `navigateBackward` does nothing when at start
- [ ] `navigateForward` moves to next entry when available
- [ ] Selectors correctly report navigation availability
```
