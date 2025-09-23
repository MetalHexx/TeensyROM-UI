import { firstValueFrom } from 'rxjs';
import { StorageType, IStorageService } from '@teensyrom-nx/domain/storage/services';
import { StorageState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import {
  WritableStore,
  getStorage,
  isSelectedDirectory,
  setDeviceSelectedDirectory,
  setStorageLoaded,
  updateStorage,
} from '../storage-helpers';
import { LogType, logInfo, logError, logWarn, createAction } from '@teensyrom-nx/utils';

export function navigateUpOneDirectory(
  store: WritableStore<StorageState>,
  storageService: IStorageService
) {
  return {
    navigateUpOneDirectory: async ({
      deviceId,
      storageType,
    }: {
      deviceId: string;
      storageType: StorageType;
    }): Promise<void> => {
      const actionMessage = createAction('navigate-up-one-directory');
      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = getStorage(store, key);

      if (!entry) {
        logWarn(`Cannot navigate up - no entry found for ${key}`);
        return;
      }

      const currentPath = entry.currentPath;
      const parentPath = calculateParentPath(currentPath);

      if (currentPath === parentPath) {
        logInfo(LogType.Info, `Already at root directory for ${key}, cannot navigate up`);
        return;
      }

      logInfo(LogType.Navigate, `Navigating up from ${currentPath} to ${parentPath} for ${key}`);

      if (!isSelectedDirectory(store, deviceId, storageType, parentPath)) {
        setDeviceSelectedDirectory(store, deviceId, storageType, parentPath, actionMessage);
      }
      try {
        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, parentPath)
        );

        logInfo(LogType.Success, `Navigate up successful for ${key}:`, directory);

        setStorageLoaded(
          store,
          key,
          {
            currentPath: parentPath,
            directory,
          },
          actionMessage
        );

        logInfo(LogType.Finish, `Navigate up completed for ${key} to path: ${parentPath}`);
      } catch (error) {
        logError(`Navigate up failed for ${key} to path ${parentPath}:`, error);

        updateStorage(
          store,
          key,
          {
            currentPath: parentPath,
            directory: null,
            isLoaded: false,
            isLoading: false,
            error: 'Failed to navigate up one directory',
          },
          actionMessage
        );
      }
    },
  };
}

/**
 * Calculate the parent path by removing the last directory segment
 * @param currentPath - The current directory path
 * @returns The parent directory path
 */
function calculateParentPath(currentPath: string): string {
  if (!currentPath || currentPath === '/' || currentPath === '') {
    return '/';
  }
  const normalizedPath =
    currentPath.endsWith('/') && currentPath !== '/' ? currentPath.slice(0, -1) : currentPath;

  const lastSlashIndex = normalizedPath.lastIndexOf('/');

  if (lastSlashIndex <= 0) {
    return '/';
  }

  return normalizedPath.substring(0, lastSlashIndex) || '/';
}
