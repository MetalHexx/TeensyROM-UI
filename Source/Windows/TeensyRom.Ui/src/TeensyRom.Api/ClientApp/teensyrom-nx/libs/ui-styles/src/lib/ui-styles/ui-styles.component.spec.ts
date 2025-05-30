import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UiStylesComponent } from './ui-styles.component';

describe('UiStylesComponent', () => {
  let component: UiStylesComponent;
  let fixture: ComponentFixture<UiStylesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiStylesComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(UiStylesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
