import { patchState, WritableStateSource } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { AllDevices, Device, DeviceService } from '@teensyrom-nx/domain/device/services';
import { distinctUntilChanged, pipe, switchMap, tap, catchError } from 'rxjs';
import { DeviceState } from '../device-store';

type SignalStore<T> = {
  [K in keyof T]: () => T[K];
};

export function findDevices(
  store: SignalStore<DeviceState> & WritableStateSource<DeviceState>,
  deviceService: DeviceService
) {
  return {
    findDevices: rxMethod(
      pipe(
        distinctUntilChanged(),
        tap(() => patchState(store, { isLoading: true })),
        switchMap(() =>
          deviceService.findDevices().pipe(
            tap((allDevices: AllDevices) => {
              const connectedIds = new Set(
                allDevices.connectedCarts.map((device: Device) => device.deviceId)
              );

              const available = allDevices.availableCarts.filter(
                (device: Device) => !connectedIds.has(device.deviceId)
              );

              patchState(store, {
                availableDevices: available,
                connectedDevices: allDevices.connectedCarts,
                error: available.length === 0 ? 'No available devices found' : null,
                isLoading: false,
                hasInitialised: true,
              });
            }),
            catchError((error: unknown) => {
              patchState(store, {
                error: String(error),
                isLoading: false,
                hasInitialised: true,
              });
              return [];
            })
          )
        )
      )
    ),
  };
}
