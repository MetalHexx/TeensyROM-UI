import { signalStore, withState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { FileItem, LaunchMode, PlayerStatus, PlayerFilterType, PlayerScope } from '@teensyrom-nx/domain';
import { StorageKey } from '../storage/storage-key.util';
import { withPlayerSelectors } from './selectors';
import { withPlayerActions } from './actions';
import { logInfo, LogType } from '@teensyrom-nx/utils';
import { TimerState } from './timer-state.interface';

export interface LaunchedFile {
  storageKey: StorageKey;
  file: FileItem;
  parentPath: string;
  launchedAt: number;
  launchMode: LaunchMode;
  isCompatible: boolean;
}

export interface PlayerFileContext {
  storageKey: StorageKey;
  directoryPath: string;
  files: FileItem[];
  currentIndex: number;
  launchMode: LaunchMode;
}

export interface ShuffleSettings {
  scope: PlayerScope;
  filter: PlayerFilterType;
  startingDirectory?: string;
}

export interface DevicePlayerState {
  deviceId: string;
  currentFile: LaunchedFile | null;
  fileContext: PlayerFileContext | null;
  status: PlayerStatus;
  launchMode: LaunchMode;
  shuffleSettings: ShuffleSettings;
  timerState: TimerState | null;
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
