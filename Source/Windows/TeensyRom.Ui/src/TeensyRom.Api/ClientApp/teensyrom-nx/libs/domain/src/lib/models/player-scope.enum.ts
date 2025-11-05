/**
 * Scope options for player random file selection
 * Determines the range from which random files are selected
 */
export enum PlayerScope {
  /** All storage across all devices (TeensyROM Global) */
  Storage = 'STORAGE',

  /** Current directory and all subdirectories (Directory Deep) */
  DirectoryDeep = 'DIRECTORY_DEEP',

  /** Current directory only, no subdirectories (Directory Shallow) */
  DirectoryShallow = 'DIRECTORY_SHALLOW',
}
