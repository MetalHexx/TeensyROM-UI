import { Injectable, Inject } from '@angular/core';
import {
  DevicesApiService,
  FindDevicesResponse,
  ConnectDeviceResponse,
} from '@teensyrom-nx/data-access/api-client';
import { Device, IDeviceService, ALERT_SERVICE, IAlertService } from '@teensyrom-nx/domain';
import { DomainMapper } from '../domain.mapper';
import { Observable, map, from, catchError, throwError, mergeMap } from 'rxjs';
import { logError } from '@teensyrom-nx/utils';
import { extractErrorMessage } from '../error/api-error.utils';

@Injectable()
export class DeviceService implements IDeviceService {
  private readonly apiService: DevicesApiService;
  private readonly alertService: IAlertService;

  constructor(
    apiService: DevicesApiService,
    @Inject(ALERT_SERVICE) alertService: IAlertService
  ) {
    this.apiService = apiService;
    this.alertService = alertService;
  }

  findDevices(autoConnectNew: boolean): Observable<Device[]> {
    return from(this.apiService.findDevices({ autoConnectNew })).pipe(
      map((response: FindDevicesResponse) => DomainMapper.toDeviceList(response.devices)),
      catchError((error) => this.handleError(error, 'findDevices', 'Failed to find devices'))
    );
  }

  getConnectedDevices(): Observable<Device[]> {
    return from(this.apiService.findDevices()).pipe(
      map((response: FindDevicesResponse) =>
        DomainMapper.toDeviceList(response.devices.filter((d) => d.isConnected) || [])
      ),
      catchError((error) => this.handleError(error, 'getConnectedDevices', 'Failed to retrieve connected devices'))
    );
  }

  connectDevice(deviceId: string): Observable<Device> {
    return from(this.apiService.connectDevice({ deviceId })).pipe(
      map((response: ConnectDeviceResponse) => DomainMapper.toDevice(response.connectedCart)),
      catchError((error) => this.handleError(error, 'connectDevice', 'Failed to connect to device'))
    );
  }

  disconnectDevice(deviceId: string): Observable<void> {
    return from(this.apiService.disconnectDevice({ deviceId })).pipe(
      map(() => void 0),
      catchError((error) => this.handleError(error, 'disconnectDevice', 'Failed to disconnect device'))
    );
  }

  resetDevice(deviceId: string): Observable<void> {
    return from(this.apiService.resetDevice({ deviceId })).pipe(
      map(() => void 0),
      catchError((error) => this.handleError(error, 'resetDevice', 'Failed to reset device'))
    );
  }

  pingDevice(deviceId: string): Observable<void> {
    return from(this.apiService.pingDevice({ deviceId })).pipe(
      map(() => void 0),
      catchError((error) => this.handleError(error, 'pingDevice', 'Failed to ping device'))
    );
  }

  private handleError(error: unknown, methodName: string, fallbackMessage: string): Observable<never> {
    return from(extractErrorMessage(error, fallbackMessage)).pipe(
      mergeMap((message) => {
        logError(`DeviceService.${methodName} error:`, error);
        this.alertService.error(message);
        return throwError(() => error);
      })
    );
  }
}

