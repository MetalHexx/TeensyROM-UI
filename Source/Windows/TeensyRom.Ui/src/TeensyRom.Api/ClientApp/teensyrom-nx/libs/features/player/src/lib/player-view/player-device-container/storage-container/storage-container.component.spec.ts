import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StorageContainerComponent } from './storage-container.component';

describe('StorageContainerComponent', () => {
  let component: StorageContainerComponent;
  let fixture: ComponentFixture<StorageContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StorageContainerComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(StorageContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
