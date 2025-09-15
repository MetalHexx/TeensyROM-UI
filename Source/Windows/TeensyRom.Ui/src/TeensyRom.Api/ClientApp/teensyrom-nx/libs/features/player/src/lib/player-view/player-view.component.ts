import { Component, inject, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { DeviceStore } from '@teensyrom-nx/domain/device/state';
import { PlayerDeviceContainerComponent } from './player-device-container/player-device-container.component';
import { StorageKeyUtil, StorageStore } from '@teensyrom-nx/domain/storage/state';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { Device } from '@teensyrom-nx/domain/device/services';

@Component({
  selector: 'lib-player-view',
  imports: [CommonModule, MatIconModule, PlayerDeviceContainerComponent],
  templateUrl: './player-view.component.html',
  styleUrl: './player-view.component.scss',
})
export class PlayerViewComponent {
  readonly deviceStore = inject(DeviceStore);
  readonly storageStore = inject(StorageStore);

  readonly connectedDevices = computed(() =>
    this.deviceStore.devices().filter((device) => device.isConnected)
  );

  private syncStorageEffect = effect(() => {
    const devices = this.connectedDevices();
    this.initializeDevicesSequentially(devices);
  });

  private async initializeDevicesSequentially(devices: Device[]) {
    for (const device of devices) {
      if (device.sdStorage?.available) {
        const deviceEntries = this.storageStore.getDeviceStorageEntries(device.deviceId)();
        const sdKey = StorageKeyUtil.create(device.deviceId, StorageType.Sd);

        if (!deviceEntries[sdKey]) {
          await this.storageStore.initializeStorage({
            deviceId: device.deviceId,
            storageType: StorageType.Sd,
          });
        }
      }

      if (device.usbStorage?.available) {
        const deviceEntries = this.storageStore.getDeviceStorageEntries(device.deviceId)();
        const usbKey = StorageKeyUtil.create(device.deviceId, StorageType.Usb);

        if (!deviceEntries[usbKey]) {
          await this.storageStore.initializeStorage({
            deviceId: device.deviceId,
            storageType: StorageType.Usb,
          });
        }
      }
    }
  }
}
