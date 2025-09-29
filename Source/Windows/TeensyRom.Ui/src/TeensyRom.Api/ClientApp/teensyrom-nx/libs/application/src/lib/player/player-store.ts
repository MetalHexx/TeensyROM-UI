import { signalStore, withState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { FileItem, LaunchMode, PlayerStatus } from '@teensyrom-nx/domain';
import { StorageKey } from '../storage/storage-key.util';
import { withPlayerSelectors } from './selectors';
import { withPlayerActions } from './actions';
import { logInfo, LogType } from '@teensyrom-nx/utils';

export interface LaunchedFile {
  storageKey: StorageKey;
  file: FileItem;
  launchedAt: number;
  launchMode: LaunchMode;
}

export interface PlayerFileContext {
  storageKey: StorageKey;
  directoryPath: string;
  files: FileItem[];
  currentIndex: number;
  launchMode: LaunchMode;
}

export interface DevicePlayerState {
  deviceId: string;
  currentFile: LaunchedFile | null;
  fileContext: PlayerFileContext | null;
  status: PlayerStatus;
  launchMode: LaunchMode;
  isLoading: boolean;
  error: string | null;
  lastUpdated: number | null;
}

export interface PlayerState {
  players: Record<string, DevicePlayerState>;
}

const initialState: PlayerState = {
  players: {},
};
logInfo(LogType.Start, 'PlayerStore: Initializing player state management store');

export const PlayerStore = signalStore(
  { providedIn: 'root' },
  withDevtools('player'),
  withState(initialState),
  withPlayerSelectors(),
  withPlayerActions()
);

logInfo(LogType.Success, 'PlayerStore: Player store configured successfully');
