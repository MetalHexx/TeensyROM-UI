import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DirectoryTreeComponent } from './directory-tree.component';

describe('DirectoryTreeComponent', () => {
  let component: DirectoryTreeComponent;
  let fixture: ComponentFixture<DirectoryTreeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DirectoryTreeComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectoryTreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
