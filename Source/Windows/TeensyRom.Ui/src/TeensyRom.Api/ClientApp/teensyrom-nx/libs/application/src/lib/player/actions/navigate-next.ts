import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { LaunchMode, PlayerStatus, StorageType } from '@teensyrom-nx/domain';
import { IPlayerService } from '@teensyrom-nx/domain';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';
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
          // In shuffle mode, next launches a random file - duplicate launchRandom logic
          logInfo(LogType.Info, `Shuffle mode: launching random file for ${deviceId}`);

          const randomFile = await firstValueFrom(
            playerService.launchRandom(deviceId, shuffleSettings.scope, shuffleSettings.filter, shuffleSettings.startingDirectory)
          );

          // Get storage type from current player state since randomFile doesn't include it
          const existingStorageKey = playerState.currentFile?.storageKey;
          const storageKey = existingStorageKey || StorageKeyUtil.create(deviceId, StorageType.Sd); // Default to SD if no existing key

          updateState(store, actionMessage, (state) => ({
            players: {
              ...state.players,
              [deviceId]: {
                ...state.players[deviceId],
                currentFile: {
                  storageKey,
                  file: randomFile,
                  parentPath: randomFile.path.substring(0, randomFile.path.lastIndexOf('/')) || '/',
                  launchedAt: Date.now(),
                  launchMode: LaunchMode.Shuffle,
                },
                status: PlayerStatus.Playing, // Navigation continues playback
                error: null,
                lastUpdated: Date.now(),
              },
            },
          }));

        } else if (launchMode === LaunchMode.Directory && fileContext) {
          // In directory mode, advance to next file with wraparound - duplicate launchFileWithContext logic
          const { files, currentIndex, storageKey, directoryPath } = fileContext;
          const nextIndex = (currentIndex + 1) % files.length; // Wraparound
          const nextFile = files[nextIndex];
          
          logInfo(LogType.Info, `Directory mode: advancing to next file (${nextIndex + 1}/${files.length}) for ${deviceId}`, { nextFile: nextFile.name });

          const { storageType } = StorageKeyUtil.parse(storageKey);
          
          // Launch the file via API
          const launchedFile = await firstValueFrom(
            playerService.launchFile(deviceId, storageType, nextFile.path)
          );

          // Update state with launched file and updated context
          updateState(store, actionMessage, (state) => ({
            players: {
              ...state.players,
              [deviceId]: {
                ...state.players[deviceId],
                currentFile: {
                  storageKey,
                  file: launchedFile,
                  parentPath: directoryPath,
                  launchedAt: Date.now(),
                  launchMode: LaunchMode.Directory,
                },
                fileContext: {
                  ...fileContext,
                  currentIndex: nextIndex, // Update the index
                },
                status: PlayerStatus.Playing, // Navigation continues playback
                error: null,
                lastUpdated: Date.now(),
              },
            },
          }));

        } else {
          logInfo(LogType.Info, `No file context available for navigation on ${deviceId}`);
        }

        logInfo(LogType.Finish, `Navigate next completed for ${deviceId}`);

      } catch (error) {
        const errorMessage = (error as any)?.message || 'Failed to navigate to next file';
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