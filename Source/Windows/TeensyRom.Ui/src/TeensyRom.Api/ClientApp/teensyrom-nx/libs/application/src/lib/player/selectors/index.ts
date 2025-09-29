import { withMethods } from '@ngrx/signals';
import { PlayerState } from '../player-store';
import { WritableStore } from '../player-helpers';
import { getDevicePlayer } from './get-device-player';
import { getCurrentFile } from './get-current-file';
import { getPlayerFileContext } from './get-player-file-context';
import { isPlayerLoading } from './is-player-loading';
import { getPlayerError } from './get-player-error';
import { getPlayerStatus } from './get-player-status';
import { getShuffleSettings } from './get-shuffle-settings';
import { getLaunchMode } from './get-launch-mode';

export function withPlayerSelectors() {
  return withMethods((store) => {
    const writableStore = store as WritableStore<PlayerState>;
    return {
      ...getDevicePlayer(writableStore),
      ...getCurrentFile(writableStore),
      ...getPlayerFileContext(writableStore),
      ...isPlayerLoading(writableStore),
      ...getPlayerError(writableStore),
      ...getPlayerStatus(writableStore),
      ...getShuffleSettings(writableStore),
      ...getLaunchMode(writableStore),
    };
  });
}
