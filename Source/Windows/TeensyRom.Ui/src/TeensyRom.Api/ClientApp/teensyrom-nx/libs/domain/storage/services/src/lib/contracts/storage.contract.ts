import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { StorageDirectory, StorageType } from '../storage.models';

/**
 * Storage service contract defining the interface for storage operations.
 * This interface is implemented by concrete storage services in the infrastructure layer.
 */
export interface IStorageService {
  /**
   * Retrieves directory contents for a specific device and storage type.
   * @param deviceId - The unique identifier of the device
   * @param storageType - The type of storage (USB, SD, etc.)
   * @param path - Optional path within the storage (defaults to root)
   * @returns Observable of StorageDirectory containing directory contents
   */
  getDirectory(
    deviceId: string,
    storageType: StorageType,
    path?: string
  ): Observable<StorageDirectory>;
}

/**
 * Injection token for IStorageService to enable dependency injection by interface.
 * This allows the domain to depend on the interface while the infrastructure
 * provides the concrete implementation.
 */
export const STORAGE_SERVICE = new InjectionToken<IStorageService>('STORAGE_SERVICE');