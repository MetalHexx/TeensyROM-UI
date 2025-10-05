import { Injectable } from '@angular/core';
import { FilesApiService, GetDirectoryResponse } from '@teensyrom-nx/data-access/api-client';
import { StorageDirectory, StorageType, IStorageService } from '@teensyrom-nx/domain';
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
}
