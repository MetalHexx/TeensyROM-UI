/// <reference types="cypress" />

/**
 * Player domain interceptors - Barrel export file
 *
 * This file provides backward compatibility by re-exporting all player domain interceptors.
 * The actual interceptor implementations have been moved to dedicated files following
 * the 6-section architecture pattern.
 *
 * Migrated to dedicated interceptor files:
 * - launchFile.interceptors.ts
 * - launchRandom.interceptors.ts
 */

// ============================================================================
// Re-exports from dedicated interceptor files
// ============================================================================

// Launch File Interceptor
export {
  interceptLaunchFile,
  waitForLaunchFile,
  setupLaunchFile,
  setupLaunchFileWithFile,
  setupLaunchFileWithPath,
  setupErrorLaunchFile,
  setupDelayedLaunchFile,
  verifyLaunchFileRequested,
  getLastLaunchFileRequest,
  setupLaunchFileSequence,
  setupIncompatibleLaunchFile,
  LAUNCH_FILE_ENDPOINT,
  type InterceptLaunchFileOptions,
  INTERCEPT_LAUNCH_FILE,
  LAUNCH_FILE_METHOD,
} from './launchFile.interceptors';

// Launch Random Interceptor
export {
  interceptLaunchRandom,
  waitForLaunchRandom,
  setupLaunchRandom,
  setupLaunchRandomWithFile,
  setupLaunchRandomWithPool,
  setupLaunchRandomFromDirectory,
  setupErrorLaunchRandom,
  setupDelayedLaunchRandom,
  verifyLaunchRandomRequested,
  getLastLaunchRandomRequest,
  setupLaunchRandomSequence,
  setupIncompatibleLaunchRandom,
  setupUniqueLaunchRandom,
  LAUNCH_RANDOM_ENDPOINT,
  type InterceptLaunchRandomOptions,
  INTERCEPT_LAUNCH_RANDOM,
  LAUNCH_RANDOM_METHOD,
} from './launchRandom.interceptors';

// ============================================================================
// Legacy Exports (Backward Compatibility)
// ============================================================================

// Legacy exports for backward compatibility (re-exported from dedicated files)
export { LAUNCH_FILE_ALIAS } from './launchFile.interceptors';
export { LAUNCH_RANDOM_ALIAS } from './launchRandom.interceptors';
