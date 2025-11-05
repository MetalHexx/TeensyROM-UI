import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { DevicesApiService } from '@teensyrom-nx/data-access/api-client';
import { DeviceLogsService } from './device-logs.service';
import { ALERT_SERVICE, IAlertService } from '@teensyrom-nx/domain';

describe('DeviceLogsService - Alert Integration', () => {
  let service: DeviceLogsService;
  let mockDevicesApiService: {
    startLogs: ReturnType<typeof vi.fn>;
    stopLogs: ReturnType<typeof vi.fn>;
  };
  let mockAlertService: Partial<IAlertService>;
  let consoleErrorSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    mockDevicesApiService = {
      startLogs: vi.fn(),
      stopLogs: vi.fn(),
    };

    mockAlertService = {
      error: vi.fn(),
    };

    // Suppress console.error for all tests - they intentionally test error scenarios
    consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        DeviceLogsService,
        { provide: DevicesApiService, useValue: mockDevicesApiService },
        { provide: ALERT_SERVICE, useValue: mockAlertService },
      ],
    });

    service = TestBed.inject(DeviceLogsService);
  });

  afterEach(() => {
    consoleErrorSpy.mockRestore();
  });

  describe('connect() error handling', () => {
    it('should display alert when startLogs API fails', async () => {
      const error = new Error('Start logs failed');
      mockDevicesApiService.startLogs.mockReturnValue(Promise.reject(error));

      service.connect();

      // Wait one tick for promise chain to complete
      await new Promise((resolve) => setTimeout(resolve, 0));

      expect(mockAlertService.error).toHaveBeenCalledWith('Start logs failed');
    });

    it('should use fallback message when error message is missing', async () => {
      const error = { error: {} };
      mockDevicesApiService.startLogs.mockReturnValue(Promise.reject(error));

      service.connect();

      await new Promise((resolve) => setTimeout(resolve, 0));

      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to start device logs');
    });

    it('should extract message from error.error.message', async () => {
      // Test that non-Error objects with nested structure use fallback message
      const error = { error: { message: 'Device offline' } };
      mockDevicesApiService.startLogs.mockReturnValue(Promise.reject(error));

      service.connect();

      await new Promise((resolve) => setTimeout(resolve, 0));

      // Non-Error objects use fallback message
      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to start device logs');
    });
  });

  describe('disconnect() error handling', () => {
    it('should display alert when stopLogs API fails', async () => {
      const error = new Error('Stop logs failed');
      mockDevicesApiService.stopLogs.mockReturnValue(Promise.reject(error));

      service.disconnect();

      await new Promise((resolve) => setTimeout(resolve, 0));

      expect(mockAlertService.error).toHaveBeenCalledWith('Stop logs failed');
    });

    it('should use fallback message for stopLogs', async () => {
      const error = {};
      mockDevicesApiService.stopLogs.mockReturnValue(Promise.reject(error));

      service.disconnect();

      await new Promise((resolve) => setTimeout(resolve, 0));

      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to stop device logs');
    });
  });

  describe('Clear operation', () => {
    it('should clear log lines', () => {
      service.clear();
      expect(service.logs()).toEqual([]);
    });
  });
});
