import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { FileOtherComponent } from './file-other.component';

describe('FileOtherComponent', () => {
  let component: FileOtherComponent;
  let fixture: ComponentFixture<FileOtherComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideNoopAnimations()],
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
