import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ThumbnailImageComponent } from './thumbnail-image.component';

describe('ThumbnailImageComponent', () => {
  let component: ThumbnailImageComponent;
  let fixture: ComponentFixture<ThumbnailImageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ThumbnailImageComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ThumbnailImageComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not display image when imageUrl is null', () => {
    fixture.componentRef.setInput('imageUrl', null);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('img')).toBeNull();
  });

  it('should display image when imageUrl is provided', () => {
    const testUrl = 'https://example.com/image.jpg';
    fixture.componentRef.setInput('imageUrl', testUrl);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const img = compiled.querySelector('img');
    expect(img).toBeTruthy();
    expect(img?.getAttribute('src')).toBe(testUrl);
  });

  it('should apply small size class', () => {
    fixture.componentRef.setInput('imageUrl', 'test.jpg');
    fixture.componentRef.setInput('size', 'small');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const img = compiled.querySelector('img');
    expect(img?.classList.contains('thumbnail-small')).toBe(true);
  });

  it('should apply medium size class by default', () => {
    fixture.componentRef.setInput('imageUrl', 'test.jpg');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const img = compiled.querySelector('img');
    expect(img?.classList.contains('thumbnail-medium')).toBe(true);
  });

  it('should apply large size class', () => {
    fixture.componentRef.setInput('imageUrl', 'test.jpg');
    fixture.componentRef.setInput('size', 'large');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const img = compiled.querySelector('img');
    expect(img?.classList.contains('thumbnail-large')).toBe(true);
  });
});
