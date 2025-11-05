import { Injectable, inject } from '@angular/core';
import {
  FilesApiService,
  GetDirectoryResponse,
  SearchResponse,
  SaveFavoriteResponse,
  RemoveFavoriteResponse,
} from '@teensyrom-nx/data-access/api-client';
import {
  StorageDirectory,
  StorageType,
  IStorageService,
  FileItem,
  PlayerFilterType,
  ALERT_SERVICE,
} from '@teensyrom-nx/domain';
import { DomainMapper } from '../domain.mapper';
import { Observable, map, catchError, from, throwError, mergeMap } from 'rxjs';
import { logError } from '@teensyrom-nx/utils';
import { extractErrorMessage } from '../error/api-error.utils';

@Injectable({ providedIn: 'root' })
export class StorageService implements IStorageService {
  private readonly baseApiUrl: string;
  private readonly apiService = inject(FilesApiService);
  private readonly alertService = inject(ALERT_SERVICE);

  constructor() {
    // Extract base URL from API service configuration with fallback
    // Configuration is protected, so we access it via unknown cast
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const config = (this.apiService as any).configuration;
    this.baseApiUrl = config?.basePath || 'http://localhost:5168';
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
    return from(
      this.apiService.index({ deviceId, storageType: apiStorageType, startingPath })
    ).pipe(catchError((error) => this.handleError(error, 'index', 'Failed to index storage')));
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
        filterType: apiFilterType,
      })
    ).pipe(
      map((response: SearchResponse) => {
        return response.files?.map((file) => DomainMapper.toFileItem(file, this.baseApiUrl)) ?? [];
      }),
      catchError((error) => this.handleError(error, 'search', 'Search operation failed'))
    );
  }

  saveFavorite(deviceId: string, storageType: StorageType, filePath: string): Observable<FileItem> {
    const apiStorageType = DomainMapper.toApiStorageType(storageType);
    return from(
      this.apiService.saveFavorite({ deviceId, storageType: apiStorageType, filePath })
    ).pipe(
      map((response: SaveFavoriteResponse) => {
        if (!response || !response.favoriteFile) {
          throw new Error('Invalid response: favoriteFile is missing from saveFavorite response');
        }
        if (response.message) {
          this.alertService.success(response.message);
        }
        // Map API response to domain model
        return DomainMapper.toFileItem(response.favoriteFile, this.baseApiUrl);
      }),
      catchError((error) => this.handleError(error, 'saveFavorite', 'Failed to save favorite'))
    );
  }

  removeFavorite(deviceId: string, storageType: StorageType, filePath: string): Observable<void> {
    const apiStorageType = DomainMapper.toApiStorageType(storageType);
    return from(
      this.apiService.removeFavorite({ deviceId, storageType: apiStorageType, filePath })
    ).pipe(
      map((response: RemoveFavoriteResponse) => {
        if (!response) {
          throw new Error('Invalid response: removeFavorite returned empty response');
        }
        // Display success message from API response
        if (response.message) {
          this.alertService.success(response.message);
        }
        return void 0;
      }),
      catchError((error) => this.handleError(error, 'removeFavorite', 'Failed to remove favorite'))
    );
  }

  private handleError(
    error: unknown,
    methodName: string,
    fallbackMessage: string
  ): Observable<never> {
    return from(extractErrorMessage(error, fallbackMessage)).pipe(
      mergeMap((message) => {
        logError(`StorageService.${methodName} error:`, error);
        this.alertService.error(message);
        return throwError(() => error);
      })
    );
  }
}
