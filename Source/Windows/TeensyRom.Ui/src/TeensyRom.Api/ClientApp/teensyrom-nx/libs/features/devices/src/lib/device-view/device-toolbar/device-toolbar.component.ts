import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { ScalingCompactCardComponent, ActionButtonComponent } from '@teensyrom-nx/ui/components';
import { DeviceStore } from '@teensyrom-nx/application';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'lib-device-toolbar',
  imports: [
    CommonModule,
    MatCardModule,
    MatExpansionModule,
    MatIconModule,
    ScalingCompactCardComponent,
    ActionButtonComponent,
    MatButtonModule,
  ],
  templateUrl: './device-toolbar.component.html',
  styleUrl: './device-toolbar.component.scss',
})
export class DeviceToolbarComponent {
  private readonly deviceStore = inject(DeviceStore);

  onIndexAllStorage() {
    this.deviceStore.indexStorageAllStorage();
  }

  onRefreshDevices() {
    this.deviceStore.findDevices();
  }

  onResetDevices() {
    this.deviceStore.resetAllDevices();
  }

  onPingDevices() {
    this.deviceStore.pingAllDevices();
  }
}
