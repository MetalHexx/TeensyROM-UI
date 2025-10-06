import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CycleImageComponent } from './cycle-image.component';
import { ComponentRef } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('CycleImageComponent', () => {
  let component: CycleImageComponent;
  let fixture: ComponentFixture<CycleImageComponent>;
  let componentRef: ComponentRef<CycleImageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CycleImageComponent, NoopAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(CycleImageComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
  });

  it('should create', () => {
    componentRef.setInput('images', ['image1.png']);
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should display current image from array', () => {
    const images = ['image1.png', 'image2.png', 'image3.png'];
    componentRef.setInput('images', images);
    fixture.detectChanges();

    expect(component.currentImage()).toBe('image1.png');
  });

  it('should cycle through images at correct interval', fakeAsync(() => {
    const images = ['image1.png', 'image2.png', 'image3.png'];
    componentRef.setInput('images', images);
    componentRef.setInput('intervalMs', 1000);
    fixture.detectChanges();

    expect(component.currentIndex()).toBe(0);

    // Wait for first interval
    tick(1000);
    expect(component.currentIndex()).toBe(1);

    // Wait for second interval
    tick(1000);
    expect(component.currentIndex()).toBe(2);

    // Wait for third interval - should wrap around
    tick(1000);
    expect(component.currentIndex()).toBe(0);
  }));

  it('should increment animationKey on each transition', fakeAsync(() => {
    const images = ['image1.png', 'image2.png'];
    componentRef.setInput('images', images);
    componentRef.setInput('intervalMs', 1000);
    fixture.detectChanges();

    const initialKey = component.animationKey();

    tick(1000);
    expect(component.animationKey()).toBe(initialKey + 1);

    tick(1000);
    expect(component.animationKey()).toBe(initialKey + 2);
  }));

  it('should wrap currentIndex around to 0 after last image', fakeAsync(() => {
    const images = ['image1.png', 'image2.png'];
    componentRef.setInput('images', images);
    componentRef.setInput('intervalMs', 1000);
    fixture.detectChanges();

    expect(component.currentIndex()).toBe(0);

    tick(1000);
    expect(component.currentIndex()).toBe(1);

    tick(1000);
    expect(component.currentIndex()).toBe(0);
  }));

  it('should handle single image gracefully (no cycling)', fakeAsync(() => {
    const images = ['single-image.png'];
    componentRef.setInput('images', images);
    fixture.detectChanges();

    expect(component.currentImage()).toBe('single-image.png');
    expect(component.hasMultipleImages()).toBe(false);

    const initialIndex = component.currentIndex();
    
    // Wait for what would be an interval - should not cycle
    tick(10000);
    expect(component.currentIndex()).toBe(initialIndex);
  }));

  it('should show placeholder when empty array provided', () => {
    componentRef.setInput('images', []);
    componentRef.setInput('placeholderUrl', '/test-placeholder.jpg');
    fixture.detectChanges();

    expect(component.currentImage()).toBe('/test-placeholder.jpg');
    expect(component.hasMultipleImages()).toBe(false);
  });

  it('should set hasMultipleImages correctly', () => {
    // Single image
    componentRef.setInput('images', ['image1.png']);
    fixture.detectChanges();
    expect(component.hasMultipleImages()).toBe(false);

    // Multiple images
    componentRef.setInput('images', ['image1.png', 'image2.png']);
    fixture.detectChanges();
    expect(component.hasMultipleImages()).toBe(true);

    // Empty array (uses placeholder, so still single image)
    componentRef.setInput('images', []);
    fixture.detectChanges();
    expect(component.hasMultipleImages()).toBe(false);
  });

  it('should reset to first image when images input changes', fakeAsync(() => {
    const images1 = ['image1.png', 'image2.png'];
    componentRef.setInput('images', images1);
    componentRef.setInput('intervalMs', 1000);
    fixture.detectChanges();

    // Cycle to second image
    tick(1000);
    expect(component.currentIndex()).toBe(1);

    // Change images array
    const images2 = ['new1.png', 'new2.png', 'new3.png'];
    componentRef.setInput('images', images2);
    fixture.detectChanges();

    // Should reset to first image
    expect(component.currentIndex()).toBe(0);
    expect(component.currentImage()).toBe('new1.png');
  }));

  it('should update previousIndex when cycling', fakeAsync(() => {
    const images = ['image1.png', 'image2.png', 'image3.png'];
    componentRef.setInput('images', images);
    componentRef.setInput('intervalMs', 1000);
    fixture.detectChanges();

    expect(component.previousIndex()).toBeNull();

    // Cycle once
    tick(1000);
    expect(component.previousIndex()).toBe(0);
    expect(component.currentIndex()).toBe(1);

    // Cycle again
    tick(1000);
    expect(component.previousIndex()).toBe(1);
    expect(component.currentIndex()).toBe(2);
  }));

  it('should apply correct size classes', () => {
    componentRef.setInput('images', ['image1.png']);
    componentRef.setInput('size', 'thumbnail');
    fixture.detectChanges();

    const container = fixture.nativeElement.querySelector('.image-container');
    expect(container?.classList.contains('size-thumbnail')).toBe(true);

    componentRef.setInput('size', 'large');
    fixture.detectChanges();

    expect(container?.classList.contains('size-large')).toBe(true);
    expect(container?.classList.contains('size-thumbnail')).toBe(false);
  });

  it('should enable simple mode for thumbnail and small sizes', () => {
    componentRef.setInput('images', ['image1.png']);
    
    componentRef.setInput('size', 'thumbnail');
    fixture.detectChanges();
    expect(component.isSimpleMode()).toBe(true);

    componentRef.setInput('size', 'small');
    fixture.detectChanges();
    expect(component.isSimpleMode()).toBe(true);

    componentRef.setInput('size', 'medium');
    fixture.detectChanges();
    expect(component.isSimpleMode()).toBe(false);

    componentRef.setInput('size', 'large');
    fixture.detectChanges();
    expect(component.isSimpleMode()).toBe(false);
  });
});
