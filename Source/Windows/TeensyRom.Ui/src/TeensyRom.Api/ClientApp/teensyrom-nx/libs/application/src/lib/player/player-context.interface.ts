import { InjectionToken, Signal } from '@angular/core';
import { FileItem, LaunchMode, PlayerStatus, StorageType, PlayerFilterType, PlayerScope } from '@teensyrom-nx/domain';
import { LaunchedFile, PlayerFileContext, ShuffleSettings, PlayHistory } from './player-store';
import { TimerState } from './timer-state.interface';

export interface LaunchFileContextRequest {
  deviceId: string;
  storageType: StorageType;
  file: FileItem;
  directoryPath: string;
  files: FileItem[];
  launchMode?: LaunchMode;
}

export interface IPlayerContext {
  initializePlayer(deviceId: string): void;
  removePlayer(deviceId: string): void;
  startListeningToPopState(): void;
  stopListeningToPopState(): void;
  launchFileWithContext(request: LaunchFileContextRequest): Promise<void>;
  updateCurrentFileFavoriteStatus(deviceId: string, filePath: string, isFavorite: boolean): void;
  getCurrentFile(deviceId: string): Signal<LaunchedFile | null>;
  getFileContext(deviceId: string): Signal<PlayerFileContext | null>;
  isLoading(deviceId: string): Signal<boolean>;
  getError(deviceId: string): Signal<string | null>;
  getStatus(deviceId: string): Signal<PlayerStatus>;
  
  // Shuffle functionality
  launchRandomFile(deviceId: string): Promise<void>;
  toggleShuffleMode(deviceId: string): void;
  setShuffleScope(deviceId: string, scope: PlayerScope): void;
  setFilterMode(deviceId: string, filter: PlayerFilterType): void;
  getShuffleSettings(deviceId: string): Signal<ShuffleSettings | null>;
  getLaunchMode(deviceId: string): Signal<LaunchMode>;

  // Phase 3: Playback controls
  play(deviceId: string): Promise<void>;
  pause(deviceId: string): Promise<void>;
  stop(deviceId: string): Promise<void>;
  next(deviceId: string): Promise<void>;
  previous(deviceId: string): Promise<void>;
  getPlayerStatus(deviceId: string): Signal<PlayerStatus>;

  // Phase 5: Timer system
  getTimerState(deviceId: string): Signal<TimerState | null>;

  // File compatibility
  isCurrentFileCompatible(deviceId: string): Signal<boolean>;

  // Play history
  getPlayHistory(deviceId: string): Signal<PlayHistory | null>;
  getCurrentHistoryPosition(deviceId: string): Signal<number>;
  canNavigateBackwardInHistory(deviceId: string): Signal<boolean>;
  canNavigateForwardInHistory(deviceId: string): Signal<boolean>;
  clearHistory(deviceId: string): void;
  toggleHistoryView(deviceId: string): void;
  isHistoryViewVisible(deviceId: string): Signal<boolean>;
  navigateToHistoryPosition(deviceId: string, position: number): Promise<void>;
}

export const PLAYER_CONTEXT = new InjectionToken<IPlayerContext>('PLAYER_CONTEXT');
