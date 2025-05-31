import { Component, input, computed } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NgClass } from '@angular/common';
import { Device } from '@teensyrom-nx/domain/device/services';
import { IconLabelComponent } from '@teensyrom-nx/ui/components';
import { StorageStatusComponent } from './storage-status/storage-status.component';

@Component({
  selector: 'lib-device-item',
  standalone: true,
  imports: [
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    NgClass,
    IconLabelComponent,
    StorageStatusComponent,
  ],
  templateUrl: './device-item.component.html',
  styleUrl: './device-item.component.scss',
})
export class DeviceItemComponent {
  device = input<Device>();
  isConnected = input<boolean>(false);

  readonly connectionStatus = computed(() => this.isConnected());
  readonly usbStatus = computed(() => this.device()?.usbStorage?.available);
  readonly sdStatus = computed(() => this.device()?.sdStorage?.available);
}
