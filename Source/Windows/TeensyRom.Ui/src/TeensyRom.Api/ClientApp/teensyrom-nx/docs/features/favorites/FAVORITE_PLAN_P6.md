# Phase 6: Cypress Favorite Workflow E2E Tests

## 🎯 Objective

Deliver a deterministic Cypress test suite that validates the end-to-end favorite file workflow from the player toolbar, covering happy paths, failure handling, UI feedback, and directory synchronization using a **realistic, stateful mock file system backend**.

> **Revised Approach**: Build a reusable mock file system that simulates real TeensyROM storage structure, supporting stateful mutations for save/remove operations while remaining deterministic and resettable across tests.

---

## 📚 Required Reading

**Feature Documentation:**
- [x] [Favorite Feature Plan](./FAVORITE_PLAN.md) – End-to-end functional requirements
- [x] [Storage Mock Sample](../storage/STORAGE_E2E_MOCK_SAMPLE.md) – Real-world file structure examples
- [x] [E2E Test Suite Guide](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) – Cypress architecture and patterns
- [x] [Device Discovery E2E Reference](../../../apps/teensyrom-ui-e2e/src/e2e/devices/E2E_DEVICE_DISCOVERY.md) – Example workflow spec

**Standards & Guidelines:**
- [x] [Coding Standards](../../CODING_STANDARDS.md)
- [x] [Testing Standards](../../TESTING_STANDARDS.md)
- [x] [E2E Constants](../../../apps/teensyrom-ui-e2e/src/support/constants/E2E_CONSTANTS.md)
- [x] [E2E Fixtures](../../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/E2E_FIXTURES.md)
- [x] [E2E Interceptors](../../../apps/teensyrom-ui-e2e/src/support/interceptors/E2E_INTERCEPTORS.md)

**API Reference:**
- OpenAPI Spec: `openapi-spec.json` - Lines 923-1056 (FileItemDto), 1115-1129 (GetDirectoryResponse)
- Backend Code: `FavoriteFileEndpoint.cs`, `RemoveFavoriteEndpoint.cs`, `FavoriteFileModels.cs`
- TypeScript Client: `libs/data-access/api-client/src/lib/apis/FilesApiService.ts`

---

## 📐 Architecture Overview

```
MockFilesystem (in-memory state, deterministic seed)
    ↓
Storage Generators (Faker, realistic file names/sizes)
    ↓
Storage Fixtures (pre-built test scenarios)
    ↓
Storage Interceptors (cy.intercept → filesystem mutations)
    ↓
Test Helpers (navigation, actions, assertions)
    ↓
E2E Specs (Given/When/Then scenarios)
```

**Core Principles:**
1. **Deterministic** - Same seed produces identical files every time
2. **Stateful** - Interceptors mutate shared filesystem instance
3. **Resettable** - Each test gets fresh `createMockFilesystem(seed)`
4. **Realistic** - Matches real TeensyROM SD card structure
5. **Reusable** - Foundation for future storage tests (search, playlists, etc.)

---

## 📂 Real-World Storage Structure

Based on STORAGE_E2E_MOCK_SAMPLE.md (actual TeensyROM SD card):

### Source Files (Deep Nested Paths)
```
/games/Pac-Man (J1).crt                          (flat, 2000+ files)
/music/MUSICIANS/L/LukHash/Alpha.sid             (5 levels deep)
/music/DEMOS/S-Z/Star_Trek_II.sid                (3 levels deep)
/images/SonicTheHedgehog.kla                     (flat)
```

### Favorites (Flat Directories - Copies from Deep Paths)
```
/favorites/games/Pac-Man (J1).crt                (copy, parentPath: '/games')
/favorites/music/Alpha.sid                       (copy, parentPath: '/music/MUSICIANS/L/LukHash')
/favorites/images/SonicTheHedgehog.kla           (copy, parentPath: '/images')
```

### File Type → Favorites Path Mapping

| FileItemType | Source Example | Favorites Path | Backend Reference |
|--------------|----------------|----------------|-------------------|
| `Game` | `/games/Pac-Man.crt` | `/favorites/games/` | `StorageHelper.GetFavoritePath(TeensyFileType.Prg)` |
| `Song` | `/music/MUSICIANS/L/LukHash/Alpha.sid` | `/favorites/music/` | `StorageHelper.GetFavoritePath(TeensyFileType.Sid)` |
| `Image` | `/images/Sonic.kla` | `/favorites/images/` | `StorageHelper.GetFavoritePath(TeensyFileType.Koa)` |

**Critical Insight:** Favorites creates **flat copies** (no subdirectories), tracking original location via `parentPath` field.

---

## 📂 File Structure Overview

> All paths relative to workspace root: `apps/teensyrom-ui-e2e/`

```
apps/teensyrom-ui-e2e/
├── src/
│   ├── e2e/
│   │   └── storage/                           ✨ NEW
│   │       ├── favorite-operations.cy.ts      ✨ Main test spec
│   │       └── test-helpers.ts                ✨ Reusable helpers
│   └── support/
│       ├── constants/
│       │   ├── api.constants.ts               📝 Add STORAGE_ENDPOINTS
│       │   └── selector.constants.ts          📝 Add PLAYER_TOOLBAR_SELECTORS
│       ├── interceptors/
│       │   ├── storage.interceptors.ts        ✨ Stateful interceptors
│       │   └── index.ts                       📝 Export new interceptors
│       └── test-data/
│           ├── fixtures/
│           │   ├── storage-favorites.fixture.ts    ✨ Scenario fixtures
│           │   ├── README.md                       📝 Document fixtures
│           │   └── index.ts                        📝 Export fixtures
│           ├── generators/
│           │   └── storage.generators.ts           ✨ Filesystem generator
│           └── mock-filesystem/                    ✨ NEW
│               ├── filesystem.types.ts             ✨ Type definitions
│               └── mock-filesystem.ts              ✨ Core class
```

---

## 🔄 Bootstrap Sequence

**Critical Understanding:** The filesystem initialization follows this pattern:

```
1. MockFilesystem constructor creates empty Map
2. constructor() calls this.generate() → initially EMPTY (no generators yet)
3. Task 2 creates generators (generateGamesDirectory, etc.)
4. Task 2 creates createMockFilesystem() factory that:
   a. Instantiates MockFilesystem(seed)
   b. Calls filesystem.addDirectory() to populate it
   c. Returns populated filesystem
5. Tests use createMockFilesystem(12345) to get fresh instances
```

**Why this matters:** The constructor's `generate()` method is **initially empty**. Population happens via the **factory function** in Task 2.

---

## 🎮 Player Context Integration

**Critical Understanding:** Tests need to load specific files into the player before favoriting them.

### How the Player Loads Files

The player component receives the current file through one of these methods:

1. **Route Parameters** - File path passed via URL:
   ```typescript
   cy.visit(`${APP_ROUTES.player}?filePath=/games/Pac-Man%20(J1).crt`);
   ```

2. **Direct Navigation** - Click file in storage browser:
   ```typescript
   cy.get(STORAGE_VIEW_SELECTORS.directoryList).within(() => {
     cy.contains('games').click();
   });
   cy.get(STORAGE_VIEW_SELECTORS.fileItem).contains('Pac-Man (J1).crt').click();
   ```

3. **State Injection** - Pre-load file via test helper:
   ```typescript
   export function loadFileInPlayer(filePath: string, fileType: FileItemType): void {
     // Option 1: Use URL parameters
     cy.visit(`${APP_ROUTES.player}?filePath=${encodeURIComponent(filePath)}`);

     // OR Option 2: Inject into player state (if using state management)
     // cy.window().its('store').invoke('dispatch', loadFile({ path: filePath }));
   }
   ```

### Test Helper Addition (Task 7)

Add this helper to `test-helpers.ts`:

```typescript
/**
 * Load a specific file into the player
 * This simulates user navigation or direct file loading
 */
export function loadFileInPlayer(filePath: string): Cypress.Chainable<void> {
  return cy.visit(`${APP_ROUTES.player}?filePath=${encodeURIComponent(filePath)}`, {
    onBeforeLoad: (win) => {
      // Preserve device connection state but clear other session data
      const deviceState = win.sessionStorage.getItem('connectedDevice');
      win.sessionStorage.clear();
      if (deviceState) {
        win.sessionStorage.setItem('connectedDevice', deviceState);
      }
    },
  });
}
```

**Usage in Tests:**

```typescript
// Load game file
loadFileInPlayer('/games/Pac-Man (J1).crt');
cy.wait('@getDirectory'); // Wait for player to load file metadata

// Load music file
loadFileInPlayer('/music/MUSICIANS/L/LukHash/Alpha.sid');
cy.wait('@getDirectory');

// Load image file
loadFileInPlayer('/images/SonicTheHedgehog.kla');
cy.wait('@getDirectory');
```

**Important Notes:**
- Always wait for `@getDirectory` after loading to ensure file metadata loads
- The `filePath` must match exact paths in mock filesystem
- Player will trigger interceptors to fetch file details
- Icon state (`isFavorite`) comes from the loaded file's metadata

---

<details open>
<summary><h3>Task 1: Build Mock File System Core</h3></summary>

**Purpose**: Create stateful, in-memory file system that supports directory traversal and favorite operations.

**Prerequisites**: None - this is the foundation.

**Files to Create:**
- `src/support/test-data/mock-filesystem/filesystem.types.ts`
- `src/support/test-data/mock-filesystem/mock-filesystem.ts`

---

#### Step 1: Define Types (`filesystem.types.ts`)

Reference OpenAPI spec for `FileItemDto` structure.

**Important Notes:**
- **MockFile** uses `FileItemDto` directly - all fields are relevant for our mock filesystem
- This keeps us aligned with the actual API contract and ensures compatibility
- Use TypeScript path aliases for imports (e.g., `@teensyrom-nx/data-access/api-client`)

```typescript
import type {
  FileItemDto,
  FileItemType,
  DirectoryItemDto,
  StorageCacheDto,
  GetDirectoryResponse,
  SaveFavoriteResponse,
  RemoveFavoriteResponse
} from '@teensyrom-nx/data-access/api-client';

/**
 * Mock file that matches the FileItemDto structure exactly.
 * All fields from FileItemDto are used in our mock filesystem.
 */
export type MockFile = FileItemDto;

/**
 * Mock directory containing files and subdirectories.
 */
export interface MockDirectory {
  path: string;                    // "/games"
  files: MockFile[];
  subdirectories: string[];        // ["Extras", "Large"]
}

// Re-export FileItemType for convenience
export type { FileItemType };
```

---

#### Step 2: Create MockFilesystem Class (`mock-filesystem.ts`)

**Full Class Structure with ALL Methods:**

```typescript
import type {
  GetDirectoryResponse,
  SaveFavoriteResponse,
  RemoveFavoriteResponse,
  DirectoryItemDto
} from '@teensyrom-nx/data-access/api-client';
import type { MockFile, MockDirectory, FileItemType } from './filesystem.types';

export class MockFilesystem {
  private directories: Map<string, MockDirectory>;
  private readonly seed: number;

  constructor(seed: number = 12345) {
    this.seed = seed;
    this.directories = new Map();
    this.generate();  // Initially empty - populated by factory
  }

  // ============== Public API Methods ==============

  /**
   * Add a directory to the filesystem
   * Called by createMockFilesystem() factory to populate structure
   */
  addDirectory(path: string, directory: MockDirectory): void {
    this.directories.set(path, directory);
  }

  /**
   * Get directory contents (matches GetDirectoryResponse schema)
   */
  getDirectory(path: string): GetDirectoryResponse {
    const dir = this.directories.get(path);

    if (!dir) {
      // Return empty directory if not found
      return {
        storageItem: {
          path: path,
          directories: [],
          files: []
        },
        message: 'Success'
      };
    }

    // Map subdirectories to DirectoryItemDto
    const directoryItems: DirectoryItemDto[] = dir.subdirectories.map(name => ({
      name: name,
      path: `${path}/${name}`
    }));

    return {
      storageItem: {
        path: dir.path,
        directories: directoryItems,
        files: dir.files  // Already FileItemDto compatible
      },
      message: 'Success'
    };
  }

  /**
   * Save a file as favorite (mutates state)
   */
  saveFavorite(filePath: string): SaveFavoriteResponse {
    // 1. Find the file
    const file = this.findFile(filePath);
    if (!file) {
      throw new Error(`File not found: ${filePath}`);
    }

    // 2. Mark original as favorite
    file.isFavorite = true;

    // 3. Determine favorites destination
    const favoritesPath = this.getFavoritePathForType(file.type);
    const favoriteFilePath = `${favoritesPath}/${file.name}`;

    // 4. Create copy in favorites directory
    const favoriteCopy: MockFile = {
      ...file,
      path: favoriteFilePath,
      parentPath: file.parentPath,  // Tracks original location
      isFavorite: true
    };

    // 5. Add to favorites directory
    const favoritesDir = this.directories.get(favoritesPath);
    if (favoritesDir) {
      // Remove existing if already there
      favoritesDir.files = favoritesDir.files.filter(f => f.name !== file.name);
      // Add new copy
      favoritesDir.files.push(favoriteCopy);
    }

    // 6. Return response matching backend format
    return {
      message: `Favorite tagged and saved to ${favoritesPath}`,
      favoriteFile: favoriteCopy,
      favoritePath: favoritesPath
    };
  }

  /**
   * Remove a file from favorites (mutates state)
   */
  removeFavorite(filePath: string): RemoveFavoriteResponse {
    let originalFile: MockFile | null = null;
    let favoritesPath: string = '';

    // Accept either original path or favorites path
    if (filePath.startsWith('/favorites/')) {
      // Favorites path - extract file name and find original
      const fileName = filePath.split('/').pop()!;
      const fileType = this.getTypeFromFavoritesPath(filePath);
      favoritesPath = this.getFavoritePathForType(fileType);

      // Find the favorite copy to get parentPath
      const favoritesDir = this.directories.get(favoritesPath);
      const favoriteCopy = favoritesDir?.files.find(f => f.name === fileName);

      if (favoriteCopy) {
        // Use parentPath to find original
        const originalDir = this.directories.get(favoriteCopy.parentPath);
        originalFile = originalDir?.files.find(f => f.name === fileName) || null;
      }
    } else {
      // Original path - find directly
      originalFile = this.findFile(filePath);
      if (originalFile) {
        favoritesPath = this.getFavoritePathForType(originalFile.type);
      }
    }

    if (!originalFile) {
      throw new Error(`File not found: ${filePath}`);
    }

    // Mark original as not favorite
    originalFile.isFavorite = false;

    // Remove from favorites directory
    const favoritesDir = this.directories.get(favoritesPath);
    if (favoritesDir) {
      favoritesDir.files = favoritesDir.files.filter(f => f.name !== originalFile!.name);
    }

    return {
      message: `Favorite untagged and removed from ${favoritesPath}`
    };
  }

  /**
   * Reset filesystem to initial state
   */
  reset(): void {
    this.directories.clear();
    this.generate();
  }

  // ============== Private Helper Methods ==============

  /**
   * Generate initial filesystem structure
   * Note: Initially empty - populated by createMockFilesystem() factory
   */
  private generate(): void {
    // Empty by design - population happens via addDirectory()
  }

  /**
   * Get favorites path for file type
   */
  private getFavoritePathForType(type: FileItemType): string {
    switch (type) {
      case 'Game': return '/favorites/games';
      case 'Song': return '/favorites/music';
      case 'Image': return '/favorites/images';
      case 'Hex': return '/favorites/games';  // Fallback
      default: return '/favorites/games';
    }
  }

  /**
   * Get file type from favorites path
   */
  private getTypeFromFavoritesPath(path: string): FileItemType {
    if (path.includes('/favorites/games')) return 'Game';
    if (path.includes('/favorites/music')) return 'Song';
    if (path.includes('/favorites/images')) return 'Image';
    return 'Game';
  }

  /**
   * Find file by path across all directories
   */
  private findFile(path: string): MockFile | null {
    for (const dir of this.directories.values()) {
      const file = dir.files.find(f => f.path === path);
      if (file) return file;
    }
    return null;
  }
}
```

---

#### Step 3: Unit Test the Filesystem

**Create:** `src/support/test-data/mock-filesystem/mock-filesystem.spec.ts`

**Test Cases:**
1. **Determinism**: Same seed produces identical files across multiple resets
2. **saveFavorite()**:
   - Original file gets `isFavorite = true`
   - Copy appears in `/favorites/{type}/` with correct `parentPath`
   - Response matches `SaveFavoriteResponse` schema
3. **removeFavorite()**:
   - Accepts original path
   - Accepts favorites path
   - Original file gets `isFavorite = false`
   - File removed from `/favorites/{type}/`
   - Response matches `RemoveFavoriteResponse` schema
4. **getDirectory()**:
   - Returns files for existing path
   - Returns empty arrays for non-existent path
   - Response matches `GetDirectoryResponse` schema
5. **State persistence**: Changes persist across `getDirectory()` calls

</details>

---

<details open>
<summary><h3>Task 2: Build Storage Data Generators</h3></summary>

**Purpose**: Generate realistic file system structures using seeded Faker.

**Prerequisites**: Task 1 (filesystem types defined)

**Files to Create:**
- `src/support/test-data/generators/storage.generators.ts`
- `src/support/test-data/generators/storage.generators.spec.ts`

**Reference Patterns:**
- Existing: `src/support/test-data/generators/device.generators.ts`
- Faker config: `src/support/test-data/faker-config.ts`

---

#### Step 1: Create File Item Generator

```typescript
import { faker } from '../faker-config';
import type { FileItemDto } from '@teensyrom-nx/data-access/api-client';

export function generateFileItem(overrides?: Partial<FileItemDto>): FileItemDto {
  return {
    name: faker.system.fileName(),
    path: '',  // Set by caller based on directory
    size: faker.number.int({ min: 1024, max: 102400 }),
    type: 'Game',
    isFavorite: false,
    isCompatible: true,
    title: faker.lorem.words(3),
    creator: faker.person.fullName(),
    releaseInfo: faker.date.past().getFullYear().toString(),
    description: faker.lorem.sentence(),
    // ... all other required FileItemDto fields with sensible defaults
    ...overrides
  };
}
```

---

#### Step 2: Create Directory Generators

**Use STORAGE_E2E_MOCK_SAMPLE.md for realistic names/sizes:**

```typescript
export function generateGamesDirectory(seed: number): MockFile[] {
  faker.seed(seed);

  const gameNames = [
    '10th Frame.crt',
    '1942 (Music v1).crt',
    'Donkey Kong (Ocean).crt',
    'Pac-Man (J1).crt',
    'Mario Bros (Ocean) (J1).crt'
  ];

  return gameNames.map(name => generateFileItem({
    name,
    path: `/games/${name}`,
    parentPath: '/games',
    type: 'Game',
    size: faker.helpers.arrayElement([64200, 80200, 88200])  // Realistic .crt sizes
  }));
}

export function generateMusicianDirectory(
  letter: string,
  artistName: string,
  seed: number
): MockFile[] {
  faker.seed(seed + letter.charCodeAt(0));  // Vary seed per artist

  const trackNames = [
    'Alpha.sid',
    'Dreams.sid',
    'Neon_Thrills.sid',
    'Proxima.sid'
  ];

  const basePath = `/music/MUSICIANS/${letter}/${artistName}`;

  return trackNames.map(name => generateFileItem({
    name,
    path: `${basePath}/${name}`,
    parentPath: basePath,
    type: 'Song',
    creator: artistName,
    size: faker.number.int({ min: 6700, max: 22900 })  // Realistic .sid sizes
  }));
}

export function generateImagesDirectory(seed: number): MockFile[] {
  faker.seed(seed);

  const imageNames = [
    'ChrisCornell1.kla',
    'Dio2.kla',
    'SonicTheHedgehog.kla'
  ];

  return imageNames.map(name => generateFileItem({
    name,
    path: `/images/${name}`,
    parentPath: '/images',
    type: 'Image',
    size: 9800  // .kla files are exactly 9.8 KB
  }));
}
```

---

#### Step 3: Create Complete Filesystem Generator

```typescript
export function createMockFilesystem(seed: number = 12345): MockFilesystem {
  const fs = new MockFilesystem(seed);

  // Add games directory (flat)
  const games = generateGamesDirectory(seed);
  fs.addDirectory('/games', {
    path: '/games',
    files: games,
    subdirectories: ['Extras', 'Large', 'MultiLoad64']
  });

  // Add music directories (nested)
  const lukHashTracks = generateMusicianDirectory('L', 'LukHash', seed);
  fs.addDirectory('/music/MUSICIANS/L/LukHash', {
    path: '/music/MUSICIANS/L/LukHash',
    files: lukHashTracks,
    subdirectories: []
  });

  // Add images directory (flat)
  const images = generateImagesDirectory(seed);
  fs.addDirectory('/images', {
    path: '/images',
    files: images,
    subdirectories: []
  });

  // Add empty favorites directories
  fs.addDirectory('/favorites/games', { path: '/favorites/games', files: [], subdirectories: [] });
  fs.addDirectory('/favorites/music', { path: '/favorites/music', files: [], subdirectories: [] });
  fs.addDirectory('/favorites/images', { path: '/favorites/images', files: [], subdirectories: [] });

  return fs;
}
```

---

#### Step 4: Unit Test Generators

**Test Cases:**
1. Determinism: Same seed produces same file names/sizes
2. File sizes match realistic ranges from STORAGE_E2E_MOCK_SAMPLE.md
3. All FileItemDto required fields populated
4. Paths are correctly formatted (Unix-style, no trailing slashes)

</details>

---

<details open>
<summary><h3>Task 3: Create Storage Fixtures</h3></summary>

**Purpose**: Provide pre-built filesystem scenarios for common test cases.

**Prerequisites**: Task 2 (generators available)

**Files to Create:**
- `src/support/test-data/fixtures/storage-favorites.fixture.ts`
- `src/support/test-data/fixtures/storage-favorites.fixture.spec.ts`

**Files to Update:**
- `src/support/test-data/fixtures/README.md` - Add "Storage Favorites Fixtures" section
- `src/support/test-data/fixtures/index.ts` - Export new fixtures

---

#### Step 1: Create Fixtures

```typescript
import { createMockFilesystem } from '../generators/storage.generators';

// Fresh filesystem, no favorites
export const emptyFilesystem = (() => {
  return createMockFilesystem(12345);
})();

// Standard structure, ready to favorite files
export const favoriteReadyDirectory = (() => {
  return createMockFilesystem(12345);
})();

// One game file already favorited
export const alreadyFavoritedDirectory = (() => {
  const fs = createMockFilesystem(12345);
  fs.saveFavorite('/games/Pac-Man (J1).crt');
  return fs;
})();

// Game, music, and image files favorited
export const mixedFavoritesDirectory = (() => {
  const fs = createMockFilesystem(12345);
  fs.saveFavorite('/games/Donkey Kong (Ocean).crt');
  fs.saveFavorite('/music/MUSICIANS/L/LukHash/Alpha.sid');
  fs.saveFavorite('/images/SonicTheHedgehog.kla');
  return fs;
})();
```

---

#### Step 2: Document Fixtures in README.md

Add new section after existing fixture documentation:

```markdown
## Storage Favorites Fixtures

Pre-built filesystem scenarios for favorite workflow testing.

### `emptyFilesystem`
Fresh filesystem with no favorited files. Use for testing initial save operations.

**Structure:**
- `/games/` - 5 .crt files
- `/music/MUSICIANS/L/LukHash/` - 4 .sid files
- `/images/` - 3 .kla files
- `/favorites/*` - Empty

### `alreadyFavoritedDirectory`
Filesystem with one game already favorited.

**Pre-favorited:**
- `/games/Pac-Man (J1).crt` → `/favorites/games/Pac-Man (J1).crt`

### `mixedFavoritesDirectory`
Filesystem with favorites across all types.

**Pre-favorited:**
- Game: `/games/Donkey Kong (Ocean).crt`
- Music: `/music/MUSICIANS/L/LukHash/Alpha.sid`
- Image: `/images/SonicTheHedgehog.kla`
```

---

#### Step 3: Export Fixtures

Update `src/support/test-data/fixtures/index.ts`:

```typescript
// Existing exports...
export * from './devices.fixture';

// New exports
export * from './storage-favorites.fixture';
```

---

#### Step 4: Unit Test Fixtures

**Test Cases:**
1. Each fixture has expected favorite state
2. `getDirectory('/favorites/games')` returns correct files
3. `parentPath` values point to original locations
4. Fixture files are readonly (prevent mutations affecting other tests)

</details>

---

<details open>
<summary><h3>Task 4: Add Storage API Constants</h3></summary>

**Purpose**: Centralize storage endpoint definitions.

**Prerequisites**: None

**Files to Update:**
- `src/support/constants/api.constants.ts`
- `src/support/constants/api.constants.spec.ts`

**Reference:** `libs/data-access/api-client/src/lib/apis/FilesApiService.ts`

---

#### Step 1: Add Storage Endpoints

**Add to `api.constants.ts` after DEVICE_ENDPOINTS:**

```typescript
// Storage endpoints
export const STORAGE_ENDPOINTS = {
  GET_DIRECTORY: {
    method: 'GET',
    path: (deviceId: string, storageType: string) =>
      `/devices/${deviceId}/storage/${storageType}/directories`,
    full: (deviceId: string, storageType: string) =>
      `${API_CONFIG.BASE_URL}/devices/${deviceId}/storage/${storageType}/directories`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/storage/*/directories*`
  },
  SAVE_FAVORITE: {
    method: 'POST',
    path: (deviceId: string, storageType: string) =>
      `/devices/${deviceId}/storage/${storageType}/favorite`,
    full: (deviceId: string, storageType: string) =>
      `${API_CONFIG.BASE_URL}/devices/${deviceId}/storage/${storageType}/favorite`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/storage/*/favorite*`
  },
  REMOVE_FAVORITE: {
    method: 'DELETE',
    path: (deviceId: string, storageType: string) =>
      `/devices/${deviceId}/storage/${storageType}/favorite`,
    full: (deviceId: string, storageType: string) =>
      `${API_CONFIG.BASE_URL}/devices/${deviceId}/storage/${storageType}/favorite`,
    pattern: `${API_CONFIG.BASE_URL}/devices/*/storage/*/favorite*`
  }
} as const;
```

---

#### Step 2: Add Intercept Aliases

**Add to `INTERCEPT_ALIASES`:**

```typescript
export const INTERCEPT_ALIASES = {
  // ... existing aliases
  FIND_DEVICES: 'findDevices',
  CONNECT_DEVICE: 'connectDevice',

  // Storage aliases
  GET_DIRECTORY: 'getDirectory',
  SAVE_FAVORITE: 'saveFavorite',
  REMOVE_FAVORITE: 'removeFavorite'
} as const;
```

---

#### Step 3: Unit Test Constants

**Add to `api.constants.spec.ts`:**

**Test Cases:**
1. Verify patterns match FilesApiService routes
2. Verify wildcards in patterns (`*` for deviceId and storageType)
3. Verify full URLs include BASE_URL
4. Verify aliases are unique strings

</details>

---

<details open>
<summary><h3>Task 5: Add UI Selector Constants</h3></summary>

**Purpose**: Centralize UI selectors for toolbar, alerts, and storage views.

**Prerequisites**: None (coordinate with UI team to confirm selectors exist)

**Files to Update:**
- `src/support/constants/selector.constants.ts`
- `src/support/constants/selector.constants.spec.ts`

---

#### Step 1: Add Player Toolbar Selectors

**Add to `selector.constants.ts`:**

```typescript
export const PLAYER_TOOLBAR_SELECTORS = {
  // ... existing toolbar selectors if any
  favoriteButton: '[data-testid="favorite-button"]',
  favoriteIcon: '[data-testid="favorite-icon"]',
  favoriteSpinner: '[data-testid="favorite-spinner"]'
} as const;
```

---

#### Step 2: Extend Alert Selectors

**Add to existing `ALERT_SELECTORS`:**

```typescript
export const ALERT_SELECTORS = {
  container: '.alert-display',
  icon: '.alert-icon',
  message: '.alert-message',
  dismissButton: '.alert-display button[aria-label="Dismiss alert"]',
  messageInContainer: '.alert-display .alert-message',
  iconInContainer: '.alert-display .alert-icon',

  // New for favorites testing
  stackContainer: '[data-testid="alert-stack-bottom-right"]',
  toast: '.alert-display.toast'
} as const;
```

---

#### Step 3: Add Storage View Selectors

**Add new selector group:**

```typescript
export const STORAGE_VIEW_SELECTORS = {
  directoryList: '[data-testid="directory-list"]',
  fileItem: '[data-testid="file-item"]',
  directoryItem: '[data-testid="directory-item"]',
  favoritesDirectory: '[data-testid="favorites-directory"]'
} as const;
```

---

#### Step 4: Coordinate with UI Team

**Before proceeding, verify these `data-testid` attributes exist on components:**
1. Player toolbar favorite button
2. Alert stack container
3. Storage directory/file list items

**If selectors don't exist:** Request UI team add them, or use alternative selectors (CSS classes, ARIA labels).

---

#### Step 5: Unit Test Selectors

**Optional - add to `selector.constants.spec.ts`:**

**Test Cases:**
1. Verify selectors are non-empty strings
2. Verify selectors use `data-testid` pattern (preferred)
3. Document any CSS-based selectors as technical debt

</details>

---

<details open>
<summary><h3>Task 6: Build Stateful Storage Interceptors</h3></summary>

**Purpose**: Simulate storage API endpoints using mock filesystem state.

**Prerequisites**: Tasks 1-4 (filesystem, generators, fixtures, constants)

**Files to Create:**
- `src/support/interceptors/storage.interceptors.ts`
- `src/support/interceptors/storage.interceptors.spec.ts`

**Files to Update:**
- `src/support/interceptors/index.ts` - Export new interceptors

**Reference Pattern:** `src/support/interceptors/device.interceptors.ts`

---

#### Step 1: Create Interceptor Functions

**`interceptGetDirectory()`:**
```typescript
import { STORAGE_ENDPOINTS, INTERCEPT_ALIASES, createProblemDetailsResponse } from '../constants/api.constants';
import type { MockFilesystem } from '../test-data/mock-filesystem/mock-filesystem';

export function interceptGetDirectory(options?: {
  filesystem?: MockFilesystem;
  errorMode?: boolean;
  responseDelayMs?: number;
}) {
  const { filesystem, errorMode = false, responseDelayMs = 0 } = options || {};

  cy.intercept(
    STORAGE_ENDPOINTS.GET_DIRECTORY.method,
    STORAGE_ENDPOINTS.GET_DIRECTORY.pattern,
    (req) => {
      if (errorMode) {
        req.reply(createProblemDetailsResponse(400, 'Failed to get directory'));
        return;
      }

      const path = (req.query.Path as string) || '/';
      const response = filesystem?.getDirectory(path) || {
        storageItem: { directories: [], files: [], path },
        message: 'Success'
      };

      req.reply({ delay: responseDelayMs, body: response });
    }
  ).as(INTERCEPT_ALIASES.GET_DIRECTORY);
}
```

**`interceptSaveFavorite()`:**
```typescript
export function interceptSaveFavorite(options?: {
  filesystem?: MockFilesystem;
  favoriteFile?: FileItemDto;
  errorMode?: boolean;
  responseDelayMs?: number;
}) {
  const { filesystem, favoriteFile, errorMode = false, responseDelayMs = 0 } = options || {};

  cy.intercept(
    STORAGE_ENDPOINTS.SAVE_FAVORITE.method,
    STORAGE_ENDPOINTS.SAVE_FAVORITE.pattern,
    (req) => {
      if (errorMode) {
        req.reply(createProblemDetailsResponse(502, 'Failed to save favorite. Please try again.'));
        return;
      }

      const filePath = req.query.FilePath as string;

      // Use filesystem state if provided, otherwise return mock response
      const response = filesystem
        ? filesystem.saveFavorite(filePath)
        : {
            message: 'Favorite tagged and saved to /favorites/games',
            favoriteFile: favoriteFile || generateFileItem(),
            favoritePath: '/favorites/games'
          };

      req.reply({ delay: responseDelayMs, body: response });
    }
  ).as(INTERCEPT_ALIASES.SAVE_FAVORITE);
}
```

**`interceptRemoveFavorite()`:**
```typescript
export function interceptRemoveFavorite(options?: {
  filesystem?: MockFilesystem;
  errorMode?: boolean;
  responseDelayMs?: number;
}) {
  const { filesystem, errorMode = false, responseDelayMs = 0 } = options || {};

  cy.intercept(
    STORAGE_ENDPOINTS.REMOVE_FAVORITE.method,
    STORAGE_ENDPOINTS.REMOVE_FAVORITE.pattern,
    (req) => {
      if (errorMode) {
        req.reply(createProblemDetailsResponse(502, 'Failed to remove favorite. Please try again.'));
        return;
      }

      const filePath = req.query.FilePath as string;

      const response = filesystem
        ? filesystem.removeFavorite(filePath)
        : { message: 'Favorite untagged and removed from /favorites/games' };

      req.reply({ delay: responseDelayMs, body: response });
    }
  ).as(INTERCEPT_ALIASES.REMOVE_FAVORITE);
}
```

---

#### Step 2: Export Interceptors

**Update `src/support/interceptors/index.ts`:**

```typescript
// Existing exports
export * from './device.interceptors';

// New exports
export * from './storage.interceptors';
```

---

#### Step 3: Unit Test Interceptors

**Create `storage.interceptors.spec.ts` (Vitest):**

**Test Cases:**
1. Verify `cy.intercept` called with correct method, pattern, alias
2. Verify `errorMode: true` returns ProblemDetails structure
3. Verify `filesystem.saveFavorite()` called with correct path
4. Verify response delay works
5. Verify response matches expected schema

**Note:** Use Vitest mocks to spy on `cy.intercept` without running Cypress.

</details>

---

<details open>
<summary><h3>Task 7: Create Storage Test Helpers</h3></summary>

**Purpose**: Provide reusable helper functions for E2E specs.

**Prerequisites**: Task 5 (selectors defined)

**Files to Create:**
- `src/e2e/storage/test-helpers.ts`

**Reference Pattern:** `src/e2e/devices/test-helpers.ts`

---

#### Step 1: Create Helper Functions

**Navigation helpers:**
```typescript
import { APP_ROUTES } from '../../support/constants/app-routes.constants';
import { PLAYER_TOOLBAR_SELECTORS, STORAGE_VIEW_SELECTORS, ALERT_SELECTORS, INTERCEPT_ALIASES } from '../../support/constants/selector.constants';

export function navigateToPlayerView(): Cypress.Chainable<Cypress.AUTWindow> {
  return cy.visit(APP_ROUTES.player, {
    onBeforeLoad: (win) => {
      win.localStorage.clear();
      win.sessionStorage.clear();
    },
  });
}

/**
 * Load a specific file into the player
 * This simulates direct file loading via URL parameters
 */
export function loadFileInPlayer(filePath: string): Cypress.Chainable<void> {
  return cy.visit(`${APP_ROUTES.player}?filePath=${encodeURIComponent(filePath)}`, {
    onBeforeLoad: (win) => {
      // Preserve device connection state but clear other session data
      const deviceState = win.sessionStorage.getItem('connectedDevice');
      win.sessionStorage.clear();
      if (deviceState) {
        win.sessionStorage.setItem('connectedDevice', deviceState);
      }
    },
  });
}

export function openFavoritesDirectory(type: 'games' | 'music' | 'images'): void {
  // Navigate to /favorites/{type}
  cy.get(STORAGE_VIEW_SELECTORS.directoryList).within(() => {
    cy.contains('favorites').click();
  });
  cy.get(STORAGE_VIEW_SELECTORS.directoryList).within(() => {
    cy.contains(type).click();
  });
}
```

**Action helpers:**
```typescript
export function clickFavoriteButton(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.favoriteButton).click();
}
```

**Assertion helpers:**
```typescript
export function expectFavoriteIcon(expected: 'favorite' | 'favorite_border'): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.favoriteIcon).should('have.text', expected);
}

export function verifyAlert(message: string, severity: 'success' | 'error'): void {
  cy.get(ALERT_SELECTORS.container)
    .should('be.visible')
    .within(() => {
      cy.get(ALERT_SELECTORS.message).should('contain.text', message);
    });
}

export function verifyFileInDirectory(fileName: string, shouldExist: boolean): void {
  if (shouldExist) {
    cy.get(STORAGE_VIEW_SELECTORS.fileItem).should('contain.text', fileName);
  } else {
    cy.get(STORAGE_VIEW_SELECTORS.fileItem).should('not.contain.text', fileName);
  }
}
```

**Wait helpers:**
```typescript
export function waitForSaveFavorite(): void {
  cy.wait(`@${INTERCEPT_ALIASES.SAVE_FAVORITE}`);
}

export function waitForRemoveFavorite(): void {
  cy.wait(`@${INTERCEPT_ALIASES.REMOVE_FAVORITE}`);
}
```

---

#### Step 2: Re-export Constants (Optional, for convenience)

```typescript
// Re-export commonly used constants
export {
  PLAYER_TOOLBAR_SELECTORS,
  STORAGE_VIEW_SELECTORS,
  ALERT_SELECTORS
} from '../../support/constants/selector.constants';

export {
  INTERCEPT_ALIASES
} from '../../support/constants/api.constants';
```

</details>

---

<details open>
<summary><h3>Task 8: Write Favorite Workflow E2E Spec</h3></summary>

**Purpose**: Test favorite workflows end-to-end using mock filesystem.

**Prerequisites**: All previous tasks

**Files to Create:**
- `src/e2e/storage/favorite-operations.cy.ts`

**Reference:** FAVORITE_PLAN.md scenarios 1-7, 13-18

---

#### Step 1: Create Spec Scaffold

```typescript
import { createMockFilesystem } from '../../support/test-data/generators/storage.generators';
import { singleDevice } from '../../support/test-data/fixtures';
import {
  interceptFindDevices,
  interceptConnectDevice,
  interceptGetDirectory,
  interceptSaveFavorite,
  interceptRemoveFavorite
} from '../../support/interceptors';
import {
  navigateToPlayerView,
  loadFileInPlayer,
  clickFavoriteButton,
  expectFavoriteIcon,
  waitForSaveFavorite,
  waitForRemoveFavorite,
  verifyAlert,
  openFavoritesDirectory,
  verifyFileInDirectory
} from './test-helpers';
import type { MockFilesystem } from '../../support/test-data/mock-filesystem/mock-filesystem';

describe('Favorite Operations', () => {
  let filesystem: MockFilesystem;

  beforeEach(() => {
    // Create fresh filesystem for each test
    filesystem = createMockFilesystem(12345);

    // Register device interceptors
    interceptFindDevices({ fixture: singleDevice });
    interceptConnectDevice();

    // Register storage interceptors with shared filesystem
    interceptGetDirectory({ filesystem });
    interceptSaveFavorite({ filesystem });
    interceptRemoveFavorite({ filesystem });

    // Navigate to player view with initial file loaded
    loadFileInPlayer('/games/Pac-Man (J1).crt');
    cy.wait('@getDirectory');
  });

  // Tests here...
});
```

---

#### Step 2: Implement Test Cases

**Test 1 - Save favorite success (Scenarios 1 & 5):**
```typescript
it('should save favorite and display in favorites directory', () => {
  // Given: File is not favorited
  expectFavoriteIcon('favorite_border');

  // When: User clicks favorite button
  clickFavoriteButton();
  waitForSaveFavorite();

  // Then: Icon updates
  expectFavoriteIcon('favorite');

  // And: Success alert appears
  verifyAlert('Favorite tagged and saved to /favorites/games', 'success');

  // And: File appears in favorites directory
  openFavoritesDirectory('games');
  verifyFileInDirectory('Pac-Man (J1).crt', true);
});
```

**Test 2 - Save favorite failure recovery (Scenario 2):**
```typescript
it('should handle save favorite failure and retry', () => {
  // First attempt fails
  interceptSaveFavorite({ filesystem, errorMode: true });

  clickFavoriteButton();
  waitForSaveFavorite();

  expectFavoriteIcon('favorite_border');
  verifyAlert('Failed to save favorite. Please try again.', 'error');

  // Retry with success
  interceptSaveFavorite({ filesystem });
  clickFavoriteButton();
  waitForSaveFavorite();

  expectFavoriteIcon('favorite');
  verifyAlert('Favorite tagged and saved to /favorites/games', 'success');
});
```

**Test 3 - Button disabled during operation (Scenario 3 & 13):**
```typescript
it('should disable button during save operation', () => {
  interceptSaveFavorite({ filesystem, responseDelayMs: 1000 });

  clickFavoriteButton();

  // Button should be disabled
  cy.get(PLAYER_TOOLBAR_SELECTORS.favoriteButton).should('be.disabled');

  waitForSaveFavorite();

  // Button re-enabled
  cy.get(PLAYER_TOOLBAR_SELECTORS.favoriteButton).should('not.be.disabled');
  expectFavoriteIcon('favorite');
});
```

**Test 4 - Favorites directory synchronization (Scenarios 4 & 7):**
```typescript
it('should synchronize favorites directory when removing favorite', () => {
  // Pre-favorite a file
  filesystem.saveFavorite('/games/Pac-Man (J1).crt');

  expectFavoriteIcon('favorite');

  // Open favorites directory and verify file exists
  openFavoritesDirectory('games');
  verifyFileInDirectory('Pac-Man (J1).crt', true);

  // Remove favorite
  clickFavoriteButton();
  waitForRemoveFavorite();

  expectFavoriteIcon('favorite_border');
  verifyAlert('Favorite untagged and removed from /favorites/games', 'success');

  // File disappears from favorites
  verifyFileInDirectory('Pac-Man (J1).crt', false);
});
```



**Test 6 - Multi-type favorites (Scenario 14):**
```typescript
it('should favorite files across multiple types', () => {
  // Favorite game
  loadFileInPlayer('/games/Pac-Man (J1).crt');
  cy.wait('@getDirectory');
  expectFavoriteIcon('favorite_border');

  clickFavoriteButton();
  waitForSaveFavorite();

  expectFavoriteIcon('favorite');
  verifyAlert('Favorite tagged and saved to /favorites/games', 'success');

  // Favorite music
  loadFileInPlayer('/music/MUSICIANS/L/LukHash/Alpha.sid');
  cy.wait('@getDirectory');
  expectFavoriteIcon('favorite_border');

  clickFavoriteButton();
  waitForSaveFavorite();

  expectFavoriteIcon('favorite');
  verifyAlert('Favorite tagged and saved to /favorites/music', 'success');

  // Favorite image
  loadFileInPlayer('/images/SonicTheHedgehog.kla');
  cy.wait('@getDirectory');
  expectFavoriteIcon('favorite_border');

  clickFavoriteButton();
  waitForSaveFavorite();

  expectFavoriteIcon('favorite');
  verifyAlert('Favorite tagged and saved to /favorites/images', 'success');

  // Verify each in correct directory
  openFavoritesDirectory('games');
  verifyFileInDirectory('Pac-Man (J1).crt', true);

  openFavoritesDirectory('music');
  verifyFileInDirectory('Alpha.sid', true);

  openFavoritesDirectory('images');
  verifyFileInDirectory('SonicTheHedgehog.kla', true);
});
```

---

#### Step 3: Run Tests

```bash
pnpm nx e2e teensyrom-ui-e2e --spec="src/e2e/storage/favorite-operations.cy.ts"
```

**Verify:**
1. All tests pass consistently
2. No flaky failures
3. Tests run offline (no network calls)
4. Execution time is reasonable (<30 seconds total)

</details>

---

## ✅ Success Criteria

**Mock Filesystem:**
- ✅ Generates realistic structure matching STORAGE_E2E_MOCK_SAMPLE.md
- ✅ Same seed produces identical files every time (deterministic)
- ✅ State mutations work correctly (save/remove favorites)
- ✅ Resettable per test (no cross-test pollution)

**API Simulation:**
- ✅ Response messages match backend exactly
- ✅ Interceptors call filesystem methods and mutate state
- ✅ Error modes return ProblemDetails structure

**Test Coverage:**
- ✅ All scenarios from FAVORITE_PLAN.md covered (1-7, 13-18)
- ✅ Icon state updates verified
- ✅ Alert messages verified
- ✅ Directory synchronization verified
- ✅ Button disabled states verified

**Code Quality:**
- ✅ All constants centralized (no hardcoded values)
- ✅ All tests use helpers (no raw Cypress commands)
- ✅ Unit tests for filesystem, generators, interceptors
- ✅ Documentation updated (README.md)

**Execution:**
- ✅ `pnpm nx e2e teensyrom-ui-e2e` passes without flakes
- ✅ Tests run offline (interceptors prevent network calls)
- ✅ Fast execution (<30 seconds)

---

## 📝 Implementation Notes

### Filesystem State Pattern

**Key concept:** Single shared `MockFilesystem` instance per test that mutates as interceptors execute.

```typescript
beforeEach(() => {
  filesystem = createMockFilesystem(12345);  // Fresh instance

  // Pass reference to interceptors
  interceptSaveFavorite({ filesystem });
  interceptRemoveFavorite({ filesystem });
});

// During test:
clickFavoriteButton();  // → Triggers interceptor
// → Interceptor calls filesystem.saveFavorite()
// → Filesystem state mutates
// → Next getDirectory() call sees changes
```

**This pattern simulates real backend behavior** where state persists across API calls.

---

### Response Message Formats

**Match backend exactly** (see `FavoriteFileEndpoint.cs`, `RemoveFavoriteEndpoint.cs`):

| Operation | Success Message | Error Message |
|-----------|----------------|---------------|
| Save | `Favorite tagged and saved to /favorites/{type}` | `Failed to save favorite. Please try again.` |
| Remove | `Favorite untagged and removed from /favorites/{type}` | `Failed to remove favorite. Please try again.` |

**Dynamic path in message:**
- Games: `/favorites/games`
- Music: `/favorites/music`
- Images: `/favorites/images`

---

### Favorites Directory Structure

**Always flat, never nested:**

✅ **Correct:**
```
/favorites/games/Pac-Man (J1).crt
/favorites/music/Alpha.sid
```

❌ **Wrong:**
```
/favorites/music/MUSICIANS/L/LukHash/Alpha.sid
```

**`parentPath` tracks original location:**
```typescript
{
  path: '/favorites/music/Alpha.sid',
  parentPath: '/music/MUSICIANS/L/LukHash',
  isFavorite: true
}
```

---

### Test Context Setup

**Each test needs:**
1. Device context (deviceId from `singleDevice` fixture)
2. Storage context (storageType: 'SD' or 'USB')
3. Player context (file loaded in player)

**The interceptors use wildcards** to match any deviceId/storageType:
```typescript
pattern: `${API_CONFIG.BASE_URL}/devices/*/storage/*/favorite*`
```

**Actual requests** will have real values extracted from UI state.

---

## 🔗 Related Documentation

- [Favorite Feature Plan](./FAVORITE_PLAN.md) - Overall feature requirements
- [Storage Mock Sample](../storage/STORAGE_E2E_MOCK_SAMPLE.md) - Real file structure
- [E2E Test Guide](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md) - Cypress patterns
- [E2E Constants](../../../apps/teensyrom-ui-e2e/src/support/constants/E2E_CONSTANTS.md)
- [E2E Fixtures](../../../apps/teensyrom-ui-e2e/src/support/test-data/fixtures/E2E_FIXTURES.md)
- [E2E Interceptors](../../../apps/teensyrom-ui-e2e/src/support/interceptors/E2E_INTERCEPTORS.md)
- [Device Discovery Tests](../../../apps/teensyrom-ui-e2e/src/e2e/devices/E2E_DEVICE_DISCOVERY.md)

---

## 📋 Open Questions

**Before starting implementation, verify:**

1. ❓ **Alert stack selector** - Does `[data-testid="alert-stack-bottom-right"]` exist on alert component?
   - If not, coordinate with UI team to add it
   - Alternative: Use CSS selector for `.alert-display` parent container

2. ❓ **Favorite button selectors** - Confirm player toolbar has:
   - `[data-testid="favorite-button"]`
   - `[data-testid="favorite-icon"]`
   - If not, request UI team add them

3. ❓ **Storage view selectors** - Confirm directory browser has:
   - `[data-testid="directory-list"]`
   - `[data-testid="file-item"]`
   - If not, use alternative CSS selectors

4. ❓ **Alert auto-dismiss timing** - Confirm expected behavior:
   - Current assumption: 3 seconds
   - May need to adjust test waits or use custom interceptor delays
