import { InjectionToken } from '@angular/core';
import { TeensyStorageType } from '@teensyrom-nx/data-access/api-client';
import { Observable } from 'rxjs';

export interface IStorageService {
  index(
    deviceId: string,
    storageType: TeensyStorageType,
    startingPath?: string
  ): Observable<unknown>;
  indexAll(): Observable<unknown>;
}

export const DEVICE_STORAGE_SERVICE = new InjectionToken<IStorageService>('DEVICE_STORAGE_SERVICE');

