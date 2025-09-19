import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { StorageState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import { WritableStore, removeStorage as removeStorageHelper } from '../storage-helpers';

export function removeStorage(store: WritableStore<StorageState>) {
  return {
    removeStorage: ({ deviceId, storageType }: { deviceId: string; storageType: StorageType }) => {
      const key = StorageKeyUtil.create(deviceId, storageType);
      removeStorageHelper(store, key);
    },
  };
}
