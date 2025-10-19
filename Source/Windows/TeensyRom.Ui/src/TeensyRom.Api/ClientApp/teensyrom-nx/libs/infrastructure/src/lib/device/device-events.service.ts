import { Injectable, computed, signal, WritableSignal, Signal, Inject } from '@angular/core';
import { DevicesApiService } from '@teensyrom-nx/data-access/api-client';
import { DeviceState, IDeviceEventsService, ALERT_SERVICE, IAlertService } from '@teensyrom-nx/domain';
import * as signalR from '@microsoft/signalr';
import { LogType, logInfo, logError } from '@teensyrom-nx/utils';

export type DeviceEvent = {
  deviceId: string;
  state: DeviceState;
};

@Injectable()
export class DeviceEventsService implements IDeviceEventsService {
  private readonly deviceService: DevicesApiService;
  private readonly alertService: IAlertService;
  private hubConnection: signalR.HubConnection | null = null;
  private readonly _deviceEventMap: WritableSignal<Map<string, DeviceState>> = signal(new Map());
  readonly allEvents: Signal<Map<string, DeviceState>> = computed(() => this._deviceEventMap());

  constructor(
    deviceService: DevicesApiService,
    @Inject(ALERT_SERVICE) alertService: IAlertService
  ) {
    this.deviceService = deviceService;
    this.alertService = alertService;
  }

  connect() {
    // Start device events API call - fire and forget with error handling
    this.deviceService.startDeviceEvents().catch((error) => {
      const message = error instanceof Error ? error.message : 'Failed to start device events';
      logError('DeviceEventsService.connect startDeviceEvents error:', error);
      this.alertService.error(message);
    });

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
        logInfo(LogType.Success, 'Device Events Connection started');
      })
      .catch((err) => {
        const message =
          err?.message || 'Failed to start device events connection';
        logError('Error starting Device Events connection:', err);
        this.alertService.error(message);
      });
  }

  disconnect() {
    // Stop device events API call - fire and forget with error handling
    this.deviceService.stopDeviceEvents().catch((error) => {
      const message = error instanceof Error ? error.message : 'Failed to stop device events';
      logError('DeviceEventsService.disconnect stopDeviceEvents error:', error);
      this.alertService.error(message);
    });

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

