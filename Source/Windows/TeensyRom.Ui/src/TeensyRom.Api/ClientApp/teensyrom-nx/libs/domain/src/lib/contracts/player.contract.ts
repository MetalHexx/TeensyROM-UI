import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { FileItem, StorageType } from '../models';

/**
 * Optional configuration for launching random files. These values will be expanded in future phases.
 */
export interface LaunchRandomOptions {
  filterType?: "All" | "Games" | "Music" | "Hex" | "Images";
  scope?: "Storage" | "DirDeep" | "DirShallow";
  startingDirectory?: string;
}

export interface IPlayerService {
  /**
   * Launch a specific file on the TeensyROM device.
   */
  launchFile(deviceId: string, storageType: StorageType, filePath: string): Observable<FileItem>;

  /**
   * Launch a random file. Additional options will be implemented in future phases.
   */
  launchRandom(
    deviceId: string,
    storageType: StorageType,
    options?: LaunchRandomOptions
  ): Observable<FileItem>;
}

export const PLAYER_SERVICE = new InjectionToken<IPlayerService>('PLAYER_SERVICE');


