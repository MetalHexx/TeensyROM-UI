import { effect, inject, Injectable, runInInjectionContext, Injector } from '@angular/core';
import { DeviceStore } from '@teensyrom-nx/application';
import { MatSnackBar } from '@angular/material/snack-bar';
import {
  DEVICE_EVENTS_SERVICE,
  DEVICE_LOGS_SERVICE,
  IDeviceEventsService,
  IDeviceLogsService,
} from '@teensyrom-nx/domain';

@Injectable({ providedIn: 'root' })
export class AppBootstrapService {
  private readonly deviceStore = inject(DeviceStore);
  private readonly deviceLogsService: IDeviceLogsService = inject(DEVICE_LOGS_SERVICE);
  private readonly deviceEventsService: IDeviceEventsService = inject(DEVICE_EVENTS_SERVICE);
  private readonly injector = inject(Injector);
  private readonly snackBar = inject(MatSnackBar);

  async init(): Promise<void> {
    return new Promise((resolve) => {
      runInInjectionContext(this.injector, () => {
        const effectRef = effect(() => {
          if (this.deviceStore.hasInitialised()) {
            if (this.deviceStore.error() === null) {
              resolve();
            } else {
              this.snackBar.open('Could not find any TeensyROM devices.', 'Close', {
                duration: 5000,
              });
              resolve();
            }
            Promise.resolve().then(() => effectRef.destroy());
          }
        });
      });
      this.deviceLogsService.connect();
      this.deviceEventsService.connect();
      this.deviceStore.findDevices();
    });
  }
}
