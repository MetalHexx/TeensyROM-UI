import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DeviceToolbarComponent } from './device-toolbar.component';
import {
  DEVICE_SERVICE,
  DEVICE_STORAGE_SERVICE,
  DEVICE_EVENTS_SERVICE,
  DEVICE_LOGS_SERVICE,
  IDeviceService,
  IStorageService,
  IDeviceEventsService,
  IDeviceLogsService,
  Device,
  StorageDirectory,
} from '@teensyrom-nx/domain';
import { signal } from '@angular/core';
import { of } from 'rxjs';

describe('DeviceToolbarComponent', () => {
  let component: DeviceToolbarComponent;
  let fixture: ComponentFixture<DeviceToolbarComponent>;

  beforeEach(async () => {
    const mockDeviceService: Partial<IDeviceService> = {
      findDevices: () => of([]),
      getConnectedDevices: () => of([]),
      connectDevice: () => of({} as Device),
      disconnectDevice: () => of(undefined),
      resetDevice: () => of(undefined),
      pingDevice: () => of(undefined),
    };

    const mockStorageService: Partial<IStorageService> = {
      getDirectory: () => of({} as StorageDirectory),
      index: () => of({}),
      indexAll: () => of({}),
    };

    const mockEventsService: Partial<IDeviceEventsService> = {
      allEvents: signal(new Map()),
      connect: () => { /* mock implementation */ },
      disconnect: () => { /* mock implementation */ },
      getDeviceState: () => signal(null),
    };

    const mockLogsService: Partial<IDeviceLogsService> = {
      isConnected: signal(false),
      logs: signal([]),
      connect: () => { /* mock implementation */ },
      disconnect: () => { /* mock implementation */ },
      clear: () => { /* mock implementation */ },
    };

    await TestBed.configureTestingModule({
      imports: [DeviceToolbarComponent],
      providers: [
        { provide: DEVICE_SERVICE, useValue: mockDeviceService },
        { provide: DEVICE_STORAGE_SERVICE, useValue: mockStorageService },
        { provide: DEVICE_EVENTS_SERVICE, useValue: mockEventsService },
        { provide: DEVICE_LOGS_SERVICE, useValue: mockLogsService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DeviceToolbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
