import { TestBed } from '@angular/core/testing';
import { AlertService } from './alert.service';
import { AlertSeverity, AlertPosition, AlertMessage } from '@teensyrom-nx/domain';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

describe('AlertService', () => {
  let service: AlertService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AlertService],
    });
    service = TestBed.inject(AlertService);
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.clearAllTimers();
  });

  describe('show method', () => {
    it('should add alert to signal with correct properties', async () => {
      service.show('Test message', AlertSeverity.Success);

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      expect(capturedAlerts[0]).toMatchObject({
        message: 'Test message',
        severity: AlertSeverity.Success,
        position: AlertPosition.BottomCenter,
        autoDismissMs: 5000,
      });

      subscription.unsubscribe();
    });

    it('should use custom position when provided', async () => {
      service.show('Test message', AlertSeverity.Success, AlertPosition.TopLeft);

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      expect(capturedAlerts[0].position).toBe(AlertPosition.TopLeft);

      subscription.unsubscribe();
    });

    it('should use custom autoDismissMs when provided', async () => {
      service.show('Test message', AlertSeverity.Success, AlertPosition.BottomRight, 5000);

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      expect(capturedAlerts[0].autoDismissMs).toBe(5000);

      subscription.unsubscribe();
    });
  });

  describe('success method', () => {
    it('should create success alert with correct severity', async () => {
      service.success('Success message');

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      expect(capturedAlerts[0].severity).toBe(AlertSeverity.Success);

      subscription.unsubscribe();
    });
  });

  describe('error method', () => {
    it('should create error alert with correct severity', async () => {
      service.error('Error message');

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      expect(capturedAlerts[0].severity).toBe(AlertSeverity.Error);

      subscription.unsubscribe();
    });
  });

  describe('warning method', () => {
    it('should create warning alert with correct severity', async () => {
      service.warning('Warning message');

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      expect(capturedAlerts[0].severity).toBe(AlertSeverity.Warning);

      subscription.unsubscribe();
    });
  });

  describe('info method', () => {
    it('should create info alert with correct severity', async () => {
      service.info('Info message');

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      expect(capturedAlerts[0].severity).toBe(AlertSeverity.Info);

      subscription.unsubscribe();
    });
  });

  describe('dismiss method', () => {
    it('should remove alert from signal', async () => {
      service.show('Test 1', AlertSeverity.Success);
      service.show('Test 2', AlertSeverity.Error);

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(2);
      });

      const firstAlertId = capturedAlerts[0].id;
      service.dismiss(firstAlertId);

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      expect(capturedAlerts[0].message).toBe('Test 2');

      subscription.unsubscribe();
    });
  });

  describe('auto-dismiss', () => {
    it('should automatically remove alert after configured timeout', async () => {
      service.show('Test auto-dismiss', AlertSeverity.Success);

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      vi.advanceTimersByTime(5000);

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(0);
      });

      subscription.unsubscribe();
    });

    it('should use custom duration for auto-dismiss', async () => {
      service.show('Custom timeout', AlertSeverity.Info, AlertPosition.BottomRight, 5000);

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      vi.advanceTimersByTime(3000);

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      vi.advanceTimersByTime(2000);

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(0);
      });

      subscription.unsubscribe();
    });

    it('should cancel auto-dismiss timer when manually dismissed', async () => {
      service.show('Will be dismissed', AlertSeverity.Success);

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(1);
      });

      const alertId = capturedAlerts[0].id;
      service.dismiss(alertId);

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(0);
      });

      vi.advanceTimersByTime(5000);

      expect(capturedAlerts.length).toBe(0);

      subscription.unsubscribe();
    });
  });

  describe('multiple alerts', () => {
    it('should manage multiple alerts independently', async () => {
      service.success('Alert 1');
      service.error('Alert 2', AlertPosition.TopCenter, 2000);
      service.warning('Alert 3', AlertPosition.BottomLeft);

      let capturedAlerts: AlertMessage[] = [];
      const subscription = service.alerts$.subscribe((alerts) => {
        capturedAlerts = alerts;
      });

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(3);
      });

      vi.advanceTimersByTime(2000);

      await vi.waitFor(() => {
        expect(capturedAlerts.length).toBe(2);
      });

      expect(capturedAlerts.some((a) => a.message === 'Alert 2')).toBe(false);

      subscription.unsubscribe();
    });
  });
});
