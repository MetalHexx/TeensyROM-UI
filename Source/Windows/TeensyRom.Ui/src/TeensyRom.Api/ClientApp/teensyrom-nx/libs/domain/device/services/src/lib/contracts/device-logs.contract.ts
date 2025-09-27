import { InjectionToken, Signal } from '@angular/core';

export interface IDeviceLogsService {
  readonly isConnected: Signal<boolean>;
  readonly logs: Signal<string[]>;
  connect(): void;
  disconnect(): void;
  clear(): void;
}

export const DEVICE_LOGS_SERVICE = new InjectionToken<IDeviceLogsService>('DEVICE_LOGS_SERVICE');

