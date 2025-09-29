import { InjectionToken, Signal } from '@angular/core';
import { FileItem, LaunchMode, PlayerStatus, StorageType, PlayerFilterType, PlayerScope } from '@teensyrom-nx/domain';
import { LaunchedFile, PlayerFileContext, ShuffleSettings } from './player-store';

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
  launchFileWithContext(request: LaunchFileContextRequest): Promise<void>;
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
}

export const PLAYER_CONTEXT = new InjectionToken<IPlayerContext>('PLAYER_CONTEXT');
