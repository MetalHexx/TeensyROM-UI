import { Injectable, Inject } from '@angular/core';
import { signal, Signal, WritableSignal, computed } from '@angular/core';
import { DevicesApiService } from '@teensyrom-nx/data-access/api-client';
import * as signalR from '@microsoft/signalr';
import { LogType, logInfo, logError } from '@teensyrom-nx/utils';
import { IDeviceLogsService, ALERT_SERVICE, IAlertService } from '@teensyrom-nx/domain';

@Injectable()
export class DeviceLogsService implements IDeviceLogsService {
  private readonly deviceService: DevicesApiService;
  private readonly alertService: IAlertService;
  private hubConnection: signalR.HubConnection | null = null;
  private readonly _logLines: WritableSignal<string[]> = signal([]);
  private readonly _isConnected = signal(false);

  readonly isConnected = this._isConnected.asReadonly();
  readonly logs: Signal<string[]> = computed(() => this._logLines());

  constructor(
    deviceService: DevicesApiService,
    @Inject(ALERT_SERVICE) alertService: IAlertService
  ) {
    this.deviceService = deviceService;
    this.alertService = alertService;
  }

  connect() {
    // Start logs API call - fire and forget with error handling
    this.deviceService.startLogs().catch((error) => {
      const message = error instanceof Error ? error.message : 'Failed to start device logs';
      logError('DeviceLogsService.connect startLogs error:', error);
      this.alertService.error(message);
    });

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5168/logHub')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('LogProduced', (log: string) => {
      this._logLines.update((lines) => [...lines.slice(-199), log]);
    });

    this.hubConnection
      .start()
      .then(() => {
        this._isConnected.set(true);
        logInfo(LogType.Success, 'Device Logs Connection started');
      })
      .catch((err) => {
        this._isConnected.set(false);
        const message = err?.message || 'Failed to start device logs connection';
        logError('Error starting Device Logs connection:', err);
        this.alertService.error(message);
      });
  }

  disconnect() {
    // Stop logs API call - fire and forget with error handling
    this.deviceService.stopLogs().catch((error) => {
      const message = error instanceof Error ? error.message : 'Failed to stop device logs';
      logError('DeviceLogsService.disconnect stopLogs error:', error);
      this.alertService.error(message);
    });

    this.hubConnection?.stop();
    this.hubConnection = null;
    this._isConnected.set(false);
  }

  clear() {
    this._logLines.set([]);
  }
}
