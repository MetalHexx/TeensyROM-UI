import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { AppBootstrapService } from './app-bootstrap.service';
import { DeviceService, AllDevices } from '@teensyrom-nx/domain/device/services';
import { DevicesApiService, Configuration } from '@teensyrom-nx/data-access/api-client';
import { HttpClient, HttpXhrBackend } from '@angular/common/http';
import { firstValueFrom, of, Observable } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';

describe('AppBootstrapService Integration Tests', () => {
  let appBootstrapService: AppBootstrapService;
  let deviceService: DeviceService;
  let originalFindDevices: () => Observable<AllDevices>;

  beforeEach(() => {
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
      // Clean up TestBed
      TestBed.resetTestingModule();
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
    const result = await firstValueFrom(deviceService.findDevices());
    expect(result.availableCarts.length).toBeGreaterThan(0);
  });

  it('should throw error when no devices are available', async () => {
    // Arrange - mock findDevices to return no available devices
    deviceService.findDevices = () =>
      of({
        availableCarts: [],
        connectedCarts: [],
      });

    // Spy on MatSnackBar.open
    const snackBar = TestBed.inject(MatSnackBar);
    const openSpy = vi.spyOn(snackBar, 'open');

    // Act
    await appBootstrapService.init();

    // Assert
    expect(openSpy).toHaveBeenCalledWith(
      'Could not find any TeensyROM devices.',
      'Close',
      expect.objectContaining({ duration: 5000 })
    );
  });
});
