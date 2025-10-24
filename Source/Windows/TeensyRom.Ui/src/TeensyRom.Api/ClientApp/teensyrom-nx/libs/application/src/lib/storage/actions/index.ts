import { inject } from '@angular/core';
import { withMethods } from '@ngrx/signals';
import { IStorageService, STORAGE_SERVICE } from '@teensyrom-nx/domain';

import { StorageState } from '../storage-store';

import { initializeStorage } from './initialize-storage';
import { navigateToDirectory } from './navigate-to-directory';
import { navigateDirectoryBackward } from './navigate-directory-backward';
import { navigateDirectoryForward } from './navigate-directory-forward';
import { navigateUpOneDirectory } from './navigate-up-one-directory';
import { refreshDirectory } from './refresh-directory';
import { removeStorage } from './remove-storage';
import { removeAllStorage } from './remove-all-storage';
import { searchFiles } from './search-files';
import { clearSearch } from './clear-search';
import { saveFavorite } from './save-favorite';
import { removeFavorite } from './remove-favorite';
import { WritableStore } from '../storage-helpers';

export function withStorageActions() {
  return withMethods((store, storageService: IStorageService = inject(STORAGE_SERVICE)) => {
    const writableStore = store as WritableStore<StorageState>;
    return {
      ...initializeStorage(writableStore, storageService),
      ...navigateToDirectory(writableStore, storageService),
      ...navigateDirectoryBackward(writableStore, storageService),
      ...navigateDirectoryForward(writableStore, storageService),
      ...navigateUpOneDirectory(writableStore, storageService),
      ...refreshDirectory(writableStore, storageService),
      ...removeStorage(writableStore),
      ...removeAllStorage(writableStore),
      ...searchFiles(writableStore, storageService),
      ...clearSearch(writableStore),
      ...saveFavorite(writableStore, storageService),
      ...removeFavorite(writableStore, storageService),
    };
  });
}
