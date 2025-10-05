import { Injectable, inject } from '@angular/core';
import { LaunchMode, PlayerFilterType, PlayerScope, FileItemType, FileItem } from '@teensyrom-nx/domain';
import { PlayerStore, LaunchedFile } from './player-store';
import { StorageStore } from '../storage/storage-store';
import { StorageKeyUtil } from '../storage/storage-key.util';
import { IPlayerContext, LaunchFileContextRequest } from './player-context.interface';
import { PlayerTimerManager } from './player-timer-manager';
import { parsePlayLength } from './timer-utils';
import { logInfo, logWarn, LogType } from '@teensyrom-nx/utils';
import { Subscription } from 'rxjs';


@Injectable({ providedIn: 'root' })
export class PlayerContextService implements IPlayerContext {
  private readonly store = inject(PlayerStore);
  private readonly storageStore = inject(StorageStore);
  private readonly timerManager = inject(PlayerTimerManager);
  
  // Track timer subscriptions per device for cleanup
  private readonly timerSubscriptions = new Map<string, Subscription[]>();

  initializePlayer(deviceId: string): void {
    this.store.initializePlayer({ deviceId });
  }

  removePlayer(deviceId: string): void {
    // Phase 5: Cleanup timer subscriptions before removing player
    this.cleanupTimerSubscriptions(deviceId);
    
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

    // Phase 5: Only setup timer for music files if launch was successful
    // Note: We don't return early on error - the store action already set currentFile 
    // and fileContext so the UI can show which file failed
    if (!this.hasErrorAndCleanup(request.deviceId)) {
      this.setupTimerForFile(request.deviceId, request.file);
    }
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
      
      // Phase 5: Only setup timer for music files if launch was successful
      // Note: We still load directory context even on error so UI can show failed file
      if (!this.hasErrorAndCleanup(deviceId)) {
        this.setupTimerForFile(deviceId, currentFile.file);
      }
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
    // Phase 5: Do not allow play if current file is incompatible
    if (!this.store.isCurrentFileCompatible(deviceId)()) {
      logWarn(`Cannot play incompatible file on device ${deviceId}`);
      return;
    }

    await this.store.play({ deviceId });
    
    // Phase 5: Resume timer for music files
    if (this.isCurrentFileMusicType(deviceId)) {
      this.timerManager.resumeTimer(deviceId);
    }
  }

  async pause(deviceId: string): Promise<void> {
    // Phase 5: Do not allow pause if current file is incompatible
    if (!this.store.isCurrentFileCompatible(deviceId)()) {
      logWarn(`Cannot pause incompatible file on device ${deviceId}`);
      return;
    }

    await this.store.pauseMusic({ deviceId });
    
    // Phase 5: Pause timer for music files
    if (this.isCurrentFileMusicType(deviceId)) {
      this.timerManager.pauseTimer(deviceId);
    }
  }

  async stop(deviceId: string): Promise<void> {
    await this.store.stopPlayback({ deviceId });
    
    // Phase 5: Stop timer
    this.timerManager.stopTimer(deviceId);
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
    
    // Phase 5: Only setup timer for the new file if navigation was successful
    // Note: We still load directory context even on error so UI can show failed file
    const currentFile = this.store.getCurrentFile(deviceId)();
    if (currentFile && !this.hasErrorAndCleanup(deviceId)) {
      this.setupTimerForFile(deviceId, currentFile.file);
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
    
    // Phase 5: Only setup timer for the new file if navigation was successful
    // Note: We still load directory context even on error so UI can show failed file
    const currentFile = this.store.getCurrentFile(deviceId)();
    if (currentFile && !this.hasErrorAndCleanup(deviceId)) {
      this.setupTimerForFile(deviceId, currentFile.file);
    }
  }

  getPlayerStatus(deviceId: string) {
    return this.store.getPlayerStatus(deviceId);
  }

  isCurrentFileCompatible(deviceId: string) {
    return this.store.isCurrentFileCompatible(deviceId);
  }

  // Helper method to determine if current file is music type
  private isCurrentFileMusicType(deviceId: string): boolean {
    const currentFile = this.getCurrentFile(deviceId)();
    return currentFile?.file?.type === FileItemType.Song;
  }

  /**
   * Check if operation resulted in error state and cleanup timer if so.
   * Returns true if error exists (indicating operation should abort).
   */
  private hasErrorAndCleanup(deviceId: string): boolean {
    const error = this.store.getPlayerError(deviceId)();
    if (error) {
      // Operation failed - cleanup any existing timer
      this.cleanupTimerSubscriptions(deviceId);
      return true;
    }
    return false;
  }

  /**
   * Phase 5: Setup timer for music file
   * Creates timer, subscribes to updates and completion events
   */
  private setupTimerForFile(deviceId: string, file: FileItem): void {
    // Only setup timer for music files
    if (file.type !== FileItemType.Song) {
      logInfo(LogType.Info, `Skipping timer setup for non-music file: ${file.name}`);
      // Cleanup any existing timer when switching to non-music file
      this.cleanupTimerSubscriptions(deviceId);
      this.timerManager.destroyTimer(deviceId);
      return;
    }

    // Parse play length
    let totalTime = parsePlayLength(file.playLength ?? '');
    
    // If parsing failed or returned 0, use default 3-minute timer and log warning
    if (totalTime === 0) {
      const DEFAULT_TIMER_MS = 180000; // 3 minutes
      totalTime = DEFAULT_TIMER_MS;
      
      if (!file.playLength || file.playLength.trim() === '') {
        logWarn(`Music file ${file.name} has empty playLength. Using default 3-minute timer. Backend should provide playLength.`);
      } else {
        logWarn(`Music file ${file.name} has invalid playLength format: "${file.playLength}". Using default 3-minute timer. Backend should provide valid playLength.`);
      }
    }

    logInfo(LogType.Start, `Setting up timer for ${file.name} (${totalTime}ms) on device ${deviceId}`);

    // Cleanup existing timer subscriptions
    this.cleanupTimerSubscriptions(deviceId);

    // Create timer
    this.timerManager.createTimer(deviceId, totalTime);

    // Subscribe to timer updates
    const updateSub = this.timerManager.onTimerUpdate$(deviceId).subscribe((timerState) => {
      this.store.updateTimerState({ deviceId, timerState });
    });

    // Subscribe to timer completion for auto-progression
    const completeSub = this.timerManager.onTimerComplete$(deviceId).subscribe(() => {
      logInfo(LogType.Success, `Timer completed for device ${deviceId}, auto-progressing to next file`);
      void this.next(deviceId);
    });

    // Store subscriptions for cleanup
    this.timerSubscriptions.set(deviceId, [updateSub, completeSub]);

    logInfo(LogType.Success, `Timer setup complete for device ${deviceId}`);
  }

  /**
   * Phase 5: Cleanup timer subscriptions and destroy timer
   */
  private cleanupTimerSubscriptions(deviceId: string): void {
    const subs = this.timerSubscriptions.get(deviceId);
    if (subs) {
      subs.forEach(sub => sub.unsubscribe());
      this.timerSubscriptions.delete(deviceId);
    }
    this.timerManager.destroyTimer(deviceId);
    this.store.updateTimerState({ deviceId, timerState: null });
  }

  /**
   * Phase 5: Get timer state for a device
   */
  getTimerState(deviceId: string) {
    return this.store.getTimerState(deviceId);
  }
}
