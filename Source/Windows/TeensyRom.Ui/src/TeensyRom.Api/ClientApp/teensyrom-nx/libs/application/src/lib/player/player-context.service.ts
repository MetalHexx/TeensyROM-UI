import { Injectable, inject } from '@angular/core';
import { LaunchMode, PlayerFilterType, PlayerScope, FileItemType } from '@teensyrom-nx/domain';
import { PlayerStore, LaunchedFile } from './player-store';
import { StorageStore } from '../storage/storage-store';
import { StorageKeyUtil } from '../storage/storage-key.util';
import { IPlayerContext, LaunchFileContextRequest } from './player-context.interface';


@Injectable({ providedIn: 'root' })
export class PlayerContextService implements IPlayerContext {
  private readonly store = inject(PlayerStore);
  private readonly storageStore = inject(StorageStore);

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

  async launchRandomFile(deviceId: string): Promise<void> {
    this.store.initializePlayer({ deviceId });   
    await this.store.launchRandomFile({ deviceId });
    
    const currentFile = this.store.getCurrentFile(deviceId)();
    if (currentFile) {
      await this.loadDirectoryContextForRandomFile(currentFile);
    }
  }

  private async loadDirectoryContextForRandomFile(currentFile: LaunchedFile): Promise<void> {
    const { storageKey } = currentFile;
    const { deviceId, storageType } = StorageKeyUtil.parse(storageKey);

    try {
      await this.storageStore.navigateToDirectory({ 
        deviceId, 
        storageType, 
        path: currentFile.parentPath 
      });

      const directoryState = this.storageStore.getSelectedDirectoryState(deviceId)();
      
      if (directoryState?.directory?.files) {
        const currentIndex = directoryState.directory.files.findIndex(file => file.path === currentFile.file.path);
        
        if (currentIndex >= 0) {
          this.store.loadFileContext({
            deviceId,
            storageType,
            directoryPath: currentFile.parentPath,
            files: directoryState.directory.files,
            currentFileIndex: currentIndex,
            launchMode: currentFile.launchMode
          });
        }
      }
    } catch {
      // Silently ignore directory loading failures
    }
  }

  toggleShuffleMode(deviceId: string): void {
    const currentMode = this.store.getLaunchMode(deviceId)();
    const newMode = currentMode === LaunchMode.Shuffle ? LaunchMode.Directory : LaunchMode.Shuffle;

    // Update launch mode using the proper action
    this.store.updateLaunchMode({
      deviceId,
      launchMode: newMode,
    });
  }

  setShuffleScope(deviceId: string, scope: PlayerScope): void {
    this.store.updateShuffleSettings({
      deviceId,
      shuffleSettings: { scope },
    });
  }

  setFilterMode(deviceId: string, filter: PlayerFilterType): void {
    this.store.updateShuffleSettings({
      deviceId,
      shuffleSettings: { filter },
    });
  }

  getShuffleSettings(deviceId: string) {
    return this.store.getShuffleSettings(deviceId);
  }

  getLaunchMode(deviceId: string) {
    return this.store.getLaunchMode(deviceId);
  }

  // Phase 3: New playback control methods
  async play(deviceId: string): Promise<void> {
    await this.store.play({ deviceId });
  }

  async pause(deviceId: string): Promise<void> {
    await this.store.pauseMusic({ deviceId });
  }

  async stop(deviceId: string): Promise<void> {
    await this.store.stopPlayback({ deviceId });
  }

  async next(deviceId: string): Promise<void> {
    const launchMode = this.store.getLaunchMode(deviceId)();
    
    // Always call the store action first
    await this.store.navigateNext({ deviceId });
    
    // If in shuffle mode, load directory context for the new random file
    if (launchMode === LaunchMode.Shuffle) {
      const currentFile = this.store.getCurrentFile(deviceId)();
      if (currentFile) {
        await this.loadDirectoryContextForRandomFile(currentFile);
      }
    }
  }

  async previous(deviceId: string): Promise<void> {
    const launchMode = this.store.getLaunchMode(deviceId)();
    
    // Always call the store action first
    await this.store.navigatePrevious({ deviceId });
    
    // If in shuffle mode, load directory context for the new random file
    if (launchMode === LaunchMode.Shuffle) {
      const currentFile = this.store.getCurrentFile(deviceId)();
      if (currentFile) {
        await this.loadDirectoryContextForRandomFile(currentFile);
      }
    }
  }

  getPlayerStatus(deviceId: string) {
    return this.store.getPlayerStatus(deviceId);
  }

  // Helper method to determine if current file is music type
  private isCurrentFileMusicType(deviceId: string): boolean {
    const currentFile = this.getCurrentFile(deviceId)();
    return currentFile?.file?.type === FileItemType.Song;
  }
}
