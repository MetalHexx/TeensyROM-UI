import { firstValueFrom } from 'rxjs';
import { StorageType, IStorageService } from '@teensyrom-nx/domain';
import { StorageState, NavigationHistory } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import {
  WritableStore,
  getStorage,
  isSelectedDirectory,
  setDeviceSelectedDirectory,
  setStorageLoaded,
  updateStorage,
  clearSearchState,
} from '../storage-helpers';
import { LogType, logInfo, logError, logWarn, createAction } from '@teensyrom-nx/utils';
import { updateState } from '@angular-architects/ngrx-toolkit';

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

      // Clear any active search when navigating
      clearSearchState(store, key, actionMessage);

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

        const currentHistory = store.navigationHistory()[deviceId] || new NavigationHistory();
        const updatedHistory = new NavigationHistory(currentHistory.maxHistorySize);

        updatedHistory.history = [
          ...currentHistory.history.slice(0, currentHistory.currentIndex + 1),
          { path: parentPath, storageType },
        ];
        updatedHistory.currentIndex = updatedHistory.history.length - 1;
        updatedHistory.maxHistorySize = currentHistory.maxHistorySize;

        if (updatedHistory.history.length > updatedHistory.maxHistorySize) {
          const excess = updatedHistory.history.length - updatedHistory.maxHistorySize;
          updatedHistory.history = updatedHistory.history.slice(excess);
          updatedHistory.currentIndex -= excess;
        }

        updateState(store, actionMessage, (state) => ({
          navigationHistory: {
            ...state.navigationHistory,
            [deviceId]: updatedHistory,
          },
        }));

        logInfo(
          LogType.Info,
          `Added parent directory to navigation history for device: ${deviceId}, path: ${parentPath}`
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
