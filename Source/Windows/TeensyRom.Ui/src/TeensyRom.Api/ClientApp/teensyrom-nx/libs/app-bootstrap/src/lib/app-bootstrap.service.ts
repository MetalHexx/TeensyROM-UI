import { inject, Injectable } from '@angular/core';
import { DeviceService } from '@teensyrom-nx/device-services';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AppBootstrapService {
  private readonly deviceService = inject(DeviceService);

  async init(): Promise<void> {
    const allDevices = await firstValueFrom(this.deviceService.findDevices());

    if (allDevices.availableCarts.length === 0) {
      throw new Error('No available devices found');
    }

    // Optionally auto-connect logic here:
    for (const device of allDevices.availableCarts) {
      if (device.isCompatible) {
        await firstValueFrom(this.deviceService.connectDevice(device.deviceId));
      }
    }

    // Extend this later to load settings
    // or initialize feature-specific state
  }
}
