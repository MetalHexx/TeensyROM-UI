import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceStore } from '@teensyrom-nx/domain/device/state';

@Component({
  selector: 'lib-device-view',
  imports: [CommonModule],
  templateUrl: './device-view.component.html',
  styleUrl: './device-view.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DeviceViewComponent {
  private readonly deviceStore = inject(DeviceStore);
  availableDevices = this.deviceStore.availableDevices;
  connectedDevices = this.deviceStore.connectedDevices;
  isLoading = this.deviceStore.isLoading;
}
