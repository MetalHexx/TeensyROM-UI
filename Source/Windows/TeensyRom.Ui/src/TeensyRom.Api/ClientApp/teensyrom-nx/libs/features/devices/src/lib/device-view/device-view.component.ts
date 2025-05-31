import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceStore } from '@teensyrom-nx/domain/device/state';
import { DeviceItemComponent } from './device-item/device-item.component';

@Component({
  selector: 'lib-device-view',
  imports: [CommonModule, DeviceItemComponent],
  templateUrl: './device-view.component.html',
  styleUrl: './device-view.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DeviceViewComponent {
  private readonly deviceStore = inject(DeviceStore);
  availableDevices = this.deviceStore.availableDevices;
  connectedDevices = this.deviceStore.connectedDevices;
  isLoading = this.deviceStore.isLoading;
}
