import { Component, inject, computed, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceStore } from '@teensyrom-nx/application';
import { PlayerDeviceContainerComponent } from './player-device-container/player-device-container.component';
import { StorageKeyUtil, StorageStore, StorageDirectoryState } from '@teensyrom-nx/application';
import { StorageType } from '@teensyrom-nx/domain';
import { Device } from '@teensyrom-nx/domain';
import { EmptyStateMessageComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-player-view',
  imports: [CommonModule, PlayerDeviceContainerComponent, EmptyStateMessageComponent],
  templateUrl: './player-view.component.html',
  styleUrl: './player-view.component.scss',
})
export class PlayerViewComponent {
  readonly deviceStore = inject(DeviceStore);
  readonly storageStore = inject(StorageStore);

  readonly connectedDevices = computed(() =>
    this.deviceStore.devices().filter((device) => device.isConnected)
  );

  // Tracks storage initialization to prevent race conditions
  private readonly initializingStorage = signal<Set<string>>(new Set());

  private syncStorageEffect = effect(() => {
    const devices = this.connectedDevices();
    this.initializeDevicesSequentially(devices);
  });

  private async initializeDevicesSequentially(devices: Device[]) {
    for (const device of devices) {
      const deviceEntries = this.storageStore.getDeviceStorageEntries(device.deviceId)();
      if (device.usbStorage?.available) {
        await this.initializeStorageType(device.deviceId, StorageType.Usb, deviceEntries);
      }
      if (device.sdStorage?.available) {
        await this.initializeStorageType(device.deviceId, StorageType.Sd, deviceEntries);
      }
    }
  }

  private async initializeStorageType(
    deviceId: string,
    storageType: StorageType,
    deviceEntries: Record<string, StorageDirectoryState>
  ): Promise<void> {
    const storageKey = StorageKeyUtil.create(deviceId, storageType);

    if (!deviceEntries[storageKey] && !this.initializingStorage().has(storageKey)) {
      // Mark as being initialized
      this.initializingStorage.update((set) => new Set(set).add(storageKey));

      try {
        await this.storageStore.initializeStorage({
          deviceId,
          storageType,
        });
      } finally {
        // Remove from initialization tracking
        this.initializingStorage.update((set) => {
          const newSet = new Set(set);
          newSet.delete(storageKey);
          return newSet;
        });
      }
    }
  }
}
