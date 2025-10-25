import { inject } from '@angular/core';
import { ResolveFn, ActivatedRouteSnapshot } from '@angular/router';
import { StorageStore, PlayerContextService, DeviceStore } from '@teensyrom-nx/application';
import { StorageType, LaunchMode } from '@teensyrom-nx/domain';
import { logInfo, logError, logWarn, LogType } from '@teensyrom-nx/utils';

/**
 * Route resolver for player route that kicks off storage initialization
 * without blocking navigation.
 *
 * Execution flow:
 * 1. Start background initialization of all device storage
 * 2. Check for deep linking query parameters
 * 3. Return immediately - component renders while storage loads
 *
 * The component will show loading states as storage data arrives.
 *
 * Query Parameters (optional, for deep linking):
 * - `device`: Device ID to navigate to (required for deep linking)
 * - `storage`: Storage type 'SD' or 'USB' (required for deep linking)
 * - `path`: Directory path to open (required for deep linking)
 * - `file`: File name to launch (optional, auto-launch if provided)
 *
 * @returns Promise<void> - resolves immediately, initialization continues in background
 */
export const playerRouteResolver: ResolveFn<void> = async (
  route: ActivatedRouteSnapshot
): Promise<void> => {
  const storageStore = inject(StorageStore);
  const deviceStore = inject(DeviceStore);
  const playerContextService = inject(PlayerContextService);

  // Kick off initialization in background (fire-and-forget)
  initPlayer(storageStore, deviceStore, playerContextService, route);

  logInfo(LogType.Info, 'Player route resolver: initialization started (non-blocking)');

  // Return immediately - component renders while storage loads
};

/**
 * Orchestrator function that coordinates player initialization.
 * Calls sub-initialization functions in sequence:
 * 1. Wait for DeviceStore to be initialized
 * 2. Initialize all device storage
 * 3. Handle deep linking if query parameters provided
 */
async function initPlayer(
  storageStore: InstanceType<typeof StorageStore>,
  deviceStore: InstanceType<typeof DeviceStore>,
  playerContextService: PlayerContextService,
  route: ActivatedRouteSnapshot
): Promise<void> {
  await waitForDeviceStoreInitialization(deviceStore);
  await initializeAllDeviceStorage(storageStore, deviceStore);
  await initDeeplinking(storageStore, playerContextService, route);
}

/**
 * Wait for DeviceStore to finish initializing.
 * Polls hasInitialised until it becomes true.
 */
async function waitForDeviceStoreInitialization(
  deviceStore: InstanceType<typeof DeviceStore>
): Promise<void> {
  logInfo(LogType.Start, 'Waiting for DeviceStore to initialize');

  while (!deviceStore.hasInitialised()) {
    await new Promise(resolve => setTimeout(resolve, 50));
  }

  logInfo(LogType.Success, `DeviceStore initialized: ${deviceStore.devices().length} device(s) found`);
}

/**
 * Async function that initializes all storage for all connected devices.
 * This runs in the background without blocking navigation.
 */
async function initializeAllDeviceStorage(
  storageStore: InstanceType<typeof StorageStore>,
  deviceStore: InstanceType<typeof DeviceStore>
): Promise<void> {
  try {
    logInfo(LogType.Start, 'Initializing all device storage');

    // Get devices from the DeviceStore
    const devices = deviceStore.devices();
    logInfo(LogType.Info, `Found ${devices.length} connected devices`, devices.length);

    // Initialize all storage types for all devices
    for (const device of devices) {
      const deviceId = device.deviceId;

      if (device.usbStorage?.available) {
        await storageStore.initializeStorage({
          deviceId,
          storageType: StorageType.Usb,
        });
        logInfo(LogType.Info, `Initialized USB storage for ${deviceId}`);
      }

      if (device.sdStorage?.available) {
        await storageStore.initializeStorage({
          deviceId,
          storageType: StorageType.Sd,
        });
        logInfo(LogType.Info, `Initialized SD storage for ${deviceId}`);
      }
    }

    logInfo(LogType.Success, 'All device storage initialized');
  } catch (error) {
    logError(
      'Error initializing device storage:',
      error instanceof Error ? error.message : error
    );
  }
}

/**
 * Handle deep linking based on query parameters.
 * Gathers route parameters (device, storage, path, file) and:
 * 1. Navigates to the specified directory
 * 2. If file provided, waits for directory load and launches the file
 *
 * Query Parameters:
 * - `device`: Device ID (required for deep linking)
 * - `storage`: Storage type 'SD' or 'USB' (required for deep linking)
 * - `path`: Directory path to open (required for deep linking)
 * - `file`: File name to launch (optional)
 */
async function initDeeplinking(
  storageStore: InstanceType<typeof StorageStore>,
  playerContextService: PlayerContextService,
  route: ActivatedRouteSnapshot
): Promise<void> {
  try {
    // Gather route parameters
    const device = route.queryParamMap.get('device');
    const storage = route.queryParamMap.get('storage');
    const path = route.queryParamMap.get('path');
    const file = route.queryParamMap.get('file');

    // Return early if required parameters are missing
    if (!device || !storage || !path) {
      logInfo(LogType.Info, 'No deep linking parameters provided');
      return;
    }

    logInfo(LogType.Start, 'Deep linking initialization', {
      device,
      storage,
      path,
      file: file ?? 'none',
    });

    // Convert storage string to StorageType enum
    const storageType = storage.toUpperCase() === 'USB' ? StorageType.Usb : StorageType.Sd;

    // Navigate to the specified directory
    await storageStore.navigateToDirectory({
      deviceId: device,
      storageType,
      path,
    });

    logInfo(LogType.Info, `Navigated to directory: ${path}`);

    // If file name provided, find and launch it
    if (file) {
      logInfo(LogType.Info, `File launch requested: ${file}`);

      // Get the loaded directory
      const directoryState = storageStore.getSelectedDirectoryState(device)();

      if (directoryState?.directory?.files) {
        // Find the file in the directory
        const targetFile = directoryState.directory.files.find(f => f.name === file);

        if (targetFile) {
          logInfo(LogType.Info, `Found file: ${file}, launching...`);

          // Launch the file with directory context
          await playerContextService.launchFileWithContext({
            deviceId: device,
            storageType,
            file: targetFile,
            directoryPath: path,
            files: directoryState.directory.files,
            launchMode: LaunchMode.Directory,
          });

          logInfo(LogType.Success, `File launched: ${file}`);
        } else {
          logWarn(`File not found in directory: ${file}`);
        }
      } else {
        logWarn(`Directory not loaded or has no files: ${path}`);
      }
    }

    logInfo(LogType.Success, 'Deep linking initialization completed');
  } catch (error) {
    logError(
      'Error during deep linking initialization:',
      error instanceof Error ? error.message : error
    );
  }
}
