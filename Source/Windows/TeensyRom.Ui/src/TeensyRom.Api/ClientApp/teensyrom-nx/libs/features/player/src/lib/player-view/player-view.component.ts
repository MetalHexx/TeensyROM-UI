import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeviceStore } from '@teensyrom-nx/domain/device/state';
import { PlayerDeviceContainerComponent } from './player-device-container/player-device-container.component';

@Component({
  selector: 'lib-player-view',
  imports: [CommonModule, PlayerDeviceContainerComponent],
  templateUrl: './player-view.component.html',
  styleUrl: './player-view.component.scss',
})
export class PlayerViewComponent {
  readonly deviceStore = inject(DeviceStore);
}
