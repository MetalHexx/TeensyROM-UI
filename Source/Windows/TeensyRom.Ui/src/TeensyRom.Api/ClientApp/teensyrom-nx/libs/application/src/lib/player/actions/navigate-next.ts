import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { LaunchMode, PlayerStatus } from '@teensyrom-nx/domain';
import { IPlayerService } from '@teensyrom-nx/domain';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import {
  WritableStore,
  setShuffleNavigationSuccess,
  setShuffleNavigationFailure,
  setDirectoryNavigationSuccess,
  setDirectoryNavigationFailure,
} from '../player-helpers';
import { StorageKeyUtil } from '../../storage/storage-key.util';

export function navigateNext(store: WritableStore<PlayerState>, playerService: IPlayerService) {
  return {
    navigateNext: async ({ deviceId }: { deviceId: string }): Promise<void> => {
      const actionMessage = createAction('navigate-next');
      
      logInfo(LogType.Start, `Navigating to next file for ${deviceId}`, { deviceId, actionMessage });

      const currentState = store.players();
      const playerState = currentState[deviceId];
      
      if (!playerState) {
        logError(`No player state found for device ${deviceId}`);
        return;
      }

      const { launchMode, fileContext, shuffleSettings } = playerState;

      try {
        if (launchMode === LaunchMode.Shuffle) {
          // In shuffle mode, next launches a random file
          logInfo(LogType.Info, `Shuffle mode: launching random file for ${deviceId}`);

          const launchedFile = await firstValueFrom(
            playerService.launchRandom(deviceId, shuffleSettings.scope, shuffleSettings.filter, shuffleSettings.startingDirectory)
          );

          const existingStorageKey = playerState.currentFile?.storageKey;
          const isCompatible = launchedFile.isCompatible;
          
          if (!isCompatible) {
            const errorMessage = 'File is not compatible with TeensyROM hardware';
            logError(`Navigate next: Random file ${launchedFile.name} is incompatible with device ${deviceId}: ${errorMessage}`);
            setShuffleNavigationFailure(store, deviceId, launchedFile, existingStorageKey, errorMessage, actionMessage);
            return;
          }

          setShuffleNavigationSuccess(store, deviceId, launchedFile, existingStorageKey, actionMessage);

        } else if ((launchMode === LaunchMode.Directory || launchMode === LaunchMode.Search) && fileContext) {
          const modeLabel = launchMode === LaunchMode.Search ? 'Search' : 'Directory';
          const { files, currentIndex, storageKey } = fileContext;
          const nextIndex = (currentIndex + 1) % files.length; // Wraparound
          const nextFile = files[nextIndex];
          
          logInfo(LogType.Info, `${modeLabel} mode: advancing to next file (${nextIndex + 1}/${files.length}) for ${deviceId}`, { nextFile: nextFile.name });

          const { storageType } = StorageKeyUtil.parse(storageKey);
          
          // Launch the file via API
          const launchedFile = await firstValueFrom(
            playerService.launchFile(deviceId, storageType, nextFile.path)
          );

          const isCompatible = launchedFile.isCompatible;
          
          if (!isCompatible) {
            const errorMessage = 'File is not compatible with TeensyROM hardware';
            logError(`Navigate next: File ${launchedFile.name} is incompatible with device ${deviceId}: ${errorMessage}`);
            setDirectoryNavigationFailure(store, deviceId, launchedFile, fileContext, nextIndex, errorMessage, actionMessage);
            return;
          }

          setDirectoryNavigationSuccess(store, deviceId, launchedFile, fileContext, nextIndex, actionMessage);

        } else {
          logInfo(LogType.Info, `No file context available for navigation on ${deviceId}`);
        }

        logInfo(LogType.Finish, `Navigate next completed for ${deviceId}`);

      } catch (error) {
        const errorMessage = (error as Error)?.message || 'Failed to navigate to next file';
        logError(`Navigate next failed for ${deviceId}:`, error);

        updateState(store, actionMessage, (state) => ({
          players: {
            ...state.players,
            [deviceId]: {
              ...state.players[deviceId],
              status: PlayerStatus.Stopped,
              error: errorMessage,
              lastUpdated: Date.now(),
            },
          },
        }));
      }
    },
  };
}