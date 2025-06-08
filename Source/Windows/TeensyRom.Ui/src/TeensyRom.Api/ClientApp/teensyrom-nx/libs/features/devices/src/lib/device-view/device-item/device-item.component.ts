import { Component, input, computed, output, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NgClass } from '@angular/common';
import { Device, DeviceEventsService } from '@teensyrom-nx/domain/device/services';
import { IconLabelComponent } from '@teensyrom-nx/ui/components';
import { StorageStatusComponent as StorageItemComponent } from '../storage-item/storage-item.component';

@Component({
  selector: 'lib-device-item',
  standalone: true,
  imports: [
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    NgClass,
    IconLabelComponent,
    StorageItemComponent,
  ],
  templateUrl: './device-item.component.html',
  styleUrl: './device-item.component.scss',
})
export class DeviceItemComponent {
  device = input<Device>();
  connect = output<string>();
  disconnect = output<string>();
  private readonly deviceEventsService = inject(DeviceEventsService);

  readonly connectionStatus = computed(() => this.device()?.isConnected);
  readonly usbStatus = computed(() => this.device()?.usbStorage?.available);
  readonly sdStatus = computed(() => this.device()?.sdStorage?.available);

  readonly deviceState = computed(() => {
    const id = this.device()?.deviceId;
    if (!id) return this.device()?.deviceState;
    return this.deviceEventsService.getDeviceState(id)();
  });

  onConnect() {
    this.connect.emit(this.device()?.deviceId ?? '');
  }

  onDisconnect() {
    this.disconnect.emit(this.device()?.deviceId ?? '');
  }
}
