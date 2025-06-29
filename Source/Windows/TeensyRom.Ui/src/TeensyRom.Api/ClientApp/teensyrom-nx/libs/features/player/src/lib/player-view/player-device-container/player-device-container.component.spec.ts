import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PlayerDeviceContainerComponent } from './player-device-container.component';

describe('PlayerDeviceContainerComponent', () => {
  let component: PlayerDeviceContainerComponent;
  let fixture: ComponentFixture<PlayerDeviceContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerDeviceContainerComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerDeviceContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
