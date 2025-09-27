import { Injectable } from '@angular/core';
import {
  DevicesApiService,
  FindDevicesResponse,
  ConnectDeviceResponse,
} from '@teensyrom-nx/data-access/api-client';
import { Device } from '@teensyrom-nx/domain';
import { DeviceMapper } from './device.mapper';
import { Observable, map, from } from 'rxjs';
import { IDeviceService } from '@teensyrom-nx/domain';

@Injectable()
export class DeviceService implements IDeviceService {
  constructor(private readonly apiService: DevicesApiService) {}

  findDevices(autoConnectNew: boolean): Observable<Device[]> {
    return from(this.apiService.findDevices({ autoConnectNew })).pipe(
      map((response: FindDevicesResponse) => DeviceMapper.toDeviceList(response.devices))
    );
  }

  getConnectedDevices(): Observable<Device[]> {
    return from(this.apiService.findDevices()).pipe(
      map((response: FindDevicesResponse) =>
        DeviceMapper.toDeviceList(response.devices.filter((d) => d.isConnected) || [])
      )
    );
  }

  connectDevice(deviceId: string): Observable<Device> {
    return from(this.apiService.connectDevice({ deviceId })).pipe(
      map((response: ConnectDeviceResponse) => DeviceMapper.toDevice(response.connectedCart))
    );
  }

  disconnectDevice(deviceId: string): Observable<void> {
    return from(this.apiService.disconnectDevice({ deviceId })).pipe(
      map(() => void 0)
    );
  }

  resetDevice(deviceId: string): Observable<void> {
    return from(this.apiService.resetDevice({ deviceId })).pipe(
      map(() => void 0)
    );
  }

  pingDevice(deviceId: string): Observable<void> {
    return from(this.apiService.pingDevice({ deviceId })).pipe(
      map(() => void 0)
    );
  }
}

