import { firstValueFrom } from 'rxjs';
import { createAction, logInfo, logError, LogType } from '@teensyrom-nx/utils';
import { IPlayerService, LaunchMode, StorageType, FileItem } from '@teensyrom-nx/domain';
import { PlayerState } from '../player-store';
import {
  WritableStore,
  ensurePlayerState,
  setPlayerLoading,
  setPlayerLaunchSuccess,
  setPlayerLaunchFailure,
  createLaunchedFile,
  createPlayerFileContext,
} from '../player-helpers';

export interface LaunchFileWithContextParams {
  deviceId: string;
  storageType: StorageType;
  file: FileItem;
  directoryPath: string;
  files: FileItem[];
  launchMode?: LaunchMode;
}

export function launchFileWithContext(
  store: WritableStore<PlayerState>,
  playerService: IPlayerService
) {
  return {
    launchFileWithContext: async ({
      deviceId,
      storageType,
      file,
      directoryPath,
      files,
      launchMode = LaunchMode.Directory,
    }: LaunchFileWithContextParams): Promise<void> => {
      logInfo(LogType.Start, `PlayerAction: Launching file ${file.name} for device ${deviceId}`);

      const actionMessage = createAction('launch-file-with-context');
      ensurePlayerState(store, deviceId, actionMessage);
      setPlayerLoading(store, deviceId, actionMessage);

      try {
        logInfo(LogType.NetworkRequest, `PlayerAction: Requesting file launch from player service`);

        const launched = await firstValueFrom(
          playerService.launchFile(deviceId, storageType, file.path)
        );

        const resolvedFile = launched ?? file;
        const contextFiles = [...files];
        const currentIndex = contextFiles.findIndex((candidate) => candidate.path === file.path);
        const safeIndex = currentIndex >= 0 ? currentIndex : 0;
        const resolvedDirectoryPath = directoryPath ?? '';

        const launchedFile = createLaunchedFile(deviceId, storageType, resolvedFile, launchMode);
        const fileContext = createPlayerFileContext(
          deviceId,
          storageType,
          resolvedDirectoryPath,
          contextFiles,
          safeIndex,
          launchMode
        );

        setPlayerLaunchSuccess(store, deviceId, launchedFile, fileContext, actionMessage);

        logInfo(LogType.Success, `PlayerAction: File ${resolvedFile.name} launched successfully for device ${deviceId}`);
      } catch (error) {
        const errorMessage = (error as Error)?.message ?? 'Failed to launch file';
        
        logError(`PlayerAction: Failed to launch file ${file.name} for device ${deviceId}: ${errorMessage}`);

        // Create file context even on failure so UI can show which file failed
        // and directory context is available for shuffle mode
        const launchedFile = createLaunchedFile(deviceId, storageType, file, launchMode);
        const safeIndex = Math.max(0, files.findIndex((f) => f.name === file.name));
        const fileContext = createPlayerFileContext(
          deviceId,
          storageType,
          directoryPath,
          files,
          safeIndex,
          launchMode
        );

        setPlayerLaunchFailure(store, deviceId, launchedFile, fileContext, errorMessage, actionMessage);
        // Do not throw - error state is set in store for service layer to check
      }
    },
  };
}

