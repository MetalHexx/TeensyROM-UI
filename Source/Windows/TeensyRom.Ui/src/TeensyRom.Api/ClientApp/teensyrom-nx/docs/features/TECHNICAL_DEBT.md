# Technical Debt

This document tracks known technical debt items that should be addressed in future iterations.

## Testing Infrastructure

### HTTP Backend Mocking in Integration Tests

**Priority**: Medium  
**Effort**: 2-3 days  
**Created**: 2025-01-08  

**Issue**: Several integration tests are using real HTTP backends instead of mocked services, which creates dependencies on external services and makes tests less reliable.

**Affected Files**:
- `libs/domain/device/services/src/lib/device.service.integration.spec.ts` - Uses real HTTP client with `localhost:5168`
- `libs/domain/device/services/src/lib/storage.service.integration.spec.ts` - Uses real HTTP client with `localhost:5168`

**Current Problems**:
- Tests require backend API to be running on `localhost:5168`
- Tests are fragile and can fail due to network issues
- Difficult to test error scenarios consistently  
- CI/CD pipelines require backend setup
- Slower test execution due to real HTTP calls

**Recommended Solution**:
Migrate to MSW (Mock Service Worker) for all integration tests:
- Replace real HTTP clients with MSW handlers
- Mock all API endpoints used in integration tests
- Create reusable MSW setup utilities
- Test both success and error scenarios with controlled responses
- Follow the patterns established in `TESTING_STANDARDS.md`

**Benefits**:
- Faster, more reliable tests
- Better error scenario coverage  
- No backend dependencies
- CI/CD friendly
- Consistent with testing standards

**Breaking Changes**: None - tests will continue to work the same way

---

## Code Quality

### Remove Remaining `any` Usage

**Priority**: Low  
**Effort**: 1 day  
**Created**: 2025-01-08  

**Issue**: There may still be some `any` types in the codebase that should be replaced with proper TypeScript types.

**Action Items**:
- Search codebase for remaining `any` usage
- Replace with proper types where possible
- Add type annotations for better type safety
- Update ESLint rules to prevent new `any` usage

---

## Architecture

*No current items*

---

## Performance  

*No current items*

---

## Security

*No current items*

---

## Notes

- Items should be prioritized as: Critical > High > Medium > Low
- Effort estimates: < 1 day (Small), 1-3 days (Medium), 1+ weeks (Large)
- Include creation date for tracking
- Archive completed items to a "Completed" section with resolution date