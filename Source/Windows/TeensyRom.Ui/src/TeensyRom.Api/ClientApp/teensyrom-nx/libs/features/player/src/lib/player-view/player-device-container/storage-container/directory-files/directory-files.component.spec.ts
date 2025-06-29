import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DirectoryFilesComponent } from './directory-files.component';

describe('DirectoryFilesComponent', () => {
  let component: DirectoryFilesComponent;
  let fixture: ComponentFixture<DirectoryFilesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DirectoryFilesComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectoryFilesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
