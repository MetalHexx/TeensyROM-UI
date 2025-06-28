import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DeviceToolbarComponent } from './device-toolbar.component';

describe('DeviceToolbarComponent', () => {
  let component: DeviceToolbarComponent;
  let fixture: ComponentFixture<DeviceToolbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeviceToolbarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DeviceToolbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
