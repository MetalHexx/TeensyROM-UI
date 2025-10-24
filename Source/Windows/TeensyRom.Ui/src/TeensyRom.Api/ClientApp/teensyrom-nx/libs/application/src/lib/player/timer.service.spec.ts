import { TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { TimerService } from './timer.service';

describe('TimerService', () => {
  let service: TimerService;

  // Helper to wait for timer ticks
  const waitForTime = (ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms));

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TimerService],
    });
    service = TestBed.inject(TimerService);
  });

  afterEach(() => {
    // Cleanup after each test
    service.ngOnDestroy();
  });

  describe('initialization', () => {
    it('should create the service', () => {
      expect(service).toBeTruthy();
    });

    it('should have initial state with zeros', () => {
      expect(service.currentTime).toBe(0);
      expect(service.totalTime).toBe(0);
      expect(service.isRunning).toBe(false);
      expect(service.isPaused).toBe(false);
      expect(service.speed).toBe(1.0);
    });
  });

  describe('start', () => {
    it('should start timer at currentTime 0', () => {
      service.start(5000);

      expect(service.currentTime).toBe(0);
      expect(service.totalTime).toBe(5000);
      expect(service.isRunning).toBe(true);
      expect(service.isPaused).toBe(false);
    });

    it('should emit initial currentTime of 0', () => {
      const emittedValues: number[] = [];
      service.currentTime$.subscribe((time) => emittedValues.push(time));

      service.start(5000);

      expect(emittedValues).toContain(0);
    });

    it('should stop existing timer before starting new one', async () => {
      service.start(5000);
      await waitForTime(250);
      const timeAfterFirstStart = service.currentTime;
      expect(timeAfterFirstStart).toBeGreaterThan(0);

      service.start(10000);
      expect(service.currentTime).toBe(0);
      expect(service.totalTime).toBe(10000);
    });
  });

  describe('timer progression', () => {
    it('should increment currentTime correctly over time', async () => {
      service.start(5000);

      await waitForTime(150);
      expect(service.currentTime).toBeGreaterThanOrEqual(100);

      await waitForTime(200);
      expect(service.currentTime).toBeGreaterThanOrEqual(250);

      await waitForTime(500);
      // Allow tolerance for system load: expect at least 650ms of elapsed time
      expect(service.currentTime).toBeGreaterThanOrEqual(650);
    });

    it('should emit currentTime updates via observable', async () => {
      const emittedValues: number[] = [];
      service.currentTime$.subscribe((time) => emittedValues.push(time));

      service.start(1000);
      await waitForTime(350);

      expect(emittedValues).toContain(0);
      expect(emittedValues.length).toBeGreaterThan(1);
      expect(emittedValues[emittedValues.length - 1]).toBeGreaterThan(0);
    });

    it('should use 100ms tick interval', async () => {
      service.start(5000);

      await waitForTime(150);
      const firstTime = service.currentTime;
      expect(firstTime).toBeGreaterThanOrEqual(100);

      await waitForTime(150);
      const secondTime = service.currentTime;
      expect(secondTime).toBeGreaterThanOrEqual(firstTime + 100);
    });
  });

  describe('pause', () => {
    it('should pause timer and stop progression', async () => {
      service.start(5000);
      await waitForTime(250);

      service.pause();
      const pausedTime = service.currentTime;

      await waitForTime(250);
      expect(service.currentTime).toBe(pausedTime);
      expect(service.isRunning).toBe(false);
      expect(service.isPaused).toBe(true);
    });

    it('should not pause if timer is not running', () => {
      service.pause();
      expect(service.isPaused).toBe(false);
    });

    it('should not pause if already paused', async () => {
      service.start(5000);
      await waitForTime(150);
      service.pause();

      const pausedTime = service.currentTime;
      service.pause(); // Second pause call

      expect(service.currentTime).toBe(pausedTime);
      expect(service.isPaused).toBe(true);
    });
  });

  describe('resume', () => {
    it('should resume from paused state and continue progression', async () => {
      service.start(5000);
      await waitForTime(250);
      service.pause();

      const pausedTime = service.currentTime;

      service.resume();
      await waitForTime(250);

      expect(service.currentTime).toBeGreaterThan(pausedTime);
      expect(service.isRunning).toBe(true);
      expect(service.isPaused).toBe(false);
    });

    it('should not resume if not paused', () => {
      service.start(5000);
      service.resume(); // Resume when not paused

      expect(service.isRunning).toBe(true);
      expect(service.isPaused).toBe(false);
    });
  });

  describe('stop', () => {
    it('should stop timer and reset to 0', async () => {
      service.start(5000);
      await waitForTime(250);

      service.stop();

      expect(service.currentTime).toBe(0);
      expect(service.totalTime).toBe(0);
      expect(service.isRunning).toBe(false);
      expect(service.isPaused).toBe(false);
    });

    it('should emit currentTime 0 after stop', async () => {
      const emittedValues: number[] = [];
      service.currentTime$.subscribe((time) => emittedValues.push(time));

      service.start(5000);
      await waitForTime(250);
      service.stop();

      expect(emittedValues[emittedValues.length - 1]).toBe(0);
    });

    it('should not continue progression after stop', async () => {
      service.start(5000);
      await waitForTime(250);
      service.stop();

      await waitForTime(250);
      expect(service.currentTime).toBe(0);
    });
  });

  describe('reset', () => {
    it('should reset currentTime to 0', async () => {
      service.start(5000);
      await waitForTime(250);

      service.reset();

      expect(service.currentTime).toBe(0);
    });

    it('should emit currentTime 0 after reset', async () => {
      const emittedValues: number[] = [];
      service.currentTime$.subscribe((time) => emittedValues.push(time));

      service.start(5000);
      await waitForTime(250);
      service.reset();

      expect(emittedValues[emittedValues.length - 1]).toBe(0);
    });

    it('should keep timer running if it was running before reset', async () => {
      service.start(5000);
      await waitForTime(250);
      service.reset();

      await waitForTime(250);
      expect(service.currentTime).toBeGreaterThan(0);
      expect(service.isRunning).toBe(true);
    });
  });

  describe('completion', () => {
    it('should emit completion event when currentTime >= totalTime', async () => {
      const completionSpy = vi.fn();
      service.completion$.subscribe(() => completionSpy());

      service.start(300);
      await waitForTime(400);

      expect(completionSpy).toHaveBeenCalled();
    });

    it('should stop timer when completion occurs', async () => {
      service.start(300);
      await waitForTime(400);

      expect(service.isRunning).toBe(false);
    });

    it('should not continue incrementing after completion', async () => {
      service.start(300);
      await waitForTime(400);

      const completionTime = service.currentTime;
      await waitForTime(250);

      expect(service.currentTime).toBe(completionTime);
    });

    it('should handle completion at exact totalTime', async () => {
      const completionSpy = vi.fn();
      service.completion$.subscribe(() => completionSpy());

      service.start(500);
      await waitForTime(600);

      expect(service.currentTime).toBeGreaterThanOrEqual(500);
      expect(completionSpy).toHaveBeenCalled();
      expect(service.isRunning).toBe(false);
    });

    it('should handle completion slightly past totalTime', async () => {
      const completionSpy = vi.fn();
      service.completion$.subscribe(() => completionSpy());

      service.start(450);
      await waitForTime(600);

      expect(service.currentTime).toBeGreaterThanOrEqual(450);
      expect(completionSpy).toHaveBeenCalled();
      expect(service.isRunning).toBe(false);
    });
  });

  describe('setSpeed', () => {
    it('should store speed value (Phase 5: no-op)', () => {
      service.setSpeed(2.0);
      expect(service.speed).toBe(2.0);
    });

    it('should not affect timer progression in Phase 5', async () => {
      service.start(5000);
      service.setSpeed(2.0);

      await waitForTime(150);
      // Should still increment by approximately 100ms, not 200ms
      expect(service.currentTime).toBeLessThan(200);
      expect(service.currentTime).toBeGreaterThanOrEqual(100);
    });
  });

  describe('subscription cleanup', () => {
    it('should cleanup subscriptions on stop', async () => {
      service.start(5000);
      await waitForTime(250);
      service.stop();

      // Timer should not continue after stop
      await waitForTime(250);
      expect(service.currentTime).toBe(0);
    });

    it('should cleanup subscriptions on ngOnDestroy', async () => {
      service.start(5000);
      await waitForTime(250);

      service.ngOnDestroy();

      // Timer should not continue after destroy
      await waitForTime(250);
      expect(service.currentTime).toBe(0);
    });
  });

  describe('pause and resume cycle', () => {
    it('should handle multiple pause/resume cycles', async () => {
      service.start(5000);

      await waitForTime(250);
      const time1 = service.currentTime;
      expect(time1).toBeGreaterThan(0);

      service.pause();
      await waitForTime(250);
      expect(service.currentTime).toBe(time1); // Paused, no change

      service.resume();
      await waitForTime(250);
      const time2 = service.currentTime;
      expect(time2).toBeGreaterThan(time1); // Resumed, continued from time1

      service.pause();
      await waitForTime(150);
      expect(service.currentTime).toBe(time2); // Paused again

      service.resume();
      await waitForTime(250);
      expect(service.currentTime).toBeGreaterThan(time2); // Resumed again
    });
  });

  describe('edge cases', () => {
    it('should handle zero duration timer', async () => {
      const completionSpy = vi.fn();
      service.completion$.subscribe(() => completionSpy());

      service.start(0);
      await waitForTime(150);

      expect(completionSpy).toHaveBeenCalled();
      expect(service.isRunning).toBe(false);
    });

    it('should handle very short duration', async () => {
      const completionSpy = vi.fn();
      service.completion$.subscribe(() => completionSpy());

      service.start(50); // Less than tick interval
      await waitForTime(200);

      expect(completionSpy).toHaveBeenCalled();
    });

    it('should handle very long duration', async () => {
      service.start(3600000); // 1 hour

      await waitForTime(250);
      expect(service.currentTime).toBeGreaterThan(0);
      expect(service.currentTime).toBeLessThan(1000);
      expect(service.isRunning).toBe(true);
    });
  });
});
