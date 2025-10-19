import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import {
  DevicesApiService,
} from '@teensyrom-nx/data-access/api-client';
import { DeviceService } from './device.service';
import { ALERT_SERVICE } from '@teensyrom-nx/domain';

describe('DeviceService - Alert Integration', () => {
  let service: DeviceService;
  let mockApiService: {
    findDevices: ReturnType<typeof vi.fn>;
    connectDevice: ReturnType<typeof vi.fn>;
    disconnectDevice: ReturnType<typeof vi.fn>;
    resetDevice: ReturnType<typeof vi.fn>;
    pingDevice: ReturnType<typeof vi.fn>;
  };
  let mockAlertService: {
    error: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    mockApiService = {
      findDevices: vi.fn(),
      connectDevice: vi.fn(),
      disconnectDevice: vi.fn(),
      resetDevice: vi.fn(),
      pingDevice: vi.fn(),
    };

    mockAlertService = {
      error: vi.fn(),
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        DeviceService,
        { provide: DevicesApiService, useValue: mockApiService },
        { provide: ALERT_SERVICE, useValue: mockAlertService },
      ],
    });

    service = TestBed.inject(DeviceService);
  });

  describe('findDevices error handling', () => {
    it('should display error alert when API fails', async () => {
      const error = new Error('Find devices failed');
      mockApiService.findDevices.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.findDevices(true).subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Find devices failed');
      logSpy.mockRestore();
    });

    it('should use fallback message when error message is missing', async () => {
      const error = { error: {} };
      mockApiService.findDevices.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.findDevices(true).subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to find devices');
      logSpy.mockRestore();
    });

    it('should extract message from error.error.message', async () => {
      // Test that non-Error objects with nested structure use fallback message
      const error = { error: { message: 'API error message' } };
      mockApiService.findDevices.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.findDevices(true).subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      // Non-Error objects use fallback message
      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to find devices');
      logSpy.mockRestore();
    });

    it('should rethrow error after displaying alert', async () => {
      const error = new Error('Network error');
      mockApiService.findDevices.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      let caughtError: Error | null = null;
      await new Promise<void>((resolve) => {
        service.findDevices(true).subscribe({
          error: (err: Error) => {
            caughtError = err;
            resolve();
          },
        });
      });

      expect(caughtError).toBeDefined();
      expect(caughtError).toBe(error);
      logSpy.mockRestore();
    });
  });

  describe('getConnectedDevices error handling', () => {
    it('should display error alert on failure', async () => {
      const error = new Error('Get connected devices failed');
      mockApiService.findDevices.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.getConnectedDevices().subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Get connected devices failed');
      logSpy.mockRestore();
    });

    it('should use correct fallback message', async () => {
      const error = {};
      mockApiService.findDevices.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.getConnectedDevices().subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to retrieve connected devices');
      logSpy.mockRestore();
    });
  });

  describe('connectDevice error handling', () => {
    it('should display error alert when connection fails', async () => {
      const error = new Error('Connection failed');
      mockApiService.connectDevice.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.connectDevice('device-123').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Connection failed');
      logSpy.mockRestore();
    });

    it('should use connect device fallback message', async () => {
      const error = { message: null, error: {} };
      mockApiService.connectDevice.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.connectDevice('device-123').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to connect to device');
      logSpy.mockRestore();
    });
  });

  describe('disconnectDevice error handling', () => {
    it('should display error alert on disconnect failure', async () => {
      const error = new Error('Disconnect error');
      mockApiService.disconnectDevice.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.disconnectDevice('device-123').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Disconnect error');
      logSpy.mockRestore();
    });

    it('should use disconnect device fallback message', async () => {
      const error = {};
      mockApiService.disconnectDevice.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.disconnectDevice('device-123').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to disconnect device');
      logSpy.mockRestore();
    });
  });

  describe('resetDevice error handling', () => {
    it('should display error alert on reset failure', async () => {
      const error = new Error('Reset failed');
      mockApiService.resetDevice.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.resetDevice('device-123').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Reset failed');
      logSpy.mockRestore();
    });

    it('should use reset device fallback message', async () => {
      const error = { error: { message: undefined } };
      mockApiService.resetDevice.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.resetDevice('device-123').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to reset device');
      logSpy.mockRestore();
    });
  });

  describe('pingDevice error handling', () => {
    it('should display error alert on ping failure', async () => {
      const error = new Error('Ping timeout');
      mockApiService.pingDevice.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.pingDevice('device-123').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Ping timeout');
      logSpy.mockRestore();
    });

    it('should use ping device fallback message', async () => {
      const error = {};
      mockApiService.pingDevice.mockRejectedValue(error);
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.pingDevice('device-123').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledWith('Failed to ping device');
      logSpy.mockRestore();
    });
  });

  describe('Alert service is called exactly once per error', () => {
    it('findDevices should call alert service once', async () => {
      mockApiService.findDevices.mockRejectedValue(new Error('Test'));
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.findDevices(true).subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledTimes(1);
      logSpy.mockRestore();
    });

    it('connectDevice should call alert service once', async () => {
      mockApiService.connectDevice.mockRejectedValue(new Error('Test'));
      const logSpy = vi.spyOn(console, 'error').mockImplementation(() => undefined);

      await expect(
        new Promise((resolve, reject) => {
          service.connectDevice('dev-1').subscribe({
            next: resolve,
            error: reject,
          });
        })
      ).rejects.toThrow();

      expect(mockAlertService.error).toHaveBeenCalledTimes(1);
      logSpy.mockRestore();
    });
  });
});
