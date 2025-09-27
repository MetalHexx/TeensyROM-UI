import { Component, input, computed, output, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NgClass } from '@angular/common';
import {
  Device,
  DEVICE_EVENTS_SERVICE,
  IDeviceEventsService,
  StorageType,
} from '@teensyrom-nx/domain';
import { TeensyStorageType } from '@teensyrom-nx/data-access/api-client';
import {
  IconLabelComponent,
  IconButtonComponent,
  CardLayoutComponent,
} from '@teensyrom-nx/ui/components';
import { StorageStatusComponent as StorageItemComponent } from '../storage-item/storage-item.component';
import { DeviceState } from '@teensyrom-nx/domain';
import { DeviceStore } from '@teensyrom-nx/application';

@Component({
  selector: 'lib-device-item',
  standalone: true,
  imports: [
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    NgClass,
    IconLabelComponent,
    IconButtonComponent,
    CardLayoutComponent,
    StorageItemComponent,
  ],
  templateUrl: './device-item.component.html',
  styleUrl: './device-item.component.scss',
})
export class DeviceItemComponent {
  device = input<Device>();
  connect = output<string>();
  disconnect = output<string>();
  private readonly deviceEventsService: IDeviceEventsService = inject(DEVICE_EVENTS_SERVICE);
  private readonly deviceStore = inject(DeviceStore);

  readonly connectionStatus = computed(() => this.device()?.isConnected);
  readonly usbStatus = computed(() => this.device()?.usbStorage?.available);
  readonly sdStatus = computed(() => this.device()?.sdStorage?.available);
  readonly TeensyStorageType = TeensyStorageType;

  readonly deviceState = computed(() => {
    const id = this.device()?.deviceId;

    if (!id) return DeviceState.Unknown;

    return this.deviceEventsService.getDeviceState(id)();
  });

  onConnect() {
    this.connect.emit(this.device()?.deviceId ?? '');
  }

  onDisconnect() {
    this.disconnect.emit(this.device()?.deviceId ?? '');
  }

  async onIndex(storageType: TeensyStorageType) {
    const deviceId = this.device()?.deviceId ?? '';
    // Convert API type to domain type
    const domainStorageType = storageType === TeensyStorageType.Sd ? StorageType.Sd : StorageType.Usb;
    await this.deviceStore.indexStorage(deviceId, domainStorageType);
  }
}
