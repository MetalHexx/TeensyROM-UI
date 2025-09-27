import { withMethods, WritableStateSource } from '@ngrx/signals';
import { getSelectedDirectoryForDevice } from './get-selected-directory-for-device';
import { getSelectedDirectoryState } from './get-selected-directory-state';
import { getDeviceStorageEntries } from './get-device-storage-entries';
import { getDeviceDirectories } from './get-device-directories';
import { StorageState } from '../storage-store';
import { WritableStore } from '../storage-helpers';

export function withStorageSelectors() {
  return withMethods((store) => {
    const writableStore = store as WritableStore<StorageState>;
    return {
      ...getSelectedDirectoryForDevice(writableStore),
      ...getSelectedDirectoryState(writableStore),
      ...getDeviceStorageEntries(writableStore),
      ...getDeviceDirectories(writableStore),
    };
  });
}
