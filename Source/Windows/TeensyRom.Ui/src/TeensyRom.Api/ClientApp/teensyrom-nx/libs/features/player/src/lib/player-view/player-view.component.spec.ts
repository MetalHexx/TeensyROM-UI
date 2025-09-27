import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PlayerViewComponent } from './player-view.component';
import {
  DEVICE_SERVICE,
  DEVICE_STORAGE_SERVICE,
  DEVICE_EVENTS_SERVICE,
  DEVICE_LOGS_SERVICE,
  STORAGE_SERVICE,
  IDeviceService,
  IStorageService,
  IDeviceEventsService,
  IDeviceLogsService,
  Device,
  StorageDirectory,
} from '@teensyrom-nx/domain';
import { of } from 'rxjs';

describe('PlayerViewComponent', () => {
  let component: PlayerViewComponent;
  let fixture: ComponentFixture<PlayerViewComponent>;

  // Mock domain services
  const mockDeviceService: Partial<IDeviceService> = {
    getDevices: () => of([]),
    getDevice: () => of({} as Device),
    connectDevice: () => of(void 0),
    disconnectDevice: () => of(void 0),
  };

  const mockStorageService: Partial<IStorageService> = {
    getDirectories: () => of([]),
    getDirectory: () => of({} as StorageDirectory),
  };

  const mockDeviceEventsService: Partial<IDeviceEventsService> = {
    onDeviceConnected: () => of(),
    onDeviceDisconnected: () => of(),
  };

  const mockDeviceLogsService: Partial<IDeviceLogsService> = {
    getLogs: () => of([]),
    onLogReceived: () => of(),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerViewComponent],
      providers: [
        { provide: DEVICE_SERVICE, useValue: mockDeviceService },
        { provide: STORAGE_SERVICE, useValue: mockStorageService },
        { provide: DEVICE_EVENTS_SERVICE, useValue: mockDeviceEventsService },
        { provide: DEVICE_LOGS_SERVICE, useValue: mockDeviceLogsService },
        { provide: DEVICE_STORAGE_SERVICE, useValue: {} },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
