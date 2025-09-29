import { Injectable } from '@angular/core';
import { from, map, catchError, throwError, Observable } from 'rxjs';
import { PlayerApiService, LaunchRandomFilterTypeEnum, LaunchRandomScopeEnum } from '@teensyrom-nx/data-access/api-client';
import { DomainMapper } from '../domain.mapper';
import {
  FileItem,
  IPlayerService,
  LaunchRandomOptions,
  StorageType,
} from '@teensyrom-nx/domain';
import { logError } from '@teensyrom-nx/utils';

@Injectable({ providedIn: 'root' })
export class PlayerService implements IPlayerService {
  constructor(private readonly apiService: PlayerApiService) {}

  launchFile(deviceId: string, storageType: StorageType, filePath: string): Observable<FileItem> {
    const apiStorageType = DomainMapper.toApiStorageType(storageType);
    return from(
      this.apiService.launchFile({
        deviceId,
        storageType: apiStorageType,
        filePath,
      })
    ).pipe(
      map((response) => {
        if (!response?.launchedFile) {
          logError('Invalid response: launchedFile is missing');
          throw new Error('Invalid response: launchedFile is missing');
        }
        return DomainMapper.toFileItem(response.launchedFile);
      }),
      catchError((error) => {
        logError('PlayerService launchFile failed:', error);
        return throwError(() => (error instanceof Error ? error : new Error('Failed to launch file')));
      })
    );
  }

  launchRandom(
    deviceId: string,
    storageType: StorageType,
    options: LaunchRandomOptions = {}
  ): Observable<FileItem> {
    const apiStorageType = DomainMapper.toApiStorageType(storageType);
    return from(
      this.apiService.launchRandom({
        deviceId,
        storageType: apiStorageType,
        filterType: options.filterType as LaunchRandomFilterTypeEnum | undefined,
        scope: options.scope as LaunchRandomScopeEnum | undefined,
        startingDirectory: options.startingDirectory,
      })
    ).pipe(
      map((response) => {
        if (!response?.launchedFile) {
          throw new Error('Invalid response: launchedFile is missing');
        }
        return DomainMapper.toFileItem(response.launchedFile);
      }),
      catchError((error) => {
        console.error('PlayerService launchRandom failed:', error);
        return throwError(() => (error instanceof Error ? error : new Error('Failed to launch random file')));
      })
    );
  }
}




