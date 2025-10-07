import { StorageType } from '@teensyrom-nx/domain';
import { StorageState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import { WritableStore } from '../storage-helpers';
import { createAction, LogType, logInfo } from '@teensyrom-nx/utils';
import { updateState } from '@angular-architects/ngrx-toolkit';

export function clearSearch(store: WritableStore<StorageState>) {
  return {
    clearSearch: ({
      deviceId,
      storageType,
    }: {
      deviceId: string;
      storageType: StorageType;
    }): void => {
      const actionMessage = createAction('clear-search');
      const key = StorageKeyUtil.create(deviceId, storageType);

      logInfo(LogType.Info, `Clearing search state for ${key}`);

      updateState(store, actionMessage, (state) => {
        const updatedSearchState = { ...state.searchState };
        delete updatedSearchState[key];

        return {
          searchState: updatedSearchState,
        };
      });
    },
  };
}
