import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DeviceViewComponent } from './device-view.component';

describe('DevicesComponent', () => {
  let component: DeviceViewComponent;
  let fixture: ComponentFixture<DeviceViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeviceViewComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DeviceViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
