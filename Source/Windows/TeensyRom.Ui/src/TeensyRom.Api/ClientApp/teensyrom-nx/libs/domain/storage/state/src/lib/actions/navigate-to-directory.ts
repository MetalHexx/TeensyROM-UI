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
  setLoadingStorage,
  setStorageLoaded,
  updateStorage,
  isSelectedDirectory,
} from '../storage-helpers';
import { LogType, logInfo, logError } from '@teensyrom-nx/utils';

export function navigateToDirectory(
  store: WritableStore<StorageState>,
  storageService: IStorageService
) {
  return {
    navigateToDirectory: async ({
      deviceId,
      storageType,
      path,
    }: {
      deviceId: string;
      storageType: StorageType;
      path: string;
    }): Promise<void> => {
      const key = StorageKeyUtil.create(deviceId, storageType);

      logInfo(LogType.Navigate, `Navigating to ${key} at path: ${path}`);

      if (!isSelectedDirectory(store, deviceId, storageType, path)) {
        setDeviceSelectedDirectory(store, deviceId, storageType, path);
      }

      const existingEntry = getStorage(store, key);

      if (isDirectoryLoadedAtPath(existingEntry, path)) {
        logInfo(LogType.Info, `Directory already loaded for ${key} at path: ${path}`);
        return;
      }

      setLoadingStorage(store, key);

      try {
        logInfo(LogType.NetworkRequest, `Loading directory for ${key} at path: ${path}`);

        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, path)
        );

        logInfo(LogType.Success, `Directory navigation successful for ${key}:`, directory);

        setStorageLoaded(store, key, {
          currentPath: path,
          directory,
        });

        logInfo(LogType.Finish, `Navigation completed for ${key} at path: ${path}`);
      } catch (error) {
        logError(`Directory navigation failed for ${key} at path ${path}:`, error);

        updateStorage(store, key, {
          currentPath: path,
          directory: null,
          isLoaded: false,
          isLoading: false,
          error: 'Failed to navigate to directory',
        });
      }
    },
  };
}
