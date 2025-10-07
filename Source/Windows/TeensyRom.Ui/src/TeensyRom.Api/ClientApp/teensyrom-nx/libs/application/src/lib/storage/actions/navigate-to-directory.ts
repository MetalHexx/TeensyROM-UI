import { firstValueFrom } from 'rxjs';
import { StorageType, IStorageService } from '@teensyrom-nx/domain';
import { StorageState, NavigationHistory } from '../storage-store';
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
  clearSearchState,
} from '../storage-helpers';
import { createAction } from '@teensyrom-nx/utils';
import { LogType, logInfo, logError } from '@teensyrom-nx/utils';
import { updateState } from '@angular-architects/ngrx-toolkit';

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
      const actionMessage = createAction(`navigate-to-directory`);

      const key = StorageKeyUtil.create(deviceId, storageType);

      logInfo(LogType.Navigate, `Navigating to ${key} at path: ${path}`);

      // Clear any active search when navigating
      clearSearchState(store, key, actionMessage);

      if (!isSelectedDirectory(store, deviceId, storageType, path)) {
        setDeviceSelectedDirectory(store, deviceId, storageType, path, actionMessage);
      }

      const existingEntry = getStorage(store, key);

      if (isDirectoryLoadedAtPath(existingEntry, path)) {
        logInfo(LogType.Info, `Directory already loaded for ${key} at path: ${path}`);
        return;
      }

      setLoadingStorage(store, key, actionMessage);

      try {
        logInfo(LogType.NetworkRequest, `Loading directory for ${key} at path: ${path}`);

        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, path)
        );

        logInfo(LogType.Success, `Directory navigation successful for ${key}:`, directory);

        setStorageLoaded(
          store,
          key,
          {
            currentPath: path,
            directory,
          },
          actionMessage
        );

        const currentHistory = store.navigationHistory()[deviceId] || new NavigationHistory();
        const updatedHistory = new NavigationHistory(currentHistory.maxHistorySize);

        updatedHistory.history = [
          ...currentHistory.history.slice(0, currentHistory.currentIndex + 1),
          { path, storageType },
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
          `Added directory to navigation history for device: ${deviceId}, path: ${path}`
        );
        logInfo(LogType.Finish, `Navigation completed for ${key} at path: ${path}`);
      } catch (error) {
        logError(`Directory navigation failed for ${key} at path ${path}:`, error);

        updateStorage(
          store,
          key,
          {
            currentPath: path,
            directory: null,
            isLoaded: false,
            isLoading: false,
            error: 'Failed to navigate to directory',
          },
          actionMessage
        );
      }
    },
  };
}
