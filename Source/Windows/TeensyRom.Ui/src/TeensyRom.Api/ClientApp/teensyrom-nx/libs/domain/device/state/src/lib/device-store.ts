import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { withDevtools } from '@angular-architects/ngrx-toolkit';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { AllDevices, Device, DeviceService } from '@teensyrom-nx/domain/device/services';
import { distinctUntilChanged, pipe, switchMap, tap, catchError } from 'rxjs';

type DeviceState = {
  availableDevices: Device[];
  connectedDevices: Device[];
  hasInitialised: boolean;
  isLoading: boolean;
  error: string | null;
};

const initialState: DeviceState = {
  availableDevices: [],
  connectedDevices: [],
  hasInitialised: false,
  isLoading: false,
  error: null,
};

export const DeviceStore = signalStore(
  { providedIn: 'root' },
  withDevtools('devices'),
  withState(initialState),
  withMethods((store, deviceService: DeviceService = inject(DeviceService)) => ({
    findDevices: rxMethod(
      pipe(
        distinctUntilChanged(),
        tap(() => patchState(store, { isLoading: true })),
        switchMap(() =>
          deviceService.findDevices().pipe(
            tap((allDevices: AllDevices) => {
              patchState(store, {
                availableDevices: allDevices.availableCarts,
                connectedDevices: allDevices.connectedCarts,
                error: allDevices.availableCarts.length === 0 ? 'No available devices found' : null,
                isLoading: false,
                hasInitialised: true,
              });
            }),
            catchError((error: string) => {
              patchState(store, {
                error: error,
                isLoading: false,
                hasInitialised: true,
              });
              return [];
            })
          )
        )
      )
    ),
  }))
);
