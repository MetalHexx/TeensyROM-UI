import { Injectable } from '@angular/core';
import {
  DevicesApiService,
  FindDevicesResponse,
  ConnectDeviceResponse,
  DisconnectDeviceResponse,
  ResetDeviceResponse,
} from '@teensyrom-nx/data-access/api-client';
import { Device } from './device.models';
import { DeviceMapper } from './device.mapper';
import { Observable, map } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DeviceService {
  constructor(private readonly apiService: DevicesApiService) {}

  findDevices(autoConnectNew: boolean): Observable<Device[]> {
    return this.apiService
      .findDevices(autoConnectNew)
      .pipe(map((response: FindDevicesResponse) => DeviceMapper.toDeviceList(response.devices)));
  }

  getConnectedDevices(): Observable<Device[]> {
    return this.apiService
      .findDevices()
      .pipe(
        map((response: FindDevicesResponse) =>
          DeviceMapper.toDeviceList(response.devices.filter((d) => d.isConnected) || [])
        )
      );
  }

  connectDevice(deviceId: string): Observable<Device> {
    return this.apiService
      .connectDevice(deviceId)
      .pipe(
        map((response: ConnectDeviceResponse) => DeviceMapper.toDevice(response.connectedCart))
      );
  }

  disconnectDevice(deviceId: string): Observable<DisconnectDeviceResponse> {
    return this.apiService.disconnectDevice(deviceId);
  }

  resetDevice(deviceId: string): Observable<ResetDeviceResponse> {
    return this.apiService.resetDevice(deviceId);
  }
}
