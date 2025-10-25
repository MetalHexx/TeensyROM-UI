import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceStore } from '@teensyrom-nx/application';
import { PlayerDeviceContainerComponent } from './player-device-container/player-device-container.component';
import { EmptyStateMessageComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-player-view',
  imports: [CommonModule, PlayerDeviceContainerComponent, EmptyStateMessageComponent],
  templateUrl: './player-view.component.html',
  styleUrl: './player-view.component.scss',
})
export class PlayerViewComponent {
  readonly deviceStore = inject(DeviceStore);

  readonly connectedDevices = computed(() =>
    this.deviceStore.devices().filter((device) => device.isConnected)
  );
}
