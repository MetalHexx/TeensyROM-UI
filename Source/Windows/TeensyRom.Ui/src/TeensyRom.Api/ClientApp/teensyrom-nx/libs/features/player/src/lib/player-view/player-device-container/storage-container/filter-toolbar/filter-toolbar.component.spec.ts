import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FilterToolbarComponent } from './filter-toolbar.component';

describe('FilterToolbarComponent', () => {
  let component: FilterToolbarComponent;
  let fixture: ComponentFixture<FilterToolbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FilterToolbarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FilterToolbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});