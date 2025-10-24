import { updateState } from '@angular-architects/ngrx-toolkit';
import { LogType, createAction, logInfo, logWarn } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';

interface UpdateFavoriteStatusParams {
  deviceId: string;
  filePath: string;
  isFavorite: boolean;
}

export function updateFavoriteStatus(store: WritableStore<PlayerState>) {
  return {
    updateCurrentFileFavoriteStatus: ({ deviceId, filePath, isFavorite }: UpdateFavoriteStatusParams): void => {
      const actionMessage = createAction('update-current-file-favorite-status');

      logInfo(
        LogType.Start,
        `PlayerStore: Updating favorite status for ${filePath} on device ${deviceId}`,
        { deviceId, filePath, isFavorite, actionMessage }
      );

      const currentState = store.players();
      const deviceState = currentState[deviceId];

      if (!deviceState) {
        logWarn(
          `PlayerStore: Skipping favorite update for ${filePath} - device ${deviceId} is not initialized`
        );
        return;
      }

      const { currentFile, fileContext } = deviceState;
      let currentFileUpdated = false;
      let fileContextUpdated = false;

      const updatedCurrentFile = currentFile && currentFile.file.path === filePath
        ? (() => {
            if (currentFile.file.isFavorite === isFavorite) {
              return currentFile;
            }

            currentFileUpdated = true;
            return {
              ...currentFile,
              file: {
                ...currentFile.file,
                isFavorite,
              },
            };
          })()
        : currentFile;

      const updatedFileContext = fileContext
        ? (() => {
            if (!fileContext.files?.length) {
              return fileContext;
            }

            const updatedFiles = fileContext.files.map((file) => {
              if (file.path !== filePath) {
                return file;
              }

              if (file.isFavorite === isFavorite) {
                return file;
              }

              fileContextUpdated = true;
              return {
                ...file,
                isFavorite,
              };
            });

            return fileContextUpdated
              ? {
                  ...fileContext,
                  files: updatedFiles,
                }
              : fileContext;
          })()
        : fileContext;

      if (!currentFileUpdated && !fileContextUpdated) {
        logInfo(
          LogType.Info,
          `PlayerStore: No favorite update required for ${filePath} on device ${deviceId} (state already synchronized)`
        );
        return;
      }

      updateState(store, actionMessage, (state) => {
        const existingDeviceState = state.players[deviceId];
        if (!existingDeviceState) {
          return state;
        }

        return {
          players: {
            ...state.players,
            [deviceId]: {
              ...existingDeviceState,
              currentFile: updatedCurrentFile,
              fileContext: updatedFileContext,
              lastUpdated: Date.now(),
            },
          },
        };
      });

      logInfo(
        LogType.Success,
        `PlayerStore: Favorite status updated for ${filePath} on device ${deviceId}`,
        { deviceId, filePath, isFavorite }
      );
    },
  };
}

