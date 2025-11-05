import { InjectionToken, Signal } from '@angular/core';
import { DeviceState } from '../models/device-state.enum';

export interface IDeviceEventsService {
  readonly allEvents: Signal<Map<string, DeviceState>>;
  connect(): void;
  disconnect(): void;
  getDeviceState(deviceId: string): Signal<DeviceState | null>;
}

export const DEVICE_EVENTS_SERVICE = new InjectionToken<IDeviceEventsService>(
  'DEVICE_EVENTS_SERVICE'
);
