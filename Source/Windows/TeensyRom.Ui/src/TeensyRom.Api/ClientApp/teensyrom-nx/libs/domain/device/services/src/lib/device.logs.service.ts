import { inject, Injectable } from '@angular/core';
import { signal, Signal, WritableSignal, computed } from '@angular/core';
import { DevicesApiService } from '@teensyrom-nx/data-access/api-client';

@Injectable({ providedIn: 'root' })
export class DeviceLogsService {
  private readonly deviceService = inject(DevicesApiService);
  private eventSource: EventSource | null = null;
  private readonly _logLines: WritableSignal<string[]> = signal([]);
  private readonly _isConnected = signal(false);

  readonly logs: Signal<string[]> = computed(() => this._logLines());
  readonly isConnected = this._isConnected.asReadonly();

  connect() {
    if (!this.eventSource) {
      this.eventSource = new EventSource('http://localhost:5168/logs');
      this._isConnected.set(true);
    }

    this.eventSource.addEventListener('log', (event: MessageEvent) => {
      const parsed = JSON.parse(event.data);
      const newLine = parsed.message;
      this._logLines.update((lines) => [...lines.slice(-199), newLine]);
    });

    this.eventSource.onerror = (err) => {
      console.error('SSE error:', err);
    };
  }

  disconnect() {
    this.deviceService.stopLogs().subscribe();

    if (this.eventSource) {
      this.eventSource?.close();
      this.eventSource = null;
      this._isConnected.set(false);
    }
  }

  clear() {
    this._logLines.set([]);
  }
}
