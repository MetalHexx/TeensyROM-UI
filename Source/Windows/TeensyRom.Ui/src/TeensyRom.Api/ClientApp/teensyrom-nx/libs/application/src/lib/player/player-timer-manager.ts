import { Injectable } from '@angular/core';
import { Observable, Subject, Subscription } from 'rxjs';
import { TimerService } from './timer.service';
import { TimerState } from './timer-state.interface';
import { LogType, logInfo } from '@teensyrom-nx/utils';

/**
 * Multi-device timer coordination manager.
 *
 * Manages multiple TimerService instances keyed by deviceId and coordinates
 * timer events with PlayerContextService.
 *
 * Phase 5 Scope:
 * - Device-scoped timer instances
 * - Timer lifecycle coordination (create, destroy, pause, resume, stop)
 * - Speed control API (no-op for Phase 5)
 * - Observable streams for timer updates and completion events
 *
 * Future Extensions:
 * - Speed control implementation (affects timer progression)
 * - Timer synchronization across devices
 * - Advanced timer features (nudge, seek, custom durations)
 */
@Injectable({
  providedIn: 'root',
})
export class PlayerTimerManager {
  private timers = new Map<string, TimerService>();
  private timerSubscriptions = new Map<string, Subscription[]>();
  private timerUpdateSubjects = new Map<string, Subject<TimerState>>();
  private timerCompleteSubjects = new Map<string, Subject<void>>();

  /**
   * Create and start timer for device.
   *
   * @param deviceId - Device identifier
   * @param totalTime - Total duration in milliseconds
   */
  createTimer(deviceId: string, totalTime: number): void {
    logInfo(LogType.Start, `Creating timer for device ${deviceId} with duration ${totalTime}ms`);

    // Destroy existing timer if any
    this.destroyTimer(deviceId);

    // Create new timer service
    const timer = new TimerService();
    this.timers.set(deviceId, timer);

    // Create or reuse subjects for this device
    let updateSubject = this.timerUpdateSubjects.get(deviceId);
    if (!updateSubject) {
      updateSubject = new Subject<TimerState>();
      this.timerUpdateSubjects.set(deviceId, updateSubject);
    }

    let completeSubject = this.timerCompleteSubjects.get(deviceId);
    if (!completeSubject) {
      completeSubject = new Subject<void>();
      this.timerCompleteSubjects.set(deviceId, completeSubject);
    }

    // Start the timer FIRST (before subscribing)
    // This ensures timer.totalTime is set before any emissions
    timer.start(totalTime);

    // Subscribe to timer events
    const subscriptions: Subscription[] = [];

    // Subscribe to currentTime updates
    const timeSubscription = timer.currentTime$.subscribe((currentTime) => {
      const timerState: TimerState = {
        totalTime: timer.totalTime,
        currentTime,
        isRunning: timer.isRunning,
        isPaused: timer.isPaused,
        speed: timer.speed,
        showProgress: true, // Phase 5: Always true for music files
      };
      updateSubject.next(timerState);
    });
    subscriptions.push(timeSubscription);

    // Subscribe to completion events
    const completionSubscription = timer.completion$.subscribe(() => {
      logInfo(LogType.Success, `Timer completed for device ${deviceId}`);
      completeSubject.next();
    });
    subscriptions.push(completionSubscription);

    // Store subscriptions
    this.timerSubscriptions.set(deviceId, subscriptions);

    logInfo(LogType.Success, `Timer created and started for device ${deviceId}`);
  }

  /**
   * Destroy timer for device.
   *
   * Stops timer and cleans up all subscriptions.
   * Note: Does NOT complete or remove subjects - they are reused for future timers.
   *
   * @param deviceId - Device identifier
   */
  destroyTimer(deviceId: string): void {
    const timer = this.timers.get(deviceId);
    if (!timer) {
      return;
    }

    logInfo(LogType.Start, `Destroying timer for device ${deviceId}`);

    // Stop timer
    timer.stop();
    timer.ngOnDestroy();

    // Cleanup subscriptions
    const subscriptions = this.timerSubscriptions.get(deviceId);
    if (subscriptions) {
      subscriptions.forEach((sub) => sub.unsubscribe());
      this.timerSubscriptions.delete(deviceId);
    }

    // Remove timer (but keep subjects for reuse)
    this.timers.delete(deviceId);

    logInfo(LogType.Success, `Timer destroyed for device ${deviceId}`);
  }

  /**
   * Pause timer for device.
   *
   * @param deviceId - Device identifier
   */
  pauseTimer(deviceId: string): void {
    const timer = this.timers.get(deviceId);
    if (!timer) {
      return;
    }

    logInfo(LogType.Info, `Pausing timer for device ${deviceId}`);
    timer.pause();
  }

  /**
   * Resume timer for device.
   *
   * @param deviceId - Device identifier
   */
  resumeTimer(deviceId: string): void {
    const timer = this.timers.get(deviceId);
    if (!timer) {
      return;
    }

    logInfo(LogType.Info, `Resuming timer for device ${deviceId}`);
    timer.resume();
  }

  /**
   * Stop timer for device.
   *
   * Resets timer to 0 but preserves timer instance for UI display.
   *
   * @param deviceId - Device identifier
   */
  stopTimer(deviceId: string): void {
    const timer = this.timers.get(deviceId);
    if (!timer) {
      return;
    }

    logInfo(LogType.Info, `Stopping timer for device ${deviceId}`);
    timer.stop();
  }

  /**
   * Set playback speed for device timer.
   *
   * Phase 5: No-op (speed always 1.0), but API exists for future phases.
   * Future: Will affect timer progression rate.
   *
   * @param deviceId - Device identifier
   * @param speed - Speed multiplier (1.0 = normal)
   */
  setSpeed(deviceId: string, speed: number): void {
    const timer = this.timers.get(deviceId);
    if (!timer) {
      return;
    }

    logInfo(LogType.Info, `Setting timer speed for device ${deviceId} to ${speed}`);
    timer.setSpeed(speed);
  }

  /**
   * Get current timer state for device.
   *
   * @param deviceId - Device identifier
   * @returns TimerState or null if no timer exists
   */
  getTimerState(deviceId: string): TimerState | null {
    const timer = this.timers.get(deviceId);
    if (!timer) {
      return null;
    }

    return {
      totalTime: timer.totalTime,
      currentTime: timer.currentTime,
      isRunning: timer.isRunning,
      isPaused: timer.isPaused,
      speed: timer.speed,
      showProgress: true, // Phase 5: Always true for music files
    };
  }

  /**
   * Observable stream of timer updates for device.
   *
   * @param deviceId - Device identifier
   * @returns Observable of TimerState updates
   */
  onTimerUpdate$(deviceId: string): Observable<TimerState> {
    let subject = this.timerUpdateSubjects.get(deviceId);
    if (!subject) {
      // Create subject if it doesn't exist
      subject = new Subject<TimerState>();
      this.timerUpdateSubjects.set(deviceId, subject);
    }
    return subject.asObservable();
  }

  /**
   * Observable stream of timer completion events for device.
   *
   * @param deviceId - Device identifier
   * @returns Observable of completion events
   */
  onTimerComplete$(deviceId: string): Observable<void> {
    let subject = this.timerCompleteSubjects.get(deviceId);
    if (!subject) {
      // Create subject if it doesn't exist
      subject = new Subject<void>();
      this.timerCompleteSubjects.set(deviceId, subject);
    }
    return subject.asObservable();
  }
}
