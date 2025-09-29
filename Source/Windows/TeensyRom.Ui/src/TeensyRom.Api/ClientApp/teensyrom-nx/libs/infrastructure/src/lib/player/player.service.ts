import { Injectable } from '@angular/core';
import { from, map, catchError, throwError, Observable } from 'rxjs';
import { PlayerApiService } from '@teensyrom-nx/data-access/api-client';
import { DomainMapper } from '../domain.mapper';
import {
  FileItem,
  IPlayerService,
  PlayerFilterType,
  PlayerScope,
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
    scope: PlayerScope,
    filter: PlayerFilterType,
    startingDirectory?: string
  ): Observable<FileItem> {
    // For Phase 2, we default to SD storage - this should be configurable in future phases
    const apiStorageType = DomainMapper.toApiStorageType(StorageType.Sd);
    
    // Map domain enums to API enums using DomainMapper
    const apiScope = DomainMapper.toApiPlayerScope(scope);
    const apiFilter = DomainMapper.toApiPlayerFilter(filter);
    
    return from(
      this.apiService.launchRandom({
        deviceId,
        storageType: apiStorageType,
        scope: apiScope,
        filterType: apiFilter,
        startingDirectory,
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
