import { Component, input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Device } from '@teensyrom-nx/domain';
import { MatCardModule } from '@angular/material/card';
import { FileOtherComponent } from './file-other/file-other.component';
import { FileImageComponent } from './file-image/file-image.component';
import { PlayerToolbarComponent } from './player-toolbar/player-toolbar.component';
import { StorageContainerComponent } from './storage-container/storage-container.component';
import { PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';

@Component({
  selector: 'lib-player-device-container',
  imports: [
    CommonModule,
    MatCardModule,
    FileImageComponent,
    FileOtherComponent,
    PlayerToolbarComponent,
    StorageContainerComponent,
  ],
  templateUrl: './player-device-container.component.html',
  styleUrl: './player-device-container.component.scss',
})
export class PlayerDeviceContainerComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);

  device = input<Device>();

  readonly deviceId = computed(() => this.device()?.deviceId ?? '');

  readonly currentFile = computed(() => {
    const deviceId = this.deviceId();
    if (!deviceId) return null;
    return this.playerContext.getCurrentFile(deviceId)();
  });

  readonly isPlayerLoaded = computed(() => this.currentFile() !== null);
}
