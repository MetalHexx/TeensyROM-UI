import { Injectable } from '@angular/core';
import { FilesApiService, GetDirectoryResponse, SearchResponse } from '@teensyrom-nx/data-access/api-client';
import { StorageDirectory, StorageType, IStorageService, FileItem, PlayerFilterType } from '@teensyrom-nx/domain';
import { DomainMapper } from '../domain.mapper';
import { Observable, map, catchError, throwError, from } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class StorageService implements IStorageService {
  private readonly baseApiUrl: string;

  constructor(private readonly apiService: FilesApiService) {
    // Extract base URL from API service configuration with fallback
    this.baseApiUrl = (this.apiService as any).configuration?.basePath || 'http://localhost:5168';
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
      catchError((error) => {
        console.error('Storage directory fetch failed:', error);
        return throwError(() => error);
      })
    );
  }

  index(deviceId: string, storageType: StorageType, startingPath?: string): Observable<unknown> {
    const apiStorageType = DomainMapper.toApiStorageType(storageType);
    return from(this.apiService.index({ deviceId, storageType: apiStorageType, startingPath }));
  }

  indexAll(): Observable<unknown> {
    return from(this.apiService.indexAll({}));
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
      catchError((error) => {
        console.error('Storage search failed:', error);
        return throwError(() => error);
      })
    );
  }
}
