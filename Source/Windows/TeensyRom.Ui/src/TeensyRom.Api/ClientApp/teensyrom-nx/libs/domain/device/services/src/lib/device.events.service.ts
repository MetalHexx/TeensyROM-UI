import { Injectable, computed, signal, WritableSignal, Signal, inject } from '@angular/core';
import { DevicesApiService, DeviceState } from '@teensyrom-nx/data-access/api-client';
import { from } from 'rxjs';
import * as signalR from '@microsoft/signalr';

export type DeviceEvent = {
  deviceId: string;
  state: DeviceState;
};

@Injectable({ providedIn: 'root' })
export class DeviceEventsService {
  private readonly deviceService = inject(DevicesApiService);
  private hubConnection: signalR.HubConnection | null = null;
  private readonly _deviceEventMap: WritableSignal<Map<string, DeviceState>> = signal(new Map());
  readonly allEvents: Signal<Map<string, DeviceState>> = computed(() => this._deviceEventMap());

  connect() {
    from(this.deviceService.startDeviceEvents()).subscribe();
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5168/deviceEventHub')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('DeviceEvent', (event: DeviceEvent) => {
      this._deviceEventMap.update((prevMap) => {
        const updatedMap = new Map(prevMap);
        updatedMap.set(event.deviceId, event.state);
        return updatedMap;
      });
    });

    this.hubConnection
      .start()
      .then(() => {
        console.log('Device Events Connection started');
      })
      .catch((err) => {
        console.error('Error starting Device Events connection:', err);
      });
  }

  disconnect() {
    from(this.deviceService.stopDeviceEvents()).subscribe();
    this.hubConnection?.stop();
    this.hubConnection = null;
  }

  getDeviceState(deviceId: string): Signal<DeviceState | null> {
    return computed(() => {
      const test = this._deviceEventMap().get(deviceId) ?? null;
      return test;
    });
  }
}
