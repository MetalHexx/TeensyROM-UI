import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceStore } from '@teensyrom-nx/domain/device/state';
import { DeviceItemComponent } from './device-item/device-item.component';
import { DeviceLogsComponent } from './device-logs/device-logs.component';
import { DeviceToolbarComponent } from '../device-toolbar/device-toolbar.component';

@Component({
  selector: 'lib-device-view',
  imports: [CommonModule, DeviceItemComponent, DeviceLogsComponent, DeviceToolbarComponent],
  templateUrl: './device-view.component.html',
  styleUrl: './device-view.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DeviceViewComponent {
  private readonly deviceStore = inject(DeviceStore);
  devices = this.deviceStore.devices;
  isLoading = this.deviceStore.isLoading;

  onConnect(deviceId: string) {
    this.deviceStore.connectDevice(deviceId);
  }

  onDisconnect(deviceId: string) {
    this.deviceStore.disconnectDevice(deviceId);
  }
}
