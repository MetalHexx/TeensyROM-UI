import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MixingViewComponent } from './mixing-view.component';

describe('MixingViewComponent', () => {
  let component: MixingViewComponent;
  let fixture: ComponentFixture<MixingViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MixingViewComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(MixingViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
