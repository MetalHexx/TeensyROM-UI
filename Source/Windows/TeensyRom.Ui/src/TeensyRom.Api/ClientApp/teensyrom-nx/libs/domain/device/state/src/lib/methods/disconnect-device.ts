import { patchState, WritableStateSource } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { DeviceService, Device } from '@teensyrom-nx/domain/device/services';
import { pipe, switchMap, tap, catchError } from 'rxjs';
import { DeviceState } from '../device-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function disconnectDevice(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  deviceService: DeviceService
) {
  return {
    disconnectDevice: rxMethod<string>(
      pipe(
        switchMap((deviceId: string) =>
          deviceService.disconnectDevice(deviceId).pipe(
            tap(() => {
              patchState(store, {
                devices: store
                  .devices()
                  .map((d) => (d.deviceId === deviceId ? { ...d, isConnected: false } : d)),
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
