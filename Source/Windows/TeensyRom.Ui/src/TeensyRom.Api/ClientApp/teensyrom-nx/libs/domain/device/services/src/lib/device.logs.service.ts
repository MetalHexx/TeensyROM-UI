import { Injectable } from '@angular/core';
import { signal, Signal, WritableSignal, computed } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class DeviceLogsService {
  private eventSource: EventSource | null = null;
  private readonly _logLines: WritableSignal<string[]> = signal([]);

  readonly logs: Signal<string[]> = computed(() => this._logLines());

  connect() {
    if (!this.eventSource) {
      this.eventSource = new EventSource('http://localhost:5168/logs');
    }

    this.eventSource.addEventListener('log', (event: MessageEvent) => {
      const parsed = JSON.parse(event.data);
      const newLine = parsed.message;
      this._logLines.update((lines) => [...lines.slice(-199), newLine]);
    });

    this.eventSource.onerror = (err) => {
      console.error('SSE error:', err);
      this.eventSource?.close();
    };
  }

  disconnect() {
    if (this.eventSource) {
      this.eventSource?.close();
      this.eventSource = null;
    }
  }

  clear() {
    this._logLines.set([]);
  }
}
