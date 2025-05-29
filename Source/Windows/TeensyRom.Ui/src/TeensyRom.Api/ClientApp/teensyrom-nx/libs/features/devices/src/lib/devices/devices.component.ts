import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceStore } from '@teensyrom-nx/device-store';

@Component({
  selector: 'lib-devices',
  imports: [CommonModule],
  templateUrl: './devices.component.html',
  styleUrl: './devices.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DevicesComponent {
  private readonly deviceStore = inject(DeviceStore);
  availableDevices = this.deviceStore.availableDevices;
  connectedDevices = this.deviceStore.connectedDevices;
  isLoading = this.deviceStore.isLoading;
}
