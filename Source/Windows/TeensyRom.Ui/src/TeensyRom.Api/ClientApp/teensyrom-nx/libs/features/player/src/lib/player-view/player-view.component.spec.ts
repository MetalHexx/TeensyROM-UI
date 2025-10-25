import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
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
} from '@teensyrom-nx/domain';
import { of } from 'rxjs';

describe('PlayerViewComponent', () => {
  let component: PlayerViewComponent;
  let fixture: ComponentFixture<PlayerViewComponent>;

  const mockDeviceService: Partial<IDeviceService> = {
    getConnectedDevices: () => of([]),
  };

  const mockStorageService: Partial<IStorageService> = {};

  const mockDeviceEventsService: Partial<IDeviceEventsService> = {};

  const mockDeviceLogsService: Partial<IDeviceLogsService> = {};

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerViewComponent],
      providers: [
        provideNoopAnimations(),
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

  it('should have stores injected', () => {
    expect(component.deviceStore).toBeTruthy();
  });

  it('should compute connected devices', () => {
    expect(component.connectedDevices).toBeTruthy();
    expect(component.connectedDevices()).toEqual([]);
  });
});
