import { InjectionToken } from '@angular/core';
import {
  DisconnectDeviceResponse,
  PingDeviceResponse,
  ResetDeviceResponse,
} from '@teensyrom-nx/data-access/api-client';
import { Observable } from 'rxjs';
import { Device } from '../models/device.models';

export interface IDeviceService {
  findDevices(autoConnectNew: boolean): Observable<Device[]>;
  getConnectedDevices(): Observable<Device[]>;
  connectDevice(deviceId: string): Observable<Device>;
  disconnectDevice(deviceId: string): Observable<DisconnectDeviceResponse>;
  resetDevice(deviceId: string): Observable<ResetDeviceResponse>;
  pingDevice(deviceId: string): Observable<PingDeviceResponse>;
}

export const DEVICE_SERVICE = new InjectionToken<IDeviceService>('DEVICE_SERVICE');

