import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FileImageComponent } from './file-image.component';

describe('FileImageComponent', () => {
  let component: FileImageComponent;
  let fixture: ComponentFixture<FileImageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FileImageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FileImageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
