# Phase 6: Asset Image URLs & Animated Image Carousel

## 🎯 Objective

Implement full image URL mapping and create an animated carousel component to display file artwork, screenshots, and composer images with smooth scaling transitions. Images from the API will be mapped to complete URLs and displayed in a cycling carousel within the file-image component.

## 📚 Required Reading

**Complete product requirements**: [PLAYER_DOMAIN_DESIGN.md](./PLAYER_DOMAIN_DESIGN.md#phase-6-asset-image-urls--animated-image-carousel)

**Standards Documentation**:

- [x] [CODING_STANDARDS.md](../../CODING_STANDARDS.md) - Coding Standards
- [x] [STATE_STANDARDS.md](../../STATE_STANDARDS.md) - State Management Standards
- [x] [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md) - UI Component Patterns
- [x] [STYLE_GUIDE.md](../../STYLE_GUIDE.md) - Style Guide
- [x] [API_CLIENT_GENERATION.md](../../API_CLIENT_GENERATION.md) - API Client Generation

## File Tree

```
libs/
├── domain/src/lib/models/
│   └── viewable-item-image.model.ts           # MODIFIED: Add url property
│
├── infrastructure/src/lib/
│   ├── domain.mapper.ts                       # MODIFIED: Add baseApiUrl parameter
│   ├── domain.mapper.spec.ts                  # MODIFIED: Add URL construction tests
│   ├── player/
│   │   └── player.service.ts                  # MODIFIED: Pass base URL to mapper
│   └── storage/
│       └── storage.service.ts                 # MODIFIED: Pass base URL to mapper
│
├── ui/components/src/lib/
│   ├── cycle-image/                           # NEW COMPONENT
│   │   ├── cycle-image.component.ts           # NEW: Carousel logic
│   │   ├── cycle-image.component.html         # NEW: Template with ScalingContainer
│   │   ├── cycle-image.component.scss         # NEW: Rounded corner styles
│   │   └── cycle-image.component.spec.ts      # NEW: Unit tests
│   └── index.ts                               # MODIFIED: Export CycleImageComponent
│
├── features/player/src/lib/player-view/player-device-container/
│   ├── file-image/
│   │   ├── file-image.component.ts            # MODIFIED: Inject player context
│   │   └── file-image.component.html          # MODIFIED: Use CycleImageComponent
│   └── player-device-container.component.html # MODIFIED: Pass deviceId
│
└── data-access/api-client/src/lib/
    └── models/ViewableItemImageDto.ts         # GENERATED: Add baseAssetPath property

docs/features/player-state/
├── PLAYER_DOMAIN_DESIGN.md                    # MODIFIED: Add Phase 6 section
└── PLAYER_DOMAIN_P6.md                        # NEW: This document
```

## 📋 Implementation Tasks

### Task 1: API Client Regeneration

**Purpose**: Pick up `baseAssetPath` property on `ViewableItemImageDto` from backend changes.

- [ ] Build .NET API: `dotnet build ../../TeensyRom.Api.csproj`
- [ ] Regenerate TypeScript client: `pnpm run generate:api-client`
- [ ] Verify `ViewableItemImageDto` has `baseAssetPath?: string` property in generated models

**Reference**: [API_CLIENT_GENERATION.md](../../API_CLIENT_GENERATION.md)

**Note**: Assets API endpoint is commented out in backend - we only need the `baseAssetPath` property on the DTO, not the Assets service

---

### Task 2: Domain Model Update

**Purpose**: Add full URL property to domain model for image display.

**File**: `libs/domain/src/lib/models/viewable-item-image.model.ts`

- [ ] Add `url: string` property to `ViewableItemImage` interface
- [ ] Property stores full URL constructed from base API URL + baseAssetPath
- [ ] Used by UI components to display images directly

**Interface Shape**:

```typescript
export interface ViewableItemImage {
  fileName: string;
  path: string;
  source: string;
  url: string; // NEW: Full URL to image asset
}
```

---

### Task 3: Infrastructure Mapping - DomainMapper Updates

**Purpose**: Update mapper to construct full image URLs from API data.

**File**: `libs/infrastructure/src/lib/domain.mapper.ts`

- [ ] Update `toViewableItemImage()` signature to accept `baseApiUrl: string` parameter
- [ ] Construct `url` by concatenating `baseApiUrl + dto.baseAssetPath`
- [ ] Handle empty/missing `baseAssetPath` (set `url` to empty string)
- [ ] Update `toFileItem()` signature to accept `baseApiUrl: string` parameter
- [ ] Pass `baseApiUrl` to `toViewableItemImage()` when mapping images array
- [ ] Maintain null safety for all transformations

**Method Signatures**:

```typescript
static toViewableItemImage(dto: ViewableItemImageDto, baseApiUrl: string): ViewableItemImage
static toFileItem(dto: FileItemDto, baseApiUrl: string): FileItem
```

**Reference**: Existing mapper patterns in `libs/infrastructure/src/lib/domain.mapper.ts`

---

### Task 4: DomainMapper Unit Tests

**Purpose**: Verify URL construction logic with comprehensive tests.

**File**: `libs/infrastructure/src/lib/domain.mapper.spec.ts`

- [ ] Test `toViewableItemImage()` concatenates `baseApiUrl + baseAssetPath` correctly
- [ ] Test empty `baseAssetPath` results in empty `url` string
- [ ] Test `toFileItem()` passes `baseApiUrl` to image mapper
- [ ] Test multiple images in array all receive correct URLs
- [ ] Test null safety (missing properties don't cause errors)

**Test Cases**:

- Valid baseAssetPath: `http://localhost:5168` + `/Assets/Games/Screenshots/game.png` = `http://localhost:5168/Assets/Games/Screenshots/game.png`
- Empty baseAssetPath: `url` = `''`
- Multiple images: All receive proper URLs

**Reference**: Existing mapper tests in `libs/infrastructure/src/lib/domain.mapper.spec.ts`

---

### Task 5: PlayerService Update

**Purpose**: Extract base URL and pass to mapper when mapping FileItem responses.

**File**: `libs/infrastructure/src/lib/player/player.service.ts`

- [ ] Extract `baseApiUrl` from `PlayerApiService` configuration in constructor
- [ ] Store as private readonly property: `private readonly baseApiUrl: string`
- [ ] Use pattern: `(this.apiService as any).configuration?.basePath || 'http://localhost:5168'`
- [ ] Update `launchFile()` to pass `baseApiUrl` to `DomainMapper.toFileItem()`
- [ ] Update `launchRandom()` to pass `baseApiUrl` to `DomainMapper.toFileItem()`

**Reference**: See `libs/infrastructure/src/lib/storage/storage.service.ts` for similar base URL extraction pattern

---

### Task 6: StorageService Update

**Purpose**: Extract base URL and pass to mapper when mapping FileItem objects.

**File**: `libs/infrastructure/src/lib/storage/storage.service.ts`

- [ ] Extract `baseApiUrl` from `FilesApiService` configuration in constructor
- [ ] Store as private readonly property: `private readonly baseApiUrl: string`
- [ ] Use pattern: `(this.apiService as any).configuration?.basePath || 'http://localhost:5168'`
- [ ] Update `toStorageDirectoryWithBaseUrl()` helper to pass `baseApiUrl` to `DomainMapper.toFileItem()`
- [ ] Ensure all file mappings receive base URL

**Current Pattern**: Service already has `toStorageDirectoryWithBaseUrl()` helper - extend it to pass base URL

---

### Task 7: Generate CycleImageComponent

**Purpose**: Create component structure using Nx generator.

- [ ] Run: `npx nx g @nx/angular:component cycle-image --project=ui-components --export`
- [ ] Verify component created in `libs/ui/components/src/lib/cycle-image/`
- [ ] Verify component exported in `libs/ui/components/src/index.ts`

---

### Task 8: CycleImageComponent TypeScript Implementation

**Purpose**: Implement carousel logic with timer-based image cycling and DOM re-rendering.

**File**: `libs/ui/components/src/lib/cycle-image/cycle-image.component.ts`

- [ ] Import: `CommonModule`, `ScalingContainerComponent`
- [ ] Import RxJS: `interval`, `takeUntilDestroyed`
- [ ] Import Angular: `signal`, `computed`, `effect`, `DestroyRef`, `inject`
- [ ] Add input: `images = input.required<string[]>()` - Array of image URLs
- [ ] Add input: `intervalMs = input<number>(3000)` - Cycle interval (default 3000ms)
- [ ] Add input: `animationDirection = input<string>('random')` - ScalingContainer animation
- [ ] Add signal: `currentIndex = signal(0)` - Current image index
- [ ] Add signal: `imageKey = signal(0)` - Forces DOM re-render for animation
- [ ] Add computed: `currentImage = computed(() => images()[currentIndex()] || null)`
- [ ] Add computed: `hasMultipleImages = computed(() => images().length > 1)`
- [ ] Inject: `DestroyRef` for cleanup
- [ ] Use `effect()` to start carousel when `hasMultipleImages()` is true
- [ ] Use `interval(intervalMs()).pipe(takeUntilDestroyed(destroyRef))` for timer
- [ ] On interval tick: increment `currentIndex` (wrap around), increment `imageKey`
- [ ] Handle empty arrays gracefully (show nothing)

**Key Pattern**: Increment `imageKey` signal on each cycle to force `@for` track to recreate DOM element

**Reference**: [COMPONENT_LIBRARY.md - ScalingContainerComponent](../../COMPONENT_LIBRARY.md#scalingcontainercomponent)

---

### Task 9: CycleImageComponent Template

**Purpose**: Render current image with ScalingContainer animation wrapper.

**File**: `libs/ui/components/src/lib/cycle-image/cycle-image.component.html`

- [ ] Use `@if (currentImage())` to check for valid image
- [ ] Use `@for (img of [currentImage()]; track imageKey())` to force DOM re-render
- [ ] Wrap image in `<lib-scaling-container [animationEntry]="animationDirection()">`
- [ ] Add `<img>` with `[src]="img"`, `alt="Viewable item image"`, `class="carousel-image"`

**Template Structure**:

```html
@if (currentImage()) { @for (img of [currentImage()]; track imageKey()) {
<lib-scaling-container [animationEntry]="animationDirection()">
  <img [src]="img" alt="Viewable item image" class="carousel-image" />
</lib-scaling-container>
} }
```

**Key Pattern**: `track imageKey()` changes force Angular to destroy/recreate element, triggering ScalingContainer animation

**Reference**: [COMPONENT_LIBRARY.md - Animation System](../../COMPONENT_LIBRARY.md#animation-system)

---

### Task 10: CycleImageComponent Styles

**Purpose**: Apply rounded corners and proper sizing to images.

**File**: `libs/ui/components/src/lib/cycle-image/cycle-image.component.scss`

- [ ] Host: `display: block`, `width: 100%`, `height: 100%`
- [ ] `.carousel-image`: `max-width: 100%`, `max-height: 100%`
- [ ] `.carousel-image`: `width: auto`, `height: auto`
- [ ] `.carousel-image`: `object-fit: contain`
- [ ] `.carousel-image`: `display: block`, `margin: 0 auto`
- [ ] `.carousel-image`: `border-radius: 8px` (matches existing placeholder)

**Reference**: `libs/features/player/.../file-image/file-image.component.scss` for existing image styles

---

### Task 11: CycleImageComponent Unit Tests

**Purpose**: Verify carousel cycling, DOM re-rendering, and edge cases.

**File**: `libs/ui/components/src/lib/cycle-image/cycle-image.component.spec.ts`

- [ ] Test: Component creates successfully
- [ ] Test: Displays current image from array
- [ ] Test: Cycles through images at correct interval (use `fakeAsync`, `tick`)
- [ ] Test: `currentIndex` wraps around to 0 after last image
- [ ] Test: `imageKey` increments on each transition
- [ ] Test: Handles single image (no cycling)
- [ ] Test: Handles empty array (shows nothing)
- [ ] Test: No memory leaks (subscriptions cleaned up)

**Reference**: [STORE_TESTING.md](../../STORE_TESTING.md) for testing patterns

---

### Task 12: FileImageComponent TypeScript Update

**Purpose**: Inject player context and derive image URLs from current file.

**File**: `libs/features/player/src/lib/player-view/player-device-container/file-image/file-image.component.ts`

- [ ] Import: `PLAYER_CONTEXT`, `IPlayerContext` from `@teensyrom-nx/application`
- [ ] Import: `CycleImageComponent` from `@teensyrom-nx/ui/components`
- [ ] Inject: `private readonly playerContext = inject(PLAYER_CONTEXT)`
- [ ] Add input: `deviceId = input.required<string>()`
- [ ] Add computed: `currentFile = computed(() => this.playerContext.getCurrentFile(this.deviceId())())`
- [ ] Add computed: `imageUrls = computed(() => this.currentFile()?.images.map(img => img.url).filter(url => url && url.length > 0) ?? [])`
- [ ] Add computed: `hasImages = computed(() => this.imageUrls().length > 0)`
- [ ] Keep existing inputs: `creatorName`, `metadataSource`

**Pattern**: Use player context to access current file signal, derive image URLs

**Reference**: `libs/features/player/.../player-device-container/player-device-container.component.ts` for player context usage

---

### Task 13: FileImageComponent Template Update

**Purpose**: Display carousel when images available, otherwise show placeholder.

**File**: `libs/features/player/src/lib/player-view/player-device-container/file-image/file-image.component.html`

- [ ] Keep existing: `<lib-scaling-card [title]="creatorName()" [metadataSource]="metadataSource()">`
- [ ] Replace content with conditional:
  - `@if (hasImages())` - Show `<lib-cycle-image [images]="imageUrls()" [intervalMs]="3000" animationDirection="random" />`
  - `@else` - Show `<img src="/placeholder.jpg" alt="Artist Image" />`

**Template Structure**:

```html
<lib-scaling-card [title]="creatorName()" [metadataSource]="metadataSource()">
  @if (hasImages()) {
  <lib-cycle-image [images]="imageUrls()" [intervalMs]="3000" animationDirection="random" />
  } @else {
  <img src="/placeholder.jpg" alt="Artist Image" />
  }
</lib-scaling-card>
```

---

### Task 14: PlayerDeviceContainerComponent Template Update

**Purpose**: Pass deviceId to file-image component.

**File**: `libs/features/player/src/lib/player-view/player-device-container/player-device-container.component.html`

- [ ] Locate `<lib-file-image>` usage
- [ ] Add binding: `[deviceId]="deviceId()"`
- [ ] Keep existing bindings: `[creatorName]`, `[metadataSource]`

**Reference**: Component already has `deviceId()` computed signal available

---

### Task 15: Update UI Components Barrel Export

**Purpose**: Make CycleImageComponent available for import.

**File**: `libs/ui/components/src/index.ts`

- [ ] Add: `export * from './lib/cycle-image/cycle-image.component';`
- [ ] Verify alphabetical ordering with other exports

---

### Task 16: Update PLAYER_DOMAIN_DESIGN.md

**Purpose**: Add Phase 6 to high-level design document.

**File**: `docs/features/player-state/PLAYER_DOMAIN_DESIGN.md`

- [ ] Add Phase 6 section after Phase 5 in "Incremental Development Phases"
- [ ] Include goal, scope, additions, and demonstrable value
- [ ] Cross-reference this document (PLAYER_DOMAIN_P6.md)

**Section Content**:

```markdown
### Phase 6: Asset Image URLs & Animated Image Carousel

**Goal**: Display file artwork/screenshots with animated carousel cycling through available images

**Scope**:

- Regenerate API client to pick up `baseAssetPath` property on `ViewableItemImageDto`
- Add `url` property to `ViewableItemImage` domain model
- Update `DomainMapper` to construct full URLs from `baseAssetPath` + base API URL
- Extract base API URL from existing API service configurations (PlayerApiService, FilesApiService)
- Modify `PlayerService` and `StorageService` to pass base API URL to mapper
- Create reusable `CycleImageComponent` with timer-based image cycling and scaling animations
- Integrate carousel into `FileImageComponent` using player context signals
- **NO** custom Asset service - base URL extracted from existing infrastructure services

**Additions**:

- `ViewableItemImage.url` property (full URL to image asset)
- `DomainMapper.toViewableItemImage(dto, baseApiUrl)` - accepts base URL parameter
- `DomainMapper.toFileItem(dto, baseApiUrl)` - passes URL to image mapper
- Base URL extraction from PlayerApiService and FilesApiService configurations
- `CycleImageComponent` - reusable carousel with ScalingContainer animations
- `FileImageComponent` integration with player context signals

**Demonstrable Value**: Users see file artwork/screenshots with smooth animated transitions
```

---

## 🗂️ File Changes

**New Files**:

- [libs/ui/components/src/lib/cycle-image/cycle-image.component.ts](../../../libs/ui/components/src/lib/cycle-image/cycle-image.component.ts)
- [libs/ui/components/src/lib/cycle-image/cycle-image.component.html](../../../libs/ui/components/src/lib/cycle-image/cycle-image.component.html)
- [libs/ui/components/src/lib/cycle-image/cycle-image.component.scss](../../../libs/ui/components/src/lib/cycle-image/cycle-image.component.scss)
- [libs/ui/components/src/lib/cycle-image/cycle-image.component.spec.ts](../../../libs/ui/components/src/lib/cycle-image/cycle-image.component.spec.ts)

**Modified Files**:

- [libs/domain/src/lib/models/viewable-item-image.model.ts](../../../libs/domain/src/lib/models/viewable-item-image.model.ts)
- [libs/infrastructure/src/lib/domain.mapper.ts](../../../libs/infrastructure/src/lib/domain.mapper.ts)
- [libs/infrastructure/src/lib/domain.mapper.spec.ts](../../../libs/infrastructure/src/lib/domain.mapper.spec.ts)
- [libs/infrastructure/src/lib/player/player.service.ts](../../../libs/infrastructure/src/lib/player/player.service.ts)
- [libs/infrastructure/src/lib/storage/storage.service.ts](../../../libs/infrastructure/src/lib/storage/storage.service.ts)
- [libs/features/player/src/lib/player-view/player-device-container/file-image/file-image.component.ts](../../../libs/features/player/src/lib/player-view/player-device-container/file-image/file-image.component.ts)
- [libs/features/player/src/lib/player-view/player-device-container/file-image/file-image.component.html](../../../libs/features/player/src/lib/player-view/player-device-container/file-image/file-image.component.html)
- [libs/features/player/src/lib/player-view/player-device-container/player-device-container.component.html](../../../libs/features/player/src/lib/player-view/player-device-container/player-device-container.component.html)
- [libs/ui/components/src/index.ts](../../../libs/ui/components/src/index.ts)
- [PLAYER_DOMAIN_DESIGN.md](./PLAYER_DOMAIN_DESIGN.md)

**Generated Files** (from API client regeneration):

- Updates to `libs/data-access/api-client/src/lib/models/ViewableItemImageDto.ts` (adds `baseAssetPath` property)

---

## 🧪 Testing Requirements

### Unit Tests

**DomainMapper Tests** (`domain.mapper.spec.ts`):

- [ ] `toViewableItemImage()` constructs URL from baseApiUrl + baseAssetPath
- [ ] `toViewableItemImage()` handles empty baseAssetPath (returns empty url)
- [ ] `toFileItem()` passes baseApiUrl to image mapper correctly
- [ ] Multiple images in FileItem all receive proper URLs

**CycleImageComponent Tests** (`cycle-image.component.spec.ts`):

- [ ] Component creates successfully with required images input
- [ ] Displays current image from array
- [ ] Cycles through images at specified interval (fakeAsync testing)
- [ ] currentIndex wraps around to 0 after last image
- [ ] imageKey increments on each transition to force DOM re-render
- [ ] Handles single image gracefully (no cycling occurs)
- [ ] Handles empty array gracefully (renders nothing)
- [ ] Interval timer cleanup on destroy (no memory leaks)

**FileImageComponent Integration**:

- [ ] Injects player context successfully
- [ ] currentFile signal derives from player context
- [ ] imageUrls signal extracts URLs from currentFile images
- [ ] hasImages computed correctly determines if images exist
- [ ] Renders CycleImageComponent when images available
- [ ] Renders placeholder when no images available

### Integration Tests

- [ ] Full workflow: API response → DomainMapper → FileItem with URLs → Component display
- [ ] Player context provides current file to FileImageComponent
- [ ] Carousel cycles through multiple images with animations
- [ ] ScalingContainer animations trigger on each image change
- [ ] Rounded corners applied to all images
- [ ] No memory leaks from interval subscriptions

---

## ✅ Success Criteria

- [ ] API client regenerated with baseAssetPath property on ViewableItemImageDto
- [ ] ViewableItemImage has url property in domain model
- [ ] Base URL successfully extracted from PlayerApiService and FilesApiService
- [ ] DomainMapper constructs full URLs from baseApiUrl + baseAssetPath
- [ ] PlayerService and StorageService pass base URL to mapper
- [ ] All DomainMapper unit tests passing
- [ ] CycleImageComponent created and all unit tests passing
- [ ] Carousel cycles through images at 3-second intervals
- [ ] ScalingContainer animations trigger on each image transition
- [ ] Images display with 8px rounded corners
- [ ] FileImageComponent shows carousel when images available
- [ ] FileImageComponent shows placeholder when no images
- [ ] Player context integration works correctly
- [ ] No console errors or warnings
- [ ] No memory leaks from timers or subscriptions
- [ ] Ready to proceed to next phase

---

## 📝 Notes

**Design Decisions**:

- **No Asset Service**: Base URL extracted from PlayerApiService and FilesApiService configurations via `apiService.configuration?.basePath`
- **No Assets API Endpoint**: Backend Assets endpoint is commented out - only using baseAssetPath property on ViewableItemImageDto
- **DOM Re-rendering Strategy**: Using `@for` with changing `track imageKey()` to force element recreation for animations
- **Timer Cleanup**: Using `takeUntilDestroyed(DestroyRef)` for automatic subscription cleanup
- **Fallback Behavior**: Empty or missing baseAssetPath results in empty url string, component shows placeholder

**Future Enhancements** (Not in Phase 6):

- Custom timer durations per image
- Pause carousel on hover
- Manual navigation controls (prev/next buttons)
- Thumbnail indicators
- Zoom/lightbox functionality
- Image preloading for smoother transitions

**Dependencies**:

- Phase 5 must be complete (player context and timer system)
- Backend must provide baseAssetPath on ViewableItemImageDto
- ScalingContainerComponent must be available in ui/components
