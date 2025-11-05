# Phase 1: Core Alert Infrastructure & Domain Contracts

## 🎯 Objective

Establish the foundational alert system architecture by creating domain contracts, an app-layer alert service with internal signal-based state management, and basic alert display capabilities. This phase delivers a working alert system with functional rendering that can be consumed by any layer of the application.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**

- [ ] [Alert System Plan](./ALERT_SYSTEM_PLAN.md) - High-level feature plan and phases
- [ ] [Phase Template](../../PHASE_TEMPLATE.md) - Template structure for phase implementation

**Standards & Guidelines:**

- [ ] [Coding Standards](../../CODING_STANDARDS.md) - Angular 19 patterns, signals, modern control flow
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches by layer
- [ ] [NX Library Standards](../../NX_LIBRARY_STANDARDS.md) - Library creation and organization
- [ ] [Component Library](../../COMPONENT_LIBRARY.md) - Existing UI components to compose
- [ ] [Style Guide](../../STYLE_GUIDE.md) - Design tokens and utility classes
- [ ] [Overview Context](../../OVERVIEW_CONTEXT.md) - Clean Architecture and dependency rules

---

## 📂 File Structure Overview

```
libs/
├── domain/src/lib/
│   ├── contracts/
│   │   ├── alert.contract.ts                    ✨ New - IAlertService interface + token
│   │   └── index.ts                             📝 Modified - Export alert contract
│   └── models/
│       ├── alert-message.model.ts               ✨ New - AlertMessage interface
│       ├── alert-severity.enum.ts               ✨ New - AlertSeverity enum
│       ├── alert-position.enum.ts               ✨ New - AlertPosition enum
│       └── index.ts                             📝 Modified - Export alert models
│
├── app/alerts/                                  ✨ New - Alert app-layer library
│   ├── project.json                             ✨ New - Nx project configuration
│   ├── tsconfig.json                            ✨ New - TypeScript base config
│   ├── tsconfig.lib.json                        ✨ New - Library TypeScript config
│   ├── tsconfig.spec.json                       ✨ New - Test TypeScript config
│   ├── vite.config.mts                          ✨ New - Vite test configuration
│   └── src/
│       ├── index.ts                             ✨ New - Barrel exports
│       ├── test-setup.ts                        ✨ New - Test setup
│       └── lib/
│           ├── alert.service.ts                 ✨ New - Alert service implementation
│           ├── alert.service.spec.ts            ✨ New - Alert service tests
│           ├── alert-display.component.ts       ✨ New - Single alert display
│           ├── alert-display.component.html     ✨ New - Alert display template
│           ├── alert-display.component.scss     ✨ New - Alert display styles
│           ├── alert-display.component.spec.ts  ✨ New - Alert display tests
│           ├── alert-container.component.ts     ✨ New - Alert container with positioning
│           ├── alert-container.component.html   ✨ New - Container template
│           ├── alert-container.component.scss   ✨ New - Container styles
│           ├── alert-container.component.spec.ts ✨ New - Container tests
│           └── providers.ts                     ✨ New - DI provider bindings
│
└── app/shell/src/lib/layout/
    ├── layout.component.ts                      📝 Modified - Import AlertContainerComponent
    └── layout.component.html                    📝 Modified - Add alert-container

apps/teensyrom-ui/src/app/
└── app.config.ts                                📝 Modified - Register alert providers
```

---

<details open>
<summary><h3>Task 1: Create Domain Contracts and Models</h3></summary>

**Purpose**: Define the pure TypeScript contracts and models that establish the alert system's interface. This provides the abstraction that all layers can depend on while maintaining Clean Architecture boundaries.

**Related Documentation:**

- [Clean Architecture - Domain Layer](../../OVERVIEW_CONTEXT.md#1-domain-layer-libsdomain---pure-business-logic) - Domain layer responsibilities
- [Domain Contract Pattern](../../OVERVIEW_CONTEXT.md#dependency-injection-patterns) - Contract + injection token pattern

**Implementation Subtasks:**

- [ ] **Create `AlertSeverity` enum**: Define severity levels in `libs/domain/src/lib/models/alert-severity.enum.ts` with string values: `Success`, `Error`, `Warning`, `Info`
- [ ] **Create `AlertPosition` enum**: Define position options in `libs/domain/src/lib/models/alert-position.enum.ts` with string values: `TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter`, `BottomRight`
- [ ] **Create `AlertMessage` interface**: Define alert data structure in `libs/domain/src/lib/models/alert-message.model.ts` with properties for id, message, severity, position, and optional autoDismissMs
- [ ] **Create `IAlertService` contract**: Define service interface in `libs/domain/src/lib/contracts/alert.contract.ts` with methods: `show()`, `success()`, `error()`, `warning()`, `info()`, `dismiss()`, and `alerts$` observable property
- [ ] **Create `ALERT_SERVICE` injection token**: Add token to alert.contract.ts using InjectionToken pattern
- [ ] **Export from domain barrels**: Update `libs/domain/src/lib/models/index.ts` and `libs/domain/src/lib/contracts/index.ts` to export new files
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**

- [ ] **Write Tests**: No tests needed - domain contracts/models are pure TypeScript definitions used as mocks in other layers

**Key Implementation Notes:**

- All domain files must be pure TypeScript with zero dependencies (no Angular, no RxJS in models)
- Contract file can import RxJS Observable since it defines the service interface shape
- Follow existing domain contract pattern from device.contract.ts for consistency
- Each model/enum goes in its own file for tree-shaking and maintainability
- Use string enum values for better debugging and serialization
- AlertMessage interface should include: id (string), message (string), severity (AlertSeverity), position (AlertPosition), autoDismissMs (optional number)
- IAlertService should expose alerts$ as Observable of AlertMessage arrays, plus show/success/error/warning/info/dismiss methods

</details>

---

<details open>
<summary><h3>Task 2: Generate App Alerts Library</h3></summary>

**Purpose**: Create the new `libs/app/alerts` library following NX library standards with proper Clean Architecture tags. This library will contain the alert service implementation and UI components.

**Related Documentation:**

- [NX Library Standards - Library Creation](../../NX_LIBRARY_STANDARDS.md#library-creation-commands) - Library generation commands
- [NX Library Standards - Project Configuration](../../NX_LIBRARY_STANDARDS.md#projectjson-template) - Required project tags
- [Clean Architecture - App Layer](../../OVERVIEW_CONTEXT.md#4-presentation-layer-libsfeatures-libsui---user-interface) - App layer responsibilities

**Implementation Subtasks:**

- [ ] **Generate library**: Run Nx generator to create alerts library in libs/app directory with appropriate import path
- [ ] **Configure project tags**: Update project.json with tags: `scope:app` and `type:application`
- [ ] **Verify tsconfig paths**: Confirm tsconfig.base.json contains proper path mapping for @teensyrom-nx/app/alerts
- [ ] **Create barrel export**: Set up src/index.ts for public API exports (will export service, components, providers)
- [ ] **Configure Vite**: Ensure vite.config.mts is configured for Vitest testing with jsdom environment
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**

- [ ] **Write Tests**: No tests at this stage - library scaffolding only

**Key Implementation Notes:**

- Library lives in libs/app/ because alerts are a cross-cutting concern managed at the composition root (similar to shell, navigation, bootstrap)
- Use `scope:app` tag since this library can import from infrastructure layer (composition root responsibility)
- Library should be buildable=false for optimal build performance (consumed as source code)
- Follow the same structure as existing libs/app/bootstrap, libs/app/navigation, libs/app/shell
- Use Nx Angular library generator with appropriate flags for non-buildable, non-publishable library

</details>

---

<details open>
<summary><h3>Task 3: Implement Alert Service with Signal-Based State</h3></summary>

**Purpose**: Build the core alert service that manages alert state internally using signals, provides methods for showing alerts with different severity levels, implements auto-dismiss logic with configurable timing, and exposes alerts as an observable for the container component.

**Related Documentation:**

- [Coding Standards - Signals-First Policy](../../CODING_STANDARDS.md#signals‑first-policy) - Using signals for state
- [Infrastructure Service Pattern](../../OVERVIEW_CONTEXT.md#3-infrastructure-layer-libsinfrastructure---external-concerns) - Service implementation examples
- [Store Testing](../../STORE_TESTING.md) - Testing signal-based state (reference for patterns, not store-specific)

**Implementation Subtasks:**

- [ ] **Create `AlertService` class**: Implement in libs/app/alerts/src/lib/alert.service.ts with Injectable decorator (not providedIn root)
- [ ] **Implement private signal state**: Add private alertsSignal using signal with AlertMessage array type for internal queue management
- [ ] **Expose alerts observable**: Create public alerts$ property using toObservable() for reactive subscriptions
- [ ] **Implement `show()` method**: Accept message, severity, position, optional autoDismissMs; generate unique ID using crypto.randomUUID(); add to signal; start auto-dismiss timer
- [ ] **Implement convenience methods**: Create success(), error(), warning(), info() methods that call show() with appropriate severity and default position (BottomRight)
- [ ] **Implement `dismiss()` method**: Remove alert by ID from signal using update(); cancel associated auto-dismiss timer
- [ ] **Implement auto-dismiss logic**: Use setTimeout() to trigger dismiss after configured ms (default 3000); store timer refs in Map for cleanup
- [ ] **Implement timer cleanup**: Clear timer in dismiss() method to prevent memory leaks; handle manual dismiss canceling auto-dismiss
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**

- [ ] **Write Tests**: Test alert service operations (see Testing Focus below)

**Key Implementation Notes:**

- Service uses internal signal state rather than NgRx store for simplicity
- Use crypto.randomUUID() for generating unique alert IDs
- Store timer references in private Map<string, number> for proper cleanup
- Default auto-dismiss to 3000ms but allow override per alert
- Service should NOT be providedIn: 'root' - will be registered via providers in app.config
- Follow RxJS patterns from device.service.ts for observable exposure
- Use signal.update() to add/remove alerts immutably from array
- Each show() call should create a new AlertMessage object with all required properties

**Testing Focus for Task 3:**

**Behaviors to Test:**

- [ ] **show() adds alert to signal**: Verify alert appears in alerts$ observable with correct properties
- [ ] **success() creates success alert**: Verify severity is 'success' and default position is 'bottom-right'
- [ ] **error() creates error alert**: Verify severity is 'error' and default position is 'bottom-right'
- [ ] **warning() creates warning alert**: Verify severity is 'warning' and default position is 'bottom-right'
- [ ] **info() creates info alert**: Verify severity is 'info' and default position is 'bottom-right'
- [ ] **dismiss() removes alert**: Verify alert is removed from alerts$ observable
- [ ] **auto-dismiss triggers after timeout**: Use Vitest fake timers (vi.useFakeTimers(), vi.advanceTimersByTime()) to verify alert is removed after configured ms
- [ ] **manual dismiss cancels auto-dismiss**: Verify calling dismiss() before timer expires clears the timer and prevents duplicate removal
- [ ] **multiple alerts can exist**: Verify multiple alerts can be added and tracked independently with separate timers
- [ ] **custom auto-dismiss duration works**: Verify custom autoDismissMs parameter overrides default 3000ms
- [ ] **Check off completed tests**: Mark each test above as complete in this document as you verify it passes

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for unit testing approach
- Use Vitest fake timers: `vi.useFakeTimers()`, `vi.advanceTimersByTime(3000)`, `vi.restoreAllTimers()`

</details>

---

<details open>
<summary><h3>Task 4: Build Alert Display Component</h3></summary>

**Purpose**: Create a component that renders an individual alert message with icon, text content, and dismiss button, applying basic functional styling without semantic colors.

**Related Documentation:**

- [Coding Standards - Component Structure](../../CODING_STANDARDS.md#component-structure) - Component organization
- [Coding Standards - Modern Control Flow](../../CODING_STANDARDS.md#template-syntax) - @if/@for syntax
- [Component Library - IconButtonComponent](../../COMPONENT_LIBRARY.md#iconbuttoncomponent) - Dismiss button pattern

**Implementation Subtasks:**

- [ ] **Create component class**: Implement AlertDisplayComponent in libs/app/alerts/src/lib/alert-display.component.ts with standalone configuration
- [ ] **Define inputs**: Add alert input using input.required<AlertMessage>() for alert data
- [ ] **Define output**: Add dismissed output using output<string>() that emits alert ID when dismiss button clicked
- [ ] **Import dependencies**: Add CommonModule, MatIconModule, MatButtonModule to component imports array
- [ ] **Create template**: Build alert-display.component.html with div containing icon, message text span, and dismiss button
- [ ] **Add icon mapping**: Create computed signal that maps AlertSeverity to Material icon names (success → check_circle, error → error, warning → warning, info → info)
- [ ] **Add dismiss handler**: Create onDismiss() method that emits dismissed output with alert ID from input
- [ ] **Create basic styles**: Add functional layout styles in alert-display.component.scss (flexbox horizontal layout, spacing, no semantic colors yet)
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**

- [ ] **Write Tests**: Test alert display component rendering (see Testing Focus below)

**Key Implementation Notes:**

- Component is purely presentational - no business logic or service dependencies
- Use input.required<AlertMessage>() since alert is always provided by parent container
- Template should display icon, message text, and dismiss button in horizontal flexbox layout
- Basic styling should be functional only - semantic colors deferred to Phase 2
- Follow existing component patterns from device-item.component.ts for structure
- Use Material icon button for dismiss action with 'close' icon and proper aria-label
- Icon computed signal should read alert().severity and return appropriate icon string
- Dismiss handler should read alert().id and emit it through dismissed output

**Testing Focus for Task 4:**

**Behaviors to Test:**

- [ ] **Renders alert message**: Verify alert message text is displayed in the template
- [ ] **Renders correct icon**: Verify icon matches severity (success → check_circle, error → error, warning → warning, info → info)
- [ ] **Dismiss button emits event**: Verify clicking dismiss button emits dismissed output with correct alert ID
- [ ] **Displays dismiss button**: Verify close icon button is rendered with proper aria-label
- [ ] **Check off completed tests**: Mark each test above as complete in this document as you verify it passes

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for UI component testing approach

</details>

---

<details open>
<summary><h3>Task 5: Build Alert Container Component</h3></summary>

**Purpose**: Create a fixed-position container component that manages alert positioning, renders active alerts, and handles the six screen position options for alert placement.

**Related Documentation:**

- [Coding Standards - Signals-First Policy](../../CODING_STANDARDS.md#signals‑first-policy) - Using signals for reactive data
- [Component Library - Positioning Patterns](../../COMPONENT_LIBRARY.md#layout-components) - Fixed positioning examples

**Implementation Subtasks:**

- [ ] **Create component class**: Implement AlertContainerComponent in libs/app/alerts/src/lib/alert-container.component.ts with standalone configuration
- [ ] **Inject alert service**: Inject ALERT_SERVICE token using inject() function
- [ ] **Subscribe to alerts**: Convert alertService.alerts$ to signal using toSignal() with empty array default
- [ ] **Group alerts by position**: Create computed signal that groups alerts by AlertPosition into six groups (one per position enum value)
- [ ] **Handle dismiss events**: Create onAlertDismissed(id: string) method that calls alertService.dismiss(id)
- [ ] **Create template**: Build alert-container.component.html with 6 fixed-position divs for each position
- [ ] **Render alerts by position**: Use @for loops to render AlertDisplayComponent for each alert in each position group, tracking by alert.id
- [ ] **Add positioning styles**: Create alert-container.component.scss with position: fixed and appropriate top/bottom/left/right/center values for all 6 screen locations
- [ ] **Add stacking layout**: Configure flexbox direction for each position (column for top positions, column-reverse for bottom positions so new alerts appear at bottom)
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**

- [ ] **Write Tests**: Test alert container component (see Testing Focus below)

**Key Implementation Notes:**

- Container is fixed-position overlay that doesn't affect document flow
- Use CSS position: fixed with appropriate coordinate values for each of 6 positions
- Bottom positions should use flex-direction: column-reverse so new alerts appear at bottom
- Top positions should use flex-direction: column so new alerts appear at top
- Component acts as bridge between alert service and display components
- Follow layout patterns from layout.component.ts for structure
- Use z-index to ensure alerts appear above other content (suggest z-index: 1000)
- Computed signal for grouping should iterate alerts and create object with position keys
- Template should iterate over position enum values, render position div, then iterate alerts in that position group
- Pass onAlertDismissed handler to each AlertDisplayComponent via (dismissed) event binding

**Testing Focus for Task 5:**

**Behaviors to Test:**

- [ ] **Renders alerts from service**: Verify alerts from service appear in container
- [ ] **Groups alerts by position**: Verify alerts are grouped into correct position containers based on position enum value
- [ ] **Dismiss handler calls service**: Verify onAlertDismissed() calls alertService.dismiss() with correct ID
- [ ] **Position containers exist**: Verify 6 position divs are rendered in template
- [ ] **Updates when alerts change**: Verify container updates reactively when alerts signal changes (add/remove alerts)
- [ ] **Check off completed tests**: Mark each test above as complete in this document as you verify it passes

**Testing Reference:**

- See [Testing Standards](../../TESTING_STANDARDS.md) for UI component testing with mocked services

</details>

---

<details open>
<summary><h3>Task 6: Create Provider Bindings and Register Service</h3></summary>

**Purpose**: Configure dependency injection to bind the IAlertService contract to the AlertService implementation, making the service available application-wide as a singleton.

**Related Documentation:**

- [Clean Architecture - Dependency Injection](../../OVERVIEW_CONTEXT.md#dependency-injection-patterns) - DI provider patterns
- [Infrastructure Providers Example](../../OVERVIEW_CONTEXT.md#3-infrastructure-layer-libsinfrastructure---external-concerns) - Provider configuration examples

**Implementation Subtasks:**

- [ ] **Create providers file**: Create libs/app/alerts/src/lib/providers.ts following device providers pattern from infrastructure layer
- [ ] **Define ALERT_SERVICE_PROVIDER**: Export provider object that binds ALERT_SERVICE token to AlertService class using provide/useClass pattern
- [ ] **Export from barrel**: Add provider export to libs/app/alerts/src/index.ts for public API access
- [ ] **Register in app.config**: Import and add ALERT_SERVICE_PROVIDER to providers array in apps/teensyrom-ui/src/app/app.config.ts
- [ ] **Verify DI registration**: Confirm service can be injected via ALERT_SERVICE token in any component/service (test by injecting in a component)
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**

- [ ] **Write Tests**: No tests needed - configuration only

**Key Implementation Notes:**

- Follow exact pattern from libs/infrastructure/src/lib/device/providers.ts for consistency
- Provider should use useClass to bind interface to implementation
- No factory function needed for alert service (unlike API clients which need configuration)
- Service will be singleton by virtue of being in root providers array
- App.config is composition root where all infrastructure and app-layer services are wired together
- Import ALERT_SERVICE from domain contracts, AlertService from alert.service.ts
- Provider object should have provide and useClass properties only

</details>

---

<details open>
<summary><h3>Task 7: Integrate Alert Container into App Shell</h3></summary>

**Purpose**: Add the AlertContainerComponent to the app shell layout as a fixed-position sibling to the router-outlet, making alerts visible throughout the entire application.

**Related Documentation:**

- [App Shell Layout](../../OVERVIEW_CONTEXT.md#4-presentation-layer-libsfeatures-libsui---user-interface) - Shell layout responsibilities
- [Component Library - Layout Components](../../COMPONENT_LIBRARY.md#layout-components) - Layout composition patterns

**Implementation Subtasks:**

- [ ] **Import AlertContainerComponent**: Add import statement to libs/app/shell/src/lib/layout/layout.component.ts
- [ ] **Add to component imports**: Include AlertContainerComponent in LayoutComponent imports array
- [ ] **Add to template**: Insert alert-container element selector in layout.component.html after mat-sidenav-container closing tag
- [ ] **Verify rendering**: Confirm alert container renders in app layout (empty initially since no alerts, but component should be in DOM)
- [ ] **Check off completed subtasks**: Mark each subtask above as complete in this document as you finish it

**Testing Subtask:**

- [ ] **Write Tests**: Update layout.component.spec.ts to test container integration (see Testing Focus below)

**Key Implementation Notes:**

- Alert container should be outside mat-sidenav-container to avoid layout conflicts
- Container uses fixed positioning so it won't affect existing layout flow
- No styling changes needed to layout component - alert container manages its own positioning
- Follow pattern of how HeaderComponent and NavMenuComponent are integrated into layout
- Container should be at root level of layout template to ensure alerts appear above all content
- Layout template should have mat-sidenav-container, then alert-container as sibling elements

**Testing Focus for Task 7:**

**Behaviors to Test:**

- [ ] **Alert container renders**: Verify alert-container element is present in compiled template DOM
- [ ] **Layout still functions**: Verify adding alert container doesn't break existing layout behavior (navigation, header rendering)
- [ ] **Check off completed tests**: Mark each test above as complete in this document as you verify it passes

**Testing Reference:**

- Update existing layout.component.spec.ts to include alert service mock in TestBed configuration
- Mock ALERT_SERVICE with empty alerts$ observable for testing

</details>

---

## 🗂️ Files Modified or Created

**New Files:**

- `libs/domain/src/lib/models/alert-message.model.ts`
- `libs/domain/src/lib/models/alert-severity.enum.ts`
- `libs/domain/src/lib/models/alert-position.enum.ts`
- `libs/domain/src/lib/contracts/alert.contract.ts`
- `libs/app/alerts/project.json`
- `libs/app/alerts/tsconfig.json`
- `libs/app/alerts/tsconfig.lib.json`
- `libs/app/alerts/tsconfig.spec.json`
- `libs/app/alerts/vite.config.mts`
- `libs/app/alerts/src/index.ts`
- `libs/app/alerts/src/test-setup.ts`
- `libs/app/alerts/src/lib/alert.service.ts`
- `libs/app/alerts/src/lib/alert.service.spec.ts`
- `libs/app/alerts/src/lib/alert-display.component.ts`
- `libs/app/alerts/src/lib/alert-display.component.html`
- `libs/app/alerts/src/lib/alert-display.component.scss`
- `libs/app/alerts/src/lib/alert-display.component.spec.ts`
- `libs/app/alerts/src/lib/alert-container.component.ts`
- `libs/app/alerts/src/lib/alert-container.component.html`
- `libs/app/alerts/src/lib/alert-container.component.scss`
- `libs/app/alerts/src/lib/alert-container.component.spec.ts`
- `libs/app/alerts/src/lib/providers.ts`

**Modified Files:**

- `libs/domain/src/lib/models/index.ts`
- `libs/domain/src/lib/contracts/index.ts`
- `libs/app/shell/src/lib/layout/layout.component.ts`
- `libs/app/shell/src/lib/layout/layout.component.html`
- `libs/app/shell/src/lib/layout/layout.component.spec.ts`
- `apps/teensyrom-ui/src/app/app.config.ts`
- `tsconfig.base.json` (auto-updated by Nx generator)

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> Tests are written **within each task above**, not here. This section is only a summary for quick reference.

**Testing Approach**: Unit tests for service and components using Vitest with jsdom environment. Mock domain contracts and test observable behaviors.

### Test Coverage by Component

**AlertService (Task 3)**:

- Show methods add alerts to signal correctly
- Convenience methods (success/error/warning/info) create appropriate alerts
- Dismiss removes alert from signal
- Auto-dismiss triggers after configured timeout (use Vitest fake timers)
- Manual dismiss cancels auto-dismiss timer
- Multiple alerts managed independently

**AlertDisplayComponent (Task 4)**:

- Renders alert message text
- Displays correct icon for severity
- Dismiss button emits event with alert ID
- Component renders all visual elements

**AlertContainerComponent (Task 5)**:

- Subscribes to alert service and displays alerts
- Groups alerts by position correctly
- Dismiss handler calls service method
- Container updates reactively when alerts change

**LayoutComponent Integration (Task 7)**:

- Alert container renders in layout
- Layout functionality unaffected by alert container addition

### Test Execution Commands

```bash
# Run alert library tests
npx nx test app-alerts

# Run tests in watch mode
npx nx test app-alerts --watch

# Run all tests
npx nx run-many --target=test --all

# Run with coverage
npx nx test app-alerts --coverage
```

### Testing Tools

- **Framework**: Vitest with jsdom environment
- **Timer Mocking**: `vi.useFakeTimers()`, `vi.advanceTimersByTime()`, `vi.restoreAllTimers()`
- **Angular Testing**: TestBed for component testing
- **Observable Testing**: RxJS testing utilities with toSignal for reactive testing

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**

- [ ] All implementation tasks completed and checked off
- [ ] Domain contracts created in libs/domain (IAlertService, AlertMessage, enums)
- [ ] Alert library generated in libs/app/alerts with proper tags (scope:app, type:application)
- [ ] AlertService implements contract with signal-based state management
- [ ] AlertDisplayComponent renders individual alerts functionally
- [ ] AlertContainerComponent manages positioning and renders alerts
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md) (signals, modern control flow, component structure)

**Testing Requirements:**

- [ ] All testing subtasks completed within each task
- [ ] AlertService tests verify show/dismiss/auto-dismiss behaviors with Vitest fake timers
- [ ] AlertDisplayComponent tests verify rendering and dismiss event emission
- [ ] AlertContainerComponent tests verify service integration and position grouping
- [ ] All tests passing with no failures
- [ ] Test coverage meets project standards (aim for >90%)

**Integration Requirements:**

- [ ] Alert service registered in app.config.ts providers
- [ ] Alert container integrated into app shell layout
- [ ] Service can be injected via ALERT_SERVICE token in any component
- [ ] Alerts display when service methods are called programmatically
- [ ] Auto-dismiss removes alerts after configured timeout

**Quality Checks:**

- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors (`npx nx lint app-alerts`)
- [ ] ESLint module boundaries enforced (no forbidden layer dependencies)
- [ ] Code formatting is consistent
- [ ] No console errors in browser when running application

**Documentation:**

- [ ] Inline code comments added for complex logic (timer management, position grouping)
- [ ] Public service methods documented with JSDoc
- [ ] Component inputs/outputs have descriptive names
- [ ] All task checkboxes in this document marked complete

**Ready for Phase 2:**

- [ ] All Phase 1 success criteria met
- [ ] Basic alert system functional (can show/dismiss alerts programmatically)
- [ ] No known bugs or memory leaks (timer cleanup verified in tests)
- [ ] Foundation ready for visual design and animations (Phase 2)

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **App Layer Placement**: Alert system lives in libs/app/alerts as a cross-cutting concern that coordinates application-wide notifications, similar to shell/navigation/bootstrap patterns. This allows it to be a composition root concern that can coordinate across features.

- **Signal-Based State**: Alert service uses internal signal state rather than NgRx store to reduce complexity for this focused use case. Signals provide reactive updates without store overhead while maintaining clean observable exposure via toObservable().

- **Timer Management**: Auto-dismiss uses native setTimeout() with timer reference tracking in a Map. Timers are properly cleaned up in dismiss method to prevent memory leaks.

- **Severity Defaults**: Convenience methods (success/error/warning/info) default to BottomRight position for consistency, but allow override via optional parameter.

- **Basic Styling**: Phase 1 focuses on functional rendering without semantic colors or animations. Visual polish deferred to Phase 2 to maintain clear phase boundaries.

### Implementation Constraints

- **No Semantic Colors Yet**: Phase 1 uses basic functional styling. Semantic color system (--color-success, --color-error, etc.) will be integrated in Phase 2.

- **No Animations Yet**: Entry/exit animations, stacking transitions, and directional movement deferred to Phase 2. Phase 1 alerts appear/disappear instantly.

- **No Rate Limiting**: System handles unlimited rapid alerts efficiently. No deduplication or throttling implemented per plan requirements.

- **Testing Timer Limitations**: Vitest fake timers used for testing auto-dismiss. Real-time testing not needed since timer behavior is deterministic.

### Future Enhancements (Phase 2+)

- **Semantic Color System**: Map severity to design tokens (success → green, error → red, warning → yellow, info → cyan)
- **Animation System**: Integrate lib-scaling-compact-card for entry/exit animations with directional movement
- **Physics-Based Stacking**: Smooth transitions when alerts are added/removed with position-dependent stacking direction
- **Infrastructure Integration**: Add automatic error handling to all infrastructure services with alert display

### External References

- [Alert System Plan](./ALERT_SYSTEM_PLAN.md) - Complete feature plan with all phases
- [Phase Template](../../PHASE_TEMPLATE.md) - Template structure for phase documents
- [Clean Architecture Overview](../../OVERVIEW_CONTEXT.md) - Layer responsibilities and dependency rules

</details>

---

## 💡 Agent Implementation Instructions

**Critical Workflow**:

1. Read all required documentation before starting
2. Complete tasks in order (1 → 7)
3. **Mark each subtask checkbox as complete in this document** as you finish it
4. Write tests within each task (not deferred to the end)
5. Run tests and verify they pass before moving to next task
6. Update success criteria checkboxes as you meet requirements
7. Document any discoveries or deviations in Notes section

**Progress Tracking**:

- Mark subtask checkboxes ✅ as you complete each implementation step
- Mark test checkboxes ✅ as you verify each test passes
- Mark success criteria ✅ as you meet each requirement
- Keep this document as the single source of truth for phase progress

---

**Phase 1 Complete!** 🎉 This phase delivers a working alert system with domain contracts, service implementation with signal-based state, basic UI components, and app shell integration. The foundation is ready for Phase 2's visual design, semantic colors, and animations.
