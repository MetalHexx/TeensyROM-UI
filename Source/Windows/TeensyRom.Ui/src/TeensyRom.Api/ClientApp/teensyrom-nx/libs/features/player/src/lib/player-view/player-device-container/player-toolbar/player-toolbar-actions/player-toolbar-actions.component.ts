import { Component, inject, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { IconButtonComponent } from '@teensyrom-nx/ui/components';
import { PLAYER_CONTEXT, StorageStore } from '@teensyrom-nx/application';
import { LaunchMode } from '@teensyrom-nx/domain';
import { StorageKeyUtil } from '@teensyrom-nx/application';

@Component({
  selector: 'lib-player-toolbar-actions',
  imports: [CommonModule, MatIconModule, IconButtonComponent],
  templateUrl: './player-toolbar-actions.component.html',
  styleUrl: './player-toolbar-actions.component.scss',
})
export class PlayerToolbarActionsComponent {
  private readonly playerContext = inject(PLAYER_CONTEXT);
  private readonly storageStore = inject(StorageStore);

  deviceId = input.required<string>();

  currentFile = computed(() => {
    const deviceId = this.deviceId();
    if (!deviceId) return null;
    // Invoke the inner signal to subscribe to store changes
    return this.playerContext.getCurrentFile(deviceId)();
  });

  toggleShuffleMode(): void {
    const deviceId = this.deviceId();
    if (deviceId) {
      this.playerContext.toggleShuffleMode(deviceId);
    }
  }

  isShuffleMode(): boolean {
    const deviceId = this.deviceId();
    if (!deviceId) return false;

    return this.playerContext.getLaunchMode(deviceId)() === LaunchMode.Shuffle;
  }

  async toggleFavorite(): Promise<void> {
    if (this.isFavoriteOperationInProgress()) {
      return;
    }

    const launchedFile = this.currentFile();
    if (!launchedFile) {
      return;
    }

    const file = launchedFile.file;
    const { deviceId, storageType } = StorageKeyUtil.parse(launchedFile.storageKey);
    const isCurrentlyFavorite = file?.isFavorite ?? false;
    const filePath = file.path;
    const newFavoriteStatus = !isCurrentlyFavorite;

    // PESSIMISTIC UPDATE: Wait for storage operation, then update player store
    if (isCurrentlyFavorite) {
      await this.storageStore.removeFavorite({
        deviceId,
        storageType,
        filePath,
      });
    } else {
      await this.storageStore.saveFavorite({
        deviceId,
        storageType,
        filePath,
      });
    }

    const favoriteState = this.storageStore.favoriteOperationsState();

    if (favoriteState.isProcessing || favoriteState.error) {
      return;
    }

    this.playerContext.updateCurrentFileFavoriteStatus(deviceId, filePath, newFavoriteStatus);
  }

  isFavorite(): boolean {
    const launchedFile = this.currentFile();
    return launchedFile?.file?.isFavorite ?? false;
  }

  isFavoriteOperationInProgress(): boolean {
    return this.storageStore.favoriteOperationsState().isProcessing;
  }
}
