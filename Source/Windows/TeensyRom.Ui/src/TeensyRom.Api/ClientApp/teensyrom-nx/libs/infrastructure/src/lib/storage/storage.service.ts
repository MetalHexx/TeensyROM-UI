import { Injectable, Inject } from '@angular/core';
import { FilesApiService, GetDirectoryResponse, SearchResponse } from '@teensyrom-nx/data-access/api-client';
import { StorageDirectory, StorageType, IStorageService, FileItem, PlayerFilterType, ALERT_SERVICE, IAlertService } from '@teensyrom-nx/domain';
import { DomainMapper } from '../domain.mapper';
import { Observable, map, catchError, from, throwError } from 'rxjs';
import { logError } from '@teensyrom-nx/utils';

@Injectable({ providedIn: 'root' })
export class StorageService implements IStorageService {
  private readonly baseApiUrl: string;
  private readonly alertService: IAlertService;

  constructor(
    private readonly apiService: FilesApiService,
    @Inject(ALERT_SERVICE) alertService: IAlertService
  ) {
    // Extract base URL from API service configuration with fallback
    // Configuration is protected, so we access it via unknown cast
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const config = (this.apiService as any).configuration;
    this.baseApiUrl = config?.basePath || 'http://localhost:5168';
    this.alertService = alertService;
  }

  getDirectory(
    deviceId: string,
    storageType: StorageType,
    path?: string
  ): Observable<StorageDirectory> {
    const apiStorageType = DomainMapper.toApiStorageType(storageType);
    return from(this.apiService.getDirectory({ deviceId, storageType: apiStorageType, path })).pipe(
      map((response: GetDirectoryResponse) => {
        if (!response.storageItem) {
          throw new Error('Invalid response: storageItem is missing');
        }
        return DomainMapper.toStorageDirectory(response.storageItem, this.baseApiUrl);
      }),
      catchError((error) => this.handleError(error, 'getDirectory', 'Failed to retrieve directory'))
    );
  }

  index(deviceId: string, storageType: StorageType, startingPath?: string): Observable<unknown> {
    const apiStorageType = DomainMapper.toApiStorageType(storageType);
    return from(this.apiService.index({ deviceId, storageType: apiStorageType, startingPath })).pipe(
      catchError((error) => this.handleError(error, 'index', 'Failed to index storage'))
    );
  }

  indexAll(): Observable<unknown> {
    return from(this.apiService.indexAll({})).pipe(
      catchError((error) => this.handleError(error, 'indexAll', 'Failed to index all storage'))
    );
  }

  search(
    deviceId: string,
    storageType: StorageType,
    searchText: string,
    filterType?: PlayerFilterType,
    skip = 0,
    take = 1000
  ): Observable<FileItem[]> {
    const apiStorageType = DomainMapper.toApiStorageType(storageType);
    const apiFilterType = filterType ? DomainMapper.toApiSearchFilter(filterType) : undefined;
    
    return from(
      this.apiService.search({ 
        deviceId, 
        storageType: apiStorageType, 
        searchText,
        skip,
        take,
        filterType: apiFilterType 
      })
    ).pipe(
      map((response: SearchResponse) => {
        return response.files?.map(file => DomainMapper.toFileItem(file, this.baseApiUrl)) ?? [];
      }),
      catchError((error) => this.handleError(error, 'search', 'Search operation failed'))
    );
  }

  private handleError(error: unknown, methodName: string, fallbackMessage: string): Observable<never> {
    const message = error instanceof Error ? error.message : fallbackMessage;
    logError(`StorageService.${methodName} error:`, error);
    this.alertService.error(message);
    return throwError(() => error);
  }
}
