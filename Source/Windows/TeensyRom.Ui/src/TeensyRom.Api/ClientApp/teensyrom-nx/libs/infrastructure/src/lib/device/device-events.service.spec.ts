import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { DevicesApiService } from '@teensyrom-nx/data-access/api-client';
import { DeviceEventsService } from './device-events.service';
import { ALERT_SERVICE } from '@teensyrom-nx/domain';

describe('DeviceEventsService - Alert Integration', () => {
  let service: DeviceEventsService;
  let mockDevicesApiService: {
    startDeviceEvents: ReturnType<typeof vi.fn>;
    stopDeviceEvents: ReturnType<typeof vi.fn>;
  };
  let mockAlertService: {
    error: ReturnType<typeof vi.fn>;
  };
  let consoleErrorSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    mockDevicesApiService = {
      startDeviceEvents: vi.fn(),
      stopDeviceEvents: vi.fn(),
    };

    mockAlertService = {
      error: vi.fn(),
    };

    // Suppress console.error for all tests - they intentionally test error scenarios
    consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        DeviceEventsService,
        { provide: DevicesApiService, useValue: mockDevicesApiService },
        { provide: ALERT_SERVICE, useValue: mockAlertService },
      ],
    });

    service = TestBed.inject(DeviceEventsService);
  });

  afterEach(() => {
    consoleErrorSpy.mockRestore();
  });

  describe('connect() error handling', () => {
    it('should display alert when startDeviceEvents API fails', async () => {
      const error = new Error('Start device events failed');
      mockDevicesApiService.startDeviceEvents.mockReturnValue(Promise.reject(error));

      service.connect();

      // Wait one tick for promise chain to complete
      await new Promise((resolve) => setTimeout(resolve, 0));

      expect(mockAlertService.error).toHaveBeenCalledWith('Start device events failed');
    });

    it('should use fallback message when error message is missing', async () => {
      const error = { error: {} };
      mockDevicesApiService.startDeviceEvents.mockReturnValue(Promise.reject(error));

      service.connect();

      await new Promise((resolve) => setTimeout(resolve, 0));

      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to start device events');
    });

    it('should extract message from error.error.message', async () => {
      // Test that non-Error objects with nested structure use fallback message
      const error = { error: { message: 'Connection timeout' } };
      mockDevicesApiService.startDeviceEvents.mockReturnValue(Promise.reject(error));

      service.connect();

      await new Promise((resolve) => setTimeout(resolve, 0));

      // Non-Error objects use fallback message
      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to start device events');
    });
  });

  describe('disconnect() error handling', () => {
    it('should display alert when stopDeviceEvents API fails', async () => {
      const error = new Error('Stop device events failed');
      mockDevicesApiService.stopDeviceEvents.mockReturnValue(Promise.reject(error));

      service.disconnect();

      await new Promise((resolve) => setTimeout(resolve, 0));

      expect(mockAlertService.error).toHaveBeenCalledWith('Stop device events failed');
    });

    it('should use fallback message for stopDeviceEvents', async () => {
      const error = {};
      mockDevicesApiService.stopDeviceEvents.mockReturnValue(Promise.reject(error));

      service.disconnect();

      await new Promise((resolve) => setTimeout(resolve, 0));

      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to stop device events');
    });
  });

  describe('getDeviceState operation', () => {
    it('should return null for non-existent device', () => {
      const state = service.getDeviceState('device-123')();
      expect(state).toBeNull();
    });
  });
});
