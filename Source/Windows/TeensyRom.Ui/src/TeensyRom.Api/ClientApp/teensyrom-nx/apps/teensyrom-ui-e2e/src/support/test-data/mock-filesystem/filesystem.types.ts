/**
 * Mock Filesystem Type Definitions
 *
 * Type-safe definitions for the in-memory mock filesystem used in E2E tests.
 * These types align exactly with the API client models to ensure compatibility.
 *
 * @see FAVORITE_PLAN_P6.md for detailed implementation guide
 */

import type {
  FileItemDto,
  FileItemType,
  DirectoryItemDto,
  StorageCacheDto,
  GetDirectoryResponse,
  SaveFavoriteResponse,
  RemoveFavoriteResponse,
} from '@teensyrom-nx/data-access/api-client';

/**
 * Mock file that matches the FileItemDto structure exactly.
 * All fields from FileItemDto are used in our mock filesystem.
 *
 * This type alias ensures our mock data stays synchronized with the
 * actual API contract, preventing drift between tests and reality.
 */
export type MockFile = FileItemDto;

/**
 * Mock directory containing files and subdirectories.
 *
 * Represents a single directory in the mock filesystem with:
 * - path: Absolute path to this directory (e.g., "/games")
 * - files: Array of files contained in this directory
 * - subdirectories: Array of subdirectory names (not full paths)
 *
 * @example
 * ```typescript
 * const gamesDir: MockDirectory = {
 *   path: '/games',
 *   files: [pacManFile, donkeyKongFile],
 *   subdirectories: ['Extras', 'Large']
 * };
 * ```
 */
export interface MockDirectory {
  /** Absolute path to this directory (e.g., "/games", "/music/MUSICIANS/L/LukHash") */
  path: string;

  /** Files contained in this directory */
  files: MockFile[];

  /** Names of subdirectories (not full paths) */
  subdirectories: string[];
}

/**
 * Re-export FileItemType for convenience in test files.
 * Prevents need to import from api-client directly.
 */
export type { FileItemType };

/**
 * Re-export response types for use in interceptors and tests.
 */
export type {
  GetDirectoryResponse,
  SaveFavoriteResponse,
  RemoveFavoriteResponse,
  DirectoryItemDto,
  StorageCacheDto,
};
