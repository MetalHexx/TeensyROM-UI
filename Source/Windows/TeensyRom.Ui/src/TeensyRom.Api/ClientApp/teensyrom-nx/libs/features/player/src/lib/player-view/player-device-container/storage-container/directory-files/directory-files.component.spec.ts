import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { DirectoryFilesComponent } from './directory-files.component';
import { FilesApiService } from '@teensyrom-nx/data-access/api-client';
import { STORAGE_SERVICE_PROVIDER } from '@teensyrom-nx/domain/storage/services';

describe('DirectoryFilesComponent', () => {
  let component: DirectoryFilesComponent;
  let fixture: ComponentFixture<DirectoryFilesComponent>;

  // Mock service
  const mockFilesApiService = {
    getDirectory: vi.fn().mockResolvedValue({ storageItem: {} }),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DirectoryFilesComponent],
      providers: [
        { provide: FilesApiService, useValue: mockFilesApiService },
        STORAGE_SERVICE_PROVIDER,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectoryFilesComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
