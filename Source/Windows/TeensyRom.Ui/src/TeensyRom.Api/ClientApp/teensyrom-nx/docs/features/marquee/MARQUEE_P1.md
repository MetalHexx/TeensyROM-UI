# Scrolling Marquee Component - Phase 1

**Project Overview**: Create a reusable scrolling marquee component that displays file descriptions from the player state. The component will show smooth horizontal scrolling text when content overflows its container, providing a classic retro-style information display for currently playing files.

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **Component Library**: [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md)
- **Style Guide**: [STYLE_GUIDE.md](../../STYLE_GUIDE.md)

## 🎯 Project Objective

Implement a scrolling text marquee component that displays the current file's description field from player state. The marquee should only scroll when text overflows the available width, providing an elegant way to show long file descriptions without truncating important information.

**Key Value Propositions**:

- **No Information Loss**: Users see full file descriptions even when very long
- **Retro Aesthetic**: Classic scrolling text effect fits vintage computing theme
- **Reusable Component**: Can be used anywhere scrolling text is needed
- **Performance**: CSS-based animation with minimal overhead
- **Responsive**: Adapts to container size, only scrolls when necessary

## 📋 Implementation Phases

This feature is implemented in three progressive phases:

1. **Phase 1: Core Marquee Component** - Build the basic scrolling text component with overflow detection
2. **Phase 2: Player Integration** - Integrate marquee into player UI to display file descriptions
3. **Phase 3: Retro Demo Effects** - Add optional demoscene-style visual effects for creative flair

The phased approach ensures a working basic version before adding advanced visual effects.

---

## Phase 1: Core Marquee Component

### Objective

Create a standalone, reusable scrolling marquee component in the UI component library with smooth CSS-based animation.

### Key Deliverables

- [ ] ScrollingMarqueeComponent in `libs/ui/components/src/lib/scaling-compact-card/`
- [ ] CSS keyframe animation for smooth scrolling
- [ ] Overflow detection (only scroll when text is too long)
- [ ] Configurable speed and direction
- [ ] Comprehensive unit tests

### High-Level Tasks

1. **Create Component Structure**: New component in `libs/ui/components/src/lib/scrolling-marquee/`
2. **Implement Animation**: CSS `@keyframes` for continuous horizontal scroll
3. **Overflow Detection**: JavaScript logic to determine if scrolling is needed
4. **Input Properties**: Text content, speed, direction, auto-scroll behavior
5. **Styling**: Clean, minimal design that fits TeensyROM aesthetic
6. **Tests**: Component creation, overflow detection, animation trigger

### Technical Implementation

**Component Inputs**:

```typescript
text = input<string>(''); // Text to display
speed = input<number>(50); // Pixels per second scroll speed
direction = input<'left' | 'right'>('left'); // Scroll direction
pauseOnHover = input<boolean>(true); // Pause animation on mouse hover
```

**Animation Logic**:

```scss
@keyframes scroll-left {
  0% {
    transform: translateX(0);
  }
  100% {
    transform: translateX(-100%);
  }
}

.marquee-content {
  animation: scroll-left var(--scroll-duration) linear infinite;
}
```

**Overflow Detection**:

```typescript
private checkOverflow(): void {
  const container = this.containerRef.nativeElement;
  const content = this.contentRef.nativeElement;

  this.shouldScroll.set(content.scrollWidth > container.clientWidth);
}
```

---

## Phase 2: Player Integration

### Objective

Integrate marquee component into player-device-container to display current file description from player state.

### Key Deliverables

- [ ] Player-device-container updated with marquee
- [ ] PLAYER_CONTEXT service injected
- [ ] Computed signal for file description
- [ ] Marquee positioned below file-other component
- [ ] Integration tests

### High-Level Tasks

1. **Update player-device-container.component.ts**:

   - Inject PLAYER_CONTEXT service
   - Create `fileDescription` computed signal
   - Get `LaunchedFile.file.description` from current file

2. **Update player-device-container.component.html**:

   - Add marquee component below file-other
   - Pass file description to marquee
   - Style for proper spacing
   - Put this inside a Scaling-Compact-Card component.

3. **Update player-device-container.component.scss**:

   - Add marquee container styles
   - Ensure proper alignment with existing components
   - Responsive sizing

4. **Testing**:
   - Verify description updates when file changes
   - Test with empty descriptions
   - Validate responsive behavior

### Integration Code

**TypeScript**:

```typescript
export class PlayerDeviceContainerComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);

  deviceId = input.required<string>();

  fileDescription = computed(() => {
    const currentFile = this.playerContext.getCurrentFile(this.deviceId())();
    return currentFile?.file?.description ?? '';
  });
}
```

**HTML**:

```html
<div class="device-container">
  <div class="device-header">
    <lib-file-image [currentFile]="currentFile()"></lib-file-image>
    <lib-file-other [deviceId]="deviceId()"></lib-file-other>
  </div>

  <!-- New marquee component -->
  <div class="marquee-container">
    <lib-scrolling-marquee [text]="fileDescription()"></lib-scrolling-marquee>
  </div>

  @if (isPlayerLoaded()) {
  <div class="player-toolbar">
    <lib-player-toolbar [deviceId]="deviceId()"></lib-player-toolbar>
  </div>
  }
  <!-- ... rest of template -->
</div>
```

---

## Phase 3: Retro Demo Effects (Advanced)

### Objective

Add optional retro demo-style text effects to the marquee component, creating visually stunning animated text displays inspired by classic demoscene aesthetics. This phase builds upon the working basic marquee to add creative visual flair.

### Key Deliverables

- [ ] Effect system with configurable effect types
- [ ] Character-level animation transforms
- [ ] Multiple retro demo-style effects implemented
- [ ] Performance-optimized CSS/JavaScript animations
- [ ] Effect configuration inputs

### Retro Demo Effects

#### 1. **Wave Effect** (Classic Sine Wave)

**Description**: Characters flow up and down in a smooth sine wave pattern as they scroll.

**Visual**: Letters undulate like ocean waves, each character offset by its position in the text.

**Technical**:

```scss
.wave-effect .char {
  animation: wave 1.5s ease-in-out infinite;
  animation-delay: calc(var(--char-index) * 0.05s);
}

@keyframes wave {
  0%,
  100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-8px);
  }
}
```

#### 2. **Rainbow Cycling** (Commodore 64 Vibes)

**Description**: Text colors cycle through the full spectrum, creating a flowing rainbow effect.

**Visual**: Each character displays a different hue, colors shift continuously through the rainbow.

**Technical**:

```scss
.rainbow-effect .char {
  animation: rainbow 3s linear infinite;
  animation-delay: calc(var(--char-index) * 0.1s);
}

@keyframes rainbow {
  0% {
    color: #ff0000;
  }
  16.6% {
    color: #ff8800;
  }
  33.3% {
    color: #ffff00;
  }
  50% {
    color: #00ff00;
  }
  66.6% {
    color: #0088ff;
  }
  83.3% {
    color: #8800ff;
  }
  100% {
    color: #ff0000;
  }
}
```

#### 3. **Glitch Distortion** (CRT Interference)

**Description**: Random character displacement with scanline artifacts, simulating CRT display glitches.

**Visual**: Characters randomly shift position slightly, creating a digital corruption aesthetic.

**Technical**:

```scss
.glitch-effect .char {
  animation: glitch 0.3s infinite;
  animation-delay: calc(var(--char-index) * 0.02s);
}

@keyframes glitch {
  0%,
  100% {
    transform: translate(0, 0);
  }
  25% {
    transform: translate(-2px, 1px);
  }
  50% {
    transform: translate(2px, -1px);
  }
  75% {
    transform: translate(-1px, -2px);
  }
}
```

#### 4. **Bounce Elastic** (Rubber Band Physics)

**Description**: Characters bounce and stretch like elastic rubber as they move.

**Visual**: Letters squash and stretch with exaggerated timing, creating playful motion.

**Technical**:

```scss
.bounce-effect .char {
  animation: bounce 1s cubic-bezier(0.68, -0.55, 0.265, 1.55) infinite;
  animation-delay: calc(var(--char-index) * 0.03s);
}

@keyframes bounce {
  0%,
  100% {
    transform: scale(1, 1);
  }
  25% {
    transform: scale(0.9, 1.1);
  }
  50% {
    transform: scale(1.1, 0.9);
  }
  75% {
    transform: scale(0.95, 1.05);
  }
}
```

#### 5. **Copper Bars** (Amiga Demo Classic)

**Description**: Horizontal colored bands sweep through text, changing character colors as they pass.

**Visual**: Metallic copper-colored bars move vertically through the text.

**Technical**:

```scss
.copper-effect .char {
  animation: copper 2s linear infinite;
  animation-delay: calc(var(--char-index) * 0.05s);
}

@keyframes copper {
  0%,
  100% {
    color: #ff6600;
    text-shadow: 0 0 8px #ff6600;
  }
  25% {
    color: #ffaa00;
    text-shadow: 0 0 12px #ffaa00;
  }
  50% {
    color: #ffdd00;
    text-shadow: 0 0 16px #ffdd00;
  }
  75% {
    color: #ffaa00;
    text-shadow: 0 0 12px #ffaa00;
  }
}
```

#### 6. **Spiral Twist** (3D Rotation)

**Description**: Characters rotate around the text baseline, creating a twisting spiral effect.

**Visual**: Letters spin on their axis with staggered timing, appearing to spiral through space.

**Technical**:

```scss
.spiral-effect .char {
  animation: spiral 2s ease-in-out infinite;
  animation-delay: calc(var(--char-index) * 0.04s);
}

@keyframes spiral {
  0%,
  100% {
    transform: rotateY(0deg);
  }
  50% {
    transform: rotateY(180deg);
  }
}
```

### Implementation Approach

**Component Updates**:

```typescript
effect = input<MarqueeEffect>('none'); // Effect type selector

export type MarqueeEffect =
  | 'none' // Basic scroll only
  | 'wave' // Sine wave vertical motion
  | 'rainbow' // Color spectrum cycling
  | 'glitch' // Random displacement
  | 'bounce' // Elastic squash/stretch
  | 'copper' // Copper bar sweep
  | 'spiral'; // 3D rotation twist
```

**Character Wrapping**:

```html
<div class="marquee-content" [class]="effectClass()">
  @for (char of characters(); track $index) {
  <span class="char" [style.--char-index]="$index">{{ char }}</span>
  }
</div>
```

**Performance Considerations**:

- Use CSS transforms (GPU-accelerated) for position/scale effects
- Limit effects to reasonable character counts (< 100 chars)
- Provide option to disable effects on low-end devices
- Use `will-change` CSS property for animation optimization

### High-Level Tasks

1. **Split Text into Characters**: Parse text string into individual character spans
2. **Add Effect Input**: New `effect` input property with enum type
3. **Dynamic Class Application**: Apply effect class based on input
4. **Implement CSS Animations**: Create keyframes for each effect type
5. **Character Indexing**: Use CSS custom properties for animation delays
6. **Performance Testing**: Verify smooth 60fps animation with effects
7. **Effect Selector UI**: Optional dropdown/buttons to preview effects
8. **Documentation**: Add effects to COMPONENT_LIBRARY.md

### Testing Requirements

- [ ] Each effect animates correctly
- [ ] Effects work with scrolling animation
- [ ] No performance degradation with effects enabled
- [ ] Effects can be toggled at runtime
- [ ] Long text (100+ chars) performs acceptably
- [ ] Effects work across browsers (Chrome, Firefox, Safari)
- [ ] Graceful fallback if CSS features unsupported

---

## 🏗️ Architecture Overview

### Key Design Decisions

- **CSS Animation**: Use CSS `@keyframes` instead of JavaScript for smooth, hardware-accelerated animation
- **Overflow Detection**: Use `ElementRef` and `afterNextRender` to measure text width and determine if scrolling needed
- **Reusable Component**: Place in `libs/ui/components/src/lib/scaling-compact-card/` alongside other card components for organizational consistency
- **Player Integration**: Use computed signal to reactively get description from player state
- **Retro Effects**: Character-level animations using CSS custom properties for staggered timing (Phase 3)

### Component Structure

```
libs/ui/components/src/lib/scaling-compact-card/scrolling-marquee/
├── scrolling-marquee.component.ts       # Component logic, overflow detection
├── scrolling-marquee.component.html     # Marquee template with animation
├── scrolling-marquee.component.scss     # Keyframe animations and styling
└── scrolling-marquee.component.spec.ts  # Unit tests
```

**Location Rationale**: The marquee component is placed within the `scaling-compact-card/` directory because:

- It follows similar organizational patterns to other specialized card components
- Often used in combination with compact card layouts for player UI
- Keeps related presentational components grouped together
- Maintains consistency with existing UI component library structure

### Integration Structure

```
libs/features/player/src/lib/player-view/player-device-container/
├── player-device-container.component.ts
│   └── fileDescription = computed(() => getCurrentFile()?.file?.description)
├── player-device-container.component.html
│   └── <lib-scrolling-marquee [text]="fileDescription()"></lib-scrolling-marquee>
└── player-device-container.component.scss
    └── .marquee-container { /* spacing and sizing */ }
```

### Data Flow

```
Player State (PlayerStore)
    ↓
getCurrentFile() → LaunchedFile
    ↓
LaunchedFile.file.description
    ↓
fileDescription computed signal
    ↓
ScrollingMarqueeComponent [text] input
    ↓
CSS Animation (if overflow detected)
```

---

## 🧪 Testing Strategy

### Unit Tests - ScrollingMarqueeComponent

- [ ] Component creates successfully
- [ ] Text input renders correctly
- [ ] Overflow detection identifies when text is too long
- [ ] Animation applies only when shouldScroll is true
- [ ] Speed input correctly calculates animation duration
- [ ] Direction input changes animation direction
- [ ] PauseOnHover stops animation on mouse enter/leave
- [ ] Empty text displays gracefully

### Integration Tests - Player Integration

- [ ] Marquee displays current file description
- [ ] Description updates when file changes
- [ ] Empty description shows nothing (or placeholder)
- [ ] Long descriptions trigger scrolling
- [ ] Short descriptions remain static
- [ ] Component properly positioned in layout

### Visual/Manual Tests

- [ ] Smooth scrolling with no jank
- [ ] Scrolling speed feels natural (not too fast/slow)
- [ ] Text loops seamlessly (no gap between repetitions)
- [ ] Pause on hover works smoothly
- [ ] Responsive to window resize
- [ ] Works across different browsers

---

## ✅ Success Criteria

### Phase 1 & 2 (Basic Marquee)

- [ ] Marquee component scrolls text smoothly when overflow detected
- [ ] Static text displayed normally when fits in container
- [ ] File description from player state displays correctly
- [ ] Component is reusable across application
- [ ] No performance issues (CSS animation, not JavaScript)
- [ ] Proper spacing and alignment in player UI
- [ ] All tests passing
- [ ] Component documented in COMPONENT_LIBRARY.md

### Phase 3 (Retro Demo Effects)

- [ ] All 6 effects (wave, rainbow, glitch, bounce, copper, spiral) implemented
- [ ] Effects can be toggled via effect input property
- [ ] Character-level animations working correctly
- [ ] Smooth 60fps performance with effects enabled
- [ ] Effects work seamlessly with scrolling animation
- [ ] No visual glitches or rendering issues
- [ ] Effects documented with examples in COMPONENT_LIBRARY.md
- [ ] Cross-browser compatibility verified

---

## 📚 Related Documentation

- **Component Library**: [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md)
- **Player Domain**: [PLAYER_DOMAIN.md](../player-state/PLAYER_DOMAIN.md)
- **Style Guide**: [STYLE_GUIDE.md](../../STYLE_GUIDE.md)

---

## 📝 Technical Notes

### CSS Animation Approach

**Advantages**:

- Hardware-accelerated (GPU-based transform)
- Smooth 60fps animation
- No JavaScript interval overhead
- Automatic pause when tab inactive (browser optimization)

**Implementation**:

```scss
.marquee-container {
  overflow: hidden;
  width: 100%;
}

.marquee-content {
  display: inline-block;
  white-space: nowrap;

  &.scrolling {
    animation: scroll-left var(--scroll-duration) linear infinite;

    &:hover {
      animation-play-state: paused; // Pause on hover
    }
  }
}

@keyframes scroll-left {
  from {
    transform: translateX(0);
  }
  to {
    transform: translateX(-100%);
  }
}
```

### Overflow Detection

Use `afterNextRender` hook to measure after DOM updated:

```typescript
constructor() {
  afterNextRender(() => {
    this.checkOverflow();
  });

  // Re-check on window resize
  effect(() => {
    this.text(); // React to text changes
    this.checkOverflow();
  });
}
```

### Speed Calculation

Calculate animation duration based on content width and desired speed:

```typescript
private calculateDuration(): void {
  const content = this.contentRef.nativeElement;
  const widthPx = content.scrollWidth;
  const speedPxPerSec = this.speed();

  const durationSec = widthPx / speedPxPerSec;
  content.style.setProperty('--scroll-duration', `${durationSec}s`);
}
```

---

## 🎯 Future Enhancements

- **Bidirectional Scrolling**: Support right-to-left for RTL languages
- **Multiple Lines**: Vertical scrolling for multi-line content
- **Gradient Fade**: Fade edges for smoother visual transition
- **Rich Content**: Support for icons, images, or formatted text within marquee
- **Advanced Accessibility**: Enhanced ARIA attributes and screen reader support
- **Auto-Size**: Dynamically adjust height based on content
- **Additional Effects**: Plasma patterns, starfield depth, typewriter reveal
- **Effect Combinations**: Mix multiple effects (e.g., wave + rainbow)
- **User-Customizable Effects**: Let users configure effect parameters

---

## 📐 Design Specifications

### Spacing & Sizing

- **Height**: Auto (based on text size)
- **Padding**: 8px vertical, 16px horizontal
- **Font Size**: Inherit from parent (or 0.875rem)
- **Gap from file-other**: 1rem (consistent with other card spacing)

### Color & Styling

- **Text Color**: Inherit from theme
- **Background**: Transparent or subtle card background
- **Border**: Optional subtle border for definition
- **Hover State**: Slightly brighter on pause

### Animation Timing

- **Default Speed**: 50px/second (configurable)
- **Pause Duration on Hover**: Immediate (smooth transition)
- **Loop Gap**: Duplicate text with spacing to create seamless loop

---

## 🚀 Implementation Checklist

### Phase 1 Tasks

- [ ] Create `scrolling-marquee` component directory
- [ ] Implement component with text input
- [ ] Add CSS keyframe animations
- [ ] Implement overflow detection logic
- [ ] Add speed/direction/pause inputs
- [ ] Write component unit tests
- [ ] Export from ui/components index

### Phase 2 Tasks

- [ ] Inject PLAYER_CONTEXT in player-device-container
- [ ] Create fileDescription computed signal
- [ ] Add marquee to template below file-other
- [ ] Style marquee container
- [ ] Test with various file descriptions
- [ ] Update COMPONENT_LIBRARY.md documentation

### Phase 3 Tasks (Retro Demo Effects)

- [ ] Add MarqueeEffect type definition
- [ ] Add effect input property to component
- [ ] Implement character splitting logic
- [ ] Create wave effect keyframe animation
- [ ] Create rainbow cycling effect
- [ ] Create glitch distortion effect
- [ ] Create bounce elastic effect
- [ ] Create copper bars effect
- [ ] Create spiral twist effect
- [ ] Add effect class application logic
- [ ] Performance test all effects (60fps target)
- [ ] Add effect documentation to COMPONENT_LIBRARY.md
- [ ] Create effect preview/demo page (optional)

---

## 📊 Estimated Timeline

- **Phase 1** (Component Creation): 1-2 days

  - Component implementation: 4-6 hours
  - Animation and overflow logic: 2-3 hours
  - Testing: 2-3 hours

- **Phase 2** (Integration): 0.5-1 day

  - Player integration: 2-3 hours
  - Styling and layout: 1-2 hours
  - Testing and polish: 1-2 hours

- **Phase 3** (Retro Demo Effects): 2-3 days
  - Character splitting logic: 2-3 hours
  - Wave effect implementation: 1-2 hours
  - Rainbow cycling effect: 1-2 hours
  - Glitch effect: 2-3 hours
  - Bounce elastic effect: 1-2 hours
  - Copper bars effect: 2-3 hours
  - Spiral twist effect: 2-3 hours
  - Performance optimization: 2-4 hours
  - Testing and polish: 2-3 hours

**Total Estimated Time**:

- **Phases 1-2** (Basic Marquee): 2-3 days
- **Phase 3** (Retro Effects): 2-3 days additional
- **Complete Implementation**: 4-6 days total
