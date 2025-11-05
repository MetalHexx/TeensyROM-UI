import { TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { PlayerTimerManager } from './player-timer-manager';
import { TimerState } from './timer-state.interface';

describe('PlayerTimerManager', () => {
  let manager: PlayerTimerManager;

  // Helper to wait for timer ticks
  const waitForTime = (ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms));

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PlayerTimerManager],
    });
    manager = TestBed.inject(PlayerTimerManager);
  });

  afterEach(() => {
    // Cleanup all timers after each test
    manager.destroyTimer('device1');
    manager.destroyTimer('device2');
    manager.destroyTimer('device3');
  });

  describe('createTimer', () => {
    it('should create and start timer for device', () => {
      manager.createTimer('device1', 5000);

      const state = manager.getTimerState('device1');
      expect(state).not.toBeNull();
      expect(state?.totalTime).toBe(5000);
      expect(state?.currentTime).toBe(0);
      expect(state?.isRunning).toBe(true);
      expect(state?.isPaused).toBe(false);
      expect(state?.showProgress).toBe(true);
    });

    it('should destroy existing timer before creating new one', async () => {
      manager.createTimer('device1', 5000);
      await waitForTime(250);

      const firstTime = manager.getTimerState('device1')?.currentTime || 0;
      expect(firstTime).toBeGreaterThan(0);

      manager.createTimer('device1', 10000);

      const state = manager.getTimerState('device1');
      expect(state?.totalTime).toBe(10000);
      expect(state?.currentTime).toBe(0);
    });

    it('should emit timer updates via observable', async () => {
      const emittedStates: TimerState[] = [];
      // Subscribe BEFORE creating timer
      manager.onTimerUpdate$('device1').subscribe((state) => emittedStates.push(state));

      manager.createTimer('device1', 5000);
      await waitForTime(250);

      expect(emittedStates.length).toBeGreaterThan(1);
      expect(emittedStates[0].currentTime).toBe(0);
      expect(emittedStates[emittedStates.length - 1].currentTime).toBeGreaterThan(0);
    });
  });

  describe('destroyTimer', () => {
    it('should destroy timer and cleanup resources', async () => {
      manager.createTimer('device1', 5000);
      await waitForTime(250);

      manager.destroyTimer('device1');

      const state = manager.getTimerState('device1');
      expect(state).toBeNull();
    });

    it('should stop timer progression after destroy', async () => {
      manager.createTimer('device1', 5000);
      await waitForTime(250);

      manager.destroyTimer('device1');

      // Wait and verify timer doesn't continue
      await waitForTime(250);
      const state = manager.getTimerState('device1');
      expect(state).toBeNull();
    });

    it('should handle destroying non-existent timer gracefully', () => {
      expect(() => manager.destroyTimer('nonexistent')).not.toThrow();
    });

    it('should keep observable subjects active after destroy for reuse', async () => {
      let emissionCount = 0;
      // Subscribe BEFORE creating timer
      manager.onTimerUpdate$('device1').subscribe({
        next: () => emissionCount++,
      });

      manager.createTimer('device1', 5000);
      await waitForTime(150);
      const firstEmissions = emissionCount;

      manager.destroyTimer('device1');

      // Create new timer - should reuse same subject and subscriber should still receive events
      manager.createTimer('device1', 5000);
      await waitForTime(150);

      expect(emissionCount).toBeGreaterThan(firstEmissions);
    });
  });

  describe('pauseTimer', () => {
    it('should pause timer progression', async () => {
      manager.createTimer('device1', 5000);
      await waitForTime(250);

      manager.pauseTimer('device1');
      const pausedTime = manager.getTimerState('device1')?.currentTime || 0;

      await waitForTime(250);
      const currentTime = manager.getTimerState('device1')?.currentTime || 0;

      expect(currentTime).toBe(pausedTime);
      expect(manager.getTimerState('device1')?.isPaused).toBe(true);
    });

    it('should handle pausing non-existent timer gracefully', () => {
      expect(() => manager.pauseTimer('nonexistent')).not.toThrow();
    });
  });

  describe('resumeTimer', () => {
    it('should resume timer from paused state', async () => {
      manager.createTimer('device1', 5000);
      await waitForTime(250);

      manager.pauseTimer('device1');
      const pausedTime = manager.getTimerState('device1')?.currentTime || 0;

      manager.resumeTimer('device1');
      await waitForTime(250);

      const currentTime = manager.getTimerState('device1')?.currentTime || 0;
      expect(currentTime).toBeGreaterThan(pausedTime);
      expect(manager.getTimerState('device1')?.isRunning).toBe(true);
      expect(manager.getTimerState('device1')?.isPaused).toBe(false);
    });

    it('should handle resuming non-existent timer gracefully', () => {
      expect(() => manager.resumeTimer('nonexistent')).not.toThrow();
    });
  });

  describe('stopTimer', () => {
    it('should stop timer and reset to 0', async () => {
      manager.createTimer('device1', 5000);
      await waitForTime(250);

      manager.stopTimer('device1');

      const state = manager.getTimerState('device1');
      expect(state?.currentTime).toBe(0);
      expect(state?.isRunning).toBe(false);
    });

    it('should preserve timer instance for UI display', async () => {
      manager.createTimer('device1', 5000);
      await waitForTime(250);

      manager.stopTimer('device1');

      const state = manager.getTimerState('device1');
      expect(state).not.toBeNull();
      expect(state?.totalTime).toBe(0); // Reset by TimerService.stop()
    });

    it('should handle stopping non-existent timer gracefully', () => {
      expect(() => manager.stopTimer('nonexistent')).not.toThrow();
    });
  });

  describe('setSpeed', () => {
    it('should delegate speed changes to TimerService', () => {
      manager.createTimer('device1', 5000);

      manager.setSpeed('device1', 2.0);

      const state = manager.getTimerState('device1');
      expect(state?.speed).toBe(2.0);
    });

    it('should not affect other device timers', () => {
      manager.createTimer('device1', 5000);
      manager.createTimer('device2', 5000);

      manager.setSpeed('device1', 2.0);

      expect(manager.getTimerState('device1')?.speed).toBe(2.0);
      expect(manager.getTimerState('device2')?.speed).toBe(1.0);
    });

    it('should persist speed through pause/resume cycles', async () => {
      manager.createTimer('device1', 5000);
      manager.setSpeed('device1', 1.5);

      manager.pauseTimer('device1');
      await waitForTime(100);

      manager.resumeTimer('device1');
      await waitForTime(100);

      expect(manager.getTimerState('device1')?.speed).toBe(1.5);
    });

    it('should handle setting speed on non-existent timer gracefully', () => {
      expect(() => manager.setSpeed('nonexistent', 2.0)).not.toThrow();
    });
  });

  describe('getTimerState', () => {
    it('should return current timer state', async () => {
      manager.createTimer('device1', 5000);
      await waitForTime(250);

      const state = manager.getTimerState('device1');
      expect(state).not.toBeNull();
      expect(state?.totalTime).toBe(5000);
      expect(state?.currentTime).toBeGreaterThan(0);
      expect(state?.isRunning).toBe(true);
      expect(state?.showProgress).toBe(true);
    });

    it('should return null for non-existent timer', () => {
      const state = manager.getTimerState('nonexistent');
      expect(state).toBeNull();
    });

    it('should return updated state after pause', async () => {
      manager.createTimer('device1', 5000);
      await waitForTime(250);

      manager.pauseTimer('device1');

      const state = manager.getTimerState('device1');
      expect(state?.isPaused).toBe(true);
      expect(state?.isRunning).toBe(false);
    });
  });

  describe('onTimerUpdate$', () => {
    it('should emit timer state updates', async () => {
      const emittedStates: TimerState[] = [];
      // Subscribe BEFORE creating timer
      manager.onTimerUpdate$('device1').subscribe((state) => emittedStates.push(state));

      manager.createTimer('device1', 5000);
      await waitForTime(350);

      expect(emittedStates.length).toBeGreaterThan(1);
      expect(emittedStates[0].currentTime).toBe(0);
      expect(emittedStates[emittedStates.length - 1].currentTime).toBeGreaterThan(0);
    });

    it('should emit complete timer state including all properties', async () => {
      const emittedStates: TimerState[] = [];
      // Subscribe BEFORE creating timer
      manager.onTimerUpdate$('device1').subscribe((state) => emittedStates.push(state));

      manager.createTimer('device1', 5000);
      await waitForTime(150);

      expect(emittedStates.length).toBeGreaterThan(0);
      const latestState = emittedStates[emittedStates.length - 1];
      expect(latestState).toHaveProperty('totalTime');
      expect(latestState).toHaveProperty('currentTime');
      expect(latestState).toHaveProperty('isRunning');
      expect(latestState).toHaveProperty('isPaused');
      expect(latestState).toHaveProperty('speed');
      expect(latestState).toHaveProperty('showProgress');
    });

    it('should create subject if it does not exist', () => {
      const observable = manager.onTimerUpdate$('device1');
      expect(observable).toBeDefined();
    });
  });

  describe('onTimerComplete$', () => {
    it('should emit completion event when timer finishes', async () => {
      const completionSpy = vi.fn();
      // Subscribe BEFORE creating timer
      manager.onTimerComplete$('device1').subscribe(() => completionSpy());

      manager.createTimer('device1', 300);
      await waitForTime(450);

      expect(completionSpy).toHaveBeenCalled();
    });

    it('should create subject if it does not exist', () => {
      const observable = manager.onTimerComplete$('device1');
      expect(observable).toBeDefined();
    });
  });

  describe('multiple independent device timers', () => {
    it('should manage multiple timers independently', async () => {
      manager.createTimer('device1', 5000);
      manager.createTimer('device2', 10000);

      await waitForTime(250);

      const state1 = manager.getTimerState('device1');
      const state2 = manager.getTimerState('device2');

      expect(state1).not.toBeNull();
      expect(state2).not.toBeNull();
      expect(state1?.totalTime).toBe(5000);
      expect(state2?.totalTime).toBe(10000);
    });

    it('should allow independent pause/resume per device', async () => {
      manager.createTimer('device1', 5000);
      manager.createTimer('device2', 5000);

      await waitForTime(250);

      manager.pauseTimer('device1');
      const device1PausedTime = manager.getTimerState('device1')?.currentTime || 0;

      await waitForTime(250);

      expect(manager.getTimerState('device1')?.currentTime).toBe(device1PausedTime);
      expect(manager.getTimerState('device2')?.currentTime).toBeGreaterThan(device1PausedTime);
    });

    it('should allow independent speed control per device', () => {
      manager.createTimer('device1', 5000);
      manager.createTimer('device2', 5000);

      manager.setSpeed('device1', 1.5);
      manager.setSpeed('device2', 2.0);

      expect(manager.getTimerState('device1')?.speed).toBe(1.5);
      expect(manager.getTimerState('device2')?.speed).toBe(2.0);
    });

    it('should emit independent completion events', async () => {
      const device1Complete = vi.fn();
      const device2Complete = vi.fn();

      // Subscribe BEFORE creating timers
      manager.onTimerComplete$('device1').subscribe(() => device1Complete());
      manager.onTimerComplete$('device2').subscribe(() => device2Complete());

      manager.createTimer('device1', 300);
      manager.createTimer('device2', 600);

      await waitForTime(450);
      expect(device1Complete).toHaveBeenCalled();
      expect(device2Complete).not.toHaveBeenCalled();

      await waitForTime(300);
      expect(device2Complete).toHaveBeenCalled();
    });

    it('should cleanup specific device without affecting others', async () => {
      manager.createTimer('device1', 5000);
      manager.createTimer('device2', 5000);

      await waitForTime(250);

      manager.destroyTimer('device1');

      expect(manager.getTimerState('device1')).toBeNull();
      expect(manager.getTimerState('device2')).not.toBeNull();
    });
  });

  describe('observable stream isolation', () => {
    it('should maintain separate update streams per device', async () => {
      const device1Updates: TimerState[] = [];
      const device2Updates: TimerState[] = [];

      // Subscribe BEFORE creating timers
      manager.onTimerUpdate$('device1').subscribe((state) => device1Updates.push(state));
      manager.onTimerUpdate$('device2').subscribe((state) => device2Updates.push(state));

      manager.createTimer('device1', 5000);
      await waitForTime(150);
      manager.createTimer('device2', 5000);
      await waitForTime(150);

      expect(device1Updates.length).toBeGreaterThan(0);
      expect(device2Updates.length).toBeGreaterThan(0);
      expect(device1Updates[0].totalTime).toBe(5000);
      expect(device2Updates[0].totalTime).toBe(5000);
    });

    it('should maintain separate completion streams per device', async () => {
      const device1Complete = vi.fn();
      const device2Complete = vi.fn();

      // Subscribe BEFORE creating timers
      manager.onTimerComplete$('device1').subscribe(() => device1Complete());
      manager.onTimerComplete$('device2').subscribe(() => device2Complete());

      manager.createTimer('device1', 300);
      manager.createTimer('device2', 5000);

      await waitForTime(450);

      expect(device1Complete).toHaveBeenCalled();
      expect(device2Complete).not.toHaveBeenCalled();
    });
  });

  describe('timer lifecycle edge cases', () => {
    it('should handle rapid create/destroy cycles', () => {
      for (let i = 0; i < 5; i++) {
        manager.createTimer('device1', 1000);
        manager.destroyTimer('device1');
      }

      const state = manager.getTimerState('device1');
      expect(state).toBeNull();
    });

    it('should handle multiple pause/resume cycles', async () => {
      manager.createTimer('device1', 5000);

      for (let i = 0; i < 3; i++) {
        await waitForTime(100);
        manager.pauseTimer('device1');
        await waitForTime(100);
        manager.resumeTimer('device1');
      }

      const state = manager.getTimerState('device1');
      expect(state).not.toBeNull();
      expect(state?.currentTime).toBeGreaterThan(0);
    });

    it('should handle completion during cleanup', async () => {
      manager.createTimer('device1', 200);
      await waitForTime(250);
      manager.destroyTimer('device1');

      const state = manager.getTimerState('device1');
      expect(state).toBeNull();
    });
  });
});
