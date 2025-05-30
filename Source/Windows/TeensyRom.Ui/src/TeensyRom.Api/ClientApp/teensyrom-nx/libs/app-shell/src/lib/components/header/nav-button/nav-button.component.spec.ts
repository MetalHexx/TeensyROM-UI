import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { NavButtonComponent } from './nav-button.component';
import '../../../../test-setup';

describe('NavButtonComponent', () => {
  let component: NavButtonComponent;
  let fixture: ComponentFixture<NavButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(NavButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
