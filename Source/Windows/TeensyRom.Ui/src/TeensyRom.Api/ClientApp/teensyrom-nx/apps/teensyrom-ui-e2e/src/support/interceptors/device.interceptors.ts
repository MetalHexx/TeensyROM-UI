/// <reference types="cypress" />

import type {
  CartDto,
  ConnectDeviceResponse,
  DisconnectDeviceResponse,
  FindDevicesResponse,
  PingDeviceResponse,
  ProblemDetails,
} from '@teensyrom-nx/data-access/api-client';
import { singleDevice } from '../test-data/fixtures';
import type { MockDeviceFixture } from '../test-data/fixtures/fixture.types';
import {
  DEVICE_ENDPOINTS,
  INTERCEPT_ALIASES,
} from '../constants';

/**
 * Options for interceptFindDevices interceptor
 */
export interface InterceptFindDevicesOptions {
  /** Override default singleDevice fixture with custom fixture */
  fixture?: MockDeviceFixture;
  /** When true, return HTTP 500 error to simulate API failure */
  errorMode?: boolean;
}

/**
 * Options for interceptConnectDevice interceptor
 */
export interface InterceptConnectDeviceOptions {
  /** Override default device (first device from singleDevice fixture) */
  device?: CartDto;
  /** When true, return HTTP 500 error to simulate connection failure */
  errorMode?: boolean;
}

/**
 * Options for interceptDisconnectDevice interceptor
 */
export interface InterceptDisconnectDeviceOptions {
  /** When true, return HTTP 500 error to simulate disconnection failure */
  errorMode?: boolean;
}

/**
 * Options for interceptPingDevice interceptor
 */
export interface InterceptPingDeviceOptions {
  /** When true (default) device responds as alive. When false, not responding */
  isAlive?: boolean;
  /** When true, return HTTP 500 error to simulate server error */
  errorMode?: boolean;
}

/** Creates a realistic ProblemDetails error response */
function createErrorResponse(): ProblemDetails {
  return {
    type: 'https://api.example.com/errors/internal-error',
    title: 'Internal Server Error',
    status: 500,
    detail: 'Device API call failed',
    instance: '/devices',
  };
}

/**
 * Intercepts GET /devices - Device discovery endpoint
 * Returns a list of discovered TenesyROM devices using the provided or default fixture.
 * 
 * Note: Intercepts requests to http://localhost:5168/devices which may have query params
 */
export function interceptFindDevices(options: InterceptFindDevicesOptions = {}): void {
  const fixture = options.fixture ?? singleDevice;

  cy.intercept(
    DEVICE_ENDPOINTS.FIND_DEVICES.method,
    DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
    (req) => {
      if (options.errorMode) {
        req.reply({
          statusCode: 500,
          body: createErrorResponse(),
        });
      } else {
        const response: FindDevicesResponse = {
          devices: [...fixture.devices],
          message: `Found ${fixture.devices.length} device(s)`,
        };
        req.reply({
          statusCode: 200,
          body: response,
        });
      }
    }
  ).as(INTERCEPT_ALIASES.FIND_DEVICES);
}

/**
 * Intercepts POST /devices/{deviceId}/connect - Device connection endpoint
 * Route matches any deviceId via wildcard: POST http://localhost:5168/devices/<wildcard>/connect
 */
export function interceptConnectDevice(options: InterceptConnectDeviceOptions = {}): void {
  const device = options.device ?? singleDevice.devices[0];

  cy.intercept(
    DEVICE_ENDPOINTS.CONNECT_DEVICE.method,
    DEVICE_ENDPOINTS.CONNECT_DEVICE.pattern,
    (req) => {
      if (options.errorMode) {
        req.reply({
          statusCode: 500,
          body: createErrorResponse(),
        });
      } else {
        const response: ConnectDeviceResponse = {
          connectedCart: device,
          message: `Connected to device ${device.deviceId}`,
        };
        req.reply({
          statusCode: 200,
          body: response,
        });
      }
    }
  ).as(INTERCEPT_ALIASES.CONNECT_DEVICE);
}

/**
 * Intercepts DELETE /devices/{deviceId} - Device disconnection endpoint
 * Route matches any deviceId via wildcard: DELETE http://localhost:5168/devices/<wildcard>
 */
export function interceptDisconnectDevice(options: InterceptDisconnectDeviceOptions = {}): void {
  cy.intercept(
    DEVICE_ENDPOINTS.DISCONNECT_DEVICE.method,
    DEVICE_ENDPOINTS.DISCONNECT_DEVICE.pattern,
    (req) => {
      if (options.errorMode) {
        req.reply({
          statusCode: 500,
          body: createErrorResponse(),
        });
      } else {
        const response: DisconnectDeviceResponse = {
          message: 'Device disconnected successfully',
        };
        req.reply({
          statusCode: 200,
          body: response,
        });
      }
    }
  ).as(INTERCEPT_ALIASES.DISCONNECT_DEVICE);
}

/**
 * Intercepts GET /devices/{deviceId}/ping - Device health check endpoint
 * Route matches any deviceId via wildcard: GET http://localhost:5168/devices/<wildcard>/ping
 */
export function interceptPingDevice(options: InterceptPingDeviceOptions = {}): void {
  const isAlive = options.isAlive ?? true;

  cy.intercept(
    DEVICE_ENDPOINTS.PING_DEVICE.method,
    DEVICE_ENDPOINTS.PING_DEVICE.pattern,
    (req) => {
      if (options.errorMode) {
        req.reply({
          statusCode: 500,
          body: createErrorResponse(),
        });
      } else {
        const response: PingDeviceResponse = {
          message: isAlive ? 'Device is responding' : 'Device is not responding',
        };
        req.reply({
          statusCode: 200,
          body: response,
        });
      }
    }
  ).as(INTERCEPT_ALIASES.PING_DEVICE);
}
