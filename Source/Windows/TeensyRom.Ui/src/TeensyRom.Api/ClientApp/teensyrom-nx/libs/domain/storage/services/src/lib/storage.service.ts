import { Injectable, InjectionToken } from '@angular/core';
import { FilesApiService, GetDirectoryResponse } from '@teensyrom-nx/data-access/api-client';
import { StorageDirectory, StorageType } from './storage.models';
import { StorageMapper } from './storage.mapper';
import { Observable, map, catchError, throwError, from } from 'rxjs';

// Interface describing the storage service contract
export interface IStorageService {
  getDirectory(
    deviceId: string,
    storageType: StorageType,
    path?: string
  ): Observable<StorageDirectory>;
}

// Injection token for IStorageService to enable DI-by-interface
export const STORAGE_SERVICE = new InjectionToken<IStorageService>('STORAGE_SERVICE');

@Injectable({ providedIn: 'root' })
export class StorageService implements IStorageService {
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

// Provider helper for application root to bind the interface token
export const STORAGE_SERVICE_PROVIDER = {
  provide: STORAGE_SERVICE,
  useExisting: StorageService,
};
