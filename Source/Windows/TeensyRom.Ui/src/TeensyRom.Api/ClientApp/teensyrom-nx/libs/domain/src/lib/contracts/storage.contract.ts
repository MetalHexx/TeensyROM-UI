import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { StorageDirectory, StorageType, FileItem, PlayerFilterType } from '../models';

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

  /**
   * Index storage on a device.
   * @param deviceId - The unique identifier of the device
   * @param storageType - The type of storage (USB, SD, etc.)
   * @param startingPath - Optional starting path for indexing
   * @returns Observable of index operation result
   */
  index(deviceId: string, storageType: StorageType, startingPath?: string): Observable<unknown>;

  /**
   * Index all storage on all devices.
   * @returns Observable of index all operation result
   */
  indexAll(): Observable<unknown>;

  /**
   * Searches for files across the entire storage hierarchy based on search text and filter criteria.
   *
   * This method performs a comprehensive search across all files in the storage device,
   * searching through file names, titles, creators, and descriptions. Results are returned
   * as a flat list of files only (no directories).
   *
   * @param deviceId - The unique identifier of the device
   * @param storageType - The type of storage (USB, SD, etc.)
   * @param searchText - Text to search for in file names, titles, creators, and descriptions
   * @param filterType - Optional filter to restrict search by file type (All, Games, Music, Images, Hex)
   * @param skip - Optional number of results to skip for pagination (defaults to 0)
   * @param take - Optional number of results to return for pagination (defaults to 1000)
   * @returns Observable of FileItem array containing all matching files from the entire storage hierarchy
   *
   * @remarks
   * - Returns files only, no directories in the search results
   * - Files come from the entire storage hierarchy, not just a single directory
   * - Search is performed across multiple metadata fields for comprehensive results
   * - FilterType parameter allows restricting results to specific file types
   * - Results are ranked by relevance using a weighted search algorithm
   * - Excludes favorites and playlist directories from search results
   * - Supports pagination via skip/take parameters for handling large result sets
   */
  search(
    deviceId: string,
    storageType: StorageType,
    searchText: string,
    filterType?: PlayerFilterType,
    skip?: number,
    take?: number
  ): Observable<FileItem[]>;

  /**
   * Saves a file to favorites.
   * @param deviceId - The unique identifier of the device
   * @param storageType - The type of storage (USB, SD, etc.)
   * @param filePath - The path to the file to add to favorites
   * @returns Observable of FileItem with updated isFavorite flag
   */
  saveFavorite(deviceId: string, storageType: StorageType, filePath: string): Observable<FileItem>;

  /**
   * Removes a file from favorites.
   * @param deviceId - The unique identifier of the device
   * @param storageType - The type of storage (USB, SD, etc.)
   * @param filePath - The path to the file to remove from favorites
   * @returns Observable that completes when the operation succeeds
   */
  removeFavorite(deviceId: string, storageType: StorageType, filePath: string): Observable<void>;
}

/**
 * Injection token for IStorageService to enable dependency injection by interface.
 * This allows the domain to depend on the interface while the infrastructure
 * provides the concrete implementation.
 */
export const STORAGE_SERVICE = new InjectionToken<IStorageService>('STORAGE_SERVICE');
