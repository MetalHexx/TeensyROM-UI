import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, Subject, Subscription, interval } from 'rxjs';

/**
 * Core RxJS timer implementation for individual timer instances.
 *
 * Provides 100ms tick precision with pause/resume capabilities.
 * Single timer instance per service - use PlayerTimerManager for multi-device coordination.
 *
 * Phase 5 Scope:
 * - Speed always 1.0 (setSpeed is no-op)
 * - Music files only (FileItemType.Song)
 * - Basic start/pause/resume/stop lifecycle
 *
 * Future Extensions:
 * - Variable speed control (setSpeed implementation)
 * - Custom timer durations for games/images
 * - Advanced timing features (nudge, seek, etc.)
 */
@Injectable()
export class TimerService implements OnDestroy {
  private readonly TICK_INTERVAL_MS = 100;

  private _totalTime = 0;
  private _currentTime = 0;
  private _isRunning = false;
  private _isPaused = false;
  private _speed = 1.0;

  private currentTimeSubject = new BehaviorSubject<number>(0);
  private completionSubject = new Subject<void>();
  private intervalSubscription: Subscription | null = null;

  /**
   * Observable stream of current time updates.
   * Emits currentTime in milliseconds at 100ms intervals when timer is running.
   */
  readonly currentTime$: Observable<number> = this.currentTimeSubject.asObservable();

  /**
   * Observable stream of completion events.
   * Emits when currentTime >= totalTime.
   */
  readonly completion$: Observable<void> = this.completionSubject.asObservable();

  /**
   * Current time position in milliseconds.
   */
  get currentTime(): number {
    return this._currentTime;
  }

  /**
   * Total duration in milliseconds.
   */
  get totalTime(): number {
    return this._totalTime;
  }

  /**
   * Timer is actively counting.
   */
  get isRunning(): boolean {
    return this._isRunning;
  }

  /**
   * Timer is paused (maintains currentTime).
   */
  get isPaused(): boolean {
    return this._isPaused;
  }

  /**
   * Current speed multiplier (Phase 5: always 1.0).
   */
  get speed(): number {
    return this._speed;
  }

  /**
   * Start timer with specified total duration.
   *
   * @param totalTime - Total duration in milliseconds
   */
  start(totalTime: number): void {
    // Stop any existing timer
    this.stop();

    // Set total time
    this._totalTime = totalTime;
    this._currentTime = 0;
    this._isRunning = true;
    this._isPaused = false;

    // Emit initial state
    this.currentTimeSubject.next(this._currentTime);

    // Start interval
    this.startInterval();
  }

  /**
   * Pause timer (maintains currentTime).
   */
  pause(): void {
    if (!this._isRunning || this._isPaused) {
      return;
    }

    this._isRunning = false;
    this._isPaused = true;

    // Stop interval
    this.stopInterval();

    // Emit current state with updated pause status
    this.currentTimeSubject.next(this._currentTime);
  }

  /**
   * Resume timer from paused state.
   */
  resume(): void {
    if (!this._isPaused) {
      return;
    }

    this._isRunning = true;
    this._isPaused = false;

    // Emit current state with updated resume status
    this.currentTimeSubject.next(this._currentTime);

    // Restart interval
    this.startInterval();
  }

  /**
   * Stop timer and reset to 0.
   */
  stop(): void {
    this._isRunning = false;
    this._isPaused = false;
    this._currentTime = 0;
    this._totalTime = 0;

    // Stop interval
    this.stopInterval();

    // Emit reset state
    this.currentTimeSubject.next(this._currentTime);
  }

  /**
   * Reset currentTime to 0 (keeps timer running if it was running).
   */
  reset(): void {
    this._currentTime = 0;
    this.currentTimeSubject.next(this._currentTime);
  }

  /**
   * Set playback speed multiplier.
   *
   * Phase 5: No-op (speed always 1.0).
   * Future: Will affect timer progression rate.
   *
   * @param speed - Speed multiplier (1.0 = normal)
   */
  setSpeed(speed: number): void {
    // Phase 5: No-op - speed control reserved for future phases
    // Store the value for future implementation
    this._speed = speed;
  }

  /**
   * Cleanup on service destruction.
   */
  ngOnDestroy(): void {
    this.stop();
    this.currentTimeSubject.complete();
    this.completionSubject.complete();
  }

  /**
   * Start the RxJS interval for timer ticks.
   */
  private startInterval(): void {
    // Clear any existing interval
    this.stopInterval();

    // Create new interval
    this.intervalSubscription = interval(this.TICK_INTERVAL_MS).subscribe(() => {
      // Increment currentTime by tick interval
      this._currentTime += this.TICK_INTERVAL_MS;

      // Emit updated time
      this.currentTimeSubject.next(this._currentTime);

      // Check for completion
      if (this._currentTime >= this._totalTime) {
        this.handleCompletion();
      }
    });
  }

  /**
   * Stop the RxJS interval.
   */
  private stopInterval(): void {
    if (this.intervalSubscription) {
      this.intervalSubscription.unsubscribe();
      this.intervalSubscription = null;
    }
  }

  /**
   * Handle timer completion.
   */
  private handleCompletion(): void {
    // Stop the timer
    this._isRunning = false;

    // Stop interval
    this.stopInterval();

    // Emit completion event
    this.completionSubject.next();
  }
}
