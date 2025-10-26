import { Injectable, inject, Signal } from '@angular/core';
import { Location } from '@angular/common';
import { LaunchMode, PlayerFilterType, PlayerScope, FileItemType, FileItem, StorageTypeUtil } from '@teensyrom-nx/domain';
import { PlayerStore, LaunchedFile, HistoryEntry } from './player-store';
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
  private readonly location = inject(Location);

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

    // Hide history view when launching new files (directory clicks, search clicks)
    this.store.updateHistoryViewVisibility({ deviceId: request.deviceId, visible: false });

    await this.store.launchFileWithContext({
      deviceId: request.deviceId,
      storageType: request.storageType,
      file: request.file,
      directoryPath,
      files,
      launchMode,
    });

    // Only setup timer for music files if launch was successful
    // Note: We don't return early on error - the store action already set currentFile
    // and fileContext so the UI can show which file failed
    if (!this.hasErrorAndCleanup(request.deviceId)) {
      this.recordHistoryIfSuccessful(request.deviceId);
      this.setupTimerForFile(request.deviceId, request.file);
      this.updateUrlForLaunchedFile(request.deviceId);
    }
  }

  updateCurrentFileFavoriteStatus(deviceId: string, filePath: string, isFavorite: boolean): void {
    this.store.updateCurrentFileFavoriteStatus({ deviceId, filePath, isFavorite });
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

      // Only setup timer for music files if launch was successful
      // Note: We still load directory context even on error so UI can show failed file
      if (!this.hasErrorAndCleanup(deviceId)) {
        this.recordHistoryIfSuccessful(deviceId);
        this.setupTimerForFile(deviceId, currentFile.file);
        this.updateUrlForLaunchedFile(deviceId);
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
            launchMode: this.store.getLaunchMode(deviceId)()
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

    // If in shuffle mode AND forward history is available, use history navigation
    if (launchMode === LaunchMode.Shuffle && this.store.canNavigateForwardInHistory(deviceId)()) {
      logInfo(LogType.Info, `Next: Using forward history navigation for shuffle mode on device ${deviceId}`);

      // Navigate forward through history
      await this.store.navigateForwardInHistory({ deviceId });

      // Load directory context for the history file
      const currentFile = this.store.getCurrentFile(deviceId)();
      if (currentFile) {
        await this.loadDirectoryContextForRandomFile(currentFile);
      }

      // Setup timer if navigation was successful (no error)
      if (currentFile && !this.hasErrorAndCleanup(deviceId)) {
        this.setupTimerForFile(deviceId, currentFile.file);
        this.updateUrlForLaunchedFile(deviceId);
        // Important: DO NOT record history when navigating through existing history
      }

      return; // Early exit - don't fall through to default behavior
    }

    // Default behavior: Launch new random file (shuffle) or next in directory
    // Always call the store action first
    await this.store.navigateNext({ deviceId });

    // If in shuffle mode, load directory context for the new random file
    if (launchMode === LaunchMode.Shuffle) {
      const currentFile = this.store.getCurrentFile(deviceId)();
      if (currentFile) {
        await this.loadDirectoryContextForRandomFile(currentFile);
      }
    }

    // Only setup timer for the new file if navigation was successful
    // Note: We still load directory context even on error so UI can show failed file
    const currentFile = this.store.getCurrentFile(deviceId)();
    if (currentFile && !this.hasErrorAndCleanup(deviceId)) {
      this.recordHistoryIfSuccessful(deviceId);
      this.setupTimerForFile(deviceId, currentFile.file);
      this.updateUrlForLaunchedFile(deviceId);
    }
  }

  async previous(deviceId: string): Promise<void> {
    const launchMode = this.store.getLaunchMode(deviceId)();

    // If in shuffle mode AND history navigation is available, use history navigation
    if (launchMode === LaunchMode.Shuffle && this.store.canNavigateBackwardInHistory(deviceId)()) {
      logInfo(LogType.Info, `Previous: Using history navigation for shuffle mode on device ${deviceId}`);

      // Navigate backward through history
      await this.store.navigateBackwardInHistory({ deviceId });

      // Load directory context for the history file
      const currentFile = this.store.getCurrentFile(deviceId)();
      if (currentFile) {
        await this.loadDirectoryContextForRandomFile(currentFile);
      }

      // Setup timer if successful - DO NOT record history (navigating existing entries)
      if (currentFile && !this.hasErrorAndCleanup(deviceId)) {
        this.setupTimerForFile(deviceId, currentFile.file);
        this.updateUrlForLaunchedFile(deviceId);
      }

      return; // Exit early - don't continue to default behavior
    }

    // Default behavior: call store action (directory/search mode OR shuffle with no history)
    await this.store.navigatePrevious({ deviceId });

    // If in shuffle mode, load directory context for the new random file
    if (launchMode === LaunchMode.Shuffle) {
      const currentFile = this.store.getCurrentFile(deviceId)();
      if (currentFile) {
        await this.loadDirectoryContextForRandomFile(currentFile);
      }
    }

    // Only setup timer for the new file if navigation was successful
    // Note: We still load directory context even on error so UI can show failed file
    const currentFile = this.store.getCurrentFile(deviceId)();
    if (currentFile && !this.hasErrorAndCleanup(deviceId)) {
      this.recordHistoryIfSuccessful(deviceId); // Record history for new file launches
      this.setupTimerForFile(deviceId, currentFile.file);
      this.updateUrlForLaunchedFile(deviceId);
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
   * Get timer state for a device
   */
  getTimerState(deviceId: string) {
    return this.store.getTimerState(deviceId);
  }

  /**
   * Update browser URL when file is launched
   * Extracts current file info and updates the URL with device, storage, path, and file query parameters
   * Forward slashes in path parameters are preserved for cleaner, more readable URLs
   */
  private updateUrlForLaunchedFile(deviceId: string): void {
    const currentFile = this.store.getCurrentFile(deviceId)();
    if (!currentFile) return;

    const { storageType } = StorageKeyUtil.parse(currentFile.storageKey);

    // Build query string manually to preserve forward slashes in path
    // This avoids encoding slashes (%2F) for cleaner, more readable URLs
    const queryString = [
      `device=${encodeURIComponent(deviceId)}`,
      `storage=${encodeURIComponent(StorageTypeUtil.toString(storageType))}`,
      `path=${currentFile.parentPath}`,
      `file=${encodeURIComponent(currentFile.file.name)}`,
    ].join('&');

    // Update URL without triggering route navigation
    this.location.go(`/player?${queryString}`);
  }

  /**
   * Record history if file launch was successful
   */
  private recordHistoryIfSuccessful(deviceId: string): void {
    const currentFile = this.store.getCurrentFile(deviceId)();

    if (!currentFile || !currentFile.isCompatible) {
      return;
    }

    const entry: HistoryEntry = {
      file: currentFile.file,
      storageKey: currentFile.storageKey,
      parentPath: currentFile.parentPath,
      timestamp: currentFile.launchedAt,
      isCompatible: currentFile.isCompatible,
    };

    this.store.recordHistory({ deviceId, entry });
  }

  /**
   * Phase 1: Get play history for a device
   */
  getPlayHistory(deviceId: string) {
    return this.store.getPlayHistory(deviceId);
  }

  /**
   * Phase 1: Get current history position for a device
   */
  getCurrentHistoryPosition(deviceId: string) {
    return this.store.getCurrentHistoryPosition(deviceId);
  }

  /**
   * Phase 1: Check if can navigate backward in history
   */
  canNavigateBackwardInHistory(deviceId: string) {
    return this.store.canNavigateBackwardInHistory(deviceId);
  }

  /**
   * Phase 1: Check if can navigate forward in history
   */
  canNavigateForwardInHistory(deviceId: string) {
    return this.store.canNavigateForwardInHistory(deviceId);
  }

  /**
   * Phase 1: Clear play history for a device
   */
  clearHistory(deviceId: string): void {
    this.store.clearHistory({ deviceId });
  }

  /**
   * Phase 3: Toggle history view visibility for a device
   */
  toggleHistoryView(deviceId: string): void {
    const currentVisibility = this.store.isHistoryViewVisible(deviceId)();
    this.store.updateHistoryViewVisibility({ deviceId, visible: !currentVisibility });
  }

  /**
   * Phase 3: Check if history view is visible for a device
   */
  isHistoryViewVisible(deviceId: string): Signal<boolean> {
    return this.store.isHistoryViewVisible(deviceId);
  }

  /**
   * Navigate to a specific position in history
   * Loads directory context and sets up timer after navigation
   */
  async navigateToHistoryPosition(deviceId: string, position: number): Promise<void> {
    // Navigate to the history position (launches file and updates position)
    await this.store.navigateToHistoryPosition({ deviceId, position });

    // Check for errors
    if (this.hasErrorAndCleanup(deviceId)) {
      return;
    }

    // Load directory context for the file
    const currentFile = this.store.getCurrentFile(deviceId)();
    if (currentFile) {
      await this.loadDirectoryContextForRandomFile(currentFile);
    }

    // Setup timer if file is compatible
    if (currentFile?.isCompatible) {
      this.setupTimerForFile(deviceId, currentFile.file);
      this.updateUrlForLaunchedFile(deviceId);
    }
  }
}

