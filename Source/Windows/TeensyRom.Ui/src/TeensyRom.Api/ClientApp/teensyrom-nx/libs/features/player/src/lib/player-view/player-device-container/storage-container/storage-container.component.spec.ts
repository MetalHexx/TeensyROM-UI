import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StorageContainerComponent } from './storage-container.component';
import {
  STORAGE_SERVICE,
  IStorageService,
  StorageDirectory,
} from '@teensyrom-nx/domain';
import { of } from 'rxjs';

describe('StorageContainerComponent', () => {
  let component: StorageContainerComponent;
  let fixture: ComponentFixture<StorageContainerComponent>;

  // Mock domain service
  const mockStorageService: Partial<IStorageService> = {
    getDirectories: () => of([]),
    getDirectory: () => of({} as StorageDirectory),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StorageContainerComponent],
      providers: [
        { provide: STORAGE_SERVICE, useValue: mockStorageService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(StorageContainerComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
