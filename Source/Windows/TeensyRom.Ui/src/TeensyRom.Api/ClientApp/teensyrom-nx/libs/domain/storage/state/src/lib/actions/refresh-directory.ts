import { firstValueFrom } from 'rxjs';
import { StorageType, IStorageService } from '@teensyrom-nx/domain/storage/services';
import { StorageState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import {
  WritableStore,
  getStorage,
  setLoadingStorage,
  setStorageLoaded,
  updateStorage,
} from '../storage-helpers';
import { LogType, logInfo, logError, logWarn, createAction } from '@teensyrom-nx/utils';

export function refreshDirectory(
  store: WritableStore<StorageState>,
  storageService: IStorageService
) {
  return {
    refreshDirectory: async ({
      deviceId,
      storageType,
    }: {
      deviceId: string;
      storageType: StorageType;
    }): Promise<void> => {
      const actionMessage = createAction('refresh-directory');
      const key = StorageKeyUtil.create(deviceId, storageType);
      const entry = getStorage(store, key);

      if (!entry) {
        logWarn(`Cannot refresh - no entry found for ${key}`);
        return;
      }

      logInfo(LogType.Refresh, `Refreshing directory for ${key} at path: ${entry.currentPath}`);

      setLoadingStorage(store, key, actionMessage);

      try {
        const directory = await firstValueFrom(
          storageService.getDirectory(deviceId, storageType, entry.currentPath)
        );

        logInfo(LogType.Success, `Directory refresh successful for ${key}:`, directory);

        setStorageLoaded(
          store,
          key,
          {
            currentPath: entry.currentPath,
            directory,
          },
          actionMessage
        );

        logInfo(LogType.Finish, `Refresh completed for ${key}`);
      } catch (error) {
        logError(`Directory refresh failed for ${key}:`, error);

        updateStorage(
          store,
          key,
          {
            isLoading: false,
            error: 'Failed to refresh directory',
          },
          actionMessage
        );
      }
    },
  };
}
