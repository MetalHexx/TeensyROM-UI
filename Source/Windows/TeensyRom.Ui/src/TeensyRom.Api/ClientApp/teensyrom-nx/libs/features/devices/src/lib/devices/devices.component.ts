import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceStore } from '@teensyrom-nx/device-store';

@Component({
  selector: 'lib-devices',
  imports: [CommonModule],
  providers: [DeviceStore],
  templateUrl: './devices.component.html',
  styleUrl: './devices.component.css',
})
export class DevicesComponent {
  private readonly deviceStore = inject(DeviceStore);
  devices = this.deviceStore.devices;
}
