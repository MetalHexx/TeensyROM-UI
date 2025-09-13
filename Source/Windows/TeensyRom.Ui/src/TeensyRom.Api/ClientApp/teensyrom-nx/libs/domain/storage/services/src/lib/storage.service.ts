import { Injectable } from '@angular/core';
import { FilesApiService, GetDirectoryResponse } from '@teensyrom-nx/data-access/api-client';
import { StorageDirectory, StorageType } from './storage.models';
import { StorageMapper } from './storage.mapper';
import { Observable, map, catchError, throwError, from } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class StorageService {
  constructor(private readonly apiService: FilesApiService) {}

  getDirectory(
    deviceId: string,
    storageType: StorageType,
    path?: string
  ): Observable<StorageDirectory> {
    const apiStorageType = StorageMapper.toApiStorageType(storageType);
    return from(this.apiService.getDirectory({ deviceId, storageType: apiStorageType, path })).pipe(
      map((response: GetDirectoryResponse) => {
        if (!response.storageItem) {
          throw new Error('Invalid response: storageItem is missing');
        }
        return StorageMapper.toStorageDirectory(response.storageItem);
      }),
      catchError((error) => {
        console.error('Storage directory fetch failed:', error);
        return throwError(() => error);
      })
    );
  }
}
