import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  StorageType,
  IStorageService,
  STORAGE_SERVICE,
} from '@teensyrom-nx/domain';
import { StorageState, NavigationHistory } from '../storage-store';
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
} from '../storage-helpers';
import { LogType, logInfo, logError, createAction } from '@teensyrom-nx/utils';
import { updateState } from '@angular-architects/ngrx-toolkit';

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
      const actionMessage = createAction('initialize-storage');
      const key = StorageKeyUtil.create(deviceId, storageType);

      logInfo(LogType.Start, `Starting async initialization for ${key}`);

      setDeviceSelectedDirectory(store, deviceId, storageType, '/', actionMessage);

      const existingEntry = getStorage(store, key);

      if (isDirectoryLoadedAtPath(existingEntry, '/')) {
        logInfo(LogType.Info, `${key} already loaded, skipping initialization`);
        return;
      }

      if (!existingEntry) {
        createStorage(store, deviceId, storageType, '/', actionMessage);
      } else {
        updateStorage(
          store,
          key,
          {
            currentPath: '/',
            isLoading: true,
            error: null,
          },
          actionMessage
        );
      }

      try {
        logInfo(LogType.NetworkRequest, `Making API call for ${key}`);

        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, '/')
        );

        logInfo(LogType.Success, `API call successful for ${key}:`, directory);

        setStorageLoaded(
          store,
          key,
          {
            currentPath: '/',
            directory,
          },
          actionMessage
        );

        const currentHistory = store.navigationHistory()[deviceId] || new NavigationHistory();
        const updatedHistory = new NavigationHistory(currentHistory.maxHistorySize);

        updatedHistory.history = [{ path: '/', storageType }];
        updatedHistory.currentIndex = 0;
        updatedHistory.maxHistorySize = currentHistory.maxHistorySize;

        updateState(store, actionMessage, (state) => ({
          navigationHistory: {
            ...state.navigationHistory,
            [deviceId]: updatedHistory,
          },
        }));

        logInfo(LogType.Info, `Added root directory to navigation history for device: ${deviceId}`);
        logInfo(LogType.Finish, `Initialization completed for ${key}`);
      } catch (error) {
        logError(`API error for ${key}:`, error);

        setStorageError(
          store,
          key,
          error instanceof Error ? error.message : 'Failed to initialize storage',
          actionMessage
        );
      }
    },
  };
}
