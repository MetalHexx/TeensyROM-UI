import { patchState, WritableStateSource } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { DeviceService } from '@teensyrom-nx/domain/device/services';
import { pipe, switchMap, tap, catchError } from 'rxjs';
import { DeviceState } from '../device-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function connectDevice(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  deviceService: DeviceService
) {
  return {
    connectDevice: rxMethod<string>(
      pipe(
        tap(() => patchState(store, { error: null })),
        switchMap((deviceId: string) =>
          deviceService.connectDevice(deviceId).pipe(
            tap(() => {
              patchState(store, {
                isLoading: false,
                devices: store
                  .devices()
                  .map((d) => (d.deviceId === deviceId ? { ...d, isConnected: true } : d)),
              });
            }),
            catchError((error: unknown) => {
              patchState(store, { error: String(error) });
              return [];
            })
          )
        )
      )
    ),
  };
}
