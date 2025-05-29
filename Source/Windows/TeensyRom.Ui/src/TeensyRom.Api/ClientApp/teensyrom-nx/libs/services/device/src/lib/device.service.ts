import { Injectable } from '@angular/core';
import {
  DevicesApiService,
  FindDevicesResponse,
  ConnectDeviceResponse,
  DisconnectDeviceResponse,
} from '@teensyrom-nx/api-client';
import { AllDevices, Device } from './device.models';
import { DeviceMapper } from './device.mapper';
import { Observable, map } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DeviceService {
  constructor(private readonly devicesService: DevicesApiService) {}

  findDevices(): Observable<AllDevices> {
    return this.devicesService
      .findDevices()
      .pipe(map((response: FindDevicesResponse) => DeviceMapper.toAllDevices(response)));
  }

  getConnectedDevices(): Observable<Device[]> {
    return this.devicesService
      .findDevices()
      .pipe(
        map((response: FindDevicesResponse) =>
          DeviceMapper.toDeviceList(response.connectedCarts || [])
        )
      );
  }

  connectDevice(deviceId: string): Observable<Device> {
    return this.devicesService
      .connectDevice(deviceId)
      .pipe(
        map((response: ConnectDeviceResponse) => DeviceMapper.toDevice(response.connectedCart))
      );
  }

  disconnectDevice(deviceId: string): Observable<DisconnectDeviceResponse> {
    return this.devicesService.disconnectDevice(deviceId);
  }
}
