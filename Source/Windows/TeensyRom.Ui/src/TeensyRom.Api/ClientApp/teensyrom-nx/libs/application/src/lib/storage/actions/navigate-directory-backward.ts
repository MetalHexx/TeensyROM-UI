import { firstValueFrom } from 'rxjs';
import { IStorageService } from '@teensyrom-nx/domain';
import { StorageState, NavigationHistory } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import {
  WritableStore,
  setDeviceSelectedDirectory,
  getStorage,
  isDirectoryLoadedAtPath,
  setLoadingStorage,
  setStorageLoaded,
  setStorageError,
} from '../storage-helpers';
import { createAction } from '@teensyrom-nx/utils';
import { LogType, logInfo, logError } from '@teensyrom-nx/utils';
import { updateState } from '@angular-architects/ngrx-toolkit';

export function navigateDirectoryBackward(
  store: WritableStore<StorageState>,
  storageService: IStorageService
) {
  return {
    navigateDirectoryBackward: async ({ deviceId }: { deviceId: string }): Promise<void> => {
      const actionMessage = createAction('navigate-directory-backward');

      logInfo(LogType.Navigate, `Navigating backward for device: ${deviceId}`);

      // Get current NavigationHistory for device
      const currentHistory = store.navigationHistory()[deviceId] || new NavigationHistory();

      // Check if backward navigation is possible
      if (currentHistory.currentIndex <= 0) {
        logInfo(LogType.Info, `Already at beginning of history for device: ${deviceId}`);
        return;
      }

      // Calculate new index and get target item with storage type
      const newIndex = currentHistory.currentIndex - 1;
      const targetItem = currentHistory.history[newIndex];

      if (!targetItem) {
        logInfo(LogType.Info, `No valid history item at index ${newIndex} for device: ${deviceId}`);
        return;
      }

      const { path: targetPath, storageType } = targetItem;

      // Update NavigationHistory state
      const updatedHistory = new NavigationHistory(currentHistory.maxHistorySize);
      updatedHistory.history = [...currentHistory.history];
      updatedHistory.currentIndex = newIndex;
      updatedHistory.maxHistorySize = currentHistory.maxHistorySize;

      updateState(store, actionMessage, (state) => ({
        navigationHistory: {
          ...state.navigationHistory,
          [deviceId]: updatedHistory,
        },
      }));

      logInfo(
        LogType.Info,
        `Updated history index to ${newIndex} for device: ${deviceId}, target path: ${targetPath}, storageType: ${storageType}`
      );
      const key = StorageKeyUtil.create(deviceId, storageType);

      setDeviceSelectedDirectory(store, deviceId, storageType, targetPath, actionMessage);

      const existingEntry = getStorage(store, key);
      if (isDirectoryLoadedAtPath(existingEntry, targetPath)) {
        logInfo(LogType.Info, `Directory already loaded for ${key} at path: ${targetPath}`);
        return;
      }
      setLoadingStorage(store, key, actionMessage);

      try {
        logInfo(LogType.NetworkRequest, `Loading directory for ${key} at path: ${targetPath}`);

        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, targetPath)
        );

        logInfo(LogType.Success, `Directory backward navigation successful for ${key}:`, directory);

        setStorageLoaded(
          store,
          key,
          {
            currentPath: targetPath,
            directory,
          },
          actionMessage
        );

        logInfo(LogType.Finish, `Backward navigation completed for ${key} at path: ${targetPath}`);
      } catch (error) {
        logError(`Directory backward navigation failed for ${key} at path ${targetPath}:`, error);

        setStorageError(store, key, 'Failed to load directory from history', actionMessage);
      }
    },
  };
}
