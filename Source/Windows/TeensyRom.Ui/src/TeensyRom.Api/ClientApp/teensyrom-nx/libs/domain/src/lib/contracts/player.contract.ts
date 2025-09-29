import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { FileItem, PlayerFilterType, PlayerScope, StorageType } from '../models';

export interface IPlayerService {
  /**
   * Launch a specific file on the TeensyROM device.
   */
  launchFile(deviceId: string, storageType: StorageType, filePath: string): Observable<FileItem>;

  /**
   * Launch a random file with scope and filter configuration.
   * 
   * @param deviceId - Target TeensyROM device identifier
   * @param scope - Random selection scope (Storage, DirectoryDeep, DirectoryShallow)
   * @param filter - Content filtering (All, Games, Music, Images, Hex)
   * @param startingDirectory - Optional starting directory for scoped operations
   * @returns Observable<FileItem> - The randomly selected and launched file
   */
  launchRandom(
    deviceId: string, 
    scope: PlayerScope, 
    filter: PlayerFilterType, 
    startingDirectory?: string
  ): Observable<FileItem>;
}

export const PLAYER_SERVICE = new InjectionToken<IPlayerService>('PLAYER_SERVICE');


