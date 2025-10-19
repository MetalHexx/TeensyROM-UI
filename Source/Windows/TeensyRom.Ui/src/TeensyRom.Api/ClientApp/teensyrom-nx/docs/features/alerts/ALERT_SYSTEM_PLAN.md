# 🔔 TeensyROM Alert System - Implementation Plan

**Project Overview**: Design and implement a comprehensive alert/notification system for TeensyROM that provides beautiful, modern toast-style notifications with configurable positioning, semantic color coding (success, error, warning, info), and physics-based stacking animations. The system will integrate with the infrastructure layer for automatic error handling and provide a dedicated app-level library for alert management.

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **Component Library**: [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md)
- **Style Guide**: [STYLE_GUIDE.md](../../STYLE_GUIDE.md)
- **NX Library Standards**: [NX_LIBRARY_STANDARDS.md](../../NX_LIBRARY_STANDARDS.md)

---

## 🎯 Project Objective

Create a modern, flexible alert/notification system that provides visual feedback for user actions, system events, errors, warnings, and success states throughout the TeensyROM application. The system will display beautiful animated toast-style notifications that can be positioned at six locations (top-left, top-center, top-right, bottom-left, bottom-center, bottom-right) with semantic color coding based on message severity.

**User Value**: Users receive clear, non-intrusive visual feedback for all important system events—successful operations appear with green success messages, errors display with red warnings, informational messages use cyan highlights, and warnings show in yellow/amber tones. Multiple alerts stack gracefully with smooth physics-based animations as new messages arrive and shift according to their positioning (bottom alerts push up, top alerts push down), creating a polished, professional user experience. Alerts automatically dismiss after 3 seconds while allowing users to manually dismiss messages when needed.

**System Benefits**: The alert system provides a centralized, consistent notification mechanism across all features and layers of the application. Infrastructure services automatically display error messages from API responses when operations fail, eliminating the need for individual error handling UI in each component. The domain contract pattern allows the alert service to be easily mocked for testing while the actual implementation lives in the app layer, maintaining clean separation of concerns. The service manages alert state internally without requiring NgRx store overhead.

---

## 📋 Implementation Phases

<details open>
<summary><h3>Phase 1: Core Alert Infrastructure & Domain Contracts</h3></summary>

### Objective

Establish the foundational alert system architecture by creating domain contracts, internal state management within the alert service, and basic alert display capabilities. This phase delivers a working alert system with simple rendering that can be consumed by any layer of the application.

### Key Deliverables

- [ ] Domain contracts defined for alert service interface and alert message data structures
- [ ] New app-layer library created following NX library standards (`libs/app/alerts`)
- [ ] Core alert service created with internal state management (queue as private array)
- [ ] Alert service implements show methods (success, error, warning, info) with configurable 3-second auto-dismiss
- [ ] Auto-dismiss logic with timer management and cleanup
- [ ] Basic alert display component rendering messages with icon and text
- [ ] Alert container component managing fixed positioning
- [ ] Unit tests for all alert service logic and component rendering
- [ ] Alert service registered in DI system and available application-wide

### High-Level Tasks

1. **Create Domain Contracts**: Define alert service interface, alert message data structure with severity and position enums, establish contract for all layers to consume
2. **Generate App Alerts Library**: Create new library in app layer following NX library standards with proper Clean Architecture dependency tags
3. **Implement Alert Service**: Build service with internal state management, methods to show alerts with different severity levels, auto-dismiss logic with configurable timing, and proper cleanup on dismiss
4. **Build Alert Display Component**: Create component that renders individual alert messages with icon, text content, and dismiss action, applying severity-based styling
5. **Build Alert Container Component**: Create fixed-position container that manages alert positioning, renders active alerts, and handles stacking behavior for six screen positions
6. **Write Comprehensive Tests**: Unit test alert service operations, component rendering, timer behavior, and cleanup to ensure no memory leaks
7. **Register Service Globally**: Configure dependency injection to make alert service available application-wide as singleton

### Open Questions Resolved

- **Alert Queue Limit**: ✅ No queue limit - unlimited alerts can be displayed
- **Default Auto-Dismiss Duration**: ✅ 3 seconds (configurable per alert via optional parameter)
- **Duplicate Alert Handling**: ✅ No deduplication - show each alert occurrence

</details>

---

<details open>
<summary><h3>Phase 2: Infrastructure Integration & Service Error Handling</h3></summary>

### Objective

Integrate the alert system throughout the infrastructure layer by adding automatic error handling to all services, ensuring that API failures automatically display error messages extracted from API response objects. This phase proves the alert system works end-to-end with real services before investing in visual polish. Success notifications are deferred to the application layer for explicit control over when operations warrant user feedback.

### Key Deliverables

- [ ] Alert service injected into all infrastructure services via domain contract (IAlertService)
- [ ] Error handling added to all observable service methods using RxJS `catchError`
- [ ] Error messages extracted from API response objects (response.message or response.error.message)
- [ ] Fallback error messages crafted for scenarios where API doesn't provide messages
- [ ] Error handling follows player.service.ts pattern: log error, display alert, rethrow for upstream handling
- [ ] Application layer retains control over success notifications (no automatic success alerts from infrastructure)
- [ ] Integration tests validating error handling triggers alerts correctly with proper messages
- [ ] All existing infrastructure services audited and updated with consistent error handling
- [ ] End-to-end validation: real API errors display basic alerts successfully

### High-Level Tasks

1. **Audit Existing Services**: Review all infrastructure services to identify operations needing error handling and alert integration
2. **Inject Alert Service**: Add alert service dependency to all infrastructure services via domain contract interface
3. **Add Error Handling Pattern**: Implement consistent error handling across all service operations following established service patterns for logging, alerting, and error propagation
4. **Extract Error Messages from API Responses**: Retrieve user-friendly error messages from API response objects when available
5. **Create Contextual Fallback Messages**: Craft clear, actionable fallback messages for operations where API doesn't provide descriptive error text
6. **Leave Success Notifications to Application Layer**: Ensure infrastructure services do not automatically trigger success alerts - application layer controls when to show success feedback
7. **Write Integration Tests**: Test that service error scenarios trigger appropriate alerts with correct messages, verify error propagation behavior
8. **Test End-to-End**: Validate alert system integration by triggering real service errors and confirming user-facing alert behavior with basic styling

### Open Questions Resolved

- **Error Message Source**: ✅ Use messages from API response objects (response.message or response.error.message)
- **Error Rethrowing**: ✅ Follow player.service.ts pattern - display alert, then rethrow error for upstream handling
- **Success Alert Control**: ✅ Leave success notifications to application layer - no automatic success alerts from infrastructure
- **Error Message Verbosity**: ✅ Use API messages as-is, provide clear contextual fallbacks when API doesn't provide messages

</details>

---

<details open>
<summary><h3>Phase 3: Visual Design, Positioning, and Animations</h3></summary>

### Objective

Transform the basic alert display into a beautiful, polished notification system with proper positioning, semantic color coding using design tokens, smooth directional entry/exit animations, and physics-based stacking behavior where alerts push existing messages according to their position (bottom alerts push up, top alerts push down). This phase enhances the working system from Phase 2 with production-ready visual design.

### Key Deliverables

- [ ] Six positioning options fully implemented with proper CSS fixed positioning and flexbox stacking
- [ ] Semantic color system integrated: success (green), error (red), warning (new `--color-warning` using directory yellow), info (cyan highlight)
- [ ] Alert component redesigned using `lib-scaling-compact-card` with directional animations matching position
- [ ] Physics-based stacking animations: bottom positions stack upward (new alerts push from below), top positions stack downward (new alerts push from above)
- [ ] Icon-label component integrated with color-coded icons per severity (check_circle, error, warning, info)
- [ ] Design tokens added: `--color-warning` aliasing `--color-directory` values in style guide
- [ ] Animation timing tuned: entry 400ms, exit 300ms, stacking shift 250ms with smooth easing
- [ ] Visual tests confirming all positioning and color schemes work in light/dark themes

### High-Level Tasks

1. **Implement Positioning System**: Create positioning logic for six screen locations with proper fixed positioning and layout structure for each location
2. **Add Warning Color Token**: Add warning color design token to style guide that references existing directory color values, maintaining theme consistency
3. **Integrate Animation Components**: Enhance alert display with existing animation components, configuring entry/exit directions based on screen position
4. **Implement Directional Stacking**: Configure stacking behavior where bottom-positioned alerts push upward and top-positioned alerts push downward, with smooth transitions between states
5. **Apply Semantic Color System**: Map alert severity levels to design tokens, integrate colored icons from component library, ensure proper icon selection per severity
6. **Configure Animation Timing**: Set animation durations for entry, exit, and stacking transitions that feel responsive and polished
7. **Add Stacking Shift Animations**: Implement smooth repositioning of existing alerts when new alerts arrive or when alerts are dismissed
8. **Test Visual Behavior**: Verify positioning, stacking, colors, and animations work correctly across all combinations and in both light/dark themes

### Open Questions Resolved

- **Stacking Direction**: ✅ Position-dependent: bottom positions stack upward (new alerts push from below), top positions stack downward (new alerts push from above)
- **Animation Durations**: ✅ Entry 400ms, exit 300ms, stacking shift 250ms (tuned for smoothness)
- **Warning Color**: ✅ Create `--color-warning` that aliases `--color-directory` (light: #f5d76e, dark: #f5e29e)
- **Info Color**: ✅ Use existing `--color-highlight` (cyan)

</details>

---

<details open>
<summary><h3>Phase 4: Testing, Refinement, and Documentation</h3></summary>

### Objective

Ensure the alert system is fully tested, documented, and ready for production use with comprehensive unit test coverage, updated component library documentation, example usage patterns, and accessibility features properly implemented.

### Key Deliverables

- [ ] Complete unit test coverage for alert service, components, and all alert-related code
- [ ] Integration tests covering alert workflows and service integration scenarios
- [ ] Component library documentation updated with alert component usage examples
- [ ] Style guide updated with `--color-warning` token and alert styling patterns
- [ ] Developer guidelines documented for triggering alerts from any layer
- [ ] Accessibility features implemented: ARIA `role="alert"` for announcements, keyboard-accessible dismiss buttons, screen reader friendly messages
- [ ] Performance testing confirms alert system doesn't impact application responsiveness
- [ ] No rate limiting needed - system handles rapid alerts efficiently

### High-Level Tasks

1. **Complete Unit Test Coverage**: Fill any testing gaps from previous phases, test all service methods, component rendering, timer management, positioning logic, achieve >90% coverage
2. **Write Integration Tests**: Test multi-component flows (service error → alert service → alert display → auto-dismiss → DOM removal), test service integration with mocked alert service
3. **Update Component Library**: Add alert component section to COMPONENT_LIBRARY.md with component selectors, props, usage examples, positioning options, animation behavior
4. **Document Design Tokens**: Update STYLE_GUIDE.md with `--color-warning` token definition and usage, document alert-specific classes and patterns
5. **Write Developer Guide**: Create clear examples in alert library README showing how to trigger alerts from services, components, and stores, document best practices
6. **Implement Accessibility Features**: Add `role="alert"` to alert container for screen reader announcements, ensure dismiss buttons have proper aria-labels and keyboard navigation (Tab + Enter/Space), test with screen readers
7. **Performance Testing**: Test rendering many simultaneous alerts (20+), verify animations remain smooth, confirm no memory leaks from timer cleanup
8. **Final Refinement**: Polish animations, tune timing values if needed, ensure consistent behavior across all browsers

### Open Questions Resolved

- **E2E Tests**: ✅ No Cypress tests needed - focus on unit and integration tests only
- **ARIA Announcements**: ✅ Use `role="alert"` for screen reader announcements
- **Keyboard Shortcuts**: ✅ No keyboard shortcuts for dismissing all alerts
- **Alert Persistence**: ✅ No special persistence - all alerts auto-dismiss after 3 seconds
- **Performance Limits**: ✅ No rate limiting needed - system should handle rapid alerts efficiently

</details>

---

<details open>
<summary><h2>🏗️ Architecture Overview</h2></summary>

### Key Design Decisions

- **App-Layer Library**: Alert system lives in `libs/app/alerts` (scope:app) as a cross-cutting concern that coordinates application-wide notifications. This follows the pattern where shell, navigation, and bootstrap also live in the app layer as composition root concerns.

- **Domain Contract Pattern**: Alert service interface (`IAlertService`) defined in domain layer allows infrastructure services to depend on the abstraction while maintaining Clean Architecture dependency rules. Concrete implementation lives in app layer and is provided at composition root.

- **Internal State Management**: Alert service manages state internally using private arrays and RxJS subjects rather than NgRx store, reducing complexity and avoiding store overhead for this focused use case. Service exposes observable of current alerts for container component subscription.

- **Component Composition**: Alert display leverages existing UI components (scaling-compact-card for animations, icon-label for content, styled-icon for semantic colors, icon-button for dismiss) ensuring visual consistency and reducing maintenance burden.

- **Timer-Based Animation Control**: Alert auto-dismiss uses RxJS timers to emit signals that trigger scaling-compact-card entry/exit animations. Timers are properly cleaned up on dismiss to prevent memory leaks.

- **Directional Stacking with Flexbox**: Alert stacking direction matches position intuitively (bottom alerts push upward using `flex-direction: column-reverse`, top alerts push downward using `flex-direction: column`) with smooth CSS transitions on transform for physics-based shifting.

- **API Message Integration**: Error messages extracted from API response objects provide user-friendly, contextual feedback. Infrastructure services follow consistent pattern: log error, display alert with API message, rethrow for upstream handling.

### Integration Points

- **Domain Layer**: Defines `IAlertService` contract interface and alert data structures (`AlertMessage`, `AlertSeverity`, `AlertPosition` enums). Pure TypeScript contracts with no dependencies, consumable by any layer.

- **App Layer (alerts library)**: Contains alert service implementation (`AlertService` implementing `IAlertService`), alert container component (fixed-position wrapper with positioning logic), individual alert display component (scaling-compact-card + icon-label composition), and DI provider bindings.

- **Infrastructure Layer**: All services inject `IAlertService` via domain contract and use it in `catchError` blocks to display errors automatically. Error messages extracted from API response objects or fallback to contextual messages. Services rethrow errors for upstream handling following player.service.ts pattern.

- **Application Layer**: Stores and components trigger success alerts explicitly when operations warrant user feedback. Application layer decides which operations are significant enough for success notifications.

- **UI Components**: Alert system composes scaling-compact-card (animation), icon-label (content layout), styled-icon (semantic colors), and icon-button (dismiss action) from shared UI component library. Alert-specific styling uses design tokens from style guide.

- **Style Guide**: New `--color-warning` design token added, aliasing `--color-directory` values for consistency. Alert components documented in component library. Color mapping clarified for all severity levels.

- **Composition Root (app bootstrap)**: Alert service providers registered in app bootstrap module to make service available application-wide with singleton scope. Alert container component rendered in app shell layout as fixed-position sibling to router-outlet.

</details>

---

<details open>
<summary><h2>🧪 Testing Strategy</h2></summary>

### Unit Tests (Focus Area)

- [ ] Alert service show methods (success, error, warning, info) add alerts to internal queue
- [ ] Alert service dismiss method removes specific alert by ID
- [ ] Auto-dismiss timers start correctly and remove alerts after 3 seconds (use Jasmine fake timers)
- [ ] Custom auto-dismiss duration parameter works correctly
- [ ] Manual dismiss cancels auto-dismiss timer and cleans up properly
- [ ] Alert service observable emits current alert list on changes
- [ ] Alert service handles rapid alert additions efficiently
- [ ] Alert display component renders icon, message, and dismiss button correctly
- [ ] Alert display component applies correct severity classes and colors
- [ ] Alert container component renders all active alerts
- [ ] Alert container component applies correct positioning classes for six positions
- [ ] Alert container stacking direction matches position (bottom pushes up, top pushes down)
- [ ] Animation trigger signals coordinate entry/exit with auto-dismiss
- [ ] Timer cleanup prevents memory leaks (no orphaned subscriptions)

### Integration Tests

- [ ] Complete alert lifecycle: add → display → render with animation → auto-dismiss after 3s → remove from DOM
- [ ] Service error triggers infrastructure service catchError → alert service → alert displays with API message
- [ ] Multiple simultaneous alerts stack correctly with smooth position shifts
- [ ] Position-specific behavior: all six positions render correctly with appropriate stacking direction
- [ ] Theme integration: color tokens render correctly in both light and dark modes
- [ ] Infrastructure service integration: device.service error displays alert and rethrows error
- [ ] Infrastructure service integration: storage.service error displays alert with API response message
- [ ] Alert service can be mocked in consuming component tests

### E2E Tests

- ✅ **Not Required**: Per user decision, no Cypress tests needed at this time

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

- [ ] Alert system displays success, error, warning, and info messages with semantic colors from design tokens
- [ ] Alerts positioned at six locations (top-left, top-center, top-right, bottom-left, bottom-center, bottom-right)
- [ ] Multiple alerts stack correctly: bottom positions push upward, top positions push downward
- [ ] Stacking shifts are smooth with physics-based animations (250ms transitions)
- [ ] Alerts automatically dismiss after 3 seconds (configurable per alert)
- [ ] Users can manually dismiss alerts using dismiss button
- [ ] Alert entry animations match position direction (bottom enters from-bottom, top enters from-top)
- [ ] All infrastructure services automatically display error alerts with messages from API responses
- [ ] Infrastructure services follow consistent error pattern: log, alert, rethrow
- [ ] Application layer controls success notifications (no automatic success alerts from infrastructure)
- [ ] Alert component integrates scaling-compact-card, icon-label, styled-icon, and icon-button from component library
- [ ] Design token `--color-warning` added to style guide (aliasing directory color)
- [ ] Alert system is fully accessible (ARIA `role="alert"`, keyboard navigation, screen reader support)
- [ ] All unit and integration tests pass with >90% coverage
- [ ] Component library documentation includes complete alert system usage guide
- [ ] Alert system performs efficiently with many simultaneous alerts (no lag or memory leaks)
- [ ] Feature ready for production deployment and integrated throughout application

</details>

---

<details open>
<summary><h2>🎭 User Scenarios</h2></summary>

### Alert Display and Dismissal Scenarios

<details open>
<summary><strong>Scenario 1: User Sees Error Alert from API Failure</strong></summary>

```gherkin
Given the user attempts an operation that calls the backend API
When the API returns an error response with a message
Then a red error alert appears at the configured position with an error icon and the API's error message
And the alert automatically dismisses after 3 seconds
And the user can manually dismiss the alert by clicking the dismiss button before auto-dismiss
```
</details>

<details open>
<summary><strong>Scenario 2: Application Layer Shows Success Alert</strong></summary>

```gherkin
Given the user completes a significant operation (e.g., connects to device)
When the application layer explicitly triggers a success alert
Then a green success alert appears with a check icon and success message
And the alert automatically dismisses after 3 seconds
```
</details>

<details open>
<summary><strong>Scenario 3: User Sees Warning Alert</strong></summary>

```gherkin
Given a non-critical warning condition occurs
When a warning alert is triggered
Then a yellow/amber warning alert appears with a warning icon and descriptive message
And the alert automatically dismisses after 3 seconds
```
</details>

<details open>
<summary><strong>Scenario 4: User Sees Info Alert</strong></summary>

```gherkin
Given the system needs to provide informational feedback
When an info alert is triggered
Then a cyan info alert appears with an info icon and the message
And the alert automatically dismisses after 3 seconds
```
</details>

---

### Stacking and Positioning Scenarios

<details open>
<summary><strong>Scenario 5: Bottom-Positioned Alerts Push Upward</strong></summary>

```gherkin
Given alerts are configured for bottom-right position
And two alerts are already displayed stacked vertically
When a third alert is triggered
Then the new alert appears at the bottom of the container
And existing alerts shift smoothly upward with 250ms animation to make room
And the new alert entry animation plays from-bottom
```
</details>

<details open>
<summary><strong>Scenario 6: Top-Positioned Alerts Push Downward</strong></summary>

```gherkin
Given alerts are configured for top-left position
And two alerts are already displayed stacked vertically
When a third alert is triggered
Then the new alert appears at the top of the container
And existing alerts shift smoothly downward with 250ms animation to make room
And the new alert entry animation plays from-top
```
</details>

<details open>
<summary><strong>Scenario 7: Alerts Respect Six Position Options</strong></summary>

```gherkin
Given alerts can be positioned at six locations
When alerts are triggered with different position settings
Then each alert appears at its specified fixed position
And alerts at the same position stack correctly according to their top/bottom placement
And all positions render correctly in viewport corners/edges
```
</details>

<details open>
<summary><strong>Scenario 8: Alert Stacking Shifts Smoothly on Dismiss</strong></summary>

```gherkin
Given three alerts are stacked at bottom-right position
When the middle alert is dismissed (manually or auto-dismiss)
Then the dismissed alert plays exit animation and is removed
And remaining alerts shift smoothly to fill the gap with 250ms animation
And no jumpy or abrupt position changes occur
```
</details>

---

### Service Integration Scenarios

<details open>
<summary><strong>Scenario 9: Infrastructure Service Error Displays API Message</strong></summary>

```gherkin
Given a user attempts to connect to a device
When the API call fails and returns response with error message "Device not found"
Then the device service catchError block extracts the message
And displays a red error alert with "Device not found"
And logs the error to console
And rethrows the error for upstream handling
```
</details>

<details open>
<summary><strong>Scenario 10: Service Error Uses Fallback Message</strong></summary>

```gherkin
Given a user attempts an operation
When the API call fails without a response message
Then the service displays an error alert with contextual fallback message
And the fallback message describes the operation and context
And the error is logged and rethrown
```
</details>

<details open>
<summary><strong>Scenario 11: Application Layer Controls Success Notifications</strong></summary>

```gherkin
Given a user completes a routine operation successfully
When the infrastructure service returns success
Then no automatic success alert is displayed
And the application layer decides whether to show success feedback
And significant operations trigger explicit success alerts from application layer
```
</details>

---

### Animation and Timing Scenarios

<details open>
<summary><strong>Scenario 12: Bottom Alert Entry Animation Plays from Bottom</strong></summary>

```gherkin
Given alerts are configured for bottom-center position
When an alert is triggered
Then the scaling-compact-card entry animation plays from-bottom direction
And the alert scales and fades in smoothly over 400ms
```
</details>

<details open>
<summary><strong>Scenario 13: Top Alert Entry Animation Plays from Top</strong></summary>

```gherkin
Given alerts are configured for top-right position
When an alert is triggered
Then the scaling-compact-card entry animation plays from-top direction
And the alert scales and fades in smoothly over 400ms
```
</details>

<details open>
<summary><strong>Scenario 14: Alert Exit Animation Plays on Dismiss</strong></summary>

```gherkin
Given an alert is displayed
When the user clicks the dismiss button or auto-dismiss timer expires
Then the scaling-compact-card exit animation plays
And the alert scales and fades out smoothly over 300ms
And the alert is removed from the DOM after animation completes
```
</details>

<details open>
<summary><strong>Scenario 15: Manual Dismiss Cancels Auto-Dismiss Timer</strong></summary>

```gherkin
Given an alert is displayed with a 3-second auto-dismiss timer running
When the user manually dismisses the alert after 1 second
Then the auto-dismiss timer is immediately cancelled and cleaned up
And the exit animation plays immediately
And no memory leak occurs from timer subscription
```
</details>

<details open>
<summary><strong>Scenario 16: Custom Auto-Dismiss Duration Works</strong></summary>

```gherkin
Given an alert is triggered with custom 5-second auto-dismiss duration
When the alert appears
Then the alert remains visible for 5 seconds instead of default 3 seconds
And automatically dismisses after the custom duration
```
</details>

---

### Accessibility Scenarios

<details open>
<summary><strong>Scenario 17: Screen Reader Announces Alert</strong></summary>

```gherkin
Given a screen reader user is using the application
When an error alert is triggered
Then the alert container has role="alert" for ARIA announcements
And the screen reader announces the alert message immediately
And the alert content is accessible to assistive technologies
```
</details>

<details open>
<summary><strong>Scenario 18: Keyboard Navigation to Dismiss Button</strong></summary>

```gherkin
Given an alert is displayed with a dismiss button
When the user presses Tab to navigate
Then the dismiss button receives keyboard focus with visible focus indicator
And pressing Enter or Space dismisses the alert
And the dismiss button has proper aria-label for screen readers
```
</details>

---

### Edge Case and Performance Scenarios

<details open>
<summary><strong>Scenario 19: Many Rapid Alerts Don't Degrade Performance</strong></summary>

```gherkin
Given a batch operation triggers 20 errors in rapid succession
When all 20 error alerts are queued quickly
Then all alerts are displayed and managed efficiently without lag
And animations remain smooth across all alerts
And no memory leaks occur from timer subscriptions
And the UI remains responsive during alert processing
```
</details>

<details open>
<summary><strong>Scenario 20: No Deduplication Allows Multiple Identical Alerts</strong></summary>

```gherkin
Given the same error occurs three times in rapid succession
When identical alert messages are triggered
Then all three alerts are displayed independently
And each alert has its own auto-dismiss timer
And users see each occurrence distinctly
```
</details>

<details open>
<summary><strong>Scenario 21: Theme Changes Update Alert Colors</strong></summary>

```gherkin
Given alerts are displayed in light mode
When the user switches to dark mode
Then all active alert colors update to dark mode design tokens
And new alerts use dark mode colors
And color transitions are smooth without visual glitches
```
</details>

</details>

---

<details open>
<summary><h2>📚 Related Documentation</h2></summary>

- **Architecture Overview**: [OVERVIEW_CONTEXT.md](../../OVERVIEW_CONTEXT.md)
- **NX Library Standards**: [NX_LIBRARY_STANDARDS.md](../../NX_LIBRARY_STANDARDS.md)
- **Component Library**: [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md)
- **Style Guide**: [STYLE_GUIDE.md](../../STYLE_GUIDE.md)
- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **Service Standards**: [SERVICE_STANDARDS.md](../../SERVICE_STANDARDS.md)

</details>

---

<details open>
<summary><h2>📝 Notes</h2></summary>

### Design Considerations

- **Internal State Management**: Alert service manages state internally rather than using NgRx store, reducing complexity while still providing reactive updates to display components through observable patterns.

- **Component Reuse Strategy**: Alert display leverages existing reusable UI components for animations, icons, labels, and buttons, ensuring visual consistency with the rest of the application and reducing maintenance burden.

- **API Message Integration**: Error messages extracted from API response objects provide user-friendly, contextual feedback. Services provide sensible fallback messages when API responses don't include descriptive error text.

- **Success Alert Philosophy**: Success notifications intentionally left to application layer control rather than automatic infrastructure-level feedback. This prevents alert fatigue from routine operations while allowing significant operations to explicitly trigger success feedback.

- **Stacking Physics**: Position-dependent stacking direction (bottom pushes up, top pushes down) matches user intuition about where new notifications should appear based on their screen position, creating natural visual flow.

### Future Enhancement Ideas

- **Alert Grouping**: Group related alerts (e.g., batch operation errors) into expandable alert showing count
- **Alert History/Center**: Notification center where users can review recently dismissed alerts
- **Custom Alert Templates**: Allow custom content beyond icon + text (progress bars, action buttons)
- **Alert Priorities**: Critical alerts stay longer or until manually dismissed
- **Persistent Alerts**: Certain alerts persist across navigation until acknowledged
- **Rich Content**: Support HTML or component projection for complex alert messages
- **Position Per Severity**: Configure default position per severity level (errors bottom-right, info top-center)
- **Sound Effects**: Optional audio feedback for different severity levels
- **Alert Analytics**: Track which alerts are most frequently displayed for UX improvements

</details>

---

## 🎨 Visual Design Reference

### Component Composition

Alert display leverages existing UI components:
- Animation wrapper component for entry/exit effects
- Icon and label component for content layout
- Styled icon component with semantic colors
- Icon button component for dismiss action

### Color Mapping

| Severity | Design Token | Light Mode | Dark Mode | Icon |
|----------|--------------|------------|-----------|------|
| Success | `--color-success` | `#86c691` | `#6fdc8c` | `check_circle` |
| Error | `--color-error` | `#cc666c` | `#ff6f6f` | `error` |
| Warning | `--color-warning` (new, aliases directory) | `#f5d76e` | `#f5e29e` | `warning` |
| Info | `--color-highlight` | `#00f7ff` | `#00f7ff` | `info` |

### Animation Specifications

- **Entry**: 400ms, `from-bottom` (bottom positions) or `from-top` (top positions)
- **Exit**: 300ms, matching entry direction
- **Stacking Shift**: 250ms cubic-bezier(0.4, 0, 0.2, 1) on transform
- **Auto-Dismiss**: 3 seconds (configurable)

### Positioning Grid

```
┌─────────────────────────────────────────┐
│ TL          TC          TR              │
│ ↓ push      ↓ push      ↓ push         │
│ down        down        down            │
│                                         │
│          Application Content            │
│                                         │
│ ↑ push      ↑ push      ↑ push         │
│ up          up          up              │
│ BL          BC          BR              │
└─────────────────────────────────────────┘

TL = Top-Left      TC = Top-Center      TR = Top-Right
BL = Bottom-Left   BC = Bottom-Center   BR = Bottom-Right
```

### Stacking Behavior

**Bottom Positions** (BL, BC, BR):
- New alerts appear at bottom of stack
- Existing alerts shift upward smoothly to make room
- Entry animation plays from bottom direction

**Top Positions** (TL, TC, TR):
- New alerts appear at top of stack
- Existing alerts shift downward smoothly to make room
- Entry animation plays from top direction

---

## ✨ Summary

This implementation plan provides a complete roadmap for building a production-ready alert system for TeensyROM. The plan:

✅ **Respects Clean Architecture** - Domain contracts, app-layer implementation, infrastructure integration
✅ **Leverages Existing Components** - Composes scaling-compact-card, icon-label, styled-icon, icon-button
✅ **Integrates Design System** - Uses design tokens, adds --color-warning alias
✅ **Follows Established Patterns** - Service error handling matches player.service.ts pattern
✅ **Phases Deliver Value** - Each phase produces working, demonstrable functionality
✅ **Testing Integrated Throughout** - Unit tests written during implementation, not deferred
✅ **User Decisions Incorporated** - All questions resolved and baked into the plan
✅ **Accessibility First** - ARIA roles, keyboard navigation, screen reader support
✅ **Performance Conscious** - Efficient state management, proper timer cleanup, smooth animations

The alert system will provide beautiful, consistent, accessible user feedback throughout the TeensyROM application while maintaining architectural cleanliness and following established patterns.
