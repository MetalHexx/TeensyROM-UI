import { updateState } from '@angular-architects/ngrx-toolkit';
import { firstValueFrom } from 'rxjs';
import { createAction, LogType, logInfo, logError } from '@teensyrom-nx/utils';
import { WritableStore } from '../player-helpers';
import { PlayerState } from '../player-store';
import { IPlayerService, StorageType } from '@teensyrom-nx/domain';
import {
  setPlayerLoading,
  setPlayerLaunchSuccess,
  setPlayerLaunchFailure,
  setPlayerError,
  ensurePlayerState,
  createLaunchedFile,
  createPlayerFileContext,
} from '../player-helpers';
import { LaunchMode, PlayerFilterType, PlayerScope } from '@teensyrom-nx/domain';

export function launchRandomFile(store: WritableStore<PlayerState>, playerService: IPlayerService) {
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

        logInfo(
          LogType.Info,
          `PlayerAction: Using shuffle settings - scope: ${scope}, filter: ${filter}, startingDirectory: ${
            startingDirectory || 'none'
          }`
        );

        // Set loading state
        setPlayerLoading(store, deviceId, actionMessage);

        logInfo(
          LogType.NetworkRequest,
          `PlayerAction: Requesting random file launch from player service`
        );

        // Launch random file using infrastructure service
        const launchedFile = await firstValueFrom(
          playerService.launchRandom(deviceId, scope, filter, startingDirectory)
        );

        logInfo(
          LogType.Success,
          `PlayerAction: Random file launched successfully: ${launchedFile.name}`
        );

        // For random files, we assume SD storage for now (this should be improved in future phases)
        // TODO: Random launch should return storage context or derive from scope settings
        const storageType = StorageType.Sd; // Default to SD storage for random launches

        // Check if file is compatible with hardware
        const isCompatible = launchedFile.isCompatible;

        // Create launched file object with shuffle mode
        const launchedFileObj = createLaunchedFile(
          deviceId,
          storageType,
          launchedFile,
          isCompatible
        );

        // Create empty file context for shuffle mode (directory coordination happens at PlayerContext level)
        const emptyFileContext = createPlayerFileContext(
          deviceId,
          storageType,
          launchedFile.parentPath || '/',
          [], // Empty files array - context coordination happens at PlayerContext level
          -1
        );

        // If file is incompatible, treat as failure but with file context preserved
        if (!isCompatible) {
          const errorMessage = 'File is not compatible with TeensyROM hardware';
          logError(
            `PlayerAction: Random file ${launchedFile.name} is incompatible with device ${deviceId}: ${errorMessage}`
          );
          setPlayerLaunchFailure(
            store,
            deviceId,
            launchedFileObj,
            emptyFileContext,
            LaunchMode.Shuffle,
            errorMessage,
            actionMessage
          );
          return;
        }

        // Update state with success
        setPlayerLaunchSuccess(
          store,
          deviceId,
          launchedFileObj,
          emptyFileContext,
          LaunchMode.Shuffle,
          actionMessage
        );

        logInfo(
          LogType.Finish,
          `PlayerAction: Random file launch completed for device ${deviceId}`
        );
      } catch (error) {
        const errorMessage = (error as Error)?.message || 'Failed to launch random file';
        logError(`PlayerAction: Random file launch failed for device ${deviceId}:`, error);
        setPlayerError(store, deviceId, errorMessage, actionMessage);
      }
    },
  };
}
