import { Injectable } from '@angular/core';
import {
  FilesApiService,
  GetDirectoryResponse,
  TeensyStorageType,
} from '@teensyrom-nx/data-access/api-client';
import { StorageDirectory } from './storage.models';
import { StorageMapper } from './storage.mapper';
import { Observable, map, catchError, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class StorageService {
  constructor(private readonly apiService: FilesApiService) {}

  getDirectory(
    deviceId: string,
    storageType: TeensyStorageType,
    path?: string
  ): Observable<StorageDirectory> {
    return this.apiService.getDirectory(deviceId, storageType, path).pipe(
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
