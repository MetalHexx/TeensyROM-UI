import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { PlayerViewComponent } from './player-view.component';
import { DevicesApiService } from '@teensyrom-nx/data-access/api-client';
import { FilesApiService } from '@teensyrom-nx/data-access/api-client';
import { STORAGE_SERVICE_PROVIDER } from '@teensyrom-nx/domain/storage/services';

describe('PlayerViewComponent', () => {
  let component: PlayerViewComponent;
  let fixture: ComponentFixture<PlayerViewComponent>;

  // Mock services
  const mockDevicesApiService = {
    findDevices: vi.fn().mockResolvedValue({ devices: [] }),
    connectDevice: vi.fn().mockResolvedValue({ connectedCart: {} }),
    disconnectDevice: vi.fn().mockResolvedValue({}),
    resetDevice: vi.fn().mockResolvedValue({}),
    pingDevice: vi.fn().mockResolvedValue({}),
  };

  const mockFilesApiService = {
    getDirectory: vi.fn().mockResolvedValue({ storageItem: {} }),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerViewComponent],
      providers: [
        { provide: DevicesApiService, useValue: mockDevicesApiService },
        { provide: FilesApiService, useValue: mockFilesApiService },
        STORAGE_SERVICE_PROVIDER,
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
