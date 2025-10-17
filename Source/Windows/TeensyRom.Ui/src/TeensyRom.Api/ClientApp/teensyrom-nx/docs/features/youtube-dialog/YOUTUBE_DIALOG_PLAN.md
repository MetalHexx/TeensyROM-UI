# Phase 1: YouTube Video Dialog Integration

## 🎯 Objective

Create a modal dialog feature that displays embedded YouTube videos when users click on YouTube video links in file metadata. This replaces the current behavior of opening videos in new browser tabs, keeping users within the application and providing a seamless viewing experience.

---

## 📚 Required Reading

> Review these documents before starting implementation. Check the boxes as you read them.

**Standards & Guidelines:**
- [ ] [Coding Standards](../../CODING_STANDARDS.md) - General coding patterns and conventions
- [ ] [Testing Standards](../../TESTING_STANDARDS.md) - Testing approaches and best practices
- [ ] [Style Guide](../../STYLE_GUIDE.md) - Component styling patterns and Material Design customizations
- [ ] [Component Library](../../COMPONENT_LIBRARY.md) - Existing UI components and patterns

---

## 📂 File Structure Overview

```
libs/ui/components/src/lib/
├── link/                                    ✨ New - Base presentation component
│   ├── link.component.ts                    ✨ New - Icon + label rendering
│   ├── link.component.scss                  ✨ New - Shared link styles
│   └── link.component.spec.ts               ✨ New - Unit tests
├── external-link/
│   ├── external-link.component.ts           📝 Modified - Compose with LinkComponent
│   ├── external-link.component.html         📝 Modified - Use lib-link
│   └── external-link.component.spec.ts      📝 Modified - Update tests
├── action-link/                             ✨ New - Event-emitting link component
│   ├── action-link.component.ts             ✨ New - Button-based link with events
│   ├── action-link.component.html           ✨ New - Template with lib-link
│   ├── action-link.component.scss           ✨ New - Button styling
│   └── action-link.component.spec.ts        ✨ New - Unit tests
└── index.ts                                 📝 Modified - Export new components

libs/features/player/src/lib/player-view/player-device-container/file-other/
├── youtube-dialog/                          ✨ New - Dialog component folder
│   ├── youtube-dialog.component.ts          ✨ New - Dialog logic
│   ├── youtube-dialog.component.html        ✨ New - Dialog template with iframe
│   ├── youtube-dialog.component.scss        ✨ New - Dialog styling
│   └── youtube-dialog.component.spec.ts     ✨ New - Unit tests
├── file-other.component.ts                  📝 Modified - Add dialog opening logic
├── file-other.component.html                📝 Modified - Replace external-link with action-link
└── file-other.component.spec.ts             📝 Modified - Add dialog tests
```

---

<details open>
<summary><h3>Task 1: Create Base Link Component</h3></summary>

**Purpose**: Extract the visual presentation (icon + label) from `ExternalLinkComponent` into a reusable base component that can be composed by both navigation and action link components.

**Related Documentation:**
- [Component Library](../../COMPONENT_LIBRARY.md#link-components) - Existing link component pattern
- [Icon Label Component](../../COMPONENT_LIBRARY.md#iconlabelcomponent) - Component we'll reuse for rendering

**Implementation Subtasks:**
- [ ] **Create `LinkComponent` class** in `libs/ui/components/src/lib/link/link.component.ts`
- [ ] **Add required inputs**: `label` (required), `icon` (default: 'link'), `iconColor` (default: 'primary')
- [ ] **Create template** that renders `IconLabelComponent` with bound inputs
- [ ] **Create styles** in `link.component.scss` with minimal host display styling
- [ ] **Export component** from `libs/ui/components/src/index.ts`

**Testing Subtask:**
- [ ] **Write Tests**: Verify link renders icon and label correctly

**Key Implementation Notes:**
- This component is pure presentation - no behavior, no events, no navigation
- Use `IconLabelComponent` internally for consistent icon + label rendering
- Keep styling minimal - just flex display and basic layout

**Testing Focus for Task 1:**

**Behaviors to Test:**
- [ ] **Renders label text**: Component displays the provided label text
- [ ] **Renders icon**: Component displays the provided Material icon
- [ ] **Icon color applied**: Component applies the specified icon color variant
- [ ] **Default values work**: Component uses default icon and color when not specified

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for behavioral testing patterns

</details>

---

<details open>
<summary><h3>Task 2: Refactor ExternalLinkComponent to Use LinkComponent</h3></summary>

**Purpose**: Refactor `ExternalLinkComponent` to compose with the new `LinkComponent`, extracting presentation logic while maintaining all existing functionality and API contracts.

**Related Documentation:**
- [Component Library](../../COMPONENT_LIBRARY.md#externallinkcomponent) - Current implementation reference
- [Coding Standards](../../CODING_STANDARDS.md) - Component composition patterns

**Implementation Subtasks:**
- [ ] **Import `LinkComponent`** into `ExternalLinkComponent` imports array
- [ ] **Update template** to wrap `lib-link` inside the `<a>` tag
- [ ] **Pass through props** - bind `icon`, `iconColor`, `label` to `lib-link`
- [ ] **Remove `IconLabelComponent`** from imports (now used by LinkComponent)
- [ ] **Verify all existing inputs/outputs** still work (href, target, rel, aria-label, etc.)

**Testing Subtask:**
- [ ] **Write Tests**: Verify refactored component maintains all existing behaviors

**Key Implementation Notes:**
- All existing API contracts must remain unchanged - zero breaking changes
- All existing tests should pass without modification (except imports)
- Template changes should only affect composition, not rendered output
- Security attributes (rel="noopener noreferrer") must still apply correctly

**Testing Focus for Task 2:**

**Behaviors to Test:**
- [ ] **All existing tests pass**: Refactored component maintains backward compatibility
- [ ] **Renders identically**: Visual output is unchanged from previous implementation
- [ ] **Security attributes applied**: External links still get proper rel attributes
- [ ] **Navigation works**: Clicking links still navigates correctly
- [ ] **ARIA labels work**: Accessibility attributes still apply correctly

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for regression testing approach

</details>

---

<details open>
<summary><h3>Task 3: Create ActionLinkComponent</h3></summary>

**Purpose**: Create a new component that looks identical to `ExternalLinkComponent` but uses a semantic `<button>` element and emits click events instead of navigating, enabling modal and action-based interactions.

**Related Documentation:**
- [Component Library](../../COMPONENT_LIBRARY.md#externallinkcomponent) - Visual pattern to match
- [Style Guide](../../STYLE_GUIDE.md) - Utility classes like `.selectable-item`

**Implementation Subtasks:**
- [ ] **Create `ActionLinkComponent` class** in `libs/ui/components/src/lib/action-link/`
- [ ] **Add inputs**: `label` (required), `icon` (default: 'link'), `iconColor` (default: 'primary'), `ariaLabel` (optional), `disabled` (default: false)
- [ ] **Add output**: `linkClick` event emitter of type `void`
- [ ] **Create template** with `<button>` wrapping `lib-link`
- [ ] **Create styles** matching external-link appearance using `.selectable-item` mixin
- [ ] **Add click handler** that emits event when not disabled
- [ ] **Export component** from `libs/ui/components/src/index.ts`

**Testing Subtask:**
- [ ] **Write Tests**: Verify action link renders and emits events correctly

**Key Implementation Notes:**
- Use semantic `<button type="button">` for proper accessibility
- Apply `.selectable-item` mixin from style guide for consistent hover/active states
- Style button to look like a link (no border, background, proper cursor, text color)
- Button should be keyboard accessible (Enter/Space trigger click)
- Disabled state should prevent event emission and show visual feedback

**Testing Focus for Task 3:**

**Behaviors to Test:**
- [ ] **Renders link appearance**: Component looks like ExternalLinkComponent
- [ ] **Emits click events**: Click on enabled component emits linkClick event
- [ ] **Disabled state works**: Disabled component does not emit events
- [ ] **Keyboard accessible**: Enter and Space keys trigger click
- [ ] **ARIA label applies**: Custom aria-label or label is set for screen readers
- [ ] **Icon and label render**: Composed LinkComponent displays correctly

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for component testing patterns

</details>

---

<details open>
<summary><h3>Task 4: Create YouTubeDialogComponent</h3></summary>

**Purpose**: Create a dialog component that displays an embedded YouTube video player using an iframe, with proper responsive sizing and Material Design dialog styling.

**Related Documentation:**
- [Component Library](../../COMPONENT_LIBRARY.md) - Dialog patterns (see BusyDialogComponent)
- [Style Guide](../../STYLE_GUIDE.md) - `.glassy` class for dialog styling
- [Layout Component](../../../libs/app/shell/src/lib/layout/layout.component.ts) - Existing dialog usage example

**Implementation Subtasks:**
- [ ] **Create `YouTubeDialogComponent` class** in `libs/features/player/.../file-other/youtube-dialog/`
- [ ] **Import Angular Material dialog modules**: `MatDialogModule`, `MAT_DIALOG_DATA`, `MatDialogRef`
- [ ] **Add dialog data interface**: Define type for video data (videoId, url, channel)
- [ ] **Inject dialog data**: Use `MAT_DIALOG_DATA` to receive YouTubeVideo object
- [ ] **Create template** with Material dialog structure (`mat-dialog-content`, actions, etc.)
- [ ] **Add YouTube iframe**: Embed using `https://www.youtube.com/embed/{videoId}`
- [ ] **Create styles**: Responsive iframe sizing, 16:9 aspect ratio, dialog width
- [ ] **Add close button**: Material icon button in dialog actions

**Testing Subtask:**
- [ ] **Write Tests**: Verify dialog renders and displays video correctly

**Key Implementation Notes:**
- YouTube embed URL format: `https://www.youtube.com/embed/{videoId}`
- Use `aspect-ratio: 16/9` CSS for responsive iframe sizing
- Dialog should be dismissible via backdrop click and ESC key (Material default)
- Consider adding `loading="lazy"` to iframe for performance
- Extract videoId from `YouTubeVideo.videoId` property
- Title should show video channel name from `YouTubeVideo.channel`

**Critical Dialog Data Type**:
```typescript
interface YouTubeDialogData {
  video: YouTubeVideo;
}
```

**Testing Focus for Task 4:**

**Behaviors to Test:**
- [ ] **Dialog opens**: Component initializes with provided video data
- [ ] **Video data displayed**: Channel name appears in dialog title
- [ ] **Iframe renders**: YouTube iframe is created with correct embed URL
- [ ] **Close button works**: Clicking close button dismisses dialog
- [ ] **Responsive sizing**: Dialog adapts to different screen sizes

**Testing Reference:**
- See [Testing Standards](../../TESTING_STANDARDS.md) for dialog component testing

</details>

---

<details open>
<summary><h3>Task 5: Document New Components in Component Library</h3></summary>

**Purpose**: Add comprehensive documentation for the new reusable components (`LinkComponent` and `ActionLinkComponent`) to the Component Library, ensuring other developers can discover and use these components correctly.

**Related Documentation:**
- [Component Library](../../COMPONENT_LIBRARY.md) - Existing component documentation patterns
- [Component Library Link Components Section](../../COMPONENT_LIBRARY.md#link-components) - Where to add new entries

**Implementation Subtasks:**
- [ ] **Add `LinkComponent` documentation** in Component Library under Link Components section
- [ ] **Add `ActionLinkComponent` documentation** in Component Library under Link Components section
- [ ] **Update `ExternalLinkComponent` documentation** to mention it now composes `LinkComponent`
- [ ] **Include usage examples** for both components with common scenarios
- [ ] **Document when to use each component** - navigation vs actions guidance
- [ ] **Add cross-references** between LinkComponent, ActionLinkComponent, and ExternalLinkComponent

**Testing Subtask:**
- [ ] **Verify Documentation**: Review that all examples are accurate and complete

**Key Implementation Notes:**
- Follow existing documentation patterns from other components in the library
- Include selector, properties, events, and usage examples for each component
- Add "See Also" references to related components
- Document the composition pattern (LinkComponent as base, composed by others)
- Explain semantic HTML differences (`<a>` vs `<button>`)

**Documentation Structure for Each Component**:
```markdown
### `ComponentName`

**Purpose**: [One sentence description]

**Selector**: `lib-component-name`

**Properties**:
- `prop1` (required/optional): `type` - Description

**Events** (if applicable):
- `eventName`: Description

**Usage Examples**: [Code blocks with HTML examples]

**Best Practice**: [When to use this component]

**See Also**: [Links to related components]
```

**Testing Focus for Task 5:**

**Documentation Completeness:**
- [ ] **All properties documented**: Every input and output is listed with types
- [ ] **Usage examples provided**: Common use cases have working code examples
- [ ] **Semantic differences explained**: Clear guidance on when to use each component
- [ ] **Cross-references added**: Links between related components work correctly

**Testing Reference:**
- Validate examples compile and follow coding standards

</details>

---

<details open>
<summary><h3>Task 6: Integrate YouTubeDialog into FileOtherComponent</h3></summary>

**Purpose**: Replace the current `ExternalLinkComponent` usage for YouTube videos with `ActionLinkComponent`, and wire up dialog opening logic when users click on YouTube video links.

**Related Documentation:**
- [file-other.component.html](../../../libs/features/player/src/lib/player-view/player-device-container/file-other/file-other.component.html) - Current implementation
- [Layout Component](../../../libs/app/shell/src/lib/layout/layout.component.ts) - Dialog opening pattern reference

**Implementation Subtasks:**
- [ ] **Inject `MatDialog` service** into `FileOtherComponent`
- [ ] **Create `openYouTubeDialog` method** that accepts `YouTubeVideo` parameter
- [ ] **Implement dialog opening logic** using `this.dialog.open()` with configuration
- [ ] **Update template** to replace `lib-external-link` with `lib-action-link` for YouTube videos
- [ ] **Bind click handler** to `(linkClick)="openYouTubeDialog(video)"`
- [ ] **Keep external link for other links** (CSDb, DeepSID, etc. - don't change them)

**Testing Subtask:**
- [ ] **Write Tests**: Verify clicking YouTube links opens dialog with correct video

**Key Implementation Notes:**
- Dialog configuration: `width: '800px'`, `maxWidth: '90vw'` for responsive sizing
- Pass video object via `data` property: `{ video: YouTubeVideo }`
- Consider adding `panelClass: 'youtube-dialog'` for custom styling hook
- Only YouTube videos should open dialog - other external links remain unchanged
- YouTube videos are identified by the `play_circle` icon in the current implementation

**Dialog Opening Pattern**:
```typescript
openYouTubeDialog(video: YouTubeVideo): void {
  this.dialog.open(YouTubeDialogComponent, {
    data: { video },
    width: '800px',
    maxWidth: '90vw'
  });
}
```

**Testing Focus for Task 6:**

**Behaviors to Test:**
- [ ] **Clicking YouTube link opens dialog**: ActionLink click handler triggers dialog
- [ ] **Correct video passed to dialog**: Dialog receives the clicked video's data
- [ ] **Dialog opens with video**: YouTubeDialogComponent displays with correct video
- [ ] **Other external links unchanged**: Non-YouTube links still navigate externally
- [ ] **Multiple videos work**: Each video link opens dialog with correct video

**Testing Reference:**
- See [Smart Component Testing](../../SMART_COMPONENT_TESTING.md) for testing component integration

</details>

---

## 🗂️ Files Modified or Created

**New Files:**
- `libs/ui/components/src/lib/link/link.component.ts`
- `libs/ui/components/src/lib/link/link.component.scss`
- `libs/ui/components/src/lib/link/link.component.spec.ts`
- `libs/ui/components/src/lib/action-link/action-link.component.ts`
- `libs/ui/components/src/lib/action-link/action-link.component.html`
- `libs/ui/components/src/lib/action-link/action-link.component.scss`
- `libs/ui/components/src/lib/action-link/action-link.component.spec.ts`
- `libs/features/player/src/lib/player-view/player-device-container/file-other/youtube-dialog/youtube-dialog.component.ts`
- `libs/features/player/src/lib/player-view/player-device-container/file-other/youtube-dialog/youtube-dialog.component.html`
- `libs/features/player/src/lib/player-view/player-device-container/file-other/youtube-dialog/youtube-dialog.component.scss`
- `libs/features/player/src/lib/player-view/player-device-container/file-other/youtube-dialog/youtube-dialog.component.spec.ts`

**Modified Files:**
- `libs/ui/components/src/index.ts`
- `libs/ui/components/src/lib/external-link/external-link.component.ts`
- `libs/ui/components/src/lib/external-link/external-link.component.html`
- `libs/ui/components/src/lib/external-link/external-link.component.spec.ts`
- `libs/features/player/src/lib/player-view/player-device-container/file-other/file-other.component.ts`
- `libs/features/player/src/lib/player-view/player-device-container/file-other/file-other.component.html`
- `libs/features/player/src/lib/player-view/player-device-container/file-other/file-other.component.spec.ts`
- `docs/COMPONENT_LIBRARY.md`

---

<details open>
<summary><h2>📝 Testing Summary</h2></summary>

> **IMPORTANT:** Tests are written **within each task above**, not here. This section is only a summary for quick reference.

### Where Tests Are Written

**Tests are embedded in each task above** with:
- **Testing Subtask**: Checkbox in the task's subtask list
- **Testing Focus**: "Behaviors to Test" section listing observable outcomes
- **Testing Reference**: Links to relevant testing documentation

**Complete each task's testing subtask before moving to the next task.**

### Test Coverage Areas

- **LinkComponent**: Pure presentation rendering (icon, label, styling)
- **ExternalLinkComponent**: Regression testing - all existing behaviors maintained
- **ActionLinkComponent**: Event emission, disabled state, accessibility
- **YouTubeDialogComponent**: Dialog initialization, iframe rendering, close behavior
- **FileOtherComponent**: Dialog integration, click handling, video passing
- **Component Library Documentation**: Accuracy and completeness of new component docs

### Test Execution Commands

```bash
# UI Components library
pnpm nx test ui-components --watch

# Player feature library
pnpm nx test player --watch

# Run all tests
pnpm nx run-many --target=test --all
```

</details>

---

<details open>
<summary><h2>✅ Success Criteria</h2></summary>

**Functional Requirements:**
- [ ] All implementation tasks completed and checked off
- [ ] All subtasks within each task completed
- [ ] Code follows [Coding Standards](../../CODING_STANDARDS.md)
- [ ] Components follow patterns from [Component Library](../../COMPONENT_LIBRARY.md)

**Testing Requirements:**
- [ ] All testing subtasks completed within each task
- [ ] All behavioral test checkboxes verified
- [ ] Tests written alongside implementation (not deferred)
- [ ] All tests passing with no failures
- [ ] Existing ExternalLinkComponent tests still pass (regression)

**Quality Checks:**
- [ ] No TypeScript errors or warnings
- [ ] Linting passes with no errors (`pnpm nx lint`)
- [ ] Code formatting is consistent
- [ ] No console errors in browser when clicking YouTube links

**User Experience:**
- [ ] Clicking YouTube video links opens modal dialog (not new tab)
- [ ] Dialog displays embedded YouTube video player
- [ ] Dialog is responsive on different screen sizes
- [ ] Dialog can be closed via close button, ESC key, or backdrop click
- [ ] Other external links (CSDb, DeepSID) still open in new tabs
- [ ] YouTube links are visually distinguishable with play_circle icon

**Documentation:**
- [ ] Component Library updated with LinkComponent and ActionLinkComponent entries (Task 5)
- [ ] ExternalLinkComponent documentation updated to mention composition pattern
- [ ] Usage examples provided for all new components
- [ ] Inline code comments added for dialog configuration
- [ ] JSDoc comments added for public component APIs

**Ready for Production:**
- [ ] All success criteria met
- [ ] No known bugs or issues
- [ ] Feature tested in browser manually
- [ ] Ready for code review

</details>

---

<details open>
<summary><h2>📝 Notes & Considerations</h2></summary>

### Design Decisions

- **Composition Pattern**: Chose to extract LinkComponent for presentation reuse rather than modifying ExternalLinkComponent directly. This follows SOLID principles and keeps each component focused on single responsibility.

- **Semantic HTML**: ActionLinkComponent uses `<button>` instead of `<a>` to properly convey intent to screen readers and browsers. Links navigate, buttons trigger actions - this is semantically correct for opening a modal.

- **Feature Co-location**: YouTubeDialogComponent lives in the player feature rather than ui/components because it's feature-specific. Can be promoted to shared components later if reused elsewhere.

- **Dialog Dismissibility**: Using Material Design's default dismissible behavior (backdrop click, ESC key) provides familiar UX and proper focus management without custom code.

### Implementation Constraints

- **YouTube iframe embed**: Must use `https://www.youtube.com/embed/{videoId}` format for proper embedding. Standard `youtube.com/watch?v=` URLs won't work in iframes.

- **Aspect ratio**: YouTube videos are 16:9 aspect ratio. Use CSS `aspect-ratio` property for responsive sizing that maintains proper proportions.

- **Clean Architecture boundaries**: LinkComponent lives in ui/components (presentation layer), while YouTubeDialogComponent lives in features/player (feature layer). This maintains proper dependency constraints enforced by ESLint.

### Future Enhancements

- **Video player controls**: Could add YouTube iframe API for programmatic control (play/pause, seek, etc.)

- **Playlist support**: If files have multiple YouTube videos, could add previous/next navigation within the dialog

- **Thumbnail preview**: Could show video thumbnail before iframe loads for faster perceived performance

- **Other video platforms**: Pattern could extend to Vimeo, Twitch, or other embeddable video platforms

- **Dialog animations**: Could add custom enter/exit animations using Angular animations API

- **Generalize ActionLink pattern**: If other features need action-triggered links, ActionLinkComponent is ready for reuse

### External References

- [YouTube Embed Parameters](https://developers.google.com/youtube/player_parameters) - Official YouTube embed documentation
- [Angular Material Dialog](https://v19.material.angular.io/components/dialog/overview) - Material 19 dialog API reference

### Discoveries During Implementation

> Add notes here as you discover important details during implementation

</details>

---

## 💡 Tips for Implementing This Phase

**Before Starting:**
1. Review the existing `ExternalLinkComponent` implementation to understand current structure
2. Look at `BusyDialogComponent` in layout component for dialog pattern reference
3. Test YouTube embed URLs manually in browser to verify format
4. Check Material Design documentation for Dialog API updates in v19

**While Implementing:**
1. Start with LinkComponent - it's the foundation for the other components
2. Test ExternalLinkComponent refactor thoroughly - ensure no regressions
3. Use ActionLinkComponent's tests to verify button semantics work correctly
4. Test dialog with different video IDs to ensure embed URL construction works
5. Check responsive behavior at mobile, tablet, and desktop sizes

**Common Pitfalls:**
- Don't forget `type="button"` on ActionLinkComponent button (prevents form submission)
- Ensure YouTube embed URL uses `embed/` path, not `watch?v=`
- Remember to export new components from index.ts barrel files
- Don't modify other external links in file-other template - only YouTube links

**Testing Strategy:**
- Write LinkComponent tests first - establishes baseline for composition
- ExternalLinkComponent tests should mostly pass without changes
- ActionLinkComponent needs comprehensive accessibility testing (keyboard, ARIA)
- Dialog component should test data passing and iframe URL construction
- Integration test in FileOtherComponent verifies end-to-end flow
