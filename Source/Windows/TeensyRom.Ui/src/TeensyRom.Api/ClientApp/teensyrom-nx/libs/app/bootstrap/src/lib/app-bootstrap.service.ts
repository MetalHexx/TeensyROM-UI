import { effect, inject, Injectable, runInInjectionContext, Injector } from '@angular/core';
import { DeviceStore } from '@teensyrom-nx/device-store';

@Injectable({
  providedIn: 'root',
})
export class AppBootstrapService {
  private readonly deviceStore = inject(DeviceStore);
  private readonly injector = inject(Injector);

  async init(): Promise<void> {
    return new Promise((resolve, reject) => {
      runInInjectionContext(this.injector, () => {
        effect(() => {
          if (this.deviceStore.hasInitialised()) {
            if (this.deviceStore.availableDevices().length === 0) {
              reject(new Error('No available devices found'));
            } else {
              resolve();
            }
          }
        });
      });

      this.deviceStore.findDevices({});
    });
  }
}
