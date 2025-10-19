# Phase 3: Visual Design, Positioning, and Animations

## 🎯 Objective

Transform the basic alert display from Phase 2 into a beautiful, polished notification system with six positioning options, semantic color coding using design tokens, smooth directional entry/exit animations, and physics-based stacking behavior where alerts push existing messages according to their position. This phase enhances the working system with production-ready visual design that provides clear, non-intrusive feedback through color-coded animations and intelligent positioning.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Feature Documentation:**
- [ ] [Alert System Plan](./ALERT_SYSTEM_PLAN.md) - High-level feature plan and complete architecture
- [ ] [Phase 2 Documentation](./ALERT_SYSTEM_P2.md) - Infrastructure integration completed in Phase 2
- [ ] [Phase Template](../../PHASE_TEMPLATE.md) - Template structure for phase implementation

**Standards & Guidelines:**
- [ ] [Component Library](../../COMPONENT_LIBRARY.md) - Animation components and usage patterns
- [ ] [Style Guide](../../STYLE_GUIDE.md) - Design tokens, color system, and utility classes
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - Angular 19 patterns and component conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Behavioral testing approaches

---

## 📂 File Structure Overview

```
libs/app/src/lib/
├── alert-display.component.ts                📝 Modified - Replace with lib-scaling-compact-card
├── alert-display.component.html              📝 Modified - Use animation components and icon-label
├── alert-display.component.scss              📝 Modified - Add semantic color styling
├── alert-container.component.ts              📝 Modified - Add positioning logic
├── alert-container.component.html            📝 Modified - Implement six position containers
├── alert-container.component.scss            📝 Modified - Add fixed positioning and stacking styles

libs/ui/styles/src/lib/theme/
└── styles.scss                               📝 Modified - Add --color-warning design token

docs/
├── COMPONENT_LIBRARY.md                      📝 Modified - Document alert components
└── STYLE_GUIDE.md                            📝 Modified - Document warning color token
```

---

<details open>
<summary><h3>Task 1: Add Warning Color Design Token to Style Guide</h3></summary>

**Purpose**: Create a new warning color design token that aliases the existing directory yellow/amber color values, ensuring consistency between warning alerts and directory highlighting while supporting both light and dark themes.

**Related Documentation:**
- [Style Guide - Custom Color Variables](../../STYLE_GUIDE.md#custom-color-variables) - Existing design token patterns
- [Alert System Plan - Phase 3](./ALERT_SYSTEM_PLAN.md#phase-3-visual-design-positioning-and-animations) - Warning color requirements

**Implementation Subtasks:**
- [ ] **Add `--color-warning` variable in light theme**: Set to same value as `--color-directory` in light mode (#f5d76e)
- [ ] **Add `--color-warning` variable in dark theme**: Set to same value as `--color-directory` in dark mode (#f5e29e)
- [ ] **Document warning color in style guide**: Add entry to Custom Color Variables section explaining warning color aliases directory color for consistency
- [ ] **Verify color consistency**: Confirm warning color matches directory color values in both themes

**Testing Subtask:**
- [ ] **Manual Visual Testing**: Verify warning color renders correctly (see Testing section below)

**Key Implementation Notes:**
- Warning color intentionally aliases directory color to maintain existing yellow/amber semantic meaning
- Both light and dark theme values are defined in `styles.scss` within their respective theme blocks
- Color will be applied to warning alerts via severity-based styling in later tasks
- No new color values needed - reuse existing directory color hexadecimal values

**Testing Focus for Task 1:**

> Visual validation in browser DevTools

**Manual Testing:**
- [ ] **Light theme warning color**: Inspect CSS variables, verify `--color-warning` equals directory color
- [ ] **Dark theme warning color**: Switch to dark theme, verify `--color-warning` equals directory color
- [ ] **Color match with directory**: Confirm warning and directory colors are identical in both themes

**Testing Reference:**
- Use browser DevTools to inspect CSS custom property values
- Test by temporarily applying warning color to a test element

</details>

---

<details open>
<summary><h3>Task 2: Implement Severity-Based Color Styling in Alert Display Component</h3></summary>

**Purpose**: Apply semantic color coding to alert display component using design tokens, ensuring success alerts are green, error alerts are red, warning alerts are yellow/amber, and info alerts are cyan. This provides instant visual feedback about alert severity without requiring users to read the message.

**Related Documentation:**
- [Component Library - IconLabelComponent](../../COMPONENT_LIBRARY.md#iconlabelcomponent) - Icon and label composition patterns
- [Component Library - StyledIconComponent](../../COMPONENT_LIBRARY.md#stylediconcomponent) - Semantic icon color system
- [Style Guide - Custom Color Variables](../../STYLE_GUIDE.md#custom-color-variables) - Design token mapping

**Implementation Subtasks:**
- [ ] **Replace basic icon rendering**: Use `lib-icon-label` component to compose styled icon and message text
- [ ] **Add icon color mapping computed signal**: Create `iconColor()` computed that maps severity to `StyledIconColor` type (success, error, warning, info maps to highlight)
- [ ] **Apply severity classes to container**: Add class binding that applies severity-specific CSS class to alert container element
- [ ] **Create severity color CSS classes**: Add four classes in component SCSS that reference design tokens
- [ ] **Map warning to new design token**: Use `var(--color-warning)` in warning severity class
- [ ] **Map other severities to existing tokens**: Success uses `--color-success`, error uses `--color-error`, info uses `--color-highlight`

**Testing Subtask:**
- [ ] **Write Tests**: Test severity color mapping (see Testing section below)

**Key Implementation Notes:**
- `lib-icon-label` provides consistent icon-text composition with proper spacing
- Warning severity maps to new `--color-warning` token added in Task 1
- Info severity uses `--color-highlight` (cyan) for informational messages
- Severity classes can style borders, backgrounds, or other visual elements in addition to icon color
- Color mapping follows established patterns from other components (device status, file types, etc.)
- Icon color mapping returns StyledIconColor type values for type safety

**Testing Focus for Task 2:**

**Behaviors to Test:**
- [ ] **Success alert renders green**: Alert with Success severity displays check_circle icon with success color
- [ ] **Error alert renders red**: Alert with Error severity displays error icon with error color
- [ ] **Warning alert renders yellow**: Alert with Warning severity displays warning icon with warning color from new design token
- [ ] **Info alert renders cyan**: Alert with Info severity displays info icon with highlight color
- [ ] **Severity classes applied**: Alert container element receives correct severity-specific CSS class
- [ ] **Icon-label integration**: Message text renders alongside icon with proper spacing

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for component testing approach
- Mock AlertMessage with different severities to test color mapping
- Verify computed signal returns correct StyledIconColor for each severity

</details>

---

<details open>
<summary><h3>Task 3: Implement Six-Position Fixed Layout in Alert Container</h3></summary>

**Purpose**: Create fixed-position containers for each of the six alert positions (top-left, top-center, top-right, bottom-left, bottom-center, bottom-right), establishing the foundation for position-aware stacking behavior. This task focuses purely on layout and positioning without animations.

**Related Documentation:**
- [Alert System Plan - Positioning Options](./ALERT_SYSTEM_PLAN.md#phase-3-visual-design-positioning-and-animations) - Six position specifications
- [Style Guide - Layout Patterns](../../STYLE_GUIDE.md) - Fixed positioning conventions

**Implementation Subtasks:**
- [ ] **Add six position containers in template**: Create container div for each `AlertPosition` enum value using `@for` loop over `positions` array
- [ ] **Apply fixed positioning CSS**: Add `position: fixed` with appropriate `top`/`bottom` and `left`/`right` values for each position class
- [ ] **Set z-index for layering**: Apply high z-index (e.g., 9999) to ensure alerts appear above all other content
- [ ] **Add padding and spacing**: Set consistent padding (e.g., 1rem) from viewport edges for all positions
- [ ] **Create position class mapping**: Add method `getPositionClass(position: AlertPosition)` that returns CSS class name for each position
- [ ] **Bind position class to containers**: Apply computed position class to each container div via class binding
- [ ] **Configure flexbox layout**: Set `display: flex` on all position containers for vertical stacking
- [ ] **Add gap between stacked alerts**: Apply gap property (e.g., 0.5rem) for consistent spacing between stacked alerts

**Testing Subtask:**
- [ ] **Manual Visual Testing**: Verify positioning layout (see Testing section below)

**Key Implementation Notes:**
- Don't configure flex-direction yet - that comes in Task 4 with stacking direction
- Center positions use left 50% with translateX for true horizontal centering
- Each position container manages its own subset of alerts filtered by position property
- Fixed positioning removes containers from document flow, preventing layout shifts in main content
- High z-index ensures alerts always appear on top of other UI elements
- Template uses iteration over positions array to render container for each position
- Alerts are filtered per-position using existing `alertsByPosition()` computed signal

**Testing Focus for Task 3:**

**Manual Testing Checklist:**
- [ ] **Top-left alerts appear in top-left corner**: Verify position and spacing from edges
- [ ] **Top-center alerts center horizontally**: Confirm exact center alignment
- [ ] **Top-right alerts appear in top-right corner**: Verify right-alignment
- [ ] **Bottom-left alerts appear in bottom-left corner**: Verify bottom-left positioning
- [ ] **Bottom-center alerts center horizontally at bottom**: Confirm exact center at bottom
- [ ] **Bottom-right alerts appear in bottom-right corner**: Verify bottom-right positioning
- [ ] **Alerts don't overlap main content**: Confirm z-index layering is correct
- [ ] **Consistent spacing from viewport edges**: All positions have same padding
- [ ] **Multiple alerts at same position stack vertically**: Gap between alerts is consistent
- [ ] **Center positions stay centered on resize**: Resize viewport, verify centering remains correct

**Testing Approach:**
- Use alert service methods to trigger test alerts at each position
- Use browser DevTools to inspect computed CSS values
- Resize viewport to verify responsive centering for center positions
- Trigger multiple alerts at same position to verify stacking and gap

</details>

---

<details open>
<summary><h3>Task 4: Configure Position-Based Stacking Direction</h3></summary>

**Purpose**: Configure flexbox flex-direction for each position container so that bottom positions stack upward (new alerts appear at bottom) and top positions stack downward (new alerts appear at top). This creates intuitive stacking behavior without animations yet.

**Related Documentation:**
- [Alert System Plan - Stacking Behavior](./ALERT_SYSTEM_PLAN.md#phase-3-visual-design-positioning-and-animations) - Position-dependent stacking specification
- [Style Guide - Flexbox Patterns](../../STYLE_GUIDE.md) - Layout conventions

**Implementation Subtasks:**
- [ ] **Add column flex-direction to top position classes**: Set in top-left, top-center, top-right CSS classes so newest alert appears at top
- [ ] **Add column-reverse flex-direction to bottom position classes**: Set in bottom-left, bottom-center, bottom-right CSS classes so newest alert appears at bottom
- [ ] **Verify top stacking behavior**: When multiple alerts added to top position, newest appears at top and pushes others downward
- [ ] **Verify bottom stacking behavior**: When multiple alerts added to bottom position, newest appears at bottom and pushes others upward
- [ ] **Test stacking order at all positions**: Trigger 3-4 alerts rapidly at each position, verify stacking direction matches expectation

**Testing Subtask:**
- [ ] **Manual Visual Testing**: Verify stacking direction (see Testing section below)

**Key Implementation Notes:**
- Column flex-direction makes items stack vertically with first item at top (natural DOM order)
- Column-reverse reverses stacking so first item appears at bottom, last item at top
- Since alerts are added to array chronologically, column-reverse makes newest alert appear at bottom for bottom positions
- No JavaScript changes needed - purely CSS configuration
- Stacking direction is set once per position class, not computed per alert
- This creates the foundation for stacking behavior; smooth transitions added later in Task 6

**Testing Focus for Task 4:**

**Manual Testing Checklist:**
- [ ] **Top-left stacking**: First alert appears at top, second alert appears below it, third below that
- [ ] **Top-center stacking**: Newest alert at top, older alerts pushed downward
- [ ] **Top-right stacking**: Same behavior - newest at top
- [ ] **Bottom-left stacking**: First alert appears at bottom, second alert appears above it, third above that
- [ ] **Bottom-center stacking**: Newest alert at bottom, older alerts pushed upward
- [ ] **Bottom-right stacking**: Same behavior - newest at bottom
- [ ] **Stacking order correct**: Alert order reflects chronological order (can verify by message content)
- [ ] **No overlap**: Alerts stack cleanly with gap between them, no z-index conflicts

**Testing Approach:**
- Trigger 3 alerts rapidly at each position with different messages
- Observe stacking order and direction
- Verify newest alert appears in expected location (top for top positions, bottom for bottom positions)
- Confirm older alerts shift in correct direction when new alert arrives

</details>

---

<details open>
<summary><h3>Task 5: Replace Alert Display with Scaling Compact Card Animation</h3></summary>

**Purpose**: Enhance alert display component by composing the `lib-scaling-compact-card` animation component, enabling smooth directional entry/exit animations. This task adds visual polish to the working color/positioning system built in previous tasks.

**Related Documentation:**
- [Component Library - ScalingCompactCardComponent](../../COMPONENT_LIBRARY.md#scalingcompactcardcomponent) - Animation component API and usage
- [Component Library - Animation System](../../COMPONENT_LIBRARY.md#animation-system) - Animation trigger and direction patterns

**Implementation Subtasks:**
- [ ] **Add `animationTrigger` input**: Create required input signal that receives visibility state (boolean) from parent
- [ ] **Add `animationEntry` input**: Create input signal that receives entry direction string (from-top or from-bottom)
- [ ] **Add `animationExit` input**: Create input signal that receives exit direction string (same as entry)
- [ ] **Replace template root with lib-scaling-compact-card**: Wrap existing alert content in scaling-compact-card component
- [ ] **Bind animation inputs to card**: Pass animationTrigger, animationEntry, animationExit inputs through to scaling-compact-card
- [ ] **Set animation duration**: Configure animationDuration to 400ms for entry animations
- [ ] **Project content into card**: Move existing content (icon-label, dismiss button) into default content slot
- [ ] **Remove any component-level animation code**: Delete any existing animation CSS since scaling-compact-card handles it
- [ ] **Update component imports**: Add ScalingCompactCardComponent to component imports array

**Testing Subtask:**
- [ ] **Write Tests**: Test animation integration (see Testing section below)

**Key Implementation Notes:**
- `lib-scaling-compact-card` provides scale, fade, and slide animations automatically
- `animationTrigger` controls visibility with smooth exit animations before DOM removal
- Entry/exit directions will be passed from parent container based on alert position
- Exit animation duration is handled by scaling-compact-card internally (300ms)
- Component must import ScalingCompactCardComponent from `@teensyrom-nx/ui/components`
- Existing severity color classes and dismiss button functionality remain unchanged

**Testing Focus for Task 5:**

**Behaviors to Test:**
- [ ] **Component accepts animationTrigger input**: Input signal binds correctly and controls visibility
- [ ] **Component accepts animationEntry input**: Entry direction input binds to scaling-compact-card
- [ ] **Component accepts animationExit input**: Exit direction input binds to scaling-compact-card
- [ ] **Alert content renders in card**: Icon-label and dismiss button project into card correctly
- [ ] **Severity colors still apply**: Existing color classes still work after wrapping in card
- [ ] **Dismiss button functional**: Dismiss event still emits when button clicked

**Testing Reference:**
- See [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) for component input testing
- Mock animation inputs in tests to verify binding
- Test dismiss button event emission after template restructuring

</details>

---

<details open>
<summary><h3>Task 6: Implement Directional Animations and Stacking Transitions</h3></summary>

**Purpose**: Configure alert container to pass appropriate animation directions to alert displays based on position, and add smooth CSS transitions for stacking shifts. This completes the animation system with position-aware directional animations and physics-based stacking motion.

**Related Documentation:**
- [Component Library - ScalingContainerComponent Animation Directions](../../COMPONENT_LIBRARY.md#scalingcontainercomponent) - Available animation directions
- [Alert System Plan - Animation Specifications](./ALERT_SYSTEM_PLAN.md#phase-3-visual-design-positioning-and-animations) - Timing and behavior requirements
- [Style Guide - CSS Transitions](../../STYLE_GUIDE.md) - Transition patterns

**Implementation Subtasks:**
- [ ] **Create `getAnimationDirection` method**: Add method to alert container that returns from-top or from-bottom direction based on AlertPosition parameter
- [ ] **Map top positions to from-top**: Return from-top direction for TopLeft, TopCenter, TopRight positions
- [ ] **Map bottom positions to from-bottom**: Return from-bottom direction for BottomLeft, BottomCenter, BottomRight positions
- [ ] **Pass animation direction to alert display**: Bind animationEntry and animationExit inputs using computed direction
- [ ] **Pass animation trigger to alert display**: Bind animationTrigger input for visible alerts to enable entry/exit animations
- [ ] **Add stacking transition CSS**: Apply transform transition with 250ms duration and Material Design easing curve to alert display component
- [ ] **Verify smooth stacking**: When alerts added/removed, remaining alerts shift smoothly with transition
- [ ] **Test directional entry**: Top alerts slide in from top, bottom alerts slide in from bottom
- [ ] **Test directional exit**: Alerts exit in same direction they entered

**Testing Subtask:**
- [ ] **Manual Visual Testing**: Verify animations and transitions (see Testing section below)

**Key Implementation Notes:**
- Animation direction is computed per alert based on its position property
- Entry and exit directions match - bottom alerts enter and exit from bottom, top from top
- 250ms stacking transition applies to flexbox-driven position changes when alerts are added or removed
- Material Design easing curve provides natural acceleration and deceleration
- No JavaScript animation needed for stacking - pure CSS transitions handle the shifting behavior
- animationTrigger remains true for visible alerts; scaling-compact-card component handles entry/exit lifecycle

**Testing Focus for Task 6:**

**Manual Testing Checklist:**

**Directional Animations:**
- [ ] **Top-left alerts animate from top**: Entry slides down from top edge
- [ ] **Top-center alerts animate from top**: Entry slides down from top edge  
- [ ] **Top-right alerts animate from top**: Entry slides down from top edge
- [ ] **Bottom-left alerts animate from bottom**: Entry slides up from bottom edge
- [ ] **Bottom-center alerts animate from bottom**: Entry slides up from bottom edge
- [ ] **Bottom-right alerts animate from bottom**: Entry slides up from bottom edge
- [ ] **Exit direction matches entry**: Alerts exit in same direction they entered

**Animation Timing:**
- [ ] **Entry feels smooth**: 400ms entry animation is polished, not sluggish
- [ ] **Exit feels responsive**: 300ms exit animation removes alert quickly
- [ ] **Entry includes scale, fade, and slide**: All three effects combine smoothly

**Stacking Transitions:**
- [ ] **Bottom alerts shift up smoothly**: Adding new bottom alert pushes existing alerts upward with 250ms transition
- [ ] **Top alerts shift down smoothly**: Adding new top alert pushes existing alerts downward with 250ms transition
- [ ] **Removal shift is smooth**: Dismissing alert causes others to shift to fill gap with transition
- [ ] **Multiple rapid additions animate**: Rapid alerts still maintain smooth transitions
- [ ] **No jank or jumpy motion**: All position changes are smooth
- [ ] **Easing feels natural**: Acceleration and deceleration is physics-based

**Testing Approach:**
- Trigger test alerts at each position using alert service
- Observe entry and exit animations in browser
- Trigger multiple alerts rapidly to test stacking transitions
- Dismiss alerts and observe smooth shifting of remaining alerts

</details>

---

<details open>
<summary><h3>Task 7: Visual Testing and Refinement Across All Combinations</h3></summary>

**Purpose**: Validate that all positioning, animation, color, and stacking behavior works correctly across all combinations of severity levels, positions, light/dark themes, and interaction patterns. This comprehensive testing ensures production-ready quality and catches edge cases before deployment.

**Related Documentation:**
- [Alert System Plan - User Scenarios](./ALERT_SYSTEM_PLAN.md#user-scenarios) - Complete scenario coverage
- [Component Library](../../COMPONENT_LIBRARY.md) - Animation and component integration patterns

**Implementation Subtasks:**
- [ ] **Create test harness component**: Build developer tool component with buttons to trigger all severity×position combinations (4 severities × 6 positions = 24 combinations)
- [ ] **Add rapid-fire test button**: Create button that triggers 5-10 alerts quickly to test stacking performance
- [ ] **Add theme toggle**: Include dark/light theme switcher in test harness for visual verification
- [ ] **Test all severity colors**: Verify success (green), error (red), warning (yellow), info (cyan) render correctly in both themes
- [ ] **Test all positions**: Confirm top-left, top-center, top-right, bottom-left, bottom-center, bottom-right work correctly
- [ ] **Test stacking at each position**: Verify multiple alerts stack correctly with proper direction (top down, bottom up)
- [ ] **Test auto-dismiss timing**: Confirm all alerts auto-dismiss after 3 seconds
- [ ] **Test manual dismiss**: Verify dismiss button works at all positions and triggers smooth exit animation
- [ ] **Test animation smoothness**: Review all entry, exit, and stacking animations for polish
- [ ] **Test viewport edges**: Resize browser to small viewport, verify alerts stay visible and don't overflow
- [ ] **Test theme transitions**: Switch themes while alerts are visible, confirm colors update correctly
- [ ] **Document visual issues**: Record any animation glitches, color problems, or layout issues for fixing

**Testing Subtask:**
- [ ] **Comprehensive Manual Testing**: Complete all visual testing checklists (see Testing section below)

**Key Implementation Notes:**
- Test harness component is development-only tool, not production code
- Focus on visual quality and user experience, not just functional correctness
- Test edge cases like many simultaneous alerts, rapid additions/removals, theme switching
- Pay attention to animation timing - does it feel polished and professional?
- Verify accessibility: can alerts be dismissed with keyboard, are colors sufficient contrast?
- Test on different screen sizes to ensure responsive behavior

**Testing Focus for Task 7:**

**Comprehensive Visual Testing Checklist:**

**Color & Severity (both themes):**
- [ ] Success alerts green with check_circle icon
- [ ] Error alerts red with error icon
- [ ] Warning alerts yellow/amber with warning icon
- [ ] Info alerts cyan with info icon
- [ ] Warning color matches directory yellow in both themes
- [ ] All colors have sufficient contrast for readability

**Positioning (all 6 positions):**
- [ ] Top-left: correct corner, stacks downward
- [ ] Top-center: horizontally centered, stacks downward
- [ ] Top-right: correct corner, stacks downward
- [ ] Bottom-left: correct corner, stacks upward
- [ ] Bottom-center: horizontally centered, stacks upward
- [ ] Bottom-right: correct corner, stacks upward

**Animations:**
- [ ] Top positions: alerts enter from top, exit to top
- [ ] Bottom positions: alerts enter from bottom, exit to bottom
- [ ] Entry animations: smooth 400ms scale+fade+slide
- [ ] Exit animations: smooth 300ms reverse animation
- [ ] Stacking shifts: smooth 250ms position transitions
- [ ] No jank or visual glitches during animations

**Stacking Behavior:**
- [ ] Multiple alerts stack correctly at each position
- [ ] New alerts push existing alerts in correct direction
- [ ] Removing alert shifts others smoothly to fill gap
- [ ] Rapid additions maintain smooth stacking
- [ ] No overlap or z-index issues

**Interaction:**
- [ ] Dismiss button works at all positions
- [ ] Manual dismiss triggers exit animation
- [ ] Auto-dismiss removes alert after 3 seconds
- [ ] Dismiss button keyboard accessible (Tab + Enter)
- [ ] Alerts have proper ARIA attributes

**Responsive & Edge Cases:**
- [ ] Alerts visible on small viewports
- [ ] Center positions remain centered on resize
- [ ] Many simultaneous alerts don't break layout
- [ ] Theme switching updates colors correctly
- [ ] No memory leaks from repeated additions/removals

**Testing Reference:**
- Create test harness in separate development component
- Use browser DevTools to inspect colors, positions, animations
- Test in both Chrome and Firefox for cross-browser compatibility
- Record screen if visual issues are difficult to describe

</details>

---

## 🗂️ Files Modified or Created

**Modified Files:**
- `libs/app/src/lib/alert-display.component.ts` - Replace with scaling-compact-card, add animation inputs
- `libs/app/src/lib/alert-display.component.html` - Wrap content in lib-scaling-compact-card, use lib-icon-label
- `libs/app/src/lib/alert-display.component.scss` - Add severity color classes using design tokens
- `libs/app/src/lib/alert-container.component.ts` - Add positioning logic and animation direction mapping
- `libs/app/src/lib/alert-container.component.html` - Implement six fixed-position containers
- `libs/app/src/lib/alert-container.component.scss` - Add fixed positioning, flexbox stacking, and transition styles
- `libs/ui/styles/src/lib/theme/styles.scss` - Add `--color-warning` design token in light and dark themes
- `docs/COMPONENT_LIBRARY.md` - Document alert display and container components with usage examples
- `docs/STYLE_GUIDE.md` - Document `--color-warning` design token and alert styling patterns

**New Files (Optional - Development Only):**
- `libs/app/src/lib/alert-test-harness.component.ts` - Developer tool for testing all alert combinations (optional, not required for production)

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are embedded in each task above. This section summarizes the overall testing approach.

### Testing Strategy

**Phase 3 Testing Focus:**
- **Visual Testing**: Primary focus is on visual quality, animation smoothness, and color accuracy
- **Manual Testing**: Most testing is manual due to visual/animation nature of this phase
- **Unit Tests**: Component input/output behavior and computed signal logic
- **Integration Tests**: Verify alert system components work together correctly

### Test Categories by Task

**Task 1 (Design Token):**
- Manual DevTools inspection of CSS custom property values
- Visual verification in both themes

**Task 2 (Color Styling):**
- Unit tests for severity-to-color mapping
- Component rendering tests with different severities
- Visual verification of colors in both themes

**Task 3 (Positioning):**
- Manual visual testing of all six positions
- Verify fixed positioning and z-index layering
- Test center position alignment at different viewport sizes

**Task 4 (Stacking Direction):**
- Manual testing of stacking order and direction
- Verify flexbox configuration for top vs bottom positions
- Test with multiple alerts at same position

**Task 5 (Animation Integration):**
- Component tests for animation inputs and content projection
- Verify dismiss button still works after template changes
- Signal flow testing for animation trigger

**Task 6 (Directional Animations & Transitions):**
- Manual visual testing of entry/exit directions
- Verify animation timing (400ms entry, 300ms exit, 250ms stacking)
- Test direction matches position (top/bottom)
- Verify stacking shift smoothness

**Task 7 (Comprehensive Testing):**
- Test harness for all severity×position combinations
- Cross-browser testing (Chrome, Firefox)
- Theme switching validation
- Accessibility verification

### Test Execution

**Unit Tests:**
```bash
# Run app library tests
npx nx test app

# Run in watch mode during development
npx nx test app --watch
```

**Manual/Visual Tests:**
- Use browser DevTools for CSS inspection
- Create test harness component to trigger all combinations
- Test in both light and dark themes
- Resize viewport to test responsive behavior

### Success Metrics

- ✅ All unit tests passing
- ✅ All manual testing checklists completed
- ✅ No visual glitches or animation jank
- ✅ Colors correct in both themes
- ✅ All positions working correctly
- ✅ Animations feel smooth and polished

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

> **Mark checkboxes as criteria are met**. All items must be checked before phase is complete.

**Functional Requirements:**
- [ ] All seven implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)
- [ ] Warning color design token added to style guide
- [ ] All six position containers implemented with fixed positioning
- [ ] Directional animations configured based on position

**Visual Quality:**
- [ ] Success alerts display green with check_circle icon
- [ ] Error alerts display red with error icon
- [ ] Warning alerts display yellow/amber with warning icon using new design token
- [ ] Info alerts display cyan with info icon
- [ ] All colors work correctly in both light and dark themes
- [ ] Animations feel smooth and polished (no jank)
- [ ] Entry animations (400ms) feel appropriate
- [ ] Exit animations (300ms) feel responsive
- [ ] Stacking shifts (250ms) feel smooth and natural

**Positioning & Layout:**
- [ ] Top-left position works correctly, stacks downward
- [ ] Top-center position works correctly, centered and stacks downward
- [ ] Top-right position works correctly, stacks downward
- [ ] Bottom-left position works correctly, stacks upward
- [ ] Bottom-center position works correctly, centered and stacks upward
- [ ] Bottom-right position works correctly, stacks upward
- [ ] Alerts don't overlap main content (proper z-index)
- [ ] Consistent spacing from viewport edges (1rem padding)

**Animation Quality:**
- [ ] Top positions: alerts animate from-top direction
- [ ] Bottom positions: alerts animate from-bottom direction
- [ ] Entry animations smooth (scale + fade + slide)
- [ ] Exit animations smooth (reverse of entry)
- [ ] Stacking shifts smooth when alerts added
- [ ] Stacking shifts smooth when alerts removed
- [ ] No visual glitches during theme switching

**Testing Requirements:**
- [ ] All unit tests passing with no failures
- [ ] All manual testing checklists completed
- [ ] Visual testing verified in both themes
- [ ] Stacking behavior tested with multiple alerts
- [ ] Rapid additions tested (no performance issues)
- [ ] Accessibility verified (keyboard navigation works)

**Quality Checks:**
- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors
- [ ] No console errors in browser
- [ ] Animations perform smoothly (60fps)
- [ ] Colors have sufficient contrast

**Documentation:**
- [ ] Component library updated with alert component examples
- [ ] Style guide updated with warning color token
- [ ] Inline comments added for animation configuration
- [ ] Position mapping logic documented

**Ready for Phase 4:**
- [ ] All success criteria met
- [ ] Visual quality meets production standards
- [ ] No known bugs or visual issues
- [ ] Ready for comprehensive testing and documentation phase

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Warning Color Aliases Directory Color**: Intentional choice to reuse existing directory yellow/amber values ensures consistency between warning alerts and directory highlighting. Users already associate this color with directories, so using it for warnings creates visual coherence.

- **400ms Entry, 300ms Exit**: Entry animations are slightly longer to feel polished and give users time to notice the alert. Exit animations are faster to feel responsive and not delay alert removal.

- **250ms Stacking Shifts**: Sweet spot between "too fast to perceive" and "too slow and sluggish". Provides clear visual feedback that stacking is happening without feeling laggy.

- **Transform-Based Animations**: Using `lib-scaling-compact-card` provides transform-based animations that don't trigger layout recalculations, ensuring smooth 60fps performance.

- **Position-Aware Directions**: Bottom alerts entering from bottom and top alerts entering from top creates intuitive spatial awareness - alerts appear from the edge they're positioned at.

### Implementation Constraints

- **No Rate Limiting**: Per plan specification, system handles unlimited rapid alerts efficiently without rate limiting. Smooth stacking transitions ensure visual clarity even with many simultaneous alerts.

- **Browser Compatibility**: Backdrop-filter effects in scaling-compact-card require modern browser support. Consider fallback styling for older browsers if needed.

- **Theme Integration**: All color styling uses CSS custom properties from design token system, ensuring automatic theme switching support.

### Future Enhancements

- **Custom Durations Per Severity**: Could allow different auto-dismiss durations for different severity levels (e.g., errors stay longer than info).

- **Alert Grouping**: Could group duplicate or similar alerts to reduce visual clutter during rapid error scenarios.

- **Sound Effects**: Could add optional audio feedback for different severity levels.

- **Notification History**: Could add notification center that tracks dismissed alerts for user review.

### External References

- [Material Design Motion Guidelines](https://m3.material.io/styles/motion/overview) - Inspiration for animation timing and easing
- [Component Library](../../COMPONENT_LIBRARY.md) - Reference for animation component APIs
- [Style Guide](../../STYLE_GUIDE.md) - Design token system and color scheme

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

</details>

---

## 💡 Remember

**This phase transforms the basic alert system into a beautiful, production-ready notification system.**

**Key Focus Areas:**
1. **Color Coding**: Semantic colors provide instant visual feedback about severity
2. **Positioning**: Six positions give developers flexibility for different use cases
3. **Animations**: Smooth directional animations create polished, professional feel
4. **Stacking**: Physics-based stacking makes multiple alerts feel cohesive

**Testing Philosophy:**
- **Visual quality is paramount** - animations must feel smooth and polished
- **Manual testing is primary** - most validation is visual/subjective
- **Test all combinations** - severity × position × theme = many cases to verify
- **Polish matters** - this is user-facing UI that represents product quality

**Progress Tracking:**
- ✅ Mark subtask checkboxes as you complete them
- 📝 Add discoveries or issues to Notes section
- 🎨 Take screenshots of before/after for documentation
- 🐛 Document any visual glitches immediately for fixing
