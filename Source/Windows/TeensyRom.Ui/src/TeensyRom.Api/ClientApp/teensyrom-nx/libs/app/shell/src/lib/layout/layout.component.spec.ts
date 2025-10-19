import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { LayoutComponent } from './layout.component';
import { of } from 'rxjs';
import '../../test-setup';

// Use a string token to avoid importing from domain in app layer
const MOCK_ALERT_SERVICE_TOKEN = 'ALERT_SERVICE';

describe('LayoutComponent', () => {
  let component: LayoutComponent;
  let fixture: ComponentFixture<LayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LayoutComponent],
      providers: [
        {
          provide: MOCK_ALERT_SERVICE_TOKEN,
          useValue: {
            alerts$: of([]),
            dismiss: vi.fn(),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
