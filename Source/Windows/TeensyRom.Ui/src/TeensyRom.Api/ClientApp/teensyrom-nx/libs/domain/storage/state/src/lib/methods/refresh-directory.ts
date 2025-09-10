import { patchState, WritableStateSource } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe } from 'rxjs';
import { switchMap, tap, catchError, of } from 'rxjs';
import { StorageType } from '@teensyrom-nx/domain/storage/services';
import { StorageService } from '@teensyrom-nx/domain/storage/services';
import { StorageKeyUtil } from '../storage-key.util';
import { StorageState } from '../storage-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function refreshDirectory(
  store: SignalStore<StorageState> & WritableStateSource<StorageState>,
  storageService: StorageService
) {
  return {
    refreshDirectory: rxMethod<{ deviceId: string; storageType: StorageType }>(
      pipe(
        switchMap(({ deviceId, storageType }) => {
          const key = StorageKeyUtil.create(deviceId, storageType);
          const entry = store.storageEntries()[key];

          if (!entry) {
            return of(null);
          }

          // Set loading state
          patchState(store, (state) => ({
            storageEntries: {
              ...state.storageEntries,
              [key]: {
                ...state.storageEntries[key],
                isLoading: true,
                error: null,
              },
            },
          }));

          // Load the directory using the current path
          return storageService.getDirectory(deviceId, storageType, entry.currentPath).pipe(
            tap((directory) => {
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
              patchState(store, (state) => ({
                storageEntries: {
                  ...state.storageEntries,
                  [key]: {
                    ...state.storageEntries[key],
                    isLoading: false,
                    error: error.message || 'Failed to refresh directory',
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
