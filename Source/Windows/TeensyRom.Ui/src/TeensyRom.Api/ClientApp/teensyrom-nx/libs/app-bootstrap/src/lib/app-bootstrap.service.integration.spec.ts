import { describe, it, expect, beforeAll, afterEach } from 'vitest';
import { AppBootstrapService } from './app-bootstrap.service';
import { DeviceService } from '@teensyrom-nx/device-services';
import { DevicesApiService, Configuration } from '@teensyrom-nx/api-client';
import { HttpClient, HttpXhrBackend } from '@angular/common/http';
import { firstValueFrom, of, Observable } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { AllDevices } from '@teensyrom-nx/device-services';

describe('AppBootstrapService Integration Tests', () => {
  let appBootstrapService: AppBootstrapService;
  let deviceService: DeviceService;
  let originalFindDevices: () => Observable<AllDevices>;

  beforeAll(() => {
    const httpHandler = new HttpXhrBackend({ build: () => new XMLHttpRequest() });
    const httpClient = new HttpClient(httpHandler);
    const config = new Configuration({ basePath: 'http://localhost:5168' });

    const devicesService = new DevicesApiService(httpClient, config.basePath || '', config);
    deviceService = new DeviceService(devicesService);

    // Store the original findDevices method
    originalFindDevices = deviceService.findDevices.bind(deviceService);

    TestBed.configureTestingModule({
      providers: [AppBootstrapService, { provide: DeviceService, useValue: deviceService }],
    });

    appBootstrapService = TestBed.inject(AppBootstrapService);
  });

  afterEach(async () => {
    try {
      const connected = await getConnectedDevices();
      for (const device of connected) {
        if (device.deviceId) {
          await firstValueFrom(deviceService.disconnectDevice(device.deviceId));
        }
      }
      // Restore original findDevices method after each test
      deviceService.findDevices = originalFindDevices;
    } catch (error) {
      console.warn('Error during cleanup:', error);
    }
  });

  async function getConnectedDevices() {
    return firstValueFrom(deviceService.getConnectedDevices());
  }

  it('should initialize successfully when compatible devices are available', async () => {
    // Act
    await expect(appBootstrapService.init()).resolves.not.toThrow();

    // Assert
    const connectedDevices = await getConnectedDevices();
    //output the connected devices
    console.log('Connected devices:', connectedDevices);

    expect(connectedDevices.length).toBeGreaterThan(0);
  });

  it('should throw error when no devices are available', async () => {
    // Arrange - mock findDevices to return no available devices
    deviceService.findDevices = () =>
      of({
        availableCarts: [],
        connectedCarts: [],
      });

    // Act & Assert
    await expect(appBootstrapService.init()).rejects.toThrow('No available devices found');
  });
});
