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

        const launchedFile = await firstValueFrom(
          playerService.launchFile(deviceId, storageType, file.path)
        );

        const resolvedFile = launchedFile ?? file;
        const contextFiles = [...files];
        const currentIndex = contextFiles.findIndex((candidate) => candidate.path === file.path);
        const safeIndex = currentIndex >= 0 ? currentIndex : 0;
        const resolvedDirectoryPath = directoryPath ?? '';

        // Check if file is compatible with hardware
        const isCompatible = resolvedFile.isCompatible;
        const launchedFileObj = createLaunchedFile(
          deviceId,
          storageType,
          resolvedFile,
          isCompatible
        );
        const fileContext = createPlayerFileContext(
          deviceId,
          storageType,
          resolvedDirectoryPath,
          contextFiles,
          safeIndex
        );

        // If file is incompatible, treat as failure but with file context preserved
        if (!isCompatible) {
          const errorMessage = 'File is not compatible with TeensyROM hardware';
          logError(
            `PlayerAction: File ${resolvedFile.name} is incompatible with device ${deviceId}: ${errorMessage}`
          );
          setPlayerLaunchFailure(
            store,
            deviceId,
            launchedFileObj,
            fileContext,
            launchMode,
            errorMessage,
            actionMessage
          );
          return;
        }

        setPlayerLaunchSuccess(
          store,
          deviceId,
          launchedFileObj,
          fileContext,
          launchMode,
          actionMessage
        );

        logInfo(
          LogType.Success,
          `PlayerAction: File ${resolvedFile.name} launched successfully for device ${deviceId}`
        );
      } catch (error) {
        const errorMessage = (error as Error)?.message ?? 'Failed to launch file';

        logError(
          `PlayerAction: Failed to launch file ${file.name} for device ${deviceId}: ${errorMessage}`
        );

        // Network/HTTP error - create file context from request so UI can show which file failed
        const launchedFile = createLaunchedFile(deviceId, storageType, file, false);
        const safeIndex = Math.max(
          0,
          files.findIndex((f) => f.name === file.name)
        );
        const fileContext = createPlayerFileContext(
          deviceId,
          storageType,
          directoryPath,
          files,
          safeIndex
        );

        setPlayerLaunchFailure(
          store,
          deviceId,
          launchedFile,
          fileContext,
          launchMode,
          errorMessage,
          actionMessage
        );
        // Do not throw - error state is set in store for service layer to check
      }
    },
  };
}
