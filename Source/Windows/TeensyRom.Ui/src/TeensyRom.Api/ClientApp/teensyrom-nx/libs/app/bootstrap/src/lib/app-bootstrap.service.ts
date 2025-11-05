import {
  effect,
  inject,
  Injectable,
  runInInjectionContext,
  Injector,
  untracked,
} from '@angular/core';
import { DeviceStore } from '@teensyrom-nx/application';
import {
  ALERT_SERVICE,
  DEVICE_EVENTS_SERVICE,
  DEVICE_LOGS_SERVICE,
  IAlertService,
  IDeviceEventsService,
  IDeviceLogsService,
} from '@teensyrom-nx/domain';

@Injectable({ providedIn: 'root' })
export class AppBootstrapService {
  private readonly deviceStore = inject(DeviceStore);
  private readonly deviceLogsService: IDeviceLogsService = inject(DEVICE_LOGS_SERVICE);
  private readonly deviceEventsService: IDeviceEventsService = inject(DEVICE_EVENTS_SERVICE);
  private readonly injector = inject(Injector);
  private readonly alertService: IAlertService = inject(ALERT_SERVICE);

  async init(): Promise<void> {
    return new Promise((resolve) => {
      runInInjectionContext(this.injector, () => {
        const effectRef = effect(() => {
          if (this.deviceStore.hasInitialised()) {
            // Use untracked to prevent the effect from re-running when we resolve
            untracked(() => {
              resolve();
            });
            // Destroy the effect immediately to prevent re-runs
            effectRef.destroy();
          }
        });
      });
      this.deviceLogsService.connect();
      this.deviceEventsService.connect();
      this.deviceStore.findDevices();
    });
  }
}
