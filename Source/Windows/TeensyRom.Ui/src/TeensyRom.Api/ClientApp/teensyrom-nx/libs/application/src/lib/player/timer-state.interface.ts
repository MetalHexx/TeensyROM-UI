/**
 * Timer state interface for player timing functionality.
 *
 * Application-layer interface (not domain) - timer is an implementation detail
 * of player orchestration, not a domain concept.
 *
 * Phase 5 Scope:
 * - Only used for FileItemType.Song with parsed playLength metadata
 * - speed always 1.0 (reserved for future phases)
 * - showProgress always true for music files
 *
 * Future Extensions:
 * - Custom timer durations for Games/Images
 * - Music timer override (replace metadata duration)
 * - Speed control integration (speed multiplier affects timer progression)
 */
export interface TimerState {
  /**
   * Total duration in milliseconds.
   * Parsed from FileItem.playLength metadata for music files.
   */
  totalTime: number;

  /**
   * Current position in milliseconds.
   * Updated at 100ms intervals by TimerService.
   */
  currentTime: number;

  /**
   * Timer actively counting.
   * true when timer is running and incrementing currentTime.
   */
  isRunning: boolean;

  /**
   * Timer paused (music only).
   * true when timer is paused, preserving currentTime.
   * Paused timers can be resumed from their current position.
   */
  isPaused: boolean;

  /**
   * Speed multiplier (1.0 = normal).
   * Phase 5: Always 1.0 (reserved for future phases).
   * Future: Will affect timer progression rate.
   */
  speed: number;

  /**
   * Display progress bar in UI.
   * Phase 5: Always true for music files.
   * Future: Will respect custom timer settings, overrides, user preferences.
   */
  showProgress: boolean;
}
