# TODO

This document tracks future feature ideas and improvements that should be considered for implementation.

## UI/UX Enhancements

### Add Tooltips to All Components

**Priority**: Medium  
**Effort**: 2-3 days  
**Created**: 2025-09-30

**Issue**: The application currently lacks comprehensive tooltips across components, which reduces discoverability and user experience for interface elements.

**Affected Areas**:
- Icon buttons throughout the application (filter buttons, navigation, player controls)
- Form inputs and controls
- Status indicators and badges
- Navigation elements
- Action buttons and controls

**Current Problems**:
- Users may not understand the purpose of icon-only buttons
- No contextual help for complex interface elements
- Reduced accessibility for screen readers
- Poor discoverability of features and functionality

**Recommended Solution**:
Implement comprehensive tooltip system:
- Add `matTooltip` directives to all interactive elements
- Create consistent tooltip messaging and positioning
- Ensure tooltips are accessible and keyboard navigable
- Consider tooltip delays and animation preferences
- Add tooltips to custom icon components and buttons
- Document tooltip usage patterns in style guide

**Benefits**:
- Improved user experience and interface discoverability
- Better accessibility compliance
- Reduced learning curve for new users
- More intuitive interface interaction
- Enhanced usability for complex features

**Breaking Changes**: None - purely additive enhancement

### Global Error Handling with Toast Notifications

**Priority**: High  
**Effort**: 3-4 days  
**Created**: 2025-09-30

**Issue**: The application currently lacks a unified error handling system, making it difficult for users to understand when operations fail or encounter issues.

**Affected Areas**:
- HTTP API calls and network errors
- File operations (upload, download, launch)
- Device communication errors
- Form validation and submission errors
- Background operations and async tasks

**Current Problems**:
- Errors are only logged to console, invisible to users
- No user feedback when operations fail
- Difficult to debug issues from user perspective
- Poor error recovery guidance
- Inconsistent error handling across components

**Recommended Solution**:
Implement global error handling with toast notification system:
- Create global error interceptor for HTTP requests
- Implement toast notification service with stacking support
- Add error boundary components for React-style error handling
- Create consistent error message formatting and categorization
- Support multiple simultaneous toast alerts with auto-dismiss
- Add retry mechanisms for recoverable errors
- Implement different toast types (error, warning, info, success)
- Consider toast positioning, duration, and user interaction options

**Benefits**:
- Immediate user feedback for all error conditions
- Consistent error messaging across the application
- Better user understanding of system state and issues
- Improved debugging and support capabilities
- Enhanced reliability perception through transparent error communication
- Better error recovery workflows

**Breaking Changes**: None - purely additive enhancement

### Granular Button Error States

**Priority**: Low  
**Effort**: 1-2 days  
**Created**: 2025-09-30

**Issue**: Currently, when operations fail, there's no visual indication on the specific button that caused the error. All buttons maintain their normal appearance even when their associated actions fail.

**Affected Components**:
- Player controls (next, previous, play, pause, stop buttons)
- Filter buttons (all files, games, music, images, random launch)
- Navigation buttons and action controls
- Any interactive button that can trigger operations

**Current Problems**:
- No visual feedback on which specific button/action caused an error
- Users cannot easily identify the source of failed operations
- All buttons look the same regardless of their operational state
- Difficult to correlate error messages with the triggering action

**Recommended Solution**:
Implement granular error state system for buttons:
- Add error state styling to `IconButtonComponent` (red border, error icon overlay, etc.)
- Create button state management for tracking individual button errors
- Implement error state timeout/reset mechanisms
- Add error state to button variants and themes
- Create consistent error styling across all interactive buttons
- Consider error state animations or visual feedback

**Benefits**:
- Clear visual feedback for failed operations
- Better user understanding of which actions are failing
- Improved debugging and user support experience
- More intuitive error identification and recovery
- Enhanced overall user experience and interface clarity

**Breaking Changes**: None - additive enhancement to existing button components

---

## Feature Ideas

_No current items - add feature ideas here as they arise_

---

## Performance Optimizations

_No current items - add performance improvement ideas here_

---

## Developer Experience

_No current items - add DX improvement ideas here_

---

## Integration & API

_No current items - add API and integration ideas here_

---

## Notes

- Items should be prioritized as: Critical > High > Medium > Low
- Effort estimates: < 1 day (Small), 1-3 days (Medium), 1+ weeks (Large)
- Include creation date for tracking
- Move completed items to TECHNICAL_DEBT.md "Completed" section with resolution date
- Use this for quick capture of ideas that can be refined later