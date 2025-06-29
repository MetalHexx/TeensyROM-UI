import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FileOtherComponent } from './file-other.component';

describe('FileOtherComponent', () => {
  let component: FileOtherComponent;
  let fixture: ComponentFixture<FileOtherComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FileOtherComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FileOtherComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
