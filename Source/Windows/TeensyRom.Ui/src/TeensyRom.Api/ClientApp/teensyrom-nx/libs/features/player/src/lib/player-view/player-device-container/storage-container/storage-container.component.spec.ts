import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { StorageContainerComponent } from './storage-container.component';
import { FilesApiService } from '@teensyrom-nx/data-access/api-client';
import { STORAGE_SERVICE_PROVIDER } from '@teensyrom-nx/infrastructure';

describe('StorageContainerComponent', () => {
  let component: StorageContainerComponent;
  let fixture: ComponentFixture<StorageContainerComponent>;

  // Mock service
  const mockFilesApiService = {
    getDirectory: vi.fn().mockResolvedValue({ storageItem: {} }),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StorageContainerComponent],
      providers: [
        { provide: FilesApiService, useValue: mockFilesApiService },
        STORAGE_SERVICE_PROVIDER,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(StorageContainerComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
