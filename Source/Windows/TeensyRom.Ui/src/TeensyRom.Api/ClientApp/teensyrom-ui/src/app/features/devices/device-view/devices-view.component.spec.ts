import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DevicesViewComponent } from './devices-view.component';

describe('DevicesViewComponent', () => {
  let component: DevicesViewComponent;
  let fixture: ComponentFixture<DevicesViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DevicesViewComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DevicesViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
