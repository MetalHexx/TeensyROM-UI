import { Injectable, Inject } from '@angular/core';
import { from, map, catchError, throwError, Observable, mergeMap } from 'rxjs';
import { PlayerApiService } from '@teensyrom-nx/data-access/api-client';
import { DomainMapper } from '../domain.mapper';
import {
  FileItem,
  IPlayerService,
  PlayerFilterType,
  PlayerScope,
  StorageType,
  ALERT_SERVICE,
  IAlertService,
} from '@teensyrom-nx/domain';
import { logError } from '@teensyrom-nx/utils';
import { extractErrorMessage } from '../error/api-error.utils';

@Injectable({ providedIn: 'root' })
export class PlayerService implements IPlayerService {
  private readonly baseApiUrl: string;
  private readonly alertService: IAlertService;

  constructor(
    private readonly apiService: PlayerApiService,
    @Inject(ALERT_SERVICE) alertService: IAlertService
  ) {
    // Extract base URL from API service configuration with fallback
    this.baseApiUrl = (this.apiService as any).configuration?.basePath || 'http://localhost:5168';
    this.alertService = alertService;
  }

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
        const fileItem = DomainMapper.toFileItem(response.launchedFile, this.baseApiUrl);

        // Check compatibility and show warning if file is not compatible
        if (!fileItem.isCompatible && response.message) {
          this.alertService.warning(response.message);
        }

        return fileItem;
      }),
      catchError((error) => this.handleError(error, 'launchFile', 'Failed to launch file'))
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
        const fileItem = DomainMapper.toFileItem(response.launchedFile, this.baseApiUrl);

        // Check compatibility and show warning if file is not compatible
        if (!fileItem.isCompatible && response.message) {
          this.alertService.warning(response.message);
        }

        return fileItem;
      }),
      catchError((error) => this.handleError(error, 'launchRandom', 'Failed to launch random file'))
    );
  }

  toggleMusic(deviceId: string): Observable<void> {
    return from(this.apiService.toggleMusic({ deviceId })).pipe(
      map((response) => {
        if (!response?.message) {
          throw new Error('Invalid response: message is missing');
        }
        // API only returns a message, not the current state
        return undefined; // Return void
      }),
      catchError((error) => this.handleError(error, 'toggleMusic', 'Failed to toggle music'))
    );
  }

  private handleError(
    error: unknown,
    methodName: string,
    fallbackMessage: string
  ): Observable<never> {
    return from(extractErrorMessage(error, fallbackMessage)).pipe(
      mergeMap((message) => {
        logError(`PlayerService.${methodName} failed:`, error);
        this.alertService.error(message);
        return throwError(() => (error instanceof Error ? error : new Error(fallbackMessage)));
      })
    );
  }
}
