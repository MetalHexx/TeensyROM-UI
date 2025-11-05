import { describe, it, expect, beforeAll, afterEach } from 'vitest';
import { DeviceService } from './device.service';
import { DevicesApiService, Configuration } from '@teensyrom-nx/data-access/api-client';
import { firstValueFrom } from 'rxjs';

// Gate integration tests behind env variable to avoid external dependency by default
const run = process.env.RUN_INTEGRATION === 'true' ? describe : describe.skip;

run('DeviceService Integration Tests', () => {
  let deviceService: DeviceService;

  beforeAll(() => {
    const config = new Configuration({
      basePath: 'http://localhost:5168',
      fetchApi: fetch,
    });

    const devicesService = new DevicesApiService(config);
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
    const devices = await firstValueFrom(deviceService.findDevices(true));
    expect(devices).toBeDefined();
    expect(Array.isArray(devices)).toBe(true);
  }, 40000);

  it('should connect to a device', async () => {
    const expectedDevice = await getDisconnectedDevice();
    const device = await deviceService.connectDevice(expectedDevice.deviceId).toPromise();
    expect(device).toBeDefined();
    expect(device?.deviceId).toBe(expectedDevice.deviceId);
  }, 40000);

  it('should disconnect from a connected device', async () => {
    const expectedDevice = await getConnectedDevice();
    await deviceService.connectDevice(expectedDevice.deviceId).toPromise();
    const result = await deviceService.disconnectDevice(expectedDevice.deviceId).toPromise();
    expect(result).toBeDefined();
    expect(result?.message).toBeDefined();
  }, 40000);
});
