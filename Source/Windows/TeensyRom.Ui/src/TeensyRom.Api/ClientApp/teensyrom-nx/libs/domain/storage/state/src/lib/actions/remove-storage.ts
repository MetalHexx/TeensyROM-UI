import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { StorageState } from '../storage-store';
import { StorageKeyUtil } from '../storage-key.util';
import { WritableStore, removeStorage as removeStorageHelper } from '../storage-helpers';
import { createAction } from '@teensyrom-nx/utils';

export function removeStorage(store: WritableStore<StorageState>) {
  return {
    removeStorage: ({ deviceId, storageType }: { deviceId: string; storageType: StorageType }) => {
      const actionMessage = createAction('remove-storage');
      const key = StorageKeyUtil.create(deviceId, storageType);
      removeStorageHelper(store, key, actionMessage);
    },
  };
}
