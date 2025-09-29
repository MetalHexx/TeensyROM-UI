import { InjectionToken, Signal } from '@angular/core';
import { FileItem, LaunchMode, PlayerStatus, StorageType } from '@teensyrom-nx/domain';
import { LaunchedFile, PlayerFileContext } from './player-store';

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
}

export const PLAYER_CONTEXT = new InjectionToken<IPlayerContext>('PLAYER_CONTEXT');
