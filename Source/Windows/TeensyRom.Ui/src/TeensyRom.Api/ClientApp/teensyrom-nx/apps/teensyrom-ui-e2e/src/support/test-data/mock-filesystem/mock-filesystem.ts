/**
 * Stateful mock filesystem used by Cypress interceptors.
 *
 * Mirrors backend DTO shapes while remaining deterministic and resettable so
 * tests can mutate state without leaking between cases.
 */
import type {
  DirectoryItemDto,
  GetDirectoryResponse,
  RemoveFavoriteResponse,
  SaveFavoriteResponse,
} from '@teensyrom-nx/data-access/api-client';
import type { FileItemType, MockDirectory, MockFile } from './filesystem.types';

export class MockFilesystem {
  private directories: Map<string, MockDirectory>;
  private initialDirectories: Map<string, MockDirectory>;
  private readonly seed: number;

  constructor(seed = 12345) {
    this.seed = seed;
    this.directories = new Map();
    this.initialDirectories = new Map();
    this.generate();
  }

  /**
   * Registers a directory with the filesystem.
   *
   * The initial snapshot is cloned so reset() can restore the original state.
   */
  addDirectory(path: string, directory: MockDirectory): void {
    const resolvedPath = this.normalizePath(path || directory.path);
    const snapshot = this.cloneDirectory({
      ...directory,
      path: resolvedPath,
    });

    this.directories.set(resolvedPath, this.cloneDirectory(snapshot));
    this.initialDirectories.set(resolvedPath, snapshot);
  }

  /**
   * Returns directory contents shaped like GetDirectoryResponse.
   */
  getDirectory(path: string): GetDirectoryResponse {
    const resolvedPath = this.normalizePath(path);
    const directory = this.directories.get(resolvedPath);

    if (!directory) {
      return {
        storageItem: {
          path: resolvedPath,
          directories: [],
          files: [],
        },
        message: 'Success',
      };
    }

    const directoryItems: DirectoryItemDto[] = directory.subdirectories.map((name) => ({
      name,
      path: this.joinPath(resolvedPath, name),
    }));

    return {
      storageItem: {
        path: directory.path,
        directories: directoryItems,
        files: directory.files.map((file) => this.cloneFile(file)),
      },
      message: 'Success',
    };
  }

  /**
   * Marks a file as favorite and creates a flat copy in the appropriate directory.
   */
  saveFavorite(filePath: string): SaveFavoriteResponse {
    const resolvedPath = this.normalizePath(filePath);
    const originalFile = this.findFile(resolvedPath);

    if (!originalFile) {
      throw new Error(`File not found: ${filePath}`);
    }

    originalFile.isFavorite = true;

    const favoritesPath = this.getFavoritePathForType(originalFile.type);
    const favoritesDirectory = this.ensureRuntimeDirectory(favoritesPath);
    const favoriteCopy: MockFile = {
      ...this.cloneFile(originalFile),
      path: `${favoritesPath}/${originalFile.name}`,
      isFavorite: true,
    };

    favoritesDirectory.files = favoritesDirectory.files.filter(
      (file) => file.name !== favoriteCopy.name
    );
    favoritesDirectory.files.push(favoriteCopy);

    return {
      message: `Favorite tagged and saved to ${favoritesPath}`,
      favoriteFile: this.cloneFile(favoriteCopy),
      favoritePath: favoritesPath,
    };
  }

  /**
   * Removes a favorite and clears isFavorite flag on the original file.
   */
  removeFavorite(filePath: string): RemoveFavoriteResponse {
    const resolvedPath = this.normalizePath(filePath);
    let originalFile: MockFile | null = null;
    let favoritesPath = '';

    if (resolvedPath.startsWith('/favorites/')) {
      const favoriteName = resolvedPath.split('/').pop();
      if (!favoriteName) {
        throw new Error(`Invalid favorite path: ${filePath}`);
      }

      const itemType = this.getTypeFromFavoritesPath(resolvedPath);
      favoritesPath = this.getFavoritePathForType(itemType);
      const favoritesDirectory = this.directories.get(favoritesPath);
      const favoriteCopy = favoritesDirectory?.files.find((file) => file.name === favoriteName);

      if (favoriteCopy) {
        const sourceDirectory = this.directories.get(favoriteCopy.parentPath);
        originalFile =
          sourceDirectory?.files.find((file) => file.name === favoriteCopy.name) ?? null;
      }
    } else {
      originalFile = this.findFile(resolvedPath);
      if (originalFile) {
        favoritesPath = this.getFavoritePathForType(originalFile.type);
      }
    }

    if (!originalFile) {
      throw new Error(`File not found: ${filePath}`);
    }

    originalFile.isFavorite = false;
    const favoritesDirectory = this.directories.get(favoritesPath);
    if (favoritesDirectory) {
      favoritesDirectory.files = favoritesDirectory.files.filter(
        (file) => file.name !== originalFile.name
      );
    }

    return {
      message: `Favorite untagged and removed from ${favoritesPath}`,
    };
  }

  /**
   * Restores the filesystem to its initial seeded state.
   */
  reset(): void {
    this.generate();
  }

  /**
   * Rehydrates the runtime directories map using the captured initial snapshot.
   */
  private generate(): void {
    this.directories = new Map();
    for (const [path, directory] of this.initialDirectories.entries()) {
      this.directories.set(path, this.cloneDirectory(directory));
    }
  }

  /**
   * Ensures a directory exists in the live map (used for favorites folders).
   */
  private ensureRuntimeDirectory(path: string): MockDirectory {
    const resolvedPath = this.normalizePath(path);
    const existing = this.directories.get(resolvedPath);

    if (existing) {
      return existing;
    }

    const created: MockDirectory = {
      path: resolvedPath,
      files: [],
      subdirectories: [],
    };

    this.directories.set(resolvedPath, created);
    if (!this.initialDirectories.has(resolvedPath)) {
      this.initialDirectories.set(resolvedPath, this.cloneDirectory(created));
    }

    return created;
  }

  /**
   * Finds a file by absolute path across all directories.
   */
  private findFile(path: string): MockFile | null {
    for (const directory of this.directories.values()) {
      const match = directory.files.find((file) => this.normalizePath(file.path) === path);
      if (match) {
        return match;
      }
    }

    return null;
  }

  private getFavoritePathForType(type: FileItemType): string {
    switch (type) {
      case 'Song':
        return '/favorites/music';
      case 'Image':
        return '/favorites/images';
      case 'Game':
      case 'Hex':
      case 'Unknown':
      default:
        return '/favorites/games';
    }
  }

  private getTypeFromFavoritesPath(path: string): FileItemType {
    if (path.includes('/favorites/music')) {
      return 'Song';
    }
    if (path.includes('/favorites/images')) {
      return 'Image';
    }
    return 'Game';
  }

  private cloneDirectory(directory: MockDirectory): MockDirectory {
    return {
      path: directory.path,
      subdirectories: [...directory.subdirectories],
      files: directory.files.map((file) => this.cloneFile(file)),
    };
  }

  private cloneFile(file: MockFile): MockFile {
    return JSON.parse(JSON.stringify(file)) as MockFile;
  }

  private normalizePath(path: string): string {
    if (!path) {
      return '/';
    }

    const standardized = path.replace(/\\/g, '/');
    if (standardized.length > 1 && standardized.endsWith('/')) {
      return standardized.slice(0, -1);
    }

    return standardized;
  }

  private joinPath(base: string, segment: string): string {
    if (!base || base === '/') {
      return `/${segment}`;
    }

    return `${base}/${segment}`;
  }
}

export default MockFilesystem;
