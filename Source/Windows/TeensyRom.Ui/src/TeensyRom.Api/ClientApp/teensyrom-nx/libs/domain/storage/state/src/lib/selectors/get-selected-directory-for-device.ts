import { StorageState, SelectedDirectory } from '../storage-store';
import { WritableStore } from '../storage-helpers';

export function getSelectedDirectoryForDevice(store: WritableStore<StorageState>) {
  return {
    getSelectedDirectoryForDevice: (deviceId: string): SelectedDirectory | null => {
      return store.selectedDirectories()[deviceId] ?? null;
    },
  };
}
