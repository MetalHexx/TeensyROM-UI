import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ThemeTesterComponent } from './theme-tester.component';

describe('ThemeTesterComponent', () => {
  let component: ThemeTesterComponent;
  let fixture: ComponentFixture<ThemeTesterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ThemeTesterComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ThemeTesterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
