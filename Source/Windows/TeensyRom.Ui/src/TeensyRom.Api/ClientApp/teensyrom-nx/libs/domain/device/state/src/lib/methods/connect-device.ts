import { patchState, WritableStateSource } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { DeviceService, Device } from '@teensyrom-nx/domain/device/services';
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
        tap(() => patchState(store, { isLoading: true, error: null })),
        switchMap((deviceId: string) =>
          deviceService.connectDevice(deviceId).pipe(
            tap((device: Device) => {
              const currentAvailable = store.availableDevices();
              const currentConnected = store.connectedDevices();

              const updatedAvailable = currentAvailable.filter(
                (d) => d.deviceId !== device.deviceId
              );
              const updatedConnected = [...currentConnected, device];

              patchState(store, {
                isLoading: false,
                availableDevices: updatedAvailable,
                connectedDevices: updatedConnected,
              });
            }),
            catchError((error: unknown) => {
              patchState(store, { isLoading: false, error: String(error) });
              return [];
            })
          )
        )
      )
    ),
  };
}
