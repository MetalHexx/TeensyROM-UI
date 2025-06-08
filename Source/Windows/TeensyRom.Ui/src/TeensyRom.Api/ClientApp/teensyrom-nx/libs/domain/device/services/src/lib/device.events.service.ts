import { Injectable, computed, signal, WritableSignal, Signal } from '@angular/core';
import { DeviceState } from '@teensyrom-nx/data-access/api-client';

export type DeviceEvent = {
  deviceId: string;
  state: DeviceState;
};

@Injectable({ providedIn: 'root' })
export class DeviceEventsService {
  private eventSource: EventSource | null = null;

  private readonly _deviceEventMap: WritableSignal<Map<string, DeviceState>> = signal(new Map());

  readonly allEvents: Signal<Map<string, DeviceState>> = computed(() => this._deviceEventMap());

  private deviceEventHandler = (event: MessageEvent) => {
    const deviceEvent: DeviceEvent = JSON.parse(event.data);
    this._deviceEventMap.update((prevMap) => {
      const updatedMap = new Map(prevMap);
      updatedMap.set(deviceEvent.deviceId, deviceEvent.state);
      return updatedMap;
    });
  };

  connect() {
    if (!this.eventSource) {
      this.eventSource = new EventSource('http://localhost:5168/device/events');
      this.eventSource.addEventListener('device-event', this.deviceEventHandler);
      this.eventSource.onerror = (err) => {
        console.error('SSE error:', err);
        this.eventSource?.close();
      };
    }
  }

  disconnect() {
    if (this.eventSource) {
      this.eventSource.removeEventListener('device-event', this.deviceEventHandler);
      this.eventSource.close();
      this.eventSource = null;
    }
  }

  getDeviceState(deviceId: string): Signal<DeviceState | null> {
    return computed(() => {
      const test = this._deviceEventMap().get(deviceId) ?? null;
      return test;
    });
  }
}
