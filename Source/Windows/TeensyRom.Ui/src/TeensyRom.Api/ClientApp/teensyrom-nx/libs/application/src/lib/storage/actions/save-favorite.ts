import { firstValueFrom } from 'rxjs';
import { StorageType, IStorageService } from '@teensyrom-nx/domain';
import { StorageState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import { WritableStore, getStorage } from '../storage-helpers';
import { LogType, logInfo, logError, createAction } from '@teensyrom-nx/utils';
import { updateState } from '@angular-architects/ngrx-toolkit';

export function saveFavorite(store: WritableStore<StorageState>, storageService: IStorageService) {
  return {
    saveFavorite: async ({
      deviceId,
      storageType,
      filePath,
    }: {
      deviceId: string;
      storageType: StorageType;
      filePath: string;
    }): Promise<void> => {
      const actionMessage = createAction('save-favorite');
      const key = StorageKeyUtil.create(deviceId, storageType);

      logInfo(LogType.Start, `Starting save favorite for ${filePath} on ${key}`);

      // Set loading state
      updateState(store, actionMessage, (state) => ({
        favoriteOperationsState: {
          isProcessing: true,
          error: null,
        },
      }));

      try {
        logInfo(LogType.NetworkRequest, `Making API call to save favorite: ${filePath}`);

        const updatedFile = await firstValueFrom(
          storageService.saveFavorite(deviceId, storageType, filePath)
        );

        logInfo(LogType.Success, `API call successful for save favorite: ${filePath}`, updatedFile);

        // Update the file in the current directory if it exists
        const storageEntry = getStorage(store, key);
        if (storageEntry?.directory?.files) {
          const fileIndex = storageEntry.directory.files.findIndex(
            (file) => file.path === filePath
          );

          if (fileIndex !== -1) {
            updateState(store, actionMessage, (state) => {
              const currentEntry = state.storageEntries[key];
              if (!currentEntry?.directory?.files) {
                return state;
              }

              const updatedFiles = [...currentEntry.directory.files];
              updatedFiles[fileIndex] = {
                ...updatedFiles[fileIndex],
                isFavorite: true,
              };

              return {
                storageEntries: {
                  ...state.storageEntries,
                  [key]: {
                    ...currentEntry,
                    directory: {
                      ...currentEntry.directory,
                      files: updatedFiles,
                    },
                  },
                },
              };
            });

            logInfo(LogType.Info, `Updated file favorite flag in directory for: ${filePath}`);
          }
        }

        // Clear loading state
        updateState(store, actionMessage, (state) => ({
          favoriteOperationsState: {
            isProcessing: false,
            error: null,
          },
        }));

        logInfo(LogType.Finish, `Save favorite completed for ${filePath}`);
      } catch (error) {
        logError(`Save favorite error for ${filePath}:`, error);

        updateState(store, actionMessage, (state) => ({
          favoriteOperationsState: {
            isProcessing: false,
            error: error instanceof Error ? error.message : 'Failed to save favorite',
          },
        }));
      }
    },
  };
}
