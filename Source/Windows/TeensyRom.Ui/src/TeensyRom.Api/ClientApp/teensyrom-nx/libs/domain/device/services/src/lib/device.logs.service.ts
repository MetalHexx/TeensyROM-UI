import { inject, Injectable } from '@angular/core';
import { signal, Signal, WritableSignal, computed } from '@angular/core';
import { DevicesApiService } from '@teensyrom-nx/data-access/api-client';
import * as signalR from '@microsoft/signalr';

interface LogDto {
  Message: string;
}

@Injectable({ providedIn: 'root' })
export class DeviceLogsService {
  private readonly deviceService = inject(DevicesApiService);
  private hubConnection: signalR.HubConnection | null = null;
  private readonly _logLines: WritableSignal<string[]> = signal([]);
  private readonly _isConnected = signal(false);

  readonly isConnected = this._isConnected.asReadonly();
  readonly logs: Signal<string[]> = computed(() => this._logLines());

  connect() {
    this.deviceService.startLogs().subscribe();
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
        console.log('Connection started');
      })
      .catch((err) => {
        this._isConnected.set(false);
        console.error('Error starting connection:', err);
      });
  }

  disconnect() {
    this.deviceService.stopLogs().subscribe();
    this.hubConnection?.stop();
    this.hubConnection = null;
    this._isConnected.set(false);
  }

  clear() {
    this._logLines.set([]);
  }
}
