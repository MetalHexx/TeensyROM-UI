import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { LaunchMode, PlayerStatus, StorageType } from '@teensyrom-nx/domain';
import { IPlayerService } from '@teensyrom-nx/domain';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';
import { StorageKeyUtil } from '../../storage/storage-key.util';

export function navigatePrevious(store: WritableStore<PlayerState>, playerService: IPlayerService) {
  return {
    navigatePrevious: async ({ deviceId }: { deviceId: string }): Promise<void> => {
      const actionMessage = createAction('navigate-previous');
      
      logInfo(LogType.Start, `Navigating to previous file for ${deviceId}`, { deviceId, actionMessage });

      const currentState = store.players();
      const playerState = currentState[deviceId];
      
      if (!playerState) {
        logError(`No player state found for device ${deviceId}`);
        return;
      }

      const { launchMode, fileContext, shuffleSettings } = playerState;

      try {
        if (launchMode === LaunchMode.Shuffle) {
          // In shuffle mode, previous launches another random file (hardcoded behavior) - duplicate launchRandom logic
          logInfo(LogType.Info, `Shuffle mode: launching another random file for previous on ${deviceId}`);

          const launchedFile = await firstValueFrom(
            playerService.launchRandom(deviceId, shuffleSettings.scope, shuffleSettings.filter, shuffleSettings.startingDirectory)
          );

          // Get storage type from current player state since launchedFile doesn't include it
          const existingStorageKey = playerState.currentFile?.storageKey;
          const storageKey = existingStorageKey || StorageKeyUtil.create(deviceId, StorageType.Sd); // Default to SD if no existing key

          // Check if file is compatible with hardware
          const isCompatible = launchedFile.isCompatible;
          
          // If incompatible, set error state
          if (!isCompatible) {
            const errorMessage = 'File is not compatible with TeensyROM hardware';
            logError(`Navigate previous: Random file ${launchedFile.name} is incompatible with device ${deviceId}: ${errorMessage}`);
            
            updateState(store, actionMessage, (state) => ({
              players: {
                ...state.players,
                [deviceId]: {
                  ...state.players[deviceId],
                  currentFile: {
                    storageKey,
                    file: launchedFile,
                    parentPath: launchedFile.path.substring(0, launchedFile.path.lastIndexOf('/')) || '/',
                    launchedAt: Date.now(),
                    launchMode: LaunchMode.Shuffle,
                    isCompatible,
                  },
                  status: PlayerStatus.Stopped,
                  error: errorMessage,
                  lastUpdated: Date.now(),
                },
              },
            }));
            return;
          }

          updateState(store, actionMessage, (state) => ({
            players: {
              ...state.players,
              [deviceId]: {
                ...state.players[deviceId],
                currentFile: {
                  storageKey,
                  file: launchedFile,
                  parentPath: launchedFile.path.substring(0, launchedFile.path.lastIndexOf('/')) || '/',
                  launchedAt: Date.now(),
                  launchMode: LaunchMode.Shuffle,
                  isCompatible,
                },
                status: PlayerStatus.Playing, // Navigation continues playback
                error: null,
                lastUpdated: Date.now(),
              },
            },
          }));

        } else if (launchMode === LaunchMode.Directory && fileContext) {
          // In directory mode, go to previous file with wraparound - duplicate launchFileWithContext logic
          const { files, currentIndex, storageKey, directoryPath } = fileContext;
          const previousIndex = currentIndex === 0 ? files.length - 1 : currentIndex - 1; // Wraparound
          const previousFile = files[previousIndex];
          
          logInfo(LogType.Info, `Directory mode: going to previous file (${previousIndex + 1}/${files.length}) for ${deviceId}`, { previousFile: previousFile.name });

          const { storageType } = StorageKeyUtil.parse(storageKey);
          
          // Launch the file via API
          const launchedFile = await firstValueFrom(
            playerService.launchFile(deviceId, storageType, previousFile.path)
          );

          // Check if file is compatible with hardware
          const isCompatible = launchedFile.isCompatible;
          
          // If incompatible, set error state
          if (!isCompatible) {
            const errorMessage = 'File is not compatible with TeensyROM hardware';
            logError(`Navigate previous: File ${launchedFile.name} is incompatible with device ${deviceId}: ${errorMessage}`);
            
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
                    isCompatible,
                  },
                  fileContext: {
                    ...fileContext,
                    currentIndex: previousIndex,
                  },
                  status: PlayerStatus.Stopped,
                  error: errorMessage,
                  lastUpdated: Date.now(),
                },
              },
            }));
            return;
          }

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
                  isCompatible,
                },
                fileContext: {
                  ...fileContext,
                  currentIndex: previousIndex, // Update the index
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

        logInfo(LogType.Finish, `Navigate previous completed for ${deviceId}`);

      } catch (error) {
        const errorMessage = (error as Error)?.message || 'Failed to navigate to previous file';
        logError(`Navigate previous failed for ${deviceId}:`, error);

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