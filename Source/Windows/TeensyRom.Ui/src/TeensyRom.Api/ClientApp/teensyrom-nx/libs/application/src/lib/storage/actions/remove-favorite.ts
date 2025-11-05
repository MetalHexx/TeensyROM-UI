import { firstValueFrom } from 'rxjs';
import { StorageType, IStorageService } from '@teensyrom-nx/domain';
import { StorageState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import { WritableStore, getStorage } from '../storage-helpers';
import { LogType, logInfo, logError, createAction } from '@teensyrom-nx/utils';
import { updateState } from '@angular-architects/ngrx-toolkit';

export function removeFavorite(
  store: WritableStore<StorageState>,
  storageService: IStorageService
) {
  return {
    removeFavorite: async ({
      deviceId,
      storageType,
      filePath,
    }: {
      deviceId: string;
      storageType: StorageType;
      filePath: string;
    }): Promise<void> => {
      const actionMessage = createAction('remove-favorite');
      const key = StorageKeyUtil.create(deviceId, storageType);

      logInfo(LogType.Start, `Starting remove favorite for ${filePath} on ${key}`);

      // Set loading state
      updateState(store, actionMessage, (state) => ({
        favoriteOperationsState: {
          isProcessing: true,
          error: null,
        },
      }));

      try {
        logInfo(LogType.NetworkRequest, `Making API call to remove favorite: ${filePath}`);

        await firstValueFrom(storageService.removeFavorite(deviceId, storageType, filePath));

        logInfo(LogType.Success, `API call successful for remove favorite: ${filePath}`);

        // Handle favorites directory case - remove file from listing
        if (filePath.startsWith('/favorites/')) {
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
                updatedFiles.splice(fileIndex, 1);

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

              logInfo(LogType.Info, `Removed file from favorites directory: ${filePath}`);
            }
          }
        } else {
          // Non-favorites directory case - update file's isFavorite flag
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
                  isFavorite: false,
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
        }

        // Clear loading state
        updateState(store, actionMessage, (state) => ({
          favoriteOperationsState: {
            isProcessing: false,
            error: null,
          },
        }));

        logInfo(LogType.Finish, `Remove favorite completed for ${filePath}`);
      } catch (error) {
        logError(`Remove favorite error for ${filePath}:`, error);

        updateState(store, actionMessage, (state) => ({
          favoriteOperationsState: {
            isProcessing: false,
            error: error instanceof Error ? error.message : 'Failed to remove favorite',
          },
        }));
      }
    },
  };
}
