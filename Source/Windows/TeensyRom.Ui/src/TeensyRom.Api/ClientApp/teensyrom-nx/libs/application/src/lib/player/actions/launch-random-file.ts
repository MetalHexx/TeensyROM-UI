import { patchState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { createAction, LogType, logInfo, logError } from '@teensyrom-nx/utils';
import { WritableStore } from '../player-helpers';
import { PlayerState } from '../player-store';
import { IPlayerService, StorageType } from '@teensyrom-nx/domain';
import {
  setPlayerLoading,
  setPlayerLaunchSuccess,
  setPlayerError,
  ensurePlayerState,
  createLaunchedFile,
  createPlayerFileContext,
} from '../player-helpers';
import { LaunchMode, PlayerFilterType, PlayerScope } from '@teensyrom-nx/domain';

export function launchRandomFile(
  store: WritableStore<PlayerState>,
  playerService: IPlayerService
) {
  return {
    launchRandomFile: async ({ deviceId }: { deviceId: string }): Promise<void> => {
      const actionMessage = createAction('launch-random-file');

      logInfo(LogType.Start, `PlayerAction: Launching random file for device ${deviceId}`);

      try {
        // Ensure player state exists to access shuffle settings
        ensurePlayerState(store, deviceId, actionMessage);
        
        // Get current player state to access shuffle settings
        const playerState = store.players()[deviceId];
        if (!playerState) {
          throw new Error(`Player state not found for device ${deviceId}`);
        }

        const { scope, filter, startingDirectory } = playerState.shuffleSettings;

        logInfo(LogType.Info, `PlayerAction: Using shuffle settings - scope: ${scope}, filter: ${filter}, startingDirectory: ${startingDirectory || 'none'}`);

        // Set loading state
        setPlayerLoading(store, deviceId, actionMessage);

        logInfo(LogType.NetworkRequest, `PlayerAction: Requesting random file launch from player service`);

        // Launch random file using infrastructure service
        const launchedFile = await firstValueFrom(
          playerService.launchRandom(deviceId, scope, filter, startingDirectory)
        );

        logInfo(LogType.Success, `PlayerAction: Random file launched successfully: ${launchedFile.name}`);

        // For random files, we assume SD storage for now (this should be improved in future phases)
        // TODO: Random launch should return storage context or derive from scope settings
        const storageType = StorageType.Sd; // Default to SD storage for random launches

        // Create launched file object with shuffle mode
        const launchedFileObj = createLaunchedFile(
          deviceId,
          storageType,
          launchedFile,
          LaunchMode.Shuffle
        );

        // Create empty file context for shuffle mode (directory coordination happens at PlayerContext level)
        const emptyFileContext = createPlayerFileContext(
          deviceId,
          storageType,
          launchedFile.parentPath || '/',
          [], // Empty files array - context coordination happens at PlayerContext level
          -1,
          LaunchMode.Shuffle
        );

        // Update state with success
        setPlayerLaunchSuccess(store, deviceId, launchedFileObj, emptyFileContext, actionMessage);

        // Ensure launch mode is set to shuffle
        patchState(store, (state) => ({
          players: {
            ...state.players,
            [deviceId]: {
              ...state.players[deviceId],
              launchMode: LaunchMode.Shuffle,
            },
          },
        }));

        logInfo(LogType.Finish, `PlayerAction: Random file launch completed for device ${deviceId}`);

      } catch (error) {
        const errorMessage = (error as any)?.message || 'Failed to launch random file';
        logError(`PlayerAction: Random file launch failed for device ${deviceId}:`, error);
        setPlayerError(store, deviceId, errorMessage, actionMessage);
      }
    },
  };
}