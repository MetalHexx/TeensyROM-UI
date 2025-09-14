import { patchState, WritableStateSource } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe } from 'rxjs';
import { switchMap, tap, catchError, of, filter } from 'rxjs';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { IStorageService } from '@teensyrom-nx/domain/storage/services';
import { StorageKeyUtil } from '../storage-key.util';
import { StorageState } from '../storage-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function navigateToDirectory(
  store: SignalStore<StorageState> & WritableStateSource<StorageState>,
  storageService: IStorageService
) {
  return {
    navigateToDirectory: rxMethod<{ deviceId: string; storageType: StorageType; path: string }>(
      pipe(
        tap(({ deviceId, storageType, path }) => {
          const key = StorageKeyUtil.create(deviceId, storageType);
          const currentState = store.storageEntries();
          const existingEntry = currentState[key];

          // Always update global selection
          patchState(store, {
            selectedDirectory: {
              deviceId,
              storageType,
              path,
            },
          });

          // Check if we already have this directory loaded
          const isAlreadyLoaded =
            existingEntry &&
            existingEntry.currentPath === path &&
            existingEntry.isLoaded &&
            existingEntry.directory &&
            !existingEntry.error;

          if (!isAlreadyLoaded) {
            // Update loading state only if we need to load
            patchState(store, (state) => ({
              storageEntries: {
                ...state.storageEntries,
                [key]: {
                  ...state.storageEntries[key],
                  currentPath: path,
                  isLoading: true,
                  error: null,
                },
              },
            }));
          }
        }),
        // Only proceed with API call if we need to load
        filter(({ deviceId, storageType, path }) => {
          const key = StorageKeyUtil.create(deviceId, storageType);
          const currentState = store.storageEntries();
          const existingEntry = currentState[key];

          const isAlreadyLoaded =
            existingEntry &&
            existingEntry.currentPath === path &&
            existingEntry.isLoaded &&
            existingEntry.directory &&
            !existingEntry.error;

          return !isAlreadyLoaded;
        }),
        switchMap(({ deviceId, storageType, path }) => {
          return storageService.getDirectory(deviceId, storageType, path).pipe(
            tap((directory) => {
              const key = StorageKeyUtil.create(deviceId, storageType);
              patchState(store, (state) => ({
                storageEntries: {
                  ...state.storageEntries,
                  [key]: {
                    ...state.storageEntries[key],
                    directory,
                    isLoaded: true,
                    isLoading: false,
                    error: null,
                    lastLoadTime: Date.now(),
                  },
                },
              }));
            }),
            catchError((error) => {
              const key = StorageKeyUtil.create(deviceId, storageType);
              patchState(store, (state) => ({
                storageEntries: {
                  ...state.storageEntries,
                  [key]: {
                    ...state.storageEntries[key],
                    isLoading: false,
                    error: error.message || 'Failed to navigate to directory',
                  },
                },
              }));
              return of(null);
            })
          );
        })
      )
    ),
  };
}
