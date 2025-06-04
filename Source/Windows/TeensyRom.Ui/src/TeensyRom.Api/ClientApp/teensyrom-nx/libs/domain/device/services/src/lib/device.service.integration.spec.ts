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
  }, 30000);

  async function getConnectedDevices() {
    return firstValueFrom(deviceService.getConnectedDevices());
  }

  async function getConnectedDevice() {
    const result = await firstValueFrom(deviceService.findDevices(true));

    return result?.[0];
  }

  async function getDisconnectedDevice() {
    const result = await firstValueFrom(deviceService.findDevices(false));

    return result?.[0];
  }

  it('should find available and connected devices', async () => {
    //Act
    const devices = await firstValueFrom(deviceService.findDevices(true));

    //Assert
    expect(devices).toBeDefined();
    expect(Array.isArray(devices)).toBe(true);
  }, 40000);

  it('should connect to a device', async () => {
    //Arrange
    const expectedDevice = await getDisconnectedDevice();
    //Act
    const device = await deviceService.connectDevice(expectedDevice.deviceId).toPromise();

    //Asert
    expect(device).toBeDefined();
    expect(device?.deviceId).toBe(expectedDevice.deviceId);
  }, 40000); // 40 seconds

  it('should disconnect from a connected device', async () => {
    //Arrange
    const expectedDevice = await getConnectedDevice();

    await deviceService.connectDevice(expectedDevice.deviceId).toPromise();

    //Act
    const result = await deviceService.disconnectDevice(expectedDevice.deviceId).toPromise();

    //Assert
    expect(result).toBeDefined();
    expect(result?.message).toBeDefined();
  }, 40000);
});
