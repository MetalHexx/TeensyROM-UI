# Phase 3: URL Update on File Launch

## 🎯 Objective

Update the browser URL when files are launched through `PlayerContextService`, enabling users to share links to currently playing files and maintain accurate browser history for back/forward navigation.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [x] [Deep Linking Plan](./DEEP_LINKING_PLAN.md) - High-level feature plan and architecture
- [x] [Player Context Service](../../../libs/application/src/lib/player/player-context.service.ts) - Service being modified

**Standards & Guidelines:**
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices

---

## 📂 File Structure Overview

> Only one file is modified in this phase - simple URL synchronization logic.

```
libs/application/src/lib/player/
├── player-context.service.ts                📝 Modified - Add Location service and URL updates
└── player-context.service.spec.ts           ⏭️ No changes - existing tests validate behavior
```

---

<details open>
<summary><h3>Task 1: Add Angular Location Service</h3></summary>

**Purpose**: Inject Angular's `Location` service to enable URL updates without triggering route navigation.

**Related Documentation:**
- [Angular Location API](https://angular.io/api/common/Location) - Official Angular documentation

**Implementation Subtasks:**
- [ ] **Import Location**: Add `Location` import from `@angular/common` at top of file
- [ ] **Inject Service**: Add `private readonly location = inject(Location)` to service class

**Testing Subtask:**
- [ ] **No Tests Required**: Simple dependency injection, validated by integration tests

**Key Implementation Notes:**
- Add after existing service injections (`PlayerStore`, `StorageStore`, `PlayerTimerManager`)
- Use readonly to prevent reassignment
- Use inject() function pattern (matches existing code style)

</details>

---

<details open>
<summary><h3>Task 2: Create URL Update Helper Method</h3></summary>

**Purpose**: Centralize URL update logic in a reusable private method that extracts current file state and updates the browser URL.

**Related Documentation:**
- [Deep Linking Plan - Phase 3](./DEEP_LINKING_PLAN.md#phase-3-url-update-on-file-launch) - Requirements and design decisions

**Implementation Subtasks:**
- [ ] **Create Method**: Add private method `updateUrlForLaunchedFile(deviceId: string): void`
- [ ] **Extract File Info**: Get current file from `store.getCurrentFile(deviceId)()`
- [ ] **Parse Storage Key**: Use `StorageKeyUtil.parse()` to extract `deviceId` and `storageType`
- [ ] **Build Query Params**: Create query string with `device`, `storage`, `path`, `file` parameters
- [ ] **Update URL**: Call `location.go('/player', queryString)` to update URL without navigation

**Testing Subtask:**
- [ ] **No Tests Required**: Simple utility method, validated by E2E tests in Phase 4

**Key Implementation Notes:**
- Return early if `currentFile` is null (nothing to update)
- Convert `StorageType` enum to string: `StorageType.Sd` → `'SD'`, `StorageType.Usb` → `'USB'`
- Use `currentFile.parentPath` for the `path` parameter
- Use `currentFile.file.name` for the `file` parameter
- Angular automatically handles URL encoding in query parameters
- Use `location.go()` not `router.navigate()` to avoid re-triggering resolver

**Critical Method Signature**:
```typescript
private updateUrlForLaunchedFile(deviceId: string): void {
  const currentFile = this.store.getCurrentFile(deviceId)();
  if (!currentFile) return;

  // Extract storage info and build URL...
}
```

</details>

---

<details open>
<summary><h3>Task 3: Integrate URL Updates After File Launches</h3></summary>

**Purpose**: Call the URL update helper after successful file launches in all file navigation methods.

**Related Documentation:**
- [Deep Linking Plan - Phase 3 Implementation](./DEEP_LINKING_PLAN.md#implementation-tasks) - All methods requiring updates

**Implementation Subtasks:**
- [ ] **launchFileWithContext**: Add `updateUrlForLaunchedFile()` call after error check (~line 58)
- [ ] **launchRandomFile**: Add `updateUrlForLaunchedFile()` call after error check (~line 99)
- [ ] **next() - Directory Mode**: Add URL update after successful navigation
- [ ] **next() - Shuffle Mode (New File)**: Add URL update after successful navigation
- [ ] **next() - Shuffle Mode (History)**: Add URL update after successful history navigation
- [ ] **previous() - Directory Mode**: Add URL update after successful navigation
- [ ] **previous() - Shuffle Mode (New File)**: Add URL update after successful navigation
- [ ] **previous() - Shuffle Mode (History)**: Add URL update after successful history navigation
- [ ] **navigateToHistoryPosition**: Add URL update after successful navigation (~line 490)

**Testing Subtask:**
- [ ] **No Tests Required**: File launch behaviors already tested, E2E tests validate URLs in Phase 4

**Key Implementation Notes:**
- **CRITICAL**: Only update URL after error check using `hasErrorAndCleanup()` returns false
- Place URL update call after `setupTimerForFile()` in each method
- For `next()` and `previous()`, update URL in both shuffle mode paths (history navigation AND new file launch)
- For history navigation paths, update URL but do NOT re-record history
- Consistent pattern: error check → record history (if new file) → setup timer → update URL

**Integration Pattern Example**:
```typescript
// After successful launch and timer setup
if (!this.hasErrorAndCleanup(deviceId)) {
  this.recordHistoryIfSuccessful(deviceId);
  this.setupTimerForFile(deviceId, currentFile.file);
  this.updateUrlForLaunchedFile(deviceId); // ← Add this
}
```

</details>

---

## 🗂️ Files Modified or Created

**Modified Files:**
- `libs/application/src/lib/player/player-context.service.ts`

**No New Files Created**

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Phase 3 requires **NO unit tests** - this is simple URL synchronization logic.

> **Testing Philosophy:**
> - Existing unit tests already validate file launch behaviors work correctly
> - URL updates are a side effect of successful launches (non-functional requirement)
> - Phase 4 E2E tests will validate URL updates in real browser environment
> - Manual testing during development ensures integration works

### Testing Approach

**No Unit Tests Required Because:**
- URL update is a simple side effect of existing validated behaviors
- `location.go()` is a framework API call (no business logic to test)
- E2E tests provide better validation of URL/browser history integration

**Phase 4 E2E Tests Will Validate:**
- URL updates when file launched from directory
- URL updates when random file launched
- URL updates when navigating next/previous
- URL updates when navigating history positions
- Browser back/forward buttons work correctly
- All four query parameters present and correct

### Manual Testing During Development

**Test these scenarios manually:**
1. Launch file from directory → URL updates with all 4 params
2. Click "Random" button → URL updates with random file
3. Click "Next" button → URL updates with next file
4. Click "Previous" button → URL updates with previous file
5. Navigate through history → URL updates correctly
6. Use browser back button → navigates to previous URL

### Test Execution Commands

**Running Existing Tests:**
```bash
# Verify no regressions in existing tests
npx nx test application

# Run player context service tests specifically
npx nx test application --testPathPattern=player-context
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
- [ ] Location service properly injected
- [ ] URL update helper method implemented
- [ ] All file launch methods update URL after successful launch

**Testing Requirements:**
- [ ] Existing unit tests still pass with no regressions
- [ ] Manual testing confirms URL updates correctly
- [ ] Manual testing confirms browser back/forward work
- [ ] Manual testing confirms all 4 query params present

**Quality Checks:**
- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors (`npm run lint`)
- [ ] Code formatting is consistent
- [ ] No console errors in browser when testing

**Documentation:**
- [ ] Inline comments added for URL update logic
- [ ] Phase 3 marked complete in [DEEP_LINKING_PLAN.md](./DEEP_LINKING_PLAN.md)

**Ready for Next Phase:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Ready to proceed to Phase 4 (E2E Testing)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Use `location.go()` not `router.navigate()`**: Prevents re-triggering the route resolver which would reload everything
- **No unit tests**: Simple URL synchronization validated better by E2E tests in Phase 4
- **Update after successful launch only**: Check `hasErrorAndCleanup()` to avoid updating URL for failed launches
- **Centralized helper method**: Avoids code duplication across 9+ call sites

### Implementation Constraints

- **Must not trigger navigation**: Using `location.go()` ensures URL updates without component reload
- **Angular handles encoding**: No manual URL encoding needed for paths/filenames with special characters

### Future Enhancements

- **Phase 4**: Comprehensive E2E tests validating URL updates and browser history
- **Potential Enhancement**: Add URL update on directory navigation (not just file launches)

### External References

- [Deep Linking Plan](./DEEP_LINKING_PLAN.md) - Overall feature architecture
- [Angular Location API](https://angular.io/api/common/Location) - Framework documentation

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

</details>

---

## 💡 Agent Implementation Guide

> **Instructions for AI agents implementing this phase**

### Implementation Order

1. **Start Simple**: Add Location service import and injection first
2. **Build Helper**: Implement URL update helper method with all logic
3. **Integrate Carefully**: Add helper calls to each navigation method after error checks
4. **Test Incrementally**: Manually test each integration point as you add it

### Key Integration Points

**Methods Requiring URL Updates (in order of implementation):**
1. `launchFileWithContext()` - Primary file launch
2. `launchRandomFile()` - Random file selection
3. `next()` - Both history navigation and new file paths
4. `previous()` - Both history navigation and new file paths
5. `navigateToHistoryPosition()` - Direct history access

### Common Pitfalls to Avoid

- ❌ **Don't use `router.navigate()`** - Will re-trigger resolver and reload component
- ❌ **Don't update URL before error check** - Failed launches shouldn't update URL
- ❌ **Don't skip shuffle mode history paths** - Both new file AND history navigation need updates
- ❌ **Don't manually encode URLs** - Angular query params handle encoding automatically

### Validation Checklist

After each method integration:
- [ ] URL update only happens after `hasErrorAndCleanup()` check
- [ ] URL update happens after timer setup
- [ ] All 4 query parameters present in URL
- [ ] Browser back button navigates to previous URL

### Remember

- This is a **simple integration phase** - no complex logic needed
- **Manual testing** is more valuable than unit tests for URL updates
- **Phase 4 E2E tests** will provide comprehensive validation
- **Mark progress incrementally** as you complete each method integration

</details>
