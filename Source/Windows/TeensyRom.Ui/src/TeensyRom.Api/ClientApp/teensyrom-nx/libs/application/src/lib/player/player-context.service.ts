import { Injectable, inject } from '@angular/core';
import { LaunchMode } from '@teensyrom-nx/domain';
import { PlayerStore } from './player-store';
import { IPlayerContext, LaunchFileContextRequest } from './player-context.interface';

@Injectable({ providedIn: 'root' })
export class PlayerContextService implements IPlayerContext {
  private readonly store = inject(PlayerStore);

  initializePlayer(deviceId: string): void {
    this.store.initializePlayer({ deviceId });
  }

  removePlayer(deviceId: string): void {
    this.store.removePlayer({ deviceId });
  }

  async launchFileWithContext(request: LaunchFileContextRequest): Promise<void> {
    const launchMode = request.launchMode ?? LaunchMode.Directory;
    const directoryPath = request.directoryPath ?? '/';
    const files = [...request.files];

    this.store.initializePlayer({ deviceId: request.deviceId });

    await this.store.launchFileWithContext({
      deviceId: request.deviceId,
      storageType: request.storageType,
      file: request.file,
      directoryPath,
      files,
      launchMode,
    });
  }

  getCurrentFile(deviceId: string) {
    return this.store.getCurrentFile(deviceId);
  }

  getFileContext(deviceId: string) {
    return this.store.getPlayerFileContext(deviceId);
  }

  isLoading(deviceId: string) {
    return this.store.isPlayerLoading(deviceId);
  }

  getError(deviceId: string) {
    return this.store.getPlayerError(deviceId);
  }

  getStatus(deviceId: string) {
    return this.store.getPlayerStatus(deviceId);
  }
}
