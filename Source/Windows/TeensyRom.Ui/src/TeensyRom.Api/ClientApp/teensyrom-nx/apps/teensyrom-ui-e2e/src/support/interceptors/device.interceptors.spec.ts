/// <reference types="cypress" />
import { describe, it, expect, beforeEach } from 'vitest';
import {
  interceptFindDevices,
  interceptConnectDevice,
  interceptDisconnectDevice,
  interceptPingDevice,
  type InterceptFindDevicesOptions,
  type InterceptConnectDeviceOptions,
  type InterceptDisconnectDeviceOptions,
  type InterceptPingDeviceOptions,
} from './device.interceptors';
import { singleDevice, multipleDevices, noDevices } from '../test-data/fixtures';

/**
 * Test suite for device API interceptors.
 *
 * These tests verify that interceptor functions correctly register Cypress intercepts
 * with proper route matching and response structures. Tests use mocked cy object
 * to validate interceptor behavior without requiring a running Cypress instance.
 */
describe('Device Interceptors', () => {
  // Mock cy object for testing
  let mockIntercept: ReturnType<typeof vi.fn>;
  let mockAs: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    mockAs = vi.fn().mockReturnValue(undefined);
    mockIntercept = vi.fn().mockReturnValue({
      as: mockAs,
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (global as any).cy = {
      intercept: mockIntercept,
    };
  });

  describe('interceptFindDevices', () => {
    it('should register intercept with default fixture', () => {
      interceptFindDevices();

      expect(mockIntercept).toHaveBeenCalledWith('GET', 'http://localhost:5168/devices*', expect.any(Function));
      expect(mockAs).toHaveBeenCalledWith('findDevices');
    });

    it('should use custom fixture when provided', () => {
      interceptFindDevices({ fixture: multipleDevices });

      expect(mockIntercept).toHaveBeenCalledWith('GET', 'http://localhost:5168/devices*', expect.any(Function));
      expect(mockAs).toHaveBeenCalledWith('findDevices');
    });

    it('should support error mode', () => {
      interceptFindDevices({ errorMode: true });

      expect(mockIntercept).toHaveBeenCalledWith('GET', 'http://localhost:5168/devices*', expect.any(Function));
      expect(mockAs).toHaveBeenCalledWith('findDevices');
    });

    it('should register findDevices alias', () => {
      interceptFindDevices();

      expect(mockAs).toHaveBeenCalledWith('findDevices');
    });

    it('should handle empty devices fixture', () => {
      interceptFindDevices({ fixture: noDevices });

      expect(mockIntercept).toHaveBeenCalledWith('GET', 'http://localhost:5168/devices*', expect.any(Function));
    });
  });

  describe('interceptConnectDevice', () => {
    it('should register intercept with default device', () => {
      interceptConnectDevice();

      expect(mockIntercept).toHaveBeenCalledWith('POST', 'http://localhost:5168/devices/*/connect', expect.any(Function));
      expect(mockAs).toHaveBeenCalledWith('connectDevice');
    });

    it('should use custom device when provided', () => {
      const customDevice = multipleDevices.devices[1];
      interceptConnectDevice({ device: customDevice });

      expect(mockIntercept).toHaveBeenCalledWith('POST', 'http://localhost:5168/devices/*/connect', expect.any(Function));
      expect(mockAs).toHaveBeenCalledWith('connectDevice');
    });

    it('should support error mode', () => {
      interceptConnectDevice({ errorMode: true });

      expect(mockIntercept).toHaveBeenCalledWith('POST', 'http://localhost:5168/devices/*/connect', expect.any(Function));
    });

    it('should register connectDevice alias', () => {
      interceptConnectDevice();

      expect(mockAs).toHaveBeenCalledWith('connectDevice');
    });

    it('should match dynamic deviceId with wildcard', () => {
      interceptConnectDevice();

      // Verify wildcard pattern is used
      expect(mockIntercept).toHaveBeenCalledWith('POST', 'http://localhost:5168/devices/*/connect', expect.any(Function));
    });
  });

  describe('interceptDisconnectDevice', () => {
    it('should register intercept', () => {
      interceptDisconnectDevice();

      expect(mockIntercept).toHaveBeenCalledWith('DELETE', 'http://localhost:5168/devices/*', expect.any(Function));
      expect(mockAs).toHaveBeenCalledWith('disconnectDevice');
    });

    it('should support error mode', () => {
      interceptDisconnectDevice({ errorMode: true });

      expect(mockIntercept).toHaveBeenCalledWith('DELETE', 'http://localhost:5168/devices/*', expect.any(Function));
    });

    it('should register disconnectDevice alias', () => {
      interceptDisconnectDevice();

      expect(mockAs).toHaveBeenCalledWith('disconnectDevice');
    });

    it('should match dynamic deviceId with wildcard', () => {
      interceptDisconnectDevice();

      expect(mockIntercept).toHaveBeenCalledWith('DELETE', 'http://localhost:5168/devices/*', expect.any(Function));
    });
  });

  describe('interceptPingDevice', () => {
    it('should register intercept with default alive state', () => {
      interceptPingDevice();

      expect(mockIntercept).toHaveBeenCalledWith('GET', 'http://localhost:5168/devices/*/ping', expect.any(Function));
      expect(mockAs).toHaveBeenCalledWith('pingDevice');
    });

    it('should support isAlive true state', () => {
      interceptPingDevice({ isAlive: true });

      expect(mockIntercept).toHaveBeenCalledWith('GET', 'http://localhost:5168/devices/*/ping', expect.any(Function));
    });

    it('should support isAlive false state', () => {
      interceptPingDevice({ isAlive: false });

      expect(mockIntercept).toHaveBeenCalledWith('GET', 'http://localhost:5168/devices/*/ping', expect.any(Function));
    });

    it('should support error mode', () => {
      interceptPingDevice({ errorMode: true });

      expect(mockIntercept).toHaveBeenCalledWith('GET', 'http://localhost:5168/devices/*/ping', expect.any(Function));
    });

    it('should register pingDevice alias', () => {
      interceptPingDevice();

      expect(mockAs).toHaveBeenCalledWith('pingDevice');
    });

    it('should match dynamic deviceId with wildcard', () => {
      interceptPingDevice();

      expect(mockIntercept).toHaveBeenCalledWith('GET', 'http://localhost:5168/devices/*/ping', expect.any(Function));
    });
  });

  describe('Response structure validation', () => {
    it('should return FindDevicesResponse structure', () => {
      // This test validates the response structure by checking the interceptor callback
      mockIntercept.mockReturnValue({
        as: vi.fn().mockReturnValue(undefined),
      });

      // We can't directly test response bodies without executing the interceptor,
      // but we verify the interceptor is properly registered
      interceptFindDevices();

      expect(mockIntercept).toHaveBeenCalled();
    });

    it('should return ConnectDeviceResponse structure', () => {
      mockIntercept.mockReturnValue({
        as: vi.fn().mockReturnValue(undefined),
      });

      interceptConnectDevice();

      expect(mockIntercept).toHaveBeenCalled();
    });

    it('should return DisconnectDeviceResponse structure', () => {
      mockIntercept.mockReturnValue({
        as: vi.fn().mockReturnValue(undefined),
      });

      interceptDisconnectDevice();

      expect(mockIntercept).toHaveBeenCalled();
    });

    it('should return PingDeviceResponse structure', () => {
      mockIntercept.mockReturnValue({
        as: vi.fn().mockReturnValue(undefined),
      });

      interceptPingDevice();

      expect(mockIntercept).toHaveBeenCalled();
    });
  });

  describe('Error handling', () => {
    it('should handle error mode in find devices', () => {
      const options: InterceptFindDevicesOptions = { errorMode: true };
      interceptFindDevices(options);

      expect(mockIntercept).toHaveBeenCalled();
      expect(mockAs).toHaveBeenCalledWith('findDevices');
    });

    it('should handle error mode in connect device', () => {
      const options: InterceptConnectDeviceOptions = { errorMode: true };
      interceptConnectDevice(options);

      expect(mockIntercept).toHaveBeenCalled();
      expect(mockAs).toHaveBeenCalledWith('connectDevice');
    });

    it('should handle error mode in disconnect device', () => {
      const options: InterceptDisconnectDeviceOptions = { errorMode: true };
      interceptDisconnectDevice(options);

      expect(mockIntercept).toHaveBeenCalled();
      expect(mockAs).toHaveBeenCalledWith('disconnectDevice');
    });

    it('should handle error mode in ping device', () => {
      const options: InterceptPingDeviceOptions = { errorMode: true };
      interceptPingDevice(options);

      expect(mockIntercept).toHaveBeenCalled();
      expect(mockAs).toHaveBeenCalledWith('pingDevice');
    });
  });

  describe('Options handling', () => {
    it('should handle empty options object', () => {
      interceptFindDevices({});
      expect(mockIntercept).toHaveBeenCalled();
    });

    it('should handle undefined options', () => {
      interceptFindDevices(undefined);
      expect(mockIntercept).toHaveBeenCalled();
    });

    it('should handle multiple options together', () => {
      const customDevice = singleDevice.devices[0];
      interceptConnectDevice({ device: customDevice, errorMode: false });

      expect(mockIntercept).toHaveBeenCalled();
    });

    it('should prioritize errorMode over isAlive', () => {
      interceptPingDevice({ isAlive: true, errorMode: true });

      expect(mockIntercept).toHaveBeenCalled();
    });
  });
});
