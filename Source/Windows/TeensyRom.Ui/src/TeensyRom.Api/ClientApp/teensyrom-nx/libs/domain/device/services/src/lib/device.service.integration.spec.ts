import { describe, it, expect, beforeAll, afterEach } from 'vitest';
import { DeviceService } from './device.service';
import { DevicesApiService, Configuration } from '@teensyrom-nx/data-access/api-client';
import { HttpClient, HttpXhrBackend } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

describe('DeviceService Integration Tests', () => {
  let deviceService: DeviceService;

  beforeAll(() => {
    const httpHandler = new HttpXhrBackend({ build: () => new XMLHttpRequest() });
    const httpClient = new HttpClient(httpHandler);
    const config = new Configuration({ basePath: 'http://localhost:5168' });

    const devicesService = new DevicesApiService(httpClient, config.basePath || '', config);
    deviceService = new DeviceService(devicesService);
  });

  afterEach(async () => {
    try {
      const connected = await getConnectedDevices();
      for (const device of connected) {
        if (device.deviceId) {
          await deviceService.disconnectDevice(device.deviceId).toPromise();
        }
      }
    } catch (error) {
      console.warn('Error during cleanup:', error);
    }
  });

  async function getConnectedDevices() {
    return firstValueFrom(deviceService.getConnectedDevices());
  }

  async function getAvailableDevice() {
    const result = await firstValueFrom(deviceService.findDevices());

    return result?.availableCarts?.[0];
  }

  it('should find available and connected devices', async () => {
    //Act
    const result = await firstValueFrom(deviceService.findDevices());

    //Assert
    expect(result).toBeDefined();
    expect(Array.isArray(result?.availableCarts)).toBe(true);
    expect(Array.isArray(result?.connectedCarts)).toBe(true);
  });

  it('should connect to a device', async () => {
    //Arrange
    const expectedDevice = await getAvailableDevice();
    //Act
    const result = await deviceService.connectDevice(expectedDevice.deviceId).toPromise();

    //Asert
    expect(result).toBeDefined();
    expect(result?.deviceId).toBe(expectedDevice.deviceId);
  });

  it('should disconnect from a connected device', async () => {
    //Arrange
    const expectedDevice = await getAvailableDevice();

    await deviceService.connectDevice(expectedDevice.deviceId).toPromise();

    //Act
    const result = await deviceService.disconnectDevice(expectedDevice.deviceId).toPromise();

    //Assert
    expect(result).toBeDefined();
    expect(result?.message).toBeDefined();
  });
});
