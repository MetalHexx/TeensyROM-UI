import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  StorageType,
  IStorageService,
  STORAGE_SERVICE,
} from '@teensyrom-nx/domain/storage/services';
import { StorageState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import {
  WritableStore,
  setDeviceSelectedDirectory,
  getStorage,
  isDirectoryLoadedAtPath,
  createStorage,
  updateStorage,
  setStorageLoaded,
  setStorageError,
  LogType,
  logInfo,
  logError,
} from '../storage-helpers';

export function initializeStorage(
  store: WritableStore<StorageState>,
  storageService: IStorageService
) {
  return {
    initializeStorage: async ({
      deviceId,
      storageType,
    }: {
      deviceId: string;
      storageType: StorageType;
    }): Promise<void> => {
      const key = StorageKeyUtil.create(deviceId, storageType);

      logInfo(LogType.Start, `Starting async initialization for ${key}`);

      setDeviceSelectedDirectory(store, deviceId, storageType, '/');

      const existingEntry = getStorage(store, key);

      if (isDirectoryLoadedAtPath(existingEntry, '/')) {
        logInfo(LogType.Info, `${key} already loaded, skipping initialization`);
        return;
      }

      if (!existingEntry) {
        createStorage(store, deviceId, storageType, '/');
      } else {
        updateStorage(store, key, {
          currentPath: '/',
          isLoading: true,
          error: null,
        });
      }

      try {
        logInfo(LogType.NetworkRequest, `Making API call for ${key}`);

        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, '/')
        );

        logInfo(LogType.Success, `API call successful for ${key}:`, directory);

        setStorageLoaded(store, key, {
          currentPath: '/',
          directory,
        });

        logInfo(LogType.Finish, `Initialization completed for ${key}`);
      } catch (error) {
        logError(`API error for ${key}:`, error);

        setStorageError(
          store,
          key,
          error instanceof Error ? error.message : 'Failed to initialize storage'
        );
      }
    },
  };
}
