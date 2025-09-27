import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { Device } from '../models';

export interface IDeviceService {
  findDevices(autoConnectNew: boolean): Observable<Device[]>;
  getConnectedDevices(): Observable<Device[]>;
  connectDevice(deviceId: string): Observable<Device>;
  disconnectDevice(deviceId: string): Observable<void>;
  resetDevice(deviceId: string): Observable<void>;
  pingDevice(deviceId: string): Observable<void>;
}

export const DEVICE_SERVICE = new InjectionToken<IDeviceService>('DEVICE_SERVICE');